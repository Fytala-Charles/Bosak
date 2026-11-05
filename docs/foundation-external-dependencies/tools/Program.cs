// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 30 October 2026
// PURPOSE              : Inspects external public-signature type dependencies for the foundation API review.
// SPECIAL NOTES        : Read-only metadata inspection; no API adoption or framework-package inference.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 30-10-2026     | Creation; external signature dependency inspection                                   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;

if (args.Length == 1 && args[0] == "--self-test")
{
    var checks = new List<string>();
    void AssertShape(string name, bool pass) { if (!pass) throw new InvalidOperationException(name); checks.Add(name); }
    var found = new HashSet<Type>();
    Expand(typeof(Dictionary<string, List<System.Xml.Schema.XmlSchemaSet[]>>), found);
    AssertShape("nested-generic-array-arguments", found.Contains(typeof(System.Xml.Schema.XmlSchemaSet)) && found.Contains(typeof(Dictionary<,>)) && found.Contains(typeof(List<>)));
    found.Clear(); Expand(typeof(System.Xml.Schema.XmlSchemaSet).MakeArrayType().MakeByRefType(), found);
    AssertShape("byref-array-element", found.Contains(typeof(System.Xml.Schema.XmlSchemaSet)));
    found.Clear(); Expand(typeof(int).MakePointerType(), found);
    AssertShape("pointer-element", found.SetEquals([typeof(int)]));
    found.Clear(); Expand(typeof(System.Xml.Schema.ValidationEventHandler), found);
    AssertShape("named-delegate-invoke-shape", found.Contains(typeof(System.Xml.Schema.ValidationEventArgs)) && found.Contains(typeof(void)));
    found.Clear(); Expand(typeof(ConstraintProbe<>).GetGenericArguments()[0], found);
    AssertShape("generic-parameter-constraint", found.Contains(typeof(IDisposable)));
    var count = found.Count; Expand(typeof(ConstraintProbe<>).GetGenericArguments()[0], found);
    AssertShape("constraint-revisit-termination", found.Count == count);
    Console.WriteLine(JsonSerializer.Serialize(new { passed = checks.Count, failed = 0, checks }));
    return;
}
if (args.Length != 3) throw new ArgumentException("Usage: dependencies <repository-root> <baseline-json> <output-json>");
var root = Path.GetFullPath(args[0]);
using var baseline = JsonDocument.Parse(File.ReadAllBytes(args[1]));
var baselineRows = baseline.RootElement.GetProperty("assemblies").EnumerateArray().ToDictionary(
    a => a.GetProperty("assembly").GetString()!,
    a => a.GetProperty("declarations").EnumerateArray().Select(x => x.GetString()!).ToHashSet(StringComparer.Ordinal));
var result = new List<object>();
var observedRows = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
var artifacts = new List<object>();
foreach (var project in Directory.GetFiles(Path.Combine(root, "src"), "*.csproj", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
{
    var name = Path.GetFileNameWithoutExtension(project);
    observedRows[name] = new HashSet<string>(StringComparer.Ordinal);
    var path = Path.Combine(Path.GetDirectoryName(project)!, "bin", "Release", "net10.0", name + ".dll");
    artifacts.Add(new { assembly = name, sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant(), identity = AssemblyName.GetAssemblyName(path).FullName });
    var context = new ApiContext(Path.GetDirectoryName(path)!);
    try
    {
        var assembly = context.LoadFromAssemblyPath(path);
        foreach (var type in assembly.GetTypes().Where(VisibleType))
        {
            var key = T(type);
            void Add(string row, IEnumerable<Type> direct)
            {
                if (!baselineRows[name].Contains(row)) throw new InvalidOperationException("Signature drift: " + row);
                if (!observedRows[name].Add(row)) throw new InvalidOperationException("Duplicate row: " + row);
                var types = new HashSet<Type>();
                foreach (var dependency in direct) Expand(dependency, types);
                var external = types.Where(t => !t.IsGenericParameter && t.Assembly != assembly && !baselineRows.ContainsKey(t.Assembly.GetName().Name!))
                    .Select(t => new { type = T(t), definitionType = t.IsGenericType ? t.GetGenericTypeDefinition().FullName : t.FullName,
                        resolvedAssembly = t.Assembly.GetName().Name, resolvedAssemblyIdentity = t.Assembly.FullName,
                        isConstructedGeneric = t.IsConstructedGenericType, isSchemaType = (t.Namespace ?? "").StartsWith("System.Xml.Schema", StringComparison.Ordinal) })
                    .OrderBy(t => t.type, StringComparer.Ordinal).ThenBy(t => t.resolvedAssembly, StringComparer.Ordinal).ToArray();
                result.Add(new { entryId = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(name + "\n" + row))).ToLowerInvariant(),
                    assembly = name, declaringType = key, kind = row[..1], signature = row, externalTypes = external });
            }
            Add("T " + key + ":" + type.Attributes + ":layout=" + type.StructLayoutAttribute?.Value + ":pack=" + type.StructLayoutAttribute?.Pack + ":sizeMetadata=" + type.StructLayoutAttribute?.Size + ":base=" + (type.BaseType is null ? "" : T(type.BaseType)) + ":interfaces=" + string.Join(",", type.GetInterfaces().Select(T).Order(StringComparer.Ordinal)) + ":constraints=" + Constraints(type.GetGenericArguments()),
                (type.BaseType is null ? [] : new[] {type.BaseType}).Concat(type.GetInterfaces()).Concat(type.GetGenericArguments()));
            const BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            foreach (var method in type.GetMethods(flags).Cast<MethodBase>().Concat(type.GetConstructors(flags)).Where(Visible))
            {
                var info = method as MethodInfo;
                var row = "M " + key + "." + method.Name + ":" + method.Attributes + ":" + method.CallingConvention + ":" + (info is null ? "void" : Parameter(info.ReturnParameter)) + "(" + string.Join(",", method.GetParameters().Select(Parameter)) + "):" + (info?.IsGenericMethod == true ? Constraints(info.GetGenericArguments()) : "");
                var types = method.GetParameters().SelectMany(ParameterTypes);
                if (info is not null) types = types.Concat(ParameterTypes(info.ReturnParameter)).Concat(info.GetGenericArguments());
                Add(row, types);
            }
            foreach (var field in type.GetFields(flags).Where(f => f.IsPublic || f.IsFamily || f.IsFamilyOrAssembly))
                Add("F " + key + "." + field.Name + ":" + field.Attributes + ":" + T(field.FieldType) + ":" + (field.IsLiteral ? V(field.GetRawConstantValue()) : "") + ":attrs=" + Attr(field.CustomAttributes), new[] {field.FieldType}.Concat(field.GetRequiredCustomModifiers()).Concat(field.GetOptionalCustomModifiers()));
            foreach (var property in type.GetProperties(flags).Where(p => p.GetAccessors(true).Any(Visible)))
                Add("P " + key + "." + property.Name + ":" + T(property.PropertyType) + "(" + string.Join(",", property.GetIndexParameters().Select(Parameter)) + "):attrs=" + Attr(property.CustomAttributes), new[] {property.PropertyType}.Concat(property.GetIndexParameters().SelectMany(ParameterTypes)).Concat(property.GetRequiredCustomModifiers()).Concat(property.GetOptionalCustomModifiers()));
            foreach (var ev in type.GetEvents(flags).Where(e => e.AddMethod is not null && Visible(e.AddMethod)))
                Add("E " + key + "." + ev.Name + ":" + T(ev.EventHandlerType!) + ":attrs=" + Attr(ev.CustomAttributes), [ev.EventHandlerType!]);
        }
    }
    finally { context.Unload(); }
    if (!observedRows[name].SetEquals(baselineRows[name].Where(r => "TMFPE".Contains(r[0]))))
        throw new InvalidOperationException("Incomplete public declaration coverage: " + name);
}
var output = new { formatVersion = 1,
    scope = "Observed external public-signature dependencies, recursively expanding generic arguments, element types, generic constraints, delegate Invoke shapes and required/optional modifiers; resolved implementation assemblies are not NuGet/reference-pack identities",
    runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
    baselineSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(args[1]))).ToLowerInvariant(),
    observedAssemblies = artifacts, declarations = result };
File.WriteAllText(args[2], JsonSerializer.Serialize(output, new JsonSerializerOptions {WriteIndented=true}) + "\n");
Console.WriteLine($"Inspected {result.Count} exact baseline public declarations across {observedRows.Count} assemblies.");

static IEnumerable<Type> ParameterTypes(ParameterInfo p) => new[] {p.ParameterType}.Concat(p.GetRequiredCustomModifiers()).Concat(p.GetOptionalCustomModifiers());
static void Expand(Type type, HashSet<Type> found)
{
    if (type.IsGenericParameter)
    {
        if (!found.Add(type)) return;
        foreach (var constraint in type.GetGenericParameterConstraints()) Expand(constraint, found);
        return;
    }
    if (type.HasElementType) { Expand(type.GetElementType()!, found); return; }
    if (type.IsFunctionPointer)
    {
        Expand(type.GetFunctionPointerReturnType(), found);
        foreach (var parameter in type.GetFunctionPointerParameterTypes()) Expand(parameter, found);
        return;
    }
    if (!found.Add(type)) return;
    if (typeof(MulticastDelegate).IsAssignableFrom(type) && type.GetMethod("Invoke") is { } invoke)
    {
        foreach (var shape in ParameterTypes(invoke.ReturnParameter).Concat(invoke.GetParameters().SelectMany(ParameterTypes))) Expand(shape, found);
    }
    if (type.IsGenericType)
    {
        found.Add(type.GetGenericTypeDefinition());
        foreach (var argument in type.GetGenericArguments()) Expand(argument, found);
    }
}
static string T(Type type) => type.IsByRef ? T(type.GetElementType()!) + "&" : type.IsPointer ? T(type.GetElementType()!) + "*"
    : type.IsArray ? T(type.GetElementType()!) + "[" + new string(',', type.GetArrayRank() - 1) + "]"
    : type.IsGenericParameter ? (type.DeclaringMethod is null ? "!" : "!!") + type.GenericParameterPosition
    : type.IsGenericType ? type.GetGenericTypeDefinition().FullName + "<" + string.Join(",", type.GetGenericArguments().Select(T)) + ">" : type.FullName ?? type.Name;
static string Attr(IEnumerable<CustomAttributeData> attrs) => string.Join(";", attrs.Select(a => a.ToString()).Order(StringComparer.Ordinal));
static string V(object? value) => value is null ? "null" : value is Type t ? T(t) : value is string s ? JsonSerializer.Serialize(s) : Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? "";
static string Constraints(Type[] types) => string.Join(";", types.Where(t => t.IsGenericParameter).Select(t => T(t) + ":" + t.GenericParameterAttributes + ":" + string.Join(",", t.GetGenericParameterConstraints().Select(T).Order(StringComparer.Ordinal))));
static string Parameter(ParameterInfo p) => T(p.ParameterType) + " " + p.Name + ":" + p.Attributes + ":" + (p.HasDefaultValue ? V(p.RawDefaultValue) : "none")
    + ":required=" + string.Join(",", p.GetRequiredCustomModifiers().Select(T)) + ":optional=" + string.Join(",", p.GetOptionalCustomModifiers().Select(T)) + ":attrs=" + Attr(p.CustomAttributes);
static bool Visible(MethodBase method) => method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly;
static bool VisibleType(Type t) => t.IsNested ? (t.IsNestedPublic || t.IsNestedFamily || t.IsNestedFamORAssem) && VisibleType(t.DeclaringType!) : t.IsPublic;
internal sealed class ApiContext(string root) : AssemblyLoadContext(isCollectible: true)
{
    /// <inheritdoc/>
    protected override Assembly? Load(AssemblyName name)
    {
        var file = Path.Combine(root, name.Name + ".dll");
        if (File.Exists(file)) return LoadFromAssemblyPath(file);
        var src = Path.GetFullPath(Path.Combine(root, "../../../../"));
        foreach (var project in Directory.GetDirectories(src)) {
            file = Path.Combine(project, "bin", "Release", "net10.0", name.Name + ".dll");
            if (File.Exists(file)) return LoadFromAssemblyPath(file);
        }
        return null;
    }
}

internal sealed class ConstraintProbe<T> where T : IDisposable { }

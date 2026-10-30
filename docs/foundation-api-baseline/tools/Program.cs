// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 30 October 2026
// PURPOSE              : Inventories compiled Bosak API metadata for the proposed shared foundation scope.
// SPECIAL NOTES        : Read-only API audit; does not approve a freeze or consumer compatibility.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 30-10-2026     | Creation; compiled API baseline inventory                                   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;

if (args.Length != 2) throw new ArgumentException("Usage: inventory <repository-root> <output-json>");
var root = Path.GetFullPath(args[0]);
var projects = Directory.GetFiles(Path.Combine(root, "src"), "*.csproj", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray();
var assemblies = projects.Select(project => {
    var name = Path.GetFileNameWithoutExtension(project);
    var path = Path.Combine(Path.GetDirectoryName(project)!, "bin", "Release", "net10.0", name + ".dll");
    var declarations = Read(path);
    return new { project = Path.GetRelativePath(root, project).Replace('\\', '/'), assembly = name,
        sha256 = Hash(path), identity = AssemblyName.GetAssemblyName(path).FullName,
        publicTypes = declarations.Count(x => x.StartsWith("T ", StringComparison.Ordinal)),
        declarationCount = declarations.Length, declarations };
}).ToArray();
File.WriteAllText(args[1], JsonSerializer.Serialize(new { formatVersion = 1,
    scope = "Observed compiled public/protected API including defaults, enum constants, attributes and value-type field shape; not freeze acceptance, behavior, computed managed size, package or consumer ABI qualification",
    assemblies }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
Console.WriteLine($"Inventoried {assemblies.Length} assemblies; {assemblies.Sum(a => a.publicTypes)} exported types; {assemblies.Sum(a => a.declarationCount)} entries.");
return 0;
static string Hash(string file) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file)));
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
static string[] Read(string path)
{
    var context = new ApiContext(Path.GetDirectoryName(Path.GetFullPath(path))!);
    try
    {
        var assembly = context.LoadFromAssemblyPath(Path.GetFullPath(path)); var rows = new List<string>();
        foreach (var type in assembly.GetTypes().Where(VisibleType))
        {
            var key = T(type); rows.Add("A " + key + ":" + Attr(type.CustomAttributes));
            if (type.IsValueType) foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)) rows.Add("L " + key + "." + f.Name + ":" + T(f.FieldType) + ":" + f.Attributes + ":" + Attr(f.CustomAttributes));
            rows.Add("T " + key + ":" + type.Attributes + ":layout=" + type.StructLayoutAttribute?.Value + ":pack=" + type.StructLayoutAttribute?.Pack + ":sizeMetadata=" + type.StructLayoutAttribute?.Size + ":base=" + (type.BaseType is null ? "" : T(type.BaseType)) + ":interfaces=" + string.Join(",", type.GetInterfaces().Select(T).Order(StringComparer.Ordinal)) + ":constraints=" + Constraints(type.GetGenericArguments()));
            const BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            foreach (var method in type.GetMethods(flags).Cast<MethodBase>().Concat(type.GetConstructors(flags)).Where(Visible))
            {
                var info = method as MethodInfo;
                rows.Add("A " + key + "." + method.Name + "(" + string.Join(",",method.GetParameters().Select(Parameter)) + "):" + Attr(method.CustomAttributes));
                rows.Add("M " + key + "." + method.Name + ":" + method.Attributes + ":" + method.CallingConvention + ":" + (info is null ? "void" : Parameter(info.ReturnParameter)) + "(" + string.Join(",", method.GetParameters().Select(Parameter)) + "):" + (info?.IsGenericMethod == true ? Constraints(info.GetGenericArguments()) : ""));
            }
            foreach (var field in type.GetFields(flags).Where(f => f.IsPublic || f.IsFamily || f.IsFamilyOrAssembly))
                rows.Add("F " + key + "." + field.Name + ":" + field.Attributes + ":" + T(field.FieldType) + ":" + (field.IsLiteral ? V(field.GetRawConstantValue()) : "") + ":attrs=" + Attr(field.CustomAttributes));
            foreach (var property in type.GetProperties(flags).Where(p => p.GetAccessors(true).Any(Visible)))
                rows.Add("P " + key + "." + property.Name + ":" + T(property.PropertyType) + "(" + string.Join(",", property.GetIndexParameters().Select(Parameter)) + "):attrs=" + Attr(property.CustomAttributes));
            foreach (var ev in type.GetEvents(flags).Where(e => e.AddMethod is not null && Visible(e.AddMethod)))
                rows.Add("E " + key + "." + ev.Name + ":" + T(ev.EventHandlerType!) + ":attrs=" + Attr(ev.CustomAttributes));
        }
        return rows.Order(StringComparer.Ordinal).ToArray();
    }
    finally { context.Unload(); }
}
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

// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 November 2026
// PURPOSE              : Records normalized public Core API rows for additive compatibility review.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;

internal static class Program
{
    private static string Name(Type t) => t.IsByRef ? Name(t.GetElementType()!) + "&" : t.IsArray ? Name(t.GetElementType()!) + "[" + new string(',', t.GetArrayRank()-1) + "]" : t.IsGenericType ? t.GetGenericTypeDefinition().FullName + "<" + string.Join(",", t.GetGenericArguments().Select(Name)) + ">" : t.FullName ?? t.Name;
    private static int Main(string[] args)
    {
        try
        {
            var context = new AssemblyLoadContext("surface",true);
            context.Resolving += (loader,name) => { var candidate=Path.Combine(Path.GetDirectoryName(Path.GetFullPath(args[0]))!,name.Name+".dll");return File.Exists(candidate)?loader.LoadFromAssemblyPath(candidate):null; };
            var a = context.LoadFromAssemblyPath(Path.GetFullPath(args[0])); var types = a.GetExportedTypes().OrderBy(t=>t.FullName).ToArray(); var rows=new List<string>();
            const BindingFlags f = BindingFlags.Public|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly;
            string Params(MethodBase m) => string.Join(",",m.GetParameters().Select(p=>Name(p.ParameterType)+" "+p.Name+(p.IsOptional?"="+(p.DefaultValue?.ToString()??"null"):"")));
            foreach(var t in types)
            {
                rows.Add("T|"+Name(t));
                foreach(var c in t.GetConstructors(f))rows.Add("C|"+Name(t)+"|"+Params(c));
                foreach(var m in t.GetMethods(f).Where(m=>!(m.IsSpecialName&&(m.Name.StartsWith("get_")||m.Name.StartsWith("set_")||m.Name.StartsWith("add_")||m.Name.StartsWith("remove_")))))rows.Add("M|"+Name(t)+"|"+m.Name+"|"+Name(m.ReturnType)+"|"+Params(m));
                foreach(var p in t.GetProperties(f))rows.Add("P|"+Name(t)+"|"+p.Name+"|"+Name(p.PropertyType)+"|"+p.CanRead+"|"+p.CanWrite);
                foreach(var field in t.GetFields(f))rows.Add("F|"+Name(t)+"|"+field.Name+"|"+Name(field.FieldType)+"|"+(field.IsLiteral?field.GetRawConstantValue()?.ToString():"readonly="+field.IsInitOnly));
            }
            File.WriteAllText(args[1],JsonSerializer.Serialize(new{types=types.Select(t=>Name(t)),rows=rows.OrderBy(x=>x)},new JsonSerializerOptions{WriteIndented=true}));return 0;
        }
        catch(Exception e){Console.Error.WriteLine("Caught API diagnostic failure: "+e);return 1;}
    }
}

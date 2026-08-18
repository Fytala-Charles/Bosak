// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 mei 2026
// PURPOSE              : Represents a single QT3 test-case with its metadata and expected result.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 20-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 17-07-2026     | Accept inherited test-set dependencies for dependency filtering                          |
//                      | Charles Korthout | 0.3   | 21-07-2026     | Add BaseDirectory for resolving assert-xml file references                               |
//                      | Charles Korthout | 0.4   | 25-07-2026     | Add OwnDependencies (case-level spec deps) for XPath-vs-XQuery pipeline routing          |
//                      | Charles Korthout | 0.5   | 25-07-2026     | OwnDependencies for XPath-only pipeline routing                                          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.6   | 27-07-2026     | Parse <module> catalog entries (uri, location, resolved file path)                       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.7   | 18-08-2026     | Load <test file="..."> query text from referenced file                                   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;

namespace Bosak.XPath.Conformance;

internal sealed class TestCase
{
    public string Name { get; }
    public string Description { get; }
    public string Expression { get; }
    public IReadOnlyList<Dependency> Dependencies { get; }
    public IReadOnlyList<Dependency> OwnDependencies { get; }
    public XElement ResultElement { get; }
    public string BaseDirectory { get; }
    public IReadOnlyList<TestCaseModule> Modules { get; }

    private TestCase(string name, string description, string expression, List<Dependency> dependencies, IReadOnlyList<Dependency> ownDependencies, XElement resultElement, string baseDirectory, IReadOnlyList<TestCaseModule> modules)
    {
        Name = name;
        Description = description;
        Expression = expression;
        Dependencies = dependencies;
        OwnDependencies = ownDependencies;
        ResultElement = resultElement;
        BaseDirectory = baseDirectory;
        Modules = modules;
    }

    private static string ReadTestExpression(XElement? testElement, string baseDirectory)
    {
        if (testElement is null)
            return "";

        string? file = (string?)testElement.Attribute("file");
        if (!string.IsNullOrEmpty(file))
        {
            string resolvedPath = Path.IsPathRooted(file) ? file! : Path.Combine(baseDirectory, file!);
            return File.Exists(resolvedPath) ? File.ReadAllText(resolvedPath) : "";
        }

        return (string?)testElement ?? "";
    }

    public static TestCase FromElement(XElement element, XNamespace ns, IEnumerable<Dependency> inheritedDependencies, string baseDirectory)
    {
        string name = (string?)element.Attribute("name") ?? "unknown";
        string description = (string?)element.Element(ns + "description") ?? "";
        string expression = ReadTestExpression(element.Element(ns + "test"), baseDirectory);

        var dependencies = new List<Dependency>(inheritedDependencies);
        var ownDependencies = new List<Dependency>();
        foreach (var depElem in element.Elements(ns + "dependency"))
        {
            var dep = Dependency.FromElement(depElem);
            dependencies.Add(dep);
            ownDependencies.Add(dep);
        }

        // Library modules referenced by the query's module imports; the file path is
        // relative to the test-set document (catalog-schema xsd for <module>).
        var modules = new List<TestCaseModule>();
        foreach (var moduleElem in element.Elements(ns + "module"))
        {
            string uri = (string?)moduleElem.Attribute("uri") ?? "";
            string? location = (string?)moduleElem.Attribute("location");
            string file = (string?)moduleElem.Attribute("file") ?? "";
            string resolvedPath = Path.IsPathRooted(file) ? file : Path.Combine(baseDirectory, file);
            modules.Add(new TestCaseModule(uri, location, resolvedPath));
        }

        var resultElem = element.Element(ns + "result") ?? new XElement(ns + "result");

        return new TestCase(name, description, expression, dependencies, ownDependencies, resultElem, baseDirectory, modules);
    }
}

/// <summary>A QT3 <c>&lt;module uri="..." location="..." file="..."/&gt;</c> entry with the file path resolved.</summary>
internal sealed record TestCaseModule(string Uri, string? Location, string FilePath);

internal sealed class Dependency
{
    public string Type { get; }
    public string Value { get; }
    public bool Satisfied { get; }

    public Dependency(string type, string value, bool satisfied)
    {
        Type = type;
        Value = value;
        Satisfied = satisfied;
    }

    public static Dependency FromElement(XElement element)
    {
        string type = (string?)element.Attribute("type") ?? "";
        string value = (string?)element.Attribute("value") ?? "";
        string? satisfiedAttr = (string?)element.Attribute("satisfied");
        bool satisfied = satisfiedAttr is null || satisfiedAttr == "true";
        return new Dependency(type, value, satisfied);
    }
}

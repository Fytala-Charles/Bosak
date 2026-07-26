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

    private TestCase(string name, string description, string expression, List<Dependency> dependencies, IReadOnlyList<Dependency> ownDependencies, XElement resultElement, string baseDirectory)
    {
        Name = name;
        Description = description;
        Expression = expression;
        Dependencies = dependencies;
        OwnDependencies = ownDependencies;
        ResultElement = resultElement;
        BaseDirectory = baseDirectory;
    }

    public static TestCase FromElement(XElement element, XNamespace ns, IEnumerable<Dependency> inheritedDependencies, string baseDirectory)
    {
        string name = (string?)element.Attribute("name") ?? "unknown";
        string description = (string?)element.Element(ns + "description") ?? "";
        string expression = (string?)element.Element(ns + "test") ?? "";

        var dependencies = new List<Dependency>(inheritedDependencies);
        var ownDependencies = new List<Dependency>();
        foreach (var depElem in element.Elements(ns + "dependency"))
        {
            var dep = Dependency.FromElement(depElem);
            dependencies.Add(dep);
            ownDependencies.Add(dep);
        }

        var resultElem = element.Element(ns + "result") ?? new XElement(ns + "result");

        return new TestCase(name, description, expression, dependencies, ownDependencies, resultElem, baseDirectory);
    }
}

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

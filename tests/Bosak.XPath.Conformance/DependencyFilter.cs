// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 mei 2026
// PURPOSE              : Filters QT3 tests based on Bosak's supported XPath 3.1 feature set.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 20-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 22-05-2026     | Skip XSD 1.0 tests (Bosak implements XSD 1.1 per XPath 3.1)                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Conformance;

internal sealed class DependencyFilter
{
    // Features that Bosak XPath 3.1 does NOT support
    private static readonly HashSet<string> UnsupportedFeatures = new(StringComparer.OrdinalIgnoreCase)
    {
        "schemaAware",
        "schema-import",
        "schema-validation",
        "static-typing",
        "staticTyping",
        "typedData",
        "serialization",
        "moduleImport",
        "xpath-1.0-compatibility",
        "advanced-uca-fallback",
        "olson-timezone",
    };

    // Spec tokens that Bosak supports (XPath versions)
    private static readonly HashSet<string> SupportedSpecs = new(StringComparer.OrdinalIgnoreCase)
    {
        "XP20+",
        "XP30+",
        "XP31+",
    };

    // Spec tokens that are XQuery-only and not supported
    private static readonly HashSet<string> XqueryOnlySpecs = new(StringComparer.OrdinalIgnoreCase)
    {
        "XQ10", "XQ10+",
        "XQ30", "XQ30+",
        "XQ31", "XQ31+",
    };

    public bool IsSupported(IReadOnlyList<Dependency> dependencies)
    {
        bool hasSpecDependency = false;
        bool specSupported = false;

        foreach (var dep in dependencies)
        {
            if (!dep.Satisfied)
            {
                // Negative dependency: if we DO support this feature, skip the test
                if (IsUnsupportedFeature(dep))
                    continue; // we don't support it, so negative dependency is satisfied

                if (IsXqueryOnlySpec(dep))
                    continue; // we don't support XQuery, so negative dependency is satisfied

                // Negative dependency on something we support -> skip
                return false;
            }

            if (dep.Type == "feature" && UnsupportedFeatures.Contains(dep.Value))
                return false;

            if (dep.Type == "spec")
            {
                hasSpecDependency = true;
                var tokens = dep.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var token in tokens)
                {
                    if (SupportedSpecs.Contains(token))
                    {
                        specSupported = true;
                    }
                }
            }

            if (dep.Type == "xml-version")
            {
                // Only support XML 1.0
                if (dep.Value != "1.0")
                    return false;
            }

            if (dep.Type == "xsd-version")
            {
                // Bosak implements XSD 1.1 (required by XPath 3.1); skip XSD 1.0-only tests
                if (dep.Value == "1.0")
                    return false;
                if (dep.Value != "1.1")
                    return false;
            }
        }

        // If there are spec dependencies, at least one must be a supported XPath version
        if (hasSpecDependency && !specSupported)
            return false;

        return true;
    }

    private static bool IsUnsupportedFeature(Dependency dep)
    {
        return dep.Type == "feature" && UnsupportedFeatures.Contains(dep.Value);
    }

    private static bool IsXqueryOnlySpec(Dependency dep)
    {
        if (dep.Type != "spec")
            return false;
        var tokens = dep.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return tokens.All(t => XqueryOnlySpecs.Contains(t));
    }
}

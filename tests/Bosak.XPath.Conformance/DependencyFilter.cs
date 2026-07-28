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
//                      | Charles Korthout | 0.2   | 22-05-2026     | Skip XSD 1.0 tests (Bosak implements XSD 1.1 per XPath 3.1)                            |
//                      | Charles Korthout | 0.3   | 17-07-2026     | Skip arbitraryPrecisionDecimal tests (.NET decimal is fixed-precision 128-bit)          |
//                      | Charles Korthout | 0.4   | 18-07-2026     | Skip XQ31-only positive spec dependencies (tests using XQuery direct constructors)        |
//                      | Charles Korthout | 0.6   | 19-07-2026     | AND spec dependencies across dependency elements (fixes XP30-only tests in XP31+ mode) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.7   | 21-07-2026     | Skip tests that declare a specific unicode-version dependency                            |
//                      | Charles Korthout | 0.8   | 21-07-2026     | Skip tests that declare a specific xml-version dependency (Bosak uses XML 1.1)              |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.9   | 25-07-2026     | Optional allowXQuerySpecs mode: positive XQuery-only spec deps are treated as satisfied |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.10  | 25-07-2026     | XQuery 3.1 spec-token awareness: exact XQ10/XQ30-only tests are not applicable          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.11  | 25-07-2026     | serialization feature supported; XML 1.1 xml-version admitted                           |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.12  | 27-07-2026     | moduleImport feature supported (library modules implemented)                            |
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
        "xpath-1.0-compatibility",
        "advanced-uca-fallback",
        "olson-timezone",
        "arbitraryPrecisionDecimal",
        "fn-load-xquery-module",
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

    // XQuery spec tokens satisfied by Bosak's XQuery 3.1 processor: XQ31 and any
    // "or later" range that includes 3.1. Exact earlier versions (XQ10, XQ30) do
    // not match — tests gated to those versions assert pre-3.1 semantics.
    private static readonly HashSet<string> SupportedXQuerySpecs = new(StringComparer.OrdinalIgnoreCase)
    {
        "XQ10+", "XQ30+", "XQ31", "XQ31+",
    };

    public bool IsSupported(IReadOnlyList<Dependency> dependencies, bool allowXQuerySpecs = false)
    {

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
                // A positive dependency that is purely XQuery-only means the test uses
                // XQuery syntax (e.g. direct element constructors) and is not applicable
                // to an XPath-only processor. When the caller can route the query to the
                // XQuery pipeline (allowXQuerySpecs), these dependencies are satisfied.
                if (IsXqueryOnlySpec(dep))
                {
                    if (!allowXQuerySpecs)
                        return false;
                    // Bosak implements XQuery 3.1: exact earlier-version tokens
                    // (XQ10, XQ30) are not satisfied; '+' ranges and XQ31 are.
                    // XPath tokens in a mixed dependency follow the XPath rules.
                    var xqTokens = dep.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (!xqTokens.Any(t => SupportedXQuerySpecs.Contains(t) || SupportedSpecs.Contains(t)))
                        return false;
                    continue;
                }

                var tokens = dep.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                bool thisDepSupported = false;
                foreach (var token in tokens)
                {
                    if (SupportedSpecs.Contains(token))
                    {
                        thisDepSupported = true;
                        break;
                    }
                }

                // Spec dependencies are AND-ed: each dependency element must be satisfied.
                if (!thisDepSupported)
                    return false;
            }

            if (dep.Type == "xml-version")
            {
                // Bosak uses XML 1.1 throughout; XML 1.0-only tests are not applicable.
                // Tests allowing 1.1 (value "1.1" or "1.0 1.1") are supported.
                var tokens = dep.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (!tokens.Contains("1.1"))
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

            if (dep.Type == "default-language")
            {
                // Bosak only supports "en" as default language
                if (dep.Value != "en")
                    return false;
            }

            if (dep.Type == "unicode-version")
            {
                // Bosak uses .NET's case folding / Unicode normalization; we do not report or
                // guarantee a specific Unicode version. Skip tests that depend on one.
                return false;
            }
        }

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
        // A spec dependency that mentions any XQuery version is not applicable to an
        // XPath-only processor, even when combined with an XPath version (e.g. XQ10+ XP30+).
        return tokens.Any(t => XqueryOnlySpecs.Contains(t));
    }

    /// <summary>
    /// True when any dependency carries positive XQuery-only spec tokens. Such tests may
    /// use XQuery-only grammar (e.g. multi-clause FLWOR) even when the query text does
    /// not otherwise look like XQuery, so they must run on the XQuery pipeline.
    /// </summary>
    public static bool HasXQueryOnlySpecDependency(IReadOnlyList<Dependency> dependencies)
    {
        return dependencies.Any(d => d.Satisfied && IsXqueryOnlySpec(d));
    }
}

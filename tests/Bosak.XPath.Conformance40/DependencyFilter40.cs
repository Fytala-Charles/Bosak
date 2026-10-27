// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 09 oktober 2026
// PURPOSE              : Filters qt4tests cases based on Bosak's supported XPath 4.0 feature set.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 09-10-2026     | Creation (REQ-123 4.0-Exp S2): XP40+ spec tokens; 4.0 features Bosak lacks are skipped;   |
//                      |                  |       |                | no XQuery routing — XQ-only tests skip (XQuery 4.0 mode does not exist yet)              |
//                      | Charles Korthout | 0.2   | 10-10-2026     | REQ-123 sort-with slice: typedData declared unsupported (no PSVI-typed source nodes)     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 27-10-2026     | Invert spec/default-language predicates consistently; clarify schema profile ownership. |
//                      | Charles Korthout | 0.4   | 27-10-2026     | Require recognized feature names; preserve explicit frozen catalog admissions.         |
//                      | Charles Korthout | 0.5   | 27-10-2026     | Admit existing bounded non-codepoint collations; retain language and URI guards.       |
//                      | Charles Korthout | 0.6   | 27-10-2026     | Admit reviewed bounded CLDR integer cases; preserve other profile and language guards. |
//                      | Charles Korthout | 0.7   | 27-10-2026     | Recognize reviewed unsupported FBF4 sequence with correct dependency polarity.           |
//                      | Charles Korthout | 0.8   | 27-10-2026     | Recognize reviewed unsupported CB calendar with correct dependency polarity.             |
// ===========================================================================================================================================================

namespace Bosak.XPath.Conformance;

/// <summary>
/// qt4tests dependency filtering for the XPath 4.0 harness. Deliberately stricter than
/// the QT3 <c>DependencyFilter</c>: a spec dependency is satisfied only when at least one
/// XPath token is satisfiable (<c>XP20+</c>…<c>XP40+</c>); tests that carry only XQuery
/// spec tokens (including <c>XQ40+</c>) are skipped because the harness has no XQuery
/// pipeline — Bosak.XQuery is 3.1 and has no 4.0 mode.
/// </summary>
internal sealed class DependencyFilter40
{
    // Features that Bosak XPath 4.0 does NOT support.
    private static readonly HashSet<string> UnsupportedFeatures = new(StringComparer.OrdinalIgnoreCase)
    {
        "schema-location-hint", // remote schema location hints not supported
        "static-typing",
        "staticTyping",
        "xpath-1.0-compatibility",
        "olson-timezone",
        "arbitraryPrecisionDecimal",
        // XPath/XQuery 4.0 features not supported by Bosak:
        "XQUpdate",                          // XQuery Update not implemented
        "fullText",                          // XQuery Full Text not implemented
        "fullText-ignore",
        "fullText-units",
        "fullText-defaults",
        "binary",                            // EXPath-style binary data model not implemented
        "typedData",                         // harness does not build schema-validated (PSVI-typed) source nodes
    };

    // Explicit legacy catalog admission, not a certification of complete feature support.
    // Keep schema/backend and harness-specific limits in the capability review ledger.
    // Unrecognized names require review before either polarity can admit a test.
    private static readonly HashSet<string> FrozenAdmittedFeatures = new(StringComparer.OrdinalIgnoreCase)
    {
        "advanced-uca-fallback",
        "collection-stability",
        "directory-as-collection-uri",
        "expath-file",
        "fn-format-integer-CLDR", // Reviewed bounded word schemes; not complete CLDR/RBNF.
        "fn-load-xquery-module",
        "fn-transform-XSLT",
        "fn-transform-XSLT30",
        "higherOrderFunctions",
        "infoset-dtd",
        "moduleImport",
        "namespace-axis",
        "non_empty_sequence_collection",
        "non_unicode_codepoint_collation", // Existing HTML ASCII/UCA routes; URI support is bounded.
        "remote_http",
        "schemaImport",
        "schemaValidation",
        "serialization",
        "simple-uca-fallback",
    };

    // XPath spec tokens satisfiable at the 4.0 compatibility level (a superset of 3.1,
    // so every "or later" range up to and including XP40+ applies).
    private static readonly HashSet<string> SupportedSpecs = new(StringComparer.OrdinalIgnoreCase)
    {
        "XP20+",
        "XP30+",
        "XP31+",
        "XP40+",
    };

    // Spec tokens that are XQuery-only. The 4.0 harness runs the XPath pipeline only,
    // so any dependency mentioning these is not satisfiable here.
    private static readonly HashSet<string> XqueryOnlySpecs = new(StringComparer.OrdinalIgnoreCase)
    {
        "XQ10", "XQ10+",
        "XQ30", "XQ30+",
        "XQ31", "XQ31+",
        "XQ40", "XQ40+",
    };

    public bool IsSupported(IReadOnlyList<Dependency> dependencies)
    {
        foreach (var dep in dependencies)
        {
            // CB is a reviewed unsupported calendar: the formatter falls back to AD.
            // Other calendar names retain their existing admission policy.
            if (dep.Type == "calendar" && dep.Value == "CB")
            {
                if (dep.Satisfied)
                    return false;
                continue;
            }

            // This reviewed numbering sequence is unsupported; the engine uses decimal fallback.
            // Keep other sequence names and dependency kinds under their existing policy.
            if (dep.Type == "format-integer-sequence" && dep.Value == "\uFBF4")
            {
                if (dep.Satisfied)
                    return false;
                continue;
            }

            if (dep.Type == "feature"
                && !FrozenAdmittedFeatures.Contains(dep.Value)
                && !UnsupportedFeatures.Contains(dep.Value))
                return false; // Unknown is not evidence of support or non-support.

            if (!dep.Satisfied)
            {
                // Apply polarity to the same profile predicate used by positive dependencies.
                // Other dependency kinds retain their existing capability policy.
                if (dep.Type is "spec" or "default-language")
                {
                    var positive = new Dependency(dep.Type, dep.Value, satisfied: true);
                    if (IsSupported(new[] { positive }))
                        return false;
                    continue;
                }

                // Negative dependency: if we DO NOT support this feature/spec, the
                // negation holds and the test stays admissible.
                if (IsUnsupportedFeature(dep))
                    continue;
                if (IsXqueryOnlySpec(dep))
                    continue; // no XQuery pipeline in this harness
                // Negative dependency on something we support -> skip
                return false;
            }

            if (dep.Type == "feature" && UnsupportedFeatures.Contains(dep.Value))
                return false;

            if (dep.Type == "spec")
            {
                var tokens = dep.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                bool thisDepSupported = tokens.Any(t => SupportedSpecs.Contains(t));
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
                // Frozen catalog admission profile; this is not an XSD 1.1 capability claim.
                // Bosak.Schema owns staged XSD 1.1; the current core seam uses XmlSchemaSet.
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
        return tokens.Any(t => XqueryOnlySpecs.Contains(t));
    }
}

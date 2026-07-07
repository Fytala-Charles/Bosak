// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Options that control how an XPath expression is compiled
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 24-06-2026     | Added DefiningElementDefaultNamespace for element-available default namespace            |
//                      | Charles Korthout | 0.3   | 26-06-2026     | Added BackwardsCompatible for XSLT 1.0 static constant folding                         |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Api;

/// <summary>
/// Options that control how an XPath expression is compiled.
/// </summary>
public sealed class CompileOptions
{
    /// <summary>
    /// Default options: XPath 3.1 compatibility, no static context.
    /// </summary>
    public static CompileOptions Default { get; } = new();

    /// <summary>
    /// The XPath language version to target. Defaults to <see cref="XPathCompatibility.XPath31"/>.
    /// </summary>
    public XPathCompatibility Compatibility { get; init; } = XPathCompatibility.XPath31;

    /// <summary>
    /// Namespace bindings available during static analysis.
    /// Key = prefix, Value = namespace URI.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Namespaces { get; init; }

    /// <summary>
    /// The default element namespace URI for unprefixed element and type names
    /// in XPath expressions. When null or empty, unprefixed names match no namespace.
    /// </summary>
    public string? DefaultElementNamespace { get; init; }

    /// <summary>
    /// The default namespace URI of the element that contains the XPath expression.
    /// Used by XSLT's <c>fn:element-available</c> to expand unprefixed lexical QNames
    /// per the XSLT specification, which differs from the XPath default element namespace.
    /// </summary>
    public string? DefiningElementDefaultNamespace { get; init; }

    /// <summary>
    /// Statically-known variable types for optimization and early error detection.
    /// Key = variable QName (e.g., "xs:QName"), Value = XDM type.
    /// </summary>
    public IReadOnlyDictionary<string, string>? StaticVariableTypes { get; init; }

    /// <summary>
    /// If true, the compiler performs additional validations and may
    /// reject expressions that rely on dynamic context information.
    /// </summary>
    public bool StrictStaticTyping { get; init; }

    /// <summary>
    /// The static base URI for resolving relative URIs in expressions
    /// (e.g. <c>resolve-uri('file.xml')</c>). When null, the runtime
    /// context's <see cref="EvaluationContext.BaseUri"/> is used.
    /// </summary>
    public string? BaseUri { get; init; }

    /// <summary>
    /// If true, enables IL JIT compilation for hot expressions after
    /// a threshold of executions. Defaults to false (register VM only).
    /// </summary>
    public bool EnableJit { get; init; }

    /// <summary>
    /// If true, the expression is compiled in XPath 1.0 backwards-compatible
    /// mode. This affects static type folding (e.g. integer arithmetic is
    /// promoted to <c>xs:double</c>) and may influence parser diagnostics.
    /// </summary>
    public bool BackwardsCompatible { get; init; }
}

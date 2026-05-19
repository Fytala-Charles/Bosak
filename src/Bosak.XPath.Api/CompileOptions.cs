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
    /// If true, enables IL JIT compilation for hot expressions after
    /// a threshold of executions. Defaults to false (register VM only).
    /// </summary>
    public bool EnableJit { get; init; }
}

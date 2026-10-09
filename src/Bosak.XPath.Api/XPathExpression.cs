// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 09 oktober 2026
// PURPOSE              : Version-neutral entry point for compiling and evaluating XPath expressions
// SPECIAL NOTES        : Public surface API for compiling and evaluating XPath 3.1 expressions.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 09-10-2026     | Creation: v1.0.0 API naming resolution (REQ-118 dossier §5.4) — thin facade over           |
//                      |                  |       |                | XPath31Expression with a version-neutral name; XPath31Expression is unchanged            |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.XPath.Api;

/// <summary>
/// Version-neutral entry point for compiling and evaluating XPath expressions.
/// Compiles XPath 3.1 by default; set <see cref="CompileOptions.Compatibility"/> to
/// <see cref="XPathCompatibility.XPath40"/> to opt into the XPath 4.0 surfaces.
/// This is a thin facade over <see cref="XPath31Expression"/> with an identical
/// surface, so consumers can adopt it by renaming the type only; both types remain
/// fully supported.
/// </summary>
public sealed class XPathExpression
{
    private readonly XPath31Expression _inner;

    private XPathExpression(XPath31Expression inner)
    {
        _inner = inner;
    }

    /// <summary>
    /// Parses and compiles an XPath expression with default options (XPath 3.1).
    /// </summary>
    /// <param name="expression">The XPath expression to compile.</param>
    /// <returns>The compiled expression, ready for repeated evaluation.</returns>
    public static XPathExpression Compile(string expression)
        => new(XPath31Expression.Compile(expression));

    /// <summary>
    /// Parses and compiles an XPath expression with the specified options.
    /// </summary>
    /// <param name="expression">The XPath expression to compile.</param>
    /// <param name="options">The compilation options (language version, namespaces, schema set, …).</param>
    /// <returns>The compiled expression, ready for repeated evaluation.</returns>
    public static XPathExpression Compile(string expression, CompileOptions options)
        => new(XPath31Expression.Compile(expression, options));

    /// <summary>
    /// Evaluates the compiled expression against the given context item (typically a document node).
    /// </summary>
    /// <param name="contextItem">The initial context item.</param>
    /// <returns>The evaluation result.</returns>
    public XdmValue Evaluate(IXdmNode contextItem)
        => _inner.Evaluate(contextItem);

    /// <summary>
    /// Evaluates the compiled expression with a custom evaluation context.
    /// </summary>
    /// <param name="context">The evaluation context (variables, namespaces, focus, …).</param>
    /// <returns>The evaluation result.</returns>
    public XdmValue Evaluate(EvaluationContext context)
        => _inner.Evaluate(context);

    /// <summary>
    /// Evaluates the compiled expression and returns the result as a node sequence.
    /// </summary>
    /// <param name="contextItem">The initial context item.</param>
    /// <returns>The node sequence result; non-node results yield an empty sequence.</returns>
    public XdmSequence EvaluateNodes(IXdmNode contextItem)
        => _inner.EvaluateNodes(contextItem);
}

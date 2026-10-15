// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 15 October 2026
// PURPOSE              : Prove function and namespace availability for the bounded insertion expression grammar.
// SPECIAL NOTES        : Part of the Bosak XSLT source-preserving authoring API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 15-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Parser;
using Bosak.XPath.Parser.Ast;
using Bosak.XPath.Standard.Functions;

namespace Bosak.Xslt.Authoring;

internal static class InsertionExpressionCoverage
{
    internal static void Check(AuthoringSnapshot snapshot, AuthoringAttributeDescriptor slot, CancellationToken token)
    {
        var context = slot.SlotContext;
        if (context is null) return;
        var text = slot.ExpandedValue;
        if (slot.SlotKind == AuthoringAttributeSlotKind.Avt)
        {
            // Literal AVTs are covered. Embedded expressions need their own segment provenance;
            // refuse them in this slice instead of relying on the runtime's permissive binding.
            if (text.Replace("{{", "").Replace("}}", "").Contains('{'))
                throw new CoverageException("Existing AVTs with expression segments are outside insertion coverage.");
            return;
        }
        var namespaces = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["xml"] = "http://www.w3.org/XML/1998/namespace",
            ["xs"] = "http://www.w3.org/2001/XMLSchema",
            ["fn"] = "http://www.w3.org/2005/xpath-functions",
            ["map"] = "http://www.w3.org/2005/xpath-functions/map",
            ["array"] = "http://www.w3.org/2005/xpath-functions/array",
            ["math"] = "http://www.w3.org/2005/xpath-functions/math",
        };
        foreach (var binding in context.InScopeNamespaces) namespaces[binding.Prefix] = binding.Uri;
        Patterns.PatternCompiler.ValidateNamespacePrefixes(text, namespaces);
        XPathAstNode ast;
        try { ast = XPathParser.Parse(text, xpath40: context.EffectiveVersion == "4.0"); }
        catch (Exception) when (slot.SlotKind == AuthoringAttributeSlotKind.Pattern)
        { throw new CoverageException("This pattern needs static coverage beyond the insertion grammar."); }
        Visit(ast);

        void Function(string? ns, string? prefix, string local, int arity)
        {
            ns ??= prefix is null ? "http://www.w3.org/2005/xpath-functions"
                : namespaces.TryGetValue(prefix, out var uri) ? uri
                : throw new InvalidOperationException($"XPST0081: Undeclared function prefix '{prefix}'.");
            if (FunctionLibrary.TryGetFunction(ns, local, arity, out _) || snapshot.FunctionSignatures?.Resolve(ns, local, arity) is not null)
                return;
            // XSLT-only functions and schema constructors are not all registered in FunctionLibrary.
            // They are explicitly refused until their signatures can be proven without execution.
            throw new CoverageException($"Function Q{{{ns}}}{local}#{arity} is outside proven insertion signatures.");
        }

        void Many(IEnumerable<XPathAstNode> nodes) { foreach (var node in nodes) Visit(node); }
        void Visit(XPathAstNode node)
        {
            token.ThrowIfCancellationRequested();
            switch (node)
            {
                case BooleanLiteralNode or IntegerLiteralNode or DecimalLiteralNode or DoubleLiteralNode
                    or StringLiteralNode or VariableReferenceNode or ContextItemNode or QNameLiteralNode:
                    return;
                case FunctionCallNode f:
                    if (f.KeywordArguments?.Count > 0) throw new CoverageException("Keyword calls require expanded signature coverage.");
                    Function(f.NamespaceUri, f.Prefix, f.LocalName, f.Arguments.Count); Many(f.Arguments); return;
                case NamedFunctionRefNode f: Function(f.NamespaceUri, f.Prefix, f.LocalName, f.Arity); return;
                case ParenthesizedExprNode p: Visit(p.Expression); return;
                case BinaryExpressionNode b: Visit(b.Left); Visit(b.Right); return;
                case UnaryExpressionNode u: Visit(u.Operand); return;
                case IfExpressionNode i: Visit(i.Condition); Visit(i.ThenBranch); Visit(i.ElseBranch); return;
                case SequenceExpressionNode s: Many(s.Expressions); return;
                case RangeExpressionNode r: Visit(r.From); Visit(r.To); return;
                case PathExprNode p: Many(p.Steps); return;
                case StepNode s:
                    if (s.NodeTest.LookupExpression is not null) Visit(s.NodeTest.LookupExpression);
                    Many(s.Predicates); return;
                case PredicateNode p: Visit(p.Expression); return;
                case PostfixPredicateNode p: Visit(p.Expression); Visit(p.Predicate); return;
                case LookupNode l: Visit(l.Expression); Visit(l.Key); return;
                case LookupWildcardNode l: Visit(l.Expression); return;
                case MapConstructorNode m: foreach (var e in m.Entries) { Visit(e.Key); Visit(e.Value); } return;
                case ArrayConstructorNode a: Many(a.Items); return;
                case ForExpressionNode f: foreach (var b in f.Bindings) Visit(b.Expression); Visit(f.ReturnExpression); return;
                case LetExpressionNode l: foreach (var b in l.Bindings) Visit(b.Expression); Visit(l.Body); return;
                case QuantifiedExpressionNode q: foreach (var b in q.Bindings) Visit(b.Expression); Visit(q.SatisfiesExpression); return;
                default: throw new CoverageException($"Expression construct {node.GetType().Name} is outside proven insertion coverage.");
            }
        }
    }

    internal sealed class CoverageException(string message) : Exception(message);
}

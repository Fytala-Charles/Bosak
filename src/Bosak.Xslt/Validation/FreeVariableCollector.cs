// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Collects the free variable references of a parsed XPath expression, honoring in-expression binders.
// SPECIAL NOTES        : Part of the Bosak XSLT 3.0 processor — controlled validation and preview API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation (REQ-125 Slice A)                                                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Parser.Ast;

namespace Bosak.Xslt.Validation;

/// <summary>
/// Collects the variable references of one parsed XPath expression that are <em>free</em>: not bound
/// by an in-expression binder (for/let/quantified bindings, inline function parameters, typeswitch
/// case variables). References bound inside the expression itself (for example <c>let $x := 1 return
/// $x</c>) are not reported, so the validator only sees references that must be satisfied by the
/// stylesheet's declarations. Standalone compilation resolves variables at run time; this collector
/// is the additive seam that gives validation the missing static variable-scope check (XPST0008).
/// </summary>
internal static class FreeVariableCollector
{
    private readonly record struct BoundName(string? Prefix, string? NamespaceUri, string LocalName);

    public static IReadOnlyList<VariableReferenceNode> Collect(XPathAstNode node)
    {
        var result = new List<VariableReferenceNode>();
        Visit(node, new List<BoundName>(), result);
        return result;
    }

    private static void Visit(XPathAstNode node, List<BoundName> bound, List<VariableReferenceNode> result)
    {
        switch (node)
        {
            case VariableReferenceNode reference:
                if (!IsBound(reference, bound))
                {
                    result.Add(reference);
                }

                return;
            case ParenthesizedExprNode p:
                Visit(p.Expression, bound, result);
                return;
            case BinaryExpressionNode b:
                Visit(b.Left, bound, result);
                Visit(b.Right, bound, result);
                return;
            case UnaryExpressionNode u:
                Visit(u.Operand, bound, result);
                return;
            case IfExpressionNode i:
                Visit(i.Condition, bound, result);
                Visit(i.ThenBranch, bound, result);
                Visit(i.ElseBranch, bound, result);
                return;
            case SequenceExpressionNode s:
                foreach (var e in s.Expressions)
                {
                    Visit(e, bound, result);
                }

                return;
            case RangeExpressionNode r:
                Visit(r.From, bound, result);
                Visit(r.To, bound, result);
                return;
            case PathExprNode path:
                foreach (var step in path.Steps)
                {
                    Visit(step, bound, result);
                }

                return;
            case StepNode step:
                foreach (var predicate in step.Predicates)
                {
                    Visit(predicate, bound, result);
                }

                return;
            case PredicateNode predicate:
                Visit(predicate.Expression, bound, result);
                return;
            case PostfixPredicateNode postfix:
                Visit(postfix.Expression, bound, result);
                Visit(postfix.Predicate, bound, result);
                return;
            case FunctionCallNode f:
                foreach (var argument in f.Arguments)
                {
                    Visit(argument, bound, result);
                }

                if (f.KeywordArguments is not null)
                {
                    foreach (var keyword in f.KeywordArguments)
                    {
                        Visit(keyword.Value, bound, result);
                    }
                }

                return;
            case DynamicFunctionCallNode d:
                Visit(d.Function, bound, result);
                foreach (var argument in d.Arguments)
                {
                    Visit(argument, bound, result);
                }

                return;
            case CastNode cast:
                Visit(cast.Expression, bound, result);
                return;
            case CastableNode castable:
                Visit(castable.Expression, bound, result);
                return;
            case InstanceOfNode instanceOf:
                Visit(instanceOf.Expression, bound, result);
                return;
            case TreatNode treat:
                Visit(treat.Expression, bound, result);
                return;
            case ArrowExprNode arrow:
                Visit(arrow.Source, bound, result);
                Visit(arrow.Target, bound, result);
                return;
            case PipelineExprNode pipeline:
                Visit(pipeline.Source, bound, result);
                Visit(pipeline.Target, bound, result);
                return;
            case LookupNode lookup:
                Visit(lookup.Expression, bound, result);
                Visit(lookup.Key, bound, result);
                return;
            case LookupWildcardNode wildcard:
                Visit(wildcard.Expression, bound, result);
                return;
            case MapConstructorNode map:
                foreach (var entry in map.Entries)
                {
                    Visit(entry.Key, bound, result);
                    Visit(entry.Value, bound, result);
                }

                return;
            case ArrayConstructorNode array:
                foreach (var item in array.Items)
                {
                    Visit(item, bound, result);
                }

                return;
            case ForExpressionNode forExpression:
                VisitBindingsThen(forExpression.Bindings, forExpression.ReturnExpression, bound, result);
                return;
            case LetExpressionNode let:
                VisitBindingsThen(let.Bindings, let.Body, bound, result);
                return;
            case QuantifiedExpressionNode quantified:
                VisitBindingsThen(quantified.Bindings, quantified.SatisfiesExpression, bound, result);
                return;
            case SwitchExpressionNode switchExpression:
                Visit(switchExpression.Operand, bound, result);
                foreach (var @case in switchExpression.Cases)
                {
                    foreach (var value in @case.Values)
                    {
                        Visit(value, bound, result);
                    }

                    Visit(@case.Return, bound, result);
                }

                Visit(switchExpression.Default, bound, result);
                return;
            case TypeswitchExpressionNode typeswitch:
                Visit(typeswitch.Operand, bound, result);
                foreach (var @case in typeswitch.Cases)
                {
                    VisitBound(@case.Return, bound, result, @case.VariablePrefix, @case.VariableNamespaceUri, @case.VariableName);
                }

                VisitBound(typeswitch.Default, bound, result, typeswitch.DefaultVariablePrefix, typeswitch.DefaultVariableNamespaceUri, typeswitch.DefaultVariableName);
                return;
            case TryCatchNode tryCatch:
                Visit(tryCatch.TryExpression, bound, result);
                foreach (var clause in tryCatch.Clauses)
                {
                    Visit(clause.Expression, bound, result);
                }

                return;
            case InlineFunctionNode inline:
            {
                var mark = bound.Count;
                foreach (var parameter in inline.Parameters)
                {
                    bound.Add(new BoundName(null, null, parameter.Name));
                }

                Visit(inline.Body, bound, result);
                bound.RemoveRange(mark, bound.Count - mark);
                return;
            }

            case StringConstructorNode stringConstructor:
                foreach (var part in stringConstructor.Parts)
                {
                    Visit(part, bound, result);
                }

                return;
            case StringTemplateNode stringTemplate:
                foreach (var part in stringTemplate.Parts)
                {
                    Visit(part, bound, result);
                }

                return;
            case DirectElementConstructorNode element:
                foreach (var attribute in element.Attributes)
                {
                    foreach (var part in attribute.ValueParts)
                    {
                        Visit(part, bound, result);
                    }
                }

                foreach (var content in element.Content)
                {
                    Visit(content, bound, result);
                }

                return;
            case ComputedElementConstructorNode computedElement:
                if (computedElement.NameExpression is not null)
                {
                    Visit(computedElement.NameExpression, bound, result);
                }

                Visit(computedElement.ContentExpression, bound, result);
                return;
            case ComputedAttributeConstructorNode computedAttribute:
                if (computedAttribute.NameExpression is not null)
                {
                    Visit(computedAttribute.NameExpression, bound, result);
                }

                Visit(computedAttribute.ValueExpression, bound, result);
                return;
            case ComputedDocumentConstructorNode document:
                Visit(document.ContentExpression, bound, result);
                return;
            case ComputedTextConstructorNode text:
                Visit(text.ValueExpression, bound, result);
                return;
            case ComputedCommentConstructorNode comment:
                Visit(comment.ValueExpression, bound, result);
                return;
            case ComputedPIConstructorNode pi:
                if (pi.TargetExpression is not null)
                {
                    Visit(pi.TargetExpression, bound, result);
                }

                Visit(pi.ValueExpression, bound, result);
                return;
            case ComputedNamespaceConstructorNode namespaceNode:
                if (namespaceNode.PrefixExpression is not null)
                {
                    Visit(namespaceNode.PrefixExpression, bound, result);
                }

                Visit(namespaceNode.UriExpression, bound, result);
                return;
        }
    }

    /// <summary>
    /// Visits the binding expressions in the outer scope, then the body with the bindings pushed:
    /// <c>let $x := $y return $x</c> sees the outer <c>$y</c> but the bound <c>$x</c>.
    /// </summary>
    private static void VisitBindingsThen(
        IReadOnlyList<QuantifiedBinding> bindings,
        XPathAstNode body,
        List<BoundName> bound,
        List<VariableReferenceNode> result)
    {
        var mark = bound.Count;
        foreach (var binding in bindings)
        {
            Visit(binding.Expression, bound, result);
        }

        foreach (var binding in bindings)
        {
            bound.Add(new BoundName(binding.VariablePrefix, binding.VariableNamespaceUri, binding.VariableName));
            if (binding.PositionalVariableName is not null)
            {
                bound.Add(new BoundName(null, null, binding.PositionalVariableName));
            }

            if (binding.EntryValueVariableName is not null)
            {
                bound.Add(new BoundName(binding.EntryValueVariablePrefix, binding.EntryValueVariableNamespaceUri, binding.EntryValueVariableName));
            }
        }

        Visit(body, bound, result);
        bound.RemoveRange(mark, bound.Count - mark);
    }

    private static void VisitBound(
        XPathAstNode body,
        List<BoundName> bound,
        List<VariableReferenceNode> result,
        string? prefix,
        string? namespaceUri,
        string? variableName)
    {
        if (variableName is null)
        {
            Visit(body, bound, result);
            return;
        }

        var mark = bound.Count;
        bound.Add(new BoundName(prefix, namespaceUri, variableName));
        Visit(body, bound, result);
        bound.RemoveRange(mark, bound.Count - mark);
    }

    private static bool IsBound(VariableReferenceNode reference, List<BoundName> bound)
    {
        for (var i = bound.Count - 1; i >= 0; i--)
        {
            var name = bound[i];
            if (name.LocalName != reference.LocalName)
            {
                continue;
            }

            // A binder matches lexically (same written QName) or, when both sides carry a resolved
            // namespace URI, by expanded name.
            var referencePrefix = reference.Prefix ?? string.Empty;
            var boundPrefix = name.Prefix ?? string.Empty;
            if (referencePrefix == boundPrefix)
            {
                return true;
            }

            if (reference.NamespaceUri is not null && name.NamespaceUri is not null &&
                reference.NamespaceUri == name.NamespaceUri)
            {
                return true;
            }
        }

        return false;
    }
}

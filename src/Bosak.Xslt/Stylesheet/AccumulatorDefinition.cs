// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 12 juni 2026
// PURPOSE              : Represents an xsl:accumulator declaration and its rules.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 12-06-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 27-08-2026     | Statically detect variable references other than $value in accumulator-rule match patterns |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 02-09-2026     | XTSE0010 when initial-value is missing or no xsl:accumulator-rule is present (REQ-082)   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Bosak.XPath.Parser.Ast;

namespace Bosak.Xslt.Stylesheet;

/// <summary>
/// Represents a single <c>xsl:accumulator</c> declaration.
/// </summary>
public sealed class AccumulatorDefinition
{
    /// <summary>The original XElement of the xsl:accumulator declaration.</summary>
    public XElement Element { get; }

    /// <summary>The local name of the accumulator.</summary>
    public string LocalName { get; }

    /// <summary>The namespace URI of the accumulator (empty if unprefixed).</summary>
    public string NamespaceUri { get; }

    /// <summary>The accumulator name in Clark notation.</summary>
    public string ClarkName => string.IsNullOrEmpty(NamespaceUri) ? LocalName : $"{{{NamespaceUri}}}{LocalName}";

    /// <summary>The declared type of the accumulator value.</summary>
    public string? As { get; }

    /// <summary>The initial-value expression.</summary>
    public string InitialValue { get; }

    /// <summary>The accumulator rules, in declaration order.</summary>
    public IReadOnlyList<AccumulatorRule> Rules { get; }

    private AccumulatorDefinition(XElement element, string localName, string namespaceUri, string? asType, string initialValue, IReadOnlyList<AccumulatorRule> rules)
    {
        Element = element;
        LocalName = localName;
        NamespaceUri = namespaceUri;
        As = asType;
        InitialValue = initialValue;
        Rules = rules;
    }

    /// <summary>
    /// Parses an <c>xsl:accumulator</c> element into an <see cref="AccumulatorDefinition"/>.
    /// </summary>
    public static AccumulatorDefinition? FromElement(XElement element, Stylesheet stylesheet)
    {
        var nameAttr = element.Attribute("name")?.Value;
        if (string.IsNullOrEmpty(nameAttr))
            return null;

        // XTSE0010: the initial-value attribute is mandatory on xsl:accumulator (it may be
        // supplied through the static shadow attribute _initial-value).
        if (element.Attribute("initial-value") == null && element.Attribute("_initial-value") == null)
            throw new InvalidOperationException($"XTSE0010: The initial-value attribute is required on xsl:accumulator '{nameAttr}'.");

        var (localName, namespaceUri) = ResolveAccumulatorName(nameAttr, element);
        var initialValue = element.Attribute("initial-value")?.Value ?? "()";
        var asType = element.Attribute("as")?.Value;

        var rules = element.Elements(XName.Get("accumulator-rule", Stylesheet.XslNamespace))
            .Select(r => AccumulatorRule.FromElement(r, stylesheet))
            .Where(r => r != null)
            .Cast<AccumulatorRule>()
            .ToList();

        // XTSE0010: an accumulator must declare at least one xsl:accumulator-rule.
        if (rules.Count == 0)
            throw new InvalidOperationException($"XTSE0010: xsl:accumulator '{nameAttr}' must have at least one xsl:accumulator-rule.");

        return new AccumulatorDefinition(element, localName, namespaceUri, asType, initialValue, rules);
    }

    private static (string LocalName, string NamespaceUri) ResolveAccumulatorName(string name, XElement element)
    {
        var colon = name.IndexOf(':');
        if (colon < 0)
            return (name, string.Empty);

        var prefix = name[..colon];
        var local = name[(colon + 1)..];
        var ns = element.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? string.Empty;
        return (local, ns);
    }
}

/// <summary>
/// Represents a single <c>xsl:accumulator-rule</c> within an accumulator.
/// </summary>
public sealed class AccumulatorRule
{
    /// <summary>The original XElement of the accumulator rule.</summary>
    public XElement Element { get; }

    /// <summary>The match pattern.</summary>
    public string Match { get; }

    /// <summary>The select expression that computes the new accumulator value.</summary>
    public string? Select { get; }

    /// <summary>The rule phase ("start" or "end"), if specified.</summary>
    public string? Phase { get; }

    private AccumulatorRule(XElement element, string match, string? select, string? phase)
    {
        Element = element;
        Match = match;
        Select = select;
        Phase = phase;
    }

    /// <summary>
    /// Parses an <c>xsl:accumulator-rule</c> element.
    /// </summary>
    public static AccumulatorRule? FromElement(XElement element, Stylesheet stylesheet)
    {
        var match = element.Attribute("match")?.Value;
        if (string.IsNullOrEmpty(match))
            return null;

        ValidateAccumulatorRuleMatchPattern(match);

        var select = element.Attribute("select")?.Value;
        var phase = element.Attribute("phase")?.Value;
        return new AccumulatorRule(element, match, select, phase);
    }

    /// <summary>
    /// Validates that an <c>xsl:accumulator-rule/@match</c> pattern only references
    /// external variables that are available in the accumulator-rule context.
    /// XSLT 3.0 §9.2 restricts those external bindings to the accumulator value
    /// (<c>$value</c>); variables bound inside the pattern itself (for example by
    /// <c>let</c>) are allowed. Any disallowed variable reference raises <c>XPST0008</c>.
    /// </summary>
    private static void ValidateAccumulatorRuleMatchPattern(string match)
    {
        try
        {
            var ast = XPathParser.Parse(match);
            CheckVariableReferences(ast, new HashSet<(string LocalName, string? NamespaceUri)>());
        }
        catch (Bosak.XPath.Parser.ParseException)
        {
            // Leave XPath syntax errors for the pattern compiler / static-error machinery.
        }
    }

    /// <summary>
    /// Returns <c>true</c> when the variable reference denotes the accumulator value
    /// variable <c>$value</c> (possibly written as <c>Q{}value</c>).
    /// </summary>
    private static bool IsAccumulatorValueReference(VariableReferenceNode variable)
        => variable.LocalName == "value"
           && string.IsNullOrEmpty(variable.Prefix)
           && string.IsNullOrEmpty(variable.NamespaceUri);

    /// <summary>
    /// Recursively checks every variable reference in the AST against the supplied
    /// in-scope variable set. Bindings introduced by <c>for</c>, <c>let</c>,
    /// <c>some</c>/<c>every</c>, typeswitch, FLWOR clauses and inline functions are
    /// added to the scope before checking their dependent expressions.
    /// </summary>
    private static void CheckVariableReferences(XPathAstNode? node, HashSet<(string LocalName, string? NamespaceUri)> scope)
    {
        if (node is null) return;

        switch (node)
        {
            case VariableReferenceNode vrn:
                if (!IsAccumulatorValueReference(vrn) && !scope.Contains((vrn.LocalName, vrn.NamespaceUri)))
                {
                    var displayName = string.IsNullOrEmpty(vrn.Prefix)
                        ? vrn.LocalName
                        : $"{vrn.Prefix}:{vrn.LocalName}";
                    throw new InvalidOperationException($"XPST0008: Variable ${displayName} is not available in an xsl:accumulator-rule/@match pattern. Only $value is permitted.");
                }
                break;

            case StepNode step:
                foreach (var predicate in step.Predicates)
                    CheckVariableReferences(predicate, scope);
                break;

            case PathExprNode path:
                foreach (var step in path.Steps)
                    CheckVariableReferences(step, scope);
                break;

            case ParenthesizedExprNode paren:
                CheckVariableReferences(paren.Expression, scope);
                break;

            case PredicateNode predicate:
                CheckVariableReferences(predicate.Expression, scope);
                break;

            case FunctionCallNode call:
                foreach (var arg in call.Arguments)
                    CheckVariableReferences(arg, scope);
                break;

            case SequenceExpressionNode sequence:
                foreach (var expr in sequence.Expressions)
                    CheckVariableReferences(expr, scope);
                break;

            case RangeExpressionNode range:
                CheckVariableReferences(range.From, scope);
                CheckVariableReferences(range.To, scope);
                break;

            case IfExpressionNode ifExpr:
                CheckVariableReferences(ifExpr.Condition, scope);
                CheckVariableReferences(ifExpr.ThenBranch, scope);
                CheckVariableReferences(ifExpr.ElseBranch, scope);
                break;

            case BinaryExpressionNode binary:
                CheckVariableReferences(binary.Left, scope);
                CheckVariableReferences(binary.Right, scope);
                break;

            case UnaryExpressionNode unary:
                CheckVariableReferences(unary.Operand, scope);
                break;

            case CastNode cast:
                CheckVariableReferences(cast.Expression, scope);
                break;

            case CastableNode castable:
                CheckVariableReferences(castable.Expression, scope);
                break;

            case InstanceOfNode instanceOf:
                CheckVariableReferences(instanceOf.Expression, scope);
                break;

            case TreatNode treat:
                CheckVariableReferences(treat.Expression, scope);
                break;

            case ArrowExprNode arrow:
                CheckVariableReferences(arrow.Source, scope);
                CheckVariableReferences(arrow.Target, scope);
                break;

            case LookupNode lookup:
                CheckVariableReferences(lookup.Expression, scope);
                CheckVariableReferences(lookup.Key, scope);
                break;

            case LookupWildcardNode lookupWild:
                CheckVariableReferences(lookupWild.Expression, scope);
                break;

            case DynamicFunctionCallNode dynamicCall:
                CheckVariableReferences(dynamicCall.Function, scope);
                foreach (var arg in dynamicCall.Arguments)
                    CheckVariableReferences(arg, scope);
                break;

            case PostfixPredicateNode postfix:
                CheckVariableReferences(postfix.Expression, scope);
                CheckVariableReferences(postfix.Predicate, scope);
                break;

            case NamedFunctionRefNode:
            case ArgumentPlaceholderNode:
            case ContextItemNode:
            case BooleanLiteralNode:
            case IntegerLiteralNode:
            case DecimalLiteralNode:
            case DoubleLiteralNode:
            case StringLiteralNode:
            case DirectCommentNode:
            case SignificantTextNode:
                break;

            case ForExpressionNode forExpr:
                {
                    var innerScope = new HashSet<(string, string?)>(scope);
                    foreach (var binding in forExpr.Bindings)
                    {
                        CheckVariableReferences(binding.Expression, innerScope);
                        innerScope.Add((binding.VariableName, binding.VariableNamespaceUri));
                        if (!string.IsNullOrEmpty(binding.PositionalVariableName))
                            innerScope.Add((binding.PositionalVariableName!, binding.VariableNamespaceUri));
                    }
                    CheckVariableReferences(forExpr.ReturnExpression, innerScope);
                }
                break;

            case LetExpressionNode letExpr:
                {
                    var innerScope = new HashSet<(string, string?)>(scope);
                    foreach (var binding in letExpr.Bindings)
                    {
                        CheckVariableReferences(binding.Expression, innerScope);
                        innerScope.Add((binding.VariableName, binding.VariableNamespaceUri));
                        if (!string.IsNullOrEmpty(binding.PositionalVariableName))
                            innerScope.Add((binding.PositionalVariableName!, binding.VariableNamespaceUri));
                    }
                    CheckVariableReferences(letExpr.Body, innerScope);
                }
                break;

            case QuantifiedExpressionNode quantified:
                {
                    var innerScope = new HashSet<(string, string?)>(scope);
                    foreach (var binding in quantified.Bindings)
                    {
                        CheckVariableReferences(binding.Expression, innerScope);
                        innerScope.Add((binding.VariableName, binding.VariableNamespaceUri));
                        if (!string.IsNullOrEmpty(binding.PositionalVariableName))
                            innerScope.Add((binding.PositionalVariableName!, binding.VariableNamespaceUri));
                    }
                    CheckVariableReferences(quantified.SatisfiesExpression, innerScope);
                }
                break;

            case SwitchExpressionNode switchExpr:
                CheckVariableReferences(switchExpr.Operand, scope);
                foreach (var clause in switchExpr.Cases)
                {
                    foreach (var value in clause.Values)
                        CheckVariableReferences(value, scope);
                }
                CheckVariableReferences(switchExpr.Default, scope);
                break;

            case TypeswitchExpressionNode typeswitch:
                CheckVariableReferences(typeswitch.Operand, scope);
                foreach (var clause in typeswitch.Cases)
                {
                    var caseScope = new HashSet<(string, string?)>(scope);
                    if (!string.IsNullOrEmpty(clause.VariableName))
                        caseScope.Add((clause.VariableName!, clause.VariableNamespaceUri));
                    CheckVariableReferences(clause.Return, caseScope);
                }
                {
                    var defaultScope = new HashSet<(string, string?)>(scope);
                    if (!string.IsNullOrEmpty(typeswitch.DefaultVariableName))
                        defaultScope.Add((typeswitch.DefaultVariableName!, typeswitch.DefaultVariableNamespaceUri));
                    CheckVariableReferences(typeswitch.Default, defaultScope);
                }
                break;

            case FlworExpressionNode flwor:
                {
                    var innerScope = new HashSet<(string, string?)>(scope);
                    foreach (var clause in flwor.Clauses)
                    {
                        switch (clause)
                        {
                            case ForClauseNode forClause:
                                foreach (var binding in forClause.Bindings)
                                {
                                    CheckVariableReferences(binding.Expression, innerScope);
                                    innerScope.Add((binding.VariableName, binding.VariableNamespaceUri));
                                    if (!string.IsNullOrEmpty(binding.PositionalVariableName))
                                        innerScope.Add((binding.PositionalVariableName!, binding.VariableNamespaceUri));
                                }
                                break;

                            case LetClauseNode letClause:
                                foreach (var binding in letClause.Bindings)
                                {
                                    CheckVariableReferences(binding.Expression, innerScope);
                                    innerScope.Add((binding.VariableName, binding.VariableNamespaceUri));
                                    if (!string.IsNullOrEmpty(binding.PositionalVariableName))
                                        innerScope.Add((binding.PositionalVariableName!, binding.VariableNamespaceUri));
                                }
                                break;

                            case WhereClauseNode where:
                                CheckVariableReferences(where.Condition, innerScope);
                                break;

                            case OrderByClauseNode order:
                                foreach (var spec in order.Specs)
                                    CheckVariableReferences(spec.KeyExpression, innerScope);
                                break;

                            case CountClauseNode count:
                                innerScope.Add((count.VariableName, count.NamespaceUri));
                                break;

                            case GroupByClauseNode group:
                                foreach (var spec in group.Specs)
                                {
                                    if (spec.KeyExpression != null)
                                        CheckVariableReferences(spec.KeyExpression, innerScope);
                                    innerScope.Add((spec.VariableName, spec.NamespaceUri));
                                }
                                break;

                            case WindowClauseNode window:
                                CheckVariableReferences(window.InExpression, innerScope);
                                var windowScope = new HashSet<(string, string?)>(innerScope);
                                if (!string.IsNullOrEmpty(window.VariableName))
                                    windowScope.Add((window.VariableName!, window.NamespaceUri));
                                if (window.StartCondition != null)
                                {
                                    if (!string.IsNullOrEmpty(window.StartCondition.CurrentItemVariable))
                                        windowScope.Add((window.StartCondition.CurrentItemVariable!, window.NamespaceUri));
                                    if (!string.IsNullOrEmpty(window.StartCondition.PositionalVariable))
                                        windowScope.Add((window.StartCondition.PositionalVariable!, window.NamespaceUri));
                                    if (!string.IsNullOrEmpty(window.StartCondition.PreviousItemVariable))
                                        windowScope.Add((window.StartCondition.PreviousItemVariable!, window.NamespaceUri));
                                    if (!string.IsNullOrEmpty(window.StartCondition.NextItemVariable))
                                        windowScope.Add((window.StartCondition.NextItemVariable!, window.NamespaceUri));
                                    CheckVariableReferences(window.StartCondition.WhenExpression, windowScope);
                                }
                                if (window.EndCondition != null)
                                {
                                    var endScope = new HashSet<(string, string?)>(windowScope);
                                    if (!string.IsNullOrEmpty(window.EndCondition.CurrentItemVariable))
                                        endScope.Add((window.EndCondition.CurrentItemVariable!, window.NamespaceUri));
                                    if (!string.IsNullOrEmpty(window.EndCondition.PositionalVariable))
                                        endScope.Add((window.EndCondition.PositionalVariable!, window.NamespaceUri));
                                    if (!string.IsNullOrEmpty(window.EndCondition.PreviousItemVariable))
                                        endScope.Add((window.EndCondition.PreviousItemVariable!, window.NamespaceUri));
                                    if (!string.IsNullOrEmpty(window.EndCondition.NextItemVariable))
                                        endScope.Add((window.EndCondition.NextItemVariable!, window.NamespaceUri));
                                    CheckVariableReferences(window.EndCondition.WhenExpression, endScope);
                                }
                                break;
                        }
                    }
                    CheckVariableReferences(flwor.ReturnExpression, innerScope);
                }
                break;

            case InlineFunctionNode inline:
                {
                    var functionScope = new HashSet<(string, string?)>(scope);
                    foreach (var param in inline.Parameters)
                        functionScope.Add((param.Name, null));
                    CheckVariableReferences(inline.Body, functionScope);
                }
                break;

            case TryCatchNode tryCatch:
                CheckVariableReferences(tryCatch.TryExpression, scope);
                foreach (var clause in tryCatch.Clauses)
                    CheckVariableReferences(clause.Expression, scope);
                break;

            case ValidateExpressionNode validate:
                CheckVariableReferences(validate.Expression, scope);
                break;

            case MapConstructorNode map:
                foreach (var entry in map.Entries)
                {
                    CheckVariableReferences(entry.Key, scope);
                    CheckVariableReferences(entry.Value, scope);
                }
                break;

            case ArrayConstructorNode array:
                foreach (var item in array.Items)
                    CheckVariableReferences(item, scope);
                break;

            case StringConstructorNode str:
                foreach (var part in str.Parts)
                    CheckVariableReferences(part, scope);
                break;

            case DirectElementConstructorNode directElem:
                foreach (var attr in directElem.Attributes)
                {
                    foreach (var part in attr.ValueParts)
                        CheckVariableReferences(part, scope);
                }
                foreach (var content in directElem.Content)
                    CheckVariableReferences(content, scope);
                break;

            case ComputedElementConstructorNode compElem:
                CheckVariableReferences(compElem.NameExpression, scope);
                CheckVariableReferences(compElem.ContentExpression, scope);
                break;

            case ComputedAttributeConstructorNode compAttr:
                CheckVariableReferences(compAttr.NameExpression, scope);
                CheckVariableReferences(compAttr.ValueExpression, scope);
                break;

            case ComputedDocumentConstructorNode compDoc:
                CheckVariableReferences(compDoc.ContentExpression, scope);
                break;

            case ComputedTextConstructorNode compText:
                CheckVariableReferences(compText.ValueExpression, scope);
                break;

            case ComputedCommentConstructorNode compComment:
                CheckVariableReferences(compComment.ValueExpression, scope);
                break;

            case ComputedPIConstructorNode compPi:
                CheckVariableReferences(compPi.TargetExpression, scope);
                CheckVariableReferences(compPi.ValueExpression, scope);
                break;

            case ComputedNamespaceConstructorNode compNs:
                CheckVariableReferences(compNs.PrefixExpression, scope);
                CheckVariableReferences(compNs.UriExpression, scope);
                break;

            default:
                // Unknown node type: fall back to reflection-based traversal so that future
                // AST additions do not silently bypass this check.
                CheckUnknownNodeVariableReferences(node, scope);
                break;
        }
    }

    /// <summary>
    /// Fallback traversal for AST node types that are not explicitly handled above.
    /// Treats every property that is an <see cref="XPathAstNode"/> or a collection as a
    /// child expression evaluated in the current scope.
    /// </summary>
    private static void CheckUnknownNodeVariableReferences(XPathAstNode node, HashSet<(string LocalName, string? NamespaceUri)> scope)
    {
        foreach (var property in node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanRead) continue;
            var value = property.GetValue(node);
            if (value is null) continue;

            if (value is XPathAstNode child)
            {
                CheckVariableReferences(child, scope);
            }
            else if (value is not string && value is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is XPathAstNode childNode)
                        CheckVariableReferences(childNode, scope);
                }
            }
        }
    }
}

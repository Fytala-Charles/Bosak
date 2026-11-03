// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 November 2026
// PURPOSE              : Validates the bounded ordinary-source string and boolean expression profile.
// SPECIAL NOTES        : Public surface API for compiling and evaluating XPath 3.1 expressions.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 03-11-2026     | Creation; bounded public ordinary-source facade (1.45.0).                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Parser.Ast;
using Bosak.XPath.Parser.Lexer;

namespace Bosak.XPath.Api.Schema;

/// <summary>Rejects unavailable numeric and schema syntax before compilation or optimization.</summary>
internal static class OrdinaryQueryProfile
{
    internal const string Name = "bosak-ordinary-string-boolean-v1";
    private const string Functions = "http://www.w3.org/2005/xpath-functions";
    internal static bool Supports(string expression, CompileOptions options)
    {
        if (options.Compatibility != XPathCompatibility.XPath31 || options.BackwardsCompatible || options.Xml11LineEndings
            || options.EnumerationItemTypes is not null || options.ItemTypeAliases is not null
            || options.StaticVariableTypes is not null || options.NamedRecordTypeNames is not null) return false;
        var lexer = new XPathLexer(expression.AsSpan());
        for (var token = lexer.NextToken(); token.Kind != TokenKind.Eof; token = lexer.NextToken())
            if (token.Kind is TokenKind.IntegerLiteral or TokenKind.DecimalLiteral or TokenKind.DoubleLiteral) return false;
        var ast = XPathParser.Parse(expression);
        if (!ScalarResult(ast)) return false;
        var pending = new Stack<XPathAstNode>(); pending.Push(ast);
        while (pending.TryPop(out var node))
        {
            switch (node)
            {
                case StringLiteralNode or BooleanLiteralNode or VariableReferenceNode or ContextItemNode: break;
                case ParenthesizedExprNode p: pending.Push(p.Expression); break;
                case PathExprNode p:
                    foreach (var step in p.Steps) pending.Push(step);
                    break;
                case StepNode s:
                    if (s.NodeTest.Kind is NameTestKind.LookupKey or NameTestKind.LookupIndex or NameTestKind.LookupComputed
                        || s.NodeTest.KindTestTypeName is not null || s.NodeTest.LookupExpression is not null
                        || s.NodeTest.Name?.StartsWith("schema-", StringComparison.Ordinal) == true
                        || s.NodeTest.KindTestInnerName?.StartsWith("schema-", StringComparison.Ordinal) == true) return false;
                    foreach (var predicate in s.Predicates) pending.Push(predicate);
                    break;
                case PredicateNode p: pending.Push(p.Expression); break;
                case BinaryExpressionNode b when b.Operator is BinaryOperator.And or BinaryOperator.Or:
                    pending.Push(b.Left); pending.Push(b.Right); break;
                case IfExpressionNode i:
                    pending.Push(i.Condition); pending.Push(i.ThenBranch); pending.Push(i.ElseBranch); break;
                case FunctionCallNode f:
                    var ns = f.NamespaceUri;
                    if (ns is null)
                    {
                        if (f.Prefix is null) ns = Functions;
                        else if (options.Namespaces?.TryGetValue(f.Prefix, out var resolved) == true) ns = resolved;
                        else if (f.Prefix == "fn") ns = Functions;
                    }
                    if (ns != Functions || f.KeywordArguments is { Count: > 0 } || !Allowed(f.LocalName, f.Arguments.Count)) return false;
                    foreach (var argument in f.Arguments) pending.Push(argument);
                    break;
                default: return false;
            }
        }
        return true;
    }
    private static bool Allowed(string name, int arity) => name switch
    {
        "string" or "name" or "local-name" or "namespace-uri" => arity is 0 or 1,
        "boolean" or "exists" or "empty" or "not" => arity == 1,
        "true" or "false" => arity == 0,
        _ => false
    };
    private static bool ScalarResult(XPathAstNode node) => node switch
    {
        StringLiteralNode or BooleanLiteralNode or VariableReferenceNode or FunctionCallNode => true,
        ParenthesizedExprNode p => ScalarResult(p.Expression),
        BinaryExpressionNode b => b.Operator is BinaryOperator.And or BinaryOperator.Or,
        IfExpressionNode i => ScalarResult(i.ThenBranch) && ScalarResult(i.ElseBranch),
        _ => false
    };
}

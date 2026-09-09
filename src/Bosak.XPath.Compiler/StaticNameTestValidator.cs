// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 07 September 2026
// PURPOSE              : Statically validates namespace prefixes in name tests at compile time.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 07-09-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 09-09-2026     | XQST0040 for duplicate expanded attribute names in direct element constructors           |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 09-09-2026     | XML doc coverage on public API (Beta review)                                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Parser.Ast;

namespace Bosak.XPath.Compiler;

/// <summary>
/// Compile-time validation of namespace prefixes used in name tests (XPST0081) and of
/// schema-aware kind tests (XPST0008, this engine has no schema awareness).
///
/// The runtime resolves prefixes when a <c>NamespaceTest</c> opcode executes, which is
/// too late: without a context item the step fails earlier with XPDY0002, and static
/// errors must be reported at compile time. This walker mirrors the runtime resolution
/// rules of <c>VmEngine</c>'s NamespaceTest handler: an operand that binds in the static
/// context is a prefix; an operand containing '/' or ':' is a literal URI (EQName form);
/// anything else is an undeclared prefix (XPST0017-free XPST0081).
///
/// Direct element constructor namespace declaration attributes
/// (<c>xmlns:p="uri"</c>) are tracked: their bindings are in scope for the enclosed
/// expressions of the constructor (attributes and content) but not beyond it.
/// </summary>
public static class StaticNameTestValidator
{
    /// <summary>
    /// Validates all name-test prefixes in the expression against the supplied static
    /// namespace resolver. Throws <see cref="InvalidOperationException"/> whose message
    /// carries the error code (XPST0081 / XPST0008) on the first violation.
    /// </summary>
    /// <param name="node">The expression AST to validate.</param>
    /// <param name="resolvePrefix">
    /// Maps a prefix to its namespace URI, or returns null when the prefix is not declared
    /// in the static context. The predefined <c>xml</c> prefix need not be supplied.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> or <paramref name="resolvePrefix"/> is null.</exception>
    /// <exception cref="InvalidOperationException">A name-test prefix is not declared
    /// (XPST0081), a kind test requires schema awareness (XPST0008), or a direct element
    /// constructor declares duplicate attribute names (XQST0040).</exception>
    public static void Validate(XPathAstNode node, Func<string, string?> resolvePrefix)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(resolvePrefix);
        ValidateNode(node, new Scope(resolvePrefix, null));
    }

    private sealed class Scope
    {
        private readonly Scope? _parent;
        private readonly Dictionary<string, string>? _bindings;
        private readonly Func<string, string?> _resolve;

        public Scope(Func<string, string?> resolve, Scope? parent, Dictionary<string, string>? bindings = null)
        {
            _resolve = resolve;
            _parent = parent;
            _bindings = bindings;
        }

        public bool IsDeclared(string prefix)
            => prefix == "xml"
               || (_bindings is not null && _bindings.ContainsKey(prefix))
               || (_parent?.IsDeclared(prefix) ?? false)
               || _resolve(prefix) is not null;

        /// <summary>Resolves a prefix to its namespace URI, or null when undeclared.</summary>
        public string? Resolve(string prefix)
        {
            if (prefix == "xml")
                return "http://www.w3.org/XML/1998/namespace";
            if (_bindings is not null && _bindings.TryGetValue(prefix, out var uri))
                return uri;
            return _parent?.Resolve(prefix) ?? _resolve(prefix);
        }

        /// <summary>Returns a child scope with additional local bindings (constructor xmlns attributes).</summary>
        public Scope Extend(IReadOnlyDictionary<string, string> bindings)
            => new(_resolve, this, new Dictionary<string, string>(bindings, StringComparer.Ordinal));
    }

    private static void ValidateNode(XPathAstNode node, Scope scope)
    {
        switch (node)
        {
            case StepNode step:
                ValidateStepTest(step.NodeTest, scope);
                foreach (var pred in step.Predicates)
                    ValidateNode(pred, scope);
                break;
            case PathExprNode path:
                foreach (var step in path.Steps)
                    ValidateNode(step, scope);
                break;
            case ParenthesizedExprNode p:
                ValidateNode(p.Expression, scope);
                break;
            case PredicateNode pred:
                ValidateNode(pred.Expression, scope);
                break;
            case PostfixPredicateNode pp:
                ValidateNode(pp.Expression, scope);
                ValidateNode(pp.Predicate, scope);
                break;
            case SequenceExpressionNode seq:
                foreach (var e in seq.Expressions)
                    ValidateNode(e, scope);
                break;
            case RangeExpressionNode range:
                ValidateNode(range.From, scope);
                ValidateNode(range.To, scope);
                break;
            case IfExpressionNode ife:
                ValidateNode(ife.Condition, scope);
                ValidateNode(ife.ThenBranch, scope);
                ValidateNode(ife.ElseBranch, scope);
                break;
            case ForExpressionNode fe:
                foreach (var b in fe.Bindings)
                    ValidateNode(b.Expression, scope);
                ValidateNode(fe.ReturnExpression, scope);
                break;
            case LetExpressionNode le:
                foreach (var b in le.Bindings)
                    ValidateNode(b.Expression, scope);
                ValidateNode(le.Body, scope);
                break;
            case QuantifiedExpressionNode qe:
                foreach (var b in qe.Bindings)
                    ValidateNode(b.Expression, scope);
                ValidateNode(qe.SatisfiesExpression, scope);
                break;
            case BinaryExpressionNode bin:
                ValidateNode(bin.Left, scope);
                ValidateNode(bin.Right, scope);
                break;
            case UnaryExpressionNode un:
                ValidateNode(un.Operand, scope);
                break;
            case CastNode cast:
                ValidateNode(cast.Expression, scope);
                break;
            case CastableNode castable:
                ValidateNode(castable.Expression, scope);
                break;
            case InstanceOfNode io:
                ValidateNode(io.Expression, scope);
                break;
            case TreatNode treat:
                ValidateNode(treat.Expression, scope);
                break;
            case ArrowExprNode arrow:
                ValidateNode(arrow.Source, scope);
                ValidateNode(arrow.Target, scope);
                break;
            case TryCatchNode tc:
                ValidateNode(tc.TryExpression, scope);
                foreach (var c in tc.Clauses)
                    ValidateNode(c.Expression, scope);
                break;
            case StringConstructorNode sc:
                foreach (var part in sc.Parts)
                    ValidateNode(part, scope);
                break;
            case LookupNode lookup:
                ValidateNode(lookup.Expression, scope);
                ValidateNode(lookup.Key, scope);
                break;
            case LookupWildcardNode lw:
                ValidateNode(lw.Expression, scope);
                break;
            case InlineFunctionNode inf:
                ValidateNode(inf.Body, scope);
                break;
            case MapConstructorNode mc:
                foreach (var e in mc.Entries)
                {
                    ValidateNode(e.Key, scope);
                    ValidateNode(e.Value, scope);
                }
                break;
            case ArrayConstructorNode ac:
                foreach (var item in ac.Items)
                    ValidateNode(item, scope);
                break;
            case DynamicFunctionCallNode dfc:
                ValidateNode(dfc.Function, scope);
                foreach (var a in dfc.Arguments)
                    ValidateNode(a, scope);
                break;
            case FlworExpressionNode flwor:
                foreach (var clause in flwor.Clauses)
                    ValidateFlworClause(clause, scope);
                ValidateNode(flwor.ReturnExpression, scope);
                break;
            case SwitchExpressionNode sw:
                ValidateNode(sw.Operand, scope);
                foreach (var c in sw.Cases)
                {
                    foreach (var v in c.Values)
                        ValidateNode(v, scope);
                    ValidateNode(c.Return, scope);
                }
                ValidateNode(sw.Default, scope);
                break;
            case TypeswitchExpressionNode ts:
                ValidateNode(ts.Operand, scope);
                foreach (var c in ts.Cases)
                    ValidateNode(c.Return, scope);
                ValidateNode(ts.Default, scope);
                break;
            case ValidateExpressionNode v:
                ValidateNode(v.Expression, scope);
                break;
            case DirectElementConstructorNode elem:
                ValidateDirectElementConstructor(elem, scope);
                break;
            case ComputedElementConstructorNode ce:
                if (ce.NameExpression is not null)
                    ValidateNode(ce.NameExpression, scope);
                ValidateNode(ce.ContentExpression, scope);
                break;
            case ComputedAttributeConstructorNode ca:
                if (ca.NameExpression is not null)
                    ValidateNode(ca.NameExpression, scope);
                ValidateNode(ca.ValueExpression, scope);
                break;
            case ComputedDocumentConstructorNode cd:
                ValidateNode(cd.ContentExpression, scope);
                break;
            case ComputedTextConstructorNode ct:
                ValidateNode(ct.ValueExpression, scope);
                break;
            case ComputedCommentConstructorNode cc:
                ValidateNode(cc.ValueExpression, scope);
                break;
            case ComputedPIConstructorNode cp:
                if (cp.TargetExpression is not null)
                    ValidateNode(cp.TargetExpression, scope);
                ValidateNode(cp.ValueExpression, scope);
                break;
            case ComputedNamespaceConstructorNode cn:
                if (cn.PrefixExpression is not null)
                    ValidateNode(cn.PrefixExpression, scope);
                ValidateNode(cn.UriExpression, scope);
                break;
            default:
                break; // literals, context item, variables, function calls: no name tests
        }
    }

    private static void ValidateFlworClause(FlworClauseNode clause, Scope scope)
    {
        switch (clause)
        {
            case ForClauseNode forClause:
                foreach (var b in forClause.Bindings)
                    ValidateNode(b.Expression, scope);
                break;
            case LetClauseNode letClause:
                foreach (var b in letClause.Bindings)
                    ValidateNode(b.Expression, scope);
                break;
            case WhereClauseNode whereClause:
                ValidateNode(whereClause.Condition, scope);
                break;
            case OrderByClauseNode orderClause:
                foreach (var s in orderClause.Specs)
                    ValidateNode(s.KeyExpression, scope);
                break;
            case GroupByClauseNode groupClause:
                foreach (var s in groupClause.Specs)
                    if (s.KeyExpression is not null)
                        ValidateNode(s.KeyExpression, scope);
                break;
            case WindowClauseNode windowClause:
                ValidateNode(windowClause.InExpression, scope);
                ValidateNode(windowClause.StartCondition.WhenExpression, scope);
                if (windowClause.EndCondition is not null)
                    ValidateNode(windowClause.EndCondition.WhenExpression, scope);
                break;
        }
    }

    /// <summary>
    /// Validates a direct element constructor. Namespace declaration attributes extend the
    /// statically known namespaces for the element's enclosed expressions (attribute value
    /// expressions and content) and for nested constructors, but not for anything outside
    /// the constructor (K2-DirectConElemNamespace-1/14/26).
    /// </summary>
    private static void ValidateDirectElementConstructor(DirectElementConstructorNode node, Scope scope)
    {
        // Collect this element's namespace declaration attributes (the parser guarantees
        // literal-only values; anything else is XQST0022 before this point).
        Dictionary<string, string>? bindings = null;
        foreach (var attr in node.Attributes)
        {
            if (attr.Prefix != "xmlns")
                continue;
            bindings ??= new Dictionary<string, string>(StringComparer.Ordinal);
            bindings[attr.Name] = string.Concat(attr.ValueParts.OfType<StringLiteralNode>().Select(p => p.Value));
        }

        var inner = bindings is null ? scope : scope.Extend(bindings);

        // The element name's own prefix is resolved against the element's declarations too
        // (XML namespace scoping; Constr-namespace-1/14).
        if (!string.IsNullOrEmpty(node.Prefix) && !inner.IsDeclared(node.Prefix))
            throw new InvalidOperationException($"XPST0081: Prefix '{node.Prefix}' is not declared.");

        // XQST0040: two attributes in one direct element constructor must not have the same
        // expanded QName — including differently-prefixed names bound to the same URI
        // (Constr-attr-distnames-4, K2-DefaultNamespaceProlog-10). Unprefixed attribute
        // names are in no namespace.
        var seenAttributes = new HashSet<(string NamespaceUri, string LocalName)>();
        foreach (var attr in node.Attributes)
        {
            if (attr.Prefix == "xmlns" || (attr.Prefix is null && attr.Name == "xmlns"))
                continue;
            string attrNs = attr.Prefix is null ? "" : inner.Resolve(attr.Prefix) ?? ("undeclared:" + attr.Prefix);
            if (!seenAttributes.Add((attrNs, attr.Name)))
            {
                string lexical = attr.Prefix is null ? attr.Name : attr.Prefix + ":" + attr.Name;
                throw new InvalidOperationException($"XQST0040: Duplicate attribute '{lexical}' in a direct element constructor.");
            }
        }

        foreach (var attr in node.Attributes)
        {
            foreach (var part in attr.ValueParts)
            {
                if (part is not StringLiteralNode and not SignificantTextNode)
                    ValidateNode(part, inner);
            }
        }
        foreach (var content in node.Content)
            ValidateNode(content, inner);
    }

    /// <summary>
    /// Validates the namespace prefixes referenced by a single step's node test.
    /// </summary>
    private static void ValidateStepTest(NodeTest test, Scope scope)
    {
        switch (test.Kind)
        {
            case NameTestKind.QName:
                // NamespaceUri carries a real prefix ("ncname"), the wildcard marker "*",
                // or a literal URI (EQName / xml forms resolved by the parser).
                if (!string.IsNullOrEmpty(test.NamespaceUri) && test.NamespaceUri != "*")
                    CheckNamespaceOperand(test.NamespaceUri);
                break;
            case NameTestKind.NamespaceAny:
                // prefix:* stores the prefix in Name; Q{uri}* stores the URI in NamespaceUri.
                if (!string.IsNullOrEmpty(test.Name))
                    CheckNamespaceOperand(test.Name);
                break;
            case NameTestKind.KindTest:
                ValidateKindTest(test, scope);
                break;
        }

        void CheckNamespaceOperand(string operand)
        {
            // Mirrors the runtime NamespaceTest resolution order.
            if (operand.Length == 0 || operand == "Q{}")
                return; // default element namespace marker / empty-namespace wildcard sentinel
            if (scope.IsDeclared(operand))
                return;
            if (operand.Contains('/') || operand.Contains(':'))
                return; // literal URI from a Q{uri} EQName form
            throw new InvalidOperationException($"XPST0081: Prefix '{operand}' is not declared.");
        }
    }

    /// <summary>
    /// Validates kind-test arguments: prefixed names are XPST0081 when the prefix is
    /// undeclared (K2-NodeTest-22..27, K2-NameTest-35/36/41); schema-aware kind tests with
    /// a resolvable prefix are XPST0008 because this engine has no schema awareness
    /// (K2-NameTest-39/40, K2-NodeTest-20).
    /// </summary>
    private static void ValidateKindTest(NodeTest test, Scope scope)
    {
        // document-node(element(x)) / document-node(schema-element(x)).
        if (test.Name == "document-node")
        {
            if (test.KindTestInnerName is not null && !string.IsNullOrEmpty(test.KindTestArgument) && test.KindTestArgument != "*")
                ValidateKindTestArgument(test.KindTestArgument, test.KindTestInnerName);
            return;
        }

        if (!string.IsNullOrEmpty(test.KindTestArgument) && test.KindTestArgument != "*")
            ValidateKindTestArgument(test.KindTestArgument, test.Name ?? "");

        void ValidateKindTestArgument(string argument, string testName)
        {
            if (argument.StartsWith("Q{", StringComparison.Ordinal))
                return; // URI-qualified argument: no prefix to resolve
            int colon = argument.IndexOf(':');
            if (colon <= 0)
                return; // unprefixed name (schema-aware unprefixed names are XPST0008 at parse time)
            var prefix = argument[..colon];
            if (!scope.IsDeclared(prefix))
                throw new InvalidOperationException($"XPST0081: Prefix '{prefix}' is not declared.");
            if (testName is "schema-element" or "schema-attribute")
                throw new InvalidOperationException("XPST0008: Schema-aware kind tests are not supported (no schema awareness).");
        }
    }
}

// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 September 2026
// PURPOSE              : Static streamability analysis raising XTSE3430 for non-streamable constructs per XSLT 3.0 §19.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-09-2026     | Creation (Phase C milestone C2)                                                          |
//                      | Charles Korthout | 0.2   | 16-09-2026     | Union/intersect/except: wider sweep (max) per §19.8.8.4; fixes sf-boolean-001 false positive |
//                      | Charles Korthout | 0.3   | 16-09-2026     | Calibration vs XSLT 3.0 test suite strm sets: §19.10 striding unions, LeafItem buffered-item model, function streamability rules (absorbing consuming-ref limit, inspection/filter result postures, shallow-descent striding-arg + bang-delivery), source-document grounded-result escapes, constructor climbing-delivery check, xsl:map implicit-fork consuming-use rule, if-expression max sweep |
//                      | Charles Korthout | 0.4   | 17-09-2026     | False-positive regression fixes: buffered leaf items atomizable after !, unclassified-function atomic-param atomization, map/array constructor implicit-fork max consumption, leaf child steps from crawling operands, no-arg atomizers on leaf contexts, current() captured only in leaf pattern predicates, streamable accumulator checks (initial-value motionless, rule pattern/body, post-descent accumulator-after), streamable merge-source select must be striding / no sort-before-merge |
//                      | Charles Korthout | 0.5   | 21-09-2026     | current-group() with no group lexically in scope is a static XTSE3430 over a streamed context (si-fork-116); current-grouping-key() stays motionless |
//                      | Charles Korthout | 0.6   | 21-09-2026     | False-positive fixes (su-filter/su-unclassified): boolean-typed lone variable predicate is a filter predicate, not positional; positional motionless predicate on a striding step stays striding; unclassified functions atomize atomic-typed params in any argument position |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Parser.Ast;

namespace Bosak.Xslt.Stylesheet;

/// <summary>
/// Compile-time streamability analyzer (XSLT 3.0 §19). Walks streamable contexts —
/// <c>xsl:source-document streamable="yes"</c> content constructors and template rules
/// of streamable modes — and raises static error XTSE3430 when a construct cannot be
/// evaluated using streaming. Analysis is deliberately conservative: it bails out
/// cheaply when a stylesheet contains no streamable constructs at all, and it only
/// flags constructs the corpus of §19 tests demonstrates as non-streamable, so that
/// guaranteed-streamable stylesheets keep compiling.
/// </summary>
internal static class StreamabilityAnalyzer
{
    /// <summary>
    /// Runs the streamability analysis over the root stylesheet, if it contains any
    /// streamable constructs.
    /// </summary>
    /// <param name="stylesheet">The root stylesheet module.</param>
    /// <exception cref="InvalidOperationException">XTSE3430: a construct inside a streamable context is not streamable.</exception>
    public static void Analyze(Stylesheet stylesheet)
    {
        var root = stylesheet.Root;
        if (!MayRequireAnalysis(root, stylesheet))
            return;
        new Worker(stylesheet).Run();
    }

    /// <summary>
    /// Cheap bail-out: scans the raw XElements for streamability-related attributes
    /// before any XPath parsing happens. Ordinary stylesheets exit here.
    /// </summary>
    private static bool MayRequireAnalysis(XElement root, Stylesheet stylesheet)
    {
        foreach (var el in root.DescendantsAndSelf())
        {
            if (el.Name.NamespaceName != Stylesheet.XslNamespace)
                continue;
            var local = el.Name.LocalName;
            if (local == "function" && el.Attribute("streamability") != null)
                return true;
            if ((local == "mode" || local == "source-document" || local == "attribute-set" || local == "accumulator"
                    || local == "merge-source")
                && EffectiveStreamable(el) != null)
                return true;
        }

        // Streamable modes can be declared in imported modules; consult the merged
        // mode definitions for the modes used by this stylesheet's template rules.
        foreach (var rule in stylesheet.GetAllTemplateRules())
        {
            foreach (var mode in rule.Modes)
            {
                if (mode is "#all" or "#current")
                {
                    if (AnyStreamableMode(root))
                        return true;
                }
                else if (stylesheet.GetModeDefinition(mode)?.Streamable == true)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static bool AnyStreamableMode(XElement root)
        => root.DescendantsAndSelf()
            .Where(e => e.Name.NamespaceName == Stylesheet.XslNamespace && e.Name.LocalName == "mode")
            .Any(EffectiveStreamableYes);

    /// <summary>
    /// Returns the effective boolean value of a streamable attribute: the literal
    /// (yes/true/1 or no/false/0, case-insensitive), or the value of a single
    /// <c>{ $name }</c> AVT bound to a static boolean parameter; null when the
    /// attribute is absent or cannot be resolved statically.
    /// </summary>
    private static bool? EffectiveStreamable(XElement el)
    {
        var attr = el.Attribute("streamable") ?? el.Attribute("_streamable");
        if (attr == null)
            return null;
        return ParseStreamableValue(attr.Value);
    }

    private static bool EffectiveStreamableYes(XElement el) => EffectiveStreamable(el) == true;

    private static bool? ParseStreamableValue(string? raw)
    {
        if (raw == null)
            return null;
        var value = raw.Trim();
        switch (value.ToLowerInvariant())
        {
            case "yes": case "true": case "1": return true;
            case "no": case "false": case "0": return false;
        }
        // Single {$name} AVT referencing a static parameter/variable.
        if (value.Length > 2 && value[0] == '{' && value[value.Length - 1] == '}'
            && !value.Substring(1, value.Length - 2).Contains('{'))
            return null; // resolved by caller via static variable lookup
        return null;
    }

    private static InvalidOperationException Error(string message)
        => new($"XTSE3430: {message}");

    // ------------------------------------------------------------------
    // Posture model (XSLT 3.0 §19.4, simplified)
    // ------------------------------------------------------------------

    private enum Posture
    {
        Grounded,
        Striding,
        Crawling,
        Climbing,
        Roaming,
    }

    /// <summary>Analysis result for one XPath expression.</summary>
    private readonly record struct Info(
        Posture Posture = Posture.Grounded,
        int Consumes = 0,
        bool Motionless = true,
        bool UsesLast = false,
        bool UsesPosition = false,
        bool RefsStreamed = false,
        bool Fresh = false,
        bool Captured = false,
        bool Roaming = false,
        int BadRefs = 0,
        bool LeafItem = false,
        bool StridingUnion = false)
    {
        public static readonly Info GroundedMotionless = new(Posture.Grounded, 0, Motionless: true);
    }

    /// <summary>Variable binding tracked during the walk.</summary>
    private sealed class Var
    {
        public Posture Posture;
        public bool Streamed;
        public bool BooleanTyped;
        public bool Roaming;
    }

    /// <summary>Lexically-scoped analysis environment (immutable parent chain).</summary>
    private sealed class Env
    {
        public static readonly Env Base = new();
        private Env() { }

        public Env? Parent;
        public bool ContextStreamed;
        public bool ContextFresh = true;
        public Posture ContextPosture;
        public bool CurrentStreamed;
        public bool GroupInScope;
        public bool GroupOutside;
        public Posture? GroupSelectPosture;
        public bool InPattern;
        public bool LeafContext;
        public bool InAttributeSet;
        public string? ShallowDescentParam;
        public Dictionary<string, Var>? Vars;

        public Env Spawn() => new()
        {
            Parent = this,
            ContextStreamed = ContextStreamed,
            ContextFresh = ContextFresh,
            ContextPosture = ContextPosture,
            CurrentStreamed = CurrentStreamed,
            GroupInScope = GroupInScope,
            GroupOutside = GroupOutside,
            GroupSelectPosture = GroupSelectPosture,
            InPattern = InPattern,
            LeafContext = LeafContext,
            InAttributeSet = InAttributeSet,
            ShallowDescentParam = ShallowDescentParam,
        };

        public Env WithContext(Posture posture, bool streamed, bool fresh = true)
        {
            var e = Spawn();
            e.ContextPosture = posture;
            e.ContextStreamed = streamed;
            e.ContextFresh = fresh;
            return e;
        }

        public Env WithVar(string name, Var var)
        {
            var e = Spawn();
            e.Vars = new Dictionary<string, Var>(StringComparer.Ordinal) { [name] = var };
            return e;
        }

        public Var? Lookup(string name)
        {
            for (var e = this; e != null; e = e.Parent)
            {
                if (e.Vars != null && e.Vars.TryGetValue(name, out var v))
                    return v;
            }
            return null;
        }
    }

    // ------------------------------------------------------------------
    // Worker
    // ------------------------------------------------------------------

    private sealed class Worker
    {
        private readonly Stylesheet _stylesheet;
        private readonly XElement _root;
        private readonly Dictionary<string, bool> _staticBools = new(StringComparer.Ordinal);
        private Dictionary<(string ns, string name, int arity), XsltFunctionDefinition>? _functions;
        private Dictionary<(string local, string ns), List<AttributeSetDefinition>>? _attrSets;

        public Worker(Stylesheet stylesheet)
        {
            _stylesheet = stylesheet;
            _root = stylesheet.Root;
        }

        public void Run()
        {
            CollectStaticBools();
            ValidateFunctionDeclarations();
            AnalyzeTemplateRules();
            AnalyzeSourceDocuments();
            AnalyzeAccumulators();
            AnalyzeMergeSources();
        }

        // ---------------- streamable merge sources (§19.8.4.16) ----------------

        /// <summary>
        /// A streamable xsl:merge-source makes the merge a streaming construct even when the
        /// enclosing template belongs to a non-streamable mode, so these are scanned
        /// globally: the select expression must be striding (merge-094/095: log//record
        /// is crawling → XTSE3430).
        /// </summary>
        private void AnalyzeMergeSources()
        {
            foreach (var el in _root.DescendantsAndSelf())
            {
                if (el.Name.NamespaceName != Stylesheet.XslNamespace || el.Name.LocalName != "merge-source")
                    continue;
                if (ResolveStreamable(el) != true)
                    continue;
                // A streamable merge source must deliver items in merge-key order: sorting
                // the source first requires buffering it (merge-095 → XTSE3430).
                var sortBefore = el.Attribute("sort-before-merge")?.Value?.Trim();
                if (sortBefore is "yes" or "true" or "1")
                    throw Error("a streamable merge source must not specify sort-before-merge=\"yes\".");
                var env = Env.Base.Spawn();
                env.ContextStreamed = true;
                env.ContextPosture = Posture.Striding;
                env.ContextFresh = true;
                var info = AnalyzeSurface(el, "select", env);
                if (info.Posture is Posture.Crawling or Posture.Roaming)
                    throw Error("the select expression of a streamable merge source must be striding.");
            }
        }

        // ---------------- static parameters (for _streamable AVTs) ----------------

        private void CollectStaticBools()
        {
            foreach (var el in _root.DescendantsAndSelf())
            {
                if (el.Name.NamespaceName != Stylesheet.XslNamespace)
                    continue;
                var local = el.Name.LocalName;
                if (local != "param" && local != "variable")
                    continue;
                var staticAttr = (el.Attribute("static") ?? el.Attribute("_static"))?.Value?.Trim().ToLowerInvariant();
                if (staticAttr is not ("yes" or "true" or "1"))
                    continue;
                var name = el.Attribute("name")?.Value?.Trim();
                var select = el.Attribute("select")?.Value?.Trim();
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(select))
                    continue;
                switch (select.ToLowerInvariant())
                {
                    case "true()": case "true": case "yes": case "1":
                        _staticBools[name] = true;
                        break;
                    case "false()": case "false": case "no": case "0":
                        _staticBools[name] = false;
                        break;
                }
            }
        }

        private bool? ResolveStreamable(XElement el)
        {
            var attr = el.Attribute("streamable") ?? el.Attribute("_streamable");
            if (attr == null)
                return null;
            var value = attr.Value.Trim();
            switch (value.ToLowerInvariant())
            {
                case "yes": case "true": case "1": return true;
                case "no": case "false": case "0": return false;
            }
            if (value.Length > 2 && value[0] == '{' && value[value.Length - 1] == '}')
            {
                var name = value.Substring(1, value.Length - 2).Trim();
                if (name.StartsWith("$", StringComparison.Ordinal))
                    name = name.Substring(1);
                if (_staticBools.TryGetValue(name, out var b))
                    return b;
            }
            return null;
        }

        // ---------------- function / attribute-set lookups ----------------

        private Dictionary<(string ns, string name, int arity), XsltFunctionDefinition> Functions()
            => _functions ??= _stylesheet.GetAllFunctionDefinitions(includePrivate: true);

        private XsltFunctionDefinition? FindUserFunction(string? prefix, string local, int arity, XElement context)
        {
            if (string.IsNullOrEmpty(prefix))
                return null; // unprefixed calls resolve to the fn namespace
            var ns = context.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? "";
            return Functions().TryGetValue((ns, local, arity), out var def) ? def : null;
        }

        private Dictionary<(string local, string ns), List<AttributeSetDefinition>> AttributeSets()
            => _attrSets ??= _stylesheet.GetAllAttributeSets();

        private List<AttributeSetDefinition>? FindAttributeSet(string token, XElement context)
        {
            string local = token, ns = "";
            var colon = token.IndexOf(':');
            if (colon >= 0)
            {
                var prefix = token.Substring(0, colon);
                local = token.Substring(colon + 1);
                ns = context.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? "";
            }
            return AttributeSets().TryGetValue((local, ns), out var list) ? list : null;
        }

        // ---------------- xsl:function @streamability validation (F14) ----------------

        private void ValidateFunctionDeclarations()
        {
            var seen = new HashSet<XElement>(ReferenceEqualityComparer.Instance);
            foreach (var def in Functions().Values)
            {
                if (!seen.Add(def.Element))
                    continue;
                var streamability = def.Element.Attribute("streamability")?.Value?.Trim().ToLowerInvariant();
                if (streamability == null)
                    continue;
                ValidateFunctionBody(def, streamability);
            }
        }

        private void ValidateFunctionBody(XsltFunctionDefinition def, string streamability)
        {
            // Parameter declarations and types.
            var paramEls = def.Element.Elements(XName.Get("param", Stylesheet.XslNamespace)).ToList();
            if (streamability == "shallow-descent")
            {
                // The first (descent) parameter must be explicitly typed.
                if (paramEls.Count > 0 && string.IsNullOrEmpty(paramEls[0].Attribute("as")?.Value))
                    throw Error($"shallow-descent function '{def.LocalName}' must declare a type on its first parameter.");
            }
            if (streamability is "inspection" or "filter" or "ascent" && paramEls.Count > 0)
            {
                // The first parameter must be a single node, not a sequence (node()* / item()*).
                var asType = paramEls[0].Attribute("as")?.Value ?? "item()*";
                if (asType.EndsWith("*", StringComparison.Ordinal)
                    || (asType.EndsWith("?", StringComparison.Ordinal) && asType != "node()?"))
                    throw Error($"{streamability} function '{def.LocalName}' must declare its first parameter as a single node.");
            }

            var env = Env.Base.Spawn();
            var absorbingParam = streamability == "absorbing" && def.ParameterNames.Count > 0 ? def.ParameterNames[0] : null;
            if (streamability == "shallow-descent" && def.ParameterNames.Count > 0)
                env.ShallowDescentParam = def.ParameterNames[0];
            foreach (var name in def.ParameterNames)
            {
                var p = paramEls.FirstOrDefault(e => e.Attribute("name")?.Value == name);
                var isFirst = def.ParameterNames.Count > 0 && name == def.ParameterNames[0];
                var streamed = isFirst && streamability is "inspection" or "filter" or "ascent" or "absorbing" or "shallow-descent";
                env = env.WithVar(name, new Var
                {
                    Posture = streamed ? Posture.Striding : Posture.Grounded,
                    Streamed = streamed,
                    BooleanTyped = IsBooleanTyped(p?.Attribute("as")?.Value),
                });
            }

            // An absorbing function absorbs its first argument into memory, so it may
            // reference the argument any number of times; a result it DELIVERS via
            // xsl:sequence must nevertheless be grounded (su-absorbing-901 returns the
            // streamed node itself). Surfaces feeding constructors (copy/@select, AVTs)
            // build grounded results and are not delivery surfaces.
            if (absorbingParam != null)
            {
                foreach (var seq in def.Element.DescendantsAndSelf()
                             .Where(e => e.Name.NamespaceName == Stylesheet.XslNamespace && e.Name.LocalName == "sequence"))
                {
                    var ast = TryParse(seq.Attribute("select")?.Value ?? "");
                    if (ast == null)
                        continue;
                    if (Analyze(ast, env) is { Posture: not Posture.Grounded, Captured: false })
                        throw Error($"absorbing function '{def.LocalName}' has a non-grounded result and is not streamable.");
                }
                // At most one consuming reference to the absorbed argument when the declared
                // type permits a sequence (Saxon issue 4561, adopted by the test suite:
                // su-absorbing-205 head()+tail(), su-absorbing-908 empty()+$input/* — both
                // declare node()*/element()*). A single-node parameter may be referenced
                // any number of times (su-absorbing-204/007). References inside inspection
                // functions — has-children, exists, namespace-uri, local-name — do not
                // consume the in-memory argument.
                var firstAs = paramEls.FirstOrDefault(e => e.Attribute("name")?.Value == def.ParameterNames[0])?.Attribute("as")?.Value ?? "item()*";
                if (firstAs.EndsWith("*", StringComparison.Ordinal) || firstAs.EndsWith("+", StringComparison.Ordinal))
                {
                    var consumingRefs = 0;
                    foreach (var el in def.Element.DescendantsAndSelf())
                    {
                        foreach (var surface in XPathSurfaces(el))
                        {
                            var ast = TryParse(surface);
                            if (ast != null)
                                consumingRefs += CountConsumingRefs(ast, absorbingParam);
                        }
                    }
                    if (consumingRefs > 1)
                        throw Error($"absorbing function '{def.LocalName}' has multiple consuming references to its argument and is not streamable.");
                }
            }

            // Inspection, filter, and ascent functions guarantee a motionless body: any
            // consuming use of the streamed parameter makes the function non-streamable
            // (su-inspection-901 / su-ascent-901: string($element) inside the body).
            if (streamability is "inspection" or "filter" or "ascent")
            {
                foreach (var el in def.Element.DescendantsAndSelf())
                {
                    foreach (var surface in XPathSurfaces(el))
                    {
                        var ast = TryParse(surface);
                        if (ast == null)
                            continue;
                        var info = Analyze(ast, env);
                        if (info.Consumes > 0)
                            throw Error($"{streamability} function '{def.LocalName}' has a consuming reference to its streamed argument and is not streamable.");
                        // An inspection function must deliver a grounded result: it only
                        // looks at the streamed node, it may not return it (su-inspection-903
                        // returns $element itself). Ascent results keep the argument's
                        // posture by design and are validated at the call site instead.
                        if (streamability == "inspection" && info.Posture != Posture.Grounded && !info.Captured)
                            throw Error($"inspection function '{def.LocalName}' has a non-grounded result and is not streamable.");
                        // §19.8.5.4: a filter body must be striding (or grounded, e.g. an
                        // atomized result) — it may not deliver crawling or climbing nodes
                        // (su-filter-903 returns $element/..).
                        if (streamability == "filter" && info.Posture is Posture.Crawling or Posture.Climbing or Posture.Roaming)
                            throw Error($"filter function '{def.LocalName}' delivers non-striding nodes and is not streamable.");
                    }
                }
            }

            // A shallow-descent argument may only be delivered once, via path steps.
            // Delivering it as a bare operand of the mapping operator repeats the delivery
            // without consuming the argument (su-shallow-descent-903: (1 to 5) ! $n).
            if (streamability == "shallow-descent" && def.ParameterNames.Count > 0)
            {
                var descent = def.ParameterNames[0];
                foreach (var el in def.Element.DescendantsAndSelf())
                {
                    foreach (var surface in XPathSurfaces(el))
                    {
                        var ast = TryParse(surface);
                        if (ast != null && DeliversParamViaBang(ast, descent))
                            throw Error($"shallow-descent function '{def.LocalName}' delivers its argument via the mapping operator without consuming it and is not streamable.");
                    }
                }
            }

            WalkConstructor(def.Element, env);
        }

        private static bool IsBooleanTyped(string? asType)
            => asType != null && asType.Trim().StartsWith("xs:boolean", StringComparison.Ordinal);

        private static bool IsBareParamRef(XPathAstNode node, string name) => node switch
        {
            VariableReferenceNode v => v.LocalName == name,
            ParenthesizedExprNode p => IsBareParamRef(p.Expression, name),
            _ => false,
        };

        // A shallow-descent argument may only be delivered once, via path steps. Delivering
        // it as a bare operand of the mapping operator repeats the delivery without consuming
        // the argument (su-shallow-descent-903: (1 to 5) ! $n).
        private static bool DeliversParamViaBang(XPathAstNode node, string name) => node switch
        {
            BinaryExpressionNode b when b.Operator == BinaryOperator.SimpleMap =>
                IsBareParamRef(b.Left, name) || IsBareParamRef(b.Right, name)
                || DeliversParamViaBang(b.Left, name) || DeliversParamViaBang(b.Right, name),
            BinaryExpressionNode b => DeliversParamViaBang(b.Left, name) || DeliversParamViaBang(b.Right, name),
            ParenthesizedExprNode p => DeliversParamViaBang(p.Expression, name),
            IfExpressionNode i => DeliversParamViaBang(i.Condition, name) || DeliversParamViaBang(i.ThenBranch, name) || DeliversParamViaBang(i.ElseBranch, name),
            SequenceExpressionNode s => s.Expressions.Any(e => DeliversParamViaBang(e, name)),
            FunctionCallNode f => f.Arguments.Any(a => DeliversParamViaBang(a, name)),
            PathExprNode p => p.Steps.Any(s => s is StepNode sn && sn.Predicates.Any(pr => ContainsBangInPredicates(pr, name))),
            PostfixPredicateNode pp => DeliversParamViaBang(pp.Expression, name) || ContainsBangInPredicates(pp.Predicate, name),
            UnaryExpressionNode u => DeliversParamViaBang(u.Operand, name),
            RangeExpressionNode r => DeliversParamViaBang(r.From, name) || DeliversParamViaBang(r.To, name),
            LetExpressionNode l => l.Bindings.Any(b => DeliversParamViaBang(b.Expression, name)) || DeliversParamViaBang(l.Body, name),
            ForExpressionNode f => f.Bindings.Any(b => DeliversParamViaBang(b.Expression, name)) || DeliversParamViaBang(f.ReturnExpression, name),
            QuantifiedExpressionNode q => q.Bindings.Any(b => DeliversParamViaBang(b.Expression, name)) || DeliversParamViaBang(q.SatisfiesExpression, name),
            TryCatchNode t => DeliversParamViaBang(t.TryExpression, name),
            ArrowExprNode a => DeliversParamViaBang(a.Source, name),
            DynamicFunctionCallNode d => DeliversParamViaBang(d.Function, name) || d.Arguments.Any(x => DeliversParamViaBang(x, name)),
            MapConstructorNode m => m.Entries.Any(e => DeliversParamViaBang(e.Key, name) || DeliversParamViaBang(e.Value, name)),
            ArrayConstructorNode a => a.Items.Any(i => DeliversParamViaBang(i, name)),
            LookupNode l => DeliversParamViaBang(l.Expression, name) || DeliversParamViaBang(l.Key, name),
            _ => false,
        };

        private static bool ContainsBangInPredicates(XPathAstNode node, string name) => node switch
        {
            PredicateNode pd => DeliversParamViaBang(pd.Expression, name),
            _ => DeliversParamViaBang(node, name),
        };

        // Whether an instruction is nested inside a constructor (literal result element,
        // xsl:element, or xsl:copy content) rather than directly in a sequence constructor
        // such as xsl:function or xsl:template.
        private static bool InConstructor(XElement el) => el.Ancestors().Any(a =>
            a.Name.NamespaceName != Stylesheet.XslNamespace
            || (a.Name.NamespaceName == Stylesheet.XslNamespace && a.Name.LocalName is "element" or "copy"));


        private static int CountVarRefs(XPathAstNode node, string name) => node switch
        {
            VariableReferenceNode v => v.LocalName == name ? 1 : 0,
            StepNode s => s.Predicates.Sum(p => CountVarRefs(p, name)),
            PathExprNode p => p.Steps.Sum(s => CountVarRefs(s, name)),
            PostfixPredicateNode pp => CountVarRefs(pp.Expression, name) + CountVarRefs(pp.Predicate, name),
            PredicateNode pd => CountVarRefs(pd.Expression, name),
            FunctionCallNode f => f.Arguments.Sum(a => CountVarRefs(a, name)),
            NamedFunctionRefNode => 0,
            SequenceExpressionNode s => s.Expressions.Sum(e => CountVarRefs(e, name)),
            ParenthesizedExprNode p => CountVarRefs(p.Expression, name),
            IfExpressionNode i => CountVarRefs(i.Condition, name) + CountVarRefs(i.ThenBranch, name) + CountVarRefs(i.ElseBranch, name),
            BinaryExpressionNode b => CountVarRefs(b.Left, name) + CountVarRefs(b.Right, name),
            UnaryExpressionNode u => CountVarRefs(u.Operand, name),
            RangeExpressionNode r => CountVarRefs(r.From, name) + CountVarRefs(r.To, name),
            LetExpressionNode l => l.Bindings.Sum(b => CountVarRefs(b.Expression, name)) + CountVarRefs(l.Body, name),
            ForExpressionNode f => f.Bindings.Sum(b => CountVarRefs(b.Expression, name)) + CountVarRefs(f.ReturnExpression, name),
            QuantifiedExpressionNode q => q.Bindings.Sum(b => CountVarRefs(b.Expression, name)) + CountVarRefs(q.SatisfiesExpression, name),
            MapConstructorNode m => m.Entries.Sum(e => CountVarRefs(e.Key, name) + CountVarRefs(e.Value, name)),
            ArrayConstructorNode a => a.Items.Sum(i => CountVarRefs(i, name)),
            LookupNode l => CountVarRefs(l.Expression, name) + CountVarRefs(l.Key, name),
            LookupWildcardNode l => CountVarRefs(l.Expression, name),
            InlineFunctionNode i => 0, // body scope hides the outer param
            ArrowExprNode a => CountVarRefs(a.Source, name),
            DynamicFunctionCallNode d => CountVarRefs(d.Function, name) + d.Arguments.Sum(a => CountVarRefs(a, name)),
            TryCatchNode t => CountVarRefs(t.TryExpression, name),
            _ => 0,
        };

        // ---------------- template rules of streamable modes ----------------

        private void AnalyzeTemplateRules()
        {
            foreach (var rule in _stylesheet.GetAllTemplateRules())
            {
                if (!IsStreamableRule(rule))
                    continue;
                if (!string.IsNullOrEmpty(rule.Match))
                    CheckPattern(rule.Match, rule.Element, "template match pattern");
                var env = Env.Base.Spawn();
                // A rule whose match is a type pattern for a non-node type (map, array,
                // atomic, function) is invoked with a grounded context item, not a
                // streamed node (si-fork-119: match=".[. instance of map(...)]").
                if (!IsNonNodeTypePattern(rule.Match))
                {
                    env.ContextStreamed = true;
                    env.ContextPosture = Posture.Striding;
                    env.CurrentStreamed = true;
                }
                CheckAccumulatorUsage(rule.Element);
                WalkConstructor(rule.Element, env);
            }
        }

        private static bool IsNonNodeTypePattern(string? match)
        {
            if (string.IsNullOrEmpty(match))
                return false;
            var i = match.IndexOf("instance of", StringComparison.Ordinal);
            if (i < 0)
                return false;
            var rest = match[(i + "instance of".Length)..].TrimStart();
            return rest.StartsWith("map(", StringComparison.Ordinal)
                || rest.StartsWith("array(", StringComparison.Ordinal)
                || rest.StartsWith("xs:", StringComparison.Ordinal)
                || rest.StartsWith("function(", StringComparison.Ordinal);
        }

        private bool IsStreamableRule(TemplateRule rule)
        {
            foreach (var mode in rule.Modes)
            {
                if (mode is "#all" or "#current")
                {
                    if (AnyStreamableMode(_root))
                        return true;
                }
                else if (_stylesheet.GetModeDefinition(mode)?.Streamable == true)
                {
                    return true;
                }
            }
            return false;
        }

        // ---------------- xsl:source-document (streamable) ----------------

        private void AnalyzeSourceDocuments()
        {
            foreach (var el in _root.DescendantsAndSelf())
            {
                if (el.Name.NamespaceName != Stylesheet.XslNamespace || el.Name.LocalName != "source-document")
                    continue;
                if (ResolveStreamable(el) != true)
                    continue;
                // Template rules with a match attribute are analyzed during the rule walk
                // (with correct outer scope); named templates are analyzed here.
                if (el.Ancestors().Any(a => a.Name.NamespaceName == Stylesheet.XslNamespace
                    && a.Name.LocalName == "template" && a.Attribute("match") != null))
                    continue;
                var env = Env.Base.Spawn();
                env.ContextStreamed = true;
                env.ContextPosture = Posture.Striding;
                // Route through WalkInstruction so the source-document case applies its
                // content checks (bare xsl:sequence escape rule) as well.
                WalkInstruction(el, env);
            }
        }

        // ---------------- streamable accumulators (§18.2.4) ----------------

        /// <summary>
        /// XTSE3430 checks for accumulators declared streamable: the initial-value expression
        /// must be motionless, every rule match pattern must be motionless and non-positional,
        /// and a rule body may neither consume the streamed descendants of the matched node
        /// nor return a reference to it (accumulator-009s/019s/029s/030s/059/060/076).
        /// </summary>
        private void AnalyzeAccumulators()
        {
            foreach (var acc in _root.DescendantsAndSelf())
            {
                if (acc.Name.NamespaceName != Stylesheet.XslNamespace || acc.Name.LocalName != "accumulator")
                    continue;
                if (ResolveStreamable(acc) != true)
                    continue;
                var accName = acc.Attribute("name")?.Value ?? "";
                var initial = acc.Attribute("initial-value") ?? acc.Attribute("_initial-value");
                if (initial != null && TryParse(initial.Value) is { } initAst)
                {
                    var initEnv = Env.Base.Spawn();
                    initEnv.ContextStreamed = true;
                    initEnv.ContextPosture = Posture.Striding;
                    var initInfo = Analyze(initAst, initEnv);
                    if (initInfo.Consumes > 0 || !initInfo.Motionless)
                        throw Error($"the initial-value expression of streamable accumulator '{accName}' is not motionless ('{initial.Value}').");
                }
                foreach (var rule in acc.Elements(XName.Get("accumulator-rule", Stylesheet.XslNamespace)))
                {
                    var match = rule.Attribute("match")?.Value;
                    if (match != null)
                        CheckPattern(match, rule, "accumulator-rule match pattern");
                    var select = rule.Attribute("select") ?? rule.Attribute("_select");
                    if (select == null)
                        continue; // sequence-constructor rule bodies are validated at runtime
                    if (TryParse(select.Value) is not { } ast)
                        continue;
                    var env = Env.Base.Spawn();
                    env.ContextStreamed = true;
                    env.ContextPosture = Posture.Striding;
                    // The rule fires on the node being visited; its descendants have not
                    // been read yet, so navigating downward from it consumes the stream.
                    env.ContextFresh = true;
                    // Rules matched on buffered leaf nodes (text()/comment()/PI) may
                    // atomize the context (accumulator-059: tokenize(.) on match="text()").
                    if (RuleMatchSelectsLeaf(match))
                        env.LeafContext = true;
                    var info = Analyze(ast, env);
                    if (info.Consumes > 0)
                        throw Error($"the select expression of a rule of streamable accumulator '{accName}' must not navigate the streamed descendants of the matched node ('{select.Value}').");
                    if (info.Posture != Posture.Grounded && !info.Captured)
                        throw Error($"the result of a rule of streamable accumulator '{accName}' must be grounded ('{select.Value}').");
                }
            }
        }

        private static bool RuleMatchSelectsLeaf(string? match)
        {
            if (string.IsNullOrEmpty(match))
                return false;
            var lastStep = match;
            var slash = match.LastIndexOf('/');
            if (slash >= 0)
                lastStep = match[(slash + 1)..];
            return lastStep.Contains("text()", StringComparison.Ordinal)
                || lastStep.Contains("comment()", StringComparison.Ordinal)
                || lastStep.Contains("processing-instruction(", StringComparison.Ordinal)
                || lastStep.TrimStart().StartsWith("@", StringComparison.Ordinal);
        }

        // ---------------- accumulator functions in streamable modes ----------------

        private static readonly Regex AccumulatorPathStep = new(@"/\s*accumulator-(?:after|before)\s*\(",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// §18.2.4: within a template rule of a streamable mode, accumulator-after() may only
        /// be evaluated once the descendants of the context node have been processed — i.e. in
        /// a post-descent instruction. An instruction is post-descent when it follows (in the
        /// same sequence constructor, at any nesting depth) an instruction that descends into
        /// or consumes the context node's descendants: xsl:apply-templates/next-match/
        /// apply-imports, or any instruction whose select expression consumes the stream
        /// (accumulator-015/036/053). xsl:attribute/@select is exempt: attribute creation
        /// order is implementation-defined, so accumulator-after is always allowed there
        /// (accumulator-008). Both accumulator functions may only be called on the context
        /// node, never as a path step (accumulator-060).
        /// </summary>
        private void CheckAccumulatorUsage(XElement ruleEl)
        {
            var env = Env.Base.Spawn();
            env.ContextStreamed = true;
            env.ContextPosture = Posture.Striding;
            env.ContextFresh = true;
            env.CurrentStreamed = true;
            CheckAccumulatorDescent(ruleEl, false, env);
        }

        /// <summary>Checks one sequence constructor; returns true when its evaluation consumed
        /// the descendants of the context node (so following siblings are post-descent).</summary>
        private bool CheckAccumulatorDescent(XElement container, bool postDescent, Env env)
        {
            foreach (var el in container.Elements())
            {
                // xsl:attribute/@select is exempt from the post-descent rule.
                var exempt = el.Name.NamespaceName == Stylesheet.XslNamespace
                    && el.Name.LocalName == "attribute";
                if (!exempt)
                {
                    foreach (var attr in el.Attributes())
                    {
                        var v = attr.Value;
                        if (AccumulatorPathStep.IsMatch(v))
                            throw Error("accumulator-before()/accumulator-after() may only be called on the context node in a streamable template.");
                        if (!postDescent && v.Contains("accumulator-after(", StringComparison.Ordinal))
                            throw Error("accumulator-after() is a post-descent function and may not be evaluated before the descendants of the context node have been processed.");
                    }
                }
                if (CheckAccumulatorDescent(el, postDescent, env))
                    postDescent = true;
            }
            return postDescent || ConsumesDescendants(container, env);
        }

        private bool ConsumesDescendants(XElement el, Env env)
        {
            if (el.Name.NamespaceName != Stylesheet.XslNamespace)
                return false;
            if (el.Name.LocalName is "next-match" or "apply-imports")
                return true;
            var select = el.Attribute("select")?.Value;
            if (string.IsNullOrEmpty(select))
                return el.Name.LocalName == "apply-templates";
            if (el.Name.LocalName is not ("apply-templates" or "for-each" or "iterate" or "for-each-group"
                or "value-of" or "copy-of" or "sequence" or "variable" or "if" or "message" or "result-document"))
                return false;
            if (TryParse(select) is not { } ast)
                return false;
            try
            {
                var info = Analyze(ast, env);
                return info.Consumes > 0;
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith("XTSE3430:", StringComparison.Ordinal))
            {
                // The accumulator post-descent scan uses an approximate environment (no
                // group context); a streamability error raised here does not reflect the
                // real walk and is conservatively treated as a consuming expression.
                return true;
            }
        }

        // ------------------------------------------------------------------
        // Instruction walking (R3 per-instruction consuming-use counting)
        // ------------------------------------------------------------------

        private void WalkConstructor(XElement container, Env env)
        {
            foreach (var child in container.Elements())
                WalkInstruction(child, env);
        }

        private void WalkInstruction(XElement el, Env env)
        {
            if (el.Name.NamespaceName != Stylesheet.XslNamespace)
            {
                WalkLiteralResultElement(el, env);
                return;
            }

            switch (el.Name.LocalName)
            {
                case "source-document":
                {
                    var streamable = ResolveStreamable(el);
                    if (streamable == true)
                    {
                        var inner = env.Spawn();
                        inner.ContextStreamed = true;
                        inner.ContextPosture = Posture.Striding;
                        inner.GroupInScope = false;
                        inner.GroupOutside = env.GroupInScope || env.GroupOutside;
                        // §19.8.4.41: the result of the instruction must be grounded. A bare
                        // direct-child xsl:sequence returning streamed nodes escapes the
                        // streaming pass; nested sequences feed a constructor and are fine
                        // (si-coco-001). The same applies to a direct-child xsl:for-each or
                        // xsl:iterate whose body sequences deliver the result of the
                        // instruction (si-for-each-907 / si-iterate-907).
                        foreach (var seq in el.Elements(XName.Get("sequence", Stylesheet.XslNamespace)))
                        {
                            var selInfo = AnalyzeSurface(seq, "select", inner);
                            if (selInfo.Posture != Posture.Grounded && !selInfo.Captured)
                                throw Error("the result of xsl:source-document must be grounded (a bare xsl:sequence returns streamed nodes).");
                        }
                        foreach (var loop in el.Elements()
                                     .Where(e => e.Name.NamespaceName == Stylesheet.XslNamespace
                                              && (e.Name.LocalName == "for-each" || e.Name.LocalName == "iterate")))
                        {
                            foreach (var seq in loop.Elements(XName.Get("sequence", Stylesheet.XslNamespace)))
                            {
                                var bodyInfo = AnalyzeSurface(seq, "select", inner);
                                if (bodyInfo.Posture != Posture.Grounded && !bodyInfo.Captured)
                                    throw Error("the result of xsl:source-document must be grounded (the loop delivers streamed nodes).");
                            }
                        }
                        WalkConstructor(el, inner);
                    }
                    else
                    {
                        WalkConstructor(el, env);
                    }
                    return;
                }

                case "for-each":
                case "iterate":
                {
                    var sel = AnalyzeSurface(el, "select", env);
                    var selectName = el.Name.LocalName == "iterate" ? "xsl:iterate" : "xsl:for-each";
                    if (sel.Posture is Posture.Roaming)
                        throw Error($"the select expression of {selectName} has roaming posture and is not streamable.");
                    CheckSortInstructions(el, sel.Posture != Posture.Grounded);

                    // §19.8.4.18/22: a crawling select is allowed unless the body's sweep is
                    // consuming; the body is walked with the select's context posture, in
                    // which any consuming access already raises XTSE3430 (si-group-B/C feg-005).
                    var body = env.Spawn();
                    body.GroupInScope = false;
                    body.GroupOutside = env.GroupInScope || env.GroupOutside;
                    if (sel.Posture != Posture.Grounded)
                    {
                        body.ContextStreamed = true;
                        body.ContextPosture = sel.Posture;
                        // Buffered items (attributes, text, comments, PIs) may be atomized
                        // or copied repeatedly without advancing the stream (si-assert-004/006).
                        if (sel.LeafItem)
                            body.LeafContext = true;
                    }
                    else
                    {
                        body.ContextStreamed = false;
                        body.ContextPosture = Posture.Grounded;
                    }
                    if (sel.Posture == Posture.Crawling && ConstructorConsumes(el, body) > 0)
                        throw Error($"the contained sequence constructor of {selectName} consumes a streamed value selected with crawling posture.");

                    if (el.Name.LocalName == "iterate")
                    {
                        foreach (var child in el.Elements())
                        {
                            if (child.Name.NamespaceName == Stylesheet.XslNamespace)
                            {
                                if (child.Name.LocalName == "param")
                                {
                                    body = WalkParamBinding(child, body, env, isIterateParam: true);
                                    continue;
                                }
                                if (child.Name.LocalName == "next-iteration")
                                {
                                    foreach (var wp in child.Elements(XName.Get("with-param", Stylesheet.XslNamespace)))
                                        WalkWithParam(wp, body, requireGrounded: true);
                                    continue;
                                }
                                if (child.Name.LocalName == "sort")
                                    continue; // handled by CheckSortInstructions
                            }
                            WalkInstruction(child, body);
                        }
                        return;
                    }

                    WalkConstructor(el, body);
                    return;
                }

                case "for-each-group":
                {
                    var sel = AnalyzeSurface(el, "select", env);
                    if (sel.Posture is Posture.Roaming)
                        throw Error("the select expression of xsl:for-each-group has roaming posture and is not streamable.");
                    CheckSortInstructions(el, sel.Posture != Posture.Grounded);

                    // Grouping keys must be motionless (F7). Keys that atomize the delivered
                    // buffered items (text/attributes) are motionless too (si-fork-805).
                    var streamedItems = sel.Posture != Posture.Grounded;
                    var keyEnv = streamedItems ? env.WithContext(Posture.Striding, streamed: true) : env.WithContext(Posture.Grounded, streamed: false);
                    if (sel.LeafItem)
                        keyEnv.LeafContext = true;
                    foreach (var keyAttr in new[] { "group-by", "group-adjacent" })
                    {
                        var expr = el.Attribute(keyAttr);
                        if (expr == null)
                            continue;
                        var info = AnalyzeSurface(el, keyAttr, keyEnv);
                        if (!info.Motionless)
                            throw Error($"the {keyAttr} expression of xsl:for-each-group must be motionless.");
                    }
                    foreach (var keyAttr in new[] { "group-starting-with", "group-ending-with" })
                    {
                        var pattern = el.Attribute(keyAttr)?.Value;
                        if (!string.IsNullOrEmpty(pattern))
                            CheckPattern(pattern, el, keyAttr, sel);
                    }

                    var body = env.Spawn();
                    body.GroupOutside = env.GroupOutside;
                    if (streamedItems)
                    {
                        body.ContextStreamed = true;
                        body.ContextPosture = sel.Posture;
                        body.GroupInScope = true;
                        body.GroupSelectPosture = sel.Posture;
                        if (sel.LeafItem)
                            body.LeafContext = true;
                    }
                    else
                    {
                        // Grounded selection (copy-of()/snapshot()): the body context is
                        // grounded and current-group() holds grounded items (si-group-203).
                        body.ContextStreamed = false;
                        body.ContextPosture = Posture.Grounded;
                        body.GroupInScope = true;
                        body.GroupSelectPosture = Posture.Grounded;
                    }
                    if (sel.Posture == Posture.Crawling && ConstructorConsumes(el, body) > 0)
                        throw Error("the contained sequence constructor of xsl:for-each-group consumes a streamed value selected with crawling posture.");
                    foreach (var child in el.Elements())
                    {
                        if (child.Name.NamespaceName == Stylesheet.XslNamespace && child.Name.LocalName == "sort")
                            continue;
                        WalkInstruction(child, body);
                    }
                    return;
                }

                case "apply-templates":
                {
                    Info sel;
                    if (el.Attribute("select") == null)
                    {
                        // default select="child::node()"
                        sel = env.ContextStreamed
                            ? new Info(Posture.Striding, 1, Motionless: false, RefsStreamed: true, Fresh: false)
                            : Info.GroundedMotionless;
                    }
                    else
                    {
                        sel = AnalyzeSurface(el, "select", env);
                    }
                    if (sel.Posture is Posture.Crawling or Posture.Climbing)
                        throw Error("the select expression of xsl:apply-templates is not streamable.");
                    CheckSortInstructions(el, sel.Posture != Posture.Grounded);
                    foreach (var child in el.Elements(XName.Get("with-param", Stylesheet.XslNamespace)))
                        WalkWithParam(child, env);
                    return;
                }

                case "call-template":
                case "next-match":
                {
                    foreach (var child in el.Elements(XName.Get("with-param", Stylesheet.XslNamespace)))
                        WalkWithParam(child, env);
                    WalkConstructor(el, env);
                    return;
                }

                case "sequence":
                {
                    // §19.8.4.36: posture/sweep follow the general streamability rules; the
                    // select operand usage is transmission — a consuming select is one use,
                    // not an error. (The groundedness rule applies only to xsl:fork branches.)
                    CheckUnit(el, env, RawAttributes(el, "select"));
                    // A climbing or roaming node cannot be delivered into a constructor: it
                    // has already been passed by the input stream (su-ascent-903 / su-filter-903
                    // deliver ancestors into a literal result element; si-shallow-descent-905
                    // delivers a roaming result). Delivering striding children (si-coco-001)
                    // or a captured attribute (su-ascent-A's `! @id`) remains streamable.
                    // Function-body sequences are not constructor feeds (an ascent function
                    // legitimately returns climbing nodes from its body).
                    if (el.Attribute("select") != null && InConstructor(el))
                    {
                        var delivered = AnalyzeSurface(el, "select", env);
                        if (delivered.Posture is Posture.Climbing or Posture.Roaming && !delivered.Captured)
                            throw Error("a climbing or roaming node cannot be delivered into a constructor.");
                    }
                    WalkConstructor(el, env);
                    return;
                }

                case "copy-of":
                case "value-of":
                case "analyze-string":
                case "merge-key":
                {
                    CheckUnit(el, env, SelectLikeAttributes(el));
                    WalkConstructor(el, env);
                    return;
                }

                case "copy":
                {
                    if (el.Attribute("select") != null)
                        CheckUnit(el, env, new[] { "select" });
                    CheckAttributeSets(el, env);
                    // The content of xsl:copy with a select attribute is a higher-order operand:
                    // current-group() (and streaming function params) consumed here are not
                    // streamable (§19.3.1 note; bug 29482 → si-group-031).
                    var contentEnv = env;
                    if (el.Attribute("select") != null)
                    {
                        contentEnv = env.Spawn();
                        contentEnv.GroupInScope = false;
                        contentEnv.GroupOutside = env.GroupInScope || env.GroupOutside;
                    }
                    WalkConstructor(el, contentEnv);
                    return;
                }

                case "element":
                {
                    CheckUnit(el, env, AvtAttributes(el, "name", "namespace", "inherit-namespace"));
                    CheckAttributeSets(el, env);
                    WalkConstructor(el, env);
                    return;
                }

                case "attribute":
                {
                    CheckUnit(el, env, RawAttributes(el, "select").Concat(AvtAttributes(el, "name", "namespace")));
                    return;
                }

                case "if":
                case "when":
                {
                    CheckUnit(el, env, new[] { "test" });
                    WalkConstructor(el, env);
                    return;
                }

                case "assert":
                {
                    // xsl:assert counts test and content constructor as one instruction (F6).
                    var total = AnalyzeSurface(el, "test", env).Consumes;
                    foreach (var child in el.Elements())
                        total += UnitConsumes(child, env);
                    if (total > 1)
                        throw Error("xsl:assert has more than one consuming use of a streamed value.");
                    WalkConstructor(el, env);
                    return;
                }

                case "variable":
                case "param":
                {
                    env = WalkVariable(el, env);
                    WalkConstructor(el, env);
                    return;
                }

                case "with-param":
                {
                    WalkWithParam(el, env);
                    return;
                }

                case "sort":
                {
                    CheckSortInstructions(el, env.ContextStreamed, standalone: true);
                    return;
                }

                case "map":
                {
                    // §19.8.4.24: the content is a sequence constructor producing maps; it may
                    // contain any instructions (e.g. xsl:for-each yielding map-entries,
                    // si-where-populated-013/014). Streamed nodes inside entries are rejected
                    // by the xsl:map-entry and map-constructor rules. When not every child is
                    // an xsl:map-entry the implicit fork does not apply, so the children share
                    // one consuming use of the streamed value (si-map-903).
                    if (!el.Elements().All(e => e.Name.NamespaceName == Stylesheet.XslNamespace && e.Name.LocalName == "map-entry"))
                    {
                        var total = el.Elements().Sum(child => UnitConsumes(child, env));
                        if (total > 1)
                            throw Error("xsl:map has more than one consuming use of a streamed value.");
                    }
                    WalkConstructor(el, env);
                    return;
                }

                case "map-entry":
                {
                    var key = AnalyzeSurface(el, "key", env);
                    var value = el.Attribute("select") != null
                        ? AnalyzeSurface(el, "select", env)
                        : AnalyzeSurface(el, "value", env);
                    // Keys are atomized; captured attributes/namespaces of streamed nodes are
                    // buffered and may be stored. Elements/texts of the streamed document
                    // cannot (si-map-901: select="//AUTHOR" must fail).
                    if (key.Posture != Posture.Grounded && !key.Captured
                        || value.Posture != Posture.Grounded && !value.Captured)
                        throw Error("xsl:map-entry must not contain nodes from a streamed document.");
                    if (key.Consumes + value.Consumes > 1)
                        throw Error("xsl:map-entry has more than one consuming use of a streamed value.");
                    return;
                }

                case "number":
                {
                    CheckUnit(el, env, SelectLikeAttributes(el, "value", "count", "from", "select"));
                    return;
                }

                case "comment":
                case "processing-instruction":
                case "namespace":
                case "result-document":
                case "message":
                {
                    CheckUnit(el, env, RawAttributes(el, "select").Concat(AvtAttributes(el, "name", "href", "format")));
                    WalkConstructor(el, env);
                    return;
                }

                case "choose":
                case "otherwise":
                case "try":
                case "catch":
                case "fallback":
                case "on-completion":
                case "matching-substring":
                case "non-matching-substring":
                case "fork":
                case "where-populated":
                {
                    WalkConstructor(el, env);
                    return;
                }

                case "evaluate":
                case "perform-sort":
                {
                    CheckUnit(el, env, SelectLikeAttributes(el));
                    WalkConstructor(el, env);
                    return;
                }

                case "merge-source":
                {
                    // §19.8.4.16: for a streamable merge source the select expression must be
                    // striding (merge-094/095: log//record is crawling → XTSE3430).
                    if (ResolveStreamable(el) == true)
                    {
                        var selEnv = env.Spawn();
                        selEnv.ContextStreamed = true;
                        selEnv.ContextPosture = Posture.Striding;
                        selEnv.ContextFresh = true;
                        var info = AnalyzeSurface(el, "select", selEnv);
                        if (info.Posture is Posture.Crawling or Posture.Roaming)
                            throw Error("the select expression of a streamable merge source must be striding.");
                    }
                    WalkConstructor(el, env);
                    return;
                }

                default:
                {
                    CheckUnit(el, env, SelectLikeAttributes(el));
                    WalkConstructor(el, env);
                    return;
                }
            }
        }

        /// <summary>xsl:with-param: the select has type-determined usage based on the with-param's
        /// own @as (§19.8.4.5): an atomic type absorbs (consume once), item()* transmits. A striding
        /// binding is therefore streamable (si-call-template-001/002 atomize it in the callee);
        /// only last() over the streamed sequence is rejected. xsl:iterate rebinding
        /// (next-iteration) is stricter: the parameter must stay grounded (F8/F9).</summary>
        private void WalkWithParam(XElement el, Env env, bool requireGrounded = false)
        {
            var info = AnalyzeSurface(el, "select", env);
            if (requireGrounded && info.Posture != Posture.Grounded)
                throw Error("an xsl:iterate parameter must not be bound to a node from a streamed document.");
            if (info.UsesLast && env.ContextStreamed)
                throw Error("last() cannot be evaluated over a streamed node (with-param select).");
        }

        /// <summary>Local xsl:variable / xsl:param: a variable must not be bound to a node from
        /// a streamed document (§19.8.4.41: without as= the select has navigation usage, with
        /// as= a node type also gives navigation; either way the binding is free-ranging).</summary>
        private Env WalkVariable(XElement el, Env env)
        {
            var name = el.Attribute("name")?.Value?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                CheckUnit(el, env, SelectLikeAttributes(el));
                return env;
            }
            Var var;
            if (el.Attribute("select") != null)
            {
                var info = AnalyzeSurface(el, "select", env);
                if (info.Posture != Posture.Grounded)
                    throw Error($"the variable or parameter '{name}' cannot be bound to a node in a streamed document (use copy-of() or atomize the value).");
                var = new Var
                {
                    Posture = Posture.Grounded,
                    BooleanTyped = IsBooleanTyped(el.Attribute("as")?.Value),
                };
                if (info.Consumes > 1)
                    throw Error($"the select expression of xsl:variable '{name}' has more than one consuming use of a streamed value.");
            }
            else if (el.Name.LocalName == "param")
            {
                // A parameter without a select attribute receives its value from the caller;
                // the binding established when the function/template was entered stays in force.
                return env;
            }
            else
            {
                // Content constructor: grounded result. Children are walked by the caller.
                var = new Var { Posture = Posture.Grounded };
            }
            return env.WithVar(name, var);
        }

        /// <summary>xsl:param binding inside xsl:iterate (initial or rebinding).</summary>
        private Env WalkParamBinding(XElement el, Env bodyEnv, Env outerEnv, bool isIterateParam)
        {
            var name = el.Attribute("name")?.Value?.Trim() ?? "";
            if (el.Attribute("select") != null)
            {
                var info = AnalyzeSurface(el, "select", outerEnv);
                if (info.Posture != Posture.Grounded)
                    throw Error("an xsl:iterate parameter must not be bound to a node from a streamed document.");
            }
            return bodyEnv.WithVar(name, new Var
            {
                Posture = Posture.Grounded,
                BooleanTyped = IsBooleanTyped(el.Attribute("as")?.Value),
            });
        }

        private void CheckSortInstructions(XElement parent, bool streamedItems, bool standalone = false)
        {
            if (!streamedItems)
                return;
            foreach (var sort in parent.Elements(XName.Get("sort", Stylesheet.XslNamespace)))
            {
                var expr = sort.Attribute("select")?.Value;
                if (string.IsNullOrEmpty(expr))
                    continue;
                var ast = TryParse(expr);
                if (ast == null)
                    continue;
                Info info;
                try
                {
                    info = Analyze(ast, Env.Base.WithContext(Posture.Striding, streamed: true));
                }
                catch (InvalidOperationException ex) when (ex.Message.StartsWith("XTSE3430:", StringComparison.Ordinal))
                {
                    // Approximate environment (no lexical group context): streamability errors
                    // raised here do not reflect the real walk and are left to it.
                    continue;
                }
                if (info.UsesPosition || info.UsesLast)
                    throw Error("xsl:sort/@select must not use position() or last() over a streamed sequence.");
            }
        }

        private void WalkLiteralResultElement(XElement el, Env env)
        {
            var surfaces = new List<string>();
            foreach (var attr in el.Attributes())
            {
                if (attr.IsNamespaceDeclaration)
                    continue;
                if (attr.Name.NamespaceName == Stylesheet.XslNamespace)
                    continue; // xsl: attributes are not AVTs
                surfaces.AddRange(ExtractAvtExpressions(attr.Value));
            }
            if (surfaces.Count > 0)
                CheckUnit(el, env, surfaces);
            if (el.Attribute(XName.Get("use-attribute-sets", Stylesheet.XslNamespace)) is { } use)
                CheckAttributeSetReference(use.Value, el, env);
            WalkConstructor(el, env);
        }

        // ---------------- attribute sets (F5) ----------------

        private void CheckAttributeSets(XElement el, Env env)
        {
            var use = el.Attribute("use-attribute-sets")?.Value;
            if (!string.IsNullOrEmpty(use))
                CheckAttributeSetReference(use, el, env);
        }

        private void CheckAttributeSetReference(string use, XElement context, Env env)
        {
            foreach (var token in use.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var sets = FindAttributeSet(token, context);
                if (sets == null)
                    continue;
                foreach (var def in sets)
                {
                    var streamable = def.Element.Attribute("streamable")?.Value?.Trim().ToLowerInvariant();
                    if (streamable is "no" or "false" or "0")
                        throw Error($"attribute set '{token}' is not streamable.");
                    var bodyEnv = env.Spawn();
                    bodyEnv.InAttributeSet = true;
                    foreach (var child in def.Element.Elements(XName.Get("attribute", Stylesheet.XslNamespace)))
                    {
                        // Every attribute value in the set must be grounded and motionless;
                        // consuming bodies fail the analysis.
                        var select = child.Attribute("select")?.Value;
                        if (select == null)
                            continue;
                        var ast = TryParse(select);
                        if (ast == null)
                            continue;
                        var info = Analyze(ast, bodyEnv);
                        if (streamable == null)
                        {
                            if (info.Consumes > 0 || !info.Motionless || info.UsesLast)
                                throw Error($"attribute set '{token}' is not guaranteed streamable; declare streamable=\"yes\".");
                        }
                        else
                        {
                            if (info.UsesLast && bodyEnv.ContextStreamed)
                                throw Error($"attribute set '{token}' uses last() over a streamed node.");
                        }
                    }
                }
            }
        }

        // ---------------- patterns ----------------

        /// <summary>
        /// Checks a pattern used in a streamable context: every top-level predicate must be
        /// motionless and non-positional (XSLT 3.0 §19.8.10).
        /// </summary>
        private void CheckPattern(string pattern, XElement context, string what, Info? selectInfo = null)
        {
            foreach (var (open, close) in TopLevelPredicateSpans(pattern))
            {
                var predicate = pattern.Substring(open + 1, close - open - 1);
                if (ContainsCall(predicate, "current") || ContainsCall(predicate, "current-group"))
                {
                    // current() is allowed in motionless predicates; classify below will catch
                    // consuming uses. current-group() in patterns is always an error.
                    if (ContainsCall(predicate, "current-group"))
                        throw Error($"{what} must not call current-group().");
                }
                var ast = TryParse(predicate);
                if (ast == null)
                    continue;
                var env = Env.Base.Spawn();
                if (selectInfo is { Posture: Posture.Grounded })
                {
                    // The grouped items are grounded (e.g. //Item/copy-of()), so predicates
                    // navigating them are motionless (si-group-203).
                    env.ContextStreamed = false;
                    env.ContextPosture = Posture.Grounded;
                }
                else
                {
                    env.ContextStreamed = true;
                    env.ContextPosture = Posture.Striding;
                    // Buffered items (select="*/text()") may be atomized in the pattern
                    // (si-group-033: text()[ends-with(., ':')]); likewise when the pattern
                    // step itself selects leaf nodes (si-apply-templates-011).
                    if (selectInfo?.LeafItem == true || PredicateOnLeafStep(pattern, open))
                        env.LeafContext = true;
                }
                env.CurrentStreamed = true;
                env.InPattern = true;
                var info = Analyze(ast, env);
                if (info.Consumes > 0 || !info.Motionless)
                    throw Error($"{what} contains a predicate that is not motionless ('{predicate}').");
                if (info.UsesPosition || info.UsesLast)
                    throw Error($"{what} contains a positional predicate ('{predicate}').");
            }
        }

        private static bool ContainsCall(string expr, string name)        {
            var idx = 0;
            while ((idx = expr.IndexOf(name + "(", idx, StringComparison.Ordinal)) >= 0)
            {
                var before = idx == 0 ? ' ' : expr[idx - 1];
                if (!char.IsLetterOrDigit(before) && before != '_' && before != ':')
                    return true;
                idx += name.Length;
            }
            return false;
        }

        // (openIndex, closeIndex) spans of depth-1 [...] predicates, quote-aware.
        private static IEnumerable<(int Open, int Close)> TopLevelPredicateSpans(string pattern)
        {
            var i = 0;
            while (i < pattern.Length)
            {
                var c = pattern[i];
                if (c == '\'' || c == '"')
                {
                    var q = c;
                    i++;
                    while (i < pattern.Length && pattern[i] != q)
                        i++;
                    i++;
                    continue;
                }
                if (c == '[')
                {
                    var open = i;
                    var depth = 1;
                    i++;
                    while (i < pattern.Length && depth > 0)
                    {
                        var ch = pattern[i];
                        if (ch == '\'' || ch == '"')
                        {
                            var q = ch;
                            i++;
                            while (i < pattern.Length && pattern[i] != q)
                                i++;
                        }
                        else if (ch == '[') depth++;
                        else if (ch == ']') depth--;
                        i++;
                    }
                    yield return (open, i - 1);
                    continue;
                }
                i++;
            }
        }

        // Whether the step carrying the predicate at <paramref name="open"/> selects buffered
        // leaf items (text/attribute/namespace/comment/PI nodes): their string values are
        // available without advancing the stream, so atomizing predicates on them are
        // motionless (si-apply-templates-011: email/text()[. = 'abcde']).
        private static bool PredicateOnLeafStep(string pattern, int open)
        {
            var i = open - 1;
            var depth = 0;
            while (i >= 0)
            {
                var c = pattern[i];
                if (c == ')') depth++;
                else if (c == '(') depth--;
                else if (c == '/' && depth == 0) break;
                i--;
            }
            var segment = pattern.Substring(i + 1, open - i - 1);
            return segment.Contains("text()", StringComparison.Ordinal)
                || segment.Contains("comment()", StringComparison.Ordinal)
                || segment.Contains("processing-instruction(", StringComparison.Ordinal)
                || segment.Contains("attribute::", StringComparison.Ordinal)
                || segment.Contains("namespace::", StringComparison.Ordinal)
                || segment.TrimStart().StartsWith("@", StringComparison.Ordinal);
        }

        // ------------------------------------------------------------------
        // XPath surfaces and per-unit counting (R3)
        // ------------------------------------------------------------------

        /// <summary>XPath-bearing attributes of an element (select/test/group keys).</summary>
        private static IEnumerable<string> SelectLikeAttributes(XElement el, params string[] extra)
        {
            foreach (var name in new[] { "select", "test", "group-by", "group-adjacent" }.Concat(extra))
            {
                var value = el.Attribute(name)?.Value;
                if (!string.IsNullOrEmpty(value))
                    yield return value;
            }
        }

        private static IEnumerable<string> SelectLikeAttributes(XElement el) => SelectLikeAttributes(el, Array.Empty<string>());

        /// <summary>Attribute values that may contain AVT expressions.</summary>
        /// <summary>AVT fragments of the named attributes: only the {expr} parts are XPath surfaces —
        /// a plain literal such as name="a" is not an XPath expression and must not be analyzed.</summary>
        private static IEnumerable<string> AvtAttributes(XElement el, params string[] names)
        {
            foreach (var name in names)
            {
                var value = el.Attribute(name)?.Value;
                if (!string.IsNullOrEmpty(value))
                    foreach (var expr in ExtractAvtExpressions(value))
                        yield return expr;
            }
        }

        /// <summary>Raw XPath expressions of named plain (non-AVT) attributes such as select.</summary>
        private static IEnumerable<string> RawAttributes(XElement el, params string[] names)
        {
            foreach (var name in names)
            {
                var value = el.Attribute(name)?.Value;
                if (!string.IsNullOrEmpty(value))
                    yield return value;
            }
        }

        /// <summary>All XPath surfaces of an element: attributes plus AVT fragments.</summary>
        private static IEnumerable<string> XPathSurfaces(XElement el)
        {
            foreach (var value in SelectLikeAttributes(el))
                yield return value;
            foreach (var attr in el.Attributes())
            {
                if (attr.IsNamespaceDeclaration || attr.Name.NamespaceName == Stylesheet.XslNamespace)
                    continue;
                foreach (var avt in ExtractAvtExpressions(attr.Value))
                    yield return avt;
            }
        }

        /// <summary>Extracts the {expr} fragments of an attribute value template.</summary>
        private static IEnumerable<string> ExtractAvtExpressions(string value)
        {
            var i = 0;
            while (i < value.Length)
            {
                var c = value[i];
                if (c == '\'' || c == '"')
                {
                    var q = c;
                    i++;
                    while (i < value.Length && value[i] != q)
                        i++;
                    i++;
                    continue;
                }
                if (c == '{' && i + 1 < value.Length && value[i + 1] == '{')
                {
                    i += 2;
                    continue;
                }
                if (c == '{' && i > 0 && value[i - 1] == 'Q')
                {
                    i++;
                    continue;
                }
                if (c == '{')
                {
                    var start = i + 1;
                    var depth = 1;
                    i++;
                    while (i < value.Length && depth > 0)
                    {
                        var ch = value[i];
                        if (ch == '\'' || ch == '"')
                        {
                            var q = ch;
                            i++;
                            while (i < value.Length && value[i] != q)
                                i++;
                        }
                        else if (ch == '{') depth++;
                        else if (ch == '}') depth--;
                        i++;
                    }
                    var expr = value.Substring(start, i - start - 1);
                    if (!string.IsNullOrWhiteSpace(expr))
                        yield return expr;
                    continue;
                }
                i++;
            }
        }

        private XPathAstNode? TryParse(string expr)
        {
            try
            {
                var ast = XPathParser.Parse(expr);
                _lastParsedText = expr;
                return ast;
            }
            catch (Exception)
            {
                return null; // unparsable surfaces are skipped (conservative)
            }
        }

        /// <summary>Analyzes one XPath attribute surface and enforces unit-level rules.</summary>
        private Info AnalyzeSurface(XElement el, string attr, Env env)
        {
            var value = el.Attribute(attr)?.Value;
            if (string.IsNullOrEmpty(value))
                return Info.GroundedMotionless;
            return AnalyzeUnitExpression(value, env, $"{attr} expression");
        }

        private Info AnalyzeUnitExpression(string expr, Env env, string what)
        {
            var ast = TryParse(expr);
            if (ast == null)
                return Info.GroundedMotionless;
            var info = Analyze(ast, env);
            if (info.Consumes > 1)
                throw Error($"more than one consuming use of a streamed value in one instruction ({what} '{expr}').");
            if (info.UsesLast && env.ContextStreamed)
                throw Error($"last() cannot be evaluated over a streamed node ({what} '{expr}').");
            return info;
        }

        /// <summary>A consuming use count of an analyzed surface: the counted passes plus one
        /// when the surface is a bare non-grounded uncaptured streamed value (e.g. <c>.</c> as
        /// the whole select attribute) that the instruction atomizes.</summary>
        private static int UseCount(Info i)
            => i.Consumes + (i.Consumes == 0 && i.Posture != Posture.Grounded && !i.Captured && i.RefsStreamed ? 1 : 0);

        /// <summary>R3 unit check over several surfaces of one instruction.</summary>
        private void CheckUnit(XElement el, Env env, IEnumerable<string> surfaces)
        {
            var total = 0;
            foreach (var surface in surfaces)
            {
                var ast = TryParse(surface);
                if (ast == null)
                    continue;
                var info = Analyze(ast, env);
                total += UseCount(info);
                if (info.UsesLast && env.ContextStreamed)
                    throw Error($"last() cannot be evaluated over a streamed node ('{surface}').");
            }
            if (total > 1)
                throw Error($"more than one consuming use of a streamed value in one instruction ({el.Name.LocalName}).");
        }

        /// <summary>Consuming uses of one instruction unit (without raising R3 errors).</summary>
        private int UnitConsumes(XElement el, Env env)
        {
            var total = 0;
            foreach (var surface in SelectLikeAttributes(el))
            {
                var ast = TryParse(surface);
                if (ast != null)
                    total += UseCount(Analyze(ast, env));
            }
            return total;
        }

        /// <summary>Total consuming uses of every XPath surface in a constructor, assessed with
        /// one environment (used for the crawling-select body check of xsl:for-each and friends).</summary>
        private int ConstructorConsumes(XElement container, Env env)
        {
            var total = 0;
            foreach (var el in container.Descendants())
            {
                if (el.Name.NamespaceName == Stylesheet.XslNamespace && el.Name.LocalName == "sort")
                    continue;
                foreach (var surface in XPathSurfaces(el))
                {
                    var ast = TryParse(surface);
                    if (ast != null)
                        total += UseCount(Analyze(ast, env));
                }
            }
            return total;
        }

        // ------------------------------------------------------------------
        // XPath expression analysis
        // ------------------------------------------------------------------

        private Info Analyze(XPathAstNode? node, Env env)
        {
            if (node == null)
                return Info.GroundedMotionless;
            switch (node)
            {
                case BooleanLiteralNode:
                case IntegerLiteralNode:
                case DecimalLiteralNode:
                case DoubleLiteralNode:
                case StringLiteralNode:
                case ArgumentPlaceholderNode:
                    return Info.GroundedMotionless;

                case ContextItemNode:
                    // The string value of a text/attribute/comment/PI node is available without
                    // advancing the stream, so atomizing comparisons on such a context are
                    // motionless (§19.8.10: text()[. = 'Introduction'] is a motionless pattern).
                    if (env.LeafContext)
                        return Info.GroundedMotionless;
                    return new Info(env.ContextPosture, 0, Motionless: true,
                        RefsStreamed: env.ContextStreamed, Fresh: env.ContextStreamed && env.ContextFresh);

                case VariableReferenceNode v:
                {
                    var var = env.Lookup(v.LocalName);
                    if (var == null)
                        return Info.GroundedMotionless; // global or unknown variable: treat as grounded
                    return new Info(var.Posture, 0, Motionless: true,
                        RefsStreamed: var.Streamed, Fresh: var.Streamed, Roaming: var.Roaming);
                }

                case ParenthesizedExprNode p:
                    return Analyze(p.Expression, env);

                case PathExprNode path:
                    return AnalyzePath(path, env);

                case StepNode s:
                    // A single-step path parses to a bare StepNode.
                    return AnalyzePath(new PathExprNode(false, new XPathAstNode[] { s }), env);

                case PostfixPredicateNode pp:
                    return AnalyzeFilter(Analyze(pp.Expression, env), pp.Predicate, env, pp.Expression is VariableReferenceNode);

                case SequenceExpressionNode seq:
                {
                    var consumes = 0;
                    var motionless = true;
                    var usesLast = false;
                    var usesPosition = false;
                    var refs = false;
                    var fresh = false;
                    var captured = false;
                    var roaming = false;
                    var badRefs = 0;
                    var posture = Posture.Grounded;
                    var leafItem = false;
                    foreach (var expr in seq.Expressions)
                    {
                        var i = Analyze(expr, env);
                        // Each operand of a sequence expression is a separate operand: two
                        // references to the streamed value are two consuming uses (§19.8.1;
                        // count((author, editor)) is free-ranging).
                        consumes += i.Consumes + (i.Posture != Posture.Grounded && !i.Captured && i.Consumes == 0 ? 1 : 0);
                        motionless &= i.Motionless;
                        usesLast |= i.UsesLast;
                        usesPosition |= i.UsesPosition;
                        refs |= i.RefsStreamed;
                        fresh |= i.Fresh;
                        captured |= i.Captured;
                        roaming |= i.Roaming;
                        badRefs += i.BadRefs;
                        leafItem |= i.LeafItem;
                        posture = CombinePosture(posture, i.Posture);
                    }
                    return new Info(posture, consumes, motionless, usesLast, usesPosition, refs, fresh, captured, roaming, badRefs, leafItem);
                }

                case RangeExpressionNode r:
                {
                    var f = Analyze(r.From, env);
                    var t = Analyze(r.To, env);
                    return Combine(f, t) with { Posture = Posture.Grounded };
                }

                case BinaryExpressionNode b:
                    return AnalyzeBinary(b, env);

                case UnaryExpressionNode u:
                {
                    var operand = Analyze(u.Operand, env);
                    var extra = ExtraConsume(operand);
                    return operand with
                    {
                        Posture = Posture.Grounded,
                        Consumes = operand.Consumes + extra,
                        Motionless = operand.Motionless && extra == 0,
                    };
                }

                case IfExpressionNode i:
                {
                    var cond = Analyze(i.Condition, env);
                    var thenInfo = Analyze(i.ThenBranch, env);
                    var elseInfo = Analyze(i.ElseBranch, env);
                    // Only one branch evaluates: the sweep is the wider of the two, not the
                    // sum (su-shallow-descent-A's f:odd-children consumes the descent
                    // argument in both branches of an if).
                    var branches = Combine(thenInfo, elseInfo) with { Consumes = Math.Max(thenInfo.Consumes, elseInfo.Consumes) };
                    return new Info(branches.Posture, cond.Consumes + branches.Consumes,
                        cond.Motionless && branches.Motionless,
                        cond.UsesLast || branches.UsesLast,
                        cond.UsesPosition || branches.UsesPosition,
                        cond.RefsStreamed || branches.RefsStreamed,
                        Fresh: false,
                        Captured: (thenInfo.Captured || thenInfo.Posture == Posture.Grounded)
                            && (elseInfo.Captured || elseInfo.Posture == Posture.Grounded),
                        Roaming: cond.Roaming || branches.Roaming,
                        BadRefs: cond.BadRefs + branches.BadRefs);
                }

                case FunctionCallNode f:
                    return AnalyzeCall(f, env);

                case NamedFunctionRefNode nf:
                {
                    var def = string.IsNullOrEmpty(nf.Prefix)
                        ? null
                        : Functions().TryGetValue((nf.NamespaceUri ?? ResolvePrefix(nf.Prefix), nf.LocalName, nf.Arity), out var d)
                            ? d
                            : FindUserFunction(nf.Prefix, nf.LocalName, nf.Arity, CurrentContext);
                    if (def != null && def.Element.Attribute("streamability") == null)
                        return new Info(BadRefs: 1);
                    return Info.GroundedMotionless;
                }

                case LetExpressionNode let:
                {
                    var bodyEnv = env;
                    foreach (var binding in let.Bindings)
                        bodyEnv = bodyEnv.WithVar(binding.VariableName, BindingVar(Analyze(binding.Expression, bodyEnv), binding.DeclaredType));
                    return Analyze(let.Body, bodyEnv);
                }

                case ForExpressionNode forExpr:
                {
                    // §19.8.8.1: if the in-expression is not grounded then roaming/free-ranging;
                    // the return clause is a higher-order operand, so it must not consume.
                    var bodyEnv = env;
                    var consumes = 0;
                    foreach (var binding in forExpr.Bindings)
                    {
                        var b = Analyze(binding.Expression, bodyEnv);
                        if (b.Posture != Posture.Grounded)
                            throw Error("the in-expression of a for expression over a streamed sequence is not streamable.");
                        consumes += b.Consumes;
                        bodyEnv = bodyEnv.WithVar(binding.VariableName, BindingVar(b, binding.DeclaredType));
                    }
                    if (env.GroupInScope)
                    {
                        bodyEnv = bodyEnv.Spawn();
                        bodyEnv.GroupInScope = false;
                        bodyEnv.GroupOutside = true;
                    }
                    var ret = Analyze(forExpr.ReturnExpression, bodyEnv);
                    if (ret.Consumes > 0)
                        throw Error("the return clause of a for expression must not consume a streamed value.");
                    return new Info(Posture.Grounded, consumes, ret.Motionless,
                        UsesLast: ret.UsesLast, UsesPosition: ret.UsesPosition,
                        RefsStreamed: ret.RefsStreamed, Roaming: ret.Roaming, BadRefs: ret.BadRefs);
                }

                case QuantifiedExpressionNode q:
                {
                    // §19.8.8.2: same shape as for expressions.
                    var bodyEnv = env;
                    var consumes = 0;
                    var motionless = true;
                    foreach (var binding in q.Bindings)
                    {
                        var b = Analyze(binding.Expression, bodyEnv);
                        if (b.Posture != Posture.Grounded)
                            throw Error("the in-expression of a quantified expression over a streamed sequence is not streamable.");
                        consumes += b.Consumes;
                        motionless &= b.Motionless;
                        bodyEnv = bodyEnv.WithVar(binding.VariableName, BindingVar(b, binding.DeclaredType));
                    }
                    if (env.GroupInScope)
                    {
                        bodyEnv = bodyEnv.Spawn();
                        bodyEnv.GroupInScope = false;
                        bodyEnv.GroupOutside = true;
                    }
                    var sat = Analyze(q.SatisfiesExpression, bodyEnv);
                    if (sat.Consumes > 0)
                        throw Error("the satisfies clause of a quantified expression must not consume a streamed value.");
                    return new Info(Posture.Grounded, consumes + sat.Consumes, motionless && sat.Motionless,
                        UsesLast: sat.UsesLast, UsesPosition: sat.UsesPosition,
                        RefsStreamed: sat.RefsStreamed, Roaming: sat.Roaming, BadRefs: sat.BadRefs);
                }

                case InstanceOfNode io:
                {
                    var operand = Analyze(io.Expression, env);
                    return operand with { Posture = Posture.Grounded };
                }

                case CastableNode ca:
                {
                    var operand = Analyze(ca.Expression, env);
                    return operand with { Posture = Posture.Grounded };
                }

                case TreatNode t:
                {
                    var operand = Analyze(t.Expression, env);
                    return operand with { Posture = operand.Posture == Posture.Grounded ? Posture.Grounded : Posture.Crawling };
                }

                case CastNode cast:
                {
                    var operand = Analyze(cast.Expression, env);
                    var extra = ExtraConsume(operand);
                    return new Info(Posture.Grounded, operand.Consumes + extra,
                        Motionless: operand.Motionless && extra == 0,
                        UsesLast: operand.UsesLast, UsesPosition: operand.UsesPosition,
                        RefsStreamed: operand.RefsStreamed, Roaming: operand.Roaming, BadRefs: operand.BadRefs);
                }

                case MapConstructorNode m:
                {
                    // §19.8.8.17: equivalent to xsl:map whose children are all xsl:map-entry,
                    // so the implicit fork applies — each entry consumes its own fork of the
                    // input, and the consumption of the constructor is the MAX over entries,
                    // not the sum (sx-MapExpr-001: map{'a': string(X), 'b': string(Y)}).
                    var consumes = 0;
                    var motionless = true;
                    foreach (var entry in m.Entries)
                    {
                        var key = Analyze(entry.Key, env);
                        var value = Analyze(entry.Value, env);
                        consumes = Math.Max(consumes, key.Consumes + value.Consumes);
                        motionless &= key.Motionless && value.Motionless;
                        // Maps can hold only grounded items; an atomized value derived from
                        // streamed nodes (current-group()/@name/string()) is fine.
                        if (value.Posture != Posture.Grounded)
                            throw Error("a map constructor must not contain nodes from a streamed document.");
                    }
                    return new Info(Posture.Grounded, consumes, motionless);
                }

                case ArrayConstructorNode a:
                {
                    if (!a.IsSquare)
                    {
                        var inner = Analyze(a.Items[0], env);
                        return inner with { Posture = Posture.Grounded };
                    }
                    // Square array constructor: like the map constructor, members are
                    // evaluated with an implicit fork of the input, so consumption is the
                    // MAX over members (sx-square-array-101).
                    var consumes = 0;
                    var motionless = true;
                    var posture = Posture.Grounded;
                    foreach (var item in a.Items)
                    {
                        var i = Analyze(item, env);
                        consumes = Math.Max(consumes, i.Consumes);
                        motionless &= i.Motionless;
                        posture = CombinePosture(posture, i.Posture);
                    }
                    return new Info(posture, consumes, motionless);
                }

                case LookupNode l:
                {
                    var inner = Analyze(l.Expression, env);
                    var key = Analyze(l.Key, env);
                    return Combine(inner, key) with
                    {
                        Posture = inner.Posture,
                        Consumes = inner.Consumes + key.Consumes + (inner.Posture != Posture.Grounded && !inner.Captured ? 1 : 0),
                    };
                }

                case LookupWildcardNode lw:
                {
                    var inner = Analyze(lw.Expression, env);
                    return inner with
                    {
                        Consumes = inner.Consumes + (inner.Posture != Posture.Grounded && !inner.Captured ? 1 : 0),
                    };
                }

                case InlineFunctionNode:
                    return Info.GroundedMotionless;

                case ArrowExprNode arrow:
                {
                    var source = Analyze(arrow.Source, env);
                    var extra = ExtraConsume(source);
                    var target = Analyze(arrow.Target, env);
                    return new Info(Posture.Grounded, source.Consumes + target.Consumes + extra,
                        Motionless: source.Motionless && target.Motionless && extra == 0,
                        UsesLast: source.UsesLast || target.UsesLast,
                        UsesPosition: source.UsesPosition || target.UsesPosition,
                        RefsStreamed: source.RefsStreamed || target.RefsStreamed,
                        BadRefs: source.BadRefs + target.BadRefs);
                }

                case DynamicFunctionCallNode d:
                {
                    var fn = Analyze(d.Function, env);
                    var acc = fn;
                    foreach (var arg in d.Arguments)
                        acc = Combine(acc, Analyze(arg, env));
                    return acc with { Posture = Posture.Grounded };
                }

                case TryCatchNode t:
                {
                    var acc = Analyze(t.TryExpression, env);
                    foreach (var clause in t.Clauses)
                        acc = Combine(acc, Analyze(clause.Expression, env));
                    return acc;
                }

                default:
                    return Info.GroundedMotionless;
            }
        }

        private XElement CurrentContext => _root;

        private string ResolvePrefix(string prefix) => CurrentContext.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? "";

        private static Var BindingVar(Info info, FlworTypeDeclaration? declaredType) => new()
        {
            Posture = info.Posture,
            Streamed = info.Posture != Posture.Grounded,
            BooleanTyped = declaredType?.TypeName is "boolean" or "Boolean",
            Roaming = info.Roaming || (info.Posture is Posture.Striding or Posture.Crawling or Posture.Climbing && info.RefsStreamed && !info.Captured),
        };

        private static Posture CombinePosture(Posture a, Posture b)
        {
            if (a == b)
                return a;
            if (a == Posture.Grounded)
                return b;
            if (b == Posture.Grounded)
                return a;
            return Posture.Crawling;
        }

        private static Info Combine(Info a, Info b) => new(
            CombinePosture(a.Posture, b.Posture),
            a.Consumes + b.Consumes,
            a.Motionless && b.Motionless,
            a.UsesLast || b.UsesLast,
            a.UsesPosition || b.UsesPosition,
            a.RefsStreamed || b.RefsStreamed,
            a.Fresh || b.Fresh,
            a.Captured || b.Captured,
            a.Roaming || b.Roaming,
            a.BadRefs + b.BadRefs,
            a.LeafItem || b.LeafItem,
            a.StridingUnion || b.StridingUnion);

        /// <summary>Extra consumption when an expression is consumed/atomized by an enclosing operator.
        /// Climbing operands are motionless with respect to the streamed sequence (ancestor access is
        /// buffered), so they never add a consuming use.</summary>
        private static int ExtraConsume(Info arg)
            => arg.Posture is Posture.Striding or Posture.Crawling && !arg.Captured && arg.Consumes == 0 ? 1 : 0;

        // ---------------- paths and steps ----------------

        private Info AnalyzePath(PathExprNode path, Env env)
        {
            var input = new Info(env.ContextStreamed ? env.ContextPosture : Posture.Grounded, 0, Motionless: true,
                RefsStreamed: env.ContextStreamed, Fresh: env.ContextStreamed && env.ContextFresh);
            var steps = path.Steps;
            for (var i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                if (step is StepNode sn)
                    input = AnalyzeAxisStep(input, sn, path, env);
                else
                    input = AnalyzeGeneralStep(input, step, env);
                if (input.Posture == Posture.Roaming)
                {
                    // §19.8.8.8: reassess the path prefix ending at this step as a scanning
                    // expression (a motionless pattern); crawling if it selects elements.
                    var prefix = new PathExprNode(path.IsAbsolute, steps.Take(i + 1).ToList());
                    if (IsMotionlessPatternShape(prefix, env))
                        input = new Info(PathSelectsElements(prefix) ? Posture.Crawling : Posture.Striding,
                            input.Consumes, Motionless: false, RefsStreamed: input.RefsStreamed);
                    else
                        throw Error("downward navigation from a non-striding operand is not streamable.");
                }
            }
            return input;
        }

        /// <summary>General (non-axis) step: evaluated with the path input as context item.</summary>
        private Info AnalyzeGeneralStep(Info input, XPathAstNode step, Env env)
        {
            var stepEnv = env.WithContext(input.Posture, input.Posture != Posture.Grounded, input.Fresh);
            return Analyze(step, stepEnv);
        }

        private Info AnalyzeAxisStep(Info input, StepNode sn, PathExprNode path, Env env)
        {
            var posture = input.Posture;
            var consumes = 0;
            var motionless = true;
            var captured = false;
            var roaming = input.Roaming;

            if (posture == Posture.Roaming)
                return input with { Posture = Posture.Roaming, Fresh = false };
            // A striding-compatible union (§19.10) may be atomized but not navigated into:
            // the merged peer sequence cannot be re-entered downward (sx-union-202).
            if (input.StridingUnion && sn.Axis is XdmAxis.Child or XdmAxis.Descendant or XdmAxis.DescendantOrSelf)
                return input with { Posture = Posture.Roaming, Fresh = false, StridingUnion = false };
            if (posture == Posture.Climbing && sn.Axis is not (XdmAxis.Self or XdmAxis.Attribute or XdmAxis.Namespace
                or XdmAxis.Parent or XdmAxis.Ancestor or XdmAxis.AncestorOrSelf))
            {
                // §19.8.8.9 table: every other combination is roaming.
                return input with { Posture = Posture.Roaming, Fresh = false };
            }

            if (posture != Posture.Grounded)
            {
                switch (sn.Axis)
                {
                    case XdmAxis.Child:
                        if (posture is Posture.Crawling && IsLeafStep(sn))
                        {
                            // Leaf nodes (text/comment/PI) are buffered while the crawl
                            // advances, so they may be selected from a crawling operand
                            // without re-entering the stream (sx-except-022: //text()).
                            captured = true;
                            posture = Posture.Striding;
                            break;
                        }
                        if (posture is Posture.Crawling or Posture.Climbing)
                            return input with { Posture = Posture.Roaming, Fresh = false };
                        consumes += input.Fresh ? 1 : 0;
                        motionless = false;
                        posture = Posture.Striding;
                        break;

                    case XdmAxis.Descendant:
                    case XdmAxis.DescendantOrSelf:
                        if (posture is Posture.Crawling or Posture.Climbing)
                            return input with { Posture = Posture.Roaming, Fresh = false };
                        consumes += input.Fresh ? 1 : 0;
                        motionless = false;
                        // A numeric focus-independent predicate makes the result a singleton.
                        if (sn.Predicates.Count > 0 && AllPredicatesSingleton(sn.Predicates, env))
                        {
                            posture = Posture.Striding;
                        }
                        else
                        {
                            posture = NodeTestSelectsElements(sn.NodeTest, sn.Axis) ? Posture.Crawling : Posture.Striding;
                        }
                        break;

                    case XdmAxis.Attribute:
                    case XdmAxis.Namespace:
                        // From striding, climbing, or crawling: striding and motionless (§19.8.8.9 table).
                        captured = true;
                        posture = Posture.Striding;
                        break;

                    case XdmAxis.Self:
                        if (NodeTestSelectsElements(sn.NodeTest, sn.Axis))
                        {
                            // self::x from crawling stays crawling; from climbing stays climbing.
                        }
                        else if (posture == Posture.Crawling)
                        {
                            posture = Posture.Striding;
                        }
                        break;

                    case XdmAxis.Parent:
                    case XdmAxis.Ancestor:
                    case XdmAxis.AncestorOrSelf:
                        posture = Posture.Climbing;
                        break;

                    default:
                        // following/preceding axes: roaming.
                        return input with { Posture = Posture.Roaming, Fresh = false };
                }
            }

            var result = new Info(posture, input.Consumes + consumes, motionless && input.Motionless,
                UsesLast: input.UsesLast, UsesPosition: input.UsesPosition,
                RefsStreamed: input.RefsStreamed, Fresh: false, Captured: captured, Roaming: roaming,
                LeafItem: StepSelectsBufferedItems(sn));

            // Predicates on the step (F1): every predicate must be motionless, or positional
            // over a striding operand (§19.8.8.9 rule 5: a positional predicate on a striding
            // step filters each item as it passes — su-unclassified-001 ITEM[position() ne 42]).
            // A consuming predicate or any use of last() makes the step roaming and the
            // path-level scanning reassessment below decides whether it is still streamable.
            // A grounded variable predicate such as [$i] is motionless and allowed here
            // (§19.8.8.1: for $i in 1 to 3 return name(ancestor::x[$i]) is streamable).
            if (sn.Predicates.Count > 0 && result.Posture != Posture.Grounded)
            {
                var predEnv = env.WithContext(Posture.Striding, streamed: true);
                predEnv.LeafContext = IsLeafStep(sn);
                foreach (var pred in sn.Predicates)
                {
                    var expr = pred is PredicateNode pd ? pd.Expression : pred;
                    var info = Analyze(expr, predEnv);
                    if (info.Consumes == 0 && !info.UsesLast && info.Motionless)
                        continue;
                    if (result.Posture == Posture.Striding && info.Consumes == 0 && !info.UsesLast)
                        continue;
                    return result with { Posture = Posture.Roaming };
                }
            }
            return result;
        }

        /// <summary>All steps are axis steps using pattern-like axes with motionless non-positional
        /// predicates. As a processor extension permitted by §19.10, a focus-independent numeric
        /// literal predicate in a non-final step is tolerated (sf-insert-before-121:
        /// /BOOKLIST/BOOKS/ITEM[1]//text()); a positional predicate on the final step still
        /// disqualifies the path (§19.8.8.8: //section/head[1] is not streamable), as does any
        /// focus-dependent positional predicate such as [position() ne last()].</summary>
        private bool IsMotionlessPatternShape(PathExprNode path, Env env)
        {
            var lastIndex = path.Steps.Count - 1;
            for (var i = 0; i < path.Steps.Count; i++)
            {
                var step = path.Steps[i];
                // A leading "." (context item expression) is transparent: .//x scans like //x.
                if (step is ContextItemNode)
                    continue;
                if (step is not StepNode sn)
                    return false;
                if (sn.Axis is not (XdmAxis.Child or XdmAxis.Descendant or XdmAxis.DescendantOrSelf
                    or XdmAxis.Self or XdmAxis.Attribute))
                    return false;
                foreach (var pred in sn.Predicates)
                {
                    var expr = pred is PredicateNode pd ? pd.Expression : pred;
                    var predEnv = env.WithContext(Posture.Striding, streamed: true);
                    predEnv.LeafContext = IsLeafStep(sn);
                    var info = Analyze(expr, predEnv);
                    if (info.Consumes > 0 || !info.Motionless || info.UsesPosition || info.UsesLast)
                        return false;
                    if (IsNumericFocusIndependent(expr) && i < lastIndex)
                        continue;
                    if (IsNumericFocusIndependent(expr))
                        return false;
                }
            }
            return true;
        }

        /// <summary>Text, attribute, namespace, comment, and PI nodes expose their string value
        /// without advancing the stream, so predicates on such steps may atomize the context.</summary>
        private static bool IsLeafStep(StepNode sn)
        {
            if (sn.Axis is XdmAxis.Attribute or XdmAxis.Namespace)
                return true;
            return sn.NodeTest.Kind == NameTestKind.KindTest
                && sn.NodeTest.Name is "text" or "comment" or "processing-instruction";
        }

        private static bool PathSelectsElements(PathExprNode path)            => path.Steps.OfType<StepNode>().Any(s =>
                s.Axis is XdmAxis.Child or XdmAxis.Descendant or XdmAxis.DescendantOrSelf or XdmAxis.Self
                && NodeTestSelectsElements(s.NodeTest, s.Axis));

        private static bool NodeTestSelectsElements(NodeTest test, XdmAxis axis)
        {
            if (axis == XdmAxis.Attribute || axis == XdmAxis.Namespace)
                return false;
            return test.Kind switch
            {
                NameTestKind.AnyName or NameTestKind.NamespaceAny or NameTestKind.PrefixedName
                    or NameTestKind.LocalName or NameTestKind.QName => true,
                NameTestKind.KindTest => test.Name switch
                {
                    "node" or "element" or "document-node" or "schema-element" => true,
                    _ => false,
                },
                _ => false,
            };
        }

        /// <summary>True when the step selects items whose entire content is buffered when
        /// delivered (attributes, namespaces, text, comments, PIs): atomizing the delivered
        /// context item is then motionless (§19.8.10).</summary>
        private static bool StepSelectsBufferedItems(StepNode sn)
        {
            if (sn.Axis is XdmAxis.Attribute or XdmAxis.Namespace)
                return true;
            if (sn.NodeTest.Kind != NameTestKind.KindTest)
                return false;
            return sn.NodeTest.Name is "text" or "comment" or "processing-instruction";
        }

        /// <summary>True for a relative path of child-axis name-test steps without predicates:
        /// such paths select disjoint peer nodes, so a union of two of them is
        /// striding-compatible (§19.10) rather than crawling.</summary>
        private static bool IsSimpleChildNamePath(XPathAstNode node)
        {
            if (node is ParenthesizedExprNode p)
                node = p.Expression;
            if (node is not PathExprNode path || path.Steps.Count == 0)
                return false;
            foreach (var step in path.Steps)
            {
                if (step is not StepNode sn || sn.Axis != XdmAxis.Child || sn.Predicates.Count > 0)
                    return false;
                if (sn.NodeTest.Kind == NameTestKind.KindTest && sn.NodeTest.Name is not ("element" or "schema-element"))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Predicate-based singleton detection: descendant::x[1] is striding when the predicate
        /// is numeric and independent of the focus (§19.8.8.9 rule 4).
        /// </summary>
        private bool AllPredicatesSingleton(IReadOnlyList<XPathAstNode> predicates, Env env)
        {
            foreach (var pred in predicates)
            {
                var expr = pred is PredicateNode pd ? pd.Expression : pred;
                if (!IsNumericFocusIndependent(expr))
                    return false;
            }
            return predicates.Count > 0;
        }

        /// <summary>Numeric-typed predicate that does not depend on the focus.</summary>
        private bool IsNumericFocusIndependent(XPathAstNode node) => node switch
        {
            IntegerLiteralNode or DecimalLiteralNode or DoubleLiteralNode => true,
            RangeExpressionNode => true,
            UnaryExpressionNode u => IsNumericFocusIndependent(u.Operand),
            BinaryExpressionNode b when b.Operator is BinaryOperator.Plus or BinaryOperator.Minus
                or BinaryOperator.Multiply or BinaryOperator.Divide or BinaryOperator.Idiv or BinaryOperator.Mod
                or BinaryOperator.To => IsNumericFocusIndependent(b.Left) && IsNumericFocusIndependent(b.Right),
            VariableReferenceNode => true,
            FunctionCallNode f => f.Arguments.All(IsNumericFocusIndependent) && f.LocalName is "count" or "index-of" or "abs" or "floor" or "ceiling" or "round" or "max" or "min" or "sum" or "string-length" && !ContainsFocusAccess(f),
            ParenthesizedExprNode p => IsNumericFocusIndependent(p.Expression),
            _ => false,
        };

        private static bool ContainsFocusAccess(XPathAstNode node) => node switch
        {
            ContextItemNode => true,
            StepNode => true,
            FunctionCallNode f when f.LocalName is "position" or "last" or "current" or "current-group" => true,
            _ => false,
        };

        // ---------------- filter (postfix) predicates ----------------

        private Info AnalyzeFilter(Info baseInfo, XPathAstNode predicate, Env env, bool baseIsStreamedVar = false)
        {
            if (baseInfo.Posture == Posture.Grounded)
                return baseInfo;
            var expr = predicate is PredicateNode pd ? pd.Expression : predicate;
            // A lone variable predicate is positional unless the variable is typed xs:boolean.
            if (expr is VariableReferenceNode vref)
            {
                var v = env.Lookup(vref.LocalName);
                if (v != null && !v.BooleanTyped)
                    throw Error($"a predicate that is a lone variable reference ('{vref.LocalName}') must be typed as xs:boolean.");
                // A boolean-typed variable predicate is a filter predicate, not a positional
                // one: it is motionless and keeps the posture and sweep of the base
                // (su-filter-003/004: $input[$test] with $test as xs:boolean).
                if (v != null && v.BooleanTyped && !v.Streamed)
                    return baseInfo;
            }
            // A positional predicate on a bare streamed variable requires arbitrary access
            // to the buffered sequence (su-absorbing-905: deep-equal($element[1], $element[2])).
            // Positional predicates on path STEPS remain legal and are handled per-step.
            var predEnv = env.WithContext(Posture.Striding, streamed: true);
            var info = Analyze(expr, predEnv);
            if (baseIsStreamedVar
                && (IsNumericFocusIndependent(expr) || info.UsesPosition || info.UsesLast))
                throw Error($"a positional predicate on a streamed variable is not streamable ('{ExprText(expr)}').");

            if (baseInfo.Posture == Posture.Crawling)
            {
                // §19.8.8.10: (a) numeric focus-independent predicate → singleton → striding;
                // (b) motionless predicate (this includes position()-based predicates, since
                // position() is grounded and motionless) → posture and sweep of the base.
                if (IsNumericFocusIndependent(expr))
                    return baseInfo with { Posture = Posture.Striding };
                if (info.Motionless && !info.UsesLast)
                    return baseInfo;
                throw Error($"a predicate on a streamed sequence is not motionless ('{ExprText(expr)}').");
            }

            // Striding/climbing operand: predicate must be motionless (a positional predicate
            // such as [$i] requires arbitrary access to the streamed sequence).
            if (info.Consumes == 0 && info.Motionless && !info.UsesLast)
                return baseInfo;
            throw Error($"a predicate on a streamed sequence is not streamable ('{ExprText(expr)}').");
        }

        private static string ExprText(XPathAstNode node)
        {
            var text = _lastParsedText;
            if (text == null)
                return node.GetType().Name;
            var span = node.Span;
            return span.Length > 0 ? text.Substring(span.Start, Math.Min(span.Length, text.Length - span.Start)) : node.GetType().Name;
        }

        [ThreadStatic]
        private static string? _lastParsedText;

        // ---------------- binary operators ----------------

        private Info AnalyzeBinary(BinaryExpressionNode b, Env env)
        {
            switch (b.Operator)
            {
                case BinaryOperator.Union:
                {
                    var l = Analyze(b.Left, env);
                    var r = Analyze(b.Right, env);
                    // §19.8.8.4: union of two striding/crawling expressions is crawling with the
                    // WIDER sweep — consuming if either operand is consuming, not the sum.
                    var posture = l.Posture == Posture.Grounded ? r.Posture
                        : r.Posture == Posture.Grounded ? l.Posture
                        : Posture.Crawling;
                    // §19.10 processor extension (Saxon): a union of two simple child-name
                    // paths selects disjoint peers in document order, so it may be treated
                    // as striding (sx-union-302/r-015); descendant-based unions stay crawling.
                    // Such a union must not be navigated INTO (sx-union-202) — only atomized.
                    var stridingUnion = false;
                    if (posture == Posture.Crawling
                        && l.Posture == Posture.Striding && r.Posture == Posture.Striding
                        && IsSimpleChildNamePath(b.Left) && IsSimpleChildNamePath(b.Right))
                    {
                        posture = Posture.Striding;
                        stridingUnion = true;
                    }
                    return Combine(l, r) with { Posture = posture, Consumes = Math.Max(l.Consumes, r.Consumes), StridingUnion = stridingUnion };
                }

                case BinaryOperator.Intersect:
                case BinaryOperator.Except:
                {
                    var l = Analyze(b.Left, env);
                    var r = Analyze(b.Right, env);
                    var posture = l.Posture == Posture.Grounded ? r.Posture
                        : r.Posture == Posture.Grounded ? l.Posture
                        : l.Posture == r.Posture ? l.Posture : Posture.Crawling;
                    return Combine(l, r) with { Posture = posture, Consumes = Math.Max(l.Consumes, r.Consumes) };
                }

                case BinaryOperator.Is:
                case BinaryOperator.Precedes:
                case BinaryOperator.Follows:
                {
                    var l = Analyze(b.Left, env);
                    var r = Analyze(b.Right, env);
                    return Combine(l, r) with { Posture = Posture.Grounded, Consumes = l.Consumes + r.Consumes };
                }

                case BinaryOperator.InstanceOf:
                case BinaryOperator.CastableAs:
                {
                    var l = Analyze(b.Left, env);
                    var r = Analyze(b.Right, env);
                    return Combine(l, r) with { Posture = Posture.Grounded };
                }

                case BinaryOperator.TreatAs:
                {
                    var l = Analyze(b.Left, env);
                    var r = Analyze(b.Right, env);
                    var posture = l.Posture == Posture.Grounded ? Posture.Grounded : Posture.Crawling;
                    return Combine(l, r) with { Posture = posture };
                }

                case BinaryOperator.SimpleMap:
                {
                    // §19.8.8.7: the posture is the posture of the right-hand operand assessed
                    // with a context posture set from the left; the sweep is the wider of the
                    // two sweeps. The items delivered by the left operand are processed in the
                    // same pass, so consumption is the maximum, not the sum (sf-sum-022/050).
                    var left = Analyze(b.Left, env);
                    var streamed = left.Posture != Posture.Grounded;
                    var rightEnv = streamed
                        ? env.WithContext(Posture.Striding, streamed: true, fresh: false)
                        // A grounded left operand delivers grounded items: the right-hand
                        // context item is not the streamed node (si-fork-017/018 group-by).
                        : env.WithContext(Posture.Grounded, streamed: false);
                    // Buffered items (attributes, text) delivered by the left operand may be
                    // atomized or compared in the right operand without advancing the stream
                    // (sx-bang: account/transaction/@value ! (if (. > 0) then string(.))).
                    if (streamed && left.LeafItem)
                        rightEnv.LeafContext = true;
                    var right = Analyze(b.Right, rightEnv);
                    return new Info(right.Posture, Math.Max(left.Consumes, right.Consumes),
                        Motionless: left.Motionless && right.Motionless,
                        UsesLast: left.UsesLast || right.UsesLast,
                        UsesPosition: left.UsesPosition || right.UsesPosition,
                        RefsStreamed: left.RefsStreamed || right.RefsStreamed,
                        Fresh: false, Captured: right.Captured,
                        Roaming: left.Roaming || right.Roaming,
                        BadRefs: left.BadRefs + right.BadRefs);
                }

                default:
                {
                    // Arithmetic, value/general comparisons, boolean ops, string concat.
                    // Boolean operands need only their effective boolean value: a motionless
                    // operand (self::BOOKS) contributes no consuming use (si-try-009).
                    var l = Analyze(b.Left, env);
                    var r = Analyze(b.Right, env);
                    var booleanOp = b.Operator is BinaryOperator.Or or BinaryOperator.And;
                    int OperandConsume(Info i) => i.Consumes + (booleanOp
                        ? (i.Consumes == 0 && !i.Motionless && i.Posture != Posture.Grounded && !i.Captured ? 1 : 0)
                        : ExtraConsume(i));
                    var consumes = OperandConsume(l) + OperandConsume(r);
                    var motionless = l.Motionless && r.Motionless && consumes == l.Consumes + r.Consumes;
                    return new Info(Posture.Grounded, consumes, motionless,
                        UsesLast: l.UsesLast || r.UsesLast,
                        UsesPosition: l.UsesPosition || r.UsesPosition,
                        RefsStreamed: l.RefsStreamed || r.RefsStreamed,
                        Roaming: l.Roaming || r.Roaming,
                        BadRefs: l.BadRefs + r.BadRefs);
                }
            }
        }

        // ---------------- function calls ----------------

        private static readonly HashSet<string> Atomizers = new(StringComparer.Ordinal)
        {
            "string", "data", "number", "string-length", "normalize-space", "sum", "avg", "min", "max",
            "string-join", "concat", "tokenize", "contains", "starts-with", "ends-with", "substring",
            "substring-before", "substring-after", "translate", "upper-case", "lower-case", "replace",
            "matches", "compare", "abs", "floor", "ceiling", "round", "round-half-to-even", "format-number",
        };

        private static readonly HashSet<string> ClimbingProducers = new(StringComparer.Ordinal)
        {
            "reverse", "innermost", "outermost",
        };

        private Info AnalyzeCall(FunctionCallNode f, Env env)
        {
            var local = f.LocalName;
            if (local == "position" && f.Arguments.Count == 0)
                return new Info(UsesPosition: true);
            if (local == "last" && f.Arguments.Count == 0)
                return env.ContextStreamed
                    ? new Info(UsesLast: true, Motionless: false)
                    : new Info(UsesLast: true);
            if (local == "current" && f.Arguments.Count == 0)
                // current() is the node matched by the template rule. Inside a pattern
                // predicate on a leaf step it is a buffered text/attribute node whose
                // atomization is free (stream-200: part-name/text()[$selected-parts =
                // current()]); on an element step it must be treated like the context
                // item, since atomizing it reads the descendants (stream-204 expects
                // XTSE3430 for part-name[$selected-parts = current()]).
                return new Info(env.CurrentStreamed ? Posture.Striding : Posture.Grounded, 0, Motionless: true,
                    RefsStreamed: env.CurrentStreamed, Fresh: env.CurrentStreamed,
                    Captured: env.InPattern && env.LeafContext);
            if (local == "current-group" && f.Arguments.Count == 0)
            {
                if (!env.GroupInScope && env.GroupOutside)
                    throw Error("current-group() cannot be consumed inside a nested streamable document.");
                if (!env.GroupInScope && env.ContextStreamed)
                    // No group is lexically available (a called template never sees the
                    // caller's group, XSLT 3.0 §14.4). Over a streamed context the group
                    // members would be ungrounded streamed nodes, so current-group() is a
                    // consuming reference that makes the construct non-streamable
                    // (si-fork-116 → XTSE3430). In grounded contexts it stays a dynamic
                    // XTDE1061. current-grouping-key() remains motionless (si-fork-115).
                    throw Error("current-group() is not available in this streamable template.");
                return env.GroupInScope
                    ? new Info(env.GroupSelectPosture ?? Posture.Striding, 0, Motionless: true, RefsStreamed: true, Fresh: true)
                    : Info.GroundedMotionless;
            }
            if (local == "current-grouping-key" && f.Arguments.Count == 0)
                return Info.GroundedMotionless;

            // Inspection functions only look at the node's identity, not its content, so a
            // streamed operand is not absorbed (sf-current-100/901; su-inspection family).
            // The argument is still analyzed: invalid filters on a streamed operand (such as
            // a lone non-boolean variable predicate, su-absorbing-906) and the argument's
            // own consumption (two separate paths inside one instruction) must surface.
            if (f.Arguments.Count == 1 && local is "namespace-uri" or "local-name" or "name" or "generate-id" or "node-name")
            {
                var arg = Analyze(f.Arguments[0], env);
                return new Info(Posture.Grounded, arg.Consumes, arg.Motionless,
                    UsesLast: arg.UsesLast, UsesPosition: arg.UsesPosition,
                    RefsStreamed: arg.RefsStreamed, Roaming: arg.Roaming, BadRefs: arg.BadRefs);
            }

            // User-defined stylesheet function?
            var userDef = FindUserFunction(f.Prefix, local, f.Arguments.Count, CurrentContext);
            if (userDef != null)
                return AnalyzeUserFunctionCall(f, userDef, env);

            // Higher-order functions accepting function references.
            if (local is "filter" or "fold-left" or "fold-right" or "for-each" or "for-each-pair")
            {
                var infos = f.Arguments.Select(a => Analyze(a, env)).ToList();
                if (infos.Any(i => i.BadRefs > 0) && infos.Any(i => i.Posture != Posture.Grounded))
                    throw Error($"a non-streamable stylesheet function is passed to {local}() with a streamed operand.");
                var acc = infos.Aggregate(Info.GroundedMotionless, Combine);
                return acc with { Posture = Posture.Grounded };
            }

            if (ClimbingProducers.Contains(local))
            {
                var arg = f.Arguments.Count > 0 ? Analyze(f.Arguments[0], env) : Info.GroundedMotionless;
                if (local == "outermost")
                {
                    // §19.8.9.15: crawling argument yields a striding result; otherwise the
                    // general rules apply (transmission usage keeps the operand posture).
                    if (arg.Posture == Posture.Crawling)
                        return arg with { Posture = Posture.Striding, Fresh = false, Motionless = false };
                    return arg with { Fresh = false };
                }
                // reverse (§19.8.9.17) and innermost (§19.8.9.13) have navigation usage:
                // the operand must be grounded or striding.
                if (arg.Posture is Posture.Crawling or Posture.Climbing)
                    throw Error($"the operand of {local}() is not streamable.");
                return arg with { Fresh = false };
            }

            var args = f.Arguments.Select(a => Analyze(a, env)).ToList();
            var combined = args.Aggregate(Info.GroundedMotionless, Combine);

            if (local == "path")
            {
                if (args.Count == 0 && env.ContextStreamed)
                    throw Error("fn:path() is not streamable over a streamed node.");
                if (args.Count > 0 && args[0].Posture != Posture.Grounded && !args[0].Captured)
                    throw Error("fn:path() is not streamable over a streamed node.");
                return combined with { Posture = Posture.Grounded };
            }

            if (local == "copy-of" || local == "snapshot")
                return combined with
                {
                    Posture = Posture.Grounded,
                    Consumes = combined.Consumes + ExtraConsume(combined),
                    Motionless = false,
                    Fresh = false,
                    Captured = false,
                };

            if (local == "head" || local == "tail" || local == "zero-or-one" || local == "subsequence" || local == "first" || local == "exactly-one")
            {
                // §19.8.8.5: a cardinality-zero-or-one filter maps a crawling operand to
                // striding (head(//PRICE) is streamable — sf-sum-044); a climbing operand
                // keeps its posture — climbing is streamable until navigated downward.
                var first = args.Count > 0
                    ? args[0]
                    : env.ContextStreamed
                        ? new Info(env.ContextPosture, 0, Motionless: true, RefsStreamed: true, Fresh: env.ContextStreamed && env.ContextFresh)
                        : Info.GroundedMotionless;
                var posture = first.Posture == Posture.Crawling ? Posture.Striding : first.Posture;
                return combined with
                {
                    Posture = posture,
                    Consumes = combined.Consumes + ExtraConsume(first),
                    Motionless = false,
                    Fresh = false,
                    Captured = false,
                };
            }

            if (local == "count" || Atomizers.Contains(local) || (f.Prefix is "xs" or "xsd" && args.Count > 0))
            {
                // Atomizing/consuming functions: the first streamed operand is consumed once.
                // With no explicit argument the context item is the operand.
                if (env.ShallowDescentParam != null && args.Count > 0 && ReferencesVar(f.Arguments[0], env.ShallowDescentParam))
                    throw Error("the descent argument of a shallow-descent function must be navigated with striding steps, not atomized.");
                var consumes = combined.Consumes;
                // A no-argument atomizer on a buffered leaf context (text()/attribute()
                // in a pattern predicate or after !) atomizes without advancing the stream.
                var firstArg = args.Count > 0
                    ? args[0]
                    : env.LeafContext
                        ? Info.GroundedMotionless
                        : env.ContextStreamed
                            ? new Info(env.ContextPosture, 0, Motionless: true,
                                RefsStreamed: true, Fresh: env.ContextStreamed && env.ContextFresh)
                            : Info.GroundedMotionless;
                consumes += ExtraConsume(firstArg);
                // A shallow-descent parameter may only be consumed by striding child steps.
                if (env.ShallowDescentParam != null)
                {
                    foreach (var a in f.Arguments)
                        if (ReferencesVar(a, env.ShallowDescentParam))
                            throw Error("the descent argument of a shallow-descent function may only be used in striding steps.");
                }
                return combined with
                {
                    Posture = Posture.Grounded,
                    Consumes = consumes,
                    Motionless = combined.Motionless && consumes == 0,
                    Fresh = false,
                    Captured = false,
                };
            }

            // Unknown/system functions: result is grounded; operands contribute their consumption.
            return combined with { Posture = Posture.Grounded, Fresh = false, Captured = false };
        }

        private Info AnalyzeUserFunctionCall(FunctionCallNode f, XsltFunctionDefinition def, Env env)
        {
            var streamability = def.Element.Attribute("streamability")?.Value?.Trim().ToLowerInvariant();
            var infos = f.Arguments.Select(a => Analyze(a, env)).ToList();
            var combined = infos.Aggregate(Info.GroundedMotionless, Combine);

            var paramTypes = def.Element
                .Elements(XName.Get("param", Stylesheet.XslNamespace))
                .Select(p => p.Attribute("as")?.Value?.Trim())
                .ToList();

            // §19.8.5.1 shared by undeclared and unclassified functions: an argument whose
            // declared parameter type is atomic is atomized — a consuming use that is allowed
            // over a streamed node. Undeclared functions reject a streamed node bound to any
            // other parameter outright (F4/F15); unclassified (declared) functions reject it
            // with the declared-function wording.
            Info AnalyzeAtomizingArgs(Func<int, string> nonAtomicError)
            {
                var consumes = combined.Consumes;
                for (var idx = 0; idx < infos.Count; idx++)
                {
                    var i = infos[idx];
                    if (i.Posture == Posture.Grounded || i.Captured)
                        continue;
                    var asType = idx < paramTypes.Count ? paramTypes[idx] : null;
                    var atomizing = asType != null
                        && (asType.StartsWith("xs:", StringComparison.Ordinal) || asType.StartsWith("xsd:", StringComparison.Ordinal));
                    if (!atomizing)
                        throw Error(nonAtomicError(idx));
                    consumes += ExtraConsume(i);
                }
                return combined with { Posture = Posture.Grounded, Consumes = consumes };
            }

            if (streamability == null)
            {
                // A call to an unclassified stylesheet function with a streamed argument is
                // not streamable (F4/F15) — unless the function signature atomizes the
                // argument (an atomic-typed parameter): §19.8.5.1 permits streamed nodes in
                // arguments "unless the function signature causes such nodes to be atomized"
                // (j:escape(.) with <xsl:param name="in" as="xs:string"/> in xml-to-json).
                return AnalyzeAtomizingArgs(_ => $"stylesheet function '{def.LocalName}' is not declared streamable and cannot be called with a streamed node.");
            }

            if (streamability == "unclassified")
            {
                // The same atomization rule applies in every argument position: a streamed
                // node may be supplied where the signature atomizes it (su-unclassified-006
                // passes a striding path as argument 2 of an xs:decimal* parameter).
                return AnalyzeAtomizingArgs(idx => $"a streamed node cannot be passed in argument {idx + 1} of stylesheet function '{def.LocalName}'.")
                    with { Motionless = false, Fresh = false, Captured = false };
            }

            var first = infos.Count > 0 ? infos[0] : Info.GroundedMotionless;
            // A grounded first argument makes the whole call in-memory: the remaining
            // arguments are not streaming operands (su-shallow-descent-101 passes
            // outermost(//TITLE) as the second argument over a grounded node).
            if (first.Posture == Posture.Grounded)
            {
                foreach (var i in infos)
                    if (i.Posture == Posture.Roaming)
                        throw Error($"the argument of stylesheet function '{def.LocalName}' is not streamable.");
                return combined with { Posture = Posture.Grounded, Motionless = false, Fresh = false, Captured = false };
            }
            // Declared-streamable functions: arguments must be grounded or striding
            // (climbing is motionless and allowed — recursive ascent passes
            // $input/ancestor::x[1]). A shallow-descent call with a non-grounded
            // supplementary argument is roaming and free-ranging rather than an immediate
            // error (§19.8.5.5; su-shallow-descent-907 is caught when the roaming result
            // is delivered into a constructor).
            var shallowDescentRoaming = false;
            for (var idx = 0; idx < infos.Count; idx++)
            {
                var i = infos[idx];
                if (i.Posture is Posture.Crawling or Posture.Roaming)
                    throw Error($"the argument of stylesheet function '{def.LocalName}' is not streamable.");
                if (idx > 0 && i.Posture != Posture.Grounded && !i.Captured)
                {
                    if (streamability == "shallow-descent")
                        shallowDescentRoaming = true;
                    else
                        throw Error($"a streamed node cannot be passed in argument {idx + 1} of stylesheet function '{def.LocalName}'.");
                }
            }
            // §19.8.5.5: if P0 (first-argument posture) is not striding or grounded, the
            // call is roaming and free-ranging; usage checks at the delivery site decide
            // (su-shallow-descent-905 delivers the result bare inside xsl:for-each, while
            // recursive shallow-descent functions pass climbing ancestors into conditionals).
            if (streamability == "shallow-descent" && first.Posture == Posture.Climbing)
                return combined with
                {
                    Posture = Posture.Roaming,
                    Roaming = true,
                    Consumes = combined.Consumes + ExtraConsume(first),
                    Motionless = false,
                    Fresh = false,
                    Captured = false,
                };
            if (streamability == "shallow-descent" && first.Posture is not (Posture.Striding or Posture.Grounded))
                throw Error($"the descent argument of stylesheet function '{def.LocalName}' must be striding.");
            // Result posture by streamability kind (§19.8.8.8): absorbing and inspection
            // deliver grounded results; ascent delivers a climbing node; filter and
            // shallow-descent transmit the first argument's posture. A non-grounded
            // supplementary argument of a shallow-descent call makes the whole call
            // roaming and free-ranging (§19.8.5.5).
            var resultPosture = shallowDescentRoaming ? Posture.Roaming
                : streamability switch
                {
                    "ascent" => Posture.Climbing,
                    "filter" or "shallow-descent" => first.Posture,
                    _ => Posture.Grounded,
                };
            return combined with
            {
                Posture = resultPosture,
                Consumes = combined.Consumes + ExtraConsume(first),
                Motionless = false,
                Fresh = false,
                Captured = false,
            };
        }

        private static bool ReferencesVar(XPathAstNode node, string name) => CountVarRefs(node, name) > 0;

        // Inspection functions (has-children, exists, namespace-uri, local-name, ...) look
        // at the already-absorbed in-memory argument and do not consume it. Every other use
        // — navigation, atomization, head/tail — consumes.
        private static bool IsInspectionCall(FunctionCallNode f)
            => f.Prefix is "" or "fn"
               && f.LocalName is "exists" or "has-children" or "not" or "boolean" or "true" or "false"
                   or "namespace-uri" or "local-name";

        private static int CountConsumingRefs(XPathAstNode node, string name) => node switch
        {
            FunctionCallNode f when IsInspectionCall(f) => 0,
            VariableReferenceNode v => v.LocalName == name ? 1 : 0,
            StepNode s => s.Predicates.Sum(p => CountConsumingRefs(p, name)),
            PathExprNode p => p.Steps.Sum(s => CountConsumingRefs(s, name)),
            PostfixPredicateNode pp => CountConsumingRefs(pp.Expression, name) + CountConsumingRefs(pp.Predicate, name),
            PredicateNode pd => CountConsumingRefs(pd.Expression, name),
            FunctionCallNode f => f.Arguments.Sum(a => CountConsumingRefs(a, name)),
            NamedFunctionRefNode => 0,
            SequenceExpressionNode s => s.Expressions.Sum(e => CountConsumingRefs(e, name)),
            ParenthesizedExprNode p => CountConsumingRefs(p.Expression, name),
            IfExpressionNode i => CountConsumingRefs(i.Condition, name) + CountConsumingRefs(i.ThenBranch, name) + CountConsumingRefs(i.ElseBranch, name),
            BinaryExpressionNode b => CountConsumingRefs(b.Left, name) + CountConsumingRefs(b.Right, name),
            UnaryExpressionNode u => CountConsumingRefs(u.Operand, name),
            RangeExpressionNode r => CountConsumingRefs(r.From, name) + CountConsumingRefs(r.To, name),
            LetExpressionNode l => l.Bindings.Sum(b => CountConsumingRefs(b.Expression, name)) + CountConsumingRefs(l.Body, name),
            ForExpressionNode f => f.Bindings.Sum(b => CountConsumingRefs(b.Expression, name)) + CountConsumingRefs(f.ReturnExpression, name),
            QuantifiedExpressionNode q => q.Bindings.Sum(b => CountConsumingRefs(b.Expression, name)) + CountConsumingRefs(q.SatisfiesExpression, name),
            MapConstructorNode m => m.Entries.Sum(e => CountConsumingRefs(e.Key, name) + CountConsumingRefs(e.Value, name)),
            ArrayConstructorNode a => a.Items.Sum(i => CountConsumingRefs(i, name)),
            LookupNode l => CountConsumingRefs(l.Expression, name) + CountConsumingRefs(l.Key, name),
            LookupWildcardNode l => CountConsumingRefs(l.Expression, name),
            InlineFunctionNode => 0,
            ArrowExprNode a => CountConsumingRefs(a.Source, name),
            DynamicFunctionCallNode d => CountConsumingRefs(d.Function, name) + d.Arguments.Sum(a => CountConsumingRefs(a, name)),
            TryCatchNode t => CountConsumingRefs(t.TryExpression, name),
            _ => 0,
        };
    }
}

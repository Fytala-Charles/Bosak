// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 mei 2026
// PURPOSE              : A single template rule with its match pattern, priority, mode, and compiled body.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 24-05-2026     | Added ImportPrecedence for xsl:import priority resolution                              |
//                      | Charles Korthout | 0.3   | 24-05-2026     | Added multi-mode support (Modes array, #all, #current, #default)                       |
//                      | Charles Korthout | 0.4   | 05-06-2026     | Strip outer parens in priority computation; added FindMatchingParen helper             |
//                      | Charles Korthout | 0.5   | 07-06-2026     | StripXPathComments in ComputeDefaultPriority; fixes comment-stripped PredicatePattern   |
//                      | Charles Korthout | 0.6   | 07-06-2026     | ValidateUnionPattern before split; restores XTSE0340 for union patterns                |
//                      | Charles Korthout | 0.7   | 08-06-2026     | ParseModes expands QNames to Clark notation; fixes mode-0901 QName comparison          |
//                      | Charles Korthout | 0.8   | 09-06-2026     | Trim default-mode attribute; use ModeDefinition.NormalizeModeName for empty URI       |
//                      | Charles Korthout | 0.9   | 24-06-2026     | GetXPathDefaultNamespace no longer falls back to xmlns declaration                     |
//                      | Charles Korthout | 1.0   | 25-06-2026     | Trim xsl:template/@name values to normalize whitespace/EQName forms                    |
//                      | Charles Korthout | 1.1   | 26-06-2026     | Default priority for match="/" is -0.5 per XSLT 2.0/3.0 spec                            |
//                      | Charles Korthout | 1.2   | 03-07-2026     | ImportPrecedence now reads from Stylesheet for dynamic precedence assignment            |
//                      | Charles Korthout | 1.3   | 26-06-2026     | Added xsl:context-item parsing and static validation                                     |
//                      | Charles Korthout | 1.4   | 05-07-2026     | Reject AVT syntax in xsl:template/@match with XTSE0340                                 |
//                      | Charles Korthout | 1.5   | 08-07-2026     | Allow Q{uri} EQName braces in xsl:template/@match AVT check                            |
//                      | Charles Korthout | 1.6   | 14-07-2026     | namespace-node() default priority -0.5 (kind test)                                     |
//                      | Charles Korthout | 1.7    | 14-07-2026     | Template @visibility exposed for package entry-point checks (XTDE0040)                  |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.8   | 30-08-2026     | Added EffectiveVisibility for xsl:accept override propagation                          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.9   | 31-08-2026     | Made ParseModes internal for mode-aware visibility lookup                              |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.0   | 31-08-2026     | Added AcceptedBy property for used-package private template visibility tracking        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.1   | 02-09-2026     | Added OverriddenTemplate link for xsl:original dispatch; xsl:override/@default-mode    |
//                      |                  |       |                | applies to template rules declared inside xsl:override (override-m-010)                |
//                      | Charles Korthout | 2.2   | 21-09-2026     | API freeze stage B: internalized                                                       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.3   | 24-09-2026     | REQ-105 (PA-3): default priority 0.25 for the typed kind-test pattern forms           |
//                      |                  |       |                | element(*,T)/element(N,T)/attribute(*,T)/attribute(N,T), incl. after an axis          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.4   | 29-09-2026     | Typed-mode pattern variants: strict/lax compilations where top-level QName branches  |
//                      |                  |       |                | mean schema-element(QName); SchemaRewriteableQNames + HasSchemaDependentPattern       |
//                      |                  |       |                | (XTTE3110) wiring; match-218/219/220/221/222/243/244                                  |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.5   | 11-10-2026     | Carry effective XPath profiles into pattern predicate compilation.                      |
//                      | Charles Korthout | 2.6   | 13-10-2026     | Resolve standalone type patterns in their declaring scope and compute type priorities.   |
//                      | Charles Korthout | 2.7   | 13-10-2026     | Apply profile-specific XPath 4.0 kind-test wildcard/type priorities.
//                      | Charles Korthout | 2.8   | 13-10-2026     | Keep 4.0 node union patterns as one rule with maximum alternative priority.
//                      | Charles Korthout | 2.9   | 13-10-2026     | Normalize 4.0 attribute shorthand and forward-axis default priorities.
//                      | Charles Korthout | 2.10  | 13-10-2026     | Apply first-operand set priorities and 4.0 predicate-pattern priorities.
//                      | Charles Korthout | 2.11  | 13-10-2026     | Preserve parenthesized item-pattern scopes and expanded type priorities.
//                      | Charles Korthout | 2.12  | 13-10-2026     | Use lexical delimiter matching for pattern normalization and priority.
//                      | Charles Korthout | 2.13  | 13-10-2026     | Share lexical union splitting for earlier-profile rule expansion and type choices.
//                      | Charles Korthout | 2.14  | 13-10-2026     | Resolve 4.0 pattern prefixes using applicable static namespace bindings                |
//                      | Charles Korthout | 2.15  | 13-10-2026     | Resolve XSLT 4.0 template mode membership using applicable namespaces.                   |
//                      | Charles Korthout | 2.16  | 13-10-2026     | Preserve module fixed bindings in locally earlier template match patterns.                 |
//                      | Charles Korthout | 2.17  | 14-10-2026     | Retain module fixed mode identities independently of local earlier grammar.                |
//                      | Charles Korthout | 2.18  | 16-10-2026     | Admit XSLT 4.0 map-pattern braces and compute string-key map default priorities.          |
//                      | Charles Korthout | 2.19  | 16-10-2026     | Compute atomic pattern priorities using resolved types and minimum alternatives.          |
//                      | Charles Korthout | 2.20  | 16-10-2026     | Compute named record pattern priorities independently of record shape.                   |
//                      | Charles Korthout | 2.21  | 16-10-2026     | Compute array priorities recursively with lexical predicate detection.                    |
//                      | Charles Korthout | 2.22  | 22-10-2026     | Use minimum alternative priority for 4.0 node unions; preserve 3.0 dispatch.               |
// ===========================================================================================================================================================

using Bosak.XPath.Api;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.Xslt.Stylesheet;

/// <summary>
/// Represents a single xsl:template rule within a stylesheet.
/// </summary>
internal sealed class TemplateRule
{
    /// <summary>The original XElement of the xsl:template.</summary>
    public XElement Element { get; }

    /// <summary>The match pattern string (e.g. "foo[bar]"), or null for named templates.</summary>
    public string? Match { get; internal set; }

    /// <summary>The template name, or null for match-only templates.</summary>
    public string? Name { get; }

    /// <summary>The modes this template participates in (default is empty string for the default mode).</summary>
    public IReadOnlyList<string> Modes { get; }

    /// <summary>True if this template matches all modes.</summary>
    public bool MatchesAllModes { get; }

    /// <summary>The explicit or computed priority.</summary>
    public double Priority { get; private set; }

    /// <summary>The import precedence (0 = main stylesheet, higher = deeper import).</summary>
    public int ImportPrecedence => Stylesheet.ImportPrecedence;

    /// <summary>The compiled match predicate, or null for named-only templates.</summary>
    public Patterns.PatternPredicate? CompiledMatch { get; private set; }

    /// <summary>
    /// The compiled match predicate for a strict typed mode: top-level QName branches are
    /// interpreted as <c>schema-element(QName)</c>. Null when the rule has no rewriteable
    /// QName branch or no schema set was in scope at compile time.
    /// </summary>
    public Patterns.PatternPredicate? CompiledMatchStrict { get; private set; }

    /// <summary>
    /// The compiled match predicate for a lax typed mode: like the strict variant, except
    /// that a QName with no element declaration in the schema set falls back to plain name
    /// matching. Null under the same conditions as <see cref="CompiledMatchStrict"/>.
    /// </summary>
    public Patterns.PatternPredicate? CompiledMatchLax { get; private set; }

    /// <summary>
    /// The expanded names of the plain-QName steps that a strict/lax typed mode would
    /// interpret as <c>schema-element(QName)</c> (empty when none). Computed at compile
    /// time for the XTSE3105 static check.
    /// </summary>
    public IReadOnlyList<(string NamespaceUri, string LocalName)> SchemaRewriteableQNames { get; private set; }
        = Array.Empty<(string NamespaceUri, string LocalName)>();

    /// <summary>
    /// Whether the match pattern is schema-dependent: it contains <c>schema-element()</c>,
    /// <c>schema-attribute()</c>, or a kind test with a type argument (<c>element(N, T)</c> /
    /// <c>attribute(N, T)</c>). In a mode with <c>typed="no"</c> such a rule matching a
    /// schema-annotated node raises XTTE3110.
    /// </summary>
    public bool HasSchemaDependentPattern { get; private set; }

    /// <summary>The parent stylesheet.</summary>
    public Stylesheet Stylesheet { get; }

    /// <summary>The optional xsl:context-item declaration for this template.</summary>
    public ContextItemDeclaration? ContextItem { get; }

    /// <summary>
    /// The value of the template's <c>@visibility</c> attribute (public, private, final,
    /// or abstract), or null when absent. In an xsl:package the default visibility of a
    /// named template is private; xsl:initial-template is implicitly public.
    /// </summary>
    public string? Visibility => Element.Attribute("visibility")?.Value?.Trim()?.ToLowerInvariant();

    /// <summary>
    /// The effective visibility after <c>xsl:accept</c> rules from any <c>xsl:use-package</c>
    /// have been applied. Used by the runtime visibility filter.
    /// </summary>
    public string? EffectiveVisibility { get; internal set; }

    /// <summary>
    /// For a template declared inside <c>xsl:override</c>, the used-package declaration it
    /// replaces. <c>xsl:call-template name="xsl:original"</c> inside the overriding template
    /// dispatches to this rule while it executes (XSLT 3.0 §3.5.7.2).
    /// </summary>
    public TemplateRule? OverriddenTemplate { get; internal set; }

    /// <summary>
    /// The package whose <c>xsl:accept</c> rules set <see cref="EffectiveVisibility"/> to
    /// <c>private</c>, or <c>null</c> when the private visibility comes from the declaring
    /// package itself.
    /// </summary>
    public Stylesheet? AcceptedBy { get; internal set; }

    private TemplateRule(XElement element, string? match, string? name, IReadOnlyList<string> modes, double priority, Stylesheet stylesheet, ContextItemDeclaration? contextItem = null)
    {
        Element = element;
        Match = match;
        Name = name;
        Modes = modes;
        MatchesAllModes = modes.Contains("#all");
        Priority = priority;
        Stylesheet = stylesheet;
        ContextItem = contextItem;
    }

    /// <summary>
    /// Creates one or more <see cref="TemplateRule"/> instances from an xsl:template element.
    /// Earlier-profile union patterns are split into separate rules for <c>xsl:next-match</c>.
    /// Effective 4.0 node unions remain one rule with the minimum alternative default priority.
    /// </summary>
    /// <param name="element">The declaring template element.</param>
    /// <param name="stylesheet">The owning stylesheet and its effective static context.</param>
    /// <returns>The compiled template rules for the effective profile.</returns>
    public static IReadOnlyList<TemplateRule> FromElement(XElement element, Stylesheet stylesheet)
    {
        var contextItem = ContextItemDeclaration.FromTemplate(element, stylesheet);
        ValidateContextItemAgainstMatch(element, contextItem, stylesheet.GetXPathCompatibility(element));

        var match = element.Attribute("match")?.Value;
        if (!string.IsNullOrEmpty(match) && stylesheet.GetXPathCompatibility(element) < XPathCompatibility.XPath40 && ContainsAvtExpression(match))
            throw new InvalidOperationException("XTSE0340: The match attribute of xsl:template must not contain an attribute value template");
        if (string.IsNullOrEmpty(match))
            match = element.Attribute("_match")?.Value;
        var name = element.Attribute("name")?.Value?.Trim();
        var modeAttr = element.Attribute("mode")?.Value;
        // For declarations inside xsl:override, the default mode comes from the
        // xsl:override/@default-mode attribute before the stylesheet's (override-m-010).
        var overrideDefaultMode = element.Parent is { } overrideParent
            && overrideParent.Name.LocalName == "override"
            && overrideParent.Name.NamespaceName == Stylesheet.XslNamespace
            ? overrideParent.Attribute("default-mode")?.Value?.Trim()
            : null;
        var effectiveDefaultMode = string.IsNullOrEmpty(overrideDefaultMode)
            ? stylesheet.DefaultMode
            : ExpandModeName(overrideDefaultMode, element);
        var modes = ParseModes(modeAttr, element, effectiveDefaultMode);

        if (string.IsNullOrEmpty(match) && string.IsNullOrEmpty(name))
            return Array.Empty<TemplateRule>(); // Invalid template

        var priorityAttr = element.Attribute("priority");
        double explicitPriority = 0.0;
        bool hasExplicitPriority = priorityAttr != null && double.TryParse(priorityAttr.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out explicitPriority);

        if (string.IsNullOrEmpty(match))
        {
            // Named template only
            return new[] { new TemplateRule(element, match, name, modes, 0.5, stylesheet, contextItem) };
        }

        if (stylesheet.GetXPathCompatibility(element) >= XPathCompatibility.XPath40)
            _ = SplitNodeUnion40(StripXPathComments(match).Trim());

        // When an explicit priority is given, the entire union is a single template rule.
        // Splitting only happens when priorities are computed per-branch, so that
        // xsl:next-match can continue to other branches with different priorities.
        if (hasExplicitPriority)
        {
            return new[] { new TemplateRule(element, match, name, modes, explicitPriority, stylesheet, contextItem) };
        }

        var trimmed = StripXPathComments(match).Trim();

        // Strip outer parentheses: (pattern) is semantically equivalent to pattern
        if (trimmed.StartsWith('('))
        {
            int close = FindMatchingParen(trimmed, 0);
            if (close == trimmed.Length - 1)
                trimmed = trimmed[1..close].Trim();
        }

        // Validate union pattern constraints before splitting (XTSE0340)
        if (stylesheet.GetXPathCompatibility(element) < XPathCompatibility.XPath40)
            ValidateUnionPattern(trimmed);

        if (stylesheet.GetXPathCompatibility(element) >= XPathCompatibility.XPath40)
        {
            // A 4.0 union is one rule: next-match must not revisit another alternative.
            return new[] { new TemplateRule(element, match, name, modes,
                ComputeDefaultPriority(match, xpath40: true), stylesheet, contextItem) };
        }

        var branches = SplitUnionBranches(trimmed);
        if (branches.Length <= 1)
        {
            double priority = ComputeDefaultPriority(match);
            return new[] { new TemplateRule(element, match, name, modes, priority, stylesheet, contextItem) };
        }

        // Create a separate TemplateRule for each branch of the union
        var rules = new List<TemplateRule>(branches.Length);
        foreach (var branch in branches)
        {
            double priority = ComputeSinglePatternPriority(branch);
            rules.Add(new TemplateRule(element, branch, name, modes, priority, stylesheet, contextItem));
        }
        return rules;
    }

    /// <summary>
    /// Detects the static type error described in XSLT 3.0 §10.1.1: if the required
    /// context item type is an atomic/item type that cannot match any node selected
    /// by the match pattern, the processor may (and does) report XTTE0590 at compile time.
    /// </summary>
    private static void ValidateContextItemAgainstMatch(XElement element, ContextItemDeclaration? contextItem, XPathCompatibility compatibility)
    {
        if (contextItem == null || string.IsNullOrEmpty(contextItem.AsType))
            return;

        var match = element.Attribute("match")?.Value;
        if (string.IsNullOrEmpty(match))
            return;

        var asType = contextItem.AsType.Trim().ToLowerInvariant();
        if (asType.StartsWith("xs:"))
            asType = asType[3..];
        else if (asType.StartsWith("xsd:"))
            asType = asType[4..];

        // Node tests, item(), and function/map/array types can potentially match nodes/functions.
        if (asType is "item()" or "item" or "node()" or "node" or "text()" or "text"
            or "comment()" or "comment" or "processing-instruction()" or "processing-instruction"
            or "namespace-node()" or "namespace-node" or "element" or "element()" or "attribute" or "attribute()"
            or "document-node" or "document-node()")
            return;

        if (asType.Contains('('))
        {
            // element(...), attribute(...), document-node(...), schema-*, function(*), map(*), array(*)
            if (asType.StartsWith("element(") || asType.StartsWith("attribute(")
                || asType.StartsWith("document-node(") || asType.StartsWith("schema-element(")
                || asType.StartsWith("schema-attribute(") || asType.StartsWith("function(")
                || asType.StartsWith("map(") || asType.StartsWith("array("))
                return;
        }

        if (compatibility >= XPathCompatibility.XPath40 &&
            (Patterns.AtomicPattern.StartsAtomicPattern(StripXPathComments(match).Trim()) ||
             Patterns.RecordPattern.StartsRecordPattern(StripXPathComments(match).Trim()))) return;

        // Any other type is atomic; no node can satisfy an atomic required type.
        // This static check applies only when the match pattern can match nodes only:
        // a pattern like "." also matches atomic items, so the error is dynamic instead
        // (XPTY0019 when an axis step runs on an atomic context item, context-item-911).
        if (match.Trim() == ".")
            return;

        throw new InvalidOperationException($"XTTE0590: Required context item type '{contextItem.AsType}' is incompatible with match pattern '{match}'.");
    }

    internal static IReadOnlyList<string> ParseModes(string? modeAttr, XElement element, string stylesheetDefaultMode)
    {
        if (modeAttr is not null && Stylesheet.UsesApplicableModeNamespaces(element))
            Stylesheet.ValidateTemplateModeAttribute(element, modeAttr);

        if (string.IsNullOrEmpty(modeAttr))
        {
            // If the template has default-mode, use that; otherwise use stylesheet's default-mode
            var templateDefaultMode = element.Attribute("default-mode")?.Value?.Trim();
            if (!string.IsNullOrEmpty(templateDefaultMode))
                return new[] { ExpandModeName(templateDefaultMode, element) };
            if (!string.IsNullOrEmpty(stylesheetDefaultMode))
                return new[] { stylesheetDefaultMode };
            return new[] { "" }; // unnamed mode
        }

        var modes = modeAttr.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (modes.Length == 0)
            return new[] { "" };

        var result = new List<string>(modes.Length);
        foreach (var m in modes)
        {
            if (m == "#default")
            {
                result.Add("");
            }
            else if (m == "#all" || m == "#current")
            {
                result.Add(m);
            }
            else
            {
                result.Add(ExpandModeName(m, element));
            }
        }
        return result;
    }

    /// <summary>
    /// Expands a mode name to Clark notation ({uri}local) using the in-scope
    /// namespaces of the given element. No-op for #default, #all, #current.
    /// </summary>
    private static string ExpandModeName(string mode, XElement element)
    {
        if (Stylesheet.UsesApplicableModeNamespaces(element))
            return mode == "#unnamed" ? "" : Stylesheet.ExpandModeName(mode, element);

        int colon = mode.IndexOf(':');
        if (colon < 0)
            return ModeDefinition.NormalizeModeName(mode); // no prefix

        var prefix = mode.Substring(0, colon);
        var local = mode.Substring(colon + 1);

        // Find the namespace URI for this prefix in the element's scope
        var nsAttr = element.GetPrefixOfNamespace(XNamespace.Get("http://dummy"));
        // Use the element's attributes to find xmlns:prefix
        foreach (var attr in element.Attributes())
        {
            if (attr.IsNamespaceDeclaration && attr.Name.LocalName == prefix)
            {
                return ModeDefinition.NormalizeModeName($"{{{attr.Value}}}{local}");
            }
        }
        // If not found on this element, try ancestor elements
        var ancestor = element.Parent;
        while (ancestor != null)
        {
            foreach (var attr in ancestor.Attributes())
            {
                if (attr.IsNamespaceDeclaration && attr.Name.LocalName == prefix)
                {
                    return ModeDefinition.NormalizeModeName($"{{{attr.Value}}}{local}");
                }
            }
            ancestor = ancestor.Parent;
        }
        // Prefix not found — return normalized name (will likely fail to match, which is correct)
        return ModeDefinition.NormalizeModeName(mode);
    }

    /// <summary>
    /// Compiles the match pattern into an executable predicate.
    /// </summary>
    /// <param name="compiler">The reusable pattern compiler.</param>
    /// <param name="compatibility">The expression profile of the declaring template.</param>
    public void CompileMatch(Patterns.PatternCompiler compiler, XPathCompatibility compatibility = XPathCompatibility.XPath31)
    {
        if (!string.IsNullOrEmpty(Match))
        {
            HasSchemaDependentPattern = ComputeSchemaDependentPattern(Match);
            if (Match.Trim() == "/")
                CompiledMatch = (item, ctx) => item.IsNode && item.NodeValue.NodeKind == XdmNodeKind.Document;
            else
            {
                // Resolve namespace prefixes in the pattern to Q{uri}local syntax
                // so the pattern compiler can match namespaced elements correctly.
                var normalized = StripXPathComments(Match).Trim();
                while (compatibility < XPathCompatibility.XPath40 && normalized.StartsWith('(') && FindMatchingParen(normalized, 0) == normalized.Length - 1)
                    normalized = normalized[1..^1].Trim();
                var typePattern = normalized.StartsWith('~');
                var resolved = typePattern ? normalized : ResolveNamespacePrefixes(Match);
                var defaultNs = GetXPathDefaultNamespace(Element);
                if (typePattern)
                {
                    var owner = Stylesheet.OwningPackage ?? Stylesheet;
                    var namespaces = Stylesheet.GetApplicableStaticNamespaces(Element);
                    var aliases = owner.GetBindingItemTypes();
                    CompiledMatch = compiler.Compile(resolved, defaultNs, namespaces: namespaces,
                        compatibility: compatibility, itemTypeAliases: aliases,
                        namedRecordTypeNames: owner.GetNamedRecordTypes().Keys.ToHashSet(StringComparer.Ordinal));
                    if (Element.Attribute("priority") is null)
                    {
                        var (type, predicates) = Patterns.PatternCompiler.GetTypePatternParts(resolved);
                        string Resolve(string name)
                        {
                            var key = Stylesheet.ExpandItemTypeName(name, Element);
                            if (aliases.TryGetValue(key, out var alias)) return alias;
                            const string schema = "Q{http://www.w3.org/2001/XMLSchema}";
                            return key.StartsWith(schema, StringComparison.Ordinal) ? "xs:" + key[schema.Length..] : key;
                        }
                        var expanded = ItemTypeAliasExpansion.Expand(type, Resolve) ?? type;
                        Priority = predicates.Length > 0 ? 0.5 : ComputeTypePatternPriority(expanded);
                    }
                }
                else
                {
                    var owner = Stylesheet.OwningPackage ?? Stylesheet;
                    var aliases = compatibility >= XPathCompatibility.XPath40 ? owner.GetBindingItemTypes() : null;
                    var namespaces = compatibility >= XPathCompatibility.XPath40 || Stylesheet.HasModuleFixedNamespaces(Element)
                        ? Stylesheet.GetApplicableStaticNamespaces(Element) : null;
                    if (namespaces is not null) Patterns.PatternCompiler.ValidateNamespacePrefixes(Match, namespaces);
                    CompiledMatch = compiler.Compile(resolved, defaultNs, namespaces: namespaces, compatibility: compatibility,
                        itemTypeAliases: aliases, namedRecordTypeNames: compatibility >= XPathCompatibility.XPath40
                            ? owner.GetNamedRecordTypes().Keys.ToHashSet(StringComparer.Ordinal) : null);
                    string Resolve(string name)
                    {
                        var key = Stylesheet.ExpandItemTypeName(name, Element);
                        if (aliases is not null && aliases.TryGetValue(key, out var alias)) return alias;
                        const string schema = "Q{http://www.w3.org/2001/XMLSchema}";
                        return key.StartsWith(schema, StringComparison.Ordinal) ? "xs:" + key[schema.Length..] : key;
                    }
                    if (compatibility >= XPathCompatibility.XPath40 && Element.Attribute("priority") is null)
                        Priority = ComputeDefaultPriority(resolved, xpath40: true, typeResolver: Resolve);
                }
                // In a schema-aware stylesheet, also compile the strict/lax typed-mode
                // variants in which a top-level QName branch means schema-element(QName)
                // (XSLT 3.0 §5.5.3). Rules without rewriteable QName branches keep using
                // the default predicate in every mode.
                if (compiler.HasSchemaSet)
                {
                    SchemaRewriteableQNames = Patterns.PatternCompiler.GetSchemaRewriteableQNames(resolved, defaultNs);
                    if (SchemaRewriteableQNames.Count > 0)
                    {
                        CompiledMatchStrict = compiler.Compile(resolved, defaultNs, ModeTyped.Strict, compatibility: compatibility);
                        CompiledMatchLax = compiler.Compile(resolved, defaultNs, ModeTyped.Lax, compatibility: compatibility);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Lexically detects whether a match pattern is schema-dependent: it contains
    /// <c>schema-element()</c>, <c>schema-attribute()</c>, or a kind test with a type
    /// argument (<c>element(N, T)</c> / <c>attribute(N, T)</c>).
    /// </summary>
    private static bool ComputeSchemaDependentPattern(string match)
    {
        var trimmed = StripXPathComments(match);
        if (trimmed.Contains("schema-element(", StringComparison.Ordinal)
            || trimmed.Contains("schema-attribute(", StringComparison.Ordinal))
            return true;
        for (int i = 0; i < trimmed.Length; i++)
        {
            bool isElement = trimmed.AsSpan(i).StartsWith("element(", StringComparison.Ordinal);
            bool isAttribute = !isElement && trimmed.AsSpan(i).StartsWith("attribute(", StringComparison.Ordinal);
            if (!isElement && !isAttribute)
                continue;
            // Not a kind test when part of a longer name (schema-element()).
            if (i > 0 && IsQNameChar(trimmed[i - 1]))
                continue;
            int open = i + (isElement ? "element".Length : "attribute".Length);
            int close = FindMatchingParen(trimmed, open);
            if (close < 0)
                continue;
            if (HasTopLevelComma(trimmed[(open + 1)..close]))
                return true;
            i = close;
        }
        return false;
    }

    private static bool IsQNameChar(char c) => char.IsLetterOrDigit(c) || c is '-' or '_' or '.';

    /// <summary>
    /// Returns the effective xpath-default-namespace for the given element by walking
    /// the ancestor chain and finding the nearest xpath-default-namespace attribute.
    /// </summary>
    private static string? GetXPathDefaultNamespace(XElement element)
    {
        var current = element;
        while (current != null)
        {
            // The XSLT-namespaced form (e.g. xsl:xpath-default-namespace) is effective on any element
            var attr = current.Attribute(XName.Get("xpath-default-namespace", Stylesheet.XslNamespace));
            if (attr != null)
            {
                // XTSE0090: xsl:xpath-default-namespace is not allowed on XSLT elements
                if (current.Name.NamespaceName == Stylesheet.XslNamespace)
                    throw new InvalidOperationException("XTSE0090");
                return attr.Value;
            }
            // The no-namespace form is only effective on XSLT elements
            if (current.Name.NamespaceName == Stylesheet.XslNamespace)
            {
                attr = current.Attribute("xpath-default-namespace");
                if (attr != null) return attr.Value;
            }
            current = current.Parent;
        }
        return null;
    }

    /// <summary>
    /// Replaces prefix:local-name occurrences in a pattern with Q{uri}local-name,
    /// resolving prefixes using applicable static namespaces in 4.0 and native
    /// declarations in earlier profiles.
    /// </summary>
    private string ResolveNamespacePrefixes(string pattern)
    {
        // Quick exit if no colon present (no prefixes to resolve)
        if (!pattern.Contains(':'))
            return pattern;

        var staticNamespaces = Stylesheet.GetXPathCompatibility(Element) >= XPathCompatibility.XPath40 || Stylesheet.HasModuleFixedNamespaces(Element)
            ? Stylesheet.GetApplicableStaticNamespaces(Element) : null;
        var sb = new System.Text.StringBuilder();
        int i = 0;
        while (i < pattern.Length)
        {
            char c = pattern[i];
            // Skip string literals
            if (c == '\'' || c == '\"')
            {
                char quote = c;
                sb.Append(c);
                i++;
                while (i < pattern.Length && pattern[i] != quote)
                {
                    sb.Append(pattern[i]);
                    i++;
                }
                if (i < pattern.Length)
                {
                    sb.Append(pattern[i]);
                    i++;
                }
                continue;
            }
            // Check for Q{…} syntax — already resolved, copy through
            if (c == 'Q' && i + 1 < pattern.Length && pattern[i + 1] == '{')
            {
                if (Stylesheet.GetXPathCompatibility(Element) >= XPathCompatibility.XPath40)
                {
                    int close = pattern.IndexOf('}', i + 2);
                    if (close < 0) throw new InvalidOperationException("XPST0003: Unclosed EQName URI in a pattern.");
                    sb.Append(pattern, i, close - i + 1);
                    i = close + 1;
                    continue;
                }
                sb.Append(c);
                i++;
                continue;
            }
            // Look for prefix:local-name
            if (char.IsLetter(c) || c == '_')
            {
                int start = i;
                while (i < pattern.Length && (char.IsLetterOrDigit(pattern[i]) || pattern[i] == '_' || pattern[i] == '-'))
                    i++;
                if (i < pattern.Length && pattern[i] == ':')
                {
                    // Check if this is :: (axis separator) rather than : (namespace prefix)
                    if (i + 1 < pattern.Length && pattern[i + 1] == ':')
                    {
                        sb.Append(pattern[start..i]);
                        continue;
                    }
                    var prefix = pattern[start..i];
                    i++; // skip ':'
                    int localStart = i;
                    while (i < pattern.Length && (char.IsLetterOrDigit(pattern[i]) || pattern[i] == '_' || pattern[i] == '-' || pattern[i] == '.'))
                        i++;
                    var local = pattern[localStart..i];
                    // Preserve unresolved prefixes for the pattern compiler to diagnose.
                    var nsUri = staticNamespaces is null
                        ? Element.GetNamespaceOfPrefix(prefix)?.NamespaceName
                        : staticNamespaces.GetValueOrDefault(prefix);
                    if (!string.IsNullOrEmpty(nsUri))
                    {
                        sb.Append($"Q{{{nsUri}}}{local}");
                    }
                    else
                    {
                        sb.Append(prefix);
                        sb.Append(':');
                        sb.Append(local);
                    }
                }
                else
                {
                    sb.Append(pattern[start..i]);
                }
                continue;
            }
            sb.Append(c);
            i++;
        }
        return sb.ToString();
    }

    /// <summary>
    /// Computes default priority using the owning profile for supported node kind tests.
    /// </summary>
    private static double ComputeDefaultPriority(string? match, bool xpath40 = false, Func<string, string>? typeResolver = null)
    {
        if (string.IsNullOrEmpty(match))
            return 0.5;

        var trimmed = StripXPathComments(match).Trim();

        if (xpath40 && Patterns.StringKeyMapPattern.StartsMapPattern(trimmed))
            return Patterns.StringKeyMapPattern.Parse(trimmed).DefaultPriority;

        if (xpath40 && Patterns.AtomicPattern.StartsAtomicPattern(trimmed))
            return Patterns.AtomicPattern.Parse(trimmed).DefaultPriority(typeResolver);

        if (xpath40 && Patterns.RecordPattern.StartsRecordPattern(trimmed))
            return Patterns.RecordPattern.Parse(trimmed).DefaultPriority;

        if (xpath40 && Patterns.ArrayPattern.StartsArrayPattern(trimmed))
        {
            var array = Patterns.ArrayPattern.Parse(trimmed);
            return array.HasPredicates || Patterns.ArrayPattern.IsEmptySequence(array.Members)
                ? 0.5 : ComputeDefaultPriority(array.ItemPattern, xpath40: true, typeResolver: typeResolver);
        }

        // Strip outer parentheses: (pattern) has same priority as pattern
        if (trimmed.StartsWith('('))
        {
            int close = FindMatchingParen(trimmed, 0);
            if (close == trimmed.Length - 1)
                return ComputeDefaultPriority(trimmed[1..close].Trim(), xpath40, typeResolver);
        }

        // XSLT 4.0 assigns one rule the minimum alternative priority; retain the 3.0 path.
        var branches = xpath40 ? SplitNodeUnion40(trimmed) : SplitUnionBranches(trimmed);
        if (branches.Length > 1)
            return xpath40
                ? branches.Min(branch => ComputeDefaultPriority(branch, true, typeResolver))
                : branches.Max(branch => ComputeSinglePatternPriority(branch));

        if (xpath40)
        {
            var operands = SplitNodeUnion40(trimmed, setOperators: true);
            if (operands.Length > 1)
                return ComputeDefaultPriority(operands[0], xpath40: true, typeResolver: typeResolver);
        }

        if (xpath40 && trimmed.StartsWith('~'))
        {
            var (type, predicates) = Patterns.PatternCompiler.GetTypePatternParts(trimmed);
            var expanded = typeResolver is null ? type : ItemTypeAliasExpansion.Expand(type, typeResolver) ?? type;
            return predicates.Length > 0 ? 0.5 : ComputeTypePatternPriority(expanded);
        }
        return ComputeSinglePatternPriority(trimmed, xpath40);
    }

    private static double ComputeTypePatternPriority(string type)
    {
        var text = type.Trim();
        if (text.StartsWith('(') && FindMatchingParen(text, 0) == text.Length - 1)
            return SplitUnionBranches(text[1..^1]).Max(ComputeTypePatternPriority);
        if (text == "item()") return -1;
        if (text == "node()") return -0.75;
        if (text == "function(*)" || text == "xs:anyAtomicType") return -0.5;
        if (text is "array(*)" or "map(*)") return -0.25;
        if (text.StartsWith("enum(", StringComparison.Ordinal)) return 0.25;
        if (text is "xs:dayTimeDuration" or "xs:yearMonthDuration" or "xs:dateTimeStamp") return 0.25;
        if (text.StartsWith("xs:", StringComparison.Ordinal))
        {
            var atomic = System.Xml.Schema.XmlSchemaType.GetBuiltInSimpleType(
                new System.Xml.XmlQualifiedName(text[3..], "http://www.w3.org/2001/XMLSchema"));
            int depth = 0;
            for (var current = atomic; current is not null && current.QualifiedName.Name != "anySimpleType";
                current = current.BaseXmlSchemaType as System.Xml.Schema.XmlSchemaSimpleType)
                depth++;
            return depth <= 1 ? 0 : 0.5 - Math.Pow(2, -depth);
        }
        if (text.StartsWith("element(") || text.StartsWith("attribute(") ||
            text.StartsWith("document-node(") || text.StartsWith("schema-element(") ||
            text.StartsWith("schema-attribute(") || text.StartsWith("processing-instruction(") ||
            text is "text()" or "comment()" or "namespace-node()")
            return ComputeSinglePatternPriority(text, xpath40: true);
        return 0;
    }

    private static double ComputeSinglePatternPriority(string trimmed, bool xpath40 = false)
    {
        trimmed = StripXPathComments(trimmed).Trim();

        // Strip outer parentheses: (pattern) has same priority as pattern
        if (trimmed.StartsWith('('))
        {
            int close = FindMatchingParen(trimmed, 0);
            if (close == trimmed.Length - 1)
                return ComputeSinglePatternPriority(trimmed[1..close].Trim(), xpath40);
        }

        // Predicate patterns precede path classification: slashes inside their predicates are expressions.
        if (xpath40 && (trimmed == "." ||
            (trimmed.StartsWith('.') && trimmed[1..].TrimStart().StartsWith('['))))
            return trimmed == "." ? -1 : 1;

        // Root pattern
        if (trimmed == "/")
            return -0.5;

        // Document patterns
        if (trimmed.StartsWith("doc(") || trimmed.StartsWith("document("))
            return 0.5;

        // Path patterns (contain / outside of Q{} braces)
        bool hasPathSlash = false;
        int qDepth = 0;
        for (int pi = 0; pi < trimmed.Length; pi++)
        {
            if (trimmed[pi] == 'Q' && pi + 1 < trimmed.Length && trimmed[pi + 1] == '{')
            {
                qDepth++;
                pi++;
            }
            else if (qDepth > 0 && trimmed[pi] == '}')
            {
                qDepth--;
            }
            else if (qDepth == 0 && trimmed[pi] == '/')
            {
                hasPathSlash = true;
                break;
            }
        }
        if (hasPathSlash)
            return 0.5;

        // PredicatePattern: .[expr]
        if (trimmed.StartsWith("."))
            return trimmed.Contains('[') ? 1.0 : -0.5;

        // A predicate increases node-pattern priority, including explicit-axis kind tests.
        if (xpath40 && MaskPatternUris(trimmed).Contains('[')) return 0.5;

        if (xpath40 && trimmed.StartsWith('@'))
            return ComputeSinglePatternPriority(trimmed[1..].Trim(), xpath40: true);

        // EQName URI punctuation must not be interpreted as an axis separator.
        var axisText = xpath40 ? MaskPatternUris(trimmed) : trimmed;
        int axisSeparator = axisText.IndexOf("::", StringComparison.Ordinal);
        if (xpath40 && axisSeparator >= 0)
        {
            var forwardAxis = trimmed[..axisSeparator].Trim();
            if (forwardAxis is "child" or "descendant" or "attribute" or "self" or "descendant-or-self" or "namespace")
                return ComputeSinglePatternPriority(trimmed[(axisSeparator + 2)..].Trim(), xpath40: true);
        }

        // Explicit axis step in earlier profiles.
        if (axisSeparator >= 0)
        {
            int colonIdx = axisSeparator;
            string axis = trimmed[..colonIdx].Trim().ToLowerInvariant();
            string nodeTest = trimmed[(colonIdx + 2)..].Trim();

            // child::QName → 0.0; attribute::QName → 0.5
            if ((axis == "child" || axis == "attribute") && IsQNameNodeTest(nodeTest))
                return axis == "child" ? 0.0 : 0.5;

            // element()/attribute() node tests take the same default priority as the
            // axis-free form (XSLT 3.0 §6.4): wildcard name → -0.5, name only → 0.0,
            // a type argument → 0.25 (match-167/171).
            if (nodeTest.StartsWith("element(") || nodeTest.StartsWith("attribute("))
                return ElementOrAttributeTestPriority(nodeTest, xpath40);

            // Namespace wildcards: NCName:*, *:NCName, or Q{uri}* → -0.25
            if (nodeTest.EndsWith(":*") || nodeTest.StartsWith("*:") ||
                (nodeTest.StartsWith("Q{") && nodeTest.EndsWith("*")))
                return -0.25;

            // KindTest or * → -0.5
            if (nodeTest == "*" || IsKindTest(nodeTest))
                return -0.5;

            // Any other axis step → 0.5
            return 0.5;
        }

        // @attr is shorthand for attribute::attr → 0.5
        if (trimmed.StartsWith('@'))
        {
            var name = trimmed[1..].Trim();
            if (name == "*")
                return -0.5;
            if (name.EndsWith(":*") || name.StartsWith("*:") ||
                (name.StartsWith("Q{") && name.EndsWith("*")))
                return -0.25;
            if (!name.Contains('*') && !name.Contains('('))
                return 0.5;
            return 0.5;
        }

        // Wildcards and kind tests without axis
        if (trimmed == "*" || IsKindTest(trimmed) || trimmed == ".")
            return -0.5;

        // ElementTest and AttributeTest with arguments
        if (trimmed.StartsWith("element(") && trimmed.EndsWith(")"))
            return ElementOrAttributeTestPriority(trimmed, xpath40);

        if (trimmed.StartsWith("attribute(") && trimmed.EndsWith(")"))
            return ElementOrAttributeTestPriority(trimmed, xpath40);

        // processing-instruction("name") or processing-instruction(name) → 0
        if (trimmed.StartsWith("processing-instruction(") && trimmed.EndsWith(")"))
        {
            var arg = ExtractFunctionArg(trimmed);
            return string.IsNullOrEmpty(arg) ? -0.5 : 0.0;
        }

        // document-node() → -0.5; document-node(element(E)) → priority of inner test
        if (trimmed.StartsWith("document-node(") && trimmed.EndsWith(")"))
        {
            var arg = ExtractFunctionArg(trimmed);
            return string.IsNullOrEmpty(arg) ? -0.5 : ComputeSinglePatternPriority(arg, xpath40);
        }

        // schema-element and schema-attribute have priority 0.25
        if ((trimmed.StartsWith("schema-element(") || trimmed.StartsWith("schema-attribute(")) &&
            trimmed.EndsWith(")"))
            return 0.25;

        // Namespace wildcards without axis: NCName:*, *:NCName, or Q{uri}*
        if (trimmed.EndsWith(":*") || trimmed.StartsWith("*:") ||
            (trimmed.StartsWith("Q{") && trimmed.EndsWith("*")))
            return -0.25;

        // Patterns with a predicate
        if (trimmed.Contains('['))
            return 0.5;

        // QName without axis (implicit child::)
        if (IsQNameNodeTest(trimmed))
            return 0.0;

        // Fallback: any other pattern
        return 0.5;
    }

    private static bool IsQNameNodeTest(string s)
    {
        // A QName has no wildcards, no parentheses, no brackets, no operators
        if (string.IsNullOrEmpty(s)) return false;
        foreach (char c in s)
        {
            if (c == '*' || c == '(' || c == ')' || c == '[' || c == ']' || c == '|' || c == '/')
                return false;
        }
        return true;
    }

    private static bool IsKindTest(string s)
    {
        return s switch
        {
            "node()" or "text()" or "comment()" or "processing-instruction()"
            or "element()" or "attribute()" or "schema-element()" or "schema-attribute()"
            or "namespace-node()" or "document-node()" => true,
            _ => false
        };
    }

    /// <summary>
    /// Computes the default priority of an <c>element(...)</c> or <c>attribute(...)</c>
    /// kind test per XSLT 3.0 §6.4: no argument or a wildcard name only → -0.5; a name
    /// only → 0.0; a type argument after a top-level comma → 0.25. XSLT 4.0 instead
    /// assigns partial wildcards -0.25 and combines name/type priority as 0.25 + 0.5 * name priority.
    /// </summary>
    private static double ElementOrAttributeTestPriority(string nodeTest, bool xpath40 = false)
    {
        var arg = ExtractFunctionArg(nodeTest);
        if (xpath40)
        {
            int comma = FindKindTestComma(arg);
            var names = comma < 0 ? arg : arg[..comma];
            double namePriority = SplitKindTestNames(names).Max(name =>
            {
                var test = name.Trim();
                if (test.Length == 0 || test == "*") return -0.5;
                if (test.StartsWith("*:", StringComparison.Ordinal) || test.EndsWith(":*", StringComparison.Ordinal)
                    || (test.StartsWith("Q{", StringComparison.Ordinal) && test.EndsWith("}*", StringComparison.Ordinal))) return -0.25;
                return 0;
            });
            return comma < 0 ? namePriority : 0.25 + 0.5 * namePriority;
        }
        if (string.IsNullOrEmpty(arg) || arg == "*")
            return -0.5;                    // element() / element(*) / attribute() / attribute(*)
        if (HasTopLevelComma(arg))
            return 0.25;                    // element(*, T) / element(E, T) / attribute(*, T) / attribute(A, T)
        return 0.0;                         // element(E) / attribute(A)
    }

    private static string MaskPatternUris(string text)
    {
        var chars = text.ToCharArray();
        for (int i = 0; i + 1 < text.Length; i++)
        {
            if (text[i] != 'Q' || text[i + 1] != '{') continue;
            int close = text.IndexOf('}', i + 2);
            if (close < 0) break;
            Array.Fill(chars, ' ', i + 2, close - i - 2);
            i = close;
        }
        return new string(chars);
    }

    private static IEnumerable<string> SplitKindTestNames(string text)
    {
        int start = 0;
        bool eqName = false;
        for (int i = 0; i < text.Length; i++)
        {
            if (eqName) { if (text[i] == '}') eqName = false; continue; }
            if (text[i] == 'Q' && i + 1 < text.Length && text[i + 1] == '{') { eqName = true; i++; continue; }
            if (text[i] == '|') { yield return text[start..i]; start = i + 1; }
        }
        yield return text[start..];
    }

    private static int FindKindTestComma(string text)
    {
        int depth = 0;
        bool eqName = false;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (eqName) { if (c == '}') eqName = false; continue; }
            if (c == 'Q' && i + 1 < text.Length && text[i + 1] == '{') { eqName = true; i++; continue; }
            if (c == '(') depth++;
            else if (c == ')') depth--;
            else if (c == ',' && depth == 0) return i;
        }
        return -1;
    }

    /// <summary>
    /// Returns true when the text contains a comma at the top level (not inside
    /// parentheses, brackets, or <c>Q{uri}</c> braces).
    /// </summary>
    private static bool HasTopLevelComma(string text)
    {
        int depth = 0;
        int braceDepth = 0;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == 'Q' && i + 1 < text.Length && text[i + 1] == '{')
            {
                braceDepth++;
                i++;
                continue;
            }
            if (braceDepth > 0)
            {
                if (c == '}')
                    braceDepth--;
                continue;
            }
            if (c == '(' || c == '[') depth++;
            else if (c == ')' || c == ']') depth--;
            else if (c == ',' && depth == 0)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Validates union pattern constraints (XTSE0340) before splitting.
    /// </summary>
    private static void ValidateUnionPattern(string pattern)
    {
        var branches = SplitUnionBranches(pattern);
        if (branches.Length <= 1) return;

        bool hasNodePattern = false;
        bool hasTypePattern = false;
        bool hasPredicatePattern = false;
        foreach (var b in branches)
        {
            var s = b.Trim();
            if (LooksLikeTypePattern(s))
                hasTypePattern = true;
            else
                hasNodePattern = true;
            if (s.StartsWith(".["))
                hasPredicatePattern = true;
        }
        if (hasNodePattern && hasTypePattern)
        {
            throw new InvalidOperationException("XTSE0340: Union of node pattern and type pattern is not allowed.");
        }
        if (hasPredicatePattern)
        {
            throw new InvalidOperationException("XTSE0340: Predicate pattern is not allowed in a union.");
        }
    }

    private static bool LooksLikeTypePattern(string branch)
    {
        var s = branch.Trim();
        if (s.StartsWith("(.[") || (s.StartsWith(".[") && s.Contains("instance of")))
            return true;
        if (s.StartsWith("element(") || s.StartsWith("attribute(") ||
            s.StartsWith("text(") || s.StartsWith("comment(") ||
            s.StartsWith("processing-instruction(") || s.StartsWith("document-node(") ||
            s.StartsWith("node("))
        {
            return false;
        }
        if (s.Contains("instance of"))
            return true;
        return false;
    }

    /// <summary>Splits 4.0 union or intersect/except operands without treating nested content as operators.</summary>
    private static string[] SplitNodeUnion40(string pattern, bool setOperators = false)
    {
        var parts = new List<string>();
        int start = 0, depth = 0;
        char quote = '\0';
        bool uri = false;
        for (int i = 0; i < pattern.Length; i++)
        {
            char c = pattern[i];
            if (quote != '\0')
            {
                if (c == quote)
                {
                    if (i + 1 < pattern.Length && pattern[i + 1] == quote) i++;
                    else quote = '\0';
                }
                continue;
            }
            if (uri) { if (c == '}') uri = false; continue; }
            if (c == 'Q' && i + 1 < pattern.Length && pattern[i + 1] == '{') { uri = true; i++; continue; }
            if (c is '\'' or '"') { quote = c; continue; }
            if (c is '(' or '[') { depth++; continue; }
            if (c is ')' or ']') { depth--; continue; }
            if (depth != 0) continue;
            int width = !setOperators && c == '|' ? 1 : 0;
            string? word = setOperators
                ? (pattern.AsSpan(i).StartsWith("intersect", StringComparison.Ordinal) ? "intersect"
                    : pattern.AsSpan(i).StartsWith("except", StringComparison.Ordinal) ? "except" : null)
                : "union";
            if (width == 0 && word is not null && i > start && pattern.AsSpan(i).StartsWith(word, StringComparison.Ordinal)
                && (char.IsWhiteSpace(pattern[i - 1]) || (setOperators && pattern[i - 1] is ')' or ']' or '*'))
                && (i + word.Length == pattern.Length || char.IsWhiteSpace(pattern[i + word.Length]) ||
                    (setOperators && pattern[i + word.Length] is '(' or '/' or '@' or '*')))
            {
                int previous = i - 1;
                while (previous >= start && char.IsWhiteSpace(pattern[previous])) previous--;
                if (previous >= start && pattern[previous] is not '/' and not '@' and not ':' and not '|') width = word.Length;
            }
            if (width == 0) continue;
            parts.Add(pattern[start..i].Trim());
            start = i + width;
            i += width - 1;
        }
        parts.Add(pattern[start..].Trim());
        if (parts.Count > 1 && parts.Any(string.IsNullOrEmpty))
            throw new InvalidOperationException("XTSE0340: A node set pattern requires an operand on each side of its operator.");
        return parts.ToArray();
    }

    private static string[] SplitUnionBranches(string pattern)
        => Patterns.PatternCompiler.SplitPatternTopLevel(pattern, "|");

    /// <summary>
    /// Removes XPath comments (: ... :) from the text, preserving string literals.
    /// </summary>
    private static string StripXPathComments(string text)
    {
        var sb = new System.Text.StringBuilder();
        int i = 0;
        while (i < text.Length)
        {
            char c = text[i];
            // Preserve string literals
            if (c == '\'' || c == '"')
            {
                char quote = c;
                sb.Append(c);
                i++;
                while (i < text.Length && text[i] != quote)
                {
                    sb.Append(text[i]);
                    i++;
                }
                if (i < text.Length)
                {
                    sb.Append(text[i]);
                    i++;
                }
                continue;
            }
            // Skip comment
            if (i + 1 < text.Length && text[i] == '(' && text[i + 1] == ':')
            {
                i += 2;
                int depth = 1;
                while (i < text.Length && depth > 0)
                {
                    if (i + 1 < text.Length && text[i] == ':' && text[i + 1] == ')')
                    {
                        depth--;
                        i += 2;
                    }
                    else if (i + 1 < text.Length && text[i] == '(' && text[i + 1] == ':')
                    {
                        depth++;
                        i += 2;
                    }
                    else
                    {
                        i++;
                    }
                }
                continue;
            }
            sb.Append(c);
            i++;
        }
        return sb.ToString();
    }

    /// <summary>
    /// Extracts the argument string from a function-like call, e.g. "name, type" from "element(name, type)".
    /// </summary>
    private static string ExtractFunctionArg(string s)
    {
        int open = s.IndexOf('(');
        if (open < 0) return "";
        int close = FindMatchingParen(s, open);
        if (close < 0) return "";
        return s[(open + 1)..close].Trim();
    }

    /// <summary>
    /// Finds the index of the matching closing parenthesis for the paren at startIndex.
    /// Returns -1 if not found.
    /// </summary>
    private static int FindMatchingParen(string text, int startIndex)
        => Patterns.PatternCompiler.FindMatchingDelimiter(text, startIndex, '(', ')');

    /// <summary>
    /// Returns true if the attribute value contains an unescaped AVT expression delimiter.
    /// Curly braces that are part of an XPath 3.1 EQName (<c>Q{uri}</c>) are not AVTs.
    /// </summary>
    private static bool ContainsAvtExpression(string value)
    {
        for (int i = 0; i < value.Length; i++)
        {
            if (value[i] == '{')
            {
                if (i + 1 < value.Length && value[i + 1] == '{')
                {
                    i++; // escaped brace
                }
                else if (i > 0 && value[i - 1] == 'Q')
                {
                    // EQName syntax: Q{namespace-uri}local-name
                    continue;
                }
                else
                {
                    return true;
                }
            }
        }
        return false;
    }
}

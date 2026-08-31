// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 mei 2026
// PURPOSE              : A single template rule with its match pattern, priority, mode, and compiled body.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
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
// ===========================================================================================================================================================

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.Xslt.Stylesheet;

/// <summary>
/// Represents a single xsl:template rule within a stylesheet.
/// </summary>
public sealed class TemplateRule
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
    public double Priority { get; }

    /// <summary>The import precedence (0 = main stylesheet, higher = deeper import).</summary>
    public int ImportPrecedence => Stylesheet.ImportPrecedence;

    /// <summary>The compiled match predicate, or null for named-only templates.</summary>
    public Patterns.PatternPredicate? CompiledMatch { get; private set; }

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
    /// Union patterns in the match attribute (e.g. <c>match="a|b"</c>) are split into
    /// separate rules so that <c>xsl:next-match</c> can continue to the other branches.
    /// </summary>
    public static IReadOnlyList<TemplateRule> FromElement(XElement element, Stylesheet stylesheet)
    {
        var contextItem = ContextItemDeclaration.FromTemplate(element);
        ValidateContextItemAgainstMatch(element, contextItem);

        var match = element.Attribute("match")?.Value;
        if (!string.IsNullOrEmpty(match) && ContainsAvtExpression(match))
            throw new InvalidOperationException("XTSE0340: The match attribute of xsl:template must not contain an attribute value template");
        if (string.IsNullOrEmpty(match))
            match = element.Attribute("_match")?.Value;
        var name = element.Attribute("name")?.Value?.Trim();
        var modeAttr = element.Attribute("mode")?.Value;
        var modes = ParseModes(modeAttr, element, stylesheet.DefaultMode);

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
        ValidateUnionPattern(trimmed);

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
    private static void ValidateContextItemAgainstMatch(XElement element, ContextItemDeclaration? contextItem)
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

        // Any other type is atomic; no node can satisfy an atomic required type.
        throw new InvalidOperationException($"XTTE0590: Required context item type '{contextItem.AsType}' is incompatible with match pattern '{match}'.");
    }

    private static IReadOnlyList<string> ParseModes(string? modeAttr, XElement element, string stylesheetDefaultMode)
    {
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
    public void CompileMatch(Patterns.PatternCompiler compiler)
    {
        if (!string.IsNullOrEmpty(Match))
        {
            if (Match.Trim() == "/")
                CompiledMatch = (item, ctx) => item.IsNode && item.NodeValue.NodeKind == XdmNodeKind.Document;
            else
            {
                // Resolve namespace prefixes in the pattern to Q{uri}local syntax
                // so the pattern compiler can match namespaced elements correctly.
                var resolved = ResolveNamespacePrefixes(Match);
                var defaultNs = GetXPathDefaultNamespace(Element);
                CompiledMatch = compiler.Compile(resolved, defaultNs);
            }
        }
    }

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
    /// resolving prefixes using the namespace declarations in scope on the
    /// xsl:template element.
    /// </summary>
    private string ResolveNamespacePrefixes(string pattern)
    {
        // Quick exit if no colon present (no prefixes to resolve)
        if (!pattern.Contains(':'))
            return pattern;

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
                    // Resolve prefix; if not found, keep original (will fail to match, which is correct)
                    var nsUri = Element.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? "";
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
    /// Computes the default priority for a match pattern per the XSLT 2.0/3.0 spec.
    /// </summary>
    private static double ComputeDefaultPriority(string? match)
    {
        if (string.IsNullOrEmpty(match))
            return 0.5;

        var trimmed = StripXPathComments(match).Trim();

        // Strip outer parentheses: (pattern) has same priority as pattern
        if (trimmed.StartsWith('('))
        {
            int close = FindMatchingParen(trimmed, 0);
            if (close == trimmed.Length - 1)
                return ComputeDefaultPriority(trimmed[1..close].Trim());
        }

        // Union patterns: take the maximum priority of all branches
        var branches = SplitUnionBranches(trimmed);
        if (branches.Length > 1)
            return branches.Max(ComputeSinglePatternPriority);

        return ComputeSinglePatternPriority(trimmed);
    }

    private static double ComputeSinglePatternPriority(string trimmed)
    {
        trimmed = StripXPathComments(trimmed).Trim();

        // Strip outer parentheses: (pattern) has same priority as pattern
        if (trimmed.StartsWith('('))
        {
            int close = FindMatchingParen(trimmed, 0);
            if (close == trimmed.Length - 1)
                return ComputeSinglePatternPriority(trimmed[1..close].Trim());
        }

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

        // Explicit axis step
        if (trimmed.Contains("::"))
        {
            int colonIdx = trimmed.IndexOf("::");
            string axis = trimmed[..colonIdx].Trim().ToLowerInvariant();
            string nodeTest = trimmed[(colonIdx + 2)..].Trim();

            // child::QName → 0.0; attribute::QName → 0.5
            if ((axis == "child" || axis == "attribute") && IsQNameNodeTest(nodeTest))
                return axis == "child" ? 0.0 : 0.5;

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
        {
            var arg = ExtractFunctionArg(trimmed);
            if (string.IsNullOrEmpty(arg) || arg == "*")
                return -0.5;                    // element() or element(*)
            if (arg.Contains(','))
            {
                var parts = arg.Split(',').Select(s => s.Trim()).ToArray();
                if (parts.Length == 2)
                    return parts[0] == "*" ? 0.0 : 0.25; // element(*,T) or element(E,T)
            }
            return 0.0;                         // element(E)
        }

        if (trimmed.StartsWith("attribute(") && trimmed.EndsWith(")"))
        {
            var arg = ExtractFunctionArg(trimmed);
            if (string.IsNullOrEmpty(arg) || arg == "*")
                return -0.5;                    // attribute() or attribute(*)
            if (arg.Contains(','))
            {
                var parts = arg.Split(',').Select(s => s.Trim()).ToArray();
                if (parts.Length == 2)
                    return parts[0] == "*" ? 0.0 : 0.25; // attribute(*,T) or attribute(A,T)
            }
            return 0.0;                         // attribute(A)
        }

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
            return string.IsNullOrEmpty(arg) ? -0.5 : ComputeSinglePatternPriority(arg);
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

    private static string[] SplitUnionBranches(string pattern)
    {
        var parts = new List<string>();
        int start = 0;
        int depth = 0;
        for (int i = 0; i < pattern.Length; i++)
        {
            char c = pattern[i];
            if (c == '(' || c == '[') depth++;
            else if (c == ')' || c == ']') depth--;
            else if (c == '|' && depth == 0)
            {
                parts.Add(pattern[start..i].Trim());
                start = i + 1;
            }
        }
        parts.Add(pattern[start..].Trim());
        return parts.Where(p => !string.IsNullOrEmpty(p)).ToArray();
    }

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
    {
        if (text[startIndex] != '(') return -1;
        int depth = 1;
        for (int i = startIndex + 1; i < text.Length; i++)
        {
            if (text[i] == '(') depth++;
            else if (text[i] == ')') depth--;
            if (depth == 0) return i;
        }
        return -1;
    }

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

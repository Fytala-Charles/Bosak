// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 09 October 2026
// PURPOSE              : XML↔map conversion shared by fn:element-to-map, fn:map-to-element and fn:element-to-map-plan (F&O 4.0 §17.6)
// SPECIAL NOTES        : Part of the standard XPath / XQuery function library.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 09-10-2026     | Creation                                                                                 |
//                      |------------------|-------|----------------|------------------------------------------------------------------------------------------|
//                      | Charles Korthout | 0.2   | 09-10-2026     | InferType numeric subtype promotion: a value mix of integers and decimals infers        |
//                      |                  |       |                | xs:decimal, any exponent form infers xs:double (element-to-map-552/553 ground truth)    |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.XPath.Standard.Functions;

/// <summary>
/// Shared machinery for the F+O 4.0 §17.6 XML/map conversion functions fn:element-to-map,
/// fn:map-to-element and fn:element-to-map-plan: option and plan validation (XPTY0004),
/// layout inference (empty / empty-plus / simple / simple-plus / list / list-plus / record /
/// sequence / mixed / xml / deep-skip / error), the PR2688 untyped-value type inference
/// (boolean / integer / decimal / double), conversion to prescribed types (FOJS0010, liberal
/// fallback), plan-driven conversion (FOJS0008 with '*' fallback) and parentless-element
/// construction from maps (FOJS0009).
/// </summary>
internal static class ElementMap
{
    internal const string FnNamespace = "http://www.w3.org/2005/xpath-functions";
    internal const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";
    internal const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";

    /// <summary>The layout names of F&O 4.0 §17.6 (option/plan values are lower-case).</summary>
    internal enum Layout
    {
        Empty, EmptyPlus, Simple, SimplePlus, List, ListPlus, Record, Sequence, Mixed,
        Xml, DeepSkip, Error,
    }

    private static readonly Dictionary<string, Layout> s_layouts = new(StringComparer.Ordinal)
    {
        ["empty"] = Layout.Empty,
        ["empty-plus"] = Layout.EmptyPlus,
        ["simple"] = Layout.Simple,
        ["simple-plus"] = Layout.SimplePlus,
        ["list"] = Layout.List,
        ["list-plus"] = Layout.ListPlus,
        ["record"] = Layout.Record,
        ["sequence"] = Layout.Sequence,
        ["mixed"] = Layout.Mixed,
        ["xml"] = Layout.Xml,
        ["deep-skip"] = Layout.DeepSkip,
        ["error"] = Layout.Error,
    };

    private static readonly string[] s_validTypes =
        ["integer", "decimal", "double", "boolean", "string", "skip"];

    // Leading/trailing whitespace is trimmed before matching (F&O 14.6.3), so the
    // lexical space tests apply to the trimmed value; an explicit plus sign is allowed.
    private static readonly Regex s_integerRe = new(@"^[+-]?(0|[1-9][0-9]*)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex s_decimalRe = new(@"^[+-]?((0|[1-9][0-9]*)\.[0-9]+|\.[0-9]+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex s_doubleRe = new(@"^[+-]?((0|[1-9][0-9]*)(\.[0-9]+)?|\.[0-9]+)[eE][+-]?[0-9]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>Thrown when input is inconsistent with a forced plan layout (FOJS0008).</summary>
    private sealed class InconsistentLayoutException : Exception
    {
    }

    /// <summary>A validated <c>plan</c> option entry (only the keys of F&O PR2903 are allowed).</summary>
    internal sealed class PlanEntry
    {
        public Layout? Layout;
        public string? Child;   // null = absent
        public string? Type;    // null = absent, "" = explicitly ()
        public bool Validated;  // lazy entry validation (only matched entries are checked)
    }

    /// <summary>Validated options of the three conversion functions.</summary>
    internal sealed class EmOptions
    {
        public string NameFormat = "default";
        public string AttributeMarker = "@";
        public string ContentKey = "#content";
        public Dictionary<string, PlanEntry>? Plan;
        public bool Liberal;
    }

    // =====================================================================================
    // Option parsing (shared)
    // =====================================================================================

    /// <summary>
    /// Validates the <c>$options</c> map: known keys only (XPTY0004), <c>name-format</c>
    /// eqname/lexical/local, string <c>attribute-marker</c>/<c>content-key</c>, boolean
    /// <c>liberal</c>, and the <c>plan</c> map (entry keys layout/child/type only, valid
    /// layout names, valid type names). Plan entries are validated lazily on first match
    /// (element-to-map-597: unmatched entries are ignored).
    /// </summary>
    internal static EmOptions ParseOptions(XdmValue optionsArg)
    {
        var o = new EmOptions();
        if (IsEmpty(optionsArg))
            return o;
        if (!optionsArg.IsMap)
            throw new InvalidOperationException("XPTY0004: options must be a single map");

        var map = optionsArg.MapValue;
        foreach (var keyValue in map.Keys)
        {
            if (!map.TryGetValue(keyValue, out var value))
                continue;
            switch (Str(keyValue))
            {
                case "name-format":
                    o.NameFormat = Str(value);
                    if (o.NameFormat is not ("default" or "eqname" or "lexical" or "local"))
                        throw new InvalidOperationException("XPTY0004: name-format must be 'default', 'eqname', 'lexical' or 'local'");
                    break;
                case "attribute-marker":
                    if (!IsSingleString(value))
                        throw new InvalidOperationException("XPTY0004: attribute-marker must be a single string");
                    o.AttributeMarker = Str(value);
                    break;
                case "content-key":
                    if (!IsSingleString(value))
                        throw new InvalidOperationException("XPTY0004: content-key must be a single string");
                    o.ContentKey = Str(value);
                    break;
                case "liberal":
                    if (value.IsAtomic && value.Kind == XdmValueKind.Boolean)
                        o.Liberal = value.ToString() == "true";
                    else
                        throw new InvalidOperationException("XPTY0004: liberal must be a single boolean");
                    break;
                case "plan":
                    o.Plan = ParsePlan(value);
                    break;
                default:
                    throw new InvalidOperationException($"XPTY0004: unknown option '{Str(keyValue)}'.");
            }
        }
        return o;
    }

    private static Dictionary<string, PlanEntry> ParsePlan(XdmValue value)
    {
        if (!value.IsMap)
            throw new InvalidOperationException("XPTY0004: plan must be a map");
        var result = new Dictionary<string, PlanEntry>(StringComparer.Ordinal);
        var map = value.MapValue;
        foreach (var keyValue in map.Keys)
        {
            if (!map.TryGetValue(keyValue, out var entryValue))
                continue;
            if (!IsSingleString(keyValue))
                throw new InvalidOperationException("XPTY0004: plan keys must be strings");
            // Entries whose keys are not in the plan-key format are ignored
            // (F&O 14.6.5, element-to-map-597).
            if (!IsValidPlanKey(Str(keyValue)))
                continue;
            if (!entryValue.IsMap)
                throw new InvalidOperationException("XPTY0004: plan entries must be maps");
            var entry = new PlanEntry();
            var entryMap = entryValue.MapValue;
            foreach (var ek in entryMap.Keys)
            {
                if (!entryMap.TryGetValue(ek, out var ev))
                    continue;
                switch (Str(ek))
                {
                    case "layout":
                        if (!IsSingleString(ev) || !s_layouts.TryGetValue(Str(ev), out var layout))
                            throw new InvalidOperationException("XPTY0004: plan layout must be a valid layout name");
                        entry.Layout = layout;
                        break;
                    case "child":
                        if (IsEmpty(ev))
                            break;   // () = absent
                        if (!IsSingleString(ev) || !IsValidChildName(Str(ev)))
                            throw new InvalidOperationException("XPTY0004: plan child must be a valid element name");
                        entry.Child = Str(ev);
                        break;
                    case "type":
                        if (IsEmpty(ev))
                        {
                            entry.Type = "";
                        }
                        else if (IsSingleString(ev) && Array.IndexOf(s_validTypes, Str(ev)) >= 0)
                        {
                            entry.Type = Str(ev);
                        }
                        else
                        {
                            throw new InvalidOperationException("XPTY0004: plan type must be a valid type name");
                        }
                        break;
                    default:
                        throw new InvalidOperationException($"XPTY0004: unknown plan entry key '{Str(ek)}'.");
                }
            }
            result[Str(keyValue)] = entry;
        }
        return result;
    }

    /// <summary>Child names in plans are EQNames or local names, never lexical (element-to-map-598).</summary>
    private static bool IsValidChildName(string name)
    {
        if (name.StartsWith("Q{", StringComparison.Ordinal))
        {
            int close = name.IndexOf('}');
            return close >= 2 && close < name.Length - 1 && IsNcName(name[(close + 1)..]);
        }
        return IsNcName(name);
    }

    /// <summary>
    /// Plan-key format of F&O 14.6.5: <c>local</c>, <c>Q{uri}local</c>, <c>*</c>,
    /// <c>@local</c> or <c>@Q{uri}local</c>.
    /// </summary>
    private static bool IsValidPlanKey(string key)
    {
        if (key == "*")
            return true;
        string body = key.StartsWith('@') ? key[1..] : key;
        if (body.StartsWith("Q{", StringComparison.Ordinal))
        {
            int close = body.IndexOf('}');
            return close >= 2 && close < body.Length - 1 && IsNcName(body[(close + 1)..]);
        }
        return IsNcName(body);
    }

    /// <summary>Validates a plan entry on first use (entries for unmatched names are ignored).</summary>
    private static void ValidateOnUse(PlanEntry entry, string key)
    {
        if (entry.Validated)
            return;
        entry.Validated = true;
        bool isAttribute = key.StartsWith('@');
        if (isAttribute && entry.Layout.HasValue)
            throw new InvalidOperationException("XPTY0004: attribute plan entries cannot have a layout");
        if (entry.Layout is Layout.List or Layout.ListPlus && entry.Child == null)
            throw new InvalidOperationException("XPTY0004: list layout requires a child name");
        if (entry.Type is { Length: > 0 } && entry.Layout.HasValue &&
            entry.Layout is not (Layout.Simple or Layout.SimplePlus))
            throw new InvalidOperationException("XPTY0004: a type is only allowed on a simple layout");
    }

    private static PlanEntry? LookupPlan(EmOptions o, params string?[] keys)
        => LookupPlanCore(o, true, keys);

    /// <summary>Looks up a specific entry only; the '*' fallback rule is not consulted.</summary>
    private static PlanEntry? LookupPlanExact(EmOptions o, params string?[] keys)
        => LookupPlanCore(o, false, keys);

    private static PlanEntry? LookupPlanCore(EmOptions o, bool allowWildcard, params string?[] keys)
    {
        if (o.Plan == null)
            return null;
        foreach (var key in keys)
        {
            if (key != null && o.Plan.TryGetValue(key, out var entry))
            {
                ValidateOnUse(entry, key);
                return entry;
            }
        }
        if (allowWildcard && o.Plan.TryGetValue("*", out var wild))
        {
            ValidateOnUse(wild, "*");
            return wild;
        }
        return null;
    }

    // =====================================================================================
    // Name formatting and parsing
    // =====================================================================================

    /// <summary>
    /// Formats an element name for a map key. The default format abbreviates a child to a
    /// bare local name only when it is in the same namespace as its parent (an absent
    /// parent namespace counts as no namespace); every other namespaced element uses
    /// Q{uri}local, including Q{} for a no-namespace child of a namespaced parent
    /// (element-to-map-012/013/207/208).
    /// </summary>
    internal static string FormatElementName(IXdmNode el, string? parentNs, EmOptions o)
        => o.NameFormat switch
        {
            "local" => el.LocalName,
            "lexical" => el.Prefix.Length > 0 ? el.Prefix + ":" + el.LocalName : el.LocalName,
            // eqname: every name in a namespace keeps its Q{uri}local form, never
            // abbreviated by the parent relationship (element-to-map-590).
            "eqname" => el.NamespaceUri.Length == 0
                ? el.LocalName
                : "Q{" + el.NamespaceUri + "}" + el.LocalName,
            _ => el.NamespaceUri == (parentNs ?? string.Empty)
                ? el.LocalName
                : "Q{" + el.NamespaceUri + "}" + el.LocalName,
        };

    /// <summary>Formats an attribute name (with the attribute marker) for a map key.</summary>
    internal static string FormatAttributeName(IXdmNode attr, EmOptions o)
    {
        string n = o.NameFormat switch
        {
            "local" => attr.LocalName,
            "lexical" => attr.Prefix.Length > 0 ? attr.Prefix + ":" + attr.LocalName : attr.LocalName,
            _ => attr.NamespaceUri == XmlNamespace ? "xml:" + attr.LocalName
                : attr.NamespaceUri.Length == 0 ? attr.LocalName
                : "Q{" + attr.NamespaceUri + "}" + attr.LocalName,
        };
        return o.AttributeMarker + n;
    }

    /// <summary>The absolute EQName of an element/attribute (never parent-abbreviated).</summary>
    internal static string AbsoluteName(IXdmNode node)
        => node.NamespaceUri.Length == 0 ? node.LocalName : "Q{" + node.NamespaceUri + "}" + node.LocalName;

    /// <summary>
    /// NCName validation approximating XML 1.1 / XSD (the 𝄞-class astral characters are
    /// accepted, which XmlConvert.VerifyNCName rejects — map-to-element-053).
    /// </summary>
    private static bool IsNcName(string name)
    {
        if (name.Length == 0)
            return false;
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            if (char.IsSurrogatePair(name, i))
            {
                // Astral characters are valid name characters (surrogate pair).
                i++;
                continue;
            }
            if (!IsNameChar(c, i == 0))
                return false;
        }
        return true;
    }

    private static bool IsNameChar(char c, bool isStart)
    {
        if (c == '_' || c == '-')
            return !isStart || c == '_';
        if (c == '.')
            return !isStart;
        var cat = char.GetUnicodeCategory(c);
        return cat switch
        {
            System.Globalization.UnicodeCategory.UppercaseLetter or
            System.Globalization.UnicodeCategory.LowercaseLetter or
            System.Globalization.UnicodeCategory.TitlecaseLetter or
            System.Globalization.UnicodeCategory.ModifierLetter or
            System.Globalization.UnicodeCategory.OtherLetter or
            System.Globalization.UnicodeCategory.NonSpacingMark or
            System.Globalization.UnicodeCategory.SpacingCombiningMark or
            System.Globalization.UnicodeCategory.DecimalDigitNumber or
            System.Globalization.UnicodeCategory.ConnectorPunctuation or
            System.Globalization.UnicodeCategory.LetterNumber => !isStart || cat is not
                (System.Globalization.UnicodeCategory.NonSpacingMark or
                 System.Globalization.UnicodeCategory.SpacingCombiningMark or
                 System.Globalization.UnicodeCategory.DecimalDigitNumber or
                 System.Globalization.UnicodeCategory.ConnectorPunctuation),
            _ => false,
        };
    }

    /// <summary>Result of parsing a map key as an XML name.</summary>
    internal readonly record struct ParsedName(string Uri, string Local, string Prefix);

    /// <summary>
    /// Parses a map key as an element or attribute name per the name-format option;
    /// FOJS0009 when the name is invalid or a lexical prefix is not in scope. Under the
    /// default name format a bare local name yields the namespace of the parent element
    /// (nothing at the top level), the inverse of <see cref="FormatElementName"/>
    /// (map-to-element-046/110).
    /// </summary>
    internal static ParsedName ParseName(string key, bool isAttribute, EmOptions o, EvaluationContext ctx, string? parentNs = null)
    {
        if (key.Length == 0)
            throw new InvalidOperationException("FOJS0009: map key is not a valid XML name");
        // The Q{uri}local form is absolute and accepted in every name format
        // (map-to-element-048); so is the predefined xml prefix (map-to-element-050).
        if (key.StartsWith("Q{", StringComparison.Ordinal))
        {
            int qclose = key.IndexOf('}');
            if (qclose < 0 || qclose == key.Length - 1 || !IsNcName(key[(qclose + 1)..]))
                FailName(key);
            return new ParsedName(key[2..qclose], key[(qclose + 1)..], string.Empty);
        }
        if (key.StartsWith("xml:", StringComparison.Ordinal) && IsNcName(key[4..]))
            return new ParsedName(XmlNamespace, key[4..], "xml");
        if (o.NameFormat == "local")
        {
            if (!IsNcName(key))
                FailName(key);
            return new ParsedName(string.Empty, key, string.Empty);
        }
        if (o.NameFormat == "lexical")
        {
            int colon = key.IndexOf(':');
            if (colon < 0)
            {
                if (!IsNcName(key))
                    FailName(key);
                string dns = isAttribute ? string.Empty : (ctx.DefaultElementNamespace ?? string.Empty);
                return new ParsedName(dns, key, string.Empty);
            }
            if (colon == 0 || colon != key.LastIndexOf(':') || colon == key.Length - 1)
                FailName(key);
            string prefix = key[..colon], local = key[(colon + 1)..];
            if (prefix == "xmlns" || !IsNcName(prefix) || !IsNcName(local))
                FailName(key);
            if (!ctx.TryResolveNamespace(prefix, out var uri))
                throw new InvalidOperationException($"FOJS0009: namespace prefix '{prefix}' is not in scope");
            return new ParsedName(uri, local, prefix);
        }
        // eqname (and default): a bare local name.
        if (!IsNcName(key))
            FailName(key);
        if (!isAttribute && o.NameFormat == "default" && parentNs is { Length: > 0 })
            return new ParsedName(parentNs, key, string.Empty);
        return new ParsedName(string.Empty, key, string.Empty);
    }

    private static void FailName(string key)
        => throw new InvalidOperationException($"FOJS0009: map key '{key}' is not a valid XML name");

    // =====================================================================================
    // Layout inference
    // =====================================================================================

    /// <summary>The element content facts used by layout inference.</summary>
    internal readonly record struct ContentFacts(
        List<IXdmNode> Attrs, List<IXdmNode> Children, bool HasText, bool HasNonWsText);

    internal static ContentFacts Analyze(IXdmNode el)
    {
        var attrs = new List<IXdmNode>();
        foreach (var item in el.Attributes())
        {
            var a = item.NodeValue;
            // xsi:type / xsi:nil never appear in the map representation; xmlns
            // declarations are not attributes in XDM either. XDocument surfaces the
            // default namespace declaration as an attribute named "xmlns" in no
            // namespace and prefixed declarations in the xmlns namespace.
            if (a.NamespaceUri == XsiNamespace && (a.LocalName == "type" || a.LocalName == "nil"))
                continue;
            if (a.NamespaceUri == "http://www.w3.org/2000/xmlns/")
                continue;
            if (a.NamespaceUri.Length == 0 && a.LocalName == "xmlns")
                continue;
            attrs.Add(a);
        }
        var children = new List<IXdmNode>();
        var texts = new List<IXdmNode>();
        foreach (var item in el.Children())
        {
            var c = item.NodeValue;
            if (c.NodeKind == XdmNodeKind.Element)
                children.Add(c);
            else if (c.NodeKind == XdmNodeKind.Text)
                texts.Add(c);
        }
        bool hasText = texts.Count > 0;
        bool hasNonWs = false;
        foreach (var t in texts)
        {
            if (t.StringValue.Trim().Length > 0) { hasNonWs = true; break; }
        }
        return new ContentFacts(attrs, children, hasText, hasNonWs);
    }

    /// <summary>Infers the §17.6 layout of an untyped element from its content.</summary>
    internal static Layout InferLayout(in ContentFacts f)
    {
        if (f.Children.Count == 0)
        {
            if (f.Attrs.Count == 0)
                return f.HasText ? Layout.Simple : Layout.Empty;
            return f.HasText ? Layout.SimplePlus : Layout.EmptyPlus;
        }
        if (f.HasNonWsText)
            return Layout.Mixed;
        bool allSame = true, allDistinct = true;
        var seen = new HashSet<(string, string)>();
        var first = (f.Children[0].NamespaceUri, f.Children[0].LocalName);
        foreach (var c in f.Children)
        {
            if (c.NamespaceUri != first.Item1 || c.LocalName != first.Item2)
                allSame = false;
            if (!seen.Add((c.NamespaceUri, c.LocalName)))
                allDistinct = false;
        }
        if (f.Attrs.Count == 0)
        {
            if (allSame)
                return f.Children.Count == 1 ? Layout.Record : Layout.List;
            if (allDistinct)
                return Layout.Record;
            return Layout.Sequence;
        }
        if (allSame)
            return f.Children.Count == 1 ? Layout.Record : Layout.ListPlus;
        if (allDistinct)
            return Layout.Record;
        return Layout.Sequence;
    }

    // =====================================================================================
    // PR2688 untyped value type inference
    // =====================================================================================

    /// <summary>Accumulates raw string values per map key for the PR2688 type inference.</summary>
    internal sealed class Inference
    {
        public readonly Dictionary<string, List<string>> Values = new(StringComparer.Ordinal);

        public void Add(string key, string raw)
        {
            if (!Values.TryGetValue(key, out var list))
                Values[key] = list = new List<string>();
            list.Add(raw);
        }

        /// <summary>Returns 'boolean'/'integer'/'decimal'/'double' or null (untyped).</summary>
        public string? TypeOf(string key)
            => Values.TryGetValue(key, out var list) ? InferType(list) : null;
    }

    private static string? InferType(List<string> values)
    {
        if (values.Count == 0)
            return null;
        bool hasTrueFalse = false, allBool = true;
        bool allNumeric = true, hasDec = false, hasExp = false, leadingZero = false;
        foreach (var raw in values)
        {
            string v = raw.Trim();
            if (v.Length == 0)
                return null;
            if (v is "true" or "false")
                hasTrueFalse = true;
            else if (v is not ("0" or "1"))
                allBool = false;
            if (s_integerRe.IsMatch(v))
            {
                if (v.TrimStart('+', '-').Length > 1 && v.TrimStart('+', '-').StartsWith('0'))
                    leadingZero = true;
            }
            else if (s_decimalRe.IsMatch(v))
            {
                hasDec = true;
            }
            else if (s_doubleRe.IsMatch(v))
            {
                hasExp = true;
            }
            else
            {
                allNumeric = false;
            }
        }
        if (hasTrueFalse && allBool)
            return "boolean";
        // Numeric subtype promotion: a mix of integers and decimals infers decimal,
        // any exponent form infers double (element-to-map-552/553).
        if (allNumeric)
        {
            if (hasExp)
                return "double";
            if (hasDec)
                return "decimal";
            return leadingZero ? null : "integer";
        }
        return null;
    }

    // =====================================================================================
    // fn:element-to-map
    // =====================================================================================

    /// <summary>Converts a single element (or document's element) to its map representation.</summary>
    internal static XdmValue ElementToMap(XdmValue input, EmOptions o)
    {
        if (IsEmpty(input))
            return XdmValue.Undefined;
        var items = new List<XdmValue>();
        foreach (var item in AsSequence(input))
            items.Add(item);
        if (items.Count != 1 || !items[0].IsNode)
            throw new InvalidOperationException("XPTY0004: fn:element-to-map input must be a single element or document node");
        var node = items[0].NodeValue;
        var el = node.NodeKind switch
        {
            XdmNodeKind.Element => node,
            XdmNodeKind.Document => FirstElementChild(node),
            _ => throw new InvalidOperationException("XPTY0004: fn:element-to-map input must be an element or document node"),
        };
        if (el == null)
            return XdmValue.Undefined;

        var inference = new Inference();
        VisitForInference(el, null, o, inference);
        var conv = new Converter(o, inference);
        var result = new XdmMap();
        var value = conv.ElementValue(el, null);
        if (!value.HasValue)
            return XdmValue.Undefined;   // the root element was deep-skipped (element-to-map-513)
        result.Add(XdmValue.FromString(FormatElementName(el, null, o)), value.Value);
        return XdmValue.FromMap(result);
    }

    private static IXdmNode? FirstElementChild(IXdmNode node)
    {
        foreach (var item in node.Children())
        {
            var c = item.NodeValue;
            if (c.NodeKind == XdmNodeKind.Element)
                return c;
        }
        return null;
    }

    /// <summary>Pre-pass collecting raw values per key, driven by the same layout decisions.</summary>
    private static void VisitForInference(IXdmNode el, string? parentNs, EmOptions o, Inference inf)
    {
        var f = Analyze(el);
        string key = FormatElementName(el, parentNs, o);
        var entry = LookupPlan(o, AbsoluteName(el), el.Prefix.Length > 0 ? el.Prefix + ":" + el.LocalName : null, el.LocalName);
        var layout = entry?.Layout ?? InferLayout(f);
        switch (layout)
        {
            case Layout.Simple:
                inf.Add(key, el.StringValue);
                break;
            case Layout.SimplePlus:
                inf.Add(ContentKeyFor(f.Attrs, o), el.StringValue);
                break;
            case Layout.Record:
            case Layout.Sequence:
            case Layout.Mixed:
            case Layout.List:
            case Layout.ListPlus:
                foreach (var c in f.Children)
                    VisitForInference(c, el.NamespaceUri, o, inf);
                break;
        }
    }

    /// <summary>The content key, '#' -prefixed when it collides with an attribute key.</summary>
    private static string ContentKeyFor(List<IXdmNode> attrs, EmOptions o)
    {
        foreach (var a in attrs)
        {
            if (FormatAttributeName(a, o) == o.ContentKey)
                return "#" + o.ContentKey;
        }
        return o.ContentKey;
    }

    /// <summary>Converts elements to map values; null signals a deep-skip drop.</summary>
    private sealed class Converter
    {
        private readonly EmOptions _o;
        private readonly Inference _inf;

        public Converter(EmOptions o, Inference inf) { _o = o; _inf = inf; }

        public XdmValue? ElementValue(IXdmNode el, string? parentNs)
        {
            var f = Analyze(el);
            string key = FormatElementName(el, parentNs, _o);
            var exact = LookupPlanExact(_o, AbsoluteName(el),
                el.Prefix.Length > 0 ? el.Prefix + ":" + el.LocalName : null, el.LocalName);
            var entry = exact ?? LookupPlan(_o, "*");
            if (entry?.Layout == Layout.Xml)
                return Untyped(el.ToXmlString());
            if (entry?.Layout == Layout.DeepSkip)
                return null;
            if (entry?.Layout == Layout.Error)
                throw new InvalidOperationException("FOJS0008: input inconsistent with plan layout");

            var layout = entry?.Layout ?? InferLayout(f);
            try
            {
                return ByLayout(el, key, f, layout, entry);
            }
            catch (InconsistentLayoutException) when (exact != null)
            {
                // The exact entry failed; retry with the '*' fallback rule
                // (element-to-map-401/511). Without a usable fallback the error is
                // unrecoverable (element-to-map-420/421).
                var wild = LookupPlan(_o, "*");
                if (wild == null || ReferenceEquals(wild, exact) || wild.Layout == Layout.Error)
                    throw new InvalidOperationException("FOJS0008: input inconsistent with plan layout");
                if (wild.Layout == Layout.Xml)
                    return Untyped(el.ToXmlString());
                if (wild.Layout == Layout.DeepSkip)
                    return null;
                return ByLayout(el, key, f, wild.Layout.Value, wild);
            }
            catch (InconsistentLayoutException)
            {
                throw new InvalidOperationException("FOJS0008: input inconsistent with plan layout");
            }
        }

        private XdmValue ByLayout(IXdmNode el, string key, in ContentFacts f, Layout layout,
            PlanEntry? entry)
        {
            switch (layout)
            {
                case Layout.Empty:
                    if (f.Children.Count > 0 || f.HasNonWsText)
                        throw new InconsistentLayoutException();
                    return Typed(key, AbsoluteName(el), string.Empty);
                case Layout.Simple:
                    if (f.Children.Count > 0)
                        throw new InconsistentLayoutException();
                    return Typed(key, AbsoluteName(el), el.StringValue);
                case Layout.EmptyPlus:
                    if (f.Children.Count > 0 || f.HasNonWsText)
                        throw new InconsistentLayoutException();
                    return XdmValue.FromMap(AttrsMap(f.Attrs));
                case Layout.SimplePlus:
                    if (f.Children.Count > 0)
                        throw new InconsistentLayoutException();
                    {
                        var map = AttrsMap(f.Attrs);
                        Add(map, ContentKeyFor(f.Attrs, _o), Typed(ContentKeyFor(f.Attrs, _o), AbsoluteName(el), el.StringValue));
                        return XdmValue.FromMap(map);
                    }
                case Layout.List:
                    if (f.HasNonWsText)
                        throw new InconsistentLayoutException();
                    ValidateListChildren(f, entry);
                    return XdmValue.FromArray(ChildArray(el, f));
                case Layout.ListPlus:
                    if (f.HasNonWsText)
                        throw new InconsistentLayoutException();
                    ValidateListChildren(f, entry);
                    {
                        var map = AttrsMap(f.Attrs);
                        if (f.Children.Count > 0)
                            Add(map, FormatElementName(f.Children[0], el.NamespaceUri, _o), XdmValue.FromArray(ChildArray(el, f)));
                        else if (entry?.Child != null)
                            // With a plan-supplied child name the content property is
                            // present even when empty (element-to-map-504).
                            Add(map, ChildKey(entry.Child, el.NamespaceUri), XdmValue.FromArray(new XdmArray()));
                        return XdmValue.FromMap(map);
                    }
                case Layout.Record:
                    if (f.HasNonWsText)
                        throw new InconsistentLayoutException();
                    {
                        var map = AttrsMap(f.Attrs);
                        var groups = new Dictionary<string, List<XdmValue>>(StringComparer.Ordinal);
                        var order = new List<string>();
                        foreach (var c in f.Children)
                        {
                            var v = ElementValue(c, el.NamespaceUri);
                            if (!v.HasValue)
                                continue;
                            string ck = FormatElementName(c, el.NamespaceUri, _o);
                            if (!groups.TryGetValue(ck, out var list))
                                groups[ck] = list = new List<XdmValue>();
                            if (list.Count == 0)
                                order.Add(ck);
                            list.Add(v.Value);
                        }
                        foreach (var ck in order)
                        {
                            var list = groups[ck];
                            Add(map, ck, list.Count == 1 ? list[0] : XdmValue.FromArray(new XdmArray(list)));
                        }
                        return XdmValue.FromMap(map);
                    }
                case Layout.Sequence:
                    if (f.HasNonWsText)
                        throw new InconsistentLayoutException();
                    return SequenceArray(el, f);
                case Layout.Mixed:
                    return MixedArray(f.Attrs, el);
                default:
                    throw new InconsistentLayoutException();
            }
        }

        /// <summary>
        /// A forced list layout requires every child to match the plan's child name
        /// (F&O 14.6.1.5/14.6.1.6 errors; element-to-map-420). Child attributes are
        /// discarded in both list layouts.
        /// </summary>
        private void ValidateListChildren(in ContentFacts f, PlanEntry? entry)
        {
            if (f.Children.Count == 0 || entry?.Child == null)
                return;
            string? childUri = null;
            string childLocal;
            if (entry.Child.StartsWith("Q{", StringComparison.Ordinal))
            {
                int close = entry.Child.IndexOf('}');
                childUri = entry.Child[2..close];
                childLocal = entry.Child[(close + 1)..];
            }
            else
            {
                childLocal = entry.Child;
            }
            foreach (var c in f.Children)
            {
                if (c.LocalName != childLocal || c.NamespaceUri != (childUri ?? string.Empty))
                    throw new InconsistentLayoutException();
            }
        }

        private XdmArray ChildArray(IXdmNode el, in ContentFacts f)
        {
            var arr = new XdmArray();
            foreach (var c in f.Children)
            {
                var v = ElementValue(c, el.NamespaceUri);
                if (v.HasValue)
                    arr.Add(v.Value);
            }
            return arr;
        }

        /// <summary>Formats a plan child name relative to the parent namespace.</summary>
        private static string ChildKey(string child, string parentNs)
        {
            if (child.StartsWith("Q{", StringComparison.Ordinal))
            {
                int close = child.IndexOf('}');
                return child[2..close] == parentNs ? child[(close + 1)..] : child;
            }
            return child;
        }

        private XdmValue SequenceArray(IXdmNode el, in ContentFacts f)
        {
            var items = new List<XdmValue>();
            // Each attribute is represented by its own single-entry map (F&O 14.6.1.9;
            // element-to-map-593).
            foreach (var a in f.Attrs)
                items.Add(SingletonMap(FormatAttributeName(a, _o), AttrValue(a)));
            foreach (var childItem in el.Children())
            {
                var c = childItem.NodeValue;
                switch (c.NodeKind)
                {
                    case XdmNodeKind.Element:
                        items.Add(SingletonMap(FormatElementName(c, el.NamespaceUri, _o),
                            ElementValue(c, el.NamespaceUri) ?? XdmValue.Undefined));
                        break;
                    case XdmNodeKind.Comment:
                        items.Add(SingletonMap("#comment", Untyped(c.StringValue)));
                        break;
                    case XdmNodeKind.ProcessingInstruction:
                        {
                            var pi = new XdmMap();
                            Add(pi, "#target", Untyped(c.LocalName));
                            Add(pi, "#data", Untyped(c.StringValue));
                            items.Add(SingletonMap("#processing-instruction", XdmValue.FromMap(pi)));
                            break;
                        }
                }
            }
            return XdmValue.FromArray(new XdmArray(items));
        }

        private XdmValue MixedArray(List<IXdmNode> attrs, IXdmNode el)
        {
            var items = new List<XdmValue>();
            foreach (var a in attrs)
                items.Add(SingletonMap(FormatAttributeName(a, _o), AttrValue(a)));
            foreach (var childItem in el.Children())
            {
                var c = childItem.NodeValue;
                switch (c.NodeKind)
                {
                    case XdmNodeKind.Text:
                        items.Add(Untyped(c.StringValue));
                        break;
                    case XdmNodeKind.Element:
                        items.Add(SingletonMap(FormatElementName(c, el.NamespaceUri, _o), ElementValue(c, el.NamespaceUri) ?? XdmValue.Undefined));
                        break;
                    case XdmNodeKind.Comment:
                        items.Add(SingletonMap("#comment", Untyped(c.StringValue)));
                        break;
                    case XdmNodeKind.ProcessingInstruction:
                        {
                            var pi = new XdmMap();
                            Add(pi, "#target", Untyped(c.LocalName));
                            Add(pi, "#data", Untyped(c.StringValue));
                            items.Add(SingletonMap("#processing-instruction", XdmValue.FromMap(pi)));
                            break;
                        }
                }
            }
            return XdmValue.FromArray(new XdmArray(items));
        }

        private XdmMap AttrsMap(List<IXdmNode> attrs)
        {
            var map = new XdmMap();
            foreach (var a in attrs)
                Add(map, FormatAttributeName(a, _o), AttrValue(a));
            return map;
        }

        private XdmValue AttrValue(IXdmNode a)
        {
            // Attribute values are typed only by a plan entry (element-to-map-210/550).
            string? t = PlanType("@" + AbsoluteName(a));
            return ConvertByType(a.StringValue, t is { Length: > 0 } ? t : null, _o.Liberal);
        }

        private XdmValue Typed(string outKey, string absKey, string raw)
        {
            string? t = PlanType(absKey) ?? _inf.TypeOf(outKey);
            return ConvertByType(raw, t, _o.Liberal);
        }

        private string? PlanType(string absKey)
        {
            var entry = LookupPlan(_o, absKey);
            return entry?.Type is { Length: > 0 } ? entry.Type : null;
        }
    }

    internal static XdmValue ConvertByType(string s, string? t, bool liberal)
    {
        if (t == null || t is "string" or "skip")
            return Untyped(s);
        if (s.Length == 0)
            return Untyped(string.Empty);            // element-to-map-569/581
        if (s.Trim().Length == 0)
            return Untyped(s);                       // element-to-map-570: whitespace preserved
        string v = s.Trim();
        try
        {
            switch (t)
            {
                case "integer":
                    if (long.TryParse(v, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var l))
                        return XdmValue.FromInteger(l);
                    break;
                case "decimal":
                    if (decimal.TryParse(v, NumberStyles.Number, CultureInfo.InvariantCulture, out var d))
                        return XdmValue.FromDecimal(d);
                    break;
                case "double":
                    return XdmValue.FromDouble(XmlConvert.ToDouble(v));
                case "boolean":
                    if (v is "true" or "1")
                        return XdmValue.True;
                    if (v is "false" or "0")
                        return XdmValue.False;
                    break;
            }
        }
        catch (FormatException) { }
        catch (OverflowException) { }
        if (liberal)
            return Untyped(s);
        throw new InvalidOperationException($"FOJS0010: value '{s}' cannot be converted to the prescribed type '{t}'");
    }

    // =====================================================================================
    // fn:element-to-map-plan
    // =====================================================================================

    /// <summary>
    /// Union of the content facts of all elements sharing one name across the corpus
    /// (F&O 14.6.2 decides the layout from $EE, the union of same-named elements).
    /// </summary>
    private sealed class UnionFacts
    {
        public int AttrCount;
        public int ChildCount;
        public int TextCount;
        public bool HasNonWsText;
        public bool HasMultiChildInstance;          // some instance has two or more children
        public bool AllChildrenSameName = true;     // across the whole union
        public bool AllInstancesDistinct = true;    // within every single instance
        public string? Child;                       // absolute name of the first child seen
    }

    /// <summary>Builds the merged conversion plan of one or more input trees.</summary>
    internal static XdmValue ElementToMapPlan(XdmValue input)
    {
        var groups = new Dictionary<string, UnionFacts>(StringComparer.Ordinal);
        var attrValues = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var inf = new Inference();

        foreach (var item in AsSequence(input))
        {
            if (!item.IsNode)
                throw new InvalidOperationException("XPTY0004: fn:element-to-map-plan input must be nodes");
            var node = item.NodeValue;
            var el = node.NodeKind switch
            {
                XdmNodeKind.Element => node,
                XdmNodeKind.Document => FirstElementChild(node),
                _ => throw new InvalidOperationException("XPTY0004: fn:element-to-map-plan input must be elements or document nodes"),
            };
            if (el != null)
                PlanVisit(el, groups, attrValues, inf);
        }

        var result = new XdmMap();
        foreach (var (name, g) in groups)
        {
            var layout = UnionLayout(g);
            var entry = new XdmMap();
            Add(entry, "layout", Untyped(LayoutName(layout)));
            string? child = layout is Layout.List or Layout.ListPlus ? g.Child : null;
            Add(entry, "child", child != null ? Untyped(child) : XdmValue.Undefined);
            string? t = layout is Layout.Simple or Layout.SimplePlus ? inf.TypeOf(name) : null;
            Add(entry, "type", t != null ? Untyped(t) : XdmValue.Undefined);
            Add(result, name, XdmValue.FromMap(entry));
        }
        foreach (var (name, values) in attrValues)
        {
            string? t = InferType(values);
            if (t == null)
                continue;
            var entry = new XdmMap();
            Add(entry, "type", Untyped(t));
            Add(result, name, XdmValue.FromMap(entry));
        }
        return XdmValue.FromMap(result);
    }

    private static Layout UnionLayout(UnionFacts g)
    {
        if (g.ChildCount == 0 && g.TextCount == 0)
            return g.AttrCount == 0 ? Layout.Empty : Layout.EmptyPlus;
        if (g.ChildCount == 0)
            return g.AttrCount == 0 ? Layout.Simple : Layout.SimplePlus;
        if (g.HasNonWsText)
            return Layout.Mixed;
        if (g.AllChildrenSameName && g.HasMultiChildInstance)
            return g.AttrCount == 0 ? Layout.List : Layout.ListPlus;
        if (g.AllInstancesDistinct)
            return Layout.Record;
        return Layout.Sequence;
    }

    private static void PlanVisit(IXdmNode el, Dictionary<string, UnionFacts> groups,
        Dictionary<string, List<string>> attrValues, Inference inf)
    {
        var f = Analyze(el);
        string abs = AbsoluteName(el);
        if (!groups.TryGetValue(abs, out var g))
            groups[abs] = g = new UnionFacts();
        g.AttrCount += f.Attrs.Count;
        g.ChildCount += f.Children.Count;
        if (f.HasText)
            g.TextCount++;
        if (f.HasNonWsText)
            g.HasNonWsText = true;
        if (f.Children.Count >= 2)
            g.HasMultiChildInstance = true;
        var seen = new HashSet<(string Ns, string Local)>();
        bool distinct = true;
        foreach (var c in f.Children)
        {
            g.Child ??= AbsoluteName(c);
            if (g.Child != AbsoluteName(c))
                g.AllChildrenSameName = false;
            if (!seen.Add((c.NamespaceUri, c.LocalName)))
                distinct = false;
        }
        if (!distinct)
            g.AllInstancesDistinct = false;
        PlanAttrs(f.Attrs, attrValues);
        inf.Add(abs, el.StringValue);
        foreach (var c in f.Children)
            PlanVisit(c, groups, attrValues, inf);
    }

    private static void PlanAttrs(List<IXdmNode> attrs, Dictionary<string, List<string>> attrValues)
    {
        foreach (var a in attrs)
        {
            string key = "@" + AbsoluteName(a);
            if (!attrValues.TryGetValue(key, out var list))
                attrValues[key] = list = new List<string>();
            list.Add(a.StringValue);
        }
    }

    private static string LayoutName(Layout l) => l switch
    {
        Layout.Empty => "empty",
        Layout.EmptyPlus => "empty-plus",
        Layout.Simple => "simple",
        Layout.SimplePlus => "simple-plus",
        Layout.List => "list",
        Layout.ListPlus => "list-plus",
        Layout.Record => "record",
        Layout.Sequence => "sequence",
        Layout.Mixed => "mixed",
        Layout.Xml => "xml",
        Layout.DeepSkip => "deep-skip",
        _ => "error",
    };

    // =====================================================================================
    // fn:map-to-element
    // =====================================================================================

    /// <summary>Converts a single-entry map to a parentless element; FOJS0009 on bad maps.</summary>
    internal static XdmValue MapToElement(XdmValue input, EmOptions o, EvaluationContext ctx)
    {
        if (IsEmpty(input))
            return XdmValue.Undefined;
        if (!input.IsMap)
            throw new InvalidOperationException("XPTY0004: fn:map-to-element input must be a map");
        var map = input.MapValue;
        var keys = map.Keys.ToList();
        if (keys.Count == 0 || keys.Count > 1)
            throw new InvalidOperationException("FOJS0009: map must have exactly one entry");

        var keyValue = keys[0];
        if (keyValue.Kind != XdmValueKind.String)
            throw new InvalidOperationException("FOJS0009: map key must be a string");
        string key = keyValue.ToString();
        if (!map.TryGetValue(keyValue, out var value) || IsEmpty(value))
            value = XdmValue.Undefined;

        var name = ParseName(key, isAttribute: false, o, ctx);
        var entry = LookupPlan(o, key,
            name.Uri.Length == 0 ? name.Local : "Q{" + name.Uri + "}" + name.Local,
            name.Prefix.Length > 0 ? name.Prefix + ":" + name.Local : null,
            name.Local);
        if (entry?.Layout is Layout.DeepSkip or Layout.Error)
            entry = null;   // accepted and ignored for map input (map-to-element-133/134)

        var b = new Builder(o, ctx);
        XElement root;
        if (entry?.Layout == Layout.Xml)
        {
            root = ParseSingleElement(value);
        }
        else if (entry?.Layout == Layout.ListPlus)
        {
            root = b.CreateElement(name);
            b.FillListPlusContent(root, value, entry);
        }
        else if (entry?.Layout == Layout.List)
        {
            root = b.CreateElement(name);
            b.FillListContent(root, value, entry);
        }
        else
        {
            root = b.CreateElement(name);
            b.FillContent(root, value, isRoot: true);
        }
        b.DeclareNamespaces(root);
        return XdmValue.FromNode(XDocumentNode.Wrap(root));
    }

    private static XElement ParseSingleElement(XdmValue value)
    {
        if (!IsSingleString(value))
            throw new InvalidOperationException("FOJS0009: xml layout value must be a single string");
        XElement el;
        try
        {
            el = XDocument.Parse(Str(value), LoadOptions.None).Root
                ?? throw new InvalidOperationException("FOJS0009: xml layout value must be a single element");
        }
        catch (System.Xml.XmlException)
        {
            throw new InvalidOperationException("FOJS0009: xml layout value must be well-formed XML with a single element");
        }
        return new XElement(el);   // fresh copy
    }

    /// <summary>Builds parentless elements from map content.</summary>
    private sealed class Builder
    {
        private readonly EmOptions _o;
        private readonly EvaluationContext _ctx;
        private readonly Dictionary<string, string> _decls = new(StringComparer.Ordinal);

        public Builder(EmOptions o, EvaluationContext ctx) { _o = o; _ctx = ctx; }

        public XElement CreateElement(ParsedName name)
        {
            var el = new XElement(XName.Get(name.Local, name.Uri));
            if (name.Uri.Length > 0)
            {
                if (name.Prefix.Length > 0)
                    _decls.TryAdd(name.Prefix, name.Uri);
                else if (_ctx.DefaultElementNamespace == null)
                    _decls.TryAdd(string.Empty, name.Uri);
            }
            return el;
        }

        public void DeclareNamespaces(XElement root)
        {
            foreach (var (prefix, uri) in _decls)
            {
                if (prefix.Length == 0)
                    root.SetAttributeValue("xmlns", uri);
                else
                    root.SetAttributeValue(XNamespace.Xmlns + prefix, uri);
            }
        }

        /// <summary>Plan list layout: each array member becomes one <c>child</c> element.</summary>
        public void FillListContent(XElement parent, XdmValue value, PlanEntry entry)
        {
            if (IsEmpty(value))
                return;
            if (!value.IsArray)
                throw new InvalidOperationException("FOJS0009: list layout requires an array value");
            var childName = ParseName(entry.Child!, isAttribute: false, _o, _ctx);
            foreach (var member in value.ArrayValue.Values)
            {
                if (IsEmpty(member))
                    continue;
                var child = CreateElement(childName);
                if (member.IsAtomic)
                    AddAtomicContent(child, member);
                else if (member.IsMap)
                    FillContent(child, member, isRoot: false);
                else
                    throw new InvalidOperationException("FOJS0009: array members must be atomic values or maps");
                parent.Add(child);
            }
        }

        /// <summary>
        /// Plan list-plus layout: the value is a map whose attribute-marker entries become
        /// attributes and whose remaining entry holds the array of <c>child</c> elements
        /// (map-to-element-118).
        /// </summary>
        public void FillListPlusContent(XElement el, XdmValue value, PlanEntry entry)
        {
            if (IsEmpty(value))
                return;
            if (!value.IsMap)
                throw new InvalidOperationException("FOJS0009: list-plus layout requires a map value");
            foreach (var keyValue in value.MapValue.Keys)
            {
                if (!value.MapValue.TryGetValue(keyValue, out var v) || IsEmpty(v))
                    v = XdmValue.Undefined;
                string key = keyValue.ToString();
                if (key.Length >= _o.AttributeMarker.Length &&
                    key.StartsWith(_o.AttributeMarker, StringComparison.Ordinal) &&
                    key is not ("#comment" or "#processing-instruction"))
                {
                    AddAttribute(el, key[_o.AttributeMarker.Length..], v);
                    continue;
                }
                if (key == _o.ContentKey)
                {
                    // Nilled element: {"#content": #fn:null}
                    if (!IsEmpty(v) && v.Kind == XdmValueKind.QName &&
                        v.QNameValue is { NamespaceUri: FnNamespace, LocalName: "null" })
                        el.SetAttributeValue(XName.Get("nil", XsiNamespace), "true");
                    else if (!IsEmpty(v))
                        AddAtomicContent(el, v);
                    continue;
                }
                if (key == "#comment" || key == "#processing-instruction")
                    throw new InvalidOperationException($"FOJS0009: '{key}' is only allowed inside an array");
                if (!v.IsArray)
                    throw new InvalidOperationException("FOJS0009: list-plus content must be an array value");
                foreach (var member in v.ArrayValue.Values)
                {
                    if (IsEmpty(member))
                        continue;
                    var child = CreateElement(ParseName(key, isAttribute: false, _o, _ctx, el.Name.NamespaceName));
                    if (member.IsAtomic)
                        AddAtomicContent(child, member);
                    else if (member.IsMap)
                        FillContent(child, member, isRoot: false);
                    else
                        throw new InvalidOperationException("FOJS0009: array members must be atomic values or maps");
                    el.Add(child);
                }
            }
        }

        /// <summary>Converts a value into the content of <paramref name="el"/>.</summary>
        public void FillContent(XElement el, XdmValue value, bool isRoot)
        {
            if (IsEmpty(value))
                return;
            if (value.IsAtomic)
            {
                AddAtomicContent(el, value);
                return;
            }
            if (value.IsMap)
            {
                FillFromMap(el, value.MapValue);
                return;
            }
            if (value.IsArray)
            {
                FillFromArray(el, value.ArrayValue, isRoot);
                return;
            }
            throw new InvalidOperationException("FOJS0009: map values must be atomic, maps or arrays");
        }

        private void AddAtomicContent(XElement el, XdmValue value)
        {
            if (value.Kind == XdmValueKind.QName)
            {
                var q = value.QNameValue;
                if (q.NamespaceUri == FnNamespace && q.LocalName == "null")
                {
                    el.SetAttributeValue(XName.Get("nil", XsiNamespace), "true");
                    return;
                }
                el.Add(new XText("Q{" + q.NamespaceUri + "}" + q.LocalName));
                return;
            }
            string s = value.ToString();
            if (s.Length > 0)
                el.Add(new XText(s));
        }

        private void FillFromMap(XElement el, XdmMap map)
        {
            bool hasContent = false, hasElements = false;
            var contentNodes = new List<object>();
            foreach (var keyValue in map.Keys)
            {
                if (!map.TryGetValue(keyValue, out var v) || IsEmpty(v))
                    v = XdmValue.Undefined;
                if (keyValue.Kind != XdmValueKind.String)
                    throw new InvalidOperationException("FOJS0009: map keys must be strings");
                string key = keyValue.ToString();

                if (key == _o.ContentKey)
                {
                    hasContent = true;
                    if (!IsEmpty(v) && !v.IsAtomic)
                        throw new InvalidOperationException("FOJS0009: content must be a single atomic value");
                    if (!IsEmpty(v))
                    {
                        if (v.Kind == XdmValueKind.QName && v.QNameValue is { NamespaceUri: FnNamespace, LocalName: "null" })
                            el.SetAttributeValue(XName.Get("nil", XsiNamespace), "true");
                        else
                            contentNodes.Add(new XText(LexicalOf(v)));
                    }
                    continue;
                }
                if (key.Length >= _o.AttributeMarker.Length &&
                    key.StartsWith(_o.AttributeMarker, StringComparison.Ordinal))
                {
                    if (key == "#comment" || key == "#processing-instruction")
                        throw new InvalidOperationException($"FOJS0009: '{key}' is only allowed inside an array");
                    AddAttribute(el, key[_o.AttributeMarker.Length..], v);
                    continue;
                }
                if (key == "#comment" || key == "#processing-instruction")
                    throw new InvalidOperationException($"FOJS0009: '{key}' is only allowed inside an array");
                hasElements = true;
                AddChildElement(el, key, v);
            }
            if (hasContent && hasElements)
                throw new InvalidOperationException("FOJS0009: content cannot be combined with child elements");
            foreach (var n in contentNodes)
                el.Add(n);
        }

        private void AddAttribute(XElement el, string name, XdmValue v)
        {
            if (name.Length == 0 || name == "xmlns" || name.StartsWith("xmlns:", StringComparison.Ordinal))
                throw new InvalidOperationException($"FOJS0009: '{_o.AttributeMarker}{name}' is not a valid attribute name");
            var parsed = ParseName(name, isAttribute: true, _o, _ctx);
            if (IsEmpty(v))
                return;
            if (!v.IsAtomic)
                throw new InvalidOperationException("FOJS0009: attribute values must be single atomic values");
            if (v.Kind == XdmValueKind.QName)
                throw new InvalidOperationException("FOJS0009: attribute values cannot be QNames");
            // A plan type 'skip' leaves the lexical value untouched (map-to-element-137).
            el.SetAttributeValue(XName.Get(parsed.Local, parsed.Uri), LexicalOf(v));
        }

        private void AddChildElement(XElement parent, string key, XdmValue v)
        {
            var parsed = ParseName(key, isAttribute: false, _o, _ctx, parent.Name.NamespaceName);
            var entry = LookupPlan(_o, key,
                parsed.Uri.Length == 0 ? parsed.Local : "Q{" + parsed.Uri + "}" + parsed.Local,
                parsed.Prefix.Length > 0 ? parsed.Prefix + ":" + parsed.Local : null,
                parsed.Local);
            if (entry?.Layout is Layout.DeepSkip or Layout.Error)
                entry = null;   // accepted and ignored for map input (map-to-element-133/134)

            if (IsEmpty(v))
            {
                parent.Add(CreateElement(parsed));
                return;
            }
            if (entry?.Layout == Layout.Xml)
            {
                parent.Add(ParseSingleElement(v));
                return;
            }
            var el = CreateElement(parsed);
            if (entry?.Layout == Layout.ListPlus)
            {
                FillListPlusContent(el, v, entry);
                parent.Add(el);
                return;
            }
            if (entry?.Layout == Layout.List)
            {
                // A list layout wraps the whole array in ONE element named by the key,
                // with one <child> per member (map-to-element-039/117).
                FillListContent(el, v, entry);
                parent.Add(el);
                return;
            }
            if (v.IsArray)
            {
                // A nested array becomes repeated elements named by the key, one per
                // member (atomic member → text; map member → the map as content)
                // (map-to-element-023/027/029).
                foreach (var m in v.ArrayValue.Values)
                {
                    if (IsEmpty(m))
                        continue;
                    var child = CreateElement(parsed);
                    if (m.IsAtomic)
                        AddAtomicContent(child, m);
                    else if (m.IsMap)
                        FillContent(child, m, isRoot: false);
                    else
                        throw new InvalidOperationException("FOJS0009: array members must be singletons");
                    parent.Add(child);
                }
                return;
            }
            FillContent(el, v, isRoot: false);
            parent.Add(el);
        }

        private void FillFromArray(XElement el, XdmArray array, bool isRoot)
        {
            var members = new List<XdmValue>(array.Values);
            var addedAttrs = new HashSet<string>();
            bool lastWasAtomic = false;
            foreach (var m in members)
            {
                if (IsEmpty(m))
                {
                    lastWasAtomic = false;
                    continue;
                }
                if (m.IsAtomic)
                {
                    // Two adjacent atomic members would merge into one text node (map-to-element-042).
                    if (isRoot && lastWasAtomic)
                        throw new InvalidOperationException("FOJS0009: adjacent atomic values in an array");
                    lastWasAtomic = true;
                    AddAtomicContent(el, m);
                    continue;
                }
                lastWasAtomic = false;
                if (m.IsMap)
                {
                    var map = m.MapValue;
                    bool isAttrMap = false, isContentMap = false, hasElementKey = false;
                    var attrNames = new List<string>();
                    foreach (var kv in map.Keys)
                    {
                        string k = kv.ToString();
                        if (k == _o.ContentKey && map.Keys.Take(2).Count() == 1)
                            isContentMap = true;
                        else if (k.Length >= _o.AttributeMarker.Length &&
                                 k.StartsWith(_o.AttributeMarker, StringComparison.Ordinal) &&
                                 k is not ("#comment" or "#processing-instruction"))
                        {
                            isAttrMap = true;
                            attrNames.Add(k);
                        }
                        else if (k == "#comment" || k == "#processing-instruction")
                            hasElementKey = true;
                        else
                            hasElementKey = true;
                    }
                    if (isRoot && isContentMap)
                    {
                        // {'#content': value} inside a root array supplies the root content
                        // (map-to-element-132).
                        map.TryGetValue(map.Keys.First(), out var cv);
                        if (!IsEmpty(cv))
                        {
                            if (cv.Kind == XdmValueKind.QName && cv.QNameValue is { NamespaceUri: FnNamespace, LocalName: "null" })
                                el.SetAttributeValue(XName.Get("nil", XsiNamespace), "true");
                            else if (!cv.IsAtomic)
                                throw new InvalidOperationException("FOJS0009: content must be a single atomic value");
                            else
                                el.Add(new XText(LexicalOf(cv)));
                        }
                        continue;
                    }
                    if (isRoot && isAttrMap && !hasElementKey)
                    {
                        // Attribute maps inside a root array become root attributes
                        // (map-to-element-030); duplicates are FOJS0009 (map-to-element-089).
                        foreach (var an in attrNames)
                        {
                            if (!addedAttrs.Add(an))
                                throw new InvalidOperationException($"FOJS0009: duplicate attribute '{an}'");
                            map.TryGetValue(XdmValue.FromString(an), out var av);
                            AddAttribute(el, an[_o.AttributeMarker.Length..], av);
                        }
                        continue;
                    }
                    if (IsSingleEntryMap(m))
                    {
                        var (k, v) = SingleEntry(m);
                        if (k == "#comment")
                        {
                            string text = RequireStringValue(v, "comment");
                            if (text.Contains("--") || text.EndsWith('-'))
                                throw new InvalidOperationException("FOJS0009: invalid comment text");
                            el.Add(new XComment(text));
                            continue;
                        }
                        if (k == "#processing-instruction")
                        {
                            el.Add(BuildPi(v));
                            continue;
                        }
                        if (hasElementKey)
                        {
                            var child = CreateElement(ParseName(k, isAttribute: false, _o, _ctx, el.Name.NamespaceName));
                            FillContent(child, v, isRoot: false);
                            el.Add(child);
                            continue;
                        }
                    }
                    throw new InvalidOperationException("FOJS0009: array members must be singletons");
                }
                throw new InvalidOperationException("FOJS0009: array members must be singletons");
            }
        }

        private XProcessingInstruction BuildPi(XdmValue v)
        {
            if (!v.IsMap)
                throw new InvalidOperationException("FOJS0009: processing instruction must be a map");
            string? target = null, data = string.Empty;
            var mm = v.MapValue;
            foreach (var kv in mm.Keys)
            {
                if (!mm.TryGetValue(kv, out var mv))
                    continue;
                switch (kv.ToString())
                {
                    case "#target": target = RequireStringValue(mv, "PI target"); break;
                    case "#data": data = IsEmpty(mv) ? string.Empty : RequireStringValue(mv, "PI data"); break;
                    default: throw new InvalidOperationException("FOJS0009: invalid processing instruction map");
                }
            }
            if (target == null || !IsNcName(target) || target.Equals("xml", StringComparison.OrdinalIgnoreCase) ||
                data.Contains("?>"))
                throw new InvalidOperationException("FOJS0009: invalid processing instruction");
            return new XProcessingInstruction(target, data);
        }

        private string RequireStringValue(XdmValue v, string what)
        {
            if (!IsSingleString(v))
                throw new InvalidOperationException($"FOJS0009: {what} must be a single string");
            return Str(v);
        }
    }

    private static bool IsSingleEntryMap(XdmValue v)
        => v.IsMap && v.MapValue.Keys.Take(2).Count() == 1;

    private static (string Key, XdmValue Value) SingleEntry(XdmValue v)
    {
        var map = v.MapValue;
        var key = map.Keys.First();
        map.TryGetValue(key, out var value);
        return (key.ToString(), value);
    }

    internal static string LexicalOf(XdmValue v) => v.IsNode ? v.NodeValue.StringValue : v.ToString();

    // =====================================================================================
    // fn:jvalue (minimal: used by the element-to-map qt4tests asserts)
    // =====================================================================================

    /// <summary>
    /// Converts an item to a JSON value: nodes atomize to untypedAtomic, maps and arrays
    /// convert recursively, atomic values pass through unchanged.
    /// </summary>
    internal static XdmValue JValue(XdmValue input)
    {
        if (IsEmpty(input))
            return XdmValue.Undefined;
        if (input.IsNode)
            return Untyped(input.NodeValue.StringValue);
        if (input.IsMap)
        {
            var src = input.MapValue;
            var dst = new XdmMap();
            foreach (var key in src.Keys)
            {
                if (src.TryGetValue(key, out var v))
                    dst.Add(key, JValue(v));
            }
            return XdmValue.FromMap(dst);
        }
        if (input.IsArray)
        {
            var dst = new XdmArray();
            foreach (var member in input.ArrayValue.Values)
                dst.Add(JValue(member));
            return XdmValue.FromArray(dst);
        }
        return input;
    }

    // =====================================================================================
    // Shared small helpers
    // =====================================================================================

    internal static XdmValue Untyped(string s) => XdmValue.FromString(s, "untypedAtomic");

    private static XdmValue SingletonMap(string key, XdmValue value)
    {
        var map = new XdmMap();
        Add(map, key, value);
        return XdmValue.FromMap(map);
    }

    private static void Add(XdmMap map, string key, XdmValue value) => map.Add(XdmValue.FromString(key), value);

    private static bool IsEmpty(XdmValue value)
    {
        if (value.IsUndefined)
            return true;
        if (!value.IsSequence)
            return false;
        foreach (var _ in XdmSequence.FromSource(value.SequenceValue!))
            return false;
        return true;
    }

    private static bool IsSingleString(XdmValue value)
        => value.IsAtomic && value.Kind == XdmValueKind.String;

    private static string Str(XdmValue value) => value.ToString();

    private static XdmSequence AsSequence(XdmValue value)
        => value.IsSequence ? XdmSequence.FromSource(value.SequenceValue!) : XdmSequence.Singleton(value);
}

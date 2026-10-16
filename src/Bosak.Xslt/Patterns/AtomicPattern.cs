// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Parses atomic patterns and validates built-in atomic type alternatives.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-10-2026     | Creation                                                                                 |
// ===========================================================================================================================================================

using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using Bosak.XPath.Api;

namespace Bosak.Xslt.Patterns;

/// <summary>Parses atomic patterns and validates built-in atomic type alternatives.</summary>
internal sealed record AtomicPattern(string[] Names, string Predicates)
{
    private const string Schema = "Q{http://www.w3.org/2001/XMLSchema}";
    internal static bool StartsAtomicPattern(string text) => Regex.IsMatch(text, @"^atomic\s*\(");

    internal static AtomicPattern Parse(string text)
    {
        var opening = text.IndexOf('(');
        var close = PatternCompiler.FindMatchingDelimiter(text, opening, '(', ')');
        if (close < 0) throw new InvalidOperationException("XTSE0340: Unclosed atomic pattern.");
        var arguments = text[(opening + 1)..close].Trim();
        var lexical = Regex.Replace(arguments, @"Q\{[^}]*\}", "Q{}");
        if (lexical.StartsWith('|') || lexical.EndsWith('|') || lexical.Contains("||", StringComparison.Ordinal) ||
            Regex.IsMatch(lexical, @"\|\s*\|"))
            throw new InvalidOperationException("XTSE0340: Empty atomic type alternative.");
        var names = PatternCompiler.SplitPatternTopLevel(arguments, "|");
        foreach (var name in names)
            if (!Regex.IsMatch(name, @"^(?:Q\{[^}]*\})?[\p{L}_][\p{L}\p{M}\p{N}_.-]*(?::[\p{L}_][\p{L}\p{M}\p{N}_.-]*)?$"))
                throw new InvalidOperationException("XTSE0340: An atomic pattern requires type EQNames.");
        var predicates = text[(close + 1)..].Trim();
        var rest = predicates;
        while (rest.StartsWith('['))
        {
            var end = PatternCompiler.FindMatchingDelimiter(rest, 0, '[', ']');
            if (end < 0) throw new InvalidOperationException("XTSE0340: Unclosed atomic-pattern predicate.");
            rest = rest[(end + 1)..].TrimStart();
        }
        if (rest.Length != 0) throw new InvalidOperationException("XTSE0340: Unexpected text after atomic pattern.");
        return new(names, predicates);
    }

    internal string[] ResolveTypes(Func<string, string> resolveName) => Names.Select(name =>
    {
        var expanded = ItemTypeAliasExpansion.Expand(name, resolveName) ?? name;
        _ = TypePriority(expanded); // Reject structural/list/unknown types before matching any candidate.
        return expanded;
    }).ToArray();

    internal double DefaultPriority(Func<string, string>? resolveName)
    {
        if (resolveName is null) return Predicates.Length > 0 ? 0.5 : 0; // Binding occurs after declarations load.
        var types = ResolveTypes(resolveName);
        return Predicates.Length > 0 ? 0.5 : types.Min(TypePriority);
    }

    private static double TypePriority(string text)
    {
        text = text.Trim();
        if (text.StartsWith('(') && PatternCompiler.FindMatchingDelimiter(text, 0, '(', ')') == text.Length - 1)
            return PatternCompiler.SplitPatternTopLevel(text[1..^1], "|").Min(TypePriority);
        if (text.StartsWith(Schema, StringComparison.Ordinal)) text = "xs:" + text[Schema.Length..];
        if (text == "xs:anyAtomicType") return -0.5;
        if (text is "xs:untypedAtomic" or "xs:numeric") return 0;
        if (text is "xs:dayTimeDuration" or "xs:yearMonthDuration" or "xs:dateTimeStamp") return 0.25;
        var type = text.StartsWith("xs:", StringComparison.Ordinal)
            ? XmlSchemaType.GetBuiltInSimpleType(new XmlQualifiedName(text[3..], XmlSchema.Namespace)) : null;
        if (type is null || type.QualifiedName.Name == "anySimpleType" || type.Datatype?.Variety != XmlSchemaDatatypeVariety.Atomic)
            throw new InvalidOperationException("XTSE0340: Atomic patterns require supported atomic or pure-union types: " + text);
        int depth = 0;
        for (var current = type; current is not null && current.QualifiedName.Name != "anySimpleType";
            current = current.BaseXmlSchemaType as XmlSchemaSimpleType) depth++;
        return depth <= 1 ? 0 : 0.5 - Math.Pow(2, -depth);
    }
}

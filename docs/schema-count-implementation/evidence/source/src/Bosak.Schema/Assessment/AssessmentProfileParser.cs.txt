// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 29 October 2026
// PURPOSE              : Compiles only complete finite assessment profiles and exact decimal/assertion lexical forms.
// SPECIAL NOTES        : Bounded native XSD 1.1 assessment; decimal-sequence-assertions-v1 profile.
//
// COPYRIGHT            : Fytala
// LICENSE              : LICENSE.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 29-10-2026     | Creation (REQ-009 bounded document assessment, 0.5 development line)                      |
//                      | Charles Korthout | 0.2   | 29-10-2026     | Bound input capture and refuse malformed type QNames |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 29-10-2026     | Compile bounded optional local particles; preserve all required-only coverage (0.6)      |
//                      | Charles Korthout | 0.4   | 29-10-2026     | Compile bounded repeated particles and enforce aggregate maximum sixteen (0.7)           |
//                      | Charles Korthout | 0.5   | 29-10-2026     | Admit bounded count operands with complete-parse feature tracking and call depth (0.8)   |
// ===========================================================================================================================================================

using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Bosak.XPath.Api;

namespace Bosak.Schema;

internal static class AssessmentProfileParser
{
    private static readonly XNamespace Xs = "http://www.w3.org/2001/XMLSchema";
    private static readonly Regex DecimalLexical = new(@"^[+-]?(?:[0-9]+(?:\.[0-9]*)?|\.[0-9]+)$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
    internal static readonly char[] XmlWhitespace = [' ', '\t', '\r', '\n'];
    internal static bool IsXmlWhitespace(string value) => value.All(c => c is ' ' or '\t' or '\r' or '\n');
    internal static bool WithinInputBounds(XDocument document, int maximumCharacters)
    {
        long characters = 0;
        var nodes = 0;
        foreach (var node in document.DescendantNodes())
        {
            if (++nodes > 256) return false;
            if (node is XText text) characters += text.Value.Length;
            if (node is XComment comment) characters += comment.Value.Length;
            if (node is XProcessingInstruction instruction) characters += instruction.Data.Length + instruction.Target.Length;
            if (node is XElement element)
            {
                characters += element.Name.LocalName.Length + element.Name.NamespaceName.Length;
                foreach (var attribute in element.Attributes())
                {
                    characters += attribute.Value.Length + attribute.Name.LocalName.Length + attribute.Name.NamespaceName.Length;
                    if (characters > maximumCharacters) return false;
                }
            }
            if (characters > maximumCharacters) return false;
        }
        return true;
    }

    internal static XsdDocumentAssessor Compile(XDocument schema)
    {
        ArgumentNullException.ThrowIfNull(schema);
        if (schema.DocumentType is not null || schema.Declaration?.Version is not (null or "1.0") || !WithinInputBounds(schema, 32768))
            return Refused(string.Empty, "Schema DTD, XML version or node-count coverage is unsupported.");
        var text = schema.ToString(SaveOptions.DisableFormatting);
        if (text.Length > 32768) return Refused(string.Empty, "Schema size exceeds the profile bound.");
        var copy = new XDocument(schema);
        var root = copy.Root;
        if (root is null || root.Name != Xs + "schema" || !Attributes(root, "version", "xpathDefaultNamespace") || !LocalDefault(root))
            return Refused(text, "Only a no-target-namespace schema and the documented profile attributes are supported.");
        var globals = root.Elements().ToArray();
        if (globals.Length != 1 || globals[0].Name != Xs + "element" || !Attributes(globals[0], "name") || !TryName(globals[0], out var rootName))
            return Refused(text, "Exactly one named global element with an anonymous type is required.");
        var types = globals[0].Elements().ToArray();
        if (types.Length != 1 || types[0].Name != Xs + "complexType" || !Attributes(types[0]))
            return Refused(text, "Only an anonymous element-only complex type is supported.");
        var parts = types[0].Elements().ToArray();
        if (parts.Length is < 2 or > 9 || parts[0].Name != Xs + "sequence" || !Attributes(parts[0], "minOccurs", "maxOccurs") || !ExactlyOne(parts[0]) || parts.Skip(1).Any(p => p.Name != Xs + "assert"))
            return Refused(text, "Require a single sequence followed by one to eight assertions.");
        var declarations = parts[0].Elements().ToArray();
        if (declarations.Length is < 1 or > 16) return Refused(text, "The sequence requires one to sixteen decimal children.");
        var anonymous = new AssessedSchemaType(null, true);
        var rootDeclaration = new AssessedElementDeclaration(XName.Get(rootName), anonymous, "/schema/element");
        var decimalType = new AssessedSchemaType(Xs + "decimal", false);
        var names = new HashSet<string>(StringComparer.Ordinal);
        var children = new List<AssessmentParticle>();
        var totalMaximum = 0;
        for (var i = 0; i < declarations.Length; i++)
        {
            var declaration = declarations[i];
            if (declaration.Name != Xs + "element" || !Attributes(declaration, "name", "type", "minOccurs", "maxOccurs") ||
                declaration.HasElements || !LocalOccurrences(declaration, out var minimum, out var maximum) || !TryName(declaration, out var name) || !names.Add(name) || !IsDecimalType(declaration))
                return Refused(text, "Every local particle must be a unique xs:decimal element with minimum zero or one and finite maximum 1–16.");
            totalMaximum += maximum;
            if (totalMaximum > 16) return Refused(text, "The total local maximum occurrence budget exceeds sixteen.");
            children.Add(new(new AssessedElementDeclaration(XName.Get(name), decimalType, $"/schema/element/complexType/sequence/element[{i + 1}]"), minimum, maximum));
        }
        var plans = new List<XPath31Expression>();
        var usesCount = false;
        foreach (var assertion in parts.Skip(1))
        {
            var expression = assertion.Attribute("test")?.Value;
            if (!Attributes(assertion, "test", "xpathDefaultNamespace") || !LocalDefault(assertion) || assertion.HasElements ||
                expression is null)
                return Refused(text, "An assertion exceeds the bounded child/decimal/count comparison and logical-expression grammar.");
            var admission = AssertionGrammar.Admit(expression, names);
            if (admission is null)
                return Refused(text, "An assertion exceeds the bounded child/decimal/count comparison and logical-expression grammar.");
            usesCount |= admission.Value.UsesCount;
            try { plans.Add(XPath31Expression.Compile(expression, new CompileOptions { Compatibility = XPathCompatibility.XPath20 })); }
            catch (Exception error) when (error is not OutOfMemoryException and not OperationCanceledException)
            { return Refused(text, "The XPath 2.0 compiler refused the assertion: " + error.Message); }
        }
        return new(text, [], rootDeclaration, children, plans, usesCount);
    }

    private static bool Attributes(XElement element, params string[] permitted) =>
        element.Attributes().All(a => a.IsNamespaceDeclaration || a.Name.NamespaceName.Length == 0 && permitted.Contains(a.Name.LocalName, StringComparer.Ordinal)) &&
        element.Nodes().OfType<XText>().All(t => IsXmlWhitespace(t.Value));
    private static bool LocalDefault(XElement element) => element.Attribute("xpathDefaultNamespace") is not { } a || a.Value.Trim(XmlWhitespace) == "##local";
    private static bool ExactlyOne(XElement element) => new[] { "minOccurs", "maxOccurs" }.All(name => element.Attribute(name) is not { } a || a.Value.Trim(XmlWhitespace).TrimStart('0') == "1");
    private static bool LocalOccurrences(XElement element, out int minimum, out int maximum)
    {
        maximum = 0;
        return BoundedOccurrence(element.Attribute("minOccurs")?.Value ?? "1", 0, 1, out minimum)
            && BoundedOccurrence(element.Attribute("maxOccurs")?.Value ?? "1", 1, 16, out maximum);
    }
    private static bool BoundedOccurrence(string lexical, int lower, int upper, out int value)
    {
        value = 0;
        lexical = lexical.Trim(XmlWhitespace);
        if (lexical.Length == 0) return false;
        foreach (var digit in lexical)
        {
            if (digit is < '0' or > '9') return false;
            value = value * 10 + digit - '0';
            // Value never exceeds 16 before this iteration, so oversized lexicals cannot overflow.
            if (value > upper) return false;
        }
        return value >= lower;
    }
    private static bool TryName(XElement element, out string name)
    {
        name = element.Attribute("name")?.Value.Trim(XmlWhitespace) ?? string.Empty;
        try { XmlConvert.VerifyNCName(name); return true; }
        catch (XmlException) { return false; }
    }
    private static bool IsDecimalType(XElement element)
    {
        var lexical = element.Attribute("type")?.Value.Trim(XmlWhitespace);
        if (lexical is null) return false;
        var parts = lexical.Split(':');
        if (parts.Length == 1) return parts[0] == "decimal" && element.GetDefaultNamespace() == Xs;
        if (parts.Length != 2 || parts[1] != "decimal" || parts[0].Length == 0) return false;
        try { XmlConvert.VerifyNCName(parts[0]); }
        catch (XmlException) { return false; }
        return element.GetNamespaceOfPrefix(parts[0]) == Xs;
    }
    private static XsdDocumentAssessor Refused(string xml, string message) => new(xml,
        [new SchemaDiagnostic(SchemaDiagnosticCodes.AssessmentUnsupported, message, null)], null, [], []);

    internal static SchemaAssessmentOutcome ParseDecimal(string lexical, out decimal value, out string normalized)
    {
        normalized = lexical.Trim(XmlWhitespace);
        value = default;
        if (!DecimalLexical.IsMatch(normalized)) return SchemaAssessmentOutcome.Invalid;
        if (!decimal.TryParse(normalized, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value))
            return SchemaAssessmentOutcome.Unsupported;
        return Canonical(normalized) == Canonical(value.ToString(CultureInfo.InvariantCulture))
            ? SchemaAssessmentOutcome.Valid : SchemaAssessmentOutcome.Unsupported;
    }
    private static string Canonical(string lexical)
    {
        var negative = lexical.StartsWith('-');
        var parts = lexical.TrimStart('+', '-').Split('.');
        var integer = parts[0].TrimStart('0');
        if (integer.Length == 0) integer = "0";
        var fraction = parts.Length == 2 ? parts[1].TrimEnd('0') : string.Empty;
        return (negative && (integer != "0" || fraction.Length != 0) ? "-" : "") + integer + (fraction.Length == 0 ? "" : "." + fraction);
    }

    /// <summary>Features recorded only after complete bounded assertion admission.</summary>
    internal readonly record struct AssertionAdmission(bool UsesCount);

    private sealed class AssertionGrammar
    {
        private readonly string[] _tokens;
        private readonly HashSet<string> _names;
        private int _position;
        private bool _usesCount;
        private AssertionGrammar(string[] tokens, HashSet<string> names) { _tokens = tokens; _names = names; }
        internal static AssertionAdmission? Admit(string expression, HashSet<string> names)
        {
            if (expression.Length is 0 or > 512 || expression.Any(c => char.IsWhiteSpace(c) && c is not (' ' or '\t' or '\r' or '\n'))) return null;
            var tokens = expression.Replace("(", " ( ", StringComparison.Ordinal).Replace(")", " ) ", StringComparison.Ordinal)
                .Split(XmlWhitespace, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length is 0 or > 128) return null;
            var parser = new AssertionGrammar(tokens, names);
            return parser.Or(0) && parser._position == tokens.Length ? new AssertionAdmission(parser._usesCount) : null;
        }
        private bool Or(int depth)
        {
            if (!And(depth)) return false;
            while (Take("or")) if (!And(depth)) return false;
            return true;
        }
        private bool And(int depth)
        {
            if (!Comparison(depth)) return false;
            while (Take("and")) if (!Comparison(depth)) return false;
            return true;
        }
        private bool Comparison(int depth)
        {
            if (depth > 16) return false;
            if (Take("(")) return Or(depth + 1) && Take(")");
            if (!Operand(depth) || _position >= _tokens.Length || _tokens[_position++] is not ("eq" or "ne" or "lt" or "le" or "gt" or "ge")) return false;
            return Operand(depth);
        }
        private bool Operand(int depth)
        {
            if (_position >= _tokens.Length) return false;
            var token = _tokens[_position++];
            if (token == "count" && _position < _tokens.Length && _tokens[_position] == "(")
            {
                if (depth >= 16) return false;
                _position++;
                if (_position >= _tokens.Length || !_names.Contains(_tokens[_position++]) || !Take(")")) return false;
                _usesCount = true;
                return true;
            }
            return _names.Contains(token) || ParseDecimal(token, out _, out _) == SchemaAssessmentOutcome.Valid;
        }
        private bool Take(string token)
        {
            if (_position >= _tokens.Length || _tokens[_position] != token) return false;
            _position++; return true;
        }
    }
}

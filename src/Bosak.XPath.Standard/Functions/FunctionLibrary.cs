// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : The standard XPath / XQuery function library (fn, math, map, array, xs)
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 19-05-2026     | Added string, sequence, and aggregate standard functions                                 |
//                      | Charles Korthout | 0.3   | 19-05-2026     | Added map:* and array:* standard functions                                             |
//                      | Charles Korthout | 0.4   | 19-05-2026     | Added numeric and node-name accessor functions                                         |
//                      | Charles Korthout | 0.5   | 19-05-2026     | Added current-dateTime, current-date, current-time functions                           |
//                      | Charles Korthout | 0.6   | 22-05-2026     | Fixed xs: constructor functions to use VmEngine.Cast for validation                      |
//                      | Charles Korthout | 0.6   | 19-05-2026     | Added fn:node-name                                                                     |
//                      | Charles Korthout | 0.7   | 19-05-2026     | Added fn:number, fn:data, fn:root                                                      |
//                      | Charles Korthout | 0.8   | 19-05-2026     | Added date/time component extractors                                                   |
//                      | Charles Korthout | 0.9   | 19-05-2026     | Added fn:deep-equal, fn:generate-id, fn:compare                                        |
//                      | Charles Korthout | 1.0   | 19-05-2026     | Added URI encoders and QName functions                                                   |
//                      | Charles Korthout | 1.1   | 19-05-2026     | Added fn:doc and fn:collection with document identity caching                          |
//                      | Charles Korthout | 1.2   | 19-05-2026     | Added substring-before, substring-after, string-to-codepoints, codepoints-to-string, parse-xml |
//                      | Charles Korthout | 1.3   | 19-05-2026     | Added fn:analyze-string with regex group extraction                                    |
//                      | Charles Korthout | 1.4   | 19-05-2026     | Added fn:serialize                                                                     |
//                      | Charles Korthout | 1.5   | 26-06-2026     | JSON serialization for maps, arrays and atomic values in fn:serialize                  |
//                      | Charles Korthout | 1.6   | 19-05-2026     | Added fn:trace, fn:boolean, fn:zero-or-one, fn:one-or-more, fn:exactly-one, fn:base-uri, fn:document-uri |
//                      | Charles Korthout | 1.6   | 21-05-2026     | Fixed fn:deep-equal numeric cross-type, NaN, sequence, map key comparison              |
//                      | Charles Korthout | 1.7   | 21-05-2026     | Fixed fn:distinct-values to use deep-equal semantics; fixed xs:boolean string cast     |
//                      | Charles Korthout | 1.8   | 22-05-2026     | Fixed fn:base-uri/fn:document-uri empty sequence, type errors, fn:id atomization        |
//                      | Charles Korthout | 1.9   | 22-05-2026     | Added fn:format-number#2/#3 with grammar-based picture parser                           |
//                      | Charles Korthout | 2.0   | 23-05-2026     | Registered missing xs: constructors; duration normalization in xs: constructors         |
//                      | Charles Korthout | 2.2   | 26-06-2026     | Added fn:current-output-uri; hide XSLT dynamic functions in static context            |
//                      | Charles Korthout | 2.1   | 23-05-2026     | Added math:log10, math:exp10, math:asin, math:acos, math:atan, math:atan2             |
//                      | Charles Korthout | 2.2   | 23-05-2026     | Added fn:parse-xml-fragment, fn:has-children, fn:path, fn:unordered, map:put           |
//                      | Charles Korthout | 2.3   | 24-05-2026     | Fixed fn:substring rounding, fn:round-half-to-even decimal, fn:subsequence lazy ranges  |
//                      | Charles Korthout | 2.4   | 24-05-2026     | Implemented RFC-822/1123 parser for fn:parse-ietf-date with full timezone support        |
//                      | Charles Korthout | 2.5   | 24-05-2026     | Fixed fn:subsequence edge cases: negative start, INF/NaN bounds, XPTY0004 for strings    |
//                      | Charles Korthout | 2.6   | 24-05-2026     | Fixed fn:path sibling index, namespace parent axis, path#0; date/time cross-type checks |
//                      | Charles Korthout | 2.7   | 30-05-2026     | Fixed fn:generate-id to use underlying XObject as stable key for node identity         |
//                      | Charles Korthout | 2.7   | 26-05-2026     | Fixed fn:substring rounding to round-half-to-even; fixed fn:replace replacement string    |
//                      | Charles Korthout | 2.8   | 26-05-2026     | Added fn:document#1/#2 for XSLT compatibility                                            |
//                      | Charles Korthout | 2.9   | 27-05-2026     | Added fn:parse-json, fn:json-to-xml, fn:xml-to-json, fn:json-doc with options support   |
//                      | Charles Korthout | 3.0   | 27-05-2026     | Fixed fn:sum nested arrays, fn:function-arity curried, fn:round half-up, JSON escape/fallback |
//                      | Charles Korthout | 3.1   | 27-05-2026     | Fixed fn:tokenize one-arg normalize-space, fn:string/fn:data FOTY0013/FOTY0014/XPTY0004 |
//                      | Charles Korthout | 3.2   | 27-05-2026     | Fixed array:sort numeric/sequence comparison; fn:contains-token token trimming          |
//                      | Charles Korthout | 3.3   | 27-05-2026     | Added default collation support; fixed UCA starts-with/ends-with alternate=blanked     |
//                      | Charles Korthout | 3.4   | 30-05-2026     | Fixed fn:string-length to count Unicode code points via EnumerateRunes()                |
//                      | Charles Korthout | 3.5   | 30-05-2026     | Fixed fn:substring to use Unicode code points; Unicode full case mapping for upper/lower-case |
//                      | Charles Korthout | 3.6   | 01-06-2026     | Fixed fn:doc/fn:document to resolve empty string against base URI instead of returning empty sequence |
//                      | Charles Korthout | 3.7   | 02-06-2026     | MinMax treats atomized node strings as numeric (untypedAtomic→double semantics)         |
//                      | Charles Korthout | 3.8   | 02-06-2026     | Fixed fn:sort/array:sort default fn:data#1 key and lexicographic multi-value key compare |
//                      | Charles Korthout | 3.9   | 05-06-2026     | Atomize sort keys from key function; fix fn-sort-spec-6 node sequence ordering           |
//                      | Charles Korthout | 4.0   | 05-06-2026     | parse-xml/parse-xml-fragment preserve element whitespace; strip document-level whitespace |
//                      | Charles Korthout | 4.1   | 05-06-2026     | Fix fn:function-name to include standard namespace prefix for built-in functions         |
//                      | Charles Korthout | 4.2   | 11-06-2026     | Added fn:id#2; atomize node/sequence arguments for document()/doc-available()           |
//                      | Charles Korthout | 4.3   | 13-06-2026     | ToDoubleValue converts xs:boolean to 1/0; fixes sort-046 conditional sort key          |
//                      | Charles Korthout | 4.4   | 11-06-2026     | fn:root/fn:path handle parentless (temporary-tree) element roots; fixes accumulator-088 |
//                      | Charles Korthout | 4.5   | 13-06-2026     | fn:sum returns xs:integer when all atomized items are integers                           |
//                      | Charles Korthout | 4.6   | 13-06-2026     | fn:type-available parses Q{uri}local EQName syntax                                       |
//                      | Charles Korthout | 4.7   | 13-06-2026     | fn:document#1/#2 resolves fragment identifiers to elements                                |
//                      | Charles Korthout | 4.8   | 13-06-2026     | fn:collection resolves relative URIs against base URI for xsl:merge tests               |
//                      | Charles Korthout | 4.9   | 13-06-2026     | Adjust-time/date/dateTime use implicit timezone; dateTime constructor supports extended years |
//                      | Charles Korthout | 5.0   | 13-06-2026     | Registered fn:regex-group#1 for xsl:analyze-string                                      |
//                      | Charles Korthout | 5.1   | 13-06-2026     | Shared RegexHelper: XSD validation, backreference translation, and quote-preserving unquote |
//                      | Charles Korthout | 5.2   | 24-06-2026     | element-available uses DefiningElementDefaultNamespace for unprefixed QNames             |
//                      | Charles Korthout | 5.3   | 24-06-2026     | fn:doc('') resolves against static base URI; atomizes and validates sequence argument    |
//                      | Charles Korthout | 5.4   | 24-06-2026     | fn:unparsed-text-lines drops trailing empty line; validates XML characters (FOUT1190)  |
//                      | Charles Korthout | 5.5   | 24-06-2026     | fn:function-available validates QName and reports XTDE1400 for invalid/unbound names   |
//                      | Charles Korthout | 5.6   | 24-06-2026     | Implemented fn:snapshot; fixed fn:innermost/fn:outermost descendant/ancestor checks    |
//                      | Charles Korthout | 5.7   | 25-06-2026     | Added fn:nilled#0/#1; system-property namespace expansion; regex options in matches/replace/tokenize |
//                      | Charles Korthout | 5.8   | 25-06-2026     | fn:resolve-QName validates lexical QName and raises FOCA0002; xs:boolean string cast is case-sensitive |
//                      | Charles Korthout | 5.9   | 25-06-2026     | function-available hides XSLT dynamic functions from static (use-when) evaluation        |
//                      | Charles Korthout | 5.10  | 27-06-2026     | XPath INF/NaN parsing, atomize floor/ceiling/round args, decimal rounding for ties      |
//                      | Charles Korthout | 5.11  | 27-06-2026     | fn:snapshot copies in-scope namespace bindings to element copies                       |
//                      | Charles Korthout | 5.12  | 28-06-2026     | fn:document#1 uses node base URIs; fn:resolve-uri validates base and relative URIs     |
//                      | Charles Korthout | 5.13  | 28-06-2026     | FORG0002 for relative base/malformed relative URIs; dotted-path resolution             |
//                      | Charles Korthout | 5.14  | 26-06-2026     | Allow XML 1.1 C0 controls in codepoints-to-string; honor duplicates in json-to-xml     |
//                      | Charles Korthout | 5.15  | 02-07-2026     | ToDoubleValueStrict parses xs:untypedAtomic; fn:remove and QName-from functions atomize  |
//                      | Charles Korthout | 5.16  | 02-07-2026     | available-system-properties returns xs:QName values; added missing XSLT system properties |
//                      | Charles Korthout | 5.17  | 26-06-2026     | Default-collation aware default-collation(), deep-equal, max/min, index-of, distinct-values |
//                      | Charles Korthout | 5.18  | 05-07-2026     | Default-collation aware fn:compare, contains, starts-with, ends-with, substring-before/after |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Frozen;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Functions;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.XPath.Standard.Functions;

/// <summary>
/// The standard XPath / XQuery function library (fn, math, map, array, xs).
/// </summary>
public static class FunctionLibrary
{
    private static readonly FrozenDictionary<(string ns, string name, int arity), FunctionSignature> StandardFunctions;

    static FunctionLibrary()
    {
        var functions = new Dictionary<(string, string, int), FunctionSignature>
        {
            // ----- fn:string --------------------------------------------------
            [(Namespaces.Fn, "string", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "string",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.String,
                Implementation = String_0
            },
            [(Namespaces.Fn, "string", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "string",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.String,
                Implementation = String_1
            },

            // ----- fn:count ---------------------------------------------------
            [(Namespaces.Fn, "count", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "count",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Integer,
                Implementation = Count
            },

            // ----- fn:position / fn:last --------------------------------------
            [(Namespaces.Fn, "position", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "position",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.Integer,
                Implementation = Position
            },
            [(Namespaces.Fn, "last", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "last",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.Integer,
                Implementation = Last
            },
            [(Namespaces.Fn, "current", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "current",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.Sequence,
                Implementation = Current
            },
            [(Namespaces.Fn, "current-output-uri", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "current-output-uri",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.Sequence,
                Implementation = CurrentOutputUri
            },

            // ----- fn:exists --------------------------------------------------
            [(Namespaces.Fn, "exists", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "exists",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Boolean,
                Implementation = Exists
            },

            // ----- fn:empty ---------------------------------------------------
            [(Namespaces.Fn, "empty", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "empty",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Boolean,
                Implementation = Empty
            },

            // ----- fn:head ----------------------------------------------------
            [(Namespaces.Fn, "head", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "head",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Undefined,
                Implementation = Head
            },

            // ----- fn:tail ----------------------------------------------------
            [(Namespaces.Fn, "tail", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "tail",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Sequence,
                Implementation = Tail
            },

            // ----- fn:not -----------------------------------------------------
            [(Namespaces.Fn, "not", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "not",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Boolean],
                ReturnType = XdmValueKind.Boolean,
                Implementation = Not
            },

            // ----- fn:true / fn:false -----------------------------------------
            [(Namespaces.Fn, "true", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "true",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.Boolean,
                Implementation = (_, _) => XdmValue.True
            },
            [(Namespaces.Fn, "false", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "false",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.Boolean,
                Implementation = (_, _) => XdmValue.False
            },

            // ----- fn:concat (variable arity 2+) -----------------------------
            [(Namespaces.Fn, "concat", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "concat",
                Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = ConcatN
            },
            [(Namespaces.Fn, "concat", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "concat",
                Arity = 3,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = ConcatN
            },
            [(Namespaces.Fn, "concat", 4)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "concat",
                Arity = 4,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = ConcatN
            },
            [(Namespaces.Fn, "concat", 5)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "concat",
                Arity = 5,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = ConcatN
            },
            [(Namespaces.Fn, "concat", 6)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "concat", Arity = 6,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String, Implementation = ConcatN
            },
            [(Namespaces.Fn, "concat", 7)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "concat", Arity = 7,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String, Implementation = ConcatN
            },
            [(Namespaces.Fn, "concat", 8)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "concat", Arity = 8,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String, Implementation = ConcatN
            },
            [(Namespaces.Fn, "concat", 9)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "concat", Arity = 9,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String, Implementation = ConcatN
            },
            [(Namespaces.Fn, "concat", 10)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "concat", Arity = 10,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String, Implementation = ConcatN
            },
            [(Namespaces.Fn, "concat", 11)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "concat", Arity = 11,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String, Implementation = ConcatN
            },
            [(Namespaces.Fn, "concat", 12)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "concat", Arity = 12,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String, Implementation = ConcatN
            },
            [(Namespaces.Fn, "concat", 13)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "concat", Arity = 13,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String, Implementation = ConcatN
            },

            // ----- fn:string-length -------------------------------------------
            [(Namespaces.Fn, "string-length", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "string-length",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.Integer,
                Implementation = StringLength_0
            },
            [(Namespaces.Fn, "string-length", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "string-length",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Integer,
                Implementation = StringLength_1
            },

            // ----- fn:substring -----------------------------------------------
            [(Namespaces.Fn, "substring", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "substring",
                Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.Double],
                ReturnType = XdmValueKind.String,
                Implementation = Substring_2
            },
            [(Namespaces.Fn, "substring", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "substring",
                Arity = 3,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.Double, XdmValueKind.Double],
                ReturnType = XdmValueKind.String,
                Implementation = Substring_3
            },

            // ----- fn:substring-before ----------------------------------------
            [(Namespaces.Fn, "substring-before", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "substring-before", Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = SubstringBefore_2
            },
            [(Namespaces.Fn, "substring-before", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "substring-before", Arity = 3,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = SubstringBefore_3
            },

            // ----- fn:substring-after -----------------------------------------
            [(Namespaces.Fn, "substring-after", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "substring-after", Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = SubstringAfter_2
            },
            [(Namespaces.Fn, "substring-after", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "substring-after", Arity = 3,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = SubstringAfter_3
            },

            // ----- fn:string-to-codepoints ------------------------------------
            [(Namespaces.Fn, "string-to-codepoints", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "string-to-codepoints", Arity = 1,
                ParameterTypes = [XdmValueKind.String],
                ReturnType = XdmValueKind.Sequence,
                Implementation = StringToCodepoints
            },

            // ----- fn:codepoints-to-string ------------------------------------
            [(Namespaces.Fn, "codepoints-to-string", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "codepoints-to-string", Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.String,
                Implementation = CodepointsToString
            },

            // ----- fn:parse-xml -----------------------------------------------
            [(Namespaces.Fn, "parse-xml", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "parse-xml", Arity = 1,
                ParameterTypes = [XdmValueKind.String],
                ReturnType = XdmValueKind.Node,
                Implementation = ParseXml_1
            },
            [(Namespaces.Fn, "parse-xml-fragment", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "parse-xml-fragment", Arity = 1,
                ParameterTypes = [XdmValueKind.String],
                ReturnType = XdmValueKind.Node,
                Implementation = ParseXmlFragment_1
            },
            [(Namespaces.Fn, "has-children", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "has-children", Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.Boolean,
                Implementation = HasChildren_0
            },
            [(Namespaces.Fn, "has-children", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "has-children", Arity = 1,
                ParameterTypes = [XdmValueKind.Node],
                ReturnType = XdmValueKind.Boolean,
                Implementation = HasChildren_1
            },
            [(Namespaces.Fn, "path", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "path", Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.String,
                Implementation = Path_0
            },
            [(Namespaces.Fn, "path", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "path", Arity = 1,
                ParameterTypes = [XdmValueKind.Node],
                ReturnType = XdmValueKind.String,
                Implementation = Path_1
            },
            [(Namespaces.Fn, "unordered", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "unordered", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Undefined,
                Implementation = Unordered_1
            },

            // ----- fn:serialize -----------------------------------------------
            [(Namespaces.Fn, "serialize", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "serialize", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.String,
                Implementation = Serialize_1
            },

            // ----- fn:analyze-string ------------------------------------------
            [(Namespaces.Fn, "analyze-string", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "analyze-string", Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.Node,
                Implementation = AnalyzeString_2
            },
            [(Namespaces.Fn, "analyze-string", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "analyze-string", Arity = 3,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.Node,
                Implementation = AnalyzeString_3
            },
            [(Namespaces.Fn, "regex-group", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "regex-group", Arity = 1,
                ParameterTypes = [XdmValueKind.Integer],
                ReturnType = XdmValueKind.String,
                Implementation = RegexGroup
            },

            // ----- fn:apply ---------------------------------------------------
            [(Namespaces.Fn, "apply", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "apply", Arity = 2,
                ParameterTypes = [XdmValueKind.Function, XdmValueKind.Array],
                ReturnType = XdmValueKind.Undefined,
                Implementation = Apply
            },

            // ----- fn:available-environment-variables -------------------------
            [(Namespaces.Fn, "available-environment-variables", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "available-environment-variables", Arity = 0,
                ParameterTypes = [], ReturnType = XdmValueKind.Sequence,
                Implementation = AvailableEnvironmentVariables
            },
            [(Namespaces.Fn, "environment-variable", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "environment-variable", Arity = 1,
                ParameterTypes = [XdmValueKind.String], ReturnType = XdmValueKind.String,
                Implementation = EnvironmentVariable
            },

            // ----- fn:contains ------------------------------------------------
            [(Namespaces.Fn, "contains", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "contains",
                Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.Boolean,
                Implementation = Contains
            },
            [(Namespaces.Fn, "contains", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "contains",
                Arity = 3,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.Boolean,
                Implementation = Contains_3
            },

            // ----- fn:starts-with ---------------------------------------------
            [(Namespaces.Fn, "starts-with", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "starts-with",
                Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.Boolean,
                Implementation = StartsWith
            },
            [(Namespaces.Fn, "starts-with", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "starts-with",
                Arity = 3,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.Boolean,
                Implementation = StartsWith_3
            },

            // ----- fn:ends-with -----------------------------------------------
            [(Namespaces.Fn, "ends-with", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "ends-with",
                Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.Boolean,
                Implementation = EndsWith
            },
            [(Namespaces.Fn, "ends-with", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "ends-with",
                Arity = 3,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.Boolean,
                Implementation = EndsWith_3
            },

            // ----- fn:contains-token ------------------------------------------
            [(Namespaces.Fn, "contains-token", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "contains-token",
                Arity = 2,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.String],
                ReturnType = XdmValueKind.Boolean,
                Implementation = ContainsToken_2
            },
            [(Namespaces.Fn, "contains-token", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "contains-token",
                Arity = 3,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.Boolean,
                Implementation = ContainsToken_3
            },

            // ----- fn:codepoint-equal -----------------------------------------
            [(Namespaces.Fn, "codepoint-equal", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "codepoint-equal",
                Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.Boolean,
                Implementation = CodepointEqual
            },

            // ----- fn:collation-key -------------------------------------------
            [(Namespaces.Fn, "collation-key", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "collation-key",
                Arity = 1,
                ParameterTypes = [XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = CollationKey_1
            },
            [(Namespaces.Fn, "collation-key", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "collation-key",
                Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = CollationKey_2
            },

            // ----- fn:normalize-space -----------------------------------------
            [(Namespaces.Fn, "normalize-space", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "normalize-space",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.String,
                Implementation = NormalizeSpace_0
            },
            [(Namespaces.Fn, "normalize-space", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "normalize-space",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.String,
                Implementation = NormalizeSpace_1
            },
            [(Namespaces.Fn, "normalize-unicode", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "normalize-unicode", Arity = 1,
                ParameterTypes = [XdmValueKind.String], ReturnType = XdmValueKind.String,
                Implementation = NormalizeUnicode_1
            },
            [(Namespaces.Fn, "normalize-unicode", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "normalize-unicode", Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String], ReturnType = XdmValueKind.String,
                Implementation = NormalizeUnicode_2
            },

            // ----- fn:translate -----------------------------------------------
            [(Namespaces.Fn, "translate", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "translate",
                Arity = 3,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = Translate
            },

            // ----- fn:upper-case ----------------------------------------------
            [(Namespaces.Fn, "upper-case", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "upper-case",
                Arity = 1,
                ParameterTypes = [XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = UpperCase
            },

            // ----- fn:lower-case ----------------------------------------------
            [(Namespaces.Fn, "lower-case", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "lower-case",
                Arity = 1,
                ParameterTypes = [XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = LowerCase
            },

            // ----- fn:matches -------------------------------------------------
            [(Namespaces.Fn, "matches", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "matches",
                Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.Boolean,
                Implementation = Matches_2
            },
            [(Namespaces.Fn, "matches", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "matches",
                Arity = 3,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.Boolean,
                Implementation = Matches_3
            },

            // ----- fn:replace -------------------------------------------------
            [(Namespaces.Fn, "replace", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "replace",
                Arity = 3,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = Replace_3
            },
            [(Namespaces.Fn, "replace", 4)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "replace",
                Arity = 4,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = Replace_4
            },

            // ----- fn:tokenize ------------------------------------------------
            [(Namespaces.Fn, "tokenize", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "tokenize",
                Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.Sequence,
                Implementation = Tokenize_2
            },
            [(Namespaces.Fn, "tokenize", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "tokenize",
                Arity = 3,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.Sequence,
                Implementation = Tokenize_3
            },
            [(Namespaces.Fn, "tokenize", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "tokenize",
                Arity = 1,
                ParameterTypes = [XdmValueKind.String],
                ReturnType = XdmValueKind.Sequence,
                Implementation = Tokenize_1
            },

            // ----- fn:insert-before -------------------------------------------
            [(Namespaces.Fn, "insert-before", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "insert-before",
                Arity = 3,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.Integer, XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Sequence,
                Implementation = InsertBefore
            },

            // ----- fn:remove --------------------------------------------------
            [(Namespaces.Fn, "remove", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "remove",
                Arity = 2,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.Integer],
                ReturnType = XdmValueKind.Sequence,
                Implementation = Remove
            },

            // ----- fn:reverse -------------------------------------------------
            [(Namespaces.Fn, "reverse", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "reverse",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Sequence,
                Implementation = Reverse
            },

            // ----- fn:subsequence ---------------------------------------------
            [(Namespaces.Fn, "subsequence", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "subsequence",
                Arity = 2,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.Double],
                ReturnType = XdmValueKind.Sequence,
                Implementation = Subsequence_2
            },
            [(Namespaces.Fn, "subsequence", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "subsequence",
                Arity = 3,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.Double, XdmValueKind.Double],
                ReturnType = XdmValueKind.Sequence,
                Implementation = Subsequence_3
            },

            // ----- fn:distinct-values -----------------------------------------
            [(Namespaces.Fn, "distinct-values", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "distinct-values",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Sequence,
                Implementation = DistinctValues_1
            },
            [(Namespaces.Fn, "distinct-values", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "distinct-values",
                Arity = 2,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.String],
                ReturnType = XdmValueKind.Sequence,
                Implementation = DistinctValues_2
            },

            // ----- fn:index-of ------------------------------------------------
            [(Namespaces.Fn, "index-of", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "index-of",
                Arity = 2,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Sequence,
                Implementation = IndexOf_2
            },
            [(Namespaces.Fn, "index-of", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "index-of",
                Arity = 3,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.Undefined, XdmValueKind.String],
                ReturnType = XdmValueKind.Sequence,
                Implementation = IndexOf_3
            },

            // ----- fn:sum -----------------------------------------------------
            [(Namespaces.Fn, "sum", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "sum",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Undefined,
                Implementation = Sum_1
            },
            [(Namespaces.Fn, "sum", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "sum",
                Arity = 2,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Undefined,
                Implementation = Sum_2
            },

            // ----- fn:avg -----------------------------------------------------
            [(Namespaces.Fn, "avg", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "avg",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Undefined,
                Implementation = Avg
            },

            // ----- fn:min -----------------------------------------------------
            [(Namespaces.Fn, "min", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "min",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Undefined,
                Implementation = Min_1
            },
            [(Namespaces.Fn, "min", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "min",
                Arity = 2,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.String],
                ReturnType = XdmValueKind.Undefined,
                Implementation = Min_2
            },

            // ----- fn:max -----------------------------------------------------
            [(Namespaces.Fn, "max", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "max",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Undefined,
                Implementation = Max_1
            },
            [(Namespaces.Fn, "max", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "max",
                Arity = 2,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.String],
                ReturnType = XdmValueKind.Undefined,
                Implementation = Max_2
            },

            // ----- fn:string-join ---------------------------------------------
            [(Namespaces.Fn, "string-join", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "string-join",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.String,
                Implementation = StringJoin_1
            },
            [(Namespaces.Fn, "string-join", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "string-join",
                Arity = 2,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = StringJoin_2
            },

            // ----- map:get ----------------------------------------------------
            [(Namespaces.Map, "get", 2)] = new()
            {
                NamespaceUri = Namespaces.Map,
                LocalName = "get",
                Arity = 2,
                ParameterTypes = [XdmValueKind.Map, XdmValueKind.String],
                ReturnType = XdmValueKind.Undefined,
                Implementation = MapGet
            },

            // ----- map:size ---------------------------------------------------
            [(Namespaces.Map, "size", 1)] = new()
            {
                NamespaceUri = Namespaces.Map,
                LocalName = "size",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Map],
                ReturnType = XdmValueKind.Integer,
                Implementation = MapSize
            },

            // ----- map:contains -----------------------------------------------
            [(Namespaces.Map, "contains", 2)] = new()
            {
                NamespaceUri = Namespaces.Map,
                LocalName = "contains",
                Arity = 2,
                ParameterTypes = [XdmValueKind.Map, XdmValueKind.String],
                ReturnType = XdmValueKind.Boolean,
                Implementation = MapContains
            },

            // ----- map:keys ---------------------------------------------------
            [(Namespaces.Map, "keys", 1)] = new()
            {
                NamespaceUri = Namespaces.Map,
                LocalName = "keys",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Map],
                ReturnType = XdmValueKind.Sequence,
                Implementation = MapKeys
            },

            // ----- map:merge --------------------------------------------------
            [(Namespaces.Map, "merge", 1)] = new()
            {
                NamespaceUri = Namespaces.Map,
                LocalName = "merge",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Map,
                Implementation = MapMerge
            },
            [(Namespaces.Map, "merge", 2)] = new()
            {
                NamespaceUri = Namespaces.Map, LocalName = "merge", Arity = 2,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.Map],
                ReturnType = XdmValueKind.Map,
                Implementation = MapMerge
            },
            [(Namespaces.Map, "remove", 2)] = new()
            {
                NamespaceUri = Namespaces.Map, LocalName = "remove", Arity = 2,
                ParameterTypes = [XdmValueKind.Map, XdmValueKind.String],
                ReturnType = XdmValueKind.Map,
                Implementation = MapRemove
            },
            [(Namespaces.Map, "put", 3)] = new()
            {
                NamespaceUri = Namespaces.Map, LocalName = "put", Arity = 3,
                ParameterTypes = [XdmValueKind.Map, XdmValueKind.Undefined, XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Map,
                Implementation = MapPut
            },
            [(Namespaces.Map, "entry", 2)] = new()
            {
                NamespaceUri = Namespaces.Map, LocalName = "entry", Arity = 2,
                ParameterTypes = [XdmValueKind.Undefined, XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Map,
                Implementation = MapEntry
            },
            [(Namespaces.Map, "for-each", 2)] = new()
            {
                NamespaceUri = Namespaces.Map, LocalName = "for-each", Arity = 2,
                ParameterTypes = [XdmValueKind.Map, XdmValueKind.Function],
                ReturnType = XdmValueKind.Sequence,
                Implementation = MapForEach
            },

            // ----- array:size -------------------------------------------------
            [(Namespaces.Array, "size", 1)] = new()
            {
                NamespaceUri = Namespaces.Array,
                LocalName = "size",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Array],
                ReturnType = XdmValueKind.Integer,
                Implementation = ArraySize
            },

            // ----- array:get --------------------------------------------------
            [(Namespaces.Array, "get", 2)] = new()
            {
                NamespaceUri = Namespaces.Array,
                LocalName = "get",
                Arity = 2,
                ParameterTypes = [XdmValueKind.Array, XdmValueKind.Integer],
                ReturnType = XdmValueKind.Undefined,
                Implementation = ArrayGet
            },

            // ----- array:contains ---------------------------------------------
            [(Namespaces.Array, "contains", 2)] = new()
            {
                NamespaceUri = Namespaces.Array,
                LocalName = "contains",
                Arity = 2,
                ParameterTypes = [XdmValueKind.Array, XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Boolean,
                Implementation = ArrayContains
            },

            // ----- array:head -------------------------------------------------
            [(Namespaces.Array, "head", 1)] = new()
            {
                NamespaceUri = Namespaces.Array,
                LocalName = "head",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Array],
                ReturnType = XdmValueKind.Undefined,
                Implementation = ArrayHead
            },

            // ----- array:tail -------------------------------------------------
            [(Namespaces.Array, "tail", 1)] = new()
            {
                NamespaceUri = Namespaces.Array,
                LocalName = "tail",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Array],
                ReturnType = XdmValueKind.Array,
                Implementation = ArrayTail
            },
            [(Namespaces.Array, "put", 3)] = new()
            {
                NamespaceUri = Namespaces.Array, LocalName = "put", Arity = 3,
                ParameterTypes = [XdmValueKind.Array, XdmValueKind.Integer, XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Array,
                Implementation = ArrayPut
            },
            [(Namespaces.Array, "remove", 2)] = new()
            {
                NamespaceUri = Namespaces.Array, LocalName = "remove", Arity = 2,
                ParameterTypes = [XdmValueKind.Array, XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Array,
                Implementation = ArrayRemove
            },
            [(Namespaces.Array, "append", 2)] = new()
            {
                NamespaceUri = Namespaces.Array, LocalName = "append", Arity = 2,
                ParameterTypes = [XdmValueKind.Array, XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Array,
                Implementation = ArrayAppend
            },
            [(Namespaces.Array, "subarray", 2)] = new()
            {
                NamespaceUri = Namespaces.Array, LocalName = "subarray", Arity = 2,
                ParameterTypes = [XdmValueKind.Array, XdmValueKind.Integer],
                ReturnType = XdmValueKind.Array,
                Implementation = ArraySubarray_2
            },
            [(Namespaces.Array, "subarray", 3)] = new()
            {
                NamespaceUri = Namespaces.Array, LocalName = "subarray", Arity = 3,
                ParameterTypes = [XdmValueKind.Array, XdmValueKind.Integer, XdmValueKind.Integer],
                ReturnType = XdmValueKind.Array,
                Implementation = ArraySubarray_3
            },
            [(Namespaces.Array, "reverse", 1)] = new()
            {
                NamespaceUri = Namespaces.Array, LocalName = "reverse", Arity = 1,
                ParameterTypes = [XdmValueKind.Array],
                ReturnType = XdmValueKind.Array,
                Implementation = ArrayReverse
            },
            [(Namespaces.Array, "join", 1)] = new()
            {
                NamespaceUri = Namespaces.Array, LocalName = "join", Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Array,
                Implementation = ArrayJoin
            },
            [(Namespaces.Array, "filter", 2)] = new()
            {
                NamespaceUri = Namespaces.Array, LocalName = "filter", Arity = 2,
                ParameterTypes = [XdmValueKind.Array, XdmValueKind.Function],
                ReturnType = XdmValueKind.Array,
                Implementation = ArrayFilter
            },
            [(Namespaces.Array, "fold-left", 3)] = new()
            {
                NamespaceUri = Namespaces.Array, LocalName = "fold-left", Arity = 3,
                ParameterTypes = [XdmValueKind.Array, XdmValueKind.Undefined, XdmValueKind.Function],
                ReturnType = XdmValueKind.Undefined,
                Implementation = ArrayFoldLeft
            },
            [(Namespaces.Array, "fold-right", 3)] = new()
            {
                NamespaceUri = Namespaces.Array, LocalName = "fold-right", Arity = 3,
                ParameterTypes = [XdmValueKind.Array, XdmValueKind.Undefined, XdmValueKind.Function],
                ReturnType = XdmValueKind.Undefined,
                Implementation = ArrayFoldRight
            },
            [(Namespaces.Array, "for-each", 2)] = new()
            {
                NamespaceUri = Namespaces.Array, LocalName = "for-each", Arity = 2,
                ParameterTypes = [XdmValueKind.Array, XdmValueKind.Function],
                ReturnType = XdmValueKind.Array,
                Implementation = ArrayForEach
            },
            [(Namespaces.Array, "for-each-pair", 3)] = new()
            {
                NamespaceUri = Namespaces.Array, LocalName = "for-each-pair", Arity = 3,
                ParameterTypes = [XdmValueKind.Array, XdmValueKind.Array, XdmValueKind.Function],
                ReturnType = XdmValueKind.Array,
                Implementation = ArrayForEachPair
            },
            [(Namespaces.Array, "sort", 1)] = new()
            {
                NamespaceUri = Namespaces.Array, LocalName = "sort", Arity = 1,
                ParameterTypes = [XdmValueKind.Array],
                ReturnType = XdmValueKind.Array,
                Implementation = ArraySort_1
            },
            [(Namespaces.Array, "sort", 2)] = new()
            {
                NamespaceUri = Namespaces.Array, LocalName = "sort", Arity = 2,
                ParameterTypes = [XdmValueKind.Array, XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Array,
                Implementation = ArraySort_2
            },
            [(Namespaces.Array, "sort", 3)] = new()
            {
                NamespaceUri = Namespaces.Array, LocalName = "sort", Arity = 3,
                ParameterTypes = [XdmValueKind.Array, XdmValueKind.Undefined, XdmValueKind.Function],
                ReturnType = XdmValueKind.Array,
                Implementation = ArraySort_3
            },
            [(Namespaces.Array, "flatten", 1)] = new()
            {
                NamespaceUri = Namespaces.Array, LocalName = "flatten", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Sequence,
                Implementation = ArrayFlatten
            },
            [(Namespaces.Array, "insert-before", 3)] = new()
            {
                NamespaceUri = Namespaces.Array, LocalName = "insert-before", Arity = 3,
                ParameterTypes = [XdmValueKind.Array, XdmValueKind.Integer, XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Array,
                Implementation = ArrayInsertBefore
            },

            // ----- fn:abs -----------------------------------------------------
            [(Namespaces.Fn, "abs", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "abs",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Undefined,
                Implementation = Abs
            },

            // ----- fn:floor ---------------------------------------------------
            [(Namespaces.Fn, "floor", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "floor",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Undefined,
                Implementation = Floor
            },

            // ----- fn:ceiling -------------------------------------------------
            [(Namespaces.Fn, "ceiling", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "ceiling",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Undefined,
                Implementation = Ceiling
            },

            // ----- fn:round ---------------------------------------------------
            [(Namespaces.Fn, "round", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "round",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Undefined,
                Implementation = Round_1
            },
            [(Namespaces.Fn, "round", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "round",
                Arity = 2,
                ParameterTypes = [XdmValueKind.Undefined, XdmValueKind.Integer],
                ReturnType = XdmValueKind.Undefined,
                Implementation = Round_2
            },

            // ----- fn:round-half-to-even --------------------------------------
            [(Namespaces.Fn, "round-half-to-even", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "round-half-to-even",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Undefined,
                Implementation = RoundHalfToEven_1
            },
            [(Namespaces.Fn, "round-half-to-even", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "round-half-to-even",
                Arity = 2,
                ParameterTypes = [XdmValueKind.Undefined, XdmValueKind.Integer],
                ReturnType = XdmValueKind.Undefined,
                Implementation = RoundHalfToEven_2
            },

            // ----- fn:local-name ----------------------------------------------
            [(Namespaces.Fn, "local-name", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "local-name",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.String,
                Implementation = LocalName_0
            },
            [(Namespaces.Fn, "local-name", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "local-name",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.String,
                Implementation = LocalName_1
            },

            // ----- fn:namespace-uri -------------------------------------------
            [(Namespaces.Fn, "namespace-uri", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "namespace-uri",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.String,
                Implementation = NamespaceUri_0
            },
            [(Namespaces.Fn, "namespace-uri", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "namespace-uri",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.String,
                Implementation = NamespaceUri_1
            },

            // ----- fn:name ----------------------------------------------------
            [(Namespaces.Fn, "name", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "name",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.String,
                Implementation = Name_0
            },
            [(Namespaces.Fn, "name", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "name",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.String,
                Implementation = Name_1
            },

            [(Namespaces.Fn, "lang", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "lang", Arity = 1,
                ParameterTypes = [XdmValueKind.String],
                ReturnType = XdmValueKind.Boolean,
                Implementation = Lang_1
            },
            [(Namespaces.Fn, "lang", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "lang", Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.Node],
                ReturnType = XdmValueKind.Boolean,
                Implementation = Lang_2
            },

            // ----- fn:dateTime ------------------------------------------------
            [(Namespaces.Fn, "dateTime", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "dateTime",
                Arity = 2,
                ParameterTypes = [XdmValueKind.Date, XdmValueKind.Time],
                ReturnType = XdmValueKind.DateTime,
                Implementation = DateTime_2
            },

            // ----- fn:current-dateTime ----------------------------------------
            [(Namespaces.Fn, "current-dateTime", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "current-dateTime",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.DateTime,
                Implementation = CurrentDateTime
            },

            // ----- fn:default-collation ---------------------------------------
            [(Namespaces.Fn, "default-collation", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "default-collation", Arity = 0,
                ParameterTypes = [], ReturnType = XdmValueKind.String,
                Implementation = DefaultCollation
            },

            // ----- fn:system-property -----------------------------------------
            [(Namespaces.Fn, "system-property", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "system-property", Arity = 1,
                ParameterTypes = [XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = SystemProperty
            },

            // ----- fn:available-system-properties -------------------------------
            [(Namespaces.Fn, "available-system-properties", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "available-system-properties", Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.Sequence,
                Implementation = AvailableSystemProperties
            },

            // ----- fn:function-available ----------------------------------------
            [(Namespaces.Fn, "function-available", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "function-available", Arity = 1,
                ParameterTypes = [XdmValueKind.String],
                ReturnType = XdmValueKind.Boolean,
                Implementation = FunctionAvailable
            },
            [(Namespaces.Fn, "function-available", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "function-available", Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.Integer],
                ReturnType = XdmValueKind.Boolean,
                Implementation = FunctionAvailable
            },

            // ----- fn:element-available -----------------------------------------
            [(Namespaces.Fn, "element-available", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "element-available", Arity = 1,
                ParameterTypes = [XdmValueKind.String],
                ReturnType = XdmValueKind.Boolean,
                Implementation = ElementAvailable
            },

            // ----- fn:type-available --------------------------------------------
            [(Namespaces.Fn, "type-available", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "type-available", Arity = 1,
                ParameterTypes = [XdmValueKind.String],
                ReturnType = XdmValueKind.Boolean,
                Implementation = TypeAvailable
            },

            // ----- fn:static-base-uri -------------------------------------------
            [(Namespaces.Fn, "static-base-uri", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "static-base-uri", Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.String,
                Implementation = StaticBaseUri
            },

            // ----- fn:current-date --------------------------------------------
            [(Namespaces.Fn, "current-date", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "current-date",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.Date,
                Implementation = CurrentDate
            },

            // ----- fn:current-time --------------------------------------------
            [(Namespaces.Fn, "current-time", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "current-time",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.Time,
                Implementation = CurrentTime
            },
            [(Namespaces.Fn, "parse-ietf-date", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "parse-ietf-date", Arity = 1,
                ParameterTypes = [XdmValueKind.String],
                ReturnType = XdmValueKind.DateTime,
                Implementation = ParseIetfDate
            },
            [(Namespaces.Fn, "format-integer", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "format-integer", Arity = 2,
                ParameterTypes = [XdmValueKind.Integer, XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = FormatInteger_2
            },
            [(Namespaces.Fn, "format-integer", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "format-integer", Arity = 3,
                ParameterTypes = [XdmValueKind.Integer, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = FormatInteger_3
            },

            // ----- fn:format-number -------------------------------------------
            [(Namespaces.Fn, "format-number", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "format-number", Arity = 2,
                ParameterTypes = [XdmValueKind.Undefined, XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = FormatNumber_2
            },
            [(Namespaces.Fn, "format-number", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "format-number", Arity = 3,
                ParameterTypes = [XdmValueKind.Undefined, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = FormatNumber_3
            },
            [(Namespaces.Fn, "format-date", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "format-date", Arity = 2,
                ParameterTypes = [XdmValueKind.Date, XdmValueKind.String], ReturnType = XdmValueKind.String,
                Implementation = FormatDate_2
            },
            [(Namespaces.Fn, "format-date", 5)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "format-date", Arity = 5,
                ParameterTypes = [XdmValueKind.Date, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String], ReturnType = XdmValueKind.String,
                Implementation = FormatDate_5
            },
            [(Namespaces.Fn, "format-time", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "format-time", Arity = 2,
                ParameterTypes = [XdmValueKind.Time, XdmValueKind.String], ReturnType = XdmValueKind.String,
                Implementation = FormatTime_2
            },
            [(Namespaces.Fn, "format-time", 5)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "format-time", Arity = 5,
                ParameterTypes = [XdmValueKind.Time, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String], ReturnType = XdmValueKind.String,
                Implementation = FormatTime_5
            },
            [(Namespaces.Fn, "format-dateTime", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "format-dateTime", Arity = 2,
                ParameterTypes = [XdmValueKind.DateTime, XdmValueKind.String], ReturnType = XdmValueKind.String,
                Implementation = FormatDateTime_2
            },
            [(Namespaces.Fn, "format-dateTime", 5)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "format-dateTime", Arity = 5,
                ParameterTypes = [XdmValueKind.DateTime, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String, XdmValueKind.String], ReturnType = XdmValueKind.String,
                Implementation = FormatDateTime_5
            },

            // ----- fn:adjust-date-to-timezone ---------------------------------
            [(Namespaces.Fn, "adjust-date-to-timezone", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "adjust-date-to-timezone", Arity = 1,
                ParameterTypes = [XdmValueKind.Date], ReturnType = XdmValueKind.Date,
                Implementation = AdjustDateToTimezone_1
            },
            [(Namespaces.Fn, "adjust-date-to-timezone", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "adjust-date-to-timezone", Arity = 2,
                ParameterTypes = [XdmValueKind.Date, XdmValueKind.String], ReturnType = XdmValueKind.Date,
                Implementation = AdjustDateToTimezone_2
            },

            // ----- fn:adjust-time-to-timezone ---------------------------------
            [(Namespaces.Fn, "adjust-time-to-timezone", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "adjust-time-to-timezone", Arity = 1,
                ParameterTypes = [XdmValueKind.Time], ReturnType = XdmValueKind.Time,
                Implementation = AdjustTimeToTimezone_1
            },
            [(Namespaces.Fn, "adjust-time-to-timezone", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "adjust-time-to-timezone", Arity = 2,
                ParameterTypes = [XdmValueKind.Time, XdmValueKind.String], ReturnType = XdmValueKind.Time,
                Implementation = AdjustTimeToTimezone_2
            },

            // ----- fn:adjust-dateTime-to-timezone -----------------------------
            [(Namespaces.Fn, "adjust-dateTime-to-timezone", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "adjust-dateTime-to-timezone", Arity = 1,
                ParameterTypes = [XdmValueKind.DateTime], ReturnType = XdmValueKind.DateTime,
                Implementation = AdjustDateTimeToTimezone_1
            },
            [(Namespaces.Fn, "adjust-dateTime-to-timezone", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "adjust-dateTime-to-timezone", Arity = 2,
                ParameterTypes = [XdmValueKind.DateTime, XdmValueKind.String], ReturnType = XdmValueKind.DateTime,
                Implementation = AdjustDateTimeToTimezone_2
            },

            // ----- fn:implicit-timezone ---------------------------------------
            [(Namespaces.Fn, "implicit-timezone", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "implicit-timezone", Arity = 0,
                ParameterTypes = [], ReturnType = XdmValueKind.Duration,
                Implementation = ImplicitTimezone
            },

            // ----- fn:node-name -----------------------------------------------
            [(Namespaces.Fn, "node-name", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "node-name",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.QName,
                Implementation = NodeName_0
            },
            [(Namespaces.Fn, "node-name", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "node-name",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.QName,
                Implementation = NodeName_1
            },

            // ----- fn:number --------------------------------------------------
            [(Namespaces.Fn, "number", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "number",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.Double,
                Implementation = Number_0
            },
            [(Namespaces.Fn, "number", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "number",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Double,
                Implementation = Number_1
            },

            // ----- fn:data ----------------------------------------------------
            [(Namespaces.Fn, "data", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "data",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.Undefined,
                Implementation = Data_0
            },
            [(Namespaces.Fn, "data", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "data",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Undefined,
                Implementation = Data_1
            },

            // ----- fn:root ----------------------------------------------------
            [(Namespaces.Fn, "root", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "root",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.Node,
                Implementation = Root_0
            },
            [(Namespaces.Fn, "root", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "root",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Node,
                Implementation = Root_1
            },

            // ----- fn:*-from-dateTime -----------------------------------------
            [(Namespaces.Fn, "year-from-dateTime", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "year-from-dateTime",
                Arity = 1,
                ParameterTypes = [XdmValueKind.DateTime],
                ReturnType = XdmValueKind.Integer,
                Implementation = YearFromDateTime
            },
            [(Namespaces.Fn, "month-from-dateTime", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "month-from-dateTime",
                Arity = 1,
                ParameterTypes = [XdmValueKind.DateTime],
                ReturnType = XdmValueKind.Integer,
                Implementation = MonthFromDateTime
            },
            [(Namespaces.Fn, "day-from-dateTime", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "day-from-dateTime",
                Arity = 1,
                ParameterTypes = [XdmValueKind.DateTime],
                ReturnType = XdmValueKind.Integer,
                Implementation = DayFromDateTime
            },
            [(Namespaces.Fn, "hours-from-dateTime", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "hours-from-dateTime",
                Arity = 1,
                ParameterTypes = [XdmValueKind.DateTime],
                ReturnType = XdmValueKind.Integer,
                Implementation = HoursFromDateTime
            },
            [(Namespaces.Fn, "minutes-from-dateTime", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "minutes-from-dateTime",
                Arity = 1,
                ParameterTypes = [XdmValueKind.DateTime],
                ReturnType = XdmValueKind.Integer,
                Implementation = MinutesFromDateTime
            },
            [(Namespaces.Fn, "seconds-from-dateTime", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "seconds-from-dateTime",
                Arity = 1,
                ParameterTypes = [XdmValueKind.DateTime],
                ReturnType = XdmValueKind.Decimal,
                Implementation = SecondsFromDateTime
            },
            [(Namespaces.Fn, "timezone-from-dateTime", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "timezone-from-dateTime",
                Arity = 1,
                ParameterTypes = [XdmValueKind.DateTime],
                ReturnType = XdmValueKind.Duration,
                Implementation = TimezoneFromDateTime
            },

            // ----- fn:*-from-date ---------------------------------------------
            [(Namespaces.Fn, "year-from-date", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "year-from-date",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Date],
                ReturnType = XdmValueKind.Integer,
                Implementation = YearFromDate
            },
            [(Namespaces.Fn, "month-from-date", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "month-from-date",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Date],
                ReturnType = XdmValueKind.Integer,
                Implementation = MonthFromDate
            },
            [(Namespaces.Fn, "day-from-date", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "day-from-date",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Date],
                ReturnType = XdmValueKind.Integer,
                Implementation = DayFromDate
            },
            [(Namespaces.Fn, "timezone-from-date", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "timezone-from-date",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Date],
                ReturnType = XdmValueKind.Duration,
                Implementation = TimezoneFromDate
            },

            // ----- fn:*-from-time ---------------------------------------------
            [(Namespaces.Fn, "hours-from-time", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "hours-from-time",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Time],
                ReturnType = XdmValueKind.Integer,
                Implementation = HoursFromTime
            },
            [(Namespaces.Fn, "minutes-from-time", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "minutes-from-time",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Time],
                ReturnType = XdmValueKind.Integer,
                Implementation = MinutesFromTime
            },
            [(Namespaces.Fn, "seconds-from-time", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "seconds-from-time",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Time],
                ReturnType = XdmValueKind.Decimal,
                Implementation = SecondsFromTime
            },
            [(Namespaces.Fn, "timezone-from-time", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "timezone-from-time",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Time],
                ReturnType = XdmValueKind.Duration,
                Implementation = TimezoneFromTime
            },

            // ----- fn:*-from-duration -----------------------------------------
            [(Namespaces.Fn, "years-from-duration", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "years-from-duration", Arity = 1,
                ParameterTypes = [XdmValueKind.String], ReturnType = XdmValueKind.Integer,
                Implementation = YearsFromDuration
            },
            [(Namespaces.Fn, "months-from-duration", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "months-from-duration", Arity = 1,
                ParameterTypes = [XdmValueKind.String], ReturnType = XdmValueKind.Integer,
                Implementation = MonthsFromDuration
            },
            [(Namespaces.Fn, "days-from-duration", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "days-from-duration", Arity = 1,
                ParameterTypes = [XdmValueKind.String], ReturnType = XdmValueKind.Integer,
                Implementation = DaysFromDuration
            },
            [(Namespaces.Fn, "hours-from-duration", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "hours-from-duration", Arity = 1,
                ParameterTypes = [XdmValueKind.String], ReturnType = XdmValueKind.Integer,
                Implementation = HoursFromDuration
            },
            [(Namespaces.Fn, "minutes-from-duration", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "minutes-from-duration", Arity = 1,
                ParameterTypes = [XdmValueKind.String], ReturnType = XdmValueKind.Integer,
                Implementation = MinutesFromDuration
            },
            [(Namespaces.Fn, "seconds-from-duration", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "seconds-from-duration", Arity = 1,
                ParameterTypes = [XdmValueKind.String], ReturnType = XdmValueKind.Decimal,
                Implementation = SecondsFromDuration
            },

            // ----- fn:deep-equal ----------------------------------------------
            [(Namespaces.Fn, "deep-equal", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "deep-equal",
                Arity = 2,
                ParameterTypes = [XdmValueKind.Undefined, XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Boolean,
                Implementation = DeepEqual_2
            },
            [(Namespaces.Fn, "deep-equal", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "deep-equal",
                Arity = 3,
                ParameterTypes = [XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.String],
                ReturnType = XdmValueKind.Boolean,
                Implementation = DeepEqual_3
            },

            // ----- fn:generate-id ---------------------------------------------
            [(Namespaces.Fn, "generate-id", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "generate-id",
                Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.String,
                Implementation = GenerateId_0
            },
            [(Namespaces.Fn, "generate-id", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "generate-id",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.String,
                Implementation = GenerateId_1
            },

            // ----- fn:compare -------------------------------------------------
            [(Namespaces.Fn, "compare", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "compare",
                Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.Integer,
                Implementation = Compare_2
            },
            [(Namespaces.Fn, "compare", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "compare",
                Arity = 3,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.Integer,
                Implementation = Compare_3
            },

            // ----- fn:encode-for-uri / fn:iri-to-uri / fn:escape-html-uri -----
            [(Namespaces.Fn, "encode-for-uri", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "encode-for-uri",
                Arity = 1,
                ParameterTypes = [XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = EncodeForUri
            },
            [(Namespaces.Fn, "iri-to-uri", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "iri-to-uri",
                Arity = 1,
                ParameterTypes = [XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = IriToUri
            },
            [(Namespaces.Fn, "escape-html-uri", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "escape-html-uri",
                Arity = 1,
                ParameterTypes = [XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = EscapeHtmlUri
            },

            // ----- fn:QName / fn:resolve-QName --------------------------------
            [(Namespaces.Fn, "QName", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "QName",
                Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.QName,
                Implementation = Qname
            },
            [(Namespaces.Fn, "resolve-QName", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "resolve-QName",
                Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.Node],
                ReturnType = XdmValueKind.QName,
                Implementation = ResolveQName
            },
            [(Namespaces.Fn, "local-name-from-QName", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "local-name-from-QName", Arity = 1,
                ParameterTypes = [XdmValueKind.QName], ReturnType = XdmValueKind.String,
                Implementation = LocalNameFromQName
            },
            [(Namespaces.Fn, "namespace-uri-from-QName", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "namespace-uri-from-QName", Arity = 1,
                ParameterTypes = [XdmValueKind.QName], ReturnType = XdmValueKind.String,
                Implementation = NamespaceUriFromQName
            },
            [(Namespaces.Fn, "prefix-from-QName", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "prefix-from-QName", Arity = 1,
                ParameterTypes = [XdmValueKind.QName], ReturnType = XdmValueKind.String,
                Implementation = PrefixFromQName
            },
            // ----- fn:for-each, fn:filter, fn:fold-left, fn:fold-right, fn:for-each-pair -----
            [(Namespaces.Fn, "function-name", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "function-name", Arity = 1,
                ParameterTypes = [XdmValueKind.Function], ReturnType = XdmValueKind.QName,
                Implementation = FunctionName
            },
            [(Namespaces.Fn, "function-arity", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "function-arity", Arity = 1,
                ParameterTypes = [XdmValueKind.Function], ReturnType = XdmValueKind.Integer,
                Implementation = FunctionArity
            },
            [(Namespaces.Fn, "for-each", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "for-each",
                Arity = 2,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.Function],
                ReturnType = XdmValueKind.Sequence,
                Implementation = ForEach_2
            },
            [(Namespaces.Fn, "filter", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "filter",
                Arity = 2,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.Function],
                ReturnType = XdmValueKind.Sequence,
                Implementation = Filter_2
            },
            [(Namespaces.Fn, "fold-left", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "fold-left",
                Arity = 3,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.Undefined, XdmValueKind.Function],
                ReturnType = XdmValueKind.Undefined,
                Implementation = FoldLeft_3
            },
            [(Namespaces.Fn, "fold-right", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "fold-right",
                Arity = 3,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.Undefined, XdmValueKind.Function],
                ReturnType = XdmValueKind.Undefined,
                Implementation = FoldRight_3
            },
            [(Namespaces.Fn, "for-each-pair", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "for-each-pair",
                Arity = 3,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.Sequence, XdmValueKind.Function],
                ReturnType = XdmValueKind.Sequence,
                Implementation = ForEachPair_3
            },
            [(Namespaces.Fn, "sort", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "sort", Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Sequence,
                Implementation = Sort_1
            },
            [(Namespaces.Fn, "sort", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "sort", Arity = 2,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Sequence,
                Implementation = Sort_2
            },
            [(Namespaces.Fn, "sort", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "sort", Arity = 3,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.Undefined, XdmValueKind.Function],
                ReturnType = XdmValueKind.Sequence,
                Implementation = Sort_3
            },
            [(Namespaces.Fn, "innermost", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "innermost", Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Sequence,
                Implementation = Innermost
            },
            [(Namespaces.Fn, "outermost", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "outermost", Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Sequence,
                Implementation = Outermost
            },
            [(Namespaces.Fn, "snapshot", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "snapshot", Arity = 1,
                ParameterTypes = [XdmValueKind.Node],
                ReturnType = XdmValueKind.Node,
                Implementation = Snapshot
            },
            [(Namespaces.Fn, "resolve-uri", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "resolve-uri", Arity = 1,
                ParameterTypes = [XdmValueKind.String],
                ReturnType = XdmValueKind.Uri,
                Implementation = ResolveUri_1
            },
            [(Namespaces.Fn, "resolve-uri", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "resolve-uri", Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.Uri,
                Implementation = ResolveUri_2
            },
            // ----- xs:* constructor functions ---------------------------------
            [(Namespaces.Xs, "string", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "string", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.String,
                Implementation = XsString
            },
            [(Namespaces.Xs, "integer", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "integer", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Integer,
                Implementation = XsInteger
            },
            [(Namespaces.Xs, "decimal", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "decimal", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Decimal,
                Implementation = XsDecimal
            },
            [(Namespaces.Xs, "double", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "double", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Double,
                Implementation = XsDouble
            },
            [(Namespaces.Xs, "float", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "float", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Float,
                Implementation = XsFloat
            },
            [(Namespaces.Xs, "boolean", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "boolean", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Boolean,
                Implementation = XsBoolean
            },
            [(Namespaces.Xs, "dateTime", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "dateTime", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.DateTime,
                Implementation = XsDateTime
            },
            [(Namespaces.Xs, "date", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "date", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Date,
                Implementation = XsDate
            },
            [(Namespaces.Xs, "time", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "time", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Time,
                Implementation = XsTime
            },
            [(Namespaces.Xs, "QName", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "QName", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.QName,
                Implementation = XsQNameConstructor
            },
            [(Namespaces.Xs, "byte", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "byte", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Integer,
                Implementation = XsByte
            },
            [(Namespaces.Xs, "short", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "short", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Integer,
                Implementation = XsShort
            },
            [(Namespaces.Xs, "int", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "int", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Integer,
                Implementation = XsInt
            },
            [(Namespaces.Xs, "long", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "long", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Integer,
                Implementation = XsLong
            },
            [(Namespaces.Xs, "unsignedByte", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "unsignedByte", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Integer,
                Implementation = XsUnsignedByte
            },
            [(Namespaces.Xs, "unsignedShort", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "unsignedShort", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Integer,
                Implementation = XsUnsignedShort
            },
            [(Namespaces.Xs, "unsignedInt", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "unsignedInt", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Integer,
                Implementation = XsUnsignedInt
            },
            [(Namespaces.Xs, "unsignedLong", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "unsignedLong", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Integer,
                Implementation = XsUnsignedLong
            },
            [(Namespaces.Xs, "positiveInteger", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "positiveInteger", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Integer,
                Implementation = XsPositiveInteger
            },
            [(Namespaces.Xs, "negativeInteger", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "negativeInteger", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Integer,
                Implementation = XsNegativeInteger
            },
            [(Namespaces.Xs, "nonPositiveInteger", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "nonPositiveInteger", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Integer,
                Implementation = XsNonPositiveInteger
            },
            [(Namespaces.Xs, "nonNegativeInteger", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "nonNegativeInteger", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Integer,
                Implementation = XsNonNegativeInteger
            },
            [(Namespaces.Xs, "dayTimeDuration", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "dayTimeDuration", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.String,
                Implementation = XsDayTimeDuration
            },
            [(Namespaces.Xs, "yearMonthDuration", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "yearMonthDuration", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.String,
                Implementation = XsYearMonthDuration
            },
            [(Namespaces.Xs, "untypedAtomic", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "untypedAtomic", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.String,
                Implementation = XsUntypedAtomic
            },
            [(Namespaces.Xs, "anyURI", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "anyURI", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.String,
                Implementation = XsAnyUri
            },
            [(Namespaces.Xs, "hexBinary", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "hexBinary", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.String,
                Implementation = XsHexBinary
            },
            [(Namespaces.Xs, "base64Binary", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "base64Binary", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.String,
                Implementation = XsBase64Binary
            },
            [(Namespaces.Xs, "gDay", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "gDay", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.String,
                Implementation = XsGDay
            },
            [(Namespaces.Xs, "gMonth", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "gMonth", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.String,
                Implementation = XsGMonth
            },
            [(Namespaces.Xs, "gYear", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "gYear", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.String,
                Implementation = XsGYear
            },
            [(Namespaces.Xs, "gYearMonth", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "gYearMonth", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.String,
                Implementation = XsGYearMonth
            },
            [(Namespaces.Xs, "gMonthDay", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "gMonthDay", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.String,
                Implementation = XsGMonthDay
            },
            [(Namespaces.Xs, "NCName", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "NCName", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.String,
                Implementation = XsNCName
            },
            [(Namespaces.Xs, "duration", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "duration", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.String,
                Implementation = XsDuration
            },
            [(Namespaces.Xs, "language", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "language", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.String,
                Implementation = XsLanguage
            },
            [(Namespaces.Xs, "Name", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "Name", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.String,
                Implementation = XsName
            },
            [(Namespaces.Xs, "normalizedString", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "normalizedString", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.String,
                Implementation = XsNormalizedString
            },
            [(Namespaces.Xs, "token", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "token", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.String,
                Implementation = XsToken
            },
            [(Namespaces.Xs, "ID", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "ID", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.String,
                Implementation = XsID
            },
            [(Namespaces.Xs, "IDREF", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "IDREF", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.String,
                Implementation = XsIDREF
            },
            [(Namespaces.Xs, "NMTOKEN", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "NMTOKEN", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.String,
                Implementation = XsNMTOKEN
            },
            [(Namespaces.Xs, "ENTITY", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "ENTITY", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.String,
                Implementation = XsENTITY
            },
            [(Namespaces.Xs, "IDREFS", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "IDREFS", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Sequence,
                Implementation = XsIDREFS
            },
            [(Namespaces.Xs, "NMTOKENS", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "NMTOKENS", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Sequence,
                Implementation = XsNMTOKENS
            },
            [(Namespaces.Xs, "ENTITIES", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "ENTITIES", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Sequence,
                Implementation = XsENTITIES
            },
            // ----- math:* functions -------------------------------------------
            [(Namespaces.Math, "pi", 0)] = new()
            {
                NamespaceUri = Namespaces.Math, LocalName = "pi", Arity = 0,
                ParameterTypes = [], ReturnType = XdmValueKind.Double,
                Implementation = MathPi
            },
            [(Namespaces.Math, "sin", 1)] = new()
            {
                NamespaceUri = Namespaces.Math, LocalName = "sin", Arity = 1,
                ParameterTypes = [XdmValueKind.Double], ReturnType = XdmValueKind.Double,
                Implementation = MathSin
            },
            [(Namespaces.Math, "cos", 1)] = new()
            {
                NamespaceUri = Namespaces.Math, LocalName = "cos", Arity = 1,
                ParameterTypes = [XdmValueKind.Double], ReturnType = XdmValueKind.Double,
                Implementation = MathCos
            },
            [(Namespaces.Math, "tan", 1)] = new()
            {
                NamespaceUri = Namespaces.Math, LocalName = "tan", Arity = 1,
                ParameterTypes = [XdmValueKind.Double], ReturnType = XdmValueKind.Double,
                Implementation = MathTan
            },
            [(Namespaces.Math, "pow", 2)] = new()
            {
                NamespaceUri = Namespaces.Math, LocalName = "pow", Arity = 2,
                ParameterTypes = [XdmValueKind.Double, XdmValueKind.Double], ReturnType = XdmValueKind.Double,
                Implementation = MathPow
            },
            [(Namespaces.Math, "sqrt", 1)] = new()
            {
                NamespaceUri = Namespaces.Math, LocalName = "sqrt", Arity = 1,
                ParameterTypes = [XdmValueKind.Double], ReturnType = XdmValueKind.Double,
                Implementation = MathSqrt
            },
            [(Namespaces.Math, "exp", 1)] = new()
            {
                NamespaceUri = Namespaces.Math, LocalName = "exp", Arity = 1,
                ParameterTypes = [XdmValueKind.Double], ReturnType = XdmValueKind.Double,
                Implementation = MathExp
            },
            [(Namespaces.Math, "log", 1)] = new()
            {
                NamespaceUri = Namespaces.Math, LocalName = "log", Arity = 1,
                ParameterTypes = [XdmValueKind.Double], ReturnType = XdmValueKind.Double,
                Implementation = MathLog
            },
            [(Namespaces.Math, "log10", 1)] = new()
            {
                NamespaceUri = Namespaces.Math, LocalName = "log10", Arity = 1,
                ParameterTypes = [XdmValueKind.Double], ReturnType = XdmValueKind.Double,
                Implementation = MathLog10
            },
            [(Namespaces.Math, "exp10", 1)] = new()
            {
                NamespaceUri = Namespaces.Math, LocalName = "exp10", Arity = 1,
                ParameterTypes = [XdmValueKind.Double], ReturnType = XdmValueKind.Double,
                Implementation = MathExp10
            },
            [(Namespaces.Math, "asin", 1)] = new()
            {
                NamespaceUri = Namespaces.Math, LocalName = "asin", Arity = 1,
                ParameterTypes = [XdmValueKind.Double], ReturnType = XdmValueKind.Double,
                Implementation = MathAsin
            },
            [(Namespaces.Math, "acos", 1)] = new()
            {
                NamespaceUri = Namespaces.Math, LocalName = "acos", Arity = 1,
                ParameterTypes = [XdmValueKind.Double], ReturnType = XdmValueKind.Double,
                Implementation = MathAcos
            },
            [(Namespaces.Math, "atan", 1)] = new()
            {
                NamespaceUri = Namespaces.Math, LocalName = "atan", Arity = 1,
                ParameterTypes = [XdmValueKind.Double], ReturnType = XdmValueKind.Double,
                Implementation = MathAtan
            },
            [(Namespaces.Math, "atan2", 2)] = new()
            {
                NamespaceUri = Namespaces.Math, LocalName = "atan2", Arity = 2,
                ParameterTypes = [XdmValueKind.Double, XdmValueKind.Double], ReturnType = XdmValueKind.Double,
                Implementation = MathAtan2
            },
            [(Namespaces.Fn, "function-lookup", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "function-lookup", Arity = 2,
                ParameterTypes = [XdmValueKind.QName, XdmValueKind.Integer], ReturnType = XdmValueKind.Function,
                Implementation = FunctionLookup
            },
            // ----- fn:error ---------------------------------------------------
            [(Namespaces.Fn, "doc", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "doc", Arity = 1,
                ParameterTypes = [XdmValueKind.String], ReturnType = XdmValueKind.Node,
                Implementation = Doc_1
            },
            [(Namespaces.Fn, "document", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "document", Arity = 1,
                ParameterTypes = [XdmValueKind.String], ReturnType = XdmValueKind.Node,
                Implementation = Document_1
            },
            [(Namespaces.Fn, "document", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "document", Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.Node], ReturnType = XdmValueKind.Node,
                Implementation = Document_2
            },
            [(Namespaces.Fn, "doc-available", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "doc-available", Arity = 1,
                ParameterTypes = [XdmValueKind.String], ReturnType = XdmValueKind.Boolean,
                Implementation = DocAvailable_1
            },
            [(Namespaces.Fn, "id", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "id", Arity = 1,
                ParameterTypes = [XdmValueKind.String], ReturnType = XdmValueKind.Sequence,
                Implementation = Id_1
            },
            [(Namespaces.Fn, "id", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "id", Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.Node], ReturnType = XdmValueKind.Sequence,
                Implementation = Id_2
            },
            [(Namespaces.Fn, "element-with-id", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "element-with-id", Arity = 1,
                ParameterTypes = [XdmValueKind.String], ReturnType = XdmValueKind.Sequence,
                Implementation = ElementWithId_1
            },
            [(Namespaces.Fn, "default-language", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "default-language", Arity = 0,
                ParameterTypes = [], ReturnType = XdmValueKind.String,
                Implementation = DefaultLanguage_0
            },
            [(Namespaces.Fn, "collection", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "collection", Arity = 0,
                ParameterTypes = [], ReturnType = XdmValueKind.Sequence,
                Implementation = Collection_0
            },
            [(Namespaces.Fn, "collection", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "collection", Arity = 1,
                ParameterTypes = [XdmValueKind.String], ReturnType = XdmValueKind.Sequence,
                Implementation = Collection_1
            },
            [(Namespaces.Fn, "unparsed-text", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "unparsed-text", Arity = 1,
                ParameterTypes = [XdmValueKind.String], ReturnType = XdmValueKind.String,
                Implementation = UnparsedText_1
            },
            [(Namespaces.Fn, "unparsed-text", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "unparsed-text", Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String], ReturnType = XdmValueKind.String,
                Implementation = UnparsedText_2
            },
            [(Namespaces.Fn, "unparsed-text-available", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "unparsed-text-available", Arity = 1,
                ParameterTypes = [XdmValueKind.String], ReturnType = XdmValueKind.Boolean,
                Implementation = UnparsedTextAvailable_1
            },
            [(Namespaces.Fn, "unparsed-text-available", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "unparsed-text-available", Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String], ReturnType = XdmValueKind.Boolean,
                Implementation = UnparsedTextAvailable_2
            },
            [(Namespaces.Fn, "unparsed-text-lines", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "unparsed-text-lines", Arity = 1,
                ParameterTypes = [XdmValueKind.String], ReturnType = XdmValueKind.Sequence,
                Implementation = UnparsedTextLines_1
            },
            [(Namespaces.Fn, "unparsed-text-lines", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "unparsed-text-lines", Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String], ReturnType = XdmValueKind.Sequence,
                Implementation = UnparsedTextLines_2
            },
            [(Namespaces.Fn, "random-number-generator", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "random-number-generator", Arity = 0,
                ParameterTypes = [], ReturnType = XdmValueKind.Map,
                Implementation = RandomNumberGenerator_0
            },
            [(Namespaces.Fn, "random-number-generator", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "random-number-generator", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Map,
                Implementation = RandomNumberGenerator_1
            },
            [(Namespaces.Fn, "serialize", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "serialize", Arity = 2,
                ParameterTypes = [XdmValueKind.Undefined, XdmValueKind.Map], ReturnType = XdmValueKind.String,
                Implementation = Serialize_2
            },
            // ----- fn:trace ---------------------------------------------------
            [(Namespaces.Fn, "trace", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "trace", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Undefined,
                Implementation = Trace_1
            },
            [(Namespaces.Fn, "trace", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "trace", Arity = 2,
                ParameterTypes = [XdmValueKind.Undefined, XdmValueKind.String],
                ReturnType = XdmValueKind.Undefined,
                Implementation = Trace_2
            },

            // ----- fn:boolean -------------------------------------------------
            [(Namespaces.Fn, "boolean", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "boolean", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Boolean,
                Implementation = Boolean_1
            },

            // ----- fn:zero-or-one / fn:one-or-more / fn:exactly-one -----------
            [(Namespaces.Fn, "zero-or-one", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "zero-or-one", Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Undefined,
                Implementation = ZeroOrOne_1
            },
            [(Namespaces.Fn, "one-or-more", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "one-or-more", Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Sequence,
                Implementation = OneOrMore_1
            },
            [(Namespaces.Fn, "exactly-one", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "exactly-one", Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence],
                ReturnType = XdmValueKind.Undefined,
                Implementation = ExactlyOne_1
            },

            // ----- fn:base-uri ------------------------------------------------
            [(Namespaces.Fn, "base-uri", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "base-uri", Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.String,
                Implementation = BaseUri_0
            },
            [(Namespaces.Fn, "base-uri", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "base-uri", Arity = 1,
                ParameterTypes = [XdmValueKind.Node],
                ReturnType = XdmValueKind.String,
                Implementation = BaseUri_1
            },

            // ----- fn:document-uri --------------------------------------------
            [(Namespaces.Fn, "document-uri", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "document-uri", Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.String,
                Implementation = DocumentUri_0
            },
            [(Namespaces.Fn, "document-uri", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "document-uri", Arity = 1,
                ParameterTypes = [XdmValueKind.Node],
                ReturnType = XdmValueKind.String,
                Implementation = DocumentUri_1
            },

            // ----- fn:nilled --------------------------------------------------
            [(Namespaces.Fn, "nilled", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "nilled", Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.Boolean,
                Implementation = Nilled_0
            },
            [(Namespaces.Fn, "nilled", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "nilled", Arity = 1,
                ParameterTypes = [XdmValueKind.Node],
                ReturnType = XdmValueKind.Boolean,
                Implementation = Nilled_1
            },

            [(Namespaces.Fn, "error", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "error", Arity = 0,
                ParameterTypes = [], ReturnType = XdmValueKind.Undefined,
                Implementation = Error_0
            },
            [(Namespaces.Fn, "error", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "error", Arity = 1,
                ParameterTypes = [XdmValueKind.QName], ReturnType = XdmValueKind.Undefined,
                Implementation = Error_1
            },
            [(Namespaces.Fn, "error", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "error", Arity = 2,
                ParameterTypes = [XdmValueKind.QName, XdmValueKind.String], ReturnType = XdmValueKind.Undefined,
                Implementation = Error_2
            },
            [(Namespaces.Fn, "error", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "error", Arity = 3,
                ParameterTypes = [XdmValueKind.QName, XdmValueKind.String, XdmValueKind.Undefined], ReturnType = XdmValueKind.Undefined,
                Implementation = Error_3
            },

            // ----- fn:parse-json ----------------------------------------------
            [(Namespaces.Fn, "parse-json", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "parse-json", Arity = 1,
                ParameterTypes = [XdmValueKind.String],
                ReturnType = XdmValueKind.Undefined,
                Implementation = ParseJson_1
            },
            [(Namespaces.Fn, "parse-json", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "parse-json", Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.Map],
                ReturnType = XdmValueKind.Undefined,
                Implementation = ParseJson_2
            },

            // ----- fn:json-to-xml ---------------------------------------------
            [(Namespaces.Fn, "json-to-xml", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "json-to-xml", Arity = 1,
                ParameterTypes = [XdmValueKind.String],
                ReturnType = XdmValueKind.Node,
                Implementation = JsonToXml_1
            },
            [(Namespaces.Fn, "json-to-xml", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "json-to-xml", Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.Map],
                ReturnType = XdmValueKind.Node,
                Implementation = JsonToXml_2
            },

            // ----- fn:xml-to-json ---------------------------------------------
            [(Namespaces.Fn, "xml-to-json", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "xml-to-json", Arity = 1,
                ParameterTypes = [XdmValueKind.Node],
                ReturnType = XdmValueKind.String,
                Implementation = XmlToJson_1
            },
            [(Namespaces.Fn, "xml-to-json", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "xml-to-json", Arity = 2,
                ParameterTypes = [XdmValueKind.Node, XdmValueKind.Map],
                ReturnType = XdmValueKind.String,
                Implementation = XmlToJson_2
            },

            // ----- fn:json-doc ------------------------------------------------
            [(Namespaces.Fn, "json-doc", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "json-doc", Arity = 1,
                ParameterTypes = [XdmValueKind.String],
                ReturnType = XdmValueKind.Undefined,
                Implementation = JsonDoc_1
            },
            [(Namespaces.Fn, "json-doc", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "json-doc", Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.Map],
                ReturnType = XdmValueKind.Undefined,
                Implementation = JsonDoc_2
            },

            // ----- fn:copy-of (XSLT 3.0) ------------------------------------
            [(Namespaces.Fn, "copy-of", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "copy-of", Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.Node,
                Implementation = CopyOf_0
            },
            [(Namespaces.Fn, "copy-of", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "copy-of", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Node,
                Implementation = CopyOf_1
            },

            // ----- fn:in-scope-prefixes / fn:namespace-uri-for-prefix --------
            [(Namespaces.Fn, "in-scope-prefixes", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "in-scope-prefixes", Arity = 1,
                ParameterTypes = [XdmValueKind.Node],
                ReturnType = XdmValueKind.Sequence,
                Implementation = InScopePrefixes
            },
            [(Namespaces.Fn, "namespace-uri-for-prefix", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "namespace-uri-for-prefix", Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.Node],
                ReturnType = XdmValueKind.String,
                Implementation = NamespaceUriForPrefix
            },
        };

        StandardFunctions = functions.ToFrozenDictionary();
    }

    /// <summary>
    /// Populates the evaluation context with all standard functions.
    /// </summary>
    public static void Populate(EvaluationContext context)
    {
        foreach (var sig in StandardFunctions.Values)
        {
            // XSLT-defined functions that depend on the dynamic evaluation context are
            // not available in a static (use-when / shadow attribute) context.
            if (context.IsStaticEvaluation && sig.NamespaceUri == Namespaces.Fn && XsltDynamicFunctions.Contains(sig.LocalName))
                continue;
            context.RegisterFunction(sig);
        }

        // Set up default document loader if not already configured
        if (context.DocumentLoader is null)
        {
            context.DocumentLoader = uri =>
            {
                var doc = XDocument.Load(uri);
                return doc.ToXdmNode();
            };
        }
    }

    /// <summary>
    /// Attempts to resolve a standard function by qualified name and arity.
    /// </summary>
    public static bool TryGetFunction(string namespaceUri, string localName, int arity, out FunctionSignature signature)
        => StandardFunctions.TryGetValue((namespaceUri, localName, arity), out signature!);

    // ------------------------------------------------------------------
    // Implementations
    // ------------------------------------------------------------------

    private static XdmValue String_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var item = ctx.ContextItem;
        if (item.IsUndefined)
            throw new InvalidOperationException("fn:string() called with no context item.");
        if (item.IsFunction || item.IsArray || item.IsMap)
            throw new InvalidOperationException("FOTY0014");
        return XdmValue.FromString(item.ToString());
    }

    private static XdmValue String_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (arg.IsUndefined)
            return XdmValue.FromString(string.Empty);
        if (arg.IsFunction || arg.IsArray || arg.IsMap)
            throw new InvalidOperationException("FOTY0014");
        if (arg.IsSequence)
        {
            var items = new List<XdmValue>();
            foreach (var item in XdmSequence.FromSource(arg.SequenceValue!))
                items.Add(item);
            if (items.Count == 0)
                return XdmValue.FromString(string.Empty);
            if (items.Count > 1)
                throw new InvalidOperationException("XPTY0004");
            return XdmValue.FromString(items[0].ToString());
        }
        return XdmValue.FromString(arg.ToString());
    }

    private static XdmValue Count(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var seq = args[0];
        if (seq.IsUndefined)
            return XdmValue.FromInteger(0);
        if (!seq.IsSequence)
            return XdmValue.FromInteger(1);

        if (seq.SequenceValue is IntegerRangeSequence range)
            return XdmValue.FromInteger(range.To - range.From + 1);

        if (seq.SequenceValue!.TryGetLength(out var len))
            return XdmValue.FromInteger(len);

        // Materialize to count
        long count = 0;
        foreach (var _ in XdmSequence.FromSource(seq.SequenceValue!))
            count++;
        return XdmValue.FromInteger(count);
    }

    private static XdmValue Exists(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (arg.IsUndefined)
            return XdmValue.FromBoolean(false);
        if (arg.IsSequence && arg.SequenceValue is not null && arg.SequenceValue.TryGetLength(out var len))
            return XdmValue.FromBoolean(len > 0);
        return XdmValue.FromBoolean(true);
    }

    private static XdmValue Empty(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (arg.IsUndefined)
            return XdmValue.FromBoolean(true);
        if (arg.IsSequence && arg.SequenceValue is not null && arg.SequenceValue.TryGetLength(out var len))
            return XdmValue.FromBoolean(len == 0);
        return XdmValue.FromBoolean(false);
    }

    private static XdmValue Head(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var seq = args[0];
        if (!seq.IsSequence)
            return seq;

        foreach (var item in XdmSequence.FromSource(seq.SequenceValue!))
            return item;

        return XdmValue.Undefined;
    }

    private static XdmValue Tail(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var seq = args[0];
        if (!seq.IsSequence)
            return XdmValue.Undefined;

        // TODO: Return a lazy sequence view skipping the first item.
        // For now, materialize.
        var list = new List<XdmValue>();
        bool first = true;
        foreach (var item in XdmSequence.FromSource(seq.SequenceValue!))
        {
            if (first) { first = false; continue; }
            list.Add(item);
        }
        return XdmValue.FromSequence(Bosak.XPath.Core.Xdm.MaterializedSequence.FromList(list));
    }

    // ------------------------------------------------------------------
    // Higher-order functions
    // ------------------------------------------------------------------

    private static IEnumerable<XdmValue> AsSequence(XdmValue value)
    {
        if (value.IsUndefined)
            yield break;
        if (value.IsSequence)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                yield return item;
        }
        else
        {
            yield return value;
        }
    }

    private static void AppendResult(XdmValue result, List<XdmValue> target)
    {
        if (result.IsUndefined)
            return;
        if (result.IsSequence)
        {
            foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
                target.Add(item);
        }
        else
        {
            target.Add(result);
        }
    }

    private static XdmValue FunctionName(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var funcValue = args[0].FunctionValue;
        if (funcValue is NamedFunctionItem named)
            return XdmValue.FromQName(new XsQName(named.LocalName, named.NamespaceUri, GetStandardPrefix(named.NamespaceUri)));
        if (funcValue is CurriedFunctionItem curried)
        {
            // Walk to the base named function
            FunctionItem baseFunc = curried.BaseFunction;
            while (baseFunc is CurriedFunctionItem cf)
                baseFunc = cf.BaseFunction;
            if (baseFunc is NamedFunctionItem nm)
                return XdmValue.FromQName(new XsQName(nm.LocalName, nm.NamespaceUri, GetStandardPrefix(nm.NamespaceUri)));
        }
        return XdmValue.Undefined;
    }

    private static string GetStandardPrefix(string namespaceUri) => namespaceUri switch
    {
        Namespaces.Fn => "fn",
        Namespaces.Math => "math",
        Namespaces.Map => "map",
        Namespaces.Array => "array",
        _ => ""
    };

    private static XdmValue FunctionArity(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var funcValue = args[0].FunctionValue;
        if (funcValue is NamedFunctionItem named)
            return XdmValue.FromInteger(named.ArityValue);
        if (funcValue is InlineFunctionItem inline)
            return XdmValue.FromInteger(inline.Parameters.Count);
        if (funcValue is CurriedFunctionItem curried)
            return XdmValue.FromInteger(curried.Arity);
        return XdmValue.Undefined;
    }

    private static XdmValue ForEach_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var func = args[1];
        var result = new List<XdmValue>();
        foreach (var item in AsSequence(args[0]))
        {
            AppendResult(VmEngine.InvokeFunctionItem(func, ctx, new[] { item }), result);
        }
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static XdmValue Filter_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var func = args[1];
        var result = new List<XdmValue>();
        foreach (var item in AsSequence(args[0]))
        {
            var pred = VmEngine.InvokeFunctionItem(func, ctx, new[] { item });
            if (pred.IsUndefined)
                throw new InvalidOperationException("XPTY0004");
            if (pred.Kind != XdmValueKind.Boolean)
                throw new InvalidOperationException("XPTY0004");
            if (pred.BooleanValue)
                result.Add(item);
        }
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static XdmValue FoldLeft_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var func = args[2];
        var accumulator = args[1];
        foreach (var item in AsSequence(args[0]))
        {
            accumulator = VmEngine.InvokeFunctionItem(func, ctx, new[] { accumulator, item });
        }
        return accumulator;
    }

    private static XdmValue FoldRight_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var func = args[2];
        var items = AsSequence(args[0]).ToList();
        var accumulator = args[1];
        for (int i = items.Count - 1; i >= 0; i--)
        {
            accumulator = VmEngine.InvokeFunctionItem(func, ctx, new[] { items[i], accumulator });
        }
        return accumulator;
    }

    private static XdmValue ForEachPair_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var func = args[2];
        int arity = GetFunctionArity(func);
        if (arity != 2)
            throw new InvalidOperationException("XPTY0004");
        var seq1 = AsSequence(args[0]).ToList();
        var seq2 = AsSequence(args[1]).ToList();
        var result = new List<XdmValue>();
        int minLen = Math.Min(seq1.Count, seq2.Count);
        for (int i = 0; i < minLen; i++)
        {
            AppendResult(VmEngine.InvokeFunctionItem(func, ctx, new[] { seq1[i], seq2[i] }), result);
        }
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static int GetFunctionArity(XdmValue func)
    {
        if (func.IsFunction)
        {
            var fi = func.FunctionValue;
            if (fi is NamedFunctionItem named) return named.ArityValue;
            if (fi is InlineFunctionItem inline) return inline.Parameters.Count;
            if (fi is CurriedFunctionItem curried) return curried.Arity;
            if (fi is DelegateFunctionItem del) return del.Arity;
        }
        return -1;
    }

    private static XdmValue Sort_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => Sort(ctx, args[0], null, null);

    private static XdmValue Sort_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => Sort(ctx, args[0], args[1], null);

    private static XdmValue Sort_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => Sort(ctx, args[0], args[1], args[2]);

    private static XdmValue Sort(EvaluationContext ctx, XdmValue input, XdmValue? collation, XdmValue? keyFunc)
    {
        var items = AsSequence(input).ToList();
        string? collationUri = collation is not null && !collation.Value.IsUndefined ? collation.ToString() : null;
        var keyed = new List<(XdmValue Key, XdmValue Item, int Index)>();
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var key = keyFunc is not null && !keyFunc.Value.IsUndefined
                ? Data(VmEngine.InvokeFunctionItem(keyFunc.Value, ctx, new[] { item }))
                : Data(item);
            keyed.Add((key, item, i));
        }
        keyed.Sort((a, b) =>
        {
            int cmp = CompareSortKeys(a.Key, b.Key, collationUri);
            return cmp != 0 ? cmp : a.Index.CompareTo(b.Index);
        });
        items = keyed.Select(k => k.Item).ToList();
        return XdmValue.FromSequence(MaterializedSequence.FromList(items));
    }

    private static int CompareSortKeys(XdmValue a, XdmValue b, string? collation = null)
    {
        var itemsA = Materialize(a);
        var itemsB = Materialize(b);
        int minLen = Math.Min(itemsA.Count, itemsB.Count);
        for (int i = 0; i < minLen; i++)
        {
            int cmp = CompareSortItem(itemsA[i], itemsB[i], collation);
            if (cmp != 0) return cmp;
        }
        return itemsA.Count.CompareTo(itemsB.Count);
    }

    private static int CompareSortItem(XdmValue a, XdmValue b, string? collation)
    {
        if (a.Kind == XdmValueKind.String && b.Kind == XdmValueKind.String && collation is not null)
            return CompareStrings(a.StringValue, b.StringValue, collation);
        return XdmValueComparer.Instance.Compare(a, b);
    }

    private static XdmValue Innermost(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var nodes = AsSequence(args[0]).Where(v => v.IsNode).Select(v => v.NodeValue!).ToList();
        var result = new List<XdmValue>();
        foreach (var node in nodes)
        {
            bool hasDescendantInSet = false;
            foreach (var other in nodes)
            {
                if (other.IsSameNode(node)) continue;
                if (IsDescendant(other, node))
                {
                    hasDescendantInSet = true;
                    break;
                }
            }
            if (!hasDescendantInSet)
                result.Add(XdmValue.FromNode(node));
        }
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static XdmValue Outermost(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var nodes = AsSequence(args[0]).Where(v => v.IsNode).Select(v => v.NodeValue!).ToList();
        var result = new List<XdmValue>();
        foreach (var node in nodes)
        {
            bool hasAncestorInSet = false;
            var current = node.Parent;
            while (current is not null)
            {
                if (nodes.Any(n => n.IsSameNode(current)))
                {
                    hasAncestorInSet = true;
                    break;
                }
                current = current.Parent;
            }
            if (!hasAncestorInSet)
                result.Add(XdmValue.FromNode(node));
        }
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static bool IsDescendant(IXdmNode? descendant, IXdmNode ancestor)
    {
        var current = descendant?.Parent;
        while (current is not null)
        {
            if (current.IsSameNode(ancestor))
                return true;
            current = current.Parent;
        }
        return false;
    }

    private static XdmValue Snapshot(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (arg.IsUndefined)
            return XdmValue.Undefined;

        var nodes = new List<IXdmNode>();
        if (arg.IsNode && arg.NodeValue != null)
        {
            nodes.Add(arg.NodeValue);
        }
        else if (arg.IsSequence && arg.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(arg.SequenceValue))
            {
                if (item.IsNode && item.NodeValue != null)
                    nodes.Add(item.NodeValue);
            }
        }

        if (nodes.Count == 0)
            return XdmValue.Undefined;

        var copies = new List<XdmValue>();
        foreach (var node in nodes)
        {
            var copied = SnapshotNode(node);
            if (copied == null)
                throw new InvalidOperationException("FOTY0013");
            copies.Add(XdmValue.FromNode(copied));
        }

        if (copies.Count == 1)
            return copies[0];
        return XdmValue.FromSequence(MaterializedSequence.FromList(copies));
    }

    private static IXdmNode? SnapshotNode(IXdmNode node)
    {
        if (node is not Providers.Xml.XDocumentNode xdocNode)
            return null;

        // Collect ancestor path from root to the target node.
        var path = new List<IXdmNode>();
        var current = node;
        while (current != null)
        {
            path.Add(current);
            current = current.Parent;
        }
        path.Reverse();

        if (path.Count == 0)
            return null;

        XContainer? container = null;
        XObject? targetCopy = null;

        for (int i = 0; i < path.Count; i++)
        {
            var original = path[i];
            bool isLast = i == path.Count - 1;

            switch (original.NodeKind)
            {
                case XdmNodeKind.Document:
                    var docCopy = new XDocument();
                    container = docCopy;
                    targetCopy = docCopy;
                    break;

                case XdmNodeKind.Element:
                    if (((Providers.Xml.XDocumentNode)original).UnderlyingObject is not XElement elem)
                        continue;
                    var elemCopy = ShallowCopyElement(elem);
                    container?.Add(elemCopy);
                    container = elemCopy;
                    if (isLast)
                        targetCopy = elemCopy;
                    break;

                case XdmNodeKind.Attribute:
                    if (isLast && ((Providers.Xml.XDocumentNode)original).UnderlyingObject is XAttribute attr)
                    {
                        var attrCopy = new XAttribute(XName.Get(attr.Name.LocalName, attr.Name.NamespaceName), attr.Value);
                        container?.Add(attrCopy);
                        targetCopy = attrCopy;
                    }
                    break;

                case XdmNodeKind.Text:
                    if (isLast && ((Providers.Xml.XDocumentNode)original).UnderlyingObject is XText text)
                    {
                        var textCopy = new XText(text.Value);
                        container?.Add(textCopy);
                        targetCopy = textCopy;
                    }
                    break;

                case XdmNodeKind.Comment:
                    if (isLast && ((Providers.Xml.XDocumentNode)original).UnderlyingObject is XComment comment)
                    {
                        var commentCopy = new XComment(comment.Value);
                        container?.Add(commentCopy);
                        targetCopy = commentCopy;
                    }
                    break;

                case XdmNodeKind.ProcessingInstruction:
                    if (isLast && ((Providers.Xml.XDocumentNode)original).UnderlyingObject is XProcessingInstruction pi)
                    {
                        var piCopy = new XProcessingInstruction(pi.Target, pi.Data);
                        container?.Add(piCopy);
                        targetCopy = piCopy;
                    }
                    break;

                case XdmNodeKind.Namespace:
                    // Namespace nodes are represented as attributes in the XDocument model.
                    if (isLast && ((Providers.Xml.XDocumentNode)original).UnderlyingObject is XAttribute nsAttr)
                    {
                        var nsCopy = new XAttribute(XName.Get(nsAttr.Name.LocalName, nsAttr.Name.NamespaceName), nsAttr.Value);
                        container?.Add(nsCopy);
                        targetCopy = nsCopy;
                    }
                    break;
            }
        }

        // Deep-copy the descendants of the target element.
        if (targetCopy is XElement targetElem && node.NodeKind == XdmNodeKind.Element)
        {
            if (((Providers.Xml.XDocumentNode)node).UnderlyingObject is XElement origElem)
            {
                foreach (var child in origElem.Nodes())
                {
                    targetElem.Add(DeepCopyXNode(child));
                }
            }
        }

        // Deep-copy children of a target document node.
        if (targetCopy is XDocument targetDoc && node.NodeKind == XdmNodeKind.Document)
        {
            if (((Providers.Xml.XDocumentNode)node).UnderlyingObject is XDocument origDoc)
            {
                foreach (var child in origDoc.Nodes())
                {
                    targetDoc.Add(DeepCopyXNode(child));
                }
            }
        }

        if (targetCopy == null)
            return null;

        return new Providers.Xml.XDocumentNode(targetCopy);
    }

    private static XElement ShallowCopyElement(XElement element)
    {
        var copy = new XElement(XName.Get(element.Name.LocalName, element.Name.NamespaceName));
        foreach (var attr in element.Attributes())
        {
            copy.SetAttributeValue(XName.Get(attr.Name.LocalName, attr.Name.NamespaceName), attr.Value);
        }
        CopyInScopeNamespaces(element, copy);
        return copy;
    }

    private static XNode DeepCopyXNode(XNode node)
    {
        switch (node)
        {
            case XElement elem:
                return DeepCopyElement(elem);
            case XText text:
                return new XText(text.Value);
            case XComment comment:
                return new XComment(comment.Value);
            case XProcessingInstruction pi:
                return new XProcessingInstruction(pi.Target, pi.Data);
            default:
                return new XText(node.ToString());
        }
    }

    private static XdmValue ResolveUri_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (args[0].IsUndefined || IsEmptySequence(args[0]))
            return XdmValue.Undefined;
        return ResolveUri(ctx, AtomizedString(args[0]), ctx.BaseUri);
    }

    private static XdmValue ResolveUri_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (args[0].IsUndefined || IsEmptySequence(args[0]))
            return XdmValue.Undefined;
        return ResolveUri(ctx, AtomizedString(args[0]), AtomizedString(args[1]));
    }

    private static XdmValue ResolveUri(EvaluationContext ctx, string relative, string? baseUri)
    {
        if (relative == null)
            return XdmValue.Undefined;
        if (string.IsNullOrEmpty(relative))
        {
            if (string.IsNullOrEmpty(baseUri))
                throw new InvalidOperationException("FODC0005: No base URI available");
            return XdmValue.FromString(baseUri, "anyURI");
        }
        if (IsAbsoluteUri(relative))
            return XdmValue.FromString(relative, "anyURI");
        if (string.IsNullOrEmpty(baseUri))
            throw new InvalidOperationException("FODC0005: No base URI available");

        // RFC 3986 / XPath FO.E1: the base URI must be absolute and syntactically valid.
        if (!Uri.TryCreate(baseUri, UriKind.Absolute, out var baseUriObj)
            || !baseUriObj.IsAbsoluteUri
            || !Uri.IsWellFormedUriString(baseUri, UriKind.Absolute))
        {
            throw new InvalidOperationException("FORG0002: Invalid base URI");
        }

        // Validate the relative reference by attempting to resolve it against a well-formed
        // dummy base. This catches malformed references such as "##some.uri".
        if (!Uri.TryCreate(baseUriObj, relative, out var dummyResolved)
            || !Uri.IsWellFormedUriString(dummyResolved.OriginalString, UriKind.Absolute))
        {
            throw new InvalidOperationException("FORG0002: Invalid relative URI");
        }

        var resolved = ResolveRelativeUri(baseUri, relative);
        return XdmValue.FromString(resolved, "anyURI");
    }

    /// <summary>
    /// Checks whether a string is an absolute URI per RFC 3986.
    /// More permissive than <see cref="Uri.IsWellFormedUriString"/>:
    /// accepts <c>g:h</c> style URIs that .NET rejects as DOS paths.
    /// </summary>
    public static bool IsAbsoluteUri(string uri)
    {
        if (Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            return true;

        // Manual RFC 3986 check: scheme ':' [path]
        int colonIndex = uri.IndexOf(':');
        if (colonIndex <= 0)
            return false;

        // If '/' '?' or '#' appear before ':', it's not a scheme
        for (int i = 0; i < colonIndex; i++)
        {
            char c = uri[i];
            if (c == '/' || c == '?' || c == '#')
                return false;
        }

        // Scheme must start with a letter
        if (!char.IsLetter(uri[0]))
            return false;

        return true;
    }

    /// <summary>
    /// Resolves a relative URI against a base URI, handling edge cases
    /// that .NET's <see cref="Uri"/> class misinterprets.
    /// </summary>
    private static string ResolveRelativeUri(string baseUri, string relative)
    {
        try
        {
            var resolved = new Uri(new Uri(baseUri), relative).AbsoluteUri;
            // RFC 3986: network-path references like //g have empty path.
            // .NET normalizes empty path to "/", so strip trailing "/" when
            // the relative URI is //authority with no path.
            if (relative.StartsWith("//") && resolved.EndsWith("/"))
            {
                int pathStart = relative.IndexOf('/', 2);
                if (pathStart < 0)
                    resolved = resolved.TrimEnd('/');
            }
            return resolved;
        }
        catch (UriFormatException)
        {
            // If the base URI itself is not a valid .NET Uri,
            // fall back to simple concatenation
            if (baseUri.EndsWith('/'))
                return baseUri + relative;
            int lastSlash = baseUri.LastIndexOf('/');
            if (lastSlash >= 0)
                return baseUri.Substring(0, lastSlash + 1) + relative;
            return relative;
        }
    }

    private static XdmValue Not(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromBoolean(!args[0].EffectiveBooleanValue());

    private static XdmValue Position(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (ctx.ContextItem.IsUndefined)
            throw new InvalidOperationException("XPDY0002: fn:position() called with no context item.");
        return XdmValue.FromInteger(ctx.ContextPosition);
    }

    private static XdmValue Last(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (ctx.ContextItem.IsUndefined)
            throw new InvalidOperationException("XPDY0002: fn:last() called with no context item.");
        return XdmValue.FromInteger(ctx.ContextSize);
    }

    private static XdmValue Current(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ctx.CurrentItem;

    private static XdmValue CurrentOutputUri(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (string.IsNullOrEmpty(ctx.CurrentOutputUri))
            return XdmValue.Undefined;
        return XdmValue.FromString(ctx.CurrentOutputUri, "anyURI");
    }

    // ------------------------------------------------------------------
    // String functions
    // ------------------------------------------------------------------

    private static int CountCodePoints(string s)
    {
        int count = 0;
        foreach (var _ in s.EnumerateRunes())
            count++;
        return count;
    }

    private static XdmValue StringLength_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var item = ctx.ContextItem;
        if (item.IsUndefined)
            throw new InvalidOperationException("fn:string-length() called with no context item.");
        return XdmValue.FromInteger(CountCodePoints(AtomizedString(item)));
    }

    private static XdmValue StringLength_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromInteger(CountCodePoints(AtomizedString(args[0])));

    private static int RoundForSubstring(double value)
    {
        if (double.IsNaN(value)) return 0;
        if (double.IsPositiveInfinity(value)) return int.MaxValue;
        if (double.IsNegativeInfinity(value)) return int.MinValue;
        return (int)RoundDouble(value, 0);
    }

    private static XdmValue Substring_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string s = AtomizedString(args[0]);
        double startD = ToDoubleValue(args[1]);
        if (double.IsNaN(startD)) return XdmValue.FromString(string.Empty);
        int start = RoundForSubstring(startD);
        return XdmValue.FromString(SubstringByCodepoints(s, start, int.MaxValue));
    }

    private static XdmValue Substring_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string s = AtomizedString(args[0]);
        double startD = ToDoubleValue(args[1]);
        double lenD = ToDoubleValue(args[2]);
        if (double.IsNaN(startD) || double.IsNaN(lenD)) return XdmValue.FromString(string.Empty);
        int start = RoundForSubstring(startD);
        int len = RoundForSubstring(lenD);
        if (len <= 0) return XdmValue.FromString(string.Empty);
        return XdmValue.FromString(SubstringByCodepoints(s, start, len));
    }

    /// <summary>
    /// Extracts a substring by Unicode code points (not UTF-16 code units),
    /// matching XPath <c>fn:substring</c> semantics.
    /// </summary>
    private static string SubstringByCodepoints(string s, int start, int length)
    {
        // XPath fn:substring($s, $start, $length) returns characters whose
        // 1-based position p satisfies: $start <= p < $start + $length
        if (length <= 0) return string.Empty;
        var sb = new StringBuilder();
        int codepointIndex = 1;
        // Use long to avoid overflow (e.g., int.MinValue + int.MaxValue)
        long end = (long)start + length;
        foreach (Rune rune in s.EnumerateRunes())
        {
            if (codepointIndex >= end)
                break;
            if (codepointIndex >= start)
                sb.Append(rune.ToString());
            codepointIndex++;
        }
        return sb.ToString();
    }

    private static XdmValue SubstringBefore_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string s = AtomizedString(args[0]);
        string search = AtomizedString(args[1]);
        int idx = StringIndexOf(s, search, ctx.DefaultCollation);
        return XdmValue.FromString(idx >= 0 ? s[..idx] : string.Empty);
    }

    private static XdmValue SubstringBefore_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string s = AtomizedString(args[0]);
        string search = AtomizedString(args[1]);
        string collation = AtomizedString(args[2]);
        ValidateCollation(collation);
        int idx = StringIndexOf(s, search, collation);
        return XdmValue.FromString(idx >= 0 ? s[..idx] : string.Empty);
    }

    private static XdmValue SubstringAfter_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string s = AtomizedString(args[0]);
        string search = AtomizedString(args[1]);
        string collation = ctx.DefaultCollation;
        if (TryParseUca(collation, out var uca))
        {
            int idx = uca.CompareInfo.IndexOf(s, search, uca.Options);
            if (idx < 0)
                return XdmValue.FromString(string.Empty);
            if (uca.CompareInfo.Compare(search, string.Empty, uca.Options) == 0)
                return XdmValue.FromString(s[idx..]);
            string suffix = s[idx..];
            int matchLen = -1;
            for (int len = 1; len <= suffix.Length; len++)
            {
                if (uca.CompareInfo.IsPrefix(suffix[..len], search, uca.Options))
                {
                    matchLen = len;
                    break;
                }
            }
            return XdmValue.FromString(matchLen > 0 ? s[(idx + matchLen)..] : string.Empty);
        }
        int plainIdx = s.IndexOf(search, GetStringComparison(collation));
        return XdmValue.FromString(plainIdx >= 0 ? s[(plainIdx + search.Length)..] : string.Empty);
    }

    private static XdmValue SubstringAfter_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string s = AtomizedString(args[0]);
        string search = AtomizedString(args[1]);
        string collation = AtomizedString(args[2]);
        ValidateCollation(collation);

        if (TryParseUca(collation, out var uca))
        {
            int idx = uca.CompareInfo.IndexOf(s, search, uca.Options);
            if (idx < 0)
                return XdmValue.FromString(string.Empty);

            // If search consists entirely of ignorable characters, match length is zero
            if (uca.CompareInfo.Compare(search, string.Empty, uca.Options) == 0)
                return XdmValue.FromString(s[idx..]);

            // Find the minimum prefix length that matches the search pattern
            string suffix = s[idx..];
            int matchLen = -1;
            for (int len = 1; len <= suffix.Length; len++)
            {
                if (uca.CompareInfo.IsPrefix(suffix[..len], search, uca.Options))
                {
                    matchLen = len;
                    break;
                }
            }
            return XdmValue.FromString(matchLen > 0 ? s[(idx + matchLen)..] : string.Empty);
        }

        int plainIdx = s.IndexOf(search, GetStringComparison(collation));
        return XdmValue.FromString(plainIdx >= 0 ? s[(plainIdx + search.Length)..] : string.Empty);
    }

    private static XdmValue StringToCodepoints(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string s = AtomizedString(args[0]);
        var values = new List<XdmValue>(s.Length);
        foreach (Rune rune in s.EnumerateRunes())
            values.Add(XdmValue.FromInteger(rune.Value));
        return XdmValue.FromSequence(MaterializedSequence.FromList(values));
    }

    private static XdmValue CodepointsToString(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var items = Materialize(args[0]);
        var sb = new StringBuilder(items.Count);
        foreach (var item in items)
        {
            int cp = (int)item.IntegerValue;
            if (cp < 0 || cp > 0x10FFFF || (cp >= 0xD800 && cp <= 0xDFFF) ||
                cp == 0 ||
                cp == 0xFFFE || cp == 0xFFFF || (cp >= 0xFDD0 && cp <= 0xFDEF) ||
                (cp & 0xFFFE) == 0xFFFE && cp > 0xFFFF)
                throw new InvalidOperationException("FOCH0001");
            sb.Append(char.ConvertFromUtf32(cp));
        }
        return XdmValue.FromString(sb.ToString());
    }

    private static XdmValue Apply(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var func = args[0];
        var array = args[1].ArrayValue;
        var callArgs = new XdmValue[array.Count];
        for (int i = 0; i < array.Count; i++)
            callArgs[i] = array.Get(i + 1);
        return VmEngine.InvokeFunctionItem(func, ctx, callArgs);
    }

    private static XdmValue AvailableEnvironmentVariables(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var vars = Environment.GetEnvironmentVariables();
        var items = new List<XdmValue>(vars.Count);
        foreach (var key in vars.Keys.Cast<string>().OrderBy(k => k, StringComparer.Ordinal))
            items.Add(XdmValue.FromString(key));
        return XdmValue.FromSequence(MaterializedSequence.FromList(items));
    }

    private static XdmValue EnvironmentVariable(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (IsEmptySequence(arg))
            throw new InvalidOperationException("XPTY0004");
        if (arg.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float)
            throw new InvalidOperationException("XPTY0004");
        if (arg.Kind == XdmValueKind.Boolean)
            throw new InvalidOperationException("XPTY0004");
        var name = AtomizedString(arg);
        var vars = Environment.GetEnvironmentVariables();
        foreach (var key in vars.Keys)
        {
            if (key?.ToString() == name)
                return XdmValue.FromString(vars[key]?.ToString() ?? string.Empty);
        }
        return XdmValue.Undefined;
    }

    private static XdmValue DefaultCollation(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromString(string.IsNullOrEmpty(ctx.DefaultCollation) ? CodepointCollation : ctx.DefaultCollation);

    private static readonly string[] SystemProperties =
    [
        "xsl:version", "xsl:vendor", "xsl:vendor-url",
        "xsl:product-name", "xsl:product-version",
        "xsl:is-schema-aware", "xsl:supports-serialization",
        "xsl:supports-backwards-compatibility", "xsl:supports-namespace-axis",
        "xsl:supports-streaming", "xsl:supports-dynamic-evaluation",
        "xsl:xpath-version", "xsl:xsd-version"
    ];

    private static XdmValue SystemProperty(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string name = AtomizedString(args[0]);
        name = ExpandXsltPropertyName(name, ctx);
        string value = name switch
        {
            "xsl:version" => "3.0",
            "xsl:vendor" => "Bosak",
            "xsl:vendor-url" => "https://github.com/poco-irrilevante/Bosak",
            "xsl:product-name" => "Bosak XPath",
            "xsl:product-version" => "1.0",
            "xsl:is-schema-aware" => "no",
            "xsl:supports-serialization" => "yes",
            "xsl:supports-backwards-compatibility" => "yes",
            "xsl:supports-namespace-axis" => "yes",
            "xsl:supports-streaming" => "no",
            "xsl:supports-dynamic-evaluation" => "yes",
            "xsl:xpath-version" => "3.1",
            "xsl:xsd-version" => "1.1",
            _ => ""
        };
        return XdmValue.FromString(value);
    }

    private static string ExpandXsltPropertyName(string name, EvaluationContext ctx)
    {
        // system-property accepts a lexical QName; expand any prefix bound to the
        // XSLT namespace to the canonical "xsl:" form used by the implementation.
        if (name.StartsWith("xsl:"))
            return name;
        int colon = name.IndexOf(':');
        if (colon >= 0)
        {
            var prefix = name[..colon];
            var local = name[(colon + 1)..];
            if (ctx.TryResolveNamespace(prefix, out var nsUri) && nsUri == Namespaces.Xsl)
                return $"xsl:{local}";
        }
        return name;
    }

    private static XdmValue AvailableSystemProperties(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var items = new List<XdmValue>();
        foreach (var prop in SystemProperties)
        {
            var local = prop.StartsWith("xsl:") ? prop[4..] : prop;
            items.Add(XdmValue.FromQName(new XsQName(local, Namespaces.Xsl, "xsl")));
        }
        return XdmValue.FromSequence(MaterializedSequence.FromList(items));
    }

    private static XdmValue StaticBaseUri(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (string.IsNullOrEmpty(ctx.BaseUri))
            return XdmValue.Undefined;
        return XdmValue.FromString(ctx.BaseUri, "anyURI");
    }

    private static XdmValue TypeAvailable(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string name = AtomizedString(args[0]);
        // For now, return true for common built-in schema types that are always available
        string localName = name;
        string? nsUri = null;
        if (name.StartsWith("Q{"))
        {
            // EQName syntax: Q{uri}local
            int close = name.IndexOf('}');
            if (close >= 2)
            {
                nsUri = name.Substring(2, close - 2);
                localName = name.Substring(close + 1);
            }
        }
        else if (name.StartsWith("{"))
        {
            int close = name.IndexOf('}');
            if (close > 0)
            {
                nsUri = name.Substring(1, close - 1);
                localName = name.Substring(close + 1);
            }
        }
        else
        {
            int colon = name.IndexOf(':');
            if (colon > 0)
                localName = name.Substring(colon + 1);
        }
        localName = localName.ToLowerInvariant();

        // If an explicit namespace URI was supplied, the type is only available
        // when it is the XML Schema namespace.
        if (nsUri is not null && nsUri != Namespaces.Xs)
            return XdmValue.False;

        // For an EQName with no namespace (Q{}local), no schema is in scope.
        if (nsUri == string.Empty && name.StartsWith("Q{"))
            return XdmValue.False;
        string[] builtInTypes =
        [
            "string", "boolean", "integer", "decimal", "float", "double",
            "date", "time", "datetime", "dateTime", "duration", "yearmonthduration",
            "yearMonthDuration", "daytimeduration", "dayTimeDuration",
            "gday", "gmonth", "gyear", "gmonthday", "gyearmonth",
            "hexbinary", "base64binary", "anyuri", "qname", "notation",
            "normalizedstring", "token", "language", "nmtoken", "name", "ncname",
            "id", "idref", "entity", "int", "long", "short", "byte",
            "nonnegativeinteger", "positiveinteger", "unsignedlong", "unsignedint",
            "unsignedshort", "unsignedbyte", "nonpositiveinteger", "negativeinteger",
            "untyped", "anytype", "anysimpletype", "untypedatomic", "anyatomictype"
        ];
        return XdmValue.FromBoolean(builtInTypes.Contains(localName));
    }

    private static XdmValue FunctionAvailable(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string name = AtomizedString(args[0]);
        int arity = args.Length > 1 ? (int)ToIntegerValue(args[1]) : -1;

        var (nsUri, localName) = ParseFunctionAvailableName(ctx, name);

        // In a static (use-when) context, XSLT-defined functions that depend on the
        // dynamic evaluation context are not available even though they use the fn
        // namespace and are registered in the runtime function library.
        if (ctx.IsStaticEvaluation && nsUri == Namespaces.Fn && XsltDynamicFunctions.Contains(localName))
            return XdmValue.FromBoolean(false);

        if (arity >= 0)
        {
            // fn:concat is variadic: any arity >= 2 is valid.
            if (nsUri == Namespaces.Fn && localName == "concat" && arity >= 2)
                return XdmValue.FromBoolean(true);

            // Some XSLT 3.0 functions are reported as available even when not fully implemented.
            // These are not visible in a static (use-when) context because most of them depend
            // on the dynamic evaluation context.
            if (!ctx.IsStaticEvaluation && IsXslt30FunctionReportedAvailable(nsUri, localName, arity))
                return XdmValue.FromBoolean(true);

            return XdmValue.FromBoolean(ctx.TryResolveFunction(nsUri, localName, arity, out _));
        }
        else
        {
            // Check any arity
            for (int a = 0; a <= 20; a++)
            {
                if (!ctx.IsStaticEvaluation && IsXslt30FunctionReportedAvailable(nsUri, localName, a))
                    return XdmValue.FromBoolean(true);
                if (ctx.TryResolveFunction(nsUri, localName, a, out _))
                    return XdmValue.FromBoolean(true);
            }
            return XdmValue.FromBoolean(false);
        }
    }

    private static XdmValue ElementAvailable(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string name = AtomizedString(args[0]);
        // XSLT specifies that element-available uses the default namespace of the
        // defining element, not the XPath default element namespace.
        var defaultUri = ctx.DefiningElementDefaultNamespace ?? ctx.DefaultElementNamespace ?? string.Empty;
        var (nsUri, localName) = ParseQNameArgument(ctx, name, defaultUri: defaultUri);

        if (nsUri != Namespaces.Xsl)
            return XdmValue.FromBoolean(false);

        return XdmValue.FromBoolean(XsltInstructionNames.Contains(localName));
    }

    private static (string nsUri, string localName) ParseFunctionAvailableName(EvaluationContext ctx, string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new InvalidOperationException("XTDE1400");

        string nsUri;
        string localName;

        if (name.Length > 2 && name[0] == 'Q' && name[1] == '{')
        {
            int close = name.IndexOf('}');
            if (close < 2 || close == name.Length - 1)
                throw new InvalidOperationException("XTDE1400");
            nsUri = name.Substring(2, close - 2);
            localName = name.Substring(close + 1);
        }
        else if (name.StartsWith("{"))
        {
            throw new InvalidOperationException("XTDE1400");
        }
        else
        {
            int colon = name.IndexOf(':');
            if (colon >= 0)
            {
                string prefix = name.Substring(0, colon);
                localName = name.Substring(colon + 1);
                if (prefix == "xml")
                {
                    nsUri = "http://www.w3.org/XML/1998/namespace";
                }
                else if (!ctx.TryResolveNamespace(prefix, out nsUri))
                {
                    throw new InvalidOperationException("XTDE1400");
                }
            }
            else
            {
                nsUri = Namespaces.Fn;
                localName = name;
            }
        }

        try
        {
            System.Xml.XmlConvert.VerifyNCName(localName);
        }
        catch
        {
            throw new InvalidOperationException("XTDE1400");
        }

        return (nsUri, localName);
    }

    private static (string nsUri, string localName) ParseQNameArgument(EvaluationContext ctx, string name, string defaultUri)
    {
        string nsUri = defaultUri;
        string localName = name;

        if (name.Length > 2 && name[0] == 'Q' && name[1] == '{')
        {
            int close = name.IndexOf('}');
            if (close >= 2)
            {
                nsUri = name.Substring(2, close - 2);
                localName = name.Substring(close + 1);
            }
        }
        else if (name.StartsWith("{"))
        {
            int close = name.IndexOf('}');
            if (close > 0)
            {
                nsUri = name.Substring(1, close - 1);
                localName = name.Substring(close + 1);
            }
        }
        else
        {
            int colon = name.IndexOf(':');
            if (colon >= 0)
            {
                string prefix = name.Substring(0, colon);
                localName = name.Substring(colon + 1);
                if (ctx.TryResolveNamespace(prefix, out var resolvedNs))
                    nsUri = resolvedNs;
                else if (prefix == "xml")
                    nsUri = "http://www.w3.org/XML/1998/namespace";
            }
        }

        return (nsUri, localName);
    }

    /// <summary>
    /// XSLT-defined functions in the fn namespace that depend on the dynamic
    /// evaluation context and are therefore not available during static evaluation
    /// of <c>use-when</c> expressions.
    /// </summary>
    private static readonly HashSet<string> XsltDynamicFunctions = new(StringComparer.Ordinal)
    {
        "accumulator-after",
        "accumulator-before",
        "copy-of",
        "current",
        "current-group",
        "current-grouping-key",
        "current-merge-group",
        "current-merge-key",
        "current-output-uri",
        "document",
        "key",
        "regex-group",
        "stream-available",
        "unparsed-entity-public-id",
        "unparsed-entity-uri"
    };

    private static bool IsXslt30FunctionReportedAvailable(string nsUri, string localName, int arity)
    {
        if (nsUri != Namespaces.Fn)
            return false;

        return localName switch
        {
            "current-group" => arity is 0,
            "current-grouping-key" => arity is 0,
            "current-merge-group" => arity is 0 or 1,
            "current-merge-key" => arity is 0,
            "regex-group" => arity is 1,
            "stream-available" => arity is 1,
            "accumulator-before" => arity is 1,
            "accumulator-after" => arity is 1,
            "copy-of" => arity is 0 or 1,
            "snapshot" => arity is 0 or 1,
            "document" => arity is 1 or 2,
            "key" => arity is 2 or 3,
            "current" => arity is 0,
            "unparsed-entity-uri" => arity is 1 or 2,
            "unparsed-entity-public-id" => arity is 1 or 2,
            "system-property" => arity is 1,
            "collation-key" => arity is 1 or 2,
            "function-available" => arity is 1 or 2,
            "element-available" => arity is 1,
            "type-available" => arity is 1,
            "current-output-uri" => arity is 0,
            _ => false
        };
    }

    private static readonly HashSet<string> XsltInstructionNames = new(StringComparer.Ordinal)
    {
        "analyze-string", "apply-imports", "apply-templates", "assert", "attribute",
        "attribute-set", "break", "call-template", "catch", "character-map", "choose",
        "comment", "context-item", "copy", "copy-of", "decimal-format", "document",
        "element", "evaluate", "expose", "fallback", "for-each", "for-each-group",
        "function", "global-context-item", "if", "import", "import-schema", "include",
        "iterate", "key", "map", "map-entry", "matching-substring", "merge",
        "merge-action", "merge-key", "merge-source", "message", "mode", "namespace",
        "namespace-alias", "next-match", "non-matching-substring", "number", "on-completion",
        "on-empty", "on-non-empty", "otherwise", "output", "output-character", "package",
        "param", "perform-sort", "preserve-space", "processing-instruction", "result-document",
        "sequence", "sort", "source-document", "strip-space", "stylesheet", "template",
        "text", "transform", "try", "value-of", "variable", "when", "where-populated",
        "with-param"
    };

    private static XdmValue ImplicitTimezone(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        int totalMinutes = ctx.ImplicitTimezoneOffsetMinutes;
        bool negative = totalMinutes < 0;
        totalMinutes = Math.Abs(totalMinutes);
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;
        var sb = new System.Text.StringBuilder();
        if (negative) sb.Append('-');
        sb.Append("PT");
        if (hours > 0) sb.Append(hours).Append('H');
        if (minutes > 0) sb.Append(minutes).Append('M');
        if (hours == 0 && minutes == 0) sb.Append("0S");
        return XdmValue.FromDuration(sb.ToString());
    }

    private static XdmValue XsQNameConstructor(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string lexical = AtomizedString(args[0]);
        if (string.IsNullOrEmpty(lexical))
            throw new InvalidOperationException("FOCA0002");

        string prefix, local;
        int colon = lexical.IndexOf(':');
        if (colon >= 0)
        {
            prefix = lexical[..colon];
            local = lexical[(colon + 1)..];
            if (string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(local))
                throw new InvalidOperationException("FOCA0002");
        }
        else
        {
            prefix = string.Empty;
            local = lexical;
        }

        if (!ctx.TryResolveNamespace(prefix, out string nsUri))
            nsUri = string.Empty;
        return XdmValue.FromQName(new XsQName(local, nsUri, prefix));
    }

    private static XdmValue ParseXml_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string xml = AtomizedString(args[0]);
        if (string.IsNullOrEmpty(xml))
            throw new InvalidOperationException("fn:parse-xml argument must not be empty.");
        var doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
        XDocumentProvider.StripDocumentLevelWhitespace(doc);
        return XdmValue.FromNode(doc.ToXdmNode());
    }

    private static XdmValue Serialize_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var value = args[0];
        if (value.IsUndefined)
            return XdmValue.FromString(string.Empty);

        if (!value.IsSequence)
            return XdmValue.FromString(SerializeItem(value));

        var sb = new StringBuilder();
        foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
            sb.Append(SerializeItem(item));
        return XdmValue.FromString(sb.ToString());
    }

    private static string SerializeItem(XdmValue value)
    {
        if (value.IsNode)
            return value.NodeValue.ToXmlString();
        return value.ToString();
    }

    private static XdmValue ParseXmlFragment_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string xml = AtomizedString(args[0]);
        if (string.IsNullOrEmpty(xml))
            return XdmValue.Undefined;
        var wrapper = $"<wrapper xmlns=\"http://www.w3.org/2005/xpath-functions\">{xml}</wrapper>";
        var doc = XDocument.Parse(wrapper, LoadOptions.PreserveWhitespace);
        XDocumentProvider.StripDocumentLevelWhitespace(doc);
        var wrapperEl = doc.Root;
        if (wrapperEl is null || !wrapperEl.HasElements)
            return XdmValue.Undefined;
        var firstChild = wrapperEl.Elements().First();
        return XdmValue.FromNode(new XDocumentNode(firstChild));
    }

    private static XdmValue HasChildren_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => HasChildren(ctx.ContextItem);

    private static XdmValue HasChildren_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => HasChildren(args[0]);

    private static XdmValue HasChildren(XdmValue value)
    {
        if (value.IsUndefined)
            return XdmValue.False;
        if (!value.IsNode)
            throw new InvalidOperationException("XPTY0004");
        var node = value.NodeValue;
        if (node.NodeKind is not XdmNodeKind.Element and not XdmNodeKind.Document)
            return XdmValue.False;
        foreach (var _ in node.Axis(XdmAxis.Child))
            return XdmValue.True;
        return XdmValue.False;
    }

    private static XdmValue Path_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var item = ctx.ContextItem;
        if (item.IsUndefined)
            throw new InvalidOperationException("XPDY0002");
        return Path_1(ctx, new[] { item });
    }

    private static XdmValue Path_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var value = args[0];
        if (value.IsUndefined || IsEmptySequence(value))
            return XdmValue.Undefined;

        // Unwrap singleton sequences
        if (value.IsSequence && value.SequenceValue is not null)
        {
            if (value.SequenceValue.TryGetLength(out var len))
            {
                if (len == 0)
                    return XdmValue.Undefined;
                if (len > 1)
                    throw new InvalidOperationException("XPTY0004");
            }
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                value = item;
                break;
            }
        }

        if (!value.IsNode)
            throw new InvalidOperationException("XPTY0004");
        return XdmValue.FromString(GetPath(value.NodeValue));
    }

    private static string GetPath(IXdmNode node)
    {
        if (node.NodeKind == XdmNodeKind.Document)
            return "/";

        var segments = new List<string>();
        var current = node;
        bool reachedDocument = false;
        while (true)
        {
            string seg = current.NodeKind switch
            {
                XdmNodeKind.Element => $"Q{{{current.NamespaceUri}}}{current.LocalName}[{GetSiblingIndex(current)}]",
                XdmNodeKind.Attribute => string.IsNullOrEmpty(current.NamespaceUri)
                ? $"@{current.LocalName}"
                : $"@Q{{{current.NamespaceUri}}}{current.LocalName}",
                XdmNodeKind.Text => "text()[1]",
                XdmNodeKind.Comment => "comment()[1]",
                XdmNodeKind.ProcessingInstruction => $"processing-instruction({current.LocalName})[1]",
                XdmNodeKind.Namespace => string.IsNullOrEmpty(current.LocalName)
                ? "namespace::*[Q{http://www.w3.org/2005/xpath-functions}local-name()=\"\"]"
                : $"namespace::{current.LocalName}",
                _ => $"node()[{GetSiblingIndex(current)}]"
            };
            segments.Add(seg);
            var parentSeq = current.Axis(XdmAxis.Parent);
            var enumerator = parentSeq.GetEnumerator();
            if (!enumerator.MoveNext())
                break;
            var parent = enumerator.Current.NodeValue;
            if (parent.NodeKind == XdmNodeKind.Document)
            {
                reachedDocument = true;
                break;
            }
            current = parent;
        }

        if (!reachedDocument)
            return "Q{http://www.w3.org/2005/xpath-functions}root()";

        segments.Reverse();
        return "/" + string.Join("/", segments);
    }

    private static int GetSiblingIndex(IXdmNode node)
    {
        int index = 1;
        var parentSeq = node.Axis(XdmAxis.Parent);
        var penum = parentSeq.GetEnumerator();
        if (!penum.MoveNext()) return index;
        var parent = penum.Current.NodeValue;
        foreach (var sibling in parent.Axis(XdmAxis.Child))
        {
            if (sibling.NodeValue.IsSameNode(node))
                return index;
            if (sibling.NodeValue.NodeKind == node.NodeKind)
            {
                if (node.NodeKind == XdmNodeKind.Element)
                {
                    if (sibling.NodeValue.NamespaceUri == node.NamespaceUri &&
                        sibling.NodeValue.LocalName == node.LocalName)
                        index++;
                }
                else
                {
                    index++;
                }
            }
        }
        return index;
    }

    private static XdmValue Unordered_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => args[0];

    private static XdmValue AnalyzeString_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => AnalyzeString(AtomizedString(args[0]), AtomizedString(args[1]), string.Empty);

    private static XdmValue AnalyzeString_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => AnalyzeString(AtomizedString(args[0]), AtomizedString(args[1]), AtomizedString(args[2]));

    private static XdmValue RegexGroup(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (args[0].IsUndefined || !long.TryParse(AtomizedString(args[0]), out long n))
            return XdmValue.FromString(string.Empty);
        if (ctx.RegexGroups == null || n < 0 || n >= ctx.RegexGroups.Length)
            return XdmValue.FromString(string.Empty);
        return XdmValue.FromString(ctx.RegexGroups[n]);
    }

    private static XdmValue AnalyzeString(string value, string pattern, string flags)
    {
        XNamespace fn = "http://www.w3.org/2005/xpath-functions";
        var result = new XElement(fn + "analyze-string-result");

        if (string.IsNullOrEmpty(value))
            return XdmValue.FromNode(new XDocumentNode(result));

        var options = RegexHelper.ParseRegexFlags(flags, out bool isQuoteMode);
        if (isQuoteMode)
            pattern = Regex.Escape(pattern);
        else
            pattern = RegexHelper.ValidateAndTranslatePattern(pattern);

        RegexHelper.CheckZeroLengthMatch(pattern, options);

        var matches = Regex.Matches(value, pattern, options);
        int pos = 0;

        foreach (Match match in matches)
        {
            if (match.Index > pos)
                result.Add(new XElement(fn + "non-match", value[pos..match.Index]));

            var matchEl = new XElement(fn + "match");
            int[] parents = RegexHelper.GetCapturingGroupParents(pattern);
            var root = new GroupNode(match.Index, match.Length);
            var nodes = new GroupNode?[match.Groups.Count];
            for (int g = 1; g < match.Groups.Count; g++)
            {
                var group = match.Groups[g];
                if (!group.Success)
                    continue;

                var node = new GroupNode(g, group.Index, group.Length);
                nodes[g] = node;
                int parent = parents.Length > g ? parents[g] : 0;
                if (parent == 0 || nodes[parent] == null)
                    root.Children.Add(node);
                else
                    nodes[parent]!.Children.Add(node);
            }

            RenderNode(matchEl, root, value, fn);
            result.Add(matchEl);
            pos = match.Index + match.Length;
        }

        if (pos < value.Length)
            result.Add(new XElement(fn + "non-match", value[pos..]));

        return XdmValue.FromNode(new XDocumentNode(result));
    }

    private sealed class GroupNode
    {
        public int? Nr { get; }
        public int Index { get; }
        public int Length { get; }
        public int End => Index + Length;
        public List<GroupNode> Children { get; } = new List<GroupNode>();

        public GroupNode(int index, int length)
        {
            Index = index;
            Length = length;
        }

        public GroupNode(int nr, int index, int length)
        {
            Nr = nr;
            Index = index;
            Length = length;
        }
    }

    private static void RenderNode(XContainer outer, GroupNode node, string value, XNamespace fn)
    {
        XContainer container = outer;
        XElement? groupEl = null;
        if (node.Nr.HasValue)
        {
            groupEl = new XElement(fn + "group");
            groupEl.SetAttributeValue("nr", node.Nr.Value);
            container = groupEl;
        }

        int pos = node.Index;
        foreach (var child in node.Children)
        {
            if (child.Index > pos)
                container.Add(new XText(value[pos..child.Index]));
            RenderNode(container, child, value, fn);
            pos = child.End;
        }

        if (node.End > pos)
            container.Add(new XText(value[pos..node.End]));

        if (groupEl != null)
            outer.Add(groupEl);
    }

    private static XdmValue Contains(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromBoolean(StringContains(RequireString(args[0]), RequireString(args[1]), ctx.DefaultCollation));

    private static XdmValue Contains_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string s = RequireString(args[0]);
        string search = RequireString(args[1]);
        string collation = AtomizedString(args[2]);
        ValidateCollation(collation);
        return XdmValue.FromBoolean(StringContains(s, search, collation));
    }

    private static XdmValue StartsWith(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromBoolean(StringStartsWith(RequireString(args[0]), RequireString(args[1]), ctx.DefaultCollation));

    private static XdmValue StartsWith_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string s = RequireString(args[0]);
        string search = RequireString(args[1]);
        string collation = AtomizedString(args[2]);
        ValidateCollation(collation);
        return XdmValue.FromBoolean(StringStartsWith(s, search, collation));
    }

    private static XdmValue EndsWith(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromBoolean(StringEndsWith(RequireString(args[0]), RequireString(args[1]), ctx.DefaultCollation));

    private static XdmValue EndsWith_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string s = RequireString(args[0]);
        string search = RequireString(args[1]);
        string collation = AtomizedString(args[2]);
        ValidateCollation(collation);
        return XdmValue.FromBoolean(StringEndsWith(s, search, collation));
    }

    private static XdmValue ContainsToken_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ContainsToken(args[0], AtomizedString(args[1]), ctx.DefaultCollation);

    private static XdmValue ContainsToken_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ContainsToken(args[0], AtomizedString(args[1]), AtomizedString(args[2]));

    private static XdmValue ContainsToken(XdmValue input, string token, string collation)
    {
        ValidateCollation(collation);
        var comparer = GetCollationEqualityComparer(collation);

        token = token.Trim();
        if (string.IsNullOrEmpty(token))
            return XdmValue.FromBoolean(false);

        var strings = Materialize(input);
        foreach (var item in strings)
        {
            string s = AtomizedString(item);
            if (string.IsNullOrWhiteSpace(s))
                continue;

            var parts = s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (comparer.Equals(part, token))
                    return XdmValue.FromBoolean(true);
            }
        }
        return XdmValue.FromBoolean(false);
    }

    private static XdmValue CodepointEqual(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        // fn:codepoint-equal returns empty sequence if either argument is empty sequence
        if (args[0].IsUndefined || IsEmptySequence(args[0]))
            return XdmValue.Undefined;
        if (args[1].IsUndefined || IsEmptySequence(args[1]))
            return XdmValue.Undefined;

        var a1 = AtomizeValue(args[0]);
        var a2 = AtomizeValue(args[1]);
        if (a1.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float)
            throw new InvalidOperationException("XPTY0004");
        if (a2.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float)
            throw new InvalidOperationException("XPTY0004");

        string s1 = AtomizedString(args[0]);
        string s2 = AtomizedString(args[1]);
        return XdmValue.FromBoolean(s1.Equals(s2, StringComparison.Ordinal));
    }

    private static XdmValue CollationKey_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => CollationKey(AtomizedString(args[0]), string.Empty);

    private static XdmValue CollationKey_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => CollationKey(AtomizedString(args[0]), AtomizedString(args[1]));

    private static XdmValue CollationKey(string value, string collation)
    {
        ValidateCollation(collation);
        if (collation == CodepointCollation)
            return XdmValue.FromString(value);
        if (collation == HtmlAsciiCaseInsensitiveCollation)
            return XdmValue.FromString(ToAsciiLower(value));
        if (TryParseUca(collation, out var uca))
        {
            var sortKey = uca.CompareInfo.GetSortKey(value, uca.Options);
            return XdmValue.FromString(Convert.ToHexString(sortKey.KeyData));
        }
        return XdmValue.FromString(value);
    }

    private static string ToAsciiLower(string value)
    {
        var sb = new System.Text.StringBuilder(value.Length);
        foreach (char c in value)
        {
            if (c >= 'A' && c <= 'Z')
                sb.Append((char)(c + 32));
            else
                sb.Append(c);
        }
        return sb.ToString();
    }

    private const string CodepointCollation = "http://www.w3.org/2005/xpath-functions/collation/codepoint";
    private const string HtmlAsciiCaseInsensitiveCollation = "http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive";
    private const string CaseblindCollation = "http://www.w3.org/2010/09/qt-fots-catalog/collation/caseblind";
    private const string UcaCollationPrefix = "http://www.w3.org/2013/collation/UCA";

    private static void ValidateCollation(string collation)
    {
        if (string.IsNullOrEmpty(collation))
            return;
        if (collation == CodepointCollation)
            return;
        if (collation == HtmlAsciiCaseInsensitiveCollation)
            return;
        if (collation == CaseblindCollation)
            return;
        if (TryParseUca(collation, out _))
            return;
        throw new InvalidOperationException("FOCH0002");
    }

    private static StringComparison GetStringComparison(string collation)
    {
        if (collation == HtmlAsciiCaseInsensitiveCollation || collation == CaseblindCollation)
            return StringComparison.OrdinalIgnoreCase;
        return StringComparison.Ordinal;
    }

    private static IEqualityComparer<string> GetStringComparer(string collation)
    {
        if (collation == HtmlAsciiCaseInsensitiveCollation || collation == CaseblindCollation)
            return StringComparer.OrdinalIgnoreCase;
        return StringComparer.Ordinal;
    }

    /// <summary>
    /// Compares two strings using the supplied collation URI. Returns a negative value,
    /// zero, or a positive value using the same conventions as <see cref="string.Compare"/>.
    /// </summary>
    public static int CompareStrings(string s1, string s2, string collation)
    {
        if (TryParseUca(collation, out var uca))
            return uca.CompareInfo.Compare(s1, s2, uca.Options);
        var comparison = GetStringComparison(collation);
        if (comparison == StringComparison.Ordinal)
            return CompareCodepoints(s1, s2);
        return string.Compare(s1, s2, comparison);
    }

    private static int CompareCodepoints(string s1, string s2)
    {
        int i1 = 0, i2 = 0;
        while (i1 < s1.Length && i2 < s2.Length)
        {
            int cp1 = char.ConvertToUtf32(s1, i1);
            int cp2 = char.ConvertToUtf32(s2, i2);
            if (cp1 != cp2)
                return cp1 < cp2 ? -1 : 1;
            i1 += char.IsHighSurrogate(s1[i1]) ? 2 : 1;
            i2 += char.IsHighSurrogate(s2[i2]) ? 2 : 1;
        }
        if (i1 < s1.Length) return 1;
        if (i2 < s2.Length) return -1;
        return 0;
    }

    private static bool StringContains(string s, string search, string collation)
    {
        if (TryParseUca(collation, out var uca))
            return uca.CompareInfo.IndexOf(s, search, uca.Options) >= 0;
        return s.Contains(search, GetStringComparison(collation));
    }

    private static bool StringStartsWith(string s, string search, string collation)
    {
        if (TryParseUca(collation, out var uca))
        {
            // IsPrefix is unreliable with IgnoreSymbols when both strings start with the same symbol.
            // Use IndexOf and verify the match is at or before the first non-ignorable character.
            int firstNonIgnorable = 0;
            while (firstNonIgnorable < s.Length &&
                   uca.CompareInfo.Compare(s[firstNonIgnorable].ToString(), string.Empty, uca.Options) == 0)
                firstNonIgnorable++;

            int matchPos = uca.CompareInfo.IndexOf(s, search, uca.Options);
            return matchPos >= 0 && matchPos <= firstNonIgnorable;
        }
        return s.StartsWith(search, GetStringComparison(collation));
    }

    private static bool StringEndsWith(string s, string search, string collation)
    {
        if (TryParseUca(collation, out var uca))
        {
            // IsSuffix is unreliable with IgnoreSymbols for empty source or trailing-symbol edge cases.
            // Use LastIndexOf and verify the match extends to cover the last non-ignorable character.
            int lastMatchPos = uca.CompareInfo.LastIndexOf(s, search, uca.Options);
            if (lastMatchPos < 0)
                return false;

            int matchLen = 0;
            for (int len = 1; len <= s.Length - lastMatchPos; len++)
            {
                if (uca.CompareInfo.IsPrefix(s.AsSpan(lastMatchPos, len), search, uca.Options))
                {
                    matchLen = len;
                    break;
                }
            }

            int lastNonIgnorablePos = s.Length - 1;
            while (lastNonIgnorablePos >= 0 &&
                   uca.CompareInfo.Compare(s[lastNonIgnorablePos].ToString(), string.Empty, uca.Options) == 0)
                lastNonIgnorablePos--;

            return lastMatchPos + matchLen > lastNonIgnorablePos;
        }
        return s.EndsWith(search, GetStringComparison(collation));
    }

    private static int StringIndexOf(string s, string search, string collation)
    {
        if (TryParseUca(collation, out var uca))
            return uca.CompareInfo.IndexOf(s, search, uca.Options);
        return s.IndexOf(search, GetStringComparison(collation));
    }

    private static IEqualityComparer<string> GetCollationEqualityComparer(string collation)
    {
        if (TryParseUca(collation, out var uca))
            return new UcaStringComparer(uca.CompareInfo, uca.Options);
        return GetStringComparer(collation);
    }

    private static bool TryParseUca(string uri, out UcaCollationInfo info)
    {
        info = default;
        if (!uri.StartsWith(UcaCollationPrefix, StringComparison.Ordinal))
            return false;

        string query = uri.Length > UcaCollationPrefix.Length && uri[UcaCollationPrefix.Length] == '?'
            ? uri[(UcaCollationPrefix.Length + 1)..]
            : string.Empty;

        string lang = "en";
        string strength = "tertiary";
        bool alternateBlanked = false;
        foreach (var param in query.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            int eq = param.IndexOf('=');
            if (eq < 0) continue;
            string key = param[..eq].Trim();
            string val = param[(eq + 1)..].Trim();
            if (key == "lang")
                lang = val;
            else if (key == "strength")
                strength = val;
            else if (key == "alternate" && val == "blanked")
                alternateBlanked = true;
            else if (string.Equals(key, "fallback", StringComparison.OrdinalIgnoreCase) &&
                     string.Equals(val, "no", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("FOCH0002: Unsupported UCA collation (fallback=no)");
        }

        var culture = CultureInfo.GetCultureInfo(lang);
        var options = strength.ToLowerInvariant() switch
        {
            "primary" => CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace,
            "secondary" => CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace,
            "tertiary" => CompareOptions.IgnoreNonSpace,
            "quaternary" => CompareOptions.None,
            "identical" => CompareOptions.Ordinal,
            _ => CompareOptions.None,
        };

        if (alternateBlanked)
            options |= CompareOptions.IgnoreSymbols;

        info = new UcaCollationInfo(lang, strength, options, culture.CompareInfo);
        return true;
    }

    private readonly record struct UcaCollationInfo(string Lang, string Strength, CompareOptions Options, CompareInfo CompareInfo);

    private sealed class UcaStringComparer : IEqualityComparer<string>
    {
        private readonly CompareInfo _compareInfo;
        private readonly CompareOptions _options;

        public UcaStringComparer(CompareInfo compareInfo, CompareOptions options)
        {
            _compareInfo = compareInfo;
            _options = options;
        }

        public bool Equals(string? x, string? y)
            => _compareInfo.Compare(x, y, _options) == 0;

        public int GetHashCode(string obj)
            => _compareInfo.GetSortKey(obj, _options).GetHashCode();
    }

    private static XdmValue NormalizeSpace_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var item = ctx.ContextItem;
        if (item.IsUndefined)
            throw new InvalidOperationException("fn:normalize-space() called with no context item.");
        return XdmValue.FromString(NormalizeSpaceString(AtomizedString(item)));
    }

    private static XdmValue NormalizeSpace_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromString(NormalizeSpaceString(AtomizedString(args[0])));

    private static string NormalizeSpaceString(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        var parts = s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', parts);
    }

    private static XdmValue NormalizeUnicode_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => NormalizeUnicode(AtomizedString(args[0]), "NFC");

    private static XdmValue NormalizeUnicode_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => NormalizeUnicode(AtomizedString(args[0]), AtomizedString(args[1]));

    private static XdmValue NormalizeUnicode(string input, string form)
    {
        var nf = form switch
        {
            "NFC" => System.Text.NormalizationForm.FormC,
            "NFD" => System.Text.NormalizationForm.FormD,
            "NFKC" => System.Text.NormalizationForm.FormKC,
            "NFKD" => System.Text.NormalizationForm.FormKD,
            "FULLY-NORMALIZED" => throw new InvalidOperationException("FOCH0003"),
            _ => throw new InvalidOperationException("FOCH0003")
        };
        return XdmValue.FromString(input.Normalize(nf));
    }

    private static XdmValue Translate(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string arg = AtomizedString(args[0]);
        string map = AtomizedString(args[1]);
        string trans = AtomizedString(args[2]);
        var sb = new StringBuilder(arg.Length);
        foreach (char c in arg)
        {
            int idx = map.IndexOf(c);
            if (idx >= 0)
            {
                if (idx < trans.Length)
                    sb.Append(trans[idx]);
            }
            else
            {
                sb.Append(c);
            }
        }
        return XdmValue.FromString(sb.ToString());
    }

    private static XdmValue UpperCase(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromString(ApplyUnicodeCaseMapping(RequireString(args[0]), toUpper: true));

    private static XdmValue LowerCase(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromString(ApplyUnicodeCaseMapping(RequireString(args[0]), toUpper: false));

    /// <summary>
    /// Applies Unicode full case mapping, handling one-to-many mappings
    /// (e.g., U+00DF 'ß' → "SS") that .NET's ToUpperInvariant omits.
    /// </summary>
    private static string ApplyUnicodeCaseMapping(string input, bool toUpper)
    {
        var sb = new StringBuilder(input.Length);
        foreach (Rune rune in input.EnumerateRunes())
        {
            if (toUpper)
            {
                switch (rune.Value)
                {
                    case 0x00DF: sb.Append("SS"); continue; // ß → SS
                    case 0x0149: sb.Append("\u02BCN"); continue; // ŉ → ʼN
                    case 0x017F: sb.Append('S'); continue; // ſ → S
                    case 0x01F0: sb.Append("J\u030C"); continue; // ǰ → J + caron
                }

                // Greek characters with iota subscript (full mapping strips iota subscript and adds iota)
                if (rune.Value is >= 0x1F80 and <= 0x1F87)
                {
                    // ἀ + iota subscript variants → Α + etc + ι
                    sb.Append(Rune.ToUpperInvariant(new Rune(rune.Value - 0x1F80 + 0x1F08)));
                    sb.Append('\u0399');
                    continue;
                }
                if (rune.Value is >= 0x1F90 and <= 0x1F97)
                {
                    sb.Append(Rune.ToUpperInvariant(new Rune(rune.Value - 0x1F90 + 0x1F28)));
                    sb.Append('\u0399');
                    continue;
                }
                if (rune.Value is >= 0x1FA0 and <= 0x1FA7)
                {
                    sb.Append(Rune.ToUpperInvariant(new Rune(rune.Value - 0x1FA0 + 0x1F68)));
                    sb.Append('\u0399');
                    continue;
                }
                if (rune.Value is >= 0x1FB3 and <= 0x1FB4)
                {
                    sb.Append('\u0391');
                    sb.Append('\u0399');
                    continue;
                }
                if (rune.Value == 0x1FB6)
                {
                    sb.Append('\u0391');
                    sb.Append('\u0342');
                    continue;
                }
                if (rune.Value == 0x1FB7)
                {
                    sb.Append('\u0391');
                    sb.Append('\u0342');
                    sb.Append('\u0399');
                    continue;
                }
                if (rune.Value == 0x1FBE)
                {
                    sb.Append('\u0399');
                    continue;
                }
                if (rune.Value is >= 0x1FC3 and <= 0x1FC4)
                {
                    sb.Append('\u0397');
                    sb.Append('\u0399');
                    continue;
                }
                if (rune.Value == 0x1FC6)
                {
                    sb.Append('\u0397');
                    sb.Append('\u0342');
                    continue;
                }
                if (rune.Value == 0x1FC7)
                {
                    sb.Append('\u0397');
                    sb.Append('\u0342');
                    sb.Append('\u0399');
                    continue;
                }
                if (rune.Value is >= 0x1FF3 and <= 0x1FF4)
                {
                    sb.Append('\u03A9');
                    sb.Append('\u0399');
                    continue;
                }
                if (rune.Value == 0x1FF6)
                {
                    sb.Append('\u03A9');
                    sb.Append('\u0342');
                    continue;
                }
                if (rune.Value == 0x1FF7)
                {
                    sb.Append('\u03A9');
                    sb.Append('\u0342');
                    sb.Append('\u0399');
                    continue;
                }

                sb.Append(Rune.ToUpperInvariant(rune));
            }
            else
            {
                switch (rune.Value)
                {
                    case 0x0130: sb.Append("i\u0307"); continue; // İ → i + combining dot above
                }

                // Greek upper with iota adscript → lower with iota subscript
                if (rune.Value is >= 0x1F88 and <= 0x1F8F)
                {
                    sb.Append(new Rune(rune.Value - 0x1F88 + 0x1F80));
                    continue;
                }
                if (rune.Value is >= 0x1F98 and <= 0x1F9F)
                {
                    sb.Append(new Rune(rune.Value - 0x1F98 + 0x1F90));
                    continue;
                }
                if (rune.Value is >= 0x1FA8 and <= 0x1FAF)
                {
                    sb.Append(new Rune(rune.Value - 0x1FA8 + 0x1FA0));
                    continue;
                }
                if (rune.Value is >= 0x1FB8 and <= 0x1FB9)
                {
                    sb.Append(new Rune(rune.Value - 0x1FB8 + 0x1FB0));
                    continue;
                }
                if (rune.Value is >= 0x1FBC and <= 0x1FBD)
                {
                    sb.Append('\u03B1');
                    sb.Append('\u03B9');
                    continue;
                }
                if (rune.Value is >= 0x1FC8 and <= 0x1FCB)
                {
                    sb.Append(new Rune(rune.Value - 0x1FC8 + 0x1F72));
                    continue;
                }
                if (rune.Value == 0x1FCC)
                {
                    sb.Append('\u03B7');
                    sb.Append('\u03B9');
                    continue;
                }
                if (rune.Value is >= 0x1FD8 and <= 0x1FDB)
                {
                    sb.Append(new Rune(rune.Value - 0x1FD8 + 0x1FD0));
                    continue;
                }
                if (rune.Value is >= 0x1FE8 and <= 0x1FEC)
                {
                    sb.Append(new Rune(rune.Value - 0x1FE8 + 0x1FE0));
                    continue;
                }
                if (rune.Value is >= 0x1FF8 and <= 0x1FFB)
                {
                    sb.Append(new Rune(rune.Value - 0x1FF8 + 0x1F78));
                    continue;
                }
                if (rune.Value == 0x1FFC)
                {
                    sb.Append('\u03C9');
                    sb.Append('\u03B9');
                    continue;
                }

                sb.Append(Rune.ToLowerInvariant(rune));
            }
        }
        return sb.ToString();
    }

    private static XdmValue Matches_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string input = AtomizedString(args[0]);
        string pattern = RegexHelper.ValidateAndTranslatePattern(AtomizedString(args[1]));
        return XdmValue.FromBoolean(Regex.IsMatch(input, pattern));
    }

    private static XdmValue Matches_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string input = AtomizedString(args[0]);
        string pattern = AtomizedString(args[1]);
        var options = RegexHelper.ParseRegexFlags(AtomizedString(args[2]), out bool isQuoteMode);
        if (isQuoteMode)
            pattern = Regex.Escape(pattern);
        else
            pattern = RegexHelper.ValidateAndTranslatePattern(pattern, options);
        return XdmValue.FromBoolean(Regex.IsMatch(input, pattern, options));
    }


    private static XdmValue Replace_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string input = AtomizedString(args[0]);
        string originalPattern = AtomizedString(args[1]);
        string pattern = RegexHelper.ValidateAndTranslatePattern(originalPattern);
        string replacement = AtomizedString(args[2]);
        RegexHelper.CheckZeroLengthMatch(pattern, RegexOptions.None);
        int groupCount = RegexHelper.CountCapturingGroups(originalPattern);
        string netReplacement = RegexHelper.ValidateAndTranslateReplacement(replacement, groupCount);
        return XdmValue.FromString(Regex.Replace(input, pattern, netReplacement));
    }

    private static XdmValue Replace_4(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string input = AtomizedString(args[0]);
        string originalPattern = AtomizedString(args[1]);
        string replacement = AtomizedString(args[2]);
        var options = RegexHelper.ParseRegexFlags(AtomizedString(args[3]), out bool isQuoteMode);
        string pattern;
        string netReplacement;
        if (isQuoteMode)
        {
            pattern = Regex.Escape(originalPattern);
            netReplacement = RegexHelper.EscapeReplacementForQuoteMode(replacement);
        }
        else
        {
            pattern = RegexHelper.ValidateAndTranslatePattern(originalPattern, options);
            int groupCount = RegexHelper.CountCapturingGroups(originalPattern);
            netReplacement = RegexHelper.ValidateAndTranslateReplacement(replacement, groupCount);
        }
        RegexHelper.CheckZeroLengthMatch(pattern, options);
        return XdmValue.FromString(Regex.Replace(input, pattern, netReplacement, options));
    }

    private static XdmValue Tokenize_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        // One-argument form is equivalent to tokenize(normalize-space($input), ' ')
        var input = NormalizeSpaceString(AtomizedString(args[0]));
        return DoTokenize(input, " ", string.Empty);
    }

    private static XdmValue Tokenize_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string input = AtomizedString(args[0]);
        string pattern = AtomizedString(args[1]);
        return DoTokenize(input, pattern, string.Empty);
    }

    private static XdmValue Tokenize_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string input = AtomizedString(args[0]);
        string pattern = AtomizedString(args[1]);
        string flags = AtomizedString(args[2]);
        return DoTokenize(input, pattern, flags);
    }

    private static XdmValue DoTokenize(string input, string pattern, string flags)
    {
        if (string.IsNullOrEmpty(input))
            return XdmValue.FromSequence(XdmSequence.Empty);

        var options = RegexHelper.ParseRegexFlags(flags, out bool isQuoteMode);
        if (isQuoteMode)
            pattern = Regex.Escape(pattern);
        else
            pattern = RegexHelper.ValidateAndTranslatePattern(pattern, options);

        RegexHelper.CheckZeroLengthMatch(pattern, options);

        var parts = Regex.Split(input, pattern, options);
        var result = new List<XdmValue>();

        foreach (var part in parts)
            result.Add(XdmValue.FromString(part));

        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    // ------------------------------------------------------------------
    // xs:* constructor functions
    // ------------------------------------------------------------------

    private static XdmValue XsString(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "string");

    private static XdmValue XsInteger(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "integer");

    private static XdmValue XsDecimal(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "decimal");

    private static XdmValue XsDouble(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "double");

    private static XdmValue XsFloat(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "float");

    private static XdmValue XsBoolean(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "boolean");

    private static XdmValue XsDateTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "dateTime");

    private static XdmValue XsDate(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "date");

    private static XdmValue XsTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "time");

    private static XdmValue XsByte(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "byte");

    private static XdmValue XsShort(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "short");

    private static XdmValue XsInt(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "int");

    private static XdmValue XsLong(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "long");

    private static XdmValue XsUnsignedByte(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "unsignedByte");

    private static XdmValue XsUnsignedShort(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "unsignedShort");

    private static XdmValue XsUnsignedInt(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "unsignedInt");

    private static XdmValue XsUnsignedLong(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "unsignedLong");

    private static XdmValue XsPositiveInteger(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "positiveInteger");

    private static XdmValue XsNegativeInteger(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "negativeInteger");

    private static XdmValue XsNonPositiveInteger(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "nonPositiveInteger");

    private static XdmValue XsNonNegativeInteger(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "nonNegativeInteger");

    private static XdmValue XsDayTimeDuration(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "dayTimeDuration");

    private static XdmValue XsYearMonthDuration(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "yearMonthDuration");

    private static XdmValue XsUntypedAtomic(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "untypedAtomic");

    private static XdmValue XsAnyUri(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "anyURI");

    private static XdmValue XsHexBinary(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "hexBinary");

    private static XdmValue XsBase64Binary(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "base64Binary");

    private static XdmValue XsGDay(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "gDay");

    private static XdmValue XsGMonth(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "gMonth");

    private static XdmValue XsGYear(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "gYear");

    private static XdmValue XsGYearMonth(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "gYearMonth");

    private static XdmValue XsGMonthDay(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "gMonthDay");

    private static XdmValue XsNCName(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "NCName");

    private static XdmValue XsDuration(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "duration");

    private static XdmValue XsLanguage(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "language");

    private static XdmValue XsName(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "Name");

    private static XdmValue XsNormalizedString(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "normalizedString");

    private static XdmValue XsToken(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "token");

    private static XdmValue XsID(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "ID");

    private static XdmValue XsIDREF(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "IDREF");

    private static XdmValue XsNMTOKEN(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "NMTOKEN");

    private static XdmValue XsENTITY(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "ENTITY");

    private static XdmValue XsIDREFS(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "IDREFS");

    private static XdmValue XsNMTOKENS(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "NMTOKENS");

    private static XdmValue XsENTITIES(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "ENTITIES");

    // ------------------------------------------------------------------
    // math:* functions
    // ------------------------------------------------------------------

    private static XdmValue MathPi(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromDouble(Math.PI);

    private static XdmValue MathSin(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ApplyMath(args[0], d => Math.Sin(d));

    private static XdmValue MathCos(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ApplyMath(args[0], d => Math.Cos(d));

    private static XdmValue MathTan(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ApplyMath(args[0], d => Math.Tan(d));

    private static XdmValue MathPow(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var a = AtomizeValue(args[0]);
        var b = AtomizeValue(args[1]);
        if (a.IsUndefined || b.IsUndefined) return XdmValue.Undefined;
        return XdmValue.FromDouble(Math.Pow(ToDoubleValue(a), ToDoubleValue(b)));
    }

    private static XdmValue MathSqrt(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ApplyMath(args[0], d => Math.Sqrt(d));

    private static XdmValue MathExp(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ApplyMath(args[0], d => Math.Exp(d));

    private static XdmValue MathLog(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ApplyMath(args[0], d => Math.Log(d));

    private static XdmValue MathLog10(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ApplyMath(args[0], d => Math.Log10(d));

    private static XdmValue MathExp10(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ApplyMath(args[0], d => Math.Pow(10.0, d));

    private static XdmValue MathAsin(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ApplyMath(args[0], d => Math.Asin(d));

    private static XdmValue MathAcos(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ApplyMath(args[0], d => Math.Acos(d));

    private static XdmValue MathAtan(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ApplyMath(args[0], d => Math.Atan(d));

    private static XdmValue MathAtan2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var a = AtomizeValue(args[0]);
        var b = AtomizeValue(args[1]);
        if (a.IsUndefined || b.IsUndefined) return XdmValue.Undefined;
        return XdmValue.FromDouble(Math.Atan2(ToDoubleValue(a), ToDoubleValue(b)));
    }

    private static XdmValue ApplyMath(XdmValue value, Func<double, double> fn)
    {
        value = AtomizeValue(value);
        if (value.IsUndefined) return XdmValue.Undefined;
        return XdmValue.FromDouble(fn(ToDoubleValue(value)));
    }

    private static XdmValue FunctionLookup(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var qname = args[0].QNameValue;
        int arity = (int)args[1].IntegerValue;
        if (ctx.TryResolveFunction(qname.NamespaceUri, qname.LocalName, arity, out var sig))
            return XdmValue.FromFunction(new NamedFunctionItem(sig.NamespaceUri, sig.LocalName, sig.Arity));
        return XdmValue.Undefined;
    }

    // ------------------------------------------------------------------
    // fn:error
    // ------------------------------------------------------------------

    private static XdmValue Doc_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        // fn:doc accepts a single optional URI. Atomize the argument; empty sequence
        // yields empty sequence; more than one item is a type error.
        var uris = AtomizedUriStrings(args[0]);
        if (uris.Count == 0)
            return XdmValue.Undefined;
        if (uris.Count > 1)
            throw new InvalidOperationException("XPTY0004: fn:doc expects a single URI");

        var uri = uris[0];
        // In XSLT, doc('') resolves against the static base URI (the stylesheet module).
        // In pure XPath with no base URI, the empty string yields the empty sequence.
        var resolvedUri = ResolveDocumentUri(uri, ctx.BaseUri);
        if (string.IsNullOrEmpty(resolvedUri))
            return XdmValue.Undefined;
        var node = ctx.LoadDocument(resolvedUri);
        return XdmValue.FromNode(node);
    }

    private static XdmValue Document_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var pairs = GetDocumentUriPairs(ctx, args[0]);
        if (pairs.Count == 0)
            return XdmValue.Undefined;

        if (pairs.Count == 1)
            return LoadDocumentWithFragment(ctx, pairs[0].uri, pairs[0].baseUri);

        return LoadDocumentsDistinct(ctx, pairs);
    }

    private static XdmValue Document_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var pairs = GetDocumentUriPairs(ctx, args[0], args[1]);
        if (pairs.Count == 0)
            return XdmValue.Undefined;

        if (pairs.Count == 1)
            return LoadDocumentWithFragment(ctx, pairs[0].uri, pairs[0].baseUri);

        return LoadDocumentsDistinct(ctx, pairs);
    }

    /// <summary>
    /// Builds the list of (URI, base-URI) pairs for the XSLT <c>document()</c> function.
    /// When no explicit base is supplied and the supplied value is a node (or sequence of nodes),
    /// each URI is resolved against the base URI of the corresponding node.
    /// </summary>
    private static List<(string uri, string? baseUri)> GetDocumentUriPairs(EvaluationContext ctx, XdmValue value, XdmValue? explicitBase = null)
    {
        var pairs = new List<(string, string?)>();

        var baseNodes = new List<IXdmNode?>();
        if (explicitBase != null)
        {
            if (explicitBase.Value.IsNode)
            {
                baseNodes.Add(explicitBase.Value.NodeValue);
            }
            else if (explicitBase.Value.IsSequence && explicitBase.Value.SequenceValue != null)
            {
                foreach (var item in XdmSequence.FromSource(explicitBase.Value.SequenceValue))
                    baseNodes.Add(item.IsNode ? item.NodeValue : null);
            }
            if (baseNodes.Count == 0)
                baseNodes.Add(null);
        }

        var items = new List<XdmValue>();
        if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                items.Add(item);
        }
        else if (!value.IsUndefined)
        {
            items.Add(value);
        }

        for (int i = 0; i < items.Count; i++)
        {
            var uri = AtomizedString(items[i]);

            string? baseUri;
            if (explicitBase != null)
            {
                var bn = i < baseNodes.Count ? baseNodes[i] : baseNodes[^1];
                baseUri = bn?.BaseUri ?? ctx.BaseUri;
            }
            else if (items[i].IsNode)
            {
                baseUri = items[i].NodeValue!.BaseUri ?? ctx.BaseUri;
            }
            else
            {
                baseUri = ctx.BaseUri;
            }

            pairs.Add((uri, baseUri));
        }

        return pairs;
    }

    private static string ResolveDocumentUri(string uri, string? baseUri)
    {
        // document('') resolves against the stylesheet's base URI per XSLT spec
        if (string.IsNullOrEmpty(uri))
            uri = baseUri ?? string.Empty;
        return uri;
    }

    private static XdmValue LoadDocumentsDistinct(EvaluationContext ctx, List<(string uri, string? baseUri)> pairs)
    {
        // XSLT document() returns the union of the node-sets, which eliminates
        // duplicate document nodes (e.g. the same URI appearing more than once).
        var seen = new HashSet<IXdmNode>();
        var docs = new List<XdmValue>(pairs.Count);
        foreach (var (uri, baseUri) in pairs)
        {
            var loaded = LoadDocumentWithFragment(ctx, uri, baseUri);
            if (loaded.IsNode)
            {
                if (seen.Add(loaded.NodeValue!))
                    docs.Add(loaded);
            }
            else if (!loaded.IsUndefined)
            {
                docs.Add(loaded);
            }
        }

        if (docs.Count == 0)
            return XdmValue.Undefined;
        if (docs.Count == 1)
            return docs[0];
        return XdmValue.FromSequence(MaterializedSequence.FromList(docs));
    }

    private static XdmValue LoadDocumentWithFragment(EvaluationContext ctx, string uri, string? baseUri)
    {
        string? fragment = null;
        var hash = uri.IndexOf('#');
        if (hash >= 0)
        {
            fragment = uri[(hash + 1)..];
            uri = uri[..hash];
        }

        var documentUri = ResolveDocumentUri(uri, baseUri);
        var savedBaseUri = ctx.BaseUri;
        try
        {
            if (!string.IsNullOrEmpty(baseUri))
                ctx.BaseUri = baseUri;
            var doc = ctx.LoadDocument(documentUri);
            if (string.IsNullOrEmpty(fragment))
                return XdmValue.FromNode(doc);

            var target = FindElementById(doc, fragment);
            return target != null ? XdmValue.FromNode(target) : XdmValue.Undefined;
        }
        finally
        {
            ctx.BaseUri = savedBaseUri;
        }
    }

    private static IXdmNode? FindElementById(IXdmNode root, string id)
    {
        foreach (var item in root.Axis(XdmAxis.DescendantOrSelf))
        {
            if (!item.IsNode)
                continue;
            var node = item.NodeValue!;
            if (node.NodeKind != XdmNodeKind.Element)
                continue;
            foreach (var attrItem in node.Axis(XdmAxis.Attribute))
            {
                if (!attrItem.IsNode)
                    continue;
                var attr = attrItem.NodeValue!;
                if (attr.LocalName == "id" && attr.NamespaceUri == "" && attr.StringValue == id)
                    return node;
                if (attr.LocalName == "id" && attr.NamespaceUri == "http://www.w3.org/XML/1998/namespace" && attr.StringValue == id)
                    return node;
            }
        }
        return null;
    }

    private static List<string> AtomizedUriStrings(XdmValue value)
    {
        var result = new List<string>();
        if (value.IsUndefined || IsEmptySequence(value))
            return result;

        if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                if (!item.IsUndefined)
                    result.Add(item.ToString());
            }
        }
        else
        {
            result.Add(value.ToString());
        }
        return result;
    }

    private static XdmValue DocAvailable_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var uris = AtomizedUriStrings(args[0]);
        if (uris.Count == 0)
            return XdmValue.FromBoolean(false);
        if (uris.Count > 1)
            throw new InvalidOperationException("XPTY0004: fn:doc-available expects a single URI");

        var uri = uris[0];
        if (string.IsNullOrEmpty(uri))
            return XdmValue.FromBoolean(false);
        try
        {
            ctx.LoadDocument(uri);
            return XdmValue.FromBoolean(true);
        }
        catch
        {
            return XdmValue.FromBoolean(false);
        }
    }

    private static XdmValue Id_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var ids = ParseIdTokens(args[0].ToString());
        if (ids.Count == 0)
            return XdmValue.Undefined;

        var result = new List<XdmValue>();
        var focus = ctx.ContextItem;
        if (focus.IsNode)
        {
            var doc = focus.NodeValue.Document ?? focus.NodeValue;
            if (doc is not null)
                CollectIdElements(doc, ids, result);
        }
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static XdmValue Id_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var ids = ParseIdTokens(args[0].ToString());
        if (ids.Count == 0)
            return XdmValue.Undefined;

        var result = new List<XdmValue>();
        var nodeArg = args[1];
        var node = nodeArg.IsNode ? nodeArg.NodeValue : null;
        if (node == null && nodeArg.IsSequence && nodeArg.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(nodeArg.SequenceValue))
            {
                if (item.IsNode)
                {
                    node = item.NodeValue;
                    break;
                }
            }
        }
        if (node != null)
        {
            var doc = node.Document ?? node;
            if (doc is not null)
                CollectIdElements(doc, ids, result);
        }
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static XdmValue ElementWithId_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var ids = ParseIdTokens(args[0].ToString());
        if (ids.Count == 0)
            return XdmValue.Undefined;

        var result = new List<XdmValue>();
        var focus = ctx.ContextItem;
        if (focus.IsNode)
        {
            var doc = focus.NodeValue.Document ?? focus.NodeValue;
            if (doc is not null)
                CollectElementWithId(doc, ids, result);
        }
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static HashSet<string> ParseIdTokens(string value)
    {
        var tokens = value.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(s => s.Trim())
                          .Where(s => s.Length > 0);
        return new HashSet<string>(tokens);
    }

    private static void CollectElementWithId(IXdmNode node, HashSet<string> ids, List<XdmValue> result)
    {
        if (node.NodeKind == XdmNodeKind.Element)
        {
            // Check for xml:id attribute (treated as ID even without DTD/schema)
            foreach (var attr in node.Attributes("id", ""))
            {
                if (ids.Contains(AtomizedString(attr).Trim()))
                {
                    result.Add(XdmValue.FromNode(node));
                    return;
                }
            }
            foreach (var attr in node.Attributes("id", "http://www.w3.org/XML/1998/namespace"))
            {
                if (ids.Contains(AtomizedString(attr).Trim()))
                {
                    result.Add(XdmValue.FromNode(node));
                    return;
                }
            }
        }
        foreach (var child in node.Children(XdmNodeKind.Element))
        {
            if (child.IsNode)
                CollectElementWithId(child.NodeValue!, ids, result);
        }
    }

    private static XdmValue DefaultLanguage_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromString("en", "language");

    private static void CollectIdElements(IXdmNode node, HashSet<string> ids, List<XdmValue> result)
    {
        if (node.NodeKind == XdmNodeKind.Element)
        {
            foreach (var attr in node.Attributes("id", ""))
            {
                if (ids.Contains(AtomizedString(attr).Trim()))
                {
                    result.Add(XdmValue.FromNode(node));
                    break;
                }
            }
            foreach (var attr in node.Attributes("id", "http://www.w3.org/XML/1998/namespace"))
            {
                if (ids.Contains(AtomizedString(attr).Trim()))
                {
                    result.Add(XdmValue.FromNode(node));
                    break;
                }
            }
        }
        foreach (var child in node.Children(XdmNodeKind.Element))
        {
            if (child.IsNode)
                CollectIdElements(child.NodeValue!, ids, result);
        }
    }

    private static XdmValue Collection_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.Undefined;

    private static XdmValue Collection_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var uri = args[0].ToString();
        if (string.IsNullOrEmpty(uri))
            return XdmValue.Undefined;

        var resolved = ResolveUriAgainstBase(uri, ctx.BaseUri);
        if (System.IO.Directory.Exists(resolved))
        {
            var files = System.IO.Directory.GetFiles(resolved, "*.xml");
            var nodes = new List<XdmValue>(files.Length);
            foreach (var file in files.OrderBy(f => f, StringComparer.Ordinal))
            {
                nodes.Add(XdmValue.FromNode(ctx.LoadDocument(file)));
            }
            return XdmValue.FromSequence(MaterializedSequence.FromList(nodes));
        }

        return XdmValue.Undefined;
    }

    private static string ResolveUriAgainstBase(string uri, string? baseUri)
    {
        if (System.IO.Path.IsPathRooted(uri))
            return uri;
        if (string.IsNullOrEmpty(baseUri))
            return uri;
        if (Uri.IsWellFormedUriString(baseUri, UriKind.Absolute))
        {
            try
            {
                var baseObj = new Uri(baseUri);
                var resolved = new Uri(baseObj, uri);
                if (resolved.IsFile)
                    return resolved.LocalPath;
                return resolved.AbsoluteUri;
            }
            catch { }
        }
        var baseDir = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(baseUri));
        if (!string.IsNullOrEmpty(baseDir))
            return System.IO.Path.Combine(baseDir, uri);
        return uri;
    }

    private static XdmValue UnparsedText_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => UnparsedText(args[0].ToString(), null, ctx.BaseUri);

    private static XdmValue UnparsedText_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => UnparsedText(args[0].ToString(), args[1].ToString(), ctx.BaseUri);

    private static XdmValue UnparsedText(string href, string? encoding, string? baseUri = null)
    {
        if (string.IsNullOrEmpty(href))
            throw new InvalidOperationException("FOUT1170");
        try
        {
            var path = ResolveUriAgainstBase(href, baseUri);
            if (!File.Exists(path))
                throw new InvalidOperationException("FOUT1170");
            encoding ??= "UTF-8";
            var enc = System.Text.Encoding.GetEncoding(encoding);
            var content = File.ReadAllText(path, enc);
            ValidateXmlCharacters(content);
            return XdmValue.FromString(content);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"FOUT1170: {ex.Message}");
        }
    }

    private static XdmValue UnparsedTextAvailable_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => UnparsedTextAvailable(args[0].ToString(), null, ctx.BaseUri);

    private static XdmValue UnparsedTextAvailable_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => UnparsedTextAvailable(args[0].ToString(), args[1].ToString(), ctx.BaseUri);

    private static XdmValue UnparsedTextAvailable(string href, string? encoding, string? baseUri = null)
    {
        if (string.IsNullOrEmpty(href))
            return XdmValue.False;
        try
        {
            var path = ResolveUriAgainstBase(href, baseUri);
            if (!File.Exists(path))
                return XdmValue.False;
            if (encoding is not null)
                _ = System.Text.Encoding.GetEncoding(encoding);
            return XdmValue.True;
        }
        catch
        {
            return XdmValue.False;
        }
    }

    private static XdmValue UnparsedTextLines_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => UnparsedTextLines(args[0].ToString(), null, ctx.BaseUri);

    private static XdmValue UnparsedTextLines_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => UnparsedTextLines(args[0].ToString(), args[1].ToString(), ctx.BaseUri);

    private static XdmValue UnparsedTextLines(string href, string? encoding, string? baseUri = null)
    {
        var textValue = UnparsedText(href, encoding, baseUri);
        var text = textValue.StringValue;
        if (string.IsNullOrEmpty(text))
            return XdmValue.Undefined;
        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines = normalized.Split('\n');
        // A final line terminator does not create an empty trailing line.
        if (lines.Length > 0 && string.IsNullOrEmpty(lines[lines.Length - 1]) && normalized.EndsWith('\n'))
            lines = lines[..^1];
        var items = new List<XdmValue>(lines.Length);
        foreach (var line in lines)
            items.Add(XdmValue.FromString(line));
        return XdmValue.FromSequence(MaterializedSequence.FromList(items));
    }

    private static void ValidateXmlCharacters(string text)
    {
        foreach (var rune in text.EnumerateRunes())
        {
            var c = rune.Value;
            if (c == 0x09 || c == 0x0A || c == 0x0D)
                continue;
            if (c >= 0x20 && c <= 0xD7FF)
                continue;
            if (c >= 0xE000 && c <= 0xFFFD)
                continue;
            if (c >= 0x10000 && c <= 0x10FFFF)
                continue;
            throw new InvalidOperationException("FOUT1190");
        }
    }

    private static string ResolveUri(string href)
    {
        if (Path.IsPathRooted(href) || href.Contains(':'))
            return href;
        return Path.GetFullPath(href);
    }

    private static XdmValue RandomNumberGenerator_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => CreateRandomGenerator(123);

    private static XdmValue RandomNumberGenerator_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var seedValue = AtomizeValue(args[0]);
        if (seedValue.IsUndefined)
            return CreateRandomGenerator(123);
        long seed = seedValue.Kind switch
        {
            XdmValueKind.Integer => seedValue.IntegerValue,
            XdmValueKind.Decimal => (long)seedValue.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => (long)seedValue.DoubleValue,
            _ => long.TryParse(seedValue.ToString(), out var s) ? s : 0
        };
        return CreateRandomGenerator(seed);
    }

    private static XdmValue CreateRandomGenerator(long seed)
    {
        var rng = new SplitMix64(seed);
        double number = rng.NextDouble();
        long nextSeed = rng.State;

        // next: a function that returns the next generator
        var nextFunc = new DelegateFunctionItem(0, (_, _) => CreateRandomGenerator(nextSeed));

        // permute: a function that takes a sequence and returns it in random order
        var permuteFunc = new DelegateFunctionItem(1, (ctx, a) => PermuteSequence(a[0], new SplitMix64(nextSeed)));

        var map = new XdmMap();
        map.Add(XdmValue.FromString("number"), XdmValue.FromDouble(number));
        map.Add(XdmValue.FromString("next"), XdmValue.FromFunction(nextFunc));
        map.Add(XdmValue.FromString("permute"), XdmValue.FromFunction(permuteFunc));
        return XdmValue.FromMap(map);
    }

    private static XdmValue PermuteSequence(XdmValue value, SplitMix64 rng)
    {
        var items = Materialize(value);
        if (items.Count <= 1)
            return value;
        // Fisher-Yates shuffle
        for (int i = items.Count - 1; i > 0; i--)
        {
            int j = (int)(rng.NextDouble() * (i + 1));
            (items[i], items[j]) = (items[j], items[i]);
        }
        return XdmValue.FromSequence(MaterializedSequence.FromList(items));
    }

    private sealed class SplitMix64
    {
        public long State { get; private set; }
        public SplitMix64(long seed) => State = seed;
        public ulong Next()
        {
            ulong z = (ulong)(State += unchecked((long)0x9e3779b97f4a7c15));
            z = (z ^ (z >> 30)) * 0xbf58476d1ce4e5b9;
            z = (z ^ (z >> 27)) * 0x94d049bb133111eb;
            return z ^ (z >> 31);
        }
        public double NextDouble()
        {
            // Generate a double in [0, 1) using 53 bits of precision
            return (Next() >> 11) * (1.0 / (1ul << 53));
        }
    }

    private static XdmValue Serialize_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var value = args[0];
        var options = args[1].MapValue;
        bool indent = false;
        string method = "xml";
        if (options.TryGetValue(XdmValue.FromString("indent"), out var indentVal))
            indent = indentVal.BooleanValue;
        if (options.TryGetValue(XdmValue.FromString("method"), out var methodVal))
            method = methodVal.ToString().ToLowerInvariant();

        if (value.IsUndefined)
            return XdmValue.FromString(string.Empty);

        if (!value.IsSequence)
            return XdmValue.FromString(SerializeItem(value, indent, method));

        var sb = new StringBuilder();
        foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
            sb.Append(SerializeItem(item, indent, method));
        return XdmValue.FromString(sb.ToString());
    }

    private static string SerializeItem(XdmValue value, bool indent, string method)
    {
        if (method == "json")
        {
            if (value.IsNode)
            {
                if (value.NodeValue.NodeKind == XdmNodeKind.Document)
                {
                    // For JSON method on document, serialize the root element's content
                    var doc = value.NodeValue;
                    foreach (var child in doc.Axis(XdmAxis.Child))
                    {
                        if (child.NodeValue.NodeKind == XdmNodeKind.Element)
                            return child.NodeValue.ToXmlString();
                    }
                }
                return value.NodeValue.ToXmlString();
            }
            return SerializeJson(value);
        }

        if (value.IsNode)
            return value.NodeValue.ToXmlString();
        return value.ToString();
    }

    /// <summary>
    /// Serializes an XDM value as JSON.
    /// </summary>
    private static string SerializeJson(XdmValue value)
    {
        if (value.IsUndefined)
            return "null";

        if (value.IsSequence && value.SequenceValue != null)
        {
            var sb = new StringBuilder("[");
            bool first = true;
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                if (!first)
                    sb.Append(',');
                sb.Append(SerializeJson(item));
                first = false;
            }
            sb.Append(']');
            return sb.ToString();
        }

        if (value.IsMap)
        {
            var sb = new StringBuilder("{");
            bool first = true;
            foreach (var kvp in value.MapValue.Entries)
            {
                if (!first)
                    sb.Append(',');
                sb.Append(EncodeJsonString(kvp.Key.ToString()));
                sb.Append(':');
                sb.Append(SerializeJson(kvp.Value));
                first = false;
            }
            sb.Append('}');
            return sb.ToString();
        }

        if (value.IsArray)
        {
            var arr = value.ArrayValue;
            var sb = new StringBuilder("[");
            for (int i = 1; i <= arr.Count; i++)
            {
                if (i > 1)
                    sb.Append(',');
                sb.Append(SerializeJson(arr.Get(i)));
            }
            sb.Append(']');
            return sb.ToString();
        }

        if (value.Kind == XdmValueKind.Boolean)
            return value.BooleanValue ? "true" : "false";

        if (value.Kind == XdmValueKind.String)
            return EncodeJsonString(value.StringValue);

        if (IsNumeric(value))
            return value.ToString();

        return EncodeJsonString(value.ToString());
    }

    /// <summary>
    /// Encodes a string as a JSON string literal, escaping quotes, backslashes and
    /// control characters.
    /// </summary>
    private static string EncodeJsonString(string value)
    {
        var sb = new StringBuilder(value.Length + 2);
        sb.Append('"');
        foreach (var c in value)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < 0x20)
                        sb.Append($"\\u{(int)c:X4}");
                    else
                        sb.Append(c);
                    break;
            }
        }
        sb.Append('"');
        return sb.ToString();
    }

    private static XdmValue Error_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => throw new InvalidOperationException("fn:error() called");

    private static XdmValue Error_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => throw new InvalidOperationException($"fn:error({args[0].QNameValue}) called");

    private static XdmValue Error_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => throw new InvalidOperationException($"fn:error({args[0].QNameValue}): {args[1]}");

    private static XdmValue Error_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => throw new InvalidOperationException($"fn:error({args[0].QNameValue}): {args[1]}");

    private static XdmValue Trace_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var value = args[0];
        System.Diagnostics.Trace.WriteLine($"[trace] {value}");
        return value;
    }

    private static XdmValue Trace_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var value = args[0];
        var label = args[1].ToString();
        System.Diagnostics.Trace.WriteLine($"[{label}] {value}");
        return value;
    }

    private static XdmValue Boolean_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (arg.IsUndefined || IsEmptySequence(arg))
            return XdmValue.FromBoolean(false);

        if (arg.IsSequence)
        {
            var items = Materialize(arg);
            if (items.Count == 0)
                return XdmValue.FromBoolean(false);
            if (items[0].IsNode)
                return XdmValue.FromBoolean(true);
            if (items.Count > 1)
                throw new InvalidOperationException("FORG0006");
            arg = items[0];
        }

        if (arg.Kind == XdmValueKind.String)
        {
            string? schemaType = arg.SchemaTypeName?.ToLowerInvariant();
            if (schemaType is "gyear" or "gyearmonth" or "gmonthday" or "gday" or "gmonth"
                or "hexbinary" or "base64binary")
                throw new InvalidOperationException("FORG0006");
            return XdmValue.FromBoolean(arg.EffectiveBooleanValue());
        }

        return arg.Kind switch
        {
            XdmValueKind.Boolean or XdmValueKind.Integer
                or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float
                or XdmValueKind.Node
                => XdmValue.FromBoolean(arg.EffectiveBooleanValue()),
            XdmValueKind.QName => throw new InvalidOperationException("FORG0006"),
            XdmValueKind.DateTime or XdmValueKind.Date or XdmValueKind.Time
                or XdmValueKind.Duration
                => throw new InvalidOperationException("FORG0006"),
            _ => throw new InvalidOperationException("FORG0006")
        };
    }

    private static bool IsEmptySequence(XdmValue value)
    {
        if (value.IsUndefined) return true;
        if (!value.IsSequence) return false;
        foreach (var _ in XdmSequence.FromSource(value.SequenceValue!))
            return false;
        return true;
    }

    private static int SequenceLength(XdmValue value)
    {
        if (value.IsUndefined) return 0;
        if (!value.IsSequence) return 1;
        int count = 0;
        foreach (var _ in XdmSequence.FromSource(value.SequenceValue!))
        {
            count++;
            if (count > 2) return count; // Don't need exact count past 2
        }
        return count;
    }

    private static XdmValue ZeroOrOne_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (IsEmptySequence(arg))
            return XdmValue.Undefined;
        if (!arg.IsSequence)
            return arg;
        if (SequenceLength(arg) > 1)
            throw new InvalidOperationException("fn:zero-or-one called with a sequence containing more than one item.");
        return XdmValue.Undefined;
    }

    private static XdmValue OneOrMore_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (IsEmptySequence(arg))
            throw new InvalidOperationException("fn:one-or-more called with an empty sequence.");
        if (!arg.IsSequence)
            return arg;
        return arg;
    }

    private static XdmValue ExactlyOne_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (IsEmptySequence(arg))
            throw new InvalidOperationException("fn:exactly-one called with an empty sequence.");
        if (!arg.IsSequence)
            return arg;
        XdmValue first = default;
        int count = 0;
        foreach (var item in XdmSequence.FromSource(arg.SequenceValue!))
        {
            first = item;
            count++;
            if (count > 1)
                throw new InvalidOperationException("fn:exactly-one called with a sequence containing more than one item.");
        }
        if (count == 0)
            throw new InvalidOperationException("fn:exactly-one called with an empty sequence.");
        return first;
    }

    private static XdmValue BaseUri_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var item = ctx.ContextItem;
        if (item.IsUndefined || IsEmptySequence(item))
            throw new InvalidOperationException("XPDY0002: fn:base-uri() called with no context item.");
        if (!item.IsNode)
            throw new InvalidOperationException("XPTY0004: fn:base-uri() context item is not a node.");
        var uri = item.NodeValue!.BaseUri;
        return string.IsNullOrEmpty(uri) ? XdmValue.Undefined : XdmValue.FromString(uri, "anyURI");
    }

    private static XdmValue BaseUri_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (IsEmptySequence(arg))
            return XdmValue.Undefined;
        if (arg.IsSequence)
        {
            XdmValue? first = null;
            int count = 0;
            foreach (var x in XdmSequence.FromSource(arg.SequenceValue!))
            {
                first = x;
                count++;
                if (count > 1) break;
            }
            if (count == 0) return XdmValue.Undefined;
            if (count > 1) throw new InvalidOperationException("XPTY0004");
            arg = first!.Value;
        }
        if (!arg.IsNode)
            throw new InvalidOperationException("XPTY0004: fn:base-uri() argument is not a node.");
        var uri = arg.NodeValue!.BaseUri;
        return string.IsNullOrEmpty(uri) ? XdmValue.Undefined : XdmValue.FromString(uri, "anyURI");
    }

    private static XdmValue DocumentUri_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var item = ctx.ContextItem;
        if (item.IsUndefined || IsEmptySequence(item))
            throw new InvalidOperationException("XPDY0002: fn:document-uri() called with no context item.");
        if (!item.IsNode)
            throw new InvalidOperationException("XPTY0004: fn:document-uri() context item is not a node.");
        var node = item.NodeValue!;
        if (node.NodeKind != XdmNodeKind.Document)
            return XdmValue.Undefined;
        var uri = node.DocumentUri;
        return string.IsNullOrEmpty(uri) ? XdmValue.Undefined : XdmValue.FromString(uri);
    }

    private static XdmValue DocumentUri_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (IsEmptySequence(arg))
            return XdmValue.Undefined;
        if (arg.IsSequence)
        {
            XdmValue? first = null;
            int count = 0;
            foreach (var x in XdmSequence.FromSource(arg.SequenceValue!))
            {
                first = x;
                count++;
                if (count > 1) break;
            }
            if (count == 0) return XdmValue.Undefined;
            if (count > 1) throw new InvalidOperationException("XPTY0004");
            arg = first!.Value;
        }
        if (!arg.IsNode)
            throw new InvalidOperationException("XPTY0004: fn:document-uri() argument is not a node.");
        var node = arg.NodeValue!;
        if (node.NodeKind != XdmNodeKind.Document)
            return XdmValue.Undefined;
        var uri = node.DocumentUri;
        return string.IsNullOrEmpty(uri) ? XdmValue.Undefined : XdmValue.FromString(uri);
    }

    private static XdmValue Nilled_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var item = ctx.ContextItem;
        if (item.IsUndefined || IsEmptySequence(item))
            throw new InvalidOperationException("XPDY0002: fn:nilled() called with no context item.");
        if (!item.IsNode || item.NodeValue == null)
            throw new InvalidOperationException("XPTY0004: fn:nilled() argument is not a node.");
        return NilledOfNode(item.NodeValue);
    }

    private static XdmValue Nilled_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (IsEmptySequence(arg))
            return XdmValue.Undefined;
        if (arg.IsSequence)
        {
            XdmValue? first = null;
            int count = 0;
            foreach (var x in XdmSequence.FromSource(arg.SequenceValue!))
            {
                first = x;
                count++;
                if (count > 1) break;
            }
            if (count == 0) return XdmValue.Undefined;
            if (count > 1) throw new InvalidOperationException("XPTY0004");
            arg = first!.Value;
        }
        if (!arg.IsNode)
            throw new InvalidOperationException("XPTY0004: fn:nilled() argument is not a node.");
        return NilledOfNode(arg.NodeValue!);
    }

    private static XdmValue NilledOfNode(IXdmNode node)
    {
        // fn:nilled is defined only for element nodes. For all other node kinds it
        // returns the empty sequence; for elements it returns false unless the
        // element is schema-validated with xsi:nil="true" (not supported here).
        return node.NodeKind == XdmNodeKind.Element ? XdmValue.False : XdmValue.Undefined;
    }

    // ------------------------------------------------------------------
    // Sequence functions
    // ------------------------------------------------------------------

    private static XdmValue InsertBefore(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var target = Materialize(args[0]);
        long pos = args[1].IntegerValue;
        var inserts = Materialize(args[2]);
        if (pos < 1) pos = 1;
        if (pos > target.Count + 1) pos = target.Count + 1;
        target.InsertRange((int)pos - 1, inserts);
        return XdmValue.FromSequence(MaterializedSequence.FromList(target));
    }

    private static XdmValue Remove(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var target = Materialize(args[0]);
        var posValue = AtomizeValue(args[1]);
        if (posValue.IsUndefined || IsEmptySequence(posValue))
            return XdmValue.FromSequence(MaterializedSequence.FromList(target));
        double posD = ToDoubleValueStrict(posValue);
        if (double.IsNaN(posD) || posD < 1 || posD > target.Count)
            return XdmValue.FromSequence(MaterializedSequence.FromList(target));
        target.RemoveAt((int)posD - 1);
        return XdmValue.FromSequence(MaterializedSequence.FromList(target));
    }

    private static XdmValue Reverse(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var items = AsSequence(args[0]).ToList();
        items.Reverse();
        return XdmValue.FromSequence(MaterializedSequence.FromList(items));
    }

    private static XdmValue Subsequence_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        double startD = ToDoubleValueStrict(args[1]);
        if (double.IsNaN(startD)) return XdmValue.Undefined;
        double startRounded = Math.Floor(startD + 0.5);
        if (double.IsPositiveInfinity(startRounded)) return XdmValue.Undefined;

        // Fast path for lazy integer ranges
        if (args[0].IsSequence && args[0].SequenceValue is IntegerRangeSequence range)
        {
            long newFrom;
            if (double.IsNegativeInfinity(startRounded) || startRounded <= 1.0)
            {
                newFrom = range.From;
            }
            else
            {
                double offset = startRounded - 1.0;
                if (offset >= (double)long.MaxValue)
                    return XdmValue.Undefined;
                long offsetL = (long)offset;
                if (offsetL > 0 && range.From > long.MaxValue - offsetL)
                    return XdmValue.Undefined;
                newFrom = range.From + offsetL;
                if (newFrom > range.To)
                    return XdmValue.Undefined;
            }
            return XdmValue.FromSequence(XdmSequence.FromSource(new IntegerRangeSequence(newFrom, range.To)));
        }

        var seq = AsSequence(args[0]);
        var result = new List<XdmValue>();
        long pos = 1;
        foreach (var item in seq)
        {
            if (pos >= startRounded)
                result.Add(item);
            pos++;
        }
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static XdmValue Subsequence_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        double startD = ToDoubleValueStrict(args[1]);
        double lenD = ToDoubleValueStrict(args[2]);
        if (double.IsNaN(startD) || double.IsNaN(lenD)) return XdmValue.Undefined;
        double startRounded = Math.Floor(startD + 0.5);
        double lenRounded = Math.Floor(lenD + 0.5);
        double end = startRounded + lenRounded;
        if (double.IsNaN(end)) return XdmValue.Undefined;
        if (double.IsPositiveInfinity(startRounded)) return XdmValue.Undefined;
        if (!double.IsPositiveInfinity(end) && end <= 1.0) return XdmValue.Undefined;

        // Fast path for lazy integer ranges
        if (args[0].IsSequence && args[0].SequenceValue is IntegerRangeSequence range)
        {
            long newFrom;
            if (double.IsNegativeInfinity(startRounded) || startRounded <= 1.0)
            {
                newFrom = range.From;
            }
            else
            {
                double offset = startRounded - 1.0;
                if (offset >= (double)long.MaxValue)
                    return XdmValue.Undefined;
                long offsetL = (long)offset;
                if (offsetL > 0 && range.From > long.MaxValue - offsetL)
                    return XdmValue.Undefined;
                newFrom = range.From + offsetL;
            }

            long newTo;
            if (double.IsPositiveInfinity(end))
            {
                newTo = range.To;
            }
            else
            {
                // end is finite and > 1 (guarded above)
                double newToD = (double)range.From + end - 2.0;
                if (newToD >= (double)long.MaxValue)
                {
                    newTo = range.To;
                }
                else
                {
                    newTo = (long)newToD;
                    if (newTo > range.To)
                        newTo = range.To;
                }
            }

            if (newFrom > newTo)
                return XdmValue.Undefined;
            return XdmValue.FromSequence(XdmSequence.FromSource(new IntegerRangeSequence(newFrom, newTo)));
        }

        var seq = AsSequence(args[0]);
        var result = new List<XdmValue>();
        long pos = 1;
        foreach (var item in seq)
        {
            if (pos >= startRounded && pos < end)
                result.Add(item);
            pos++;
        }
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static XdmValue DistinctValues_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => DistinctValuesImpl(args[0], ctx.DefaultCollation);

    private static XdmValue DistinctValuesImpl(XdmValue sequence, string collation)
    {
        var items = Materialize(sequence);
        var seen = new List<XdmValue>();
        var result = new List<XdmValue>();
        foreach (var item in items)
        {
            var atomized = AtomizeValue(item);
            bool isDistinct = true;
            foreach (var s in seen)
            {
                if (AtomicValuesEqual(atomized, s, collation))
                {
                    isDistinct = false;
                    break;
                }
            }
            if (isDistinct)
            {
                seen.Add(atomized);
                result.Add(item);
            }
        }
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static XdmValue DistinctValues_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string collation = AtomizedString(args[1]);
        ValidateCollation(collation);
        return DistinctValuesImpl(args[0], collation);
    }

    /// <summary>
    /// Compares two atomized XDM values using XPath <c>eq</c> semantics and the
    /// supplied collation for string comparisons.
    /// </summary>
    private static bool AtomicValuesEqual(XdmValue a, XdmValue b, string collation)
    {
        if (a.IsUndefined || b.IsUndefined)
            return false;

        if (IsNumericValue(a) && IsNumericValue(b))
        {
            bool aIsNaN = (a.Kind is XdmValueKind.Double or XdmValueKind.Float) && double.IsNaN(a.DoubleValue);
            bool bIsNaN = (b.Kind is XdmValueKind.Double or XdmValueKind.Float) && double.IsNaN(b.DoubleValue);
            if (aIsNaN || bIsNaN)
                return false;

            if (a.Kind == XdmValueKind.Double || b.Kind == XdmValueKind.Double)
                return ToDoubleValue(a) == ToDoubleValue(b);
            if (a.Kind == XdmValueKind.Float || b.Kind == XdmValueKind.Float)
                return (float)ToDoubleValue(a) == (float)ToDoubleValue(b);
            if (a.Kind == XdmValueKind.Decimal || b.Kind == XdmValueKind.Decimal)
                return ToDecimalValue(a) == ToDecimalValue(b);
            return a.IntegerValue == b.IntegerValue;
        }

        if (a.Kind != b.Kind)
        {
            // Untyped atomic is cast to the other operand's type for comparison.
            if (IsUntypedAtomic(a))
                return UntypedAtomicEqualsOther(a, b, collation);
            if (IsUntypedAtomic(b))
                return UntypedAtomicEqualsOther(b, a, collation);
            return false;
        }

        return a.Kind switch
        {
            XdmValueKind.String => CompareStrings(a.StringValue, b.StringValue, collation) == 0,
            XdmValueKind.Boolean => a.BooleanValue == b.BooleanValue,
            XdmValueKind.Integer => a.IntegerValue == b.IntegerValue,
            XdmValueKind.Decimal => a.DecimalValue == b.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => a.DoubleValue == b.DoubleValue,
            XdmValueKind.DateTime => a.DateTimeValue == b.DateTimeValue,
            XdmValueKind.Date => a.DateValue == b.DateValue,
            XdmValueKind.Time => a.TimeValue == b.TimeValue,
            XdmValueKind.QName => a.QNameValue.Equals(b.QNameValue),
            XdmValueKind.Uri => CompareStrings(a.StringValue, b.StringValue, collation) == 0,
            _ => false
        };
    }

    private static bool IsUntypedAtomic(XdmValue value)
        => value.Kind == XdmValueKind.String &&
           string.Equals(value.SchemaTypeName, "untypedAtomic", StringComparison.OrdinalIgnoreCase);

    private static bool UntypedAtomicEqualsOther(XdmValue untyped, XdmValue other, string collation)
    {
        if (other.Kind is XdmValueKind.String or XdmValueKind.Uri)
            return CompareStrings(untyped.StringValue, other.StringValue, collation) == 0;
        return false;
    }

    private static bool IsNumericValue(XdmValue value)
        => value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Float or XdmValueKind.Double;

    private static XdmValue IndexOf_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => IndexOfImpl(args[0], AtomizedString(args[1]), ctx.DefaultCollation);

    private static XdmValue IndexOfImpl(XdmValue sequence, string search, string collation)
    {
        var seq = Materialize(sequence);
        var result = new List<XdmValue>();
        for (int i = 0; i < seq.Count; i++)
        {
            if (CompareStrings(AtomizedString(seq[i]), search, collation) == 0)
                result.Add(XdmValue.FromInteger(i + 1));
        }
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static XdmValue IndexOf_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string collation = AtomizedString(args[2]);
        ValidateCollation(collation);
        return IndexOfImpl(args[0], AtomizedString(args[1]), collation);
    }

    // ------------------------------------------------------------------
    // Aggregate functions
    // ------------------------------------------------------------------

    private static XdmValue Sum_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var items = Materialize(args[0]);
        if (items.Count == 0) return XdmValue.FromInteger(0);
        return Sum(items);
    }

    private static XdmValue Sum_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var items = Materialize(args[0]);
        if (items.Count == 0) return args[1];
        return Sum(items);
    }

    private static XdmValue Avg(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var items = Materialize(args[0]);
        if (items.Count == 0) return XdmValue.Undefined;

        bool hasNumeric = false;
        bool hasYearMonth = false;
        bool hasDayTime = false;
        bool hasGenericDuration = false;

        foreach (var item in items)
        {
            var a = AtomizeValue(item);
            if (a.Kind == XdmValueKind.Integer || a.Kind == XdmValueKind.Decimal
                || a.Kind == XdmValueKind.Double || a.Kind == XdmValueKind.Float)
            {
                hasNumeric = true;
            }
            else if (a.Kind == XdmValueKind.Duration)
            {
                var s = a.DurationValue;
                if (IsGenericDurationString(s))
                    hasGenericDuration = true;
                else if (IsYearMonthDurationString(s))
                    hasYearMonth = true;
                else if (IsDayTimeDurationString(s))
                    hasDayTime = true;
                else
                    throw new InvalidOperationException("FORG0006");
            }
            else if (a.Kind == XdmValueKind.String && double.TryParse(a.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                hasNumeric = true;
            }
            else
            {
                throw new InvalidOperationException("FORG0006");
            }
        }

        int categories = (hasNumeric ? 1 : 0) + (hasYearMonth ? 1 : 0) + (hasDayTime ? 1 : 0) + (hasGenericDuration ? 1 : 0);
        if (categories != 1)
            throw new InvalidOperationException("FORG0006");

        if (hasGenericDuration)
            throw new InvalidOperationException("FORG0006");

        var total = Sum(items);
        if (total.Kind == XdmValueKind.Duration)
        {
            var s = total.DurationValue;
            if (IsYearMonthDurationString(s))
            {
                var (years, months, _, _, _, _) = ParseDuration(s);
                long totalMonths = years * 12 + months;
                return XdmValue.FromDuration(FormatYearMonthDuration((long)Math.Round((decimal)totalMonths / items.Count)));
            }
            if (IsDayTimeDurationString(s))
            {
                var (_, _, days, hours, minutes, seconds) = ParseDuration(s);
                decimal totalSec = days * 86400m + hours * 3600m + minutes * 60m + seconds;
                return XdmValue.FromDuration(FormatDayTimeDurationFromSeconds(totalSec / items.Count));
            }
            throw new InvalidOperationException("FORG0006");
        }
        return total.Kind switch
        {
            XdmValueKind.Integer => XdmValue.FromDecimal((decimal)total.IntegerValue / items.Count),
            XdmValueKind.Decimal => XdmValue.FromDecimal(total.DecimalValue / items.Count),
            XdmValueKind.Float => XdmValue.FromFloat((float)total.DoubleValue / items.Count),
            _ => XdmValue.FromDouble(total.DoubleValue / items.Count)
        };
    }

    private static XdmValue Min_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var items = Materialize(args[0]);
        if (items.Count == 0) return XdmValue.Undefined;
        return MinMax(items, true, ctx.DefaultCollation);
    }

    private static XdmValue Min_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var items = Materialize(args[0]);
        if (items.Count == 0) return XdmValue.Undefined;
        string collation = AtomizedString(args[1]);
        ValidateCollation(collation);
        return MinMax(items, true, collation);
    }

    private static XdmValue Max_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var items = Materialize(args[0]);
        if (items.Count == 0) return XdmValue.Undefined;
        return MinMax(items, false, ctx.DefaultCollation);
    }

    private static XdmValue Max_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var items = Materialize(args[0]);
        if (items.Count == 0) return XdmValue.Undefined;
        string collation = AtomizedString(args[1]);
        ValidateCollation(collation);
        return MinMax(items, false, collation);
    }

    private static XdmValue StringJoin_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => StringJoin(ctx, new[] { args[0], XdmValue.FromString("") }.AsSpan());

    private static XdmValue StringJoin_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => StringJoin(ctx, args);

    private static XdmValue StringJoin(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var items = Materialize(args[0]);
        string sep = AtomizedString(args[1]);
        var strings = new List<string>(items.Count);
        foreach (var item in items)
            strings.Add(AtomizedString(item));
        return XdmValue.FromString(string.Join(sep, strings));
    }

    private static XdmValue ConcatN(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var sb = new StringBuilder();
        foreach (var arg in args)
            sb.Append(AtomizedString(arg));
        return XdmValue.FromString(sb.ToString());
    }

    // ------------------------------------------------------------------
    // Map functions
    // ------------------------------------------------------------------

    private static XdmValue MapGet(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var map = args[0].MapValue;
        var key = AtomizeMapKey(args[1]);
        if (map.TryGetValue(key, out var value))
            return value;
        return XdmValue.Undefined;
    }

    private static XdmValue MapSize(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromInteger(args[0].MapValue.Count);

    private static XdmValue MapContains(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromBoolean(args[0].MapValue.ContainsKey(AtomizeMapKey(args[1])));

    private static XdmValue MapKeys(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var keys = args[0].MapValue.Keys.ToList();
        return XdmValue.FromSequence(MaterializedSequence.FromList(keys));
    }

    private static XdmValue MapMerge(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var result = new XdmMap();
        var maps = Materialize(args[0]);
        foreach (var mapVal in maps)
        {
            if (mapVal.IsMap)
            {
                foreach (var kvp in mapVal.MapValue.Entries)
                    result.Add(kvp.Key, kvp.Value);
            }
        }
        return XdmValue.FromMap(result);
    }

    private static XdmValue MapRemove(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var map = args[0].MapValue;
        var key = AtomizeMapKey(args[1]);
        var result = new XdmMap();
        foreach (var kvp in map.Entries)
        {
            if (!XdmValueEqualityComparer.Instance.Equals(kvp.Key, key))
                result.Add(kvp.Key, kvp.Value);
        }
        return XdmValue.FromMap(result);
    }

    private static XdmValue MapPut(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var map = args[0].MapValue;
        var key = AtomizeMapKey(args[1]);
        var value = args[2];
        var result = new XdmMap();
        foreach (var kvp in map.Entries)
        {
            if (!XdmValueEqualityComparer.Instance.Equals(kvp.Key, key))
                result.Add(kvp.Key, kvp.Value);
        }
        result.Add(key, value);
        return XdmValue.FromMap(result);
    }

    // ------------------------------------------------------------------
    // Array functions
    // ------------------------------------------------------------------

    private static XdmValue ArraySize(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromInteger(args[0].ArrayValue.Count);

    private static XdmValue ArrayGet(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arr = args[0].ArrayValue;
        int idx = (int)args[1].IntegerValue;
        return arr.Get(idx);
    }

    private static XdmValue ArrayContains(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromBoolean(args[0].ArrayValue.Contains(args[1]));

    private static XdmValue ArrayHead(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => args[0].ArrayValue.Get(1);

    private static XdmValue ArrayPut(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arr = args[0].ArrayValue;
        int idx = (int)args[1].IntegerValue;
        var value = args[2];
        var items = new List<XdmValue>();
        foreach (var item in arr.Values)
            items.Add(item);
        // XPath arrays are 1-based
        int pos = idx - 1;
        if (pos >= 0 && pos < items.Count)
            items[pos] = value;
        else if (pos == items.Count)
            items.Add(value);
        return XdmValue.FromArray(new XdmArray(items));
    }

    private static XdmValue ArrayTail(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arr = args[0].ArrayValue;
        var items = new List<XdmValue>();
        bool first = true;
        foreach (var item in arr.Values)
        {
            if (first) { first = false; continue; }
            items.Add(item);
        }
        return XdmValue.FromArray(new XdmArray(items));
    }

    private static XdmValue ArrayRemove(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arr = args[0].ArrayValue;
        var removePositions = new HashSet<int>();
        foreach (var posVal in AsSequence(args[1]))
        {
            int pos = (int)posVal.IntegerValue;
            if (pos >= 1 && pos <= arr.Count)
                removePositions.Add(pos);
        }
        var items = new List<XdmValue>();
        int idx = 1;
        foreach (var item in arr.Values)
        {
            if (!removePositions.Contains(idx))
                items.Add(item);
            idx++;
        }
        return XdmValue.FromArray(new XdmArray(items));
    }

    private static XdmValue MapEntry(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var map = new XdmMap();
        map.Add(AtomizeMapKey(args[0]), args[1]);
        return XdmValue.FromMap(map);
    }

    private static XdmValue MapForEach(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var map = args[0].MapValue;
        var func = args[1];
        var result = new List<XdmValue>();
        foreach (var kvp in map.Entries)
        {
            var r = VmEngine.InvokeFunctionItem(func, ctx, new[] { kvp.Key, kvp.Value });
            AppendResult(r, result);
        }
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static XdmValue ArrayAppend(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arr = args[0].ArrayValue;
        var items = new List<XdmValue>();
        foreach (var item in arr.Values)
            items.Add(item);
        items.Add(args[1]);
        return XdmValue.FromArray(new XdmArray(items));
    }

    private static XdmValue ArraySubarray_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ArraySubarray(ctx, args[0].ArrayValue, (int)args[1].IntegerValue, int.MaxValue);

    private static XdmValue ArraySubarray_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ArraySubarray(ctx, args[0].ArrayValue, (int)args[1].IntegerValue, (int)args[2].IntegerValue);

    private static XdmValue ArraySubarray(EvaluationContext ctx, XdmArray arr, int start, int length)
    {
        var items = new List<XdmValue>();
        int i = 1;
        foreach (var item in arr.Values)
        {
            if (i >= start)
            {
                if (length-- <= 0) break;
                items.Add(item);
            }
            i++;
        }
        return XdmValue.FromArray(new XdmArray(items));
    }

    private static XdmValue ArrayReverse(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arr = args[0].ArrayValue;
        var items = new List<XdmValue>();
        foreach (var item in arr.Values)
            items.Add(item);
        items.Reverse();
        return XdmValue.FromArray(new XdmArray(items));
    }

    private static XdmValue ArrayJoin(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var result = new List<XdmValue>();
        foreach (var item in AsSequence(args[0]))
        {
            if (item.IsArray)
            {
                foreach (var arrItem in item.ArrayValue.Values)
                    result.Add(arrItem);
            }
        }
        return XdmValue.FromArray(new XdmArray(result));
    }

    private static XdmValue ArrayFilter(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arr = args[0].ArrayValue;
        var func = args[1];
        var result = new List<XdmValue>();
        foreach (var item in arr.Values)
        {
            var pred = VmEngine.InvokeFunctionItem(func, ctx, new[] { item });
            if (pred.IsUndefined)
                throw new InvalidOperationException("XPTY0004");
            if (pred.Kind != XdmValueKind.Boolean)
                throw new InvalidOperationException("XPTY0004");
            if (pred.BooleanValue)
                result.Add(item);
        }
        return XdmValue.FromArray(new XdmArray(result));
    }

    private static XdmValue ArrayFoldLeft(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arr = args[0].ArrayValue;
        var accumulator = args[1];
        var func = args[2];
        foreach (var item in arr.Values)
        {
            accumulator = VmEngine.InvokeFunctionItem(func, ctx, new[] { accumulator, item });
        }
        return accumulator;
    }

    private static XdmValue ArrayFoldRight(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arr = args[0].ArrayValue;
        var items = new List<XdmValue>();
        foreach (var item in arr.Values)
            items.Add(item);
        var accumulator = args[1];
        var func = args[2];
        for (int i = items.Count - 1; i >= 0; i--)
        {
            accumulator = VmEngine.InvokeFunctionItem(func, ctx, new[] { items[i], accumulator });
        }
        return accumulator;
    }

    private static XdmValue ArrayForEach(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arr = args[0].ArrayValue;
        var func = args[1];
        var result = new List<XdmValue>();
        foreach (var item in arr.Values)
        {
            var r = VmEngine.InvokeFunctionItem(func, ctx, new[] { item });
            result.Add(r);
        }
        return XdmValue.FromArray(new XdmArray(result));
    }

    private static XdmValue ArrayForEachPair(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arr1 = args[0].ArrayValue;
        var arr2 = args[1].ArrayValue;
        var func = args[2];
        var result = new List<XdmValue>();
        var items1 = new List<XdmValue>();
        var items2 = new List<XdmValue>();
        foreach (var item in arr1.Values) items1.Add(item);
        foreach (var item in arr2.Values) items2.Add(item);
        int minLen = Math.Min(items1.Count, items2.Count);
        for (int i = 0; i < minLen; i++)
        {
            var r = VmEngine.InvokeFunctionItem(func, ctx, new[] { items1[i], items2[i] });
            result.Add(r);
        }
        return XdmValue.FromArray(new XdmArray(result));
    }

    private static XdmValue ArraySort_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ArraySort(ctx, args[0].ArrayValue, null, null);

    private static XdmValue ArraySort_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ArraySort(ctx, args[0].ArrayValue, args[1], null);

    private static XdmValue ArraySort_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ArraySort(ctx, args[0].ArrayValue, args[1], args[2]);

    private static XdmValue ArraySort(EvaluationContext ctx, XdmArray arr, XdmValue? collation, XdmValue? keyFunc)
    {
        var items = new List<XdmValue>();
        foreach (var item in arr.Values)
            items.Add(item);

        string? collationUri = collation is not null && !collation.Value.IsUndefined ? collation.ToString() : null;
        var keyed = new List<(XdmValue Key, XdmValue Item, int Index)>();
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var key = keyFunc is not null && !keyFunc.Value.IsUndefined
                ? Data(VmEngine.InvokeFunctionItem(keyFunc.Value, ctx, new[] { item }))
                : Data(item);
            keyed.Add((key, item, i));
        }
        keyed.Sort((a, b) =>
        {
            int cmp = CompareSortKeys(a.Key, b.Key, collationUri);
            return cmp != 0 ? cmp : a.Index.CompareTo(b.Index);
        });
        items = keyed.Select(k => k.Item).ToList();
        return XdmValue.FromArray(new XdmArray(items));
    }

    private static XdmValue ArrayInsertBefore(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arr = args[0].ArrayValue;
        int pos = (int)args[1].IntegerValue;
        var value = args[2];
        var items = new List<XdmValue>();
        int i = 1;
        foreach (var item in arr.Values)
        {
            if (i == pos)
                items.Add(value);
            items.Add(item);
            i++;
        }
        if (pos > arr.Count)
            items.Add(value);
        return XdmValue.FromArray(new XdmArray(items));
    }

    private static XdmValue ArrayFlatten(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var result = new List<XdmValue>();
        FlattenValue(args[0], result);
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static void FlattenValue(XdmValue value, List<XdmValue> result)
    {
        if (value.IsArray)
        {
            foreach (var item in value.ArrayValue.Values)
                FlattenValue(item, result);
        }
        else if (value.IsSequence)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                FlattenValue(item, result);
        }
        else if (!value.IsUndefined)
        {
            result.Add(value);
        }
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static string AtomizedString(XdmValue value)
    {
        if (value.IsUndefined)
            return string.Empty;

        if (value.IsNode)
            return value.NodeValue.StringValue;

        if (value.IsFunction || value.IsMap || value.IsArray)
            throw new InvalidOperationException("FOTY0013");

        if (value.IsSequence)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                return AtomizedString(item);
            return string.Empty;
        }

        return value.ToString();
    }

    /// <summary>
    /// Validates that the value is suitable for a string-typed function argument
    /// (xs:string?, xs:string, etc.) and returns its string value.
    /// Nodes are atomized; empty sequence becomes "".
    /// Non-string atomic types (integer, date, boolean, etc.) raise XPTY0004.
    /// </summary>
    private static string RequireString(XdmValue value)
    {
        if (value.IsUndefined)
            return string.Empty;

        if (value.IsNode)
            return value.NodeValue.StringValue;

        if (value.IsFunction || value.IsMap || value.IsArray)
            throw new InvalidOperationException("FOTY0013");

        if (value.IsSequence)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                return RequireString(item);
            return string.Empty;
        }

        if (value.Kind == XdmValueKind.String)
            return value.StringValue;

        if (string.Equals(value.SchemaTypeName, "untypedAtomic", StringComparison.OrdinalIgnoreCase))
            return value.ToString();

        throw new InvalidOperationException("XPTY0004");
    }

    private static XdmValue AtomizeValue(XdmValue value)
    {
        if (value.IsUndefined)
            return XdmValue.Undefined;

        if (value.IsNode)
        {
            // XDM §2.7.2: typed value of comments and PIs is xs:string;
            // for elements, attributes, text, and document nodes in the untyped
            // case it is xs:untypedAtomic.
            var node = value.NodeValue;
            if (node.NodeKind is XdmNodeKind.ProcessingInstruction or XdmNodeKind.Comment)
                return XdmValue.FromString(node.StringValue);
            return XdmValue.FromString(node.StringValue, "untypedAtomic");
        }

        if (value.IsSequence)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                return AtomizeValue(item);
            return XdmValue.Undefined;
        }

        return value;
    }

    private static XdmValue AtomizeMapKey(XdmValue value)
    {
        if (value.IsFunction || value.IsMap || value.IsArray)
            throw new InvalidOperationException("FOTY0013");
        return AtomizeValue(value);
    }

    private static List<XdmValue> Materialize(XdmValue value)
    {
        if (value.IsUndefined)
            return new List<XdmValue>();

        if (value.IsArray)
        {
            var list = new List<XdmValue>();
            var arr = value.ArrayValue;
            for (int i = 1; i <= arr.Count; i++)
            {
                var member = arr.Get(i);
                if (member.IsArray)
                    list.AddRange(Materialize(member));
                else if (member.IsSequence)
                    list.AddRange(Materialize(member));
                else
                    list.Add(member);
            }
            return list;
        }

        if (!value.IsSequence)
            return new List<XdmValue> { value };

        var seqList = new List<XdmValue>();
        foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
            seqList.Add(item);
        return seqList;
    }

    private static double ToDoubleValue(XdmValue value)
    {
        value = AtomizeValue(value);
        return value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => (double)value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => value.DoubleValue,
            XdmValueKind.Boolean => value.BooleanValue ? 1.0 : 0.0,
            _ => ParseXPathDouble(value.ToString())
        };
    }

    private static double ToDoubleValueStrict(XdmValue value)
    {
        value = AtomizeValue(value);
        return value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => (double)value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => value.DoubleValue,
            XdmValueKind.String when value.SchemaTypeName?.Equals("untypedAtomic", StringComparison.OrdinalIgnoreCase) == true =>
                double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
                    ? d
                    : throw new InvalidOperationException("FORG0001"),
            _ => throw new InvalidOperationException("XPTY0004")
        };
    }

    private static decimal ToDecimalValue(XdmValue value)
    {
        value = AtomizeValue(value);
        return value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => (decimal)value.DoubleValue,
            _ => decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0m
        };
    }

    private static long ToIntegerValue(XdmValue value)
    {
        value = AtomizeValue(value);
        return value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => (long)value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => (long)value.DoubleValue,
            _ => long.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0
        };
    }

    private static XdmValue Sum(List<XdmValue> items)
    {
        bool allIntegerOrDecimal = true;
        bool anyDouble = false;
        bool anyUntyped = false;
        bool allYearMonthDuration = true;
        bool allDayTimeDuration = true;
        foreach (var item in items)
        {
            var a = AtomizeValue(item);
            if (a.Kind == XdmValueKind.Double)
                anyDouble = true;
            if (a.Kind == XdmValueKind.String && double.TryParse(a.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                anyUntyped = true;
            if (a.Kind != XdmValueKind.Integer && a.Kind != XdmValueKind.Decimal)
                allIntegerOrDecimal = false;
            var str = a.Kind == XdmValueKind.Duration ? a.DurationValue
                  : a.Kind == XdmValueKind.String ? a.ToString()
                  : "";
            bool isYmd = IsYearMonthDurationString(str);
            bool isDtd = IsDayTimeDurationString(str);
            if (!isYmd) allYearMonthDuration = false;
            if (!isDtd) allDayTimeDuration = false;
        }

        if (allYearMonthDuration)
        {
            long totalMonths = 0;
            foreach (var item in items)
            {
                var a = AtomizeValue(item);
                var s = a.Kind == XdmValueKind.Duration ? a.DurationValue : a.ToString();
                var (years, months, _, _, _, _) = ParseDuration(s);
                totalMonths += years * 12 + months;
            }
            return XdmValue.FromDuration(FormatYearMonthDuration(totalMonths));
        }

        if (allDayTimeDuration)
        {
            decimal totalSeconds = 0m;
            foreach (var item in items)
            {
                var a = AtomizeValue(item);
                var s = a.Kind == XdmValueKind.Duration ? a.DurationValue : a.ToString();
                var (_, _, days, hours, minutes, seconds) = ParseDuration(s);
                totalSeconds += days * 86400m + hours * 3600m + minutes * 60m + seconds;
            }
            return XdmValue.FromDuration(FormatDayTimeDurationFromSeconds(totalSeconds));
        }

        if (allIntegerOrDecimal)
        {
            bool allInteger = items.All(item => AtomizeValue(item).Kind == XdmValueKind.Integer);
            if (allInteger)
            {
                long intSum = 0;
                foreach (var item in items)
                    intSum += ToIntegerValue(item);
                return XdmValue.FromInteger(intSum);
            }
            decimal decSum = 0m;
            foreach (var item in items)
                decSum += ToDecimalValue(item);
            return XdmValue.FromDecimal(decSum);
        }
        if (!anyDouble && !anyUntyped)
        {
            float sumF = 0.0f;
            foreach (var item in items)
                sumF += (float)ToDoubleValue(item);
            return XdmValue.FromFloat(sumF);
        }
        double sumD = 0.0;
        foreach (var item in items)
            sumD += ToDoubleValue(item);
        return XdmValue.FromDouble(sumD);
    }

    private static XdmValue MinMax(List<XdmValue> items, bool min, string collation)
    {
        var atomized = items.Select(AtomizeValue).ToList();

        // All date/time
        bool allDateTime = atomized.All(a => a.Kind is XdmValueKind.DateTime or XdmValueKind.Date or XdmValueKind.Time or XdmValueKind.Duration);
        if (allDateTime)
        {
            var first = atomized[0];
            var result = first;
            for (int i = 1; i < atomized.Count; i++)
            {
                var cmp = CompareDateTimeValues(atomized[i], result);
                if (min ? cmp < 0 : cmp > 0)
                    result = atomized[i];
            }
            return result;
        }

        // All string
        bool allString = atomized.All(a => a.Kind == XdmValueKind.String);
        if (allString)
        {
            // XPath spec: untypedAtomic (from node atomization) is cast to xs:double
            // for min/max. Explicit strings remain strings. Since our XDM doesn't
            // distinguish untypedAtomic from string, we check if any original item
            // was a node — if so, the strings came from atomization and are numeric.
            bool anyOriginalNode = items.Any(i => i.IsNode);
            if (anyOriginalNode)
            {
                double resultNum = ToDoubleValue(atomized[0]);
                for (int i = 1; i < atomized.Count; i++)
                {
                    double v = ToDoubleValue(atomized[i]);
                    if (min ? v < resultNum : v > resultNum)
                        resultNum = v;
                }
                return XdmValue.FromDouble(resultNum);
            }

            var result = atomized[0].StringValue;
            var resultVal = atomized[0];
            for (int i = 1; i < atomized.Count; i++)
            {
                var s = atomized[i].StringValue;
                int cmp = CompareStrings(s, result, collation);
                if (min ? cmp < 0 : cmp > 0)
                {
                    result = s;
                    resultVal = atomized[i];
                }
            }
            return resultVal;
        }

        bool allIntegerOrDecimal = true;
        bool anyDouble = false;
        foreach (var a in atomized)
        {
            if (a.Kind == XdmValueKind.Double)
                anyDouble = true;
            if (a.Kind != XdmValueKind.Integer && a.Kind != XdmValueKind.Decimal)
                allIntegerOrDecimal = false;
        }
        if (allIntegerOrDecimal)
        {
            decimal result = ToDecimalValue(items[0]);
            for (int i = 1; i < items.Count; i++)
            {
                decimal v = ToDecimalValue(items[i]);
                if (min ? v < result : v > result)
                    result = v;
            }
            return XdmValue.FromDecimal(result);
        }
        if (!anyDouble)
        {
            float resultF = (float)ToDoubleValue(items[0]);
            for (int i = 1; i < items.Count; i++)
            {
                float v = (float)ToDoubleValue(items[i]);
                if (min ? v < resultF : v > resultF)
                    resultF = v;
            }
            return XdmValue.FromFloat(resultF);
        }
        double resultD = ToDoubleValue(items[0]);
        for (int i = 1; i < items.Count; i++)
        {
            double v = ToDoubleValue(items[i]);
            if (min ? v < resultD : v > resultD)
                resultD = v;
        }
        return XdmValue.FromDouble(resultD);
    }

    private static int CompareDateTimeValues(XdmValue a, XdmValue b)
    {
        if (a.Kind is XdmValueKind.DateTime or XdmValueKind.Date or XdmValueKind.Time
            && b.Kind is XdmValueKind.DateTime or XdmValueKind.Date or XdmValueKind.Time)
        {
            return XdmValueComparer.Instance.Compare(a, b);
        }
        if (a.Kind == XdmValueKind.Duration && b.Kind == XdmValueKind.Duration)
            return XdmValueComparer.Instance.Compare(a, b);
        return string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------
    // Numeric functions
    // ------------------------------------------------------------------

    private static XdmValue Abs(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        XdmValue arg = AtomizeValue(args[0]);
        if (arg.IsUndefined)
            return XdmValue.Undefined;

        return arg.Kind switch
        {
            XdmValueKind.Integer => XdmValue.FromInteger(Math.Abs(arg.IntegerValue)),
            XdmValueKind.Decimal => XdmValue.FromDecimal(Math.Abs(arg.DecimalValue)),
            XdmValueKind.Double => XdmValue.FromDouble(Math.Abs(arg.DoubleValue)),
            XdmValueKind.Float => XdmValue.FromFloat(Math.Abs((float)arg.DoubleValue)),
            _ => XdmValue.FromDouble(Math.Abs(ConvertToDouble(arg)))
        };
    }

    private static XdmValue Floor(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        XdmValue arg = AtomizeValue(args[0]);
        if (arg.IsUndefined || IsEmptySequence(arg))
            return XdmValue.Undefined;

        return arg.Kind switch
        {
            XdmValueKind.Integer => arg,
            XdmValueKind.Decimal => XdmValue.FromDecimal(Math.Floor(arg.DecimalValue)),
            XdmValueKind.Double => XdmValue.FromDouble(Math.Floor(arg.DoubleValue)),
            XdmValueKind.Float => XdmValue.FromFloat((float)Math.Floor(arg.DoubleValue)),
            _ => XdmValue.FromDouble(Math.Floor(ConvertToDouble(arg)))
        };
    }

    private static XdmValue Ceiling(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        XdmValue arg = AtomizeValue(args[0]);
        if (arg.IsUndefined || IsEmptySequence(arg))
            return XdmValue.Undefined;

        return arg.Kind switch
        {
            XdmValueKind.Integer => arg,
            XdmValueKind.Decimal => XdmValue.FromDecimal(Math.Ceiling(arg.DecimalValue)),
            XdmValueKind.Double => XdmValue.FromDouble(Math.Ceiling(arg.DoubleValue)),
            XdmValueKind.Float => XdmValue.FromFloat((float)Math.Ceiling(arg.DoubleValue)),
            _ => XdmValue.FromDouble(Math.Ceiling(ConvertToDouble(arg)))
        };
    }

    /// <summary>
    /// Converts an XDM value to double for numeric functions that accept
    /// string/untyped-atomic arguments (e.g., floor, ceiling, round).
    /// </summary>
    private static double ConvertToDouble(XdmValue value)
    {
        return value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => (double)value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => value.DoubleValue,
            XdmValueKind.Boolean => value.BooleanValue ? 1.0 : 0.0,
            _ => ParseXPathDouble(value.ToString())
        };
    }

    /// <summary>
    /// Parses XPath lexical double forms including <c>INF</c>, <c>-INF</c>, and <c>NaN</c>.
    /// </summary>
    private static double ParseXPathDouble(string s)
    {
        if (s == "INF") return double.PositiveInfinity;
        if (s == "-INF") return double.NegativeInfinity;
        if (s == "NaN") return double.NaN;
        return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : double.NaN;
    }

    private static XdmValue Round_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => Round(ctx, args[0], 0);

    private static XdmValue Round_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => Round(ctx, args[0], args[1].IntegerValue);

    private static XdmValue Round(EvaluationContext ctx, XdmValue arg, long precision)
    {
        arg = AtomizeValue(arg);
        if (arg.IsUndefined || IsEmptySequence(arg))
            return XdmValue.Undefined;

        // For non-numeric types (string, untypedAtomic, etc.), convert to double first.
        bool isNumeric = arg.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float;
        if (!isNumeric)
        {
            double d = ConvertToDouble(arg);
            if (double.IsNaN(d)) return XdmValue.FromDouble(double.NaN);
            return XdmValue.FromDouble(RoundDouble(d, (int)precision));
        }

        if (precision >= 0)
        {
            int p = (int)precision;
            return arg.Kind switch
            {
                XdmValueKind.Integer => arg,
                XdmValueKind.Decimal =>
                    XdmValue.FromDecimal(RoundDecimal(arg.DecimalValue, p)),
                XdmValueKind.Double =>
                    XdmValue.FromDouble(RoundDouble(arg.DoubleValue, p)),
                XdmValueKind.Float =>
                    XdmValue.FromFloat((float)RoundDouble(arg.DoubleValue, p)),
                _ => throw new InvalidOperationException("XPTY0004")
            };
        }
        else
        {
            return arg.Kind switch
            {
                XdmValueKind.Integer =>
                    XdmValue.FromInteger((long)RoundDecimal((decimal)arg.IntegerValue, (int)precision)),
                XdmValueKind.Decimal =>
                    XdmValue.FromDecimal(RoundDecimal(arg.DecimalValue, (int)precision)),
                XdmValueKind.Double =>
                    XdmValue.FromDouble(RoundDouble(arg.DoubleValue, (int)precision)),
                XdmValueKind.Float =>
                    XdmValue.FromFloat((float)RoundDouble(arg.DoubleValue, (int)precision)),
                _ => throw new InvalidOperationException("XPTY0004")
            };
        }
    }

    private static double RoundDouble(double value, int precision)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            return value;

        // For values that fit in decimal, round via an exact decimal representation
        // so ties reflect the true double value (e.g. -13.65e0 rounds to -13.7).
        if (Math.Abs(value) is >= 1e-28 and <= (double)decimal.MaxValue)
        {
            try
            {
                var dec = DoubleToExactDecimal(value);
                return (double)RoundDecimal(dec, precision);
            }
            catch { /* fall back to double arithmetic */ }
        }

        // fn:round rounds to nearest; ties are rounded toward positive infinity.
        double factor = precision >= 0 ? Math.Pow(10.0, precision) : Math.Pow(10.0, -precision);
        double scaled = precision >= 0 ? value * factor : value / factor;
        if (double.IsInfinity(scaled))
            return value;

        double floor = Math.Floor(scaled);
        double ceil = Math.Ceiling(scaled);
        double diffFloor = scaled - floor;
        double diffCeil = ceil - scaled;
        if (diffFloor < diffCeil)
            return precision >= 0 ? floor / factor : floor * factor;
        if (diffCeil < diffFloor)
            return precision >= 0 ? ceil / factor : ceil * factor;
        // Exactly halfway: round toward positive infinity.
        return precision >= 0 ? ceil / factor : ceil * factor;
    }

    private static decimal RoundDecimal(decimal value, int precision)
    {
        if (precision >= 0)
        {
            decimal factor = (decimal)Math.Pow(10.0, precision);
            if (factor == 0m || Math.Abs(value) > decimal.MaxValue / factor)
                return value;
            decimal scaled = value * factor;
            decimal floor = Math.Floor(scaled);
            decimal ceil = Math.Ceiling(scaled);
            decimal diffFloor = scaled - floor;
            decimal diffCeil = ceil - scaled;
            if (diffFloor < diffCeil) return floor / factor;
            if (diffCeil < diffFloor) return ceil / factor;
            return ceil / factor;
        }
        else
        {
            decimal factor = (decimal)Math.Pow(10.0, -precision);
            if (factor == 0m)
                return value;
            decimal scaled = value / factor;
            decimal floor = Math.Floor(scaled);
            decimal ceil = Math.Ceiling(scaled);
            decimal diffFloor = scaled - floor;
            decimal diffCeil = ceil - scaled;
            if (diffFloor < diffCeil) return floor * factor;
            if (diffCeil < diffFloor) return ceil * factor;
            return ceil * factor;
        }
    }

    /// <summary>
    /// Converts a double to a decimal using enough digits to preserve rounding decisions.
    /// Values smaller than the decimal range underflow to zero; values larger overflow and throw.
    /// </summary>
    private static decimal DoubleToExactDecimal(double value)
    {
        var s = value.ToString("G30", CultureInfo.InvariantCulture);
        if (decimal.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
            return d;
        return (decimal)value;
    }

    private static XdmValue RoundHalfToEven_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => RoundHalfToEven(ctx, args[0], 0);

    private static XdmValue RoundHalfToEven_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => RoundHalfToEven(ctx, args[0], args[1].IntegerValue);

    private static XdmValue RoundHalfToEven(EvaluationContext ctx, XdmValue arg, long precision)
    {
        arg = AtomizeValue(arg);
        if (arg.IsUndefined || IsEmptySequence(arg))
            return XdmValue.Undefined;

        if (precision >= 0)
        {
            return arg.Kind switch
            {
                XdmValueKind.Integer => arg,
                XdmValueKind.Decimal =>
                    XdmValue.FromDecimal(RoundHalfToEvenDecimal(arg.DecimalValue, (int)precision)),
                XdmValueKind.Double =>
                    XdmValue.FromDouble(RoundHalfToEvenDouble(arg.DoubleValue, (int)precision)),
                XdmValueKind.Float =>
                    XdmValue.FromFloat((float)RoundHalfToEvenDouble(arg.DoubleValue, (int)precision)),
                _ => throw new InvalidOperationException("XPTY0004")
            };
        }
        else
        {
            return arg.Kind switch
            {
                XdmValueKind.Integer =>
                    XdmValue.FromInteger((long)RoundHalfToEvenDecimal((decimal)arg.IntegerValue, (int)precision)),
                XdmValueKind.Decimal =>
                    XdmValue.FromDecimal(RoundHalfToEvenDecimal(arg.DecimalValue, (int)precision)),
                XdmValueKind.Double =>
                    XdmValue.FromDouble(RoundHalfToEvenDouble(arg.DoubleValue, (int)precision)),
                XdmValueKind.Float =>
                    XdmValue.FromFloat((float)RoundHalfToEvenDouble(arg.DoubleValue, (int)precision)),
                _ => throw new InvalidOperationException("XPTY0004")
            };
        }
    }

    private static double RoundHalfToEvenDouble(double value, int precision)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            return value;

        // Use the exact decimal representation of the double so rounding reflects the
        // true double value (e.g. 150.015e0 -> 150.01, 250.025e0 -> 250.03).
        if (Math.Abs(value) is >= 1e-28 and <= (double)decimal.MaxValue)
        {
            try
            {
                var dec = DoubleToExactDecimal(value);
                return (double)RoundHalfToEvenDecimal(dec, precision);
            }
            catch { /* fall back to double arithmetic */ }
        }

        if (precision >= 0)
        {
            double factor = Math.Pow(10.0, precision);
            return Math.Round(value * factor, MidpointRounding.ToEven) / factor;
        }
        else
        {
            double factor = Math.Pow(10.0, -precision);
            return Math.Round(value / factor, MidpointRounding.ToEven) * factor;
        }
    }

    private static decimal RoundHalfToEvenDecimal(decimal value, int precision)
    {
        if (precision >= 0)
        {
            decimal factor = (decimal)Math.Pow(10.0, precision);
            if (factor == 0m || Math.Abs(value) > decimal.MaxValue / factor)
                return value;
            return Math.Round(value * factor, MidpointRounding.ToEven) / factor;
        }
        else
        {
            decimal factor = (decimal)Math.Pow(10.0, -precision);
            if (factor == 0m)
                return value;
            return Math.Round(value / factor, MidpointRounding.ToEven) * factor;
        }
    }

    // ------------------------------------------------------------------
    // Node-name accessors
    // ------------------------------------------------------------------

    private static IXdmNode? GetNodeFromValue(XdmValue value)
    {
        if (value.IsUndefined)
            return null;
        if (value.IsNode)
            return value.NodeValue;
        if (value.IsSequence && value.SequenceValue is not null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                if (item.IsNode)
                    return item.NodeValue;
                break; // first item only
            }
        }
        return null;
    }

    private static XdmValue LocalName_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var node = ctx.ContextItem.IsNode ? ctx.ContextItem.NodeValue : null;
        return XdmValue.FromString(node?.LocalName ?? string.Empty);
    }

    private static XdmValue LocalName_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var node = GetNodeFromValue(args[0]);
        return XdmValue.FromString(node?.LocalName ?? string.Empty);
    }

    private static XdmValue NamespaceUri_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var node = ctx.ContextItem.IsNode ? ctx.ContextItem.NodeValue : null;
        return XdmValue.FromString(node?.NamespaceUri ?? string.Empty);
    }

    private static XdmValue NamespaceUri_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var node = GetNodeFromValue(args[0]);
        return XdmValue.FromString(node?.NamespaceUri ?? string.Empty);
    }

    private static XdmValue Name_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var node = ctx.ContextItem.IsNode ? ctx.ContextItem.NodeValue : null;
        return XdmValue.FromString(GetQualifiedName(node));
    }

    private static XdmValue Name_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var node = GetNodeFromValue(args[0]);
        return XdmValue.FromString(GetQualifiedName(node));
    }

    private static string GetQualifiedName(IXdmNode? node)
    {
        if (node is null)
            return string.Empty;
        string prefix = node.Prefix;
        string local = node.LocalName;
        return string.IsNullOrEmpty(prefix) ? local : prefix + ":" + local;
    }

    private static XdmValue Lang_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var node = ctx.ContextItem.IsNode ? ctx.ContextItem.NodeValue : null;
        return Lang(AtomizedString(args[0]), node);
    }

    private static XdmValue Lang_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var node = GetNodeFromValue(args[1]);
        return Lang(AtomizedString(args[0]), node);
    }

    private static XdmValue Lang(string testLang, IXdmNode? node)
    {
        if (string.IsNullOrEmpty(testLang) || node is null)
            return XdmValue.False;
        var current = node;
        while (current is not null)
        {
            string? langAttr = null;
            foreach (var attr in current.Attributes("lang", "http://www.w3.org/XML/1998/namespace"))
            {
                langAttr = attr.ToString();
                break;
            }
            if (langAttr is not null)
            {
                bool matches = LangMatches(testLang, langAttr);
                return XdmValue.FromBoolean(matches);
            }
            current = current.Parent;
        }
        return XdmValue.False;
    }

    private static bool LangMatches(string testLang, string nodeLang)
    {
        // Case-insensitive prefix match: "en" matches "en", "en-US", "EN-us"
        var test = testLang.ToLowerInvariant();
        var node = nodeLang.ToLowerInvariant();
        if (test == node) return true;
        if (node.StartsWith(test + "-", StringComparison.Ordinal)) return true;
        return false;
    }

    // ------------------------------------------------------------------
    // Date / Time functions
    // ------------------------------------------------------------------

    private static XdmValue ParseIetfDate(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (args[0].IsUndefined || IsEmptySequence(args[0]))
            return XdmValue.Undefined;

        string input = AtomizedString(args[0]);
        if (string.IsNullOrEmpty(input))
            throw new InvalidOperationException("FORG0010: Invalid IETF date");

        if (TryParseIetfDateCore(input.Trim(), out var result))
            return XdmValue.FromDateTime(result);

        throw new InvalidOperationException("FORG0010: Invalid IETF date");
    }

    private static bool TryParseIetfDateCore(string input, out DateTimeOffset result)
    {
        result = default;
        if (input.Length == 0) return false;

        // Reject ISO 8601 format (starts with yyyy-MM-ddT)
        if (input.Length >= 10 && input[4] == '-' && input[7] == '-' && input[10] == 'T')
            return false;

        int pos = 0;

        // Optional day name: Mon, Monday, Tue, Tuesday, etc.
        var dayNameMatch = Regex.Match(input, @"^(?:Mon(?:day)?|Tue(?:sday)?|Wed(?:nesday)?|Thu(?:rsday)?|Fri(?:day)?|Sat(?:urday)?|Sun(?:day)?)(?:\s+|,\s+)", RegexOptions.IgnoreCase);
        if (dayNameMatch.Success)
        {
            pos = dayNameMatch.Length;
        }

        string rest = input.Substring(pos);

        // Find time pattern: H+:MM with optional :SS and .fraction
        var timeMatch = Regex.Match(rest, @"(\d{1,2}):(\d{2})(?::(\d{2})(?:\.(\d+))?)?");
        if (!timeMatch.Success) return false;

        string beforeTime = rest.Substring(0, timeMatch.Index).TrimEnd();
        string afterTime = rest.Substring(timeMatch.Index + timeMatch.Length).TrimStart();

        int hour = int.Parse(timeMatch.Groups[1].Value, CultureInfo.InvariantCulture);
        int minute = int.Parse(timeMatch.Groups[2].Value, CultureInfo.InvariantCulture);
        int second = timeMatch.Groups[3].Success ? int.Parse(timeMatch.Groups[3].Value, CultureInfo.InvariantCulture) : 0;
        int ms = 0;
        if (timeMatch.Groups[4].Success)
        {
            string frac = timeMatch.Groups[4].Value;
            if (frac.Length > 3) frac = frac.Substring(0, 3);
            else if (frac.Length < 3) frac = frac.PadRight(3, '0');
            ms = int.Parse(frac, CultureInfo.InvariantCulture);
        }

        // Parse date from beforeTime
        int day = 0, month = 0, year = 0;
        bool needYearFromAfter = false;

        if (!TryParseDatePart(beforeTime, out day, out month, out year, out needYearFromAfter))
            return false;

        // Parse timezone and year from afterTime
        TimeSpan offset = TimeSpan.Zero;
        bool hasTz = false;

        // Timezone name in parentheses must come immediately after offset
        if (!string.IsNullOrEmpty(afterTime))
        {
            if (TryParseTimezone(ref afterTime, out offset, out string? parenName))
            {
                hasTz = true;
                if (parenName is not null && !IsValidTzName(parenName))
                    return false;
            }
            else if (afterTime.TrimStart().StartsWith("("))
            {
                // Parenthesized name without preceding offset is an error
                return false;
            }
        }

        // Extract year from remaining afterTime if needed
        if (!string.IsNullOrWhiteSpace(afterTime))
        {
            var yearMatch = Regex.Match(afterTime, @"^\s*(\d{2,4})\s*$");
            if (yearMatch.Success)
            {
                string yStr = yearMatch.Groups[1].Value;
                if (yStr.Length == 1 || yStr.Length == 3)
                    return false; // year must be 2 or 4 digits
                int y = int.Parse(yStr, CultureInfo.InvariantCulture);
                if (needYearFromAfter)
                {
                    year = y;
                    needYearFromAfter = false;
                }
                else if (y != year)
                {
                    return false; // conflicting year
                }
            }
            else
            {
                return false; // unexpected trailing content
            }
        }

        if (needYearFromAfter) return false;

        // Two-digit year → 19xx
        if (year < 100) year += 1900;

        // Handle 24:00
        if (hour == 24 && minute == 0 && second == 0 && ms == 0)
        {
            hour = 0;
            try
            {
                var dt24 = new DateTime(year, month, day, 0, 0, 0);
                dt24 = dt24.AddDays(1);
                year = dt24.Year;
                month = dt24.Month;
                day = dt24.Day;
            }
            catch { return false; }
        }

        try
        {
            var dt = new DateTime(year, month, day, hour, minute, second, ms);
            result = new DateTimeOffset(dt, hasTz ? offset : TimeSpan.Zero);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryParseDatePart(string dateStr, out int day, out int month, out int year, out bool needYearFromAfter)
    {
        day = month = year = 0;
        needYearFromAfter = false;
        if (string.IsNullOrWhiteSpace(dateStr)) return false;

        var tokens = Regex.Split(dateStr, @"[\s-]+").Where(s => !string.IsNullOrEmpty(s)).ToList();
        if (tokens.Count == 0) return false;

        // Handle 3-token date: dd MMM yyyy, MMM dd yyyy, dd MMM yy, MMM dd yy, etc.
        if (tokens.Count == 3)
        {
            if (int.TryParse(tokens[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int d)
                && TryParseMonth(tokens[1], out int m)
                && int.TryParse(tokens[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int y))
            {
                if (tokens[0].Length > 2) return false;
                if (tokens[2].Length == 1 || tokens[2].Length == 3) return false;
                day = d; month = m; year = y; return true;
            }
            if (TryParseMonth(tokens[0], out m)
                && int.TryParse(tokens[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out d)
                && int.TryParse(tokens[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out y))
            {
                if (tokens[1].Length > 2) return false;
                if (tokens[2].Length == 1 || tokens[2].Length == 3) return false;
                day = d; month = m; year = y; return true;
            }
            return false;
        }

        // Handle 2-token date: dd MMM or MMM dd (year comes after time)
        if (tokens.Count == 2)
        {
            if (int.TryParse(tokens[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int d)
                && TryParseMonth(tokens[1], out int m))
            {
                day = d; month = m; needYearFromAfter = true; return true;
            }
            if (TryParseMonth(tokens[0], out m)
                && int.TryParse(tokens[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out d))
            {
                day = d; month = m; needYearFromAfter = true; return true;
            }
            return false;
        }

        return false;
    }

    private static bool TryParseMonth(string token, out int month)
    {
        month = token switch
        {
            _ when token.Equals("Jan", StringComparison.OrdinalIgnoreCase) => 1,
            _ when token.Equals("Feb", StringComparison.OrdinalIgnoreCase) => 2,
            _ when token.Equals("Mar", StringComparison.OrdinalIgnoreCase) => 3,
            _ when token.Equals("Apr", StringComparison.OrdinalIgnoreCase) => 4,
            _ when token.Equals("May", StringComparison.OrdinalIgnoreCase) => 5,
            _ when token.Equals("Jun", StringComparison.OrdinalIgnoreCase) => 6,
            _ when token.Equals("Jul", StringComparison.OrdinalIgnoreCase) => 7,
            _ when token.Equals("Aug", StringComparison.OrdinalIgnoreCase) => 8,
            _ when token.Equals("Sep", StringComparison.OrdinalIgnoreCase) => 9,
            _ when token.Equals("Oct", StringComparison.OrdinalIgnoreCase) => 10,
            _ when token.Equals("Nov", StringComparison.OrdinalIgnoreCase) => 11,
            _ when token.Equals("Dec", StringComparison.OrdinalIgnoreCase) => 12,
            _ => 0,
        };
        return month != 0;
    }

    private static bool TryParseTimezone(ref string str, out TimeSpan offset, out string? parenName)
    {
        offset = TimeSpan.Zero;
        parenName = null;
        str = str.TrimStart();
        if (string.IsNullOrEmpty(str)) return false;

        // Named timezone: must not be followed by a word character
        var namedMatch = Regex.Match(str, @"^(UT|UTC|GMT|EST|EDT|CST|CDT|MST|MDT|PST|PDT)(?!\w)", RegexOptions.IgnoreCase);
        if (namedMatch.Success)
        {
            string name = namedMatch.Groups[1].Value.ToUpperInvariant();
            offset = name switch
            {
                "UT" or "UTC" or "GMT" => TimeSpan.Zero,
                "EST" => TimeSpan.FromHours(-5),
                "EDT" => TimeSpan.FromHours(-4),
                "CST" => TimeSpan.FromHours(-6),
                "CDT" => TimeSpan.FromHours(-5),
                "MST" => TimeSpan.FromHours(-7),
                "MDT" => TimeSpan.FromHours(-6),
                "PST" => TimeSpan.FromHours(-8),
                "PDT" => TimeSpan.FromHours(-7),
                _ => TimeSpan.Zero,
            };
            str = str.Substring(namedMatch.Length);
        }
        else if (TryParseOffsetWithColon(str, out offset, out int colonLen))
        {
            str = str.Substring(colonLen);
        }
        else if (TryParseOffsetNoColon(str, out offset, out int noColonLen))
        {
            str = str.Substring(noColonLen);
        }
        else
        {
            return false;
        }

        // Check for optional timezone name in parentheses after offset
        str = str.TrimStart();
        if (str.StartsWith("("))
        {
            int closeIdx = str.IndexOf(')');
            if (closeIdx < 0) { offset = TimeSpan.Zero; return false; }
            parenName = str.Substring(1, closeIdx - 1).Trim();
            if (string.IsNullOrEmpty(parenName)) { offset = TimeSpan.Zero; return false; }
            str = str.Substring(closeIdx + 1);
        }

        return true;
    }

    private static bool TryParseOffsetWithColon(string str, out TimeSpan offset, out int length)
    {
        offset = TimeSpan.Zero;
        length = 0;
        var match = Regex.Match(str, @"^([+-]\d{1,2}:\d{0,2})(?!\d)");
        if (!match.Success) return false;

        string tz = match.Groups[1].Value;
        var parts = tz.Split(':');
        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int h))
            return false;

        int m = 0;
        if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
        {
            if (parts[1].Length != 2) return false; // minutes must be 2 digits
            if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out m))
                return false;
        }

        if (Math.Abs(h) > 14 || Math.Abs(m) >= 60) return false;
        offset = new TimeSpan(h, m, 0);
        length = match.Length;
        return true;
    }

    private static bool TryParseOffsetNoColon(string str, out TimeSpan offset, out int length)
    {
        offset = TimeSpan.Zero;
        length = 0;
        var match = Regex.Match(str, @"^([+-]\d{1,4})\b");
        if (!match.Success) return false;

        string tz = match.Groups[1].Value;
        int sign = tz[0] == '-' ? -1 : 1;
        string num = tz.Substring(1);
        int h, m;

        switch (num.Length)
        {
            case 1: h = num[0] - '0'; m = 0; break;
            case 2: h = int.Parse(num, CultureInfo.InvariantCulture); m = 0; break;
            case 3: h = num[0] - '0'; m = int.Parse(num.Substring(1), CultureInfo.InvariantCulture); break;
            case 4: h = int.Parse(num.Substring(0, 2), CultureInfo.InvariantCulture); m = int.Parse(num.Substring(2), CultureInfo.InvariantCulture); break;
            default: return false;
        }

        h = sign * h;
        m = sign * m;
        if (Math.Abs(h) > 14 || Math.Abs(m) >= 60) return false;
        offset = new TimeSpan(h, m, 0);
        length = match.Length;
        return true;
    }

    private static bool IsValidTzName(string name)
    {
        return name.Equals("UT", StringComparison.OrdinalIgnoreCase)
            || name.Equals("UTC", StringComparison.OrdinalIgnoreCase)
            || name.Equals("GMT", StringComparison.OrdinalIgnoreCase)
            || name.Equals("EST", StringComparison.OrdinalIgnoreCase)
            || name.Equals("EDT", StringComparison.OrdinalIgnoreCase)
            || name.Equals("CST", StringComparison.OrdinalIgnoreCase)
            || name.Equals("CDT", StringComparison.OrdinalIgnoreCase)
            || name.Equals("MST", StringComparison.OrdinalIgnoreCase)
            || name.Equals("MDT", StringComparison.OrdinalIgnoreCase)
            || name.Equals("PST", StringComparison.OrdinalIgnoreCase)
            || name.Equals("PDT", StringComparison.OrdinalIgnoreCase);
    }

    private static XdmValue FormatInteger_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => FormatInteger(ctx, args[0], AtomizedString(args[1]), null);

    private static XdmValue FormatInteger_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => FormatInteger(ctx, args[0], AtomizedString(args[1]), AtomizedString(args[2]));

    private static XdmValue FormatNumber_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        return FormatNumber(ctx, args[0], RequireString(args[1]), null);
    }

    private static XdmValue FormatNumber_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        return FormatNumber(ctx, args[0], RequireString(args[1]), RequireString(args[2]));
    }

    private static XdmValue FormatNumber(EvaluationContext ctx, XdmValue value, string picture, string? formatName)
    {
        value = AtomizeValue(value);

        var format = string.IsNullOrEmpty(formatName)
            ? ctx.DefaultDecimalFormat
            : ResolveDecimalFormat(ctx, formatName);

        if (format == null)
            throw new InvalidOperationException("FODF1280");

        string result = FormatNumberEngine.Format(value, picture, format);
        return XdmValue.FromString(result);
    }

    private static XdmValue FormatDate_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => FormatDateTime(args[0], AtomizedString(args[1]), null, null, null, DateTimeComponents.Date);

    private static XdmValue FormatDate_5(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => FormatDateTime(args[0], AtomizedString(args[1]), AtomizedString(args[2]), AtomizedString(args[3]), AtomizedString(args[4]), DateTimeComponents.Date);

    private static XdmValue FormatTime_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => FormatDateTime(args[0], AtomizedString(args[1]), null, null, null, DateTimeComponents.Time);

    private static XdmValue FormatTime_5(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => FormatDateTime(args[0], AtomizedString(args[1]), AtomizedString(args[2]), AtomizedString(args[3]), AtomizedString(args[4]), DateTimeComponents.Time);

    private static XdmValue FormatDateTime_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => FormatDateTime(args[0], AtomizedString(args[1]), null, null, null, DateTimeComponents.DateTime);

    private static XdmValue FormatDateTime_5(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => FormatDateTime(args[0], AtomizedString(args[1]), AtomizedString(args[2]), AtomizedString(args[3]), AtomizedString(args[4]), DateTimeComponents.DateTime);

    private static XdmValue FormatDateTime(XdmValue value, string picture, string? language, string? calendar, string? place, DateTimeComponents components)
    {
        if (value.IsUndefined)
            return XdmValue.FromString(string.Empty);

        XPathDateTime xdt = value.Kind switch
        {
            XdmValueKind.DateTime => value.DateTimeXPathValue,
            XdmValueKind.Date => value.DateXPathValue,
            XdmValueKind.Time => value.TimeXPathValue,
            _ => throw new InvalidOperationException("XPTY0004")
        };

        string result = FormatDateTimeEngine.Format(xdt, picture, language, calendar, place, components);
        return XdmValue.FromString(result);
    }

    private static DecimalFormat? ResolveDecimalFormat(EvaluationContext ctx, string name)
    {
        name = name.Trim();

        // EQName syntax
        if (name.StartsWith("Q{"))
        {
            int end = name.IndexOf('}');
            if (end > 2)
            {
                string ns = name.Substring(2, end - 2);
                string local = name.Substring(end + 1);
                return ctx.GetDecimalFormat(local) ?? ctx.GetDecimalFormat(local, ns);
            }
        }

        return ctx.GetDecimalFormat(name);
    }

    private static XdmValue FormatInteger(EvaluationContext ctx, XdmValue value, string picture, string? language)
    {
        // Handle empty sequence and undefined
        if (value.IsUndefined)
            return XdmValue.FromString("");
        if (value.IsSequence && value.SequenceValue is not null && value.SequenceValue.TryGetLength(out var len) && len == 0)
            return XdmValue.FromString("");

        long n = ToIntegerValue(value);
        string result = FormatIntegerEngine.Format(ctx, n, picture, language);
        return XdmValue.FromString(result);
    }

    private static string ToAlphabetic(long n, bool upper)
    {
        if (n <= 0) return "";
        var sb = new StringBuilder();
        while (n > 0)
        {
            n--;
            char c = upper ? (char)('A' + (n % 26)) : (char)('a' + (n % 26));
            sb.Insert(0, c);
            n /= 26;
        }
        return sb.ToString();
    }

    private static string ToRoman(long n, bool upper)
    {
        if (n <= 0 || n > 3999) return "";
        var values = new[] { (1000, "M"), (900, "CM"), (500, "D"), (400, "CD"), (100, "C"), (90, "XC"), (50, "L"), (40, "XL"), (10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I") };
        var sb = new StringBuilder();
        foreach (var (val, sym) in values)
        {
            while (n >= val)
            {
                sb.Append(sym);
                n -= val;
            }
        }
        return upper ? sb.ToString() : sb.ToString().ToLowerInvariant();
    }

    private static string ToWords(long n, bool upper)
    {
        string s = NumberToWords(n);
        return upper ? s.ToUpperInvariant() : s.ToLowerInvariant();
    }

    private static string ToWordsTitle(long n)
    {
        string s = NumberToWords(n);
        return System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.ToLowerInvariant());
    }

    private static string NumberToWords(long n)
    {
        if (n == 0) return "zero";
        if (n < 0) return "minus " + NumberToWords(-n);
        if (n <= 19) return new[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" }[n - 1];
        if (n < 100)
        {
            var tens = new[] { "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };
            string r = tens[n / 10 - 2];
            if (n % 10 > 0) r += "-" + NumberToWords(n % 10);
            return r;
        }
        if (n < 1000)
        {
            string r = NumberToWords(n / 100) + " hundred";
            if (n % 100 > 0) r += " and " + NumberToWords(n % 100);
            return r;
        }
        if (n < 1000000)
        {
            string r = NumberToWords(n / 1000) + " thousand";
            if (n % 1000 > 0) r += " " + NumberToWords(n % 1000);
            return r;
        }
        if (n < 1000000000)
        {
            string r = NumberToWords(n / 1000000) + " million";
            if (n % 1000000 > 0) r += " " + NumberToWords(n % 1000000);
            return r;
        }
        string rr = NumberToWords(n / 1000000000) + " billion";
        if (n % 1000000000 > 0) rr += " " + NumberToWords(n % 1000000000);
        return rr;
    }

    private static XdmValue DateTime_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        // Atomize the arguments: nodes/sequences are passed through AtomizeValue.
        var dateInput = AtomizeValue(args[0]);
        var timeInput = AtomizeValue(args[1]);
        if (dateInput.IsUndefined || timeInput.IsUndefined)
            return XdmValue.Undefined;

        // Cast the arguments to xs:date and xs:time. This handles nodes and untypedAtomic
        // values (e.g. attributes/elements passed to fn:dateTime in xsl:merge-key).
        var dateArg = VmEngine.Cast(dateInput, "date");
        var timeArg = VmEngine.Cast(timeInput, "time");
        if (dateArg.IsUndefined || timeArg.IsUndefined)
            return XdmValue.Undefined;

        var date = dateArg.DateXPathValue;
        var time = timeArg.TimeXPathValue;
        bool dateHasTz = date.HasTimezone;
        bool timeHasTz = time.HasTimezone;

        TimeSpan offset;
        bool hasTimezone;

        if (dateHasTz && timeHasTz)
        {
            if (date.TimezoneOffsetMinutes != time.TimezoneOffsetMinutes)
                throw new InvalidOperationException("FORG0008");
            offset = TimeSpan.FromMinutes(date.TimezoneOffsetMinutes);
            hasTimezone = true;
        }
        else if (dateHasTz)
        {
            offset = TimeSpan.FromMinutes(date.TimezoneOffsetMinutes);
            hasTimezone = true;
        }
        else if (timeHasTz)
        {
            offset = TimeSpan.FromMinutes(time.TimezoneOffsetMinutes);
            hasTimezone = true;
        }
        else
        {
            offset = TimeSpan.Zero;
            hasTimezone = false;
        }

        int offsetMinutes = (int)offset.TotalMinutes;
        var combined = new XPathDateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Millisecond, offsetMinutes, hasTimezone);
        return XdmValue.FromDateTime(combined, hasTimezone);
    }

    private static XdmValue CurrentDateTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromDateTime(ctx.CurrentDateTimeSnapshot, hasTimezone: true);

    private static XdmValue CurrentDate(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var now = ctx.CurrentDateTimeSnapshot;
        return XdmValue.FromDate(new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset), hasTimezone: true);
    }

    private static XdmValue CurrentTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var now = ctx.CurrentDateTimeSnapshot;
        return XdmValue.FromTime(new DateTimeOffset(1, 1, 1, now.Hour, now.Minute, now.Second, now.Offset), hasTimezone: true);
    }

    private static XdmValue AdjustDateToTimezone_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (arg.IsUndefined || IsEmptySequence(arg))
            return XdmValue.Undefined;
        var xdt = arg.DateXPathValue;
        bool hasTz = arg.HasTimezone;
        return DoAdjustDateToTimezone(xdt, hasTz, ctx.ImplicitTimezoneOffsetMinutes, removeTimezone: false);
    }

    private static XdmValue AdjustDateToTimezone_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        var tzArg = args[1];
        if (arg.IsUndefined || IsEmptySequence(arg))
            return XdmValue.Undefined;

        var xdt = arg.DateXPathValue;
        bool hasTz = arg.HasTimezone;

        if (tzArg.IsUndefined || IsEmptySequence(tzArg))
            return DoAdjustDateToTimezone(xdt, hasTz, 0, removeTimezone: true);

        int targetOffset = (int)XmlConvert.ToTimeSpan(AtomizedString(tzArg)).TotalMinutes;
        return DoAdjustDateToTimezone(xdt, hasTz, targetOffset, removeTimezone: false);
    }

    private static XdmValue DoAdjustDateToTimezone(XPathDateTime xdt, bool hasTz, int targetOffset, bool removeTimezone)
    {
        if (removeTimezone)
        {
            return XdmValue.FromDate(new XPathDateTime(xdt.Year, xdt.Month, xdt.Day, 0, 0, 0, 0, 0, false), false);
        }

        if (!hasTz)
        {
            return XdmValue.FromDate(new XPathDateTime(xdt.Year, xdt.Month, xdt.Day, 0, 0, 0, 0, targetOffset, true), true);
        }

        var normalized = XPathDateTimeHelper.NormalizeToUtc(xdt);
        var withTarget = new XPathDateTime(normalized.Year, normalized.Month, normalized.Day, normalized.Hour, normalized.Minute, normalized.Second, normalized.Millisecond, -targetOffset, true);
        var targetLocal = XPathDateTimeHelper.NormalizeToUtc(withTarget);
        return XdmValue.FromDate(new XPathDateTime(targetLocal.Year, targetLocal.Month, targetLocal.Day, 0, 0, 0, 0, targetOffset, true), true);
    }

    private static XdmValue AdjustTimeToTimezone_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (arg.IsUndefined || IsEmptySequence(arg))
            return XdmValue.Undefined;
        var xdt = arg.TimeXPathValue;
        bool hasTz = arg.HasTimezone;
        return DoAdjustTimeToTimezone(xdt, hasTz, ctx.ImplicitTimezoneOffsetMinutes, removeTimezone: false);
    }

    private static XdmValue AdjustTimeToTimezone_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        var tzArg = args[1];
        if (arg.IsUndefined || IsEmptySequence(arg))
            return XdmValue.Undefined;

        var xdt = arg.TimeXPathValue;
        bool hasTz = arg.HasTimezone;

        if (tzArg.IsUndefined || IsEmptySequence(tzArg))
            return DoAdjustTimeToTimezone(xdt, hasTz, 0, removeTimezone: true);

        int targetOffset = (int)XmlConvert.ToTimeSpan(AtomizedString(tzArg)).TotalMinutes;
        return DoAdjustTimeToTimezone(xdt, hasTz, targetOffset, removeTimezone: false);
    }

    private static XdmValue DoAdjustTimeToTimezone(XPathDateTime xdt, bool hasTz, int targetOffset, bool removeTimezone)
    {
        if (removeTimezone)
        {
            return XdmValue.FromTime(new XPathDateTime(1, 1, 1, xdt.Hour, xdt.Minute, xdt.Second, xdt.Millisecond, 0, false), false);
        }

        if (!hasTz)
        {
            // Add a timezone while preserving the local time-of-day.
            return XdmValue.FromTime(new XPathDateTime(1, 1, 1, xdt.Hour, xdt.Minute, xdt.Second, xdt.Millisecond, targetOffset, true), true);
        }

        // Translate an existing timezone to the target timezone, preserving the instant.
        // Use a safe reference date to avoid DateTimeOffset range issues near year 0.
        var effective = new XPathDateTime(2000, 1, 1, xdt.Hour, xdt.Minute, xdt.Second, xdt.Millisecond, xdt.TimezoneOffsetMinutes, true);
        var utc = XPathDateTimeHelper.NormalizeToUtc(effective);
        var withTarget = new XPathDateTime(utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute, utc.Second, utc.Millisecond, -targetOffset, true);
        var targetLocal = XPathDateTimeHelper.NormalizeToUtc(withTarget);
        return XdmValue.FromTime(new XPathDateTime(1, 1, 1, targetLocal.Hour, targetLocal.Minute, targetLocal.Second, targetLocal.Millisecond, targetOffset, true), true);
    }

    private static XdmValue AdjustDateTimeToTimezone_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (arg.IsUndefined || IsEmptySequence(arg))
            return XdmValue.Undefined;
        var xdt = arg.DateTimeXPathValue;
        bool hasTz = arg.HasTimezone;
        return DoAdjustDateTimeToTimezone(xdt, hasTz, ctx.ImplicitTimezoneOffsetMinutes, removeTimezone: false);
    }

    private static XdmValue AdjustDateTimeToTimezone_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        var tzArg = args[1];
        if (arg.IsUndefined || IsEmptySequence(arg))
            return XdmValue.Undefined;

        var xdt = arg.DateTimeXPathValue;
        bool hasTz = arg.HasTimezone;

        if (tzArg.IsUndefined || IsEmptySequence(tzArg))
            return DoAdjustDateTimeToTimezone(xdt, hasTz, 0, removeTimezone: true);

        int targetOffset = (int)XmlConvert.ToTimeSpan(AtomizedString(tzArg)).TotalMinutes;
        return DoAdjustDateTimeToTimezone(xdt, hasTz, targetOffset, removeTimezone: false);
    }

    private static XdmValue DoAdjustDateTimeToTimezone(XPathDateTime xdt, bool hasTz, int targetOffset, bool removeTimezone)
    {
        if (removeTimezone)
        {
            return XdmValue.FromDateTime(new XPathDateTime(xdt.Year, xdt.Month, xdt.Day, xdt.Hour, xdt.Minute, xdt.Second, xdt.Millisecond, 0, false), false);
        }

        if (!hasTz)
        {
            return XdmValue.FromDateTime(new XPathDateTime(xdt.Year, xdt.Month, xdt.Day, xdt.Hour, xdt.Minute, xdt.Second, xdt.Millisecond, targetOffset, true), true);
        }

        var normalized = XPathDateTimeHelper.NormalizeToUtc(xdt);
        var withTarget = new XPathDateTime(normalized.Year, normalized.Month, normalized.Day, normalized.Hour, normalized.Minute, normalized.Second, normalized.Millisecond, -targetOffset, true);
        var targetLocal = XPathDateTimeHelper.NormalizeToUtc(withTarget);
        return XdmValue.FromDateTime(new XPathDateTime(targetLocal.Year, targetLocal.Month, targetLocal.Day, targetLocal.Hour, targetLocal.Minute, targetLocal.Second, targetLocal.Millisecond, targetOffset, true), true);
    }

    private static XdmValue NodeName_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var item = ctx.ContextItem;
        if (!item.IsNode)
            return XdmValue.Undefined;
        return NodeToQName(item.NodeValue);
    }

    private static XdmValue NodeName_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var node = GetNodeFromValue(args[0]);
        return node is null ? XdmValue.Undefined : NodeToQName(node);
    }

    private static XdmValue NodeToQName(IXdmNode node)
    {
        var kind = node.NodeKind;
        if (kind is not XdmNodeKind.Element and not XdmNodeKind.Attribute and not XdmNodeKind.Namespace and not XdmNodeKind.ProcessingInstruction)
            return XdmValue.Undefined;
        return XdmValue.FromQName(new XsQName(node.LocalName, node.NamespaceUri, node.Prefix));
    }

    // ------------------------------------------------------------------
    // fn:number / fn:data / fn:root
    // ------------------------------------------------------------------

    private static XdmValue Number_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => Number(ctx.ContextItem);

    private static XdmValue Number_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => Number(args[0]);

    private static XdmValue Number(XdmValue value)
    {
        if (value.IsUndefined)
            return XdmValue.FromDouble(double.NaN);
        return XdmValue.FromDouble(ToDoubleValue(value));
    }

    private static XdmValue Data_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => Data(ctx.ContextItem);

    private static XdmValue Data_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => Data(args[0]);

    private static XdmValue Data(XdmValue value)
    {
        if (value.IsUndefined)
            return XdmValue.Undefined;

        if (value.IsFunction)
            throw new InvalidOperationException("FOTY0013");

        if (value.IsNode)
        {
            // XDM §2.7.2: typed value of comments and PIs is xs:string;
            // for elements, attributes, text, and document nodes in the untyped
            // case it is xs:untypedAtomic.
            var node = value.NodeValue;
            if (node.NodeKind is XdmNodeKind.ProcessingInstruction or XdmNodeKind.Comment)
                return XdmValue.FromString(node.StringValue);
            return XdmValue.FromString(node.StringValue, "untypedAtomic");
        }

        if (value.IsArray)
        {
            var arr = value.ArrayValue;
            var items = new List<XdmValue>();
            for (int i = 1; i <= arr.Count; i++)
            {
                var atomized = Data(arr.Get(i));
                AppendAtomized(atomized, items);
            }
            if (items.Count == 0)
                return XdmValue.Undefined;
            if (items.Count == 1)
                return items[0];
            return XdmValue.FromSequence(MaterializedSequence.FromList(items));
        }

        if (value.IsMap)
        {
            // fn:data on a map raises FOTY0013 (type error)
            throw new InvalidOperationException("FOTY0013");
        }

        if (!value.IsSequence)
            return value;

        var seq = value.SequenceValue;
        if (seq is null)
            return XdmValue.Undefined;

        var seqItems = new List<XdmValue>();
        foreach (var item in XdmSequence.FromSource(seq))
        {
            var atomized = Data(item);
            AppendAtomized(atomized, seqItems);
        }

        if (seqItems.Count == 0)
            return XdmValue.Undefined;
        if (seqItems.Count == 1)
            return seqItems[0];
        return XdmValue.FromSequence(MaterializedSequence.FromList(seqItems));
    }

    private static void AppendAtomized(XdmValue atomized, List<XdmValue> items)
    {
        if (atomized.IsUndefined)
            return;
        if (atomized.IsSequence && atomized.SequenceValue is not null)
        {
            foreach (var sub in atomized.SequenceValue)
                items.Add(sub);
        }
        else
        {
            items.Add(atomized);
        }
    }

    private static XdmValue Root_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var item = ctx.ContextItem;
        if (!item.IsNode)
            return XdmValue.Undefined;
        return Root(item.NodeValue);
    }

    private static XdmValue Root_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var node = GetNodeFromValue(args[0]);
        return node is null ? XdmValue.Undefined : Root(node);
    }

    private static XdmValue Root(IXdmNode node)
    {
        var current = node;
        while (current.Parent is not null)
            current = current.Parent;
        return XdmValue.FromNode(current);
    }

    // ------------------------------------------------------------------
    // Date / Time component extractors
    // ------------------------------------------------------------------

    private static XdmValue UnwrapSequenceOrUndefined(XdmValue value)
    {
        if (value.IsUndefined)
            return XdmValue.Undefined;
        if (value.IsSequence && value.SequenceValue is not null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                return item;
            return XdmValue.Undefined;
        }
        return value;
    }

    private static XdmValue YearFromDateTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var v = UnwrapSequenceOrUndefined(args[0]);
        return v.IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(v.DateTimeValue.Year);
    }

    private static XdmValue MonthFromDateTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var v = UnwrapSequenceOrUndefined(args[0]);
        return v.IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(v.DateTimeValue.Month);
    }

    private static XdmValue DayFromDateTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var v = UnwrapSequenceOrUndefined(args[0]);
        return v.IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(v.DateTimeValue.Day);
    }

    private static XdmValue HoursFromDateTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var v = UnwrapSequenceOrUndefined(args[0]);
        return v.IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(v.DateTimeValue.Hour);
    }

    private static XdmValue MinutesFromDateTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var v = UnwrapSequenceOrUndefined(args[0]);
        return v.IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(v.DateTimeValue.Minute);
    }

    private static XdmValue SecondsFromDateTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var v = UnwrapSequenceOrUndefined(args[0]);
        if (v.IsUndefined) return XdmValue.Undefined;
        var dto = v.DateTimeValue;
        return XdmValue.FromDecimal(dto.Second + dto.Millisecond / 1000.0m + dto.Microsecond / 1_000_000.0m + dto.Nanosecond / 1_000_000_000.0m);
    }

    private static XdmValue YearFromDate(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var v = UnwrapSequenceOrUndefined(args[0]);
        return v.IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(v.DateValue.Year);
    }

    private static XdmValue MonthFromDate(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var v = UnwrapSequenceOrUndefined(args[0]);
        return v.IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(v.DateValue.Month);
    }

    private static XdmValue DayFromDate(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var v = UnwrapSequenceOrUndefined(args[0]);
        return v.IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(v.DateValue.Day);
    }

    private static XdmValue HoursFromTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var v = UnwrapSequenceOrUndefined(args[0]);
        return v.IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(v.TimeValue.Hour);
    }

    private static XdmValue MinutesFromTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var v = UnwrapSequenceOrUndefined(args[0]);
        return v.IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(v.TimeValue.Minute);
    }

    private static XdmValue SecondsFromTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var v = UnwrapSequenceOrUndefined(args[0]);
        if (v.IsUndefined) return XdmValue.Undefined;
        var dto = v.TimeValue;
        return XdmValue.FromDecimal(dto.Second + dto.Millisecond / 1000.0m + dto.Microsecond / 1_000_000.0m + dto.Nanosecond / 1_000_000_000.0m);
    }

    private static XdmValue TimezoneFromDateTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => TimezoneFromValue(UnwrapSequenceOrUndefined(args[0]), v => v.DateTimeValue);

    private static XdmValue TimezoneFromDate(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => TimezoneFromValue(UnwrapSequenceOrUndefined(args[0]), v => v.DateValue);

    private static XdmValue TimezoneFromTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => TimezoneFromValue(UnwrapSequenceOrUndefined(args[0]), v => v.TimeValue);

    private static XdmValue TimezoneFromValue(XdmValue value, Func<XdmValue, DateTimeOffset> getDto)
    {
        if (value.IsUndefined) return XdmValue.Undefined;
        if (!value.HasTimezone) return XdmValue.Undefined;
        var offset = getDto(value).Offset;
        return XdmValue.FromDuration(FormatDayTimeDuration(offset));
    }

    private static XdmValue YearsFromDuration(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ExtractDurationComponent(args[0], DurationPart.Years);

    private static XdmValue MonthsFromDuration(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ExtractDurationComponent(args[0], DurationPart.Months);

    private static XdmValue DaysFromDuration(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ExtractDurationComponent(args[0], DurationPart.Days);

    private static XdmValue HoursFromDuration(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ExtractDurationComponent(args[0], DurationPart.Hours);

    private static XdmValue MinutesFromDuration(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ExtractDurationComponent(args[0], DurationPart.Minutes);

    private static XdmValue SecondsFromDuration(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ExtractDurationComponent(args[0], DurationPart.Seconds);

    private enum DurationPart { Years, Months, Days, Hours, Minutes, Seconds }

    private static XdmValue ExtractDurationComponent(XdmValue value, DurationPart part)
    {
        if (value.IsUndefined || value.IsSequence) return XdmValue.Undefined;
        var s = value.ToString();
        if (string.IsNullOrEmpty(s)) return XdmValue.Undefined;

        var (years, months, days, hours, minutes, seconds) = ParseDuration(s);

        if (IsYearMonthDurationString(s))
        {
            long totalMonths = years * 12 + months;
            long normYears = totalMonths / 12;
            long normMonths = totalMonths % 12;
            return part switch
            {
                DurationPart.Years => XdmValue.FromInteger(normYears),
                DurationPart.Months => XdmValue.FromInteger(normMonths),
                DurationPart.Days => XdmValue.FromInteger(0),
                DurationPart.Hours => XdmValue.FromInteger(0),
                DurationPart.Minutes => XdmValue.FromInteger(0),
                DurationPart.Seconds => XdmValue.FromDecimal(0m),
                _ => XdmValue.Undefined
            };
        }
        else
        {
            decimal totalSeconds = days * 86400m + hours * 3600m + minutes * 60m + seconds;
            bool negative = totalSeconds < 0;
            totalSeconds = negative ? -totalSeconds : totalSeconds;
            long normDays = (long)(totalSeconds / 86400m);
            totalSeconds -= normDays * 86400m;
            long normHours = (long)(totalSeconds / 3600m);
            totalSeconds -= normHours * 3600m;
            long normMinutes = (long)(totalSeconds / 60m);
            decimal normSeconds = totalSeconds - normMinutes * 60m;
            if (negative)
            {
                normDays = -normDays;
                normHours = -normHours;
                normMinutes = -normMinutes;
                normSeconds = -normSeconds;
            }
            return part switch
            {
                DurationPart.Years => XdmValue.FromInteger(0),
                DurationPart.Months => XdmValue.FromInteger(0),
                DurationPart.Days => XdmValue.FromInteger(normDays),
                DurationPart.Hours => XdmValue.FromInteger(normHours),
                DurationPart.Minutes => XdmValue.FromInteger(normMinutes),
                DurationPart.Seconds => XdmValue.FromDecimal(normSeconds),
                _ => XdmValue.Undefined
            };
        }
    }

    private static (long Years, long Months, long Days, long Hours, long Minutes, decimal Seconds) ParseDuration(string s)
    {
        bool negative = s.StartsWith('-');
        s = negative ? s[1..] : s;
        if (!s.StartsWith('P')) return (0, 0, 0, 0, 0, 0m);
        s = s[1..];

        long years = 0, months = 0, days = 0, hours = 0, minutes = 0;
        decimal seconds = 0m;

        int tIndex = s.IndexOf('T');
        string datePart = tIndex >= 0 ? s[..tIndex] : s;
        string timePart = tIndex >= 0 ? s[(tIndex + 1)..] : "";

        years = ParseDurationNumber(ref datePart, 'Y');
        months = ParseDurationNumber(ref datePart, 'M');
        days = ParseDurationNumber(ref datePart, 'D');

        hours = ParseDurationNumber(ref timePart, 'H');
        minutes = ParseDurationNumber(ref timePart, 'M');
        seconds = ParseDurationDecimal(ref timePart, 'S');

        if (negative)
        {
            years = -years;
            months = -months;
            days = -days;
            hours = -hours;
            minutes = -minutes;
            seconds = -seconds;
        }

        return (years, months, days, hours, minutes, seconds);
    }

    private static long ParseDurationNumber(ref string s, char suffix)
    {
        int idx = s.IndexOf(suffix);
        if (idx < 0) return 0;
        var numStr = s[..idx];
        s = s[(idx + 1)..];
        return long.TryParse(numStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;
    }

    private static decimal ParseDurationDecimal(ref string s, char suffix)
    {
        int idx = s.IndexOf(suffix);
        if (idx < 0) return 0m;
        var numStr = s[..idx];
        s = s[(idx + 1)..];
        return decimal.TryParse(numStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0m;
    }

    private static bool IsDurationString(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        if (s.StartsWith('-')) s = s[1..];
        return s.StartsWith('P');
    }

    private static bool IsYearMonthDurationString(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        var t = s.StartsWith('-') ? s[1..] : s;
        if (!t.StartsWith('P')) return false;
        int tIndex = t.IndexOf('T');
        bool hasYm = t.Contains('Y') || (tIndex < 0 ? t.Contains('M') : t[..tIndex].Contains('M'));
        bool hasDt = t.Contains('D') || tIndex >= 0;
        return hasYm && !hasDt;
    }

    private static bool IsDayTimeDurationString(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        var t = s.StartsWith('-') ? s[1..] : s;
        if (!t.StartsWith('P')) return false;
        int tIndex = t.IndexOf('T');
        bool hasYm = t.Contains('Y') || (tIndex < 0 ? t.Contains('M') : t[..tIndex].Contains('M'));
        bool hasDt = t.Contains('D') || tIndex >= 0;
        return !hasYm && hasDt;
    }

    private static bool IsGenericDurationString(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        var t = s.StartsWith('-') ? s[1..] : s;
        if (!t.StartsWith('P')) return false;
        int tIndex = t.IndexOf('T');
        bool hasYm = t.Contains('Y') || (tIndex < 0 ? t.Contains('M') : t[..tIndex].Contains('M'));
        bool hasDt = t.Contains('D') || tIndex >= 0;
        return hasYm && hasDt;
    }

    private static string FormatYearMonthDuration(long totalMonths)
    {
        bool negative = totalMonths < 0;
        totalMonths = negative ? -totalMonths : totalMonths;
        long years = totalMonths / 12;
        long months = totalMonths % 12;
        var sb = new System.Text.StringBuilder();
        if (negative) sb.Append('-');
        sb.Append('P');
        if (years > 0) sb.Append($"{years}Y");
        if (months > 0 || (years == 0 && months == 0)) sb.Append($"{months}M");
        return sb.ToString();
    }

    private static string FormatDayTimeDurationFromSeconds(decimal totalSeconds)
    {
        bool negative = totalSeconds < 0;
        totalSeconds = negative ? -totalSeconds : totalSeconds;
        long days = (long)(totalSeconds / 86400m);
        totalSeconds -= days * 86400m;
        long hours = (long)(totalSeconds / 3600m);
        totalSeconds -= hours * 3600m;
        long minutes = (long)(totalSeconds / 60m);
        decimal seconds = totalSeconds - minutes * 60m;
        var sb = new System.Text.StringBuilder();
        if (negative) sb.Append('-');
        sb.Append('P');
        if (days > 0) sb.Append($"{days}D");
        if (hours > 0 || minutes > 0 || seconds > 0)
        {
            sb.Append('T');
            if (hours > 0) sb.Append($"{hours}H");
            if (minutes > 0) sb.Append($"{minutes}M");
            if (seconds > 0 || (hours == 0 && minutes == 0))
            {
                sb.Append(FormatDecimalTrim(seconds));
                sb.Append('S');
            }
        }
        if (sb.Length == (negative ? 2 : 1)) sb.Append("T0S");
        return sb.ToString();
    }

    private static string FormatDecimalTrim(decimal value)
    {
        string s = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (s.Contains('.')) s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }

    private static string FormatDayTimeDuration(TimeSpan ts)
    {
        bool negative = ts.TotalMilliseconds < 0;
        ts = negative ? ts.Negate() : ts;
        var sb = new System.Text.StringBuilder();
        if (negative) sb.Append('-');
        sb.Append('P');
        if (ts.Days > 0) sb.Append($"{ts.Days}D");
        if (ts.Hours > 0 || ts.Minutes > 0 || ts.Seconds > 0 || ts.Milliseconds > 0)
        {
            sb.Append('T');
            if (ts.Hours > 0) sb.Append($"{ts.Hours}H");
            if (ts.Minutes > 0) sb.Append($"{ts.Minutes}M");
            if (ts.Seconds > 0 || ts.Milliseconds > 0)
            {
                sb.Append($"{ts.Seconds}");
                if (ts.Milliseconds > 0)
                    sb.Append($".{ts.Milliseconds:000}");
                sb.Append('S');
            }
        }
        if (sb.Length == (negative ? 2 : 1)) sb.Append("T0S");
        return sb.ToString();
    }

    // ------------------------------------------------------------------
    // fn:deep-equal / fn:generate-id / fn:compare
    // ------------------------------------------------------------------

    private static XdmValue DeepEqual_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => DeepEqual(args[0], args[1], ctx.DefaultCollation);

    private static XdmValue DeepEqual_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string collation = AtomizedString(args[2]);
        ValidateCollation(collation);
        return DeepEqual(args[0], args[1], collation);
    }

    private static XdmValue DeepEqual(XdmValue a, XdmValue b, string collation)
    {
        var itemsA = ToItemList(a);
        var itemsB = ToItemList(b);
        if (itemsA.Count != itemsB.Count)
            return XdmValue.False;
        for (int i = 0; i < itemsA.Count; i++)
        {
            if (!DeepEqualItem(itemsA[i], itemsB[i], collation))
                return XdmValue.False;
        }
        return XdmValue.True;
    }

    private static List<XdmValue> ToItemList(XdmValue value)
    {
        if (value.IsUndefined)
            return new List<XdmValue>();
        if (!value.IsSequence)
            return new List<XdmValue> { value };
        var list = new List<XdmValue>();
        if (value.SequenceValue is not null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                list.Add(item);
        }
        return list;
    }

    private static bool DeepEqualItem(XdmValue a, XdmValue b, string collation)
    {
        // Numeric cross-type comparison: integer, decimal, float, double are all comparable
        if (IsNumeric(a) && IsNumeric(b))
        {
            // deep-equal treats NaN as equal to NaN (unlike eq)
            bool aIsNaN = a.Kind is XdmValueKind.Double or XdmValueKind.Float && double.IsNaN(a.DoubleValue);
            bool bIsNaN = b.Kind is XdmValueKind.Double or XdmValueKind.Float && double.IsNaN(b.DoubleValue);
            if (aIsNaN && bIsNaN)
                return true;

            // If either is double, promote both to double
            if (a.Kind == XdmValueKind.Double || b.Kind == XdmValueKind.Double)
            {
                double da = a.Kind == XdmValueKind.Integer ? a.IntegerValue :
                            a.Kind == XdmValueKind.Decimal ? (double)a.DecimalValue :
                            a.Kind == XdmValueKind.Float ? a.DoubleValue : a.DoubleValue;
                double db = b.Kind == XdmValueKind.Integer ? b.IntegerValue :
                            b.Kind == XdmValueKind.Decimal ? (double)b.DecimalValue :
                            b.Kind == XdmValueKind.Float ? b.DoubleValue : b.DoubleValue;
                return da == db;
            }

            // If either is float, promote both to float
            if (a.Kind == XdmValueKind.Float || b.Kind == XdmValueKind.Float)
            {
                float fa = a.Kind == XdmValueKind.Integer ? a.IntegerValue :
                           a.Kind == XdmValueKind.Decimal ? (float)a.DecimalValue : (float)a.DoubleValue;
                float fb = b.Kind == XdmValueKind.Integer ? b.IntegerValue :
                           b.Kind == XdmValueKind.Decimal ? (float)b.DecimalValue : (float)b.DoubleValue;
                return fa == fb;
            }

            // Both are integer or decimal
            decimal ma = a.Kind == XdmValueKind.Integer ? a.IntegerValue : a.DecimalValue;
            decimal mb = b.Kind == XdmValueKind.Integer ? b.IntegerValue : b.DecimalValue;
            return ma == mb;
        }

        if (a.Kind != b.Kind)
            return false;

        // Duration equality: normalize to total months and total seconds
        if (a.Kind == XdmValueKind.Duration)
        {
            var (aYears, aMonths, aDays, aHours, aMinutes, aSeconds) = ParseDuration(a.DurationValue);
            var (bYears, bMonths, bDays, bHours, bMinutes, bSeconds) = ParseDuration(b.DurationValue);
            long aTotalMonths = aYears * 12 + aMonths;
            long bTotalMonths = bYears * 12 + bMonths;
            decimal aTotalSeconds = aDays * 86400m + aHours * 3600m + aMinutes * 60m + aSeconds;
            decimal bTotalSeconds = bDays * 86400m + bHours * 3600m + bMinutes * 60m + bSeconds;
            return aTotalMonths == bTotalMonths && aTotalSeconds == bTotalSeconds;
        }

        return a.Kind switch
        {
            XdmValueKind.Undefined => true,
            XdmValueKind.Boolean => a.BooleanValue == b.BooleanValue,
            XdmValueKind.Integer => a.IntegerValue == b.IntegerValue,
            XdmValueKind.Decimal => a.DecimalValue == b.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => a.DoubleValue == b.DoubleValue,
            XdmValueKind.String => CompareStrings(a.StringValue, b.StringValue, collation) == 0,
            XdmValueKind.DateTime => a.DateTimeValue == b.DateTimeValue,
            XdmValueKind.Date => a.DateValue == b.DateValue,
            XdmValueKind.Time => a.TimeValue == b.TimeValue,
            XdmValueKind.QName => a.QNameValue.Equals(b.QNameValue),
            XdmValueKind.Node => DeepEqualNode(a.NodeValue, b.NodeValue, collation),
            XdmValueKind.Sequence => DeepEqual(a, b, collation).BooleanValue,
            XdmValueKind.Map => DeepEqualMap(a.MapValue, b.MapValue, collation),
            XdmValueKind.Array => DeepEqualArray(a.ArrayValue, b.ArrayValue, collation),
            _ => false
        };
    }

    private static bool IsNumeric(XdmValue value)
        => value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Float or XdmValueKind.Double;

    private static bool DeepEqualNode(IXdmNode a, IXdmNode b, string collation)
    {
        if (a.NodeKind != b.NodeKind)
            return false;
        if (a.LocalName != b.LocalName)
            return false;
        if (a.NamespaceUri != b.NamespaceUri)
            return false;
        if (CompareStrings(a.StringValue, b.StringValue, collation) != 0)
            return false;

        if (a.NodeKind == XdmNodeKind.Element)
        {
            var attrsA = SortNodes(a.Attributes());
            var attrsB = SortNodes(b.Attributes());
            if (attrsA.Count != attrsB.Count)
                return false;
            for (int i = 0; i < attrsA.Count; i++)
            {
                if (!DeepEqualNode(attrsA[i].NodeValue, attrsB[i].NodeValue, collation))
                    return false;
            }

            var childrenA = ToNodeList(a.Children());
            var childrenB = ToNodeList(b.Children());
            if (childrenA.Count != childrenB.Count)
                return false;
            for (int i = 0; i < childrenA.Count; i++)
            {
                if (!DeepEqualNode(childrenA[i], childrenB[i], collation))
                    return false;
            }
        }
        return true;
    }

    private static List<XdmValue> SortNodes(XdmSequence sequence)
    {
        var list = new List<XdmValue>();
        foreach (var item in sequence)
            list.Add(item);
        list.Sort((x, y) =>
        {
            var nx = x.NodeValue;
            var ny = y.NodeValue;
            int cmp = string.CompareOrdinal(nx.NamespaceUri, ny.NamespaceUri);
            return cmp != 0 ? cmp : string.CompareOrdinal(nx.LocalName, ny.LocalName);
        });
        return list;
    }

    private static List<IXdmNode> ToNodeList(XdmSequence sequence)
    {
        var list = new List<IXdmNode>();
        foreach (var item in sequence)
            list.Add(item.NodeValue);
        return list;
    }

    private static bool DeepEqualMap(XdmMap a, XdmMap b, string collation)
    {
        if (a.Count != b.Count)
            return false;
        var entriesA = a.Entries.ToList();
        var entriesB = b.Entries.ToList();
        foreach (var (keyA, valA) in entriesA)
        {
            bool found = false;
            foreach (var (keyB, valB) in entriesB)
            {
                if (DeepEqualItem(keyA, keyB, collation) && DeepEqualItem(valA, valB, collation))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                return false;
        }
        return true;
    }

    private static bool DeepEqualArray(XdmArray a, XdmArray b, string collation)
    {
        if (a.Count != b.Count)
            return false;
        var av = a.Values.ToList();
        var bv = b.Values.ToList();
        for (int i = 0; i < av.Count; i++)
        {
            if (!DeepEqualItem(av[i], bv[i], collation))
                return false;
        }
        return true;
    }

    private static long _generateIdCounter;
    private static readonly ConditionalWeakTable<object, string> _generateIdMap = new();
    private static readonly object _generateIdLock = new();

    private static XdmValue GenerateId_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var item = ctx.ContextItem;
        if (!item.IsNode)
            return XdmValue.FromString(string.Empty);
        return XdmValue.FromString(GetNodeId(item.NodeValue));
    }

    private static XdmValue GenerateId_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var node = GetNodeFromValue(args[0]);
        return XdmValue.FromString(node is null ? string.Empty : GetNodeId(node));
    }

    private static string GetNodeId(IXdmNode node)
    {
        // Use the underlying XObject as the key so that different XDocumentNode
        // wrappers around the same LINQ-to-XML node get the same ID.
        var key = node is Bosak.XPath.Providers.Xml.XDocumentNode xdoc ? (object)xdoc.UnderlyingObject : node;
        if (_generateIdMap.TryGetValue(key, out var id))
            return id;
        lock (_generateIdLock)
        {
            if (_generateIdMap.TryGetValue(key, out id))
                return id;
            id = "id" + Interlocked.Increment(ref _generateIdCounter);
            _generateIdMap.AddOrUpdate(key, id);
            return id;
        }
    }

    private static XdmValue Compare_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (IsEmptySequence(args[0]) || IsEmptySequence(args[1]))
            return XdmValue.Undefined;
        return Compare(args[0], args[1], ctx.DefaultCollation);
    }

    private static XdmValue Compare_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (IsEmptySequence(args[0]) || IsEmptySequence(args[1]))
            return XdmValue.Undefined;
        string collation = AtomizedString(args[2]);
        ValidateCollation(collation);
        return Compare(args[0], args[1], collation);
    }

    private static XdmValue Compare(XdmValue a, XdmValue b, string collation = "")
    {
        var s1 = AtomizedString(a);
        var s2 = AtomizedString(b);
        int cmp = CompareStrings(s1, s2, collation);
        return XdmValue.FromInteger(cmp < 0 ? -1 : cmp > 0 ? 1 : 0);
    }

    // ------------------------------------------------------------------
    // URI encoding functions
    // ------------------------------------------------------------------

    private static XdmValue EncodeForUri(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (IsEmptySequence(arg))
            return XdmValue.FromString("");
        if (arg.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float)
            throw new InvalidOperationException("XPTY0004");
        var s = AtomizedString(arg);
        return XdmValue.FromString(Uri.EscapeDataString(s));
    }

    private static XdmValue IriToUri(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var s = AtomizedString(args[0]);
        var sb = new StringBuilder();
        foreach (var rune in s.EnumerateRunes())
        {
            if (rune.Value <= 0x7E && IsUriChar((char)rune.Value))
                sb.Append(rune);
            else
                AppendPercentEncoded(sb, rune);
        }
        return XdmValue.FromString(sb.ToString());
    }

    private static XdmValue EscapeHtmlUri(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (IsEmptySequence(arg))
            return XdmValue.FromString("");
        if (arg.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float)
            throw new InvalidOperationException("XPTY0004");
        var s = AtomizedString(arg);
        var sb = new StringBuilder();
        foreach (var rune in s.EnumerateRunes())
        {
            if (rune.Value is >= 0x20 and <= 0x7E)
                sb.Append(rune);
            else
                AppendPercentEncoded(sb, rune);
        }
        return XdmValue.FromString(sb.ToString());
    }

    private static bool IsUriChar(char c)
    {
        // unreserved + reserved + '%'
        return c is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or (>= '0' and <= '9')
            or '-' or '.' or '_' or '~'
            or ':' or '/' or '?' or '#' or '[' or ']' or '@' or '!' or '$' or '&' or '\'' or '(' or ')' or '*' or '+' or ',' or ';' or '='
            or '%';
    }

    private static void AppendPercentEncoded(StringBuilder sb, Rune rune)
    {
        Span<byte> utf8 = stackalloc byte[4];
        int bytesWritten = rune.EncodeToUtf8(utf8);
        foreach (byte b in utf8[..bytesWritten])
            sb.Append($"%{b:X2}");
    }

    // ------------------------------------------------------------------
    // QName functions
    // ------------------------------------------------------------------

    private static XdmValue Qname(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var ns = AtomizedString(args[0]);
        var lexical = AtomizedString(args[1]);
        var local = lexical.Contains(':') ? lexical[(lexical.IndexOf(':') + 1)..] : lexical;
        var prefix = lexical.Contains(':') ? lexical[..lexical.IndexOf(':')] : string.Empty;
        return XdmValue.FromQName(new XsQName(local, ns, prefix));
    }

    private static XdmValue ResolveQName(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var lexical = AtomizedString(args[0]);
        IXdmNode? node = null;
        if (args[1].IsSequence && args[1].SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(args[1].SequenceValue))
            {
                if (item.IsNode)
                {
                    node = item.NodeValue;
                    break;
                }
            }
        }
        else if (args[1].IsNode)
        {
            node = args[1].NodeValue;
        }

        if (node == null || string.IsNullOrEmpty(lexical))
            return XdmValue.Undefined;

        string prefix;
        string local;
        var colonCount = lexical.Count(c => c == ':');
        if (colonCount > 1)
        {
            throw new InvalidOperationException("FOCA0002: Invalid lexical QName");
        }
        if (lexical.Contains(':'))
        {
            var idx = lexical.IndexOf(':');
            prefix = lexical[..idx];
            local = lexical[(idx + 1)..];
            if (string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(local))
                throw new InvalidOperationException("FOCA0002: Invalid lexical QName");
        }
        else
        {
            prefix = string.Empty;
            local = lexical;
        }

        try
        {
            if (!string.IsNullOrEmpty(prefix))
                System.Xml.XmlConvert.VerifyNCName(prefix);
            System.Xml.XmlConvert.VerifyNCName(local);
        }
        catch (System.Xml.XmlException)
        {
            throw new InvalidOperationException("FOCA0002: Invalid lexical QName");
        }

        var nsUri = ResolvePrefix(node, prefix);
        if (!string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(nsUri))
            throw new InvalidOperationException("FONS0004: No namespace binding for prefix '" + prefix + "'");
        return XdmValue.FromQName(new XsQName(local, nsUri, prefix));
    }

    private static string ResolvePrefix(IXdmNode node, string prefix)
    {
        // Try to find the namespace URI by walking the namespace axis
        var seq = node.Axis(XdmAxis.Namespace);
        foreach (var item in seq)
        {
            var nsNode = item.NodeValue;
            if (nsNode.LocalName == prefix)
                return nsNode.StringValue;
        }
        return string.Empty;
    }

    private static XdmValue LocalNameFromQName(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var atomized = AtomizeValue(args[0]);
        if (atomized.Kind == XdmValueKind.Undefined || IsEmptySequence(atomized))
            return XdmValue.FromSequence(XdmSequence.Empty);
        var qn = atomized.QNameValue;
        return XdmValue.FromString(qn.LocalName);
    }

    private static XdmValue NamespaceUriFromQName(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var atomized = AtomizeValue(args[0]);
        if (atomized.Kind == XdmValueKind.Undefined || IsEmptySequence(atomized))
            return XdmValue.FromSequence(XdmSequence.Empty);
        var qn = atomized.QNameValue;
        return XdmValue.FromString(qn.NamespaceUri);
    }

    private static XdmValue PrefixFromQName(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var atomized = AtomizeValue(args[0]);
        if (atomized.Kind == XdmValueKind.Undefined || IsEmptySequence(atomized))
            return XdmValue.FromSequence(XdmSequence.Empty);
        var qn = atomized.QNameValue;
        return XdmValue.FromString(qn.Prefix);
    }

    // ------------------------------------------------------------------
    // JSON functions (parse-json, json-to-xml, xml-to-json, json-doc)
    // ------------------------------------------------------------------

    private static readonly string JsonXmlNs = "http://www.w3.org/2005/xpath-functions";

    private readonly record struct JsonOptions(bool Liberal, string Duplicates, bool Escape, bool Indent, XdmValue? Fallback = null)
    {
        public static JsonOptions Default => new(false, "use-first", false, false, null);
    }

    private static JsonOptions ParseJsonOptions(XdmValue? options)
    {
        if (options is null || options.Value.IsUndefined)
            return JsonOptions.Default;
        if (!options.Value.IsMap)
            return JsonOptions.Default;

        var map = options.Value.MapValue;
        var result = JsonOptions.Default;

        if (map.TryGetValue(XdmValue.FromString("liberal"), out var liberal))
            result = result with { Liberal = liberal.BooleanValue };
        if (map.TryGetValue(XdmValue.FromString("duplicates"), out var dup))
        {
            var dupStr = RequireString(dup);
            if (dupStr != "use-first" && dupStr != "retain" && dupStr != "reject")
                throw new InvalidOperationException("FOJS0005: Invalid duplicates option");
            result = result with { Duplicates = dupStr };
        }
        if (map.TryGetValue(XdmValue.FromString("escape"), out var escape))
            result = result with { Escape = escape.BooleanValue };
        if (map.TryGetValue(XdmValue.FromString("indent"), out var indent))
            result = result with { Indent = indent.BooleanValue };
        if (map.TryGetValue(XdmValue.FromString("fallback"), out var fallback))
            result = result with { Fallback = fallback };

        return result;
    }

    private static XdmValue ParseJson_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ParseJson(ctx, AtomizedString(args[0]), JsonOptions.Default);

    private static XdmValue ParseJson_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ParseJson(ctx, AtomizedString(args[0]), ParseJsonOptions(args[1]));

    private static XdmValue ParseJson(EvaluationContext ctx, string json, JsonOptions options)
    {
        if (string.IsNullOrEmpty(json))
            throw new InvalidOperationException("FOJS0001: Empty string is not valid JSON");

        var docOptions = new JsonDocumentOptions
        {
            AllowTrailingCommas = options.Liberal
        };

        using var document = JsonDocument.Parse(json, docOptions);
        return JsonElementToXdmValue(ctx, document.RootElement, options);
    }

    private static XdmValue JsonElementToXdmValue(EvaluationContext ctx, JsonElement element, JsonOptions options)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                {
                    var map = new XdmMap();
                    foreach (var property in element.EnumerateObject())
                    {
                        var key = XdmValue.FromString(options.Escape ? JsonEscapeString(property.Name) : property.Name);
                        var value = JsonElementToXdmValue(ctx, property.Value, options);
                        if (map.ContainsKey(key))
                        {
                            if (options.Duplicates == "reject")
                                throw new InvalidOperationException("FOJS0003: Duplicate key in JSON object");
                            if (options.Duplicates == "use-first")
                                continue;
                        }
                        map.Add(key, value);
                    }
                    return XdmValue.FromMap(map);
                }
            case JsonValueKind.Array:
                {
                    var array = new XdmArray();
                    foreach (var item in element.EnumerateArray())
                        array.Add(JsonElementToXdmValue(ctx, item, options));
                    return XdmValue.FromArray(array);
                }
            case JsonValueKind.String:
                return XdmValue.FromString(ProcessJsonString(element.GetString()!, options, ctx));
            case JsonValueKind.Number:
                return XdmValue.FromDouble(element.GetDouble());
            case JsonValueKind.True:
                return XdmValue.True;
            case JsonValueKind.False:
                return XdmValue.False;
            case JsonValueKind.Null:
                return XdmValue.Undefined;
            default:
                return XdmValue.Undefined;
        }
    }

    private static string JsonEscapeString(string s)
    {
        var sb = new StringBuilder();
        foreach (char c in s)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < 0x20)
                        sb.Append($"\\u{(int)c:X4}");
                    else
                        sb.Append(c);
                    break;
            }
        }
        return sb.ToString();
    }

    private static string ProcessJsonString(string s, JsonOptions options, EvaluationContext ctx)
    {
        if (!s.Any(c => c < 0x20))
            return options.Escape ? JsonEscapeString(s) : s;

        // If escape=true, JSON-escape control characters directly
        if (options.Escape)
            return JsonEscapeString(s);

        var sb = new StringBuilder();
        foreach (char c in s)
        {
            if (c < 0x20)
            {
                if (options.Fallback is not null && !options.Fallback.Value.IsUndefined)
                {
                    var fallbackArg = XdmValue.FromString($"\\u{(int)c:X4}");
                    var fallbackResult = VmEngine.InvokeFunctionItem(options.Fallback.Value, ctx, new[] { fallbackArg });
                    sb.Append(AtomizedString(fallbackResult));
                }
                else
                {
                    sb.Append('\uFFFD');
                }
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    private static XdmValue JsonToXml_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => JsonToXml(ctx, AtomizedString(args[0]), JsonOptions.Default);

    private static XdmValue JsonToXml_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => JsonToXml(ctx, AtomizedString(args[0]), ParseJsonOptions(args[1]));

    private static XdmValue JsonToXml(EvaluationContext ctx, string json, JsonOptions options)
    {
        if (string.IsNullOrEmpty(json))
            throw new InvalidOperationException("FOJS0001: Empty string is not valid JSON");

        var docOptions = new JsonDocumentOptions
        {
            AllowTrailingCommas = options.Liberal
        };

        using var document = JsonDocument.Parse(json, docOptions);
        var rootElement = JsonElementToXml(ctx, document.RootElement, options);
        var xdoc = new XDocument(rootElement);
        return XdmValue.FromNode(new XDocumentNode(xdoc));
    }

    private static XElement JsonElementToXml(EvaluationContext ctx, JsonElement element, JsonOptions options)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                {
                    var mapEl = new XElement(XName.Get("map", JsonXmlNs));
                    var seenKeys = new HashSet<string>();
                    foreach (var property in element.EnumerateObject())
                    {
                        var key = options.Escape ? JsonEscapeString(property.Name) : property.Name;
                        if (!seenKeys.Add(key))
                        {
                            if (options.Duplicates == "reject")
                                throw new InvalidOperationException("FOJS0003: Duplicate key in JSON object");
                            if (options.Duplicates == "use-first")
                                continue;
                        }
                        var child = JsonElementToXml(ctx, property.Value, options);
                        child.SetAttributeValue(XName.Get("key"), key);
                        mapEl.Add(child);
                    }
                    return mapEl;
                }
            case JsonValueKind.Array:
                {
                    var arrEl = new XElement(XName.Get("array", JsonXmlNs));
                    foreach (var item in element.EnumerateArray())
                        arrEl.Add(JsonElementToXml(ctx, item, options));
                    return arrEl;
                }
            case JsonValueKind.String:
                {
                    var processed = ProcessJsonString(element.GetString()!, options, ctx);
                    var strEl = new XElement(XName.Get("string", JsonXmlNs), processed);
                    if (options.Escape && processed != element.GetString()!)
                        strEl.SetAttributeValue(XName.Get("escaped"), "true");
                    return strEl;
                }
            case JsonValueKind.Number:
                return new XElement(XName.Get("number", JsonXmlNs), element.GetRawText());
            case JsonValueKind.True:
                return new XElement(XName.Get("boolean", JsonXmlNs), "true");
            case JsonValueKind.False:
                return new XElement(XName.Get("boolean", JsonXmlNs), "false");
            case JsonValueKind.Null:
                return new XElement(XName.Get("null", JsonXmlNs));
            default:
                return new XElement(XName.Get("null", JsonXmlNs));
        }
    }

    private static XdmValue XmlToJson_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XmlToJson(args[0], JsonOptions.Default);

    private static XdmValue XmlToJson_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XmlToJson(args[0], ParseJsonOptions(args[1]));

    private static XdmValue XmlToJson(XdmValue nodeValue, JsonOptions options)
    {
        if (!nodeValue.IsNode)
            throw new InvalidOperationException("XPTY0004: xml-to-json requires a node");

        var node = nodeValue.NodeValue;
        IXdmNode? root = null;
        if (node.NodeKind == XdmNodeKind.Document)
        {
            foreach (var child in node.Axis(XdmAxis.Child))
            {
                var childNode = child.NodeValue!;
                if (childNode.NodeKind == XdmNodeKind.Element)
                {
                    root = childNode;
                    break;
                }
            }
            if (root is null)
                throw new InvalidOperationException("FOJS0006: Document node has no element child");
        }
        else if (node.NodeKind == XdmNodeKind.Element)
        {
            root = node;
        }
        else
        {
            throw new InvalidOperationException("XPTY0004: xml-to-json requires an element or document node");
        }

        var sb = new StringBuilder();
        XmlNodeToJsonString(root, sb, options);
        return XdmValue.FromString(sb.ToString());
    }

    private static void XmlNodeToJsonString(IXdmNode node, StringBuilder sb, JsonOptions options)
    {
        var localName = node.LocalName;
        switch (localName)
        {
            case "map":
                sb.Append('{');
                var first = true;
                foreach (var child in node.Axis(XdmAxis.Child))
                {
                    var childNode = child.NodeValue!;
                    if (childNode.NodeKind != XdmNodeKind.Element)
                        continue;
                    if (!first) sb.Append(',');
                    first = false;
                    string? key = null;
                    foreach (var attr in childNode.Attributes("key"))
                    {
                        key = attr.NodeValue?.StringValue;
                        break;
                    }
                    if (key is null)
                        throw new InvalidOperationException("FOJS0006: Missing key attribute in map entry");
                    sb.Append('"');
                    sb.Append(JsonEscapeKey(key));
                    sb.Append("\":");
                    XmlNodeToJsonString(childNode, sb, options);
                }
                sb.Append('}');
                break;
            case "array":
                sb.Append('[');
                first = true;
                foreach (var child in node.Axis(XdmAxis.Child))
                {
                    var childNode = child.NodeValue!;
                    if (childNode.NodeKind != XdmNodeKind.Element)
                        continue;
                    if (!first) sb.Append(',');
                    first = false;
                    XmlNodeToJsonString(childNode, sb, options);
                }
                sb.Append(']');
                break;
            case "string":
                sb.Append('"');
                sb.Append(JsonEscapeString(node.StringValue));
                sb.Append('"');
                break;
            case "number":
                sb.Append(node.StringValue);
                break;
            case "boolean":
                sb.Append(node.StringValue);
                break;
            case "null":
                sb.Append("null");
                break;
            default:
                throw new InvalidOperationException($"FOJS0006: Unexpected element {localName} in JSON XML representation");
        }
    }

    private static string JsonEscapeKey(string s)
    {
        var sb = new StringBuilder();
        foreach (char c in s)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < 0x20)
                        sb.Append($"\\u{(int)c:X4}");
                    else
                        sb.Append(c);
                    break;
            }
        }
        return sb.ToString();
    }

    private static XdmValue JsonDoc_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => JsonDoc(ctx, AtomizedString(args[0]), JsonOptions.Default);

    private static XdmValue JsonDoc_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => JsonDoc(ctx, AtomizedString(args[0]), ParseJsonOptions(args[1]));

    private static XdmValue JsonDoc(EvaluationContext ctx, string uri, JsonOptions options)
    {
        if (string.IsNullOrEmpty(uri))
            throw new InvalidOperationException("FOUT1170: Invalid URI");

        string json;
        if (ctx.DocumentLoader is not null)
        {
            var node = ctx.DocumentLoader(uri);
            json = node.StringValue;
        }
        else
        {
            try
            {
                json = File.ReadAllText(uri);
            }
            catch
            {
                throw new InvalidOperationException($"FOUT1170: Cannot load JSON document {uri}");
            }
        }

        return ParseJson(ctx, json, options);
    }

    private static XdmValue CopyOf_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var item = ctx.ContextItem;
        if (item.IsUndefined)
            throw new InvalidOperationException("XPDY0002: copy-of() requires a context item.");
        return CopyOf(item);
    }

    private static XdmValue CopyOf_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var item = args[0];
        if (item.IsUndefined)
            return XdmValue.Undefined;
        return CopyOf(item);
    }

    private static XdmValue CopyOf(XdmValue item)
    {
        // If a singleton sequence was passed, extract the node.
        if (item.IsSequence)
        {
            var items = new List<XdmValue>();
            foreach (var seqItem in XdmSequence.FromSource(item.SequenceValue!))
                items.Add(seqItem);
            if (items.Count == 1)
                return CopyOf(items[0]);
            // Map over each item for multi-item sequences.
            var results = new List<XdmValue>(items.Count);
            foreach (var i in items)
                results.Add(CopyOf(i));
            return XdmValue.FromSequence(MaterializedSequence.FromList(results));
        }

        if (item.IsNode && item.NodeValue != null)
        {
            var copied = DeepCopyNode(item.NodeValue);
            if (copied != null)
                return XdmValue.FromNode(copied);
        }
        // For atomic values, return the value itself (copy-of on atomic is identity)
        return item;
    }

    private static IXdmNode? DeepCopyNode(IXdmNode node)
    {
        if (node is Providers.Xml.XDocumentNode xdocNode)
        {
            var obj = xdocNode.UnderlyingObject;
            switch (obj)
            {
                case XElement elem:
                    return new Providers.Xml.XDocumentNode(DeepCopyElement(elem));
                case XDocument doc:
                    return new Providers.Xml.XDocumentNode(DeepCopyDocument(doc));
                case XText text:
                    return new Providers.Xml.XDocumentNode(new XText(text.Value));
                case XComment comment:
                    return new Providers.Xml.XDocumentNode(new XComment(comment.Value));
                case XProcessingInstruction pi:
                    return new Providers.Xml.XDocumentNode(new XProcessingInstruction(pi.Target, pi.Data));
                case XAttribute attr:
                    return new Providers.Xml.XDocumentNode(new XAttribute(XName.Get(attr.Name.LocalName, attr.Name.NamespaceName), attr.Value));
            }
        }
        return null;
    }

    private static XElement DeepCopyElement(XElement element)
    {
        var copy = new XElement(XName.Get(element.Name.LocalName, element.Name.NamespaceName));
        foreach (var attr in element.Attributes())
        {
            copy.SetAttributeValue(XName.Get(attr.Name.LocalName, attr.Name.NamespaceName), attr.Value);
        }
        CopyInScopeNamespaces(element, copy);
        foreach (var child in element.Nodes())
        {
            switch (child)
            {
                case XElement childElem:
                    copy.Add(DeepCopyElement(childElem));
                    break;
                case XText text:
                    copy.Add(new XText(text.Value));
                    break;
                case XComment comment:
                    copy.Add(new XComment(comment.Value));
                    break;
                case XProcessingInstruction pi:
                    copy.Add(new XProcessingInstruction(pi.Target, pi.Data));
                    break;
            }
        }
        return copy;
    }

    /// <summary>
    /// Copies all namespace bindings that are in scope for <paramref name="source"/> to
    /// <paramref name="target"/>, mirroring the behaviour of <c>xsl:copy-of</c> which
    /// preserves namespace nodes on the copied element.
    /// </summary>
    private static void CopyInScopeNamespaces(XElement source, XElement target)
    {
        var seen = new HashSet<string>();
        var current = source;
        while (current != null)
        {
            foreach (var attr in current.Attributes())
            {
                if (!attr.IsNamespaceDeclaration)
                    continue;

                string prefix = attr.Name.LocalName == "xmlns" ? string.Empty : attr.Name.LocalName;
                if (prefix == "xml")
                    continue;
                if (!seen.Add(prefix))
                    continue;
                if (string.IsNullOrEmpty(attr.Value))
                    continue;

                if (string.IsNullOrEmpty(prefix))
                {
                    var existing = target.Attribute("xmlns");
                    if (existing == null)
                        target.SetAttributeValue("xmlns", attr.Value);
                    else if (existing.Value != attr.Value)
                        target.SetAttributeValue("xmlns", attr.Value);
                }
                else
                {
                    XName declName = XNamespace.Xmlns + prefix;
                    var existing = target.Attribute(declName);
                    if (existing == null)
                        target.SetAttributeValue(declName, attr.Value);
                    else if (existing.Value != attr.Value)
                        target.SetAttributeValue(declName, attr.Value);
                }
            }
            current = current.Parent;
        }
    }

    private static XDocument DeepCopyDocument(XDocument document)
    {
        var copy = new XDocument();
        foreach (var node in document.Nodes())
        {
            switch (node)
            {
                case XElement elem:
                    copy.Add(DeepCopyElement(elem));
                    break;
                case XComment comment:
                    copy.Add(new XComment(comment.Value));
                    break;
                case XProcessingInstruction pi:
                    copy.Add(new XProcessingInstruction(pi.Target, pi.Data));
                    break;
            }
        }
        return copy;
    }

    private static XdmValue InScopePrefixes(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var node = GetNodeFromValue(args[0]);
        if (node == null || node.NodeKind != XdmNodeKind.Element)
            return XdmValue.Undefined;

        var prefixes = new List<XdmValue>();
        var seen = new HashSet<string>();

        if (node is Providers.Xml.XDocumentNode xdocNode && xdocNode.UnderlyingObject is XElement elem)
        {
            // Collect the ancestor-or-self chain.
            var path = new List<XElement>();
            var current = elem;
            while (current != null)
            {
                path.Add(current);
                current = current.Parent;

                // Stop at namespace inheritance barrier (inherit-namespaces="no").
                if (current is XElement parent && parent.Annotation<NamespaceInheritanceBarrier>() != null)
                    break;
            }

            // Process from target element toward the root so that innermost
            // declarations (and undeclarations) win.
            foreach (var el in path)
            {
                foreach (var attr in el.Attributes())
                {
                    if (attr.IsNamespaceDeclaration)
                    {
                        var prefix = attr.Name.LocalName == "xmlns" ? "" : attr.Name.LocalName;
                        if (attr.Value == "" && prefix == "")
                        {
                            // xmlns="" undeclares the default namespace.
                            seen.Add("");
                        }
                        else if (seen.Add(prefix))
                        {
                            prefixes.Add(XdmValue.FromString(prefix));
                        }
                    }
                }
            }
        }

        // xml prefix is always in scope
        if (seen.Add("xml"))
            prefixes.Add(XdmValue.FromString("xml"));

        return XdmValue.FromSequence(MaterializedSequence.FromList(prefixes));
    }

    private static XdmValue NamespaceUriForPrefix(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var prefix = AtomizedString(args[0]);
        var node = GetNodeFromValue(args[1]);
        if (node == null || node.NodeKind != XdmNodeKind.Element)
            return XdmValue.Undefined;

        if (prefix == "xml")
            return XdmValue.FromString("http://www.w3.org/XML/1998/namespace");

        if (node is Providers.Xml.XDocumentNode xdocNode && xdocNode.UnderlyingObject is XElement elem)
        {
            var current = elem;
            while (current != null)
            {
                foreach (var attr in current.Attributes())
                {
                    if (attr.IsNamespaceDeclaration)
                    {
                        var attrPrefix = attr.Name.LocalName == "xmlns" ? "" : attr.Name.LocalName;
                        if (attrPrefix == prefix)
                        {
                            // xmlns="" undeclares the default namespace.
                            if (attr.Value == "" && prefix == "")
                                return XdmValue.Undefined;
                            return XdmValue.FromString(attr.Value);
                        }
                    }
                }
                current = current.Parent;

                // Stop at namespace inheritance barrier (inherit-namespaces="no").
                if (current is XElement parent && parent.Annotation<NamespaceInheritanceBarrier>() != null)
                    break;
            }
        }

        return XdmValue.Undefined;
    }
}

file static class Namespaces
{
    public const string Fn = "http://www.w3.org/2005/xpath-functions";
    public const string Math = "http://www.w3.org/2005/xpath-functions/math";
    public const string Map = "http://www.w3.org/2005/xpath-functions/map";
    public const string Array = "http://www.w3.org/2005/xpath-functions/array";
    public const string Xs = "http://www.w3.org/2001/XMLSchema";
    public const string Xsl = "http://www.w3.org/1999/XSL/Transform";
}



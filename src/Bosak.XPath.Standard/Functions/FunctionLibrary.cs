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
//                      | Charles Korthout | 0.6   | 19-05-2026     | Added fn:node-name                                                                     |
//                      | Charles Korthout | 0.7   | 19-05-2026     | Added fn:number, fn:data, fn:root                                                      |
//                      | Charles Korthout | 0.8   | 19-05-2026     | Added date/time component extractors                                                   |
//                      | Charles Korthout | 0.9   | 19-05-2026     | Added fn:deep-equal, fn:generate-id, fn:compare                                        |
//                      | Charles Korthout | 1.0   | 19-05-2026     | Added URI encoders and QName functions                                                   |
//                      | Charles Korthout | 1.1   | 19-05-2026     | Added fn:doc and fn:collection with document identity caching                          |
//                      | Charles Korthout | 1.2   | 19-05-2026     | Added substring-before, substring-after, string-to-codepoints, codepoints-to-string, parse-xml |
//                      | Charles Korthout | 1.3   | 19-05-2026     | Added fn:analyze-string with regex group extraction                                    |
//                      | Charles Korthout | 1.4   | 19-05-2026     | Added fn:serialize                                                                     |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Frozen;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

            // ----- fn:concat --------------------------------------------------
            [(Namespaces.Fn, "concat", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "concat",
                Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.String],
                ReturnType = XdmValueKind.String,
                Implementation = Concat
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
            // ----- fn:for-each, fn:filter, fn:fold-left, fn:fold-right, fn:for-each-pair -----
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
        return XdmValue.FromString(item.ToString());
    }

    private static XdmValue String_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (arg.IsSequence)
        {
            // fn:string on a sequence takes the first item
            foreach (var item in XdmSequence.FromSource(arg.SequenceValue!))
                return XdmValue.FromString(item.ToString());
            return XdmValue.FromString(string.Empty);
        }
        return XdmValue.FromString(arg.ToString());
    }

    private static XdmValue Concat(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromString(args[0].ToString() + args[1].ToString());

    private static XdmValue Count(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var seq = args[0];
        if (!seq.IsSequence)
            return XdmValue.FromInteger(1);

        if (seq.SequenceValue!.TryGetLength(out var len))
            return XdmValue.FromInteger(len);

        // Materialize to count
        long count = 0;
        foreach (var _ in XdmSequence.FromSource(seq.SequenceValue!))
            count++;
        return XdmValue.FromInteger(count);
    }

    private static XdmValue Exists(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromBoolean(!args[0].IsUndefined && args[0].EffectiveBooleanValue());

    private static XdmValue Empty(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromBoolean(args[0].IsUndefined || !args[0].EffectiveBooleanValue());

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
            return XdmValue.FromSequence(XdmSequence.Empty);

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

    private static XdmValue ForEach_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var func = (FunctionItem)args[1].FunctionValue;
        var result = new List<XdmValue>();
        foreach (var item in AsSequence(args[0]))
        {
            AppendResult(VmEngine.InvokeFunctionItem(func, ctx, new[] { item }), result);
        }
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static XdmValue Filter_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var func = (FunctionItem)args[1].FunctionValue;
        var result = new List<XdmValue>();
        foreach (var item in AsSequence(args[0]))
        {
            var pred = VmEngine.InvokeFunctionItem(func, ctx, new[] { item });
            if (pred.EffectiveBooleanValue())
                result.Add(item);
        }
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static XdmValue FoldLeft_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var func = (FunctionItem)args[2].FunctionValue;
        var accumulator = args[1];
        foreach (var item in AsSequence(args[0]))
        {
            accumulator = VmEngine.InvokeFunctionItem(func, ctx, new[] { accumulator, item });
        }
        return accumulator;
    }

    private static XdmValue FoldRight_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var func = (FunctionItem)args[2].FunctionValue;
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
        var func = (FunctionItem)args[2].FunctionValue;
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

    private static XdmValue Not(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromBoolean(!args[0].EffectiveBooleanValue());

    private static XdmValue Position(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromInteger(ctx.ContextPosition);

    private static XdmValue Last(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromInteger(ctx.ContextSize);

    // ------------------------------------------------------------------
    // String functions
    // ------------------------------------------------------------------

    private static XdmValue StringLength_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var item = ctx.ContextItem;
        if (item.IsUndefined)
            throw new InvalidOperationException("fn:string-length() called with no context item.");
        return XdmValue.FromInteger(AtomizedString(item).Length);
    }

    private static XdmValue StringLength_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromInteger(AtomizedString(args[0]).Length);

    private static XdmValue Substring_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string s = AtomizedString(args[0]);
        double startD = ToDoubleValue(args[1]);
        if (double.IsNaN(startD)) return XdmValue.FromString(string.Empty);
        int start = (int)Math.Round(startD);
        if (start <= 0) start = 1;
        if (start > s.Length) return XdmValue.FromString(string.Empty);
        return XdmValue.FromString(s[(start - 1)..]);
    }

    private static XdmValue Substring_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string s = AtomizedString(args[0]);
        double startD = ToDoubleValue(args[1]);
        double lenD = ToDoubleValue(args[2]);
        if (double.IsNaN(startD) || double.IsNaN(lenD)) return XdmValue.FromString(string.Empty);
        int start = (int)Math.Round(startD);
        if (start <= 0) start = 1;
        if (start > s.Length) return XdmValue.FromString(string.Empty);
        int len = (int)Math.Round(lenD);
        if (len <= 0) return XdmValue.FromString(string.Empty);
        int end = Math.Min(start - 1 + len, s.Length);
        return XdmValue.FromString(s[(start - 1)..end]);
    }

    private static XdmValue SubstringBefore_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string s = AtomizedString(args[0]);
        string search = AtomizedString(args[1]);
        int idx = s.IndexOf(search, StringComparison.Ordinal);
        return XdmValue.FromString(idx >= 0 ? s[..idx] : string.Empty);
    }

    private static XdmValue SubstringBefore_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string s = AtomizedString(args[0]);
        string search = AtomizedString(args[1]);
        // Collation argument ignored for now — default to ordinal
        int idx = s.IndexOf(search, StringComparison.Ordinal);
        return XdmValue.FromString(idx >= 0 ? s[..idx] : string.Empty);
    }

    private static XdmValue SubstringAfter_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string s = AtomizedString(args[0]);
        string search = AtomizedString(args[1]);
        int idx = s.IndexOf(search, StringComparison.Ordinal);
        return XdmValue.FromString(idx >= 0 ? s[(idx + search.Length)..] : string.Empty);
    }

    private static XdmValue SubstringAfter_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string s = AtomizedString(args[0]);
        string search = AtomizedString(args[1]);
        int idx = s.IndexOf(search, StringComparison.Ordinal);
        return XdmValue.FromString(idx >= 0 ? s[(idx + search.Length)..] : string.Empty);
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
            sb.Append(char.ConvertFromUtf32(cp));
        }
        return XdmValue.FromString(sb.ToString());
    }

    private static XdmValue ParseXml_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string xml = AtomizedString(args[0]);
        if (string.IsNullOrEmpty(xml))
            throw new InvalidOperationException("fn:parse-xml argument must not be empty.");
        var doc = XDocument.Parse(xml);
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

    private static XdmValue AnalyzeString_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => AnalyzeString(AtomizedString(args[0]), AtomizedString(args[1]), string.Empty);

    private static XdmValue AnalyzeString_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => AnalyzeString(AtomizedString(args[0]), AtomizedString(args[1]), AtomizedString(args[2]));

    private static XdmValue AnalyzeString(string value, string pattern, string flags)
    {
        XNamespace fn = "http://www.w3.org/2005/xpath-functions";
        var result = new XElement(fn + "analyze-string-result");

        if (string.IsNullOrEmpty(value))
            return XdmValue.FromNode(new XDocumentNode(result));

        var options = ParseRegexFlags(flags, out bool isQuoteMode);
        if (isQuoteMode) pattern = Regex.Escape(pattern);

        var matches = Regex.Matches(value, pattern, options);
        int pos = 0;

        foreach (Match match in matches)
        {
            if (match.Index > pos)
                result.Add(new XElement(fn + "non-match", value[pos..match.Index]));

            var matchEl = new XElement(fn + "match", match.Value);
            for (int g = 1; g < match.Groups.Count; g++)
            {
                var group = match.Groups[g];
                if (group.Success)
                {
                    var groupEl = new XElement(fn + "group", group.Value);
                    groupEl.SetAttributeValue("nr", g);
                    matchEl.Add(groupEl);
                }
            }
            result.Add(matchEl);
            pos = match.Index + match.Length;
        }

        if (pos < value.Length)
            result.Add(new XElement(fn + "non-match", value[pos..]));

        return XdmValue.FromNode(new XDocumentNode(result));
    }

    private static XdmValue Contains(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromBoolean(AtomizedString(args[0]).Contains(AtomizedString(args[1])));

    private static XdmValue StartsWith(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromBoolean(AtomizedString(args[0]).StartsWith(AtomizedString(args[1])));

    private static XdmValue EndsWith(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromBoolean(AtomizedString(args[0]).EndsWith(AtomizedString(args[1])));

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
        => XdmValue.FromString(AtomizedString(args[0]).ToUpperInvariant());

    private static XdmValue LowerCase(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromString(AtomizedString(args[0]).ToLowerInvariant());

    private static XdmValue Matches_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string input = AtomizedString(args[0]);
        string pattern = AtomizedString(args[1]);
        return XdmValue.FromBoolean(Regex.IsMatch(input, pattern));
    }

    private static XdmValue Matches_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string input = AtomizedString(args[0]);
        string pattern = AtomizedString(args[1]);
        var options = ParseRegexFlags(AtomizedString(args[2]), out bool isQuoteMode);
        if (isQuoteMode) pattern = Regex.Escape(pattern);
        return XdmValue.FromBoolean(Regex.IsMatch(input, pattern, options));
    }

    private static XdmValue Replace_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string input = AtomizedString(args[0]);
        string pattern = AtomizedString(args[1]);
        string replacement = AtomizedString(args[2]);
        return XdmValue.FromString(Regex.Replace(input, pattern, replacement));
    }

    private static XdmValue Replace_4(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string input = AtomizedString(args[0]);
        string pattern = AtomizedString(args[1]);
        string replacement = AtomizedString(args[2]);
        var options = ParseRegexFlags(AtomizedString(args[3]), out bool isQuoteMode);
        if (isQuoteMode) pattern = Regex.Escape(pattern);
        return XdmValue.FromString(Regex.Replace(input, pattern, replacement, options));
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

        var options = ParseRegexFlags(flags, out bool isQuoteMode);
        if (isQuoteMode) pattern = Regex.Escape(pattern);

        if (Regex.IsMatch(string.Empty, pattern, options))
            throw new InvalidOperationException("fn:tokenize: pattern must not match the empty string");

        var parts = Regex.Split(input, pattern, options);
        var result = new List<XdmValue>();

        // Strip leading empty strings
        int start = 0;
        while (start < parts.Length && parts[start].Length == 0)
            start++;

        // Strip trailing empty strings
        int end = parts.Length;
        while (end > start && parts[end - 1].Length == 0)
            end--;

        for (int i = start; i < end; i++)
            result.Add(XdmValue.FromString(parts[i]));

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

    // ------------------------------------------------------------------
    // math:* functions
    // ------------------------------------------------------------------

    private static XdmValue MathPi(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromDouble(Math.PI);

    private static XdmValue MathSin(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromDouble(Math.Sin(ToDoubleValue(args[0])));

    private static XdmValue MathCos(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromDouble(Math.Cos(ToDoubleValue(args[0])));

    private static XdmValue MathTan(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromDouble(Math.Tan(ToDoubleValue(args[0])));

    private static XdmValue MathPow(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromDouble(Math.Pow(ToDoubleValue(args[0]), ToDoubleValue(args[1])));

    private static XdmValue MathSqrt(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromDouble(Math.Sqrt(ToDoubleValue(args[0])));

    private static XdmValue MathExp(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromDouble(Math.Exp(ToDoubleValue(args[0])));

    private static XdmValue MathLog(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromDouble(Math.Log(ToDoubleValue(args[0])));

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
        var uri = args[0].ToString();
        if (string.IsNullOrEmpty(uri))
            return XdmValue.Undefined;
        var node = ctx.LoadDocument(uri);
        return XdmValue.FromNode(node);
    }

    private static XdmValue Collection_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.Undefined;

    private static XdmValue Collection_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var uri = args[0].ToString();
        if (string.IsNullOrEmpty(uri))
            return XdmValue.Undefined;

        if (System.IO.Directory.Exists(uri))
        {
            var files = System.IO.Directory.GetFiles(uri, "*.xml");
            var nodes = new List<XdmValue>(files.Length);
            foreach (var file in files)
            {
                nodes.Add(XdmValue.FromNode(ctx.LoadDocument(file)));
            }
            return XdmValue.FromSequence(MaterializedSequence.FromList(nodes));
        }

        return XdmValue.Undefined;
    }

    private static XdmValue Error_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => throw new InvalidOperationException("fn:error() called");

    private static XdmValue Error_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => throw new InvalidOperationException($"fn:error({args[0].QNameValue}) called");

    private static XdmValue Error_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => throw new InvalidOperationException($"fn:error({args[0].QNameValue}): {args[1]}");

    private static XdmValue Error_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => throw new InvalidOperationException($"fn:error({args[0].QNameValue}): {args[1]}");

    private static RegexOptions ParseRegexFlags(string flags, out bool isQuoteMode)
    {
        var options = RegexOptions.None;
        isQuoteMode = false;
        foreach (char c in flags)
        {
            switch (c)
            {
                case 'i': options |= RegexOptions.IgnoreCase; break;
                case 'm': options |= RegexOptions.Multiline; break;
                case 's': options |= RegexOptions.Singleline; break;
                case 'x': options |= RegexOptions.IgnorePatternWhitespace; break;
                case 'q': isQuoteMode = true; break;
                default: throw new InvalidOperationException($"Unknown regex flag: '{c}'");
            }
        }
        return options;
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
        long pos = args[1].IntegerValue;
        if (pos >= 1 && pos <= target.Count)
            target.RemoveAt((int)pos - 1);
        return XdmValue.FromSequence(MaterializedSequence.FromList(target));
    }

    private static XdmValue Reverse(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var items = Materialize(args[0]);
        items.Reverse();
        return XdmValue.FromSequence(MaterializedSequence.FromList(items));
    }

    private static XdmValue Subsequence_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var items = Materialize(args[0]);
        double startD = ToDoubleValue(args[1]);
        if (double.IsNaN(startD)) return XdmValue.Undefined;
        int start = (int)Math.Round(startD);
        if (start < 1) start = 1;
        if (start > items.Count) return XdmValue.Undefined;
        return XdmValue.FromSequence(MaterializedSequence.FromList(items.Skip(start - 1).ToList()));
    }

    private static XdmValue Subsequence_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var items = Materialize(args[0]);
        double startD = ToDoubleValue(args[1]);
        double lenD = ToDoubleValue(args[2]);
        if (double.IsNaN(startD) || double.IsNaN(lenD)) return XdmValue.Undefined;
        int start = (int)Math.Round(startD);
        int len = (int)Math.Round(lenD);
        if (start < 1) start = 1;
        if (start > items.Count || len <= 0) return XdmValue.Undefined;
        int count = Math.Min(len, items.Count - start + 1);
        return XdmValue.FromSequence(MaterializedSequence.FromList(items.Skip(start - 1).Take(count).ToList()));
    }

    private static XdmValue DistinctValues_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var items = Materialize(args[0]);
        var seen = new HashSet<string>();
        var result = new List<XdmValue>();
        foreach (var item in items)
        {
            string key = AtomizedString(item);
            if (seen.Add(key))
                result.Add(item);
        }
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static XdmValue DistinctValues_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => DistinctValues_1(ctx, args);

    private static XdmValue IndexOf_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var seq = Materialize(args[0]);
        string search = AtomizedString(args[1]);
        var result = new List<XdmValue>();
        for (int i = 0; i < seq.Count; i++)
        {
            if (AtomizedString(seq[i]) == search)
                result.Add(XdmValue.FromInteger(i + 1));
        }
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static XdmValue IndexOf_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => IndexOf_2(ctx, args);

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
        var total = Sum(items);
        if (total.Kind == XdmValueKind.Decimal)
            return XdmValue.FromDecimal(total.DecimalValue / items.Count);
        return XdmValue.FromDouble(ToDoubleValue(total) / items.Count);
    }

    private static XdmValue Min_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var items = Materialize(args[0]);
        if (items.Count == 0) return XdmValue.Undefined;
        return MinMax(items, true);
    }

    private static XdmValue Min_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => Min_1(ctx, args);

    private static XdmValue Max_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var items = Materialize(args[0]);
        if (items.Count == 0) return XdmValue.Undefined;
        return MinMax(items, false);
    }

    private static XdmValue Max_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => Max_1(ctx, args);

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
        string key = AtomizedString(args[1]);
        if (map.TryGetValue(key, out var value))
            return value;
        return XdmValue.Undefined;
    }

    private static XdmValue MapSize(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromInteger(args[0].MapValue.Count);

    private static XdmValue MapContains(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromBoolean(args[0].MapValue.ContainsKey(AtomizedString(args[1])));

    private static XdmValue MapKeys(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var keys = args[0].MapValue.Keys.Select(XdmValue.FromString).ToList();
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
                foreach (var kvp in mapVal.MapValue.Values.Select((v, i) => new { v, i }))
                {
                    // We need key-value pairs, but Values doesn't give us keys
                }
            }
        }
        // Re-implement using Keys
        foreach (var mapVal in maps)
        {
            if (mapVal.IsMap)
            {
                var m = mapVal.MapValue;
                foreach (var key in m.Keys)
                    result.Add(key, m.TryGetValue(key, out var v) ? v : XdmValue.Undefined);
            }
        }
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

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static string AtomizedString(XdmValue value)
    {
        if (value.IsUndefined)
            return string.Empty;

        if (value.IsNode)
            return value.NodeValue.StringValue;

        if (value.IsSequence)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                return AtomizedString(item);
            return string.Empty;
        }

        return value.ToString();
    }

    private static XdmValue AtomizeValue(XdmValue value)
    {
        if (value.IsUndefined)
            return XdmValue.Undefined;

        if (value.IsNode)
            return XdmValue.FromString(value.NodeValue.StringValue);

        if (value.IsSequence)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                return AtomizeValue(item);
            return XdmValue.Undefined;
        }

        return value;
    }

    private static List<XdmValue> Materialize(XdmValue value)
    {
        if (value.IsUndefined)
            return new List<XdmValue>();

        if (!value.IsSequence)
            return new List<XdmValue> { value };

        var list = new List<XdmValue>();
        foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
            list.Add(item);
        return list;
    }

    private static double ToDoubleValue(XdmValue value)
    {
        value = AtomizeValue(value);
        return value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => (double)value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => value.DoubleValue,
            _ => double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : double.NaN
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

    private static XdmValue Sum(List<XdmValue> items)
    {
        bool allIntegerOrDecimal = true;
        foreach (var item in items)
        {
            var a = AtomizeValue(item);
            if (a.Kind != XdmValueKind.Integer && a.Kind != XdmValueKind.Decimal)
            {
                allIntegerOrDecimal = false;
                break;
            }
        }
        if (allIntegerOrDecimal)
        {
            decimal sum = 0m;
            foreach (var item in items)
                sum += ToDecimalValue(item);
            return XdmValue.FromDecimal(sum);
        }
        double sumD = 0.0;
        foreach (var item in items)
            sumD += ToDoubleValue(item);
        return XdmValue.FromDouble(sumD);
    }

    private static XdmValue MinMax(List<XdmValue> items, bool min)
    {
        bool allIntegerOrDecimal = true;
        foreach (var item in items)
        {
            var a = AtomizeValue(item);
            if (a.Kind != XdmValueKind.Integer && a.Kind != XdmValueKind.Decimal)
            {
                allIntegerOrDecimal = false;
                break;
            }
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
        double resultD = ToDoubleValue(items[0]);
        for (int i = 1; i < items.Count; i++)
        {
            double v = ToDoubleValue(items[i]);
            if (min ? v < resultD : v > resultD)
                resultD = v;
        }
        return XdmValue.FromDouble(resultD);
    }

    // ------------------------------------------------------------------
    // Numeric functions
    // ------------------------------------------------------------------

    private static XdmValue Abs(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        XdmValue arg = args[0];
        if (arg.IsUndefined)
            return XdmValue.FromInteger(0);

        return arg.Kind switch
        {
            XdmValueKind.Integer => XdmValue.FromInteger(Math.Abs(arg.IntegerValue)),
            XdmValueKind.Decimal => XdmValue.FromDecimal(Math.Abs(arg.DecimalValue)),
            XdmValueKind.Double => XdmValue.FromDouble(Math.Abs(arg.DoubleValue)),
            _ => XdmValue.FromDouble(Math.Abs(ToDoubleValue(arg)))
        };
    }

    private static XdmValue Floor(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        XdmValue arg = args[0];
        if (arg.IsUndefined)
            return XdmValue.FromInteger(0);

        return arg.Kind switch
        {
            XdmValueKind.Integer => arg,
            XdmValueKind.Decimal => XdmValue.FromDecimal(Math.Floor(arg.DecimalValue)),
            XdmValueKind.Double => XdmValue.FromDouble(Math.Floor(arg.DoubleValue)),
            _ => XdmValue.FromDouble(Math.Floor(ToDoubleValue(arg)))
        };
    }

    private static XdmValue Ceiling(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        XdmValue arg = args[0];
        if (arg.IsUndefined)
            return XdmValue.FromInteger(0);

        return arg.Kind switch
        {
            XdmValueKind.Integer => arg,
            XdmValueKind.Decimal => XdmValue.FromDecimal(Math.Ceiling(arg.DecimalValue)),
            XdmValueKind.Double => XdmValue.FromDouble(Math.Ceiling(arg.DoubleValue)),
            _ => XdmValue.FromDouble(Math.Ceiling(ToDoubleValue(arg)))
        };
    }

    private static XdmValue Round_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => Round(ctx, args[0], 0);

    private static XdmValue Round_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => Round(ctx, args[0], args[1].IntegerValue);

    private static XdmValue Round(EvaluationContext ctx, XdmValue arg, long precision)
    {
        if (arg.IsUndefined)
            return XdmValue.FromInteger(0);

        if (precision >= 0)
        {
            double factor = Math.Pow(10.0, precision);
            return arg.Kind switch
            {
                XdmValueKind.Integer => arg,
                XdmValueKind.Decimal =>
                    XdmValue.FromDecimal((decimal)(Math.Round((double)arg.DecimalValue * factor, MidpointRounding.AwayFromZero) / factor)),
                XdmValueKind.Double =>
                    XdmValue.FromDouble(Math.Round(arg.DoubleValue * factor, MidpointRounding.AwayFromZero) / factor),
                _ => XdmValue.FromDouble(Math.Round(ToDoubleValue(arg) * factor, MidpointRounding.AwayFromZero) / factor)
            };
        }
        else
        {
            double factor = Math.Pow(10.0, -precision);
            return arg.Kind switch
            {
                XdmValueKind.Integer =>
                    XdmValue.FromInteger((long)(Math.Round(arg.IntegerValue / factor, MidpointRounding.AwayFromZero) * factor)),
                XdmValueKind.Decimal =>
                    XdmValue.FromDecimal((decimal)(Math.Round((double)arg.DecimalValue / factor, MidpointRounding.AwayFromZero) * factor)),
                XdmValueKind.Double =>
                    XdmValue.FromDouble(Math.Round(arg.DoubleValue / factor, MidpointRounding.AwayFromZero) * factor),
                _ => XdmValue.FromDouble(Math.Round(ToDoubleValue(arg) / factor, MidpointRounding.AwayFromZero) * factor)
            };
        }
    }

    private static XdmValue RoundHalfToEven_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => RoundHalfToEven(ctx, args[0], 0);

    private static XdmValue RoundHalfToEven_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => RoundHalfToEven(ctx, args[0], args[1].IntegerValue);

    private static XdmValue RoundHalfToEven(EvaluationContext ctx, XdmValue arg, long precision)
    {
        if (arg.IsUndefined)
            return XdmValue.FromInteger(0);

        if (precision >= 0)
        {
            double factor = Math.Pow(10.0, precision);
            return arg.Kind switch
            {
                XdmValueKind.Integer => arg,
                XdmValueKind.Decimal =>
                    XdmValue.FromDecimal((decimal)(Math.Round((double)arg.DecimalValue * factor, MidpointRounding.ToEven) / factor)),
                XdmValueKind.Double =>
                    XdmValue.FromDouble(Math.Round(arg.DoubleValue * factor, MidpointRounding.ToEven) / factor),
                _ => XdmValue.FromDouble(Math.Round(ToDoubleValue(arg) * factor, MidpointRounding.ToEven) / factor)
            };
        }
        else
        {
            double factor = Math.Pow(10.0, -precision);
            return arg.Kind switch
            {
                XdmValueKind.Integer =>
                    XdmValue.FromInteger((long)(Math.Round(arg.IntegerValue / factor, MidpointRounding.ToEven) * factor)),
                XdmValueKind.Decimal =>
                    XdmValue.FromDecimal((decimal)(Math.Round((double)arg.DecimalValue / factor, MidpointRounding.ToEven) * factor)),
                XdmValueKind.Double =>
                    XdmValue.FromDouble(Math.Round(arg.DoubleValue / factor, MidpointRounding.ToEven) * factor),
                _ => XdmValue.FromDouble(Math.Round(ToDoubleValue(arg) / factor, MidpointRounding.ToEven) * factor)
            };
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

    // ------------------------------------------------------------------
    // Date / Time functions
    // ------------------------------------------------------------------

    private static XdmValue CurrentDateTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromDateTime(DateTimeOffset.Now);

    private static XdmValue CurrentDate(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var now = DateTimeOffset.Now;
        return XdmValue.FromDate(new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset));
    }

    private static XdmValue CurrentTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var now = DateTimeOffset.Now;
        return XdmValue.FromTime(new DateTimeOffset(1, 1, 1, now.Hour, now.Minute, now.Second, now.Offset));
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
        return XdmValue.FromQName(new XsQName(node.LocalName, node.NamespaceUri));
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

        if (value.IsNode)
            return XdmValue.FromString(value.NodeValue.StringValue);

        if (!value.IsSequence)
            return value;

        var seq = value.SequenceValue;
        if (seq is null)
            return XdmValue.Undefined;

        var items = new List<XdmValue>();
        foreach (var item in XdmSequence.FromSource(seq))
        {
            var atomized = Data(item);
            if (!atomized.IsUndefined)
                items.Add(atomized);
        }

        if (items.Count == 0)
            return XdmValue.Undefined;
        if (items.Count == 1)
            return items[0];
        return XdmValue.FromSequence(MaterializedSequence.FromList(items));
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

    private static XdmValue YearFromDateTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => args[0].IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(args[0].DateTimeValue.Year);

    private static XdmValue MonthFromDateTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => args[0].IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(args[0].DateTimeValue.Month);

    private static XdmValue DayFromDateTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => args[0].IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(args[0].DateTimeValue.Day);

    private static XdmValue HoursFromDateTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => args[0].IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(args[0].DateTimeValue.Hour);

    private static XdmValue MinutesFromDateTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => args[0].IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(args[0].DateTimeValue.Minute);

    private static XdmValue SecondsFromDateTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (args[0].IsUndefined) return XdmValue.Undefined;
        var dto = args[0].DateTimeValue;
        return XdmValue.FromDecimal(dto.Second + dto.Millisecond / 1000.0m + dto.Microsecond / 1_000_000.0m + dto.Nanosecond / 1_000_000_000.0m);
    }

    private static XdmValue YearFromDate(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => args[0].IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(args[0].DateValue.Year);

    private static XdmValue MonthFromDate(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => args[0].IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(args[0].DateValue.Month);

    private static XdmValue DayFromDate(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => args[0].IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(args[0].DateValue.Day);

    private static XdmValue HoursFromTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => args[0].IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(args[0].TimeValue.Hour);

    private static XdmValue MinutesFromTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => args[0].IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(args[0].TimeValue.Minute);

    private static XdmValue SecondsFromTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (args[0].IsUndefined) return XdmValue.Undefined;
        var dto = args[0].TimeValue;
        return XdmValue.FromDecimal(dto.Second + dto.Millisecond / 1000.0m + dto.Microsecond / 1_000_000.0m + dto.Nanosecond / 1_000_000_000.0m);
    }

    // ------------------------------------------------------------------
    // fn:deep-equal / fn:generate-id / fn:compare
    // ------------------------------------------------------------------

    private static XdmValue DeepEqual_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => DeepEqual(args[0], args[1]);

    private static XdmValue DeepEqual_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => DeepEqual(args[0], args[1]);

    private static XdmValue DeepEqual(XdmValue a, XdmValue b)
    {
        var itemsA = ToItemList(a);
        var itemsB = ToItemList(b);
        if (itemsA.Count != itemsB.Count)
            return XdmValue.False;
        for (int i = 0; i < itemsA.Count; i++)
        {
            if (!DeepEqualItem(itemsA[i], itemsB[i]))
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

    private static bool DeepEqualItem(XdmValue a, XdmValue b)
    {
        if (a.Kind != b.Kind)
            return false;
        return a.Kind switch
        {
            XdmValueKind.Undefined => true,
            XdmValueKind.Boolean => a.BooleanValue == b.BooleanValue,
            XdmValueKind.Integer => a.IntegerValue == b.IntegerValue,
            XdmValueKind.Decimal => a.DecimalValue == b.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => a.DoubleValue == b.DoubleValue,
            XdmValueKind.String => a.StringValue == b.StringValue,
            XdmValueKind.DateTime => a.DateTimeValue == b.DateTimeValue,
            XdmValueKind.Date => a.DateValue == b.DateValue,
            XdmValueKind.Time => a.TimeValue == b.TimeValue,
            XdmValueKind.QName => a.QNameValue.Equals(b.QNameValue),
            XdmValueKind.Node => DeepEqualNode(a.NodeValue, b.NodeValue),
            XdmValueKind.Map => DeepEqualMap(a.MapValue, b.MapValue),
            XdmValueKind.Array => DeepEqualArray(a.ArrayValue, b.ArrayValue),
            _ => false
        };
    }

    private static bool DeepEqualNode(IXdmNode a, IXdmNode b)
    {
        if (a.NodeKind != b.NodeKind)
            return false;
        if (a.LocalName != b.LocalName)
            return false;
        if (a.NamespaceUri != b.NamespaceUri)
            return false;
        if (a.StringValue != b.StringValue)
            return false;

        if (a.NodeKind == XdmNodeKind.Element)
        {
            var attrsA = SortNodes(a.Attributes());
            var attrsB = SortNodes(b.Attributes());
            if (attrsA.Count != attrsB.Count)
                return false;
            for (int i = 0; i < attrsA.Count; i++)
            {
                if (!DeepEqualNode(attrsA[i].NodeValue, attrsB[i].NodeValue))
                    return false;
            }

            var childrenA = ToNodeList(a.Children());
            var childrenB = ToNodeList(b.Children());
            if (childrenA.Count != childrenB.Count)
                return false;
            for (int i = 0; i < childrenA.Count; i++)
            {
                if (!DeepEqualNode(childrenA[i], childrenB[i]))
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

    private static bool DeepEqualMap(XdmMap a, XdmMap b)
    {
        if (a.Count != b.Count)
            return false;
        foreach (var key in a.Keys)
        {
            if (!a.TryGetValue(key, out var av))
                return false;
            if (!b.TryGetValue(key, out var bv))
                return false;
            if (!DeepEqualItem(av, bv))
                return false;
        }
        return true;
    }

    private static bool DeepEqualArray(XdmArray a, XdmArray b)
    {
        if (a.Count != b.Count)
            return false;
        var av = a.Values.ToList();
        var bv = b.Values.ToList();
        for (int i = 0; i < av.Count; i++)
        {
            if (!DeepEqualItem(av[i], bv[i]))
                return false;
        }
        return true;
    }

    private static long _generateIdCounter;
    private static readonly ConditionalWeakTable<IXdmNode, string> _generateIdMap = new();

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
        if (_generateIdMap.TryGetValue(node, out var id))
            return id;
        id = "id" + Interlocked.Increment(ref _generateIdCounter);
        _generateIdMap.AddOrUpdate(node, id);
        return id;
    }

    private static XdmValue Compare_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => Compare(args[0], args[1]);

    private static XdmValue Compare_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => Compare(args[0], args[1]);

    private static XdmValue Compare(XdmValue a, XdmValue b)
    {
        var s1 = AtomizedString(a);
        var s2 = AtomizedString(b);
        int cmp = string.CompareOrdinal(s1, s2);
        return XdmValue.FromInteger(cmp < 0 ? -1 : cmp > 0 ? 1 : 0);
    }

    // ------------------------------------------------------------------
    // URI encoding functions
    // ------------------------------------------------------------------

    private static XdmValue EncodeForUri(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var s = AtomizedString(args[0]);
        return XdmValue.FromString(Uri.EscapeDataString(s));
    }

    private static XdmValue IriToUri(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var s = AtomizedString(args[0]);
        var sb = new StringBuilder();
        foreach (char c in s)
        {
            if (IsUriChar(c))
                sb.Append(c);
            else
                AppendPercentEncoded(sb, c);
        }
        return XdmValue.FromString(sb.ToString());
    }

    private static XdmValue EscapeHtmlUri(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var s = AtomizedString(args[0]);
        var sb = new StringBuilder();
        foreach (char c in s)
        {
            if (c >= 0x20 && c <= 0x7E)
                sb.Append(c);
            else
                AppendPercentEncoded(sb, c);
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

    private static void AppendPercentEncoded(StringBuilder sb, char c)
    {
        foreach (byte b in Encoding.UTF8.GetBytes(c.ToString()))
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
        return XdmValue.FromQName(new XsQName(local, ns));
    }

    private static XdmValue ResolveQName(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var lexical = AtomizedString(args[0]);
        var node = args[1].NodeValue;

        if (string.IsNullOrEmpty(lexical))
            return XdmValue.Undefined;

        string prefix;
        string local;
        if (lexical.Contains(':'))
        {
            var idx = lexical.IndexOf(':');
            prefix = lexical[..idx];
            local = lexical[(idx + 1)..];
        }
        else
        {
            prefix = string.Empty;
            local = lexical;
        }

        var nsUri = ResolvePrefix(node, prefix);
        return XdmValue.FromQName(new XsQName(local, nsUri));
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
}

file static class Namespaces
{
    public const string Fn = "http://www.w3.org/2005/xpath-functions";
    public const string Math = "http://www.w3.org/2005/xpath-functions/math";
    public const string Map = "http://www.w3.org/2005/xpath-functions/map";
    public const string Array = "http://www.w3.org/2005/xpath-functions/array";
    public const string Xs = "http://www.w3.org/2001/XMLSchema";
}



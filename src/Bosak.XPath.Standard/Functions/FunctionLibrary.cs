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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Frozen;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Bosak.XPath.Core.Xdm;
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
        => XdmValue.FromBoolean(Regex.IsMatch(AtomizedString(args[0]), AtomizedString(args[1])));

    private static XdmValue Matches_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string input = AtomizedString(args[0]);
        string pattern = AtomizedString(args[1]);
        var options = ParseRegexFlags(AtomizedString(args[2]));
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
        var options = ParseRegexFlags(AtomizedString(args[3]));
        return XdmValue.FromString(Regex.Replace(input, pattern, replacement, options));
    }

    private static XdmValue Tokenize_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string input = AtomizedString(args[0]);
        string pattern = AtomizedString(args[1]);
        var tokens = Regex.Split(input, pattern)
            .Where(t => !string.IsNullOrEmpty(t))
            .Select(XdmValue.FromString)
            .ToList();
        return XdmValue.FromSequence(MaterializedSequence.FromList(tokens));
    }

    private static XdmValue Tokenize_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string input = AtomizedString(args[0]);
        string pattern = AtomizedString(args[1]);
        var options = ParseRegexFlags(AtomizedString(args[2]));
        var tokens = Regex.Split(input, pattern, options)
            .Where(t => !string.IsNullOrEmpty(t))
            .Select(XdmValue.FromString)
            .ToList();
        return XdmValue.FromSequence(MaterializedSequence.FromList(tokens));
    }

    private static RegexOptions ParseRegexFlags(string flags)
    {
        var options = RegexOptions.None;
        foreach (char c in flags)
        {
            switch (c)
            {
                case 'i': options |= RegexOptions.IgnoreCase; break;
                case 'm': options |= RegexOptions.Multiline; break;
                case 's': options |= RegexOptions.Singleline; break;
                case 'x': options |= RegexOptions.IgnorePatternWhitespace; break;
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
}

file static class Namespaces
{
    public const string Fn = "http://www.w3.org/2005/xpath-functions";
    public const string Math = "http://www.w3.org/2005/xpath-functions/math";
    public const string Map = "http://www.w3.org/2005/xpath-functions/map";
    public const string Array = "http://www.w3.org/2005/xpath-functions/array";
}



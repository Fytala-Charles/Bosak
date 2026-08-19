// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 18 August 2026
// PURPOSE              : Provides LSP hover information for XPath/XQuery functions and XSLT elements.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 18-08-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Bosak.LanguageServer;

/// <summary>
/// Provides hover information for XPath/XQuery function names and XSLT elements.
/// Hovering over a function shows its signature and a short description.
/// </summary>
public class HoverHandler : HoverHandlerBase
{
    private readonly DocumentManager _documents;

    /// <summary>
    /// Initializes a new instance of the <see cref="HoverHandler"/> class.
    /// </summary>
    /// <param name="documents">The document manager holding open document contents.</param>
    public HoverHandler(DocumentManager documents)
    {
        _documents = documents;
    }

    /// <summary>
    /// Returns hover information for the symbol at the cursor position.
    /// </summary>
    /// <param name="request">The hover parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The hover content, or null when there is nothing to show.</returns>
    public override Task<Hover?> Handle(HoverParams request, CancellationToken cancellationToken)
    {
        if (!_documents.TryGet(request.TextDocument.Uri.ToString(), out var text))
            return Task.FromResult<Hover?>(null);

        var word = GetWordAt(text, request.Position.Line, request.Position.Character);
        if (word is null)
            return Task.FromResult<Hover?>(null);

        if (!FunctionInfo.TryGet(word, out var info))
            return Task.FromResult<Hover?>(null);

        var markdown = new MarkupContent
        {
            Kind = MarkupKind.Markdown,
            Value = $"```xpath\n{info.Signature}\n```\n\n{info.Description}"
        };

        return Task.FromResult<Hover?>(new Hover { Contents = new MarkedStringsOrMarkupContent(markdown) });
    }

    /// <summary>
    /// Extracts the (possibly prefixed) name at the given position.
    /// </summary>
    internal static string? GetWordAt(string text, int line, int character)
    {
        var lines = text.Split('\n');
        if (line < 0 || line >= lines.Length)
            return null;
        var lineText = lines[line];
        if (character < 0 || character > lineText.Length)
            return null;

        int start = character;
        int end = character;
        bool IsNameChar(char c) => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.' || c == ':';
        while (start > 0 && IsNameChar(lineText[start - 1]))
            start--;
        while (end < lineText.Length && IsNameChar(lineText[end]))
            end++;
        if (start >= end)
            return null;
        var word = lineText[start..end];
        // Strip a trailing colon (e.g. hovering "fn:" in "fn:concat").
        return word.TrimEnd(':');
    }

    /// <summary>
    /// Creates the registration options used when advertising this handler to the client.
    /// </summary>
    /// <param name="capability">The client's hover capability.</param>
    /// <param name="clientCapabilities">The full client capabilities.</param>
    /// <returns>Registration options for the hover provider.</returns>
    protected override HoverRegistrationOptions CreateRegistrationOptions(
        HoverCapability capability, ClientCapabilities clientCapabilities)
    {
        return new HoverRegistrationOptions
        {
            DocumentSelector = new TextDocumentSelector(
                new TextDocumentFilter { Pattern = "**/*.xsl" },
                new TextDocumentFilter { Pattern = "**/*.xslt" },
                new TextDocumentFilter { Pattern = "**/*.xpath" }
            ),
        };
    }

    /// <summary>A hoverable function's signature and description.</summary>
    internal readonly record struct FunctionInfo(string Signature, string Description)
    {
        private static readonly Dictionary<string, FunctionInfo> ByName = BuildMap();

        /// <summary>Attempts to resolve a (possibly prefixed) function name.</summary>
        public static bool TryGet(string word, out FunctionInfo info)
        {
            // Normalize a prefix: fn:concat, map:get, array:head, math:sqrt all resolve.
            var local = word;
            var colon = word.IndexOf(':');
            if (colon >= 0)
                local = word[(colon + 1)..];
            return ByName.TryGetValue(local, out info);
        }

        private static Dictionary<string, FunctionInfo> BuildMap()
        {
            var map = new Dictionary<string, FunctionInfo>(StringComparer.Ordinal);
            void Add(string name, string signature, string description) => map[name] = new(signature, description);

            // String functions
            Add("concat", "fn:concat($arg1 as xs:anyAtomicType?, $arg2 as xs:anyAtomicType?, ...) as xs:string", "Concatenates two or more atomic values into a string.");
            Add("string-join", "fn:string-join($arg as xs:string*, $separator as xs:string := \"\") as xs:string", "Joins a sequence of strings with a separator.");
            Add("substring", "fn:substring($source as xs:string?, $start as xs:double, $length as xs:double?) as xs:string", "Returns the portion of a string beginning at $start, optionally limited to $length characters.");
            Add("substring-before", "fn:substring-before($arg1 as xs:string?, $arg2 as xs:string?) as xs:string", "Returns the part of $arg1 before the first occurrence of $arg2.");
            Add("substring-after", "fn:substring-after($arg1 as xs:string?, $arg2 as xs:string?) as xs:string", "Returns the part of $arg1 after the first occurrence of $arg2.");
            Add("contains", "fn:contains($arg1 as xs:string?, $arg2 as xs:string?) as xs:boolean", "True when $arg1 contains $arg2.");
            Add("starts-with", "fn:starts-with($arg1 as xs:string?, $arg2 as xs:string?) as xs:boolean", "True when $arg1 starts with $arg2.");
            Add("ends-with", "fn:ends-with($arg1 as xs:string?, $arg2 as xs:string?) as xs:boolean", "True when $arg1 ends with $arg2.");
            Add("string-length", "fn:string-length($arg as xs:string?) as xs:integer", "Returns the length of a string in characters.");
            Add("normalize-space", "fn:normalize-space($arg as xs:string?) as xs:string", "Strips leading/trailing whitespace and collapses internal runs to a single space.");
            Add("upper-case", "fn:upper-case($arg as xs:string?) as xs:string", "Uppercases a string.");
            Add("lower-case", "fn:lower-case($arg as xs:string?) as xs:string", "Lowercases a string.");
            Add("replace", "fn:replace($input as xs:string?, $pattern as xs:string, $replacement as xs:string) as xs:string", "Replaces substrings matching a regular expression.");
            Add("matches", "fn:matches($input as xs:string?, $pattern as xs:string) as xs:boolean", "True when the input matches the regular expression.");
            Add("tokenize", "fn:tokenize($input as xs:string?, $pattern as xs:string?) as xs:string*", "Splits a string into a sequence at matches of a pattern.");
            Add("codepoints-to-string", "fn:codepoints-to-string($arg as xs:integer*) as xs:string", "Builds a string from a sequence of Unicode code points.");
            Add("string-to-codepoints", "fn:string-to-codepoints($arg as xs:string?) as xs:integer*", "Returns the Unicode code points of a string.");

            // Numeric functions
            Add("abs", "fn:abs($arg as xs:numeric?) as xs:numeric", "Absolute value.");
            Add("ceiling", "fn:ceiling($arg as xs:numeric?) as xs:numeric", "Rounds up to the nearest integer.");
            Add("floor", "fn:floor($arg as xs:numeric?) as xs:numeric", "Rounds down to the nearest integer.");
            Add("round", "fn:round($arg as xs:numeric?, $precision as xs:integer?) as xs:numeric", "Rounds to the nearest value with the given precision.");
            Add("round-half-to-even", "fn:round-half-to-even($arg as xs:numeric?, $precision as xs:integer?) as xs:numeric", "Rounds half-to-even (banker's rounding).");
            Add("sum", "fn:sum($arg as xs:anyAtomicType*) as xs:anyAtomicType?", "Sums a sequence of numeric values.");
            Add("avg", "fn:avg($arg as xs:anyAtomicType*) as xs:anyAtomicType?", "Averages a sequence of numeric values.");
            Add("min", "fn:min($arg as xs:anyAtomicType*) as xs:anyAtomicType?", "Returns the minimum value.");
            Add("max", "fn:max($arg as xs:anyAtomicType*) as xs:anyAtomicType?", "Returns the maximum value.");
            Add("number", "fn:number($arg as xs:anyAtomicType?) as xs:double", "Converts a value to xs:double.");

            // Sequence functions
            Add("count", "fn:count($arg as item()*) as xs:integer", "Returns the number of items in a sequence.");
            Add("empty", "fn:empty($arg as item()*) as xs:boolean", "True when the sequence is empty.");
            Add("exists", "fn:exists($arg as item()*) as xs:boolean", "True when the sequence is non-empty.");
            Add("distinct-values", "fn:distinct-values($arg as xs:anyAtomicType*) as xs:anyAtomicType*", "Removes duplicate values.");
            Add("reverse", "fn:reverse($arg as item()*) as item()*", "Reverses a sequence.");
            Add("subsequence", "fn:subsequence($source as item()*, $start as xs:double, $length as xs:double?) as item()*", "Returns a contiguous subsequence.");
            Add("head", "fn:head($arg as item()*) as item()?", "Returns the first item.");
            Add("tail", "fn:tail($arg as item()*) as item()*", "Returns all but the first item.");
            Add("sort", "fn:sort($input as item()*) as item()*", "Sorts a sequence.");
            Add("index-of", "fn:index-of($seq as xs:anyAtomicType*, $search as xs:anyAtomicType) as xs:integer*", "Returns the positions of a value in a sequence.");
            Add("insert-before", "fn:insert-before($target as item()*, $position as xs:integer, $inserts as item()*) as item()*", "Inserts items into a sequence.");
            Add("remove", "fn:remove($target as item()*, $position as xs:integer) as item()*", "Removes the item at the given position.");
            Add("deep-equal", "fn:deep-equal($parameter1 as item()*, $parameter2 as item()*) as xs:boolean", "Deep comparison of two sequences.");
            Add("for-each", "fn:for-each($seq as item()*, $action as function(item()) as item()*) as item()*", "Applies a function to each item.");
            Add("filter", "fn:filter($seq as item()*, $pred as function(item()) as xs:boolean) as item()*", "Keeps items matching a predicate.");
            Add("fold-left", "fn:fold-left($seq as item()*, $zero as item()*, $f as function(item()*, item()) as item()*) as item()*", "Left fold over a sequence.");
            Add("fold-right", "fn:fold-right($seq as item()*, $zero as item()*, $f as function(item()*, item()) as item()*) as item()*", "Right fold over a sequence.");

            // Node / document functions
            Add("name", "fn:name($arg as node()?) as xs:string", "The name of a node.");
            Add("local-name", "fn:local-name($arg as node()?) as xs:string", "The local name of a node.");
            Add("namespace-uri", "fn:namespace-uri($arg as node()?) as xs:anyURI", "The namespace URI of a node.");
            Add("node-name", "fn:node-name($arg as node()?) as xs:QName?", "The QName of a node.");
            Add("string", "fn:string($arg as item()?) as xs:string", "The string value of an item.");
            Add("data", "fn:data($arg as item()*) as xs:anyAtomicType*", "The atomized (typed) value of items.");
            Add("root", "fn:root($arg as node()?) as node()?", "The root of the tree containing the node.");
            Add("doc", "fn:doc($uri as xs:string?) as document-node()?", "Loads a document from a URI.");
            Add("doc-available", "fn:doc-available($uri as xs:string?) as xs:boolean", "True when the document is available.");
            Add("collection", "fn:collection($arg as xs:string?) as item()*", "Returns a collection of documents.");
            Add("base-uri", "fn:base-uri($arg as node()?) as xs:anyURI?", "The base URI of a node.");
            Add("document-uri", "fn:document-uri($arg as node()?) as xs:anyURI?", "The document URI of a node's document.");
            Add("in-scope-prefixes", "fn:in-scope-prefixes($element as element()) as xs:string*", "The namespace prefixes in scope on an element.");
            Add("path", "fn:path($arg as node()?) as xs:string?", "A path expression locating the node.");
            Add("generate-id", "fn:generate-id($arg as node()?) as xs:string", "A unique identifier for a node.");

            // Boolean / comparison
            Add("true", "fn:true() as xs:boolean", "The boolean value true.");
            Add("false", "fn:false() as xs:boolean", "The boolean value false.");
            Add("not", "fn:not($arg as item()*) as xs:boolean", "Logical negation.");
            Add("compare", "fn:compare($comparand1 as xs:string?, $comparand2 as xs:string?) as xs:integer?", "Compares two strings (-1, 0, 1).");
            Add("codepoint-equal", "fn:codepoint-equal($comparand1 as xs:string?, $comparand2 as xs:string?) as xs:boolean?", "Compares two strings codepoint by codepoint.");

            // Date/time
            Add("current-date", "fn:current-date() as xs:date", "The current date.");
            Add("current-time", "fn:current-time() as xs:time", "The current time.");
            Add("current-dateTime", "fn:current-dateTime() as xs:dateTimeStamp", "The current date and time.");
            Add("year-from-date", "fn:year-from-date($arg as xs:date?) as xs:integer?", "The year component of a date.");
            Add("month-from-date", "fn:month-from-date($arg as xs:date?) as xs:integer?", "The month component of a date.");
            Add("day-from-date", "fn:day-from-date($arg as xs:date?) as xs:integer?", "The day component of a date.");
            Add("format-date", "fn:format-date($value as xs:date?, $picture as xs:string) as xs:string?", "Formats a date using a picture string.");
            Add("format-dateTime", "fn:format-dateTime($value as xs:dateTime?, $picture as xs:string) as xs:string?", "Formats a dateTime using a picture string.");
            Add("format-time", "fn:format-time($value as xs:time?, $picture as xs:string) as xs:string?", "Formats a time using a picture string.");
            Add("format-number", "fn:format-number($value as xs:numeric?, $picture as xs:string) as xs:string", "Formats a number using a picture string.");
            Add("format-integer", "fn:format-integer($value as xs:integer?, $picture as xs:string) as xs:string", "Formats an integer using a picture string.");

            // Higher-order / function introspection
            Add("function-name", "fn:function-name($func as function(*)) as xs:QName?", "The name of a function item.");
            Add("function-arity", "fn:function-arity($func as function(*)) as xs:integer", "The arity of a function item.");
            Add("function-lookup", "fn:function-lookup($name as xs:QName, $arity as xs:integer) as function(*)?", "Looks up a function by name and arity.");
            Add("apply", "fn:apply($function as function(*), $array as array(*)) as item()*", "Applies a function to an array of arguments.");

            // Map / array
            Add("map", "map:merge($maps as map(*)*) as map(*)", "Merges maps into one.");
            Add("get", "map:get($map as map(*), $key as xs:anyAtomicType) as item()*", "Returns the value for a key in a map.");
            Add("contains-key", "map:contains($map as map(*), $key as xs:anyAtomicType) as xs:boolean", "True when the map contains the key.");
            Add("keys", "map:keys($map as map(*)) as xs:anyAtomicType*", "The keys of a map.");
            Add("put", "map:put($map as map(*), $key as xs:anyAtomicType, $value as item()*) as map(*)", "Returns a map with the key/value added or replaced.");
            Add("remove-map", "map:remove($map as map(*), $keys as xs:anyAtomicType*) as map(*)", "Returns a map with keys removed.");
            Add("size", "array:size($array as array(*)) as xs:integer", "The number of members in an array.");
            Add("array-get", "array:get($array as array(*), $position as xs:integer) as item()*", "Returns the member at the given position.");
            Add("append", "array:append($array as array(*), $appendage as item()*) as array(*)", "Appends a member to an array.");
            Add("flatten", "array:flatten($input as item()*) as item()*", "Flattens nested arrays into a sequence.");

            // JSON
            Add("parse-json", "fn:parse-json($json-text as xs:string?) as item()?", "Parses JSON text into XDM maps/arrays.");
            Add("json-doc", "fn:json-doc($href as xs:string?) as item()?", "Loads and parses a JSON document from a URI.");
            Add("json-to-xml", "fn:json-to-xml($json-text as xs:string?) as document-node()?", "Converts JSON to an XML representation.");
            Add("xml-to-json", "fn:xml-to-json($input as node()?) as xs:string?", "Converts an XML JSON representation back to JSON text.");

            // URI / encoding
            Add("resolve-uri", "fn:resolve-uri($relative as xs:string?, $base as xs:string?) as xs:anyURI?", "Resolves a relative URI against a base.");
            Add("encode-for-uri", "fn:encode-for-uri($uri-part as xs:string?) as xs:string", "Percent-encodes a string for use in a URI.");
            Add("iri-to-uri", "fn:iri-to-uri($iri as xs:string?) as xs:string", "Converts an IRI to a URI.");
            Add("escape-html-uri", "fn:escape-html-uri($uri as xs:string?) as xs:string", "Escapes a URI for use in HTML.");
            Add("static-base-uri", "fn:static-base-uri() as xs:anyURI?", "The static base URI of the expression.");

            // Serialization / error
            Add("serialize", "fn:serialize($arg as item()*) as xs:string", "Serializes a value to a string.");
            Add("parse-xml", "fn:parse-xml($arg as xs:string?) as document-node()?", "Parses an XML string into a document node.");
            Add("parse-xml-fragment", "fn:parse-xml-fragment($arg as xs:string?) as document-node()?", "Parses an XML fragment into a document node.");
            Add("error", "fn:error($code as xs:QName?, $description as xs:string?) as none", "Raises a dynamic error.");
            Add("trace", "fn:trace($value as item()*, $label as xs:string) as item()*", "Traces a value for debugging.");
            Add("unparsed-text", "fn:unparsed-text($href as xs:string?) as xs:string?", "Reads a text resource as a string.");
            Add("unparsed-text-lines", "fn:unparsed-text-lines($href as xs:string?) as xs:string*", "Reads a text resource as lines.");

            // QName / casting
            Add("QName", "fn:QName($paramURI as xs:string?, $paramQName as xs:string) as xs:QName", "Constructs a QName from a namespace URI and lexical QName.");
            Add("resolve-QName", "fn:resolve-QName($qname as xs:string?, $element as element()) as xs:QName?", "Resolves a lexical QName against an element's namespaces.");
            Add("prefix-from-QName", "fn:prefix-from-QName($arg as xs:QName?) as xs:NCName?", "The prefix of a QName.");
            Add("local-name-from-QName", "fn:local-name-from-QName($arg as xs:QName?) as xs:NCName?", "The local name of a QName.");
            Add("namespace-uri-from-QName", "fn:namespace-uri-from-QName($arg as xs:QName?) as xs:anyURI?", "The namespace URI of a QName.");

            return map;
        }
    }
}

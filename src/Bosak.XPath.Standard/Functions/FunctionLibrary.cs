// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : The standard XPath / XQuery function library (fn, math, map, array, xs)
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
//                      | Charles Korthout | 5.74  | 27-07-2026     | fn:error throws structured XPathErrorException; FORG0003/0004/0005 codes; parse-xml(-fragment) FODC0006 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.75  | 28-07-2026     | in-scope-prefixes/namespace-uri-for-prefix skip non-propagating ancestor bindings |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.76  | 01-08-2026     | xs:QName constructor atomizes nodes, unwraps singletons, maps empty to empty       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.77  | 01-08-2026     | fn:current rejected in static (use-when) expressions — XPST0017                      |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.78  | 01-08-2026     | Declared-empty collection returns (); XSLT-mode calendar fallback [Calendar: AD]     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.79  | 01-08-2026     | fn:round exact rational scaling for extreme magnitudes (math-3701)                   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.80  | 01-08-2026     | fn:namespace-uri/fn:string take the FIRST sequence item in backwards-compat mode     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.81  | 07-08-2026     | fn:filter applies function conversion to xs:boolean; fn:parse-xml(()) returns (); namespace-uri-for-prefix returns xs:anyURI |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.82  | 14-08-2026     | fn:sort/array:sort fall back to EvaluationContext.DefaultCollation when no collation argument is supplied |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.83  | 14-08-2026     | fn:deep-equal ignores comments and processing instructions when comparing element children |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.84  | 14-08-2026     | map:merge default duplicates option is use-first (F+O 3.1 §14.5.1) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.85  | 15-08-2026     | fn:*-from-dateTime/date/time declare ParameterTypeNames so nodes are atomized |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.86  | 17-08-2026     | unparsed-text-available#2 raises XPTY0004 on empty $encoding (fn-unparsed-text-available-012) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.87  | 17-08-2026     | fn:analyze-string result element declares fn namespace explicitly (analyzeString-028) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.88  | 18-08-2026     | fn:distinct-values distinguishes XSD string type families (gYear/binary/string) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.89  | 18-08-2026     | fn:json-doc wraps DocumentLoader failures as FOUT1170 (json-doc-error-028..032)       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.90  | 18-08-2026     | fn:json-doc resolves relative URIs against base URI and reads local JSON files as text |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.91  | 18-08-2026     | UCA alternate=blanked + strength=identical uses codepoint tie-break (compare-042) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.92  | 19-08-2026     | Register schema simple-type constructor functions for user-defined types (qischema030) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.93  | 21-08-2026     | Declare ParameterTypeNames/ReturnTypeName for built-in list constructors (IDREFS/NMTOKENS/ENTITIES) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.94  | 21-08-2026     | fn:idref honors PSVI is-idrefs property for schema-validated IDREF/IDREFS nodes         |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.95  | 23-08-2026     | fn:local-name-from-QName / namespace-uri-from-QName / prefix-from-QName raise XPTY0004 on multi-item sequences |
//                      | Charles Korthout | 5.96  | 23-08-2026     | Advanced UCA collation parameters: caseFirst, numeric, backwards, caseLevel, alternate=shifted |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.97  | 23-08-2026     | UCA fallback=no rejects unsupported parameters; numeric strength mapping; substring-after numeric guard |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.98  | 23-08-2026     | UCA caseLevel secondary ordering and shifted variable tie-break sign |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.99  | 23-08-2026     | Schema-aware deep-equal, atomized string functions, list-typed fn:sum, analyze-string.xsd validation |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.100 | 26-08-2026     | fn:type-available validates its argument as an EQName and raises XTDE1428 when invalid   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.101 | 26-08-2026     | fn:current raises XTDE1360 when the current item is absent                               |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.102 | 26-08-2026     | fn:element-available validates its argument as an EQName and raises XTDE1440 when invalid |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.103 | 27-08-2026     | json-to-xml validate=true raises FOJS0004 (non-schema-aware processor)                   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.104 | 29-08-2026     | fn:unparsed-entity-uri returns xs:anyURI; unparsed entities preserved in fn:snapshot    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.105 | 31-08-2026     | json-to-xml validate=true performs schema validation against built-in JSON schema      |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.106 | 03-09-2026     | xsl:vendor-url points to the Fytala-Charles/Bosak repository (org transfer prep)        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.107 | 03-09-2026     | Warning-free build: CS8629/CS8604 null-flow guards; bHasType deep-equal typo (a->b NodeKind) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.108 | 03-09-2026     | xsl:product-version reports the assembly informational version (0.9.0-preview), stays in sync |
//                      |==================|=======|================|=========================================================================================
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
//                      | Charles Korthout | 2.2   | 15-07-2026     | fn:parse-xml-fragment now returns a document node containing all fragment children       |
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
//                      | Charles Korthout | 5.19  | 19-07-2026     | Fixed UCA collation strength mapping; HTML ASCII case-insensitive folding |
//                      | Charles Korthout | 5.19  | 26-06-2026     | Backwards-compatible argument coercion for string and node functions                   |
//                      | Charles Korthout | 5.20  | 07-07-2026     | fn:subsequence uses BC numeric coercion for start/length; fixes xpath-compat-0401     |
//                      | Charles Korthout | 5.21  | 08-07-2026     | unparsed-text encoding detection and HTTP fetch; distinct-values NaN; BC string coercion |
//                      | Charles Korthout | 5.22  | 26-06-2026     | Added xsl:accept, accumulator, accumulator-rule, fork, next-iteration, override, use-package |
//                      | Charles Korthout | 5.23  | 12-07-2026     | Fix fn:current-time DateTimeOffset underflow near midnight with positive timezone offsets. |
//                      | Charles Korthout | 5.24  | 13-07-2026     | Fallback to day 2 for current-time when positive offset pushes UTC before year 1.        |
//                      | Charles Korthout | 5.25  | 13-07-2026     | HOF: FOTY0015 deep-equal, FOTY0013 atomize, anonymous curried names, arity unwrap      |
//                      | Charles Korthout | 5.26  | 14-07-2026     | fn:concat/fn:compare#2 params now pass-through anyAtomicType; fixes HOF-064            |
//                      | Charles Korthout | 5.27  | 14-07-2026     | fn:concat registered up to arity 32 (unicode-90 concat#16); codepoints-to-string XML-char fix; Rune-based translate
//                      | Charles Korthout | 5.28  | 15-07-2026     | QT3 quick wins: XPath-whitespace normalize-space; tokenize excludes captures; translate arg-type XPTY0004
//                      | Charles Korthout | 5.29  | 15-07-2026     | fn:json-doc/unparsed-text(-available/-lines) consult ResourceUriMapper for URI->local-file mapping
//                      | Charles Korthout | 5.30  | 15-07-2026     | JSON parse failures (parse-json/json-doc/json-to-xml) now raise FOJS0001 instead of JsonException
//                      | Charles Korthout | 5.31  | 15-07-2026     | JSON BOM strip; fallback for unpaired \uXXXX surrogates; strict unparsed-text decoding (FOUT1200/1190)
//                      | Charles Korthout | 5.32  | 15-07-2026     | Spec-correct ParameterTypes for dynamic calls (fn:not, *-from-duration, adjust-*, fn:id/idref/element-with-id, map key args); implemented map:find#2; fn:load-xquery-module resolvable stub (invocation raises FOQM0001); fn:serialize sequence normalization (space-join) and empty-sequence options
//                      | Charles Korthout | 5.33  | 15-07-2026     | fn:xml-to-json: empty/single/multi-node argument handling (XPTY0004); full j:* validation (FOJS0006/FOJS0007); escaped/escaped-key unescaping; F+O escaping (solidus, C0/C1/DEL, non-BMP surrogate pairs); j:number cast to xs:double
//                      | Charles Korthout | 5.34  | 15-07-2026     | fn:min/fn:max: untypedAtomic cast to xs:double (FORG0001), NaN propagation, FORG0006 for incomparable mixes (K-SeqMAX/MINFunc)
//                      | Charles Korthout | 5.35  | 15-07-2026     | fn:parse-json rewrite: recursive-descent parser; empty input; duplicates use-last/reject + canonical-key detection; spec escape semantics (raw retention, fallback with escape-as-written, default U+FFFD); escape+fallback and json-to-xml use-last FOJS0005; deep-equal empty-sequence representation fix
//                      | Charles Korthout | 5.36  | 15-07-2026     | fn:json-to-xml: duplicates='retain' accepted and is the default (retains duplicate keys); use-last still FOJS0005
//                      | Charles Korthout | 5.37  | 15-07-2026     | fn:json-to-xml on JsonReader via JNode tree (dup-preserving): escaped/escaped-key attrs, () input, BaseUri annotation, raw j:number; escape=true now decodes quotes (json-doc-012); eager fallback/validate option validation (XPTY0004); removed System.Text.Json path
//                      | Charles Korthout | 5.29  | 15-07-2026     | fn:normalize-unicode: case-insensitive trimmed form names, empty form, FULLY-NORMALIZED; matches arg-type XPTY0004
//                      | Charles Korthout | 5.38  | 15-07-2026     | fn:serialize rewritten on XdmSerializer: map+element option forms, xml/json/adaptive methods, char maps, CDATA, indent, item-separator, SENR0001/SERE002x |
//                      | Charles Korthout | 5.39  | 15-07-2026     | map:merge duplicates option (use-first/use-last/use-any/combine/reject); map:remove multi-key; strict singleton map keys (XPTY0004); array bounds FOAY0001/FOAY0002; deep-equal map keys collation-free (QT3 Tier-2i) |
//                      | Charles Korthout | 5.40  | 15-07-2026     | Tier-2j: numeric fns (abs/floor/ceiling/round) reject non-numeric non-untypedAtomic (XPTY0004); fn:sum/fn:avg reject xs:string/xs:boolean items (FORG0006) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.41  | 15-07-2026     | Tier-2k: fn:outermost/innermost reject non-node items (XPTY0004); round/round-half-to-even keep xs:integer type for negative precision (F+O instance-of-T rule); huge-precision identity guard; fn:min/max/sum preserve integer subtype annotations (least common type) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.42  | 15-07-2026     | fn:system-property('xsl:version') honors EvaluationContext.XsltVersion override                        |
//                      | Charles Korthout | 5.43  | 20-07-2026     | xs:QName constructor uses default element namespace for unprefixed lexical QNames     |
//                      | Charles Korthout | 5.44  | 20-07-2026     | fn:current and fn:system-property raise XPST0017 outside XSLT mode                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.43  | 17-07-2026     | fn:unparsed-text/-available: resolve href against base URI before URI mapping; reject fragment identifiers |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.44  | 17-07-2026     | Persistent XdmMap backing; map:remove/map:put now O(log n) via structural sharing (op-same-key) |
//                      | Charles Korthout | 5.45  | 18-07-2026     | map:remove/map:put use XdmMap.WithAdded/WithRemoved to preserve insertion order          |
//                      | Charles Korthout | 5.46  | 18-07-2026     | fn:collection/fn:uri-collection use EvaluationContext.Collections + FODC errors         |
//                      | Charles Korthout | 5.47  | 18-07-2026     | fn:function-lookup captures EvaluationContext so context-dependent functions use it   |
//                      | Charles Korthout | 5.48  | 18-07-2026     | fn:id/fn:idref/fn:element-with-id support DTD-declared ID/IDREF and raise XPTY0004    |
//                      | Charles Korthout | 5.49  | 19-07-2026     | Tier-2u: xs:numeric cast and xs:numeric#1 constructor                                  |
//                      | Charles Korthout | 5.50  | 19-07-2026     | cbcl fixes: XML 1.0 codepoints-to-string; QName whitespace validation; current-date/time use implicit timezone; distinct-values/index-of honor implicit timezone; duration*NaN raises FOCA0005 |
//                      | Charles Korthout | 5.50  | 19-07-2026     | Tier-2w: fn:has-children context-item and singleton-sequence fixes                     |
//                      | Charles Korthout | 5.51  | 19-07-2026     | Tier-2y: fn:index-of uses eq semantics, validates single search/collation, NaN-safe    |
//                      | Charles Korthout | 5.52  | 19-07-2026     | fn:id/fn:element-with-id use IsId and support schema-validated element/attribute IDs    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.53  | 19-07-2026     | FunctionLibrary.Populate sets default CollationComparer for XPath value comparisons     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.55  | 19-07-2026     | fn:compare now validates arguments with RequireString (XPTY0004 for non-string atomics) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.55  | 19-07-2026     | fn:iri-to-uri now validates argument with RequireString (XPTY0004 for non-string/many items) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.56  | 19-07-2026     | fn:substring-before/after resolve relative collation URIs against EvaluationContext.BaseUri |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.57  | 19-07-2026     | fn:doc/fn:doc-available now validate URI argument with RequireString (XPTY0004)          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.58  | 21-08-2026     | fn:function-lookup captures in-scope namespace bindings for dynamic constructor calls   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.59  | 26-08-2026     | fn:system-property validates the argument as a QName and raises XTDE1390 when invalid   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.53  | 19-07-2026     | fn:format-number passes BackwardsCompatible to FormatNumberEngine                            
//                      | Charles Korthout | 5.54  | 19-07-2026     | fn:zero-or-one returns the single item when given a one-item sequence            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.55  | 19-07-2026     | Tier-2z: fn:root/fn:name/fn:local-name context-item checks; fn:QName/xs:QName empty-prefix and empty-sequence fixes |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.56  | 19-07-2026     | Tier-2z: fn:lang context-item and node-arg type checks; fn:in-scope-prefixes element-node validation; documented XML 1.0 codepoints-to-string skips |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.57  | 19-07-2026     | Tier-2z: fn:string-length#0 uses fn:string(.) semantics; type checks for string-join/string-to-codepoints/replace/remove |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.58  | 19-07-2026     | Tier-2z: adjust-*-to-timezone validate target offset range and minute resolution (FODT0003) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.59  | 19-07-2026     | Tier-2z: distinct-values/index-of duration equality; generic xs:duration component extraction |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.60  | 20-07-2026     | Tier-2z: fn:number#0 raises XPDY0002 when called with no context item                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.61  | 20-07-2026     | Tier-2z: fn:upper-case Armenian ligature men xeh (U+FB17) → U+0544 U+053D                |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.62  | 20-07-2026     | Tier-2z: fn:data() raises FOTY0012 for complex element-only/empty schema elements      |
//                      | Charles Korthout | 5.63  | 20-07-2026     | Tier-2z: fn:deep-equal timezone-aware dateTime/date/time comparison                  |
//                      | Charles Korthout | 5.64  | 20-07-2026     | Tier-2z: fn:number returns NaN for non-numeric/non-string atomic types               |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.65  | 21-07-2026     | Static codepoints-to-string declares xs:integer* parameter for function conversion       |
//                      | Charles Korthout | 5.66  | 21-07-2026     | fn:resolve-uri rejects base URIs with fragments and relative refs with colon in first segment |
//                      | Charles Korthout | 5.68  | 21-07-2026     | Registered xs:dateTimeStamp#1 constructor and added TypeAvailable entry                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.69  | 21-07-2026     | fn:*-from-time use XPathDateTime to avoid DateTimeOffset out-of-range near year boundary |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.67  | 21-07-2026     | year/month-from-dateTime use XPathDateTime to support extended years (fn-*-from-dateTime-6) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.70  | 25-07-2026     | Variadic fn:concat registration; g* date equality in distinct-values/deep-equal       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.71  | 25-07-2026     | distinct-values returns atomized values; day-from-dateTime parameter conversion       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.72  | 25-07-2026     | fn:document-uri returns an xs:anyURI-annotated value (K2-DocumentURIFunc-11)            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.73  | 25-07-2026     | fn:serialize honors static output parameters and parameter-document                     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.74  | 29-07-2026     | fn:id/idref/element-with-id require a document-rooted tree (FODC0001) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.75  | 29-07-2026     | deep-equal compares map values and array members with sequence semantics |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.76  | 29-07-2026     | fn:data returns xs:string for namespace nodes (XDM typed value) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.77  | 29-07-2026     | fn:round-half-to-even coerces untypedAtomic to double (hof-043) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.78  | 29-07-2026     | min/max booleans and FORG0006; fn:error empty arg; generate-id checks; base-uri static fallback |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.79  | 03-08-2026     | fn:path keeps steps below a parentless root (Q{...}root()/...) and indexes text/comment/PI siblings |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.80  | 03-08-2026     | fn:xml-to-json rejects documents with more than one element child (FOJS0006) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.81  | 03-08-2026     | fn:snapshot#0 (context-item form; square-array-010/018/019/117/118) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.82  | 07-08-2026     | fn:xml-to-json: indent option pretty-prints; @escaped allowed on any j:* element (bug 29917); escaped content copies valid escapes unchanged (xml-to-json-057/065/071) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.83  | 07-08-2026     | fn:json-doc(())/fn:unparsed-text(-lines)(()) return (), unparsed-text-available(()) false; fn:resolve-uri keeps literal IRI characters (fn-resolve-uri-30) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.84  | 07-08-2026     | Argument type strictness: fn:concat rejects multi-item arguments (XPTY0004, K2-ConcatFunc); fn:collation-key and fn:unparsed-text-available require string-typed arguments (XPTY0004); fn:data#0 raises XPDY0002 with no context item (K2-DataFunc-4) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.85  | 15-08-2026     | fn:collection/fn:uri-collection honor EvaluationContext.CollectionValues for query-based collections |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.87  | 17-08-2026     | fn:analyze-string result element declares fn namespace explicitly (analyzeString-028) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.88  | 21-08-2026     | fn:nilled honors PSVI nilled status; fn:data returns PSVI typed value for schema-validated nodes |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.89  | 21-08-2026     | fn:json-to-xml validate=true performs schema validation against built-in JSON schema   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.90  | 28-08-2026     | fn:collection/fn:uri-collection support fragment identifiers and ?select= query params |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.91  | 01-09-2026     | fn:function-lookup skips signatures hidden via IsHiddenFromFunctionLookup (xsl:original) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.92  | 02-09-2026     | XPTY0004 for non-boolean liberal/escape/indent JSON options; fn:resolve-QName validates  |
//                      |                  |       |                | the element argument's cardinality/kind (REQ-082)                                        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.93  | 02-09-2026     | load-xquery-module stub dispatches via EvaluationContext.XQueryModuleLoader (XSLT hosts) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 5.94  | 02-09-2026     | fn:function-lookup captures the resolved signature on the returned function             |
//                      |                  |       |                | item (override-f-014)                                                                   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Frozen;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
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
                ParameterTypes = [XdmValueKind.Sequence],
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
                ParameterTypes = [XdmValueKind.Undefined, XdmValueKind.Undefined],
                ReturnType = XdmValueKind.String,
                Implementation = ConcatN,
                IsVariadic = true
            },
            [(Namespaces.Fn, "concat", 3)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "concat",
                Arity = 3,
                ParameterTypes = [XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined],
                ReturnType = XdmValueKind.String,
                Implementation = ConcatN
            },
            [(Namespaces.Fn, "concat", 4)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "concat",
                Arity = 4,
                ParameterTypes = [XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined],
                ReturnType = XdmValueKind.String,
                Implementation = ConcatN
            },
            [(Namespaces.Fn, "concat", 5)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "concat",
                Arity = 5,
                ParameterTypes = [XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined],
                ReturnType = XdmValueKind.String,
                Implementation = ConcatN
            },
            [(Namespaces.Fn, "concat", 6)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "concat", Arity = 6,
                ParameterTypes = [XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined],
                ReturnType = XdmValueKind.String, Implementation = ConcatN
            },
            [(Namespaces.Fn, "concat", 7)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "concat", Arity = 7,
                ParameterTypes = [XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined],
                ReturnType = XdmValueKind.String, Implementation = ConcatN
            },
            [(Namespaces.Fn, "concat", 8)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "concat", Arity = 8,
                ParameterTypes = [XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined],
                ReturnType = XdmValueKind.String, Implementation = ConcatN
            },
            [(Namespaces.Fn, "concat", 9)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "concat", Arity = 9,
                ParameterTypes = [XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined],
                ReturnType = XdmValueKind.String, Implementation = ConcatN
            },
            [(Namespaces.Fn, "concat", 10)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "concat", Arity = 10,
                ParameterTypes = [XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined],
                ReturnType = XdmValueKind.String, Implementation = ConcatN
            },
            [(Namespaces.Fn, "concat", 11)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "concat", Arity = 11,
                ParameterTypes = [XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined],
                ReturnType = XdmValueKind.String, Implementation = ConcatN
            },
            [(Namespaces.Fn, "concat", 12)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "concat", Arity = 12,
                ParameterTypes = [XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined],
                ReturnType = XdmValueKind.String, Implementation = ConcatN
            },
            [(Namespaces.Fn, "concat", 13)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "concat", Arity = 13,
                ParameterTypes = [XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined, XdmValueKind.Undefined],
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
                ParameterTypeNames = ["xs:integer*"],
                ReturnType = XdmValueKind.String,
                ReturnTypeName = "xs:string",
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
                ParameterTypes = [XdmValueKind.Map, XdmValueKind.Undefined],
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
                ParameterTypes = [XdmValueKind.Map, XdmValueKind.Undefined],
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
                ParameterTypes = [XdmValueKind.Map, XdmValueKind.Undefined],
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

            // ----- map:find ---------------------------------------------------
            [(Namespaces.Map, "find", 2)] = new()
            {
                NamespaceUri = Namespaces.Map, LocalName = "find", Arity = 2,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Array,
                Implementation = MapFind
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
                ParameterTypeNames = ["xs:numeric?"],
                ReturnTypeName = "xs:numeric?",
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
                ParameterTypeNames = ["xs:numeric?"],
                ReturnTypeName = "xs:numeric?",
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
                ParameterTypeNames = ["xs:numeric?"],
                ReturnTypeName = "xs:numeric?",
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
                ParameterTypeNames = ["node()?"],
                ReturnTypeName = "xs:string",
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
                ParameterTypes = [XdmValueKind.Date, XdmValueKind.Duration], ReturnType = XdmValueKind.Date,
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
                ParameterTypes = [XdmValueKind.Time, XdmValueKind.Duration], ReturnType = XdmValueKind.Time,
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
                ParameterTypes = [XdmValueKind.DateTime, XdmValueKind.Duration], ReturnType = XdmValueKind.DateTime,
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
                ParameterTypeNames = ["xs:dateTime?"],
                ReturnType = XdmValueKind.Integer,
                Implementation = YearFromDateTime
            },
            [(Namespaces.Fn, "month-from-dateTime", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "month-from-dateTime",
                Arity = 1,
                ParameterTypes = [XdmValueKind.DateTime],
                ParameterTypeNames = ["xs:dateTime?"],
                ReturnType = XdmValueKind.Integer,
                Implementation = MonthFromDateTime
            },
            [(Namespaces.Fn, "day-from-dateTime", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "day-from-dateTime",
                Arity = 1,
                ParameterTypes = [XdmValueKind.DateTime],
                ParameterTypeNames = ["xs:dateTime?"],
                ReturnType = XdmValueKind.Integer,
                Implementation = DayFromDateTime
            },
            [(Namespaces.Fn, "hours-from-dateTime", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "hours-from-dateTime",
                Arity = 1,
                ParameterTypes = [XdmValueKind.DateTime],
                ParameterTypeNames = ["xs:dateTime?"],
                ReturnType = XdmValueKind.Integer,
                Implementation = HoursFromDateTime
            },
            [(Namespaces.Fn, "minutes-from-dateTime", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "minutes-from-dateTime",
                Arity = 1,
                ParameterTypes = [XdmValueKind.DateTime],
                ParameterTypeNames = ["xs:dateTime?"],
                ReturnType = XdmValueKind.Integer,
                Implementation = MinutesFromDateTime
            },
            [(Namespaces.Fn, "seconds-from-dateTime", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "seconds-from-dateTime",
                Arity = 1,
                ParameterTypes = [XdmValueKind.DateTime],
                ParameterTypeNames = ["xs:dateTime?"],
                ReturnType = XdmValueKind.Decimal,
                Implementation = SecondsFromDateTime
            },
            [(Namespaces.Fn, "timezone-from-dateTime", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "timezone-from-dateTime",
                Arity = 1,
                ParameterTypes = [XdmValueKind.DateTime],
                ParameterTypeNames = ["xs:dateTime?"],
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
                ParameterTypeNames = ["xs:date?"],
                ReturnType = XdmValueKind.Integer,
                Implementation = YearFromDate
            },
            [(Namespaces.Fn, "month-from-date", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "month-from-date",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Date],
                ParameterTypeNames = ["xs:date?"],
                ReturnType = XdmValueKind.Integer,
                Implementation = MonthFromDate
            },
            [(Namespaces.Fn, "day-from-date", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "day-from-date",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Date],
                ParameterTypeNames = ["xs:date?"],
                ReturnType = XdmValueKind.Integer,
                Implementation = DayFromDate
            },
            [(Namespaces.Fn, "timezone-from-date", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "timezone-from-date",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Date],
                ParameterTypeNames = ["xs:date?"],
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
                ParameterTypeNames = ["xs:time?"],
                ReturnType = XdmValueKind.Integer,
                Implementation = HoursFromTime
            },
            [(Namespaces.Fn, "minutes-from-time", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "minutes-from-time",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Time],
                ParameterTypeNames = ["xs:time?"],
                ReturnType = XdmValueKind.Integer,
                Implementation = MinutesFromTime
            },
            [(Namespaces.Fn, "seconds-from-time", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "seconds-from-time",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Time],
                ParameterTypeNames = ["xs:time?"],
                ReturnType = XdmValueKind.Decimal,
                Implementation = SecondsFromTime
            },
            [(Namespaces.Fn, "timezone-from-time", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "timezone-from-time",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Time],
                ParameterTypeNames = ["xs:time?"],
                ReturnType = XdmValueKind.Duration,
                Implementation = TimezoneFromTime
            },

            // ----- fn:*-from-duration -----------------------------------------
            [(Namespaces.Fn, "years-from-duration", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "years-from-duration", Arity = 1,
                ParameterTypes = [XdmValueKind.Duration], ReturnType = XdmValueKind.Integer,
                Implementation = YearsFromDuration
            },
            [(Namespaces.Fn, "months-from-duration", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "months-from-duration", Arity = 1,
                ParameterTypes = [XdmValueKind.Duration], ReturnType = XdmValueKind.Integer,
                Implementation = MonthsFromDuration
            },
            [(Namespaces.Fn, "days-from-duration", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "days-from-duration", Arity = 1,
                ParameterTypes = [XdmValueKind.Duration], ReturnType = XdmValueKind.Integer,
                Implementation = DaysFromDuration
            },
            [(Namespaces.Fn, "hours-from-duration", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "hours-from-duration", Arity = 1,
                ParameterTypes = [XdmValueKind.Duration], ReturnType = XdmValueKind.Integer,
                Implementation = HoursFromDuration
            },
            [(Namespaces.Fn, "minutes-from-duration", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "minutes-from-duration", Arity = 1,
                ParameterTypes = [XdmValueKind.Duration], ReturnType = XdmValueKind.Integer,
                Implementation = MinutesFromDuration
            },
            [(Namespaces.Fn, "seconds-from-duration", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "seconds-from-duration", Arity = 1,
                ParameterTypes = [XdmValueKind.Duration], ReturnType = XdmValueKind.Decimal,
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
                ParameterTypes = [XdmValueKind.Undefined, XdmValueKind.Undefined],
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
            [(Namespaces.Fn, "snapshot", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "snapshot", Arity = 0,
                ParameterTypes = [],
                ReturnType = XdmValueKind.Node,
                Implementation = Snapshot_0
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
            [(Namespaces.Xs, "numeric", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "numeric", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Double,
                ParameterTypeNames = ["xs:anyAtomicType?"], ReturnTypeName = "xs:numeric?",
                Implementation = XsNumeric
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
            [(Namespaces.Xs, "dateTimeStamp", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "dateTimeStamp", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.DateTime,
                Implementation = XsDateTimeStamp
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
                ParameterTypes = [XdmValueKind.Undefined],
                ParameterTypeNames = ["xs:anyAtomicType?"],
                ReturnType = XdmValueKind.Sequence,
                ReturnTypeName = "xs:IDREF*",
                Implementation = XsIDREFS
            },
            [(Namespaces.Xs, "NMTOKENS", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "NMTOKENS", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ParameterTypeNames = ["xs:anyAtomicType?"],
                ReturnType = XdmValueKind.Sequence,
                ReturnTypeName = "xs:NMTOKEN*",
                Implementation = XsNMTOKENS
            },
            [(Namespaces.Xs, "ENTITIES", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "ENTITIES", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ParameterTypeNames = ["xs:anyAtomicType?"],
                ReturnType = XdmValueKind.Sequence,
                ReturnTypeName = "xs:ENTITY*",
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
            // ----- fn:load-xquery-module (stub: resolvable, invocation raises FOQM0001) ----
            [(Namespaces.Fn, "load-xquery-module", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "load-xquery-module", Arity = 1,
                ParameterTypes = [XdmValueKind.String], ReturnType = XdmValueKind.Map,
                Implementation = LoadXQueryModuleStub
            },
            [(Namespaces.Fn, "load-xquery-module", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "load-xquery-module", Arity = 2,
                ParameterTypes = [XdmValueKind.String, XdmValueKind.Map], ReturnType = XdmValueKind.Map,
                Implementation = LoadXQueryModuleStub
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
                ParameterTypes = [XdmValueKind.Sequence], ReturnType = XdmValueKind.Sequence,
                Implementation = Id_1
            },
            [(Namespaces.Fn, "id", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "id", Arity = 2,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.Node], ReturnType = XdmValueKind.Sequence,
                Implementation = Id_2
            },
            [(Namespaces.Fn, "element-with-id", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "element-with-id", Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence], ReturnType = XdmValueKind.Sequence,
                Implementation = ElementWithId_1
            },
            [(Namespaces.Fn, "element-with-id", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "element-with-id", Arity = 2,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.Node], ReturnType = XdmValueKind.Sequence,
                Implementation = ElementWithId_2
            },
            [(Namespaces.Fn, "idref", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "idref", Arity = 1,
                ParameterTypes = [XdmValueKind.Sequence], ReturnType = XdmValueKind.Sequence,
                Implementation = Idref_1
            },
            [(Namespaces.Fn, "idref", 2)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "idref", Arity = 2,
                ParameterTypes = [XdmValueKind.Sequence, XdmValueKind.Node], ReturnType = XdmValueKind.Sequence,
                Implementation = Idref_2
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
            [(Namespaces.Fn, "uri-collection", 0)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "uri-collection", Arity = 0,
                ParameterTypes = [], ReturnType = XdmValueKind.Sequence,
                Implementation = UriCollection_0
            },
            [(Namespaces.Fn, "uri-collection", 1)] = new()
            {
                NamespaceUri = Namespaces.Fn, LocalName = "uri-collection", Arity = 1,
                ParameterTypes = [XdmValueKind.String], ReturnType = XdmValueKind.Sequence,
                Implementation = UriCollection_1
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
            // xs:error#1 appeared in early F&O 3.0 drafts; the W3C XSLT test suite
            // (function-1901) requires it to be reported as available. It behaves
            // exactly like fn:error#1.
            [(Namespaces.Xs, "error", 1)] = new()
            {
                NamespaceUri = Namespaces.Xs, LocalName = "error", Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined], ReturnType = XdmValueKind.Undefined,
                Implementation = XsError_1
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

            // ----- EXSLT common ------------------------------------------------
            [(Namespaces.ExsltCommon, "node-set", 1)] = new()
            {
                NamespaceUri = Namespaces.ExsltCommon,
                LocalName = "node-set",
                Arity = 1,
                ParameterTypes = [XdmValueKind.Undefined],
                ReturnType = XdmValueKind.Sequence,
                Implementation = ExsltNodeSet
            },
        };

        // fn:concat is variadic (any arity >= 2): register the higher arities not covered by
        // the explicit entries above (e.g. concat#16 used by the unicode-90 conformance tests).
        for (int concatArity = 14; concatArity <= 32; concatArity++)
        {
            functions[(Namespaces.Fn, "concat", concatArity)] = new()
            {
                NamespaceUri = Namespaces.Fn,
                LocalName = "concat",
                Arity = concatArity,
                ParameterTypes = Enumerable.Repeat(XdmValueKind.Undefined, concatArity).ToArray(),
                ReturnType = XdmValueKind.String,
                Implementation = ConcatN
            };
        }

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

        // Register constructor functions for simple types declared in imported schemas.
        // XSD-derived simple types (e.g. hat:hatsize) are available as Q{uri}local#1
        // constructors just like the built-in xs:* constructors (qischema030).
        if (context.SchemaSet is not null)
        {
            foreach (XmlSchemaType schemaType in context.SchemaSet.GlobalTypes.Values)
            {
                if (schemaType is not XmlSchemaSimpleType simpleType)
                    continue;
                if (schemaType.QualifiedName.Namespace == Namespaces.Xs)
                    continue; // Built-in xs:* constructors are already registered.
                if (string.IsNullOrEmpty(schemaType.QualifiedName.Namespace))
                    continue; // No namespace; not a valid constructor target.

                string ns = schemaType.QualifiedName.Namespace;
                string local = schemaType.QualifiedName.Name;
                if (context.TryResolveFunction(ns, local, 1, out _))
                    continue; // Already registered, e.g. from a loaded module.

                var typeName = $"Q{{{ns}}}{local}";
                context.RegisterFunction(new FunctionSignature
                {
                    NamespaceUri = ns,
                    LocalName = local,
                    Arity = 1,
                    ParameterTypes = [XdmValueKind.Undefined],
                    ParameterTypeNames = ["xs:anyAtomicType?"],
                    ReturnType = XdmValueKind.Undefined,
                    ReturnTypeName = typeName,
                    Implementation = (ctx, args) => UserDefinedTypeConstructor(ctx, args, typeName)
                });
            }
        }

        // Set up default document loader if not already configured
        if (context.DocumentLoader is null)
        {
            context.DocumentLoader = XDocumentProvider.LoadXml;
        }

        // Provide the default collation-aware string comparer used by XPath value
        // and general comparison operators. Consumers may override this after Populate.
        if (context.CollationComparer is null)
        {
            context.CollationComparer = CompareStrings;
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
            // XPath 1.0 backwards compatibility uses the string value of the first item.
            if (items.Count > 1 && !ctx.BackwardsCompatible)
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
        var funcValue = SingleFunctionItem(args[0]).FunctionValue;
        if (funcValue is NamedFunctionItem named)
            return XdmValue.FromQName(new XsQName(named.LocalName, named.NamespaceUri, GetStandardPrefix(named.NamespaceUri)));
        // Partially applied, coerced, and inline functions are anonymous: function-name
        // returns the empty sequence (higher-order-functions-069).
        return XdmValue.Undefined;
    }

    /// <summary>
    /// Unwraps a single-item sequence to the contained function item, as produced by
    /// filter expressions over function-item sequences (higher-order-functions-031).
    /// </summary>
    private static XdmValue SingleFunctionItem(XdmValue value)
    {
        if (!value.IsSequence)
            return value;
        var items = new List<XdmValue>();
        foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
            items.Add(item);
        if (items.Count != 1)
            throw new InvalidOperationException("XPTY0004: Expected a single function item");
        return items[0];
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
        var funcValue = SingleFunctionItem(args[0]).FunctionValue;
        if (funcValue is NamedFunctionItem named)
            return XdmValue.FromInteger(named.ArityValue);
        if (funcValue is InlineFunctionItem inline)
            return XdmValue.FromInteger(inline.Parameters.Count);
        if (funcValue is CurriedFunctionItem curried)
            return XdmValue.FromInteger(curried.Arity);
        if (funcValue is CoercedFunctionItem coerced)
            return XdmValue.FromInteger(coerced.ParamTypes.Count);
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
            // fn:filter converts the predicate result to xs:boolean by the function
            // conversion rules (atomize, then cast untypedAtomic) — not by effective
            // boolean value (filter-006: an element atomizing to "0" must not be kept).
            if (VmEngine.ApplyFunctionConversion(pred, "xs:boolean", ctx).BooleanValue)
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
        string? collationUri = collation is not null && !IsEmptySequence(collation.Value)
            ? collation.ToString()
            : (string.IsNullOrEmpty(ctx.DefaultCollation) ? null : ctx.DefaultCollation);
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
        var items = AsSequence(args[0]).ToList();
        if (items.Any(v => !v.IsNode))
            throw new InvalidOperationException("XPTY0004: fn:innermost() requires a sequence of nodes");
        var nodes = items.Select(v => v.NodeValue!).ToList();
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
        var items = AsSequence(args[0]).ToList();
        if (items.Any(v => !v.IsNode))
            throw new InvalidOperationException("XPTY0004: fn:outermost() requires a sequence of nodes");
        var nodes = items.Select(v => v.NodeValue!).ToList();
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

    private static XdmValue Snapshot_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        // fn:snapshot() with no argument is fn:snapshot(.); an absent context item is XPDY0002.
        var item = ctx.ContextItem;
        if (item.IsUndefined)
            throw new InvalidOperationException("XPDY0002");
        return Snapshot(ctx, new[] { item });
    }

    private static XdmValue Snapshot(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (arg.IsUndefined)
            return XdmValue.Undefined;

        // F&O: fn:snapshot returns the items of $arg in order; node items are
        // replaced by their snapshot copies, non-node items are returned unchanged.
        var items = new List<XdmValue>();
        if (arg.IsSequence && arg.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(arg.SequenceValue))
                items.Add(item);
        }
        else
        {
            items.Add(arg);
        }

        if (items.Count == 0)
            return XdmValue.Undefined;

        var copies = new List<XdmValue>();
        foreach (var item in items)
        {
            if (item.IsNode && item.NodeValue != null)
            {
                var copied = SnapshotNode(item.NodeValue);
                if (copied == null)
                    throw new InvalidOperationException("FOTY0013");
                // XSLT hook: fn:snapshot copies accumulator values onto the copy.
                ctx.AccumulatorValueCopier?.Invoke(item.NodeValue, copied);
                copies.Add(XdmValue.FromNode(copied));
            }
            else
            {
                copies.Add(item);
            }
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
                    if (!string.IsNullOrEmpty(node.BaseUri))
                        docCopy.AddAnnotation(node.BaseUri);
                    if (node is Providers.Xml.XDocumentNode srcDocNode)
                        srcDocNode.CopyUnparsedEntitiesTo(docCopy);
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
                        // The shallow-copied parent already carries this attribute;
                        // reuse it so the snapshot copy keeps its parent element.
                        if (container is XElement parentElem)
                        {
                            var existing = parentElem.Attributes()
                                .FirstOrDefault(a => a.Name == attr.Name);
                            if (existing != null)
                            {
                                targetCopy = existing;
                                break;
                            }
                        }
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
                    // The shallow-copied parent already carries the namespace declaration;
                    // reuse it so the snapshot copy keeps its parent element and its
                    // namespace-node identity.
                    if (isLast && original is Providers.Xml.XDocumentNode nsNode)
                    {
                        string prefix = nsNode.LocalName ?? string.Empty;
                        if (container is XElement parentElem)
                        {
                            var existing = parentElem.Attributes().FirstOrDefault(a =>
                                a.IsNamespaceDeclaration &&
                                (a.Name.LocalName == "xmlns" ? string.Empty : a.Name.LocalName) == prefix);
                            if (existing != null)
                            {
                                return Providers.Xml.XDocumentNode.CreateNamespaceNode(existing, parentElem);
                            }
                        }
                        if (nsNode.UnderlyingObject is XAttribute nsAttr)
                        {
                            var nsCopy = new XAttribute(XName.Get(nsAttr.Name.LocalName, nsAttr.Name.NamespaceName), nsAttr.Value);
                            if (container is XElement ownerElem)
                            {
                                ownerElem.Add(nsCopy);
                                return Providers.Xml.XDocumentNode.CreateNamespaceNode(nsCopy, ownerElem);
                            }
                            container?.Add(nsCopy);
                            targetCopy = nsCopy;
                        }
                    }
                    break;
            }
        }

        // Deep-copy the descendants of the target element. The target already carries
        // all in-scope namespace declarations, so descendants must not redeclare them.
        if (targetCopy is XElement targetElem && node.NodeKind == XdmNodeKind.Element)
        {
            if (((Providers.Xml.XDocumentNode)node).UnderlyingObject is XElement origElem)
            {
                var declared = new HashSet<string>(StringComparer.Ordinal);
                foreach (var attr in targetElem.Attributes())
                {
                    if (attr.IsNamespaceDeclaration)
                        declared.Add(attr.Name.LocalName == "xmlns" ? string.Empty : attr.Name.LocalName);
                }
                foreach (var child in origElem.Nodes())
                {
                    targetElem.Add(DeepCopyXNode(child, declared));
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

    private static XNode DeepCopyXNode(XNode node, IReadOnlySet<string>? inheritedPrefixes = null)
    {
        switch (node)
        {
            case XElement elem:
                return DeepCopyElement(elem, inheritedPrefixes);
            case XText text:
                return new XText(text.Value);
            case XComment comment:
                return new XComment(comment.Value);
            case XProcessingInstruction pi:
                return new XProcessingInstruction(pi.Target, pi.Data);
            case XDocumentType docType:
                return new XDocumentType(docType.Name, docType.PublicId, docType.SystemId, docType.InternalSubset);
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

        // RFC 3986 / XPath FO.E1: the base URI must be absolute and syntactically valid,
        // and must not contain a fragment (a fragment is not part of a base URI).
        if (!Uri.TryCreate(baseUri, UriKind.Absolute, out var baseUriObj)
            || !baseUriObj.IsAbsoluteUri
            || !Uri.IsWellFormedUriString(baseUri, UriKind.Absolute)
            || !string.IsNullOrEmpty(baseUriObj.Fragment))
        {
            throw new InvalidOperationException("FORG0002: Invalid base URI");
        }

        // RFC 3986 §4.2: a relative reference that does not begin with '/' and is not
        // path-empty must have a first path segment that does not contain ':'.
        if (!relative.StartsWith("/"))
        {
            int segmentEnd = relative.AsSpan().IndexOfAny("/?#");
            string firstSegment = segmentEnd < 0 ? relative : relative[..segmentEnd];
            if (firstSegment.Contains(':'))
                throw new InvalidOperationException("FORG0002: Invalid relative URI");
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
            // IRI support: .NET percent-encodes non-ASCII characters in AbsoluteUri, but
            // fn:resolve-uri keeps IRI characters literal (fn-resolve-uri-30). Restore the
            // literal non-ASCII characters of the inputs; percent-encodings already present
            // in the inputs stay encoded (fn-resolve-uri-31).
            resolved = RestoreLiteralIriCharacters(resolved, relative);
            resolved = RestoreLiteralIriCharacters(resolved, baseUri);
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

    /// <summary>
    /// Replaces the percent-encoded UTF-8 forms of the non-ASCII characters that appear
    /// literally in <paramref name="source"/> with the characters themselves.
    /// </summary>
    private static string RestoreLiteralIriCharacters(string resolved, string source)
    {
        for (int i = 0; i < source.Length; i++)
        {
            char c = source[i];
            if (c <= 0x7F)
                continue;
            string literal;
            if (char.IsHighSurrogate(c) && i + 1 < source.Length && char.IsLowSurrogate(source[i + 1]))
            {
                literal = source.Substring(i, 2);
                i++;
            }
            else if (char.IsSurrogate(c))
            {
                continue; // A lone surrogate is not a valid IRI character.
            }
            else
            {
                literal = c.ToString();
            }
            resolved = resolved.Replace(Uri.EscapeDataString(literal), literal);
        }
        return resolved;
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
    {
        if (!ctx.IsXsltMode)
            throw new InvalidOperationException("XPST0017: Function fn:current is available only in XSLT.");
        // fn:current is not in the static function library (XSLT 3.0 §24.1): it cannot be
        // called from use-when or other static expressions.
        if (ctx.IsStaticEvaluation)
            throw new InvalidOperationException("XPST0017: Function fn:current is not available in static expressions.");
        if (ctx.CurrentItem.IsUndefined)
            throw new InvalidOperationException("XTDE1360");
        return ctx.CurrentItem;
    }

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
            throw new InvalidOperationException("XPDY0002: fn:string-length() called with no context item.");
        // fn:string-length() is equivalent to fn:string-length(fn:string(.))
        return XdmValue.FromInteger(CountCodePoints(AtomizedString(item)));
    }

    private static XdmValue StringLength_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromInteger(CountCodePoints(RequireString(args[0], ctx.BackwardsCompatible)));

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
        string collation = ResolveCollationUri(AtomizedString(args[2]), ctx.BaseUri);
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
            if (uca.Numeric)
                throw new InvalidOperationException("FOCH0004: Numeric collation does not support substring matching");
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
        int plainIdx = StringIndexOf(s, search, collation);
        return XdmValue.FromString(plainIdx >= 0 ? s[(plainIdx + search.Length)..] : string.Empty);
    }

    private static XdmValue SubstringAfter_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string s = AtomizedString(args[0]);
        string search = AtomizedString(args[1]);
        string collation = ResolveCollationUri(AtomizedString(args[2]), ctx.BaseUri);
        ValidateCollation(collation);

        if (TryParseUca(collation, out var uca))
        {
            if (uca.Numeric)
                throw new InvalidOperationException("FOCH0004: Numeric collation does not support substring matching");
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

        int plainIdx = StringIndexOf(s, search, collation);
        return XdmValue.FromString(plainIdx >= 0 ? s[(plainIdx + search.Length)..] : string.Empty);
    }

    private static XdmValue StringToCodepoints(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string s = RequireString(args[0], ctx.BackwardsCompatible);
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
            // XML 1.1 Char production (Bosak is an XML 1.1-capable implementation):
            // #x1-#xD7FF | #xE000-#xFFFD | #x10000-#x10FFFF. Only NUL, surrogates, and
            // U+FFFE/U+FFFF are invalid; C0 controls and noncharacters such as FDD0-FDEF
            // and astral xFFFE/xFFFF are legal (FOCH0001 otherwise).
            if (cp < 1 || cp > 0x10FFFF || (cp >= 0xD800 && cp <= 0xDFFF) || cp == 0xFFFE || cp == 0xFFFF)
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
        "xsl:supports-higher-order-functions",
        "xsl:xpath-version", "xsl:xsd-version"
    ];

    private static XdmValue SystemProperty(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (!ctx.IsXsltMode)
            throw new InvalidOperationException("XPST0017: Function fn:system-property is available only in XSLT.");
        string name = AtomizedString(args[0]);
        name = ExpandXsltPropertyName(name, ctx);
        string value = name switch
        {
            "xsl:version" => ctx.XsltVersion?.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) ?? "3.0",
            "xsl:vendor" => "Bosak",
            "xsl:vendor-url" => "https://github.com/Fytala-Charles/Bosak",
            "xsl:product-name" => "Bosak XPath",
            "xsl:product-version" => typeof(FunctionLibrary).Assembly.GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.9.0-preview",
            "xsl:is-schema-aware" => "no",
            "xsl:supports-serialization" => "yes",
            "xsl:supports-backwards-compatibility" => "yes",
            "xsl:supports-namespace-axis" => "yes",
            "xsl:supports-streaming" => "no",
            "xsl:supports-dynamic-evaluation" => "yes",
            "xsl:supports-higher-order-functions" => "yes",
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

        // EQName form: Q{http://www.w3.org/1999/XSL/Transform}version
        if (name.StartsWith("Q{") || name.StartsWith("q{"))
        {
            int close = name.IndexOf('}');
            if (close <= 2)
                throw new InvalidOperationException("XTDE1390");
            var nsUri = name[2..close];
            var local = name[(close + 1)..];
            if (string.IsNullOrEmpty(local))
                throw new InvalidOperationException("XTDE1390");
            VerifyNCName(local);
            if (nsUri == Namespaces.Xsl)
                return $"xsl:{local}";
            return name;
        }

        int colon = name.IndexOf(':');
        if (colon >= 0)
        {
            var prefix = name[..colon];
            var local = name[(colon + 1)..];
            if (string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(local))
                throw new InvalidOperationException("XTDE1390");
            VerifyNCName(prefix);
            VerifyNCName(local);
            if (!ctx.TryResolveNamespace(prefix, out var nsUri))
                throw new InvalidOperationException("XTDE1390");
            if (nsUri == Namespaces.Xsl)
                return $"xsl:{local}";
            return name;
        }

        // Unprefixed name: must be a valid NCName, but no prefix resolution is required.
        VerifyNCName(name);
        return name;
    }

    private static void VerifyNCName(string name)
    {
        try
        {
            System.Xml.XmlConvert.VerifyNCName(name);
        }
        catch (System.Xml.XmlException)
        {
            throw new InvalidOperationException("XTDE1390");
        }
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
        var (nsUri, localName) = ParseTypeAvailableName(ctx, name);
        string lowerLocal = localName.ToLowerInvariant();

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
            "date", "time", "datetime", "dateTime", "datetimestamp", "dateTimeStamp", "duration", "yearmonthduration",
            "yearMonthDuration", "daytimeduration", "dayTimeDuration",
            "gday", "gmonth", "gyear", "gmonthday", "gyearmonth",
            "hexbinary", "base64binary", "anyuri", "qname", "notation",
            "normalizedstring", "token", "language", "nmtoken", "name", "ncname",
            "id", "idref", "entity", "int", "long", "short", "byte",
            "nonnegativeinteger", "positiveinteger", "unsignedlong", "unsignedint",
            "unsignedshort", "unsignedbyte", "nonpositiveinteger", "negativeinteger",
            "untyped", "anytype", "anysimpletype", "untypedatomic", "anyatomictype"
        ];
        return XdmValue.FromBoolean(builtInTypes.Contains(lowerLocal));
    }

    /// <summary>
    /// Parses the argument of <c>fn:type-available</c> as an EQName.
    /// Raises <c>XTDE1428</c> when the value is not a valid EQName or when a lexical
    /// QName prefix is not declared in the static context.
    /// </summary>
    private static (string nsUri, string localName) ParseTypeAvailableName(EvaluationContext ctx, string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new InvalidOperationException("XTDE1428");

        string nsUri;
        string localName;

        if (name.Length > 2 && name[0] == 'Q' && name[1] == '{')
        {
            int close = name.IndexOf('}');
            if (close < 2 || close == name.Length - 1)
                throw new InvalidOperationException("XTDE1428");
            nsUri = name.Substring(2, close - 2);
            if (nsUri.IndexOfAny(new[] { '{', '}' }) >= 0)
                throw new InvalidOperationException("XTDE1428");
            localName = name.Substring(close + 1);
        }
        else if (name.StartsWith("{"))
        {
            // Clark notation {uri}local is not a valid EQName for fn:type-available.
            throw new InvalidOperationException("XTDE1428");
        }
        else
        {
            int colon = name.IndexOf(':');
            if (colon >= 0)
            {
                string prefix = name.Substring(0, colon);
                localName = name.Substring(colon + 1);
                if (prefix.Length == 0)
                    throw new InvalidOperationException("XTDE1428");
                if (!ctx.TryResolveNamespace(prefix, out nsUri))
                    throw new InvalidOperationException("XTDE1428");
            }
            else
            {
                nsUri = string.Empty;
                localName = name;
            }
        }

        try
        {
            System.Xml.XmlConvert.VerifyNCName(localName);
        }
        catch
        {
            throw new InvalidOperationException("XTDE1428");
        }

        return (nsUri, localName);
    }

    /// <summary>
    /// Parses the argument of <c>fn:element-available</c> as an EQName.
    /// Raises <c>XTDE1440</c> when the value is not a valid EQName or when a lexical
    /// QName prefix is not declared in the static context.
    /// </summary>
    private static (string nsUri, string localName) ParseElementAvailableName(EvaluationContext ctx, string name, string defaultUri)
    {
        if (string.IsNullOrEmpty(name))
            throw new InvalidOperationException("XTDE1440");

        string nsUri;
        string localName;

        if (name.Length > 2 && name[0] == 'Q' && name[1] == '{')
        {
            int close = name.IndexOf('}');
            if (close < 2 || close == name.Length - 1)
                throw new InvalidOperationException("XTDE1440");
            nsUri = name.Substring(2, close - 2);
            if (nsUri.IndexOfAny(new[] { '{', '}' }) >= 0)
                throw new InvalidOperationException("XTDE1440");
            localName = name.Substring(close + 1);
        }
        else if (name.StartsWith("{"))
        {
            // Clark notation {uri}local is not a valid EQName for fn:element-available.
            throw new InvalidOperationException("XTDE1440");
        }
        else
        {
            int colon = name.IndexOf(':');
            if (colon >= 0)
            {
                string prefix = name.Substring(0, colon);
                localName = name.Substring(colon + 1);
                if (prefix.Length == 0)
                    throw new InvalidOperationException("XTDE1440");
                if (!ctx.TryResolveNamespace(prefix, out nsUri))
                    throw new InvalidOperationException("XTDE1440");
            }
            else
            {
                nsUri = defaultUri;
                localName = name;
            }
        }

        try
        {
            System.Xml.XmlConvert.VerifyNCName(localName);
        }
        catch
        {
            throw new InvalidOperationException("XTDE1440");
        }

        return (nsUri, localName);
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
        var (nsUri, localName) = ParseElementAvailableName(ctx, name, defaultUri);

        if (nsUri == Namespaces.ExsltCommon && localName == "document")
            return XdmValue.FromBoolean(true);

        if (nsUri != Namespaces.Xsl)
            return XdmValue.FromBoolean(false);

        return XdmValue.FromBoolean(XsltInstructionNames.Contains(localName));
    }

    /// <summary>
    /// Implements the EXSLT common <c>node-set</c> extension function. In XSLT 1.0
    /// this converts a result-tree-fragment to a node-set. A result tree fragment is
    /// represented internally as a document node, so this function returns the
    /// document's children (typically the single root element) rather than the
    /// document node itself, matching the behaviour expected by XSLT 1.0 stylesheets
    /// such as DocBook.
    /// </summary>
    private static XdmValue ExsltNodeSet(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (arg.IsNode)
        {
            var node = arg.NodeValue;
            if (node.NodeKind == XdmNodeKind.Document)
            {
                var children = new List<XdmValue>();
                foreach (var child in node.Axis(XdmAxis.Child))
                {
                    if (child.IsNode)
                        children.Add(child);
                }
                if (children.Count == 0)
                    return XdmValue.FromSequence(XdmSequence.Empty);
                return XdmValue.FromSequence(MaterializedSequence.FromList(children));
            }
            return arg;
        }
        if (arg.IsSequence)
            return arg;
        return XdmValue.FromSequence(XdmSequence.Empty);
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
        "accept", "accumulator", "accumulator-rule", "analyze-string", "apply-imports", "apply-templates", "assert", "attribute",
        "attribute-set", "break", "call-template", "catch", "character-map", "choose",
        "comment", "context-item", "copy", "copy-of", "decimal-format", "document",
        "element", "evaluate", "expose", "fallback", "for-each", "for-each-group", "fork",
        "function", "global-context-item", "if", "import", "import-schema", "include",
        "iterate", "key", "map", "map-entry", "matching-substring", "merge",
        "merge-action", "merge-key", "merge-source", "message", "mode", "namespace",
        "namespace-alias", "next-match", "next-iteration", "non-matching-substring", "number", "on-completion",
        "on-empty", "on-non-empty", "otherwise", "output", "output-character", "override", "package",
        "param", "perform-sort", "preserve-space", "processing-instruction", "result-document",
        "sequence", "sort", "source-document", "strip-space", "stylesheet", "template",
        "text", "transform", "try", "use-package", "value-of", "variable", "when", "where-populated",
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
        var arg = args[0];

        // Constructor functions atomize their argument (F&O §18.1): the empty sequence
        // yields the empty sequence, a singleton sequence unwraps, and a node yields its
        // typed value (xs:untypedAtomic) — function-lookup-008: xs:QName(function) with
        // an element node argument.
        if (arg.IsSequence && arg.SequenceValue is not null)
        {
            XdmValue? single = null;
            foreach (var item in XdmSequence.FromSource(arg.SequenceValue))
            {
                if (single is not null)
                    throw new InvalidOperationException("XPTY0004");
                single = item;
            }
            if (single is null)
                return XdmValue.Undefined;
            arg = single.Value;
        }
        if (arg.IsUndefined)
            return XdmValue.Undefined;
        if (arg.IsNode)
            arg = XdmValue.FromString(arg.NodeValue.StringValue, "untypedAtomic");

        if (arg.Kind != XdmValueKind.String)
            throw new InvalidOperationException("XPTY0004");

        string lexical = arg.StringValue.Trim();
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

        if (!IsValidNcName(local) || (!string.IsNullOrEmpty(prefix) && !IsValidNcName(prefix)))
            throw new InvalidOperationException("FOCA0002");

        if (!string.IsNullOrEmpty(prefix))
        {
            if (!ctx.TryResolveNamespace(prefix, out string nsUri))
                throw new InvalidOperationException($"FONS0004: No namespace binding for prefix '{prefix}'.");
            return XdmValue.FromQName(new XsQName(local, nsUri, prefix));
        }

        // Unprefixed lexical QNames in the xs:QName constructor use the default element
        // namespace from the static context (or the empty namespace when none is defined).
        string defaultNsUri = ctx.DefaultElementNamespace ?? string.Empty;
        return XdmValue.FromQName(new XsQName(local, defaultNsUri, string.Empty));
    }

    private static XdmValue ParseXml_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        // An empty-sequence argument returns the empty sequence; FODC0006 is reserved
        // for a non-well-formed (including empty-string) input (parse-xml-016/017).
        if (IsEmptySequence(args[0]))
            return XdmValue.Undefined;
        string xml = AtomizedString(args[0]);
        if (string.IsNullOrEmpty(xml))
            throw new InvalidOperationException("FODC0006: fn:parse-xml argument must not be empty.");
        XDocument doc;
        try
        {
            // The static base URI feeds external DTD/entity resolution (parse-xml-008).
            doc = Xml11Loader.Parse(xml, LoadOptions.PreserveWhitespace, ctx.BaseUri);
        }
        catch (System.Xml.XmlException ex)
        {
            throw new InvalidOperationException($"FODC0006: Error parsing XML: {ex.Message}");
        }
        XDocumentProvider.StripDocumentLevelWhitespace(doc);
        return XdmValue.FromNode(doc.ToXdmNode());
    }

    private static XdmValue Serialize_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromString(ctx.StaticOutputParameters is not null
            ? XdmSerializer.Serialize(args[0], BuildStaticSerializationParameters(ctx))
            : XdmSerializer.Serialize(args[0]));

    // Static serialization parameters from the query's output declarations. An
    // output:parameter-document option resolves (lazily) to element-form parameters over
    // which the remaining output declarations take precedence. Unlike bare fn:serialize
    // (which omits the XML declaration), the XSLT/XQuery output-declaration default
    // includes it (Serialization-xml-01, K2-Serialization-22/24).
    private static XdmSerializer.SerializationParameters BuildStaticSerializationParameters(EvaluationContext ctx)
    {
        var dict = ctx.StaticOutputParameters!;
        var staticDefault = new XdmSerializer.SerializationParameters { OmitXmlDeclaration = false };
        XdmSerializer.SerializationParameters? baseParams = null;
        if (dict.TryGetValue(("http://www.w3.org/2010/xslt-xquery-serialization", "parameter-document"), out var paramDocUri))
        {
            var doc = ctx.LoadDocument(paramDocUri);
            baseParams = XdmSerializer.ParametersFromElementForm(doc, staticDefault);
        }
        return XdmSerializer.ParametersFromOutputDictionary(dict, baseParams ?? staticDefault);
    }

    private static XdmValue ParseXmlFragment_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string xml = AtomizedString(args[0]);
        if (string.IsNullOrEmpty(xml))
            return XdmValue.Undefined;
        // The fragment is parsed as external-entity content; a leading text declaration
        // (<?xml ...?>) is legal there and ignored (parse-xml-fragment-001). The text
        // declaration requires an encoding and forbids 'standalone' (FODC0006).
        var trimmed = xml.TrimStart();
        if (trimmed.StartsWith("<?xml", StringComparison.Ordinal))
        {
            int declEnd = trimmed.IndexOf("?>", StringComparison.Ordinal);
            if (declEnd >= 0)
            {
                var declaration = trimmed[5..declEnd];
                if (declaration.Contains("standalone", StringComparison.Ordinal))
                    throw new InvalidOperationException("FODC0006: A text declaration in an external parsed entity must not contain 'standalone'.");
                if (!declaration.Contains("encoding", StringComparison.Ordinal))
                    throw new InvalidOperationException("FODC0006: A text declaration in an external parsed entity requires an encoding.");
                xml = trimmed[(declEnd + 2)..];
            }
        }
        // Parse the fragment inside a synthetic document wrapper so it can hold any
        // sequence of top-level nodes. The wrapper is transparent to XDM axes.
        var wrapper = $"<__xdm_doc__>{xml}</__xdm_doc__>";
        XDocument doc;
        try
        {
            doc = XDocument.Parse(wrapper, LoadOptions.PreserveWhitespace);
        }
        catch (System.Xml.XmlException ex)
        {
            throw new InvalidOperationException($"FODC0006: Error parsing XML fragment: {ex.Message}");
        }
        XDocumentProvider.StripDocumentLevelWhitespace(doc);
        return XdmValue.FromNode(new XDocumentNode(doc));
    }

    private static XdmValue HasChildren_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var item = ctx.ContextItem;
        if (item.IsUndefined)
            throw new InvalidOperationException("XPDY0002: Context item is absent for fn:has-children().");
        return HasChildren(item);
    }

    private static XdmValue HasChildren_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => HasChildren(args[0]);

    private static XdmValue HasChildren(XdmValue value)
    {
        // Spec signature: $node as node()?
        // Unwrap singleton sequences; empty sequence is valid and returns false.
        if (value.IsSequence && value.SequenceValue is not null)
        {
            if (!value.SequenceValue.TryGetLength(out var len))
                throw new InvalidOperationException("XPTY0004: fn:has-children argument must be a node or empty sequence.");
            if (len == 0)
                return XdmValue.False;
            if (len != 1)
                throw new InvalidOperationException("XPTY0004: fn:has-children argument must be a single node.");
            var enumerator = XdmSequence.FromSource(value.SequenceValue).GetEnumerator();
            enumerator.MoveNext();
            value = enumerator.Current;
        }

        if (value.IsUndefined)
            return XdmValue.False;
        if (!value.IsNode)
            throw new InvalidOperationException("XPTY0004: fn:has-children argument must be a node.");
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
                XdmNodeKind.Text => $"text()[{GetSiblingIndex(current)}]",
                XdmNodeKind.Comment => $"comment()[{GetSiblingIndex(current)}]",
                XdmNodeKind.ProcessingInstruction => $"processing-instruction({current.LocalName})[{GetSiblingIndex(current)}]",
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
        {
            // The walk ended at a parentless root node that is not a document: its own
            // step is replaced by the fn:root() designator; steps below the root are
            // kept (F&O fn:path — accessor-059: Q{...}root()/Q{}inner[1]).
            segments.RemoveAt(segments.Count - 1);
            segments.Reverse();
            var suffix = string.Join("/", segments);
            return "Q{http://www.w3.org/2005/xpath-functions}root()" + (suffix.Length == 0 ? "" : "/" + suffix);
        }

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
                else if (node.NodeKind == XdmNodeKind.ProcessingInstruction)
                {
                    // fn:path indexes processing instructions among same-named siblings.
                    if (sibling.NodeValue.LocalName == node.LocalName)
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
        var result = new XElement(fn + "analyze-string-result",
            new XAttribute(XNamespace.Xmlns + "fn", fn.NamespaceName));

        if (string.IsNullOrEmpty(value))
            return XdmValue.FromNode(new XDocumentNode(result));

        var options = RegexHelper.ParseRegexFlags(flags, out bool isQuoteMode, out bool caseInsensitive);
        if (isQuoteMode)
            pattern = Regex.Escape(pattern);
        else
            pattern = RegexHelper.ValidateAndTranslatePatternCached(pattern, options, caseInsensitive);

        RegexHelper.CheckZeroLengthMatch(pattern, options);

        var matches = RegexHelper.GetRegex(pattern, options).Matches(value);
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

        // Attach PSVI annotations so that schema-aware tests see typed attributes
        // (e.g. @nr as xs:positiveInteger) and schema-element tests succeed.
        var wrapper = new XDocument(result);
        XDocumentProvider.ValidateXDocument(wrapper, AnalyzeStringSchemaSet);
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
        => XdmValue.FromBoolean(StringContains(RequireString(args[0], ctx.BackwardsCompatible), RequireString(args[1], ctx.BackwardsCompatible), ctx.DefaultCollation));

    private static XdmValue Contains_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string s = RequireString(args[0], ctx.BackwardsCompatible);
        string search = RequireString(args[1], ctx.BackwardsCompatible);
        string collation = AtomizedString(args[2]);
        ValidateCollation(collation);
        return XdmValue.FromBoolean(StringContains(s, search, collation));
    }

    private static XdmValue StartsWith(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromBoolean(StringStartsWith(RequireString(args[0], ctx.BackwardsCompatible), RequireString(args[1], ctx.BackwardsCompatible), ctx.DefaultCollation));

    private static XdmValue StartsWith_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string s = RequireString(args[0], ctx.BackwardsCompatible);
        string search = RequireString(args[1], ctx.BackwardsCompatible);
        string collation = AtomizedString(args[2]);
        ValidateCollation(collation);
        return XdmValue.FromBoolean(StringStartsWith(s, search, collation));
    }

    private static XdmValue EndsWith(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromBoolean(StringEndsWith(RequireString(args[0], ctx.BackwardsCompatible), RequireString(args[1], ctx.BackwardsCompatible), ctx.DefaultCollation));

    private static XdmValue EndsWith_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string s = RequireString(args[0], ctx.BackwardsCompatible);
        string search = RequireString(args[1], ctx.BackwardsCompatible);
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

            var parts = s.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
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
        => CollationKey(RequireString(PromoteUriToString(args[0])), string.Empty);

    private static XdmValue CollationKey_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => CollationKey(RequireString(PromoteUriToString(args[0])), RequireString(PromoteUriToString(args[1])));

    // xs:anyURI promotes to xs:string under the function conversion rules (collation-key-006);
    // other non-string atomic types are rejected by RequireString (collation-key-901).
    private static XdmValue PromoteUriToString(XdmValue value)
        => value.Kind == XdmValueKind.Uri ? XdmValue.FromString(value.StringValue) : value;

    private static XdmValue CollationKey(string value, string collation)
    {
        ValidateCollation(collation);
        if (collation == CodepointCollation)
            return XdmValue.FromString(value);
        if (collation == HtmlAsciiCaseInsensitiveCollation)
            return XdmValue.FromString(ToAsciiLower(value));
        if (TryParseUca(collation, out var uca))
        {
            // caseFirst=upper requests uppercase to sort before lowercase. .NET's default
            // sort key orders lowercase first, so swapping the case of the source string
            // before generating the key produces the inverse order.
            string keyInput = uca.CaseFirst == "upper" ? SwapCase(value) : value;
            var sortKey = uca.CompareInfo.GetSortKey(keyInput, uca.Options);
            return XdmValue.FromString(Convert.ToHexString(sortKey.KeyData));
        }
        return XdmValue.FromString(value);
    }

    private static string SwapCase(string value)
    {
        var sb = new System.Text.StringBuilder(value.Length);
        foreach (Rune rune in value.EnumerateRunes())
        {
            Rune upper = Rune.ToUpperInvariant(rune);
            if (upper != rune)
                sb.Append(upper.ToString());
            else
                sb.Append(Rune.ToLowerInvariant(rune).ToString());
        }
        return sb.ToString();
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

    private static bool AsciiCaseInsensitiveMatchAt(string s, int start, string search)
    {
        for (int i = 0; i < search.Length; i++)
        {
            char a = s[start + i];
            char b = search[i];
            if (a != b)
            {
                if (a >= 'A' && a <= 'Z') a = (char)(a + 32);
                if (b >= 'A' && b <= 'Z') b = (char)(b + 32);
                if (a != b) return false;
            }
        }
        return true;
    }

    private static bool AsciiCaseInsensitiveContains(string s, string search)
    {
        if (search.Length == 0) return true;
        for (int i = 0; i <= s.Length - search.Length; i++)
        {
            if (AsciiCaseInsensitiveMatchAt(s, i, search))
                return true;
        }
        return false;
    }

    private static bool AsciiCaseInsensitiveStartsWith(string s, string search)
    {
        if (search.Length > s.Length) return false;
        return AsciiCaseInsensitiveMatchAt(s, 0, search);
    }

    private static bool AsciiCaseInsensitiveEndsWith(string s, string search)
    {
        if (search.Length > s.Length) return false;
        return AsciiCaseInsensitiveMatchAt(s, s.Length - search.Length, search);
    }

    private static int AsciiCaseInsensitiveIndexOf(string s, string search)
    {
        if (search.Length == 0) return 0;
        for (int i = 0; i <= s.Length - search.Length; i++)
        {
            if (AsciiCaseInsensitiveMatchAt(s, i, search))
                return i;
        }
        return -1;
    }

    private sealed class AsciiCaseInsensitiveComparer : IEqualityComparer<string>
    {
        public static readonly AsciiCaseInsensitiveComparer Instance = new();

        public bool Equals(string? x, string? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            if (x.Length != y.Length) return false;
            return AsciiCaseInsensitiveMatchAt(x, 0, y);
        }

        public int GetHashCode(string obj)
            => ToAsciiLower(obj).GetHashCode();
    }

    private const string CodepointCollation = "http://www.w3.org/2005/xpath-functions/collation/codepoint";
    private const string HtmlAsciiCaseInsensitiveCollation = "http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive";
    private const string CaseblindCollation = "http://www.w3.org/2010/09/qt-fots-catalog/collation/caseblind";
    private const string UcaCollationPrefix = "http://www.w3.org/2013/collation/UCA";

    private static string ResolveCollationUri(string collation, string? baseUri)
    {
        if (string.IsNullOrEmpty(collation))
            return string.Empty;
        if (Uri.IsWellFormedUriString(collation, UriKind.Absolute))
            return collation;
        if (!string.IsNullOrEmpty(baseUri) &&
            Uri.TryCreate(new Uri(baseUri), collation, out var resolved))
        {
            return resolved.AbsoluteUri;
        }
        return collation;
    }

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
        if (collation == CaseblindCollation)
            return StringComparison.OrdinalIgnoreCase;
        return StringComparison.Ordinal;
    }

    private static IEqualityComparer<string> GetStringComparer(string collation)
    {
        if (collation == HtmlAsciiCaseInsensitiveCollation)
            return AsciiCaseInsensitiveComparer.Instance;
        if (collation == CaseblindCollation)
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
            return CompareUca(s1, s2, uca);
        if (collation == HtmlAsciiCaseInsensitiveCollation)
            return string.Compare(ToAsciiLower(s1), ToAsciiLower(s2), StringComparison.Ordinal);
        var comparison = GetStringComparison(collation);
        if (comparison == StringComparison.Ordinal)
            return CompareCodepoints(s1, s2);
        return string.Compare(s1, s2, comparison);
    }

    private static int CompareUca(string s1, string s2, UcaCollationInfo uca)
    {
        if (uca.Numeric)
            return CompareUcaNumeric(s1, s2, uca);
        if (uca.CaseLevel)
            return CompareUcaCaseLevel(s1, s2, uca);
        if (uca.Backwards)
            return CompareUcaBackwards(s1, s2, uca);
        return CompareUcaStandard(s1, s2, uca);
    }

    private static int CompareUcaStandard(string s1, string s2, UcaCollationInfo uca)
    {
        var ignoreSymbols = uca.Alternate is "blanked" or "shifted" ? CompareOptions.IgnoreSymbols : CompareOptions.None;
        int strengthLevel = GetUcaStrengthLevel(uca.Strength);

        int primary = uca.CompareInfo.Compare(s1, s2, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | ignoreSymbols);
        if (primary != 0 || strengthLevel == 1)
            return primary;

        int secondary = uca.CompareInfo.Compare(s1, s2, CompareOptions.IgnoreCase | ignoreSymbols);
        if (secondary != 0 || strengthLevel == 2)
            return secondary;

        int caseFirst = strengthLevel >= 3 ? ApplyCaseFirst(s1, s2, uca) : 0;
        if (caseFirst != 0)
            return caseFirst;

        int tertiary = uca.CompareInfo.Compare(s1, s2, CompareOptions.None | ignoreSymbols);
        if (tertiary != 0 || strengthLevel == 3)
            return tertiary;

        // strength quaternary or identical: any remaining variable-level tie-break.
        return UcaVariableTieBreak(s1, s2, uca);
    }

    private static int GetUcaStrengthLevel(string strength)
        => strength.ToLowerInvariant() switch
        {
            "primary" => 1,
            "secondary" => 2,
            "tertiary" => 3,
            "quaternary" => 4,
            "identical" => 5,
            _ => 3
        };

    private static int CompareUcaCaseLevel(string s1, string s2, UcaCollationInfo uca)
    {
        var ignoreSymbols = uca.Alternate is "blanked" or "shifted" ? CompareOptions.IgnoreSymbols : CompareOptions.None;
        int strengthLevel = GetUcaStrengthLevel(uca.Strength);

        int primary = uca.CompareInfo.Compare(s1, s2, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | ignoreSymbols);
        if (primary != 0)
            return primary;

        if (strengthLevel >= 2)
        {
            int secondary = uca.CompareInfo.Compare(s1, s2, CompareOptions.IgnoreCase | ignoreSymbols);
            if (secondary != 0)
                return secondary;
        }

        int caseFirst = ApplyCaseFirst(s1, s2, uca);
        if (caseFirst != 0)
            return caseFirst;

        int caseOrder = uca.CompareInfo.Compare(s1, s2, CompareOptions.IgnoreNonSpace | ignoreSymbols);
        if (caseOrder != 0)
            return caseOrder;

        if (strengthLevel >= 3)
        {
            int tertiary = uca.CompareInfo.Compare(s1, s2, CompareOptions.None | ignoreSymbols);
            if (tertiary != 0)
                return tertiary;
        }

        return UcaVariableTieBreak(s1, s2, uca);
    }

    private static int CompareUcaBackwards(string s1, string s2, UcaCollationInfo uca)
    {
        var ignoreSymbols = uca.Alternate is "blanked" or "shifted" ? CompareOptions.IgnoreSymbols : CompareOptions.None;

        int primary = uca.CompareInfo.Compare(s1, s2, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | ignoreSymbols);
        if (primary != 0)
            return primary;

        int accentForward = uca.CompareInfo.Compare(s1, s2, CompareOptions.IgnoreCase | ignoreSymbols);
        int baseAndCase = uca.CompareInfo.Compare(s1, s2, CompareOptions.IgnoreNonSpace | ignoreSymbols);
        if (accentForward != 0 && baseAndCase == 0)
            return -accentForward;

        int tertiary = uca.CompareInfo.Compare(s1, s2, CompareOptions.None | ignoreSymbols);
        if (tertiary != 0)
            return tertiary;

        return UcaVariableTieBreak(s1, s2, uca);
    }

    private static int CompareUcaNumeric(string s1, string s2, UcaCollationInfo uca)
    {
        var tokens1 = TokenizeForNumeric(s1);
        var tokens2 = TokenizeForNumeric(s2);
        int i = 0;
        while (i < tokens1.Count && i < tokens2.Count)
        {
            var t1 = tokens1[i];
            var t2 = tokens2[i];
            int cmp;
            if (t1.IsDigits && t2.IsDigits)
            {
                cmp = CompareNumericDigitRuns(t1.Text, t2.Text);
            }
            else
            {
                cmp = CompareUcaStandard(t1.Text, t2.Text, uca);
            }
            if (cmp != 0)
                return cmp;
            i++;
        }
        if (i < tokens1.Count)
            return 1;
        if (i < tokens2.Count)
            return -1;
        return 0;
    }

    private static List<(bool IsDigits, string Text)> TokenizeForNumeric(string s)
    {
        var tokens = new List<(bool, string)>();
        if (string.IsNullOrEmpty(s))
            return tokens;
        bool inDigits = char.IsDigit(s[0]);
        var sb = new System.Text.StringBuilder();
        foreach (char c in s)
        {
            bool isDigit = char.IsDigit(c);
            if (isDigit != inDigits)
            {
                tokens.Add((inDigits, sb.ToString()));
                sb.Clear();
                inDigits = isDigit;
            }
            sb.Append(c);
        }
        tokens.Add((inDigits, sb.ToString()));
        return tokens;
    }

    private static int CompareNumericDigitRuns(string a, string b)
    {
        int i = 0;
        while (i < a.Length - 1 && a[i] == '0')
            i++;
        int j = 0;
        while (j < b.Length - 1 && b[j] == '0')
            j++;
        var trimmedA = a[i..];
        var trimmedB = b[j..];
        if (trimmedA.Length != trimmedB.Length)
            return trimmedA.Length < trimmedB.Length ? -1 : 1;
        return string.CompareOrdinal(trimmedA, trimmedB);
    }

    private static int ApplyCaseFirst(string s1, string s2, UcaCollationInfo uca)
    {
        if (string.IsNullOrEmpty(uca.CaseFirst))
            return 0;

        int i1 = 0, i2 = 0;
        while (i1 < s1.Length && i2 < s2.Length)
        {
            int cp1 = char.ConvertToUtf32(s1, i1);
            int cp2 = char.ConvertToUtf32(s2, i2);
            var r1 = new Rune(cp1);
            var r2 = new Rune(cp2);
            var lower1 = Rune.ToLowerInvariant(r1);
            var lower2 = Rune.ToLowerInvariant(r2);

            if (lower1 != lower2)
                return 0;

            if (r1 != r2)
            {
                bool isUpper1 = Rune.ToUpperInvariant(r1) == r1 && lower1 != r1;
                bool isUpper2 = Rune.ToUpperInvariant(r2) == r2 && lower2 != r2;
                if (isUpper1 != isUpper2)
                {
                    if (uca.CaseFirst == "upper")
                        return isUpper1 ? -1 : 1;
                    return isUpper1 ? 1 : -1;
                }
            }

            i1 += char.IsHighSurrogate(s1[i1]) ? 2 : 1;
            i2 += char.IsHighSurrogate(s2[i2]) ? 2 : 1;
        }
        return 0;
    }

    private static int UcaVariableTieBreak(string s1, string s2, UcaCollationInfo uca)
    {
        bool apply = uca.Alternate switch
        {
            "blanked" => string.Equals(uca.Strength, "identical", StringComparison.OrdinalIgnoreCase),
            "shifted" => string.Equals(uca.Strength, "quaternary", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(uca.Strength, "identical", StringComparison.OrdinalIgnoreCase),
            _ => false
        };
        if (!apply)
            return 0;
        var v1 = ExtractUcaVariableCharacters(s1, uca.CompareInfo);
        var v2 = ExtractUcaVariableCharacters(s2, uca.CompareInfo);
        // In shifted UCA, a string that contains more variable characters sorts
        // before one with fewer, so a greater variable sequence yields a lesser result.
        return -string.CompareOrdinal(v1, v2);
    }

    private static string ExtractUcaVariableCharacters(string s, CompareInfo compareInfo)
    {
        var sb = new System.Text.StringBuilder();
        foreach (char c in s)
        {
            if (compareInfo.Compare(c.ToString(), string.Empty, CompareOptions.IgnoreSymbols) == 0)
                sb.Append(c);
        }
        return sb.ToString();
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
        {
            if (uca.Numeric)
                throw new InvalidOperationException("FOCH0004: Numeric collation does not support substring matching");
            return uca.CompareInfo.IndexOf(s, search, uca.Options) >= 0;
        }
        if (collation == HtmlAsciiCaseInsensitiveCollation)
            return AsciiCaseInsensitiveContains(s, search);
        return s.Contains(search, GetStringComparison(collation));
    }

    private static bool StringStartsWith(string s, string search, string collation)
    {
        if (TryParseUca(collation, out var uca))
        {
            if (uca.Numeric)
                throw new InvalidOperationException("FOCH0004: Numeric collation does not support substring matching");

            // IsPrefix is unreliable with IgnoreSymbols when both strings start with the same symbol.
            // Use IndexOf and verify the match is at or before the first non-ignorable character.
            int firstNonIgnorable = 0;
            while (firstNonIgnorable < s.Length &&
                   uca.CompareInfo.Compare(s[firstNonIgnorable].ToString(), string.Empty, uca.Options) == 0)
                firstNonIgnorable++;

            int matchPos = uca.CompareInfo.IndexOf(s, search, uca.Options);
            return matchPos >= 0 && matchPos <= firstNonIgnorable;
        }
        if (collation == HtmlAsciiCaseInsensitiveCollation)
            return AsciiCaseInsensitiveStartsWith(s, search);
        return s.StartsWith(search, GetStringComparison(collation));
    }

    private static bool StringEndsWith(string s, string search, string collation)
    {
        if (TryParseUca(collation, out var uca))
        {
            if (uca.Numeric)
                throw new InvalidOperationException("FOCH0004: Numeric collation does not support substring matching");

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
        if (collation == HtmlAsciiCaseInsensitiveCollation)
            return AsciiCaseInsensitiveEndsWith(s, search);
        return s.EndsWith(search, GetStringComparison(collation));
    }

    private static int StringIndexOf(string s, string search, string collation)
    {
        if (TryParseUca(collation, out var uca))
        {
            if (uca.Numeric)
                throw new InvalidOperationException("FOCH0004: Numeric collation does not support substring matching");
            return uca.CompareInfo.IndexOf(s, search, uca.Options);
        }
        if (collation == HtmlAsciiCaseInsensitiveCollation)
            return AsciiCaseInsensitiveIndexOf(s, search);
        return s.IndexOf(search, GetStringComparison(collation));
    }

    private static IEqualityComparer<string> GetCollationEqualityComparer(string collation)
    {
        if (TryParseUca(collation, out var uca))
            return new UcaStringComparer(uca, collation);
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
        string alternate = "";
        string caseFirst = "";
        bool numeric = false;
        bool backwards = false;
        bool caseLevel = false;
        bool fallbackNo = false;
        var rawParams = query.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (var param in rawParams)
        {
            int eq = param.IndexOf('=');
            if (eq < 0)
                continue;
            string key = param[..eq].Trim();
            string val = param[(eq + 1)..].Trim();

            if (key == "lang")
                lang = val;
            else if (key == "strength")
                strength = val;
            else if (key == "alternate")
                alternate = val.ToLowerInvariant();
            else if (string.Equals(key, "caseFirst", StringComparison.OrdinalIgnoreCase))
                caseFirst = val.ToLowerInvariant();
            else if (string.Equals(key, "numeric", StringComparison.OrdinalIgnoreCase))
                numeric = string.Equals(val, "yes", StringComparison.OrdinalIgnoreCase);
            else if (string.Equals(key, "backwards", StringComparison.OrdinalIgnoreCase))
                backwards = string.Equals(val, "yes", StringComparison.OrdinalIgnoreCase);
            else if (string.Equals(key, "caseLevel", StringComparison.OrdinalIgnoreCase))
                caseLevel = string.Equals(val, "yes", StringComparison.OrdinalIgnoreCase);
            else if (string.Equals(key, "fallback", StringComparison.OrdinalIgnoreCase))
                fallbackNo = string.Equals(val, "no", StringComparison.OrdinalIgnoreCase);
        }

        // With explicit fallback=no the implementation must reject unsupported parameters
        // and invalid values (F+O 3.1 §7.3.1). With fallback=yes/absent we stay lenient.
        if (fallbackNo)
        {
            foreach (var param in rawParams)
            {
                int eq = param.IndexOf('=');
                if (eq < 0)
                    throw new InvalidOperationException("FOCH0002");
                string key = param[..eq].Trim();
                string val = param[(eq + 1)..].Trim();
                string keyLower = key.ToLowerInvariant();
                string valLower = val.ToLowerInvariant();

                if (keyLower is "version" or "normalization" or "hiraganaquaternary" or "reorder" or "maxvariable")
                    throw new InvalidOperationException("FOCH0002");

                if (keyLower == "lang")
                    continue;

                if (keyLower == "strength")
                {
                    if (string.IsNullOrEmpty(valLower) ||
                        !(valLower is "primary" or "secondary" or "tertiary" or "quaternary" or "identical" ||
                          (int.TryParse(valLower, out int s) && s >= 1 && s <= 5)))
                        throw new InvalidOperationException("FOCH0002");
                    continue;
                }

                if (keyLower == "alternate")
                {
                    if (string.IsNullOrEmpty(valLower) || valLower is not ("blanked" or "shifted" or "non-ignorable"))
                        throw new InvalidOperationException("FOCH0002");
                    continue;
                }

                if (keyLower == "casefirst")
                {
                    if (string.IsNullOrEmpty(valLower) || valLower is not ("upper" or "lower" or "off"))
                        throw new InvalidOperationException("FOCH0002");
                    continue;
                }

                if (keyLower is "numeric" or "backwards" or "caselevel" or "fallback")
                {
                    if (valLower is not ("yes" or "no"))
                        throw new InvalidOperationException("FOCH0002");
                    continue;
                }

                // Any other keyword is unknown.
                throw new InvalidOperationException("FOCH0002");
            }
        }

        // Map numeric strength values (1..5) to named strengths in both modes.
        if (int.TryParse(strength, out int strengthNum) && strengthNum >= 1 && strengthNum <= 5)
        {
            strength = strengthNum switch
            {
                1 => "primary",
                2 => "secondary",
                3 => "tertiary",
                4 => "quaternary",
                5 => "identical",
                _ => strength
            };
        }

        var culture = CultureInfo.GetCultureInfo(lang);
        var isIdentical = string.Equals(strength, "identical", StringComparison.OrdinalIgnoreCase);
        var alternateBlanked = alternate == "blanked";
        var alternateShifted = alternate == "shifted";
        var alternateIgnored = alternateBlanked || alternateShifted;

        var options = strength.ToLowerInvariant() switch
        {
            "primary" => CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace,
            "secondary" => CompareOptions.IgnoreCase,
            "tertiary" => CompareOptions.None,
            "quaternary" => CompareOptions.None,
            // CompareOptions.Ordinal cannot be combined with IgnoreSymbols; for shifted/blanked
            // we rely on the variable tie-break in CompareStrings.
            "identical" => alternateIgnored ? CompareOptions.None : CompareOptions.Ordinal,
            _ => CompareOptions.None,
        };

        if (alternateIgnored)
            options |= CompareOptions.IgnoreSymbols;

        info = new UcaCollationInfo(lang, strength, options, culture.CompareInfo, isIdentical && alternateBlanked,
            caseFirst, numeric, backwards, caseLevel, alternate);
        return true;
    }

    private readonly record struct UcaCollationInfo(
        string Lang,
        string Strength,
        CompareOptions Options,
        CompareInfo CompareInfo,
        bool IsIdenticalBlanked,
        string CaseFirst,
        bool Numeric,
        bool Backwards,
        bool CaseLevel,
        string Alternate);

    private sealed class UcaStringComparer : IEqualityComparer<string>
    {
        private readonly UcaCollationInfo _uca;
        private readonly string _collation;

        public UcaStringComparer(UcaCollationInfo uca, string collation)
        {
            _uca = uca;
            _collation = collation;
        }

        public bool Equals(string? x, string? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;
            if (_uca.IsIdenticalBlanked)
                return string.Equals(x, y, StringComparison.Ordinal);
            if (HasAdvancedFlags(_uca))
                return CompareStrings(x, y, _collation) == 0;
            return _uca.CompareInfo.Compare(x, y, _uca.Options) == 0;
        }

        public int GetHashCode(string obj)
        {
            if (_uca.IsIdenticalBlanked)
                return StringComparer.Ordinal.GetHashCode(obj);
            if (HasAdvancedFlags(_uca))
                return 0;
            return _uca.CompareInfo.GetSortKey(obj, _uca.Options).GetHashCode();
        }
    }

    private static bool HasAdvancedFlags(UcaCollationInfo uca)
        => uca.Numeric || uca.Backwards || uca.CaseLevel || !string.IsNullOrEmpty(uca.CaseFirst) || uca.Alternate == "shifted";

    private static XdmValue NormalizeSpace_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var item = ctx.ContextItem;
        if (item.IsUndefined)
            throw new InvalidOperationException("fn:normalize-space() called with no context item.");
        return XdmValue.FromString(NormalizeSpaceString(AtomizedString(item)));
    }

    private static XdmValue NormalizeSpace_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromString(NormalizeSpaceString(RequireString(args[0], ctx.BackwardsCompatible)));

    private static string NormalizeSpaceString(string s)
    {
        // XPath fn:normalize-space collapses only #x20, #x9, #xD and #xA; other Unicode
        // whitespace (e.g. NBSP U+00A0) is NOT whitespace for XPath (fn-tokenize-51).
        var parts = s.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', parts);
    }

    private static XdmValue NormalizeUnicode_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => NormalizeUnicode(RequireString(args[0]), "NFC");

    private static XdmValue NormalizeUnicode_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => NormalizeUnicode(RequireString(args[0]), RequireStringRequired(args[1]));

    private static XdmValue NormalizeUnicode(string input, string form)
    {
        // The form name is matched case-insensitively after stripping whitespace; the
        // zero-length form performs no normalization (fn-normalize-unicode-1/2args-2/4).
        switch (form.Trim().ToUpperInvariant())
        {
            case "":
                return XdmValue.FromString(input);
            case "NFC":
                return XdmValue.FromString(input.Normalize(System.Text.NormalizationForm.FormC));
            case "NFD":
                return XdmValue.FromString(input.Normalize(System.Text.NormalizationForm.FormD));
            case "NFKC":
                return XdmValue.FromString(input.Normalize(System.Text.NormalizationForm.FormKC));
            case "NFKD":
                return XdmValue.FromString(input.Normalize(System.Text.NormalizationForm.FormKD));
            case "FULLY-NORMALIZED":
                // XML 1.1 full normalization: NFC and not starting with a non-starter
                // (combining mark; spacing marks such as U+09BE are starters).
                if (!input.IsNormalized(System.Text.NormalizationForm.FormC) ||
                    (input.Length > 0 && char.GetUnicodeCategory(input, 0) is
                        System.Globalization.UnicodeCategory.NonSpacingMark or
                        System.Globalization.UnicodeCategory.EnclosingMark))
                    throw new InvalidOperationException("FOCH0003");
                return XdmValue.FromString(input);
            default:
                throw new InvalidOperationException("FOCH0003");
        }
    }

    private static XdmValue Translate(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        // Signature: translate($arg as xs:string?, $map as xs:string, $trans as xs:string).
        // $map/$trans are required: an empty sequence or a non-string atomic raises XPTY0004.
        string arg = RequireString(args[0]);
        string map = RequireStringRequired(args[1]);
        string trans = RequireStringRequired(args[2]);
        // Code-point aware: astral characters count as single characters.
        var transRunes = new List<int>(trans.Length);
        foreach (Rune rune in trans.EnumerateRunes())
            transRunes.Add(rune.Value);
        var mapIndex = new Dictionary<int, int>(map.Length);
        int pos = 0;
        foreach (Rune rune in map.EnumerateRunes())
        {
            // First occurrence of a character in $map wins.
            mapIndex.TryAdd(rune.Value, pos);
            pos++;
        }
        var sb = new StringBuilder(arg.Length);
        foreach (Rune rune in arg.EnumerateRunes())
        {
            if (mapIndex.TryGetValue(rune.Value, out int idx))
            {
                if (idx < transRunes.Count)
                    sb.Append(char.ConvertFromUtf32(transRunes[idx]));
                // else: character is deleted
            }
            else
            {
                sb.Append(rune.ToString());
            }
        }
        return XdmValue.FromString(sb.ToString());
    }

    private static XdmValue UpperCase(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromString(ApplyUnicodeCaseMapping(RequireString(args[0], ctx.BackwardsCompatible), toUpper: true));

    private static XdmValue LowerCase(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromString(ApplyUnicodeCaseMapping(RequireString(args[0], ctx.BackwardsCompatible), toUpper: false));

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
                    case 0xFB17: sb.Append("\u0544\u053D"); continue; // Armenian ligature men xeh → ՄԽ (MEN + XEH)
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
        string input = RequireString(args[0]);
        string pattern = RequireStringRequired(args[1]);
        return XdmValue.FromBoolean(RegexHelper.GetRegexForXsdPattern(pattern, RegexOptions.None, false).IsMatch(input));
    }

    private static XdmValue Matches_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string input = RequireString(args[0]);
        string pattern = RequireStringRequired(args[1]);
        var options = RegexHelper.ParseRegexFlags(RequireStringRequired(args[2]), out bool isQuoteMode, out bool caseInsensitive);
        if (isQuoteMode)
            return XdmValue.FromBoolean(RegexHelper.GetRegex(Regex.Escape(pattern), options).IsMatch(input));
        return XdmValue.FromBoolean(RegexHelper.GetRegexForXsdPattern(pattern, options, caseInsensitive).IsMatch(input));
    }


    private static XdmValue Replace_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string input = AtomizedString(args[0]);
        string originalPattern = RequireStringRequired(args[1], ctx.BackwardsCompatible);
        var regex = RegexHelper.GetRegexForXsdPattern(originalPattern, RegexOptions.None, false);
        string replacement = RequireStringRequired(args[2], ctx.BackwardsCompatible);
        RegexHelper.CheckZeroLengthMatch(regex);
        int groupCount = RegexHelper.CountCapturingGroups(originalPattern);
        string netReplacement = RegexHelper.ValidateAndTranslateReplacement(replacement, groupCount);
        return XdmValue.FromString(regex.Replace(input, netReplacement));
    }

    private static XdmValue Replace_4(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string input = AtomizedString(args[0]);
        string originalPattern = RequireStringRequired(args[1], ctx.BackwardsCompatible);
        string replacement = RequireStringRequired(args[2], ctx.BackwardsCompatible);
        var options = RegexHelper.ParseRegexFlags(RequireStringRequired(args[3], ctx.BackwardsCompatible), out bool isQuoteMode, out bool caseInsensitive);
        Regex regex;
        string netReplacement;
        if (isQuoteMode)
        {
            regex = RegexHelper.GetRegex(Regex.Escape(originalPattern), options);
            netReplacement = RegexHelper.EscapeReplacementForQuoteMode(replacement);
        }
        else
        {
            regex = RegexHelper.GetRegexForXsdPattern(originalPattern, options, caseInsensitive);
            int groupCount = RegexHelper.CountCapturingGroups(originalPattern);
            netReplacement = RegexHelper.ValidateAndTranslateReplacement(replacement, groupCount);
        }
        RegexHelper.CheckZeroLengthMatch(regex);
        return XdmValue.FromString(regex.Replace(input, netReplacement));
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

        var options = RegexHelper.ParseRegexFlags(flags, out bool isQuoteMode, out bool caseInsensitive);
        Regex regex = isQuoteMode
            ? RegexHelper.GetRegex(Regex.Escape(pattern), options)
            : RegexHelper.GetRegexForXsdPattern(pattern, options, caseInsensitive);

        RegexHelper.CheckZeroLengthMatch(regex);

        // Slice between matches: Regex.Split would also return the values of capturing
        // groups, but fn:tokenize must deliver only the substrings between separators.
        var result = new List<XdmValue>();
        int lastEnd = 0;
        foreach (System.Text.RegularExpressions.Match match in regex.Matches(input))
        {
            result.Add(XdmValue.FromString(input[lastEnd..match.Index]));
            lastEnd = match.Index + match.Length;
        }
        result.Add(XdmValue.FromString(input[lastEnd..]));

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

    private static XdmValue XsNumeric(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "numeric");

    private static XdmValue XsBoolean(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "boolean");

    private static XdmValue XsDateTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "dateTime");

    private static XdmValue XsDateTimeStamp(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => VmEngine.Cast(args[0], "dateTimeStamp");

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

    /// <summary>
    /// Constructor function for a user-defined schema simple type. Atomizes the argument
    /// and casts it to the target type using the schema validator (qischema030).
    /// </summary>
    private static XdmValue UserDefinedTypeConstructor(EvaluationContext ctx, ReadOnlySpan<XdmValue> args, string typeName)
    {
        var arg = args[0];
        if (arg.IsUndefined)
            return XdmValue.Undefined;
        return VmEngine.Cast(arg, typeName, ctx);
    }

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
        // XSLT package scopes may intercept the lookup to expose the declaring package's
        // own declarations instead of overrides contributed by using packages.
        if (ctx.FunctionLookupInterceptor?.Invoke(ctx, qname.NamespaceUri, qname.LocalName, arity) is { } intercepted)
            return intercepted;
        if (ctx.TryResolveFunction(qname.NamespaceUri, qname.LocalName, arity, out var sig) &&
            !sig.IsHiddenFromFunctionLookup)
            return XdmValue.FromFunction(new NamedFunctionItem(sig.NamespaceUri, sig.LocalName, sig.Arity)
            {
                DefiningContext = ctx,
                CapturedContextItem = ctx.ContextItem,
                CapturedContextPosition = ctx.ContextPosition,
                CapturedContextSize = ctx.ContextSize,
                CapturedBaseUri = ctx.BaseUri,
                CapturedNamespaces = ctx.SnapshotNamespaces(),
                CapturedSignature = sig
            });
        return XdmValue.Undefined;
    }

    /// <summary>
    /// Stub for fn:load-xquery-module: the function is resolvable (so fn:function-lookup
    /// finds it) in every host, but this library has no XQuery module loader of its own.
    /// Hosts that support module loading (XQuery executions, XSLT 3.0 transforms) set
    /// <see cref="EvaluationContext.XQueryModuleLoader"/> on their context; the stub
    /// dispatches through it and raises FOQM0001 only when no loader is present.
    /// </summary>
    private static XdmValue LoadXQueryModuleStub(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ctx.XQueryModuleLoader?.Invoke(ctx, args)
            ?? throw new InvalidOperationException(
                "FOQM0001: fn:load-xquery-module is not supported by this processor (no XQuery module loader).");

    // ------------------------------------------------------------------
    // fn:error
    // ------------------------------------------------------------------

    private static XdmValue Doc_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        // fn:doc accepts a single optional URI. Empty sequence yields empty sequence.
        if (IsEmptySequence(args[0]))
            return XdmValue.Undefined;

        var uri = RequireString(args[0]);
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
                // XTDE1162: a relative URI cannot be resolved when the explicit base node has no base URI.
                if (bn == null || string.IsNullOrEmpty(bn.BaseUri))
                    throw new InvalidOperationException($"XTDE1162: No base URI available to resolve the relative reference '{uri}'.");
                baseUri = bn.BaseUri;
            }
            else if (items[i].IsNode)
            {
                var nodeBaseUri = items[i].NodeValue!.BaseUri;
                // XTDE1162: a node supplying a relative URI must itself have a base URI.
                if (string.IsNullOrEmpty(nodeBaseUri))
                    throw new InvalidOperationException($"XTDE1162: No base URI available to resolve the relative reference '{uri}'.");
                baseUri = nodeBaseUri;
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
        var uri = RequireString(args[0]);
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
        var focus = ctx.ContextItem;
        if (!focus.IsNode)
            throw new InvalidOperationException("XPTY0004: fn:id() context item is not a node.");
        RequireDocumentRootedTree(focus.NodeValue, "fn:id");

        var ids = ParseIdTokens(args[0]);
        if (ids.Count == 0)
            return XdmValue.Undefined;

        var result = new List<XdmValue>();
        var doc = focus.NodeValue.Document ?? focus.NodeValue;
        if (doc is not null)
            CollectIdElements(doc, ids, result);
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static XdmValue Id_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var node = FirstNode(args[1]);
        if (node == null)
            throw new InvalidOperationException("XPTY0004: fn:id() argument is not a node.");
        RequireDocumentRootedTree(node, "fn:id");

        var ids = ParseIdTokens(args[0]);
        if (ids.Count == 0)
            return XdmValue.Undefined;

        var result = new List<XdmValue>();
        var doc = node.Document ?? node;
        if (doc is not null)
            CollectIdElements(doc, ids, result);
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    // FODC0001: fn:id/fn:idref/fn:element-with-id require the target node to be in a
    // tree whose root is a document node (a constructed element fragment is not).
    private static void RequireDocumentRootedTree(IXdmNode node, string functionName)
    {
        var root = node;
        while (root.Parent is not null)
            root = root.Parent;
        if (root.NodeKind != XdmNodeKind.Document)
            throw new InvalidOperationException($"FODC0001: {functionName} requires a node in a tree whose root is a document node.");
    }

    private static XdmValue ElementWithId_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var focus = ctx.ContextItem;
        if (!focus.IsNode)
            throw new InvalidOperationException("XPTY0004: fn:element-with-id() context item is not a node.");
        RequireDocumentRootedTree(focus.NodeValue, "fn:element-with-id");

        var ids = ParseIdTokens(args[0]);
        if (ids.Count == 0)
            return XdmValue.Undefined;

        var result = new List<XdmValue>();
        var doc = focus.NodeValue.Document ?? focus.NodeValue;
        if (doc is not null)
            CollectElementWithId(doc, ids, result);
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static HashSet<string> ParseIdTokens(XdmValue value)
    {
        var ids = new HashSet<string>();
        foreach (var item in AsSequence(value))
        {
            foreach (var token in item.ToString().Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = token.Trim();
                if (trimmed.Length > 0)
                    ids.Add(trimmed);
            }
        }
        return ids;
    }

    private static HashSet<string> ParseIdTokens(string value)
    {
        var tokens = value.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(s => s.Trim())
                          .Where(s => s.Length > 0);
        return new HashSet<string>(tokens);
    }

    /// <summary>
    /// Builds the candidate IDREF value set for fn:idref(). Unlike fn:id(), the strings
    /// in the argument sequence are not tokenized. The only exception is when the argument
    /// value originates from a DTD-declared IDREFS attribute, whose string value is a
    /// whitespace-separated list of IDREF values.
    /// </summary>
    private static HashSet<string> ParseIdrefArguments(XdmValue value)
    {
        var ids = new HashSet<string>();
        foreach (var item in AsSequence(value))
        {
            if (item.IsNode && item.NodeValue is { } node && IsDtdIdrefsAttribute(node))
            {
                foreach (var token in ParseIdTokens(node.StringValue))
                    ids.Add(token);
                continue;
            }
            var atomized = AtomizedString(item);
            if (atomized.Length > 0)
                ids.Add(atomized);
        }
        return ids;
    }

    private static bool IsDtdIdrefsAttribute(IXdmNode node)
    {
        if (node.NodeKind != XdmNodeKind.Attribute)
            return false;
        var parent = node.Parent;
        if (parent is null || parent.NodeKind != XdmNodeKind.Element)
            return false;
        var dtdInfo = GetDtdAttributeInfo(node);
        if (!dtdInfo.IdrefsAttributes.TryGetValue(parent.LocalName, out var attrs))
            return false;
        return attrs.Contains(node.LocalName);
    }

    private static void CollectElementWithId(IXdmNode node, HashSet<string> ids, List<XdmValue> result)
    {
        var dtdInfo = GetDtdAttributeInfo(node);
        CollectElementWithId(node, ids, result, dtdInfo);
    }

    private static void CollectElementWithId(IXdmNode node, HashSet<string> ids, List<XdmValue> result, DtdAttributeInfo dtdInfo)
    {
        if (node.NodeKind == XdmNodeKind.Element)
        {
            if (ElementHasId(node, ids, dtdInfo))
                result.Add(XdmValue.FromNode(node));
        }
        foreach (var child in node.Children(XdmNodeKind.Element))
        {
            if (child.IsNode)
                CollectElementWithId(child.NodeValue!, ids, result, dtdInfo);
        }
    }

    private static XdmValue ElementWithId_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var node = FirstNode(args[1]);
        if (node == null)
            throw new InvalidOperationException("XPTY0004: fn:element-with-id() argument is not a node.");
        RequireDocumentRootedTree(node, "fn:element-with-id");

        var ids = ParseIdTokens(args[0]);
        if (ids.Count == 0)
            return XdmValue.Undefined;

        var result = new List<XdmValue>();
        var doc = node.Document ?? node;
        if (doc is not null)
            CollectElementWithId(doc, ids, result);
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static XdmValue Idref_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var focus = ctx.ContextItem;
        if (!focus.IsNode)
            throw new InvalidOperationException("XPTY0004: fn:idref() context item is not a node.");
        RequireDocumentRootedTree(focus.NodeValue, "fn:idref");

        var ids = ParseIdrefArguments(args[0]);
        if (ids.Count == 0)
            return XdmValue.Undefined;

        var result = new List<XdmValue>();
        var doc = focus.NodeValue.Document ?? focus.NodeValue;
        if (doc is not null)
            CollectIdrefElements(doc, ids, result);
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static XdmValue Idref_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var node = FirstNode(args[1]);
        if (node == null)
            throw new InvalidOperationException("XPTY0004: fn:idref() argument is not a node.");
        RequireDocumentRootedTree(node, "fn:idref");

        var ids = ParseIdrefArguments(args[0]);
        if (ids.Count == 0)
            return XdmValue.Undefined;

        var result = new List<XdmValue>();
        var doc = node.Document ?? node;
        if (doc is not null)
            CollectIdrefElements(doc, ids, result);
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static IXdmNode? FirstNode(XdmValue value)
    {
        if (value.IsNode)
            return value.NodeValue;
        if (value.IsUndefined)
            return null;
        if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                if (item.IsNode)
                    return item.NodeValue;
            }
        }
        return null;
    }

    private static void CollectIdrefElements(IXdmNode node, HashSet<string> ids, List<XdmValue> result)
    {
        var dtdInfo = GetDtdAttributeInfo(node);
        CollectIdrefElements(node, ids, result, dtdInfo);
    }

    private static void CollectIdrefElements(IXdmNode node, HashSet<string> ids, List<XdmValue> result, DtdAttributeInfo dtdInfo)
    {
        if (node.NodeKind == XdmNodeKind.Element)
        {
            // Schema-validated IDREF-typed elements (including derived list/union types).
            if (node.IsIdref)
            {
                var tokens = ParseIdTokens(node.StringValue);
                if (tokens.Overlaps(ids))
                    result.Add(XdmValue.FromNode(node));
            }

            foreach (var attr in node.Attributes())
            {
                if (!attr.IsNode)
                    continue;

                var attrNode = attr.NodeValue!;

                // Schema-validated IDREF/IDREFS attributes.
                if (attrNode.IsIdref)
                {
                    var tokens = ParseIdTokens(AtomizedString(attr));
                    if (tokens.Overlaps(ids))
                        result.Add(attr);
                    continue;
                }

                // Without schema/DTD processing no attribute is typed as xs:IDREF; by
                // analogy with our fn:id treatment of "id"/"xml:id", an attribute named
                // "idref" (no namespace) is treated as IDREF-typed.
                if (attrNode.LocalName == "idref" && attrNode.NamespaceUri == "")
                {
                    var tokens = ParseIdTokens(AtomizedString(attr));
                    if (tokens.Overlaps(ids))
                        result.Add(attr);
                    continue;
                }

                if (dtdInfo.IdrefAttributes.TryGetValue(node.LocalName, out var dtdIdrefAttrs)
                    && dtdIdrefAttrs.Contains(attrNode.LocalName))
                {
                    var tokens = ParseIdTokens(AtomizedString(attr));
                    if (tokens.Overlaps(ids))
                        result.Add(attr);
                }

                if (dtdInfo.IdrefsAttributes.TryGetValue(node.LocalName, out var dtdIdrefsAttrs)
                    && dtdIdrefsAttrs.Contains(attrNode.LocalName))
                {
                    var tokens = ParseIdTokens(AtomizedString(attr));
                    if (tokens.Overlaps(ids))
                        result.Add(attr);
                }
            }
        }
        foreach (var child in node.Children(XdmNodeKind.Element))
        {
            if (child.IsNode)
                CollectIdrefElements(child.NodeValue!, ids, result, dtdInfo);
        }
    }

    private static XdmValue DefaultLanguage_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromString("en", "language");

    private static void CollectIdElements(IXdmNode node, HashSet<string> ids, List<XdmValue> result)
    {
        var dtdInfo = GetDtdAttributeInfo(node);
        CollectIdElements(node, ids, result, dtdInfo);
    }

    private static void CollectIdElements(IXdmNode node, HashSet<string> ids, List<XdmValue> result, DtdAttributeInfo dtdInfo)
    {
        if (node.NodeKind == XdmNodeKind.Element)
        {
            if (ElementHasIdValue(node, ids, dtdInfo))
                result.Add(XdmValue.FromNode(node));
        }
        foreach (var child in node.Children(XdmNodeKind.Element))
        {
            if (child.IsNode)
                CollectIdElements(child.NodeValue!, ids, result, dtdInfo);
        }
    }

    private static bool ElementHasIdValue(IXdmNode element, HashSet<string> ids, DtdAttributeInfo dtdInfo)
    {
        // The element itself is ID-valued (for example an element of type xs:ID or
        // a list-of-ID element with a singleton value).
        if (element.IsId && ids.Contains(element.StringValue.Trim()))
            return true;

        // ID-typed attributes (schema-validated, xml:id, or name fallback).
        foreach (var attr in element.Attributes())
        {
            if (attr.IsNode && attr.NodeValue!.IsId && ids.Contains(attr.NodeValue.StringValue.Trim()))
                return true;
        }

        // DTD-declared ID attributes not covered by the generic attribute scan.
        if (dtdInfo.IdAttributes.TryGetValue(element.LocalName, out var dtdIdAttrs))
        {
            foreach (var attrName in dtdIdAttrs)
            {
                foreach (var attr in element.Attributes(attrName, ""))
                {
                    if (ids.Contains(AtomizedString(attr).Trim()))
                        return true;
                }
            }
        }

        return false;
    }

    private static bool ElementHasId(IXdmNode element, HashSet<string> ids, DtdAttributeInfo dtdInfo)
    {
        // ID-typed attributes.
        foreach (var attr in element.Attributes())
        {
            if (attr.IsNode && attr.NodeValue!.IsId && ids.Contains(attr.NodeValue.StringValue.Trim()))
                return true;
        }

        // DTD-declared ID attributes.
        if (dtdInfo.IdAttributes.TryGetValue(element.LocalName, out var dtdIdAttrs))
        {
            foreach (var attrName in dtdIdAttrs)
            {
                foreach (var attr in element.Attributes(attrName, ""))
                {
                    if (ids.Contains(AtomizedString(attr).Trim()))
                        return true;
                }
            }
        }

        // Child elements whose typed value is an ID.
        foreach (var child in element.Children(XdmNodeKind.Element))
        {
            if (child.IsNode && child.NodeValue!.IsId && ids.Contains(child.NodeValue.StringValue.Trim()))
                return true;
        }

        return false;
    }

    private sealed record DtdAttributeInfo(
        Dictionary<string, List<string>> IdAttributes,
        Dictionary<string, List<string>> IdrefAttributes,
        Dictionary<string, List<string>> IdrefsAttributes);

    private static readonly ConditionalWeakTable<IXdmNode, DtdAttributeInfo> DtdAttributeCache = new();

    private static DtdAttributeInfo GetDtdAttributeInfo(IXdmNode node)
    {
        var doc = node.Document ?? node;
        if (!doc.HasDocumentType || string.IsNullOrEmpty(doc.InternalSubset))
            return EmptyDtdAttributeInfo;

        return DtdAttributeCache.GetValue(doc, static d =>
        {
            try
            {
                return ParseDtdAttlistDeclarations(d.InternalSubset);
            }
            catch
            {
                return EmptyDtdAttributeInfo;
            }
        });
    }

    private static readonly DtdAttributeInfo EmptyDtdAttributeInfo =
        new(
            new Dictionary<string, List<string>>(StringComparer.Ordinal),
            new Dictionary<string, List<string>>(StringComparer.Ordinal),
            new Dictionary<string, List<string>>(StringComparer.Ordinal));

    private static DtdAttributeInfo ParseDtdAttlistDeclarations(string subset)
    {
        var idAttrs = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var idrefAttrs = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var idrefsAttrs = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        // ATTLIST declarations may contain multiple attributes spread across lines.
        // Capture the whole body of each declaration and scan it for ID/IDREF/IDREFS
        // attribute definitions.
        foreach (Match match in AttlistDeclarationRegex.Matches(subset))
        {
            string element = match.Groups[1].Value;
            string body = match.Groups[2].Value;

            foreach (Match attrMatch in AttlistAttributeRegex.Matches(body))
            {
                string attr = attrMatch.Groups[1].Value;
                string type = attrMatch.Groups[2].Value;

                if (type.Equals("ID", StringComparison.Ordinal))
                {
                    if (!idAttrs.TryGetValue(element, out var list))
                        idAttrs[element] = list = new List<string>();
                    list.Add(attr);
                }
                else if (type.Equals("IDREF", StringComparison.Ordinal))
                {
                    if (!idrefAttrs.TryGetValue(element, out var list))
                        idrefAttrs[element] = list = new List<string>();
                    list.Add(attr);
                }
                else if (type.Equals("IDREFS", StringComparison.Ordinal))
                {
                    if (!idrefsAttrs.TryGetValue(element, out var list))
                        idrefsAttrs[element] = list = new List<string>();
                    list.Add(attr);
                }
            }
        }

        return new DtdAttributeInfo(idAttrs, idrefAttrs, idrefsAttrs);
    }

    private static readonly Regex AttlistDeclarationRegex = new(
        @"<!ATTLIST\s+(\S+)\s+(.*?)>",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

    private static readonly Regex AttlistAttributeRegex = new(
        @"\b(\S+)\s+(ID|IDREF|IDREFS)\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

    private static XdmValue Collection_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ResolveCollection(null, ctx, returnUris: false);

    private static XdmValue UriCollection_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ResolveCollection(null, ctx, returnUris: true);

    private static XdmValue UriCollection_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ResolveCollection(AtomizedString(args[0]), ctx, returnUris: true);

    private static XdmValue Collection_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ResolveCollection(AtomizedString(args[0]), ctx, returnUris: false);

    private static XdmValue ResolveCollection(string? uri, EvaluationContext ctx, bool returnUris)
    {
        string key = uri ?? "";
        if (ctx.CollectionValues.TryGetValue(key, out var precomputed))
        {
            // Environment collections declared via <collection><query> are evaluated by the
            // harness and stored as ready-made XDM sequences. They take precedence over the
            // document-path Collections dictionary.
            if (returnUris)
            {
                var uris = new List<XdmValue>();
                foreach (var item in FlattenValue(precomputed))
                {
                    if (item.IsNode)
                        uris.Add(XdmValue.FromString(item.NodeValue.DocumentUri, "anyURI"));
                    else
                        uris.Add(XdmValue.FromString("", "anyURI"));
                }
                return XdmValue.FromSequence(MaterializedSequence.FromList(uris));
            }
            return precomputed;
        }

        if (ctx.Collections.TryGetValue(key, out var docs))
        {
            // A declared collection is available even when it contains no documents:
            // fn:collection / fn:uri-collection then return the empty sequence
            // (collection-003's empty default collection). FODC0002/FODC0003 apply only
            // to collections that are not declared at all.
            var items = new List<XdmValue>(docs.Count);
            foreach (var doc in docs)
            {
                var (docPath, fragment) = SplitCollectionPathAndFragment(doc);
                var node = ctx.LoadDocument(docPath);
                string itemUri = node.DocumentUri;
                if (fragment != null)
                {
                    node = LoadDocumentFragment(node, fragment, itemUri);
                    itemUri += "#" + fragment;
                }
                if (returnUris)
                    items.Add(XdmValue.FromString(itemUri, "anyURI"));
                else
                    items.Add(XdmValue.FromNode(node));
            }
            return XdmValue.FromSequence(MaterializedSequence.FromList(items));
        }

        if (!string.IsNullOrEmpty(uri))
        {
            // Absolute filesystem paths are valid collection arguments even though they are
            // not RFC 3986 URIs; everything else must be a well-formed URI.
            if (!System.IO.Path.IsPathRooted(uri) && !Uri.IsWellFormedUriString(uri, UriKind.RelativeOrAbsolute))
                throw new InvalidOperationException($"FODC0004: Invalid URI: {uri}");

            // Separate path and query so that directory collections can honor the
            // W3C test-suite convention ?select=<glob> (merge-097).
            var (uriPath, uriQuery) = SplitUriPathAndQuery(uri);
            var resolved = ResolveUriAgainstBase(uriPath, ctx.BaseUri);
            if (System.IO.Directory.Exists(resolved))
            {
                string pattern = GetCollectionSelectPattern(uriQuery) ?? "*.xml";
                var files = System.IO.Directory.GetFiles(resolved, pattern);
                if (returnUris)
                {
                    var uris = new List<XdmValue>(files.Length);
                    foreach (var file in files.OrderBy(f => f, StringComparer.Ordinal))
                        uris.Add(XdmValue.FromString(new Uri(System.IO.Path.GetFullPath(file)).AbsoluteUri, "anyURI"));
                    return XdmValue.FromSequence(MaterializedSequence.FromList(uris));
                }
                var nodes = new List<XdmValue>(files.Length);
                foreach (var file in files.OrderBy(f => f, StringComparer.Ordinal))
                    nodes.Add(XdmValue.FromNode(ctx.LoadDocument(file)));
                return XdmValue.FromSequence(MaterializedSequence.FromList(nodes));
            }
        }

        if (string.IsNullOrEmpty(uri))
            throw new InvalidOperationException("FODC0003: Default collection is not available");
        throw new InvalidOperationException($"FODC0002: Collection not available: {uri}");
    }

    /// <summary>
    /// Splits an absolute file path that may carry a URI fragment (e.g. "C:\a\b.xml#frag")
    /// into the filesystem path and the fragment identifier.
    /// </summary>
    private static (string Path, string? Fragment) SplitCollectionPathAndFragment(string value)
    {
        int hash = value.IndexOf('#');
        if (hash < 0) return (value, null);
        return (value.Substring(0, hash), value.Substring(hash + 1));
    }

    /// <summary>
    /// Splits a collection URI into the path portion and the query string portion.
    /// </summary>
    private static (string Path, string? Query) SplitUriPathAndQuery(string value)
    {
        int q = value.IndexOf('?');
        if (q < 0) return (value, null);
        return (value.Substring(0, q), value.Substring(q + 1));
    }

    /// <summary>
    /// Extracts the <c>select</c> query parameter used by the W3C test suite to choose
    /// files from a directory collection (e.g. <c>?select=merge-097-*.xml</c>).
    /// </summary>
    private static string? GetCollectionSelectPattern(string? query)
    {
        if (string.IsNullOrEmpty(query)) return null;
        foreach (var part in query.Split('&'))
        {
            if (part.StartsWith("select=", StringComparison.OrdinalIgnoreCase))
                return Uri.UnescapeDataString(part.Substring("select=".Length));
        }
        return null;
    }

    /// <summary>
    /// Loads a sub-document node identified by an ID/fragment reference inside an already
    /// loaded document (e.g. <c>doc15.xml#frag2</c> from the W3C collection tests).
    /// </summary>
    private static IXdmNode LoadDocumentFragment(IXdmNode documentNode, string fragment, string documentUri)
    {
        if (documentNode is not XDocumentNode xdn || xdn.UnderlyingObject is not System.Xml.Linq.XDocument doc)
            throw new InvalidOperationException($"FODC0002: Cannot resolve fragment in collection item: {documentUri}#{fragment}");

        var element = FindElementById(doc, fragment);
        if (element == null)
            throw new InvalidOperationException($"FODC0002: Fragment not found in collection item: {documentUri}#{fragment}");

        var fragmentDoc = new System.Xml.Linq.XDocument(new System.Xml.Linq.XElement(element));
        var node = new XDocumentNode(fragmentDoc);
        node.SetDocumentUri(documentUri + "#" + fragment);
        return node;
    }

    /// <summary>
    /// Finds the first element with an <c>xml:id</c> or plain <c>id</c> attribute equal to
    /// <paramref name="id"/> anywhere in the document.
    /// </summary>
    private static System.Xml.Linq.XElement? FindElementById(System.Xml.Linq.XDocument doc, string id)
    {
        var xmlId = System.Xml.Linq.XNamespace.Xml.GetName("id");
        foreach (var e in doc.Descendants())
        {
            if ((string?)e.Attribute(xmlId) == id || (string?)e.Attribute("id") == id)
                return e;
        }
        return null;
    }

    private static IEnumerable<XdmValue> FlattenValue(XdmValue value)
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
    {
        // FO31: an empty-sequence $href yields the empty sequence.
        if (args[0].IsUndefined || IsEmptySequence(args[0]))
            return XdmValue.Undefined;
        return UnparsedText(AtomizedString(args[0]), null, ctx);
    }

    private static XdmValue UnparsedText_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (args[0].IsUndefined || IsEmptySequence(args[0]))
            return XdmValue.Undefined;
        return UnparsedText(AtomizedString(args[0]), AtomizedString(args[1]), ctx);
    }

    private static readonly HttpClient _httpClient = new HttpClient();

    private static XdmValue UnparsedText(string href, string? encoding, EvaluationContext ctx)
    {
        try
        {
            // Resolve against the static base URI first, then check for a fragment
            // identifier (which is not allowed) and apply the published-URI mapper.
            var resolved = ResolveUriAgainstBase(href, ctx.BaseUri);
            if (Uri.TryCreate(resolved, UriKind.Absolute, out var resolvedUri) &&
                !string.IsNullOrEmpty(resolvedUri.Fragment))
            {
                throw new InvalidOperationException("FOUT1170");
            }

            var path = ctx.ResourceUriMapper?.Invoke(resolved) ?? resolved;

            string content;
            if (File.Exists(path))
            {
                content = DecodeBytes(File.ReadAllBytes(path), encoding);
            }
            else if (Uri.TryCreate(path, UriKind.Absolute, out var uri) &&
                     (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                if (string.IsNullOrEmpty(encoding))
                {
                    // Let HttpClient honor the response's Content-Type charset.
                    content = _httpClient.GetStringAsync(uri).GetAwaiter().GetResult();
                }
                else
                {
                    var bytes = _httpClient.GetByteArrayAsync(uri).GetAwaiter().GetResult();
                    content = DecodeBytes(bytes, encoding);
                }
            }
            else
            {
                throw new InvalidOperationException("FOUT1170");
            }

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

    private static string DecodeBytes(byte[] bytes, string? encoding)
    {
        var bomLength = GetBomLength(bytes);
        var enc = DetectEncodingFromBytes(bytes);

        if (!string.IsNullOrEmpty(encoding))
        {
            try
            {
                enc = Encoding.GetEncoding(encoding, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
            }
            catch (ArgumentException)
            {
                throw new InvalidOperationException($"FOUT1200: Unknown encoding '{encoding}'");
            }
        }
        else if (bomLength == 0)
        {
            // Only consult the text declaration when there is no BOM; a BOM overrides
            // any encoding declaration for endianness-sensitive UTF-16/32 encodings.
            // The header probe must be lenient: a 512-byte window can split a surrogate pair.
            var lenient = (Encoding)enc.Clone();
            lenient.DecoderFallback = DecoderFallback.ReplacementFallback;
            var headerLength = Math.Min(bytes.Length - bomLength, 512);
            var header = lenient.GetString(bytes, bomLength, headerLength);
            var declaredEncoding = ExtractDeclaredEncoding(header);
            if (!string.IsNullOrEmpty(declaredEncoding))
            {
                try
                {
                    enc = Encoding.GetEncoding(declaredEncoding, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
                }
                catch (ArgumentException)
                {
                    throw new InvalidOperationException($"FOUT1200: Unknown encoding '{declaredEncoding}'");
                }
            }
        }

        try
        {
            return enc.GetString(bytes, bomLength, bytes.Length - bomLength);
        }
        catch (DecoderFallbackException)
        {
            // With an explicit encoding the spec error is FOUT1200; with an inferred
            // (default UTF-8) encoding the suite expects FOUT1190 (fn-unparsed-text-045).
            throw new InvalidOperationException(!string.IsNullOrEmpty(encoding)
                ? $"FOUT1200: Resource cannot be decoded using encoding '{encoding}'"
                : "FOUT1190: Resource is not decodable in the detected encoding");
        }
    }

    private static int GetBomLength(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return 3;
        if (bytes.Length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xFE && bytes[2] == 0x00 && bytes[3] == 0x00)
            return 4;
        if (bytes.Length >= 4 && bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xFE && bytes[3] == 0xFF)
            return 4;
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return 2;
        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            return 2;
        return 0;
    }

    private static Encoding DetectEncodingFromBytes(byte[] bytes)
    {
        // BOM-based detection. Decoders are strict (throwOnInvalidBytes) so undecodable
        // content raises FOUT1200/FOUT1190 instead of silently producing U+FFFD.
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return new UTF8Encoding(false, true);
        if (bytes.Length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xFE && bytes[2] == 0x00 && bytes[3] == 0x00)
            return new UTF32Encoding(false, false, true);
        if (bytes.Length >= 4 && bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xFE && bytes[3] == 0xFF)
            return new UTF32Encoding(true, false, true);
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return new UnicodeEncoding(false, false, true);
        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            return new UnicodeEncoding(true, false, true);

        // BOM-less XML declaration detection (UTF-16/32 declarations are self-describing).
        if (bytes.Length >= 4)
        {
            if (bytes[0] == 0x00 && bytes[1] == 0x3C && bytes[2] == 0x00 && bytes[3] == 0x3F)
                return new UnicodeEncoding(true, false, true);  // UTF-16 BE
            if (bytes[0] == 0x3C && bytes[1] == 0x00 && bytes[2] == 0x3F && bytes[3] == 0x00)
                return new UnicodeEncoding(false, false, true); // UTF-16 LE
            if (bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0x00 && bytes[3] == 0x3C)
                return new UTF32Encoding(true, false, true);    // UTF-32 BE
            if (bytes[0] == 0x3C && bytes[1] == 0x00 && bytes[2] == 0x00 && bytes[3] == 0x00)
                return new UTF32Encoding(false, false, true);   // UTF-32 LE
        }

        return new UTF8Encoding(false, true);
    }

    private static readonly Regex EncodingDeclarationRegex = new(
        @"encoding\s*=\s*[""']([^""']+)[""']",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        TimeSpan.FromSeconds(1));

    private static string? ExtractDeclaredEncoding(string header)
    {
        var trimmed = header.TrimStart();
        if (!trimmed.StartsWith("<?xml", StringComparison.Ordinal))
            return null;
        int declEnd = trimmed.IndexOf("?>", StringComparison.Ordinal);
        if (declEnd < 0)
            return null;
        var decl = trimmed.Substring(0, declEnd + 2);
        var match = EncodingDeclarationRegex.Match(decl);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static XdmValue UnparsedTextAvailable_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        // FO31: an empty-sequence $href yields false (fn-unparsed-text-available-053).
        if (args[0].IsUndefined || IsEmptySequence(args[0]))
            return XdmValue.False;
        // The $href argument is xs:string?: a non-string atomic value is a type error
        // (fn-unparsed-text-available-008).
        return UnparsedTextAvailable(RequireString(PromoteUriToString(args[0])), null, ctx);
    }

    private static XdmValue UnparsedTextAvailable_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (args[0].IsUndefined || IsEmptySequence(args[0]))
            return XdmValue.False;
        // $href is xs:string?: non-string atomic values are type errors
        // (fn-unparsed-text-available-008/010). $encoding, when supplied, must be a
        // single string; the empty sequence raises XPTY0004
        // (fn-unparsed-text-available-012).
        return UnparsedTextAvailable(RequireString(PromoteUriToString(args[0])), RequireStringRequired(args[1]), ctx);
    }

    private static XdmValue UnparsedTextAvailable(string href, string? encoding, EvaluationContext ctx)
    {
        try
        {
            // Resolve against the static base URI first, then check for a fragment
            // identifier (which makes unparsed-text fail, so availability is false).
            var resolved = ResolveUriAgainstBase(href, ctx.BaseUri);
            if (Uri.TryCreate(resolved, UriKind.Absolute, out var resolvedUri) &&
                !string.IsNullOrEmpty(resolvedUri.Fragment))
            {
                return XdmValue.False;
            }

            var path = ctx.ResourceUriMapper?.Invoke(resolved) ?? resolved;
            if (File.Exists(path))
            {
                // Spec: true iff fn:unparsed-text with the same arguments would succeed.
                // Undecodable content (FOUT1200) and non-XML characters (FOUT1190) mean false.
                try
                {
                    _ = UnparsedText(href, encoding, ctx);
                    return XdmValue.True;
                }
                catch
                {
                    return XdmValue.False;
                }
            }

            if (Uri.TryCreate(path, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                try
                {
                    using var response = _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri), HttpCompletionOption.ResponseHeadersRead)
                        .GetAwaiter().GetResult();
                    return response.IsSuccessStatusCode ? XdmValue.True : XdmValue.False;
                }
                catch
                {
                    return XdmValue.False;
                }
            }

            return XdmValue.False;
        }
        catch
        {
            return XdmValue.False;
        }
    }

    private static XdmValue UnparsedTextLines_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        // FO31: an empty-sequence $href yields the empty sequence.
        if (args[0].IsUndefined || IsEmptySequence(args[0]))
            return XdmValue.Undefined;
        return UnparsedTextLines(args[0].ToString(), null, ctx);
    }

    private static XdmValue UnparsedTextLines_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (args[0].IsUndefined || IsEmptySequence(args[0]))
            return XdmValue.Undefined;
        return UnparsedTextLines(args[0].ToString(), args[1].ToString(), ctx);
    }

    private static XdmValue UnparsedTextLines(string href, string? encoding, EvaluationContext ctx)
    {
        var textValue = UnparsedText(href, encoding, ctx);
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
        => XdmValue.FromString(XdmSerializer.Serialize(args[0], args[1],
            ctx.StaticOutputParameters is not null
                ? BuildStaticSerializationParameters(ctx)
                : null));

    private static XdmValue Error_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => throw new XPathErrorException(XPathError.ErrNs, "FOER0000", "err", "fn:error() called");

    private static XdmValue Error_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        // An empty code argument behaves as err:FOER0000 (fn-error-5/6).
        if (IsEmptySequence(args[0]))
            throw new XPathErrorException(XPathError.ErrNs, "FOER0000", "err", "fn:error() called");
        var code = args[0].QNameValue;
        throw new XPathErrorException(code.NamespaceUri, code.LocalName, code.Prefix, $"fn:error({code}) called");
    }

    private static XdmValue Error_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (IsEmptySequence(args[0]))
            throw new XPathErrorException(XPathError.ErrNs, "FOER0000", "err", args[1].ToString());
        var code = args[0].QNameValue;
        throw new XPathErrorException(code.NamespaceUri, code.LocalName, code.Prefix, args[1].ToString());
    }

    private static XdmValue Error_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (IsEmptySequence(args[0]))
            throw new XPathErrorException(XPathError.ErrNs, "FOER0000", "err", args[1].ToString(), args[2]);
        var code = args[0].QNameValue;
        throw new XPathErrorException(code.NamespaceUri, code.LocalName, code.Prefix, args[1].ToString(), args[2]);
    }

    // xs:error type constructor: the abstract error type has no instances — the empty
    // sequence is returned unchanged (xs-error-034/035/041); any non-empty value is a
    // cast error (FORG0001, xs-error-038/039).
    private static XdmValue XsError_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (IsEmptySequence(args[0]))
            return XdmValue.Undefined;
        throw new InvalidOperationException("FORG0001: Cannot cast a value to xs:error (the type is abstract and has no instances).");
    }

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

    /// <summary>
    /// Returns the single node contained in <paramref name="value"/>, or <c>null</c> if the
    /// value is the empty sequence. Raises <c>XPTY0004</c> for a non-node atomic value or a
    /// multi-item sequence (unless backwards-compatible mode allows multiple items).
    /// </summary>
    private static IXdmNode? GetOptionalSingleNode(XdmValue value, bool backwardsCompatible)
    {
        if (IsEmptySequence(value))
            return null;
        var arg = value;
        if (arg.IsSequence)
        {
            XdmValue? first = null;
            int count = 0;
            foreach (var x in XdmSequence.FromSource(arg.SequenceValue!))
            {
                // XPath 1.0 backwards compatibility keeps the FIRST item of the
                // sequence (string-003); do not overwrite it with later items.
                if (count == 0)
                    first = x;
                count++;
                if (count > 1 && !backwardsCompatible)
                    break;
            }
            if (count == 0) return null;
            if (!backwardsCompatible && count > 1)
                throw new InvalidOperationException("XPTY0004");
            arg = first!.Value;
        }
        if (!arg.IsNode)
            throw new InvalidOperationException("XPTY0004");
        return arg.NodeValue;
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
            throw new InvalidOperationException("FORG0003: fn:zero-or-one called with a sequence containing more than one item.");
        // Sequence contains exactly one item: return that item.
        foreach (var item in XdmSequence.FromSource(arg.SequenceValue!))
            return item;
        return XdmValue.Undefined;
    }

    private static XdmValue OneOrMore_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (IsEmptySequence(arg))
            throw new InvalidOperationException("FORG0004: fn:one-or-more called with an empty sequence.");
        if (!arg.IsSequence)
            return arg;
        return arg;
    }

    private static XdmValue ExactlyOne_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arg = args[0];
        if (IsEmptySequence(arg))
            throw new InvalidOperationException("FORG0005: fn:exactly-one called with an empty sequence.");
        if (!arg.IsSequence)
            return arg;
        XdmValue first = default;
        int count = 0;
        foreach (var item in XdmSequence.FromSource(arg.SequenceValue!))
        {
            first = item;
            count++;
            if (count > 1)
                throw new InvalidOperationException("FORG0005: fn:exactly-one called with a sequence containing more than one item.");
        }
        if (count == 0)
            throw new InvalidOperationException("FORG0005: fn:exactly-one called with an empty sequence.");
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
        // A node with no base URI of its own (constructed documents and their content)
        // inherits the static base URI of the query — elements and documents only;
        // comments, PIs, and text have no base URI (fn-base-uri-4/5 vs 12/32).
        if (string.IsNullOrEmpty(uri) && item.NodeValue.NodeKind is XdmNodeKind.Element or XdmNodeKind.Document)
            uri = ctx.BaseUri;
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
                if (first == null)
                    first = x;
                count++;
                if (!ctx.BackwardsCompatible && count > 1)
                    break;
            }
            if (count == 0) return XdmValue.Undefined;
            if (!ctx.BackwardsCompatible && count > 1) throw new InvalidOperationException("XPTY0004");
            arg = first!.Value;
        }
        if (!arg.IsNode)
            throw new InvalidOperationException("XPTY0004: fn:base-uri() argument is not a node.");
        var uri = arg.NodeValue!.BaseUri;
        if (string.IsNullOrEmpty(uri) && arg.NodeValue.NodeKind is XdmNodeKind.Element or XdmNodeKind.Document)
            uri = ctx.BaseUri;
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
        return string.IsNullOrEmpty(uri) ? XdmValue.Undefined : XdmValue.FromString(uri, "anyURI");
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
        return string.IsNullOrEmpty(uri) ? XdmValue.Undefined : XdmValue.FromString(uri, "anyURI");
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
        // returns the empty sequence; for elements it returns true when the node is
        // nilled according to its PSVI annotation, otherwise false.
        if (node.NodeKind != XdmNodeKind.Element)
            return XdmValue.Undefined;
        return node.IsNilled ? XdmValue.True : XdmValue.False;
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
        long pos = RequireInteger(posValue, ctx.BackwardsCompatible);
        if (pos < 1 || pos > target.Count)
            return XdmValue.FromSequence(MaterializedSequence.FromList(target));
        target.RemoveAt((int)pos - 1);
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
        double startD = ctx.BackwardsCompatible ? ToDoubleValue(args[1]) : ToDoubleValueStrict(args[1]);
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
        double startD = ctx.BackwardsCompatible ? ToDoubleValue(args[1]) : ToDoubleValueStrict(args[1]);
        double lenD = ctx.BackwardsCompatible ? ToDoubleValue(args[2]) : ToDoubleValueStrict(args[2]);
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
        => DistinctValuesImpl(args[0], ctx.DefaultCollation, ctx.ImplicitTimezoneOffsetMinutes);

    private static XdmValue DistinctValuesImpl(XdmValue sequence, string collation, int implicitTimezoneOffsetMinutes)
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
                if (AtomicValuesEqual(atomized, s, collation, implicitTimezoneOffsetMinutes) || BothNaN(atomized, s))
                {
                    isDistinct = false;
                    break;
                }
            }
            if (isDistinct)
            {
                seen.Add(atomized);
                // fn:distinct-values returns the atomized values, not the original nodes.
                result.Add(atomized);
            }
        }
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static bool TypedStringValuesEqual(XdmValue a, XdmValue b, string collation, int implicitTimezoneOffsetMinutes)
    {
        // Values stored as XdmValueKind.String cover several XSD type families. Two values
        // are equal only when they belong to the same family and the family-specific equality
        // rule says so. Cross-family values (e.g. xs:gYear vs xs:string, xs:hexBinary vs
        // xs:base64Binary) are not comparable and are therefore distinct.
        var aFamily = StringTypeFamily(a.SchemaTypeName);
        var bFamily = StringTypeFamily(b.SchemaTypeName);

        if (aFamily != bFamily)
            return false;

        switch (aFamily)
        {
            case StringTypeFamilyKind.String:
                return CompareStrings(a.StringValue, b.StringValue, collation) == 0;

            case StringTypeFamilyKind.GDate:
                var aSubtype = GDateSubtypeName(a.SchemaTypeName);
                var bSubtype = GDateSubtypeName(b.SchemaTypeName);
                if (aSubtype is null || aSubtype != bSubtype)
                    return false;
                var cmp = VmEngine.CompareDateTimeValues(a, b, aSubtype, implicitTimezoneOffsetMinutes);
                return cmp.HasValue ? cmp.Value == 0 : string.CompareOrdinal(a.StringValue, b.StringValue) == 0;

            case StringTypeFamilyKind.HexBinary:
                return BinaryValueEquals(a.StringValue, b.StringValue, fromHex: true);

            case StringTypeFamilyKind.Base64Binary:
                return BinaryValueEquals(a.StringValue, b.StringValue, fromHex: false);

            case StringTypeFamilyKind.Other:
                // Unknown annotated string types compare only when they share the exact type name.
                return string.Equals(a.SchemaTypeName, b.SchemaTypeName, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(a.StringValue, b.StringValue, StringComparison.Ordinal);

            default:
                return false;
        }
    }

    private enum StringTypeFamilyKind { String, GDate, HexBinary, Base64Binary, Other }

    private static StringTypeFamilyKind StringTypeFamily(string? schemaTypeName) => schemaTypeName?.ToLowerInvariant() switch
    {
        null or "" or "string" or "untypedatomic" or "anyuri"
            or "normalizedstring" or "token" or "language" or "nmtoken" or "nmtokens"
            or "name" or "ncname" or "id" or "idref" or "idrefs" or "entity" or "entities"
            => StringTypeFamilyKind.String,
        "gyear" or "gyearmonth" or "gmonth" or "gmonthday" or "gday"
            => StringTypeFamilyKind.GDate,
        "hexbinary" => StringTypeFamilyKind.HexBinary,
        "base64binary" => StringTypeFamilyKind.Base64Binary,
        _ => StringTypeFamilyKind.Other
    };

    private static string? GDateSubtypeName(string? schemaTypeName) => schemaTypeName?.ToLowerInvariant() switch
    {
        "gyear" => "gYear",
        "gyearmonth" => "gYearMonth",
        "gmonth" => "gMonth",
        "gmonthday" => "gMonthDay",
        "gday" => "gDay",
        _ => null
    };

    private static bool BinaryValueEquals(string a, string b, bool fromHex)
    {
        try
        {
            var bytesA = fromHex ? Convert.FromHexString(a) : Convert.FromBase64String(a);
            var bytesB = fromHex ? Convert.FromHexString(b) : Convert.FromBase64String(b);
            return bytesA.SequenceEqual(bytesB);
        }
        catch
        {
            // If the lexical forms cannot be decoded, fall back to exact lexical comparison.
            return string.Equals(a, b, StringComparison.Ordinal);
        }
    }

    private static bool BothNaN(XdmValue a, XdmValue b)
    {
        if (!IsNumericValue(a) || !IsNumericValue(b))
            return false;
        bool aIsNaN = (a.Kind is XdmValueKind.Double or XdmValueKind.Float) && double.IsNaN(a.DoubleValue);
        bool bIsNaN = (b.Kind is XdmValueKind.Double or XdmValueKind.Float) && double.IsNaN(b.DoubleValue);
        return aIsNaN && bIsNaN;
    }

    private static XdmValue DistinctValues_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string collation = AtomizedString(args[1]);
        ValidateCollation(collation);
        return DistinctValuesImpl(args[0], collation, ctx.ImplicitTimezoneOffsetMinutes);
    }

    /// <summary>
    /// Compares two atomized XDM values using XPath <c>eq</c> semantics and the
    /// supplied collation for string comparisons. Date/time values without an explicit
    /// timezone are treated as having <paramref name="implicitTimezoneOffsetMinutes"/>.
    /// </summary>
    private static bool AtomicValuesEqual(XdmValue a, XdmValue b, string collation, int implicitTimezoneOffsetMinutes)
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
            XdmValueKind.String => TypedStringValuesEqual(a, b, collation, implicitTimezoneOffsetMinutes),
            XdmValueKind.Boolean => a.BooleanValue == b.BooleanValue,
            XdmValueKind.Integer => a.IntegerValue == b.IntegerValue,
            XdmValueKind.Decimal => a.DecimalValue == b.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => a.DoubleValue == b.DoubleValue,
            XdmValueKind.DateTime => DateTimeValuesEqual(a, b, XdmValueKind.DateTime, implicitTimezoneOffsetMinutes),
            XdmValueKind.Date => DateTimeValuesEqual(a, b, XdmValueKind.Date, implicitTimezoneOffsetMinutes),
            XdmValueKind.Time => DateTimeValuesEqual(a, b, XdmValueKind.Time, implicitTimezoneOffsetMinutes),
            XdmValueKind.QName => a.QNameValue.Equals(b.QNameValue),
            XdmValueKind.Uri => CompareStrings(a.StringValue, b.StringValue, collation) == 0,
            XdmValueKind.Duration => DurationValuesEqual(a, b),
            _ => false
        };
    }

    private static bool DurationValuesEqual(XdmValue a, XdmValue b)
    {
        var (aYears, aMonths, aDays, aHours, aMinutes, aSeconds) = ParseDuration(a.DurationValue);
        var (bYears, bMonths, bDays, bHours, bMinutes, bSeconds) = ParseDuration(b.DurationValue);
        bool aNegative = a.DurationValue.StartsWith('-');
        bool bNegative = b.DurationValue.StartsWith('-');
        long aTotalMonths = (aYears * 12 + aMonths) * (aNegative ? -1 : 1);
        long bTotalMonths = (bYears * 12 + bMonths) * (bNegative ? -1 : 1);
        decimal aTotalSeconds = (aDays * 86400m + aHours * 3600m + aMinutes * 60m + aSeconds) * (aNegative ? -1 : 1);
        decimal bTotalSeconds = (bDays * 86400m + bHours * 3600m + bMinutes * 60m + bSeconds) * (bNegative ? -1 : 1);
        return aTotalMonths == bTotalMonths && aTotalSeconds == bTotalSeconds;
    }

    /// <summary>
    /// Compares date, time, or dateTime values per XPath eq semantics, treating values
    /// without an explicit timezone as having the implicit timezone.
    /// </summary>
    private static bool DateTimeValuesEqual(XdmValue a, XdmValue b, XdmValueKind kind, int implicitTimezoneOffsetMinutes)
    {
        XPathDateTime GetXdt(XdmValue v)
        {
            return kind switch
            {
                XdmValueKind.DateTime => v.DateTimeXPathValue,
                XdmValueKind.Date => v.DateXPathValue,
                XdmValueKind.Time => v.TimeXPathValue,
                _ => throw new InvalidOperationException()
            };
        }

        bool aHasTz = a.HasTimezone;
        bool bHasTz = b.HasTimezone;
        var aXdt = GetXdt(a);
        var bXdt = GetXdt(b);

        var aEffective = aHasTz ? aXdt : new XPathDateTime(aXdt.Year, aXdt.Month, aXdt.Day, aXdt.Hour, aXdt.Minute, aXdt.Second, aXdt.Millisecond, implicitTimezoneOffsetMinutes, true);
        var bEffective = bHasTz ? bXdt : new XPathDateTime(bXdt.Year, bXdt.Month, bXdt.Day, bXdt.Hour, bXdt.Minute, bXdt.Second, bXdt.Millisecond, implicitTimezoneOffsetMinutes, true);

        var aUtc = XPathDateTimeHelper.NormalizeToUtc(aEffective);
        var bUtc = XPathDateTimeHelper.NormalizeToUtc(bEffective);
        return XPathDateTimeHelper.CompareComponents(aUtc, bUtc) == 0;
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
        => IndexOfImpl(args[0], args[1], ctx.DefaultCollation, ctx.ImplicitTimezoneOffsetMinutes);

    private static XdmValue IndexOfImpl(XdmValue sequence, XdmValue search, string collation, int implicitTimezoneOffsetMinutes)
    {
        // $search must be a single item (xs:anyAtomicType, not empty/multi sequence).
        if (IsEmptySequence(search))
            throw new InvalidOperationException("XPTY0004: fn:index-of search argument must be a single item.");
        if (search.IsSequence && SequenceLength(search) > 1)
            throw new InvalidOperationException("XPTY0004: fn:index-of search argument must be a single item.");

        var atomizedSearch = AtomizeValue(search);
        var seq = Materialize(sequence);
        var result = new List<XdmValue>();
        for (int i = 0; i < seq.Count; i++)
        {
            var atomizedItem = AtomizeValue(seq[i]);
            if (AtomicValuesEqual(atomizedItem, atomizedSearch, collation, implicitTimezoneOffsetMinutes))
                result.Add(XdmValue.FromInteger(i + 1));
        }
        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private static XdmValue IndexOf_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string collation = RequireStringRequired(args[2]);
        ValidateCollation(collation);
        return IndexOfImpl(args[0], args[1], collation, ctx.ImplicitTimezoneOffsetMinutes);
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
            else if (IsUntypedAtomic(a) && double.TryParse(a.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
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
        string sep = RequireStringRequired(args[1], ctx.BackwardsCompatible);
        var strings = new List<string>(items.Count);
        foreach (var item in items)
            strings.Add(AtomizedString(item));
        return XdmValue.FromString(string.Join(sep, strings));
    }

    private static XdmValue ConcatN(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var sb = new StringBuilder();
        foreach (var arg in args)
        {
            // Each fn:concat argument is xs:anyAtomicType?: atomization must yield at
            // most one atomic value, so a multi-item argument is a type error
            // (K2-ConcatFunc-1/2/3).
            if (arg.IsSequence && arg.SequenceValue is not null)
            {
                int count = 0;
                foreach (var unused in XdmSequence.FromSource(arg.SequenceValue))
                {
                    if (++count > 1)
                        throw new InvalidOperationException(
                            "XPTY0004: fn:concat arguments must atomize to a single atomic value or the empty sequence.");
                }
            }
            sb.Append(AtomizedString(arg));
        }
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
        var maps = Materialize(args[0]);

        // F+O 3.1 §14.5.1: the options map may carry a 'duplicates' entry selecting
        // the duplicate-key strategy: use-first (default) | use-last | use-any |
        // combine | reject. An empty-sequence options argument is a type error.
        string duplicates = "use-first";
        if (args.Length > 1)
        {
            var opts = args[1];
            if (opts.IsUndefined || !opts.IsMap)
                throw new InvalidOperationException("XPTY0004: map:merge options must be a single map");
            if (opts.MapValue.TryGetValue(XdmValue.FromString("duplicates"), out var dupOpt))
            {
                duplicates = AtomizeValue(dupOpt).ToString();
                switch (duplicates)
                {
                    case "use-first": case "use-last": case "use-any": case "combine": case "reject":
                        break;
                    default:
                        throw new InvalidOperationException($"FOJS0005: Invalid value for the duplicates option of map:merge: '{duplicates}'");
                }
            }
        }

        var result = new XdmMap();
        foreach (var mapVal in maps)
        {
            if (!mapVal.IsMap)
                continue;
            foreach (var kvp in mapVal.MapValue.Entries)
            {
                if (!result.TryGetValue(kvp.Key, out var existing))
                {
                    result.Add(kvp.Key, kvp.Value);
                    continue;
                }
                switch (duplicates)
                {
                    case "use-first":
                    case "use-any": // implementation-defined choice; we keep the first
                        break;
                    case "combine":
                    {
                        // The associated value is the concatenation of all values for
                        // the key, in input order (map-merge-025/027).
                        var combined = new List<XdmValue>();
                        combined.AddRange(AsSequence(existing));
                        combined.AddRange(AsSequence(kvp.Value));
                        result.Add(kvp.Key, XdmValue.FromSequence(MaterializedSequence.FromList(combined)));
                        break;
                    }
                    case "reject":
                        throw new InvalidOperationException("FOJS0003: map:merge found duplicate keys and the duplicates option is 'reject'");
                    default: // use-last
                        result.Add(kvp.Key, kvp.Value);
                        break;
                }
            }
        }
        return XdmValue.FromMap(result);
    }

    private static XdmValue MapRemove(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var map = args[0].MapValue;
        // map:remove accepts a sequence of keys (spec bug 29660): every listed key
        // is removed; each key must be a single atomic value.
        var keys = new List<XdmValue>();
        foreach (var k in AsSequence(args[1]))
            keys.Add(AtomizeMapKey(k));

        var result = map;
        foreach (var key in keys)
            result = result.WithRemoved(key);

        return XdmValue.FromMap(result);
    }

    private static XdmValue MapPut(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var map = args[0].MapValue;
        var key = AtomizeMapKey(args[1]);
        var value = args[2];

        return XdmValue.FromMap(map.WithAdded(key, value));
    }

    // ------------------------------------------------------------------
    // Array functions
    // ------------------------------------------------------------------

    private static XdmValue ArraySize(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromInteger(args[0].ArrayValue.Count);

    private static XdmValue ArrayGet(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arr = args[0].ArrayValue;
        long idx = args[1].IntegerValue;
        if (idx < 1 || idx > arr.Count)
            throw new InvalidOperationException($"FOAY0001: Array index {idx} is out of bounds (array size {arr.Count}).");
        return arr.Get((int)idx);
    }

    private static XdmValue ArrayContains(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XdmValue.FromBoolean(args[0].ArrayValue.Contains(args[1]));

    private static XdmValue ArrayHead(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arr = args[0].ArrayValue;
        if (arr.Count == 0)
            throw new InvalidOperationException("FOAY0001: array:head is not defined for the empty array.");
        return arr.Get(1);
    }

    private static XdmValue ArrayPut(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arr = args[0].ArrayValue;
        long idx = args[1].IntegerValue;
        var value = args[2];
        if (idx < 1 || idx > arr.Count)
            throw new InvalidOperationException($"FOAY0001: array:put position {idx} is out of bounds (array size {arr.Count}).");
        var items = new List<XdmValue>();
        foreach (var item in arr.Values)
            items.Add(item);
        // XPath arrays are 1-based
        items[(int)idx - 1] = value;
        return XdmValue.FromArray(new XdmArray(items));
    }

    private static XdmValue ArrayTail(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var arr = args[0].ArrayValue;
        if (arr.Count == 0)
            throw new InvalidOperationException("FOAY0001: array:tail is not defined for the empty array.");
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
        var removePositions = new HashSet<long>();
        foreach (var posVal in AsSequence(args[1]))
        {
            long pos = posVal.IntegerValue;
            if (pos < 1 || pos > arr.Count)
                throw new InvalidOperationException($"FOAY0001: array:remove position {pos} is out of bounds (array size {arr.Count}).");
            removePositions.Add(pos);
        }
        var items = new List<XdmValue>();
        long idx = 1;
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

    private static XdmValue MapFind(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var key = AtomizeMapKey(args[1]);
        var found = new XdmArray();
        MapFindInto(args[0], key, found);
        return XdmValue.FromArray(found);
    }

    /// <summary>
    /// Recursively searches maps and arrays in the input for the given key, appending
    /// each matching value to <paramref name="found"/> (map:find, F+O 3.1 §14.4.6).
    /// </summary>
    private static void MapFindInto(XdmValue input, XdmValue key, XdmArray found)
    {
        if (input.IsUndefined)
            return;
        if (input.IsMap)
        {
            var map = input.MapValue;
            if (map.TryGetValue(key, out var hit))
                found.Add(hit);
            foreach (var value in map.Values)
                MapFindInto(value, key, found);
            return;
        }
        if (input.IsArray)
        {
            foreach (var member in input.ArrayValue.Values)
                MapFindInto(member, key, found);
            return;
        }
        if (input.IsSequence)
        {
            foreach (var item in XdmSequence.FromSource(input.SequenceValue!))
                MapFindInto(item, key, found);
        }
        // Atomic values and nodes are ignored.
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
        => ArraySubarray(ctx, args[0].ArrayValue, args[1].IntegerValue, null);

    private static XdmValue ArraySubarray_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => ArraySubarray(ctx, args[0].ArrayValue, args[1].IntegerValue, args[2].IntegerValue);

    /// <summary>
    /// F+O 3.1 §16.3.4: FOAY0001 when $start is less than 1 or when $start+$length-1
    /// exceeds the array size; FOAY0002 when $length is negative. The 2-argument form
    /// uses an implicit length of size-$start+1.
    /// </summary>
    private static XdmValue ArraySubarray(EvaluationContext ctx, XdmArray arr, long start, long? lengthArg)
    {
        long length = lengthArg ?? (arr.Count - start + 1);
        if (start < 1)
            throw new InvalidOperationException($"FOAY0001: array:subarray start {start} is out of bounds (array size {arr.Count}).");
        if (length < 0)
            throw new InvalidOperationException($"FOAY0002: array:subarray length {length} is negative.");
        if (start + length - 1 > arr.Count)
            throw new InvalidOperationException($"FOAY0001: array:subarray range {start}..{start + length - 1} exceeds the array size {arr.Count}.");
        var items = new List<XdmValue>();
        long i = 1;
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

        string? collationUri = collation is not null && !IsEmptySequence(collation.Value)
            ? collation.ToString()
            : (string.IsNullOrEmpty(ctx.DefaultCollation) ? null : ctx.DefaultCollation);
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
        long pos = args[1].IntegerValue;
        var value = args[2];
        if (pos < 1 || pos > arr.Count + 1L)
            throw new InvalidOperationException($"FOAY0001: array:insert-before position {pos} is out of bounds (array size {arr.Count}).");
        var items = new List<XdmValue>();
        long i = 1;
        foreach (var item in arr.Values)
        {
            if (i == pos)
                items.Add(value);
            items.Add(item);
            i++;
        }
        if (pos == arr.Count + 1L)
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
    private static string RequireString(XdmValue value, bool backwardsCompatible = false)
    {
        if (value.IsUndefined)
            return string.Empty;

        if (value.IsNode)
        {
            if (backwardsCompatible)
                return value.NodeValue.StringValue;
            value = AtomizeValue(value);
            // AtomizeValue returns Undefined only for empty sequence; fall through to handle it.
        }

        if (value.IsFunction || value.IsMap || value.IsArray)
            throw new InvalidOperationException("FOTY0013");

        if (value.IsSequence)
        {
            XdmValue? first = null;
            int count = 0;
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
            {
                if (first == null)
                    first = item;
                count++;
                if (!backwardsCompatible && count > 1)
                    break;
            }
            if (count == 0)
                return string.Empty;
            if (!backwardsCompatible && count > 1)
                throw new InvalidOperationException("XPTY0004");
            return RequireString(first!.Value, backwardsCompatible);
        }

        if (value.Kind == XdmValueKind.String)
            return value.StringValue;

        if (string.Equals(value.SchemaTypeName, "untypedAtomic", StringComparison.OrdinalIgnoreCase))
            return value.ToString();

        if (backwardsCompatible && value.IsAtomic)
            return value.ToString();

        throw new InvalidOperationException("XPTY0004");
    }

    /// <summary>
    /// Like <see cref="RequireString"/> but for required (non-optional) string parameters:
    /// the empty sequence raises XPTY0004 instead of returning "".
    /// </summary>
    private static string RequireStringRequired(XdmValue value, bool backwardsCompatible = false)
    {
        if (value.IsUndefined)
            throw new InvalidOperationException("XPTY0004");
        if (value.IsSequence)
        {
            bool any = false;
            foreach (var unused in XdmSequence.FromSource(value.SequenceValue!))
            {
                any = true;
                break;
            }
            if (!any)
                throw new InvalidOperationException("XPTY0004");
        }
        return RequireString(value, backwardsCompatible);
    }

    /// <summary>
    /// Validates that the value is suitable for an xs:integer parameter.
    /// Atomizes if needed; empty sequence and non-integer atomics (including decimal/double/float)
    /// raise XPTY0004. xs:untypedAtomic is parsed as an integer.
    /// </summary>
    private static long RequireInteger(XdmValue value, bool backwardsCompatible = false)
    {
        if (value.IsUndefined)
            throw new InvalidOperationException("XPTY0004");

        XdmValue atomized = value.IsNode ? AtomizeValue(value) : value;

        if (atomized.IsSequence)
        {
            bool any = false;
            foreach (var unused in XdmSequence.FromSource(atomized.SequenceValue!))
            {
                any = true;
                break;
            }
            if (!any)
                throw new InvalidOperationException("XPTY0004");
        }

        if (atomized.Kind == XdmValueKind.Integer)
            return atomized.IntegerValue;

        if (IsUntypedAtomic(atomized))
        {
            if (long.TryParse(atomized.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                return parsed;
            throw new InvalidOperationException("XPTY0004");
        }

        if (backwardsCompatible && atomized.IsAtomic)
        {
            // XPath 1.0 compatibility: numeric values are coerced to integer.
            if (atomized.Kind == XdmValueKind.Decimal)
                return (long)atomized.DecimalValue;
            if (atomized.Kind is XdmValueKind.Double or XdmValueKind.Float)
                return (long)atomized.DoubleValue;
            if (atomized.Kind == XdmValueKind.Integer)
                return atomized.IntegerValue;
        }

        throw new InvalidOperationException("XPTY0004");
    }

    private static XdmValue AtomizeValue(XdmValue value)
    {
        if (value.IsUndefined)
            return XdmValue.Undefined;

        // Atomizing a function item is a type error (FOTY0013), e.g. number(f#1).
        if (value.IsFunction)
            throw new InvalidOperationException("FOTY0013: Cannot atomize a function item");

        if (value.IsNode)
        {
            // XDM §2.7.2: typed value of comments and PIs is xs:string;
            // for elements, attributes, text, and document nodes in the untyped
            // case it is xs:untypedAtomic; for schema-validated nodes it is the
            // PSVI typed value, and element-only/empty complex types raise FOTY0012.
            var node = value.NodeValue;
            if (node.NodeKind is XdmNodeKind.ProcessingInstruction or XdmNodeKind.Comment)
                return XdmValue.FromString(node.StringValue);
            if (node.SchemaTypeAnnotation is null)
                return XdmValue.FromString(node.StringValue, "untypedAtomic");
            if (node.HasNoTypedValue)
                throw new InvalidOperationException("FOTY0012: Cannot atomize a node that has no typed value.");
            return node.TypedValue;
        }

        if (value.IsSequence)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                return AtomizeValue(item);
            return XdmValue.Undefined;
        }

        return value;
    }

    /// <summary>
    /// Atomizes a value that must be a single atomic value or the empty sequence.
    /// Multi-item sequences raise <c>XPTY0004</c>; function items raise <c>FOTY0013</c>.
    /// </summary>
    private static XdmValue AtomizeSingleton(XdmValue value)
    {
        if (value.IsUndefined)
            return XdmValue.Undefined;

        if (value.IsFunction || value.IsMap || value.IsArray)
            throw new InvalidOperationException("FOTY0013: Cannot atomize a function item, map, or array");

        if (value.IsSequence)
        {
            XdmValue single = XdmValue.Undefined;
            int count = 0;
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
            {
                count++;
                if (count > 1)
                    throw new InvalidOperationException("XPTY0004: Expected a single atomic value, but got a sequence of multiple items.");
                single = item;
            }
            if (count == 0)
                return XdmValue.Undefined;
            value = single;
        }

        return AtomizeValue(value);
    }

    /// <summary>
    /// Atomizes a map key expression. The key must be a single atomic value:
    /// the empty sequence or a multi-item sequence is a type error (XPTY0004),
    /// and function items cannot be atomized (FOTY0013).
    /// </summary>
    private static XdmValue AtomizeMapKey(XdmValue value)
    {
        if (value.IsFunction || value.IsMap || value.IsArray)
            throw new InvalidOperationException("FOTY0013");
        if (value.IsUndefined)
            throw new InvalidOperationException("XPTY0004: A map key must be a single atomic value, not the empty sequence");
        if (value.IsSequence)
        {
            XdmValue single = XdmValue.Undefined;
            int count = 0;
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
            {
                count++;
                if (count > 1)
                    throw new InvalidOperationException("XPTY0004: A map key must be a single atomic value, not a sequence");
                single = item;
            }
            if (count == 0)
                throw new InvalidOperationException("XPTY0004: A map key must be a single atomic value, not the empty sequence");
            value = single;
        }
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
        // Atomize every input item and flatten any list-typed node into its
        // constituent atomic values (cbcl-data-004: sum of elements whose typed
        // value is a list of xs:integer).
        var atomized = new List<XdmValue>();
        foreach (var item in items)
        {
            var a = AtomizeValue(item);
            if (a.IsSequence)
            {
                foreach (var sub in XdmSequence.FromSource(a.SequenceValue!))
                    atomized.Add(sub);
            }
            else
            {
                atomized.Add(a);
            }
        }

        bool allIntegerOrDecimal = true;
        bool anyDouble = false;
        bool anyUntyped = false;
        bool allYearMonthDuration = true;
        bool allDayTimeDuration = true;
        foreach (var a in atomized)
        {
            // Only numeric, duration, and xs:untypedAtomic items may be summed;
            // xs:string, xs:boolean and other types are FORG0006.
            if (a.Kind is not (XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double
                or XdmValueKind.Float or XdmValueKind.Duration)
                && !IsUntypedAtomic(a))
                throw new InvalidOperationException(
                    "FORG0006: fn:sum() requires a sequence of numeric or xs:duration values");
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
            foreach (var a in atomized)
            {
                var s = a.Kind == XdmValueKind.Duration ? a.DurationValue : a.ToString();
                var (years, months, _, _, _, _) = ParseDuration(s);
                totalMonths += years * 12 + months;
            }
            return XdmValue.FromDuration(FormatYearMonthDuration(totalMonths));
        }

        if (allDayTimeDuration)
        {
            decimal totalSeconds = 0m;
            foreach (var a in atomized)
            {
                var s = a.Kind == XdmValueKind.Duration ? a.DurationValue : a.ToString();
                var (_, _, days, hours, minutes, seconds) = ParseDuration(s);
                totalSeconds += days * 86400m + hours * 3600m + minutes * 60m + seconds;
            }
            return XdmValue.FromDuration(FormatDayTimeDurationFromSeconds(totalSeconds));
        }

        // A duration that survived the homogeneous-type branches above is mixed with
        // an incompatible type (numerics or the other duration subtype): FORG0006.
        if (atomized.Any(a => a.Kind == XdmValueKind.Duration))
            throw new InvalidOperationException(
                "FORG0006: fn:sum() cannot combine xs:duration with numeric or other duration subtypes");

        if (allIntegerOrDecimal)
        {
            bool allInteger = atomized.All(a => a.Kind == XdmValueKind.Integer);
            if (allInteger)
            {
                // Sum of a single item is the item itself — keep its type annotation
                // (e.g. xs:unsignedShort) per the F+O least-common-type rule.
                if (atomized.Count == 1)
                    return atomized[0];
                long intSum = 0;
                foreach (var a in atomized)
                    intSum += a.IntegerValue;
                return XdmValue.FromInteger(intSum);
            }
            decimal decSum = 0m;
            foreach (var a in atomized)
                decSum += a.Kind == XdmValueKind.Integer ? a.IntegerValue : a.DecimalValue;
            return XdmValue.FromDecimal(decSum);
        }
        if (!anyDouble && !anyUntyped)
        {
            float sumF = 0.0f;
            foreach (var a in atomized)
                sumF += (float)ToDoubleValue(a);
            return XdmValue.FromFloat(sumF);
        }
        double sumD = 0.0;
        foreach (var a in atomized)
            sumD += ToDoubleValue(a);
        return XdmValue.FromDouble(sumD);
    }

    private static XdmValue MinMax(List<XdmValue> items, bool min, string collation)
    {
        var atomized = items.Select(AtomizeValue).ToList();

        // XPath spec: xs:untypedAtomic values (including atomized untyped nodes) are
        // cast to xs:double before comparison; an uncastable value raises FORG0001.
        for (int i = 0; i < atomized.Count; i++)
            if (IsUntypedAtomic(atomized[i]))
                atomized[i] = XdmValue.FromDouble(CastUntypedAtomicToDouble(atomized[i].StringValue));

        // All booleans: xs:boolean is orderable (false < true) and the result is a
        // boolean — not the 0/1 numeric coercion (cbcl-min-001/002).
        if (atomized.Count > 0 && atomized.All(a => a.Kind == XdmValueKind.Boolean))
        {
            var result = atomized[0].BooleanValue;
            for (int i = 1; i < atomized.Count; i++)
            {
                if (min ? (!atomized[i].BooleanValue && result) : (atomized[i].BooleanValue && !result))
                    result = atomized[i].BooleanValue;
            }
            return XdmValue.FromBoolean(result);
        }
        // Booleans mixed with any other type are not comparable (cbcl-min-003).
        if (atomized.Any(a => a.Kind == XdmValueKind.Boolean))
            throw new InvalidOperationException("FORG0006: fn:min/fn:max arguments must not mix xs:boolean with other types");

        // Date/time family: each kind is orderable only within itself (dates, times,
        // dateTimes, and durations within one orderable subtype); any mix is FORG0006
        // (cbcl-min-006/014/016/017).
        if (atomized.All(a => a.Kind is XdmValueKind.DateTime or XdmValueKind.Date or XdmValueKind.Time or XdmValueKind.Duration))
        {
            var kinds = new HashSet<XdmValueKind>(atomized.Select(a => a.Kind));
            if (kinds.Count > 1)
                throw new InvalidOperationException("FORG0006: fn:min/fn:max arguments must not mix date/time kinds");
            if (kinds.Contains(XdmValueKind.Duration))
            {
                // Only the orderable duration subtypes compare — plain xs:duration is
                // FORG0006 by type annotation (cbcl-min-008, fn-max-9/fn-min-9); values
                // without an annotation fall back to the lexical pattern, and the two
                // subtypes do not mix.
                bool anyYearMonth = false, anyDayTime = false;
                foreach (var a in atomized)
                {
                    var lexical = a.ToString();
                    if (a.SchemaTypeName?.Equals("duration", StringComparison.OrdinalIgnoreCase) == true)
                        throw new InvalidOperationException("FORG0006: xs:duration is not orderable (only xs:yearMonthDuration and xs:dayTimeDuration are)");
                    if (!IsOrderableDurationLexical(lexical))
                        throw new InvalidOperationException("FORG0006: xs:duration is not orderable (only xs:yearMonthDuration and xs:dayTimeDuration are)");
                    anyYearMonth |= IsYearMonthDurationString(lexical);
                    anyDayTime |= IsDayTimeDurationString(lexical);
                }
                if (anyYearMonth && anyDayTime)
                    throw new InvalidOperationException("FORG0006: fn:min/fn:max arguments must not mix xs:yearMonthDuration and xs:dayTimeDuration");
            }
            var result = atomized[0];
            for (int i = 1; i < atomized.Count; i++)
            {
                var cmp = CompareDateTimeValues(atomized[i], result);
                if (min ? cmp < 0 : cmp > 0)
                    result = atomized[i];
            }
            return result;
        }

        // All string — after the untypedAtomic conversion above, only true xs:string
        // values remain, so a string comparison (with collation) is correct.
        bool allString = atomized.All(a => a.Kind == XdmValueKind.String);
        if (allString)
        {
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

        // Anything that is not numeric cannot be compared against numbers or mixed
        // with strings: FORG0006.
        bool allNumeric = true;
        foreach (var a in atomized)
            if (a.Kind is not (XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Float or XdmValueKind.Double))
                allNumeric = false;
        if (!allNumeric)
            throw new InvalidOperationException("FORG0006: fn:min/fn:max requires arguments of a single comparable type");

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
            // fn:min/fn:max return the selected item converted to the least common
            // type of the input: all xs:integer input yields the winning item itself
            // (preserving a subtype annotation such as xs:unsignedShort).
            bool anyDecimal = atomized.Any(a => a.Kind == XdmValueKind.Decimal);
            int winner = 0;
            decimal best = ToDecimalValue(atomized[0]);
            for (int i = 1; i < atomized.Count; i++)
            {
                decimal v = ToDecimalValue(atomized[i]);
                if (min ? v < best : v > best)
                {
                    best = v;
                    winner = i;
                }
            }
            return anyDecimal ? XdmValue.FromDecimal(best) : atomized[winner];
        }
        if (!anyDouble)
        {
            float resultF = (float)ToDoubleValue(atomized[0]);
            bool nanF = float.IsNaN(resultF);
            for (int i = 1; i < atomized.Count; i++)
            {
                float v = (float)ToDoubleValue(atomized[i]);
                if (float.IsNaN(v)) { nanF = true; continue; }
                if (!nanF && (min ? v < resultF : v > resultF))
                    resultF = v;
            }
            // XPath spec: if the converted sequence contains NaN, the result is NaN.
            return XdmValue.FromFloat(nanF ? float.NaN : resultF);
        }
        double resultD = ToDoubleValue(atomized[0]);
        bool nanD = double.IsNaN(resultD);
        for (int i = 1; i < atomized.Count; i++)
        {
            double v = ToDoubleValue(atomized[i]);
            if (double.IsNaN(v)) { nanD = true; continue; }
            if (!nanD && (min ? v < resultD : v > resultD))
                resultD = v;
        }
        // XPath spec: if the converted sequence contains NaN, the result is NaN.
        return XdmValue.FromDouble(nanD ? double.NaN : resultD);
    }

    /// <summary>
    /// Determines lexically whether a duration value is an orderable subtype
    /// (xs:yearMonthDuration or xs:dayTimeDuration) rather than a generic xs:duration
    /// mixing year/month with day/time components.
    /// </summary>
    private static bool IsOrderableDurationLexical(string lexical)
    {
        string s = lexical.Trim().TrimStart('-', '+');
        int tIdx = s.IndexOf('T');
        string datePart = tIdx >= 0 ? s[..tIdx] : s;
        bool hasYearMonth = datePart.Contains('Y') || datePart.Contains('M');
        bool hasDayTime = datePart.Contains('D') || (tIdx >= 0 && tIdx < s.Length - 1);
        return hasYearMonth != hasDayTime || (!hasYearMonth && !hasDayTime);
    }

    /// <summary>
    /// Casts an xs:untypedAtomic lexical value to xs:double per the XPath casting
    /// rules (whitespace collapsed; INF/-INF/NaN accepted); raises FORG0001 otherwise.
    /// </summary>
    private static double CastUntypedAtomicToDouble(string lexical)
    {
        string t = lexical.Trim();
        if (t == "NaN") return double.NaN;
        if (t == "INF") return double.PositiveInfinity;
        if (t == "-INF") return double.NegativeInfinity;
        if (double.TryParse(t,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent,
                CultureInfo.InvariantCulture, out var d))
            return d;
        throw new InvalidOperationException($"FORG0001: Cannot cast xs:untypedAtomic(\"{lexical}\") to xs:double");
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
            return ctx.BackwardsCompatible ? XdmValue.FromDouble(double.NaN) : XdmValue.Undefined;

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
            return ctx.BackwardsCompatible ? XdmValue.FromDouble(double.NaN) : XdmValue.Undefined;

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
    /// Converts an XDM value to double for numeric functions (abs, floor, ceiling, round).
    /// Only numeric and xs:untypedAtomic arguments are accepted; anything else is XPTY0004.
    /// </summary>
    private static double ConvertToDouble(XdmValue value)
    {
        return value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => (double)value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => value.DoubleValue,
            _ when IsUntypedAtomic(value) => ParseXPathDouble(value.ToString()),
            _ => throw new InvalidOperationException(
                $"XPTY0004: Numeric function argument must be numeric or xs:untypedAtomic, but got {value.Kind}")
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
            return ctx.BackwardsCompatible ? XdmValue.FromDouble(double.NaN) : XdmValue.Undefined;

        // Precision beyond the significant digits of any supported numeric type
        // is the identity function (avoids int overflow for huge precisions).
        if (precision > 1000)
            return arg;

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
                // Per F+O 3.1 the result is an instance of the argument's type, so
                // rounding an xs:integer (even with negative precision) stays xs:integer.
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
        // For magnitudes where the exact-decimal path above does not apply, compute
        // value * 10^precision exactly with rational arithmetic so the rounded integer
        // converts back to the nearest double of the exact result (math-3701: 9.9e-99
        // rounds to the double whose shortest form is 1.0E-98, not 1.0000000000000001E-98).
        return RoundDoubleExact(value, precision);
    }

    /// <summary>
    /// Rounds a double to <paramref name="precision"/> decimal places using exact
    /// BigInteger rational arithmetic: the double is decomposed as mantissa × 2^exp and
    /// scaled by 10^precision exactly; fn:round is floor(scaled + 0.5). The rounded
    /// integer is converted back by formatting the exact decimal and parsing to the
    /// nearest double (.NET parses correctly rounded).
    /// </summary>
    private static double RoundDoubleExact(double value, int precision)
    {
        if (value == 0.0)
            return value;

        long bits = BitConverter.DoubleToInt64Bits(value);
        int rawExp = (int)((bits >> 52) & 0x7FF);
        long mantissa = bits & 0xFFFFFFFFFFFFFL;
        int exp = rawExp == 0 ? -1074 : rawExp - 1075;
        if (rawExp != 0)
            mantissa |= 1L << 52;
        var numerator = new BigInteger(bits < 0 ? -mantissa : mantissa);
        var denominator = BigInteger.One;

        // scaled = value * 10^precision = mantissa * 2^(exp+precision) * 5^precision.
        if (precision >= 0)
            numerator *= BigInteger.Pow(5, precision);
        else
            denominator = BigInteger.Pow(5, -precision);
        int binExp = exp + precision;
        if (binExp >= 0)
            numerator <<= binExp;
        else
            denominator <<= -binExp;

        // fn:round = floor(scaled + 0.5); BigInteger division truncates toward zero,
        // so negative non-exact quotients need a true-floor adjustment.
        var twoN = (numerator << 1) + denominator;
        var twoD = denominator << 1;
        var q0 = BigInteger.Divide(twoN, twoD);
        if (twoN.Sign < 0 && twoN != q0 * twoD)
            q0 -= BigInteger.One;

        // Result = q0 * 10^-precision as an exact decimal string, parsed to the
        // nearest double.
        string result = q0.ToString(CultureInfo.InvariantCulture)
            + "E" + (-precision).ToString(CultureInfo.InvariantCulture);
        return double.Parse(result, NumberStyles.Float, CultureInfo.InvariantCulture);
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

        // Precision beyond the significant digits of any supported numeric type
        // is the identity function (avoids int overflow for huge precisions).
        if (precision > 1000)
            return arg;

        // For non-numeric types (string, untypedAtomic, etc.), convert to double first:
        // the function conversion rules cast untypedAtomic to xs:double for an xs:numeric
        // parameter (hof-043).
        bool isNumeric = arg.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float;
        if (!isNumeric)
        {
            double d = ConvertToDouble(arg);
            if (double.IsNaN(d)) return XdmValue.FromDouble(double.NaN);
            return XdmValue.FromDouble(RoundHalfToEvenDouble(d, (int)precision));
        }

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
                // Per F+O 3.1 the result is an instance of the argument's type, so
                // rounding an xs:integer (even with negative precision) stays xs:integer.
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
        var item = ctx.ContextItem;
        if (item.IsUndefined)
            throw new InvalidOperationException("XPDY0002");
        if (!item.IsNode)
            throw new InvalidOperationException("XPTY0004");
        return XdmValue.FromString(item.NodeValue.LocalName);
    }

    private static XdmValue LocalName_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var node = GetOptionalSingleNode(args[0], ctx.BackwardsCompatible);
        return XdmValue.FromString(node?.LocalName ?? string.Empty);
    }

    private static XdmValue NamespaceUri_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var item = ctx.ContextItem;
        if (item.IsUndefined)
            throw new InvalidOperationException("XPDY0002");
        if (!item.IsNode)
            throw new InvalidOperationException("XPTY0004");
        return XdmValue.FromString(item.NodeValue.NamespaceUri, "anyURI");
    }

    private static XdmValue NamespaceUri_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var node = GetOptionalSingleNode(args[0], ctx.BackwardsCompatible);
        return XdmValue.FromString(node?.NamespaceUri ?? string.Empty, "anyURI");
    }

    private static XdmValue Name_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var item = ctx.ContextItem;
        if (item.IsUndefined)
            throw new InvalidOperationException("XPDY0002");
        if (!item.IsNode)
            throw new InvalidOperationException("XPTY0004");
        return XdmValue.FromString(GetQualifiedName(item.NodeValue));
    }

    private static XdmValue Name_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var node = GetOptionalSingleNode(args[0], ctx.BackwardsCompatible);
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
        var item = ctx.ContextItem;
        if (item.IsUndefined)
            throw new InvalidOperationException("XPDY0002: fn:lang() called with no context item.");
        if (!item.IsNode)
            throw new InvalidOperationException("XPTY0004: fn:lang() context item is not a node.");
        return Lang(AtomizedString(args[0]), item.NodeValue);
    }

    private static XdmValue Lang_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var node = GetOptionalSingleNode(args[1], ctx.BackwardsCompatible);
        if (node is null)
            throw new InvalidOperationException("XPTY0004: fn:lang() argument is not a node.");
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
        return FormatNumber(ctx, args[0], RequireString(args[1], ctx.BackwardsCompatible), null);
    }

    private static XdmValue FormatNumber_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        return FormatNumber(ctx, args[0], RequireString(args[1], ctx.BackwardsCompatible), RequireString(args[2], ctx.BackwardsCompatible));
    }

    private static XdmValue FormatNumber(EvaluationContext ctx, XdmValue value, string picture, string? formatName)
    {
        value = AtomizeValue(value);

        var format = string.IsNullOrEmpty(formatName)
            ? ctx.DefaultDecimalFormat
            : ResolveDecimalFormat(ctx, formatName);

        if (format == null)
            throw new InvalidOperationException("FODF1280");

        string result = FormatNumberEngine.Format(value, picture, format, ctx.BackwardsCompatible);
        return XdmValue.FromString(result);
    }

    private static XdmValue FormatDate_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => FormatDateTime(args[0], AtomizedString(args[1]), null, null, null, DateTimeComponents.Date, ctx.IsXsltMode);

    private static XdmValue FormatDate_5(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => FormatDateTime(args[0], AtomizedString(args[1]), AtomizedString(args[2]), AtomizedString(args[3]), AtomizedString(args[4]), DateTimeComponents.Date, ctx.IsXsltMode);

    private static XdmValue FormatTime_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => FormatDateTime(args[0], AtomizedString(args[1]), null, null, null, DateTimeComponents.Time, ctx.IsXsltMode);

    private static XdmValue FormatTime_5(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => FormatDateTime(args[0], AtomizedString(args[1]), AtomizedString(args[2]), AtomizedString(args[3]), AtomizedString(args[4]), DateTimeComponents.Time, ctx.IsXsltMode);

    private static XdmValue FormatDateTime_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => FormatDateTime(args[0], AtomizedString(args[1]), null, null, null, DateTimeComponents.DateTime, ctx.IsXsltMode);

    private static XdmValue FormatDateTime_5(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => FormatDateTime(args[0], AtomizedString(args[1]), AtomizedString(args[2]), AtomizedString(args[3]), AtomizedString(args[4]), DateTimeComponents.DateTime, ctx.IsXsltMode);

    private static XdmValue FormatDateTime(XdmValue value, string picture, string? language, string? calendar, string? place, DateTimeComponents components, bool isXsltMode)
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

        string result = FormatDateTimeEngine.Format(xdt, picture, language, calendar, place, components, isXsltMode);
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
    {
        var now = ctx.CurrentDateTimeSnapshot;
        var offset = TimeSpan.FromMinutes(ctx.ImplicitTimezoneOffsetMinutes);
        return XdmValue.FromDateTime(new DateTimeOffset(now.DateTime, offset), hasTimezone: true);
    }

    private static XdmValue CurrentDate(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var now = ctx.CurrentDateTimeSnapshot;
        var offset = TimeSpan.FromMinutes(ctx.ImplicitTimezoneOffsetMinutes);
        return XdmValue.FromDate(new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, offset), hasTimezone: true);
    }

    private static XdmValue CurrentTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var now = ctx.CurrentDateTimeSnapshot;
        var offset = TimeSpan.FromMinutes(ctx.ImplicitTimezoneOffsetMinutes);
        // Keep the date part at year 1 when possible. For timezone offsets where the
        // local time is earlier than the offset, the UTC instant would fall before
        // DateTimeOffset.MinValue; fall back to day 2 in that case.
        DateTimeOffset time;
        try
        {
            time = new DateTimeOffset(1, 1, 1, now.Hour, now.Minute, now.Second, now.Millisecond, offset);
        }
        catch (ArgumentException)
        {
            time = new DateTimeOffset(1, 1, 2, now.Hour, now.Minute, now.Second, now.Millisecond, offset);
        }
        return XdmValue.FromTime(time, hasTimezone: true);
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

        int targetOffset = ParseTimezoneOffset(tzArg);
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

        int targetOffset = ParseTimezoneOffset(tzArg);
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

        int targetOffset = ParseTimezoneOffset(tzArg);
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

    /// <summary>
    /// Parses an xs:dayTimeDuration timezone offset and validates it is in the
    /// range -PT14H to +PT14H inclusive with a resolution of one minute.
    /// Raises FODT0003 if the offset is out of range or has seconds/milliseconds.
    /// </summary>
    private static int ParseTimezoneOffset(XdmValue tzValue)
    {
        TimeSpan ts = XmlConvert.ToTimeSpan(AtomizedString(tzValue));
        if (ts.Ticks % TimeSpan.TicksPerMinute != 0)
            throw new InvalidOperationException("FODT0003");
        long totalMinutes = ts.Ticks / TimeSpan.TicksPerMinute;
        if (Math.Abs(totalMinutes) > 14 * 60)
            throw new InvalidOperationException("FODT0003");
        return (int)totalMinutes;
    }

    private static XdmValue NodeName_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var item = ctx.ContextItem;
        if (item.IsUndefined)
            throw new InvalidOperationException("XPDY0002");
        if (!item.IsNode)
            throw new InvalidOperationException("XPTY0004");
        return NodeToQName(item.NodeValue);
    }

    private static XdmValue NodeName_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var node = GetOptionalSingleNode(args[0], ctx.BackwardsCompatible);
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
    {
        if (ctx.ContextItem.IsUndefined)
            throw new InvalidOperationException("XPDY0002: fn:number() called with no context item.");
        return Number(ctx.ContextItem);
    }

    private static XdmValue Number_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => Number(args[0]);

    private static XdmValue Number(XdmValue value)
    {
        if (value.IsUndefined)
            return XdmValue.FromDouble(double.NaN);

        value = AtomizeValue(value);
        if (value.IsUndefined)
            return XdmValue.FromDouble(double.NaN);

        // fn:number converts numeric types, xs:string, xs:untypedAtomic and xs:boolean
        // to xs:double; other atomic types (anyURI, gYear, QName, dateTime, etc.) return NaN.
        if (VmEngine.TryCast(value, "xs:double", out var casted))
            return casted;

        return XdmValue.FromDouble(double.NaN);
    }

    private static XdmValue Data_0(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        // fn:data() is fn:data(.); an absent context item is XPDY0002 (K2-DataFunc-4).
        if (ctx.ContextItem.IsUndefined)
            throw new InvalidOperationException("XPDY0002: fn:data() called with no context item.");
        return Data(ctx.ContextItem);
    }

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
            // case it is xs:untypedAtomic. Schema-validated nodes return their
            // PSVI typed value (empty for nilled elements). Complex element-only/
            // empty elements have no typed value and raise FOTY0012.
            var node = value.NodeValue;
            if (node.HasNoTypedValue)
                throw new InvalidOperationException("FOTY0012: The argument node does not have a typed value.");
            // XDM §2.7.2: namespace nodes also have an xs:string typed value (nscons-012).
            if (node.NodeKind is XdmNodeKind.ProcessingInstruction or XdmNodeKind.Comment or XdmNodeKind.Namespace)
                return XdmValue.FromString(node.StringValue);
            if (node.SchemaTypeAnnotation is not null)
                return node.TypedValue;
            // DTD-declared IDREFS attributes have a typed value that is a sequence of
            // xs:IDREF strings (one per whitespace-separated token).
            if (node.NodeKind == XdmNodeKind.Attribute && IsDtdIdrefsAttribute(node))
            {
                var tokens = ParseIdTokens(node.StringValue);
                var items = new List<XdmValue>(tokens.Count);
                foreach (var token in tokens)
                    items.Add(XdmValue.FromString(token, "IDREF"));
                if (items.Count == 0)
                    return XdmValue.Undefined;
                if (items.Count == 1)
                    return items[0];
                return XdmValue.FromSequence(MaterializedSequence.FromList(items));
            }
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
        if (item.IsUndefined)
            throw new InvalidOperationException("XPDY0002");
        if (!item.IsNode)
            throw new InvalidOperationException("XPTY0004");
        return Root(item.NodeValue);
    }

    private static XdmValue Root_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var node = GetOptionalSingleNode(args[0], ctx.BackwardsCompatible);
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
        return v.IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(v.DateTimeXPathValue.Year);
    }

    private static XdmValue MonthFromDateTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var v = UnwrapSequenceOrUndefined(args[0]);
        return v.IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(v.DateTimeXPathValue.Month);
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
        return v.IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(v.TimeXPathValue.Hour);
    }

    private static XdmValue MinutesFromTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var v = UnwrapSequenceOrUndefined(args[0]);
        return v.IsUndefined ? XdmValue.Undefined : XdmValue.FromInteger(v.TimeXPathValue.Minute);
    }

    private static XdmValue SecondsFromTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var v = UnwrapSequenceOrUndefined(args[0]);
        if (v.IsUndefined) return XdmValue.Undefined;
        var xdt = v.TimeXPathValue;
        return XdmValue.FromDecimal(xdt.Second + xdt.Millisecond / 1000.0m);
    }

    private static XdmValue TimezoneFromDateTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => TimezoneFromValue(UnwrapSequenceOrUndefined(args[0]), v => v.DateTimeValue);

    private static XdmValue TimezoneFromDate(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => TimezoneFromValue(UnwrapSequenceOrUndefined(args[0]), v => v.DateValue);

    private static XdmValue TimezoneFromTime(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var v = UnwrapSequenceOrUndefined(args[0]);
        if (v.IsUndefined) return XdmValue.Undefined;
        if (!v.HasTimezone) return XdmValue.Undefined;
        var offset = TimeSpan.FromMinutes(v.TimeXPathValue.TimezoneOffsetMinutes);
        return XdmValue.FromDuration(FormatDayTimeDuration(offset));
    }

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

        bool isYearMonth = IsYearMonthDurationString(s) || IsGenericDurationString(s);
        bool isDayTime = IsDayTimeDurationString(s) || IsGenericDurationString(s);

        long normYears = 0, normMonths = 0;
        if (isYearMonth)
        {
            long totalMonths = years * 12 + months;
            normYears = totalMonths / 12;
            normMonths = totalMonths % 12;
        }

        long normDays = 0, normHours = 0, normMinutes = 0;
        decimal normSeconds = 0m;
        if (isDayTime)
        {
            decimal totalSeconds = days * 86400m + hours * 3600m + minutes * 60m + seconds;
            bool negative = totalSeconds < 0;
            totalSeconds = negative ? -totalSeconds : totalSeconds;
            normDays = (long)(totalSeconds / 86400m);
            totalSeconds -= normDays * 86400m;
            normHours = (long)(totalSeconds / 3600m);
            totalSeconds -= normHours * 3600m;
            normMinutes = (long)(totalSeconds / 60m);
            normSeconds = totalSeconds - normMinutes * 60m;
            if (negative)
            {
                normDays = -normDays;
                normHours = -normHours;
                normMinutes = -normMinutes;
                normSeconds = -normSeconds;
            }
        }

        return part switch
        {
            DurationPart.Years => XdmValue.FromInteger(normYears),
            DurationPart.Months => XdmValue.FromInteger(normMonths),
            DurationPart.Days => XdmValue.FromInteger(normDays),
            DurationPart.Hours => XdmValue.FromInteger(normHours),
            DurationPart.Minutes => XdmValue.FromInteger(normMinutes),
            DurationPart.Seconds => XdmValue.FromDecimal(normSeconds),
            _ => XdmValue.Undefined
        };
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
        => DeepEqual(args[0], args[1], ctx.DefaultCollation, ctx.ImplicitTimezoneOffsetMinutes);

    private static XdmValue DeepEqual_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        string collation = AtomizedString(args[2]);
        ValidateCollation(collation);
        return DeepEqual(args[0], args[1], collation, ctx.ImplicitTimezoneOffsetMinutes);
    }

    private static XdmValue DeepEqual(XdmValue a, XdmValue b, string collation, int implicitTimezoneOffsetMinutes)
        => XdmValue.FromBoolean(DeepEqualValue(a, b, collation, implicitTimezoneOffsetMinutes));

    // Compares two values with sequence semantics: map entry values and array members are
    // arbitrary sequences, and a singleton sequence must compare equal to its bare item
    // (step-built map values are sequence-wrapped while constructor values are bare nodes).
    private static bool DeepEqualValue(XdmValue a, XdmValue b, string collation, int implicitTimezoneOffsetMinutes)
    {
        var itemsA = ToItemList(a);
        var itemsB = ToItemList(b);
        if (itemsA.Count != itemsB.Count)
            return false;
        for (int i = 0; i < itemsA.Count; i++)
        {
            if (!DeepEqualItem(itemsA[i], itemsB[i], collation, implicitTimezoneOffsetMinutes))
                return false;
        }
        return true;
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

    private static bool DeepEqualItem(XdmValue a, XdmValue b, string collation, int implicitTimezoneOffsetMinutes)
    {
        // Function items cannot be compared for deep equality (XQuery 3.1 fn:deep-equal).
        if (a.IsFunction || b.IsFunction)
            throw new InvalidOperationException("FOTY0015: fn:deep-equal cannot be applied to function items");

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

        // The empty sequence is deep-equal to itself regardless of representation
        // (XdmValue.Undefined vs an empty XdmSequence instance) — fn-parse-json-007.
        if (IsEmptySequence(a) && IsEmptySequence(b))
            return true;

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
            XdmValueKind.String => TypedStringValuesEqual(a, b, collation, implicitTimezoneOffsetMinutes),
            XdmValueKind.DateTime => DateTimeEqual(a.DateTimeXPathValue, b.DateTimeXPathValue, a.HasTimezone, b.HasTimezone, implicitTimezoneOffsetMinutes),
            XdmValueKind.Date => DateTimeEqual(a.DateXPathValue, b.DateXPathValue, a.HasTimezone, b.HasTimezone, implicitTimezoneOffsetMinutes),
            XdmValueKind.Time => DateTimeEqual(a.TimeXPathValue, b.TimeXPathValue, a.HasTimezone, b.HasTimezone, implicitTimezoneOffsetMinutes),
            XdmValueKind.QName => a.QNameValue.Equals(b.QNameValue),
            XdmValueKind.Node => DeepEqualNode(a.NodeValue, b.NodeValue, collation),
            XdmValueKind.Sequence => DeepEqual(a, b, collation, implicitTimezoneOffsetMinutes).BooleanValue,
            XdmValueKind.Map => DeepEqualMap(a.MapValue, b.MapValue, collation, implicitTimezoneOffsetMinutes),
            XdmValueKind.Array => DeepEqualArray(a.ArrayValue, b.ArrayValue, collation, implicitTimezoneOffsetMinutes),
            _ => false
        };
    }

    private static bool DateTimeEqual(XPathDateTime a, XPathDateTime b, bool aHasTimezone, bool bHasTimezone, int implicitTimezoneOffsetMinutes)
    {
        // Neither has timezone: compare local components directly.
        if (!aHasTimezone && !bHasTimezone)
            return XPathDateTimeHelper.CompareComponents(a, b) == 0;

        // Apply implicit timezone to the value that lacks an explicit timezone,
        // then normalize both to UTC and compare their components.
        var aEffective = aHasTimezone
            ? a
            : new XPathDateTime(a.Year, a.Month, a.Day, a.Hour, a.Minute, a.Second, a.Millisecond, implicitTimezoneOffsetMinutes, true);
        var bEffective = bHasTimezone
            ? b
            : new XPathDateTime(b.Year, b.Month, b.Day, b.Hour, b.Minute, b.Second, b.Millisecond, implicitTimezoneOffsetMinutes, true);

        var aUtc = XPathDateTimeHelper.NormalizeToUtc(aEffective);
        var bUtc = XPathDateTimeHelper.NormalizeToUtc(bEffective);
        return XPathDateTimeHelper.CompareComponents(aUtc, bUtc) == 0;
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

        // For element/attribute nodes that are both schema-validated:
        //   * If both have simple types (including list/union), compare typed values
        //     and ignore the type annotation name.
        //   * If either has a complex type, the type annotation name must agree and
        //     we fall back to string-value comparison (complex types may have element-only,
        //     mixed, or empty content where typed-value comparison is not appropriate).
        bool aHasType = a.NodeKind is XdmNodeKind.Element or XdmNodeKind.Attribute && a.SchemaTypeAnnotation.HasValue;
        bool bHasType = b.NodeKind is XdmNodeKind.Element or XdmNodeKind.Attribute && b.SchemaTypeAnnotation.HasValue;
        bool useTypedValue = aHasType && bHasType && !a.IsComplexType && !b.IsComplexType;
        if (aHasType && bHasType && !useTypedValue
            && a.SchemaTypeAnnotation is { } typeA
            && b.SchemaTypeAnnotation is { } typeB)
        {
            if (typeA.NamespaceUri != typeB.NamespaceUri || typeA.LocalName != typeB.LocalName)
                return false;
        }
        if (useTypedValue)
        {
            if (!DeepEqualValue(a.TypedValue, b.TypedValue, collation, 0))
                return false;
        }
        else
        {
            if (CompareStrings(a.StringValue, b.StringValue, collation) != 0)
                return false;
        }

        if (a.NodeKind == XdmNodeKind.Element)
        {
            // XDM: the attribute axis never contains namespace declarations. The
            // XDocument provider exposes xmlns attributes through Attributes(), so
            // they must be filtered out here (snapshot-0102: xsl:copy redeclares
            // in-scope namespaces while fn:snapshot does not).
            var attrsA = SortNodes(a.Attributes());
            var attrsB = SortNodes(b.Attributes());
            attrsA.RemoveAll(IsNamespaceDeclarationItem);
            attrsB.RemoveAll(IsNamespaceDeclarationItem);
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

    private static bool IsNamespaceDeclarationItem(XdmValue item)
    {
        if (!item.IsNode || item.NodeValue == null)
            return false;
        var node = item.NodeValue;
        // Namespace declarations surface as attributes named "xmlns" (no namespace)
        // or as attributes in the XMLNS namespace.
        return node.NodeKind == XdmNodeKind.Attribute
            && (node.NamespaceUri == "http://www.w3.org/2000/xmlns/"
                || (string.IsNullOrEmpty(node.NamespaceUri) && node.LocalName == "xmlns"));
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
        {
            var node = item.NodeValue;
            // fn:deep-equal ignores comments and processing instructions when comparing element children.
            if (node.NodeKind == XdmNodeKind.Comment || node.NodeKind == XdmNodeKind.ProcessingInstruction)
                continue;
            list.Add(node);
        }
        return list;
    }

    private static bool DeepEqualMap(XdmMap a, XdmMap b, string collation, int implicitTimezoneOffsetMinutes)
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
                // Map keys are compared with op:same-key semantics: the collation
                // parameter does NOT apply to keys (fn-deep-equal-maps-13).
                if (XdmValueEqualityComparer.Instance.Equals(keyA, keyB) && DeepEqualValue(valA, valB, collation, implicitTimezoneOffsetMinutes))
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

    private static bool DeepEqualArray(XdmArray a, XdmArray b, string collation, int implicitTimezoneOffsetMinutes)
    {
        if (a.Count != b.Count)
            return false;
        var av = a.Values.ToList();
        var bv = b.Values.ToList();
        for (int i = 0; i < av.Count; i++)
        {
            if (!DeepEqualValue(av[i], bv[i], collation, implicitTimezoneOffsetMinutes))
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
        // XPDY0002 when the context item is absent; XPTY0004 when it is not a node
        // (generate-id-901).
        if (item.IsUndefined)
            throw new InvalidOperationException("XPDY0002: fn:generate-id() requires a context item.");
        if (!item.IsNode)
            throw new InvalidOperationException("XPTY0004: fn:generate-id() context item must be a node.");
        return XdmValue.FromString(GetNodeId(item.NodeValue));
    }

    private static XdmValue GenerateId_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        // The argument must be a single node or the empty sequence (generate-id-902..905).
        if (IsEmptySequence(args[0]))
            return XdmValue.FromString(string.Empty);
        if (args[0].IsSequence && args[0].SequenceValue is { } seq)
        {
            int count = 0;
            foreach (var _ in XdmSequence.FromSource(seq))
            {
                count++;
                if (count > 1)
                    throw new InvalidOperationException("XPTY0004: fn:generate-id() argument must be a single node.");
            }
        }
        var node = GetNodeFromValue(args[0]);
        if (node is null)
            throw new InvalidOperationException("XPTY0004: fn:generate-id() argument must be a node.");
        return XdmValue.FromString(GetNodeId(node));
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
        string s1 = RequireString(args[0]);
        string s2 = RequireString(args[1]);
        return Compare(s1, s2, ctx.DefaultCollation);
    }

    private static XdmValue Compare_3(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (IsEmptySequence(args[0]) || IsEmptySequence(args[1]))
            return XdmValue.Undefined;
        string s1 = RequireString(args[0]);
        string s2 = RequireString(args[1]);
        string collation = AtomizedString(args[2]);
        ValidateCollation(collation);
        return Compare(s1, s2, collation);
    }

    private static XdmValue Compare(string s1, string s2, string collation = "")
    {
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
        var s = RequireString(args[0]);
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
        var ns = RequireString(args[0]);
        var lexical = RequireStringRequired(args[1]);

        if (string.IsNullOrWhiteSpace(lexical))
            throw new InvalidOperationException("FOCA0002");

        var local = lexical.Contains(':') ? lexical[(lexical.IndexOf(':') + 1)..] : lexical;
        var prefix = lexical.Contains(':') ? lexical[..lexical.IndexOf(':')] : string.Empty;
        if (lexical.Contains(':') && (string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(local)))
            throw new InvalidOperationException("FOCA0002");
        if (!IsValidNcName(local))
            throw new InvalidOperationException("FOCA0002");
        if (!string.IsNullOrEmpty(prefix))
        {
            if (!IsValidNcName(prefix))
                throw new InvalidOperationException("FOCA0002");
            if (string.IsNullOrEmpty(ns))
                throw new InvalidOperationException("FOCA0002");
        }

        return XdmValue.FromQName(new XsQName(local, ns, prefix));
    }

    /// <summary>
    /// Validates that <paramref name="name"/> conforms to the XML NCName production
    /// (no colon, non-empty, starts with letter or underscore, remaining chars are
    /// letters, digits, '.', '-', '_', or combining/extender characters).
    /// </summary>
    private static bool IsValidNcName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        char first = name[0];
        if (!(char.IsLetter(first) || first == '_'))
            return false;

        for (int i = 1; i < name.Length; i++)
        {
            char c = name[i];
            if (char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_')
                continue;
            var category = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (category is System.Globalization.UnicodeCategory.NonSpacingMark
                or System.Globalization.UnicodeCategory.SpacingCombiningMark
                or System.Globalization.UnicodeCategory.ConnectorPunctuation
                or System.Globalization.UnicodeCategory.Format)
                continue;
            return false;
        }

        return true;
    }

    private static XdmValue ResolveQName(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var lexical = AtomizedString(args[0]);
        IXdmNode? node = null;
        int argCount = 0;
        if (args[1].IsSequence && args[1].SequenceValue is { } seq)
        {
            foreach (var item in XdmSequence.FromSource(seq))
            {
                argCount++;
                if (item.IsNode && node == null)
                    node = item.NodeValue;
            }
        }
        else if (args[1].IsNode)
        {
            node = args[1].NodeValue;
            argCount = 1;
        }
        else if (!args[1].IsUndefined)
        {
            argCount = 1; // atomic, non-node value
        }

        // The second parameter of fn:resolve-QName is exactly one element node (F+O 3.1
        // §11.2.2): an empty sequence, a multi-item sequence, or a non-node is XPTY0004
        // (type-0158).
        if (argCount == 0)
            throw new InvalidOperationException("XPTY0004: The second argument of fn:resolve-QName must be exactly one element node; the empty sequence is not allowed.");
        if (argCount > 1)
            throw new InvalidOperationException("XPTY0004: The second argument of fn:resolve-QName must be exactly one element node; a sequence of more than one item is not allowed.");
        if (node == null)
            throw new InvalidOperationException("XPTY0004: The second argument of fn:resolve-QName must be an element node.");

        if (string.IsNullOrEmpty(lexical))
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
        var atomized = AtomizeSingleton(args[0]);
        if (atomized.Kind == XdmValueKind.Undefined || IsEmptySequence(atomized))
            return XdmValue.FromSequence(XdmSequence.Empty);
        var qn = atomized.QNameValue;
        return XdmValue.FromString(qn.LocalName, "NCName");
    }

    private static XdmValue NamespaceUriFromQName(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var atomized = AtomizeSingleton(args[0]);
        if (atomized.Kind == XdmValueKind.Undefined || IsEmptySequence(atomized))
            return XdmValue.FromSequence(XdmSequence.Empty);
        var qn = atomized.QNameValue;
        return XdmValue.FromString(qn.NamespaceUri, "anyURI");
    }

    private static XdmValue PrefixFromQName(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var atomized = AtomizeSingleton(args[0]);
        if (atomized.Kind == XdmValueKind.Undefined || IsEmptySequence(atomized))
            return XdmValue.FromSequence(XdmSequence.Empty);
        var qn = atomized.QNameValue;
        if (string.IsNullOrEmpty(qn.Prefix))
            return XdmValue.FromSequence(XdmSequence.Empty);
        return XdmValue.FromString(qn.Prefix, "NCName");
    }

    // ------------------------------------------------------------------
    // JSON functions (parse-json, json-to-xml, xml-to-json, json-doc)
    // ------------------------------------------------------------------

    private static readonly string JsonXmlNs = "http://www.w3.org/2005/xpath-functions";

    private static readonly Lazy<XmlSchemaSet> JsonSchemaSetInternal = new(() =>
    {
        var assembly = typeof(FunctionLibrary).Assembly;
        const string resourceName = "Bosak.XPath.Standard.Resources.schema-for-json.xsd";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded JSON schema resource '{resourceName}' not found.");
        using var reader = XmlReader.Create(stream);
        var schema = XmlSchema.Read(reader, null)
            ?? throw new InvalidOperationException($"Failed to read embedded JSON schema '{resourceName}'.");
        var schemaSet = new XmlSchemaSet { XmlResolver = new XmlUrlResolver() };
        schemaSet.Add(schema);
        schemaSet.Compile();
        return schemaSet;
    });

    /// <summary>
    /// The W3C schema-for-JSON used by <c>fn:json-to-xml</c> when <c>validate:=true()</c>.
    /// Exposed so that XQuery <c>import schema</c> declarations for the JSON namespace can be
    /// satisfied without requiring an external schema file.
    /// </summary>
    internal static XmlSchemaSet JsonSchemaSet => JsonSchemaSetInternal.Value;

    /// <summary>
    /// Returns a stream over the embedded schema-for-json.xsd resource. The caller owns the
    /// stream and must dispose it.
    /// </summary>
    internal static Stream GetJsonSchemaStream()
    {
        var assembly = typeof(FunctionLibrary).Assembly;
        const string resourceName = "Bosak.XPath.Standard.Resources.schema-for-json.xsd";
        return assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded JSON schema resource '{resourceName}' not found.");
    }

    /// <summary>
    /// The W3C schema for <c>fn:analyze-string-result</c> elements. Used to validate
    /// the result of <c>fn:analyze-string</c> so that its attributes and elements carry
    /// the PSVI annotations required by schema-aware tests.
    /// </summary>
    private static readonly Lazy<XmlSchemaSet> AnalyzeStringSchemaSetInternal = new(() =>
    {
        var assembly = typeof(FunctionLibrary).Assembly;
        const string resourceName = "Bosak.XPath.Standard.Resources.analyze-string.xsd";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded analyze-string schema resource '{resourceName}' not found.");
        using var reader = XmlReader.Create(stream);
        var schema = XmlSchema.Read(reader, null)
            ?? throw new InvalidOperationException($"Failed to read embedded analyze-string schema '{resourceName}'.");
        var schemaSet = new XmlSchemaSet { XmlResolver = new XmlUrlResolver() };
        schemaSet.Add(schema);
        schemaSet.Compile();
        return schemaSet;
    });

    private static XmlSchemaSet AnalyzeStringSchemaSet => AnalyzeStringSchemaSetInternal.Value;

    private readonly record struct JsonOptions(bool Liberal, string Duplicates, bool Escape, bool Indent, bool Validate, bool DuplicatesExplicit, XdmValue? Fallback = null)
    {
        public static JsonOptions Default => new(false, "use-first", false, false, false, false, null);
    }

    private static JsonOptions ParseJsonOptions(XdmValue? options, bool forJsonToXml = false)
    {
        // fn:json-to-xml retains duplicate keys by default (json-to-xml-018).
        var defaultOptions = forJsonToXml ? JsonOptions.Default with { Duplicates = "retain" } : JsonOptions.Default;
        if (options is null || options.Value.IsUndefined)
            return defaultOptions;
        if (!options.Value.IsMap)
            return defaultOptions;

        var map = options.Value.MapValue;
        var result = defaultOptions;

        if (map.TryGetValue(XdmValue.FromString("liberal"), out var liberal))
        {
            // The value must be a single xs:boolean (json-to-xml-error-020/021).
            if (liberal.Kind != XdmValueKind.Boolean)
                throw new InvalidOperationException("XPTY0004: The liberal option must be a single xs:boolean");
            result = result with { Liberal = liberal.BooleanValue };
        }
        if (map.TryGetValue(XdmValue.FromString("duplicates"), out var dup))
        {
            var dupStr = RequireString(dup);
            // F+O 3.1: fn:parse-json accepts reject, use-first and use-last ('retain'
            // appeared in an early draft — fn-parse-json-940); fn:json-to-xml accepts
            // reject, use-first and retain, but not use-last (json-to-xml-error-040).
            bool valid = forJsonToXml
                ? dupStr is "use-first" or "reject" or "retain"
                : dupStr is "use-first" or "use-last" or "reject";
            if (!valid)
                throw new InvalidOperationException("FOJS0005: Invalid duplicates option");
            result = result with { Duplicates = dupStr, DuplicatesExplicit = true };
        }
        if (map.TryGetValue(XdmValue.FromString("escape"), out var escape))
        {
            // The value must be a single xs:boolean (json-to-xml-error-025/026/027).
            if (escape.Kind != XdmValueKind.Boolean)
                throw new InvalidOperationException("XPTY0004: The escape option must be a single xs:boolean");
            result = result with { Escape = escape.BooleanValue };
        }
        if (map.TryGetValue(XdmValue.FromString("indent"), out var indent))
        {
            // The value must be a single xs:boolean (xml-to-json-C100..C103).
            if (indent.Kind != XdmValueKind.Boolean)
                throw new InvalidOperationException("XPTY0004: The indent option must be a single xs:boolean");
            result = result with { Indent = indent.BooleanValue };
        }
        if (map.TryGetValue(XdmValue.FromString("validate"), out var validate))
        {
            // fn:json-to-xml only: requests schema validation of the result. The
            // value must be a single xs:boolean (json-to-xml-error-020/021/022).
            if (validate.Kind != XdmValueKind.Boolean)
                throw new InvalidOperationException("XPTY0004: The validate option must be a single xs:boolean");
            result = result with { Validate = validate.BooleanValue };
        }
        if (map.TryGetValue(XdmValue.FromString("fallback"), out var fallback))
        {
            // Validated eagerly (json-to-xml-error-026/041, json-doc-error-016/026):
            // the fallback must be a function item of arity 1 even when the input
            // never causes it to be invoked.
            if (!fallback.IsFunction)
                throw new InvalidOperationException("XPTY0004: The fallback option must be a function item");
            if (((FunctionItem)fallback.FunctionValue).Arity != 1)
                throw new InvalidOperationException("XPTY0004: The fallback function must have arity 1");
            result = result with { Fallback = fallback };
        }

        // The escape and fallback options cannot be combined (json-doc-027).
        if (result.Escape && result.Fallback is { IsUndefined: false })
            throw new InvalidOperationException("FOJS0005: The escape and fallback options cannot be combined");

        // Validation is incompatible with explicitly retaining duplicate keys
        // (json-to-xml-error-042). When validate is requested without an explicit
        // duplicates option, duplicates are detected by schema validation instead.
        if (result.Validate && result.DuplicatesExplicit && result.Duplicates == "retain")
            throw new InvalidOperationException("FOJS0005: The validate option cannot be combined with duplicates='retain'");

        return result;
    }

    private static XdmValue ParseJson_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        // fn:parse-json($input as xs:string?): the empty sequence yields the empty
        // sequence (fn-parse-json-112..115).
        if (args[0].IsUndefined || IsEmptySequence(args[0]))
            return XdmValue.Undefined;
        return ParseJson(ctx, AtomizedString(args[0]), JsonOptions.Default);
    }

    private static XdmValue ParseJson_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (args[0].IsUndefined || IsEmptySequence(args[0]))
            return XdmValue.Undefined;
        return ParseJson(ctx, AtomizedString(args[0]), ParseJsonOptions(args[1]));
    }

    private static XdmValue ParseJson(EvaluationContext ctx, string json, JsonOptions options)
    {
        if (string.IsNullOrEmpty(json))
            throw new InvalidOperationException("FOJS0001: Empty string is not valid JSON");
        // A leading U+FEFF (byte order mark) is ignored (json-to-xml-015).
        if (json[0] == '\uFEFF')
            json = json.Substring(1);
        return JNodeToXdm(new JsonReader(json, options, ctx).ParseDocument(), options);
    }

    /// <summary>
    /// Recursive-descent JSON parser for fn:parse-json (F+O 3.1 §17.5.1). Unlike
    /// System.Text.Json it preserves raw escape sequences, which the spec requires
    /// for the escape option (escapes retained verbatim) and for fallback invocation
    /// (the fallback receives the escape sequence as written, including the backslash).
    /// </summary>
    private sealed class JsonReader
    {
        private readonly string _text;
        private readonly JsonOptions _options;
        private readonly EvaluationContext _ctx;
        private int _pos;

        public JsonReader(string text, JsonOptions options, EvaluationContext ctx)
        {
            _text = text;
            _options = options;
            _ctx = ctx;
        }

        public JNode ParseDocument()
        {
            SkipWhitespace();
            var value = ParseValue();
            SkipWhitespace();
            if (_pos != _text.Length)
                throw Error("Unexpected content after the JSON value");
            return value;
        }

        private JNode ParseValue()
        {
            if (_pos >= _text.Length)
                throw Error("Unexpected end of JSON input");
            char c = _text[_pos];
            return c switch
            {
                '{' => ParseObject(),
                '[' => ParseArray(),
                '"' => ParseStringNode(),
                't' => ExpectLiteral("true", JNode.True),
                'f' => ExpectLiteral("false", JNode.False),
                'n' => ExpectLiteral("null", JNode.Null),
                _ => c == '-' || (c >= '0' && c <= '9') ? ParseNumber() : throw Error($"Unexpected character '{c}'")
            };
        }

        private JNode ExpectLiteral(string literal, JNode value)
        {
            if (_pos + literal.Length > _text.Length || !_text.Substring(_pos, literal.Length).Equals(literal, StringComparison.Ordinal))
                throw Error($"Invalid literal (expected '{literal}')");
            _pos += literal.Length;
            return value;
        }

        private JNode ParseObject()
        {
            _pos++; // consume '{'
            // All members are preserved in source order; the writers apply the
            // duplicates option on the fully decoded canonical key.
            var members = new List<KeyValuePair<(string Key, string Canon), JNode>>();
            SkipWhitespace();
            if (Consume('}'))
                return JNode.MakeObject(members);
            while (true)
            {
                SkipWhitespace();
                if (_pos >= _text.Length || _text[_pos] != '"')
                    throw Error("Expected a string key in JSON object");
                string keyString = ParseString(out string canonical);
                SkipWhitespace();
                Expect(':');
                SkipWhitespace();
                var value = ParseValue();
                members.Add(new KeyValuePair<(string, string), JNode>((keyString, canonical), value));
                SkipWhitespace();
                if (Consume('}'))
                    return JNode.MakeObject(members);
                Expect(',');
            }
        }

        private JNode ParseArray()
        {
            _pos++; // consume '['
            var items = new List<JNode>();
            SkipWhitespace();
            if (Consume(']'))
                return JNode.MakeArray(items);
            while (true)
            {
                SkipWhitespace();
                items.Add(ParseValue());
                SkipWhitespace();
                if (Consume(']'))
                    return JNode.MakeArray(items);
                Expect(',');
            }
        }

        private JNode ParseStringNode()
        {
            string text = ParseString(out string canon);
            return JNode.MakeString(text, canon);
        }

        /// <summary>
        /// Parses a JSON string literal. With escape=true the raw escape sequences are
        /// retained verbatim; otherwise escapes are expanded and any escape denoting a
        /// character that is not a valid XML character (or an unpaired surrogate) is
        /// passed through the fallback function (default: U+FFFD).
        /// </summary>
        private string ParseString() => ParseString(null);

        private string ParseString(out string canonical)
        {
            var canon = new StringBuilder();
            string result = ParseString(canon);
            canonical = canon.ToString();
            return result;
        }

        private string ParseString(StringBuilder? canon)
        {
            _pos++; // consume the opening quote
            var sb = new StringBuilder();
            while (true)
            {
                if (_pos >= _text.Length)
                    throw Error("Unterminated string literal");
                char c = _text[_pos];
                if (c == '"')
                {
                    _pos++;
                    return sb.ToString();
                }
                if (c == '\\')
                {
                    AppendEscape(sb, canon);
                    continue;
                }
                if (c < 0x20)
                    throw Error("Unescaped control character in string literal");
                sb.Append(c);
                canon?.Append(c);
                _pos++;
            }
        }

        private void AppendEscape(StringBuilder sb, StringBuilder? canon)
        {
            if (_pos + 1 >= _text.Length)
                throw Error("Unterminated escape sequence");
            char esc = _text[_pos + 1];
            switch (esc)
            {
                case '"':
                case '\\':
                case '/':
                    canon?.Append(esc);
                    // escape=true re-escapes canonically: only the reverse solidus
                    // stays escaped; the quotation mark and solidus are decoded
                    // (json-doc-012, json-to-xml-049).
                    if (_options.Escape && esc == '\\')
                        sb.Append('\\');
                    sb.Append(esc);
                    _pos += 2;
                    break;
                case 'b':
                case 'f':
                case 'n':
                case 'r':
                case 't':
                {
                    char decoded = esc switch
                    {
                        'b' => '\b', 'f' => '\f', 'n' => '\n', 'r' => '\r', _ => '\t'
                    };
                    canon?.Append(decoded);
                    if (_options.Escape)
                    {
                        sb.Append('\\');
                        sb.Append(esc);
                    }
                    else
                    {
                        // Only TAB/LF/CR are valid XML characters; \b and \f go through
                        // the fallback (fn-parse-json-055/058/061/064).
                        sb.Append(IsValidXmlChar(decoded)
                            ? decoded.ToString()
                            : InvokeJsonFallback(_options, _ctx, $"\\{esc}"));
                    }
                    _pos += 2;
                    break;
                }
                case 'u':
                    {
                        if (_pos + 5 >= _text.Length)
                            throw Error("Truncated \\u escape sequence");
                        string raw = _text.Substring(_pos, 6);
                        int code = ParseHex4(_pos + 2);
                        // High surrogate followed by a low-surrogate escape forms an
                        // astral character; detected here for both escape modes.
                        int? astral = null;
                        if (code is >= 0xD800 and <= 0xDBFF
                            && _pos + 11 < _text.Length && _text[_pos + 6] == '\\' && _text[_pos + 7] == 'u')
                        {
                            int low = ParseHex4(_pos + 8);
                            if (low is >= 0xDC00 and <= 0xDFFF)
                                astral = 0x10000 + ((code - 0xD800) << 10) + (low - 0xDC00);
                        }
                        // The canonical (fully decoded) form is used for duplicate-key
                        // detection regardless of the escape option.
                        if (astral is int ac)
                            canon?.Append(char.ConvertFromUtf32(ac));
                        else
                            canon?.Append((char)code);
                        if (_options.Escape)
                        {
                            // escape=true: the character is decoded and then canonically
                            // re-escaped — valid XML characters verbatim (fn-parse-json-106),
                            // invalid ones as named escapes or \uXXXX (json-doc-021).
                            if (astral is int a1)
                            {
                                sb.Append(char.ConvertFromUtf32(a1));
                                _pos += 12;
                            }
                            else
                            {
                                AppendEscapedJsonChar(sb, (char)code);
                                _pos += 6;
                            }
                            break;
                        }
                        if (astral is int a2)
                        {
                            sb.Append(char.ConvertFromUtf32(a2));
                            _pos += 12;
                        }
                        else if (code is >= 0xD800 and <= 0xDFFF)
                        {
                            sb.Append(InvokeJsonFallback(_options, _ctx, raw));
                            _pos += 6;
                        }
                        else if (IsValidXmlChar((char)code))
                        {
                            sb.Append((char)code);
                            _pos += 6;
                        }
                        else
                        {
                            // Valid JSON escape denoting a character that is not valid
                            // in XML (e.g. , ￾, ￿) → fallback (fn-parse-json-053/056).
                            sb.Append(InvokeJsonFallback(_options, _ctx, raw));
                            _pos += 6;
                        }
                        break;
                    }
                default:
                    throw Error($"Invalid escape sequence '\\{esc}'");
            }
        }

        private int ParseHex4(int offset)
        {
            int value = 0;
            for (int i = offset; i < offset + 4; i++)
            {
                if (i >= _text.Length)
                    throw Error("Truncated \\u escape sequence");
                char h = _text[i];
                int digit = h is >= '0' and <= '9' ? h - '0'
                    : h is >= 'a' and <= 'f' ? h - 'a' + 10
                    : h is >= 'A' and <= 'F' ? h - 'A' + 10
                    : throw Error($"Invalid hex digit '{h}' in \\u escape sequence");
                value = (value << 4) | digit;
            }
            return value;
        }

        private JNode ParseNumber()
        {
            int start = _pos;
            if (_text[_pos] == '-') _pos++;
            // Integer part: 0 | [1-9][0-9]*
            if (_pos >= _text.Length) throw Error("Truncated number");
            if (_text[_pos] == '0') _pos++;
            else if (_text[_pos] is >= '1' and <= '9') { while (_pos < _text.Length && _text[_pos] is >= '0' and <= '9') _pos++; }
            else throw Error("Invalid number");
            // Fraction
            if (_pos < _text.Length && _text[_pos] == '.')
            {
                _pos++;
                if (_pos >= _text.Length || _text[_pos] is not (>= '0' and <= '9')) throw Error("Invalid number: digits required after '.'");
                while (_pos < _text.Length && _text[_pos] is >= '0' and <= '9') _pos++;
            }
            // Exponent
            if (_pos < _text.Length && _text[_pos] is 'e' or 'E')
            {
                _pos++;
                if (_pos < _text.Length && _text[_pos] is '+' or '-') _pos++;
                if (_pos >= _text.Length || _text[_pos] is not (>= '0' and <= '9')) throw Error("Invalid number: digits required in exponent");
                while (_pos < _text.Length && _text[_pos] is >= '0' and <= '9') _pos++;
            }
            // The raw lexical form is retained; fn:parse-json maps it to xs:double,
            // fn:json-to-xml copies it verbatim into j:number.
            return JNode.MakeNumber(_text.Substring(start, _pos - start));
        }

        private void SkipWhitespace()
        {
            while (_pos < _text.Length && _text[_pos] is ' ' or '\t' or '\n' or '\r')
                _pos++;
        }

        private bool Consume(char c)
        {
            if (_pos < _text.Length && _text[_pos] == c) { _pos++; return true; }
            return false;
        }

        private void Expect(char c)
        {
            if (!Consume(c))
                throw Error($"Expected '{c}'");
        }

        private InvalidOperationException Error(string message)
            => new($"FOJS0001: Invalid JSON: {message} at position {_pos}");
    }

    private enum JNodeKind { Object, Array, String, Number, Boolean, Null }

    /// <summary>
    /// JSON tree node produced by <see cref="JsonReader"/>. Unlike an XDM map it
    /// preserves source member order and duplicate keys, which fn:json-to-xml needs
    /// for duplicates='retain' (json-to-xml-018) and fn:parse-json for use-last.
    /// Strings carry both the processed form (per the escape/fallback options) and
    /// the fully decoded canonical form (duplicate detection, escaped-attribute
    /// detection in fn:json-to-xml).
    /// </summary>
    private sealed class JNode
    {
        public static readonly JNode True = new() { Kind = JNodeKind.Boolean, Bool = true };
        public static readonly JNode False = new() { Kind = JNodeKind.Boolean, Bool = false };
        public static readonly JNode Null = new() { Kind = JNodeKind.Null };

        public JNodeKind Kind;
        public bool Bool;
        public string? Text;   // String: processed form; Number: raw lexical form
        public string? Canon;  // String: fully decoded canonical form
        public List<JNode>? Items;
        public List<KeyValuePair<(string Key, string Canon), JNode>>? Members;

        public static JNode MakeString(string text, string canon) => new() { Kind = JNodeKind.String, Text = text, Canon = canon };
        public static JNode MakeNumber(string raw) => new() { Kind = JNodeKind.Number, Text = raw };
        public static JNode MakeArray(List<JNode> items) => new() { Kind = JNodeKind.Array, Items = items };
        public static JNode MakeObject(List<KeyValuePair<(string Key, string Canon), JNode>> members) => new() { Kind = JNodeKind.Object, Members = members };
    }

    /// <summary>
    /// Converts a parsed JSON tree to the XDM map/array model of fn:parse-json,
    /// applying the duplicates option on the fully decoded canonical keys
    /// (F+O 3.1 §17.5.1 — fn-parse-json-108/109/110).
    /// </summary>
    private static XdmValue JNodeToXdm(JNode node, JsonOptions options)
    {
        switch (node.Kind)
        {
            case JNodeKind.String:
                return XdmValue.FromString(node.Text!);
            case JNodeKind.Number:
                // JSON numbers map to xs:double; the reader's grammar guarantees a
                // parseable lexical form (overflow yields ±INF in .NET Core).
                return XdmValue.FromDouble(double.Parse(node.Text!, NumberStyles.Float, CultureInfo.InvariantCulture));
            case JNodeKind.Boolean:
                return node.Bool ? XdmValue.True : XdmValue.False;
            case JNodeKind.Null:
                return XdmValue.Undefined;
            case JNodeKind.Array:
                {
                    var array = new XdmArray();
                    foreach (var item in node.Items!)
                        array.Add(JNodeToXdm(item, options));
                    return XdmValue.FromArray(array);
                }
            case JNodeKind.Object:
                {
                    var map = new XdmMap();
                    var canonKeys = new Dictionary<string, XdmValue>();
                    foreach (var member in node.Members!)
                    {
                        var (keyText, canon) = member.Key;
                        var value = JNodeToXdm(member.Value, options);
                        if (canonKeys.TryGetValue(canon, out var existingKey))
                        {
                            if (options.Duplicates == "reject")
                                throw new InvalidOperationException("FOJS0003: Duplicate key in JSON object");
                            if (options.Duplicates == "use-last")
                            {
                                map.Remove(existingKey);
                                var newKey = XdmValue.FromString(keyText);
                                map.Add(newKey, value);
                                canonKeys[canon] = newKey;
                            }
                            // "use-first" (default): keep the first occurrence
                        }
                        else
                        {
                            var key = XdmValue.FromString(keyText);
                            map.Add(key, value);
                            canonKeys[canon] = key;
                        }
                    }
                    return XdmValue.FromMap(map);
                }
            default:
                return XdmValue.Undefined;
        }
    }

    /// <summary>
    /// Converts a parsed JSON tree to the XML representation of fn:json-to-xml
    /// (F+O 3.1 §17.5.4). Duplicate keys follow the duplicates option on the decoded
    /// canonical key (retain emits all occurrences — json-to-xml-018); with
    /// escape=true, string values and keys that still contain escape sequences are
    /// marked escaped / escaped-key (json-to-xml-019/021/024/049).
    /// </summary>
    private static XElement JNodeToXml(JNode node, JsonOptions options, (string Text, string Canon)? key = null)
    {
        XElement element;
        switch (node.Kind)
        {
            case JNodeKind.Object:
                {
                    var mapEl = new XElement(XName.Get("map", JsonXmlNs));
                    var seenKeys = new HashSet<string>();
                    foreach (var member in node.Members!)
                    {
                        var (keyText, canon) = member.Key;
                        if (!seenKeys.Add(canon))
                        {
                            if (options.Duplicates == "reject")
                                throw new InvalidOperationException("FOJS0003: Duplicate key in JSON object");
                            if (options.Duplicates == "use-first")
                                continue;
                            // "retain": every occurrence appears as a sibling.
                        }
                        mapEl.Add(JNodeToXml(member.Value, options, (keyText, canon)));
                    }
                    element = mapEl;
                    break;
                }
            case JNodeKind.Array:
                {
                    var arrEl = new XElement(XName.Get("array", JsonXmlNs));
                    foreach (var item in node.Items!)
                        arrEl.Add(JNodeToXml(item, options));
                    element = arrEl;
                    break;
                }
            case JNodeKind.String:
                element = new XElement(XName.Get("string", JsonXmlNs), node.Text);
                break;
            case JNodeKind.Number:
                element = new XElement(XName.Get("number", JsonXmlNs), node.Text);
                break;
            case JNodeKind.Boolean:
                element = new XElement(XName.Get("boolean", JsonXmlNs), node.Bool ? "true" : "false");
                break;
            default:
                element = new XElement(XName.Get("null", JsonXmlNs));
                break;
        }

        // Stable attribute order: key, escaped-key, escaped.
        if (key is { } k)
        {
            element.SetAttributeValue(XName.Get("key"), k.Text);
            if (options.Escape && k.Text != k.Canon)
                element.SetAttributeValue(XName.Get("escaped-key"), "true");
        }
        if (options.Escape && node.Kind == JNodeKind.String && node.Text != node.Canon)
            element.SetAttributeValue(XName.Get("escaped"), "true");
        return element;
    }

    private static bool IsValidXmlChar(char c)
        => c is '\t' or '\n' or '\r' or >= '\u0020' && c is <= '\uD7FF' or >= '\uE000' && c is <= '\uFFFD';

    /// <summary>
    /// Appends a character in canonically escaped JSON form (escape=true mode):
    /// named escapes where available, \uXXXX for other non-XML characters, and the
    /// character itself otherwise.
    /// </summary>
    private static void AppendEscapedJsonChar(StringBuilder sb, char c)
    {
        switch (c)
        {
            // The quotation mark is decoded like any other valid XML character
            // (json-to-xml-049); only the reverse solidus must stay escaped.
            case '"': sb.Append('"'); break;
            case '\\': sb.Append("\\\\"); break;
            case '\b': sb.Append("\\b"); break;
            case '\f': sb.Append("\\f"); break;
            case '\n': sb.Append("\\n"); break;
            case '\r': sb.Append("\\r"); break;
            case '\t': sb.Append("\\t"); break;
            default:
                if (IsValidXmlChar(c)) sb.Append(c);
                else sb.Append($"\\u{(int)c:X4}");
                break;
        }
    }

    /// <summary>
    /// Routes an escape sequence that denotes a non-XML character through the fallback
    /// function (F+O 3.1 §17.5.1). Without an explicit fallback option the default
    /// fallback returns the replacement character U+FFFD. The fallback must be a
    /// function item of arity 1 returning xs:string (XPTY0004 otherwise).
    /// </summary>
    private static string InvokeJsonFallback(JsonOptions options, EvaluationContext ctx, string escapeSequence)
    {
        if (options.Fallback is not { } fallbackValue || fallbackValue.IsUndefined)
            return "\uFFFD";
        if (!fallbackValue.IsFunction || ((FunctionItem)fallbackValue.FunctionValue).Arity != 1)
            throw new InvalidOperationException("XPTY0004: The fallback option must be a function item of arity 1");
        var fallbackResult = VmEngine.InvokeFunctionItem(fallbackValue, ctx, new[] { XdmValue.FromString(escapeSequence) });
        var atomized = AtomizeValue(fallbackResult);
        if (atomized.Kind != XdmValueKind.String)
            throw new InvalidOperationException("XPTY0004: The fallback function must return an xs:string");
        return atomized.StringValue;
    }

    private static XdmValue JsonToXml_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        // fn:json-to-xml($input as xs:string?): the empty sequence yields the empty
        // sequence (json-to-xml-035).
        if (args[0].IsUndefined || IsEmptySequence(args[0]))
            return XdmValue.Undefined;
        // fn:json-to-xml retains duplicate keys by default (json-to-xml-018).
        return JsonToXml(ctx, AtomizedString(args[0]), JsonOptions.Default with { Duplicates = "retain" });
    }

    private static XdmValue JsonToXml_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (args[0].IsUndefined || IsEmptySequence(args[0]))
            return XdmValue.Undefined;   // json-to-xml-028
        return JsonToXml(ctx, AtomizedString(args[0]), ParseJsonOptions(args[1], forJsonToXml: true));
    }

    private static XdmValue JsonToXml(EvaluationContext ctx, string json, JsonOptions options)
    {
        if (string.IsNullOrEmpty(json))
            throw new InvalidOperationException("FOJS0001: Empty string is not valid JSON");
        // A leading U+FEFF (byte order mark) is ignored (json-to-xml-015).
        if (json[0] == '\uFEFF')
            json = json.Substring(1);
        var root = new JsonReader(json, options, ctx).ParseDocument();
        var xdoc = new XDocument(JNodeToXml(root, options));
        // The base URI of the result document is the static base URI of the function
        // call (json-to-xml-041); XDocumentNode reads this string annotation.
        if (!string.IsNullOrEmpty(ctx.BaseUri))
            xdoc.AddAnnotation(ctx.BaseUri);

        if (options.Validate)
        {
            // Validate the generated XML representation against the W3C schema-for-JSON.
            // This populates PSVI annotations so that $node instance of element(j:map, j:mapType)
            // and typed-value access (e.g. data($n) instance of xs:double for j:number) work.
            var validationErrors = new List<string>();
            bool hasErrors = false;
            ValidationEventHandler handler = (sender, e) =>
            {
                if (e.Severity == XmlSeverityType.Error)
                {
                    hasErrors = true;
                    validationErrors.Add(e.Message);
                }
            };

            xdoc.Validate(JsonSchemaSet, handler, addSchemaInfo: true);
            if (hasErrors)
                throw new InvalidOperationException($"FOJS0003: The JSON XML representation is not valid: {string.Join("; ", validationErrors)}");
        }

        return XdmValue.FromNode(new XDocumentNode(xdoc));
    }

    private static XdmValue XmlToJson_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XmlToJson(args[0], JsonOptions.Default);

    private static XdmValue XmlToJson_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => XmlToJson(args[0], ParseJsonOptions(args[1]));

    private static XdmValue XmlToJson(XdmValue nodeValue, JsonOptions options)
    {
        // fn:xml-to-json takes node()?: the empty sequence yields the empty sequence
        // (xml-to-json-066); a single-node sequence is unwrapped (xml-to-json-D cluster);
        // more than one node is XPTY0004 (xml-to-json-C-001).
        if (nodeValue.IsUndefined || IsEmptySequence(nodeValue))
            return XdmValue.Undefined;
        if (nodeValue.IsSequence)
        {
            XdmValue? single = null;
            foreach (var item in XdmSequence.FromSource(nodeValue.SequenceValue!))
            {
                if (single is not null)
                    throw new InvalidOperationException("XPTY0004: xml-to-json requires a single node, got a sequence");
                single = item;
            }
            nodeValue = single ?? XdmValue.Undefined;
            if (nodeValue.IsUndefined)
                return XdmValue.Undefined;
        }
        if (!nodeValue.IsNode)
            throw new InvalidOperationException("XPTY0004: xml-to-json requires a node");

        var node = nodeValue.NodeValue;
        IXdmNode? root = null;
        if (node.NodeKind == XdmNodeKind.Document)
        {
            // fn:xml-to-json requires the document to contain exactly one element child
            // (FOJS0006 otherwise — xml-to-json-C001: two top-level j:string elements).
            foreach (var child in node.Axis(XdmAxis.Child))
            {
                var childNode = child.NodeValue!;
                if (childNode.NodeKind == XdmNodeKind.Element)
                {
                    if (root is not null)
                        throw new InvalidOperationException("FOJS0006: Document node has more than one element child");
                    root = childNode;
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
        => XmlNodeToJsonString(node, sb, options, isMapEntry: false, depth: 0);

    /// <summary>
    /// Serializes one element of the JSON XML representation (F+O 3.1 §17.5.4), validating
    /// the input against the schema rules: unknown elements/attributes, misplaced text,
    /// duplicate keys, invalid numbers and invalid escape sequences raise FOJS0006/FOJS0007.
    /// With indent=true the output is pretty-printed (the layout is implementation-defined).
    /// </summary>
    private static void XmlNodeToJsonString(IXdmNode node, StringBuilder sb, JsonOptions options, bool isMapEntry, int depth = 0)
    {
        if (node.NamespaceUri != JsonXmlNs)
            throw new InvalidOperationException($"FOJS0006: Element {{{node.NamespaceUri}}}{node.LocalName} is not in the JSON XML namespace");
        var localName = node.LocalName;
        bool isString = localName == "string";
        bool escaped = false;

        // Validate attributes: @key/@escaped-key/@escaped are allowed on any element
        // (bug 29917; xml-to-json-064/065); @key/@escaped-key only take effect for map
        // entries, @escaped only on j:string. Namespace declarations and XML/XSI
        // attributes are ignored (xml-to-json-D-001/D-002/D-302).
        foreach (var attr in node.Attributes())
        {
            var attrNode = attr.NodeValue!;
            var attrNs = attrNode.NamespaceUri;
            if (attrNode.LocalName == "xmlns" || attrNode.Prefix == "xmlns"
                || attrNs == "http://www.w3.org/XML/1998/namespace"
                || attrNs == "http://www.w3.org/2001/XMLSchema-instance")
                continue;
            var attrName = attrNode.LocalName;
            var attrValue = attrNode.StringValue;
            switch (attrName)
            {
                case "key":
                    break; // allowed everywhere; read by the containing map
                case "escaped-key":
                    _ = ParseJsonXmlBoolean(attrValue, "escaped-key");
                    break;
                case "escaped":
                    escaped = ParseJsonXmlBoolean(attrValue, "escaped") && isString;
                    break;
                default:
                    throw new InvalidOperationException($"FOJS0006: Attribute @{attrName} is not allowed on j:{localName}");
            }
        }

        switch (localName)
        {
            case "map":
                {
                    var entries = ElementChildrenOnly(node, localName);
                    sb.Append('{');
                    var first = true;
                    var seenKeys = new HashSet<string>();
                    foreach (var child in entries)
                    {
                        if (!first) sb.Append(',');
                        first = false;
                        if (options.Indent) AppendJsonIndent(sb, depth + 1);
                        var childNode = child.NodeValue!;
                        string? entryKey = null;
                        bool entryEscapedKey = false;
                        foreach (var attr in childNode.Attributes())
                        {
                            var attrNode = attr.NodeValue!;
                            if (attrNode.LocalName == "key")
                                entryKey = attrNode.StringValue;
                            else if (attrNode.LocalName == "escaped-key")
                                entryEscapedKey = ParseJsonXmlBoolean(attrNode.StringValue, "escaped-key");
                        }
                        if (entryKey is null)
                            throw new InvalidOperationException("FOJS0006: Missing key attribute in map entry");
                        // Duplicate detection compares the unescaped key (xml-to-json-D-501..503);
                        // the output copies valid escape sequences unchanged (xml-to-json-071).
                        var rawKey = entryEscapedKey ? UnescapeJsonContentStrict(entryKey) : entryKey;
                        if (!seenKeys.Add(rawKey))
                            throw new InvalidOperationException($"FOJS0006: Duplicate key '{rawKey}' in map");
                        sb.Append('"');
                        sb.Append(entryEscapedKey ? CopyEscapedJsonContent(entryKey) : XmlToJsonEscape(entryKey));
                        sb.Append(options.Indent ? "\": " : "\":");
                        XmlNodeToJsonString(childNode, sb, options, isMapEntry: true, depth + 1);
                    }
                    if (options.Indent && !first) AppendJsonIndent(sb, depth);
                    sb.Append('}');
                    break;
                }
            case "array":
                {
                    var items = ElementChildrenOnly(node, localName);
                    sb.Append('[');
                    var first = true;
                    foreach (var child in items)
                    {
                        if (!first) sb.Append(',');
                        first = false;
                        if (options.Indent) AppendJsonIndent(sb, depth + 1);
                        XmlNodeToJsonString(child.NodeValue!, sb, options, isMapEntry: false, depth + 1);
                    }
                    if (options.Indent && !first) AppendJsonIndent(sb, depth);
                    sb.Append(']');
                    break;
                }
            case "string":
                {
                    var content = ElementTextContent(node, localName);
                    sb.Append('"');
                    // escaped="true": valid escape sequences are copied to the output
                    // unchanged; only unescaped characters that JSON requires to be
                    // escaped are escaped (xml-to-json-071/072/073).
                    sb.Append(escaped ? CopyEscapedJsonContent(content) : XmlToJsonEscape(content));
                    sb.Append('"');
                    break;
                }
            case "number":
                {
                    // The content is cast to xs:double and serialized with XPath double
                    // formatting (xml-to-json-D-201..206); INF/NaN are not JSON numbers.
                    var content = ElementTextContent(node, localName).Trim();
                    if (!JsonXmlNumberPattern.IsMatch(content)
                        || !double.TryParse(content, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
                        throw new InvalidOperationException($"FOJS0006: '{content}' is not a valid JSON number");
                    sb.Append(XdmValue.FromDouble(number).ToString());
                    break;
                }
            case "boolean":
                {
                    var content = ElementTextContent(node, localName).Trim();
                    sb.Append(content switch
                    {
                        "true" or "1" => "true",
                        "false" or "0" => "false",
                        _ => throw new InvalidOperationException($"FOJS0006: '{content}' is not a valid JSON boolean")
                    });
                    break;
                }
            case "null":
                {
                    var content = ElementTextContent(node, localName);
                    if (content.Trim().Length != 0)
                        throw new InvalidOperationException("FOJS0006: j:null must be empty");
                    sb.Append("null");
                    break;
                }
            default:
                throw new InvalidOperationException($"FOJS0006: Unexpected element {localName} in JSON XML representation");
        }
    }

    private static readonly System.Text.RegularExpressions.Regex JsonXmlNumberPattern =
        new(@"^[+-]?([0-9]+(\.[0-9]*)?|\.[0-9]+)([eE][-+]?[0-9]+)?$", System.Text.RegularExpressions.RegexOptions.Compiled);

    /// <summary>
    /// Returns the element children of a map/array, rejecting non-whitespace text
    /// (FOJS0006; xml-to-json-C-011/C-015).
    /// </summary>
    private static List<XdmValue> ElementChildrenOnly(IXdmNode node, string localName)
    {
        var elements = new List<XdmValue>();
        foreach (var child in node.Axis(XdmAxis.Child))
        {
            var childNode = child.NodeValue!;
            if (childNode.NodeKind == XdmNodeKind.Element)
            {
                elements.Add(child);
            }
            else if (childNode.NodeKind == XdmNodeKind.Text && childNode.StringValue.Trim().Length != 0)
            {
                throw new InvalidOperationException($"FOJS0006: j:{localName} must not have text content");
            }
            // Whitespace-only text and comments/PIs are ignored.
        }
        return elements;
    }

    /// <summary>
    /// Returns the concatenated text content of a leaf j:* element, rejecting element
    /// children (FOJS0006; xml-to-json-C-008/C-009).
    /// </summary>
    private static string ElementTextContent(IXdmNode node, string localName)
    {
        var sb = new StringBuilder();
        foreach (var child in node.Axis(XdmAxis.Child))
        {
            var childNode = child.NodeValue!;
            if (childNode.NodeKind == XdmNodeKind.Element)
                throw new InvalidOperationException($"FOJS0006: j:{localName} must not have element children");
            // Comments and processing instructions are not content (xml-to-json-D-101).
            if (childNode.NodeKind == XdmNodeKind.Text)
                sb.Append(childNode.StringValue);
        }
        return sb.ToString();
    }

    private static bool ParseJsonXmlBoolean(string value, string attributeName)
        => value.Trim() switch
        {
            "true" or "1" => true,
            "false" or "0" => false,
            _ => throw new InvalidOperationException($"FOJS0006: Invalid value '{value}' for @{attributeName}")
        };

    /// <summary>
    /// Appends a newline followed by two spaces per depth level (fn:xml-to-json with
    /// indent=true; the exact layout is implementation-defined — xml-to-json-056/057).
    /// </summary>
    private static void AppendJsonIndent(StringBuilder sb, int depth)
        => sb.Append('\n').Append(' ', depth * 2);

    /// <summary>
    /// Processes escaped="true" string content or an escaped-key attribute value
    /// (fn:xml-to-json): valid escape sequences are copied to the output unchanged
    /// (preserving the hex digit case); unescaped characters that JSON requires to be
    /// escaped are escaped. Invalid escape sequences raise FOJS0007 (xml-to-json-071..078).
    /// </summary>
    private static string CopyEscapedJsonContent(string content)
    {
        if (!content.Contains('\\'))
            return XmlToJsonEscape(content);
        var sb = new StringBuilder(content.Length + 8);
        int i = 0;
        int runStart = 0;
        while (i < content.Length)
        {
            if (content[i] != '\\')
            {
                i++;
                continue;
            }
            // Escape the plain run preceding this escape sequence.
            if (i > runStart)
                sb.Append(XmlToJsonEscape(content[runStart..i]));
            if (i + 1 >= content.Length)
                throw new InvalidOperationException("FOJS0007: Trailing backslash in escaped string content");
            char esc = content[i + 1];
            switch (esc)
            {
                case '"' or '\\' or '/' or 'b' or 'f' or 'n' or 'r' or 't':
                    sb.Append('\\').Append(esc);
                    i += 2;
                    break;
                case 'u':
                    if (i + 5 >= content.Length
                        || !int.TryParse(content.Substring(i + 2, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
                        throw new InvalidOperationException($"FOJS0007: Invalid \\u escape in '{content}'");
                    sb.Append(content, i, 6);
                    i += 6;
                    break;
                default:
                    throw new InvalidOperationException($"FOJS0007: Invalid escape sequence '\\{esc}'");
            }
            runStart = i;
        }
        if (runStart < content.Length)
            sb.Append(XmlToJsonEscape(content[runStart..]));
        return sb.ToString();
    }

    /// <summary>
    /// Strictly unescapes JSON escape sequences in escaped="true" string content or an
    /// escaped-key attribute (fn:xml-to-json). Invalid escapes raise FOJS0007.
    /// </summary>
    private static string UnescapeJsonContentStrict(string content)
    {
        if (!content.Contains('\\'))
            return content;
        var sb = new StringBuilder(content.Length);
        int i = 0;
        while (i < content.Length)
        {
            char c = content[i];
            if (c != '\\')
            {
                sb.Append(c);
                i++;
                continue;
            }
            if (i + 1 >= content.Length)
                throw new InvalidOperationException("FOJS0007: Trailing backslash in escaped string content");
            char esc = content[i + 1];
            switch (esc)
            {
                case '"': sb.Append('"'); i += 2; break;
                case '\\': sb.Append('\\'); i += 2; break;
                case '/': sb.Append('/'); i += 2; break;
                case 'b': sb.Append('\b'); i += 2; break;
                case 'f': sb.Append('\f'); i += 2; break;
                case 'n': sb.Append('\n'); i += 2; break;
                case 'r': sb.Append('\r'); i += 2; break;
                case 't': sb.Append('\t'); i += 2; break;
                case 'u':
                    if (i + 5 >= content.Length
                        || !int.TryParse(content.Substring(i + 2, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int code))
                        throw new InvalidOperationException($"FOJS0007: Invalid \\u escape in '{content}'");
                    sb.Append((char)code);
                    i += 6;
                    break;
                default:
                    throw new InvalidOperationException($"FOJS0007: Invalid escape sequence '\\{esc}'");
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// JSON string escaping per F+O 3.1 §17.5.4: quote, backslash and solidus are escaped;
    /// control characters use short escapes or \u00XX; non-BMP characters are written as
    /// \uXXXX surrogate pairs (xml-to-json-D-005).
    /// </summary>
    private static string XmlToJsonEscape(string s)
    {
        var sb = new StringBuilder(s.Length + 8);
        int i = 0;
        while (i < s.Length)
        {
            char c = s[i];
            switch (c)
            {
                case '"': sb.Append("\\\""); i++; break;
                case '\\': sb.Append("\\\\"); i++; break;
                case '/': sb.Append("\\/"); i++; break;
                case '\b': sb.Append("\\b"); i++; break;
                case '\f': sb.Append("\\f"); i++; break;
                case '\n': sb.Append("\\n"); i++; break;
                case '\r': sb.Append("\\r"); i++; break;
                case '\t': sb.Append("\\t"); i++; break;
                default:
                    if (c < 0x20 || (c >= 0x7F && c <= 0x9F))
                    {
                        sb.Append($"\\u{(int)c:X4}");
                        i++;
                    }
                    else if (char.IsHighSurrogate(c) && i + 1 < s.Length && char.IsLowSurrogate(s[i + 1]))
                    {
                        sb.Append($"\\u{(int)c:X4}\\u{(int)s[i + 1]:X4}");
                        i += 2;
                    }
                    else
                    {
                        sb.Append(c);
                        i++;
                    }
                    break;
            }
        }
        return sb.ToString();
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
    {
        // FO31: an empty-sequence $href yields the empty sequence (json-doc-035).
        if (args[0].IsUndefined || IsEmptySequence(args[0]))
            return XdmValue.Undefined;
        return JsonDoc(ctx, AtomizedString(args[0]), JsonOptions.Default);
    }

    private static XdmValue JsonDoc_2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        // Options are validated eagerly even when $href is empty (json-doc-028).
        var options = ParseJsonOptions(args[1]);
        if (args[0].IsUndefined || IsEmptySequence(args[0]))
            return XdmValue.Undefined;
        return JsonDoc(ctx, AtomizedString(args[0]), options);
    }

    private static XdmValue JsonDoc(EvaluationContext ctx, string uri, JsonOptions options)
    {
        if (string.IsNullOrEmpty(uri))
            throw new InvalidOperationException("FOUT1170: Invalid URI");

        // Resolve relative URIs against the static base URI before loading, so that
        // QT3 test-sets such as misc-JsonTestSuite find their JSON files next to the
        // test-set file instead of in the process working directory.
        string resolvedUri = ResolveUriAgainstBase(uri, ctx.BaseUri);

        string json;
        var mappedPath = ctx.ResourceUriMapper?.Invoke(uri) ?? ctx.ResourceUriMapper?.Invoke(resolvedUri);
        if (mappedPath is not null)
        {
            // The suite maps this (typically http:) URI to a local JSON resource file.
            try
            {
                json = File.ReadAllText(mappedPath);
            }
            catch
            {
                throw new InvalidOperationException($"FOUT1170: Cannot load JSON document {uri}");
            }
        }
        else if (Uri.TryCreate(resolvedUri, UriKind.Absolute, out var resolvedUriObj) && resolvedUriObj.IsFile && File.Exists(resolvedUriObj.LocalPath))
        {
            // Local JSON file: read as plain text. This avoids routing JSON resources
            // through the XML document loader (which is what ctx.DocumentLoader does).
            try
            {
                json = File.ReadAllText(resolvedUriObj.LocalPath);
            }
            catch
            {
                throw new InvalidOperationException($"FOUT1170: Cannot load JSON document {uri}");
            }
        }
        else if (ctx.DocumentLoader is not null)
        {
            try
            {
                var node = ctx.DocumentLoader(resolvedUri);
                json = node.StringValue;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch
            {
                throw new InvalidOperationException($"FOUT1170: Cannot load JSON document {uri}");
            }
        }
        else
        {
            try
            {
                json = File.ReadAllText(resolvedUri);
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
        return CopyOf(ctx, item);
    }

    private static XdmValue CopyOf_1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var item = args[0];
        if (item.IsUndefined)
            return XdmValue.Undefined;
        return CopyOf(ctx, item);
    }

    private static XdmValue CopyOf(EvaluationContext ctx, XdmValue item)
    {
        // If a singleton sequence was passed, extract the node.
        if (item.IsSequence)
        {
            var items = new List<XdmValue>();
            foreach (var seqItem in XdmSequence.FromSource(item.SequenceValue!))
                items.Add(seqItem);
            if (items.Count == 1)
                return CopyOf(ctx, items[0]);
            // Map over each item for multi-item sequences.
            var results = new List<XdmValue>(items.Count);
            foreach (var i in items)
                results.Add(CopyOf(ctx, i));
            return XdmValue.FromSequence(MaterializedSequence.FromList(results));
        }

        if (item.IsNode && item.NodeValue != null)
        {
            var copied = DeepCopyNode(item.NodeValue);
            if (copied != null)
            {
                // XSLT hook: fn:copy-of copies accumulator values onto the copy.
                ctx.AccumulatorValueCopier?.Invoke(item.NodeValue, copied);
                return XdmValue.FromNode(copied);
            }
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

    private static XElement DeepCopyElement(XElement element, IReadOnlySet<string>? inheritedPrefixes = null)
    {
        var copy = new XElement(XName.Get(element.Name.LocalName, element.Name.NamespaceName));
        HashSet<string> childPrefixes;
        if (inheritedPrefixes == null)
        {
            // Top-level copy: preserve all in-scope namespaces (xsl:copy-of semantics).
            foreach (var attr in element.Attributes())
            {
                copy.SetAttributeValue(XName.Get(attr.Name.LocalName, attr.Name.NamespaceName), attr.Value);
            }
            CopyInScopeNamespaces(element, copy);
            childPrefixes = new HashSet<string>(StringComparer.Ordinal);
            foreach (var attr in copy.Attributes())
            {
                if (attr.IsNamespaceDeclaration)
                    childPrefixes.Add(attr.Name.LocalName == "xmlns" ? string.Empty : attr.Name.LocalName);
            }
        }
        else
        {
            // Descendant copy: namespace declarations inherited from the copy's
            // ancestors are already in scope; redeclaring them would bloat the
            // serialization and change the attribute set seen by deep-equal
            // (snapshot-0101b).
            foreach (var attr in element.Attributes())
            {
                if (attr.IsNamespaceDeclaration)
                {
                    var prefix = attr.Name.LocalName == "xmlns" ? string.Empty : attr.Name.LocalName;
                    if (inheritedPrefixes.Contains(prefix))
                        continue;
                }
                copy.SetAttributeValue(XName.Get(attr.Name.LocalName, attr.Name.NamespaceName), attr.Value);
            }
            childPrefixes = new HashSet<string>(inheritedPrefixes, StringComparer.Ordinal);
            foreach (var attr in copy.Attributes())
            {
                if (attr.IsNamespaceDeclaration)
                    childPrefixes.Add(attr.Name.LocalName == "xmlns" ? string.Empty : attr.Name.LocalName);
            }
        }
        foreach (var child in element.Nodes())
        {
            switch (child)
            {
                case XElement childElem:
                    copy.Add(DeepCopyElement(childElem, childPrefixes));
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
        if (document.DocumentType is { } docType)
            copy.Add(new XDocumentType(docType.Name, docType.PublicId, docType.SystemId, docType.InternalSubset));
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
                case XText text:
                    // Only whitespace text nodes are valid direct children of XDocument.
                    if (text.Value.All(char.IsWhiteSpace))
                        copy.Add(new XText(text.Value));
                    break;
            }
        }
        // Preserve document base URI and DTD unparsed entity declarations so
        // fn:unparsed-entity-uri() / fn:unparsed-entity-public-id() remain usable.
        if (document.BaseUri is { Length: > 0 } baseUri)
            copy.AddAnnotation(baseUri);
        else if (document.Annotation<string>() is { Length: > 0 } annotatedBaseUri)
            copy.AddAnnotation(annotatedBaseUri);
        if (new Providers.Xml.XDocumentNode(document) is { } srcDocNode)
            srcDocNode.CopyUnparsedEntitiesTo(copy);
        return copy;
    }

    private static XdmValue InScopePrefixes(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        var node = GetOptionalSingleNode(args[0], ctx.BackwardsCompatible);
        if (node is null || node.NodeKind != XdmNodeKind.Element)
            throw new InvalidOperationException("XPTY0004: fn:in-scope-prefixes argument must be an element node.");

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
            // declarations (and undeclarations) win. XML 1.1 prefixed namespace
            // undeclarations hide the same prefixes declared at or above them.
            var undeclared = new HashSet<string>();
            bool isTargetElement = true;
            foreach (var el in path)
            {
                if (el.Annotation<PrefixedNamespaceUndeclarations>() is { } undeclarations)
                    foreach (var undeclaredPrefix in undeclarations.Prefixes)
                        undeclared.Add(undeclaredPrefix);

                foreach (var attr in el.Attributes())
                {
                    // Bindings implied by attribute names do not propagate to descendants
                    // (they count only on the element that carries them).
                    if (!isTargetElement && attr.Annotation<NonPropagatingNamespaceBinding>() is not null)
                        continue;
                    if (attr.IsNamespaceDeclaration)
                    {
                        var prefix = attr.Name.LocalName == "xmlns" ? "" : attr.Name.LocalName;
                        if (undeclared.Contains(prefix))
                            continue;
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
                isTargetElement = false;
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
            return XdmValue.FromString("http://www.w3.org/XML/1998/namespace", "anyURI");

        if (node is Providers.Xml.XDocumentNode xdocNode && xdocNode.UnderlyingObject is XElement elem)
        {
            var current = elem;
            var undeclared = new HashSet<string>();
            bool isTargetElement = true;
            while (current != null)
            {
                // XML 1.1 prefixed namespace undeclarations hide the same prefixes
                // declared at or above them.
                if (current.Annotation<PrefixedNamespaceUndeclarations>() is { } undeclarations)
                    foreach (var undeclaredPrefix in undeclarations.Prefixes)
                        undeclared.Add(undeclaredPrefix);

                foreach (var attr in current.Attributes())
                {
                    // Bindings implied by attribute names do not propagate to descendants.
                    if (!isTargetElement && attr.Annotation<NonPropagatingNamespaceBinding>() is not null)
                        continue;
                    if (attr.IsNamespaceDeclaration)
                    {
                        var attrPrefix = attr.Name.LocalName == "xmlns" ? "" : attr.Name.LocalName;
                        if (attrPrefix == prefix && !undeclared.Contains(attrPrefix))
                        {
                            // xmlns="" undeclares the default namespace.
                            if (attr.Value == "" && prefix == "")
                                return XdmValue.Undefined;
                            // fn:namespace-uri-for-prefix returns xs:anyURI? — the type
                            // annotation matters for map value type tests (analyzeString-028).
                            return XdmValue.FromString(attr.Value, "anyURI");
                        }
                    }
                }
                isTargetElement = false;
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
    public const string ExsltCommon = "http://exslt.org/common";
    public const string Xs = "http://www.w3.org/2001/XMLSchema";
    public const string Xsl = "http://www.w3.org/1999/XSL/Transform";
}



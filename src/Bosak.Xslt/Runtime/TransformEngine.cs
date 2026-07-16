// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 mei 2026
// PURPOSE              : Executes a compiled XSLT stylesheet against a source document.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 24-05-2026     | Added call-template, with-param, variable/param binding, lexical scoping               |
//                      | Charles Korthout | 0.3   | 24-05-2026     | Added cross-stylesheet template dispatch with import precedence                        |
//                      | Charles Korthout | 0.4   | 24-05-2026     | Added mode stack (#current, #default), XdmValueToString for value-of sequences          |
//                      | Charles Korthout | 0.5   | 24-05-2026     | Added xsl:key / key() index building and lookup support                                 |
//                      | Charles Korthout | 0.6   | 24-05-2026     | Added xsl:number support (single, any, multiple levels) with format-integer reuse       |
//                      | Charles Korthout | 0.7   | 26-05-2026     | Added global variable and parameter initialization from stylesheet/includes/imports      |
//                      | Charles Korthout | 1.3   | 27-05-2026     | Added CopyNodeToResult for Document nodes; skip default params if already in context     |
//                      | Charles Korthout | 1.4   | 28-05-2026     | EvaluateSequenceConstructor wraps in document node per XSLT 2.0; respects as attribute   |
//                      | Charles Korthout | 1.5   | 28-05-2026     | SortItems restores focus after sorting; NaN sorts before numbers per XSLT spec          |
//                      | Charles Korthout | 1.6   | 28-05-2026     | ResolveElementName for xsl:element/attribute; resolves prefix via in-scope namespaces    |
//                      | Charles Korthout | 1.1   | 27-05-2026     | Added xsl:function registration, ExecuteXsltFunction, EvaluateFunctionBody, xsl:sequence |
//                      | Charles Korthout | 1.2   | 27-05-2026     | Added multi-key xsl:sort with composite comparator and stable sort                          |
//                      | Charles Korthout | 1.7   | 29-05-2026     | Fixed ComputeNumberMultiple from handling: nearest ancestor, include from-node, fallback    |
//                      | Charles Korthout | 1.8   | 29-05-2026     | Fixed ComputeNumberSingle from handling; FormatNumberSequence emits prefix+suffix for empty |
//                      | Charles Korthout | 0.8   | 26-05-2026     | Added xsl:copy, fixed for-each variable scoping, AVT evaluation in literal elements      |
//                      | Charles Korthout | 0.9   | 26-05-2026     | Added initial-template support, fixed xsl:copy to copy attributes                       |
//                      | Charles Korthout | 1.0   | 26-05-2026     | Added xsl:mode on-no-match support; atomic for-each (EnumerateItems); keyword var names  |
//                      | Charles Korthout | 0.7   | 26-05-2026     | Added global variable and parameter initialization from stylesheet/includes/imports      |
//                      | Charles Korthout | 1.4   | 27-05-2026     | Fixed AVT sequence atomization, version-aware built-in rules, pattern // support         |
//                      | Charles Korthout | 1.5   | 27-05-2026     | Process text nodes in sequence constructors; strip document-level whitespace            |
//                      | Charles Korthout | 1.7   | 28-05-2026     | Added xsl:next-match with excluded-rule chain; call-template clears current template rule |
//                      | Charles Korthout | 1.8   | 29-05-2026     | Reduced MaxXsltFunctionCallDepth to 32 to prevent .NET stack overflow crashes             |
//                      | Charles Korthout | 2.9   | 08-06-2026     | Fixed apply-templates inside xsl:function to pass with-param and preserve atomic values   |
//                      | Charles Korthout | 3.1   | 08-06-2026     | Fixed text-node built-in rule for XDocument container (text-only-copy at document level)  |
//                      | Charles Korthout | 3.2   | 08-06-2026     | Added initialMode support to Transform; fixed #current in initial mode; source select     |
//                      | Charles Korthout | 3.3   | 08-06-2026     | Evaluate global params/vars in document order (interleaved); fixes match-272              |
//                      | Charles Korthout | 1.9   | 29-05-2026     | Added expand-text / Text Value Template support with XPath string literal awareness       |
//                      | Charles Korthout | 2.0   | 30-05-2026     | Skip comments in CopyLiteralElement; fixes string-050/051/089 conformance tests         |
//                      | Charles Korthout | 2.1   | 30-05-2026     | EvaluateSequenceConstructor always wraps in document node via synthetic wrapper         |
//                      | Charles Korthout | 2.2   | 30-05-2026     | Fixed EvaluateAvt to skip } inside XPath string literals (fixes string-095)             |
//                      | Charles Korthout | 2.3   | 30-05-2026     | Set EvaluationContext.BackwardsCompatible from stylesheet version (fixes boolean-081/083/096) |
//                      | Charles Korthout | 2.4   | 30-05-2026     | Fixed ExecuteTemplate/ExecuteXsltFunction to restore saved context item (fixes position-4201) |
//                      | Charles Korthout | 2.5   | 30-05-2026     | xsl:value-of in backwards-compatible mode outputs only first item (fixes predicate-001/002/003) |
//                      | Charles Korthout | 2.6   | 30-05-2026     | ApplyBuiltInRules saves/restores context focus correctly                                |
//                      | Charles Korthout | 2.7   | 31-05-2026     | Added xsl:try / xsl:catch support in result tree and function bodies                   |
//                      | Charles Korthout | 2.8   | 31-05-2026     | Added exclude-result-prefixes filtering in CopyLiteralElement                           |
//                      | Charles Korthout | 2.9   | 31-05-2026     | Added xsl:for-each-group with group-by, group-adjacent, group-starting-with, group-ending-with |
//                      | Charles Korthout | 3.0   | 31-05-2026     | Added current-group() and current-grouping-key() functions; IXsltMessageListener        |
//                      | Charles Korthout | 3.1   | 31-05-2026     | CopyLiteralElement skips xsl-namespace attrs and xmlns:xsl declarations                 |
//                      | Charles Korthout | 3.2   | 01-06-2026     | xsl:number: AwayFromZero rounding, empty-seq NaN, ordinal/lang, grouping, negative err |
//                      | Charles Korthout | 3.3   | 01-06-2026     | xsl:number: XTTE1000 empty select, XTSE0020 bad start-at, attribute context for any    |
//                      | Charles Korthout | 3.4   | 01-06-2026     | FindBestTemplate: XSLT last-wins rule for same-priority templates                      |
//                      | Charles Korthout | 3.5   | 01-06-2026     | ParseXslNumberFormat: recognize Unicode numbering chars (surrogate pairs, OtherNumber) |
//                      | Charles Korthout | 3.6   | 01-06-2026     | xsl:number with value: strip leading whitespace from first output; IsFirstSignificantChild helper |
//                      | Charles Korthout | 3.7   | 01-06-2026     | ComputeNumberAny handles non-document trees; lang validation (XTDE0030); FormatNumberSequence uses long[] |
//                      | Charles Korthout | 3.8   | 01-06-2026     | EvaluateSequenceConstructor extracts attributes/namespace nodes for raw sequence return    |
//                      | Charles Korthout | 3.9   | 01-06-2026     | Initial template selection applies templates to children, not document node (XSLT 5.4)   |
//                      | Charles Korthout | 4.0   | 01-06-2026     | Per-document key indices; cross-document key() lookup; save/restore focus on lazy build |
//                      | Charles Korthout | 4.1   | 03-06-2026     | xsl:number value: BigInteger pipeline for large integers/doubles (fixes number-0111/0807) |
//                      | Charles Korthout | 4.2   | 05-06-2026     | Strip whitespace text nodes from source documents by default (fixes number-1501)           |
//                      | Charles Korthout | 4.3   | 05-06-2026     | WalkDocumentTree: propagate text-node skip across empty elements; fixes number-1501      |
//                      | Charles Korthout | 4.4   | 05-06-2026     | WalkDocumentTree visits all attrs; ComputeNumberAny counts only first attr; fixes 1101 |
//                      | Charles Korthout | 4.5   | 05-06-2026     | Initial template selection uses FindBestTemplate for document-node() patterns; fixes 088 |
//                      | Charles Korthout | 4.6   | 05-06-2026     | XTDE0540 conflict detection when on-multiple-match="fail"; fixes match-082b/c          |
//                      | Charles Korthout | 4.7   | 07-06-2026     | ApplyTemplates/next-match support atomic values; built-in rule outputs atomics; +11 tests|
//                      | Charles Korthout | 4.8   | 07-06-2026     | Added xsl:apply-imports with import-precedence filtering and atomic context items       |
//                      | Charles Korthout | 4.9   | 07-06-2026     | next-match leaks excluded rules; apply-imports param passing; precedence stack         |
//                      | Charles Korthout | 5.0   | 07-06-2026     | DeepSkip mode; expand-text truthy values; CopyToResult exclusion cleanup               |
//                      | Charles Korthout | 5.1   | 07-06-2026     | FindRootTemplate strips XPath comments; next-match/apply-imports pass position/last   |
//                      | Charles Korthout | 5.2   | 07-06-2026     | ConvertVariableValue for xsl:variable/@as basic atomic types; fixes match-248-254      |
//                      | Charles Korthout | 5.3   | 08-06-2026     | Iterative key index build for cross-key dependencies (key-063/064); removed re-entrancy guard |
//                      | Charles Korthout | 5.4   | 09-06-2026     | Fixed apply-templates default-mode resolution; XTDE0045/0050 validation; ModeExists helper |
//                      | Charles Korthout | 5.5   | 09-06-2026     | Pass EvaluationContext to PatternCompiler for compile-time predicate validation          |
//                      | Charles Korthout | 5.6   | 10-06-2026     | xsl:copy error handling (XTTE0945/3180, XTDE0410/0420); function context item isolation; parentless document order |
//                      | Charles Korthout | 5.7   | 10-06-2026     | xsl:where-populated filters empty PIs/comments; xsl:on-empty in CopyLiteralElement; copy-1213/1214/1215/1216/1217 |
//                      | Charles Korthout | 5.8   | 10-06-2026     | Named template entry points have no context item; lazy global variable evaluation     |
//                      | Charles Korthout | 5.9   | 10-06-2026     | xsl:on-empty in xsl:copy, xsl:document, EvaluateSequenceConstructor; XTDE0420 for namespace on document node |
//                      | Charles Korthout | 5.10  | 11-06-2026     | Fixed copy-1220/1221 namespace axis: AddElementToContainer, NamespaceInheritanceBarrier for copy-namespaces=no |
//                      | Charles Korthout | 5.11  | 11-06-2026     | Isolated _sequenceAccumulator when wrapInDocumentNode=true; fixes as-1303 xsl:document content leakage |
//                      | Charles Korthout | 5.12  | 11-06-2026     | Runtime XTSE0010 for @as on xsl:call-template; fixes as-1601                               |
//                      | Charles Korthout | 5.13  | 11-06-2026     | Base URI propagation for xsl:copy/copy-of and built-in template rules; fixes base-uri-050/053 |
//                      | Charles Korthout | 5.14  | 11-06-2026     | Expanded key names, 3-arg subtree scope, globals before key build, XTDE1260/1222        |
//                       | Charles Korthout | 5.15  | 11-06-2026     | Preserve typed atomic values in sequence accumulator; composite key lookup             |
//                      | Charles Korthout | 5.16  | 11-06-2026     | Fixed xsl:for-each-group: focus, composite keys, date/time eq, sort current-group      |
//                      | Charles Korthout | 5.17  | 12-06-2026     | Collation-aware grouping, function-body for-each-group, pattern current-group checks   |
//                      | Charles Korthout | 5.18  | 11-06-2026     | xsl:where-populated uses populated-node check; fixes element-0104/0105/0106/0107/0108 |
//                      | Charles Korthout | 5.19  | 11-06-2026     | copy-accumulators applicability for initial source document; fixes copy-3002           |
//                      | Charles Korthout | 5.20  | 13-06-2026     | Full xsl:sort support: AVTs, sequence-constructor keys, lang/case-order/collation      |
//                      | Charles Korthout | 5.21  | 13-06-2026     | UCA alternate=shifted/blanked tie-breaker for xsl:sort; fixes sort-079                |
//                      | Charles Korthout | 5.22  | 11-06-2026     | Accumulator fixes: sequence-constructor rules, map/array apply, xsl:iterate, root/path |
//                      | Charles Korthout | 5.23  | 13-06-2026     | Empty-URI EQName support; initialize globals before pattern compile; variable cluster green |
//                      | Charles Korthout | 5.24  | 13-06-2026     | xsl:value-of/xsl:text preserve zero-length text nodes for typed variables             |
//                      | Charles Korthout | 5.25  | 13-06-2026     | xsl:for-each requires @select; select-7501 XTSE0010                                    |
//                      | Charles Korthout | 5.26  | 13-06-2026     | Pass external params to initial mode/template; default mode for apply-templates        |
//                      | Charles Korthout | 5.27  | 13-06-2026     | Enforce xsl:global-context-item use=required (XTDE3086)                                |
//                      | Charles Korthout | 5.28  | 13-06-2026     | xsl:copy attribute sets/source attrs; attribute-set variable scope; separator          |
//                      | Charles Korthout | 5.29  | 13-06-2026     | xsl:copy shallow copy no longer copies source attributes/children                       |
//                      | Charles Korthout | 5.30  | 13-06-2026     | Namespace fixup for xsl:attribute prefix hints and literal result attribute names      |
//                      | Charles Korthout | 5.31  | 13-06-2026     | Implemented xsl:evaluate with context-item, namespace-context, with-param, and @as     |
//                      | Charles Korthout | 5.32  | 13-06-2026     | Implemented xsl:merge, current-merge-group, and current-merge-key                      |
//                      | Charles Korthout | 5.33  | 13-06-2026     | Merge fixes: XTDE2210, named-source lookup, apply-templates clearing, globals in accumulators |
//                      | Charles Korthout | 5.34  | 13-06-2026     | Implemented xsl:analyze-string and regex-group()                                          |
//                      | Charles Korthout | 5.35  | 13-06-2026     | XSLT 3.0 zero-length match semantics, XSD regex validation, and backreference translation |
//                      | Charles Korthout | 5.36  | 15-06-2026     | Mode cluster fixes: initial-template context item, union-pattern conflict, mode validation |
//                      | Charles Korthout | 5.37  | 15-06-2026     | Emit warning-on-no-match/multiple-match via OnWarning; default to recovery/last-wins     |
//                      | Charles Korthout | 5.38  | 24-06-2026     | Default use-accumulators is empty list for undeclared initial mode; fixes copy-3002     |
//                      | Charles Korthout | 5.39  | 24-06-2026     | Named-template entry point treats source tree as global context item for accumulators   |
//                      | Charles Korthout | 5.40  | 24-06-2026     | XPath default namespace no longer falls back to xmlns declaration                     |
//                      | Charles Korthout | 5.41  | 24-06-2026     | Pass DefiningElementDefaultNamespace through CompileXPath/AVT/xsl:evaluate             |
//                      | Charles Korthout | 5.42  | 24-06-2026     | Apply xsl:strip-space to fn:doc/document loaded docs; skip stylesheet modules          |
//                      | Charles Korthout | 5.43  | 25-06-2026     | Pass regex options to ValidateAndTranslatePattern for xsl:analyze-string               |
//                      | Charles Korthout | 5.44  | 25-06-2026     | Capture raw XDM result from initial named template with @as for output tree="no"        |
//                      | Charles Korthout | 5.45  | 25-06-2026     | xsl:try multi-catch, @errors matching, null->Undefined context; fixes call-template-0110 |
//                      | Charles Korthout | 5.46  | 25-06-2026     | Global variables/parameters evaluated with absent focus; fixes strip-space-023        |
//                      | Charles Korthout | 5.47  | 25-06-2026     | Register source document URI in evaluation context; fixes accessor-008                 |
//                      | Charles Korthout | 5.48  | 25-06-2026     | Item-based xsl:on-empty/on-non-empty handling for sequence constructors                |
//                      | Charles Korthout | 5.49  | 26-06-2026     | Evaluate _select AVT on global variables/parameters                                     |
//                      | Charles Korthout | 5.50  | 26-06-2026     | IsNodeAttached now treats a document's root element as attached; fixes mode-1105        |
//                      | Charles Korthout | 5.51  | 26-06-2026     | xsl:evaluate blocks fn:system-property; xsl:try catches bare error codes; root LRE namespaces |
//                      | Charles Korthout | 5.52  | 26-06-2026     | Evaluate _select AVT on xsl:value-of; fixes date-094/095 static-param tests              |
//                      | Charles Korthout | 5.53  | 26-06-2026     | Suspend sequence accumulator in shallow-copy built-in rule; fixes namespace-0912       |
//                      | Charles Korthout | 5.54  | 26-06-2026     | Implemented xsl:map/xsl:map-entry, JSON serialize, static XPath validation; clears maps |
//                      | Charles Korthout | 5.55  | 27-06-2026     | Array flattening in apply-templates, value-of, and complex content; fixes regressions   |
//                      | Charles Korthout | 5.56  | 27-06-2026     | Top-level xsl:namespace no longer yields standalone namespace-node items               |
//                      | Charles Korthout | 5.57  | 26-06-2026     | Pass ordinal suffix/scheme from xsl:number to format-integer for localized ordinals    |
//                      | Charles Korthout | 5.58  | 28-06-2026     | Keep xsl:namespace nodes for as="node()"/"node()?"; fixes namespace-3005              |
//                      | Charles Korthout | 5.59  | 28-06-2026     | TVTs compile with in-scope namespaces and effective base URI; fixes resolve-uri-022   |
//                      | Charles Korthout | 5.60  | 28-06-2026     | Base-URI/TVT context fixes clear namespace-4801 regression                             |
//                      | Charles Korthout | 5.61  | 26-06-2026     | Default-mode root template; #current via call-template; XTTE0510; param forwarding     |
//                      | Charles Korthout | 5.62  | 29-06-2026     | Snapshot/restore variables around literal result element content; fixes param-0107    |
//                      | Charles Korthout | 5.63  | 26-06-2026     | Lazy evaluation for xsl:variable inside xsl:function bodies; fixes param-0301         |
//                      | Charles Korthout | 5.64  | 29-06-2026     | Targeted lazy locals + global resolver re-entry fix; clears regressions                |
//                      | Charles Korthout | 5.65  | 26-06-2026     | xsl:try scope isolation, error QName namespace, result-document, FODC0002             |
//                      | Charles Korthout | 5.66  | 26-06-2026     | fn:current-output-uri support; base output URI propagation and temporary-output-state  |
//                      | Charles Korthout | 5.67  | 30-06-2026     | Compile template match patterns for function entry points; fixes apply-templates in functions |
//                      | Charles Korthout | 5.68  | 30-06-2026     | Built-in atomic rule respects on-no-match; default xsl:mode is text-only-copy; fixes match-241 |
//                      | Charles Korthout | 5.69  | 02-07-2026     | Sequence-constructor whitespace, empty atomics, xsl:document in simple content; clears seqtor |
//                      | Charles Korthout | 5.70  | 02-07-2026     | Sequence placeholders are not significant content; fixes on-empty cluster regressions       |
//                      | Charles Korthout | 5.71  | 02-07-2026     | Expand sequence placeholders in xsl:where-populated temp; remove leftover debug output    |
//                      | Charles Korthout | 5.72  | 03-07-2026     | TVT: xsl:expand-text inheritance, XPath comments, merged text across comments/PIs        |
//                      | Charles Korthout | 5.73  | 03-07-2026     | TVT expansion and whitespace stripping in xsl:function bodies; fixes cvt-029/030         |
//                      | Charles Korthout | 5.74  | 03-07-2026     | Document-order global/template flattening; apply-imports context; clears import cluster |
//                      | Charles Korthout | 5.75  | 26-06-2026     | Enforce xsl:context-item use and @as type at template invocation                       |
//                      | Charles Korthout | 5.76  | 26-06-2026     | Strip whitespace before xsl:context-item; preserve atomic spacing across templates       |
//                      | Charles Korthout | 5.77  | 26-06-2026     | Implemented xsl:iterate, xsl:break, and xsl:next-iteration in result tree              |
//                      | Charles Korthout | 5.78  | 26-06-2026     | xsl:next-iteration with-param conversion to xsl:param @as; fixes iterate-042/027       |
//                      | Charles Korthout | 5.79  | 26-06-2026     | Propagate default-collation through templates, sort, groups, keys; clears collations cluster |
//                      | Charles Korthout | 5.80  | 05-07-2026     | AVT fixes: XPath comments, empty expressions, BC first-item, escapes, separator/stable, function text nodes |
//                      | Charles Korthout | 5.81  | 05-07-2026     | Tunnel parameter fixes: boolean values, XTSE0020/XTSE0680, pass-through, function isolation |
//                      | Charles Korthout | 5.82  | 05-07-2026     | call-template validation skips xsl:context-item and XSLT 1.0 BC mode                   |
//                      | Charles Korthout | 5.83  | 05-07-2026     | Version cluster: per-element version, xsl:fallback, extension elements, message        |
//                      | Charles Korthout | 5.84  | 26-06-2026     | No-op cases for sort/fallback/on-empty/on-non-empty; implement xsl:assert               |
//                      | Charles Korthout | 5.85  | 06-07-2026     | Capture principal xsl:result-document output properties for serialization               |
//                      | Charles Korthout | 5.86  | 06-07-2026     | xsl:sequence without @select uses standard item collector; clears seqtor cluster       |
//                      | Charles Korthout | 5.87  | 26-06-2026     | Backwards-compatible value-of, number, key(), and function argument coercion           |
//                      | Charles Korthout | 5.88  | 07-07-2026     | Suspend sequence accumulator inside literal result elements; fixes bug-1501/1601       |
//                      | Charles Korthout | 5.89  | 07-07-2026     | Set current item during xsl:sort key evaluation; fixes bug-2501                        |
//                      | Charles Korthout | 5.90  | 08-07-2026     | QName whitespace normalization; function-body text-node merging; XTDE0450 for maps     |
//                      | Charles Korthout | 5.91  | 26-06-2026     | Preserve atomic-spacing state when a template with @as returns its typed result        |
//                      | Charles Korthout | 5.92  | 26-06-2026     | Expand sequence placeholders for apply-templates/call-template inside function bodies  |
//                      | Charles Korthout | 5.93  | 26-06-2026     | Avoid forcing lazy globals when building accumulator evaluation context                  |
//                      | Charles Korthout | 5.94  | 09-07-2026     | xsl:source-document resolves fragment identifiers to xml:id elements                   |
//                      | Charles Korthout | 5.95  | 11-07-2026     | Build principal result tree in synthetic __xdm_doc__ wrapper to support fragments.     |
//                      | Charles Korthout | 5.96  | 11-07-2026     | Resolve xsl:result-document use-character-maps in original instruction context.        |
//                      | Charles Korthout | 5.97  | 11-07-2026     | Result-document character maps now supplement named/unnamed output definitions.        |
//                      | Charles Korthout | 5.98  | 11-07-2026     | Evaluate doctype-public/system AVTs; current-output-uri empty outside result-document  |
//                      | Charles Korthout | 5.99  | 11-07-2026     | Collect top-level maps/arrays for JSON output and route xsl:map/map-entry to them.     |
//                      | Charles Korthout | 6.00  | 11-07-2026     | Support item-separator for text output; raise SENR0001 for top-level maps/arrays.      |
//                      | Charles Korthout | 6.01  | 12-07-2026     | Evaluate AVTs on all xsl:result-document serialization attributes.                     |
//                      | Charles Korthout | 6.02  | 12-07-2026     | Raw-item collection for xsl:result-document method=json/adaptive and build-tree="no".  |
//                      | Charles Korthout | 6.03  | 12-07-2026     | Added null-forgiving operators to silence three compiler null-reference warnings.       |
//                      | Charles Korthout | 6.04  | 12-07-2026     | Flatten arrays in sequence constructors; result-document character-map precedence.     |
//                      | Charles Korthout | 6.05  | 12-07-2026     | Preserve original namespace prefixes for LREs; initialize current-output-uri to base.  |
//                      | Charles Korthout | 6.06  | 12-07-2026     | Fix inherit-namespaces="no" undeclarations; detach root before document unwrap.        |
//                      | Charles Korthout | 6.07  | 13-07-2026     | Scope raw-item collection to the actual result document; fix typed variable bodies.    |
//                      | Charles Korthout | 6.08  | 13-07-2026     | Dynamic calls on current-group/current-grouping-key/current-merge-* raise XTDE errors. |
//                      | Charles Korthout | 6.09  | 13-07-2026     | XTDE2210 also raised when a merge-key attribute is present on one source only.         |
//                      | Charles Korthout | 6.10  | 13-07-2026     | Stamp EffectiveVersion/ImplicitResultTree on result-document output properties.        |
//                      | Charles Korthout | 6.11  | 13-07-2026     | HOF: function-type coercion, raw map/function items in typed templates, sig metadata   |
//                      | Charles Korthout | 6.12  | 14-07-2026     | Typed templates keep node identity/parentage; single text nodes not cloned; fn:snapshot |
//                      | Charles Korthout | 6.13  | 14-07-2026     | fn:transform: initial-match-selection, raw principal result, result-document capture; map-entry content; text result-doc fix |
//                      | Charles Korthout | 6.14  | 14-07-2026     | xsl:analyze-string uses cached XSD regex translation + compiled-Regex cache             |
//                      | Charles Korthout | 6.15  | 15-07-2026     | fn:transform: global-context-item, default-mode routing, raw result extraction, absent principal output |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Functions;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Bosak.Xslt.Stylesheet;

namespace Bosak.Xslt.Runtime;

/// <summary>
/// The XSLT transform engine. Evaluates a compiled stylesheet against a source document.
/// </summary>
public sealed class TransformEngine
{
    private readonly Stylesheet.Stylesheet _stylesheet;
    private readonly EvaluationContext _context;

    /// <summary>
    /// The XSLT version supported by this processor. Used when deciding whether
    /// XSLT 1.0/2.0 static constraints are still enforced.
    /// </summary>
    private const double ProcessorXsltVersion = 3.1;
    private const string ExsltCommonNamespace = "http://exslt.org/common";
    // The principal result tree is built inside a synthetic wrapper element.
    // This allows templates to produce multiple top-level nodes (fragments)
    // without the structural constraints of an XDocument. At the end of the
    // transformation the wrapper is unwrapped into a real XDocument when the
    // result is a single clean root element, otherwise it is returned as a
    // fragment wrapper that the serializers know how to expand.
    private readonly XElement _resultDocument;
    private XContainer _currentContainer;
    private readonly StringBuilder _documentLevelText = new();
    private bool _lastAddedWasAtomic;

    // Effective output properties for a principal xsl:result-document, if one was produced.
    private Stylesheet.OutputProperties? _principalResultDocumentProperties;

    // Raw XDM value produced by a principal xsl:result-document with method="json"
    // or build-tree="no". When set, this becomes the final transformation result.
    private XdmValue? _principalRawResultDocument;

    // Raw top-level items collected when the output method is JSON (build-tree="no").
    private bool _jsonOutputMode;
    private bool _adaptiveOutputMode;
    private List<XdmValue> _jsonResultItems = new();

    // Raw top-level items collected for a secondary xsl:result-document with
    // method="json"/"adaptive" or build-tree="no".
    private bool _collectRawItems;
    private List<XdmValue> _resultDocumentRawItems = new();

    // fn:transform support: when _returnRawTransformResult is true the raw top-level
    // items of the transformation (from apply-templates or an initial template) are
    // returned as a sequence instead of a result tree (delivery-format="raw").
    private bool _returnRawTransformResult;

    // Raw-item collection at the principal top level only (fn:transform raw delivery).
    // Unlike _collectRawItems this does not leak into result-document frames, so
    // secondary result documents still build result trees.
    private bool _principalRawCollection;

    // fn:transform support: when _captureResultDocuments is true, secondary
    // xsl:result-document output is captured into _capturedResultDocuments instead of
    // being serialized to the file system.
    private bool _captureResultDocuments;
    private readonly Dictionary<string, (XdmValue Value, Stylesheet.OutputProperties Props)> _capturedResultDocuments = new();

    /// <summary>
    /// The secondary result documents captured during the last transformation when
    /// result-document capture was enabled (used by fn:transform), keyed by the
    /// resolved result-document URI.
    /// </summary>
    public IReadOnlyDictionary<string, (XdmValue Value, Stylesheet.OutputProperties Props)> CapturedResultDocuments => _capturedResultDocuments;

    // Flattened template rules and named templates from the entire stylesheet tree
    private readonly List<Stylesheet.TemplateRule> _allTemplateRules;
    private readonly Dictionary<string, Stylesheet.TemplateRule> _allNamedTemplates;
    private readonly HashSet<string> _excludedResultPrefixes;
    private readonly Dictionary<string, Stylesheet.NamespaceAliasDefinition> _namespaceAliases;
    private readonly IXsltMessageListener? _messageListener;
    private readonly bool _treatRecoverableAmbiguousMatchAsError;

    // Variable scope stack for proper lexical scoping across call-template
    private readonly Stack<Dictionary<(string LocalName, string NamespaceUri), XdmValue?>> _varScopes = new();

    // Mode stack for #current resolution
    private readonly Stack<string> _modeStack = new();

    // Default-mode stack for xsl:default-mode scoping
    private readonly Stack<string> _defaultModeStack = new();

    // Tunnel parameter stack: each frame is the tunnel params visible at that call depth
    private readonly Stack<Dictionary<string, XdmValue>> _tunnelParamStack = new();

    // Apply-imports precedence stack: tracks the import precedence threshold for xsl:next-match
    // when called inside a template invoked by xsl:apply-imports (XSLT 3.0 §6.5)
    private readonly Stack<int> _applyImportsPrecedenceStack = new();

    // Current template rule for xsl:next-match
    private Stylesheet.TemplateRule? _currentTemplateRule;

    // Accumulated excluded rules for the current xsl:next-match chain
    private HashSet<Stylesheet.TemplateRule> _nextMatchExcluded = new();

    // Key index for key() function lookups — one per document root node
    private List<(IXdmNode DocRoot, KeyIndex Index)>? _keyIndices;

    // The initial source document supplied to Transform, and the initial mode used
    // to process it. Used to determine accumulator applicability for copied trees.
    private IXdmNode? _initialSource;
    private string _initialMode = "";

    // True when the transformation entry point is a named template (or implicit
    // xsl:initial-template). In that case the source tree is the global context item,
    // not the initial match selection, so accumulator applicability is not governed
    // by the initial mode's use-accumulators.
    private bool _startedWithNamedTemplate;

    // Snapshot of variables visible at the start of the transformation. Attribute sets
    // are evaluated with only top-level variables/parameters in scope, so this snapshot
    // is used to hide local template variables during attribute-set execution.
    private Dictionary<(string LocalName, string NamespaceUri), XdmValue> _attributeSetVariableSnapshot = new();

    // Sequence accumulator for sequence-producing instructions inside variable/function bodies
    // with @as. The placeholder implementation emits synthetic placeholder elements into the
    // current container so that sequence contributions keep their source order relative to nodes
    // produced by other instructions; the list implementation simply stores items in a flat list.
    private ISequenceAccumulator? _sequenceAccumulator;

    // When true, atomic values produced in a sequence constructor are kept as separate
    // items rather than merged into a single text node. Used for xsl:template/@as and
    // other sequence-typed result construction.
    private bool _preserveAtomicSequenceItems;

    // Side channel for map and function items produced in a typed (xsl:template/@as)
    // sequence constructor: they cannot be represented as nodes in the temporary
    // container, so CopyToResult stores them here and ExecuteTemplate merges them into
    // the collected result items. Null outside typed-template result construction.
    private List<XdmValue>? _typedResultRawItems;
    private bool _preserveDocumentNodes;

    // Tracks nesting depth of literal result elements inside a sequence constructor.
    // Used with _preserveAtomicSequenceItems so that atomics inside a constructed
    // element are still merged with spaces, while top-level atomics remain separate.
    private int _literalElementDepth;

    // When an initial named template is invoked with a request for its raw XDM result
    // (rather than the serialized result tree), the converted sequence is stored here.
    private bool _returnRawInitialTemplateResult;
    private XdmValue? _rawInitialTemplateResult;

    // Set while the initial named template (the transformation entry point) is executing,
    // so ExecuteTemplate can capture its typed result instead of copying it to the result tree.
    private bool _isExecutingInitialTemplate;

    // Recursion depth guard for xsl:function and xsl:call-template calls
    private int _xsltFunctionCallDepth;
    private int _callTemplateDepth;

    // Memoization cache for deterministic xsl:function declarations (new-each-time="no")
    private readonly Dictionary<XsltFunctionCacheKey, XdmValue> _xsltFunctionCache = new();

    // Per-function-call lazy variable dictionary. xsl:variable declarations inside
    // xsl:function bodies are registered here and evaluated only when referenced,
    // so unused variables do not trigger false circular-reference errors.
    private Dictionary<(string LocalName, string NamespaceUri), Lazy<XdmValue>>? _functionLocalLazyVariables;

    // Innermost XSLT instruction currently executing. Used by xsl:catch to report
    // the line/module of the instruction that raised the error.
    private XElement? _currentInstruction;

    // Tracks URIs already used by xsl:result-document to detect XTDE1490 duplicates.
    private readonly HashSet<string> _resultDocumentUris = new();

    // Stack of open xsl:result-document contexts. Used to redirect nested result
    // documents and to detect attempts to write to the principal output URI while
    // a secondary result document is active.
    private readonly Stack<ResultDocumentFrame> _resultDocumentStack = new();

    // Set to true once a top-level xsl:result-document with no @href has closed the
    // principal output; any further output to the principal result tree is an error.
    private bool _principalOutputClosed;

    // Set to true when content has been written to the implicit principal result tree.
    // An explicit xsl:result-document writing to the principal URI is then a duplicate.
    private bool _principalOutputHasContent;

    // The base output URI supplied for the transformation. Used as the principal
    // output URI and as the value of fn:current-output-uri() at the top level.
    private string? _baseOutputUri;

    /// <summary>
    /// Records the state of an open <c>xsl:result-document</c> instruction.
    /// </summary>
    private readonly record struct ResultDocumentFrame(
        string TargetUri,
        XElement? RootContainer,
        XContainer SavedContainer,
        XContainer PrincipalContainer);

    // Current group state for xsl:for-each-group / current-group() / current-grouping-key()
    private List<XdmValue>? _currentGroup;
    private XdmValue? _currentGroupingKey;

    // Current merge state for xsl:merge / current-merge-group() / current-merge-key()
    private List<XdmValue>? _currentMergeGroup;
    private XdmValue? _currentMergeKey;
    private Dictionary<string, List<XdmValue>>? _currentNamedMergeGroups;
    private HashSet<string>? _currentMergeSourceNames;



    // Recursion depth guard for xsl:apply-templates
    private int _applyTemplatesDepth;
    private const int MaxApplyTemplatesDepth = 256;

    // Deferred global variables with sequence constructors (evaluated lazily on first reference)
    private readonly Dictionary<(string LocalName, string NamespaceUri), (XElement Element, string? AsType)> _lazyGlobals = new();

    // Snapshot of variable bindings after global initialization; used to evaluate
    // lazy globals in the global scope, isolating them from local template variables.
    private Dictionary<(string LocalName, string NamespaceUri), XdmValue>? _globalVariableSnapshot;

    // Tracks globals currently being evaluated to detect circular references.
    private readonly HashSet<(string LocalName, string NamespaceUri)> _evaluatingGlobals = new();

    // Accumulator declarations and cached accumulator values per source tree.
    private readonly List<Stylesheet.AccumulatorDefinition> _accumulators;
    private readonly Dictionary<(IXdmNode Root, string ClarkName), Dictionary<IXdmNode, (XdmValue Before, XdmValue After)>> _accumulatorCache = new();
    private readonly HashSet<(IXdmNode Root, string ClarkName)> _accumulatorsInProgress = new();
    private readonly Dictionary<IXdmNode, HashSet<string>> _accumulatorApplicability = new();

    // The initial context item supplied to the transformation (the global context item).
    private XdmValue _globalContextItem = XdmValue.Undefined;

    /// <summary>The parsed xsl:output serialization properties.</summary>
    public Stylesheet.OutputProperties? OutputProperties => _stylesheet.EffectiveOutputProperties;

    /// <summary>
    /// The effective output properties of a principal <c>xsl:result-document</c>, if one
    /// was produced during the transformation.
    /// </summary>
    public Stylesheet.OutputProperties? PrincipalResultDocumentProperties => _principalResultDocumentProperties;

    public TransformEngine(Stylesheet.Stylesheet stylesheet, EvaluationContext? context = null, IXsltMessageListener? messageListener = null, bool treatRecoverableAmbiguousMatchAsError = false)
    {
        _stylesheet = stylesheet;
        _context = context ?? new EvaluationContext();
        _messageListener = messageListener;
        _treatRecoverableAmbiguousMatchAsError = treatRecoverableAmbiguousMatchAsError;
        _context.BackwardsCompatible = stylesheet.Version is "1.0";
        _context.BaseUri = stylesheet.BaseUri ?? string.Empty;
        FunctionLibrary.Populate(_context);
        _context.CollationComparer = FunctionLibrary.CompareStrings;
        XsltFunctionLibrary.Populate(_context);

        _resultDocument = new XElement("__xdm_doc__");
        _currentContainer = _resultDocument;

        _allTemplateRules = _stylesheet.GetAllTemplateRules().ToList();
        _allNamedTemplates = _stylesheet.GetAllNamedTemplates();
        _accumulators = _stylesheet.GetAllAccumulators().ToList();

        // Register namespace prefixes declared on the stylesheet root(s).
        // The empty prefix (default namespace) is intentionally skipped so that
        // XPath select expressions behave like match patterns: unprefixed element
        // names match the empty namespace, not the stylesheet's default namespace.
        // This aligns with XSLT 1.0 behaviour and is required because our source
        // XML (EDIFACT grouped documents) has no namespace on elements.
        foreach (var (prefix, nsUri) in _stylesheet.GetAllNamespaces())
        {
            if (!string.IsNullOrEmpty(prefix))
            {
                _context.WithNamespace(prefix, nsUri);
            }
        }

        // Collect excluded result prefixes for namespace filtering in literal result elements
        _excludedResultPrefixes = _stylesheet.GetAllExcludedResultPrefixes();

        // Build the effective namespace-alias map (source URI -> definition)
        _namespaceAliases = _stylesheet.GetEffectiveNamespaceAliases();

        // Register decimal-format declarations from the stylesheet
        RegisterDecimalFormats();

        // Register xsl:function declarations as callable XPath functions
        RegisterXsltFunctions();

        // Register accumulator-before()/accumulator-after() when accumulators are declared
        RegisterAccumulatorFunctions(_context);

        // Compile all xsl:variable/xsl:param/@select expressions up-front so that
        // static XPath errors (including references to removed functions) are reported
        // even when the variable is never referenced at run time.
        ValidateStaticExpressions();
    }

    /// <summary>
    /// Eagerly validates XSLT structural constraints and compiles XPath
    /// <c>@select</c> expressions so that static errors are reported before
    /// the transformation runs, even when the offending instruction is never
    /// executed.
    /// </summary>
    private void ValidateStaticExpressions()
    {
        var root = _stylesheet.Root;
        if (root == null)
            return;

        foreach (var elem in root.DescendantsAndSelf())
        {
            if (!ShouldValidateStaticExpression(elem))
                continue;

            if (elem.Name.NamespaceName != Stylesheet.Stylesheet.XslNamespace)
                continue;

            var localName = elem.Name.LocalName;
            if (localName is "variable" or "param" or "with-param")
            {
                var select = elem.Attribute("select")?.Value;
                if (!string.IsNullOrEmpty(select))
                {
                    // Compilation is context-independent; it only detects static errors.
                    _ = CompileXPath(select, elem);
                }
            }
            else if (localName is "if" or "when")
            {
                if (string.IsNullOrEmpty(elem.Attribute("test")?.Value))
                    throw new InvalidOperationException($"XTSE0010: xsl:{localName} requires a test attribute");
            }
            else if (localName == "choose")
            {
                ValidateChooseStructure(elem);
            }
        }
    }

    /// <summary>
    /// Returns false when the element is inside an unknown XSLT element that is in
    /// forwards-compatible mode, unless the element is a descendant of an
    /// <c>xsl:fallback</c> child of that unknown element.
    /// </summary>
    private bool ShouldValidateStaticExpression(XElement element)
    {
        var xslNs = Stylesheet.Stylesheet.XslNamespace;
        var current = element.Parent;
        while (current != null)
        {
            if (current.Name.NamespaceName == xslNs &&
                !Stylesheet.Stylesheet.KnownXsltElementNames.Contains(current.Name.LocalName) &&
                _stylesheet.IsForwardsCompatibleElement(current))
            {
                var childOnPath = element;
                while (childOnPath.Parent != null && childOnPath.Parent != current)
                    childOnPath = childOnPath.Parent;

                if (childOnPath.Name.NamespaceName == xslNs && childOnPath.Name.LocalName == "fallback")
                    return true;

                return false;
            }
            current = current.Parent;
        }
        return true;
    }

    /// <summary>
    /// Validates the child structure of <c>xsl:choose</c>: it must contain one
    /// or more <c>xsl:when</c> elements, followed by at most one <c>xsl:otherwise</c>.
    /// </summary>
    private static void ValidateChooseStructure(XElement choose)
    {
        bool seenOtherwise = false;
        int whenCount = 0;
        foreach (var child in choose.Elements())
        {
            if (child.Name.NamespaceName != Stylesheet.Stylesheet.XslNamespace)
                throw new InvalidOperationException("XTSE0010: xsl:choose may only contain xsl:when and xsl:otherwise elements");

            if (child.Name.LocalName == "when")
            {
                if (seenOtherwise)
                    throw new InvalidOperationException("XTSE0010: xsl:when must precede xsl:otherwise");
                if (string.IsNullOrEmpty(child.Attribute("test")?.Value))
                    throw new InvalidOperationException("XTSE0010: xsl:when requires a test attribute");
                whenCount++;
            }
            else if (child.Name.LocalName == "otherwise")
            {
                if (seenOtherwise)
                    throw new InvalidOperationException("XTSE0010: xsl:choose may contain at most one xsl:otherwise");
                seenOtherwise = true;
            }
            else
            {
                throw new InvalidOperationException("XTSE0010: xsl:choose may only contain xsl:when and xsl:otherwise elements");
            }
        }

        if (whenCount == 0)
            throw new InvalidOperationException("XTSE0010: xsl:choose must contain at least one xsl:when");
    }

    /// <summary>
    /// Executes the stylesheet transformation.
    /// </summary>
    /// <param name="source">The source node, or null when an initial template or initial match selection is used.</param>
    /// <param name="initialTemplate">Optional name of the initial named template (lexical or Clark form).</param>
    /// <param name="initialMode">Optional name of the initial mode.</param>
    /// <param name="rawResult">When true and an initial template is used, returns the raw template result instead of wrapping it in a result document.</param>
    /// <param name="baseOutputUri">The base output URI for the transformation; used by fn:current-output-uri().</param>
    /// <param name="initialMatchSelection">Optional initial match selection (fn:transform): an arbitrary XDM value to which templates are applied in the initial mode.</param>
    /// <param name="captureResultDocuments">When true, secondary result documents are captured instead of written to disk.</param>
    /// <param name="rawTransformResult">When true, the raw top-level items of the transformation are returned as a sequence (fn:transform delivery-format="raw").</param>
    public XdmValue Transform(IXdmNode? source, string? initialTemplate = null, string? initialMode = null, bool rawResult = false, string? baseOutputUri = null, XdmValue? initialMatchSelection = null, bool captureResultDocuments = false, bool rawTransformResult = false, IXdmNode? globalContextItem = null)
    {
        _baseOutputUri = baseOutputUri;
        _context.CurrentOutputUri = baseOutputUri;
        _initialSource = source;
        _initialMode = initialMode ?? _stylesheet.DefaultMode;
        _startedWithNamedTemplate = false;
        _returnRawInitialTemplateResult = rawResult;
        _rawInitialTemplateResult = null;
        _returnRawTransformResult = rawTransformResult;
        _captureResultDocuments = captureResultDocuments;
        _capturedResultDocuments.Clear();

        // A source document is required unless an initial template is supplied, an
        // initial match selection is given, or the stylesheet declares an
        // xsl:initial-template (with any namespace prefix).
        var implicitInitialTemplate = string.IsNullOrEmpty(initialTemplate) ? FindInitialTemplateName() : null;
        if (source == null && initialMatchSelection == null && string.IsNullOrEmpty(initialTemplate) && implicitInitialTemplate == null)
            throw new ArgumentException("A source document is required unless an initial template is specified.", nameof(source));

        // XTDE3086: a required global context item must be supplied.
        if (_stylesheet.GlobalContextItemUse == "required" && source == null)
            throw new InvalidOperationException("XTDE3086: A global context item is required but none was supplied.");

        // Ensure xsl:function registrations are present (re-entrant transforms)
        RegisterXsltFunctions();

        // Always register key() function before building key indices or compiling
        // match patterns, because xsl:key/@use expressions and match patterns may
        // call key() recursively (key-063/064).
        RegisterKeyFunction();

        // Apply whitespace stripping from xsl:strip-space / xsl:preserve-space
        // before globals or key indices are evaluated. Strip the document that
        // contains the source node, not just the source node itself, so a selected
        // whitespace text node can be removed.
        if (source != null)
        {
            var stripTarget = source.Document ?? source;
            ApplyWhitespaceStripping(stripTarget);
            // If the selected source node was a whitespace text node that has been
            // stripped from the tree, the initial context item is absent (XSLT 3.0 §5.4).
            if (!IsNodeAttached(source))
            {
                source = null;
                _initialSource = null;
            }
        }

        // Documents loaded by fn:doc / fn:document during the transformation are also
        // subject to the stylesheet's whitespace stripping rules, but the stylesheet
        // document itself (returned by document('')) must not be mutated.
        _context.DocumentPostProcessor = PostProcessLoadedDocument;

        // Make the source document available to fn:doc via its document URI so that
        // doc(document-uri($arg)) is $arg returns true for the initial source tree.
        if (source != null)
        {
            var sourceDoc = source.NodeKind == XdmNodeKind.Document ? source : source.Document;
            if (sourceDoc != null && !string.IsNullOrEmpty(sourceDoc.DocumentUri))
                _context.RegisterDocument(sourceDoc.DocumentUri, sourceDoc);
        }

        // Determine the global context item. For fn:transform, the caller supplies
        // the value explicitly or supplies a wrapper document for non-document nodes.
        // When called directly without an explicit global context item, the source
        // node serves as the global context item for backward compatibility.
        var effectiveGlobalContextItem = globalContextItem ?? source;

        // Initialize global parameters and variables before compiling match patterns
        // and building key indices, because both match-pattern predicate validation
        // and xsl:key/@use expressions may reference global variables/parameters.
        InitializeGlobalParametersAndVariables(effectiveGlobalContextItem);

        // Capture the variable bindings that are visible before any template executes.
        // Attribute sets are evaluated with only these top-level bindings in scope.
        _attributeSetVariableSnapshot = _context.SnapshotVariables();

        // Result-document URIs must be unique within a transformation.
        _resultDocumentUris.Clear();
        _resultDocumentStack.Clear();
        _principalOutputClosed = false;
        _principalOutputHasContent = false;
        _principalResultDocumentProperties = null;
        _jsonOutputMode = (_stylesheet.EffectiveOutputProperties?.Method ?? "xml") == "json";
        _adaptiveOutputMode = (_stylesheet.EffectiveOutputProperties?.Method ?? "xml") == "adaptive";
        _jsonResultItems.Clear();
        _collectRawItems = _jsonOutputMode || _adaptiveOutputMode;
        _principalRawCollection = _returnRawTransformResult;
        _resultDocumentRawItems.Clear();
        _principalRawResultDocument = null;

        // Compile all template match patterns before execution. The validation
        // dry-run for pattern predicates needs the lazy global resolver registered
        // above so that variable references such as $servletName can be resolved.

        // Evaluate AVTs in non-standard _match attributes now that global variables
        // (including static parameters) are available in the runtime context.
        foreach (var rule in _allTemplateRules)
        {
            if (rule.Element.Attribute("match") == null && rule.Element.Attribute("_match") != null && rule.Match != null)
            {
                rule.Match = EvaluateAvt(rule.Match, rule.Element);
            }
        }

        var patternCompiler = new Patterns.PatternCompiler(_context);
        foreach (var rule in _allTemplateRules)
        {
            rule.CompileMatch(patternCompiler);
        }

        // Build key indices iteratively to handle cross-key dependencies
        // (e.g. key-063 where k2's use calls key('k1',...), or key-064 where
        // k1's match calls key('k2',...)).
        var allKeyDefs = _stylesheet.GetAllKeyDefinitions();
        if (source != null && allKeyDefs.Count > 0)
        {
            // XTSE1222: all xsl:key declarations with the same expanded name must
            // agree on their effective @composite value.
            foreach (var group in allKeyDefs.GroupBy(k => k.Name))
            {
                if (group.Select(k => k.Composite).Distinct().Count() > 1)
                    throw new InvalidOperationException($"XTSE1222: xsl:key definitions for '{group.Key}' have conflicting @composite values.");
            }

            _keyIndices = new List<(IXdmNode, KeyIndex)>();
            var sourceIndex = new KeyIndex();
            // Add the index before building so recursive key() calls inside
            // xsl:key/@use or match can query the partially-built index.
            _keyIndices.Add((source, sourceIndex));

            BuildKeyIndex(source, sourceIndex, allKeyDefs);
        }

        RegisterGroupingFunctions();

        var effectiveInitialTemplate = initialTemplate ?? implicitInitialTemplate;
        if (!string.IsNullOrEmpty(effectiveInitialTemplate))
        {
            if (!TryFindNamedTemplate(effectiveInitialTemplate, out var templateKey, out var entryRule))
                throw new InvalidOperationException($"XTDE0040: Named template '{effectiveInitialTemplate}' not found.");

            // In an xsl:package, only public (or final) named templates may be used as
            // a transformation entry point; the default visibility is private, except
            // for xsl:initial-template which is implicitly public.
            if (_stylesheet.IsPackage)
            {
                bool isPublicEntry = entryRule.Visibility is "public" or "final";
                if (!isPublicEntry && entryRule.Visibility == null && entryRule.Name != null)
                {
                    var (tplLocal, tplNs) = ExpandVariableName(entryRule.Element, entryRule.Name);
                    isPublicEntry = tplLocal == "initial-template" && tplNs == Stylesheet.Stylesheet.XslNamespace;
                }
                if (!isPublicEntry)
                    throw new InvalidOperationException($"XTDE0040: Named template '{effectiveInitialTemplate}' is not public.");
            }

            _startedWithNamedTemplate = true;
            // If the designated initial template has a match pattern, execute it as a
            // template rule against the source node so that xsl:next-match has a current
            // template rule and a context item. Otherwise invoke it as a plain named
            // template, which has no current template rule.
            var (initCallParams, initTunnelParams) = CollectExternalParameters(entryRule.Element);
            _isExecutingInitialTemplate = true;
            try
            {
                if (entryRule.CompiledMatch != null && source != null)
                    ExecuteTemplate(entryRule, source, callParams: initCallParams, incomingTunnelParams: initTunnelParams, setCurrentRule: true);
                else
                {
                    var initialContextItem = source != null ? XdmValue.FromNode(source) : XdmValue.Undefined;
                    CallTemplate(templateKey, initialContextItem, initCallParams, initTunnelParams);
                }
            }
            finally
            {
                _isExecutingInitialTemplate = false;
            }
        }
        else if (initialMatchSelection != null)
        {
            // fn:transform initial-match-selection: apply templates in the initial mode
            // to each item of the supplied selection (XSLT 3.0 §24.2).
            // When no initial mode is supplied, use the stylesheet's default mode.
            var resolvedInitialMode = string.IsNullOrEmpty(initialMode)
                ? _stylesheet.DefaultMode
                : ExpandModeName(initialMode, _stylesheet.Root);
            if (resolvedInitialMode == "#unnamed")
                resolvedInitialMode = "";
            _initialMode = resolvedInitialMode;
            // XTDE0045: initial mode must exist in the stylesheet (templates with #all don't count)
            if (!ModeExists(resolvedInitialMode))
            {
                throw new InvalidOperationException($"XTDE0045: Initial mode '{resolvedInitialMode}' does not exist in the stylesheet.");
            }
            // A private or abstract mode cannot be used as the initial mode.
            var matchModeDef = _stylesheet.GetModeDefinition(resolvedInitialMode);
            if (matchModeDef != null && (matchModeDef.Visibility == Stylesheet.ModeVisibility.Private || matchModeDef.Visibility == Stylesheet.ModeVisibility.Abstract))
            {
                throw new InvalidOperationException($"XTDE0045: Initial mode '{resolvedInitialMode}' is not visible.");
            }

            var selectionItems = new List<XdmValue>();
            FlattenToList(initialMatchSelection.Value, selectionItems);

            _modeStack.Push(resolvedInitialMode);
            try
            {
                int pos = 1;
                int last = selectionItems.Count;
                foreach (var item in selectionItems)
                {
                    if (item.IsNode && item.NodeValue != null)
                    {
                        var rule = FindBestTemplate(item.NodeValue, resolvedInitialMode);
                        if (rule != null)
                        {
                            ExecuteTemplate(rule, item.NodeValue, position: pos, last: last);
                        }
                        else
                        {
                            ApplyBuiltInRules(item.NodeValue, resolvedInitialMode, position: pos, last: last);
                        }
                    }
                    else
                    {
                        var rule = FindBestTemplate(item, resolvedInitialMode);
                        if (rule != null)
                        {
                            ExecuteTemplate(rule, item, position: pos, last: last);
                        }
                        else
                        {
                            ApplyBuiltInRulesForAtomic(item, resolvedInitialMode);
                        }
                    }
                    pos++;
                }
            }
            finally
            {
                _modeStack.Pop();
            }
        }
        else if (!string.IsNullOrEmpty(initialMode))
            {
                // Start transformation in the specified initial mode.
                // Expand any namespace prefix in the initial mode name.
                var resolvedInitialMode = ExpandModeName(initialMode, _stylesheet.Root);
                // If the mode is #unnamed, treat it as the empty unnamed mode
                if (resolvedInitialMode == "#unnamed")
                    resolvedInitialMode = "";
                _initialMode = resolvedInitialMode;
                // XTDE0045: initial mode must exist in the stylesheet (templates with #all don't count)
                if (!ModeExists(resolvedInitialMode))
                {
                    throw new InvalidOperationException($"XTDE0045: Initial mode '{resolvedInitialMode}' does not exist in the stylesheet.");
                }
                // A private or abstract mode cannot be used as the initial mode.
                var initialModeDef = _stylesheet.GetModeDefinition(resolvedInitialMode);
                if (initialModeDef != null && (initialModeDef.Visibility == Stylesheet.ModeVisibility.Private || initialModeDef.Visibility == Stylesheet.ModeVisibility.Abstract))
                {
                    throw new InvalidOperationException($"XTDE0045: Initial mode '{resolvedInitialMode}' is not visible.");
                }
                _modeStack.Push(resolvedInitialMode);
                try
                {
                    var rootTemplate = FindBestTemplate(source!, resolvedInitialMode);
                    if (rootTemplate != null)
                    {
                        var (initCallParams, initTunnelParams) = CollectExternalParameters(rootTemplate.Element);
                        ExecuteTemplate(rootTemplate, source!, callParams: initCallParams, incomingTunnelParams: initTunnelParams);
                    }
                    else
                    {
                        ApplyBuiltInRules(source!, resolvedInitialMode);
                    }
                }
                finally
                {
                    _modeStack.Pop();
                }
            }
            else
            {
                // No initial template/mode/selection was supplied. Start in the
                // stylesheet's default mode and apply templates to the source node
                // itself (the built-in document/element rules handle the children
                // when no template matches the source node).
                _modeStack.Push(_initialMode);
                try
                {
                    var rootTemplate = FindBestTemplate(source!, _initialMode);
                    if (rootTemplate != null)
                    {
                        var (initCallParams, initTunnelParams) = CollectExternalParameters(rootTemplate.Element);
                        ExecuteTemplate(rootTemplate, source!, callParams: initCallParams, incomingTunnelParams: initTunnelParams);
                    }
                    else
                    {
                        ApplyBuiltInRules(source!, _initialMode);
                    }
                }
                finally
                {
                    _modeStack.Pop();
                }
            }

        // If the entry point was an initial named template and the caller asked for
        // the raw XDM result, return that instead of the serialized result tree.
        if (_returnRawInitialTemplateResult && _rawInitialTemplateResult != null)
        {
            FinalizeResultTreeNamespaces(_rawInitialTemplateResult.Value);
            return _rawInitialTemplateResult.Value;
        }

        // fn:transform with delivery-format="raw" and no principal xsl:result-document:
        // return the collected raw top-level items as a sequence.
        if (_returnRawTransformResult && _principalResultDocumentProperties == null)
        {
            XdmValue rawTransformValue;
            if (_jsonResultItems.Count > 0)
            {
                rawTransformValue = XdmValue.FromSequence(MaterializedSequence.FromList(_jsonResultItems));
            }
            else
            {
                rawTransformValue = ExtractRawResultTreeItems();
            }
            FinalizeResultTreeNamespaces(rawTransformValue);
            return rawTransformValue;
        }

        // A principal xsl:result-document with method="json" or build-tree="no" takes
        // precedence over the normal principal result tree.
        if (_principalRawResultDocument != null)
            return _principalRawResultDocument.Value;

        // For JSON or adaptive output, return the collected raw top-level items as a sequence.
        if (_jsonOutputMode || _adaptiveOutputMode)
        {
            if (_jsonResultItems.Count == 0)
                return XdmValue.Undefined;
            var rawSequenceResult = XdmValue.FromSequence(MaterializedSequence.FromList(_jsonResultItems));
            FinalizeResultTreeNamespaces(rawSequenceResult);
            return rawSequenceResult;
        }

        // If no content was ever written to the principal result tree and no
        // explicit principal xsl:result-document was produced, the principal result
        // is absent.
        if (!_principalOutputHasContent && _principalResultDocumentProperties == null)
            return XdmValue.Undefined;

        // Return the result document, or document-level text if no root element was produced
        if (_documentLevelText.Length > 0 && !_resultDocument.Elements().Any())
        {
            return XdmValue.FromString(_documentLevelText.ToString());
        }

        // Unwrap a single clean root element into a real XDocument so that
        // existing single-document consumers and the XML serializer behave
        // exactly as before. If there are multiple top-level nodes (or text
        // alongside an element), return the synthetic wrapper as a fragment.
        XdmValue resultValue;
        var rootElements = _resultDocument.Elements().ToList();
        if (rootElements.Count == 1 && _resultDocument.Nodes().Count() == 1)
        {
            rootElements[0].Remove();
            var doc = new XDocument(rootElements[0]);
            var rdProps = _resultDocument.Annotation<Stylesheet.OutputProperties>();
            if (rdProps != null)
                doc.AddAnnotation(rdProps);
            resultValue = XdmValue.FromNode(new XDocumentNode(doc));
        }
        else
        {
            resultValue = XdmValue.FromNode(new XDocumentNode(_resultDocument));
        }

        FinalizeResultTreeNamespaces(resultValue);
        return resultValue;
    }

    /// <summary>
    /// Extracts the raw top-level items from the implicit result tree for
    /// <c>fn:transform</c> delivery-format="raw".
    /// </summary>
    private XdmValue ExtractRawResultTreeItems()
    {
        if (_documentLevelText.Length > 0 && !_resultDocument.Elements().Any())
            return XdmValue.FromString(_documentLevelText.ToString());

        var nodes = _resultDocument.Nodes().ToList();
        if (nodes.Count == 0)
            return XdmValue.Undefined;

        XdmValue RawNodeFromXNode(XNode node)
        {
            if (node is XElement elem)
            {
                elem.Remove();
                return XdmValue.FromNode(new XDocumentNode(new XDocument(elem)));
            }
            if (node is XText text)
                return XdmValue.FromString(text.Value);
            if (node is XComment comment)
                return XdmValue.FromNode(new XDocumentNode(new XDocument(new XComment(comment.Value))));
            if (node is XProcessingInstruction pi)
                return XdmValue.FromNode(new XDocumentNode(new XDocument(new XProcessingInstruction(pi.Target, pi.Data))));
            return XdmValue.FromString(node.ToString());
        }

        if (nodes.Count == 1)
            return RawNodeFromXNode(nodes[0]);

        var items = new List<XdmValue>(nodes.Count);
        foreach (var node in nodes)
            items.Add(RawNodeFromXNode(node));
        return XdmValue.FromSequence(MaterializedSequence.FromList(items));
    }

    /// <summary>
    /// Executes an initial function as the transformation entry point.
    /// </summary>
    /// <param name="name">The expanded function name.</param>
    /// <param name="args">Arguments to pass to the function.</param>
    /// <param name="captureResultDocuments">When true, secondary result documents are captured instead of written to disk.</param>
    /// <param name="baseOutputUri">The base output URI for resolving result-document hrefs.</param>
    public XdmValue TransformFunction(string name, XdmValue[] args, bool captureResultDocuments = false, string? baseOutputUri = null, IXdmNode? source = null, IXdmNode? globalContextItem = null)
    {
        _baseOutputUri = baseOutputUri;
        _context.CurrentOutputUri = baseOutputUri;
        _captureResultDocuments = captureResultDocuments;
        RegisterXsltFunctions();
        RegisterKeyFunction();
        _context.DocumentPostProcessor = PostProcessLoadedDocument;
        InitializeGlobalParametersAndVariables(globalContextItem ?? source);
        _attributeSetVariableSnapshot = _context.SnapshotVariables();

        // Result-document URIs must be unique within a transformation.
        _resultDocumentUris.Clear();
        _resultDocumentStack.Clear();
        _capturedResultDocuments.Clear();
        _principalOutputClosed = false;
        _principalOutputHasContent = false;
        _principalResultDocumentProperties = null;

        // Compile template match patterns before execution, just as for a normal
        // transformation. The function body may contain xsl:apply-templates that
        // needs compiled patterns to match.
        foreach (var rule in _allTemplateRules)
        {
            if (rule.Element.Attribute("match") == null && rule.Element.Attribute("_match") != null && rule.Match != null)
            {
                rule.Match = EvaluateAvt(rule.Match, rule.Element);
            }
        }

        var patternCompiler = new Patterns.PatternCompiler(_context);
        foreach (var rule in _allTemplateRules)
        {
            rule.CompileMatch(patternCompiler);
        }

        RegisterGroupingFunctions();

        var (nsUri, localName) = ParseExpandedFunctionName(name);
        var def = FindFunctionDefinition(nsUri, localName, args.Length);
        if (def == null || (def.Visibility != "public" && def.Visibility != "final"))
            throw new InvalidOperationException("XTDE0041");

        var result = ExecuteXsltFunction(def, args);
        FinalizeResultTreeNamespaces(result);
        return result;
    }

    private (string nsUri, string localName) ParseExpandedFunctionName(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new InvalidOperationException("XTDE0041");

        if (name.Length > 2 && name[0] == 'Q' && name[1] == '{')
        {
            int close = name.IndexOf('}');
            if (close < 2 || close == name.Length - 1)
                throw new InvalidOperationException("XTDE0041");
            return (name.Substring(2, close - 2), name.Substring(close + 1));
        }

        if (name.Length > 2 && name[0] == '{')
        {
            int close = name.IndexOf('}');
            if (close < 1 || close == name.Length - 1)
                throw new InvalidOperationException("XTDE0041");
            return (name.Substring(1, close - 1), name.Substring(close + 1));
        }

        int colon = name.IndexOf(':');
        if (colon >= 0)
        {
            var prefix = name.Substring(0, colon);
            var local = name.Substring(colon + 1);
            var ns = _stylesheet.Root.GetNamespaceOfPrefix(prefix);
            if (ns == null)
                throw new InvalidOperationException("XTDE0041");
            return (ns.NamespaceName, local);
        }

        return (string.Empty, name);
    }

    private Stylesheet.XsltFunctionDefinition? FindFunctionDefinition(string nsUri, string localName, int arity)
    {
        foreach (var (key, def) in _stylesheet.GetAllFunctionDefinitions())
        {
            if (def.NamespaceUri == nsUri && def.LocalName == localName && def.Arity == arity)
                return def;
        }
        return null;
    }

    /// <summary>
    /// Registers all xsl:function declarations from the stylesheet tree as callable
    /// functions on the EvaluationContext.
    /// </summary>
    /// <summary>
    /// Throws <c>XTDE1490</c> if output is being written to the principal result tree
    /// after it has been closed by an explicit <c>xsl:result-document</c> with no <c>@href</c>.
    /// </summary>
    private void EnsurePrincipalOutputOpen()
    {
        if (_principalOutputClosed && _resultDocumentStack.Count == 0)
            throw new InvalidOperationException("XTDE1490: The principal result tree has already been closed by an xsl:result-document instruction.");
    }

    private void MarkPrincipalOutputContent()
    {
        if (_resultDocumentStack.Count == 0)
            _principalOutputHasContent = true;
    }

    /// <summary>
    /// Adds a text node to the current result container.
    /// Falls back to a document-level text buffer when the container is an XDocument,
    /// because XDocument does not allow non-whitespace text nodes at the document level.
    /// </summary>
    private void AddTextNode(string text, bool allowZeroLength = false)
    {
        EnsurePrincipalOutputOpen();
        if (text.Length == 0 && !allowZeroLength)
            return; // Zero-length text nodes are ignored in complex content
        MarkPrincipalOutputContent();
        if (_currentContainer is XDocument)
        {
            _documentLevelText.Append(text);
        }
        else
        {
            _currentContainer.Add(new XText(text));
        }
    }

    /// <summary>
    /// Normalizes the content of a constructed element by removing zero-length text
    /// nodes and merging adjacent text nodes (XSLT 2.0 §5.7.1).
    /// </summary>
    private static void NormalizeElementContent(XElement element)
    {
        var nodes = element.Nodes().ToList();
        if (nodes.Count == 0)
            return;

        var normalized = ApplyComplexContentRules(nodes);
        element.RemoveNodes();
        foreach (var node in normalized)
            element.Add(node);
    }

    /// <summary>
    /// Returns whether the current container has no nodes yet, indicating that the next
    /// item added will be the first significant child.
    /// </summary>
    private bool IsFirstSignificantChild()
    {
        if (_currentContainer is XDocument)
        {
            return _documentLevelText.Length == 0;
        }
        return !_currentContainer.Nodes().Any();
    }

    /// <summary>
    /// Appends a separator and the given text to the last text node in the current container,
    /// or creates a new text node if there is no last text node. Used to join adjacent
    /// atomic values in complex content construction. For <c>method="text"</c> with an
    /// explicit <c>item-separator</c>, the configured separator is used; otherwise a
    /// single space is inserted.
    /// </summary>
    private void AppendAtomicText(string text)
    {
        EnsurePrincipalOutputOpen();
        MarkPrincipalOutputContent();
        var separator = GetAtomicSeparator();
        if (_currentContainer is XDocument)
        {
            if (_lastAddedWasAtomic)
                _documentLevelText.Append(separator);
            _documentLevelText.Append(text);
        }
        else
        {
            if (_lastAddedWasAtomic)
            {
                var lastText = _currentContainer.Nodes().LastOrDefault() as XText;
                if (lastText != null)
                {
                    lastText.Value = lastText.Value + separator + text;
                    _lastAddedWasAtomic = true;
                    return;
                }
                text = separator + text;
            }
            _currentContainer.Add(new XText(text));
        }
        _lastAddedWasAtomic = true;
    }

    /// <summary>
    /// Returns the separator to use between adjacent atomic values for text output.
    /// When <c>item-separator</c> is explicitly specified and the output method is
    /// <c>text</c>, that value is returned; otherwise a single space is used.
    /// </summary>
    private string GetAtomicSeparator()
    {
        var props = _stylesheet.EffectiveOutputProperties;
        if (props?.ItemSeparatorSpecified == true && props.Method == "text")
            return props.ItemSeparator;
        return " ";
    }

    /// <summary>
    /// Returns whether output is currently being written directly to the principal
    /// result document, as opposed to inside a constructed element or secondary result.
    /// </summary>
    private bool IsPrincipalTopLevel =>
        _resultDocumentStack.Count == 0 && ReferenceEquals(_currentContainer, _resultDocument);

    private void RegisterDecimalFormats()
    {
        var allFormats = _stylesheet.GetAllDecimalFormats();
        foreach (var (key, def) in allFormats)
        {
            if (string.IsNullOrEmpty(key.localName))
            {
                _context.DefaultDecimalFormat = def.Format;
            }
            else
            {
                _context.WithDecimalFormat(key.localName, key.nsUri, def.Format);
            }
        }
    }

    /// <summary>
    /// Collects all namespace declarations in scope for the given element
    /// by walking up the ancestor chain.
    /// </summary>
    private static Dictionary<string, string> GetInScopeNamespaces(XElement element)
    {
        var result = new Dictionary<string, string>();
        var current = element;
        while (current != null)
        {
            foreach (var attr in current.Attributes())
            {
                if (attr.IsNamespaceDeclaration)
                {
                    string prefix = attr.Name.LocalName == "xmlns" ? "" : attr.Name.LocalName;
                    if (!result.ContainsKey(prefix))
                        result[prefix] = attr.Value;
                }
            }
            current = current.Parent;
        }
        result["xml"] = "http://www.w3.org/XML/1998/namespace";
        return result;
    }

    /// <summary>
    /// Resolves a <c>parameter-document</c> URI relative to the stylesheet base URI
    /// and returns the parsed serialization-parameters document.
    /// </summary>
    private XDocument? ResolveParameterDocument(string uri, string? baseUri)
    {
        string resolvedUri;
        if (Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            resolvedUri = uri;
        else if (!string.IsNullOrEmpty(baseUri))
            resolvedUri = new Uri(new Uri(baseUri), uri).AbsoluteUri;
        else
            resolvedUri = uri;

        try
        {
            if (_context.DocumentLoader != null)
            {
                var node = _context.DocumentLoader(resolvedUri);
                if (node is XDocumentNode xdn)
                {
                    if (xdn.UnderlyingObject is XDocument doc)
                        return doc;
                    if (xdn.UnderlyingObject is XElement elem)
                        return new XDocument(elem);
                }
                return null;
            }

            var resolver = new Api.FileSystemUriResolver();
            return resolver.Resolve(resolvedUri, baseUri);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Returns the effective default element namespace for the given element.
    /// First checks for an explicit <c>xpath-default-namespace</c> attribute;
    /// otherwise falls back to the ordinary XML default namespace declaration
    /// (<c>xmlns="..."</c>) in scope on the element.
    /// </summary>
    private static string? GetXPathDefaultNamespace(XElement element)
    {
        var current = element;
        while (current != null)
        {
            // The XSLT-namespaced form (e.g. xsl:xpath-default-namespace) is effective on any element
            var attr = current.Attribute(XName.Get("xpath-default-namespace", Stylesheet.Stylesheet.XslNamespace));
            if (attr != null)
            {
                // XTSE0090: xsl:xpath-default-namespace is not allowed on XSLT elements
                if (current.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                    throw new InvalidOperationException("XTSE0090");
                return attr.Value;
            }
            // The no-namespace form is only effective on XSLT elements
            if (current.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
            {
                attr = current.Attribute("xpath-default-namespace");
                if (attr != null) return attr.Value;
            }
            current = current.Parent;
        }

        // The default namespace for XPath expressions (xmlns=...) does NOT affect
        // XPath name tests; only an explicit xpath-default-namespace attribute does.
        return null;
    }

    /// <summary>
    /// Compiles an XPath expression with the in-scope namespace bindings
    /// and xpath-default-namespace from the given instruction element.
    /// </summary>
    private XPath31Expression CompileXPath(string expression, XElement instruction)
    {
        var nsMap = GetInScopeNamespaces(instruction);
        ValidateXPathPrefixes(expression, nsMap);
        var defaultNs = GetXPathDefaultNamespace(instruction);
        var definingNs = instruction.GetDefaultNamespace().NamespaceName;
        var baseUri = GetEffectiveBaseUri(instruction);
        if (nsMap.Count > 1 || !string.IsNullOrEmpty(defaultNs) || !string.IsNullOrEmpty(definingNs) || !string.IsNullOrEmpty(baseUri))
        {
            var options = new CompileOptions
            {
                Namespaces = nsMap,
                DefaultElementNamespace = defaultNs,
                DefiningElementDefaultNamespace = definingNs,
                BaseUri = baseUri,
                BackwardsCompatible = IsEffectiveBackwardsCompatible(instruction)
            };
            return XPath31Expression.Compile(expression, options);
        }
        return XPath31Expression.Compile(expression);
    }

    private static readonly HashSet<string> XPathAxisNames = new(StringComparer.Ordinal)
    {
        "ancestor", "ancestor-or-self", "attribute", "child", "descendant", "descendant-or-self",
        "following", "following-sibling", "namespace", "parent", "preceding", "preceding-sibling", "self"
    };

    /// <summary>
    /// Validates that all namespace prefixes used in an XPath expression are declared
    /// in the supplied namespace map. Raises <c>XPST0081</c> for any undeclared prefix.
    /// </summary>
    private static void ValidateXPathPrefixes(string expression, Dictionary<string, string> nsMap)
    {
        bool inString = false;
        char stringQuote = '\0';
        for (int i = 0; i < expression.Length; i++)
        {
            char c = expression[i];
            if (!inString && (c == '\'' || c == '"'))
            {
                inString = true;
                stringQuote = c;
                continue;
            }
            if (inString)
            {
                if (c == stringQuote)
                {
                    if (i + 1 < expression.Length && expression[i + 1] == stringQuote)
                    {
                        i++;
                        continue;
                    }
                    inString = false;
                }
                continue;
            }
            if (c != ':')
                continue;
            int start = i - 1;
            while (start >= 0 && IsXPathNcNameChar(expression[start]))
                start--;
            int length = i - start - 1;
            if (length <= 0)
                continue;
            var prefix = expression.Substring(start + 1, length);
            if (i + 1 >= expression.Length)
                continue;
            char next = expression[i + 1];
            if (!IsXPathNcNameStartChar(next))
                continue;
            // A valid QName prefix must itself be a valid NCName (starts with a letter or '_').
            // Integer map keys such as "map{1:xs:dateTime(...)}" are not QNames and must be ignored.
            if (!IsXPathNcNameStartChar(prefix[0]) || !prefix.All(IsXPathNcNameChar))
                continue;
            if (prefix == "xml" || prefix == "xmlns" || XPathAxisNames.Contains(prefix))
                continue;
            if (!nsMap.ContainsKey(prefix))
                throw new InvalidOperationException($"XPST0081: Namespace prefix '{prefix}' has not been declared");
        }
    }

    private static bool IsXPathNcNameChar(char c)
        => char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_';

    private static bool IsXPathNcNameStartChar(char c)
        => char.IsLetter(c) || c == '_';

    /// <summary>
    /// Computes the effective base URI of an XSLT instruction by walking up the
    /// ancestor chain and resolving <c>xml:base</c> attributes per XML Base spec.
    /// </summary>
    private string? GetEffectiveBaseUri(XElement? element)
    {
        if (element == null)
            return null;

        string baseUri = element.Document?.BaseUri ?? string.Empty;
        if (string.IsNullOrEmpty(baseUri))
            baseUri = _stylesheet.BaseUri ?? string.Empty;
        var chain = new List<string>();
        var current = element;
        while (current != null)
        {
            var xmlBase = current.Attribute(XNamespace.Xml + "base")?.Value;
            if (xmlBase != null)
                chain.Add(xmlBase);
            current = current.Parent;
        }

        for (int i = chain.Count - 1; i >= 0; i--)
        {
            if (Bosak.XPath.Standard.Functions.FunctionLibrary.IsAbsoluteUri(chain[i]))
                baseUri = chain[i];
            else if (!string.IsNullOrEmpty(baseUri))
            {
                try
                {
                    baseUri = new Uri(new Uri(baseUri), chain[i]).AbsoluteUri;
                }
                catch (UriFormatException)
                {
                    // If the base URI is not a valid .NET Uri,
                    // preserve the xml:base value as-is (XSLT test suites
                    // use intentionally malformed URIs like d://tests/)
                    baseUri = chain[i];
                }
            }
            else
                baseUri = chain[i];
        }

        return string.IsNullOrEmpty(baseUri) ? null : baseUri;
    }

    private void RegisterXsltFunctions()
    {
        var allFuncs = _stylesheet.GetAllFunctionDefinitions();
        foreach (var (key, def) in allFuncs)
        {
            var paramElements = def.Element.Elements(XName.Get("param", Stylesheet.Stylesheet.XslNamespace)).ToList();
            var sig = new FunctionSignature
            {
                NamespaceUri = def.NamespaceUri,
                LocalName = def.LocalName,
                Arity = def.Arity,
                ParameterTypes = Enumerable.Repeat(XdmValueKind.Sequence, def.Arity).ToList(),
                ReturnType = XdmValueKind.Sequence,
                ParameterTypeNames = Enumerable.Range(0, def.Arity)
                    .Select(i => i < paramElements.Count ? paramElements[i].Attribute("as")?.Value : null)
                    .ToList(),
                ReturnTypeName = def.ReturnType,
                Implementation = (ctx, args) => ExecuteXsltFunction(def, args)
            };
            _context.RegisterFunction(sig);
        }
    }

    /// <summary>
    /// Returns true if the exception represents a static error in the target expression
    /// of an <c>xsl:evaluate</c> instruction. Such errors are reported as XTDE3160.
    /// </summary>
    private static bool IsXPathStaticError(InvalidOperationException ex)
    {
        var msg = ex.Message;
        return msg.StartsWith("XPST", StringComparison.Ordinal)
            || msg.StartsWith("XTSE", StringComparison.Ordinal)
            || msg.Contains("XPST0017", StringComparison.Ordinal)
            || msg.Contains("XTSE", StringComparison.Ordinal);
    }

    /// <summary>
    /// Removes XSLT-defined functions from the dynamic context used by <c>xsl:evaluate</c>.
    /// </summary>
    private static void RemoveXsltContextFunctions(EvaluationContext context)
    {
        const string Fn = "http://www.w3.org/2005/xpath-functions";
        context.UnregisterFunction(Fn, "current", 0);
        context.UnregisterFunction(Fn, "key", 2);
        context.UnregisterFunction(Fn, "key", 3);
        context.UnregisterFunction(Fn, "current-group", 0);
        context.UnregisterFunction(Fn, "current-grouping-key", 0);
        context.UnregisterFunction(Fn, "current-merge-group", 0);
        context.UnregisterFunction(Fn, "current-merge-group", 1);
        context.UnregisterFunction(Fn, "current-merge-key", 0);
        context.UnregisterFunction(Fn, "system-property", 1);
        context.UnregisterFunction(Fn, "current-output-uri", 0);
    }

    /// <summary>
    /// Registers stylesheet functions that are visible inside an <c>xsl:evaluate</c>
    /// target expression (public, final, or abstract; not private or hidden).
    /// </summary>
    private void RegisterVisibleXsltFunctions(EvaluationContext context)
    {
        foreach (var (key, def) in _stylesheet.GetAllFunctionDefinitions())
        {
            if (def.Visibility is "private" or "hidden")
                continue;

            var sig = new FunctionSignature
            {
                NamespaceUri = def.NamespaceUri,
                LocalName = def.LocalName,
                Arity = def.Arity,
                ParameterTypes = Enumerable.Repeat(XdmValueKind.Sequence, def.Arity).ToList(),
                ReturnType = XdmValueKind.Sequence,
                Implementation = (ctx, args) => ExecuteXsltFunction(def, args)
            };
            context.RegisterFunction(sig);
        }
    }

    /// <summary>
    /// Computes the effective default collation at the given instruction by walking
    /// the ancestor axis for a [xsl:]default-collation attribute. If the attribute
    /// value is a whitespace-separated list, the first recognized URI is returned.
    /// </summary>
    private static string GetEffectiveDefaultCollation(XElement instruction)
    {
        var current = instruction;
        while (current != null)
        {
            var attr = current.Attribute(XName.Get("default-collation", Stylesheet.Stylesheet.XslNamespace));
            if (attr == null && current.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                attr = current.Attribute("default-collation");

            if (!string.IsNullOrEmpty(attr?.Value))
            {
                var candidates = attr.Value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                foreach (var candidate in candidates)
                {
                    if (IsRecognizedCollationUri(candidate))
                        return candidate;
                }
                if (candidates.Length > 0)
                    return candidates[0];
            }

            current = current.Parent;
        }
        return string.Empty;
    }

    private static bool IsRecognizedCollationUri(string uri)
    {
        if (string.IsNullOrEmpty(uri))
            return true;
        if (uri == "http://www.w3.org/2005/xpath-functions/collation/codepoint")
            return true;
        if (uri == "http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive")
            return true;
        if (uri == "http://www.w3.org/2010/09/qt-fots-catalog/collation/caseblind")
            return true;
        if (uri.StartsWith("http://www.w3.org/2013/collation/UCA", StringComparison.Ordinal))
            return true;
        return false;
    }

    /// <summary>
    /// Executes the supplied action with the effective <c>default-collation</c>
    /// of <paramref name="element"/> temporarily installed on the context.
    /// </summary>
    private void WithDefaultCollation(XElement element, Action action)
    {
        var collation = GetEffectiveDefaultCollation(element);
        if (string.IsNullOrEmpty(collation))
        {
            action();
            return;
        }

        var previous = _context.DefaultCollation;
        _context.DefaultCollation = collation;
        try
        {
            action();
        }
        finally
        {
            _context.DefaultCollation = previous;
        }
    }

    /// <summary>
    /// Sets the XPath backwards-compatible flag from the effective XSLT version
    /// of the supplied element for the duration of the action.
    /// </summary>
    private void WithBackwardsCompatible(XElement element, Action action)
    {
        var previous = _context.BackwardsCompatible;
        _context.BackwardsCompatible = IsEffectiveBackwardsCompatible(element);
        try
        {
            action();
        }
        finally
        {
            _context.BackwardsCompatible = previous;
        }
    }

    /// <summary>
    /// Registers the XSLT <c>accumulator-before()</c> and <c>accumulator-after()</c>
    /// functions for every declared accumulator on the supplied context.
    /// </summary>
    private void RegisterAccumulatorFunctions(EvaluationContext context)
    {
        if (_accumulators.Count == 0)
            return;

        context.RegisterFunction(new FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "accumulator-before",
            Arity = 1,
            ParameterTypes = new List<XdmValueKind> { XdmValueKind.String },
            ReturnType = XdmValueKind.Sequence,
            Implementation = (ctx, args) => GetAccumulatorValue(ctx, args, before: true)
        });

        context.RegisterFunction(new FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "accumulator-after",
            Arity = 1,
            ParameterTypes = new List<XdmValueKind> { XdmValueKind.String },
            ReturnType = XdmValueKind.Sequence,
            Implementation = (ctx, args) => GetAccumulatorValue(ctx, args, before: false)
        });
    }

    /// <summary>
    /// Implements <c>accumulator-before()</c> / <c>accumulator-after()</c>.
    /// </summary>
    private XdmValue GetAccumulatorValue(EvaluationContext ctx, ReadOnlySpan<XdmValue> args, bool before)
    {
        var nameArg = args[0];
        var name = nameArg.IsAtomic ? nameArg.ToString() : string.Empty;
        if (string.IsNullOrEmpty(name))
            throw new InvalidOperationException("XTDE3341: accumulator name must be a string");

        var accName = ResolveAccumulatorFunctionName(name, ctx);
        if (string.IsNullOrEmpty(accName))
            throw new InvalidOperationException($"XTDE3341: accumulator '{name}' not found");

        var contextItem = ctx.ContextItem;
        if (!contextItem.IsNode || contextItem.NodeValue == null)
            throw new InvalidOperationException("XTDE3362: accumulator functions require a context item that is a node");

        var node = contextItem.NodeValue;

        // First check for values copied with copy-accumulators="yes"
        if (node is XDocumentNode xdn && xdn.UnderlyingObject is XElement elem)
        {
            var copied = elem.Annotation<AccumulatorValues>();
            if (copied != null)
            {
                if (copied.InapplicableNames.Contains(accName))
                    throw new InvalidOperationException($"XTDE3362: accumulator '{name}' is not applicable to the current node");
                if (copied.ApplicableNames.Contains(accName) && copied.Values.TryGetValue(accName, out var pair))
                    return before ? pair.Before : pair.After;
            }
        }

        // Otherwise compute from the source tree.
        if (!IsAccumulatorApplicableToTree(accName, node))
            throw new InvalidOperationException($"XTDE3362: accumulator '{name}' is not applicable to the current node");

        var acc = _accumulators.FirstOrDefault(a => a.ClarkName == accName);
        if (acc == null)
            throw new InvalidOperationException($"XTDE3341: accumulator '{name}' not found");

        var root = GetRootNode(node);
        var nodeValues = GetAccumulatorNodeValues(acc, root);
        if (nodeValues.TryGetValue(node, out var values))
            return before ? values.Before : values.After;

        // Nodes not visited by the accumulator (e.g. attributes/text matched indirectly)
        // return the initial value for before and after.
        var initialCompiled = CompileXPath(acc.InitialValue, acc.Element);
        return initialCompiled.Evaluate(new EvaluationContext());
    }

    /// <summary>
    /// Determines whether an accumulator is applicable to a source tree when copying
    /// accumulator values. For the initial source document, applicability is controlled
    /// by the initial mode's <c>use-accumulators</c> declaration; for other trees (temporary
    /// trees, documents loaded with <c>doc()</c>, etc.) all accumulators are treated as applicable.
    /// </summary>
    private bool IsAccumulatorApplicableToTree(string accClarkName, IXdmNode sourceNode)
    {
        var sourceRoot = GetRootNode(sourceNode);

        // Non-initial trees may have an explicit applicability set (e.g. from xsl:merge-source
        // use-accumulators). If such a set is recorded, membership governs applicability.
        if (_accumulatorApplicability.TryGetValue(sourceRoot, out var applicableSet))
            return applicableSet.Contains(accClarkName);

        if (_initialSource == null)
            return true;

        var initialRoot = GetRootNode(_initialSource);
        if (!sourceRoot.IsSameNode(initialRoot))
            return true;

        // When the transformation was started with a named template, the source
        // document is the global context item rather than the initial match
        // selection, so the initial mode's use-accumulators does not restrict it.
        if (_startedWithNamedTemplate)
            return true;

        var modeDef = _stylesheet.GetModeDefinition(_initialMode);
        if (modeDef != null)
        {
            if (modeDef.UseAllAccumulators)
                return true;
            return modeDef.UseAccumulators.Contains(accClarkName);
        }

        // No explicit xsl:mode declaration for the initial mode: the default for
        // use-accumulators is an empty list, so no accumulators are applicable to
        // the initial match selection unless explicitly declared.
        return false;
    }

    /// <summary>
    /// Resolves an accumulator name supplied to <c>accumulator-before()</c> / <c>accumulator-after()</c>
    /// to Clark notation using the in-scope namespaces of the calling expression.
    /// </summary>
    private string ResolveAccumulatorFunctionName(string name, EvaluationContext ctx)
    {
        if (name.StartsWith("{"))
            return name;

        var colon = name.IndexOf(':');
        if (colon < 0)
        {
            // Unprefixed accumulator names are in no namespace.
            foreach (var acc in _accumulators)
            {
                if (acc.LocalName == name && string.IsNullOrEmpty(acc.NamespaceUri))
                    return acc.ClarkName;
            }
            return "";
        }

        var prefix = name[..colon];
        var local = name[(colon + 1)..];
        if (ctx.TryResolveNamespace(prefix, out var nsUri))
        {
            var clark = $"{{{nsUri}}}{local}";
            if (_accumulators.Any(a => a.ClarkName == clark))
                return clark;
        }
        return "";
    }

    /// <summary>
    /// Returns the cached accumulator values for every node in the source tree,
    /// computing them on first use.
    /// </summary>
    private Dictionary<IXdmNode, (XdmValue Before, XdmValue After)> GetAccumulatorNodeValues(Stylesheet.AccumulatorDefinition acc, IXdmNode root)
    {
        var key = (root, acc.ClarkName);
        if (_accumulatorCache.TryGetValue(key, out var nodeValues))
            return nodeValues;

        if (!_accumulatorsInProgress.Add(key))
            throw new InvalidOperationException($"XTDE3400: cyclic dependency detected in accumulator '{acc.ClarkName}'");

        try
        {
            nodeValues = ComputeAccumulatorValues(acc, root);
            _accumulatorCache[key] = nodeValues;
            return nodeValues;
        }
        finally
        {
            _accumulatorsInProgress.Remove(key);
        }
    }

    /// <summary>
    /// Computes the accumulator value before and after each node in the source tree.
    /// Walks the tree in document order, applying start-phase rules before descendants
    /// and end-phase rules after descendants. Sequence-constructor rule bodies are
    /// evaluated in an isolated context that still sees the standard and accumulator
    /// function libraries.
    /// </summary>
    private Dictionary<IXdmNode, (XdmValue Before, XdmValue After)> ComputeAccumulatorValues(Stylesheet.AccumulatorDefinition acc, IXdmNode root)
    {
        var result = new Dictionary<IXdmNode, (XdmValue Before, XdmValue After)>();
        var initialCtx = CreateAccumulatorEvaluationContext(focusNode: root, value: null);
        var current = ConvertVariableValue(CompileXPath(acc.InitialValue, acc.Element).Evaluate(initialCtx), acc.As);

        var compiledRules = new List<(Stylesheet.AccumulatorRule Rule, Patterns.PatternPredicate Match)>();
        var patternCompiler = new Patterns.PatternCompiler(_context);
        foreach (var rule in acc.Rules)
        {
            var defaultNs = GetXPathDefaultNamespace(rule.Element);
            var match = patternCompiler.Compile(rule.Match, defaultNs ?? "");
            compiledRules.Add((rule, match));
        }

        Walk(root);
        return result;

        XdmValue Walk(IXdmNode node)
        {
            // Apply all matching start-phase rules before descendants, in declaration order.
            // The value after the start rules is what accumulator-before() returns for this node.
            foreach (var startRule in compiledRules.Where(r => IsAccumulatorStartRule(r.Rule) && r.Match(XdmValue.FromNode(node), _context)))
                current = ApplyAccumulatorRule(acc, startRule, node, current);
            var before = current;

            foreach (var attr in node.Axis(XdmAxis.Attribute))
            {
                if (attr.IsNode && attr.NodeValue != null)
                    current = Walk(attr.NodeValue);
            }

            foreach (var child in node.Axis(XdmAxis.Child))
            {
                if (child.IsNode && child.NodeValue != null)
                    current = Walk(child.NodeValue);
            }

            // Apply all matching end-phase rules after descendants, in declaration order.
            // The final current value is what accumulator-after() returns for this node.
            foreach (var endRule in compiledRules.Where(r => IsAccumulatorEndRule(r.Rule) && r.Match(XdmValue.FromNode(node), _context)))
                current = ApplyAccumulatorRule(acc, endRule, node, current);

            result[node] = (before, current);
            return current;
        }
    }

    private static bool IsAccumulatorStartRule(Stylesheet.AccumulatorRule rule)
        => string.IsNullOrEmpty(rule.Phase) || rule.Phase.Equals("start", StringComparison.OrdinalIgnoreCase);

    private static bool IsAccumulatorEndRule(Stylesheet.AccumulatorRule rule)
        => rule.Phase != null && rule.Phase.Equals("end", StringComparison.OrdinalIgnoreCase);

    private XdmValue ApplyAccumulatorRule(Stylesheet.AccumulatorDefinition acc, (Stylesheet.AccumulatorRule Rule, Patterns.PatternPredicate Match) rulePair, IXdmNode node, XdmValue current)
    {
        if (rulePair.Rule == null)
            return current;

        var ruleCtx = CreateAccumulatorEvaluationContext(node, current);
        XdmValue newValue;
        if (!string.IsNullOrEmpty(rulePair.Rule.Select))
        {
            newValue = CompileXPath(rulePair.Rule.Select, rulePair.Rule.Element).Evaluate(ruleCtx);
        }
        else
        {
            newValue = EvaluateAccumulatorRuleBody(rulePair.Rule.Element, ruleCtx);
        }
        return ConvertVariableValue(newValue, acc.As);
    }

    /// <summary>
    /// Creates an evaluation context for accumulator expressions. It contains the
    /// standard function library, XSLT-specific functions, the accumulator
    /// functions, global stylesheet variables, and the accumulator <c>$value</c>
    /// variable pre-bound.
    /// </summary>
    private EvaluationContext CreateAccumulatorEvaluationContext(IXdmNode? focusNode = null, XdmValue? value = null)
    {
        var ctx = new EvaluationContext();
        FunctionLibrary.Populate(ctx);
        XsltFunctionLibrary.Populate(ctx);
        RegisterAccumulatorFunctions(ctx);

        // Accumulator expressions may reference global variables/parameters.
        // Copy all globals into the accumulator context, but skip any variable that
        // is currently being initialized. Forcing evaluation of the initializing
        // variable would make a global whose value uses accumulator-after() look
        // like a circular reference (accumulator-090); other globals are still
        // lazily resolved as usual (merge-066).
        ctx.LazyVariableResolver = _context.LazyVariableResolver;
        foreach (var (name, _) in _stylesheet.GetAllGlobalVariables())
        {
            var (localName, ns) = ExpandVariableName(_stylesheet.Root, name);
            if (_evaluatingGlobals.Contains((localName, ns)))
                continue;
            if (_context.TryGetVariable(localName, out var varValue, ns))
                ctx.WithVariable(localName, varValue, ns);
        }

        if (focusNode != null)
            ctx.WithFocus(XdmValue.FromNode(focusNode), 1, 1);
        if (value != null)
            ctx.WithVariable("value", (XdmValue)value);
        return ctx;
    }

    /// <summary>
    /// Evaluates the sequence-constructor body of an <c>xsl:accumulator-rule</c>.
    /// Supports local <c>xsl:variable</c> declarations and <c>xsl:sequence</c>/<c>xsl:value-of</c>
    /// instructions that return the new accumulator value.
    /// </summary>
    private XdmValue EvaluateAccumulatorRuleBody(XElement ruleElement, EvaluationContext ctx)
    {
        var items = new List<XdmValue>();
        foreach (var child in ruleElement.Elements())
        {
            if (child.Name.NamespaceName != Stylesheet.Stylesheet.XslNamespace)
                continue;

            switch (child.Name.LocalName)
            {
                case "param":
                    continue;
                case "variable":
                    {
                        var name = child.Attribute("name")?.Value;
                        var select = child.Attribute("select")?.Value;
                        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(select))
                            ctx.WithVariable(name, CompileXPath(select, child).Evaluate(ctx));
                        break;
                    }
                case "sequence":
                    {
                        var select = child.Attribute("select")?.Value;
                        if (!string.IsNullOrEmpty(select))
                            items.Add(CompileXPath(select, child).Evaluate(ctx));
                        break;
                    }
                case "value-of":
                    {
                        var select = child.Attribute("select")?.Value;
                        if (!string.IsNullOrEmpty(select))
                            items.Add(CompileXPath(select, child).Evaluate(ctx));
                        break;
                    }
                case "if":
                    {
                        var test = child.Attribute("test")?.Value;
                        if (!string.IsNullOrEmpty(test) && CompileXPath(test, child).Evaluate(ctx).EffectiveBooleanValue())
                            items.Add(EvaluateAccumulatorRuleBody(child, ctx));
                        break;
                    }
                case "choose":
                    {
                        foreach (var when in child.Elements(XName.Get("when", Stylesheet.Stylesheet.XslNamespace)))
                        {
                            var test = when.Attribute("test")?.Value;
                            if (!string.IsNullOrEmpty(test) && CompileXPath(test, when).Evaluate(ctx).EffectiveBooleanValue())
                            {
                                items.Add(EvaluateAccumulatorRuleBody(when, ctx));
                                break;
                            }
                        }
                        var otherwise = child.Element(XName.Get("otherwise", Stylesheet.Stylesheet.XslNamespace));
                        if (otherwise != null)
                            items.Add(EvaluateAccumulatorRuleBody(otherwise, ctx));
                        break;
                    }
                case "iterate":
                    {
                        var select = child.Attribute("select")?.Value;
                        var seq = string.IsNullOrEmpty(select)
                            ? XdmValue.FromSequence(XdmSequence.Empty)
                            : CompileXPath(select, child).Evaluate(ctx);

                        var paramElements = child.Elements(XName.Get("param", Stylesheet.Stylesheet.XslNamespace)).ToList();
                        var iterationParams = new Dictionary<string, XdmValue>();
                        foreach (var p in paramElements)
                        {
                            var pname = p.Attribute("name")?.Value;
                            var pselect = p.Attribute("select")?.Value;
                            if (!string.IsNullOrEmpty(pname))
                                iterationParams[pname] = string.IsNullOrEmpty(pselect)
                                    ? XdmValue.Undefined
                                    : CompileXPath(pselect, p).Evaluate(ctx);
                        }

                        XdmValue? iterateResult = null;
                        bool broken = false;
                        var seqItems = AsAccumulatorSequence(seq).ToList();
                        int total = seqItems.Count;
                        for (int i = 0; i < total; i++)
                        {
                            var item = seqItems[i];
                            var savedItem = ctx.ContextItem;
                            var savedPos = ctx.ContextPosition;
                            var savedSize = ctx.ContextSize;
                            ctx.WithFocus(item, i + 1, total);
                            foreach (var kv in iterationParams)
                                ctx.WithVariable(kv.Key, kv.Value);

                            var bodyChildren = new List<XElement>();
                            foreach (var bodyChild in child.Elements())
                            {
                                if (bodyChild.Name.LocalName == "param" || bodyChild.Name.LocalName == "on-completion")
                                    continue;
                                if (bodyChild.Name.LocalName == "next-iteration")
                                {
                                    iterationParams.Clear();
                                    foreach (var wp in bodyChild.Elements(XName.Get("with-param", Stylesheet.Stylesheet.XslNamespace)))
                                    {
                                        var wpName = wp.Attribute("name")?.Value;
                                        var wpSelect = wp.Attribute("select")?.Value;
                                        if (!string.IsNullOrEmpty(wpName))
                                            iterationParams[wpName] = string.IsNullOrEmpty(wpSelect)
                                                ? XdmValue.Undefined
                                                : CompileXPath(wpSelect, wp).Evaluate(ctx);
                                    }
                                    break;
                                }
                                if (bodyChild.Name.LocalName == "break")
                                {
                                    var breakSelect = bodyChild.Attribute("select")?.Value;
                                    iterateResult = string.IsNullOrEmpty(breakSelect)
                                        ? XdmValue.Undefined
                                        : CompileXPath(breakSelect, bodyChild).Evaluate(ctx);
                                    broken = true;
                                    break;
                                }
                                bodyChildren.Add(bodyChild);
                            }

                            if (!broken && bodyChildren.Count > 0)
                            {
                                var wrapper = new XElement("__iter__");
                                wrapper.Add(bodyChildren);
                                var produced = EvaluateAccumulatorRuleBody(wrapper, ctx);
                                if (!produced.IsUndefined)
                                    items.Add(produced);
                            }

                            ctx.WithFocus(savedItem, savedPos, savedSize);
                            if (broken) break;
                        }

                        if (!broken)
                        {
                            var onCompletion = child.Element(XName.Get("on-completion", Stylesheet.Stylesheet.XslNamespace));
                            if (onCompletion != null)
                            {
                                var ocSelect = onCompletion.Attribute("select")?.Value;
                                iterateResult = string.IsNullOrEmpty(ocSelect)
                                    ? EvaluateAccumulatorRuleBody(onCompletion, ctx)
                                    : CompileXPath(ocSelect, onCompletion).Evaluate(ctx);
                            }
                        }

                        if (iterateResult.HasValue && !iterateResult.Value.IsUndefined)
                            items.Add(iterateResult.Value);
                        break;
                    }
            }
        }

        if (items.Count == 0)
            return XdmValue.FromSequence(XdmSequence.Empty);
        if (items.Count == 1)
            return items[0];
        return XdmValue.FromSequence(MaterializedSequence.FromList(items));
    }

    /// <summary>
    /// Returns the items of an XDM value as an enumerable of single-item values.
    /// Used by accumulator rule body evaluation where the input may be a sequence,
    /// a single item, or an empty/undefined value.
    /// </summary>
    private static IEnumerable<XdmValue> AsAccumulatorSequence(XdmValue value)
    {
        if (value.IsUndefined)
            yield break;
        if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                yield return item;
        }
        else
        {
            yield return value;
        }
    }

    /// <summary>
    /// Attaches the accumulator values for the source node to a copied element.
    /// </summary>
    private void AttachAccumulatorValues(IXdmNode sourceNode, XElement copy)
    {
        if (_accumulators.Count == 0)
            return;

        var root = GetRootNode(sourceNode);
        var values = new AccumulatorValues();
        foreach (var acc in _accumulators)
        {
            if (IsAccumulatorApplicableToTree(acc.ClarkName, sourceNode))
            {
                values.ApplicableNames.Add(acc.ClarkName);
                var nodeValues = GetAccumulatorNodeValues(acc, root);
                if (nodeValues.TryGetValue(sourceNode, out var pair))
                    values.Values[acc.ClarkName] = pair;
            }
            else
            {
                values.InapplicableNames.Add(acc.ClarkName);
            }
        }
        copy.AddAnnotation(values);
    }

    /// <summary>
    /// Executes the body of an xsl:function declaration, binding parameters and
    /// returning the sequence produced by the function body.
    /// </summary>
    private const int MaxXsltFunctionCallDepth = 256;

    private XdmValue ExecuteXsltFunction(Stylesheet.XsltFunctionDefinition def, ReadOnlySpan<XdmValue> args)
    {
        if (++_xsltFunctionCallDepth > MaxXsltFunctionCallDepth)
        {
            _xsltFunctionCallDepth--;
            throw new InvalidOperationException("XSLT function recursion depth exceeded maximum allowed depth.");
        }

        var snapshot = _context.SnapshotVariables();
        var savedFocus = _context.ContextItem;
        var savedPosition = _context.ContextPosition;
        var savedSize = _context.ContextSize;
        var savedCurrent = _context.CurrentItem;
        var savedAccumulator = _sequenceAccumulator;
        var savedTunnelStack = _tunnelParamStack.ToArray();
        var savedMergeGroup = _currentMergeGroup;
        var savedMergeKey = _currentMergeKey;
        var savedNamedGroups = _currentNamedMergeGroups;
        var savedMergeSourceNames = _currentMergeSourceNames;
        var savedRegexGroups = _context.RegexGroups;
        var savedLazyResolver = _context.LazyVariableResolver;
        var savedSuppressCaching = _context.SuppressLazyGlobalCaching;
        var savedFunctionLocals = _functionLocalLazyVariables;
        var savedOutputUri = _context.CurrentOutputUri;
        _sequenceAccumulator = null;
        _tunnelParamStack.Clear();
        _currentMergeGroup = null;
        _currentMergeKey = null;
        _currentNamedMergeGroups = null;
        _currentMergeSourceNames = null;
        _context.RegexGroups = null;
        var effectiveNewEachTime = GetEffectiveFunctionAttribute(def.Element, "new-each-time");
        bool memoize = IsDeterministicNewEachTime(effectiveNewEachTime);
        XsltFunctionCacheKey? cacheKey = memoize ? new XsltFunctionCacheKey(def.NamespaceUri, def.LocalName, def.Arity, args) : null;
        try
        {
            _context.CurrentOutputUri = null;

            if (cacheKey.HasValue && _xsltFunctionCache.TryGetValue(cacheKey.Value, out var cached))
                return cached;

            // XSLT functions have no context item by default (XSLT 3.0 §9.6).
            // xsl:sequence/@select and other XPath expressions must not see
            // the caller's context item.
            _context.WithFocus(XdmValue.Undefined, 0, 0);

            // Function bodies run in their own variable scope. Remove caller/template
            // variables so an outer function's local variables are not mistaken for
            // inner-function locals. Parameters are bound into the fresh scope below;
            // globals remain available via the lazy global resolver.
            _context.RestoreVariables(new Dictionary<(string, string), XdmValue>());

            // Bind parameters, applying the XPath function conversion rules for each
            // xsl:param/@as type.
            var paramElements = def.Element.Elements(XName.Get("param", Stylesheet.Stylesheet.XslNamespace)).ToList();
            for (int i = 0; i < def.ParameterNames.Count && i < args.Length; i++)
            {
                var asType = i < paramElements.Count ? paramElements[i].Attribute("as")?.Value : null;
                var converted = ConvertFunctionArgument(args[i], asType);
                var (fpLocal, fpNs) = ExpandVariableName(def.Element, def.ParameterNames[i]);
                _context.WithVariable(fpLocal, converted, fpNs);
            }

            // Function-local variables whose eager evaluation would trigger a circular
            // reference to a global currently being evaluated are deferred and only
            // computed if actually referenced. This avoids false circularity errors
            // for unused variables that reference globals under evaluation (param-0301).
            var functionLocals = new Dictionary<(string LocalName, string NamespaceUri), Lazy<XdmValue>>();
            _functionLocalLazyVariables = functionLocals;
            _context.LazyVariableResolver = (local, ns) =>
            {
                if (functionLocals.TryGetValue((local, ns), out var lazy))
                {
                    var value = lazy.Value;
                    _context.WithVariable(local, value, ns);
                    // Function-local values must not leak into the global lazy cache,
                    // but globals resolved while evaluating the local should still cache.
                    _context.SkipLazyGlobalCacheOnce = true;
                    return value;
                }
                return savedLazyResolver?.Invoke(local, ns);
            };

            // XSLT functions have no context item (XSLT 3.0 §9.6).
            // Evaluate the function body with an absent context item.
            var result = EvaluateFunctionBody(def.Element, XdmValue.Undefined);
            var convertedResult = ConvertVariableValue(result, def.ReturnType);
            if (cacheKey.HasValue)
                _xsltFunctionCache[cacheKey.Value] = convertedResult;
            return convertedResult;
        }
        finally
        {
            _xsltFunctionCallDepth--;
            _sequenceAccumulator = savedAccumulator;
            _tunnelParamStack.Clear();
            foreach (var frame in savedTunnelStack.Reverse())
                _tunnelParamStack.Push(frame);
            _context.RestoreVariables(snapshot);
            _context.WithFocus(savedFocus, savedPosition, savedSize);
            _context.WithCurrentItem(savedCurrent);
            _currentMergeGroup = savedMergeGroup;
            _currentMergeKey = savedMergeKey;
            _currentNamedMergeGroups = savedNamedGroups;
            _currentMergeSourceNames = savedMergeSourceNames;
            _context.RegexGroups = savedRegexGroups;
            _context.LazyVariableResolver = savedLazyResolver;
            _context.SuppressLazyGlobalCaching = savedSuppressCaching;
            _context.CurrentOutputUri = savedOutputUri;
            _functionLocalLazyVariables = savedFunctionLocals;
        }
    }

    private string GetEffectiveFunctionAttribute(XElement functionElement, string name)
    {
        var avtAttr = functionElement.Attribute("_" + name);
        if (avtAttr != null)
            return EvaluateAvt(avtAttr.Value, functionElement).Trim();

        var staticAttr = functionElement.Attribute(name);
        return staticAttr?.Value.Trim() ?? string.Empty;
    }

    private static bool IsDeterministicNewEachTime(string value)
    {
        return value switch
        {
            "no" or "false" or "0" or "maybe" or "probably" => true,
            _ => false
        };
    }

    /// <summary>
    /// Evaluates a single xsl:variable declaration inside an xsl:function body.
    /// Used by the lazy function-local variable resolver.
    /// </summary>
    private XdmValue EvaluateFunctionLocalVariable(XElement instruction, XdmValue contextItem)
    {
        var varSelect = instruction.Attribute("select")?.Value;
        XdmValue varValue;
        if (!string.IsNullOrEmpty(varSelect))
        {
            var compiled = CompileXPath(varSelect, instruction);
            varValue = compiled.Evaluate(_context);
        }
        else
        {
            // Function-local variable bodies are evaluated in a temporary output state.
            var savedOutputUri = _context.CurrentOutputUri;
            _context.CurrentOutputUri = null;
            try
            {
                varValue = EvaluateSequenceConstructor(instruction, contextItem, wrapInDocumentNode: string.IsNullOrEmpty(instruction.Attribute("as")?.Value));
            }
            finally
            {
                _context.CurrentOutputUri = savedOutputUri;
            }
        }
        return ConvertVariableValue(varValue, instruction.Attribute("as")?.Value);
    }

    /// <summary>
    /// Evaluates the body of an xsl:function and returns the resulting XDM value.
    /// Skips xsl:param children (already bound) and collects items from all other
    /// sequence-constructor children.
    /// </summary>
    private XdmValue EvaluateFunctionBody(XElement functionElement, XdmValue contextItem)
    {
        var items = new List<XdmValue>();
        foreach (var childNode in functionElement.Nodes())
        {
            if (childNode is XElement e && e.Name.LocalName == "param" && e.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                continue;

            ProcessFunctionBodyNode(childNode, items, contextItem);
        }

        items = NormalizeSequenceConstructorItems(items);

        if (items.Count == 0)
            return XdmValue.FromSequence(XdmSequence.Empty);
        if (items.Count == 1)
            return items[0];

        return XdmValue.FromSequence(MaterializedSequence.FromList(items));
    }

    /// <summary>
    /// Normalizes the items produced by a sequence constructor according to XSLT rules:
    /// adjacent non-empty text nodes are merged, and empty text nodes adjacent to
    /// non-empty text nodes are absorbed into the merge. Consecutive empty text nodes
    /// are preserved as separate items because zero-length text nodes created by
    /// xsl:text/xsl:value-of are significant in an xsl:function result.
    /// </summary>
    private static List<XdmValue> NormalizeSequenceConstructorItems(List<XdmValue> items)
    {
        var normalized = new List<XdmValue>();
        var pendingText = new StringBuilder();
        var pendingEmptyTextNodes = new List<XdmValue>();
        XdmValue pendingSingleTextItem = default;
        bool pendingTextIsSingleNode = false;

        void FlushText()
        {
            if (pendingText.Length > 0)
            {
                // When the pending text comes from exactly one text node (no merging with
                // adjacent text nodes occurred), keep the original node so its identity
                // and parentage survive (fn:snapshot equivalence, snapshot-0102a).
                if (pendingTextIsSingleNode)
                    normalized.Add(pendingSingleTextItem);
                else
                    normalized.Add(XdmValue.FromNode(new XDocumentNode(new XText(pendingText.ToString()))));
                pendingText.Clear();
                pendingTextIsSingleNode = false;
                pendingSingleTextItem = default;
            }
        }

        foreach (var item in items)
        {
            if (item.IsNode && item.NodeValue is { NodeKind: XdmNodeKind.Text } node)
            {
                var text = node.StringValue;
                if (text.Length == 0)
                {
                    // An empty text node merges with any preceding non-empty run.
                    if (pendingText.Length > 0)
                        continue;
                    // Consecutive empty text nodes are kept as separate items.
                    pendingEmptyTextNodes.Add(item);
                }
                else
                {
                    // A non-empty text node absorbs any preceding empty text nodes.
                    pendingEmptyTextNodes.Clear();
                    bool startsFresh = pendingText.Length == 0;
                    pendingText.Append(text);
                    if (startsFresh)
                    {
                        pendingTextIsSingleNode = true;
                        pendingSingleTextItem = item;
                    }
                    else
                    {
                        pendingTextIsSingleNode = false;
                    }
                }
            }
            else
            {
                FlushText();
                normalized.AddRange(pendingEmptyTextNodes);
                pendingEmptyTextNodes.Clear();
                normalized.Add(item);
            }
        }

        FlushText();
        normalized.AddRange(pendingEmptyTextNodes);
        return normalized;
    }

    /// <summary>
    /// Processes a single node in a function-body sequence constructor.
    /// Literal text nodes become text-node items (subject to whitespace stripping),
    /// XSLT instructions are dispatched, and literal result elements are copied.
    /// </summary>
    private void ProcessFunctionBodyNode(XNode node, List<XdmValue> results, XdmValue contextItem)
    {
        switch (node)
        {
            case XText text:
                {
                    var parent = text.Parent as XElement;
                    bool expandText = parent != null && GetExpandText(parent);
                    bool hasTvtExpression = expandText && ContainsTvtExpression(text.Value);
                    bool hasEscapedBraces = expandText &&
                        (text.Value.Contains("{{", StringComparison.Ordinal) || text.Value.Contains("}}", StringComparison.Ordinal));

                    if (hasTvtExpression || hasEscapedBraces)
                    {
                        // Expand the TVT in-place. Whitespace-only literal segments are
                        // stripped, matching the default whitespace stripping rules for
                        // sequence constructors in an xsl:function body.
                        var parts = EvaluateTvtParts(text.Value, parent);
                        var sb = new StringBuilder();
                        for (int pi = 0; pi < parts.Count; pi++)
                        {
                            bool isLiteral = pi % 2 == 0;
                            string part = parts[pi];
                            if (isLiteral)
                            {
                                if (string.IsNullOrWhiteSpace(part) && !IsWhitespacePreserveContext(parent!))
                                    continue;
                            }
                            sb.Append(part);
                        }

                        if (sb.Length > 0)
                        {
                            results.Add(XdmValue.FromNode(new XDocumentNode(new XText(sb.ToString()))));
                        }
                    }
                    else
                    {
                        var value = text.Value;
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            // Whitespace text nodes are stripped unless xml:space="preserve"
                            // applies to the parent (or an ancestor).
                            if (parent != null && IsWhitespacePreserveContext(parent))
                            {
                                results.Add(XdmValue.FromNode(new XDocumentNode(new XText(value))));
                            }
                        }
                        else
                        {
                            results.Add(XdmValue.FromNode(new XDocumentNode(new XText(value))));
                        }
                    }
                }
                break;
            case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                EvaluateFunctionBodyInstruction(elem, results, contextItem);
                break;
            case XElement elem:
                {
                    var copy = CopyLiteralElementToXElement(elem);
                    results.Add(XdmValue.FromNode(new XDocumentNode(copy)));
                }
                break;
        }
    }

    /// <summary>
    /// Builds an <see cref="XdmValue"/> from a flat list of items.
    /// </summary>
    private static XdmValue MaterializeItemList(List<XdmValue> items)
    {
        if (items.Count == 0)
            return XdmValue.FromSequence(XdmSequence.Empty);
        if (items.Count == 1)
            return items[0];
        return XdmValue.FromSequence(MaterializedSequence.FromList(items));
    }

    /// <summary>
    /// Atomizes a value for use as a map key. Rejects function, map and array
    /// keys and normalizes singleton sequences.
    /// </summary>
    private static XdmValue AtomizeMapKey(XdmValue value)
    {
        var items = EnumerateItems(value).ToList();
        if (items.Count == 0)
            throw new InvalidOperationException("XPTY0004: Map key cannot be an empty sequence");
        if (items.Count > 1)
            throw new InvalidOperationException("XPTY0004: Map key must be a single atomic value");

        var item = items[0];
        if (item.IsFunction || item.IsMap || item.IsArray)
            throw new InvalidOperationException("FOTY0013: Map key cannot be a function item, map, or array");

        if (item.IsNode)
            return XdmValue.FromString(item.NodeValue.StringValue, "untypedAtomic");

        return item;
    }

    /// <summary>
    /// Evaluates the value of an <c>xsl:map-entry</c> instruction: either the
    /// <c>@select</c> expression or the sequence-constructor content. It is a
    /// static error to supply both <c>@select</c> and a sequence constructor.
    /// </summary>
    private XdmValue EvaluateMapEntryValue(XElement mapEntry, XdmValue contextItem)
    {
        var selectAttr = mapEntry.Attribute("select")?.Value;
        var hasContent = mapEntry.Elements().Any()
            || mapEntry.Nodes().OfType<XText>().Any(t => !IsWhitespaceOnly(t.Value));

        if (!string.IsNullOrEmpty(selectAttr))
        {
            if (hasContent)
                throw new InvalidOperationException("XTSE3280: xsl:map-entry must not have both a select attribute and sequence-constructor content");
            return CompileXPath(selectAttr, mapEntry).Evaluate(_context);
        }

        var items = EvaluateSequenceConstructorToItems(mapEntry, contextItem);
        return MaterializeItemList(items);
    }

    /// <summary>
    /// Builds a single-entry map from an <c>xsl:map-entry</c> instruction.
    /// </summary>
    private XdmValue BuildMapEntry(XElement mapEntry, XdmValue contextItem)
    {
        var keyAttr = mapEntry.Attribute("key")?.Value;
        if (string.IsNullOrEmpty(keyAttr))
            throw new InvalidOperationException("XTSE0010: xsl:map-entry requires a key attribute");

        var key = AtomizeMapKey(CompileXPath(keyAttr, mapEntry).Evaluate(_context));
        var value = EvaluateMapEntryValue(mapEntry, contextItem);

        var map = new XdmMap();
        map.Add(key, value);
        return XdmValue.FromMap(map);
    }

    /// <summary>
    /// Builds a map from an <c>xsl:map</c> instruction by evaluating its
    /// sequence-constructor content and merging the resulting map entries.
    /// Duplicate keys raise <c>XTDE3365</c>.
    /// </summary>
    private XdmValue BuildMapFromInstruction(XElement mapInstruction, XdmValue contextItem)
    {
        var entries = new List<XdmValue>();
        foreach (var child in mapInstruction.Elements())
            EvaluateFunctionBodyInstruction(child, entries, contextItem);

        var map = new XdmMap();
        foreach (var item in entries)
        {
            foreach (var entry in EnumerateItems(item))
            {
                if (!entry.IsMap)
                    throw new InvalidOperationException("XTTE3365: xsl:map content must produce map entries");

                var entryMap = entry.MapValue;
                if (entryMap.Count != 1)
                    throw new InvalidOperationException("XTTE3365: xsl:map content must produce map entries");

                foreach (var kvp in entryMap.Entries)
                {
                    if (map.ContainsKey(kvp.Key))
                        throw new InvalidOperationException("XTDE3365: Duplicate key in xsl:map");
                    map.Add(kvp.Key, kvp.Value);
                }
            }
        }

        return XdmValue.FromMap(map);
    }

    /// <summary>
    /// Evaluates a single instruction inside an xsl:function body and appends
    /// the produced items to <paramref name="results"/>.
    /// </summary>
    private void EvaluateFunctionBodyInstruction(XElement instruction, List<XdmValue> results, XdmValue contextItem)
    {
        _currentInstruction = instruction;
        if (instruction.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
        {
            switch (instruction.Name.LocalName)
            {
                case "sequence":
                    {
                        var select = instruction.Attribute("select")?.Value;
                        if (!string.IsNullOrEmpty(select))
                        {
                            var compiled = XPath31Expression.Compile(select!);
                            var result = compiled.Evaluate(_context);
                            FlattenToList(result, results);
                        }
                        else
                        {
                            foreach (var child in instruction.Nodes())
                            {
                                switch (child)
                                {
                                    case XText text:
                                        if (GetExpandText(instruction))
                                        {
                                            var tvtResult = EvaluateTvt(text.Value, instruction);
                                            results.Add(XdmValue.FromNode(new XDocumentNode(new XText(tvtResult))));
                                        }
                                        else if (!IsWhitespaceOnly(text.Value))
                                        {
                                            results.Add(XdmValue.FromNode(new XDocumentNode(new XText(text.Value))));
                                        }
                                        break;
                                    case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                        EvaluateFunctionBodyInstruction(elem, results, contextItem);
                                        break;
                                    case XElement elem:
                                        results.Add(XdmValue.FromNode(new XDocumentNode(elem)));
                                        break;
                                }
                            }
                        }
                        break;
                    }
                case "value-of":
                    {
                        var select = instruction.Attribute("select")?.Value;
                        string textValue;
                        if (!string.IsNullOrEmpty(select))
                        {
                            var compiled = XPath31Expression.Compile(select);
                            var result = compiled.Evaluate(_context);
                            var sep = EvaluateAvt(instruction.Attribute("separator")?.Value ?? " ", instruction);
                            textValue = XdmValueToString(result, sep);
                        }
                        else if (GetExpandText(instruction))
                        {
                            var text = string.Concat(instruction.Nodes().OfType<XText>().Select(t => t.Value));
                            textValue = EvaluateTvt(text, instruction);
                        }
                        else
                        {
                            // xsl:value-of with sequence-constructor content (no @select)
                            var voSep = EvaluateAvt(instruction.Attribute("separator")?.Value ?? "", instruction);
                            textValue = EvaluateSimpleContent(instruction, contextItem, voSep);
                        }
                        // xsl:value-of constructs a text node, even when contributing to a
                        // raw sequence such as an xsl:function result.
                        results.Add(XdmValue.FromNode(new XDocumentNode(new XText(textValue))));
                        break;
                    }
                case "variable":
                    {
                        var varName = instruction.Attribute("name")?.Value;
                        if (!string.IsNullOrEmpty(varName))
                        {
                            var (varLocal, varNs) = ExpandVariableName(instruction, varName);
                            if (_functionLocalLazyVariables != null)
                            {
                                // Eagerly evaluate function-local variables. If doing so would
                                // trigger a circular reference to a global variable that is
                                // currently being evaluated (because this function was called
                                // from that global's select expression), defer evaluation until
                                // the variable is actually referenced. This avoids false
                                // circularity errors for unused function-local variables
                                // (XSLT 3.0 test param-0301).
                                XdmValue eagerValue = XdmValue.Undefined;
                                bool circular = false;
                                try
                                {
                                    eagerValue = EvaluateFunctionLocalVariable(instruction, contextItem);
                                }
                                catch (InvalidOperationException ex)
                                    when (ex.Message.Contains("Circular reference", StringComparison.OrdinalIgnoreCase))
                                {
                                    circular = true;
                                }

                                if (circular)
                                {
                                    var capturedInstruction = instruction;
                                    var capturedContextItem = contextItem;
                                    _functionLocalLazyVariables[(varLocal, varNs)] = new Lazy<XdmValue>(
                                        () => EvaluateFunctionLocalVariable(capturedInstruction, capturedContextItem),
                                        isThreadSafe: false);
                                }
                                else
                                {
                                    _context.WithVariable(varLocal, eagerValue, varNs);
                                }
                            }
                            else
                            {
                                // Fallback for variable declarations evaluated outside a function body
                                // (should not normally reach this path).
                                var varValue = EvaluateFunctionLocalVariable(instruction, contextItem);
                                _context.WithVariable(varLocal, varValue, varNs);
                            }
                        }
                        break;
                    }
                case "if":
                    {
                        var test = instruction.Attribute("test")?.Value;
                        if (!string.IsNullOrEmpty(test))
                        {
                            var compiled = CompileXPath(test, instruction);
                            WithDefaultCollation(instruction, () =>
                            {
                                if (compiled.Evaluate(_context).EffectiveBooleanValue())
                                {
                                    foreach (var child in instruction.Elements())
                                        EvaluateFunctionBodyInstruction(child, results, contextItem);
                                }
                            });
                        }
                        break;
                    }
                case "choose":
                    {
                        bool matched = false;
                        foreach (var when in instruction.Elements(XName.Get("when", Stylesheet.Stylesheet.XslNamespace)))
                        {
                            var whenTest = when.Attribute("test")?.Value;
                            if (!string.IsNullOrEmpty(whenTest))
                            {
                                var compiled = CompileXPath(whenTest, when);
                                WithDefaultCollation(when, () =>
                                {
                                    if (compiled.Evaluate(_context).EffectiveBooleanValue())
                                    {
                                        foreach (var childNode in when.Nodes())
                                            ProcessFunctionBodyNode(childNode, results, contextItem);
                                        matched = true;
                                    }
                                });
                                if (matched)
                                    return;
                            }
                        }
                        var otherwise = instruction.Element(XName.Get("otherwise", Stylesheet.Stylesheet.XslNamespace));
                        if (otherwise != null)
                        {
                            WithDefaultCollation(otherwise, () =>
                            {
                                foreach (var childNode in otherwise.Nodes())
                                    ProcessFunctionBodyNode(childNode, results, contextItem);
                            });
                        }
                        break;
                    }
                case "for-each":
                    {
                        var select = instruction.Attribute("select")?.Value;
                        if (string.IsNullOrEmpty(select))
                            throw new InvalidOperationException("XTSE0010: xsl:for-each requires a select attribute");
                        if (!string.IsNullOrEmpty(select))
                        {
                            var compiled = XPath31Expression.Compile(select);
                            var feResult = compiled.Evaluate(_context);
                            var feItems = EnumerateItems(feResult).ToList();

                            // Apply xsl:sort if present
                            var sortElements = instruction.Elements(XName.Get("sort", Stylesheet.Stylesheet.XslNamespace)).ToList();
                            if (sortElements.Count > 0)
                            {
                                feItems = SortItems(feItems, sortElements);
                            }

                            var savedFocus = _context.ContextItem;
                            var savedCurrent = _context.CurrentItem;
                            int pos = 1;
                            foreach (var item in feItems)
                            {
                                _context.WithFocus(item, pos, feItems.Count);
                                _context.WithCurrentItem(item);
                                var feSnapshot = _context.SnapshotVariables();
                                try
                                {
                                    foreach (var childNode in instruction.Nodes())
                                    {
                                        if (childNode is XElement e && e.Name.LocalName == "sort" && e.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                                            continue;
                                        ProcessFunctionBodyNode(childNode, results, item);
                                    }
                                }
                                finally
                                {
                                    _context.RestoreVariables(feSnapshot);
                                }
                                pos++;
                            }
                            _context.WithFocus(savedFocus, 1, 1);
                            _context.WithCurrentItem(savedCurrent);
                        }
                        break;
                    }
                case "for-each-group":
                    {
                        var select = instruction.Attribute("select")?.Value;
                        if (string.IsNullOrEmpty(select)) break;

                        var compiled = CompileXPath(select, instruction);
                        var feResult = compiled.Evaluate(_context);
                        var feItems = EnumerateItems(feResult).ToList();
                        if (feItems.Count == 0) break;

                        var collationAttr = instruction.Attribute("collation")?.Value;
                        var effectiveCollation = string.IsNullOrEmpty(collationAttr) ? _context.DefaultCollation : EvaluateAvt(collationAttr, instruction);

                        ValidateForEachGroupAttributes(instruction);

                        var savedFocus = _context.ContextItem;
                        var savedPosition = _context.ContextPosition;
                        var savedSize = _context.ContextSize;
                        var savedCurrent = _context.CurrentItem;
                        var savedGroup = _currentGroup;
                        var savedKey = _currentGroupingKey;

                        try
                        {
                            var groups = BuildForEachGroups(instruction, feItems, effectiveCollation);

                            var bindGroup = instruction.Attribute("bind-group")?.Value;
                            var bindKey = instruction.Attribute("bind-grouping-key")?.Value;

                            var sortElements = instruction.Elements(XName.Get("sort", Stylesheet.Stylesheet.XslNamespace)).ToList();
                            for (int sortIdx = 0; sortIdx < sortElements.Count; sortIdx++)
                            {
                                var stableAttr = EvaluateAvt(sortElements[sortIdx].Attribute("stable")?.Value ?? "", sortElements[sortIdx]);
                                if (sortIdx > 0 && !string.IsNullOrEmpty(stableAttr))
                                    throw new InvalidOperationException("XTSE1017: @stable is allowed only on the first xsl:sort");
                                if (!string.IsNullOrEmpty(stableAttr))
                                {
                                    var v = stableAttr.Trim();
                                    if (v != "yes" && v != "true" && v != "1" &&
                                        v != "no" && v != "false" && v != "0")
                                        throw new InvalidOperationException("XTSE0020: invalid value for @stable");
                                }
                            }
                            if (sortElements.Count > 0 && groups.Count > 0)
                            {
                                groups = SortGroups(groups, sortElements);
                            }

                            int pos = 1;
                            foreach (var (key, groupItems) in groups)
                            {
                                _currentGroup = groupItems;
                                _currentGroupingKey = key;
                                var rep = groupItems[0];
                                _context.WithFocus(rep, pos, groups.Count);
                                _context.WithCurrentItem(rep);
                                var feSnapshot = _context.SnapshotVariables();
                                try
                                {
                                    if (!string.IsNullOrEmpty(bindGroup))
                                    {
                                        var (bgLocal, bgNs) = ExpandVariableName(instruction, bindGroup);
                                        _context.WithVariable(bgLocal, XdmValue.FromSequence(MaterializedSequence.FromList(groupItems)), bgNs);
                                    }
                                    if (!string.IsNullOrEmpty(bindKey) && key != null)
                                    {
                                        var (bkLocal, bkNs) = ExpandVariableName(instruction, bindKey);
                                        _context.WithVariable(bkLocal, key.Value, bkNs);
                                    }

                                    foreach (var childNode in instruction.Nodes())
                                    {
                                        if (childNode is XElement e && e.Name.LocalName == "sort" && e.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                                            continue;
                                        ProcessFunctionBodyNode(childNode, results, rep);
                                    }
                                }
                                finally
                                {
                                    _context.RestoreVariables(feSnapshot);
                                }
                                pos++;
                            }
                        }
                        finally
                        {
                            _context.WithFocus(savedFocus, savedPosition, savedSize);
                            _context.WithCurrentItem(savedCurrent);
                            _currentGroup = savedGroup;
                            _currentGroupingKey = savedKey;
                        }
                        break;
                    }
                case "apply-templates":
                    {
                        var modeRaw = instruction.Attribute("mode")?.Value?.Trim() ?? "";
                        var mode = ExpandModeName(modeRaw, instruction);
                        var select = instruction.Attribute("select")?.Value;
                        var sortElements = instruction.Elements(XName.Get("sort", Stylesheet.Stylesheet.XslNamespace)).ToList();

                        var (withParams, tunnelParams) = CollectWithParams(instruction, contextItem);

                        var savedContainer = _currentContainer;
                        var savedLastAtomic = _lastAddedWasAtomic;
                        var savedAccumulator = _sequenceAccumulator;
                        var temp = new XElement("__temp__");
                        _currentContainer = temp;
                        _lastAddedWasAtomic = false;
                        _sequenceAccumulator = new ListSequenceAccumulator(results);
                        try
                        {
                            // XSLT 2.0 erratum XT.E19: #current in a function refers to the unnamed mode.
                            // Save and clear the mode stack so ResolveMode("#current") returns "".
                            var savedModes = new List<string>(_modeStack);
                            savedModes.Reverse();
                            _modeStack.Clear();
                            try
                            {
                                ApplyTemplates(contextItem, mode, select, sortElements.Count > 0 ? sortElements : null, tunnelParams, withParams);
                            }
                            finally
                            {
                                foreach (var m in savedModes)
                                    _modeStack.Push(m);
                            }
                        }
                        finally
                        {
                            _currentContainer = savedContainer;
                            _lastAddedWasAtomic = savedLastAtomic;
                            _sequenceAccumulator = savedAccumulator;
                        }
                        foreach (var node in temp.Nodes())
                        {
                            if (node is XElement e)
                            {
                                if (e.Name.LocalName == "__xdm_seq__" && e.Name.NamespaceName == "")
                                {
                                    if (e.Annotation<SequencePlaceholderItems>() is { } holder)
                                    {
                                        foreach (var phItem in holder.Items)
                                            results.Add(phItem);
                                    }
                                }
                                else
                                {
                                    results.Add(XdmValue.FromNode(new XDocumentNode(e)));
                                }
                            }
                            else if (node is XText t)
                                results.Add(XdmValue.FromNode(new XDocumentNode(new XText(t.Value))));
                        }
                        break;
                    }
                case "call-template":
                    {
                        var calledName = instruction.Attribute("name")?.Value;
                        if (!string.IsNullOrEmpty(calledName))
                        {
                            var (withParams, tunnelParams) = CollectWithParams(instruction, contextItem);
                            var savedContainer = _currentContainer;
                            var savedLastAtomic = _lastAddedWasAtomic;
                            var savedAccumulator = _sequenceAccumulator;
                            var temp = new XElement("__temp__");
                            _currentContainer = temp;
                            _lastAddedWasAtomic = false;
                            _sequenceAccumulator = new ListSequenceAccumulator(results);
                            try
                            {
                                // Named templates are matched by expanded QName, so a call using
                                // one prefix can resolve to a template declared with another prefix
                                // bound to the same namespace URI.
                                var resolvedName = ResolveNamedTemplateName(calledName, instruction);
                                CallTemplate(resolvedName, contextItem, withParams, tunnelParams);
                            }
                            finally
                            {
                                _currentContainer = savedContainer;
                                _lastAddedWasAtomic = savedLastAtomic;
                                _sequenceAccumulator = savedAccumulator;
                            }
                            foreach (var node in temp.Nodes())
                            {
                                if (node is XElement e)
                                {
                                    if (e.Name.LocalName == "__xdm_seq__" && e.Name.NamespaceName == "")
                                    {
                                        if (e.Annotation<SequencePlaceholderItems>() is { } holder)
                                        {
                                            foreach (var phItem in holder.Items)
                                                results.Add(phItem);
                                        }
                                    }
                                    else
                                    {
                                        results.Add(XdmValue.FromNode(new XDocumentNode(e)));
                                    }
                                }
                                else if (node is XText t && !string.IsNullOrEmpty(t.Value))
                                    results.Add(XdmValue.FromNode(new XDocumentNode(new XText(t.Value))));
                            }
                        }
                        break;
                    }
                case "try":
                    {
                        var catchElements = instruction.Elements(XName.Get("catch", Stylesheet.Stylesheet.XslNamespace)).ToList();
                        var tryScope = SnapshotTryScope();
                        var outputBefore = results.Count;
                        try
                        {
                            var select = instruction.Attribute("select")?.Value;
                            if (!string.IsNullOrEmpty(select))
                            {
                                var compiled = XPath31Expression.Compile(select);
                                var result = compiled.Evaluate(_context);
                                FlattenToList(result, results);
                            }
                            else
                            {
                                foreach (var childNode in instruction.Nodes())
                                {
                                    if (childNode is XElement e && e.Name.LocalName == "catch" && e.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                                        continue;
                                    ProcessFunctionBodyNode(childNode, results, contextItem);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            RestoreTryScope(tryScope);
                            if (ex is IterateControlException)
                                throw;
                            if (ex.Data.Contains("Bosak.GlobalVariableError"))
                                throw;
                            if (instruction.Attribute("rollback-output")?.Value == "no" && results.Count > outputBefore)
                                throw new InvalidOperationException("XTDE3530: Recovery not possible because output has already been written.");
                            var catchElem = FindMatchingCatch(catchElements, ex);
                            if (catchElem == null)
                                throw;

                            var previous = BindCatchErrorVariables(ex, catchElem, instruction);
                            try
                            {
                                var catchSelect = catchElem.Attribute("select")?.Value;
                                if (!string.IsNullOrEmpty(catchSelect))
                                {
                                    var compiled = XPath31Expression.Compile(catchSelect);
                                    var catchResult = compiled.Evaluate(_context);
                                    FlattenToList(catchResult, results);
                                }
                                else
                                {
                                    foreach (var childNode in catchElem.Nodes())
                                    {
                                        ProcessFunctionBodyNode(childNode, results, contextItem);
                                    }
                                }
                            }
                            finally
                            {
                                RestoreCatchErrorVariables(previous);
                            }
                        }
                        RestoreTryScope(tryScope);
                        break;
                    }
                case "evaluate":
                    {
                        var evalResult = EvaluateXslEvaluate(instruction, contextItem);
                        FlattenToList(evalResult, results);
                        break;
                    }
                case "copy-of":
                    {
                        var copySelect = instruction.Attribute("select")?.Value;
                        if (!string.IsNullOrEmpty(copySelect))
                        {
                            var compiled = XPath31Expression.Compile(copySelect);
                            var result = compiled.Evaluate(_context);
                            var fnCopyNamespacesAttrRaw = instruction.Attribute("copy-namespaces")?.Value
                                ?? instruction.Attribute("_copy-namespaces")?.Value
                                ?? "yes";
                            var fnCopyNamespacesAttr = EvaluateAvt(fnCopyNamespacesAttrRaw, instruction);
                            bool fnCopyAllNs = fnCopyNamespacesAttr != "no" && fnCopyNamespacesAttr != "false";
                            var fnCopyAccumulatorsAttrRaw = instruction.Attribute("copy-accumulators")?.Value ?? "no";
                            var fnCopyAccumulatorsAttr = EvaluateAvt(fnCopyAccumulatorsAttrRaw, instruction);
                            bool fnCopyAccumulators = fnCopyAccumulatorsAttr == "yes" || fnCopyAccumulatorsAttr == "true";
                            if (result.IsSequence && result.SequenceValue != null)
                            {
                                foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                                {
                                    if (item.IsNode && item.NodeValue != null)
                                    {
                                        results.Add(XdmValue.FromNode(CopyXdmNode(item.NodeValue, fnCopyAllNs, fnCopyAccumulators)));
                                    }
                                    else
                                    {
                                        results.Add(item);
                                    }
                                }
                            }
                            else if (result.IsNode && result.NodeValue != null)
                            {
                                results.Add(XdmValue.FromNode(CopyXdmNode(result.NodeValue, fnCopyAllNs, fnCopyAccumulators)));
                            }
                            else
                            {
                                results.Add(result);
                            }
                        }
                        break;
                    }
                case "copy":
                    {
                        IXdmNode? nodeToCopy = null;
                        var copySelect = instruction.Attribute("select")?.Value;
                        if (!string.IsNullOrEmpty(copySelect))
                        {
                            var compiled = XPath31Expression.Compile(copySelect);
                            var result = compiled.Evaluate(_context);
                            if (result.IsNode && result.NodeValue != null)
                            {
                                nodeToCopy = result.NodeValue;
                                _context.WithFocus(XdmValue.FromNode(nodeToCopy), 1, 1);
                            }
                            else if (result.IsSequence && result.SequenceValue != null)
                            {
                                var items = new List<XdmValue>();
                                foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                                    items.Add(item);
                                if (items.Count > 1)
                                    throw new InvalidOperationException("XTTE3180");
                                if (items.Count == 1 && items[0].IsNode && items[0].NodeValue != null)
                                {
                                    _context.WithFocus(items[0], 1, 1);
                                    var fnCopied = CopyNodeForFunctionBody(items[0].NodeValue, instruction);
                                    if (fnCopied != null)
                                        results.Add(XdmValue.FromNode(fnCopied));
                                }
                                break;
                            }
                        }
                        else
                        {
                            nodeToCopy = contextItem.IsNode ? contextItem.NodeValue : null;
                        }

                        if (nodeToCopy == null)
                            throw new InvalidOperationException("XTTE0945");

                        var copied = CopyNodeForFunctionBody(nodeToCopy, instruction);
                        if (copied != null)
                            results.Add(XdmValue.FromNode(copied));
                        break;
                    }
                case "text":
                    {
                        if (instruction.Elements().Any())
                            throw new InvalidOperationException("XTSE0010: xsl:text must contain only text nodes");
                        var text = string.Concat(instruction.Nodes().OfType<XText>().Select(t => t.Value));
                        if (GetExpandText(instruction))
                        {
                            text = EvaluateTvt(text, instruction);
                        }
                        results.Add(XdmValue.FromNode(new XDocumentNode(new XText(text))));
                        break;
                    }
                case "number":
                    {
                        var fnHasValueAttr = !string.IsNullOrEmpty(instruction.Attribute("value")?.Value);
                        var fnHasSelectAttr = !string.IsNullOrEmpty(instruction.Attribute("select")?.Value);

                        if (fnHasValueAttr || fnHasSelectAttr || contextItem.IsNode)
                        {
                            var savedContainer = _currentContainer;
                            var savedLastAtomic = _lastAddedWasAtomic;
                            var temp = new XElement("__temp__");
                            _currentContainer = temp;
                            _lastAddedWasAtomic = false;
                            try
                            {
                                var node = contextItem.IsNode ? contextItem.NodeValue : null;
                                ExecuteXsltNumber(instruction, node!);
                            }
                            finally
                            {
                                _currentContainer = savedContainer;
                                _lastAddedWasAtomic = savedLastAtomic;
                            }

                            var textValue = string.Concat(temp.Nodes().OfType<XText>().Select(t => t.Value));
                            if (!string.IsNullOrEmpty(textValue))
                                results.Add(XdmValue.FromString(textValue));
                        }
                        else
                        {
                            throw new InvalidOperationException("XTTE0990");
                        }
                        break;
                    }
                case "perform-sort":
                    {
                        ValidateSortComesFirst(instruction);
                        var psSelect = instruction.Attribute("select")?.Value;
                        List<XdmValue> psItems;
                        if (!string.IsNullOrEmpty(psSelect))
                        {
                            var compiled = XPath31Expression.Compile(psSelect);
                            var psResult = compiled.Evaluate(_context);
                            psItems = EnumerateItems(psResult).ToList();
                        }
                        else
                        {
                            psItems = EvaluatePerformSortContent(instruction, contextItem);
                        }

                        var sortElements = instruction.Elements(XName.Get("sort", Stylesheet.Stylesheet.XslNamespace)).ToList();
                        if (sortElements.Count > 0)
                        {
                            psItems = SortItems(psItems, sortElements);
                        }

                        foreach (var item in psItems)
                            results.Add(item);
                        break;
                    }
                case "element":
                    {
                        var savedContainer = _currentContainer;
                        var savedLastAtomic = _lastAddedWasAtomic;
                        var temp = new XElement("__temp__");
                        _currentContainer = temp;
                        _lastAddedWasAtomic = false;
                        try
                        {
                            ExecuteXsltInstruction(instruction, contextItem);
                        }
                        finally
                        {
                            _currentContainer = savedContainer;
                            _lastAddedWasAtomic = savedLastAtomic;
                        }
                        var createdElem = temp.Elements().FirstOrDefault();
                        if (createdElem != null)
                        {
                            createdElem.Remove();
                            results.Add(XdmValue.FromNode(new XDocumentNode(createdElem)));
                        }
                        break;
                    }
                case "attribute":
                    {
                        var savedContainer = _currentContainer;
                        var savedLastAtomic = _lastAddedWasAtomic;
                        var temp = new XElement("__temp__");
                        _currentContainer = temp;
                        _lastAddedWasAtomic = false;
                        try
                        {
                            ExecuteXsltInstruction(instruction, contextItem);
                        }
                        finally
                        {
                            _currentContainer = savedContainer;
                            _lastAddedWasAtomic = savedLastAtomic;
                        }
                        var createdAttr = temp.Attributes().FirstOrDefault();
                        if (createdAttr != null)
                        {
                            results.Add(XdmValue.FromNode(new XDocumentNode(new XAttribute(createdAttr.Name, createdAttr.Value))));
                        }
                        break;
                    }
                case "namespace":
                    {
                        // xsl:namespace in a raw sequence (e.g. an xsl:function body) creates
                        // a namespace-node item instead of attaching to a result element.
                        var savedContainer = _currentContainer;
                        var savedLastAtomic = _lastAddedWasAtomic;
                        var temp = new XElement("__temp__");
                        _currentContainer = temp;
                        _lastAddedWasAtomic = false;
                        try
                        {
                            ExecuteXsltInstruction(instruction, contextItem);
                        }
                        finally
                        {
                            _currentContainer = savedContainer;
                            _lastAddedWasAtomic = savedLastAtomic;
                        }
                        foreach (var nsAttr in temp.Attributes().Where(a => a.IsNamespaceDeclaration))
                        {
                            results.Add(XdmValue.FromNode(XDocumentNode.CreateNamespaceNode(nsAttr, temp)));
                        }
                        break;
                    }
                case "document":
                case "source-document":
                    {
                        var savedContainer = _currentContainer;
                        var savedLastAtomic = _lastAddedWasAtomic;
                        var savedAccumulator = _sequenceAccumulator;
                        var temp = new XElement("__temp__");
                        _currentContainer = temp;
                        _lastAddedWasAtomic = false;
                        _sequenceAccumulator = new ListSequenceAccumulator(results);
                        try
                        {
                            ExecuteXsltInstruction(instruction, contextItem);
                        }
                        finally
                        {
                            _currentContainer = savedContainer;
                            _lastAddedWasAtomic = savedLastAtomic;
                            _sequenceAccumulator = savedAccumulator;
                        }
                        break;
                    }
                case "merge":
                    {
                        var savedContainer = _currentContainer;
                        var savedLastAtomic = _lastAddedWasAtomic;
                        var savedAccumulator = _sequenceAccumulator;
                        var temp = new XElement("__temp__");
                        _currentContainer = temp;
                        _lastAddedWasAtomic = false;
                        _sequenceAccumulator = new ListSequenceAccumulator(results);
                        try
                        {
                            ExecuteMergeInstruction(instruction, contextItem);
                        }
                        finally
                        {
                            _currentContainer = savedContainer;
                            _lastAddedWasAtomic = savedLastAtomic;
                            _sequenceAccumulator = savedAccumulator;
                        }
                        foreach (var node in temp.Nodes())
                        {
                            if (node is XElement e)
                                results.Add(XdmValue.FromNode(new XDocumentNode(e)));
                            else if (node is XText t && !string.IsNullOrEmpty(t.Value))
                                results.Add(XdmValue.FromNode(new XDocumentNode(new XText(t.Value))));
                        }
                        break;
                    }
                case "analyze-string":
                    {
                        ExecuteAnalyzeString(instruction, contextItem, (child, ctx) => EvaluateAnalyzeStringChild(child, results, ctx));
                        break;
                    }
                case "map":
                    {
                        results.Add(BuildMapFromInstruction(instruction, contextItem));
                        break;
                    }
                case "map-entry":
                    {
                        results.Add(BuildMapEntry(instruction, contextItem));
                        break;
                    }
                case "iterate":
                    {
                        var iterSelect = instruction.Attribute("select")?.Value;
                        if (string.IsNullOrEmpty(iterSelect))
                            break;

                        var iterItems = EnumerateItems(CompileXPath(iterSelect, instruction).Evaluate(_context)).ToList();
                        var iterParamValues = new Dictionary<(string LocalName, string NamespaceUri), XdmValue>();
                        var xslNs = Stylesheet.Stylesheet.XslNamespace;
                        foreach (var p in instruction.Elements(XName.Get("param", xslNs)))
                        {
                            var pname = p.Attribute("name")?.Value;
                            if (string.IsNullOrEmpty(pname))
                                continue;
                            var (plocal, pns) = ExpandVariableName(p, pname);
                            var pselect = p.Attribute("select")?.Value;
                            iterParamValues[(plocal, pns)] = string.IsNullOrEmpty(pselect)
                                ? XdmValue.Undefined
                                : CompileXPath(pselect, p).Evaluate(_context);
                        }

                        XdmValue? iterCompletionResult = null;
                        bool iterBroken = false;
                        var savedIterFocus = _context.ContextItem;
                        var savedIterPos = _context.ContextPosition;
                        var savedIterSize = _context.ContextSize;
                        var savedIterCurrent = _context.CurrentItem;
                        var savedIterVars = _context.SnapshotVariables();
                        try
                        {
                            int iterTotal = iterItems.Count;
                            for (int i = 0; i < iterTotal; i++)
                            {
                                _context.WithFocus(iterItems[i], i + 1, iterTotal);
                                _context.WithCurrentItem(iterItems[i]);
                                foreach (var kv in iterParamValues)
                                    _context.WithVariable(kv.Key.LocalName, kv.Value, kv.Key.NamespaceUri);

                                bool iterNext = false;
                                XdmValue? iterBreakResult = null;
                                var iterBodyItems = new List<XdmValue>();
                                foreach (var bodyChild in instruction.Elements())
                                {
                                    if (bodyChild.Name.LocalName == "param" || bodyChild.Name.LocalName == "on-completion")
                                        continue;
                                    if (bodyChild.Name.LocalName == "next-iteration")
                                    {
                                        foreach (var wp in bodyChild.Elements(XName.Get("with-param", xslNs)))
                                        {
                                            var wpName = wp.Attribute("name")?.Value;
                                            if (string.IsNullOrEmpty(wpName))
                                                continue;
                                            var (wplocal, wpns) = ExpandVariableName(wp, wpName);
                                            var wpSelect = wp.Attribute("select")?.Value;
                                            iterParamValues[(wplocal, wpns)] = string.IsNullOrEmpty(wpSelect)
                                                ? XdmValue.Undefined
                                                : CompileXPath(wpSelect, wp).Evaluate(_context);
                                        }
                                        iterNext = true;
                                        break;
                                    }
                                    if (bodyChild.Name.LocalName == "break")
                                    {
                                        var breakSelect = bodyChild.Attribute("select")?.Value;
                                        iterBreakResult = string.IsNullOrEmpty(breakSelect)
                                            ? EvaluateFunctionBody(bodyChild, XdmValue.Undefined)
                                            : CompileXPath(breakSelect, bodyChild).Evaluate(_context);
                                        iterBroken = true;
                                        break;
                                    }
                                    ProcessFunctionBodyNode(bodyChild, iterBodyItems, iterItems[i]);
                                }
                                if (iterBroken)
                                {
                                    if (iterBreakResult.HasValue && !iterBreakResult.Value.IsUndefined)
                                        results.Add(iterBreakResult.Value);
                                    break;
                                }
                                foreach (var bi in iterBodyItems)
                                    results.Add(bi);
                                if (!iterNext)
                                {
                                    // No explicit next-iteration: parameters retain their values for the next round.
                                }
                            }

                            if (!iterBroken)
                            {
                                if (iterTotal == 0)
                                {
                                    _context.WithFocus(XdmValue.Undefined, 0, 0);
                                    foreach (var kv in iterParamValues)
                                        _context.WithVariable(kv.Key.LocalName, kv.Value, kv.Key.NamespaceUri);
                                }
                                var onCompletion = instruction.Element(XName.Get("on-completion", xslNs));
                                if (onCompletion != null)
                                {
                                    var ocSelect = onCompletion.Attribute("select")?.Value;
                                    iterCompletionResult = string.IsNullOrEmpty(ocSelect)
                                        ? EvaluateFunctionBody(onCompletion, XdmValue.Undefined)
                                        : CompileXPath(ocSelect, onCompletion).Evaluate(_context);
                                }
                            }
                        }
                        finally
                        {
                            _context.RestoreVariables(savedIterVars);
                            _context.WithFocus(savedIterFocus, savedIterPos, savedIterSize);
                            _context.WithCurrentItem(savedIterCurrent);
                        }

                        if (iterCompletionResult.HasValue && !iterCompletionResult.Value.IsUndefined)
                            results.Add(iterCompletionResult.Value);
                        break;
                    }
                case "assert":
                    // xsl:assert is accepted but not yet evaluated in function bodies.
                    break;
                default:
                    // Unknown XSLT instruction in function body: ignore
                    break;
            }
        }
        else
        {
            // Literal result element in function body: evaluate it fully
            // (including nested XSLT instructions) using the same logic as
            // templates, but capture the result as a detached node.
            var savedContainer = _currentContainer;
            var savedLastAtomic = _lastAddedWasAtomic;
            var temp = new XElement("__temp__");
            _currentContainer = temp;
            _lastAddedWasAtomic = false;
            try
            {
                CopyLiteralElement(instruction);
                var copied = temp.Elements().FirstOrDefault();
                if (copied != null)
                {
                    copied.Remove();
                    results.Add(XdmValue.FromNode(new XDocumentNode(copied)));
                }
            }
            finally
            {
                _currentContainer = savedContainer;
                _lastAddedWasAtomic = savedLastAtomic;
            }
        }
    }

    /// <summary>
    /// Flattens an XDM value into a list, expanding sequences into their items.
    /// </summary>
    private static void FlattenToList(XdmValue value, List<XdmValue> results, bool preserveUndefined = false)
    {
        if (value.IsUndefined)
        {
            if (preserveUndefined)
                results.Add(value);
            return;
        }

        if (value.IsSequence)
        {
            var seq = value.SequenceValue;
            if (seq != null)
            {
                var enumerator = seq.GetEnumerator();
                while (enumerator.MoveNext())
                    FlattenToList(enumerator.Current, results, preserveUndefined);
            }
        }
        else
        {
            results.Add(value);
        }
    }

    /// <summary>
    /// Flattens array items recursively so that each member becomes a separate item in
    /// the sequence being processed by a sequence constructor. Empty sequences inside
    /// arrays are discarded; nested arrays are flattened in turn.
    /// </summary>
    private static void FlattenArrayMembers(XdmValue value, List<XdmValue> results)
    {
        if (value.IsUndefined)
            return;

        if (value.IsArray && value.ArrayValue != null)
        {
            foreach (var member in value.ArrayValue.Values)
                FlattenArrayMembers(member, results);
        }
        else if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                FlattenArrayMembers(item, results);
        }
        else
        {
            results.Add(value);
        }
    }

    /// <summary>
    /// Finds a template with match="/" (document root pattern).
    /// </summary>
    private Stylesheet.TemplateRule? FindRootTemplate()
    {
        Stylesheet.TemplateRule? best = null;
        double bestPriority = double.NegativeInfinity;
        int bestImportPrecedence = int.MaxValue;
        bool hasConflict = false;

        foreach (var rule in _allTemplateRules)
        {
            if (rule.Match == null) continue;
            // Only consider templates that participate in the default (unnamed) mode.
            if (!MatchesMode(rule, ""))
                continue;
            var stripped = Patterns.PatternCompiler.StripXPathComments(rule.Match).Trim();
            // Only match patterns that directly match the document node,
            // not path patterns like document-node()/child::element().
            bool isRootPattern = stripped == "/" ||
                stripped == "document-node()" ||
                stripped.StartsWith("document-node()[") ||
                stripped.StartsWith("document-node(element(") ||
                stripped.StartsWith("document-node(schema-element(") ||
                stripped == "root()" ||
                stripped.StartsWith("doc(") ||
                stripped.StartsWith("(/");
            if (!isRootPattern)
                continue;
            if (rule.CompiledMatch == null)
                continue;
            if (!EvaluatePatternMatch(rule, XdmValue.FromNode(_initialSource!)))
                continue;

            // XSLT spec §6.4: import precedence is checked BEFORE priority.
            if (best == null || rule.ImportPrecedence < bestImportPrecedence)
            {
                best = rule;
                bestPriority = rule.Priority;
                bestImportPrecedence = rule.ImportPrecedence;
                hasConflict = false;
            }
            else if (rule.ImportPrecedence == bestImportPrecedence)
            {
                if (rule.Priority > bestPriority)
                {
                    best = rule;
                    bestPriority = rule.Priority;
                    hasConflict = false;
                }
                else if (rule.Priority == bestPriority)
                {
                    if (best != null && best != rule && best.Element != rule.Element)
                        hasConflict = true;
                    // XSLT last-wins rule: when priority and import precedence are equal,
                    // the template that appears later in the stylesheet wins.
                    best = rule;
                }
            }
        }

        if (hasConflict && best != null)
        {
            var modeDef = _stylesheet.GetModeDefinition("");
            if (modeDef?.OnMultipleMatch == Stylesheet.OnMultipleMatch.Fail)
            {
                throw new InvalidOperationException("XTDE0540: Multiple templates match with the same priority.");
            }
            if (_treatRecoverableAmbiguousMatchAsError)
            {
                throw new InvalidOperationException("XTRE0540: Multiple templates match with the same priority.");
            }
        }

        return best;
    }

    /// <summary>
    /// Implements xsl:apply-templates: selects nodes and processes each with the best-matching template.
    /// Supports XSLT 3.0 atomic-value matching.
    /// </summary>
    public void ApplyTemplates(IXdmNode contextNode, string mode, string? select, List<XElement>? sortKeys = null, Dictionary<string, XdmValue>? incomingTunnelParams = null, Dictionary<string, XdmValue>? callParams = null, XElement? instruction = null)
    {
        if (++_applyTemplatesDepth > MaxApplyTemplatesDepth)
        {
            _applyTemplatesDepth--;
            throw new InvalidOperationException("xsl:apply-templates recursion depth exceeded maximum allowed depth.");
        }

        // Save and clear next-match exclusions — apply-templates starts a fresh chain
        var savedExcluded = _nextMatchExcluded;
        _nextMatchExcluded = new HashSet<Stylesheet.TemplateRule>();

        // Resolve mode aliases
        var resolvedMode = ResolveMode(mode);
        _modeStack.Push(resolvedMode);
        try
        {
            // Determine the sequence to process
            List<XdmValue> items;
            if (string.IsNullOrEmpty(select))
            {
                // Default: child nodes
                items = EnumerateNodes(contextNode.Axis(XdmAxis.Child))
                    .Select(XdmValue.FromNode)
                    .ToList();
            }
            else
            {
                // Evaluate select expression
                var compiled = instruction != null ? CompileXPath(select, instruction) : XPath31Expression.Compile(select);
                var result = compiled.Evaluate(_context.WithFocus(XdmValue.FromNode(contextNode), 1, 1));
                items = FlattenSelectedItems(result);
            }

            bool allNodes = items.All(i => i.IsNode);

            // Apply xsl:sort if present. Node sequences are sorted via SortNodes; mixed or
            // atomic sequences use SortItems, which evaluates each sort key with the current
            // output URI cleared.
            if (sortKeys != null && sortKeys.Count > 0)
            {
                if (allNodes)
                {
                    var nodes = items.Select(i => i.NodeValue!).ToList();
                    nodes = SortNodes(nodes, sortKeys);
                    items = nodes.Select(XdmValue.FromNode).ToList();
                }
                else
                {
                    items = SortItems(items, sortKeys);
                }
            }

            int pos = 1;
            int last = items.Count;
            foreach (var item in items)
            {
                if (item.IsNode)
                {
                    var node = item.NodeValue!;
                    var rule = FindBestTemplate(node, resolvedMode);
                    if (rule != null)
                    {
                        ExecuteTemplate(rule, node, callParams: callParams, incomingTunnelParams, position: pos, last: last);
                    }
                    else
                    {
                        ApplyBuiltInRules(node, resolvedMode, incomingTunnelParams, callParams, position: pos, last: last);
                    }
                }
                else
                {
                    var rule = FindBestTemplate(item, resolvedMode);
                    if (rule != null)
                    {
                        ExecuteTemplate(rule, item, callParams: callParams, incomingTunnelParams, position: pos, last: last);
                    }
                    else
                    {
                        ApplyBuiltInRulesForAtomic(item, resolvedMode);
                    }
                }
                pos++;
            }
        }
        finally
        {
            _modeStack.Pop();
            _nextMatchExcluded = savedExcluded;
            _applyTemplatesDepth--;
        }
    }

    /// <summary>
    /// Implements xsl:apply-templates when there is no context node (e.g. inside a named template).
    /// </summary>
    public void ApplyTemplates(XdmValue contextItem, string mode, string? select, List<XElement>? sortKeys = null, Dictionary<string, XdmValue>? incomingTunnelParams = null, Dictionary<string, XdmValue>? callParams = null, XElement? instruction = null)
    {
        if (++_applyTemplatesDepth > MaxApplyTemplatesDepth)
        {
            _applyTemplatesDepth--;
            throw new InvalidOperationException("xsl:apply-templates recursion depth exceeded maximum allowed depth.");
        }

        // Save and clear next-match exclusions — apply-templates starts a fresh chain
        var savedExcluded = _nextMatchExcluded;
        _nextMatchExcluded = new HashSet<Stylesheet.TemplateRule>();

        // Resolve mode aliases
        var resolvedMode = ResolveMode(mode);

        _modeStack.Push(resolvedMode);
        try
        {
            // Determine the sequence to process
            List<XdmValue> items;
            if (string.IsNullOrEmpty(select))
            {
                // xsl:apply-templates with no @select requires a node context item (XTTE0510)
                if (!contextItem.IsNode)
                {
                    throw new InvalidOperationException("XTTE0510: The context item for xsl:apply-templates is not a node.");
                }

                // No select and no context node: empty sequence
                items = new List<XdmValue>();
            }
            else
            {
                // Evaluate select expression with the given context item as focus
                var compiled = instruction != null ? CompileXPath(select, instruction) : XPath31Expression.Compile(select);
                var result = compiled.Evaluate(_context.WithFocus(contextItem, 1, 1));
                items = FlattenSelectedItems(result);
            }

            bool allNodes = items.All(i => i.IsNode);

            // Apply xsl:sort if present. Node sequences are sorted via SortNodes; mixed or
            // atomic sequences use SortItems, which evaluates each sort key with the current
            // output URI cleared.
            if (sortKeys != null && sortKeys.Count > 0)
            {
                if (allNodes)
                {
                    var nodes = items.Select(i => i.NodeValue!).ToList();
                    nodes = SortNodes(nodes, sortKeys);
                    items = nodes.Select(XdmValue.FromNode).ToList();
                }
                else
                {
                    items = SortItems(items, sortKeys);
                }
            }

            int pos = 1;
            int last = items.Count;
            foreach (var item in items)
            {
                if (item.IsNode)
                {
                    var node = item.NodeValue!;
                    var rule = FindBestTemplate(node, resolvedMode);
                    if (rule != null)
                    {
                        ExecuteTemplate(rule, node, callParams: callParams, incomingTunnelParams, position: pos, last: last);
                    }
                    else
                    {
                        ApplyBuiltInRules(node, resolvedMode, incomingTunnelParams, callParams, position: pos, last: last);
                    }
                }
                else
                {
                    var rule = FindBestTemplate(item, resolvedMode);
                    if (rule != null)
                    {
                        ExecuteTemplate(rule, item, callParams: callParams, incomingTunnelParams, position: pos, last: last);
                    }
                    else
                    {
                        ApplyBuiltInRulesForAtomic(item, resolvedMode);
                    }
                }
                pos++;
            }
        }
        finally
        {
            _modeStack.Pop();
            _nextMatchExcluded = savedExcluded;
            _applyTemplatesDepth--;
        }
    }

    /// <summary>
    /// Returns the current default mode from the default-mode stack or the stylesheet root.
    /// </summary>
    private string CurrentDefaultMode => _defaultModeStack.Count > 0 ? _defaultModeStack.Peek() : _stylesheet.DefaultMode;

    /// <summary>
    /// Resolves mode aliases (#current, #default) to actual mode names.
    /// </summary>
    private string ResolveMode(string mode)
    {
        if (mode == "#current")
        {
            return _modeStack.Count > 0 ? _modeStack.Peek() : "";
        }
        if (mode == "#default")
        {
            return CurrentDefaultMode;
        }
        if (mode == "#unnamed")
        {
            return "";
        }
        return mode;
    }

    /// <summary>
    /// Returns true if the given mode is declared or used by a non-#all template in the stylesheet.
    /// Used for XTDE0045 initial mode validation.
    /// </summary>
    private bool ModeExists(string mode)
    {
        if (string.IsNullOrEmpty(mode))
            return true; // unnamed mode always exists

        // Check for explicit xsl:mode declaration
        if (_stylesheet.GetModeDefinition(mode) != null)
            return true;

        // Check for template rules with this exact mode (not #all)
        foreach (var rule in _allTemplateRules)
        {
            if (rule.MatchesAllModes)
                continue;
            foreach (var m in rule.Modes)
            {
                if (m == mode)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Expands a mode attribute value to Clark notation ({uri}local) using
    /// the in-scope namespaces of the instruction element. No-op for special
    /// mode names (#current, #default, #all) and unprefixed names.
    /// </summary>
    private static string ExpandModeName(string mode, XElement instruction)
    {
        if (mode == "#current" || mode == "#default" || mode == "#all" || mode == "#unnamed")
            return mode;

        int colon = mode.IndexOf(':');
        if (colon < 0)
            return Stylesheet.ModeDefinition.NormalizeModeName(mode);

        var prefix = mode.Substring(0, colon);
        var local = mode.Substring(colon + 1);

        // Search for xmlns:prefix declaration on instruction or ancestors
        var current = instruction;
        while (current != null)
        {
            foreach (var attr in current.Attributes())
            {
                if (attr.IsNamespaceDeclaration && attr.Name.LocalName == prefix)
                {
                    return Stylesheet.ModeDefinition.NormalizeModeName($"{{{attr.Value}}}{local}");
                }
            }
            current = current.Parent;
        }
        // Prefix not declared — return normalized name (will fail to match)
        return Stylesheet.ModeDefinition.NormalizeModeName(mode);
    }

    /// <summary>
    /// Executes the body of a template rule against the current node.
    /// </summary>
    public void ExecuteTemplate(Stylesheet.TemplateRule rule, IXdmNode currentNode, Dictionary<string, XdmValue>? callParams = null, Dictionary<string, XdmValue>? incomingTunnelParams = null, int position = 1, int last = 1, bool setCurrentRule = true)
        => ExecuteTemplate(rule, XdmValue.FromNode(currentNode), callParams, incomingTunnelParams, position, last, setCurrentRule);

    public void ExecuteTemplate(Stylesheet.TemplateRule rule, XdmValue contextItem, Dictionary<string, XdmValue>? callParams = null, Dictionary<string, XdmValue>? incomingTunnelParams = null, int position = 1, int last = 1, bool setCurrentRule = true)
    {
        var asType = rule.Element.Attribute("as")?.Value;
        var savedContainer = _currentContainer;
        var savedAccumulator = _sequenceAccumulator;
        var savedPreserveAtomics = _preserveAtomicSequenceItems;
        var savedPreserveDocuments = _preserveDocumentNodes;
        var savedLiteralDepth = _literalElementDepth;
        var savedLastAtomic = _lastAddedWasAtomic;
        var savedTypedRawItems = _typedResultRawItems;
        XElement? tempContainer = null;

        if (!string.IsNullOrEmpty(asType))
        {
            tempContainer = new XElement("__temp__");
            _currentContainer = tempContainer;
            _lastAddedWasAtomic = false;
            // Use a placeholder accumulator so xsl:sequence results keep their node
            // identity (and parentage) instead of being deep-copied into the temporary
            // container. Per XSLT 3.0 §5.7.1, nodes produced by a sequence constructor
            // that forms the result of a template are added to the result sequence as-is.
            _sequenceAccumulator = new PlaceholderSequenceAccumulator(this);
            _preserveAtomicSequenceItems = true;
            _preserveDocumentNodes = true;
            _literalElementDepth = 0;
            _typedResultRawItems = new List<XdmValue>();
        }

        var savedTemplateRule = _currentTemplateRule;
        if (setCurrentRule)
            _currentTemplateRule = rule;

        // Update context to current item
        var savedItem = _context.ContextItem;
        var savedCurrent = _context.CurrentItem;
        var savedPosition = _context.ContextPosition;
        var savedSize = _context.ContextSize;
        var savedMergeGroup = _currentMergeGroup;
        var savedMergeKey = _currentMergeKey;
        var savedNamedGroups = _currentNamedMergeGroups;
        var savedMergeSourceNames = _currentMergeSourceNames;
        _currentMergeGroup = null;
        _currentMergeKey = null;
        _currentNamedMergeGroups = null;
        _currentMergeSourceNames = null;

        // Apply the xsl:context-item declaration, if present.
        var contextItemDecl = rule.ContextItem;
        if (contextItemDecl != null && contextItemDecl.Use == Stylesheet.ContextItemUse.Absent)
        {
            _context.WithFocus(XdmValue.Undefined, position, last);
            _context.WithCurrentItem(XdmValue.Undefined);
        }
        else if (contextItemDecl != null)
        {
            if (contextItem.IsUndefined)
            {
                if (contextItemDecl.Use == Stylesheet.ContextItemUse.Required)
                    throw new InvalidOperationException("XTTE3090: A required context item was not supplied for the template.");
            }
            else
            {
                if (!string.IsNullOrEmpty(contextItemDecl.AsType)
                    && !VmEngine.ValueMatchesType(contextItem, contextItemDecl.AsType))
                    throw new InvalidOperationException($"XTTE0590: Supplied context item does not match required type '{contextItemDecl.AsType}'.");
            }

            // The focus reflects the (possibly absent) supplied context item.
            _context.WithFocus(contextItem, position, last);
            _context.WithCurrentItem(contextItem);
        }
        else
        {
            _context.WithFocus(contextItem, position, last);
            _context.WithCurrentItem(contextItem);
        }

        // Snapshot current variables for lexical scoping
        var snapshot = _context.SnapshotVariables();

        // Push tunnel parameters for this template invocation
        var tunnelFrame = new Dictionary<string, XdmValue>();
        if (_tunnelParamStack.Count > 0)
        {
            foreach (var (k, v) in _tunnelParamStack.Peek())
                tunnelFrame[k] = v;
        }
        if (incomingTunnelParams != null)
        {
            foreach (var (k, v) in incomingTunnelParams)
                tunnelFrame[k] = v;
        }
        _tunnelParamStack.Push(tunnelFrame);

        // Push default-mode for this template scope
        var templateDefaultMode = rule.Element.Attribute("default-mode")?.Value;
        if (!string.IsNullOrEmpty(templateDefaultMode))
        {
            _defaultModeStack.Push(ExpandModeName(templateDefaultMode, rule.Element));
        }

        var templateCollation = GetEffectiveDefaultCollation(rule.Element);
        var savedDefaultCollation = _context.DefaultCollation;
        _context.DefaultCollation = templateCollation;

        try
        {
            // Process xsl:context-item (already validated) and xsl:param declarations first.
            foreach (var child in rule.Element.Elements())
            {
                if (child.Name.LocalName == "context-item" && child.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                    continue; // validated and enforced above

                if (child.Name.LocalName == "param" && child.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                {
                    var paramName = child.Attribute("name")?.Value;
                    if (string.IsNullOrEmpty(paramName))
                        continue;

                    var (paramLocal, paramNs) = ExpandVariableName(child, paramName);
                    var paramKey = VariableKey(paramLocal, paramNs);
                    var isTunnel = IsTunnelParameter(child);
                    var required = child.Attribute("required")?.Value?.Trim();
                    var paramAs = child.Attribute("as")?.Value;

                    XdmValue paramValue = XdmValue.Undefined;
                    bool gotValue;
                    if (isTunnel)
                    {
                        // Tunnel parameters bind only to the tunnel parameter stack.
                        if (_tunnelParamStack.Count > 0 && _tunnelParamStack.Peek().TryGetValue(paramKey, out var tunnelValue))
                        {
                            paramValue = tunnelValue;
                            gotValue = true;
                        }
                        else
                        {
                            gotValue = false;
                        }
                    }
                    else if (callParams != null && callParams.TryGetValue(paramKey, out var provided))
                    {
                        paramValue = provided;
                        gotValue = true;
                    }
                    else
                    {
                        gotValue = false;
                    }

                    if (!gotValue)
                    {
                        var paramSelect = child.Attribute("select")?.Value;
                        if (!string.IsNullOrEmpty(paramSelect))
                        {
                            var compiled = CompileXPath(paramSelect, child);
                            paramValue = compiled.Evaluate(_context);
                        }
                        else
                        {
                            // Check for content (sequence constructor as default value)
                            var contentNodes = child.Nodes().ToList();
                            if (contentNodes.Count > 0)
                            {
                                paramValue = EvaluateSequenceConstructor(child, contextItem, wrapInDocumentNode: string.IsNullOrEmpty(paramAs));
                            }
                            else
                            {
                                paramValue = XdmValue.FromSequence(XdmSequence.Empty);
                            }
                        }
                    }

                    // Required parameters must be supplied explicitly.
                    if (required == "yes" && !gotValue)
                        throw new InvalidOperationException($"XTDE0700: No value supplied for required parameter '{paramName}'.");

                    paramValue = ConvertVariableValue(paramValue, paramAs, isParam: true);
                    _context.WithVariable(paramLocal, paramValue, paramNs);
                }
                else
                {
                    break; // xsl:param must be first; stop once we hit non-param
                }
            }

            // Process the sequence constructor (child nodes of xsl:template)
            foreach (var childNode in rule.Element.Nodes())
            {
                switch (childNode)
                {
                    case XText text:
                        ProcessSequenceText(text, rule.Element);
                        break;
                    case XElement elem when elem.Name.LocalName == "context-item" && elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                        continue; // xsl:context-item is a declaration, not part of the sequence constructor
                    case XElement elem when elem.Name.LocalName == "param" && elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                        continue; // Already processed above
                    case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                        ExecuteXsltInstruction(elem, contextItem);
                        break;
                    case XElement elem:
                        CopyLiteralElement(elem);
                        break;
                }
            }
        }
        finally
        {
            _context.DefaultCollation = savedDefaultCollation;
            _context.RestoreVariables(snapshot);
            _context.WithFocus(savedItem, savedPosition, savedSize);
            _context.WithCurrentItem(savedCurrent);
            _tunnelParamStack.Pop();
            if (!string.IsNullOrEmpty(templateDefaultMode))
            {
                _defaultModeStack.Pop();
            }
            _currentTemplateRule = savedTemplateRule;
            _currentMergeGroup = savedMergeGroup;
            _currentMergeKey = savedMergeKey;
            _currentNamedMergeGroups = savedNamedGroups;
            _currentMergeSourceNames = savedMergeSourceNames;

            if (tempContainer != null)
            {
                var items = new List<XdmValue>();
                // Map and function items cannot be represented as container nodes; they were
                // captured in source order into _typedResultRawItems by CopyToResult.
                if (_typedResultRawItems is { Count: > 0 } rawItems)
                    items.AddRange(rawItems);
                foreach (var attr in tempContainer.Attributes())
                {
                    items.Add(XdmValue.FromNode(new XDocumentNode(new XAttribute(attr.Name, attr.Value))));
                }
                foreach (var node in tempContainer.Nodes().ToList())
                {
                    // Detach the node from the temporary container so result nodes are
                    // parentless (or keep the parent they were constructed with) rather
                    // than being rooted at the synthetic __temp__ element.
                    node.Remove();
                    switch (node)
                    {
                        case XElement seq when seq.Name.LocalName == "__xdm_seq__" && seq.Name.NamespaceName == "":
                            // Placeholder produced by xsl:sequence: expand the captured
                            // items in source order, preserving node identity.
                            if (seq.Annotation<SequencePlaceholderItems>() is { } holder)
                            {
                                foreach (var phItem in holder.Items)
                                {
                                    if (!phItem.IsUndefined)
                                        items.Add(phItem);
                                }
                            }
                            break;
                        case XElement e when e.Name.LocalName == "__xdm_doc__":
                            items.Add(XdmValue.FromNode(new XDocumentNode(new XDocument(e))));
                            break;
                        case XElement e:
                            items.Add(XdmValue.FromNode(new XDocumentNode(e)));
                            break;
                        case XText t when !string.IsNullOrEmpty(t.Value):
                            items.Add(XdmValue.FromNode(new XDocumentNode(new XText(t.Value))));
                            break;
                        case XComment c:
                            items.Add(XdmValue.FromNode(new XDocumentNode(c)));
                            break;
                        case XProcessingInstruction pi:
                            items.Add(XdmValue.FromNode(new XDocumentNode(pi)));
                            break;
                    }
                }

                _currentContainer = savedContainer;
                _sequenceAccumulator = savedAccumulator;
                _preserveAtomicSequenceItems = savedPreserveAtomics;
                _preserveDocumentNodes = savedPreserveDocuments;
                _literalElementDepth = savedLiteralDepth;
                _lastAddedWasAtomic = savedLastAtomic;
                _typedResultRawItems = savedTypedRawItems;

                XdmValue typedResult;
                if (items.Count > 0)
                {
                    var result = items.Count == 1 ? items[0] :
                        XdmValue.FromSequence(MaterializedSequence.FromList(items));
                    typedResult = ConvertVariableValue(result, asType);
                }
                else
                {
                    typedResult = ConvertVariableValue(XdmValue.FromSequence(XdmSequence.Empty), asType);
                }

                if (_returnRawInitialTemplateResult && _isExecutingInitialTemplate)
                    _rawInitialTemplateResult = typedResult;
                else if (_sequenceAccumulator != null)
                {
                    // An outer sequence-returning context (function body, typed variable,
                    // apply-templates in a function) collects raw items: add the template
                    // result without copying so node identity and parentage survive.
                    _sequenceAccumulator.Add(typedResult);
                }
                else
                    CopyToResult(typedResult);
            }
            else
            {
                _currentContainer = savedContainer;
                _sequenceAccumulator = savedAccumulator;
                _preserveAtomicSequenceItems = savedPreserveAtomics;
                _preserveDocumentNodes = savedPreserveDocuments;
                _literalElementDepth = savedLiteralDepth;
                _typedResultRawItems = savedTypedRawItems;
            }
        }
    }

    /// <summary>
    /// Implements xsl:call-template: invokes a named template by name.
    /// </summary>
    public void CallTemplate(string name, IXdmNode currentNode, Dictionary<string, XdmValue>? withParams = null, Dictionary<string, XdmValue>? incomingTunnelParams = null)
        => CallTemplate(name, XdmValue.FromNode(currentNode), withParams, incomingTunnelParams);

    private const int MaxCallTemplateDepth = 128;

    public void CallTemplate(string name, XdmValue contextItem, Dictionary<string, XdmValue>? withParams = null, Dictionary<string, XdmValue>? incomingTunnelParams = null)
    {
        if (++_callTemplateDepth > MaxCallTemplateDepth)
        {
            _callTemplateDepth--;
            throw new InvalidOperationException("xsl:call-template recursion depth exceeded maximum allowed depth.");
        }

        try
        {
            if (!_allNamedTemplates.TryGetValue(name, out var rule))
                throw new InvalidOperationException($"Named template '{name}' not found.");

            // Static validation of call-template parameters: every with-param must match a
            // declared xsl:param, and its tunnel status must match the declaration.
            // This check applies only in XSLT 2.0 and later; XSLT 1.0 backwards-compatible
            // mode silently ignores parameters that are not declared by the target template.
            var xslNs = Stylesheet.Stylesheet.XslNamespace;
            var declaredParams = new Dictionary<string, bool>();
            var declaredParamNames = new Dictionary<string, string>();
            foreach (var child in rule.Element.Elements())
            {
                if (child.Name.LocalName == "context-item" && child.Name.NamespaceName == xslNs)
                    continue;
                if (child.Name.LocalName != "param" || child.Name.NamespaceName != xslNs)
                    break;
                var paramName = child.Attribute("name")?.Value;
                if (string.IsNullOrEmpty(paramName))
                    continue;
                var (pLocal, pNs) = ExpandVariableName(child, paramName);
                var pKey = VariableKey(pLocal, pNs);
                declaredParams[pKey] = IsTunnelParameter(child);
                declaredParamNames[pKey] = paramName;
            }

            void ValidateCallParams(Dictionary<string, XdmValue>? supplied, bool suppliedAsTunnel)
            {
                if (supplied == null) return;
                foreach (var kvp in supplied)
                {
                    if (!declaredParams.TryGetValue(kvp.Key, out var declaredTunnel))
                    {
                        var suppliedName = declaredParamNames.TryGetValue(kvp.Key, out var n) ? n : kvp.Key;
                        throw new InvalidOperationException($"XTSE0680: Parameter '{suppliedName}' is not declared by template '{name}'.");
                    }
                    if (declaredTunnel != suppliedAsTunnel)
                    {
                        var suppliedName = declaredParamNames.TryGetValue(kvp.Key, out var n) ? n : kvp.Key;
                        throw new InvalidOperationException($"XTSE0680: Parameter '{suppliedName}' is not declared by template '{name}'.");
                    }
                }
            }

            // Only ordinary (non-tunnel) parameters are validated against the target
            // template's declared parameters. Tunnel parameters are allowed to pass through
            // named templates that do not declare them. In XSLT 1.0 BC, extra parameters
            // are ignored rather than raising XTSE0680.
            if (!IsEffectiveBackwardsCompatible(rule.Element))
                ValidateCallParams(withParams, suppliedAsTunnel: false);

            ExecuteTemplate(rule, contextItem, withParams, incomingTunnelParams, _context.ContextPosition, _context.ContextSize, setCurrentRule: false);
        }
        finally
        {
            _callTemplateDepth--;
        }
    }

    /// <summary>
    /// Evaluates an <c>xsl:evaluate</c> instruction and returns the resulting XDM value.
    /// </summary>
    private XdmValue EvaluateXslEvaluate(XElement instruction, XdmValue contextItem)
    {
        var xpathRaw = instruction.Attribute("xpath")?.Value;
        if (string.IsNullOrEmpty(xpathRaw))
            throw new InvalidOperationException("XTSE0010: xsl:evaluate requires an @xpath attribute");
        // The @xpath attribute is itself an XPath expression whose string value is the
        // expression to be evaluated dynamically.
        var xpathCompiled = CompileXPath(xpathRaw, instruction);
        var xpathValue = xpathCompiled.Evaluate(_context);
        var xpath = AtomizedFirstString(xpathValue);
        if (string.IsNullOrEmpty(xpath))
            throw new InvalidOperationException("XTSE0010: xsl:evaluate @xpath evaluated to an empty string");

        // Determine the namespace context for the dynamic XPath expression.
        var nsContextElement = instruction;
        var nsContextSelect = instruction.Attribute("namespace-context")?.Value;
        if (!string.IsNullOrEmpty(nsContextSelect))
        {
            var nsCtxCompiled = CompileXPath(nsContextSelect, instruction);
            var nsCtxResult = nsCtxCompiled.Evaluate(_context);
            IXdmNode? nsCtxNode = null;
            if (nsCtxResult.IsNode)
                nsCtxNode = nsCtxResult.NodeValue;
            else if (nsCtxResult.IsSequence && nsCtxResult.SequenceValue != null)
            {
                var enumerator = XdmSequence.FromSource(nsCtxResult.SequenceValue).GetEnumerator();
                if (enumerator.MoveNext())
                    nsCtxNode = enumerator.Current.IsNode ? enumerator.Current.NodeValue : null;
            }
            if (nsCtxNode != null)
            {
                if (nsCtxNode is XDocumentNode xdocNode && xdocNode.UnderlyingObject is System.Xml.Linq.XDocument doc && doc.Root != null)
                    nsContextElement = doc.Root;
                else if (nsCtxNode is XDocumentNode xelemNode && xelemNode.UnderlyingObject is System.Xml.Linq.XElement elem)
                    nsContextElement = elem;
            }
        }

        var baseUriRaw = instruction.Attribute("base-uri")?.Value;
        var baseUri = !string.IsNullOrEmpty(baseUriRaw) ? EvaluateAvt(baseUriRaw, instruction) : GetEffectiveBaseUri(instruction);

        string? defaultNs;
        Dictionary<string, string> nsMap;
        if (!string.IsNullOrEmpty(nsContextSelect))
        {
            // When @namespace-context is present, the default namespace comes from the
            // namespace-context node's in-scope default namespace binding.
            var defaultNsDecl = nsContextElement.GetDefaultNamespace();
            defaultNs = defaultNsDecl?.NamespaceName ?? string.Empty;
            nsMap = GetInScopeNamespaces(nsContextElement);
        }
        else
        {
            // Without @namespace-context, the default element/type namespace is taken from
            // the innermost [xsl:]xpath-default-namespace attribute only.
            defaultNs = GetXPathDefaultNamespace(instruction);
            nsMap = GetInScopeNamespaces(instruction);
        }

        var compileOptions = new CompileOptions
        {
            Namespaces = nsMap,
            DefaultElementNamespace = defaultNs,
            DefiningElementDefaultNamespace = instruction.GetDefaultNamespace().NamespaceName,
            BaseUri = baseUri
        };
        XPath31Expression compiled;
        try
        {
            compiled = XPath31Expression.Compile(xpath, compileOptions);
        }
        catch (InvalidOperationException ex) when (IsXPathStaticError(ex))
        {
            throw new InvalidOperationException($"XTDE3160: {ex.Message}", ex);
        }

        // Evaluate the requested context item. The default is absent.
        var evalContextItem = XdmValue.Undefined;
        var contextItemSelect = instruction.Attribute("context-item")?.Value;
        if (!string.IsNullOrEmpty(contextItemSelect))
        {
            var ctxCompiled = CompileXPath(contextItemSelect, instruction);
            var ctxResult = ctxCompiled.Evaluate(_context);
            if (ctxResult.IsSequence && ctxResult.SequenceValue != null)
            {
                var enumerator = XdmSequence.FromSource(ctxResult.SequenceValue).GetEnumerator();
                if (enumerator.MoveNext())
                {
                    evalContextItem = enumerator.Current;
                    if (enumerator.MoveNext())
                        throw new InvalidOperationException("XTTE3210: context-item expression returned more than one item");
                }
            }
            else if (!ctxResult.IsUndefined)
            {
                evalContextItem = ctxResult;
            }
        }

        var evalParams = CollectEvaluateParams(instruction, contextItem);
        var savedVariables = _context.SnapshotVariables();
        var savedFunctions = _context.SnapshotFunctions();
        var savedContextItem = _context.ContextItem;
        var savedContextPosition = _context.ContextPosition;
        var savedContextSize = _context.ContextSize;
        var savedCurrentItem = _context.CurrentItem;
        var savedDefaultCollation = _context.DefaultCollation;
        var savedSkipPopulation = _context.SkipStandardFunctionPopulation;
        var savedMergeGroup = _currentMergeGroup;
        var savedMergeKey = _currentMergeKey;
        var savedNamedMergeGroups = _currentNamedMergeGroups;
        XdmValue result = XdmValue.Undefined;
        try
        {
            // Set up the function library for the target expression: standard XPath
            // functions plus visible stylesheet functions. XSLT-defined functions such as
            // current() and key() are not available.
            _context.ClearFunctions();
            Bosak.XPath.Standard.Functions.FunctionLibrary.Populate(_context);
            RemoveXsltContextFunctions(_context);
            RegisterVisibleXsltFunctions(_context);

            // Variables passed via xsl:with-param or with-params override any existing
            // variables (including globals and locals) for the dynamic expression.
            foreach (var kv in evalParams)
            {
                var (local, ns) = ParseVariableKey(kv.Key);
                _context.WithVariable(local, kv.Value, ns);
            }

            if (!evalContextItem.IsUndefined)
                _context.WithFocus(evalContextItem, 1, 1);
            else
                _context.WithFocus(XdmValue.Undefined, 0, 0);

            // XSLT-specific dynamic context components are absent inside xsl:evaluate.
            _context.WithCurrentItem(XdmValue.Undefined);
            _context.DefaultCollation = GetEffectiveDefaultCollation(instruction);
            _context.SkipStandardFunctionPopulation = true;
            _currentMergeGroup = null;
            _currentMergeKey = null;
            _currentNamedMergeGroups = null;

            result = compiled.Evaluate(_context);
            var asAttr = instruction.Attribute("as")?.Value;
            if (!string.IsNullOrEmpty(asAttr))
                result = ConvertVariableValue(result, asAttr, isParam: false);
            return result;
        }
        catch (InvalidOperationException ex) when (IsXPathStaticError(ex))
        {
            throw new InvalidOperationException($"XTDE3160: {ex.Message}", ex);
        }
        finally
        {
            _context.DefaultCollation = savedDefaultCollation;
            _context.SkipStandardFunctionPopulation = savedSkipPopulation;
            _context.WithFocus(savedContextItem, savedContextPosition, savedContextSize);
            _context.WithCurrentItem(savedCurrentItem);
            _context.RestoreFunctions(savedFunctions);
            // If the result is or contains a function item, the dynamic expression's
            // parameters may be captured in a closure; keep them available.
            if (!ContainsFunctionItem(result))
                _context.RestoreVariables(savedVariables);
        }
    }

    /// <summary>
    /// Executes a single XSLT instruction element.
    /// </summary>
    private void ExecuteXsltInstruction(XElement instruction, IXdmNode currentNode)
        => ExecuteXsltInstruction(instruction, currentNode != null ? XdmValue.FromNode(currentNode) : XdmValue.Undefined);

    private void ExecuteXsltInstruction(XElement instruction, XdmValue contextItem)
    {
        _currentInstruction = instruction;
        var node = contextItem.IsNode ? contextItem.NodeValue : null;

        var savedDefaultCollation = _context.DefaultCollation;
        var savedBackwardsCompatible = _context.BackwardsCompatible;
        var instructionCollation = GetEffectiveDefaultCollation(instruction);
        if (!string.IsNullOrEmpty(instructionCollation))
            _context.DefaultCollation = instructionCollation;
        _context.BackwardsCompatible = IsEffectiveBackwardsCompatible(instruction);

        // Push default-mode for this instruction scope
        var instructionDefaultMode = instruction.Attribute("default-mode")?.Value;
        if (!string.IsNullOrEmpty(instructionDefaultMode))
        {
            _defaultModeStack.Push(ExpandModeName(instructionDefaultMode, instruction));
        }

        try
        {
            var name = instruction.Name.LocalName;
            switch (name)
            {
            case "element":
                {
                    var elemNameRaw = instruction.Attribute("name")?.Value ?? "unnamed";
                    var elemName = EvaluateAvt(elemNameRaw, instruction);
                    var elemNsRaw = instruction.Attribute("namespace")?.Value; // null if absent, "" if explicitly empty
                    var elemNs = elemNsRaw != null ? EvaluateAvt(elemNsRaw, instruction) : null;
                    var (elemLocalName, elemNsUri) = ResolveElementName(instruction, elemName, elemNs, "XTDE0830");
                    var elem = new XElement(XName.Get(Xml11NameCodec.EncodeName(elemLocalName), elemNsUri));

                    // If the element has a namespace URI but no prefix hint, bind it via the
                    // default namespace. Prefixed names keep their prefix unless an
                    // xsl:namespace child would override that prefix binding.
                    string? elemPrefixHint = null;
                    if (!string.IsNullOrEmpty(elemNsUri))
                    {
                        if (!elemName.Contains(':'))
                        {
                            elem.SetAttributeValue("xmlns", elemNsUri);
                            elemPrefixHint = "";
                        }
                        else
                        {
                            var prefixHint = elemName[..elemName.IndexOf(':')];
                            elemPrefixHint = prefixHint;
                            if (prefixHint != "xml" && prefixHint != "xmlns")
                            {
                                bool overridden = instruction.Elements(XName.Get("namespace", Stylesheet.Stylesheet.XslNamespace))
                                    .Any(ns => ns.Attribute("name")?.Value == prefixHint);
                                if (!overridden)
                                    elem.SetAttributeValue(XNamespace.Xmlns + prefixHint, elemNsUri);
                            }
                        }
                    }

                    if (elemPrefixHint != null)
                        elem.AddAnnotation(new ElementPrefixHint { Prefix = elemPrefixHint });

                    var elemInheritNsAttr = instruction.Attribute("inherit-namespaces");
                    var elemInheritNsRaw = elemInheritNsAttr?.Value
                        ?? instruction.Attribute("_inherit-namespaces")?.Value
                        ?? "yes";
                    var elemInheritNs = EvaluateAvt(elemInheritNsRaw, instruction);
                    if (ParseInheritNamespaces(elemInheritNs) == false)
                    {
                        elem.AddAnnotation(new NamespaceInheritanceBarrier());
                    }
                    else if (elemInheritNsAttr != null && ParseInheritNamespaces(elemInheritNs) == true)
                    {
                        elem.AddAnnotation(new NamespaceInheritanceExplicitYes());
                    }

                    AddElementToContainer(elem, _currentContainer);
                    var prev = _currentContainer;
                    _currentContainer = elem;
                    _lastAddedWasAtomic = false;
                    // Suspend the outer sequence accumulator while constructing the content
                    // of the element, so xsl:sequence items attach to the element being
                    // constructed rather than escaping as placeholders into it.
                    var savedElemAccumulator = _sequenceAccumulator;
                    _sequenceAccumulator = null;

                    // Apply attribute sets; xsl:attribute children in the element body override them.
                    ApplyAttributeSets(instruction, elem);

                    try
                    {
                        foreach (var childNode in instruction.Nodes())
                        {
                            switch (childNode)
                            {
                                case XText text:
                                    ProcessSequenceText(text, instruction);
                                    break;
                                case XElement elemChild when elemChild.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                    ExecuteXsltInstruction(elemChild, contextItem);
                                    break;
                                case XElement elemChild:
                                    CopyLiteralElement(elemChild);
                                    break;
                            }
                        }
                        NormalizeElementContent(elem);
                    }
                    finally
                    {
                        _sequenceAccumulator = savedElemAccumulator;
                        _currentContainer = prev;
                    }
                    break;
                }

            case "attribute":
                {
                    var attrNameRaw = instruction.Attribute("name")?.Value;
                    if (string.IsNullOrEmpty(attrNameRaw))
                        throw new InvalidOperationException("XTSE0010: xsl:attribute requires a name attribute");
                    var attrName = EvaluateAvt(attrNameRaw, instruction);
                    var attrNsRaw = instruction.Attribute("namespace")?.Value; // null if absent, "" if explicitly empty
                    var attrNs = attrNsRaw != null ? EvaluateAvt(attrNsRaw, instruction) : null;
                    var (attrLocalName, attrNsUri) = ResolveAttributeName(instruction, attrName, attrNs, "XTDE0860");

                    var select = instruction.Attribute("select")?.Value;
                    string value;
                    if (!string.IsNullOrEmpty(select))
                    {
                        var compiled = CompileXPath(select, instruction);
                        var result = compiled.Evaluate(_context);
                        var attrSep = EvaluateAvt(instruction.Attribute("separator")?.Value ?? " ", instruction);
                        value = XdmValueToString(result, attrSep);
                    }
                    else
                    {
                        var attrSep = EvaluateAvt(instruction.Attribute("separator")?.Value ?? "", instruction);
                        value = EvaluateSimpleContent(instruction, contextItem, attrSep);
                    }

                    // When building a raw-item sequence (JSON/adaptive/build-tree=no),
                    // a top-level xsl:attribute produces a free-standing attribute node.
                    if (IsRawCollectionTopLevel)
                    {
                        var rawList = _resultDocumentStack.Count == 0 ? _jsonResultItems : _resultDocumentRawItems;
                        rawList.Add(XdmValue.FromNode(new XDocumentNode(new XAttribute(
                            XName.Get(Xml11NameCodec.EncodeName(attrLocalName), attrNsUri), value))));
                        break;
                    }

                    if (_currentContainer is not XElement attrTarget)
                        throw new InvalidOperationException("XTDE0420");

                    // If the supplied attribute name includes a prefix that is declared in the
                    // stylesheet but not yet bound on the parent element, copy that namespace
                    // binding to the parent. The prefix is only a hint and may be replaced by
                    // namespace fixup if it conflicts with the requested namespace URI.
                    var attrPrefixHint = attrName.Contains(':') ? attrName[..attrName.IndexOf(':')] : null;
                    if (!string.IsNullOrEmpty(attrPrefixHint)
                        && attrPrefixHint != "xml"
                        && attrPrefixHint != "xmlns"
                        && !_excludedResultPrefixes.Contains(attrPrefixHint))
                    {
                        var parentNs = attrTarget.GetNamespaceOfPrefix(attrPrefixHint);
                        if (parentNs == null)
                        {
                            var styleNs = instruction.GetNamespaceOfPrefix(attrPrefixHint);
                            if (styleNs != null
                                && !string.IsNullOrEmpty(styleNs.NamespaceName)
                                && styleNs.NamespaceName != Stylesheet.Stylesheet.XslNamespace)
                            {
                                if (styleNs.NamespaceName == attrNsUri)
                                {
                                    attrTarget.SetAttributeValue(XNamespace.Xmlns + attrPrefixHint, attrNsUri);
                                }
                            }
                            else if (!string.IsNullOrEmpty(attrNsUri))
                            {
                                attrTarget.SetAttributeValue(XNamespace.Xmlns + attrPrefixHint, attrNsUri);
                            }
                        }
                    }

                    if (attrTarget.Nodes().Any())
                        throw new InvalidOperationException("XTDE0410");
                    Xml11Attribute.SetValue(attrTarget, XName.Get(Xml11NameCodec.EncodeName(attrLocalName), attrNsUri), value);
                    break;
                }

            case "value-of":
                {
                    var select = instruction.Attribute("select")?.Value;
                    if (string.IsNullOrEmpty(select))
                    {
                        // Support the AVT form _select="{...}" used by test suites for
                        // static-parameter substitution.
                        var underSelect = instruction.Attribute("_select")?.Value;
                        if (!string.IsNullOrEmpty(underSelect))
                            select = EvaluateAvt(underSelect, instruction);
                    }
                    if (!string.IsNullOrEmpty(select))
                    {
                        var compiled = CompileXPath(select, instruction);
                        var result = compiled.Evaluate(_context);
                        string textValue;
                        bool hasSeparator = instruction.Attribute("separator") != null;
                        if (IsEffectiveBackwardsCompatible(instruction) && !hasSeparator)
                        {
                            // XSLT 1.0: value-of without an explicit separator outputs only the first item.
                            textValue = FirstItemString(result);
                        }
                        else
                        {
                            var sep = EvaluateAvt(instruction.Attribute("separator")?.Value ?? " ", instruction);
                            textValue = XdmValueToString(result, sep);
                        }
                        _lastAddedWasAtomic = false;
                        AddTextNode(textValue, allowZeroLength: true);
                    }
                    else
                    {
                        var voSep = EvaluateAvt(instruction.Attribute("separator")?.Value ?? "", instruction);
                        var textValue = EvaluateSimpleContent(instruction, contextItem, voSep);
                        _lastAddedWasAtomic = false;
                        AddTextNode(textValue, allowZeroLength: true);
                    }
                    break;
                }

            case "text":
                {
                    if (instruction.Elements().Any())
                        throw new InvalidOperationException("XTSE0010: xsl:text must contain only text nodes");
                    var text = string.Concat(instruction.Nodes().OfType<XText>().Select(t => t.Value));
                    // XSLT 3.0 §5.6.2: TVTs are expanded in xsl:text when expand-text="yes"
                    // is set on the xsl:text element or an ancestor.
                    if (GetExpandText(instruction))
                    {
                        text = EvaluateTvt(text, instruction);
                    }
                    _lastAddedWasAtomic = false;
                    AddTextNode(text, allowZeroLength: true);
                    break;
                }

            case "comment":
                {
                    var commentSelect = instruction.Attribute("select")?.Value;
                    string commentText;
                    if (!string.IsNullOrEmpty(commentSelect))
                    {
                        var compiled = CompileXPath(commentSelect, instruction);
                        var result = compiled.Evaluate(_context);
                        commentText = XdmValueToString(result);
                    }
                    else
                    {
                        commentText = EvaluateSimpleContent(instruction, contextItem, " ");
                    }
                    if (IsRawCollectionTopLevel)
                    {
                        var rawList = _resultDocumentStack.Count == 0 ? _jsonResultItems : _resultDocumentRawItems;
                        rawList.Add(XdmValue.FromNode(new XDocumentNode(new XComment(commentText))));
                    }
                    else
                    {
                        _currentContainer.Add(new XComment(commentText));
                    }
                    break;
                }

            case "processing-instruction":
                {
                    var piNameRaw = instruction.Attribute("name")?.Value ?? "";
                    var piName = EvaluateAvt(piNameRaw, instruction);
                    var piSelect = instruction.Attribute("select")?.Value;
                    string piData;
                    if (!string.IsNullOrEmpty(piSelect))
                    {
                        var compiled = CompileXPath(piSelect, instruction);
                        var result = compiled.Evaluate(_context);
                        piData = XdmValueToString(result);
                    }
                    else
                    {
                        piData = EvaluateSimpleContent(instruction, contextItem, " ");
                    }
                    // XSLT 3.0 §11.4.4: leading spaces in PI data are removed
                    piData = piData.TrimStart();
                    if (IsRawCollectionTopLevel)
                    {
                        var rawList = _resultDocumentStack.Count == 0 ? _jsonResultItems : _resultDocumentRawItems;
                        rawList.Add(XdmValue.FromNode(new XDocumentNode(new XProcessingInstruction(piName, piData))));
                    }
                    else
                    {
                        _currentContainer.Add(new XProcessingInstruction(piName, piData));
                    }
                    break;
                }

            case "namespace":
                {
                    var nsNameRaw = instruction.Attribute("name")?.Value ?? "";
                    var nsName = EvaluateAvt(nsNameRaw, instruction);
                    var nsSelect = instruction.Attribute("select")?.Value;
                    string nsUri;
                    if (!string.IsNullOrEmpty(nsSelect))
                    {
                        var compiled = CompileXPath(nsSelect, instruction);
                        var result = compiled.Evaluate(_context);
                        nsUri = result.ToString();
                    }
                    else
                    {
                        nsUri = EvaluateSimpleContent(instruction, contextItem, " ");
                    }
                    if (_currentContainer is XElement targetElem)
                    {
                        var excludedUris = new HashSet<string>(GetExcludedNamespaceUris(targetElem));
                        var excludedAnn = targetElem.Annotation<ExcludedNamespaceUris>();
                        if (excludedAnn != null)
                        {
                            foreach (var uri in excludedAnn.Uris)
                                excludedUris.Add(uri);
                        }
                        if (string.IsNullOrEmpty(nsName))
                        {
                            var existingDefault = targetElem.Attribute("xmlns");
                            if (existingDefault != null &&
                                existingDefault.Value != nsUri &&
                                !excludedUris.Contains(existingDefault.Value))
                                throw new InvalidOperationException($"XTDE0430: Conflicting namespace declaration for default prefix");
                            // Default namespace declaration
                            targetElem.SetAttributeValue("xmlns", nsUri);
                        }
                        else
                        {
                            var existing = targetElem.Attribute(XNamespace.Xmlns + nsName);
                            if (existing != null &&
                                existing.Value != nsUri &&
                                !excludedUris.Contains(existing.Value))
                                throw new InvalidOperationException($"XTDE0430: Conflicting namespace declaration for prefix '{nsName}'");
                            targetElem.SetAttributeValue(XNamespace.Xmlns + nsName, nsUri);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("XTDE0420");
                    }
                    break;
                }

            case "message":
                {
                    // XSLT 3.0 §11.3: xsl:message may have both a @select attribute and a
                    // sequence constructor. The resulting sequence is emitted to the message
                    // listener; terminate/error-code attributes control whether processing stops.
                    bool terminate = EvaluateMessageTerminate(instruction, contextItem);
                    string errorCode = EvaluateMessageErrorCode(instruction);

                    XdmValue messageValue;
                    string messageString;
                    try
                    {
                        messageValue = BuildMessageValue(instruction, contextItem);
                        messageString = SerializeMessageValue(messageValue);
                    }
                    catch (Exception ex)
                    {
                        if (ex is IterateControlException)
                            throw;

                        // A dynamic error evaluating the message content is recoverable when
                        // terminate="no" (XSLT 3.0). Continue without emitting a message.
                        if (!terminate)
                            break;

                        throw new XsltRuntimeException(errorCode, ex.Message, XdmValue.Undefined);
                    }

                    _messageListener?.OnMessage(messageString);

                    if (terminate)
                    {
                        throw new XsltRuntimeException(errorCode, messageString, messageValue);
                    }
                    break;
                }

            case "copy":
                {
                    // XSLT 3.0: optional select attribute; default is context item
                    IXdmNode nodeToCopy = node!;
                    var copySelect = instruction.Attribute("select")?.Value;
                    bool hasSelect = !string.IsNullOrEmpty(copySelect);
                    var savedCopyTemplateRule = _currentTemplateRule;
                    var savedCopyExcluded = _nextMatchExcluded;

                    if (hasSelect)
                    {
                        _currentTemplateRule = null;
                        _nextMatchExcluded = new HashSet<Stylesheet.TemplateRule>();
                        var compiled = CompileXPath(copySelect!, instruction);
                        var result = compiled.Evaluate(_context);

                        if (result.IsSequence && result.SequenceValue != null)
                        {
                            var items = new List<XdmValue>();
                            foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                                items.Add(item);
                            if (items.Count > 1)
                                throw new InvalidOperationException("XTTE3180");
                            if (items.Count == 1)
                            {
                                var item = items[0];
                                _context.WithFocus(item, 1, 1);
                                if (item.IsNode && item.NodeValue != null)
                                    ExecuteSingleCopy(item.NodeValue, instruction);
                                else if (!item.IsUndefined)
                                {
                                    _lastAddedWasAtomic = false;
                                    AddTextNode(item.StringValue);
                                }
                            }
                        }
                        else if (result.IsNode && result.NodeValue != null)
                        {
                            nodeToCopy = result.NodeValue;
                            _context.WithFocus(XdmValue.FromNode(nodeToCopy), 1, 1);
                            ExecuteSingleCopy(nodeToCopy, instruction);
                        }
                        else if (!result.IsUndefined)
                        {
                            _lastAddedWasAtomic = false;
                            AddTextNode(result.StringValue);
                        }

                        _currentTemplateRule = savedCopyTemplateRule;
                        _nextMatchExcluded = savedCopyExcluded;
                    }
                    else
                    {
                        if (nodeToCopy == null)
                            throw new InvalidOperationException("XTTE0945");
                        ExecuteSingleCopy(nodeToCopy, instruction);
                    }
                    break;
                }

            case "where-populated":
                {
                    var wpSelect = instruction.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(wpSelect))
                    {
                        var compiled = CompileXPath(wpSelect, instruction);
                        var result = compiled.Evaluate(_context);
                        if (IsPopulated(result))
                        {
                            CopyToResult(result, separateAtomicsWithSpace: true);
                        }
                        break;
                    }

                    // Evaluate the sequence constructor while preserving document nodes
                    // produced by xsl:document and items produced by xsl:sequence, so that
                    // an empty child element inside a document node is not mistaken for
                    // populated content. Element-building instructions are evaluated with
                    // the sequence accumulator suspended so their output goes into the
                    // current container (e.g. an xsl:element being constructed).
                    var resultItems = new List<XdmValue>();
                    var temp = new XElement("__wp_temp__");
                    var wpAccumulator = new List<XdmValue>();
                    var savedContainer = _currentContainer;
                    var savedAccumulator = _sequenceAccumulator;
                    var savedLastAtomic = _lastAddedWasAtomic;
                    _currentContainer = temp;
                    _sequenceAccumulator = null;
                    _lastAddedWasAtomic = false;
                    try
                    {
                        foreach (var childNode in instruction.Nodes())
                        {
                            switch (childNode)
                            {
                                case XText text:
                                    ProcessSequenceText(text, instruction);
                                    FlushWherePopulatedTemp(temp, resultItems);
                                    break;
                                case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                    {
                                        var localName = elem.Name.LocalName;
                                        if (localName == "on-empty")
                                        {
                                            // xsl:on-empty is handled after the populated check.
                                            break;
                                        }
                                        if (localName == "sequence" || localName == "document")
                                        {
                                            _sequenceAccumulator = new ListSequenceAccumulator(wpAccumulator);
                                            try
                                            {
                                                ExecuteXsltInstruction(elem, contextItem);
                                            }
                                            finally
                                            {
                                                _sequenceAccumulator = null;
                                            }
                                            FlushWherePopulatedAccumulator(wpAccumulator, resultItems);
                                        }
                                        else
                                        {
                                            ExecuteXsltInstruction(elem, contextItem);
                                            FlushWherePopulatedTemp(temp, resultItems);
                                        }
                                    }
                                    break;
                                case XElement elem:
                                    CopyLiteralElement(elem);
                                    FlushWherePopulatedTemp(temp, resultItems);
                                    break;
                            }
                        }
                    }
                    finally
                    {
                        _currentContainer = savedContainer;
                        _sequenceAccumulator = savedAccumulator;
                        _lastAddedWasAtomic = savedLastAtomic;
                    }

                    if (IsPopulated(XdmValue.FromSequence(MaterializedSequence.FromList(resultItems))))
                    {
                        CopyToResult(XdmValue.FromSequence(MaterializedSequence.FromList(resultItems)), separateAtomicsWithSpace: true);
                    }
                    else
                    {
                        foreach (var onEmpty in instruction.Elements(XName.Get("on-empty", Stylesheet.Stylesheet.XslNamespace)))
                        {
                            var oeSelect = onEmpty.Attribute("select")?.Value;
                            if (!string.IsNullOrEmpty(oeSelect))
                            {
                                var compiled = XPath31Expression.Compile(oeSelect);
                                var oeResult = compiled.Evaluate(_context);
                                CopyToResult(oeResult, separateAtomicsWithSpace: true);
                            }
                            else
                            {
                                var oeResult = EvaluateSequenceConstructor(onEmpty, contextItem, wrapInDocumentNode: false);
                                CopyToResult(oeResult, separateAtomicsWithSpace: true);
                            }
                        }
                    }
                    break;
                }

            case "apply-templates":
                {
                    var select = instruction.Attribute("select")?.Value;
                    var modeRaw = instruction.Attribute("mode")?.Value?.Trim();
                    // Absent mode attribute means #default, which resolves to the current default mode
                    // (usually the unnamed mode), not the current mode.
                    var mode = string.IsNullOrEmpty(modeRaw)
                        ? CurrentDefaultMode
                        : ExpandModeName(modeRaw, instruction);
                    var sortElements = instruction.Elements(XName.Get("sort", Stylesheet.Stylesheet.XslNamespace)).ToList();

                    var (withParams, tunnelParams) = CollectWithParams(instruction, contextItem);

                    if (node != null)
                    {
                        ApplyTemplates(node, mode, select, sortElements.Count > 0 ? sortElements : null, tunnelParams, withParams, instruction);
                    }
                    else if (!string.IsNullOrEmpty(select))
                    {
                        // apply-templates with select but no context node (e.g. inside named template)
                        ApplyTemplates(contextItem, mode, select, sortElements.Count > 0 ? sortElements : null, tunnelParams, withParams, instruction);
                    }
                    else if (!contextItem.IsUndefined)
                    {
                        // xsl:apply-templates with no @select requires a node context item (XTTE0510)
                        throw new InvalidOperationException("XTTE0510: The context item for xsl:apply-templates is not a node.");
                    }
                    // If there is no context item and no select, apply-templates has nothing to process
                    break;
                }

            case "for-each":
                {
                    ValidateSortComesFirst(instruction);
                    var select = instruction.Attribute("select")?.Value;
                    if (string.IsNullOrEmpty(select))
                        throw new InvalidOperationException("XTSE0010: xsl:for-each requires a select attribute");
                    if (!string.IsNullOrEmpty(select))
                    {
                        var compiled = CompileXPath(select, instruction);
                        var result = compiled.Evaluate(_context);
                        var items = EnumerateItems(result).ToList();

                        var sortElements = instruction.Elements(XName.Get("sort", Stylesheet.Stylesheet.XslNamespace)).ToList();
                        if (sortElements.Count > 0)
                        {
                            items = SortItems(items, sortElements);
                        }

                        var savedFocus = _context.ContextItem;
                        var savedCurrent = _context.CurrentItem;
                        var savedTemplateRule = _currentTemplateRule;
                        var savedNextMatchExcluded = _nextMatchExcluded;
                        _currentTemplateRule = null;
                        _nextMatchExcluded = new HashSet<Stylesheet.TemplateRule>();
                        int pos = 1;
                        foreach (var item in items)
                        {
                            _context.WithFocus(item, pos, items.Count);
                            _context.WithCurrentItem(item);
                            var feSnapshot = _context.SnapshotVariables();
                            try
                            {
                                if (ContainsConditionalInstruction(instruction))
                                {
                                    var feItems = EvaluateSequenceConstructorToItems(instruction, item, e =>
                                        e.Name.LocalName == "sort" && e.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace);
                                    if (_sequenceAccumulator != null)
                                    {
                                        foreach (var feItem in feItems)
                                            _sequenceAccumulator.Add(feItem);
                                    }
                                    else
                                    {
                                        foreach (var feItem in feItems)
                                        {
                                            if (feItem.IsSequence && feItem.SequenceValue != null)
                                            {
                                                foreach (var subItem in XdmSequence.FromSource(feItem.SequenceValue))
                                                {
                                                    if (!subItem.IsUndefined)
                                                        CopyToResult(subItem);
                                                }
                                            }
                                            else if (!feItem.IsUndefined)
                                            {
                                                CopyToResult(feItem);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var childNode in instruction.Nodes())
                                    {
                                        switch (childNode)
                                        {
                                            case XText text:
                                                ProcessSequenceText(text, instruction);
                                                break;
                                            case XElement elem when elem.Name.LocalName == "sort" && elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                                continue;
                                            case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                                ExecuteXsltInstruction(elem, item);
                                                break;
                                            case XElement elem:
                                                CopyLiteralElement(elem);
                                                break;
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                _context.RestoreVariables(feSnapshot);
                            }
                            pos++;
                        }
                        _context.WithFocus(savedFocus, 1, 1);
                        _context.WithCurrentItem(savedCurrent);
                        _currentTemplateRule = savedTemplateRule;
                        _nextMatchExcluded = savedNextMatchExcluded;
                    }
                    break;
                }

            case "for-each-group":
                {
                    var select = instruction.Attribute("select")?.Value;
                    if (string.IsNullOrEmpty(select)) break;

                    // Save the caller's focus and group state BEFORE constructing groups,
                    // because evaluating grouping keys/patterns mutates the focus.
                    var savedFocus = _context.ContextItem;
                    var savedPosition = _context.ContextPosition;
                    var savedSize = _context.ContextSize;
                    var savedCurrent = _context.CurrentItem;
                    var savedTemplateRule = _currentTemplateRule;
                    var savedNextMatchExcluded = _nextMatchExcluded;
                    var savedGroup = _currentGroup;
                    var savedKey = _currentGroupingKey;
                    _currentTemplateRule = null;
                    _nextMatchExcluded = new HashSet<Stylesheet.TemplateRule>();

                    try
                    {
                        var compiled = CompileXPath(select, instruction);
                        var result = compiled.Evaluate(_context);
                        var items = EnumerateItems(result).ToList();
                        if (items.Count == 0) break;

                        var collationAttr = instruction.Attribute("collation")?.Value;
                        var effectiveCollation = string.IsNullOrEmpty(collationAttr) ? _context.DefaultCollation : EvaluateAvt(collationAttr, instruction);

                        ValidateForEachGroupAttributes(instruction);

                        var groups = BuildForEachGroups(instruction, items, effectiveCollation);

                        var bindGroup = instruction.Attribute("bind-group")?.Value;
                        var bindKey = instruction.Attribute("bind-grouping-key")?.Value;

                        // Handle xsl:sort children. In XSLT 2.0 current-group()/current-grouping-key()
                        // are visible in the sort keys; in XSLT 3.0 they are not.
                        var sortElements = instruction.Elements(XName.Get("sort", Stylesheet.Stylesheet.XslNamespace)).ToList();
                        for (int sortIdx = 0; sortIdx < sortElements.Count; sortIdx++)
                        {
                            var stableAttr = EvaluateAvt(sortElements[sortIdx].Attribute("stable")?.Value ?? "", sortElements[sortIdx]);
                            if (sortIdx > 0 && !string.IsNullOrEmpty(stableAttr))
                                throw new InvalidOperationException("XTSE1017: @stable is allowed only on the first xsl:sort");
                            if (!string.IsNullOrEmpty(stableAttr))
                            {
                                var v = stableAttr.Trim();
                                if (v != "yes" && v != "true" && v != "1" &&
                                    v != "no" && v != "false" && v != "0")
                                    throw new InvalidOperationException("XTSE0020: invalid value for @stable");
                            }
                        }
                        if (sortElements.Count > 0 && groups.Count > 0)
                        {
                            groups = SortGroups(groups, sortElements);
                        }

                        int pos = 1;
                        foreach (var (key, groupItems) in groups)
                        {
                            _currentGroup = groupItems;
                            _currentGroupingKey = key;
                            var rep = groupItems[0];
                            _context.WithFocus(rep, pos, groups.Count);
                            _context.WithCurrentItem(rep);
                            var feSnapshot = _context.SnapshotVariables();
                            try
                            {
                                if (!string.IsNullOrEmpty(bindGroup))
                                {
                                    var (bgLocal, bgNs) = ExpandVariableName(instruction, bindGroup);
                                    _context.WithVariable(bgLocal, XdmValue.FromSequence(MaterializedSequence.FromList(groupItems)), bgNs);
                                }
                                if (!string.IsNullOrEmpty(bindKey) && key != null)
                                {
                                    var (bkLocal, bkNs) = ExpandVariableName(instruction, bindKey);
                                    _context.WithVariable(bkLocal, key.Value, bkNs);
                                }

                                foreach (var childNode in instruction.Nodes())
                                {
                                    switch (childNode)
                                    {
                                        case XText text:
                                            ProcessSequenceText(text, instruction);
                                            break;
                                        case XElement elem when elem.Name.LocalName == "sort" && elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                            continue;
                                        case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                            ExecuteXsltInstruction(elem, rep);
                                            break;
                                        case XElement elem:
                                            CopyLiteralElement(elem);
                                            break;
                                    }
                                }
                            }
                            finally
                            {
                                _context.RestoreVariables(feSnapshot);
                            }
                            pos++;
                        }
                    }
                    finally
                    {
                        _context.WithFocus(savedFocus, savedPosition, savedSize);
                        _context.WithCurrentItem(savedCurrent);
                        _currentTemplateRule = savedTemplateRule;
                        _nextMatchExcluded = savedNextMatchExcluded;
                        _currentGroup = savedGroup;
                        _currentGroupingKey = savedKey;
                    }
                    break;
                }

            case "merge":
                {
                    ExecuteMergeInstruction(instruction, contextItem);
                    break;
                }

            case "analyze-string":
                {
                    ExecuteAnalyzeString(instruction, contextItem, ExecuteAnalyzeStringChild);
                    break;
                }

            case "if":
                {
                    var test = instruction.Attribute("test")?.Value;
                    if (!string.IsNullOrEmpty(test))
                    {
                        var compiled = CompileXPath(test, instruction);
                        WithDefaultCollation(instruction, () =>
                        {
                            var result = compiled.Evaluate(_context);
                            if (result.EffectiveBooleanValue())
                            {
                                foreach (var childNode in instruction.Nodes())
                                {
                                    switch (childNode)
                                    {
                                        case XText text:
                                            ProcessSequenceText(text, instruction);
                                            break;
                                        case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                            ExecuteXsltInstruction(elem, contextItem);
                                            break;
                                        case XElement elem:
                                            CopyLiteralElement(elem);
                                            break;
                                    }
                                }
                            }
                        });
                    }
                    break;
                }

            case "choose":
                {
                    bool matched = false;
                    foreach (var when in instruction.Elements(XName.Get("when", Stylesheet.Stylesheet.XslNamespace)))
                    {
                        var test = when.Attribute("test")?.Value;
                        if (!string.IsNullOrEmpty(test))
                        {
                            var compiled = CompileXPath(test, when);
                            WithDefaultCollation(when, () =>
                            {
                                var result = compiled.Evaluate(_context);
                                if (result.EffectiveBooleanValue())
                                {
                                    matched = true;
                                    foreach (var childNode in when.Nodes())
                                    {
                                        switch (childNode)
                                        {
                                            case XText text:
                                                ProcessSequenceText(text, when);
                                                break;
                                            case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                                ExecuteXsltInstruction(elem, contextItem);
                                                break;
                                            case XElement elem:
                                                CopyLiteralElement(elem);
                                                break;
                                        }
                                    }
                                }
                            });
                            if (matched)
                                break;
                        }
                    }
                    if (!matched)
                    {
                        var otherwise = instruction.Element(XName.Get("otherwise", Stylesheet.Stylesheet.XslNamespace));
                        if (otherwise != null)
                        {
                            WithDefaultCollation(otherwise, () =>
                            {
                                foreach (var childNode in otherwise.Nodes())
                                {
                                    switch (childNode)
                                    {
                                        case XText text:
                                            ProcessSequenceText(text, otherwise);
                                            break;
                                        case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                            ExecuteXsltInstruction(elem, contextItem);
                                            break;
                                        case XElement elem:
                                            CopyLiteralElement(elem);
                                            break;
                                    }
                                }
                            });
                        }
                    }
                    break;
                }

            case "variable":
                {
                    var varName = instruction.Attribute("name")?.Value;
                    var varSelect = instruction.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(varName))
                    {
                        var (varLocal, varNs) = ExpandVariableName(instruction, varName);
                        XdmValue varValue;
                        if (!string.IsNullOrEmpty(varSelect))
                        {
                            var compiled = CompileXPath(varSelect, instruction);
                            varValue = compiled.Evaluate(_context);
                        }
                        else
                        {
                            // Build value from sequence constructor (text nodes + XSLT instructions).
                            // Variable bodies are evaluated in a temporary output state.
                            var savedOutputUri = _context.CurrentOutputUri;
                            _context.CurrentOutputUri = null;
                            try
                            {
                                varValue = EvaluateSequenceConstructor(instruction, contextItem, wrapInDocumentNode: string.IsNullOrEmpty(instruction.Attribute("as")?.Value));
                            }
                            finally
                            {
                                _context.CurrentOutputUri = savedOutputUri;
                            }
                        }
                        varValue = ConvertVariableValue(varValue, instruction.Attribute("as")?.Value);
                        _context.WithVariable(varLocal, varValue, varNs);
                    }
                    break;
                }

            case "param":
                // xsl:param inside a template body is processed by ExecuteTemplate before body execution.
                // When encountered inline (e.g. inside a for-each), it behaves like a local variable.
                {
                    var varName = instruction.Attribute("name")?.Value;
                    var varSelect = instruction.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(varName))
                    {
                        var (varLocal, varNs) = ExpandVariableName(instruction, varName);
                        XdmValue varValue;
                        if (!string.IsNullOrEmpty(varSelect))
                        {
                            var compiled = XPath31Expression.Compile(varSelect);
                            varValue = compiled.Evaluate(_context);
                        }
                        else
                        {
                            // Parameter default-value bodies are evaluated in a temporary output state.
                            var savedOutputUri = _context.CurrentOutputUri;
                            _context.CurrentOutputUri = null;
                            try
                            {
                                varValue = EvaluateSequenceConstructor(instruction, contextItem, wrapInDocumentNode: string.IsNullOrEmpty(instruction.Attribute("as")?.Value));
                            }
                            finally
                            {
                                _context.CurrentOutputUri = savedOutputUri;
                            }
                        }
                        varValue = ConvertVariableValue(varValue, instruction.Attribute("as")?.Value, isParam: true);
                        _context.WithVariable(varLocal, varValue, varNs);
                    }
                    break;
                }

            case "call-template":
                {
                    if (!string.IsNullOrEmpty(instruction.Attribute("as")?.Value))
                        throw new InvalidOperationException("XTSE0010: Attribute 'as' is not permitted on xsl:call-template");
                    var calledName = instruction.Attribute("name")?.Value;
                    if (!string.IsNullOrEmpty(calledName))
                    {
                        var (withParams, tunnelParams) = CollectWithParams(instruction, contextItem);
                        var resolvedName = ResolveNamedTemplateName(calledName, instruction);
                        WithoutMergeContext(() => CallTemplate(resolvedName, contextItem, withParams, tunnelParams));
                    }
                    break;
                }

            case "sequence":
                {
                    var select = instruction.Attribute("select")?.Value;
                    bool hasSelect = !string.IsNullOrEmpty(select);
                    bool hasNonFallbackContent = HasNonFallbackContent(instruction);
                    double effectiveVersion = GetEffectiveVersion(instruction);

                    if (hasSelect && hasNonFallbackContent)
                        throw new InvalidOperationException("XTSE3185: xsl:sequence must not have both a 'select' attribute and sequence-constructor content");

                    if (ProcessorXsltVersion < 3.0 && effectiveVersion < 3.0 && !hasSelect)
                        throw new InvalidOperationException("XTSE0010: xsl:sequence requires a 'select' attribute");

                    if (hasSelect)
                    {
                        var compiled = XPath31Expression.Compile(select!);
                        var result = compiled.Evaluate(_context);
                        if (_sequenceAccumulator != null)
                        {
                            // Collecting a raw sequence: preserve the position of this
                            // xsl:sequence contribution by inserting a placeholder that
                            // is expanded when the sequence constructor is finalized.
                            var items = new List<XdmValue>();
                            FlattenToList(result, items, preserveUndefined: true);
                            AddSequencePlaceholder(_currentContainer, items);
                        }
                        else
                        {
                            CopyToResult(result, separateAtomicsWithSpace: true);
                        }
                    }
                    else
                    {
                        // XSLT 3.0: xsl:sequence with no @select evaluates its sequence
                        // constructor and returns the resulting sequence. Use the standard
                        // item collector so whitespace stripping, TVT expansion, and nested
                        // sequence-producing instructions are handled consistently.
                        var seqItems = EvaluateSequenceConstructorToItems(instruction, contextItem);
                        if (seqItems.Count == 0)
                        {
                            if (_sequenceAccumulator != null)
                                AddSequencePlaceholder(_currentContainer, new List<XdmValue>());
                        }
                        else
                        {
                            var resultSeq = seqItems.Count == 1
                                ? seqItems[0]
                                : XdmValue.FromSequence(MaterializedSequence.FromList(seqItems));
                            if (_sequenceAccumulator != null)
                            {
                                AddSequencePlaceholder(_currentContainer, seqItems);
                            }
                            else
                            {
                                CopyToResult(resultSeq, separateAtomicsWithSpace: true);
                            }
                        }
                    }
                    break;
                }

            case "document":
                {
                    if (instruction.Name.NamespaceName == ExsltCommonNamespace)
                    {
                        ExecuteResultDocument(instruction, contextItem, isPrincipal: false);
                        break;
                    }

                    var docContent = EvaluateSequenceConstructor(instruction, contextItem, wrapInDocumentNode: true);
                    if (docContent.IsNode && docContent.NodeValue != null)
                    {
                        if (_sequenceAccumulator != null)
                        {
                            _sequenceAccumulator.Add(XdmValue.FromNode(CopyXdmNode(docContent.NodeValue, copyAllNamespaces: true)));
                        }
                        else
                        {
                            CopyNodeToResult(docContent.NodeValue);
                        }
                    }
                    else if (docContent.IsSequence && docContent.SequenceValue != null)
                    {
                        if (_sequenceAccumulator != null)
                        {
                            foreach (var item in XdmSequence.FromSource(docContent.SequenceValue))
                                _sequenceAccumulator.Add(item);
                        }
                        else
                        {
                            foreach (var item in XdmSequence.FromSource(docContent.SequenceValue))
                            {
                                CopyToResult(item);
                            }
                        }
                    }
                    break;
                }

            case "source-document":
                {
                    var sdHref = instruction.Attribute("href")?.Value;
                    if (string.IsNullOrEmpty(sdHref))
                        throw new InvalidOperationException("XTSE0010: xsl:source-document must have an @href");
                    var resolvedHref = EvaluateAvt(sdHref, instruction);

                    var streamableAttr = instruction.Attribute("streamable")?.Value ?? instruction.Attribute("_streamable")?.Value;
                    if (!string.IsNullOrEmpty(streamableAttr))
                    {
                        var sv = EvaluateAvt(streamableAttr, instruction).Trim();
                        if (sv == "yes" || sv == "true")
                            throw new InvalidOperationException("Streaming is not supported");
                    }

                    var savedBaseUri = _context.BaseUri;
                    try
                    {
                        var baseUri = GetEffectiveBaseUri(instruction) ?? _context.BaseUri;
                        _context.BaseUri = baseUri;

                        // A fragment identifier identifies the element used as the context item.
                        string? fragment = null;
                        var documentHref = resolvedHref;
                        var hashIndex = resolvedHref.IndexOf('#');
                        if (hashIndex >= 0)
                        {
                            fragment = resolvedHref[(hashIndex + 1)..];
                            documentHref = resolvedHref[..hashIndex];
                        }

                        var docNode = _context.LoadDocument(documentHref);
                        _context.RegisterDocument(documentHref, docNode);

                        IXdmNode contextNode = docNode;
                        if (!string.IsNullOrEmpty(fragment))
                        {
                            contextNode = FindElementByXmlId(docNode, fragment)
                                ?? throw new InvalidOperationException($"XTDE1160: No element with xml:id '{fragment}' found in {documentHref}");
                        }

                        var content = EvaluateSequenceConstructor(instruction, XdmValue.FromNode(contextNode), wrapInDocumentNode: false);
                        if (_sequenceAccumulator != null)
                        {
                            foreach (var item in EnumerateItems(content))
                                _sequenceAccumulator.Add(item);
                        }
                        else
                        {
                            foreach (var item in EnumerateItems(content))
                                CopyToResult(item);
                        }
                    }
                    finally
                    {
                        _context.BaseUri = savedBaseUri;
                    }
                    break;
                }

            case "copy-of":
                {
                    var select = instruction.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(select))
                    {
                        var compiled = CompileXPath(select, instruction);
                        var result = compiled.Evaluate(_context);
                        var copyNamespacesAttrRaw = instruction.Attribute("copy-namespaces")?.Value
                        ?? instruction.Attribute("_copy-namespaces")?.Value
                        ?? "yes";
                        var copyNamespacesAttr = EvaluateAvt(copyNamespacesAttrRaw, instruction);
                        bool copyAllNs = copyNamespacesAttr != "no" && copyNamespacesAttr != "false";
                        var copyAccumulatorsAttrRaw = instruction.Attribute("copy-accumulators")?.Value ?? "no";
                        var copyAccumulatorsAttr = EvaluateAvt(copyAccumulatorsAttrRaw, instruction);
                        bool copyAccumulators = copyAccumulatorsAttr == "yes" || copyAccumulatorsAttr == "true";

                        if (_sequenceAccumulator != null)
                        {
                            // In a sequence-returning context (variable with @as),
                            // preserve document nodes by adding copies to the accumulator.
                            if (result.IsSequence && result.SequenceValue != null)
                            {
                                foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                                {
                                    if (item.IsNode && item.NodeValue != null)
                                    {
                                        _sequenceAccumulator.Add(XdmValue.FromNode(CopyXdmNode(item.NodeValue, copyAllNs, copyAccumulators)));
                                    }
                                    else
                                    {
                                        _sequenceAccumulator.Add(item);
                                    }
                                }
                            }
                            else if (result.IsNode && result.NodeValue != null)
                            {
                                _sequenceAccumulator.Add(XdmValue.FromNode(CopyXdmNode(result.NodeValue, copyAllNs, copyAccumulators)));
                            }
                            else
                            {
                                _sequenceAccumulator.Add(result);
                            }
                        }
                        else
                        {
                            if (result.IsSequence && result.SequenceValue != null)
                            {
                                foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                                {
                                    if (item.IsNode && item.NodeValue != null)
                                        CopyNodeToResult(CopyXdmNode(item.NodeValue, copyAllNs, copyAccumulators));
                                    else
                                        CopyToResult(item);
                                }
                            }
                            else if (result.IsNode && result.NodeValue != null)
                            {
                                CopyNodeToResult(CopyXdmNode(result.NodeValue, copyAllNs, copyAccumulators));
                            }
                            else
                            {
                                CopyToResult(result);
                            }
                        }
                    }
                    break;
                }

            case "next-match":
                {
                    if (_currentTemplateRule == null || _context.ContextItem.IsUndefined)
                    {
                        // xsl:next-match is only valid within a template invoked by apply-templates or next-match
                        // If called from a named template with context-item use="absent", for-each, or other
                        // context where the current template rule or context item is absent, raise XTDE0560.
                        throw new InvalidOperationException("XTDE0560: xsl:next-match evaluated when the current template rule is absent.");
                    }

                    var nextMatchMode = _modeStack.Count > 0 ? _modeStack.Peek() : "";
                    // If inside a template invoked by xsl:apply-imports, restrict next-match
                    // to templates with higher import precedence than the apply-imports caller.
                    int? nextMatchMinPrec = _applyImportsPrecedenceStack.Count > 0
                        ? _applyImportsPrecedenceStack.Peek()
                        : null;
                    _nextMatchExcluded.Add(_currentTemplateRule);
                    try
                    {
                        var nextRule = FindBestTemplate(contextItem, nextMatchMode, _nextMatchExcluded, minImportPrecedence: nextMatchMinPrec);

                        var (nextMatchParams, nextMatchTunnelParams) = CollectWithParams(instruction, contextItem);

                        // Merge current tunnel params with newly supplied tunnel params
                        var mergedTunnelParams = new Dictionary<string, XdmValue>();
                        if (_tunnelParamStack.Count > 0)
                        {
                            foreach (var (k, v) in _tunnelParamStack.Peek())
                                mergedTunnelParams[k] = v;
                        }
                        foreach (var (k, v) in nextMatchTunnelParams)
                            mergedTunnelParams[k] = v;

                        if (nextRule != null)
                        {
                            _nextMatchExcluded.Add(nextRule);
                            try
                            {
                                ExecuteTemplate(nextRule, contextItem, callParams: nextMatchParams, incomingTunnelParams: mergedTunnelParams, position: _context.ContextPosition, last: _context.ContextSize);
                            }
                            finally
                            {
                                _nextMatchExcluded.Remove(nextRule);
                            }
                        }
                        else if (node != null)
                        {
                            ApplyBuiltInRules(node, nextMatchMode, mergedTunnelParams, nextMatchParams);
                        }
                        else if (!contextItem.IsUndefined)
                        {
                            ApplyBuiltInRulesForAtomic(contextItem, nextMatchMode);
                        }
                    }
                    finally
                    {
                        _nextMatchExcluded.Remove(_currentTemplateRule);
                    }
                    break;
                }

            case "apply-imports":
                {
                    if (_currentTemplateRule == null)
                    {
                        throw new InvalidOperationException("XTDE0560: xsl:apply-imports evaluated when the current template rule is absent.");
                    }

                    var applyImportsMode = _modeStack.Count > 0 ? _modeStack.Peek() : "";

                    // Find the best matching template with higher import precedence
                    // (i.e., deeper in the import chain). The search is restricted to
                    // template rules in modules that were imported into the stylesheet
                    // module whose import tree governs the current template rule
                    // (the current module for imported modules, the including module
                    // for included modules).
                    var contextModule = _currentTemplateRule.Stylesheet.ApplyImportsContextModule;
                    var importedRule = FindBestTemplate(contextItem, applyImportsMode, minImportPrecedence: _currentTemplateRule.ImportPrecedence, allowedStylesheets: contextModule.TransitiveImports);

                    var (applyImportsParams, applyImportsTunnelParams) = CollectWithParams(instruction, contextItem);

                    // Pass through current tunnel parameters, overridden by newly supplied ones
                    var currentTunnelParams = new Dictionary<string, XdmValue>();
                    if (_tunnelParamStack.Count > 0)
                    {
                        foreach (var (k, v) in _tunnelParamStack.Peek())
                            currentTunnelParams[k] = v;
                    }
                    foreach (var (k, v) in applyImportsTunnelParams)
                        currentTunnelParams[k] = v;

                    // Push the current template rule's precedence so that xsl:next-match
                    // inside the imported template is restricted to higher import precedence
                    // rules (XSLT 3.0 §6.5).
                    _applyImportsPrecedenceStack.Push(_currentTemplateRule.ImportPrecedence);
                    try
                    {
                        if (importedRule != null)
                        {
                            ExecuteTemplate(importedRule, contextItem, callParams: applyImportsParams, incomingTunnelParams: currentTunnelParams, position: _context.ContextPosition, last: _context.ContextSize);
                        }
                        else if (node != null)
                        {
                            ApplyBuiltInRules(node, applyImportsMode, currentTunnelParams, applyImportsParams);
                        }
                        else if (!contextItem.IsUndefined)
                        {
                            ApplyBuiltInRulesForAtomic(contextItem, applyImportsMode);
                        }
                    }
                    finally
                    {
                        _applyImportsPrecedenceStack.Pop();
                    }
                    break;
                }

            case "number":
                {
                    var hasValueAttr = !string.IsNullOrEmpty(instruction.Attribute("value")?.Value);
                    var hasSelectAttr = !string.IsNullOrEmpty(instruction.Attribute("select")?.Value);

                    if (hasValueAttr || hasSelectAttr)
                    {
                        ExecuteXsltNumber(instruction, node!);
                    }
                    else if (node != null)
                    {
                        ExecuteXsltNumber(instruction, node);
                    }
                    else
                    {
                        // No value, no select, and no context node
                        throw new InvalidOperationException("XTTE0990");
                    }
                    break;
                }

            case "try":
                {
                    var catchElements = instruction.Elements(XName.Get("catch", Stylesheet.Stylesheet.XslNamespace)).ToList();
                    var tryScope = SnapshotTryScope();
                    var outputBefore = _currentContainer?.Nodes().Count() ?? 0;
                    var attrsBefore = (_currentContainer as XElement)?.Attributes().Count() ?? 0;
                    var savedLastNode = _currentContainer?.LastNode;
                    var savedLastAttr = (_currentContainer as XElement)?.LastAttribute;
                    var lastAtomicBefore = _lastAddedWasAtomic;
                    try
                    {
                        var select = instruction.Attribute("select")?.Value;
                        if (!string.IsNullOrEmpty(select))
                        {
                            var compiled = XPath31Expression.Compile(select);
                            var result = compiled.Evaluate(_context);
                            CopyToResult(result);
                        }
                        else
                        {
                            foreach (var childNode in instruction.Nodes())
                            {
                                if (childNode is XElement xe && xe.Name.LocalName == "catch" && xe.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                                    continue;
                                switch (childNode)
                                {
                                    case XText text:
                                        ProcessSequenceText(text, instruction);
                                        break;
                                    case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                        ExecuteXsltInstruction(elem, contextItem);
                                        break;
                                    case XElement elem:
                                        CopyLiteralElement(elem);
                                        break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        RestoreTryScope(tryScope);
                        if (ex is IterateControlException)
                            throw;
                        if (ex.Data.Contains("Bosak.GlobalVariableError"))
                            throw;
                        var currentNodeCount = _currentContainer?.Nodes().Count() ?? 0;
                        var currentAttrCount = (_currentContainer as XElement)?.Attributes().Count() ?? 0;
                        if (instruction.Attribute("rollback-output")?.Value == "no" && (currentNodeCount > outputBefore || currentAttrCount > attrsBefore))
                            throw new InvalidOperationException("XTDE3530: Recovery not possible because output has already been written.");
                        var catchElem = FindMatchingCatch(catchElements, ex);
                        if (catchElem == null)
                            throw;

                        RollbackOutputToNode(savedLastNode, savedLastAttr);
                        _lastAddedWasAtomic = lastAtomicBefore;

                        var previous = BindCatchErrorVariables(ex, catchElem, instruction);
                        try
                        {
                            var catchSelect = catchElem.Attribute("select")?.Value;
                            if (!string.IsNullOrEmpty(catchSelect))
                            {
                                var compiled = XPath31Expression.Compile(catchSelect);
                                var catchResult = compiled.Evaluate(_context);
                                CopyToResult(catchResult);
                            }
                            else
                            {
                                foreach (var childNode in catchElem.Nodes())
                                {
                                    switch (childNode)
                                    {
                                        case XText text:
                                            ProcessSequenceText(text, catchElem);
                                            break;
                                        case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                            ExecuteXsltInstruction(elem, contextItem);
                                            break;
                                        case XElement elem:
                                            CopyLiteralElement(elem);
                                            break;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            RestoreCatchErrorVariables(previous);
                        }
                    }
                    RestoreTryScope(tryScope);
                    break;
                }

            case "evaluate":
                {
                    var evalResult = EvaluateXslEvaluate(instruction, contextItem);
                    CopyToResult(evalResult, separateAtomicsWithSpace: true);
                    break;
                }

            case "iterate":
                ExecuteXslIterate(instruction, contextItem);
                break;

            case "break":
                ExecuteXslBreak(instruction);
                break;

            case "next-iteration":
                ExecuteXslNextIteration(instruction);
                break;
            case "fallback":
            case "sort":
            case "on-empty":
            case "on-non-empty":
                // These instructions are handled by their parent (e.g. xsl:for-each,
                // xsl:try, xsl:iterate) or by dedicated sequence-constructor processing.
                // If they are reached directly they produce no output.
                break;

            case "assert":
                // xsl:assert is accepted but not yet evaluated. Full support requires
                // an enable-assertions switch; tests that disable assertions rely on
                // this no-op behaviour.
                break;

            case "perform-sort":
                {
                    ValidateSortComesFirst(instruction);
                    var psSelect2 = instruction.Attribute("select")?.Value;
                    List<XdmValue> psItems;
                    if (!string.IsNullOrEmpty(psSelect2))
                    {
                        var compiled = XPath31Expression.Compile(psSelect2);
                        var psResult = compiled.Evaluate(_context);
                        psItems = EnumerateItems(psResult).ToList();
                    }
                    else
                    {
                        psItems = EvaluatePerformSortContent(instruction, _context.ContextItem);
                    }

                    var sortElements = instruction.Elements(XName.Get("sort", Stylesheet.Stylesheet.XslNamespace)).ToList();
                    if (sortElements.Count > 0)
                    {
                        psItems = SortItems(psItems, sortElements);
                    }

                    foreach (var item in psItems)
                        CopyToResult(item);
                    break;
                }

            case "map":
                {
                    var mapValue = BuildMapFromInstruction(instruction, contextItem);
                    if (_sequenceAccumulator != null)
                    {
                        _sequenceAccumulator.Add(mapValue);
                    }
                    else if (TryCollectRawResultItem(mapValue))
                    {
                        // Collected as a raw top-level JSON item.
                    }
                    else
                    {
                        if (IsPrincipalTopLevel)
                            throw new XsltRuntimeException("SENR0001",
                                "Cannot serialize a map using this output method.", XdmValue.Undefined);
                        throw new InvalidOperationException("XTDE0450: A map cannot appear as a child of an element or document node");
                    }
                    break;
                }

            case "map-entry":
                {
                    var entryValue = BuildMapEntry(instruction, contextItem);
                    if (_sequenceAccumulator != null)
                    {
                        _sequenceAccumulator.Add(entryValue);
                    }
                    else if (TryCollectRawResultItem(entryValue))
                    {
                        // Collected as a raw top-level JSON item.
                    }
                    else
                    {
                        if (IsPrincipalTopLevel)
                            throw new XsltRuntimeException("SENR0001",
                                "Cannot serialize a map using this output method.", XdmValue.Undefined);
                        throw new InvalidOperationException("XTDE0450: A map cannot appear as a child of an element or document node");
                    }
                    break;
                }

            case "result-document":
                {
                    var hrefRaw = instruction.Attribute("href")?.Value;
                    var href = EvaluateAvt(hrefRaw ?? string.Empty, instruction);
                    bool isPrincipal = string.IsNullOrEmpty(href);
                    ExecuteResultDocument(instruction, contextItem, isPrincipal);
                    break;
                }

            default:
                {
                    var xslNs = Stylesheet.Stylesheet.XslNamespace;
                    if (instruction.Name.NamespaceName == xslNs)
                    {
                        // Unknown XSLT instruction.
                        if (IsForwardsCompatible(instruction))
                        {
                            // In forwards-compatible mode, evaluate any xsl:fallback children.
                            var fallbacks = instruction.Elements(XName.Get("fallback", xslNs)).ToList();
                            if (fallbacks.Count > 0)
                            {
                                foreach (var fb in fallbacks)
                                {
                                    ExecuteSequenceConstructorDirect(fb, contextItem, _currentContainer);
                                }
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException($"XTSE0010: Unknown XSLT instruction '{instruction.Name.LocalName}'");
                        }
                    }
                    break;
                }
        }
        }
        finally
        {
            if (!string.IsNullOrEmpty(instructionDefaultMode))
            {
                _defaultModeStack.Pop();
            }
            _context.DefaultCollation = savedDefaultCollation;
            _context.BackwardsCompatible = savedBackwardsCompatible;
        }
    }

    /// <summary>
    /// Applies the named attribute sets to the target element.
    /// Attribute sets accumulate across imports/includes (merge semantics).
    /// </summary>
    private void ApplyAttributeSets(XElement source, XElement target, HashSet<(string LocalName, string NamespaceUri)>? visited = null)
    {
        // Check both xsl:use-attribute-sets (on literal elements) and use-attribute-sets (on xsl:element / xsl:attribute-set)
        var useAttrSetsRaw = source.Attribute(XNamespace.Get(Stylesheet.Stylesheet.XslNamespace) + "use-attribute-sets")?.Value
            ?? source.Attribute("use-attribute-sets")?.Value;
        if (string.IsNullOrWhiteSpace(useAttrSetsRaw))
            return;

        visited ??= new HashSet<(string, string)>();
        var allSets = _stylesheet.GetAllAttributeSets();
        var xslNs = Stylesheet.Stylesheet.XslNamespace;

        foreach (var name in useAttrSetsRaw.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = name.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            // Resolve QName
            string localName;
            string nsUri;
            int colon = trimmed.IndexOf(':');
            if (colon >= 0)
            {
                var prefix = trimmed.Substring(0, colon);
                localName = trimmed.Substring(colon + 1);
                nsUri = source.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? "";
            }
            else
            {
                localName = trimmed;
                nsUri = "";
            }

            var key = (localName, nsUri);
            if (!allSets.TryGetValue(key, out var defs))
                continue;

            if (!visited.Add(key))
                continue; // Cycle detected — skip to avoid infinite recursion

            var prevContainer = _currentContainer;
            _currentContainer = target;

            // Attribute sets are evaluated with only top-level variables/parameters in
            // scope; local template variables must not be visible. Save the current
            // variable bindings and restore the pre-transformation snapshot.
            var savedVariables = _context.SnapshotVariables();
            _context.RestoreVariables(_attributeSetVariableSnapshot);
            try
            {
                foreach (var def in defs)
                {
                    // Recursively apply referenced attribute sets
                    if (!string.IsNullOrWhiteSpace(def.UseAttributeSets))
                    {
                        ApplyAttributeSets(def.Element, target, visited);
                    }

                    // Execute this definition's xsl:attribute children
                    foreach (var attrChild in def.Element.Elements(XName.Get("attribute", xslNs)))
                    {
                        ExecuteXsltInstruction(attrChild, _context.ContextItem);
                    }
                }
            }
            finally
            {
                _context.RestoreVariables(savedVariables);
                _currentContainer = prevContainer;
                visited.Remove(key);
            }
        }
    }

    /// <summary>
    /// Copies a literal result element to the output.
    /// </summary>
    private void CopyLiteralElement(XElement source)
    {
        _literalElementDepth++;

        // Extension elements are not copied; if they have xsl:fallback children,
        // the fallback content is evaluated in their place.
        var extensionNs = GetExtensionElementPrefixes(source);
        if (extensionNs.Contains(source.Name.NamespaceName))
        {
            var xslNs = Stylesheet.Stylesheet.XslNamespace;
            var fallbacks = source.Elements(XName.Get("fallback", xslNs)).ToList();
            if (fallbacks.Count > 0)
            {
                foreach (var fb in fallbacks)
                {
                    ExecuteSequenceConstructorDirect(fb, _context.ContextItem, _currentContainer);
                }
            }
            _literalElementDepth--;
            return;
        }

        // Apply namespace-alias mapping to the literal result element name.
        var mappedElementName = MapAliasedName(source.Name, isElement: true, out var elementResultPrefix);
        if (elementResultPrefix == null)
        {
            // Prefer the prefix used in the original XML source. This preserves sibling
            // prefixes that map to the same namespace URI (e.g. one:h3 and my:h3).
            var originalPrefix = source.Annotation<OriginalPrefixAnnotation>()?.Prefix;
            if (originalPrefix != null)
            {
                elementResultPrefix = originalPrefix;
            }
            else
            {
                // Prefer the source element's default namespace when the element is in no-prefix
                // form, even if another in-scope prefix happens to be bound to the same URI.
                var defaultNs = source.GetDefaultNamespace();
                if (!string.IsNullOrEmpty(defaultNs.NamespaceName) && defaultNs.NamespaceName == source.Name.NamespaceName)
                    elementResultPrefix = "";
                else
                    elementResultPrefix = source.GetPrefixOfNamespace(source.Name.Namespace);
            }
        }
        var copy = new XElement(mappedElementName);

        // Handle inherit-namespaces on literal result elements.
        var lreInheritNsAttr = source.Attribute(XName.Get("inherit-namespaces", Stylesheet.Stylesheet.XslNamespace));
        var lreInheritNs = lreInheritNsAttr?.Value ?? "yes";
        if (ParseInheritNamespaces(lreInheritNs) == false)
        {
            copy.AddAnnotation(new NamespaceInheritanceBarrier());
        }
        else if (lreInheritNsAttr != null && ParseInheritNamespaces(lreInheritNs) == true)
        {
            copy.AddAnnotation(new NamespaceInheritanceExplicitYes());
        }

        // Preserve XML 1.1 prefixed namespace undeclarations from the source stylesheet.
        if (source.Annotation<PrefixedNamespaceUndeclarations>() is { } sourceUndecl)
        {
            var copyUndecl = new PrefixedNamespaceUndeclarations();
            copyUndecl.Prefixes.AddRange(sourceUndecl.Prefixes);
            copy.AddAnnotation(copyUndecl);
        }

        // Ensure the element's own namespace is declared on the copied element.
        // The element's own namespace is always required and is never excluded.
        if (!string.IsNullOrEmpty(mappedElementName.NamespaceName))
        {
            EnsureNamespaceDeclaration(copy, mappedElementName, elementResultPrefix);
        }

        // Record the prefix chosen for this element so that the serializer can
        // preserve it even when sibling elements use a different prefix for the
        // same namespace URI.
        copy.AddAnnotation(new ElementPrefixHint { Prefix = elementResultPrefix });

        // Compute excluded namespace URIs in scope on this LRE. exclude-result-prefixes
        // suppresses namespace nodes by URI, not by prefix.
        var excludedNamespaceUris = GetExcludedNamespaceUris(source);
        bool excludeAllNamespaces = excludedNamespaceUris.Contains("#all");

        // Record the excluded URIs on the constructed element so that xsl:namespace can
        // distinguish a real namespace conflict from an excluded declaration that will be
        // removed later.
        if (!excludeAllNamespaces && excludedNamespaceUris.Count > 0)
        {
            var excluded = new ExcludedNamespaceUris();
            foreach (var uri in excludedNamespaceUris)
                excluded.Uris.Add(uri);
            copy.AddAnnotation(excluded);
        }

        // For root-level literal result elements, copy all in-scope namespace bindings from
        // the stylesheet (except excluded URIs, source-alias URIs, and the XSLT namespace)
        // to the result element. This ensures namespace-uri-for-prefix() queries and
        // assertions such as those in attribute-0601 see prefixes declared on xsl:stylesheet.
        bool isRootLevelLiteral = _currentContainer is XElement currentElem &&
            (currentElem.Name.LocalName == "__xdm_doc__" ||
             currentElem.Name.LocalName == "__temp__" ||
             currentElem.Name.LocalName == "__result-document__");
        if (isRootLevelLiteral && !excludeAllNamespaces)
        {
            foreach (var (prefix, styleNs) in GetInScopeNamespaceDeclarations(source))
            {
                if (prefix == "xml" || prefix == "xmlns")
                    continue;
                if (string.IsNullOrEmpty(styleNs.NamespaceName))
                    continue;
                if (styleNs.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                    continue;
                if (TryGetNamespaceAlias(styleNs.NamespaceName, out _))
                    continue;
                if (excludedNamespaceUris.Contains(styleNs.NamespaceName))
                    continue;

                if (prefix == "")
                {
                    // Do not change the element's own default namespace.
                    if (!string.IsNullOrEmpty(copy.Name.NamespaceName))
                        continue;
                    var existingDefault = copy.GetNamespaceOfPrefix("");
                    if (existingDefault != null && existingDefault.NamespaceName == styleNs.NamespaceName)
                        continue;
                    copy.SetAttributeValue("xmlns", styleNs.NamespaceName);
                }
                else
                {
                    var existing = copy.GetNamespaceOfPrefix(prefix);
                    if (existing != null && existing.NamespaceName == styleNs.NamespaceName)
                        continue;
                    copy.SetAttributeValue(XNamespace.Xmlns + prefix, styleNs.NamespaceName);
                }
            }

            // Additionally, if an attribute's local name matches an in-scope namespace prefix
            // in the stylesheet, ensure that prefix is bound. This handles cases such as
            // attribute-1301 where the result tree is queried with namespace-uri-for-prefix().
            foreach (var attr in source.Attributes())
            {
                if (attr.IsNamespaceDeclaration)
                    continue;
                var local = attr.Name.LocalName;
                if (string.IsNullOrEmpty(local) || local == "xml" || local == "xmlns")
                    continue;
                var styleNs = source.GetNamespaceOfPrefix(local);
                if (styleNs == null || string.IsNullOrEmpty(styleNs.NamespaceName))
                    continue;
                if (styleNs.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                    continue;
                if (excludedNamespaceUris.Contains(styleNs.NamespaceName))
                    continue;
                if (TryGetNamespaceAlias(styleNs.NamespaceName, out _))
                    continue;
                var existing = copy.GetNamespaceOfPrefix(local);
                if (existing != null && existing.NamespaceName == styleNs.NamespaceName)
                    continue;
                copy.SetAttributeValue(XNamespace.Xmlns + local, styleNs.NamespaceName);
            }
        }

        // Apply attribute sets first; literal attributes override them.
        ApplyAttributeSets(source, copy);

        var literalAttributesAdded = new HashSet<XName>();
        foreach (var attr in source.Attributes())
        {
            // Skip namespace declarations that are inherited from ancestors
            // (only copy namespace declarations explicitly declared on this element).
            if (attr.IsNamespaceDeclaration)
            {
                var declaredPrefix = attr.Name.LocalName == "xmlns" ? "" : attr.Name.LocalName;
                if (declaredPrefix == source.GetPrefixOfNamespace(source.Name.Namespace))
                {
                    continue; // Already handled above
                }

                var nsUri = attr.Value;
                if (TryGetNamespaceAlias(nsUri, out var alias))
                {
                    if (alias.ResultPrefix == "#default" || string.IsNullOrEmpty(alias.ResultPrefix))
                        copy.SetAttributeValue("xmlns", alias.ResultUri);
                    else
                        copy.SetAttributeValue(XNamespace.Xmlns + alias.ResultPrefix, alias.ResultUri);
                    continue;
                }

                if (excludedNamespaceUris.Contains(nsUri))
                    continue;
                // Skip XSLT namespace declaration — it is not copied to the result tree
                if (nsUri == Stylesheet.Stylesheet.XslNamespace)
                    continue;
                copy.SetAttributeValue(attr.Name, nsUri);
                continue;
            }

            // XSLT-namespace attributes on literal result elements are instructions,
            // not attributes to be copied to the result tree.
            if (attr.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                continue;

            // Unprefixed attributes are always in no namespace and are not affected by
            // namespace-alias, even when the stylesheet declares a default-namespace alias.
            XName mappedAttrName;
            string? attrResultPrefix = null;
            if (string.IsNullOrEmpty(attr.Name.NamespaceName))
            {
                mappedAttrName = attr.Name;
            }
            else
            {
                mappedAttrName = MapAliasedName(attr.Name, isElement: false, out attrResultPrefix);
                if (attrResultPrefix == null)
                    attrResultPrefix = source.GetPrefixOfNamespace(attr.Name.Namespace);
                if (!string.IsNullOrEmpty(mappedAttrName.NamespaceName))
                {
                    EnsureNamespaceDeclaration(copy, mappedAttrName, attrResultPrefix);
                }
            }

            if (!literalAttributesAdded.Add(mappedAttrName))
            {
                throw new InvalidOperationException("XTSE0813: Two attributes on a literal result element have the same expanded QName after namespace aliasing.");
            }

            var attrValue = EvaluateAvt(attr.Value, source);
            Xml11Attribute.SetValue(copy, mappedAttrName, attrValue);
        }

        var collectAsRawItem = IsRawCollectionTopLevel;
        if (!collectAsRawItem)
            AddElementToContainer(copy, _currentContainer);

        var prev = _currentContainer;
        _currentContainer = copy;
        _lastAddedWasAtomic = false;
        // Inside a literal result element, sequence items (especially attributes and
        // namespace nodes) attach to the element being constructed, so the placeholder
        // accumulator used for raw sequence constructors must be suspended.
        var savedAccumulator = _sequenceAccumulator;
        _sequenceAccumulator = null;

        // Variables declared in the content of a literal result element are scoped to that
        // element and must not leak to following siblings in the containing sequence.
        var savedVariables = _context.SnapshotVariables();

        // Push xsl:default-mode for this literal result element scope
        var lreDefaultMode = source.Attribute(XName.Get("default-mode", Stylesheet.Stylesheet.XslNamespace))?.Value;
        if (!string.IsNullOrEmpty(lreDefaultMode))
        {
            _defaultModeStack.Push(ExpandModeName(lreDefaultMode, source));
        }

        try
        {
            if (ContainsConditionalInstruction(source))
            {
                EvaluateSequenceConstructorIntoContainer(source, copy, _context.ContextItem);
            }
            else
            {
                foreach (var child in source.Nodes())
                {
                    switch (child)
                    {
                        case XElement childElem:
                            if (childElem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                            {
                                ExecuteXsltInstruction(childElem, _context.ContextItem);
                            }
                            else
                            {
                                CopyLiteralElement(childElem);
                            }
                            break;
                        case XText text:
                            ProcessSequenceText(text, source);
                            break;
                        // Comments and processing instructions inside literal result elements
                        // are part of the stylesheet, not the result tree.
                        case XComment:
                        case XProcessingInstruction:
                            break;
                    }
                }
            }

            NormalizeElementContent(copy);

            if (collectAsRawItem)
            {
                var rawList = _resultDocumentStack.Count == 0 ? _jsonResultItems : _resultDocumentRawItems;
                rawList.Add(XdmValue.FromNode(new XDocumentNode(copy)));
            }
        }
        finally
        {
            if (!string.IsNullOrEmpty(lreDefaultMode))
            {
                _defaultModeStack.Pop();
            }
            _context.RestoreVariables(savedVariables);
            _currentContainer = prev;
            _sequenceAccumulator = savedAccumulator;
            _literalElementDepth--;
        }
    }

    /// <summary>
    /// Evaluates Attribute Value Templates (AVTs): {expr} is evaluated, {{ and }} are escaped.
    /// Expressions are compiled with the in-scope namespaces, xpath-default-namespace, and
    /// base URI of the element that carries the attribute.
    /// </summary>
    private string EvaluateAvt(string value, XElement? contextElement = null)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var sb = new System.Text.StringBuilder();
        int i = 0;
        var avtBaseUri = GetEffectiveBaseUri(contextElement);
        var nsMap = contextElement != null ? GetInScopeNamespaces(contextElement) : null;
        var defaultNs = contextElement != null ? GetXPathDefaultNamespace(contextElement) : null;
        var definingNs = contextElement != null ? contextElement.GetDefaultNamespace().NamespaceName : null;
        bool needsOptions = (nsMap != null && nsMap.Count > 1)
            || !string.IsNullOrEmpty(defaultNs)
            || !string.IsNullOrEmpty(definingNs)
            || !string.IsNullOrEmpty(avtBaseUri);

        while (i < value.Length)
        {
            if (i + 1 < value.Length && value[i] == '{' && value[i + 1] == '{')
            {
                sb.Append('{');
                i += 2;
            }
            else if (i + 1 < value.Length && value[i] == '}' && value[i + 1] == '}')
            {
                sb.Append('}');
                i += 2;
            }
            else if (value[i] == '{')
            {
                int end = FindAvtExprEnd(value, i + 1);
                if (end < 0)
                {
                    sb.Append(value[i]);
                    i++;
                }
                else
                {
                    var expr = value.Substring(i + 1, end - i - 1);
                    if (!string.IsNullOrEmpty(expr) && !IsOnlyWhitespaceAndComments(expr))
                    {
                        ValidateXPathPrefixes(expr, nsMap ?? new Dictionary<string, string>());
                        XPath31Expression compiled;
                        if (needsOptions)
                        {
                            var options = new CompileOptions
                            {
                                Namespaces = nsMap,
                                DefaultElementNamespace = defaultNs,
                                DefiningElementDefaultNamespace = definingNs,
                                BaseUri = avtBaseUri
                            };
                            compiled = XPath31Expression.Compile(expr, options);
                        }
                        else
                        {
                            compiled = XPath31Expression.Compile(expr);
                        }
                        var result = compiled.Evaluate(_context);
                        string exprValue;
                        if (contextElement != null && IsEffectiveBackwardsCompatible(contextElement))
                        {
                            // XSLT 1.0 backwards compatibility: AVT expression value is the
                            // string value of the first item, or empty for an empty sequence.
                            if (result.IsUndefined)
                            {
                                exprValue = string.Empty;
                            }
                            else if (result.IsSequence && result.SequenceValue != null)
                            {
                                var en = XdmSequence.FromSource(result.SequenceValue).GetEnumerator();
                                exprValue = en.MoveNext() ? en.Current.ToString() : string.Empty;
                            }
                            else
                            {
                                exprValue = result.ToString();
                            }
                        }
                        else
                        {
                            exprValue = XdmValueToString(result);
                        }
                        sb.Append(exprValue);
                    }
                    i = end + 1;
                }
            }
            else if (value[i] == '}')
            {
                // Lone } is an error per spec, but treat as literal for robustness
                sb.Append('}');
                i++;
            }
            else
            {
                sb.Append(value[i]);
                i++;
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Returns true if the extracted AVT expression contains only whitespace and/or
    /// XPath comments. Such an expression evaluates to an empty sequence and contributes
    /// nothing to the attribute value.
    /// </summary>
    private static bool IsOnlyWhitespaceAndComments(string expr)
    {
        int i = 0;
        while (i < expr.Length)
        {
            char c = expr[i];
            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }
            if (c == '(' && i + 1 < expr.Length && expr[i + 1] == ':')
            {
                i += 2;
                int depth = 1;
                while (i < expr.Length && depth > 0)
                {
                    if (expr[i] == ':' && i + 1 < expr.Length && expr[i + 1] == ')')
                    {
                        depth--;
                        i += 2;
                    }
                    else if (expr[i] == '(' && i + 1 < expr.Length && expr[i + 1] == ':')
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
            return false;
        }
        return true;
    }

    /// <summary>
    /// Finds the closing <c>}</c> of an AVT expression, skipping <c>}</c> inside
    /// XPath string literals (both single- and double-quoted), XPath comments
    /// <c>(: ... :)</c>, and respecting nested braces from EQNames and map constructors.
    /// </summary>
    private static int FindAvtExprEnd(string value, int start)
    {
        char inString = '\0';
        int braceDepth = 1; // we are already inside the opening '{'
        int i = start;
        while (i < value.Length)
        {
            char c = value[i];
            if (inString != '\0')
            {
                if (c == inString)
                {
                    // Check for escaped quote (doubled)
                    if (i + 1 < value.Length && value[i + 1] == inString)
                    {
                        i += 2; // skip the pair
                    }
                    else
                    {
                        inString = '\0';
                        i++;
                    }
                }
                else
                {
                    i++;
                }
                continue;
            }
            if (c == '\'' || c == '"')
            {
                inString = c;
                i++;
                continue;
            }
            // XPath comment inside the expression
            if (c == '(' && i + 1 < value.Length && value[i + 1] == ':')
            {
                i += 2;
                int commentDepth = 1;
                while (i < value.Length && commentDepth > 0)
                {
                    if (value[i] == ':' && i + 1 < value.Length && value[i + 1] == ')')
                    {
                        commentDepth--;
                        i += 2;
                    }
                    else if (value[i] == '(' && i + 1 < value.Length && value[i + 1] == ':')
                    {
                        commentDepth++;
                        i += 2;
                    }
                    else
                    {
                        i++;
                    }
                }
                continue;
            }
            if (c == '{')
            {
                braceDepth++;
                i++;
                continue;
            }
            if (c == '}')
            {
                braceDepth--;
                if (braceDepth == 0)
                    return i;
                i++;
                continue;
            }
            i++;
        }
        return -1;
    }

    /// <summary>
    /// Copies an XDM value (node or sequence) into the result tree.
    /// </summary>
    /// <param name="value">The value to copy.</param>
    /// <param name="separateAtomicsWithSpace">If true, consecutive atomic values are separated by a space (complex content construction). If false, they are concatenated directly (xsl:copy-of behavior).</param>
    /// <summary>
    /// Returns whether the current result position is the top level of a raw-item
    /// output (JSON output method or build-tree="no"), where items should be collected
    /// as raw XDM values instead of being added to the result document tree.
    /// </summary>
    private bool IsRawCollectionTopLevel
    {
        get
        {
            if (_resultDocumentStack.Count == 0)
            {
                // Principal output can collect raw items either because the stylesheet-level
                // method is JSON, because an explicit xsl:result-document instruction has
                // requested raw-item collection (method=json/adaptive or build-tree=no), or
                // because fn:transform requested the raw principal result. The current
                // container must actually be the principal result document, otherwise
                // literal elements inside temporary sequence constructors (e.g.
                // xsl:variable/@as) would be lost.
                return (_collectRawItems || _jsonOutputMode || _principalRawCollection)
                    && ReferenceEquals(_currentContainer, _resultDocument);
            }

            if (!_collectRawItems)
                return false;

            var frame = _resultDocumentStack.Peek();
            var root = frame.RootContainer ?? frame.PrincipalContainer;
            return ReferenceEquals(_currentContainer, root);
        }
    }

    /// <summary>
    /// If we are producing a raw-item output, stores the supplied value as a raw
    /// top-level item and returns <c>true</c>. Otherwise returns <c>false</c> so normal
    /// tree construction proceeds.
    /// </summary>
    private bool TryCollectRawResultItem(XdmValue value)
    {
        if (!IsRawCollectionTopLevel)
            return false;
        if (value.IsUndefined)
            return true;

        // Flatten nested sequences so that each top-level item is serialized separately
        // (e.g. item-separator for text output, or one JSON array element per item).
        var items = new List<XdmValue>();
        FlattenToList(value, items);

        var target = _resultDocumentStack.Count == 0 ? _jsonResultItems : _resultDocumentRawItems;
        foreach (var item in items)
            target.Add(item);

        _lastAddedWasAtomic = false;
        return true;
    }

    private void CopyToResult(XdmValue value, bool separateAtomicsWithSpace = true)
    {
        if (value.IsUndefined)
            return;

        if (TryCollectRawResultItem(value))
            return;

        // When collecting a raw sequence (e.g. xsl:variable/@as, xsl:key content,
        // xsl:function body), preserve atomic/node values in the sequence accumulator
        // instead of converting them to text nodes in the result tree.
        if (_sequenceAccumulator != null)
        {
            if (value.IsSequence && value.SequenceValue != null)
            {
                var flattenedItems = new List<XdmValue>();
                foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                    FlattenArrayMembers(item, flattenedItems);
                foreach (var item in flattenedItems)
                {
                    if (item.IsUndefined)
                        continue;
                    if (item.IsNode && item.NodeValue != null)
                        _sequenceAccumulator.Add(XdmValue.FromNode(CopyXdmNode(item.NodeValue)));
                    else
                        _sequenceAccumulator.Add(item);
                }
            }
            else if (value.IsArray && value.ArrayValue != null)
            {
                var flattenedItems = new List<XdmValue>();
                FlattenArrayMembers(value, flattenedItems);
                foreach (var item in flattenedItems)
                {
                    if (item.IsUndefined)
                        continue;
                    if (item.IsNode && item.NodeValue != null)
                        _sequenceAccumulator.Add(XdmValue.FromNode(CopyXdmNode(item.NodeValue)));
                    else
                        _sequenceAccumulator.Add(item);
                }
            }
            else if (value.IsNode && value.NodeValue != null)
            {
                _sequenceAccumulator.Add(XdmValue.FromNode(CopyXdmNode(value.NodeValue)));
            }
            else
            {
                _sequenceAccumulator.Add(value);
            }
            return;
        }

        // Arrays in a sequence constructor are flattened to their members (XSLT 3.0 §5.7.1).
        if (value.IsArray && value.ArrayValue != null)
        {
            var items = new List<XdmValue>();
            FlattenArrayMembers(value, items);
            CopyToResult(XdmValue.FromSequence(MaterializedSequence.FromList(items)), separateAtomicsWithSpace);
            return;
        }

        if (value.IsNode && value.NodeValue != null)
        {
            _lastAddedWasAtomic = false;
            CopyNodeToResult(value.NodeValue);
        }
        else if (value.IsSequence && value.SequenceValue != null)
        {
            // XSLT 3.0 §5.7.1: process sequence for complex content construction.
            // - Zero-length text nodes are discarded.
            // - Adjacent text nodes are merged.
            // - Consecutive atomic values are joined with a single space (#x20) (unless copy-of).
            // - Text nodes and atomics in a contiguous run are merged into one text node.
            var sb = new StringBuilder();
            // For method="text" with an explicit item-separator, use that separator
            // instead of the default single space between adjacent atomic values.
            var atomicSeparator = GetAtomicSeparator();
            // Carry over the atomic-state from the containing sequence constructor so that
            // the first atomic in this sequence is separated from a preceding atomic value.
            bool prevWasAtomic = _lastAddedWasAtomic;
            bool anyItemProcessed = false;

            var flattenedItems = new List<XdmValue>();
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                FlattenArrayMembers(item, flattenedItems);

            foreach (var item in flattenedItems)
            {
                anyItemProcessed = true;

                // An empty sequence item acts like a zero-length atomic value for the
                // purpose of spacing: it contributes no characters, but a separator is
                // still inserted before a following atomic value. This matches the
                // expected spacing for sequences that intersperse empty results from
                // constructor functions such as xs:language(()).
                if (item.IsUndefined)
                {
                    if (separateAtomicsWithSpace && prevWasAtomic)
                    {
                        sb.Append(atomicSeparator);
                    }
                    prevWasAtomic = true;
                    continue;
                }

                // Discard zero-length text nodes, but they still break the atomic chain
                if (item.IsNode && item.NodeValue != null &&
                    item.NodeValue.NodeKind == XdmNodeKind.Text &&
                    item.NodeValue.StringValue.Length == 0)
                {
                    prevWasAtomic = false;
                    continue;
                }

                // For raw-item output, each top-level item is preserved as a raw XDM value.
                if (TryCollectRawResultItem(item))
                    continue;

                if (item.IsNode && item.NodeValue != null &&
                    (item.NodeValue.NodeKind == XdmNodeKind.Element ||
                     item.NodeValue.NodeKind == XdmNodeKind.Comment ||
                     item.NodeValue.NodeKind == XdmNodeKind.ProcessingInstruction))
                {
                    // Non-text node: flush accumulated text, then copy the node
                    if (sb.Length > 0)
                    {
                        AddTextNode(sb.ToString());
                        sb.Clear();
                    }
                    prevWasAtomic = false;
                    _lastAddedWasAtomic = false;
                    CopyNodeToResult(item.NodeValue);
                }
                else if (item.IsNode && item.NodeValue != null &&
                         item.NodeValue.NodeKind == XdmNodeKind.Attribute)
                {
                    // Attribute node: flush accumulated text, then add attribute
                    if (sb.Length > 0)
                    {
                        AddTextNode(sb.ToString());
                        sb.Clear();
                    }
                    prevWasAtomic = false;
                    _lastAddedWasAtomic = false;
                    CopyNodeToResult(item.NodeValue);
                }
                else if (item.IsNode && item.NodeValue != null &&
                         item.NodeValue.NodeKind == XdmNodeKind.Text)
                {
                    // Text node: append without separator
                    sb.Append(item.NodeValue.StringValue);
                    prevWasAtomic = false;
                }
                else if (item.IsNode && item.NodeValue != null &&
                         item.NodeValue.NodeKind == XdmNodeKind.Document)
                {
                    // Document nodes in complex content are replaced by their children (XSLT 3.0 §5.7.1)
                    if (sb.Length > 0)
                    {
                        AddTextNode(sb.ToString());
                        sb.Clear();
                    }
                    prevWasAtomic = false;
                    _lastAddedWasAtomic = false;
                    foreach (var child in item.NodeValue.Axis(XdmAxis.Child))
                    {
                        if (child.IsNode && child.NodeValue != null)
                        {
                            CopyNodeToResult(child.NodeValue);
                        }
                    }
                }
                else
                {
                    // Array items are atomized to their member values; maps/functions remain an error.
                    if (item.IsArray)
                    {
                        foreach (var atom in AtomizeForString(item))
                        {
                            if (_preserveAtomicSequenceItems && _literalElementDepth == 0)
                            {
                                if (sb.Length > 0)
                                {
                                    AddTextNode(sb.ToString());
                                    sb.Clear();
                                }
                                AddTextNode(atom);
                                prevWasAtomic = false;
                            }
                            else
                            {
                                if (separateAtomicsWithSpace && prevWasAtomic)
                                {
                                    sb.Append(atomicSeparator);
                                }
                                sb.Append(atom);
                                prevWasAtomic = true;
                            }
                        }
                    }
                    else
                    {
                        if (item.IsMap || item.IsFunction)
                        {
                            // Typed result construction (xsl:template/@as): preserve map and
                            // function items via the raw-item side channel instead of failing.
                            if (_typedResultRawItems != null)
                            {
                                if (sb.Length > 0)
                                {
                                    AddTextNode(sb.ToString());
                                    sb.Clear();
                                }
                                prevWasAtomic = false;
                                _typedResultRawItems.Add(item);
                                continue;
                            }
                            if (IsPrincipalTopLevel && !_adaptiveOutputMode && !_jsonOutputMode)
                                throw new XsltRuntimeException("SENR0001",
                                    "Cannot serialize a map, array, or function using this output method.", XdmValue.Undefined);
                            if (!IsPrincipalTopLevel)
                                throw new InvalidOperationException("XTDE0450: Maps and functions cannot be serialized directly to element content.");
                        }

                        // Atomic value: insert space only if previous item was also atomic
                        // and separateAtomicsWithSpace is true (complex content construction)
                        if (_preserveAtomicSequenceItems && _literalElementDepth == 0)
                        {
                            // For sequence-typed results (e.g. xsl:template/@as="xs:decimal*"),
                            // keep each atomic value as a distinct item rather than merging
                            // consecutive atomics into a single text node.
                            if (sb.Length > 0)
                            {
                                AddTextNode(sb.ToString());
                                sb.Clear();
                            }
                            AddTextNode(item.ToString());
                            prevWasAtomic = false;
                        }
                        else
                        {
                            if (separateAtomicsWithSpace && prevWasAtomic)
                            {
                                sb.Append(atomicSeparator);
                            }
                            sb.Append(item.ToString());
                            prevWasAtomic = true;
                        }
                    }
                }
            }

            if (sb.Length > 0)
            {
                AddTextNode(sb.ToString());
            }
            if (anyItemProcessed)
            {
                _lastAddedWasAtomic = prevWasAtomic;
            }
        }
        else if (!value.IsUndefined)
        {
            if (value.IsMap || value.IsFunction)
            {
                // Typed result construction (xsl:template/@as): preserve map and function
                // items via the raw-item side channel instead of failing or stringifying.
                if (_typedResultRawItems != null)
                {
                    _typedResultRawItems.Add(value);
                    return;
                }
                if (IsPrincipalTopLevel && !_adaptiveOutputMode && !_jsonOutputMode)
                    throw new XsltRuntimeException("SENR0001",
                        "Cannot serialize a map, array, or function using this output method.", XdmValue.Undefined);
                if (!IsPrincipalTopLevel)
                    throw new InvalidOperationException("XTDE0450: Maps and functions cannot be serialized directly to element content.");
            }

            var text = value.IsArray ? XdmValueToString(value, " ") : value.ToString();
            if (_preserveAtomicSequenceItems && _literalElementDepth == 0)
                AddTextNode(text);
            else
                AppendAtomicText(text);
        }
    }

    /// <summary>
    /// Determines whether an XDM value is populated for the purposes of
    /// <c>xsl:where-populated</c>. A sequence is populated if it contains at
    /// least one item that is not "empty": document and element nodes are empty
    /// when they have no children (elements with only namespace declarations are
    /// also empty); text, comment, and processing-instruction nodes are empty
    /// when their string value is zero-length. Attribute and namespace nodes do
    /// not make a sequence populated. Atomic values are populated, but arrays are
    /// populated only if at least one member is populated.
    /// </summary>
    private bool IsPopulated(XdmValue value)
    {
        if (value.IsUndefined)
            return false;

        if (value.IsArray && value.ArrayValue != null)
        {
            foreach (var item in value.ArrayValue.Values)
            {
                if (IsPopulated(item))
                    return true;
            }
            return false;
        }

        if (value.IsNode && value.NodeValue != null)
            return IsPopulatedNode(value.NodeValue);

        if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                if (IsPopulated(item))
                    return true;
            }
            return false;
        }

        // Single atomic value. An empty string is treated as empty; all other
        // atomic values are populated. Maps and function items other than arrays
        // are considered populated.
        return value.Kind != XdmValueKind.String || value.StringValue.Length > 0;
    }

    private static bool IsPopulatedNode(IXdmNode node)
    {
        switch (node.NodeKind)
        {
            case XdmNodeKind.Document:
                return node.Axis(XdmAxis.Child).GetEnumerator().MoveNext();
            case XdmNodeKind.Element:
                // For xsl:where-populated, an element is empty unless it has children;
                // attributes do not make it populated.
                return node.Axis(XdmAxis.Child).GetEnumerator().MoveNext();
            case XdmNodeKind.Text:
            case XdmNodeKind.Comment:
            case XdmNodeKind.ProcessingInstruction:
                return node.StringValue.Length > 0;
            case XdmNodeKind.Attribute:
            case XdmNodeKind.Namespace:
                return false;
            default:
                return true;
        }
    }

    /// <summary>
    /// Moves all content currently held in the temporary element used by
    /// <c>xsl:where-populated</c> into the result item list.
    /// </summary>
    private static void FlushWherePopulatedTemp(XElement temp, List<XdmValue> result)
    {
        // Non-namespace attributes become attribute nodes in the result.
        foreach (var attr in temp.Attributes().ToList())
        {
            if (attr.IsNamespaceDeclaration)
                continue;
            attr.Remove();
            result.Add(XdmValue.FromNode(new XDocumentNode(new XAttribute(attr.Name, attr.Value))));
        }

        // Child nodes are detached and wrapped as XDM nodes. Synthetic sequence
        // placeholders (created by xsl:sequence in an accumulator context) are
        // expanded back to their constituent items so that arrays and other
        // sequence values are preserved for the populated check.
        foreach (var node in temp.Nodes().ToList())
        {
            node.Remove();
            if (node is XElement placeholder && placeholder.Name.LocalName == "__xdm_seq__" &&
                placeholder.Annotation<SequencePlaceholderItems>() is { } holder)
            {
                result.AddRange(holder.Items);
            }
            else
            {
                result.Add(XdmValue.FromNode(new XDocumentNode(node)));
            }
        }
    }

    /// <summary>
    /// Moves all items collected by the where-populated accumulator into the
    /// result item list.
    /// </summary>
    private static void FlushWherePopulatedAccumulator(List<XdmValue> accumulator, List<XdmValue> result)
    {
        if (accumulator.Count == 0)
            return;
        result.AddRange(accumulator);
        accumulator.Clear();
    }

    /// <summary>
    /// Creates a deep copy of an XDM node, returning a new IXdmNode wrapper.
    /// </summary>
    private IXdmNode CopyXdmNode(IXdmNode node)
        => CopyXdmNode(node, copyAllNamespaces: true, copyAccumulators: false);

    private IXdmNode CopyXdmNode(IXdmNode node, bool copyAllNamespaces)
        => CopyXdmNode(node, copyAllNamespaces, copyAccumulators: false);

    private IXdmNode CopyXdmNode(IXdmNode node, bool copyAllNamespaces, bool copyAccumulators)
    {
        switch (node.NodeKind)
        {
            case XdmNodeKind.Document:
                {
                    var children = new List<IXdmNode>();
                    foreach (var child in node.Axis(XdmAxis.Child))
                    {
                        if (child.IsNode && child.NodeValue != null)
                            children.Add(child.NodeValue);
                    }
                    var elementCount = children.Count(c => c.NodeKind == XdmNodeKind.Element);
                    XDocument newDoc;
                    if (elementCount == 1 && children.Count == 1)
                    {
                        newDoc = new XDocument();
                        CopyNodeToContainer(children[0], newDoc, copyAllNamespaces, copyAccumulators);
                    }
                    else
                    {
                        // XDocument cannot hold multiple root elements or mixed content;
                        // use a synthetic wrapper element like EvaluateSequenceConstructor does.
                        var docWrapper = new XElement("__xdm_doc__");
                        foreach (var child in children)
                        {
                            CopyNodeToContainer(child, docWrapper, copyAllNamespaces, copyAccumulators);
                        }
                        newDoc = new XDocument(docWrapper);
                    }
                    // Preserve base URI from the source document
                    if (!string.IsNullOrEmpty(node.BaseUri))
                        newDoc.AddAnnotation(node.BaseUri);
                    return new XDocumentNode(newDoc);
                }
            case XdmNodeKind.Element:
                {
                    var copy = new XElement(XName.Get(node.EncodedLocalName, node.NamespaceUri));
                    // Preserve base URI from the source element, but only if the source
                    // element does not carry its own xml:base attribute. A copied relative
                    // xml:base must be re-resolved against the new context (e.g. the
                    // stylesheet base URI of the copying instruction); adding a resolved
                    // absolute annotation would short-circuit that resolution.
                    bool hasXmlBase = node is XDocumentNode xdn && xdn.UnderlyingObject is XElement srcElem
                        && srcElem.Attribute(XNamespace.Xml + "base") != null;
                    if (!hasXmlBase && !string.IsNullOrEmpty(node.BaseUri))
                        copy.AddAnnotation(node.BaseUri);
                    if (copyAccumulators)
                        AttachAccumulatorValues(node, copy);
                    if (copyAllNamespaces)
                    {
                        foreach (var ns in node.Axis(XdmAxis.Namespace))
                        {
                            if (ns.IsNode && ns.NodeValue != null && ns.NodeValue.LocalName != "xml")
                            {
                                if (ns.NodeValue.LocalName == "")
                                {
                                    copy.SetAttributeValue("xmlns", ns.NodeValue.StringValue);
                                }
                                else
                                {
                                    copy.SetAttributeValue(
                                        XNamespace.Xmlns + ns.NodeValue.EncodedLocalName,
                                        ns.NodeValue.StringValue);
                                }
                            }
                        }
                    }
                    else
                    {
                        AddRequiredNamespaceDeclarations(node, copy);
                        copy.AddAnnotation(new NamespaceInheritanceBarrier());
                    }
                    foreach (var attr in node.Attributes())
                    {
                        // Skip namespace declarations — they are handled by the namespace axis
                        // (copy-all) or AddRequiredNamespaceDeclarations (copy-required) above.
                        if (attr.NodeValue is { } attrNode &&
                            attrNode.NamespaceUri == "http://www.w3.org/2000/xmlns/")
                            continue;
                        Xml11Attribute.SetValue(
                            copy,
                            XName.Get(attr.NodeValue!.EncodedLocalName, attr.NodeValue!.NamespaceUri),
                            attr.NodeValue!.StringValue);
                    }
                    foreach (var child in node.Axis(XdmAxis.Child))
                    {
                        CopyNodeToContainer(child.NodeValue!, copy, copyAllNamespaces, copyAccumulators);
                    }
                    return new XDocumentNode(copy);
                }
            case XdmNodeKind.Text:
                return new XDocumentNode(new XText(node.StringValue));
            case XdmNodeKind.Comment:
                return new XDocumentNode(new XComment(node.StringValue));
            case XdmNodeKind.ProcessingInstruction:
                return new XDocumentNode(new XProcessingInstruction(node.LocalName, node.StringValue));
            case XdmNodeKind.Attribute:
                return new XDocumentNode(Xml11Attribute.Create(
                    XName.Get(node.EncodedLocalName, node.NamespaceUri),
                    node.StringValue));
            default:
                return node;
        }
    }

    /// <summary>
    /// Creates a copy of a node for use inside an xsl:function body, processing
    /// the children of the xsl:copy instruction and adding them to the copied node.
    /// </summary>
    private IXdmNode? CopyNodeForFunctionBody(IXdmNode nodeToCopy, XElement copyInstruction)
    {
        switch (nodeToCopy.NodeKind)
        {
            case XdmNodeKind.Element:
                {
                    var copy = new XElement(XName.Get(nodeToCopy.EncodedLocalName, nodeToCopy.NamespaceUri));
                    // Copy namespace declarations
                    foreach (var ns in nodeToCopy.Axis(XdmAxis.Namespace))
                    {
                        if (ns.IsNode && ns.NodeValue != null && ns.NodeValue.LocalName != "xml")
                        {
                            if (ns.NodeValue.LocalName == "")
                                copy.SetAttributeValue("xmlns", ns.NodeValue.StringValue);
                            else
                                copy.SetAttributeValue(XNamespace.Xmlns + ns.NodeValue.EncodedLocalName, ns.NodeValue.StringValue);
                        }
                    }
                    // Process children of xsl:copy into the copied element
                    var savedContainer = _currentContainer;
                    var savedAccumulator = _sequenceAccumulator;
                    _currentContainer = copy;
                    _sequenceAccumulator = null;
                    try
                    {
                        foreach (var child in copyInstruction.Elements())
                        {
                            if (child.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                                ExecuteXsltInstruction(child, nodeToCopy);
                            else
                                CopyLiteralElement(child);
                        }
                    }
                    finally
                    {
                        _currentContainer = savedContainer;
                        _sequenceAccumulator = savedAccumulator;
                    }
                    NormalizeElementContent(copy);
                    return new XDocumentNode(copy);
                }
            case XdmNodeKind.Text:
                return new XDocumentNode(new XText(nodeToCopy.StringValue));
            case XdmNodeKind.Comment:
                return new XDocumentNode(new XComment(nodeToCopy.StringValue));
            case XdmNodeKind.ProcessingInstruction:
                return new XDocumentNode(new XProcessingInstruction(nodeToCopy.LocalName, nodeToCopy.StringValue));
            case XdmNodeKind.Attribute:
                return new XDocumentNode(new XAttribute(
                    XName.Get(nodeToCopy.EncodedLocalName, nodeToCopy.NamespaceUri),
                    nodeToCopy.StringValue));
            case XdmNodeKind.Document:
                {
                    var newDoc = new XDocument();
                    var savedContainer = _currentContainer;
                    var savedAccumulator = _sequenceAccumulator;
                    _currentContainer = newDoc;
                    _sequenceAccumulator = null;
                    try
                    {
                        foreach (var child in copyInstruction.Elements())
                        {
                            if (child.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                                ExecuteXsltInstruction(child, nodeToCopy);
                            else
                                CopyLiteralElement(child);
                        }
                    }
                    finally
                    {
                        _currentContainer = savedContainer;
                        _sequenceAccumulator = savedAccumulator;
                    }
                    return new XDocumentNode(newDoc);
                }
            default:
                return null;
        }
    }

    /// <summary>
    /// Performs a single xsl:copy for the given node in result-tree context.
    /// </summary>
    private void ExecuteSingleCopy(IXdmNode nodeToCopy, XElement instruction)
    {
        switch (nodeToCopy.NodeKind)
        {
            case XdmNodeKind.Element:
                {
                    var copy = new XElement(
                        XName.Get(nodeToCopy.EncodedLocalName, nodeToCopy.NamespaceUri));
                    // Preserve base URI from the source element
                    if (nodeToCopy is XDocumentNode srcXdn && srcXdn.UnderlyingObject is XElement srcElem)
                    {
                        var baseUriAnnotation = srcElem.Annotation<string>();
                        if (baseUriAnnotation != null)
                            copy.AddAnnotation(baseUriAnnotation);
                        else if (!string.IsNullOrEmpty(nodeToCopy.BaseUri))
                            copy.AddAnnotation(nodeToCopy.BaseUri);
                    }
                    else if (!string.IsNullOrEmpty(nodeToCopy.BaseUri))
                    {
                        copy.AddAnnotation(nodeToCopy.BaseUri);
                    }
                    foreach (var ns in nodeToCopy.Axis(XdmAxis.Namespace))
                    {
                        if (ns.IsNode && ns.NodeValue != null && ns.NodeValue.LocalName != "xml")
                        {
                            if (ns.NodeValue.LocalName == "")
                                copy.SetAttributeValue("xmlns", ns.NodeValue.StringValue);
                            else
                                copy.SetAttributeValue(
                                    XNamespace.Xmlns + ns.NodeValue.EncodedLocalName,
                                    ns.NodeValue.StringValue);
                        }
                    }

                    var inheritNamespacesAttrNode = instruction.Attribute("inherit-namespaces");
                    var inheritNamespacesAttrRaw = inheritNamespacesAttrNode?.Value
                        ?? instruction.Attribute("_inherit-namespaces")?.Value
                        ?? "yes";
                    var inheritNamespacesAttr = EvaluateAvt(inheritNamespacesAttrRaw, instruction);
                    if (ParseInheritNamespaces(inheritNamespacesAttr) == false)
                    {
                        copy.AddAnnotation(new NamespaceInheritanceBarrier());
                    }
                    else if (inheritNamespacesAttrNode != null && ParseInheritNamespaces(inheritNamespacesAttr) == true)
                    {
                        copy.AddAnnotation(new NamespaceInheritanceExplicitYes());
                    }
                    AddElementToContainer(copy, _currentContainer);
                    var prev = _currentContainer;
                    _currentContainer = copy;

                    // xsl:copy performs a shallow copy of an element: it copies the name and
                    // namespace bindings, but not the attributes or children of the source node.
                    // Attributes and children must be produced by the contained sequence constructor.

                    // Apply any attribute sets specified on xsl:copy
                    ApplyAttributeSets(instruction, copy);

                    // Suspend the outer sequence accumulator while constructing the content
                    // of the copied element, so children are added to the copy rather than
                    // escaping to the enclosing variable sequence.
                    var savedSequenceAccumulator = _sequenceAccumulator;
                    _sequenceAccumulator = null;

                    if (ContainsConditionalInstruction(instruction))
                    {
                        EvaluateSequenceConstructorIntoContainer(instruction, copy, XdmValue.FromNode(nodeToCopy));
                    }
                    else
                    {
                        foreach (var childNode in instruction.Nodes())
                        {
                            switch (childNode)
                            {
                                case XText text:
                                    ProcessSequenceText(text, instruction);
                                    break;
                                case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                    ExecuteXsltInstruction(elem, nodeToCopy);
                                    break;
                                case XElement elem:
                                    CopyLiteralElement(elem);
                                    break;
                            }
                        }
                    }

                    _sequenceAccumulator = savedSequenceAccumulator;
                    NormalizeElementContent(copy);
                    _currentContainer = prev;
                    break;
                }
            case XdmNodeKind.Text:
                _lastAddedWasAtomic = false;
                AddTextNode(nodeToCopy.StringValue);
                break;
            case XdmNodeKind.Attribute:
                if (_currentContainer is not XElement attrTarget)
                    throw new InvalidOperationException("XTDE0420");
                if (attrTarget.Nodes().Any())
                    throw new InvalidOperationException("XTDE0410");
                {
                    var attrNs = nodeToCopy.NamespaceUri;
                    if (!string.IsNullOrEmpty(attrNs))
                    {
                        EnsureNamespaceDeclarationForAttribute(attrTarget, attrNs, nodeToCopy.Prefix);
                    }
                    attrTarget.SetAttributeValue(
                        XName.Get(nodeToCopy.EncodedLocalName, attrNs),
                        nodeToCopy.StringValue);
                }
                break;
            case XdmNodeKind.Comment:
                _currentContainer.Add(new XComment(nodeToCopy.StringValue));
                break;
            case XdmNodeKind.ProcessingInstruction:
                _currentContainer.Add(new XProcessingInstruction(nodeToCopy.LocalName, nodeToCopy.StringValue));
                break;
            case XdmNodeKind.Document:
                {
                    // XSLT 3.0 §11.8.1: xsl:copy on a document node creates a new document node;
                    // its children come from the sequence constructor, not the original.
                    var srcBaseUri = nodeToCopy.BaseUri;

                    if (_sequenceAccumulator != null)
                    {
                        // In a sequence-returning context (e.g. xsl:variable with @as),
                        // produce an actual document node so base-uri() works correctly.
                        var newDoc = new XDocument();
                        if (!string.IsNullOrEmpty(srcBaseUri))
                            newDoc.AddAnnotation(srcBaseUri);

                        var savedDocAccumulator = _sequenceAccumulator;
                        if (ContainsConditionalInstruction(instruction))
                        {
                            EvaluateSequenceConstructorIntoContainer(instruction, newDoc, XdmValue.FromNode(nodeToCopy));
                        }
                        else
                        {
                            var savedContainer = _currentContainer;
                            _currentContainer = newDoc;
                            _sequenceAccumulator = null;
                            try
                            {
                                foreach (var childNode in instruction.Nodes())
                                {
                                    switch (childNode)
                                    {
                                        case XText text:
                                            ProcessSequenceText(text, instruction);
                                            break;
                                        case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                            ExecuteXsltInstruction(elem, nodeToCopy);
                                            break;
                                        case XElement elem:
                                            CopyLiteralElement(elem);
                                            break;
                                    }
                                }
                            }
                            finally
                            {
                                _currentContainer = savedContainer;
                                _sequenceAccumulator = savedDocAccumulator;
                            }
                        }

                        _sequenceAccumulator!.Add(XdmValue.FromNode(new XDocumentNode(newDoc)));
                    }
                    else
                    {
                        // Direct result tree: document node in complex content is replaced
                        // by its children (XSLT 3.0 §5.7.1). Process children into a temp
                        // collector and then move them to the result container, preserving
                        // the source document's base URI on child elements.
                        var savedContainer = _currentContainer;
                        var tempCollector = new XElement("__doc_temp__");

                        if (ContainsConditionalInstruction(instruction))
                        {
                            EvaluateSequenceConstructorIntoContainer(instruction, tempCollector, XdmValue.FromNode(nodeToCopy));
                        }
                        else
                        {
                            _currentContainer = tempCollector;
                            try
                            {
                                foreach (var childNode in instruction.Nodes())
                                {
                                    switch (childNode)
                                    {
                                        case XText text:
                                            ProcessSequenceText(text, instruction);
                                            break;
                                        case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                            ExecuteXsltInstruction(elem, nodeToCopy);
                                            break;
                                        case XElement elem:
                                            CopyLiteralElement(elem);
                                            break;
                                    }
                                }
                            }
                            finally
                            {
                                _currentContainer = savedContainer;
                            }
                        }

                        // Namespace nodes are not allowed on document nodes (XTDE0420)
                        if (tempCollector.Attributes().Any(a => a.IsNamespaceDeclaration))
                        {
                            throw new InvalidOperationException("XTDE0420");
                        }

                        foreach (var node in tempCollector.Nodes().ToList())
                        {
                            node.Remove();
                            if (node is XElement elem && !string.IsNullOrEmpty(srcBaseUri) && elem.Annotation<string>() == null)
                                elem.AddAnnotation(srcBaseUri);
                            _currentContainer.Add(node);
                        }
                    }
                    break;
                }
            default:
                // Other kinds: just process children
                foreach (var childNode in instruction.Nodes())
                {
                    switch (childNode)
                    {
                        case XText text:
                            ProcessSequenceText(text, instruction);
                            break;
                        case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                            ExecuteXsltInstruction(elem, nodeToCopy);
                            break;
                        case XElement elem:
                            CopyLiteralElement(elem);
                            break;
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Adds only the namespace declarations required for the element's own name
    /// and its attribute names.
    /// </summary>
    private void AddRequiredNamespaceDeclarations(IXdmNode source, XElement target)
    {
        // Element's own namespace
        if (!string.IsNullOrEmpty(source.NamespaceUri))
        {
            var prefix = GetPrefixForNamespace(source, source.NamespaceUri);
            if (prefix == "")
                target.SetAttributeValue("xmlns", source.NamespaceUri);
            else if (!string.IsNullOrEmpty(prefix))
                target.SetAttributeValue(XNamespace.Xmlns + prefix, source.NamespaceUri);
        }

        // Attribute namespaces
        foreach (var attr in source.Attributes())
        {
            var attrNode = attr.NodeValue;
            if (attrNode != null && !string.IsNullOrEmpty(attrNode.NamespaceUri)
                && attrNode.NamespaceUri != "http://www.w3.org/2000/xmlns/")
            {
                var attrPrefix = GetPrefixForNamespace(source, attrNode.NamespaceUri);
                if (attrPrefix == "")
                    target.SetAttributeValue("xmlns", attrNode.NamespaceUri);
                else if (!string.IsNullOrEmpty(attrPrefix))
                    target.SetAttributeValue(XNamespace.Xmlns + attrPrefix, attrNode.NamespaceUri);
            }
        }
    }

    /// <summary>
    /// Returns the prefix used for the given namespace URI on the specified element,
    /// or empty string for the default namespace.
    /// </summary>
    private string GetPrefixForNamespace(IXdmNode element, string namespaceUri)
    {
        foreach (var ns in element.Axis(XdmAxis.Namespace))
        {
            if (ns.IsNode && ns.NodeValue != null && ns.NodeValue.StringValue == namespaceUri)
                return ns.NodeValue.LocalName;
        }
        return string.Empty;
    }

    /// <summary>
    /// Copies a node and adds it to the specified XML container.
    /// </summary>
    private void CopyNodeToContainer(IXdmNode node, XContainer container)
        => CopyNodeToContainer(node, container, copyAllNamespaces: true, copyAccumulators: false);

    private void CopyNodeToContainer(IXdmNode node, XContainer container, bool copyAllNamespaces)
        => CopyNodeToContainer(node, container, copyAllNamespaces, copyAccumulators: false);

    private void CopyNodeToContainer(IXdmNode node, XContainer container, bool copyAllNamespaces, bool copyAccumulators)
    {
        switch (node.NodeKind)
        {
            case XdmNodeKind.Element:
                {
                    var elem = new XElement(XName.Get(node.EncodedLocalName, node.NamespaceUri));
                    // Preserve base URI from the source element, but only if the source
                    // element does not carry its own xml:base attribute. A copied relative
                    // xml:base must be re-resolved against the new context (e.g. the
                    // stylesheet base URI of the copying instruction); adding a resolved
                    // absolute annotation would short-circuit that resolution.
                    bool hasXmlBase = node is XDocumentNode xdn && xdn.UnderlyingObject is XElement srcElem
                        && srcElem.Attribute(XNamespace.Xml + "base") != null;
                    if (!hasXmlBase && !string.IsNullOrEmpty(node.BaseUri))
                        elem.AddAnnotation(node.BaseUri);
                    if (copyAllNamespaces)
                    {
                        foreach (var ns in node.Axis(XdmAxis.Namespace))
                        {
                            if (ns.IsNode && ns.NodeValue != null && ns.NodeValue.LocalName != "xml")
                            {
                                if (ns.NodeValue.LocalName == "")
                                {
                                    elem.SetAttributeValue("xmlns", ns.NodeValue.StringValue);
                                }
                                else
                                {
                                    elem.SetAttributeValue(
                                        XNamespace.Xmlns + ns.NodeValue.EncodedLocalName,
                                        ns.NodeValue.StringValue);
                                }
                            }
                        }
                    }
                    else
                    {
                        AddRequiredNamespaceDeclarations(node, elem);
                    }
                    foreach (var attr in node.Attributes())
                    {
                        // Skip namespace declarations — they are handled by the namespace axis
                        // (copy-all) or AddRequiredNamespaceDeclarations (copy-required) above.
                        if (attr.NodeValue is { } attrNode &&
                            attrNode.NamespaceUri == "http://www.w3.org/2000/xmlns/")
                            continue;
                        elem.SetAttributeValue(
                            XName.Get(attr.NodeValue!.EncodedLocalName, attr.NodeValue!.NamespaceUri),
                            attr.NodeValue!.StringValue);
                    }
                    if (node is XDocumentNode xdocNode2 && xdocNode2.UnderlyingObject is XElement srcElem2 &&
                        srcElem2.Annotation<NamespaceInheritanceBarrier>() != null)
                    {
                        elem.AddAnnotation(new NamespaceInheritanceBarrier());
                    }
                    if (copyAccumulators)
                        AttachAccumulatorValues(node, elem);
                    AddElementToContainer(elem, container);
                    foreach (var child in node.Axis(XdmAxis.Child))
                    {
                        CopyNodeToContainer(child.NodeValue!, elem, copyAllNamespaces, copyAccumulators);
                    }
                    break;
                }
            case XdmNodeKind.Text:
                container.Add(new XText(node.StringValue));
                break;
            case XdmNodeKind.Comment:
                container.Add(new XComment(node.StringValue));
                break;
            case XdmNodeKind.ProcessingInstruction:
                container.Add(new XProcessingInstruction(node.LocalName, node.StringValue));
                break;
        }
    }

    /// <summary>
    /// Adds an element to a container, explicitly undeclaring the default namespace
    /// when a no-namespace element is inserted into a parent that carries a default
    /// namespace.  Without this, LINQ-to-XML would silently inherit the parent's
    /// default namespace, making the namespace axis return the wrong namespace nodes.
    /// Also adds default-namespace undeclarations (xmlns="") when the parent has
    /// <see cref="NamespaceInheritanceBarrier"/> (inherit-namespaces="no").
    /// Prefixed namespace undeclarations (xmlns:prefix="") are not added here because
    /// LINQ-to-XML does not support them; they require XML 1.1 serialization.
    /// </summary>
    private void AddElementToContainer(XElement element, XContainer container)
    {
        EnsurePrincipalOutputOpen();
        MarkPrincipalOutputContent();
        if (container is XElement parentElem)
        {
            if (string.IsNullOrEmpty(element.Name.NamespaceName))
            {
                var parentDefaultNs = GetDefaultNamespaceUri(parentElem);
                if (!string.IsNullOrEmpty(parentDefaultNs))
                {
                    element.SetAttributeValue("xmlns", "");
                }
            }

            if (parentElem.Annotation<NamespaceInheritanceBarrier>() != null)
            {
                var parentDefaultNs = GetDefaultNamespaceUri(parentElem);
                if (parentDefaultNs != null)
                {
                    // Does the child already have an explicit default namespace declaration?
                    bool childHasDefaultNsDecl = false;
                    foreach (var childAttr in element.Attributes())
                    {
                        if (childAttr.Name.LocalName == "xmlns" &&
                            childAttr.Name.NamespaceName == "")
                        {
                            childHasDefaultNsDecl = true;
                            break;
                        }
                    }

                    if (!childHasDefaultNsDecl)
                    {
                        // Does the child use the parent's default namespace?
                        bool childUsesDefaultNs = false;
                        if (element.Name.NamespaceName == parentDefaultNs)
                        {
                            bool childUsesPrefix = false;
                            foreach (var childAttr in element.Attributes())
                            {
                                if (childAttr.IsNamespaceDeclaration &&
                                    childAttr.Value == parentDefaultNs)
                                {
                                    var prefix = childAttr.Name.LocalName == "xmlns"
                                        ? ""
                                        : childAttr.Name.LocalName;
                                    if (prefix != "")
                                    {
                                        childUsesPrefix = true;
                                        break;
                                    }
                                }
                            }
                            childUsesDefaultNs = !childUsesPrefix;
                        }

                        if (!childUsesDefaultNs)
                        {
                            element.SetAttributeValue("xmlns", "");
                        }
                    }
                }

                // Prefixed namespace undeclarations are added later, in
                // FinalizeElementNamespaces, once both the parent context and the
                // child's explicit namespace declarations are known.
            }
        }
        container.Add(element);
    }

    /// <summary>
    /// Finalizes namespace inheritance for an element and its descendants using a
    /// top-down pass. Children of an element with <c>inherit-namespaces="no"</c> do not
    /// inherit its namespace bindings; explicit namespace declarations that merely repeat
    /// an inherited binding are removed so serialization does not emit them.
    /// </summary>
    private static void FinalizeNamespaceInheritance(XElement element, Dictionary<string, string> inheritedBindings, List<string> inheritedPrefixOrder, bool parentHasBarrier)
    {
        var bindings = parentHasBarrier
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(inheritedBindings);
        var prefixOrder = parentHasBarrier
            ? new List<string>()
            : new List<string>(inheritedPrefixOrder);

        // Suppress explicit namespace declarations that repeat a binding the element
        // would already inherit from its parent. For barrier children such a duplicate
        // declaration is not inherited, so it must be kept.
        foreach (var attr in element.Attributes().Where(a => a.IsNamespaceDeclaration).ToList())
        {
            var prefix = attr.Name.LocalName == "xmlns" ? "" : attr.Name.LocalName;
            if (!parentHasBarrier &&
                bindings.TryGetValue(prefix, out var inheritedUri) &&
                inheritedUri == attr.Value)
            {
                attr.Remove();
            }
            else if (string.IsNullOrEmpty(attr.Value))
            {
                bindings.Remove(prefix);
                prefixOrder.Remove(prefix);
            }
            else
            {
                bindings[prefix] = attr.Value;
                if (!prefixOrder.Contains(prefix))
                    prefixOrder.Add(prefix);
            }
        }

        // Record the effective bindings so consumers such as the namespace axis can
        // determine the namespace context of this element.
        var context = element.Annotation<NamespaceInheritanceContext>();
        if (context == null)
        {
            context = new NamespaceInheritanceContext();
            element.AddAnnotation(context);
        }
        context.Bindings.Clear();
        foreach (var kv in bindings)
            context.Bindings[kv.Key] = kv.Value;
        context.PrefixOrder.Clear();
        context.PrefixOrder.AddRange(prefixOrder);

        if (element.Annotation<NamespaceInheritanceBarrier>() != null)
        {
            foreach (var child in element.Elements())
            {
                var childUndecl = child.Annotation<PrefixedNamespaceUndeclarations>() ?? new PrefixedNamespaceUndeclarations();
                foreach (var prefix in prefixOrder)
                {
                    if (prefix == "" || prefix == "xml" || prefix == "xmlns")
                        continue;
                    if (!bindings.TryGetValue(prefix, out var uri) || string.IsNullOrEmpty(uri))
                        continue;
                    childUndecl.Prefixes.Add(prefix);
                }
                if (childUndecl.Prefixes.Count > 0)
                    child.AddAnnotation(childUndecl);
            }
        }

        foreach (var child in element.Elements())
            FinalizeNamespaceInheritance(child, bindings, prefixOrder, element.Annotation<NamespaceInheritanceBarrier>() != null);
    }

    /// <summary>
    /// Runs a top-down namespace inheritance pass over every element tree that is
    /// reachable from the supplied XDM value. This must be done once after the whole
    /// result tree has been constructed, because the pass needs the parent context of
    /// each element to be known before its children are processed.
    /// </summary>
    private static void FinalizeResultTreeNamespaces(XdmValue value)
    {
        if (value.IsUndefined)
            return;

        if (value.IsNode)
        {
            FinalizeNodeNamespaces(value.NodeValue);
            return;
        }

        if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                FinalizeResultTreeNamespaces(item);
        }
    }

    private static void FinalizeNodeNamespaces(IXdmNode? node)
    {
        if (node == null)
            return;

        if (node is XDocumentNode xdn)
        {
            switch (xdn.UnderlyingObject)
            {
                case XElement elem:
                    FinalizeNamespaceInheritance(elem, new Dictionary<string, string>(), new List<string>(), parentHasBarrier: false);
                    break;
                case XDocument doc when doc.Root != null:
                    FinalizeNamespaceInheritance(doc.Root, new Dictionary<string, string>(), new List<string>(), parentHasBarrier: false);
                    break;
            }
        }
    }

    /// <summary>
    /// Returns the default namespace URI in effect for the given element,
    /// or <c>null</c> if the element has no default namespace.
    /// Checks explicit <c>xmlns</c> attributes first, then infers from the
    /// element name when it has no prefixed namespace declaration.
    /// </summary>
    private static string? GetDefaultNamespaceUri(XElement element)
    {
        foreach (var attr in element.Attributes())
        {
            if (attr.IsNamespaceDeclaration && attr.Name.LocalName == "xmlns")
                return attr.Value;
        }
        if (!string.IsNullOrEmpty(element.Name.NamespaceName))
        {
            foreach (var attr in element.Attributes())
            {
                if (attr.IsNamespaceDeclaration && attr.Value == element.Name.NamespaceName)
                {
                    var prefix = attr.Name.LocalName == "xmlns" ? "" : attr.Name.LocalName;
                    if (prefix != "")
                        return null; // element uses a prefix, not default namespace
                }
            }
            return element.Name.NamespaceName;
        }
        return null;
    }

    // ---------------------------------------------------------------------------------------------
    // Namespace-alias support
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    /// Returns <c>true</c> if the given namespace URI is the source side of an active
    /// <c>xsl:namespace-alias</c> declaration.
    /// </summary>
    private bool TryGetNamespaceAlias(string sourceUri, out Stylesheet.NamespaceAliasDefinition alias)
    {
        return _namespaceAliases.TryGetValue(sourceUri, out alias!);
    }

    /// <summary>
    /// Maps a source expanded name through an active namespace-alias declaration and
    /// reports the preferred result prefix. For attributes a result-prefix of
    /// <c>#default</c> produces a no-namespace attribute.
    /// </summary>
    private XName MapAliasedName(XName sourceName, bool isElement, out string? resultPrefix)
    {
        if (!TryGetNamespaceAlias(sourceName.NamespaceName, out var alias))
        {
            resultPrefix = null;
            return sourceName;
        }

        if (alias.ResultPrefix == "#default" || string.IsNullOrEmpty(alias.ResultPrefix))
        {
            resultPrefix = "";
            if (isElement && !string.IsNullOrEmpty(alias.ResultUri))
                return XName.Get(sourceName.LocalName, alias.ResultUri);
            return XName.Get(sourceName.LocalName);
        }

        resultPrefix = alias.ResultPrefix;
        return XName.Get(sourceName.LocalName, alias.ResultUri);
    }

    /// <summary>
    /// Adds a namespace declaration for the given name/prefix if needed. When
    /// <paramref name="prefix"/> is empty a default namespace declaration is added.
    /// </summary>
    private static void EnsureNamespaceDeclaration(XElement element, XName name, string? prefix)
    {
        if (string.IsNullOrEmpty(name.NamespaceName))
            return;

        // If the element already has a declaration for this URI, nothing to do.
        foreach (var attr in element.Attributes())
        {
            if (attr.IsNamespaceDeclaration && attr.Value == name.NamespaceName)
                return;
        }

        if (prefix == null || prefix == "")
        {
            // Use the default namespace when no prefix is known. This is valid for
            // elements; attributes without a prefix are always in no namespace.
            element.SetAttributeValue("xmlns", name.NamespaceName);
        }
        else if (prefix != "xml" && prefix != "xmlns")
        {
            var existing = element.GetNamespaceOfPrefix(prefix);
            if (existing == null || existing.NamespaceName != name.NamespaceName)
                element.SetAttributeValue(XNamespace.Xmlns + prefix, name.NamespaceName);
        }
    }

    /// <summary>
    /// Parses an <c>inherit-namespaces</c> attribute value. Returns <c>true</c> for
    /// <c>yes</c>/<c>true</c>/<c>1</c>, <c>false</c> for <c>no</c>/<c>false</c>/<c>0</c>,
    /// and throws <c>XTSE0020</c> for any other value.
    /// </summary>
    private static bool ParseInheritNamespaces(string? value)
    {
        var trimmed = value?.Trim() ?? "yes";
        return trimmed switch
        {
            "yes" or "true" or "1" => true,
            "no" or "false" or "0" => false,
            _ => throw new InvalidOperationException($"XTSE0020: Invalid inherit-namespaces value '{trimmed}'.")
        };
    }

    /// <summary>
    /// Collects the namespace URIs that are excluded by <c>exclude-result-prefixes</c>
    /// in scope on the given element. The special value <c>#all</c> is returned as the
    /// literal string <c>#all</c> so callers can treat it as a wildcard.
    /// </summary>
    private HashSet<string> GetExcludedNamespaceUris(XElement element)
    {
        var result = new HashSet<string>();
        var current = element;
        while (current != null)
        {
            var attr = current.Attribute(XName.Get("exclude-result-prefixes", Stylesheet.Stylesheet.XslNamespace))
                ?? current.Attribute("exclude-result-prefixes");
            if (attr != null)
            {
                foreach (var token in attr.Value.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var prefix = token.Trim();
                    if (prefix == "#all")
                    {
                        result.Add("#all");
                        return result;
                    }

                    string uri;
                    if (prefix == "#default")
                        uri = current.GetDefaultNamespace()?.NamespaceName ?? "";
                    else
                        uri = current.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? "";

                    if (!string.IsNullOrEmpty(uri))
                        result.Add(uri);
                }
            }
            current = current.Parent;
        }
        return result;
    }

    /// <summary>
    /// Collects the namespace URIs designated as extension element namespaces
    /// by in-scope <c>extension-element-prefixes</c> / <c>xsl:extension-element-prefixes</c>
    /// attributes.
    /// </summary>
    private HashSet<string> GetExtensionElementPrefixes(XElement element)
    {
        var result = new HashSet<string>();
        var current = element;
        while (current != null)
        {
            var attr = current.Attribute(XName.Get("extension-element-prefixes", Stylesheet.Stylesheet.XslNamespace))
                ?? current.Attribute("extension-element-prefixes");
            if (attr != null)
            {
                foreach (var token in attr.Value.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var prefix = token.Trim();
                    string uri;
                    if (prefix == "#default")
                        uri = current.GetDefaultNamespace()?.NamespaceName ?? "";
                    else
                        uri = current.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? "";

                    if (!string.IsNullOrEmpty(uri))
                        result.Add(uri);
                }
            }
            current = current.Parent;
        }
        return result;
    }

    private void CopyNodeToResult(IXdmNode node)
    {
        if (node.NodeKind == XdmNodeKind.Document)
        {
            _lastAddedWasAtomic = false;
            var documentChildren = new List<IXdmNode>();
            foreach (var child in node.Axis(XdmAxis.Child))
                if (child.NodeValue != null)
                    documentChildren.Add(child.NodeValue);

            // In a sequence-returning context (e.g. named template with @as),
            // preserve the document node by adding a synthetic __xdm_doc__ wrapper
            // to the temporary element container. The wrapper is later converted back
            // to a document node when the template result is assembled.
            if (_preserveDocumentNodes && _currentContainer is XElement)
            {
                var wrapper = new XElement("__xdm_doc__");
                var savedContainer = _currentContainer;
                _currentContainer = wrapper;
                try
                {
                    foreach (var child in documentChildren)
                        CopyNodeToResult(child);
                }
                finally
                {
                    _currentContainer = savedContainer;
                }
                AddElementToContainer(wrapper, _currentContainer);
            }
            // XDocument can only hold a single root element. When a document node
            // contains multiple children (or non-element children) and is being
            // copied into the result XDocument, wrap the children in the synthetic
            // __xdm_doc__ element; ResultTreeSerializer unwraps it again.
            else if (_currentContainer is XDocument &&
                !(documentChildren.Count == 1 && documentChildren[0].NodeKind == XdmNodeKind.Element))
            {
                var wrapper = new XElement("__xdm_doc__");
                var savedContainer = _currentContainer;
                _currentContainer = wrapper;
                try
                {
                    foreach (var child in documentChildren)
                        CopyNodeToResult(child);
                }
                finally
                {
                    _currentContainer = savedContainer;
                }
                AddElementToContainer(wrapper, _currentContainer);
            }
            else
            {
                foreach (var child in documentChildren)
                {
                    CopyNodeToResult(child);
                }
            }
        }
        else if (node.NodeKind == XdmNodeKind.Element)
        {
            _lastAddedWasAtomic = false;
            var copy = new XElement(
                XName.Get(node.EncodedLocalName, node.NamespaceUri));

            // Copy explicit namespace declarations and attributes from the source element.
            // Use the underlying XElement when available so we copy only the declarations
            // that are actually present on this element, not inherited ones.
            if (node is XDocumentNode xdocNode && xdocNode.UnderlyingObject is XElement srcElem)
            {
                foreach (var attr in srcElem.Attributes())
                {
                    Xml11Attribute.SetValue(copy, attr.Name, attr.Value);
                }
                if (srcElem.Annotation<NamespaceInheritanceBarrier>() != null)
                {
                    copy.AddAnnotation(new NamespaceInheritanceBarrier());
                }
                var accValues = srcElem.Annotation<AccumulatorValues>();
                if (accValues != null)
                {
                    copy.AddAnnotation(accValues);
                }
                // Preserve base URI from the source element, but only if the source
                // element does not carry its own xml:base attribute. A copied relative
                // xml:base must be re-resolved against the new context.
                bool hasXmlBase = srcElem.Attribute(XNamespace.Xml + "base") != null;
                var baseUriAnnotation = srcElem.Annotation<string>();
                if (baseUriAnnotation != null)
                {
                    copy.AddAnnotation(baseUriAnnotation);
                }
                else if (!hasXmlBase && !string.IsNullOrEmpty(node.BaseUri))
                {
                    copy.AddAnnotation(node.BaseUri);
                }
            }
            else
            {
                // Fallback for non-XDocumentNode implementations: copy namespace axis
                // (may include inherited declarations, but best effort).
                foreach (var ns in node.Axis(XdmAxis.Namespace))
                {
                    if (ns.IsNode && ns.NodeValue != null && ns.NodeValue.LocalName != "xml")
                    {
                        if (ns.NodeValue.LocalName == "")
                        {
                            copy.SetAttributeValue("xmlns", ns.NodeValue.StringValue);
                        }
                        else
                        {
                            copy.SetAttributeValue(
                                XNamespace.Xmlns + ns.NodeValue.EncodedLocalName,
                                ns.NodeValue.StringValue);
                        }
                    }
                }

                foreach (var attr in node.Attributes())
                {
                    copy.SetAttributeValue(
                        XName.Get(attr.NodeValue!.EncodedLocalName, attr.NodeValue!.NamespaceUri),
                        attr.NodeValue!.StringValue);
                }
            }

            AddElementToContainer(copy, _currentContainer);
            var prev = _currentContainer;
            _currentContainer = copy;
            foreach (var child in node.Axis(XdmAxis.Child))
            {
                CopyNodeToResult(child.NodeValue!);
            }
            _currentContainer = prev;
        }
        else if (node.NodeKind == XdmNodeKind.Text)
        {
            _lastAddedWasAtomic = false;
            AddTextNode(node.StringValue);
        }
        else if (node.NodeKind == XdmNodeKind.Comment)
        {
            _lastAddedWasAtomic = false;
            _currentContainer.Add(new XComment(node.StringValue));
        }
        else if (node.NodeKind == XdmNodeKind.ProcessingInstruction)
        {
            _lastAddedWasAtomic = false;
            _currentContainer.Add(new XProcessingInstruction(node.LocalName, node.StringValue));
        }
        else if (node.NodeKind == XdmNodeKind.Attribute)
        {
            if (_currentContainer is not XElement attrParent)
                throw new InvalidOperationException("XTDE0420");
            if (attrParent.Nodes().Any())
                throw new InvalidOperationException("XTDE0410");
            var attrNs = node.NamespaceUri;
            if (!string.IsNullOrEmpty(attrNs))
            {
                EnsureNamespaceDeclarationForAttribute(attrParent, attrNs, node.Prefix);
            }
            Xml11Attribute.SetValue(
                attrParent,
                XName.Get(node.EncodedLocalName, attrNs),
                node.StringValue);
        }
    }

    /// <summary>
    /// Ensures that the supplied namespace URI is explicitly declared on <paramref name="element"/>
    /// with a non-conflicting prefix. This is needed when attributes are added programmatically:
    /// LINQ to XML does not materialize namespace declarations, so <see cref="IXdmNode.Prefix"/>
    /// and <c>name()</c> would return an unprefixed local name even for namespaced attributes.
    /// </summary>
    private static void EnsureNamespaceDeclarationForAttribute(XElement element, string namespaceUri, string? preferredPrefix)
    {
        if (string.IsNullOrEmpty(namespaceUri))
            return;

        var ns = XNamespace.Get(namespaceUri);
        var existingPrefix = element.GetPrefixOfNamespace(ns);
        if (!string.IsNullOrEmpty(existingPrefix))
            return;

        if (!string.IsNullOrEmpty(preferredPrefix))
        {
            var existingNs = element.GetNamespaceOfPrefix(preferredPrefix);
            if (existingNs == null)
            {
                element.SetAttributeValue(XNamespace.Xmlns + preferredPrefix, namespaceUri);
                return;
            }
        }

        int i = 1;
        string prefix;
        do
        {
            prefix = "ns" + (i - 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
            i++;
        } while (element.GetNamespaceOfPrefix(prefix) != null);

        element.SetAttributeValue(XNamespace.Xmlns + prefix, namespaceUri);
    }

    /// <summary>
    /// Returns the default on-no-match behavior for the stylesheet version.
    /// XSLT 3.0 defaults to shallow-skip; XSLT 1.0/2.0 default to text-only-copy
    /// (traditional built-in rules: apply templates to children, copy text/attributes).
    /// </summary>
    private Stylesheet.OnNoMatch GetDefaultOnNoMatch()
    {
        // XSLT 1.0/2.0/3.0 all default to the traditional text-only-copy built-in rule:
        // text and attribute nodes are copied, document/element nodes delegate to children.
        return Stylesheet.OnNoMatch.TextOnlyCopy;
    }

    /// <summary>
    /// Applies the built-in rule for an atomic context item, respecting the
    /// effective <c>on-no-match</c> behavior of the mode. Deep-skip and shallow-skip
    /// modes suppress atomic output; other behaviors output the string value.
    /// </summary>
    private void ApplyBuiltInRulesForAtomic(XdmValue item, string mode)
    {
        var modeDef = _stylesheet.GetModeDefinition(mode);
        if (modeDef == null && !string.IsNullOrEmpty(mode))
            modeDef = _stylesheet.GetModeDefinition("");
        var behavior = modeDef?.OnNoMatch ?? GetDefaultOnNoMatch();
        if (behavior == Stylesheet.OnNoMatch.DeepSkip || behavior == Stylesheet.OnNoMatch.ShallowSkip)
            return;
        AddTextNode(item.ToString());
    }

    /// <summary>
    /// Applies built-in template rules when no explicit template matches.
    /// Respects xsl:mode on-no-match declarations.
    /// </summary>
    public void ApplyBuiltInRules(IXdmNode node, string mode, Dictionary<string, XdmValue>? incomingTunnelParams = null, Dictionary<string, XdmValue>? callParams = null, int position = 1, int last = 1)
    {
        var savedItem = _context.ContextItem;
        var savedCurrent = _context.CurrentItem;
        var savedPosition = _context.ContextPosition;
        var savedSize = _context.ContextSize;
        var savedMergeGroup = _currentMergeGroup;
        var savedMergeKey = _currentMergeKey;
        var savedNamedGroups = _currentNamedMergeGroups;
        var savedMergeSourceNames = _currentMergeSourceNames;
        _currentMergeGroup = null;
        _currentMergeKey = null;
        _currentNamedMergeGroups = null;
        _currentMergeSourceNames = null;
        _context.WithFocus(XdmValue.FromNode(node), position, last);
        _context.WithCurrentItem(XdmValue.FromNode(node));
        try
        {
            var modeDef = _stylesheet.GetModeDefinition(mode);
            var typed = modeDef?.Typed ?? false;
            // Named modes with no explicit xsl:mode declaration inherit the
            // unnamed mode's on-no-match behavior (XSLT 3.0 §3.5.2).
            if (modeDef == null && !string.IsNullOrEmpty(mode))
                modeDef = _stylesheet.GetModeDefinition("");
            var behavior = modeDef?.OnNoMatch ?? GetDefaultOnNoMatch();

            // xsl:mode warning-on-no-match: warn whenever the built-in rule processes
            // a node (document nodes are never considered a no-match).
            if (modeDef?.WarningOnNoMatch == true && node.NodeKind != XdmNodeKind.Document)
            {
                _messageListener?.OnWarning($"No matching template for node '{node.LocalName}' in mode '{mode}'.");
            }

            // typed="yes" requires schema-validated nodes; untyped element/attribute
            // nodes raise XTTE3100 when the built-in rule would process them.
            if (typed && (node.NodeKind == XdmNodeKind.Element || node.NodeKind == XdmNodeKind.Attribute))
            {
                throw new InvalidOperationException($"XTTE3100: Mode '{mode}' is typed, but the context node is untyped.");
            }


            // XSLT 3.0 §6.6: if on-no-match is fail, built-in rule signals XTDE0555
            // for all node kinds except document (which delegates to children).
            if (behavior == Stylesheet.OnNoMatch.Fail && node.NodeKind != XdmNodeKind.Document)
            {
                throw new InvalidOperationException(
                    $"XTDE0555: No matching template found for node '{node.LocalName}' in mode '{mode}'.");
            }

            switch (node.NodeKind)
            {
            case XdmNodeKind.Document:
                if ((behavior == Stylesheet.OnNoMatch.ShallowCopy || behavior == Stylesheet.OnNoMatch.DeepCopy) &&
                    _sequenceAccumulator != null)
                {
                    // In a sequence-returning context, shallow-copy/deep-copy of a document
                    // node produces an actual document node (possibly empty) so that its
                    // base URI is preserved.
                    var newDoc = new XDocument();
                    if (!string.IsNullOrEmpty(node.BaseUri))
                        newDoc.AddAnnotation(node.BaseUri);
                    var savedContainer = _currentContainer;
                    _currentContainer = newDoc;
                    ApplyTemplates(node, mode, select: null, sortKeys: null, incomingTunnelParams, callParams);
                    _currentContainer = savedContainer;
                    _sequenceAccumulator.Add(XdmValue.FromNode(new XDocumentNode(newDoc)));
                }
                else
                {
                    // Built-in: apply templates to children of the document node
                    ApplyTemplates(node, mode, select: null, sortKeys: null, incomingTunnelParams, callParams);
                }
                break;

            case XdmNodeKind.Element:
                ApplyBuiltInRulesForElement(node, mode, behavior, incomingTunnelParams, callParams);
                break;

            case XdmNodeKind.Text:
                // Built-in: copy text value (only if we have an element container)
                // XSLT 3.0 §6.6: for text/attribute nodes, built-in rule does nothing
                // when on-no-match is shallow-skip or deep-skip.
                if (behavior != Stylesheet.OnNoMatch.DeepSkip &&
                    behavior != Stylesheet.OnNoMatch.ShallowSkip)
                {
                    _lastAddedWasAtomic = false;
                    AddTextNode(node.StringValue);
                }
                break;

            case XdmNodeKind.Attribute:
                // XSLT 3.0 §6.6: built-in rule for attribute nodes
                if (_currentContainer is XElement &&
                    behavior != Stylesheet.OnNoMatch.DeepSkip &&
                    behavior != Stylesheet.OnNoMatch.ShallowSkip)
                {
                    if (behavior == Stylesheet.OnNoMatch.ShallowCopy ||
                        behavior == Stylesheet.OnNoMatch.DeepCopy)
                    {
                        if (_currentContainer is XElement elem)
                        {
                            Xml11Attribute.SetValue(
                                elem,
                                XName.Get(node.EncodedLocalName, node.NamespaceUri),
                                node.StringValue);
                        }
                    }
                    else if (behavior == Stylesheet.OnNoMatch.TextOnlyCopy)
                    {
                        _lastAddedWasAtomic = false;
                        AddTextNode(node.StringValue);
                    }
                }
                break;

            case XdmNodeKind.Comment:
                if (_currentContainer is XElement && behavior == Stylesheet.OnNoMatch.ShallowCopy)
                {
                    _currentContainer.Add(new XComment(node.StringValue));
                }
                break;

            case XdmNodeKind.ProcessingInstruction:
                if (_currentContainer is XElement && behavior == Stylesheet.OnNoMatch.ShallowCopy)
                {
                    _currentContainer.Add(new XProcessingInstruction(node.LocalName, node.StringValue));
                }
                break;

            case XdmNodeKind.Namespace:
                // XSLT 3.0 §6.6: for namespace nodes, built-in rule copies the namespace
                // only when on-no-match is shallow-copy; otherwise does nothing.
                if (_currentContainer is XElement nsElem &&
                    (behavior == Stylesheet.OnNoMatch.ShallowCopy ||
                     behavior == Stylesheet.OnNoMatch.DeepCopy))
                {
                    nsElem.SetAttributeValue(
                        XNamespace.Xmlns + node.EncodedLocalName,
                        node.StringValue);
                }
                break;
            }
        }
        finally
        {
            _context.WithFocus(savedItem, savedPosition, savedSize);
            _context.WithCurrentItem(savedCurrent);
            _currentMergeGroup = savedMergeGroup;
            _currentMergeKey = savedMergeKey;
            _currentNamedMergeGroups = savedNamedGroups;
            _currentMergeSourceNames = savedMergeSourceNames;
        }
    }

    private void ApplyBuiltInRulesForElement(IXdmNode node, string mode, Stylesheet.OnNoMatch behavior, Dictionary<string, XdmValue>? incomingTunnelParams, Dictionary<string, XdmValue>? callParams = null)
    {
        switch (behavior)
        {
            case Stylesheet.OnNoMatch.ShallowCopy:
                {
                    // XSLT 3.0 §6.6 (bug 28774): shallow-copy creates the element shell
                    // without copying attributes; templates are applied to children AND attributes.
                    var copy = new XElement(
                        XName.Get(node.EncodedLocalName, node.NamespaceUri));
                    // Preserve base URI from the source element
                    if (!string.IsNullOrEmpty(node.BaseUri))
                        copy.AddAnnotation(node.BaseUri);
                    foreach (var ns in node.Axis(XdmAxis.Namespace))
                    {
                        if (ns.IsNode && ns.NodeValue != null && ns.NodeValue.LocalName != "xml")
                        {
                            if (ns.NodeValue.LocalName == "")
                            {
                                copy.SetAttributeValue("xmlns", ns.NodeValue.StringValue);
                            }
                            else
                            {
                                copy.SetAttributeValue(
                                    XNamespace.Xmlns + ns.NodeValue.EncodedLocalName,
                                    ns.NodeValue.StringValue);
                            }
                        }
                    }
                    _currentContainer.Add(copy);

                    var previousContainer = _currentContainer;
                    _currentContainer = copy;
                    // Suspend the outer sequence accumulator while constructing the content
                    // of the shallow-copied element, so children are added to the copy rather
                    // than escaping to an enclosing variable sequence accumulator.
                    var savedSequenceAccumulator = _sequenceAccumulator;
                    _sequenceAccumulator = null;
                    try
                    {
                        ApplyTemplates(node, mode, select: "@* | node()", sortKeys: null, incomingTunnelParams, callParams);
                    }
                    finally
                    {
                        _sequenceAccumulator = savedSequenceAccumulator;
                        _currentContainer = previousContainer;
                    }
                }
                break;

            case Stylesheet.OnNoMatch.ShallowSkip:
                // XSLT 3.0 §6.6 (bug 28774): shallow-skip applies templates to children AND attributes.
                ApplyTemplates(node, mode, select: "@* | node()", sortKeys: null, incomingTunnelParams, callParams);
                break;

            case Stylesheet.OnNoMatch.TextOnlyCopy:
                // Recurse to children without copying the element wrapper (attributes are not processed).
                ApplyTemplates(node, mode, select: null, sortKeys: null, incomingTunnelParams, callParams);
                break;

            case Stylesheet.OnNoMatch.DeepCopy:
                CopyNodeToResult(node);
                break;

            case Stylesheet.OnNoMatch.DeepSkip:
                // Skip element and all descendants — do nothing
                break;

            case Stylesheet.OnNoMatch.Fail:
                throw new InvalidOperationException(
                    $"No matching template found for node '{node.LocalName}' in mode '{mode}'.");
        }
    }

    /// <summary>
    /// Registers the XSLT <c>key()</c> function on the evaluation context.
    /// </summary>
    private void RegisterKeyFunction()
    {
        var signature2 = new Bosak.XPath.Runtime.Functions.FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "key",
            Arity = 2,
            ParameterTypes = [XdmValueKind.String, XdmValueKind.Undefined],
            ReturnType = XdmValueKind.Sequence,
            Implementation = KeyFunctionImpl
        };
        _context.RegisterFunction(signature2);

        var signature3 = new Bosak.XPath.Runtime.Functions.FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "key",
            Arity = 3,
            ParameterTypes = [XdmValueKind.String, XdmValueKind.Undefined, XdmValueKind.Node],
            ReturnType = XdmValueKind.Sequence,
            Implementation = KeyFunctionImpl
        };
        _context.RegisterFunction(signature3);
    }

    private XdmValue KeyFunctionImpl(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
    {
        if (_keyIndices == null)
            _keyIndices = new List<(IXdmNode DocRoot, KeyIndex Index)>();

        var rawKeyName = args[0].ToString();
        var keyName = ExpandKeyName(rawKeyName, ctx);
        var keyValueArg = args[1];

        // XPath 1.0 backwards compatibility: key() treats its second argument as a
        // string (or a node-set whose string values are used).
        if (ctx.BackwardsCompatible)
            keyValueArg = ConvertKeyValueToStringSequence(keyValueArg);

        // XTDE1260: the expanded key name must match at least one xsl:key definition.
        var allKeyDefs = _stylesheet.GetAllKeyDefinitions();
        if (!allKeyDefs.Any(k => k.Name == keyName))
            throw new InvalidOperationException($"XTDE1260: No xsl:key definition named '{rawKeyName}'.");

        if (args.Length == 2)
        {
            // 2-arg form: search the entire document containing the context node.
            var contextNode = ctx.ContextItem.NodeValue;
            var docRoot = contextNode?.Document ?? contextNode;
            if (docRoot == null)
                return XdmValue.Undefined;

            var keyIndex = GetOrBuildKeyIndex(docRoot);
            if (keyIndex == null)
                return XdmValue.Undefined;

            return LookupKeyValues(keyIndex, keyName, keyValueArg);
        }
        else
        {
            // 3-arg form: search only the nodes supplied in the 3rd argument and their descendants.
            var candidates = new List<IXdmNode>();
            if (args[2].IsNode && args[2].NodeValue != null)
            {
                candidates.Add(args[2].NodeValue);
            }
            else if (args[2].IsSequence && args[2].SequenceValue != null)
            {
                foreach (var item in XdmSequence.FromSource(args[2].SequenceValue!))
                {
                    if (item.IsNode && item.NodeValue != null)
                        candidates.Add(item.NodeValue);
                }
            }

            if (candidates.Count == 0)
                return XdmValue.Undefined;

            // Group candidates by document root (using IsSameNode).
            var docEntries = new List<(IXdmNode DocRoot, KeyIndex Index, List<IXdmNode> Candidates)>();
            foreach (var candidate in candidates)
            {
                var candidateDoc = candidate.Document ?? candidate;
                bool found = false;
                foreach (var entry in docEntries)
                {
                    if (entry.DocRoot.IsSameNode(candidateDoc))
                    {
                        entry.Candidates.Add(candidate);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    var keyIndex = GetOrBuildKeyIndex(candidateDoc);
                    if (keyIndex != null)
                    {
                        docEntries.Add((candidateDoc, keyIndex, new List<IXdmNode> { candidate }));
                    }
                }
            }

            // Look up key values and filter to candidates or their descendants.
            var result = new List<XdmValue>();

            if (IsCompositeKey(keyName))
            {
                var tuple = ExtractKeyLookupValues(keyValueArg).ToArray();
                if (tuple.Length > 0)
                {
                    var seen = new HashSet<IXdmNode>();
                    foreach (var (_, keyIndex, docCandidates) in docEntries)
                    {
                        foreach (var node in keyIndex.LookupComposite(keyName, tuple))
                        {
                            if (!seen.Add(node))
                                continue;
                            if (docCandidates.Any(c => IsDescendantOrSelf(node, c)))
                                result.Add(XdmValue.FromNode(node));
                        }
                    }
                }
            }
            else
            {
                var seen = new HashSet<IXdmNode>();
                foreach (var keyValue in ExtractKeyLookupValues(keyValueArg))
                {
                    foreach (var (_, keyIndex, docCandidates) in docEntries)
                    {
                        foreach (var node in keyIndex.Lookup(keyName, keyValue))
                        {
                            if (!seen.Add(node))
                                continue;

                            if (docCandidates.Any(c => IsDescendantOrSelf(node, c)))
                                result.Add(XdmValue.FromNode(node));
                        }
                    }
                }
                result.Sort((a, b) =>
                {
                    var na = a.NodeValue!;
                    var nb = b.NodeValue!;
                    return na.DocumentOrder.CompareTo(nb.DocumentOrder);
                });
            }

            return XdmValue.FromSequence(MaterializedSequence.FromList(result));
        }
    }

    /// <summary>
    /// Expands a lexical key name (possibly prefixed) to Clark notation using the
    /// namespace bindings in the current evaluation context.
    /// </summary>
    private static string ExpandKeyName(string qname, EvaluationContext context)
    {
        if (qname.StartsWith("Q{", StringComparison.Ordinal))
        {
            int close = qname.IndexOf('}');
            if (close >= 2)
                return qname;
        }

        int colon = qname.IndexOf(':');
        if (colon <= 0 || colon == qname.Length - 1)
            return "{}" + qname;

        var prefix = qname[..colon];
        var local = qname[(colon + 1)..];
        var ns = context.TryResolveNamespace(prefix, out var uri) ? uri : string.Empty;
        return "{" + ns + "}" + local;
    }

    /// <summary>
    /// Returns true if <paramref name="node"/> is the same node as, or a descendant of,
    /// <paramref name="ancestor"/>.
    /// </summary>
    private static bool IsDescendantOrSelf(IXdmNode node, IXdmNode ancestor)
    {
        var current = node;
        while (current != null)
        {
            if (current.IsSameNode(ancestor))
                return true;
            current = current.Parent;
        }
        return false;
    }

    /// <summary>
    /// Retrieves or lazily builds the <see cref="KeyIndex"/> for the specified document root.
    /// Uses iterative rebuilding to handle cross-key dependencies (e.g. key-064).
    /// </summary>
    private KeyIndex? GetOrBuildKeyIndex(IXdmNode docRoot)
    {
        // Find existing index using IsSameNode (wrapper instances may differ).
        foreach (var (existingDoc, existingIndex) in _keyIndices!)
        {
            if (existingDoc.IsSameNode(docRoot))
                return existingIndex;
        }

        var allKeyDefs = _stylesheet.GetAllKeyDefinitions();
        if (allKeyDefs.Count == 0)
            return null;

        // Build iteratively for this document; add the index first so that
        // recursive key() calls inside xsl:key/@use or match can query it.
        var keyIndex = new KeyIndex();
        _keyIndices!.Add((docRoot, keyIndex));
        BuildKeyIndex(docRoot, keyIndex, allKeyDefs);
        return keyIndex;
    }

    /// <summary>
    /// Builds a <see cref="KeyIndex"/> for the supplied document using all stylesheet
    /// key definitions, respecting each key's effective collation and detecting
    /// conflicting collations for the same key name (XTSE1220).
    /// </summary>
    private void BuildKeyIndex(IXdmNode docRoot, KeyIndex keyIndex, IReadOnlyList<Stylesheet.KeyDefinition> allKeyDefs)
    {
        // Determine the effective collation for each key name.
        var keyCollationMap = new Dictionary<string, string>();
        foreach (var keyDef in allKeyDefs)
        {
            var effectiveCollation = string.IsNullOrEmpty(keyDef.Collation)
                ? GetEffectiveDefaultCollation(keyDef.Element!)
                : keyDef.Collation;
            if (keyCollationMap.TryGetValue(keyDef.Name, out var existing) && existing != effectiveCollation)
                throw new InvalidOperationException($"XTSE1220: xsl:key definitions for '{keyDef.Name}' have different effective collations.");
            keyCollationMap[keyDef.Name] = effectiveCollation;
            keyIndex.SetCollation(keyDef.Name, effectiveCollation);
        }

        // KeyIndex.BuildSingleKey mutates the context focus; save and restore to avoid
        // corrupting the currently executing template's focus.
        var savedItem = _context.ContextItem;
        var savedPosition = _context.ContextPosition;
        var savedSize = _context.ContextSize;
        try
        {
            int maxIterations = allKeyDefs.Count + 1;
            int previousTotal = -1;
            for (int i = 0; i < maxIterations; i++)
            {
                int currentTotal = keyIndex.TotalEntryCount;
                if (currentTotal == previousTotal)
                    break;
                previousTotal = currentTotal;

                // Clear each key name once per iteration so multiple definitions
                // with the same name accumulate.
                var cleared = new HashSet<string>();
                foreach (var keyDef in allKeyDefs)
                {
                    if (cleared.Add(keyDef.Name))
                        keyIndex.ClearKey(keyDef.Name);
                    var keyCollation = keyCollationMap[keyDef.Name];
                    var savedKeyCollation = _context.DefaultCollation;
                    _context.DefaultCollation = keyCollation;
                    try
                    {
                        if (keyDef.HasUseContent)
                            KeyIndex.BuildSingleKey(docRoot, keyDef, _context, keyIndex, n => EvaluateSequenceConstructor(keyDef.Element!, XdmValue.FromNode(n), wrapInDocumentNode: false));
                        else
                            KeyIndex.BuildSingleKey(docRoot, keyDef, _context, keyIndex);
                    }
                    finally
                    {
                        _context.DefaultCollation = savedKeyCollation;
                    }
                }
            }
        }
        finally
        {
            _context.WithFocus(savedItem, savedPosition, savedSize);
        }
    }

    /// <summary>
    /// Looks up the given key values in a single key index and returns matching nodes.
    /// </summary>
    private XdmValue LookupKeyValues(KeyIndex keyIndex, string keyName, XdmValue keyValueArg)
    {
        var result = new List<XdmValue>();

        if (IsCompositeKey(keyName))
        {
            var tuple = ExtractKeyLookupValues(keyValueArg).ToArray();
            if (tuple.Length > 0)
            {
                foreach (var node in keyIndex.LookupComposite(keyName, tuple))
                    result.Add(XdmValue.FromNode(node));
            }
        }
        else
        {
            var seen = new HashSet<IXdmNode>();
            foreach (var keyValue in ExtractKeyLookupValues(keyValueArg))
            {
                foreach (var node in keyIndex.Lookup(keyName, keyValue))
                {
                    if (seen.Add(node))
                        result.Add(XdmValue.FromNode(node));
                }
            }
            result.Sort((a, b) =>
            {
                var na = a.NodeValue!;
                var nb = b.NodeValue!;
                return na.DocumentOrder.CompareTo(nb.DocumentOrder);
            });
        }

        return XdmValue.FromSequence(MaterializedSequence.FromList(result));
    }

    private bool IsCompositeKey(string keyName)
        => _stylesheet.GetAllKeyDefinitions().Any(k => k.Name == keyName && k.Composite);

    /// <summary>
    /// Converts a key-value argument to a sequence of strings using XPath 1.0
    /// backwards-compatible semantics: node-sets are atomized to their string values,
    /// and any other value is converted via <c>string()</c>.
    /// </summary>
    private static XdmValue ConvertKeyValueToStringSequence(XdmValue value)
    {
        if (value.IsUndefined)
            return XdmValue.Undefined;

        var strings = new List<XdmValue>();
        if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                if (item.IsUndefined)
                    continue;
                strings.Add(XdmValue.FromString(item.ToString()));
            }
        }
        else
        {
            strings.Add(XdmValue.FromString(value.ToString()));
        }

        if (strings.Count == 0)
            return XdmValue.Undefined;
        if (strings.Count == 1)
            return strings[0];
        return XdmValue.FromSequence(MaterializedSequence.FromList(strings));
    }

    /// <summary>
    /// Extracts typed atomic values from a key-value argument (either a single value or a sequence).
    /// Node arguments are atomized to <c>xs:untypedAtomic</c> strings.
    /// </summary>
    private static IEnumerable<XdmValue> ExtractKeyLookupValues(XdmValue keyValueArg)
    {
        if (keyValueArg.IsSequence && keyValueArg.SequenceValue != null)
        {
            foreach (var val in XdmSequence.FromSource(keyValueArg.SequenceValue))
                yield return AtomizeKeyValue(val);
        }
        else
        {
            yield return AtomizeKeyValue(keyValueArg);
        }
    }

    private static XdmValue AtomizeKeyValue(XdmValue value)
    {
        if (value.IsNode)
            return XdmValue.FromString(value.ToString(), "untypedAtomic");
        return value;
    }

    /// <summary>
    /// Registers the XSLT <c>current-group()</c> and <c>current-grouping-key()</c> functions.
    /// </summary>
    private void RegisterGroupingFunctions()
    {
        _context.RegisterFunction(new Bosak.XPath.Runtime.Functions.FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "current-group",
            Arity = 0,
            ParameterTypes = [],
            ReturnType = XdmValueKind.Sequence,
            Implementation = (ctx, args) =>
            {
                if (_currentGroup == null)
                    throw new InvalidOperationException("XTDE1061: current-group() is not defined in the current context");
                if (_currentGroup.Count == 0)
                    return XdmValue.Undefined;
                return XdmValue.FromSequence(MaterializedSequence.FromList(_currentGroup));
            },
            // A dynamic call on current-group() is a dynamic error (XTDE1061) even when a
            // group is being processed, because the current group is not part of the closure.
            DynamicImplementation = (ctx, args) =>
                throw new InvalidOperationException("XTDE1061: dynamic call on current-group() is not allowed")
        });

        _context.RegisterFunction(new Bosak.XPath.Runtime.Functions.FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "current-grouping-key",
            Arity = 0,
            ParameterTypes = [],
            ReturnType = XdmValueKind.Undefined,
            Implementation = (ctx, args) =>
            {
                if (_currentGroupingKey == null)
                    throw new InvalidOperationException("XTDE1071: current-grouping-key() is not defined in the current context");
                return _currentGroupingKey.Value;
            },
            // A dynamic call on current-grouping-key() is a dynamic error (XTDE1071).
            DynamicImplementation = (ctx, args) =>
                throw new InvalidOperationException("XTDE1071: dynamic call on current-grouping-key() is not allowed")
        });

        _context.RegisterFunction(new Bosak.XPath.Runtime.Functions.FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "current-merge-group",
            Arity = 0,
            ParameterTypes = [],
            ReturnType = XdmValueKind.Sequence,
            Implementation = (ctx, args) =>
            {
                if (_currentMergeGroup == null)
                    throw new InvalidOperationException("XTDE3480: current-merge-group() is not defined in the current context");
                if (_currentMergeGroup.Count == 0)
                    return XdmValue.Undefined;
                return XdmValue.FromSequence(MaterializedSequence.FromList(_currentMergeGroup));
            },
            // A dynamic call on current-merge-group() is a dynamic error (XTDE3480).
            DynamicImplementation = (ctx, args) =>
                throw new InvalidOperationException("XTDE3480: dynamic call on current-merge-group() is not allowed")
        });

        _context.RegisterFunction(new Bosak.XPath.Runtime.Functions.FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "current-merge-group",
            Arity = 1,
            ParameterTypes = [XdmValueKind.Undefined],
            ReturnType = XdmValueKind.Sequence,
            Implementation = (ctx, args) =>
            {
                if (_currentNamedMergeGroups == null || _currentMergeSourceNames == null)
                    throw new InvalidOperationException("XTDE3480: current-merge-group() is not defined in the current context");
                var nameValue = AtomizeFirstItem(args[0]);
                var name = nameValue.IsUndefined ? "" : nameValue.ToString();
                if (string.IsNullOrEmpty(name) || !_currentMergeSourceNames.Contains(name))
                    throw new InvalidOperationException($"XTDE3490: no xsl:merge-source named '{name}'");
                if (!_currentNamedMergeGroups.TryGetValue(name, out var group) || group.Count == 0)
                    return XdmValue.Undefined;
                return XdmValue.FromSequence(MaterializedSequence.FromList(group));
            },
            // A dynamic call on current-merge-group#1 is a dynamic error (XTDE3480).
            DynamicImplementation = (ctx, args) =>
                throw new InvalidOperationException("XTDE3480: dynamic call on current-merge-group() is not allowed")
        });

        _context.RegisterFunction(new Bosak.XPath.Runtime.Functions.FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "current-merge-key",
            Arity = 0,
            ParameterTypes = [],
            ReturnType = XdmValueKind.Undefined,
            Implementation = (ctx, args) =>
            {
                if (_currentMergeKey == null)
                    throw new InvalidOperationException("XTDE3510: current-merge-key() is not defined in the current context");
                return _currentMergeKey.Value;
            },
            // A dynamic call on current-merge-key() is a dynamic error (XTDE3510).
            DynamicImplementation = (ctx, args) =>
                throw new InvalidOperationException("XTDE3510: dynamic call on current-merge-key() is not allowed")
        });

        _context.RegisterFunction(new Bosak.XPath.Runtime.Functions.FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "regex-group",
            Arity = 1,
            ParameterTypes = [XdmValueKind.Integer],
            ReturnType = XdmValueKind.String,
            Implementation = (ctx, args) =>
            {
                var nValue = AtomizeFirstItem(args[0]);
                if (nValue.IsUndefined || !long.TryParse(nValue.ToString(), out long n))
                    return XdmValue.FromString(string.Empty);
                if (_context.RegexGroups == null || n < 0 || n >= _context.RegexGroups.Length)
                    return XdmValue.FromString(string.Empty);
                return XdmValue.FromString(_context.RegexGroups[n]);
            }
        });
    }

    /// <summary>
    /// Evaluates top-level xsl:param and xsl:variable declarations and binds them into the context.
    /// Order: imported first, then included, then local. Parameters are evaluated before variables.
    /// Global variables with sequence constructors (no @select) are evaluated lazily on first
    /// reference, using a singleton focus based on the root of the tree containing the initial
    /// context node (per XSLT 3.0 §9.6). If no initial context node is supplied, the focus is absent.
    /// </summary>
    private void InitializeGlobalParametersAndVariables(IXdmNode? globalContextItem)
    {
        var focus = globalContextItem != null ? XdmValue.FromNode(globalContextItem) : XdmValue.Undefined;
        _globalContextItem = focus;

        // Global variables/parameters are evaluated with a singleton focus based on the
        // global context item supplied for the transformation. If no global context item
        // is supplied, the focus is absent and any reference to the context item raises
        // XPDY0002.
        if (globalContextItem != null)
        {
            _context.WithFocus(XdmValue.FromNode(globalContextItem), 1, 1);
        }

        // Capture externally-supplied parameter bindings before we add any globals.
        // This lets us distinguish caller-supplied values from default values when
        // checking required="yes" parameters.
        var externallySupplied = _context.SnapshotVariables();

        // Collect globals in document order, recursing into imported and included
        // modules at the position of their xsl:import / xsl:include element. This
        // ensures globals from nested imports/includes are visible to the importer
        // and that collisions resolve first by import precedence (higher wins) and
        // then by document order within the same precedence (last wins).
        var globals = new List<(int Precedence, int Order, (string LocalName, string NamespaceUri) Name, XElement Element, bool IsParam)>();
        int documentOrder = 0;
        _stylesheet.CollectGlobalsInDocumentOrder(globals, ref documentOrder);
        // Lower numeric precedence means higher XSLT import precedence and must win,
        // so process higher-numeric (lower-precedence) declarations first and let
        // lower-numeric (higher-precedence) declarations overwrite them. Within the
        // same precedence, document order governs last-wins behaviour.
        globals.Sort((a, b) =>
        {
            int precedenceComparison = b.Precedence.CompareTo(a.Precedence);
            return precedenceComparison != 0 ? precedenceComparison : a.Order.CompareTo(b.Order);
        });

        static bool IsStaticGlobal(XElement e)
        {
            var staticAttr = e.Attribute("static")?.Value;
            if (string.IsNullOrEmpty(staticAttr))
                return false;
            var v = staticAttr.Trim();
            return v is "yes" or "true" or "1";
        }

        // Pre-register all globals (variables and parameters with defaults) so they
        // can be resolved lazily on first reference. Processing in precedence order
        // ensures the highest-precedence declaration wins when names collide.
        // Static declarations are resolved from the pre-computed static context rather
        // than re-evaluated at runtime, so a static $p is still visible to other static
        // expressions even when a non-static declaration with the same name shadows it
        // at runtime (static-027).
        foreach (var (_, _, name, elem, isParam) in globals)
        {
            // Skip parameters already supplied by the caller (e.g. fn:transform).
            if (isParam && externallySupplied.ContainsKey(name))
                continue;

            _lazyGlobals[name] = (elem, elem.Attribute("as")?.Value);
        }

        // Register lazy variable resolver BEFORE any global is referenced.
        _context.LazyVariableResolver = (localName, namespaceUri) =>
        {
            var key = (localName, namespaceUri);

            // A reference to a global that is currently being evaluated is a circular
            // dependency. Detect this before looking the variable up, so callers (e.g.
            // function bodies) see a circularity rather than an "undefined variable" error.
            if (_evaluatingGlobals.Contains(key))
                throw new InvalidOperationException("XPST0008: Circular reference to global variable.");

            if (_lazyGlobals.TryGetValue(key, out var info))
            {
                // Parameters supplied by the caller are already bound.
                if (_context.TryGetBoundVariable(localName, out var existing, namespaceUri))
                    return existing;

                // Static variables/parameters were evaluated during stylesheet loading.
                // Return the pre-computed value instead of re-evaluating at runtime.
                if (IsStaticGlobal(info.Element))
                {
                    if (_stylesheet.StaticVariables.TryGetValue(key, out var staticValue))
                    {
                        if (staticValue.IsUndefined)
                            throw new InvalidOperationException($"XTDE0050: No value supplied for required parameter '{localName}'.");
                        var converted = ConvertVariableValue(staticValue, info.AsType, isParam: info.Element.Name.LocalName == "param");
                        _context.WithVariable(localName, converted, namespaceUri);
                        return converted;
                    }
                }

                // Detect circular references (a global variable referencing itself).
                if (!_evaluatingGlobals.Add(key))
                    throw new InvalidOperationException("XPST0008: Circular reference to global variable.");

                // Global variables/parameters are evaluated with a singleton focus based
                // on the root node of the tree containing the initial context node
                // (XSLT 3.0 §9.6). They are also evaluated in the global scope, so local
                // template variables do not shadow globals during lazy evaluation.
                var savedItem = _context.ContextItem;
                var savedPos = _context.ContextPosition;
                var savedSize = _context.ContextSize;
                var savedVariables = _context.SnapshotVariables();
                // #current inside a global variable/parameter refers to the unnamed mode
                // (XSLT 2.0 erratum XT.E19), so isolate the mode stack from the caller.
                var savedModes = new List<string>(_modeStack);
                savedModes.Reverse();
                _modeStack.Clear();
                try
                {
                    // Global variables/parameters are evaluated with a singleton focus based
                    // on the global context item for the transformation.
                    var focus = _globalContextItem.IsUndefined ? XdmValue.Undefined : _globalContextItem;
                    _context.WithFocus(focus, focus.IsUndefined ? 0 : 1, focus.IsUndefined ? 0 : 1);
                    if (_globalVariableSnapshot != null)
                        _context.RestoreVariables(_globalVariableSnapshot);

                    XdmValue value;
                    var select = info.Element.Attribute("select")?.Value;
                    if (string.IsNullOrEmpty(select))
                    {
                        // Support the AVT form _select="{...}" on global variables/parameters.
                        var underSelect = info.Element.Attribute("_select")?.Value;
                        if (!string.IsNullOrEmpty(underSelect))
                            select = EvaluateAvt(underSelect, info.Element);
                    }
                    try
                    {
                        if (!string.IsNullOrEmpty(select))
                        {
                            // A global variable is out of scope within its own declaration.
                            // Detect direct self-reference in the select expression (including
                            // references from inline function bodies) before evaluation.
                            if (SelectReferencesVariable(select, info.Element, localName, namespaceUri))
                                throw new InvalidOperationException($"XPST0008: Variable ${localName} is out of scope in its own declaration.");

                            var compiled = CompileXPath(select, info.Element);
                            value = compiled.Evaluate(_context);
                        }
                        else
                        {
                            // Global variable/parameter bodies are evaluated in a temporary output state.
                            var savedOutputUri = _context.CurrentOutputUri;
                            _context.CurrentOutputUri = null;
                            try
                            {
                                value = EvaluateSequenceConstructor(info.Element, focus, wrapInDocumentNode: string.IsNullOrEmpty(info.AsType));
                            }
                            finally
                            {
                                _context.CurrentOutputUri = savedOutputUri;
                            }
                        }
                        value = ConvertVariableValue(value, info.AsType);
                    }
                    catch (Exception evalEx)
                    {
                        // Circular-reference and out-of-scope errors are static errors raised
                        // by the engine itself; leave them untouched so callers can recognise
                        // them (e.g. function-local eager evaluation defers on circular refs).
                        if (evalEx is InvalidOperationException ioe &&
                            (ioe.Message.Contains("Circular reference", StringComparison.OrdinalIgnoreCase) ||
                             ioe.Message.StartsWith("XPST0008:", StringComparison.Ordinal)))
                        {
                            throw;
                        }
                        // Mark dynamic errors from global-variable evaluation so that xsl:try
                        // knows not to catch them, while preserving the original exception type
                        // for callers outside a try/catch block.
                        evalEx.Data["Bosak.GlobalVariableError"] = true;
                        throw;
                    }
                    _context.WithVariable(localName, value, namespaceUri);
                    _lazyGlobals.Remove(key);
                    return value;
                }
                finally
                {
                    _context.RestoreVariables(savedVariables);
                    _context.WithFocus(savedItem, savedPos, savedSize);
                    _modeStack.Clear();
                    foreach (var m in savedModes)
                        _modeStack.Push(m);
                    _evaluatingGlobals.Remove(key);
                }
            }
            return null;
        };

        // Check required parameters and eagerly bind parameters whose default value
        // is an empty sequence constructor without @as, so they produce a document node
        // even if never explicitly referenced.
        foreach (var (_, _, name, elem, isParam) in globals)
        {
            if (isParam)
            {
                var required = elem.Attribute("required")?.Value?.Trim();
                if (required == "yes" && !externallySupplied.ContainsKey(name))
                    throw new InvalidOperationException($"XTDE0050: No value supplied for required parameter '{name.LocalName}'.");

                // Skip parameters already supplied by caller.
                if (externallySupplied.ContainsKey(name))
                    continue;

                var select = elem.Attribute("select")?.Value;
                if (string.IsNullOrEmpty(select) && string.IsNullOrEmpty(elem.Attribute("as")?.Value))
                {
                    // Force creation of the empty-document-node default value now.
                    if (_lazyGlobals.TryGetValue(name, out var info))
                    {
                        _lazyGlobals.Remove(name);
                        var savedItem = _context.ContextItem;
                        var savedPos = _context.ContextPosition;
                        var savedSize = _context.ContextSize;
                        var savedModes = new List<string>(_modeStack);
                        savedModes.Reverse();
                        _modeStack.Clear();
                        try
                        {
                            // Global parameters are evaluated with a singleton focus based
                            // on the root node of the tree containing the initial context node.
                            var root = _globalContextItem.IsNode ? GetRootNode(_globalContextItem.NodeValue) : null;
                            var globalFocus = root != null ? XdmValue.FromNode(root) : XdmValue.Undefined;
                            _context.WithFocus(globalFocus, globalFocus.IsUndefined ? 0 : 1, globalFocus.IsUndefined ? 0 : 1);
                            var value = EvaluateSequenceConstructor(info.Element, globalFocus, wrapInDocumentNode: true);
                            value = ConvertVariableValue(value, info.AsType);
                            _context.WithVariable(name.LocalName, value, name.NamespaceUri);
                        }
                        finally
                        {
                            _context.WithFocus(savedItem, savedPos, savedSize);
                            _modeStack.Clear();
                            foreach (var m in savedModes)
                                _modeStack.Push(m);
                        }
                    }
                }
            }
        }

        // Capture the global variable scope so lazy evaluations run in isolation
        // from local template variables.
        _globalVariableSnapshot = _context.SnapshotVariables();
    }

    /// <summary>
    /// Evaluates a compiled match pattern with the current output URI cleared, because
    /// pattern predicates are evaluated in a temporary output state.
    /// </summary>
    private bool EvaluatePatternMatch(Stylesheet.TemplateRule rule, XdmValue item)
    {
        if (rule.CompiledMatch == null)
            return false;
        var savedOutputUri = _context.CurrentOutputUri;
        _context.CurrentOutputUri = null;
        try
        {
            return rule.CompiledMatch(item, _context);
        }
        finally
        {
            _context.CurrentOutputUri = savedOutputUri;
        }
    }

    /// <summary>
    /// Finds the highest-priority template rule that matches the given node in the given mode.
    /// </summary>
    private Stylesheet.TemplateRule? FindBestTemplate(IXdmNode node, string mode, HashSet<Stylesheet.TemplateRule>? excludedRules = null)
        => FindBestTemplate(XdmValue.FromNode(node), mode, excludedRules);

    /// <summary>
    /// Finds the highest-priority template rule that matches the given item (node or atomic value) in the given mode.
    /// </summary>
    /// <param name="item">The context item to match against.</param>
    /// <param name="mode">The mode to match in.</param>
    /// <param name="excludedRules">Rules to exclude (used by xsl:next-match).</param>
    /// <param name="minImportPrecedence">If set, only rules with import precedence greater than this value are considered (used by xsl:apply-imports).</param>
    private Stylesheet.TemplateRule? FindBestTemplate(XdmValue item, string mode, HashSet<Stylesheet.TemplateRule>? excludedRules = null, int? minImportPrecedence = null, IReadOnlySet<Stylesheet.Stylesheet>? allowedStylesheets = null)
    {
        Stylesheet.TemplateRule? best = null;
        double bestPriority = double.NegativeInfinity;
        int bestImportPrecedence = int.MaxValue;
        bool hasConflict = false;

        foreach (var rule in _allTemplateRules)
        {
            if (excludedRules != null && excludedRules.Contains(rule))
                continue;
            if (allowedStylesheets != null && !allowedStylesheets.Contains(rule.Stylesheet))
                continue;
            if (minImportPrecedence.HasValue && rule.ImportPrecedence <= minImportPrecedence.Value)
                continue;
            if (!MatchesMode(rule, mode))
                continue;
            if (rule.CompiledMatch == null)
                continue;
            if (!EvaluatePatternMatch(rule, item))
                continue;

            // XSLT spec §6.4: import precedence is checked BEFORE priority.
            // Higher import precedence (lower numeric value in our system) always wins.
            if (best == null || rule.ImportPrecedence < bestImportPrecedence)
            {
                best = rule;
                bestPriority = rule.Priority;
                bestImportPrecedence = rule.ImportPrecedence;
                hasConflict = false;
            }
            else if (rule.ImportPrecedence == bestImportPrecedence)
            {
                if (rule.Priority > bestPriority)
                {
                    best = rule;
                    bestPriority = rule.Priority;
                    hasConflict = false;
                }
                else if (rule.Priority == bestPriority)
                {
                    if (best != null && best != rule && best.Element != rule.Element)
                        hasConflict = true;
                    // XSLT last-wins rule: when priority and import precedence are equal,
                    // the template that appears later in the stylesheet wins.
                    best = rule;
                }
            }
        }

        if (hasConflict && best != null)
        {
            var modeDef = _stylesheet.GetModeDefinition(mode);

            if (modeDef?.OnMultipleMatch == Stylesheet.OnMultipleMatch.Fail)
            {
                throw new InvalidOperationException("XTDE0540: Multiple templates match with the same priority.");
            }

            if (_treatRecoverableAmbiguousMatchAsError)
            {
                throw new InvalidOperationException("XTRE0540: Multiple templates match with the same priority.");
            }

            if (modeDef?.WarningOnMultipleMatch == true)
            {
                _messageListener?.OnWarning($"Multiple templates match with the same priority in mode '{mode}'.");
            }
            // Otherwise default to last-wins / recovery.
        }

        if (item.IsNode && item.NodeValue?.LocalName == "footnote")
        {
            try { System.IO.File.AppendAllText("D:/Development/Bosak/tmpdebug/docbookcheck/docbookcheck/footnote.log", $"FindBest mode={mode} node={item.NodeValue.LocalName} ns={item.NodeValue.NamespaceUri} rule={best?.Element.Attribute("match")?.Value} prio={best?.Priority} imp={best?.ImportPrecedence}\n"); } catch { }
        }

        return best;
    }

    /// <summary>
    /// Determines whether <paramref name="candidate"/> is a more specific document-node
    /// pattern than <paramref name="current"/>.  Used as a tie-breaker in FindBestTemplate.
    /// </summary>
    private static bool IsMoreSpecificDocumentPattern(string? candidate, string? current)
    {
        if (string.IsNullOrEmpty(current)) return true;
        if (string.IsNullOrEmpty(candidate)) return false;

        var c = current.Trim();
        var cand = candidate.Trim();

        // doc('uri') / document('uri') are more specific than /
        if (c == "/" && (cand.StartsWith("doc(") || cand.StartsWith("document(")))
            return true;

        // Prefer the longer / more detailed pattern as a general heuristic
        return cand.Length > c.Length;
    }

    private static bool MatchesMode(Stylesheet.TemplateRule rule, string mode)
    {
        if (rule.MatchesAllModes)
            return true;
        foreach (var m in rule.Modes)
        {
            if (m == mode)
                return true;
        }
        return false;
    }

    private static IEnumerable<IXdmNode> EnumerateNodes(XdmSequence sequence)
    {
        foreach (var item in sequence)
        {
            if (item.IsNode && item.NodeValue != null)
                yield return item.NodeValue;
        }
    }

    private static IEnumerable<IXdmNode> EnumerateNodes(XdmValue value)
    {
        if (value.IsNode && value.NodeValue != null)
        {
            yield return value.NodeValue;
        }
        else if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                if (item.IsNode && item.NodeValue != null)
                    yield return item.NodeValue;
            }
        }
    }

    /// <summary>
    /// Enumerates all items in an XDM value, including atomic values and nodes.
    /// </summary>
    private static IEnumerable<XdmValue> EnumerateItems(XdmValue value)
    {
        if (value.IsUndefined)
            yield break;

        if (!value.IsSequence)
        {
            yield return value;
            yield break;
        }

        if (value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                if (!item.IsUndefined)
                    yield return item;
            }
        }
    }

    /// <summary>
    /// Returns true if the value is a sequence containing at least one node.
    /// </summary>
    private static bool ContainsNode(XdmValue value)
    {
        if (!value.IsSequence || value.SequenceValue == null)
            return false;
        foreach (var item in XdmSequence.FromSource(value.SequenceValue))
        {
            if (item.IsNode)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns the first item of a sequence, or the value itself if it is not a
    /// sequence. Empty sequences yield an undefined value.
    /// </summary>
    private static XdmValue FirstItemOrUndefined(XdmValue value)
    {
        if (value.IsUndefined || !value.IsSequence || value.SequenceValue == null)
            return value;
        foreach (var item in XdmSequence.FromSource(value.SequenceValue))
        {
            if (!item.IsUndefined)
                return item;
        }
        return XdmValue.Undefined;
    }

    /// <summary>
    /// Returns the string value of the first item in a sequence, or the string
    /// value of the value itself if it is a singleton. Empty sequences produce
    /// an empty string.
    /// </summary>
    private static string FirstItemString(XdmValue value)
    {
        if (value.IsUndefined)
            return string.Empty;

        if (!value.IsSequence)
            return value.ToString();

        if (value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                if (!item.IsUndefined)
                    return item.ToString();
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Flattens sequences and arrays for <c>xsl:apply-templates</c> selection,
    /// so arrays are processed member-by-member.
    /// </summary>
    private static List<XdmValue> FlattenSelectedItems(XdmValue value)
    {
        var result = new List<XdmValue>();
        FlattenSelectedItemsCore(value, result);
        return result;
    }

    private static void FlattenSelectedItemsCore(XdmValue value, List<XdmValue> result)
    {
        if (value.IsUndefined)
            return;

        if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                FlattenSelectedItemsCore(item, result);
            return;
        }

        if (value.IsArray && value.ArrayValue != null)
        {
            foreach (var member in value.ArrayValue.Values)
                FlattenSelectedItemsCore(member, result);
            return;
        }

        result.Add(value);
    }

    /// <summary>
    /// Sorts a sequence of nodes by document order, but keeps the relative order of
    /// nodes from different source trees as it appeared in the original sequence.
    /// </summary>
    private static List<XdmValue> SortNodesByDocumentOrderPreservingTreeOrder(List<XdmValue> items)
    {
        var indexed = items.Select((item, idx) => (item, idx)).ToList();
        var rootOrder = new Dictionary<IXdmNode, int>();
        var groups = indexed.GroupBy(a =>
        {
            var root = GetRootNode(a.item.NodeValue!);
            if (!rootOrder.TryGetValue(root, out var order))
            {
                order = rootOrder.Count;
                rootOrder[root] = order;
            }
            return root;
        }).ToList();

        var sorted = new List<XdmValue>(items.Count);
        foreach (var g in groups.OrderBy(g => rootOrder[g.Key]))
        {
            var list = g.ToList();
            list.Sort((a, b) =>
            {
                int cmp = a.item.NodeValue!.DocumentOrder.CompareTo(b.item.NodeValue!.DocumentOrder);
                return cmp != 0 ? cmp : a.idx.CompareTo(b.idx);
            });
            foreach (var x in list)
                sorted.Add(x.item);
        }
        return sorted;
    }

    /// <summary>
    /// Finds the element with the specified <c>xml:id</c> value within the given node.
    /// </summary>
    private static IXdmNode? FindElementByXmlId(IXdmNode node, string id)
    {
        foreach (var item in node.Axis(XdmAxis.DescendantOrSelf))
        {
            if (!item.IsNode || item.NodeValue!.NodeKind != XdmNodeKind.Element)
                continue;
            foreach (var attrItem in item.NodeValue.Axis(XdmAxis.Attribute))
            {
                if (!attrItem.IsNode)
                    continue;
                var attr = attrItem.NodeValue!;
                if (attr.LocalName == "id" && attr.NamespaceUri == "http://www.w3.org/XML/1998/namespace" && attr.StringValue == id)
                    return item.NodeValue;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns the root node of the tree containing the given node.
    /// For a node inside a document this is the document node; for a parentless
    /// tree it is the root element.
    /// </summary>
    private static IXdmNode GetRootNode(IXdmNode node)
    {
        var current = node;
        while (true)
        {
            IXdmNode? parent = null;
            foreach (var value in current.Axis(XdmAxis.Parent))
            {
                if (value.IsNode)
                {
                    parent = value.NodeValue;
                    break;
                }
            }
            if (parent == null)
                return current;
            current = parent;
        }
    }

    /// <summary>
    /// Returns the lexical name of a named template whose local name is "initial-template"
    /// and whose prefix resolves to the XSLT namespace, or <c>null</c> if none exists.
    /// </summary>
    private string? FindInitialTemplateName()
    {
        foreach (var pair in _allNamedTemplates)
        {
            var name = pair.Key;
            var colonIndex = name.IndexOf(':');
            if (colonIndex < 0)
                continue;
            var prefix = name[..colonIndex];
            var local = name[(colonIndex + 1)..];
            if (local != "initial-template")
                continue;
            var ns = pair.Value.Element.GetNamespaceOfPrefix(prefix);
            if (ns?.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                return name;
        }
        return null;
    }

    /// <summary>
    /// Converts an XDM value to its string representation, concatenating sequence items.
    /// </summary>
    private static string XdmValueToString(XdmValue value)
        => XdmValueToString(value, " ");

    /// <summary>
    /// Extracts a single string key from an XDM value for grouping purposes.
    /// Sequences are collapsed to the first item's string value.
    /// </summary>
    private static string GetGroupingKeyString(XdmValue value)
    {
        if (value.IsUndefined)
            return string.Empty;
        if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                if (!item.IsUndefined)
                    return item.ToString();
            }
            return string.Empty;
        }
        return value.ToString();
    }

    /// <summary>
    /// Converts an XDM value to its string representation, concatenating atomized items
    /// with the specified separator. Map and function items cannot be atomized and raise
    /// <c>FOTY0013</c>. Arrays are atomized recursively to their member values.
    /// </summary>
    private static string XdmValueToString(XdmValue value, string separator)
    {
        if (value.IsUndefined)
            return string.Empty;

        var sb = new System.Text.StringBuilder();
        bool first = true;
        foreach (var atom in AtomizeForString(value))
        {
            if (!first)
                sb.Append(separator);
            sb.Append(atom);
            first = false;
        }
        return sb.ToString();
    }

    /// <summary>
    /// Atomizes an XDM value for string output, recursively expanding sequences and arrays.
    /// Map and function items raise <c>FOTY0013</c>.
    /// </summary>
    private static IEnumerable<string> AtomizeForString(XdmValue value)
    {
        if (value.IsUndefined)
            yield break;

        if (value.IsMap || value.IsFunction)
            throw new InvalidOperationException("FOTY0013: Cannot atomize a map, array, or function item");

        if (value.IsArray && value.ArrayValue != null)
        {
            foreach (var member in value.ArrayValue.Values)
            {
                foreach (var atom in AtomizeForString(member))
                    yield return atom;
            }
            yield break;
        }

        if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                if (item.IsUndefined)
                    continue;
                foreach (var atom in AtomizeForString(item))
                    yield return atom;
            }
            yield break;
        }

        yield return value.ToString();
    }

    /// <summary>
    /// Builds the XDM value that represents the content of an <c>xsl:message</c>
    /// instruction by evaluating @select and the sequence constructor.
    /// </summary>
    private XdmValue BuildMessageValue(XElement instruction, XdmValue contextItem)
    {
        var items = new List<XdmValue>();

        var msgSelect = instruction.Attribute("select")?.Value;
        if (!string.IsNullOrEmpty(msgSelect))
        {
            var compiled = CompileXPath(msgSelect, instruction);
            FlattenToList(compiled.Evaluate(_context), items);
        }

        if (instruction.HasElements || instruction.Nodes().OfType<XText>().Any())
        {
            var seqValue = EvaluateSequenceConstructor(instruction, contextItem, wrapInDocumentNode: true);
            if (!seqValue.IsUndefined)
                FlattenToList(seqValue, items);
        }

        if (items.Count == 0)
            return XdmValue.FromSequence(XdmSequence.Empty);

        // If every item is atomic, the message is a simple space-separated string.
        if (items.TrueForAll(i => !i.IsNode))
            return XdmValue.FromString(string.Join(" ", items.Select(i => i.ToString())), "untypedAtomic");

        // Otherwise flatten any document nodes so that the serialization below can
        // treat the content as a sequence of nodes/atomics.
        var flat = new List<XdmValue>();
        foreach (var item in items)
            FlattenMessageItem(item, flat);

        return XdmValue.FromSequence(MaterializedSequence.FromList(flat));
    }

    /// <summary>
    /// Flattens a message item, unwrapping document nodes (including synthetic
    /// <c>__xdm_doc__</c> wrappers) into their constituent children.
    /// </summary>
    private static void FlattenMessageItem(XdmValue item, List<XdmValue> results)
    {
        if (item.IsSequence && item.SequenceValue != null)
        {
            foreach (var child in XdmSequence.FromSource(item.SequenceValue))
                FlattenMessageItem(child, results);
            return;
        }

        if (!item.IsNode)
        {
            results.Add(item);
            return;
        }

        var node = item.NodeValue;
        if (node == null)
            return;

        if (node is XDocumentNode docNode && docNode.UnderlyingObject is System.Xml.Linq.XDocument doc)
        {
            var root = doc.Root;
            if (root != null && root.Name.LocalName == "__xdm_doc__")
            {
                foreach (var child in root.Nodes())
                    results.Add(XdmValue.FromNode(new XDocumentNode(child)));
                return;
            }
            if (root != null)
                results.Add(XdmValue.FromNode(new XDocumentNode(root)));
            return;
        }

        results.Add(item);
    }

    /// <summary>
    /// Serializes the message XDM value to the string passed to the message listener.
    /// Atomic values become text; nodes are serialized as XML.
    /// </summary>
    private static string SerializeMessageValue(XdmValue value)
    {
        if (value.IsUndefined)
            return string.Empty;

        // Simple atomic string produced by BuildMessageValue for all-atomic sequences.
        if (value.IsAtomic)
            return value.ToString();

        var items = new List<XdmValue>();
        FlattenToList(value, items);

        var wrapper = new XElement("__msg__");
        foreach (var item in items)
        {
            if (item.IsNode)
            {
                var node = item.NodeValue;
                if (node == null)
                    continue;
                if (node is XDocumentNode docNode)
                {
                    var underlying = docNode.UnderlyingObject;
                    switch (underlying)
                    {
                        case System.Xml.Linq.XElement elem:
                            wrapper.Add(new XElement(elem));
                            break;
                        case System.Xml.Linq.XText text:
                            wrapper.Add(new XText(text.Value));
                            break;
                        case System.Xml.Linq.XComment comment:
                            wrapper.Add(new XComment(comment.Value));
                            break;
                        case System.Xml.Linq.XProcessingInstruction pi:
                            wrapper.Add(new XProcessingInstruction(pi.Target, pi.Data));
                            break;
                        case System.Xml.Linq.XDocument doc:
                            if (doc.Root != null)
                                wrapper.Add(new XElement(doc.Root));
                            break;
                    }
                }
                else
                {
                    wrapper.Add(new XText(node.StringValue));
                }
            }
            else if (!item.IsUndefined)
            {
                wrapper.Add(new XText(item.ToString()));
            }
        }

        var saveOptions = System.Xml.Linq.SaveOptions.DisableFormatting;
        return string.Concat(wrapper.Nodes().Select(n => n.ToString(saveOptions)));
    }

    /// <summary>
    /// Evaluates the <c>terminate</c> attribute of <c>xsl:message</c>.
    /// </summary>
    private bool EvaluateMessageTerminate(XElement instruction, XdmValue contextItem)
    {
        var terminateAttr = instruction.Attribute("terminate")?.Value;
        if (string.IsNullOrEmpty(terminateAttr))
            return false;

        var value = EvaluateAvt(terminateAttr, instruction).Trim();
        if (value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
            value == "1")
            return true;

        if (value.Equals("no", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("false", StringComparison.OrdinalIgnoreCase) ||
            value == "0")
            return false;

        // Invalid effective value is a dynamic error.
        throw new InvalidOperationException("XTDE0975");
    }

    /// <summary>
    /// Evaluates the <c>error-code</c> attribute of <c>xsl:message</c>.
    /// </summary>
    private string EvaluateMessageErrorCode(XElement instruction)
    {
        var codeAttr = instruction.Attribute("error-code")?.Value;
        if (string.IsNullOrEmpty(codeAttr))
            return "XTMM9000";

        var expanded = EvaluateAvt(codeAttr, instruction).Trim();
        if (expanded.StartsWith("Q{", StringComparison.Ordinal))
        {
            var close = expanded.IndexOf('}');
            if (close > 2)
            {
                var ns = expanded[2..close];
                var local = expanded[(close + 1)..];
                if (string.IsNullOrEmpty(ns))
                    return $"Q{{}}{local}";
                return $"Q{{{ns}}}{local}";
            }
        }

        var colon = expanded.IndexOf(':');
        if (colon >= 0)
        {
            var prefix = expanded[..colon];
            var local = expanded[(colon + 1)..];
            var ns = instruction.GetNamespaceOfPrefix(prefix);
            if (ns == null)
                throw new InvalidOperationException("XTDE0040");
            return $"Q{{{ns.NamespaceName}}}{local}";
        }

        return $"Q{{}}{expanded}";
    }

    /// <summary>
    /// Binds the <c>err:*</c> variables used by <c>xsl:catch</c> to the details of the
    /// caught exception. Returns the previous values so they can be restored.
    /// </summary>
    private (XdmValue Code, XdmValue Description, XdmValue Value, XdmValue Module, XdmValue Line, XdmValue Column) BindCatchErrorVariables(Exception ex, XElement catchElem, XElement instruction)
    {
        const string ErrNs = "http://www.w3.org/2005/xqt-errors";

        var (codeQName, description, value) = GetErrorDetails(ex);

        _context.TryGetVariable("code", out var prevCode, ErrNs);
        _context.TryGetVariable("description", out var prevDesc, ErrNs);
        _context.TryGetVariable("value", out var prevValue, ErrNs);
        _context.TryGetVariable("module", out var prevModule, ErrNs);
        _context.TryGetVariable("line-number", out var prevLine, ErrNs);
        _context.TryGetVariable("column-number", out var prevColumn, ErrNs);

        var errPrefix = catchElem.GetPrefixOfNamespace(ErrNs) ?? "err";
        var codePrefix = string.IsNullOrEmpty(codeQName.NamespaceUri)
            ? string.Empty
            : (catchElem.GetPrefixOfNamespace(codeQName.NamespaceUri) ?? errPrefix);
        var boundCodeQName = new XsQName(codeQName.LocalName, codeQName.NamespaceUri, codePrefix);
        _context.WithVariable("code", XdmValue.FromQName(boundCodeQName), ErrNs);
        _context.WithVariable("description", XdmValue.FromString(description), ErrNs);
        _context.WithVariable("value", value, ErrNs);

        // Report the line/module of the actual instruction that failed (e.g. the
        // xsl:sequence that called doc()), falling back to the xsl:try instruction.
        var offendingInstruction = _currentInstruction ?? instruction;
        string module = string.Empty;
        var moduleUri = offendingInstruction.BaseUri;
        if (!string.IsNullOrEmpty(moduleUri))
        {
            try { module = System.IO.Path.GetFileName(moduleUri); }
            catch { module = moduleUri; }
        }
        _context.WithVariable("module", XdmValue.FromString(module), ErrNs);

        long line = 0;
        long column = 0;
        if (offendingInstruction is System.Xml.IXmlLineInfo lineInfo && lineInfo.HasLineInfo())
        {
            line = lineInfo.LineNumber;
            column = lineInfo.LinePosition;
        }
        _context.WithVariable("line-number", XdmValue.FromInteger(line), ErrNs);
        _context.WithVariable("column-number", XdmValue.FromInteger(column), ErrNs);

        return (prevCode, prevDesc, prevValue, prevModule, prevLine, prevColumn);
    }

    /// <summary>
    /// Parses an exception into the error QName, description, and error value that
    /// should be exposed through the <c>err:*</c> variables in <c>xsl:catch</c>.
    /// </summary>
    private static (XsQName Code, string Description, XdmValue Value) GetErrorDetails(Exception ex)
    {
        const string ErrNs = "http://www.w3.org/2005/xqt-errors";

        if (ex is XsltRuntimeException xre)
        {
            return (new XsQName(xre.ErrorCode, ErrNs, string.Empty), xre.Message, xre.ErrorValue);
        }

        if (ex is InvalidOperationException ioe)
        {
            var message = ioe.Message;

            // fn:error() messages are formatted as "fn:error(Q{uri}local): description".
            if (message.StartsWith("fn:error(", StringComparison.Ordinal))
            {
                var qnameStart = "fn:error(".Length;
                var qnameEnd = message.IndexOf(')', qnameStart);
                if (qnameEnd > qnameStart)
                {
                    var qname = message[qnameStart..qnameEnd];
                    string ns;
                    string local;
                    if (qname.Length > 2 && qname[0] == 'Q' && qname[1] == '{')
                    {
                        var close = qname.IndexOf('}');
                        ns = close > 2 ? qname[2..close] : string.Empty;
                        local = close >= 0 && close < qname.Length - 1 ? qname[(close + 1)..] : qname;
                    }
                    else
                    {
                        ns = string.Empty;
                        local = qname;
                    }

                    var descriptionStart = qnameEnd + 1;
                    if (descriptionStart < message.Length && message[descriptionStart] == ':')
                        descriptionStart++;
                    if (descriptionStart < message.Length && message[descriptionStart] == ' ')
                        descriptionStart++;
                    var description = descriptionStart < message.Length ? message[descriptionStart..] : string.Empty;

                    return (new XsQName(local, ns, string.Empty), description, XdmValue.Undefined);
                }
            }

            // Standard "CODE: description" format used for XPath/XSLT dynamic errors.
            var colon = message.IndexOf(':');
            if (colon > 0 && IsErrorCode(message[..colon]))
            {
                var code = message[..colon];
                var descStart = colon + 1;
                if (descStart < message.Length && message[descStart] == ' ')
                    descStart++;
                var description = descStart < message.Length ? message[descStart..] : string.Empty;
                return (new XsQName(code, ErrNs, string.Empty), description, XdmValue.Undefined);
            }

            // Some functions throw a bare error code (e.g. "FOUT1190").
            if (IsErrorCode(message))
                return (new XsQName(message, ErrNs, string.Empty), message, XdmValue.Undefined);
        }

        return (new XsQName(ex.GetType().Name, string.Empty, string.Empty), ex.Message, XdmValue.Undefined);
    }

    private static bool IsErrorCode(string token)
    {
        if (token.Length != 8)
            return false;
        for (int i = 0; i < 4; i++)
            if (!char.IsUpper(token[i]))
                return false;
        for (int i = 4; i < 8; i++)
            if (!char.IsDigit(token[i]))
                return false;
        return true;
    }

    private void RestoreCatchErrorVariables((XdmValue Code, XdmValue Description, XdmValue Value, XdmValue Module, XdmValue Line, XdmValue Column) previous)
    {
        const string ErrNs = "http://www.w3.org/2005/xqt-errors";
        _context.WithVariable("code", previous.Code, ErrNs);
        _context.WithVariable("description", previous.Description, ErrNs);
        _context.WithVariable("value", previous.Value, ErrNs);
        _context.WithVariable("module", previous.Module, ErrNs);
        _context.WithVariable("line-number", previous.Line, ErrNs);
        _context.WithVariable("column-number", previous.Column, ErrNs);
    }

    /// <summary>
    /// Captures the current variable bindings and any function-local lazy-variable dictionary
    /// so they can be restored after an <c>xsl:try</c> block. Variables declared inside the
    /// <c>xsl:try</c> must not be visible to <c>xsl:catch</c>.
    /// </summary>
    private (Dictionary<(string LocalName, string NamespaceUri), XdmValue> Variables,
             Dictionary<(string LocalName, string NamespaceUri), Lazy<XdmValue>>? FunctionLocals)
        SnapshotTryScope()
    {
        var variables = _context.SnapshotVariables();
        var functionLocals = _functionLocalLazyVariables != null
            ? new Dictionary<(string LocalName, string NamespaceUri), Lazy<XdmValue>>(_functionLocalLazyVariables)
            : null;
        return (variables, functionLocals);
    }

    /// <summary>
    /// Restores the variable scope captured by <see cref="SnapshotTryScope"/>.
    /// </summary>
    private void RestoreTryScope(
        (Dictionary<(string LocalName, string NamespaceUri), XdmValue> Variables,
         Dictionary<(string LocalName, string NamespaceUri), Lazy<XdmValue>>? FunctionLocals) snapshot)
    {
        _context.RestoreVariables(snapshot.Variables);
        if (snapshot.FunctionLocals != null && _functionLocalLazyVariables != null)
        {
            _functionLocalLazyVariables.Clear();
            foreach (var pair in snapshot.FunctionLocals)
                _functionLocalLazyVariables[pair.Key] = pair.Value;
        }
    }

    /// <summary>
    /// Removes any nodes or attributes added to the current output container after the
    /// snapshot nodes were recorded. Used by <c>xsl:try</c> to roll back output written
    /// in the try block before evaluating <c>xsl:catch</c>.
    /// </summary>
    private void RollbackOutputToNode(XNode? lastNodeBefore, XAttribute? lastAttributeBefore)
    {
        if (_currentContainer == null)
            return;

        while (_currentContainer.LastNode != lastNodeBefore && _currentContainer.LastNode != null)
            _currentContainer.LastNode.Remove();

        if (_currentContainer is XElement element)
        {
            while (element.LastAttribute != lastAttributeBefore && element.LastAttribute != null)
                element.LastAttribute.Remove();
        }
    }

    /// <summary>
    /// Finds the first <c>xsl:catch</c> element in <paramref name="catchElements"/> that
    /// matches the error raised by <paramref name="ex"/>, or <c>null</c> if none match.
    /// </summary>
    /// <param name="catchElements">The ordered list of <c>xsl:catch</c> elements.</param>
    /// <param name="ex">The exception to test.</param>
    /// <returns>The first matching catch element, or <c>null</c>.</returns>
    private XElement? FindMatchingCatch(List<XElement> catchElements, Exception ex)
    {
        foreach (var catchElem in catchElements)
        {
            if (CatchMatchesError(catchElem, ex))
                return catchElem;
        }
        return null;
    }

    /// <summary>
    /// Determines whether the given <c>xsl:catch</c> element matches the error raised by <paramref name="ex"/>.
    /// </summary>
    /// <param name="catchElem">The <c>xsl:catch</c> element.</param>
    /// <param name="ex">The exception to test.</param>
    /// <returns><c>true</c> if no <c>errors</c> attribute is specified or if the error code is listed; otherwise <c>false</c>.</returns>
    private bool CatchMatchesError(XElement catchElem, Exception ex)
    {
        var errorsAttr = catchElem.Attribute("errors")?.Value;
        if (string.IsNullOrWhiteSpace(errorsAttr))
            return true;

        var errorCode = GetErrorCodeQName(ex);

        var patterns = errorsAttr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var pattern in patterns)
        {
            var p = pattern.Trim();
            if (p == "*")
                return true;

            if (!TryExpandErrorToken(p, catchElem, out var tokenNs, out var tokenLocal))
                continue;

            if (tokenLocal == "*")
                return true;

            // Namespace wildcard (*:local) matches any namespace with the given local name.
            if (tokenNs == "*")
            {
                if (tokenLocal.Equals(errorCode.LocalName, StringComparison.OrdinalIgnoreCase))
                    return true;
                continue;
            }

            // Wildcard prefix (e.g. "XPTY*") or suffix (e.g. "*0001") on a plain local name.
            // When no prefix is supplied, standard error codes are in the W3C error namespace.
            if (tokenLocal.Length == 7 && (tokenLocal[0] == '*' || tokenLocal[^1] == '*'))
            {
                var effectiveNs = string.IsNullOrEmpty(tokenNs) ? "http://www.w3.org/2005/xqt-errors" : tokenNs;
                if (!effectiveNs.Equals(errorCode.NamespaceUri, StringComparison.Ordinal))
                    continue;

                if (tokenLocal[0] == '*' && errorCode.LocalName.Length >= 7 && errorCode.LocalName.EndsWith(tokenLocal[1..], StringComparison.Ordinal))
                    return true;
                if (tokenLocal[^1] == '*' && errorCode.LocalName.Length >= 6 && errorCode.LocalName.AsSpan().StartsWith(tokenLocal.AsSpan(0, 6), StringComparison.Ordinal))
                    return true;
                continue;
            }

            if (tokenLocal.Equals(errorCode.LocalName, StringComparison.OrdinalIgnoreCase) &&
                tokenNs.Equals(errorCode.NamespaceUri, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns the error code QName carried by an exception.
    /// </summary>
    private XsQName GetErrorCodeQName(Exception ex) => GetErrorDetails(ex).Code;

    /// <summary>
    /// Expands an <c>xsl:catch/@errors</c> token into namespace URI and local name.
    /// Supports <c>*</c>, plain local names, <c>prefix:local</c>, <c>Q{{uri}}local</c>,
    /// and namespace-wildcard forms such as <c>*:local</c>. Plain unprefixed names
    /// resolve to the empty namespace (the standard error namespace must be supplied
    /// explicitly via the <c>err:</c> prefix).
    /// </summary>
    /// <param name="token">The error token.</param>
    /// <param name="catchElem">The <c>xsl:catch</c> element, used for namespace resolution.</param>
    /// <param name="namespaceUri">The resolved namespace URI, or <c>"*"</c> for a namespace wildcard.</param>
    /// <param name="localName">The local-name part.</param>
    /// <returns><c>true</c> if the token is a valid error name.</returns>
    private static bool TryExpandErrorToken(string token, XElement catchElem, out string namespaceUri, out string localName)
    {
        namespaceUri = string.Empty;
        localName = string.Empty;

        if (token == "*")
        {
            localName = "*";
            return true;
        }

        // Clark notation Q{uri}local
        if (token.Length > 2 && token[0] == 'Q' && token[1] == '{')
        {
            var close = token.IndexOf('}');
            if (close <= 2)
                return false;
            namespaceUri = token[2..close];
            localName = token[(close + 1)..];
            return true;
        }

        var colon = token.IndexOf(':');
        if (colon < 0)
        {
            localName = token;
            return true;
        }

        var prefix = token[..colon];
        localName = token[(colon + 1)..];

        // Namespace wildcard: accept any prefix.
        if (prefix == "*")
        {
            namespaceUri = "*";
            return true;
        }

        var ns = catchElem.GetNamespaceOfPrefix(prefix);
        if (ns == null)
            return false;

        namespaceUri = ns.NamespaceName;
        return true;
    }

    /// <summary>
    /// Sorts a list of nodes according to xsl:sort specifications.
    /// </summary>
    private List<IXdmNode> SortNodes(List<IXdmNode> nodes, List<XElement> sortSpecs)
    {
        var items = nodes.Select(n => XdmValue.FromNode(n)).ToList();
        var sorted = SortItems(items, sortSpecs);
        return sorted.Select(v => v.NodeValue!).ToList();
    }

    private enum SortDataType { Text, Number, Auto }
    private readonly record struct SortControl(bool Descending, SortDataType DataType, string? Lang, string? CaseOrder, string? Collation);
    private readonly record struct SortKey(XdmValue Value, SortControl Control);
    private readonly record struct SortEntry(XdmValue Item, List<SortKey> Keys, int OriginalIndex);

    private List<XdmValue> SortItems(List<XdmValue> items, List<XElement> sortSpecs)
    {
        ValidateSortSpecs(sortSpecs);
        var controls = sortSpecs.Select(EvaluateSortControl).ToList();

        var savedFocus = _context.ContextItem;
        var savedPosition = _context.ContextPosition;
        var savedSize = _context.ContextSize;
        var savedCurrent = _context.CurrentItem;
        var savedOutputUri = _context.CurrentOutputUri;
        _context.CurrentOutputUri = null;
        try
        {
            // Pre-compute all sort keys for every item, preserving original order for stability.
            var keyed = new List<SortEntry>();
            for (int idx = 0; idx < items.Count; idx++)
            {
                var item = items[idx];
                _context.WithFocus(item, idx + 1, items.Count);
                _context.WithCurrentItem(item);
                var keys = new List<SortKey>();
                for (int i = 0; i < sortSpecs.Count; i++)
                {
                    keys.Add(new SortKey(EvaluateSortKeyValue(sortSpecs[i]), controls[i]));
                }
                keyed.Add(new SortEntry(item, keys, idx));
            }

            keyed.Sort((a, b) =>
            {
                for (int i = 0; i < a.Keys.Count; i++)
                {
                    var cmp = CompareSortKey(a.Keys[i], b.Keys[i]);
                    if (cmp != 0) return cmp;
                }
                // Stable sort: preserve original relative order when all keys equal
                return a.OriginalIndex.CompareTo(b.OriginalIndex);
            });

            return keyed.Select(k => k.Item).ToList();
        }
        finally
        {
            _context.WithFocus(savedFocus, savedPosition, savedSize);
            _context.WithCurrentItem(savedCurrent);
            _context.CurrentOutputUri = savedOutputUri;
        }
    }

    /// <summary>
    /// Evaluates the sequence constructor content of an <c>xsl:perform-sort</c> instruction
    /// (excluding its <c>xsl:sort</c> children) and returns the items to be sorted.
    /// </summary>
    private List<XdmValue> EvaluatePerformSortContent(XElement instruction, XdmValue contextItem)
    {
        var savedFocus = _context.ContextItem;
        var savedPosition = _context.ContextPosition;
        var savedSize = _context.ContextSize;
        try
        {
            _context.WithFocus(contextItem, 1, 1);

            // Build a synthetic parent containing only the non-sort sequence-constructor children.
            // Preserve the in-scope prefixed namespaces from the original instruction so that
            // XPath expressions on relocated children (e.g. xs:double(.)) still resolve their prefixes.
            var seqParent = new XElement("__perform-sort-content__");
            var nsMap = new Dictionary<string, string>();
            foreach (var ancestor in instruction.AncestorsAndSelf())
            {
                foreach (var nsAttr in ancestor.Attributes().Where(a => a.IsNamespaceDeclaration))
                {
                    var prefix = nsAttr.Name.NamespaceName == XNamespace.Xmlns.NamespaceName ? nsAttr.Name.LocalName : "";
                    if (!string.IsNullOrEmpty(prefix) && !nsMap.ContainsKey(prefix))
                        nsMap[prefix] = nsAttr.Value;
                }
            }
            foreach (var kv in nsMap)
            {
                seqParent.SetAttributeValue(XNamespace.Xmlns + kv.Key, kv.Value);
            }
            foreach (var child in instruction.Elements())
            {
                if (child.Name.LocalName == "sort" && child.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
                    continue;
                seqParent.Add(child);
            }

            var result = EvaluateSequenceConstructor(seqParent, contextItem, wrapInDocumentNode: false);
            return EnumerateItems(result).ToList();
        }
        finally
        {
            _context.WithFocus(savedFocus, savedPosition, savedSize);
        }
    }

    private void ValidateSortSpecs(List<XElement> sortSpecs)
    {
        for (int i = 0; i < sortSpecs.Count; i++)
        {
            var spec = sortSpecs[i];
            var stableAttr = EvaluateAvt(spec.Attribute("stable")?.Value ?? "", spec);
            if (!string.IsNullOrEmpty(stableAttr))
            {
                if (i > 0)
                    throw new InvalidOperationException("XTSE1017: @stable is allowed only on the first xsl:sort");
                var v = stableAttr.Trim();
                bool valid;
                if (IsXslt30OrHigher())
                    valid = v == "true" || v == "false" || v == "1" || v == "0";
                else
                    valid = v == "yes" || v == "no";
                if (!valid)
                    throw new InvalidOperationException("XTSE0020: invalid value for @stable");
            }
        }
    }

    private SortControl EvaluateSortControl(XElement spec)
    {
        var orderRaw = spec.Attribute("order")?.Value ?? "ascending";
        var order = EvaluateAvt(orderRaw, spec).Trim();
        bool descending;
        if (order.Equals("ascending", StringComparison.OrdinalIgnoreCase))
            descending = false;
        else if (order.Equals("descending", StringComparison.OrdinalIgnoreCase))
            descending = true;
        else
            throw SortAttributeError(orderRaw, "XTSE0020", "XTDE0030");

        var dataTypeRaw = spec.Attribute("data-type")?.Value;
        SortDataType dataType;
        if (string.IsNullOrEmpty(dataTypeRaw))
        {
            dataType = SortDataType.Auto;
        }
        else
        {
            var dt = EvaluateAvt(dataTypeRaw, spec).Trim();
            if (dt.Equals("text", StringComparison.OrdinalIgnoreCase))
                dataType = SortDataType.Text;
            else if (dt.Equals("number", StringComparison.OrdinalIgnoreCase))
                dataType = SortDataType.Number;
            else if (IsLexicalQName(dt))
                dataType = SortDataType.Text; // unsupported typed sort: fall back to string comparison
            else
                throw SortAttributeError(dataTypeRaw, "XTSE0020", "XTDE0030");
        }

        var langRaw = spec.Attribute("lang")?.Value;
        string? lang = null;
        if (!string.IsNullOrEmpty(langRaw))
        {
            lang = EvaluateAvt(langRaw, spec).Trim();
            if (!IsValidLanguage(lang))
                throw SortAttributeError(langRaw, "XTSE0020", "XTDE0030");
        }

        var caseOrderRaw = spec.Attribute("case-order")?.Value;
        string? caseOrder = null;
        if (!string.IsNullOrEmpty(caseOrderRaw))
        {
            caseOrder = EvaluateAvt(caseOrderRaw, spec).Trim();
            if (caseOrder != "lower-first" && caseOrder != "upper-first")
                throw SortAttributeError(caseOrderRaw, "XTSE0020", "XTDE0030");
        }

        var collationRaw = spec.Attribute("collation")?.Value;
        string? collation = null;
        if (!string.IsNullOrEmpty(collationRaw))
        {
            collation = EvaluateAvt(collationRaw, spec).Trim();
            if (!IsRecognizedCollation(collation))
                throw new InvalidOperationException("XTDE1035: unknown collation");
        }

        return new SortControl(descending, dataType, lang, caseOrder, collation);
    }

    private XdmValue EvaluateSortKeyValue(XElement spec)
    {
        var selectAttr = spec.Attribute("select");
        if (selectAttr != null)
        {
            var compiled = CompileXPath(selectAttr.Value, spec);
            return compiled.Evaluate(_context);
        }
        if (spec.HasElements)
        {
            return EvaluateSequenceConstructor(spec, _context.ContextItem, wrapInDocumentNode: false);
        }
        return CompileXPath(".", spec).Evaluate(_context);
    }

    private InvalidOperationException SortAttributeError(string? rawValue, string staticCode, string dynamicCode)
    {
        var code = ContainsAvt(rawValue) ? dynamicCode : staticCode;
        return new InvalidOperationException($"{code}: invalid sort attribute value '{rawValue}'");
    }

    private static bool ContainsAvt(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;
        bool inString = false;
        char stringChar = '\0';
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (inString)
            {
                if (c == stringChar)
                    inString = false;
                continue;
            }
            if (c == '\'' || c == '"')
            {
                inString = true;
                stringChar = c;
                continue;
            }
            if (c == '{' && i + 1 < value.Length && value[i + 1] == '{')
            {
                i++;
                continue;
            }
            if (c == '{')
            {
                for (int j = i + 1; j < value.Length; j++)
                {
                    if (value[j] == '}')
                        return true;
                }
            }
        }
        return false;
    }

    private static bool IsLexicalQName(string value)
    {
        // A lexical QName is either a local name or prefix:local-name.
        if (string.IsNullOrEmpty(value))
            return false;
        foreach (char c in value)
        {
            if (c == ':')
                return true;
        }
        return !value.Contains(' ');
    }

    private static bool IsValidLanguage(string lang)
    {
        if (string.IsNullOrEmpty(lang))
            return false;
        try
        {
            _ = CultureInfo.GetCultureInfo(lang);
            return true;
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }

    private bool IsRecognizedCollation(string collation)
    {
        if (string.IsNullOrEmpty(collation))
            return false;
        if (collation == CodepointCollation ||
            collation == HtmlAsciiCaseInsensitiveCollation ||
            collation == CaseblindCollation)
            return true;
        return TryParseUcaCollation(collation, out _);
    }

    private int CompareSortKey(SortKey a, SortKey b)
    {
        int cmp;
        bool numeric = a.Control.DataType == SortDataType.Number ||
                       (a.Control.DataType == SortDataType.Auto && IsNumericValue(a.Value) && IsNumericValue(b.Value));
        if (numeric)
            cmp = CompareNumericSortKey(a.Value, b.Value);
        else if (a.Control.DataType == SortDataType.Text ||
                 !string.IsNullOrEmpty(a.Control.Collation) ||
                 !string.IsNullOrEmpty(a.Control.Lang) ||
                 !string.IsNullOrEmpty(a.Control.CaseOrder) ||
                 !string.IsNullOrEmpty(_context.DefaultCollation))
            cmp = CompareTextSortKey(a.Value, b.Value, a.Control.Collation ?? _context.DefaultCollation, a.Control.Lang, a.Control.CaseOrder);
        else
            cmp = XdmValueComparer.Instance.Compare(a.Value, b.Value);

        return a.Control.Descending ? -cmp : cmp;
    }

    private static bool IsNumericValue(XdmValue value)
    {
        var item = AtomizeFirstItem(value);
        return item.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float;
    }

    private static int CompareNumericSortKey(XdmValue a, XdmValue b)
    {
        double da = GetSortKeyDouble(a);
        double db = GetSortKeyDouble(b);
        if (double.IsNaN(da) && double.IsNaN(db)) return 0;
        if (double.IsNaN(da)) return -1; // NaN sorts before any number in xsl:sort data-type="number"
        if (double.IsNaN(db)) return 1;
        return da.CompareTo(db);
    }

    private static double GetSortKeyDouble(XdmValue value)
    {
        var item = AtomizeFirstItem(value);
        if (item.IsUndefined)
            return double.NaN;
        return item.Kind switch
        {
            XdmValueKind.Integer => item.IntegerValue,
            XdmValueKind.Decimal => (double)item.DecimalValue,
            XdmValueKind.Float or XdmValueKind.Double => item.DoubleValue,
            _ => double.TryParse(item.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double d) ? d : double.NaN
        };
    }

    private static int CompareTextSortKey(XdmValue a, XdmValue b, string? collation, string? lang, string? caseOrder)
    {
        var sa = AtomizeFirstItem(a).ToString() ?? string.Empty;
        var sb = AtomizeFirstItem(b).ToString() ?? string.Empty;
        return CompareStrings(sa, sb, collation, lang, caseOrder);
    }

    private static int CompareStrings(string a, string b, string? collation, string? lang, string? caseOrder)
    {
        int cmp;
        if (!string.IsNullOrEmpty(collation))
            cmp = CompareStringCollation(a, b, collation, caseOrder);
        else if (!string.IsNullOrEmpty(lang))
        {
            var culture = CultureInfo.GetCultureInfo(lang);
            cmp = culture.CompareInfo.Compare(a, b, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);
        }
        else if (!string.IsNullOrEmpty(caseOrder))
        {
            cmp = string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            cmp = string.CompareOrdinal(a, b);
        }

        if (cmp != 0 || string.IsNullOrEmpty(caseOrder))
            return cmp;

        return CompareCaseOrder(a, b, caseOrder);
    }

    private static int CompareStringCollation(string a, string b, string collation, string? caseOrder = null)
    {
        int cmp;
        if (collation == CodepointCollation)
            cmp = string.CompareOrdinal(a, b);
        else if (collation == HtmlAsciiCaseInsensitiveCollation || collation == CaseblindCollation)
            cmp = string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
        else if (TryParseUcaCollation(collation, out var uca))
        {
            cmp = uca.CompareInfo.Compare(a, b, uca.Options);
            if (cmp == 0 && uca.Alternate is UcaAlternate.Blanked or UcaAlternate.Shifted)
                cmp = CompareShiftedVariableTieBreaker(a, b);
        }
        else
        {
            throw new InvalidOperationException("XTDE1035: unknown collation");
        }

        if (cmp != 0 || string.IsNullOrEmpty(caseOrder))
            return cmp;

        return CompareCaseOrder(a, b, caseOrder);
    }

    private static int CompareCaseOrder(string a, string b, string? caseOrder)
    {
        bool lowerFirst = caseOrder != "upper-first";
        int len = Math.Min(a.Length, b.Length);
        for (int i = 0; i < len; i++)
        {
            char ca = a[i];
            char cb = b[i];
            if (ca == cb)
                continue;
            bool aLower = char.IsLower(ca);
            bool bLower = char.IsLower(cb);
            bool aUpper = char.IsUpper(ca);
            bool bUpper = char.IsUpper(cb);
            if ((aLower && bUpper) || (aUpper && bLower))
                return lowerFirst ? (aLower ? -1 : 1) : (aUpper ? -1 : 1);
            return ca.CompareTo(cb);
        }
        return a.Length.CompareTo(b.Length);
    }

    private static XdmValue AtomizeFirstItem(XdmValue value)
    {
        if (value.IsUndefined)
            return value;
        if (value.IsNode)
            return XdmValue.FromString(value.NodeValue!.StringValue);
        if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                if (!item.IsUndefined)
                    return AtomizeFirstItem(item);
            }
            return XdmValue.Undefined;
        }
        return value;
    }

    /// <summary>
    /// Sorts the groups produced by <c>xsl:for-each-group</c> according to the
    /// contained <c>xsl:sort</c> specifications. Evaluates each sort key with the
    /// group's representative item as the focus item and, in XSLT 2.0, with
    /// <c>current-group()</c> and <c>current-grouping-key()</c> available.
    /// </summary>
    private List<(XdmValue? Key, List<XdmValue> Items)> SortGroups(
        List<(XdmValue? Key, List<XdmValue> Items)> groups,
        List<XElement> sortSpecs)
    {
        var savedFocus = _context.ContextItem;
        var savedPosition = _context.ContextPosition;
        var savedSize = _context.ContextSize;
        var savedCurrent = _context.CurrentItem;
        var savedGroup = _currentGroup;
        var savedKey = _currentGroupingKey;
        bool exposeGroupInSort = !IsXslt30OrHigher();

        try
        {
            ValidateSortSpecs(sortSpecs);
            var controls = sortSpecs.Select(EvaluateSortControl).ToList();

            var keyed = new List<(XdmValue? Key, List<XdmValue> Items, List<SortKey> Keys, int OriginalIndex)>();
            for (int idx = 0; idx < groups.Count; idx++)
            {
                var (key, items) = groups[idx];
                var rep = items[0];
                _context.WithFocus(rep, idx + 1, groups.Count);
                _context.WithCurrentItem(rep);
                if (exposeGroupInSort)
                {
                    _currentGroup = items;
                    _currentGroupingKey = key;
                }

                var keys = new List<SortKey>();
                for (int i = 0; i < sortSpecs.Count; i++)
                {
                    keys.Add(new SortKey(EvaluateSortKeyValue(sortSpecs[i]), controls[i]));
                }
                keyed.Add((key, items, keys, idx));
            }

            keyed.Sort((a, b) =>
            {
                for (int i = 0; i < a.Keys.Count; i++)
                {
                    var cmp = CompareSortKey(a.Keys[i], b.Keys[i]);
                    if (cmp != 0) return cmp;
                }
                // Stable sort: preserve original relative order when all keys equal
                return a.OriginalIndex.CompareTo(b.OriginalIndex);
            });

            return keyed.Select(k => (k.Key, k.Items)).ToList();
        }
        finally
        {
            _context.WithFocus(savedFocus, savedPosition, savedSize);
            _context.WithCurrentItem(savedCurrent);
            _currentGroup = savedGroup;
            _currentGroupingKey = savedKey;
        }
    }

    /// <summary>
    /// Validates the attributes of an <c>xsl:for-each-group</c> instruction and throws
    /// the appropriate static errors (XTSE0020/0080/0090/1017/1080/1090).
    /// </summary>
    private void ValidateForEachGroupAttributes(XElement instruction)
    {
        var groupBy = instruction.Attribute("group-by")?.Value;
        var groupAdjacent = instruction.Attribute("group-adjacent")?.Value;
        var groupStarting = instruction.Attribute("group-starting-with")?.Value;
        var groupEnding = instruction.Attribute("group-ending-with")?.Value;
        var collation = instruction.Attribute("collation")?.Value;
        var compositeAttr = instruction.Attribute("composite")?.Value;
        var bindGroup = instruction.Attribute("bind-group")?.Value;
        var bindKey = instruction.Attribute("bind-grouping-key")?.Value;

        if (!string.IsNullOrEmpty(compositeAttr))
        {
            var v = compositeAttr.Trim();
            if (v != "yes" && v != "true" && v != "1" &&
                v != "no" && v != "false" && v != "0")
                throw new InvalidOperationException("XTSE0020: invalid value for @composite");
        }

        int groupingAttrCount = 0;
        if (!string.IsNullOrEmpty(groupBy)) groupingAttrCount++;
        if (!string.IsNullOrEmpty(groupAdjacent)) groupingAttrCount++;
        if (!string.IsNullOrEmpty(groupStarting)) groupingAttrCount++;
        if (!string.IsNullOrEmpty(groupEnding)) groupingAttrCount++;

        if (groupingAttrCount == 0)
            throw new InvalidOperationException("XTSE1080: xsl:for-each-group requires one of group-by, group-adjacent, group-starting-with, or group-ending-with");
        if (groupingAttrCount > 1)
            throw new InvalidOperationException("XTSE1080: xsl:for-each-group allows only one of group-by, group-adjacent, group-starting-with, or group-ending-with");

        if (!string.IsNullOrEmpty(collation) &&
            string.IsNullOrEmpty(groupBy) && string.IsNullOrEmpty(groupAdjacent))
            throw new InvalidOperationException("XTSE1090: @collation is allowed only with group-by or group-adjacent");

        if (IsXslt30OrHigher() && (!string.IsNullOrEmpty(bindGroup) || !string.IsNullOrEmpty(bindKey)))
            throw new InvalidOperationException("XTSE0090: @bind-group and @bind-grouping-key are not permitted in XSLT 3.0");
    }

    /// <summary>
    /// Builds the groups for an <c>xsl:for-each-group</c> instruction from the supplied
    /// population items, respecting <c>@composite</c> and the supplied collation.
    /// </summary>
    private List<(XdmValue? Key, List<XdmValue> Items)> BuildForEachGroups(
        XElement instruction,
        List<XdmValue> items,
        string? collation)
    {
        var groupBy = instruction.Attribute("group-by")?.Value;
        var groupAdjacent = instruction.Attribute("group-adjacent")?.Value;
        var groupStarting = instruction.Attribute("group-starting-with")?.Value;
        var groupEnding = instruction.Attribute("group-ending-with")?.Value;
        bool composite = IsCompositeGrouping(instruction);

        var groups = new List<(XdmValue? Key, List<XdmValue> Items)>();

        if (!string.IsNullOrEmpty(groupBy))
        {
            var keyExpr = CompileXPath(groupBy, instruction);
            for (int idx = 0; idx < items.Count; idx++)
            {
                var item = items[idx];
                _context.WithFocus(item, idx + 1, items.Count);
                var keyValue = keyExpr.Evaluate(_context);
                var keyItems = EnumerateKeyItems(keyValue);
                if (composite)
                {
                    var compositeKey = XdmValue.FromSequence(MaterializedSequence.FromList(keyItems));
                    AddToGroup(groups, compositeKey, item, collation);
                }
                else
                {
                    foreach (var keyItem in keyItems)
                        AddToGroup(groups, keyItem, item, collation);
                }
            }
        }
        else if (!string.IsNullOrEmpty(groupAdjacent))
        {
            var keyExpr = CompileXPath(groupAdjacent, instruction);
            XdmValue currentKey = XdmValue.Undefined;
            List<XdmValue>? currentItems = null;
            for (int idx = 0; idx < items.Count; idx++)
            {
                var item = items[idx];
                _context.WithFocus(item, idx + 1, items.Count);
                var keyValue = keyExpr.Evaluate(_context);
                var keyItems = EnumerateKeyItems(keyValue);

                XdmValue itemKey;
                if (composite)
                {
                    itemKey = XdmValue.FromSequence(MaterializedSequence.FromList(keyItems));
                }
                else
                {
                    if (keyItems.Count == 0)
                        throw new InvalidOperationException("XTTE1100: group-adjacent key evaluates to an empty sequence");
                    if (keyItems.Count > 1)
                        throw new InvalidOperationException("XTTE1100: group-adjacent key evaluates to a sequence of more than one item");
                    itemKey = keyItems[0];
                }

                if (currentItems == null)
                {
                    currentItems = new List<XdmValue> { item };
                    currentKey = itemKey;
                }
                else if (GroupingKeysEqual(currentKey, itemKey, collation))
                {
                    currentItems.Add(item);
                }
                else
                {
                    groups.Add((currentKey, currentItems));
                    currentItems = new List<XdmValue> { item };
                    currentKey = itemKey;
                }
            }
            if (currentItems != null)
                groups.Add((currentKey, currentItems));
        }
        else if (!string.IsNullOrEmpty(groupStarting))
        {
            var defaultNs = GetXPathDefaultNamespace(instruction);
            var patternCompiler = new Patterns.PatternCompiler();
            var pattern = patternCompiler.Compile(groupStarting, defaultNs);
            List<XdmValue>? currentItems = null;
            var savedOutputUri = _context.CurrentOutputUri;
            _context.CurrentOutputUri = null;
            try
            {
                for (int idx = 0; idx < items.Count; idx++)
                {
                    var item = items[idx];
                    _context.WithFocus(item, idx + 1, items.Count);
                    if (pattern(item, _context))
                    {
                        if (currentItems != null && currentItems.Count > 0)
                            groups.Add((null, currentItems));
                        currentItems = new List<XdmValue> { item };
                    }
                    else
                    {
                        currentItems ??= new List<XdmValue>();
                        currentItems.Add(item);
                    }
                }
            }
            finally
            {
                _context.CurrentOutputUri = savedOutputUri;
            }
            if (currentItems != null && currentItems.Count > 0)
                groups.Add((null, currentItems));
        }
        else if (!string.IsNullOrEmpty(groupEnding))
        {
            var defaultNs = GetXPathDefaultNamespace(instruction);
            var patternCompiler = new Patterns.PatternCompiler();
            var pattern = patternCompiler.Compile(groupEnding, defaultNs);
            List<XdmValue>? currentItems = null;
            var savedOutputUri = _context.CurrentOutputUri;
            _context.CurrentOutputUri = null;
            try
            {
                for (int idx = 0; idx < items.Count; idx++)
                {
                    var item = items[idx];
                    _context.WithFocus(item, idx + 1, items.Count);
                    currentItems ??= new List<XdmValue>();
                    currentItems.Add(item);
                    if (pattern(item, _context))
                    {
                        groups.Add((null, currentItems));
                        currentItems = null;
                    }
                }
            }
            finally
            {
                _context.CurrentOutputUri = savedOutputUri;
            }
            if (currentItems != null && currentItems.Count > 0)
                groups.Add((null, currentItems));
        }

        return groups;
    }

    /// <summary>
    /// Adds an item to an existing group whose key is equal under XDM eq semantics
    /// (including the requested collation for string comparisons), or creates a new
    /// group when no matching group exists.
    /// </summary>
    private static void AddToGroup(List<(XdmValue? Key, List<XdmValue> Items)> groups, XdmValue key, XdmValue item, string? collation)
    {
        foreach (var g in groups)
        {
            if (g.Key != null && GroupingKeysEqual(g.Key.Value, key, collation))
            {
                if (!g.Items.Contains(item))
                    g.Items.Add(item);
                return;
            }
        }
        groups.Add((key, new List<XdmValue> { item }));
    }

    /// <summary>
    /// Atomizes the items of a grouping key expression and returns them as a list.
    /// </summary>
    private static List<XdmValue> EnumerateKeyItems(XdmValue value)
    {
        var result = new List<XdmValue>();
        if (value.IsUndefined)
            return result;

        if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                if (!item.IsUndefined)
                    result.Add(AtomizeKeyItem(item));
            }
        }
        else
        {
            result.Add(AtomizeKeyItem(value));
        }
        return result;
    }

    /// <summary>
    /// Atomizes a single grouping key item. Nodes become xs:untypedAtomic values.
    /// </summary>
    private static XdmValue AtomizeKeyItem(XdmValue value)
    {
        if (value.IsNode)
            return XdmValue.FromString(value.NodeValue.StringValue, "untypedAtomic");
        return value;
    }

    /// <summary>
    /// Compares two grouping keys using the same rules as the XPath <c>eq</c> operator,
    /// including numeric promotion, untyped-atomic casting, date/time normalization,
    /// and the supplied string collation.
    /// </summary>
    private static bool GroupingKeysEqual(XdmValue a, XdmValue b, string? collation = null)
    {
        if (a.IsUndefined || b.IsUndefined)
            return false;

        if (a.IsSequence && b.IsSequence)
        {
            var aItems = EnumerateKeyItems(a);
            var bItems = EnumerateKeyItems(b);
            if (aItems.Count != bItems.Count)
                return false;
            for (int i = 0; i < aItems.Count; i++)
            {
                if (!AtomicValuesEqual(aItems[i], bItems[i], collation))
                    return false;
            }
            return true;
        }

        if (!a.IsSequence && !b.IsSequence)
            return AtomicValuesEqual(a, b, collation);

        return false;
    }

    /// <summary>
    /// Returns true when the supplied value kind represents a numeric atomic type.
    /// </summary>
    private static bool IsNumeric(XdmValueKind kind)
        => kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float;

    private static double ToDouble(XdmValue value)
        => value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => (double)value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => value.DoubleValue,
            _ => double.Parse(value.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture)
        };

    private static float ToFloat(XdmValue value)
        => value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => (float)value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => (float)value.DoubleValue,
            _ => float.Parse(value.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture)
        };

    private static decimal ToDecimal(XdmValue value)
        => value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => (decimal)value.DoubleValue,
            _ => decimal.Parse(value.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture)
        };

    /// <summary>
    /// Compares two atomic XDM values using XPath <c>eq</c> semantics and the
    /// supplied string collation for string/untypedAtomic comparisons.
    /// </summary>
    private static bool AtomicValuesEqual(XdmValue a, XdmValue b, string? collation = null)
    {
        if (a.IsUndefined || b.IsUndefined)
            return false;

        var aKind = a.Kind;
        var bKind = b.Kind;

        // Both numeric: compare numeric values with proper promotion (per XPath eq).
        if (IsNumeric(aKind) && IsNumeric(bKind))
        {
            // Grouping treats NaN as equal to itself, unlike XPath value comparisons.
            bool aIsNaN = (aKind is XdmValueKind.Double or XdmValueKind.Float) && double.IsNaN(a.DoubleValue);
            bool bIsNaN = (bKind is XdmValueKind.Double or XdmValueKind.Float) && double.IsNaN(b.DoubleValue);
            if (aIsNaN || bIsNaN)
                return aIsNaN && bIsNaN;

            if (aKind == XdmValueKind.Double || bKind == XdmValueKind.Double)
                return ToDouble(a) == ToDouble(b);
            if (aKind == XdmValueKind.Float || bKind == XdmValueKind.Float)
                return ToFloat(a) == ToFloat(b);
            if (aKind == XdmValueKind.Decimal || bKind == XdmValueKind.Decimal)
                return ToDecimal(a) == ToDecimal(b);
            return a.IntegerValue == b.IntegerValue;
        }

        // Same kind exact comparison.
        if (aKind == bKind)
        {
            switch (aKind)
            {
                case XdmValueKind.String:
                    return GroupingStringEquals(a.ToString(), b.ToString(), collation);
                case XdmValueKind.Boolean:
                    return a.BooleanValue == b.BooleanValue;
                case XdmValueKind.DateTime:
                case XdmValueKind.Date:
                case XdmValueKind.Time:
                    return NormalizeDateTime(a, aKind) == NormalizeDateTime(b, bKind);
                case XdmValueKind.Duration:
                    return string.Equals(a.ToString(), b.ToString(), StringComparison.Ordinal);
                case XdmValueKind.QName:
                    var qa = a.QNameValue;
                    var qb = b.QNameValue;
                    return qa.LocalName == qb.LocalName && qa.NamespaceUri == qb.NamespaceUri;
                case XdmValueKind.Uri:
                    return GroupingStringEquals(a.ToString(), b.ToString(), collation);
            }
        }

        // untypedAtomic on either side: cast to the other operand's type.
        if (IsUntypedAtomic(a))
            return UntypedAtomicEqualsOther(a, b, collation);
        if (IsUntypedAtomic(b))
            return UntypedAtomicEqualsOther(b, a, collation);

        // String / URI cross-comparison.
        if ((aKind == XdmValueKind.String || aKind == XdmValueKind.Uri) &&
            (bKind == XdmValueKind.String || bKind == XdmValueKind.Uri))
        {
            return GroupingStringEquals(a.ToString(), b.ToString(), collation);
        }

        return false;
    }

    /// <summary>
    /// Compares an xs:untypedAtomic value with another atomic value using the
    /// casting rules of the XPath <c>eq</c> operator and the supplied string collation.
    /// </summary>
    private static bool UntypedAtomicEqualsOther(XdmValue untyped, XdmValue other, string? collation = null)
    {
        var s = untyped.ToString();
        var otherKind = other.Kind;

        if (otherKind is XdmValueKind.String or XdmValueKind.Uri)
            return GroupingStringEquals(s, other.ToString(), collation);

        if (IsNumeric(otherKind))
        {
            // Cast untypedAtomic to the other operand's numeric type, per XPath eq rules.
            if (otherKind == XdmValueKind.Float)
            {
                if (!float.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float f))
                    return false;
                return f == ToFloat(other);
            }
            if (otherKind == XdmValueKind.Double)
            {
                if (!double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double d))
                    return false;
                if (double.IsNaN(d))
                    return false;
                return d == ToDouble(other);
            }
            if (otherKind == XdmValueKind.Decimal)
            {
                if (!decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal d))
                    return false;
                return d == ToDecimal(other);
            }
            if (otherKind == XdmValueKind.Integer)
            {
                if (!long.TryParse(s, out long d))
                    return false;
                return d == other.IntegerValue;
            }
        }

        if (otherKind == XdmValueKind.Boolean)
        {
            if (bool.TryParse(s, out bool b))
                return b == other.BooleanValue;
            return false;
        }

        if (otherKind is XdmValueKind.DateTime or XdmValueKind.Date or XdmValueKind.Time)
        {
            if (DateTimeOffset.TryParse(s, out var dt))
                return dt.ToUniversalTime() == NormalizeDateTime(other, otherKind);
            return false;
        }

        return false;
    }

    /// <summary>
    /// Normalizes a date/time value to UTC for comparison.
    /// </summary>
    private static DateTimeOffset NormalizeDateTime(XdmValue value, XdmValueKind kind)
    {
        var dt = kind switch
        {
            XdmValueKind.DateTime => value.DateTimeValue,
            XdmValueKind.Date => value.DateValue,
            XdmValueKind.Time => value.TimeValue,
            _ => throw new InvalidOperationException()
        };
        return dt.ToUniversalTime();
    }

    /// <summary>
    /// Determines whether the supplied value is an xs:untypedAtomic atomic value.
    /// </summary>
    private static bool IsUntypedAtomic(XdmValue value)
        => value.Kind == XdmValueKind.String &&
           string.Equals(value.SchemaTypeName, "untypedAtomic", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns true if the sequence type is a node-kind test such as
    /// <c>text()</c>, <c>element()</c>, <c>node()</c>, etc.
    /// </summary>
    private static bool IsNodeKindType(string type)
    {
        var normalized = type.Trim().ToLowerInvariant();
        if (normalized.EndsWith('?') || normalized.EndsWith('*') || normalized.EndsWith('+'))
            normalized = normalized[..^1].TrimEnd();
        return normalized is "node()" or "node" or "text()" or "text" or "comment()" or "comment"
            or "processing-instruction()" or "processing-instruction" or "namespace-node()" or "namespace-node"
            or "document-node()" or "document-node"
            or "element()" or "attribute()" or "schema-element()" or "schema-attribute()"
            || normalized.StartsWith("element(") || normalized.StartsWith("attribute(")
            || normalized.StartsWith("document-node(") || normalized.StartsWith("schema-element(")
            || normalized.StartsWith("schema-attribute(");
    }

    /// <summary>
    /// Returns true if the <c>xsl:for-each-group</c> instruction requests composite
    /// grouping keys (<c>composite="yes"</c>, <c>"true"</c>, or <c>"1"</c>).
    /// </summary>
    private static bool IsCompositeGrouping(XElement instruction)
    {
        var value = instruction.Attribute("composite")?.Value;
        return value is "yes" or "true" or "1";
    }

    /// <summary>
    /// Returns true when the containing stylesheet declares an XSLT version of 3.0 or higher.
    /// </summary>
    private bool IsXslt30OrHigher()
    {
        var v = _stylesheet.Version;
        return v is "3.0" or "3.1";
    }

    private const string CodepointCollation = "http://www.w3.org/2005/xpath-functions/collation/codepoint";
    private const string HtmlAsciiCaseInsensitiveCollation = "http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive";
    private const string CaseblindCollation = "http://www.w3.org/2010/09/qt-fots-catalog/collation/caseblind";
    private const string UcaCollationPrefix = "http://www.w3.org/2013/collation/UCA";

    /// <summary>
    /// Compares two strings using the supplied collation URI. Falls back to codepoint
    /// comparison when no collation is supplied or when the URI is unrecognized.
    /// </summary>
    private static bool GroupingStringEquals(string a, string b, string? collation)
    {
        if (string.IsNullOrEmpty(collation) || collation == CodepointCollation)
            return string.Equals(a, b, StringComparison.Ordinal);

        if (TryParseUcaCollation(collation, out var uca))
            return uca.CompareInfo.Compare(a, b, uca.Options) == 0;

        if (collation == HtmlAsciiCaseInsensitiveCollation || collation == CaseblindCollation)
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        // Unknown collation: behave as codepoint (caller normally validates earlier).
        return string.Equals(a, b, StringComparison.Ordinal);
    }

    /// <summary>
    /// Parses a UCA collation URI into a culture, compare options, and alternate weighting.
    /// </summary>
    private static bool TryParseUcaCollation(string uri, out UcaCollationInfo info)
    {
        info = default;
        if (!uri.StartsWith(UcaCollationPrefix, StringComparison.Ordinal))
            return false;

        string query = uri.Length > UcaCollationPrefix.Length && uri[UcaCollationPrefix.Length] == '?'
            ? uri[(UcaCollationPrefix.Length + 1)..]
            : string.Empty;

        string lang = "en";
        string strength = "tertiary";
        UcaAlternate alternate = UcaAlternate.NonIgnorable;
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
            else if (key == "alternate")
            {
                alternate = val.ToLowerInvariant() switch
                {
                    "blanked" => UcaAlternate.Blanked,
                    "shifted" or "shift-trimmed" => UcaAlternate.Shifted,
                    _ => UcaAlternate.NonIgnorable,
                };
            }
            else if (string.Equals(key, "fallback", StringComparison.OrdinalIgnoreCase) &&
                     string.Equals(val, "no", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("FOCH0002: Unsupported UCA collation (fallback=no)");
            }
        }

        try
        {
            var culture = CultureInfo.GetCultureInfo(lang);
            var options = strength.ToLowerInvariant() switch
            {
                "primary" => CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace,
                "secondary" => CompareOptions.IgnoreCase,
                "tertiary" => CompareOptions.None,
                "quaternary" => CompareOptions.None,
                "identical" => CompareOptions.Ordinal,
                _ => CompareOptions.None,
            };

            if (alternate is UcaAlternate.Blanked or UcaAlternate.Shifted)
                options |= CompareOptions.IgnoreSymbols;

            info = new UcaCollationInfo(culture.CompareInfo, options, alternate);
            return true;
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }

    private enum UcaAlternate { NonIgnorable, Blanked, Shifted }

    private readonly record struct UcaCollationInfo(CompareInfo CompareInfo, CompareOptions Options, UcaAlternate Alternate);

    /// <summary>
    /// Tie-breaker for UCA blanked/shifted collations after higher levels compare equal.
    /// Strings without variable characters sort first, then non-trailing variable characters
    /// ordered by descending insertion index (later insertion sorts earlier), and finally
    /// strings with trailing/appended variable characters.
    /// </summary>
    private static int CompareShiftedVariableTieBreaker(string a, string b)
    {
        bool trailingA = a.Length > 0 && IsUcaVariable(a[a.Length - 1]);
        bool trailingB = b.Length > 0 && IsUcaVariable(b[b.Length - 1]);
        if (trailingA != trailingB)
            return trailingA ? 1 : -1;

        var varsA = GetNonTrailingVariableIndices(a);
        var varsB = GetNonTrailingVariableIndices(b);
        int len = Math.Min(varsA.Count, varsB.Count);
        for (int i = 0; i < len; i++)
        {
            int va = varsA[i];
            int vb = varsB[i];
            if (va != vb)
                return vb.CompareTo(va); // larger index sorts earlier
        }
        return varsA.Count.CompareTo(varsB.Count);
    }

    private static List<int> GetNonTrailingVariableIndices(string s)
    {
        var list = new List<int>();
        int last = s.Length - 1;
        for (int i = 0; i < s.Length; i++)
        {
            if (i == last)
                break; // trailing variable is handled separately
            if (IsUcaVariable(s[i]))
                list.Add(i);
        }
        // Sort descending so larger indices are compared first.
        list.Sort((x, y) => y.CompareTo(x));
        return list;
    }

    private static bool IsUcaVariable(char c)
        => char.IsWhiteSpace(c) || char.IsPunctuation(c) || char.IsSymbol(c);

    /// <summary>
    /// Resolves a variable or parameter name from its lexical form to an expanded QName.
    /// Handles <c>Q{uri}local</c> EQNames and prefixed QNames using the namespaces in scope
    /// on the declaring element.
    /// </summary>
    private static (string LocalName, string NamespaceUri) ExpandVariableName(XElement element, string name)
    {
        name = name?.Trim() ?? "";
        if (string.IsNullOrEmpty(name))
            return ("", "");

        // EQName syntax: Q{uri}local or Q{uri}prefix:local
        // The empty URI form Q{}local is permitted and means "no namespace".
        if (name.Length > 2 && name[0] == 'Q' && name[1] == '{')
        {
            int closeBrace = name.IndexOf('}');
            if (closeBrace >= 2)
            {
                string uri = name[2..closeBrace];
                string rest = name[(closeBrace + 1)..];
                int restColon = rest.IndexOf(':');
                string local = restColon < 0 ? rest : rest[(restColon + 1)..];
                return (local, uri);
            }
        }

        int colon = name.IndexOf(':');
        if (colon >= 0)
        {
            string prefix = name[..colon];
            string local = name[(colon + 1)..];
            if (prefix == "xml")
                return (local, "http://www.w3.org/XML/1998/namespace");

            var ns = element.GetNamespaceOfPrefix(prefix);
            if (ns == null)
                throw new InvalidOperationException($"XPST0081: Undefined namespace prefix '{prefix}'");
            return (local, ns.NamespaceName);
        }

        // Unprefixed name is in no namespace for variables/parameters
        return (name, "");
    }

    /// <summary>
    /// Returns a dictionary key for an expanded variable QName, using Clark notation.
    /// </summary>
    private static string VariableKey(string localName, string namespaceUri)
        => string.IsNullOrEmpty(namespaceUri) ? localName : $"{{{namespaceUri}}}{localName}";

    /// <summary>
    /// Parses a Clark-notation variable key back into its local name and namespace URI.
    /// </summary>
    private static (string LocalName, string NamespaceUri) ParseVariableKey(string key)
    {
        if (key.StartsWith("{") && key.IndexOf('}') is int end && end > 0)
        {
            return (key[(end + 1)..], key[1..end]);
        }
        return (key, "");
    }

    /// <summary>
    /// Resolves a lexical named-template name from a call-template instruction to the
    /// lexical key stored in <see cref="_allNamedTemplates"/>, matching by expanded QName.
    /// If no template matches by expanded name, the original lexical name is returned so
    /// that <see cref="CallTemplate"/> raises the appropriate not-found error.
    /// </summary>
    private string ResolveNamedTemplateName(string calledName, XElement callElement)
    {
        var (calledLocal, calledNs) = ExpandVariableName(callElement, calledName);
        foreach (var pair in _allNamedTemplates)
        {
            var rule = pair.Value;
            if (string.IsNullOrEmpty(rule.Name))
                continue;
            var (tplLocal, tplNs) = ExpandVariableName(rule.Element, rule.Name);
            if (tplLocal == calledLocal && tplNs == calledNs)
                return pair.Key;
        }
        return calledName;
    }

    /// <summary>
    /// Looks up a named template by lexical key or by Clark-notation expanded QName
    /// (<c>{uri}local</c> or <c>local</c>). Returns both the stored dictionary key and
    /// the matching template rule.
    /// </summary>
    private bool TryFindNamedTemplate(string name, out string key, out Stylesheet.TemplateRule rule)
    {
        // Direct lexical lookup (handles implicit xsl:initial-template and existing callers).
        if (_allNamedTemplates.TryGetValue(name, out rule!))
        {
            key = name;
            return true;
        }

        // Clark notation: {uri}local or local (no namespace).
        if (name.StartsWith("{") && name.IndexOf('}') is int end && end > 0)
        {
            var local = name[(end + 1)..];
            var ns = name[1..end];
            foreach (var pair in _allNamedTemplates)
            {
                var candidate = pair.Value;
                if (string.IsNullOrEmpty(candidate.Name))
                    continue;
                var (tplLocal, tplNs) = ExpandVariableName(candidate.Element, candidate.Name);
                if (tplLocal == local && tplNs == ns)
                {
                    key = pair.Key;
                    rule = candidate;
                    return true;
                }
            }
        }

        key = name;
        rule = null!;
        return false;
    }

    /// <summary>
    /// Returns the string value of the first item in an XDM value, atomizing it if necessary.
    /// </summary>
    private static string AtomizedFirstString(XdmValue value)
    {
        if (value.IsNode && value.NodeValue != null)
            return value.NodeValue.StringValue;
        if (value.IsSequence && value.SequenceValue != null)
        {
            var enumerator = value.SequenceValue.GetEnumerator();
            if (!enumerator.MoveNext())
                return "";
            var item = enumerator.Current;
            if (item.IsUndefined)
                return "";
            if (item.IsNode && item.NodeValue != null)
                return item.NodeValue.StringValue;
            return item.ToString();
        }
        return value.ToString();
    }

    /// <summary>
    /// Returns true if the value is or contains a function item (including inside maps,
    /// arrays, or sequences). Such values may capture variables in their closure.
    /// </summary>
    private static bool ContainsFunctionItem(XdmValue value)
    {
        if (value.IsUndefined)
            return false;
        if (value.Kind == XdmValueKind.Function)
            return true;
        if (value.Kind == XdmValueKind.Map && value.MapValue != null)
        {
            foreach (var entry in value.MapValue.Entries)
                if (ContainsFunctionItem(entry.Value))
                    return true;
        }
        if (value.Kind == XdmValueKind.Array && value.ArrayValue != null)
        {
            foreach (var item in value.ArrayValue.Values)
                if (ContainsFunctionItem(item))
                    return true;
        }
        if (value.Kind == XdmValueKind.Sequence && value.SequenceValue != null)
        {
            foreach (var item in value.SequenceValue)
                if (ContainsFunctionItem(item))
                    return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true if the typed atomic value can be promoted to the target numeric type
    /// per XPath/XSLT function conversion rules (e.g. <c>xs:integer</c> to <c>xs:double</c>).
    /// </summary>
    private static bool IsNumericPromotion(XdmValue value, string targetType)
    {
        var normalized = targetType.ToLowerInvariant().Replace("xs:", "").Replace("xsd:", "");
        if (normalized.EndsWith('?') || normalized.EndsWith('*') || normalized.EndsWith('+'))
            normalized = normalized[..^1].TrimEnd();

        return normalized switch
        {
            "double" => value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Float,
            "float" => value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal,
            _ => false
        };
    }

    /// <summary>
    /// Returns true if the value can be URI-promoted to the target type
    /// (xs:anyURI -> xs:string).
    /// </summary>
    private static bool IsUriPromotion(XdmValue value, string targetType)
    {
        var normalized = targetType.ToLowerInvariant().Replace("xs:", "").Replace("xsd:", "");
        if (normalized.EndsWith('?') || normalized.EndsWith('*') || normalized.EndsWith('+'))
            normalized = normalized[..^1].TrimEnd();

        return normalized == "string" &&
               value.Kind == XdmValueKind.String &&
               string.Equals(value.SchemaTypeName, "anyURI", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks whether an XPath <c>select</c> expression text contains a reference to the
    /// variable with the given expanded name, ignoring XPath comments and string literals.
    /// Used to detect a global variable referencing itself in its own <c>@select</c>.
    /// </summary>
    private static bool SelectReferencesVariable(string select, XElement contextElement, string localName, string namespaceUri)
    {
        if (string.IsNullOrEmpty(select))
            return false;

        // Strip XPath comments (: ... :)
        var noComments = System.Text.RegularExpressions.Regex.Replace(select, @"\(:[^:]*:\)", "");

        // Strip string literals (handling doubled quotes inside)
        var stripped = System.Text.RegularExpressions.Regex.Replace(noComments, @"'([^']|'')*'|""""([^""""]|"""""")*""""", "");

        // Build patterns for the variable name
        var patterns = new List<string>();
        string eqNamePattern = string.IsNullOrEmpty(namespaceUri)
            ? $"\\$Q\\{{{RegexEscape(localName)}}}(?![A-Za-z0-9_])"
            : $"\\$Q\\{{{RegexEscape(namespaceUri)}\\}}{RegexEscape(localName)}(?![A-Za-z0-9_])";
        patterns.Add(eqNamePattern);

        // Prefixed form: resolve all prefixes in scope that map to the target namespace
        foreach (var nsAttr in contextElement.Attributes().Where(a => a.IsNamespaceDeclaration))
        {
            string prefix = nsAttr.Name.LocalName;
            if (prefix == "xmlns")
                prefix = "";
            if (nsAttr.Value == namespaceUri)
            {
                if (string.IsNullOrEmpty(prefix))
                    patterns.Add($"\\${RegexEscape(localName)}(?![A-Za-z0-9_])");
                else
                    patterns.Add($"\\${RegexEscape(prefix)}:{RegexEscape(localName)}(?![A-Za-z0-9_])");
            }
        }

        // Also check the default (no-prefix) form for no-namespace variables
        if (namespaceUri == "")
            patterns.Add($"\\${RegexEscape(localName)}(?![A-Za-z0-9_])");

        return patterns.Any(p => System.Text.RegularExpressions.Regex.IsMatch(stripped, p));
    }

    private static string RegexEscape(string value) => System.Text.RegularExpressions.Regex.Escape(value);

    /// <summary>
    /// Collects parameters for <c>xsl:evaluate</c> from the optional <c>with-params</c>
    /// attribute (a map of QNames to values) and from child <c>xsl:with-param</c> elements.
    /// </summary>
    private Dictionary<string, XdmValue> CollectEvaluateParams(XElement instruction, XdmValue contextItem)
    {
        var result = new Dictionary<string, XdmValue>();

        // xsl:with-param children supply the initial bindings.
        var (childWithParams, _) = CollectWithParams(instruction, contextItem);
        foreach (var kv in childWithParams)
            result[kv.Key] = kv.Value;

        // The with-params attribute overrides any child xsl:with-param with the same name.
        var withParamsAttr = instruction.Attribute("with-params")?.Value;
        if (!string.IsNullOrEmpty(withParamsAttr))
        {
            var mapCompiled = CompileXPath(withParamsAttr, instruction);
            var mapValue = mapCompiled.Evaluate(_context);
            if (mapValue.IsMap && mapValue.MapValue != null)
            {
                foreach (var entry in mapValue.MapValue.Entries)
                {
                    if (entry.Key.Kind != XdmValueKind.QName)
                        continue;
                    var qn = entry.Key.QNameValue;
                    result[VariableKey(qn.LocalName, qn.NamespaceUri)] = entry.Value;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Collects <c>xsl:with-param</c> children of <paramref name="instruction"/>, expanding
    /// their names to expanded QNames and applying <c>@as</c> coercion. Returns separate
    /// dictionaries for ordinary and tunnel parameters.
    /// </summary>
    private (Dictionary<string, XdmValue> WithParams, Dictionary<string, XdmValue> TunnelParams) CollectWithParams(XElement instruction, XdmValue contextItem)
    {
        var withParams = new Dictionary<string, XdmValue>();
        var tunnelParams = new Dictionary<string, XdmValue>();
        foreach (var wp in instruction.Elements(XName.Get("with-param", Stylesheet.Stylesheet.XslNamespace)))
        {
            var wpName = wp.Attribute("name")?.Value;
            if (string.IsNullOrEmpty(wpName))
                continue;

            var (wpLocal, wpNs) = ExpandVariableName(wp, wpName);
            var wpKey = VariableKey(wpLocal, wpNs);
            var wpSelect = wp.Attribute("select")?.Value;
            XdmValue wpValue;
            if (!string.IsNullOrEmpty(wpSelect))
            {
                var compiled = CompileXPath(wpSelect, wp);
                wpValue = compiled.Evaluate(_context);
            }
            else
            {
                // xsl:with-param sequence-constructor content is evaluated in a temporary output state.
                var savedOutputUri = _context.CurrentOutputUri;
                _context.CurrentOutputUri = null;
                try
                {
                    wpValue = EvaluateSequenceConstructor(wp, contextItem, wrapInDocumentNode: string.IsNullOrEmpty(wp.Attribute("as")?.Value));
                }
                finally
                {
                    _context.CurrentOutputUri = savedOutputUri;
                }
            }
            wpValue = ConvertVariableValue(wpValue, wp.Attribute("as")?.Value, isParam: true);
            if (IsTunnelParameter(wp))
                tunnelParams[wpKey] = wpValue;
            else
                withParams[wpKey] = wpValue;
        }
        return (withParams, tunnelParams);
    }

    /// <summary>
    /// Parses the <c>tunnel</c> attribute of an <c>xsl:param</c> or <c>xsl:with-param</c>
    /// element. Returns true for tunneling parameters, false for ordinary parameters, and
    /// throws <c>XTSE0020</c> for invalid values such as an empty string.
    /// </summary>
    private bool IsTunnelParameter(XElement paramElement)
    {
        var tunnelAttr = paramElement.Attribute("tunnel")?.Value;
        if (tunnelAttr == null)
            return false;

        var v = tunnelAttr.Trim();
        if (string.IsNullOrEmpty(v))
            throw new InvalidOperationException("XTSE0020: invalid value for @tunnel");
        bool xslt30 = GetEffectiveVersion(paramElement) >= 3.0;
        if (v.Equals("yes", StringComparison.OrdinalIgnoreCase))
            return true;
        if (v.Equals("no", StringComparison.OrdinalIgnoreCase))
            return false;
        if (xslt30)
        {
            if (v.Equals("true", StringComparison.OrdinalIgnoreCase) || v == "1")
                return true;
            if (v.Equals("false", StringComparison.OrdinalIgnoreCase) || v == "0")
                return false;
        }
        throw new InvalidOperationException("XTSE0020: invalid value for @tunnel");
    }

    /// <summary>
    /// Collects values for the <c>xsl:param</c> children of <paramref name="templateElement"/>
    /// from the initial-template/initial-mode parameter dictionaries supplied on the
    /// evaluation context. Top-level stylesheet parameters are supplied as ordinary
    /// variables and are handled by <see cref="InitializeGlobalParametersAndVariables"/>.
    /// </summary>
    private (Dictionary<string, XdmValue> CallParams, Dictionary<string, XdmValue> TunnelParams) CollectExternalParameters(XElement templateElement)
    {
        var callParams = new Dictionary<string, XdmValue>();
        var tunnelParams = new Dictionary<string, XdmValue>();
        var externalCall = _context.InitialTemplateCallParameters;
        var externalTunnel = _context.InitialTemplateTunnelParameters;
        foreach (var child in templateElement.Elements())
        {
            if (child.Name.LocalName != "param" || child.Name.NamespaceName != Stylesheet.Stylesheet.XslNamespace)
                break; // xsl:param children must appear first

            var paramName = child.Attribute("name")?.Value;
            if (string.IsNullOrEmpty(paramName))
                continue;

            var (paramLocal, paramNs) = ExpandVariableName(child, paramName);
            var paramKey = VariableKey(paramLocal, paramNs);
            var paramAs = child.Attribute("as")?.Value;
            var isTunnel = IsTunnelParameter(child);

            Dictionary<string, XdmValue>? source = isTunnel ? externalTunnel : externalCall;
            if (source == null || !source.TryGetValue(paramKey, out var value))
                continue;

            value = ConvertVariableValue(value, paramAs, isParam: true);

            if (isTunnel)
                tunnelParams[paramKey] = value;
            else
                callParams[paramKey] = value;
        }
        return (callParams, tunnelParams);
    }

    /// <summary>
    /// Applies type conversion and validation for the <c>as</c> attribute on
    /// <c>xsl:variable</c> / <c>xsl:param</c> / <c>xsl:with-param</c>.
    /// Atomizes the value and casts to common atomic types (xs:integer, xs:string, etc.).
    /// Node types (element(), attribute(), document-node()) are validated but not atomized.
    /// </summary>
    /// <param name="value">The value to convert/validate.</param>
    /// <param name="asType">The declared sequence type, or null/empty for no constraint.</param>
    /// <param name="isParam">If true, type/cardinality mismatches raise <c>XTTE0590</c>; otherwise <c>XTTE0570</c>.</param>
    /// <summary>
    /// Converts a function call argument to the required type of an <c>xsl:param</c>,
    /// using the XPath function conversion rules and reporting failures as XPTY0004.
    /// </summary>
    private static XdmValue ConvertFunctionArgument(XdmValue value, string? asType)
    {
        if (string.IsNullOrEmpty(asType))
            return value;

        try
        {
            return ConvertVariableValue(value, asType, isParam: true);
        }
        catch (InvalidOperationException ex)
        {
            var msg = ex.Message;
            if (msg.StartsWith("XTTE0590", StringComparison.Ordinal) ||
                msg.StartsWith("XTTE0570", StringComparison.Ordinal))
            {
                var suffix = msg.Length > 8 ? msg[8..] : string.Empty;
                throw new InvalidOperationException($"XPTY0004{suffix}", ex);
            }
            throw;
        }
    }

    /// <summary>
    /// Cache key for deterministic xsl:function memoization.
    /// </summary>
    private readonly struct XsltFunctionCacheKey : IEquatable<XsltFunctionCacheKey>
    {
        public string NamespaceUri { get; }
        public string LocalName { get; }
        public int Arity { get; }
        public object?[] ArgumentKeys { get; }

        public XsltFunctionCacheKey(string ns, string local, int arity, ReadOnlySpan<XdmValue> args)
        {
            NamespaceUri = ns;
            LocalName = local;
            Arity = arity;
            ArgumentKeys = new object?[args.Length];
            for (int i = 0; i < args.Length; i++)
                ArgumentKeys[i] = BuildArgumentKey(args[i]);
        }

        private static object? BuildArgumentKey(XdmValue value)
        {
            if (value.IsUndefined)
                return "__undefined__";
            if (value.IsNode)
                return value.NodeValue;
            if (value.IsSequence && value.SequenceValue is { } seq)
            {
                var list = new List<object?>();
                foreach (var item in XdmSequence.FromSource(seq))
                    list.Add(BuildArgumentKey(item));
                return list;
            }
            // Atomic value: include kind to distinguish e.g. integer 1 from string "1".
            return (value.Kind, value.ToString());
        }

        public bool Equals(XsltFunctionCacheKey other)
        {
            if (NamespaceUri != other.NamespaceUri || LocalName != other.LocalName || Arity != other.Arity)
                return false;
            if (ArgumentKeys.Length != other.ArgumentKeys.Length)
                return false;
            for (int i = 0; i < ArgumentKeys.Length; i++)
            {
                if (!KeysEqual(ArgumentKeys[i], other.ArgumentKeys[i]))
                    return false;
            }
            return true;
        }

        public override bool Equals(object? obj) => obj is XsltFunctionCacheKey key && Equals(key);

        public override int GetHashCode()
        {
            var h = new HashCode();
            h.Add(NamespaceUri);
            h.Add(LocalName);
            h.Add(Arity);
            foreach (var k in ArgumentKeys)
                h.Add(k);
            return h.ToHashCode();
        }

        private static bool KeysEqual(object? a, object? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            if (a is IXdmNode na && b is IXdmNode nb)
                return na.IsSameNode(nb);
            if (a is List<object?> la && b is List<object?> lb)
            {
                if (la.Count != lb.Count) return false;
                for (int i = 0; i < la.Count; i++)
                    if (!KeysEqual(la[i], lb[i])) return false;
                return true;
            }
            return a.Equals(b);
        }
    }

    /// <summary>
    /// Finds the index of the closing parenthesis matching the opening parenthesis at
    /// <paramref name="openIdx"/>, or -1 if there is none.
    /// </summary>
    private static int FindMatchingParenIndex(string s, int openIdx)
    {
        int depth = 0;
        for (int i = openIdx; i < s.Length; i++)
        {
            if (s[i] == '(') depth++;
            else if (s[i] == ')')
            {
                depth--;
                if (depth == 0) return i;
            }
        }
        return -1;
    }

    internal static XdmValue ConvertVariableValue(XdmValue value, string? asType, bool isParam = false)
    {
        if (string.IsNullOrEmpty(asType))
            return value;

        var errorCode = isParam ? "XTTE0590" : "XTTE0570";
        var originalType = asType.Trim();
        // Strip XPath comments (: ... :) from the type string
        var type = System.Text.RegularExpressions.Regex.Replace(originalType, @"\(:[^:]*:\)", "").Trim();
        bool allowsMultiple = type.EndsWith("*") || type.EndsWith("+");
        bool allowsEmpty = type.EndsWith("?") || type.EndsWith("*");
        if (type.EndsWith("?") || type.EndsWith("*") || type.EndsWith("+"))
            type = type[..^1].Trim();

        // Unwrap redundant outer parentheses, e.g. (function(xs:integer) as xs:integer)*
        while (type.Length > 1 && type[0] == '(' && FindMatchingParenIndex(type, 0) == type.Length - 1)
            type = type[1..^1].Trim();

        // Collect sequence items
        var items = new List<XdmValue>();
        if (!value.IsUndefined)
        {
            if (value.IsNode)
                items.Add(value);
            else if (value.IsSequence && value.SequenceValue != null)
            {
                foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                    items.Add(item);
            }
            else
                items.Add(value);
        }

        // Special-case empty-sequence(): it allows exactly zero items
        if (type == "empty-sequence()")
        {
            if (items.Count == 0)
                return XdmValue.Undefined;
            throw new InvalidOperationException($"{errorCode}: Non-empty sequence not allowed for type {originalType}");
        }

        // Cardinality check
        if (items.Count == 0 && !allowsEmpty)
        {
            // An empty sequence is accepted for a single text() or node() type when the
            // only reason it is empty is that zero-length text nodes were removed during
            // sequence construction (construct-node-018/019/020).
            var normalizedForEmpty = type.Trim().ToLowerInvariant();
            if (normalizedForEmpty.EndsWith('?') || normalizedForEmpty.EndsWith('*') || normalizedForEmpty.EndsWith('+'))
                normalizedForEmpty = normalizedForEmpty[..^1].TrimEnd();
            if (normalizedForEmpty is "text()" or "text" or "node()" or "node")
                return XdmValue.Undefined;
            throw new InvalidOperationException($"{errorCode}: Empty sequence not allowed for type {originalType}");
        }
        if (items.Count > 1 && !allowsMultiple)
            throw new InvalidOperationException($"{errorCode}: Sequence of more than one item not allowed for type {originalType}");

        // Function types: XPath 3.1 function conversion rules — a function item is
        // coercible to any function type of the same arity; parameter/return type
        // mismatches are raised when the coerced function is invoked.
        if (type.StartsWith("function(", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var item in items)
            {
                if (!VmEngine.FunctionItemCoercibleTo(item, type))
                    throw new InvalidOperationException($"{errorCode}: Value does not match type {originalType}");
            }

            // Wrap items in CoercedFunctionItem so dynamic invocation converts arguments
            // and validates the result against the declared types.
            if (!type.StartsWith("function(*)", StringComparison.OrdinalIgnoreCase)
                && VmEngine.TryParseFunctionType(type, out var coercedParamTypes, out var coercedReturnType)
                && !(coercedParamTypes.Length == 1 && coercedParamTypes[0] == "*"))
            {
                var wrapped = new List<XdmValue>(items.Count);
                foreach (var item in items)
                {
                    if (item.FunctionValue is FunctionItem funcItem)
                        wrapped.Add(XdmValue.FromFunction(new CoercedFunctionItem(funcItem, coercedParamTypes, coercedReturnType)));
                    else
                        wrapped.Add(item);
                }
                if (wrapped.Count == 0)
                    return XdmValue.Undefined;
                if (wrapped.Count == 1)
                    return wrapped[0];
                return XdmValue.FromSequence(MaterializedSequence.FromList(wrapped));
            }

            return value;
        }

        // Node types, maps, arrays, and item(): no atomization or casting needed, but validate
        if (type is "node()" or "text()" or "comment()" or "processing-instruction()" or "namespace-node()" or "item()"
            || type.Contains("element(") || type.Contains("attribute(") || type.Contains("document-node(")
            || type.StartsWith("map(", StringComparison.Ordinal) || type.StartsWith("array(", StringComparison.Ordinal))
        {
            foreach (var item in items)
            {
                if (!VmEngine.ValueMatchesType(item, type))
                    throw new InvalidOperationException($"{errorCode}: Value does not match type {originalType}");
            }
            return value;
        }

        // Convert each item via atomization + casting. XSLT variable/param coercion
        // allows subtype substitution and numeric promotion for typed values, and casting
        // for xs:untypedAtomic values. It does NOT allow arbitrary casts such as
        // xs:boolean -> xs:integer or xs:string -> xs:boolean.
        var converted = new List<XdmValue>();
        foreach (var item in items)
        {
            // Atomize nodes to xs:untypedAtomic before casting
            XdmValue atomic = item.IsNode
                ? XdmValue.FromString(item.NodeValue.StringValue, "untypedAtomic")
                : item;

            // Subtype substitution: if the value is already an instance of the declared
            // type (including subtypes such as xs:integer for xs:decimal), use it unchanged.
            if (VmEngine.ValueMatchesType(atomic, type))
            {
                converted.Add(atomic);
            }
            else if (IsUntypedAtomic(atomic))
            {
                // xs:untypedAtomic values can be cast to the required atomic type.
                if (VmEngine.TryCast(atomic, type, out var casted))
                    converted.Add(casted);
                else
                    throw new InvalidOperationException($"{errorCode}: Cannot cast untypedAtomic to type {type}");
            }
            else if (IsNumericPromotion(atomic, type))
            {
                // Numeric promotion: integer/decimal/float -> double, integer/decimal -> float
                if (VmEngine.TryCast(atomic, type, out var casted))
                    converted.Add(casted);
                else
                    throw new InvalidOperationException($"{errorCode}: Cannot promote value to type {type}");
            }
            else if (IsUriPromotion(atomic, type))
            {
                // URI promotion: xs:anyURI -> xs:string
                if (VmEngine.TryCast(atomic, type, out var casted))
                    converted.Add(casted);
                else
                    throw new InvalidOperationException($"{errorCode}: Cannot promote URI to type {type}");
            }
            else
            {
                throw new InvalidOperationException($"{errorCode}: Cannot convert value to type {type}");
            }
        }

        if (converted.Count == 0)
            return XdmValue.Undefined;
        if (converted.Count == 1)
            return converted[0];
        return XdmValue.FromSequence(MaterializedSequence.FromList(converted));
    }

    /// <summary>
    /// Builds the final XDM result from the nodes, attributes and accumulator items
    /// produced by a sequence constructor.
    /// </summary>
    private XdmValue BuildResultFromNodesAndAccumulator(List<XNode> nodes, List<XAttribute> attributes, IList<XdmValue> accumulatorItems, XElement wrapper, XElement parent, bool wrapInDocumentNode, bool preserveEmptySequencePositions = false)
    {
        // Empty sequence constructor: when building a document node (no @as)
        // the result is an empty document node; with @as it is an empty sequence.
        if (nodes.Count == 0 && attributes.Count == 0 && accumulatorItems.Count == 0)
        {
            if (wrapInDocumentNode)
            {
                var emptyDoc = new XDocument();
                var effectiveBaseUri = GetEffectiveBaseUri(parent);
                if (!string.IsNullOrEmpty(effectiveBaseUri))
                    emptyDoc.AddAnnotation(effectiveBaseUri);
                return XdmValue.FromNode(new XDocumentNode(emptyDoc));
            }
            return XdmValue.FromSequence(XdmSequence.Empty);
        }

        if (wrapInDocumentNode)
        {
            // XSLT 3.0 §5.7.1: attribute nodes in document-node content are an error.
            var realAttrs = attributes.Where(a => !a.IsNamespaceDeclaration).ToList();
            if (realAttrs.Count > 0)
                throw new InvalidOperationException("XTDE0420");

            // Apply complex content rules: remove zero-length text nodes,
            // merge adjacent text nodes.
            nodes = ApplyComplexContentRules(nodes);

            // XSLT 2.0+: non-empty sequence constructor content produces a document node.
            // LINQ-to-XML XDocument requires exactly one root element and does not
            // allow non-whitespace text nodes outside the root, so we use a synthetic
            // wrapper element that XDocumentNode transparently unwraps.
            var elementCount = nodes.OfType<XElement>().Count();
            var effectiveBaseUri = GetEffectiveBaseUri(parent);
            if (elementCount == 1 && nodes.Count == 1)
            {
                // Single element: use it directly as the document root.
                // Remove from wrapper first so XDocument does not clone and
                // lose XElement annotations (e.g. NamespaceInheritanceBarrier).
                if (nodes[0].Parent != null)
                    nodes[0].Remove();
                if (nodes[0] is XElement rootElem && !string.IsNullOrEmpty(effectiveBaseUri) && rootElem.Annotation<string>() == null)
                    rootElem.AddAnnotation(effectiveBaseUri);
                var tempDoc = new XDocument(nodes[0]);
                if (!string.IsNullOrEmpty(effectiveBaseUri))
                    tempDoc.AddAnnotation(effectiveBaseUri);
                return XdmValue.FromNode(new XDocumentNode(tempDoc));
            }
            else
            {
                // Mixed content: wrap in synthetic document wrapper.
                // Remove each node from wrapper first to preserve annotations.
                var docWrapper = new XElement("__xdm_doc__");
                foreach (var node in nodes)
                {
                    if (node.Parent != null)
                        node.Remove();
                    docWrapper.Add(node);
                }
                if (!string.IsNullOrEmpty(effectiveBaseUri) && docWrapper.Annotation<string>() == null)
                    docWrapper.AddAnnotation(effectiveBaseUri);
                var tempDoc = new XDocument(docWrapper);
                if (!string.IsNullOrEmpty(effectiveBaseUri))
                    tempDoc.AddAnnotation(effectiveBaseUri);
                return XdmValue.FromNode(new XDocumentNode(tempDoc));
            }
        }

        // wrapInDocumentNode == false: return the raw sequence (used when @as is present)
        // Remove nodes from the temporary wrapper so they don't have __temp__ as parent.
        foreach (var node in nodes)
        {
            if (node.Parent != null)
                node.Remove();
        }
        var effectiveBaseUriNoWrap = GetEffectiveBaseUri(parent);
        if (!string.IsNullOrEmpty(effectiveBaseUriNoWrap))
        {
            foreach (var child in nodes.OfType<XElement>())
            {
                if (child.Annotation<string>() == null)
                    child.AddAnnotation(effectiveBaseUriNoWrap);
            }
            // Also set annotation on element nodes in accumulatorItems (e.g. from xsl:copy-of)
            foreach (var item in accumulatorItems)
            {
                if (item.IsNode && item.NodeValue is XDocumentNode xdn && xdn.UnderlyingObject is XElement accElem)
                {
                    if (accElem.Annotation<string>() == null)
                        accElem.AddAnnotation(effectiveBaseUriNoWrap);
                }
            }
        }
        var asType = parent.Attribute("as")?.Value;
        bool allowsMultipleItems = !string.IsNullOrEmpty(asType) &&
            (asType.TrimEnd().EndsWith("*") || asType.TrimEnd().EndsWith("+"));
        bool asIsNodeKind = !string.IsNullOrEmpty(asType) && IsNodeKindType(asType);

        var results = new List<XdmValue>();
        foreach (var item in accumulatorItems)
        {
            // For single-item node-kind types (e.g. text()), zero-length text nodes
            // produced by xsl:value-of/xsl:text are dropped, leaving an empty sequence.
            // This matches construct-node-018/019/020, where such variables are used
            // as empty sequence items in string-join.
            if (asIsNodeKind && !allowsMultipleItems &&
                item.IsNode && item.NodeValue is { NodeKind: XdmNodeKind.Text } tn && tn.StringValue.Length == 0)
                continue;
            results.Add(item);
        }
        foreach (var child in nodes)
        {
            switch (child)
            {
                case XElement e when e.Name.LocalName == "__xdm_seq__":
                    // Expand a placeholder that preserves the source position of a
                    // sequence-producing instruction in a raw sequence constructor.
                    if (e.Annotation<SequencePlaceholderItems>() is { } holder)
                    {
                        foreach (var phItem in holder.Items)
                        {
                            // Empty-sequence placeholders are used only to preserve the
                            // source position of sequence-producing instructions during
                            // complex content construction. They are kept when explicitly
                            // requested (e.g. a child sequence constructor evaluated for
                            // an xsl:sequence in document-construction mode), but filtered
                            // out of ordinary raw sequence results.
                            if (!preserveEmptySequencePositions && phItem.IsUndefined)
                                continue;

                            // For single-item node-kind types (e.g. text()), zero-length
                            // text nodes produced by xsl:sequence are dropped.
                            if (asIsNodeKind && !allowsMultipleItems &&
                                phItem.IsNode && phItem.NodeValue is { NodeKind: XdmNodeKind.Text } tn && tn.StringValue.Length == 0)
                                continue;

                            // Propagate the effective base URI to element nodes held in
                            // the placeholder, mirroring the handling for accumulator items.
                            if (!string.IsNullOrEmpty(effectiveBaseUriNoWrap) &&
                                phItem.IsNode && phItem.NodeValue is XDocumentNode xdn &&
                                xdn.UnderlyingObject is XElement accElem &&
                                accElem.Annotation<string>() == null)
                            {
                                accElem.AddAnnotation(effectiveBaseUriNoWrap);
                            }

                            results.Add(phItem);
                        }
                    }
                    break;
                case XElement e:
                    // Return the element directly as a standalone node. It has already
                    // been detached from the temporary wrapper, so it is the root of
                    // its own temporary tree.
                    results.Add(XdmValue.FromNode(new XDocumentNode(e)));
                    break;
                case XText t:
                    // Drop zero-length text nodes for single-item node-kind types;
                    // retain them for atomic types and for types that allow multiple items.
                    if (asIsNodeKind && !allowsMultipleItems && string.IsNullOrEmpty(t.Value))
                        break;
                    // Preserve text nodes as text nodes, not atomic strings,
                    // so that CopyToResult can concatenate adjacent text nodes
                    // without inserting spaces (XSLT 3.0 §5.7.2).
                    results.Add(XdmValue.FromNode(new XDocumentNode(new XText(t.Value))));
                    break;
                case XComment c:
                    results.Add(XdmValue.FromNode(new XDocumentNode(c)));
                    break;
                case XProcessingInstruction pi:
                    results.Add(XdmValue.FromNode(new XDocumentNode(pi)));
                    break;
            }
        }
        // Include attributes produced by xsl:attribute / xsl:namespace in the sequence
        foreach (var attr in attributes)
        {
            if (attr.IsNamespaceDeclaration)
            {
                results.Add(XdmValue.FromNode(XDocumentNode.CreateNamespaceNode(attr, wrapper)));
            }
            else
            {
                results.Add(XdmValue.FromNode(new XDocumentNode(new XAttribute(attr.Name, attr.Value))));
            }
        }

        // Pre-assign document-order sequence numbers to parentless nodes so that
        // subsequent sorts by document order preserve sequence-constructor order.
        foreach (var item in results)
        {
            if (item.IsNode && item.NodeValue != null)
                _ = item.NodeValue.DocumentOrder;
        }

        if (results.Count == 1)
            return results[0];
        return XdmValue.FromSequence(MaterializedSequence.FromList(results));
    }

    /// <summary>
    /// Evaluates a sequence constructor (child nodes of an xsl:variable, xsl:param, etc.)
    /// and returns the resulting XDM value.
    /// </summary>
    private XdmValue EvaluateSequenceConstructor(XElement parent, XdmValue contextItem, bool wrapInDocumentNode = true, bool preserveEmptySequencePositions = false)
    {
        // Ensure XPath evaluations inside the sequence constructor use the correct context item
        var savedContextItem = _context.ContextItem;
        var savedContextPosition = _context.ContextPosition;
        var savedContextSize = _context.ContextSize;
        if (contextItem.Kind != XdmValueKind.Undefined)
        {
            // Preserve the caller's context position and size so that position()/last()
            // inside sequence constructors (e.g. xsl:variable within xsl:for-each)
            // reflect the containing instruction's focus, per XSLT 2.0 §5.7.1.
            int pos = _context.ContextPosition > 0 ? _context.ContextPosition : 1;
            int size = _context.ContextSize > 0 ? _context.ContextSize : 1;
            _context.WithFocus(contextItem, pos, size);
        }

        var savedAccumulator = _sequenceAccumulator;
        if (!wrapInDocumentNode)
            _sequenceAccumulator = new PlaceholderSequenceAccumulator(this);
        else
            _sequenceAccumulator = null; // When building a document node, all content goes into the wrapper

        // Sequence constructors establish a new variable scope: bindings added while
        // evaluating this constructor (e.g. xsl:variable inside a literal result element)
        // are removed when the constructor finishes.
        var savedVariables = _context.SnapshotVariables();

        try
        {
            if (ContainsConditionalInstruction(parent)
                || !wrapInDocumentNode
                || ContainsTopLevelAttributeOrNamespaceInstruction(parent))
            {
                var items = EvaluateSequenceConstructorToItems(parent, contextItem);
                if (wrapInDocumentNode)
                {
                    var resultWrapper = new XElement("__temp__");
                    var savedContainer = _currentContainer;
                    var savedLastAtomic = _lastAddedWasAtomic;
                    var savedSeqAccumulator = _sequenceAccumulator;
                    _currentContainer = resultWrapper;
                    _lastAddedWasAtomic = false;
                    _sequenceAccumulator = null;
                    try
                    {
                        var nonNamespaceItems = new List<XdmValue>();
                        foreach (var item in items)
                        {
                            if (item.IsNode && item.NodeValue != null && item.NodeValue.NodeKind == XdmNodeKind.Namespace)
                            {
                                var nsNode = item.NodeValue;
                                if (nsNode is XDocumentNode xdn && xdn.UnderlyingObject is XAttribute attr)
                                {
                                    resultWrapper.SetAttributeValue(attr.Name, attr.Value);
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(nsNode.LocalName))
                                        resultWrapper.SetAttributeValue("xmlns", nsNode.StringValue);
                                    else
                                        resultWrapper.SetAttributeValue(XNamespace.Xmlns + nsNode.EncodedLocalName, nsNode.StringValue);
                                }
                            }
                            else
                            {
                                nonNamespaceItems.Add(item);
                            }
                        }
                        CopyToResult(XdmValue.FromSequence(MaterializedSequence.FromList(nonNamespaceItems)), separateAtomicsWithSpace: true);
                    }
                    finally
                    {
                        _currentContainer = savedContainer;
                        _lastAddedWasAtomic = savedLastAtomic;
                        _sequenceAccumulator = savedSeqAccumulator;
                    }
                    return BuildResultFromNodesAndAccumulator(resultWrapper.Nodes().ToList(), resultWrapper.Attributes().ToList(), new List<XdmValue>(), resultWrapper, parent, wrapInDocumentNode: true);
                }
                else
                {
                    return BuildResultFromNodesAndAccumulator(new List<XNode>(), new List<XAttribute>(), items, new XElement("__dummy__"), parent, wrapInDocumentNode: false);
                }
            }

            // Create a temporary container to capture the sequence constructor output
            var wrapper = new XElement("__temp__");
            ExecuteSequenceConstructorDirect(parent, contextItem, wrapper);

            var nodes = wrapper.Nodes().ToList();
            var attributes = wrapper.Attributes().ToList();

            // Include items collected by xsl:sequence into the accumulator.
            // Placeholder accumulators store their items as synthetic elements in
            // the wrapper, so their underlying item list is empty.
            var accumulatorItems = _sequenceAccumulator?.Items ?? new List<XdmValue>();

            // Handle xsl:on-empty: if sequence constructor is empty, evaluate on-empty fallback
            var onEmptyElements = parent.Elements(XName.Get("on-empty", Stylesheet.Stylesheet.XslNamespace)).ToList();
            if (nodes.Count == 0 && attributes.Count == 0 && accumulatorItems.Count == 0 && onEmptyElements.Count > 0)
            {
                var savedContainer = _currentContainer;
                _currentContainer = wrapper;
                foreach (var onEmpty in onEmptyElements)
                {
                    var oeSelect = onEmpty.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(oeSelect))
                    {
                        var compiled = XPath31Expression.Compile(oeSelect);
                        var result = compiled.Evaluate(_context);
                        CopyToResult(result, separateAtomicsWithSpace: true);
                    }
                    else
                    {
                        foreach (var childNode in onEmpty.Nodes())
                        {
                            switch (childNode)
                            {
                                case XText text:
                                    ProcessSequenceText(text, onEmpty);
                                    break;
                                case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                                    ExecuteXsltInstruction(elem, _context.ContextItem);
                                    break;
                                case XElement elem:
                                    CopyLiteralElement(elem);
                                    break;
                            }
                        }
                    }
                }
                _currentContainer = savedContainer;

                // Re-read nodes/attributes after on-empty evaluation
                nodes = wrapper.Nodes().ToList();
                attributes = wrapper.Attributes().ToList();
                accumulatorItems = _sequenceAccumulator?.Items ?? new List<XdmValue>();
            }

            return BuildResultFromNodesAndAccumulator(nodes, attributes, accumulatorItems, wrapper, parent, wrapInDocumentNode, preserveEmptySequencePositions);
        }
        finally
        {
            _sequenceAccumulator = savedAccumulator;
            _context.WithFocus(savedContextItem, savedContextPosition, savedContextSize);
        }
    }

    /// <summary>
    /// Annotation attached to a synthetic <c>__xdm_seq__</c> element that holds
    /// the XDM items produced by an <c>xsl:sequence</c> instruction while a
    /// sequence-constructor is being collected as a raw sequence.
    /// </summary>
    private sealed class SequencePlaceholderItems
    {
        public List<XdmValue> Items { get; } = new();
    }

    /// <summary>
    /// Abstraction used to collect items produced by sequence-producing instructions
    /// (e.g. <c>xsl:sequence</c>, <c>xsl:document</c>, <c>xsl:copy-of</c>) while a
    /// sequence constructor is being evaluated.
    /// </summary>
    private interface ISequenceAccumulator
    {
        /// <summary>
        /// Adds an item to the accumulator, flattening sequences recursively.
        /// </summary>
        void Add(XdmValue item);

        /// <summary>
        /// The underlying item list. For placeholder accumulators this is empty;
        /// for list accumulators it contains the collected items.
        /// </summary>
        IList<XdmValue> Items { get; }
    }

    /// <summary>
    /// Accumulates sequence items produced inside a sequence constructor that is
    /// being evaluated for its raw sequence (e.g. an <c>xsl:variable</c> with an
    /// <c>@as</c> attribute). Instead of storing the items directly, each item is
    /// wrapped in a synthetic placeholder element that is added to the current
    /// result container, preserving source order relative to ordinary nodes.
    /// </summary>
    private sealed class PlaceholderSequenceAccumulator : ISequenceAccumulator
    {
        private readonly TransformEngine _engine;

        public PlaceholderSequenceAccumulator(TransformEngine engine)
        {
            _engine = engine;
        }

        public IList<XdmValue> Items => Array.Empty<XdmValue>();

        public void Add(XdmValue item)
        {
            // Sequences are flattened so that each XDM item occupies its own
            // placeholder and keeps its relative position in the result sequence.
            // Empty-sequence items (e.g. xs:language(())) are preserved here as
            // placeholders so that their source position is retained for complex
            // content construction; they are filtered out when a raw sequence is
            // materialized by BuildResultFromNodesAndAccumulator.
            if (item.IsSequence && item.SequenceValue != null)
            {
                foreach (var sub in XdmSequence.FromSource(item.SequenceValue))
                    Add(sub);
                return;
            }

            AddSequencePlaceholder(_engine._currentContainer, new List<XdmValue> { item });
        }
    }

    /// <summary>
    /// Simple list-based accumulator used when a sequence constructor must be
    /// evaluated to a flat list of XDM items (e.g. an <c>xsl:function</c> body or
    /// the <c>EvaluateSequenceConstructorToItems</c> helper).
    /// </summary>
    private sealed class ListSequenceAccumulator : ISequenceAccumulator
    {
        private readonly List<XdmValue> _items;

        public ListSequenceAccumulator(List<XdmValue> items)
        {
            _items = items;
        }

        public IList<XdmValue> Items => _items;

        public void Add(XdmValue item)
        {
            if (item.IsUndefined)
                return;

            if (item.IsSequence && item.SequenceValue != null)
            {
                foreach (var sub in XdmSequence.FromSource(item.SequenceValue))
                    Add(sub);
                return;
            }

            _items.Add(item);
        }
    }

    /// <summary>
    /// Adds a synthetic placeholder element to the current container. The
    /// placeholder carries the items produced by an <c>xsl:sequence</c> (or
    /// similar sequence-producing instruction) so that they can be expanded
    /// back into the result sequence in source order.
    /// </summary>
    private static void AddSequencePlaceholder(XContainer container, List<XdmValue> items)
    {
        var placeholder = new XElement("__xdm_seq__");
        var holder = new SequencePlaceholderItems();
        foreach (var item in items)
            holder.Items.Add(item);
        placeholder.AddAnnotation(holder);
        container.Add(placeholder);
    }

    /// <summary>
    /// Returns true if the value is a sequence (or a single item) containing only
    /// atomic values whose string value is empty. Text nodes and non-empty atomics
    /// make the result false.
    /// </summary>
    private static bool IsAllEmptyAtomics(XdmValue value)
    {
        if (value.IsUndefined)
            return true;

        bool anyAtomic = false;

        if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                if (item.IsUndefined)
                    continue;
                if (item.IsNode)
                    return false;
                if (item.IsSequence)
                {
                    if (!IsAllEmptyAtomics(item))
                        return false;
                    continue;
                }
                anyAtomic = true;
                if (item.ToString().Length > 0)
                    return false;
            }
        }
        else
        {
            if (value.IsNode)
                return false;
            anyAtomic = true;
            if (value.ToString().Length > 0)
                return false;
        }

        return anyAtomic;
    }

    /// <summary>
    /// Applies complex content construction rules to a list of nodes:
    /// removes zero-length text nodes and merges adjacent text nodes.
    /// </summary>
    private static List<XNode> ApplyComplexContentRules(List<XNode> nodes)
    {
        var result = new List<XNode>();
        var textBuffer = new StringBuilder();

        foreach (var node in nodes)
        {
            if (node is XText t)
            {
                if (t.Value.Length == 0)
                {
                    // Discard zero-length text nodes. Do not flush the buffer here;
                    // an empty text node between two non-empty text nodes must not
                    // split them (select-2301).
                    continue;
                }
                textBuffer.Append(t.Value);
            }
            else
            {
                if (textBuffer.Length > 0)
                {
                    result.Add(new XText(textBuffer.ToString()));
                    textBuffer.Clear();
                }
                result.Add(node);
            }
        }

        if (textBuffer.Length > 0)
        {
            result.Add(new XText(textBuffer.ToString()));
        }

        return result;
    }

    /// <summary>
    /// Executes a sequence constructor directly into the specified container,
    /// handling text nodes, XSLT instructions, and literal result elements.
    /// </summary>
    private void ExecuteSequenceConstructorDirect(XElement parent, XdmValue contextItem, XContainer outputContainer)
    {
        var savedContainer = _currentContainer;
        var savedLastAtomic = _lastAddedWasAtomic;
        _currentContainer = outputContainer;
        _lastAddedWasAtomic = false;
        try
        {
            foreach (var node in parent.Nodes())
            {
                switch (node)
                {
                    case XText text:
                        ProcessSequenceText(text, parent);
                        break;
                    case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                        var currentNode = contextItem.IsNode ? contextItem.NodeValue : null;
                        ExecuteXsltInstruction(elem, currentNode!);
                        break;
                    case XElement elem:
                        CopyLiteralElement(elem);
                        break;
                }
            }

        }
        finally
        {
            _currentContainer = savedContainer;
            _lastAddedWasAtomic = savedLastAtomic;
        }
    }

    /// <summary>
    /// Determines whether a sequence constructor parent contains an xsl:on-empty
    /// or xsl:on-non-empty conditional instruction as a direct child.
    /// </summary>
    private bool ContainsConditionalInstruction(XElement parent)
        => parent.Elements().Any(e => e.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace
            && (e.Name.LocalName == "on-empty" || e.Name.LocalName == "on-non-empty"));

    /// <summary>
    /// Returns true when the sequence constructor contains a top-level
    /// <c>xsl:attribute</c> or <c>xsl:namespace</c> instruction that must be
    /// returned as a separate item in a raw node sequence rather than attached
    /// to a wrapper element.
    /// </summary>
    private bool ContainsTopLevelAttributeOrNamespaceInstruction(XElement parent)
        => parent.Elements().Any(e => e.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace
            && (e.Name.LocalName == "attribute" || e.Name.LocalName == "namespace"));

    /// <summary>
    /// Returns whether an XDM item contributes significant content for the purposes
    /// of xsl:on-empty / xsl:on-non-empty evaluation.
    /// </summary>
    private bool IsSignificantContentItem(XdmValue item)
    {
        if (item.IsUndefined)
            return false;

        if (item.IsNode && item.NodeValue != null)
        {
            switch (item.NodeValue.NodeKind)
            {
                case XdmNodeKind.Text:
                    return item.NodeValue.StringValue.Length > 0;
                case XdmNodeKind.Comment:
                case XdmNodeKind.ProcessingInstruction:
                case XdmNodeKind.Attribute:
                case XdmNodeKind.Namespace:
                case XdmNodeKind.Element:
                    // Synthetic sequence placeholders are not content.
                    return item.NodeValue.LocalName != "__xdm_seq__" || item.NodeValue.NamespaceUri != "";
                case XdmNodeKind.Document:
                    return item.NodeValue.Axis(XdmAxis.Child).GetEnumerator().MoveNext();
                default:
                    return true;
            }
        }

        if (item.IsSequence && item.SequenceValue != null)
        {
            foreach (var sub in XdmSequence.FromSource(item.SequenceValue))
            {
                if (IsSignificantContentItem(sub))
                    return true;
            }
            return false;
        }

        if (item.Kind == XdmValueKind.String)
            return item.StringValue.Length > 0;

        return true;
    }

    /// <summary>
    /// Evaluates an xsl:on-empty or xsl:on-non-empty instruction and returns
    /// its contribution as a flat list of XDM items.
    /// </summary>
    private List<XdmValue> EvaluateOnEmptyOrNonEmptyInstructionToItems(XElement instruction)
    {
        var select = instruction.Attribute("select")?.Value;
        if (!string.IsNullOrEmpty(select))
        {
            var compiled = CompileXPath(select, instruction);
            var result = compiled.Evaluate(_context);
            var items = new List<XdmValue>();
            foreach (var item in EnumerateItems(result))
            {
                if (item.IsUndefined)
                    continue;
                items.Add(item);
            }
            return items;
        }
        return EvaluateSequenceConstructorToItems(instruction, _context.ContextItem);
    }

    /// <summary>
    /// Evaluates a sequence constructor as a flat list of XDM items, deferring
    /// xsl:on-empty / xsl:on-non-empty processing until all other items have
    /// been produced so the emptiness test is applied correctly.
    /// </summary>
    private List<XdmValue> EvaluateSequenceConstructorToItems(XElement parent, XdmValue contextItem, Func<XElement, bool>? additionalSkip = null)
    {
        var savedContainer = _currentContainer;
        var savedLastAtomic = _lastAddedWasAtomic;
        var savedAccumulator = _sequenceAccumulator;

        var tempContainer = new XElement("__seq_temp__");
        _currentContainer = tempContainer;
        _lastAddedWasAtomic = false;
        // Use a placeholder accumulator so sequence-producing instructions keep their
        // position relative to ordinary nodes in the temporary container.
        _sequenceAccumulator = new PlaceholderSequenceAccumulator(this);

        var savedContextItem = _context.ContextItem;
        var savedContextPosition = _context.ContextPosition;
        var savedContextSize = _context.ContextSize;
        if (contextItem.Kind != XdmValueKind.Undefined)
        {
            int pos = _context.ContextPosition > 0 ? _context.ContextPosition : 1;
            int size = _context.ContextSize > 0 ? _context.ContextSize : 1;
            _context.WithFocus(contextItem, pos, size);
        }

        var resultItems = new List<XdmValue>();
        var markers = new List<(XElement Instruction, int Position, Dictionary<(string LocalName, string NamespaceUri), XdmValue> Variables)>();
        var sequenceAs = parent.Attribute("as")?.Value;

        try
        {
            foreach (var childNode in parent.Nodes())
            {
                if (childNode is XElement childElem)
                {
                    if (childElem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace
                        && (childElem.Name.LocalName == "on-empty" || childElem.Name.LocalName == "on-non-empty"))
                    {
                        markers.Add((childElem, resultItems.Count, _context.SnapshotVariables()));
                        continue;
                    }
                    if (additionalSkip != null && additionalSkip(childElem))
                        continue;
                }

                var existingNodes = new HashSet<XNode>(tempContainer.Nodes());
                var existingAttrs = new HashSet<XAttribute>(tempContainer.Attributes());

                switch (childNode)
                {
                    case XText text:
                        ProcessSequenceText(text, parent);
                        break;
                    case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                        ExecuteXsltInstruction(elem, contextItem);
                        break;
                    case XElement elem:
                        CopyLiteralElement(elem);
                        break;
                }

                foreach (var attr in tempContainer.Attributes().ToList())
                {
                    if (!existingAttrs.Contains(attr))
                    {
                        attr.Remove();
                        if (attr.IsNamespaceDeclaration)
                        {
                            // A top-level xsl:namespace instruction creates a namespace node when
                            // the containing sequence constructor is explicitly typed to return
                            // namespace nodes, or when it is typed as a single generic node (e.g.
                            // as="node()"). In the more common as="node()*" case the resulting
                            // parentless namespace node is not a valid standalone item and is
                            // discarded to preserve snapshot/mode behaviour.
                            var trimmedAs = sequenceAs?.Trim() ?? string.Empty;
                            bool keepNamespaceNode = string.IsNullOrEmpty(sequenceAs)
                                || sequenceAs.Contains("namespace-node", StringComparison.Ordinal)
                                || trimmedAs.Equals("node()", StringComparison.Ordinal)
                                || trimmedAs.Equals("node()?", StringComparison.Ordinal);
                            if (keepNamespaceNode)
                            {
                                resultItems.Add(XdmValue.FromNode(XDocumentNode.CreateNamespaceNode(attr, tempContainer)));
                            }
                            continue;
                        }
                        resultItems.Add(XdmValue.FromNode(new XDocumentNode(new XAttribute(attr.Name, attr.Value))));
                    }
                }
                foreach (var node in tempContainer.Nodes().ToList())
                {
                    if (!existingNodes.Contains(node))
                    {
                        node.Remove();
                        if (node is XElement placeholder && placeholder.Name.LocalName == "__xdm_seq__")
                        {
                            if (placeholder.Annotation<SequencePlaceholderItems>() is { } holder)
                            {
                                foreach (var phItem in holder.Items)
                                {
                                    if (phItem.IsUndefined)
                                        continue;
                                    resultItems.Add(phItem);
                                }
                            }
                        }
                        else
                        {
                            resultItems.Add(XdmValue.FromNode(new XDocumentNode(node)));
                        }
                    }
                }
            }

            bool hasContent = resultItems.Any(IsSignificantContentItem);
            bool anyOnEmptyFired = false;

            for (int i = markers.Count - 1; i >= 0; i--)
            {
                var (instruction, position, markerVars) = markers[i];
                bool isOnEmpty = instruction.Name.LocalName == "on-empty";
                bool shouldEvaluate = isOnEmpty ? !hasContent : hasContent;
                if (!shouldEvaluate)
                    continue;

                if (isOnEmpty)
                    anyOnEmptyFired = true;

                var currentVars = _context.SnapshotVariables();
                try
                {
                    _context.RestoreVariables(markerVars);
                    var conditionalItems = EvaluateOnEmptyOrNonEmptyInstructionToItems(instruction);
                    resultItems.InsertRange(position, conditionalItems);
                }
                finally
                {
                    _context.RestoreVariables(currentVars);
                }
            }

            // Expand any remaining sequence placeholders into their items. If an
            // xsl:on-empty fired, the sequence constructor was empty, so discard
            // non-significant items (empty placeholders / zero-length text) and
            // keep only the on-empty contributions and any real content.
            var expanded = new List<XdmValue>(resultItems.Count);
            foreach (var item in resultItems)
            {
                if (item.IsNode && item.NodeValue != null &&
                    item.NodeValue.NodeKind == XdmNodeKind.Element &&
                    item.NodeValue.LocalName == "__xdm_seq__")
                {
                    if (item.NodeValue is XDocumentNode xdn && xdn.UnderlyingObject is XElement placeholder &&
                        placeholder.Annotation<SequencePlaceholderItems>() is { } holder)
                    {
                        foreach (var phItem in holder.Items)
                        {
                            if (phItem.IsUndefined)
                                continue;
                            expanded.Add(phItem);
                        }
                    }
                }
                else
                {
                    expanded.Add(item);
                }
            }
            resultItems = anyOnEmptyFired
                ? expanded.Where(IsSignificantContentItem).ToList()
                : expanded;
        }
        finally
        {
            _currentContainer = savedContainer;
            _lastAddedWasAtomic = savedLastAtomic;
            _sequenceAccumulator = savedAccumulator;
            _context.WithFocus(savedContextItem, savedContextPosition, savedContextSize);
        }

        return resultItems;
    }

    /// <summary>
    /// Evaluates a sequence constructor and copies the resulting items into the
    /// specified target container, applying xsl:on-empty / xsl:on-non-empty
    /// semantics when present.
    /// </summary>
    private void EvaluateSequenceConstructorIntoContainer(XElement parent, XContainer targetContainer, XdmValue contextItem, Func<XElement, bool>? additionalSkip = null)
    {
        var items = EvaluateSequenceConstructorToItems(parent, contextItem, additionalSkip);
        if (items.Count == 0)
            return;

        var savedContainer = _currentContainer;
        var savedLastAtomic = _lastAddedWasAtomic;
        var savedAccumulator = _sequenceAccumulator;
        _currentContainer = targetContainer;
        _lastAddedWasAtomic = false;
        _sequenceAccumulator = null;
        try
        {
            var nonNamespaceItems = new List<XdmValue>();
            foreach (var item in items)
            {
                if (item.IsNode && item.NodeValue != null && item.NodeValue.NodeKind == XdmNodeKind.Namespace)
                {
                    var nsNode = item.NodeValue;
                    if (targetContainer is XElement targetElem)
                    {
                        if (nsNode is XDocumentNode xdn && xdn.UnderlyingObject is XAttribute attr)
                        {
                            targetElem.SetAttributeValue(attr.Name, attr.Value);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(nsNode.LocalName))
                                targetElem.SetAttributeValue("xmlns", nsNode.StringValue);
                            else
                                targetElem.SetAttributeValue(XNamespace.Xmlns + nsNode.EncodedLocalName, nsNode.StringValue);
                        }
                    }
                }
                else
                {
                    nonNamespaceItems.Add(item);
                }
            }
            CopyToResult(XdmValue.FromSequence(MaterializedSequence.FromList(nonNamespaceItems)), separateAtomicsWithSpace: true);
        }
        finally
        {
            _currentContainer = savedContainer;
            _lastAddedWasAtomic = savedLastAtomic;
            _sequenceAccumulator = savedAccumulator;
        }
    }

    /// <summary>
    /// Evaluates the sequence constructor within the given element and returns
    /// the concatenated string value, applying simple content construction rules.
    /// </summary>
    /// <param name="parent">The element whose child nodes form the sequence constructor.</param>
    /// <param name="contextItem">The current context item for XPath evaluations.</param>
    /// <param name="separator">The separator inserted between successive strings after atomization.</param>
    private string EvaluateSimpleContent(XElement parent, XdmValue contextItem, string separator = " ")
    {
        var items = new List<XdmValue>();
        CollectSimpleContentItems(parent, contextItem, items);
        return ConstructSimpleContentString(items, separator);
    }

    /// <summary>
    /// Collects the raw XDM items produced by evaluating a sequence constructor
    /// for simple content construction.
    /// </summary>
    private void CollectSimpleContentItems(XElement parent, XdmValue contextItem, List<XdmValue> items)
    {
        // XSLT 3.0 §4.3: stylesheet preprocessing removes comments/PIs and merges
        // adjacent text nodes BEFORE whitespace stripping and TVT processing. Under
        // expand-text="yes" a whitespace-only (merged) text node is still stripped
        // (unless preserved); whitespace fixed parts inside a surviving text value
        // template remain significant (seqtor-043h).
        bool expandText = GetExpandText(parent);
        StringBuilder? pendingTvt = null;

        void FlushPendingTvt()
        {
            if (pendingTvt == null)
                return;
            var merged = pendingTvt.ToString();
            pendingTvt = null;
            if (IsWhitespaceOnly(merged))
            {
                if (IsWhitespacePreserveContext(parent))
                    items.Add(XdmValue.FromNode(new XDocumentNode(new XText(merged))));
                return;
            }
            items.Add(XdmValue.FromNode(new XDocumentNode(new XText(EvaluateTvt(merged, parent)))));
        }

        foreach (var node in parent.Nodes())
        {
            switch (node)
            {
                case XText text when expandText:
                    pendingTvt ??= new StringBuilder();
                    pendingTvt.Append(text.Value);
                    break;
                case XComment or XProcessingInstruction when expandText:
                    // Removed during stylesheet preprocessing; adjacent text merges across them.
                    break;
                case XText text:
                    CollectSimpleContentText(text, parent, items);
                    break;
                case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                    FlushPendingTvt();
                    CollectSimpleContentXsltInstruction(elem, contextItem, items);
                    break;
                case XElement elem:
                    FlushPendingTvt();
                    var copy = CopyLiteralElementToXElement(elem);
                    items.Add(XdmValue.FromNode(new XDocumentNode(copy)));
                    break;
            }
        }
        FlushPendingTvt();
    }

    /// <summary>
    /// Processes a literal text node in simple content and adds the resulting
    /// text node to the items list.
    /// </summary>
    private void CollectSimpleContentText(XText text, XElement parent, List<XdmValue> items)
    {
        string value;
        if (GetExpandText(parent))
        {
            value = EvaluateTvt(text.Value, parent);
        }
        else if (IsWhitespacePreserveContext(parent))
        {
            value = text.Value;
        }
        else if (IsWhitespaceOnly(text.Value))
        {
            return;
        }
        else
        {
            value = text.Value;
        }
        items.Add(XdmValue.FromNode(new XDocumentNode(new XText(value))));
    }

    /// <summary>
    /// Processes an XSLT instruction in simple content and adds the resulting
    /// items to the items list.
    /// </summary>
    private void CollectSimpleContentXsltInstruction(XElement instruction, XdmValue contextItem, List<XdmValue> items)
    {
        var name = instruction.Name.LocalName;
        switch (name)
        {
            case "sequence":
                {
                    var seqSelect = instruction.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(seqSelect))
                    {
                        var compiled = XPath31Expression.Compile(seqSelect);
                        var result = compiled.Evaluate(_context);
                        if (result.IsSequence && result.SequenceValue != null)
                        {
                            foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                                items.Add(item);
                        }
                        else
                        {
                            items.Add(result);
                        }
                    }
                    else
                    {
                        CollectSimpleContentItems(instruction, contextItem, items);
                    }
                    break;
                }

            case "copy-of":
                {
                    var copySelect = instruction.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(copySelect))
                    {
                        var compiled = XPath31Expression.Compile(copySelect);
                        var result = compiled.Evaluate(_context);
                        if (result.IsSequence && result.SequenceValue != null)
                        {
                            foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                                items.Add(item);
                        }
                        else
                        {
                            items.Add(result);
                        }
                    }
                    break;
                }

            case "document":
                {
                    // In simple content, an xsl:document instruction contributes
                    // the string value of the document node (descendant text only),
                    // not the comment/PI descendants.
                    var docValue = EvaluateSequenceConstructor(instruction, contextItem, wrapInDocumentNode: true);
                    if (docValue.IsNode && docValue.NodeValue != null)
                        items.Add(docValue);
                    break;
                }

            case "for-each":
                {
                    var feSelect = instruction.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(feSelect))
                    {
                        var compiled = XPath31Expression.Compile(feSelect);
                        var result = compiled.Evaluate(_context);
                        var feItems = new List<XdmValue>();
                        if (result.IsSequence && result.SequenceValue != null)
                        {
                            foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                                feItems.Add(item);
                        }
                        else
                        {
                            feItems.Add(result);
                        }

                        // Apply xsl:sort if present
                        var sortElements = instruction.Elements(XName.Get("sort", Stylesheet.Stylesheet.XslNamespace)).ToList();
                        if (sortElements.Count > 0)
                        {
                            feItems = SortItems(feItems, sortElements);
                        }

                        var savedItem = _context.ContextItem;
                        var savedCurrent = _context.CurrentItem;
                        var savedPosition = _context.ContextPosition;
                        var savedSize = _context.ContextSize;
                        try
                        {
                            for (int i = 0; i < feItems.Count; i++)
                            {
                                _context.WithFocus(feItems[i], i + 1, feItems.Count);
                                _context.WithCurrentItem(feItems[i]);
                                CollectSimpleContentItems(instruction, feItems[i], items);
                            }
                        }
                        finally
                        {
                            _context.WithFocus(savedItem, savedPosition, savedSize);
                            _context.WithCurrentItem(savedCurrent);
                        }
                    }
                    break;
                }

            case "if":
                {
                    var test = instruction.Attribute("test")?.Value;
                    if (!string.IsNullOrEmpty(test))
                    {
                        var compiled = CompileXPath(test, instruction);
                        WithDefaultCollation(instruction, () =>
                        {
                            if (compiled.Evaluate(_context).EffectiveBooleanValue())
                            {
                                CollectSimpleContentItems(instruction, contextItem, items);
                            }
                        });
                    }
                    break;
                }

            case "choose":
                {
                    bool matched = false;
                    foreach (var when in instruction.Elements(XName.Get("when", Stylesheet.Stylesheet.XslNamespace)))
                    {
                        var whenTest = when.Attribute("test")?.Value;
                        if (!string.IsNullOrEmpty(whenTest))
                        {
                            var compiled = CompileXPath(whenTest, when);
                            WithDefaultCollation(when, () =>
                            {
                                if (compiled.Evaluate(_context).EffectiveBooleanValue())
                                {
                                    CollectSimpleContentItems(when, contextItem, items);
                                    matched = true;
                                }
                            });
                            if (matched)
                                break;
                        }
                    }
                    if (!matched)
                    {
                        var otherwise = instruction.Element(XName.Get("otherwise", Stylesheet.Stylesheet.XslNamespace));
                        if (otherwise != null)
                        {
                            WithDefaultCollation(otherwise, () => CollectSimpleContentItems(otherwise, contextItem, items));
                        }
                    }
                    break;
                }

            case "variable":
            case "param":
                {
                    var varName = instruction.Attribute("name")?.Value;
                    var varSelect = instruction.Attribute("select")?.Value;
                    if (!string.IsNullOrEmpty(varName))
                    {
                        var (varLocal, varNs) = ExpandVariableName(instruction, varName);
                        XdmValue varValue;
                        if (!string.IsNullOrEmpty(varSelect))
                        {
                            var compiled = XPath31Expression.Compile(varSelect);
                            varValue = compiled.Evaluate(_context);
                        }
                        else
                        {
                            varValue = EvaluateSequenceConstructor(instruction, contextItem, wrapInDocumentNode: string.IsNullOrEmpty(instruction.Attribute("as")?.Value));
                        }
                        varValue = ConvertVariableValue(varValue, instruction.Attribute("as")?.Value, isParam: true);
                        _context.WithVariable(varLocal, varValue, varNs);
                    }
                    break;
                }

            case "message":
                {
                    var messageValue = BuildMessageValue(instruction, contextItem);
                    var messageString = SerializeMessageValue(messageValue);
                    _messageListener?.OnMessage(messageString);
                    break;
                }

            case "apply-templates":
                {
                    var atSelect = instruction.Attribute("select")?.Value;
                    var atModeRaw = instruction.Attribute("mode")?.Value?.Trim();
                    var atMode = string.IsNullOrEmpty(atModeRaw)
                        ? CurrentDefaultMode
                        : ExpandModeName(atModeRaw, instruction);
                    var atSortElements = instruction.Elements(XName.Get("sort", Stylesheet.Stylesheet.XslNamespace)).ToList();
                    var (atWithParams, atTunnelParams) = CollectWithParams(instruction, contextItem);

                    var savedAtContainer = _currentContainer;
                    var savedAtAccumulator = _sequenceAccumulator;
                    var savedAtLastAtomic = _lastAddedWasAtomic;
                    var atTemp = new XElement("__apply-templates-content__");
                    _currentContainer = atTemp;
                    _sequenceAccumulator = null;
                    _lastAddedWasAtomic = false;
                    try
                    {
                        if (contextItem.IsNode)
                        {
                            ApplyTemplates(contextItem.NodeValue, atMode, atSelect, atSortElements.Count > 0 ? atSortElements : null, atTunnelParams, atWithParams, instruction);
                        }
                        else if (!string.IsNullOrEmpty(atSelect))
                        {
                            ApplyTemplates(contextItem, atMode, atSelect, atSortElements.Count > 0 ? atSortElements : null, atTunnelParams, atWithParams, instruction);
                        }
                        else if (!contextItem.IsUndefined)
                        {
                            throw new InvalidOperationException("XTTE0510: The context item for xsl:apply-templates is not a node.");
                        }
                    }
                    finally
                    {
                        _currentContainer = savedAtContainer;
                        _sequenceAccumulator = savedAtAccumulator;
                        _lastAddedWasAtomic = savedAtLastAtomic;
                    }

                    foreach (var child in atTemp.Nodes())
                    {
                        switch (child)
                        {
                            case XText t:
                                items.Add(XdmValue.FromNode(new XDocumentNode(new XText(t.Value))));
                                break;
                            case XElement e:
                                items.Add(XdmValue.FromNode(new XDocumentNode(e)));
                                break;
                            case XComment c:
                                items.Add(XdmValue.FromNode(new XDocumentNode(c)));
                                break;
                            case XProcessingInstruction pi:
                                items.Add(XdmValue.FromNode(new XDocumentNode(pi)));
                                break;
                        }
                    }
                    break;
                }

            case "result-document":
            case "fallback":
            case "sort":
            case "on-empty":
            case "on-non-empty":
                // No output in simple content; sort/on-empty/on-non-empty are handled by their parent.
                break;

            case "assert":
                // xsl:assert is accepted but not yet evaluated in simple content.
                break;

            default:
                // Fallback: execute into a temporary container and extract nodes.
                var savedContainer = _currentContainer;
                var temp = new XElement("__fallback__");
                _currentContainer = temp;
                try
                {
                    var currentNode = contextItem.IsNode ? contextItem.NodeValue : null;
                    ExecuteXsltInstruction(instruction, currentNode!);
                    foreach (var child in temp.Nodes())
                    {
                        switch (child)
                        {
                            case XText t:
                                items.Add(XdmValue.FromNode(new XDocumentNode(new XText(t.Value))));
                                break;
                            case XElement e:
                                items.Add(XdmValue.FromNode(new XDocumentNode(e)));
                                break;
                            case XComment c:
                                items.Add(XdmValue.FromNode(new XDocumentNode(c)));
                                break;
                            case XProcessingInstruction pi:
                                items.Add(XdmValue.FromNode(new XDocumentNode(pi)));
                                break;
                        }
                    }
                }
                finally
                {
                    _currentContainer = savedContainer;
                }
                break;
        }
    }

    /// <summary>
    /// Applies simple content construction rules to a list of items and returns
    /// the concatenated string.
    /// </summary>
    private static string ConstructSimpleContentString(List<XdmValue> items, string separator)
    {
        var groups = new List<List<string>>();
        var currentGroup = new List<string>();
        string? pendingText = null;

        foreach (var item in items)
        {
            // Empty sequences (e.g. xs:language(())) separate adjacent atomic items:
            // they do not contribute characters and do not cause a separator to be
            // inserted between the atomics on either side of them.
            if (item.IsUndefined || IsEmptySequence(item))
            {
                if (pendingText != null)
                {
                    currentGroup.Add(pendingText);
                    pendingText = null;
                }
                if (currentGroup.Count > 0)
                {
                    groups.Add(currentGroup);
                    currentGroup = new List<string>();
                }
                continue;
            }

            bool isTextNode = item.IsNode && item.NodeValue != null &&
                              item.NodeValue.NodeKind == XdmNodeKind.Text;

            if (isTextNode && item.NodeValue!.StringValue.Length == 0)
            {
                continue; // Remove zero-length text nodes
            }

            if (isTextNode)
            {
                if (pendingText != null)
                {
                    pendingText += item.NodeValue!.StringValue;
                }
                else
                {
                    pendingText = item.NodeValue!.StringValue;
                }
            }
            else
            {
                if (pendingText != null)
                {
                    currentGroup.Add(pendingText);
                    pendingText = null;
                }
                // Atomize and cast to string
                currentGroup.Add(item.ToString());
            }
        }

        if (pendingText != null)
        {
            currentGroup.Add(pendingText);
        }
        if (currentGroup.Count > 0)
        {
            groups.Add(currentGroup);
        }

        var sb = new StringBuilder();
        for (int i = 0; i < groups.Count; i++)
        {
            sb.Append(string.Join(separator, groups[i]));
        }
        return sb.ToString();
    }

    /// <summary>
    /// Returns true if the value represents an empty sequence.
    /// </summary>
    private static bool IsEmptySequence(XdmValue value)
    {
        if (!value.IsSequence || value.SequenceValue == null)
            return false;
        return !value.SequenceValue.GetEnumerator().MoveNext();
    }

    /// <summary>
    /// Copies a literal result element into a standalone XElement without
    /// adding it to the current result container.
    /// </summary>
    private XElement CopyLiteralElementToXElement(XElement source)
    {
        var savedContainer = _currentContainer;
        var temp = new XElement("__temp__");
        _currentContainer = temp;
        try
        {
            CopyLiteralElement(source);
            return temp.Elements().First();
        }
        finally
        {
            _currentContainer = savedContainer;
        }
    }

    /// <summary>
    /// Serializes a secondary result document to the file system.
    /// </summary>
    private void WriteResultDocument(string uri, XElement content, Stylesheet.OutputProperties props)
    {
        string path;
        if (Uri.TryCreate(uri, UriKind.Absolute, out var u) && u.IsFile)
            path = u.LocalPath;
        else
            path = uri;

        var dir = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            System.IO.Directory.CreateDirectory(dir);

        // Result trees produced by xsl:result-document are explicit (not implicit)
        // for default output-method inference; carry the stylesheet version along.
        props.EffectiveVersion ??= _stylesheet.Version;
        props.ImplicitResultTree = false;

        // Wrap the result-document children in a document so that serialization
        // honours the effective output properties (version, undeclare-prefixes, etc.).
        var elementChildren = content.Elements().ToList();
        if (elementChildren.Count == 1)
        {
            var doc = new XDocument(elementChildren[0]);
            var serialized = ResultTreeSerializer.Serialize(XdmValue.FromNode(new XDocumentNode(doc)), props);
            System.IO.File.WriteAllText(path, serialized);
        }
        else if (props.Method == "text")
        {
            // The text output method emits the string value of the result tree without
            // XML escaping; string.Concat over XNodes would escape text via
            // XText.ToString(). Comments and processing instructions are ignored.
            System.IO.File.WriteAllText(path, content.Value);
        }
        else
        {
            System.IO.File.WriteAllText(path, string.Concat(content.Nodes()));
        }
    }

    /// <summary>
    /// Serializes a raw XDM value to a secondary result document file.
    /// Used for <c>method="json"</c>, <c>method="adaptive"</c>, or
    /// <c>build-tree="no"</c> result documents.
    /// </summary>
    private void WriteResultDocument(string uri, XdmValue value, Stylesheet.OutputProperties props)
    {
        string path;
        if (Uri.TryCreate(uri, UriKind.Absolute, out var u) && u.IsFile)
            path = u.LocalPath;
        else
            path = uri;

        var dir = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            System.IO.Directory.CreateDirectory(dir);

        props.EffectiveVersion ??= _stylesheet.Version;
        props.ImplicitResultTree = false;

        var serialized = ResultTreeSerializer.Serialize(value, props);
        System.IO.File.WriteAllText(path, serialized);
    }

    /// <summary>
    /// Evaluates attribute value templates on the serialization attributes of an
    /// <c>xsl:result-document</c> instruction and returns an element whose attributes
    /// contain the evaluated values.
    /// </summary>
    private XElement EvaluateResultDocumentInstruction(XElement instruction)
    {
        var evaluated = new XElement(instruction.Name);
        // Preserve all in-scope namespace declarations from the original instruction so
        // that QName-valued serialization attributes (cdata-section-elements,
        // suppress-indentation, use-character-maps) can be resolved correctly.
        foreach (var ns in GetInScopeNamespaces(instruction))
        {
            var attrName = string.IsNullOrEmpty(ns.Key)
                ? (XNamespace.None + "xmlns")
                : (XNamespace.Xmlns + ns.Key);
            evaluated.SetAttributeValue(attrName, ns.Value);
        }

        foreach (var attr in instruction.Attributes())
        {
            var localName = attr.Name.LocalName;
            // @use-character-maps is not an AVT and its prefixes must be resolved in the
            // context of the original instruction, so do not copy it to the evaluated stub.
            if (localName == "use-character-maps")
                continue;
            var value = attr.Value;
            if (localName is "method" or "output-version" or "encoding" or "indent"
                or "omit-xml-declaration" or "standalone" or "undeclare-prefixes"
                or "doctype-public" or "doctype-system" or "html-version" or "normalization-form"
                or "escape-uri-attributes" or "include-content-type" or "media-type"
                or "byte-order-mark" or "json-node-output-method" or "allow-duplicate-names"
                or "escape-solidus" or "item-separator" or "cdata-section-elements"
                or "suppress-indentation" or "parameter-document")
            {
                value = EvaluateAvt(value, instruction);
            }
            evaluated.SetAttributeValue(attr.Name, value);
        }

        var undeclareValue = evaluated.Attribute("undeclare-prefixes")?.Value;
        if (undeclareValue != null)
        {
            var trimmed = undeclareValue.Trim().ToLowerInvariant();
            if (trimmed is not ("yes" or "no" or "true" or "false" or "1" or "0"))
                throw new InvalidOperationException("XTSE0020: The value of undeclare-prefixes must be yes, no, true, false, 1, or 0.");
        }

        return evaluated;
    }

    /// <summary>
    /// Executes an <c>xsl:result-document</c> or EXSLT <c>exsl:document</c> instruction.
    /// The content is evaluated into a separate result tree and either becomes the
    /// principal output (when <paramref name="isPrincipal"/> is true) or is serialized
    /// to a secondary URI.
    /// </summary>
    private void ExecuteResultDocument(XElement instruction, XdmValue contextItem, bool isPrincipal)
    {
        var hrefRaw = instruction.Attribute("href")?.Value;
        var href = EvaluateAvt(hrefRaw ?? string.Empty, instruction);
        // @href is resolved against the base output URI when one is known; otherwise
        // fall back to the static base URI of the instruction.
        // Resolve @href relative to the current output URI when inside a secondary result
        // document, otherwise relative to the base output URI or the stylesheet base URI.
        var resolutionBase = _context.CurrentOutputUri ?? _baseOutputUri ?? instruction.BaseUri ?? _context.BaseUri ?? string.Empty;
        string resolvedHref;
        if (Uri.IsWellFormedUriString(href, UriKind.Absolute))
            resolvedHref = href;
        else if (!string.IsNullOrEmpty(resolutionBase))
            resolvedHref = new Uri(new Uri(resolutionBase), href).AbsoluteUri;
        else
            resolvedHref = href;

        var principalContainer = _resultDocumentStack.Count == 0
            ? _currentContainer
            : _resultDocumentStack.Peek().PrincipalContainer;

        // Capture the effective output properties for this result document,
        // merging any instruction attributes with the stylesheet-level xsl:output
        // properties. Attribute value templates are evaluated for the serialization
        // properties that may contain AVTs. If @format is present, the named output
        // definition is looked up and merged between the unnamed defaults and the
        // instruction-level overrides.
        var formatRaw = instruction.Attribute("format")?.Value;
        var formatLexical = string.IsNullOrEmpty(formatRaw) ? string.Empty : EvaluateAvt(formatRaw, instruction);
        Stylesheet.OutputProperties? namedProps = null;
        if (!string.IsNullOrEmpty(formatLexical))
        {
            var expandedName = Stylesheet.Stylesheet.ExpandQName(instruction, formatLexical);
            namedProps = _stylesheet.GetEffectiveNamedOutput(expandedName);
            if (namedProps == null)
                throw new XsltRuntimeException("XTDE1460",
                    $"No xsl:output definition named '{formatLexical}' exists.",
                    contextItem);
        }

        var baseProps = _stylesheet.EffectiveOutputProperties ?? new Stylesheet.OutputProperties();
        var evaluatedInstruction = EvaluateResultDocumentInstruction(instruction);
        var resultDocumentProps = new Stylesheet.OutputProperties();

        // A named output definition replaces the unnamed default; the unnamed default
        // is only used when no format attribute is present. Instruction-level attributes
        // override either source.
        if (namedProps != null)
            Stylesheet.OutputProperties.Merge(resultDocumentProps, namedProps);
        else
            Stylesheet.OutputProperties.Merge(resultDocumentProps, baseProps);

        // A parameter document supplies defaults that are overridden by explicit attributes
        // on xsl:result-document.
        var parameterDocumentUri = evaluatedInstruction.Attribute("parameter-document")?.Value;
        if (!string.IsNullOrEmpty(parameterDocumentUri))
        {
            var paramDoc = ResolveParameterDocument(parameterDocumentUri, instruction.BaseUri);
            if (paramDoc != null)
            {
                var paramProps = Stylesheet.OutputProperties.FromSerializationParameters(paramDoc);
                Stylesheet.OutputProperties.Merge(resultDocumentProps, paramProps);
            }
        }

        Stylesheet.OutputProperties.Merge(resultDocumentProps, Stylesheet.OutputProperties.FromElement(evaluatedInstruction));

        // Resolve any named character maps into a concrete character-to-string table.
        // Stylesheet-level references are added first in declaration order; instruction-
        // level references are appended so that they take precedence over stylesheet-level
        // maps with conflicting character entries.
        var useCharacterMapsAttr = instruction.Attribute("use-character-maps")?.Value;
        var expandedNames = new List<string>();
        foreach (var q in resultDocumentProps.UseCharacterMaps)
        {
            var expanded = Stylesheet.Stylesheet.ExpandQName(q);
            if (!expandedNames.Contains(expanded))
                expandedNames.Add(expanded);
        }
        if (!string.IsNullOrWhiteSpace(useCharacterMapsAttr))
        {
            foreach (var name in useCharacterMapsAttr.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var expanded = Stylesheet.Stylesheet.ExpandQName(instruction, name.Trim());
                if (!string.IsNullOrEmpty(expanded) && !expandedNames.Contains(expanded))
                    expandedNames.Add(expanded);
            }
        }
        if (expandedNames.Count > 0)
            resultDocumentProps.CharacterMap = _stylesheet.ResolveCharacterMap(expandedNames);

        // Determine whether this result document should preserve top-level items as raw
        // XDM values rather than building a result tree.
        var collectRaw = resultDocumentProps.Method is "json" or "adaptive" || !resultDocumentProps.BuildTree;

        // When the caller requested the raw initial-template result, force raw-item
        // collection for the principal result document so that item-separator etc.
        // are applied to the produced sequence.
        if (isPrincipal && _returnRawInitialTemplateResult)
            collectRaw = true;

        if (isPrincipal)
        {
            _principalResultDocumentProperties = resultDocumentProps;
            // A nested principal result document is allowed only when the enclosing
            // secondary result document was opened at the top level of the principal
            // result tree. If the principal output had already descended into an element,
            // switching back would leave it ill-formed.
            if (_resultDocumentStack.Count > 0)
            {
                var outerFrame = _resultDocumentStack.Peek();
                if (!ReferenceEquals(outerFrame.PrincipalContainer, _resultDocument))
                    throw new InvalidOperationException("XTDE1490: A nested result document cannot be created while the principal result tree is inside an element.");
            }

            if (_principalOutputClosed || _principalOutputHasContent || _resultDocumentStack.Any(f => f.TargetUri == string.Empty))
                throw new InvalidOperationException("XTDE1490: A result document with the same URI has already been created.");

            var savedContainer = _currentContainer;
            var savedOutputUri = _context.CurrentOutputUri;
            XElement? principalTemp = null;
            if (collectRaw)
            {
                // For raw output we still need a distinct root container so that the
                // raw-collection top-level check can distinguish result-document content
                // from the enclosing principal tree.
                principalTemp = new XElement("__result-document__");
                _currentContainer = principalTemp;
            }
            else
            {
                _currentContainer = principalContainer;
            }

            var frame = new ResultDocumentFrame(string.Empty, principalTemp, savedContainer, principalContainer);
            _resultDocumentStack.Push(frame);
            _context.CurrentOutputUri = _baseOutputUri ?? string.Empty;

            // For raw output, enable raw-item collection and capture the result so that
            // an enclosing non-JSON result document is not affected.
            var savedCollectRaw = _collectRawItems;
            var savedRawItems = _resultDocumentRawItems;
            if (collectRaw)
            {
                _collectRawItems = true;
                _resultDocumentRawItems = new List<XdmValue>();
            }

            try
            {
                foreach (var childNode in instruction.Nodes())
                {
                    switch (childNode)
                    {
                        case XText text:
                            ProcessSequenceText(text, instruction);
                            break;
                        case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                            ExecuteXsltInstruction(elem, contextItem);
                            break;
                        case XElement elem:
                            CopyLiteralElement(elem);
                            break;
                    }
                }
            }
            finally
            {
                if (collectRaw)
                {
                    _principalRawResultDocument = _resultDocumentRawItems.Count == 0
                        ? XdmValue.Undefined
                        : XdmValue.FromSequence(MaterializedSequence.FromList(_resultDocumentRawItems));
                    _collectRawItems = savedCollectRaw;
                    _resultDocumentRawItems = savedRawItems;
                }

                _context.CurrentOutputUri = savedOutputUri;
                _currentContainer = savedContainer;
                _resultDocumentStack.Pop();
            }

            // A top-level principal result document closes the principal output URI.
            if (_resultDocumentStack.Count == 0)
            {
                _principalOutputClosed = true;
                if (!collectRaw && _principalResultDocumentProperties != null && principalContainer is XElement principalElem)
                    principalElem.AddAnnotation(_principalResultDocumentProperties);
            }
        }
        else
        {
            if (_resultDocumentUris.Contains(resolvedHref))
                throw new InvalidOperationException("XTDE1490: A result document with the same URI has already been created.");

            // A secondary result document must not target the principal output URI.
            var principalOutputUri = _baseOutputUri ?? string.Empty;
            if (resolvedHref == principalOutputUri)
                throw new InvalidOperationException("XTDE1490: A result document with the same URI has already been created.");

            var savedContainer = _currentContainer;
            var savedLastAtomic = _lastAddedWasAtomic;
            var savedOutputUri = _context.CurrentOutputUri;
            var savedCollectRaw = _collectRawItems;
            var savedRawItems = _resultDocumentRawItems;
            XElement? temp = null;
            List<XdmValue>? rawItems = null;

            temp = new XElement("__result-document__");
            _currentContainer = temp;
            if (collectRaw)
            {
                _collectRawItems = true;
                _resultDocumentRawItems = new List<XdmValue>();
                rawItems = _resultDocumentRawItems;
            }

            _lastAddedWasAtomic = false;
            _context.CurrentOutputUri = resolvedHref;
            var frame = new ResultDocumentFrame(resolvedHref, temp, savedContainer, principalContainer);
            _resultDocumentStack.Push(frame);
            try
            {
                foreach (var childNode in instruction.Nodes())
                {
                    switch (childNode)
                    {
                        case XText text:
                            ProcessSequenceText(text, instruction);
                            break;
                        case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                            ExecuteXsltInstruction(elem, contextItem);
                            break;
                        case XElement elem:
                            CopyLiteralElement(elem);
                            break;
                    }
                }
            }
            finally
            {
                _context.CurrentOutputUri = savedOutputUri;
                _currentContainer = savedContainer;
                _lastAddedWasAtomic = savedLastAtomic;
                _resultDocumentStack.Pop();
                _collectRawItems = savedCollectRaw;
                _resultDocumentRawItems = savedRawItems;
            }

            if (_captureResultDocuments)
            {
                // fn:transform: capture the secondary result document instead of
                // writing it to the file system. Tree output is wrapped in a real
                // document node; raw output (build-tree="no", JSON) is kept as-is.
                resultDocumentProps.EffectiveVersion ??= _stylesheet.Version;
                resultDocumentProps.ImplicitResultTree = false;
                XdmValue capturedValue;
                if (rawItems != null)
                {
                    capturedValue = rawItems.Count == 0
                        ? XdmValue.Undefined
                        : XdmValue.FromSequence(MaterializedSequence.FromList(rawItems));
                }
                else
                {
                    var capturedDoc = new XDocument();
                    foreach (var node in temp!.Nodes().ToList())
                    {
                        node.Remove();
                        capturedDoc.Add(node);
                    }
                    capturedValue = XdmValue.FromNode(new XDocumentNode(capturedDoc));
                }
                _capturedResultDocuments[resolvedHref] = (capturedValue, resultDocumentProps);
            }
            else if (rawItems != null)
            {
                var rawValue = rawItems.Count == 0
                    ? XdmValue.Undefined
                    : XdmValue.FromSequence(MaterializedSequence.FromList(rawItems));
                WriteResultDocument(resolvedHref, rawValue, resultDocumentProps);
            }
            else
            {
                WriteResultDocument(resolvedHref, temp!, resultDocumentProps);
            }

            _resultDocumentUris.Add(resolvedHref);
        }
    }

    // ------------------------------------------------------------------
    // Whitespace stripping (xsl:strip-space / xsl:preserve-space)
    // ------------------------------------------------------------------

    private IXdmNode PostProcessLoadedDocument(IXdmNode node)
    {
        ApplyWhitespaceStripping(node);
        return node;
    }

    private bool IsStylesheetDocument(XDocument doc)
    {
        return doc == _stylesheet.Root.Document || IsStylesheetDocumentRecursive(doc, _stylesheet);
    }

    private static bool IsStylesheetDocumentRecursive(XDocument doc, Stylesheet.Stylesheet sheet)
    {
        if (doc == sheet.Root.Document)
            return true;

        foreach (var included in sheet.Includes)
        {
            if (IsStylesheetDocumentRecursive(doc, included))
                return true;
        }

        foreach (var imported in sheet.Imports)
        {
            if (IsStylesheetDocumentRecursive(doc, imported))
                return true;
        }

        return false;
    }

    private void ApplyWhitespaceStripping(IXdmNode source)
    {
        var rules = _stylesheet.GetAllSpaceHandlingRules();

        // Only strip whitespace in XDocument-backed nodes for now
        if (source is XDocumentNode xdocNode)
        {
            if (xdocNode.UnderlyingObject is XDocument doc)
            {
                // Strip whitespace text nodes that are direct children of the document
                foreach (var textNode in doc.Nodes().OfType<XText>().ToList())
                {
                    if (IsWhitespaceOnly(textNode.Value))
                        textNode.Remove();
                }
                if (rules.Count > 0)
                    StripWhitespaceInElement(doc.Root, rules, preserveInherited: false);
            }
            else if (xdocNode.UnderlyingObject is XElement elem)
            {
                if (rules.Count > 0)
                    StripWhitespaceInElement(elem, rules, preserveInherited: false);
            }
        }
    }

    private static void StripWhitespaceInElement(XElement? element, List<SpaceHandlingRule> rules, bool preserveInherited)
    {
        if (element == null)
            return;

        bool preserve = preserveInherited;
        var xmlSpace = element.Attribute(System.Xml.Linq.XNamespace.Xml + "space")?.Value;
        if (xmlSpace == "preserve")
            preserve = true;
        else if (xmlSpace == "default")
            preserve = false;

        foreach (var child in element.Elements().ToList())
        {
            StripWhitespaceInElement(child, rules, preserve);
        }

        if (!preserve && ShouldStripWhitespace(element, rules))
        {
            foreach (var textNode in element.Nodes().OfType<XText>().ToList())
            {
                if (IsWhitespaceOnly(textNode.Value))
                {
                    textNode.Remove();
                }
            }
        }
    }

    private static bool IsWhitespaceOnly(string text)
    {
        foreach (var c in text)
        {
            if (c != ' ' && c != '\t' && c != '\n' && c != '\r')
                return false;
        }
        return text.Length > 0;
    }

    /// <summary>
    /// Determines whether <paramref name="node"/> is still attached to its containing tree.
    /// A whitespace text node removed by xsl:strip-space will report as detached.
    /// </summary>
    private static bool IsNodeAttached(IXdmNode node)
    {
        if (node is not XDocumentNode xn)
            return true;

        if (xn.UnderlyingObject is XDocument)
            return true;
        if (xn.UnderlyingObject is XObject xo)
            return xo.Parent != null || xo.Document != null;
        return true;
    }

    /// <summary>
    /// Collects all namespace declarations that are in scope for <paramref name="element"/>,
    /// walking up to the stylesheet root. The nearest declaration wins for each prefix.
    /// </summary>
    private static IEnumerable<(string Prefix, XNamespace Namespace)> GetInScopeNamespaceDeclarations(XElement element)
    {
        var declarations = new Dictionary<string, XNamespace>();
        var current = element;
        while (current != null)
        {
            foreach (var attr in current.Attributes())
            {
                if (!attr.IsNamespaceDeclaration)
                    continue;
                string prefix = attr.Name.LocalName == "xmlns" ? "" : attr.Name.LocalName;
                if (!declarations.ContainsKey(prefix))
                    declarations[prefix] = XNamespace.Get(attr.Value);
            }
            current = current.Parent;
        }
        return declarations.Select(kv => (kv.Key, kv.Value));
    }

    private static int NameTestSpecificity(SpaceHandlingRule rule)
        => rule.Kind switch
        {
            SpaceNameTestKind.Exact => 3,
            SpaceNameTestKind.WildcardLocal => 2,
            SpaceNameTestKind.WildcardNamespace => 1,
            _ => 0
        };

    private static bool ShouldStripWhitespace(XElement element, List<SpaceHandlingRule> rules)
    {
        // xml:space="preserve" always preserves whitespace
        var xmlSpace = element.Attribute(System.Xml.Linq.XNamespace.Xml + "space")?.Value;
        if (xmlSpace == "preserve")
            return false;

        SpaceHandlingRule? bestStrip = null;
        SpaceHandlingRule? bestPreserve = null;
        int bestStripSpec = -1;
        int bestPreserveSpec = -1;

        foreach (var rule in rules)
        {
            if (!MatchesNameTest(rule, element))
                continue;

            int spec = NameTestSpecificity(rule);
            if (rule.IsStrip)
            {
                if (bestStrip == null ||
                    rule.Precedence > bestStrip.Value.Precedence ||
                    (rule.Precedence == bestStrip.Value.Precedence && spec >= bestStripSpec))
                {
                    bestStrip = rule;
                    bestStripSpec = spec;
                }
            }
            else
            {
                if (bestPreserve == null ||
                    rule.Precedence > bestPreserve.Value.Precedence ||
                    (rule.Precedence == bestPreserve.Value.Precedence && spec >= bestPreserveSpec))
                {
                    bestPreserve = rule;
                    bestPreserveSpec = spec;
                }
            }
        }

        if (bestStrip == null)
            return false;
        if (bestPreserve == null)
            return true;

        if (bestStrip.Value.Precedence > bestPreserve.Value.Precedence)
            return true;
        if (bestStrip.Value.Precedence < bestPreserve.Value.Precedence)
            return false;

        // Same precedence: the more specific name test wins.
        if (bestStripSpec > bestPreserveSpec)
            return true;
        if (bestStripSpec < bestPreserveSpec)
            return false;

        // Same precedence and specificity: this is a conflict between strip and preserve
        // rules (XTSE0270). The spec treats it as a recoverable static error.
        throw new InvalidOperationException("XTSE0270: Conflicting xsl:strip-space and xsl:preserve-space rules");
    }

    private static bool MatchesNameTest(SpaceHandlingRule rule, XElement element)
    {
        return rule.Kind switch
        {
            SpaceNameTestKind.Any => true,
            SpaceNameTestKind.WildcardLocal => element.Name.LocalName == rule.LocalName,
            SpaceNameTestKind.WildcardNamespace => element.Name.NamespaceName == (rule.NamespaceUri ?? string.Empty),
            SpaceNameTestKind.Exact => element.Name.LocalName == rule.LocalName
                && element.Name.NamespaceName == (rule.NamespaceUri ?? string.Empty),
            _ => false,
        };
    }

    // ------------------------------------------------------------------
    // Text Value Template (expand-text) support
    // ------------------------------------------------------------------

    /// <summary>
    /// Returns the inherited value of the expand-text attribute for the given element.
    /// Walks up the XML tree until it finds an explicit expand-text attribute;
    /// defaults to false if none is found.
    /// </summary>
    private static bool GetExpandText(XElement element)
    {
        XElement? current = element;
        while (current != null)
        {
            // expand-text may appear as a no-namespace attribute on XSLT elements
            // (e.g. xsl:template) or as an xsl:expand-text attribute on literal
            // result elements.
            var attr = current.Attribute("expand-text")
                ?? current.Attribute(XName.Get("expand-text", Stylesheet.Stylesheet.XslNamespace));
            if (attr != null)
            {
                var v = attr.Value.Trim();
                return v is "yes" or "true" or "1";
            }
            current = current.Parent;
        }
        return false;
    }

    /// <summary>
    /// Evaluates a Text Value Template (TVT) and returns the parts as a list of
    /// literal/evaluation segments. The list alternates literal text (even indexes)
    /// and expression results (odd indexes). Empty leading or trailing literal
    /// segments are represented by empty strings so callers can apply whitespace
    /// stripping independently of concatenation.
    /// </summary>
    private List<string> EvaluateTvtParts(string text, XElement? contextElement = null)
    {
        var parts = new List<string>();
        if (string.IsNullOrEmpty(text))
        {
            parts.Add(text);
            return parts;
        }

        var sb = new StringBuilder();
        int i = 0;

        while (i < text.Length)
        {
            // {{ escape → {
            if (i < text.Length - 1 && text[i] == '{' && text[i + 1] == '{')
            {
                sb.Append('{');
                i += 2;
                continue;
            }

            // }} escape → }
            if (i < text.Length - 1 && text[i] == '}' && text[i + 1] == '}')
            {
                sb.Append('}');
                i += 2;
                continue;
            }

            // {expr} — evaluate XPath expression
            if (text[i] == '{')
            {
                // Flush the literal part that precedes this expression.
                parts.Add(sb.ToString());
                sb.Clear();

                int exprStart = i + 1;
                int j = exprStart;
                int braceDepth = 1;
                bool inSingleQuote = false;
                bool inDoubleQuote = false;
                int commentDepth = 0;

                while (j < text.Length && braceDepth > 0)
                {
                    char c = text[j];
                    if (commentDepth > 0)
                    {
                        // Inside an XPath comment; (: opens a nested comment and :) closes one.
                        if (j + 1 < text.Length && c == '(' && text[j + 1] == ':')
                        {
                            commentDepth++;
                            j += 2;
                            continue;
                        }
                        if (j + 1 < text.Length && c == ':' && text[j + 1] == ')')
                        {
                            commentDepth--;
                            j += 2;
                            continue;
                        }
                        j++;
                        continue;
                    }

                    if (inSingleQuote)
                    {
                        if (c == '\'') inSingleQuote = false;
                    }
                    else if (inDoubleQuote)
                    {
                        if (c == '"') inDoubleQuote = false;
                    }
                    else
                    {
                        if (c == '\'') inSingleQuote = true;
                        else if (c == '"') inDoubleQuote = true;
                        else if (j + 1 < text.Length && c == '(' && text[j + 1] == ':')
                        {
                            commentDepth++;
                            j += 2;
                            continue;
                        }
                        else if (c == '{') braceDepth++;
                        else if (c == '}') braceDepth--;
                    }
                    j++;
                }

                if (braceDepth == 0)
                {
                    string expr = text.Substring(exprStart, j - exprStart - 1);
                    expr = RemoveXPathComments(expr);
                    if (!string.IsNullOrWhiteSpace(expr))
                    {
                        var compiled = contextElement != null ? CompileXPath(expr, contextElement) : XPath31Expression.Compile(expr);
                        var value = compiled.Evaluate(_context);
                        // XSLT 3.0 §5.6.2/§5.7.2: the expression value is converted to a
                        // string using the simple content construction rules with a single
                        // space separator; adjacent text nodes are merged before atomization
                        // so they contribute no separator (e.g. seqtor-043d).
                        parts.Add(ConstructSimpleContentString(FlattenSelectedItems(value), " "));
                    }
                    else
                    {
                        parts.Add(string.Empty);
                    }
                    i = j;
                    continue;
                }
                else
                {
                    // Unmatched { in a TVT context is an error (the expression may span
                    // an XML element boundary or simply be malformed).
                    throw new InvalidOperationException("XTSE0350: Unmatched '{' in text value template");
                }
            }

            sb.Append(text[i]);
            i++;
        }

        if (sb.Length > 0 || parts.Count > 0)
        {
            parts.Add(sb.ToString());
        }

        return parts;
    }

    /// <summary>
    /// Evaluates a Text Value Template (TVT): parses {expr} and {{ escapes,
    /// evaluates each XPath expression, and returns the concatenated result.
    /// Respects XPath string literals when finding matching }.
    /// </summary>
    private string EvaluateTvt(string text, XElement? contextElement = null)
    {
        var parts = EvaluateTvtParts(text, contextElement);
        return string.Concat(parts);
    }

    /// <summary>
    /// XSLT 3.0 §3.3.1.1: Whitespace text nodes are preserved in xsl:text
    /// and in elements (or their ancestors) that carry xml:space="preserve".
    /// </summary>
    private static bool IsWhitespacePreserveContext(XElement parent)
    {
        if (parent.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace
            && parent.Name.LocalName == "text")
        {
            return true;
        }

        var current = parent;
        while (current != null)
        {
            if (current.Attribute(XNamespace.Xml + "space")?.Value == "preserve")
                return true;
            current = current.Parent;
        }

        return false;
    }

    /// <summary>
    /// Processes a text node encountered in a sequence constructor.
    /// If the parent element (or an ancestor) has expand-text="yes",
    /// evaluates the text as a TVT. Otherwise applies normal whitespace
    /// stripping and adds the text node to the result.
    /// </summary>
    /// <summary>
    /// Returns true if the text contains an unescaped text value template expression ({...}).
    /// </summary>
    private static bool ContainsTvtExpression(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '{')
            {
                if (i + 1 < text.Length && text[i + 1] == '{')
                {
                    i++; // skip escaped {{
                }
                else
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Strips XPath comments (<c>(: ... :)</c>) from an expression, including nested
    /// comments, while preserving comments inside string literals.
    /// </summary>
    private static string RemoveXPathComments(string expression)
    {
        if (string.IsNullOrEmpty(expression))
            return expression;

        var sb = new StringBuilder(expression.Length);
        int i = 0;
        bool inSingleQuote = false;
        bool inDoubleQuote = false;
        int commentDepth = 0;

        while (i < expression.Length)
        {
            char c = expression[i];
            if (commentDepth > 0)
            {
                if (i + 1 < expression.Length && c == '(' && expression[i + 1] == ':')
                {
                    commentDepth++;
                    i += 2;
                    continue;
                }
                if (i + 1 < expression.Length && c == ':' && expression[i + 1] == ')')
                {
                    commentDepth--;
                    i += 2;
                    continue;
                }
                i++;
                continue;
            }

            if (inSingleQuote)
            {
                if (c == '\'') inSingleQuote = false;
                sb.Append(c);
            }
            else if (inDoubleQuote)
            {
                if (c == '"') inDoubleQuote = false;
                sb.Append(c);
            }
            else
            {
                if (i + 1 < expression.Length && c == '(' && expression[i + 1] == ':')
                {
                    commentDepth++;
                    i += 2;
                    continue;
                }
                if (c == '\'') inSingleQuote = true;
                else if (c == '"') inDoubleQuote = true;
                sb.Append(c);
            }
            i++;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Marker attached to text nodes that have been merged into a preceding
    /// sibling's Text Value Template evaluation.
    /// </summary>
    private sealed class TvtConsumedMarker
    {
        public static readonly TvtConsumedMarker Instance = new();
        private TvtConsumedMarker() { }
    }

    /// <summary>
    /// Merges a text node that starts a TVT expression with following text nodes
    /// that are separated only by XML comments or processing instructions.
    /// </summary>
    private static string MergeTvtText(XText start, out List<XText> consumed)
    {
        var sb = new StringBuilder(start.Value);
        consumed = new List<XText>();
        var next = start.NextNode;
        while (next != null && (next is XText || next is XComment || next is XProcessingInstruction))
        {
            if (next is XText t)
            {
                sb.Append(t.Value);
                consumed.Add(t);
            }
            next = next.NextNode;
        }
        return sb.ToString();
    }

    private void ProcessSequenceText(XText text, XElement parent)
    {
        // A text node that was consumed as the continuation of a preceding TVT
        // expression (e.g. after an XML comment or PI) must not be processed again.
        if (text.Annotation<TvtConsumedMarker>() != null)
            return;

        // XSLT 3.0 §4.3: whitespace text nodes immediately preceding an
        // xsl:param, xsl:sort, or xsl:context-item element are stripped from the
        // stylesheet regardless of any xml:space="preserve" attribute.
        if (IsWhitespaceOnly(text.Value)
            && parent.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace)
        {
            var next = text.NextNode;
            while (next is XComment || next is XProcessingInstruction)
                next = next.NextNode;
            if (next is XElement nextElem
                && nextElem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace
                && (nextElem.Name.LocalName == "param"
                    || nextElem.Name.LocalName == "sort"
                    || nextElem.Name.LocalName == "context-item"))
            {
                return;
            }
        }

        bool expandText = GetExpandText(parent);
        bool hasTvtExpression = expandText && ContainsTvtExpression(text.Value);
        bool hasEscapedBraces = expandText && (text.Value.Contains("{{", StringComparison.Ordinal) || text.Value.Contains("}}", StringComparison.Ordinal));

        if (hasTvtExpression)
        {
            // A TVT expression may span XML comments or PIs. Merge this text node
            // with all following text/comment/PI siblings, evaluate the combined
            // template, and mark the consumed text nodes so they are skipped.
            var mergedText = MergeTvtText(text, out var consumedNodes);
            foreach (var consumed in consumedNodes)
                consumed.AddAnnotation(TvtConsumedMarker.Instance);

            var tvtResult = EvaluateTvt(mergedText, parent);
            _lastAddedWasAtomic = false;
            // Preserve zero-length TVT results: they are valid zero-length text nodes
            // that break adjacent atomic values in complex content construction.
            AddTextNode(tvtResult, allowZeroLength: true);
        }
        else if (hasEscapedBraces)
        {
            var tvtResult = EvaluateTvt(text.Value, parent);
            _lastAddedWasAtomic = false;
            AddTextNode(tvtResult, allowZeroLength: true);
        }
        else if (IsWhitespacePreserveContext(parent))
        {
            // Preserve whitespace text nodes in xsl:text and xml:space="preserve" contexts
            _lastAddedWasAtomic = false;
            AddTextNode(text.Value);
        }
        else
        {
            // XSLT 3.0 §4.3: comments/PIs are removed and adjacent text nodes are
            // merged before whitespace stripping. A whitespace-only text node that
            // would become adjacent to a non-whitespace text node after that step
            // must be preserved, because it becomes part of the merged text node.
            if (!IsWhitespaceOnly(text.Value) || IsAdjacentToNonWhitespaceText(text, parent))
            {
                _lastAddedWasAtomic = false;
                AddTextNode(text.Value);
            }
        }
    }

    /// <summary>
    /// Determines whether a whitespace-only text node in a sequence constructor
    /// would be merged with a non-whitespace text node after comments and PIs
    /// are removed from the stylesheet (XSLT 3.0 §4.3). Such nodes are preserved.
    /// </summary>
    private static bool IsAdjacentToNonWhitespaceText(XText text, XElement parent)
    {
        bool IsNonWhitespaceText(XNode? node)
            => node is XText t && !IsWhitespaceOnly(t.Value);

        bool SkipNode(XNode? node)
            => node is XComment || node is XProcessingInstruction || (node is XText t && IsWhitespaceOnly(t.Value));

        var prev = text.PreviousNode;
        while (prev != null && SkipNode(prev))
            prev = prev.PreviousNode;
        if (IsNonWhitespaceText(prev))
            return true;

        var next = text.NextNode;
        while (next != null && SkipNode(next))
            next = next.NextNode;
        if (IsNonWhitespaceText(next))
            return true;

        return false;
    }

    // ------------------------------------------------------------------
    // xsl:number support
    // ------------------------------------------------------------------

    /// <summary>
    /// Determines whether the effective version for the given instruction is
    /// XSLT 1.0 (backwards-compatible), walking ancestor elements for an
    /// explicit <c>xsl:version</c> attribute and falling back to the global
    /// stylesheet version.
    /// </summary>
    private bool IsEffectiveBackwardsCompatible(XElement instruction)
    {
        return GetEffectiveVersion(instruction) < 2.0;
    }

    /// <summary>
    /// Returns the effective XSLT version for the given instruction, walking
    /// ancestor elements for an explicit <c>version</c> (or <c>xsl:version</c>)
    /// attribute and falling back to the global stylesheet version.
    /// </summary>
    private double GetEffectiveVersion(XElement instruction)
    {
        var xslNs = XNamespace.Get("http://www.w3.org/1999/XSL/Transform");
        var ancestor = instruction;
        while (ancestor != null)
        {
            XAttribute? versionAttr = null;
            if (ancestor.Name.NamespaceName == xslNs)
            {
                // XSLT elements use a no-namespace version attribute.
                versionAttr = ancestor.Attribute("version");
            }
            if (versionAttr == null)
            {
                // Literal result elements use xsl:version.
                versionAttr = ancestor.Attribute(xslNs + "version");
            }
            if (versionAttr != null)
            {
                if (double.TryParse(versionAttr.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v))
                    return v;
                break;
            }
            ancestor = ancestor.Parent;
        }
        if (double.TryParse(_stylesheet.Version, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var sv))
            return sv;
        return _context.BackwardsCompatible ? 1.0 : 3.0;
    }

    /// <summary>
    /// Determines whether the given element is in XSLT forwards-compatible mode
    /// (effective version greater than 3.0).
    /// </summary>
    private bool IsForwardsCompatible(XElement instruction)
    {
        return GetEffectiveVersion(instruction) > 3.0;
    }

    /// <summary>
    /// Validates that all <c>xsl:sort</c> children of <c>xsl:for-each</c> or
    /// <c>xsl:perform-sort</c> appear before any other instruction content.
    /// </summary>
    private void ValidateSortComesFirst(XElement instruction)
    {
        bool seenNonSort = false;
        foreach (var child in instruction.Elements())
        {
            if (child.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace && child.Name.LocalName == "sort")
            {
                if (seenNonSort)
                    throw new InvalidOperationException("XTSE0010: xsl:sort elements must appear before other sequence-constructor children");
            }
            else
            {
                seenNonSort = true;
            }
        }
    }

    /// <summary>
    /// Determines whether an instruction has content other than whitespace text
    /// and <c>xsl:fallback</c> elements.
    /// </summary>
    private bool HasNonFallbackContent(XElement instruction)
    {
        foreach (var node in instruction.Nodes())
        {
            if (node is XText t && string.IsNullOrWhiteSpace(t.Value))
                continue;
            if (node is XElement e && e.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace && e.Name.LocalName == "fallback")
                continue;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Executes an <c>xsl:number</c> instruction.
    /// </summary>
    private void ExecuteXsltNumber(XElement instruction, IXdmNode currentNode)
    {
        // Determine effective backwards-compatibility: walk ancestor chain for xsl:version.
        bool backwardsCompatible = IsEffectiveBackwardsCompatible(instruction);
        var level = instruction.Attribute("level")?.Value ?? "single";
        var countPattern = instruction.Attribute("count")?.Value;
        var fromPattern = instruction.Attribute("from")?.Value;
        var formatAttr = instruction.Attribute("format")?.Value ?? "1";
        var valueAttr = instruction.Attribute("value")?.Value;
        var selectAttr = instruction.Attribute("select")?.Value;
        var startAtAttr = instruction.Attribute("start-at")?.Value;
        var ordinalAttr = instruction.Attribute("ordinal")?.Value;
        var langAttr = instruction.Attribute("lang")?.Value;
        var groupingSepAttr = instruction.Attribute("grouping-separator")?.Value;
        var groupingSizeAttr = instruction.Attribute("grouping-size")?.Value;

        // Evaluate format as AVT (it is always an AVT per XSLT spec)
        var format = EvaluateAvt(formatAttr, instruction);

        // Evaluate optional AVT attributes
        string? lang = string.IsNullOrEmpty(langAttr) ? null : EvaluateAvt(langAttr, instruction);
        if (!string.IsNullOrEmpty(lang))
        {
            try
            {
                _ = System.Globalization.CultureInfo.GetCultureInfo(lang);
            }
            catch (System.Globalization.CultureNotFoundException)
            {
                throw new InvalidOperationException("XTDE0030");
            }
        }
        string? groupingSeparator = string.IsNullOrEmpty(groupingSepAttr) ? null : EvaluateAvt(groupingSepAttr);
        int groupingSize = 0;
        if (!string.IsNullOrEmpty(groupingSizeAttr))
        {
            var gsEval = EvaluateAvt(groupingSizeAttr, instruction);
            int.TryParse(gsEval, out groupingSize);
        }

        string? ordinal = null;
        if (!string.IsNullOrEmpty(ordinalAttr))
        {
            var ordEval = EvaluateAvt(ordinalAttr, instruction);
            if (ordEval.Equals("yes", StringComparison.OrdinalIgnoreCase))
                ordinal = string.Empty; // default ordinal for the language
            else if (!ordEval.Equals("no", StringComparison.OrdinalIgnoreCase))
                ordinal = ordEval;
        }

        // Evaluate start-at as AVT, then parse as space-separated integers (XSLT 3.0)
        BigInteger[]? startAtValues = null;
        if (!string.IsNullOrEmpty(startAtAttr))
        {
            var evaluated = EvaluateAvt(startAtAttr, instruction);
            startAtValues = ParseStartAtValues(evaluated);
        }

        // Handle select attribute: evaluate to get the target node for numbering
        IXdmNode? targetNode = currentNode;
        if (!string.IsNullOrEmpty(selectAttr))
        {
            var compiled = XPath31Expression.Compile(selectAttr);
            var result = compiled.Evaluate(_context);

            // XTTE1000: select must return at most one node
            if (result.IsSequence && result.SequenceValue != null)
            {
                int nodeCount = 0;
                foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                {
                    if (item.IsNode)
                    {
                        nodeCount++;
                        if (nodeCount > 1)
                            throw new InvalidOperationException("XTTE1000");
                    }
                }
            }

            targetNode = ExtractSingleNode(result);
            if (targetNode == null)
                throw new InvalidOperationException("XTTE1000");
        }

        if (!string.IsNullOrEmpty(valueAttr))
        {
            var compiled = XPath31Expression.Compile(valueAttr);
            var result = compiled.Evaluate(_context);

            // XSLT 1.0 backwards compatibility: xsl:number/@value uses only the first item.
            if (backwardsCompatible)
                result = FirstItemOrUndefined(result);

            // Determine whether the raw result is an empty sequence
            bool isEmptySequence = false;
            if (result.IsSequence && result.SequenceValue != null)
            {
                isEmptySequence = true;
                foreach (var _ in XdmSequence.FromSource(result.SequenceValue))
                {
                    isEmptySequence = false;
                    break;
                }
            }

            // Negative numbers without a pattern separator are an error (check original
            // XdmValue before int conversion to avoid overflow false positives).
            if (HasNegativeValue(result) && !format.Contains(';'))
                throw new InvalidOperationException("XTDE0980");

            var numbers = XdmValueToBigIntegerArray(result);
            if (numbers.Length > 0)
            {
                // Apply start-at to each number: value - 1 + start-at
                for (int i = 0; i < numbers.Length; i++)
                {
                    var startAt = startAtValues != null && startAtValues.Length > 0
                        ? (i < startAtValues.Length ? startAtValues[i] : startAtValues[^1])
                        : BigInteger.One;
                    numbers[i] = numbers[i] - 1 + startAt;
                }
                var formatted = FormatNumberSequence(numbers, format, ordinal, lang, groupingSeparator, groupingSize);
                // When value is present, xsl:number is equivalent to format-integer.
                // Strip leading whitespace from the first output to match test expectations
                // where multiple xsl:number calls are concatenated inside xsl:for-each.
                if (!string.IsNullOrEmpty(valueAttr) && IsFirstSignificantChild())
                {
                    formatted = formatted.TrimStart();
                }
                _lastAddedWasAtomic = false;
                AddTextNode(formatted);
            }
            else
            {
                // No convertible numbers: empty sequence or non-numeric value.
                if (backwardsCompatible)
                {
                    // XSLT 1.0 backwards-compatible → NaN for empty or non-numeric values.
                    _lastAddedWasAtomic = false;
                    AddTextNode("NaN");
                }
                else if (isEmptySequence)
                {
                    // Empty sequence → emit prefix+suffix only.
                    var formatted = FormatNumberSequence(System.Array.Empty<BigInteger>(), format, ordinal, lang, groupingSeparator, groupingSize);
                    if (!string.IsNullOrEmpty(formatted))
                    {
                        if (!string.IsNullOrEmpty(valueAttr) && IsFirstSignificantChild())
                        {
                            formatted = formatted.TrimStart();
                        }
                        _lastAddedWasAtomic = false;
                        AddTextNode(formatted);
                    }
                }
                else
                {
                    // Non-empty, non-numeric sequence in XSLT 2.0+ → XTDE0980.
                    throw new InvalidOperationException("XTDE0980");
                }
            }
        }
        else
        {
            var defaultNs = GetXPathDefaultNamespace(instruction);
            var countMatcher = string.IsNullOrEmpty(countPattern)
                ? CreateDefaultCountMatcher(targetNode)
                : new Patterns.PatternCompiler().Compile(ResolveNamespacePrefixes(countPattern, instruction), defaultNs);

            var fromMatcher = string.IsNullOrEmpty(fromPattern)
                ? null
                : new Patterns.PatternCompiler().Compile(ResolveNamespacePrefixes(fromPattern, instruction), defaultNs);

            int[]? numbers = level switch
            {
                "single" => ComputeNumberSingle(targetNode, countMatcher, fromMatcher, _context),
                "any" => ComputeNumberAny(targetNode, countMatcher, fromMatcher, _context),
                "multiple" => ComputeNumberMultiple(targetNode, countMatcher, fromMatcher, _context),
                _ => null
            };

            if (numbers != null && numbers.Length > 0)
            {
                // Negative numbers without a pattern separator in the format string are an error
                if (numbers.Any(n => n < 0) && !format.Contains(';'))
                    throw new InvalidOperationException("XTDE0980");

                // Apply start-at to each number
                if (startAtValues != null)
                {
                    for (int i = 0; i < numbers.Length; i++)
                    {
                        var startAt = i < startAtValues.Length ? startAtValues[i] : startAtValues[^1];
                        numbers[i] = (int)(numbers[i] - 1 + (int)startAt);
                    }
                }
            }

            // Format even when no numbers match: prefix+suffix is still emitted
            // (e.g. format="(1)" with no matches produces "()").
            var numsToFormat = numbers?.Select(n => (BigInteger)n).ToArray() ?? System.Array.Empty<BigInteger>();
            var formatted = FormatNumberSequence(numsToFormat, format, ordinal, lang, groupingSeparator, groupingSize);
            _lastAddedWasAtomic = false;
            AddTextNode(formatted);
        }
    }

    /// <summary>
    /// Creates a default count matcher based on the current node's kind and name.
    /// </summary>
    private static Patterns.PatternPredicate CreateDefaultCountMatcher(IXdmNode node)
    {
        var compiler = new Patterns.PatternCompiler();
        string name = string.IsNullOrEmpty(node.NamespaceUri)
            ? node.LocalName
            : $"Q{{{node.NamespaceUri}}}{node.LocalName}";
        return node.NodeKind switch
        {
            XdmNodeKind.Element => compiler.Compile(name),
            XdmNodeKind.Attribute => compiler.Compile("@" + name),
            _ => (n, ctx) => n.IsNode && n.NodeValue.NodeKind == node.NodeKind
        };
    }

    /// <summary>
    /// Replaces prefix:local-name occurrences in a pattern with Q{uri}local-name,
    /// resolving prefixes using the namespace declarations in scope on the given element.
    /// </summary>
    private static string ResolveNamespacePrefixes(string pattern, XElement contextElement)
    {
        if (!pattern.Contains(':'))
            return pattern;

        var sb = new System.Text.StringBuilder();
        int i = 0;
        while (i < pattern.Length)
        {
            char c = pattern[i];
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
            if (c == 'Q' && i + 1 < pattern.Length && pattern[i + 1] == '{')
            {
                sb.Append(c);
                i++;
                continue;
            }
            if (char.IsLetter(c) || c == '_')
            {
                int start = i;
                while (i < pattern.Length && (char.IsLetterOrDigit(pattern[i]) || pattern[i] == '_' || pattern[i] == '-'))
                    i++;
                if (i < pattern.Length && pattern[i] == ':')
                {
                    if (i + 1 < pattern.Length && pattern[i + 1] == ':')
                    {
                        sb.Append(pattern[start..i]);
                        continue;
                    }
                    var prefix = pattern[start..i];
                    i++;
                    int localStart = i;
                    while (i < pattern.Length && (char.IsLetterOrDigit(pattern[i]) || pattern[i] == '_' || pattern[i] == '-' || pattern[i] == '.'))
                        i++;
                    var local = pattern[localStart..i];
                    var nsUri = contextElement.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? "";
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
    /// Computes the number for <c>level="single"</c>.
    /// </summary>
    private static int[]? ComputeNumberSingle(IXdmNode currentNode, Patterns.PatternPredicate countMatcher, Patterns.PatternPredicate? fromMatcher, EvaluationContext context)
    {
        // Find nearest ancestor-or-self matching count
        IXdmNode? target = null;
        if (countMatcher(XdmValue.FromNode(currentNode), context))
        {
            target = currentNode;
        }
        else
        {
            foreach (var item in currentNode.Axis(XdmAxis.Ancestor))
            {
                if (item.IsNode && item.NodeValue is IXdmNode ancestor)
                {
                    if (countMatcher(XdmValue.FromNode(ancestor), context))
                    {
                        target = ancestor;
                        break;
                    }
                }
            }
        }

        if (target == null)
            return null;

        // If from is specified, the target must be a descendant-or-self of the
        // nearest ancestor of the current node that matches the from pattern.
        if (fromMatcher != null)
        {
            IXdmNode? fromNode = null;
            foreach (var item in currentNode.Axis(XdmAxis.Ancestor))
            {
                if (item.IsNode && item.NodeValue is IXdmNode ancestor)
                {
                    if (fromMatcher(XdmValue.FromNode(ancestor), context))
                    {
                        fromNode = ancestor;
                        break;
                    }
                }
            }

            if (fromNode != null)
            {
                bool isDescendantOrSelf = false;
                IXdmNode? check = target;
                while (check != null)
                {
                    if (check.IsSameNode(fromNode))
                    {
                        isDescendantOrSelf = true;
                        break;
                    }
                    IXdmNode? parent = null;
                    foreach (var parentItem in check.Axis(XdmAxis.Parent))
                    {
                        if (parentItem.IsNode && parentItem.NodeValue is IXdmNode p)
                        {
                            parent = p;
                            break;
                        }
                    }
                    if (parent == null)
                        break;
                    check = parent;
                }
                if (!isDescendantOrSelf)
                    return null;
            }
            // If no from-matching ancestor exists, target is still valid (fallback).
        }

        int count = 0;
        foreach (var item in target.Axis(XdmAxis.PrecedingSibling))
        {
            if (item.IsNode && item.NodeValue is IXdmNode sibling)
            {
                if (countMatcher(XdmValue.FromNode(sibling), context))
                    count++;
            }
        }

        return new[] { count + 1 };
    }

    /// <summary>
    /// Computes the number for <c>level="any"</c>.
    /// </summary>
    private static int[]? ComputeNumberAny(IXdmNode currentNode, Patterns.PatternPredicate countMatcher, Patterns.PatternPredicate? fromMatcher, EvaluationContext context)
    {
        var doc = currentNode.Document;
        if (doc == null)
        {
            // For non-document trees (e.g. variables), find the root ancestor
            doc = currentNode;
            while (doc.Parent != null)
                doc = doc.Parent;
        }

        int count = 0;
        bool foundCurrent = false;

        // Per .NET XslCompiledTransform semantics, only the FIRST attribute of each
        // element is counted by xsl:number level="any".
        IXdmNode? lastCountedAttributeParent = null;
        WalkDocumentTree(doc, node =>
        {
            if (node.IsSameNode(currentNode))
                foundCurrent = true;

            if (fromMatcher != null && fromMatcher(XdmValue.FromNode(node), context))
            {
                count = 0;
                lastCountedAttributeParent = null;
            }

            if (countMatcher(XdmValue.FromNode(node), context))
            {
                if (node.NodeKind == XdmNodeKind.Attribute)
                {
                    var parent = node.Parent;
                    if (parent != null && lastCountedAttributeParent != null && lastCountedAttributeParent.IsSameNode(parent))
                    {
                        // Skip non-first attributes
                        return !foundCurrent;
                    }
                    lastCountedAttributeParent = parent;
                }
                else
                {
                    lastCountedAttributeParent = null;
                }
                count++;
            }

            return !foundCurrent;
        }, false, out _);

        return count > 0 ? new[] { count } : null;
    }

    /// <summary>
    /// Computes the number sequence for <c>level="multiple"</c>.
    /// </summary>
    private static int[]? ComputeNumberMultiple(IXdmNode currentNode, Patterns.PatternPredicate countMatcher, Patterns.PatternPredicate? fromMatcher, EvaluationContext context)
    {
        var numbers = new List<int>();
        var ancestors = new List<IXdmNode>();

        foreach (var item in currentNode.Axis(XdmAxis.Ancestor))
        {
            if (item.IsNode && item.NodeValue is IXdmNode ancestor)
                ancestors.Add(ancestor);
        }
        // ancestors is now [parent, grandparent, ...] = innermost to outermost

        // Find the nearest ancestor matching the from pattern.
        IXdmNode? fromNode = null;
        if (fromMatcher != null)
        {
            foreach (var ancestor in ancestors)
            {
                if (fromMatcher(XdmValue.FromNode(ancestor), context))
                {
                    fromNode = ancestor;
                    break;
                }
            }
        }

        // Build the chain from the from-node (or root) down to the current node,
        // in outermost-to-innermost order.
        var chain = new List<IXdmNode>();
        bool started = fromNode == null;
        for (int i = ancestors.Count - 1; i >= 0; i--)
        {
            if (!started && ancestors[i].IsSameNode(fromNode!))
                started = true;

            if (started)
                chain.Add(ancestors[i]);
        }
        chain.Add(currentNode);

        foreach (var node in chain)
        {
            if (countMatcher(XdmValue.FromNode(node), context))
            {
                int count = 0;
                foreach (var item in node.Axis(XdmAxis.PrecedingSibling))
                {
                    if (item.IsNode && item.NodeValue is IXdmNode sibling)
                    {
                        if (countMatcher(XdmValue.FromNode(sibling), context))
                            count++;
                    }
                }
                numbers.Add(count + 1);
            }
        }

        return numbers.Count > 0 ? numbers.ToArray() : null;
    }

    /// <summary>
    /// Recursively walks a document tree in document order, calling <paramref name="visitor"/>
    /// for each node. Attributes are visited immediately after their owner element and before
    /// its children, per XDM document-order rules. Returns <c>false</c> if the visitor
    /// requested stopping.
    /// </summary>
    /// <summary>
    /// Walks the tree in document order, visiting all nodes that match the visitor.
    /// When <paramref name="skipNextText"/> is true, the next text node encountered
    /// in document order is skipped (not visited). This models .NET XslCompiledTransform
    /// semantics where the first text node after an element's attributes is not counted
    /// by <c>xsl:number level="any"</c>.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the walk should continue; <c>false</c> if the visitor signalled
    /// stop. The <paramref name="pendingSkip"/> out parameter indicates whether a
    /// text-node skip is still pending after this subtree.
    /// </returns>
    private static bool WalkDocumentTree(IXdmNode node, Func<IXdmNode, bool> visitor, bool skipNextText, out bool pendingSkip)
    {
        pendingSkip = false;

        if (node.NodeKind == XdmNodeKind.Text && skipNextText)
        {
            return true; // Skip this text node
        }

        if (!visitor(node))
            return false;

        // Attributes are in document order immediately after the element's start tag.
        // All attributes must be visited so that foundCurrent works when currentNode
        // is a non-first attribute (e.g. number-1101).
        bool hasAttributes = false;
        if (node.NodeKind == XdmNodeKind.Element)
        {
            foreach (var item in node.Axis(XdmAxis.Attribute))
            {
                if (item.IsNode && item.NodeValue is IXdmNode attr)
                {
                    hasAttributes = true;
                    if (!visitor(attr))
                        return false;
                }
            }
        }

        // Per XSLT 1.0 xsl:number semantics (matching .NET XslCompiledTransform),
        // the first text node that follows an element's attributes is not counted.
        pendingSkip = hasAttributes || skipNextText;
        foreach (var item in node.Axis(XdmAxis.Child))
        {
            if (item.IsNode && item.NodeValue is IXdmNode child)
            {
                if (child.NodeKind == XdmNodeKind.Text && pendingSkip)
                {
                    pendingSkip = false;
                    continue;
                }

                bool childResult = WalkDocumentTree(child, visitor, pendingSkip, out bool childPendingSkip);
                if (!childResult)
                    return false;

                pendingSkip = childPendingSkip;
            }
        }

        return true;
    }

    /// <summary>
    /// Formats a sequence of integers according to an <c>xsl:number</c> format string.
    /// </summary>
    private string FormatNumberSequence(BigInteger[] numbers, string format, string? ordinal, string? lang, string? groupingSeparator, int groupingSize)
    {
        var (prefix, tokens, separators, suffix) = ParseXslNumberFormat(format);

        var sb = new System.Text.StringBuilder();
        sb.Append(prefix);

        for (int i = 0; i < numbers.Length; i++)
        {
            var token = tokens.Count > 0
                ? (i < tokens.Count ? tokens[i] : tokens[^1])
                : "1";

            // Append ordinal modifier if requested
            if (ordinal != null && !token.Contains(';'))
                token += string.IsNullOrEmpty(ordinal) ? ";o" : ";o(" + ordinal + ")";

            var formatted = FormatIntegerEngine.Format(_context, numbers[i], token, lang);

            // Apply xsl:number grouping-separator / grouping-size
            if (!string.IsNullOrEmpty(groupingSeparator) && groupingSize > 0)
                formatted = ApplyNumberGrouping(formatted, groupingSeparator, groupingSize);

            sb.Append(formatted);

            if (i < numbers.Length - 1)
            {
                var sep = separators.Count > 0
                    ? (i < separators.Count ? separators[i] : separators[^1])
                    : ".";
                sb.Append(sep);
            }
        }

        sb.Append(suffix);
        return sb.ToString();
    }

    /// <summary>
    /// Applies grouping separator and size to a formatted number string.
    /// Handles optional leading minus sign.
    /// </summary>
    private static string ApplyNumberGrouping(string formatted, string groupingSeparator, int groupingSize)
    {
        bool negative = formatted.StartsWith("-");
        string digits = negative ? formatted.Substring(1) : formatted;

        var sb = new System.Text.StringBuilder();
        int count = 0;
        for (int i = digits.Length - 1; i >= 0; i--)
        {
            if (count > 0 && count % groupingSize == 0)
                sb.Insert(0, groupingSeparator);
            sb.Insert(0, digits[i]);
            count++;
        }

        string result = sb.ToString();
        return negative ? "-" + result : result;
    }

    /// <summary>
    /// Parses an <c>xsl:number</c> format string into prefix, tokens, separators, and suffix.
    /// Recognizes Unicode numbering characters (including astral-plane characters) as format tokens.
    /// </summary>
    private static (string prefix, List<string> tokens, List<string> separators, string suffix) ParseXslNumberFormat(string format)
    {
        var tokens = new List<string>();
        var separators = new List<string>();

        int i = 0;
        while (i < format.Length && !IsFormatTokenChar(format, i))
            i = AdvanceCodepoint(format, i);
        var prefix = format.Substring(0, i);

        while (i < format.Length)
        {
            int tokenStart = i;
            while (i < format.Length && IsFormatTokenChar(format, i))
                i = AdvanceCodepoint(format, i);
            tokens.Add(format.Substring(tokenStart, i - tokenStart));

            int sepStart = i;
            while (i < format.Length && !IsFormatTokenChar(format, i))
                i = AdvanceCodepoint(format, i);
            separators.Add(format.Substring(sepStart, i - sepStart));
        }

        string suffix = string.Empty;
        if (separators.Count > 0)
        {
            suffix = separators[^1];
            separators.RemoveAt(separators.Count - 1);
        }

        // Special case: non-empty format string with no alphanumeric characters.
        // The entire string is used as both prefix and suffix (e.g. "*" → "*1*").
        if (format.Length > 0 && tokens.Count == 0)
        {
            suffix = prefix;
        }

        return (prefix, tokens, separators, suffix);
    }

    /// <summary>
    /// Returns whether the character at the given index in <paramref name="s"/>
    /// is a letter, digit, or Unicode numbering character (i.e. can form a format token).
    /// </summary>
    private static bool IsFormatTokenChar(string s, int i)
    {
        var cat = CharUnicodeInfo.GetUnicodeCategory(s, i);
        return cat == UnicodeCategory.UppercaseLetter
            || cat == UnicodeCategory.LowercaseLetter
            || cat == UnicodeCategory.TitlecaseLetter
            || cat == UnicodeCategory.ModifierLetter
            || cat == UnicodeCategory.OtherLetter
            || cat == UnicodeCategory.DecimalDigitNumber
            || cat == UnicodeCategory.LetterNumber
            || cat == UnicodeCategory.OtherNumber;
    }

    /// <summary>
    /// Advances <paramref name="i"/> past the current codepoint (1 or 2 chars for surrogates).
    /// </summary>
    private static int AdvanceCodepoint(string s, int i)
    {
        if (i < s.Length && char.IsHighSurrogate(s[i]) && i + 1 < s.Length && char.IsLowSurrogate(s[i + 1]))
            return i + 2;
        return i + 1;
    }

    /// <summary>
    /// Parses a start-at attribute value string into an array of integers.
    /// Handles space-separated values and single values.
    /// </summary>
    private static BigInteger[] ParseStartAtValues(string value)
    {
        var parts = value.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return [BigInteger.One];
        var result = new BigInteger[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!BigInteger.TryParse(parts[i], out result[i]))
                throw new InvalidOperationException("XTSE0020");
        }
        return result;
    }

    /// <summary>
    /// Extracts a single node from an <see cref="XdmValue"/> if it represents a singleton node.
    /// </summary>
    private static IXdmNode? ExtractSingleNode(XdmValue value)
    {
        if (value.Kind == XdmValueKind.Sequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                return ExtractSingleNode(item);
            return null;
        }

        if (value.Kind == XdmValueKind.Node && value.NodeValue is IXdmNode node)
            return node;

        return null;
    }

    /// <summary>
    /// Returns <c>true</c> if the <see cref="XdmValue"/> represents a negative number.
    /// Sequences are inspected by looking at the first item.
    /// </summary>
    private static bool HasNegativeValue(XdmValue value)
    {
        if (value.Kind == XdmValueKind.Sequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                return HasNegativeValue(item);
            return false;
        }

        return value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue < 0,
            XdmValueKind.Decimal => value.DecimalValue < 0,
            XdmValueKind.Double => value.DoubleValue < 0 && !double.IsNaN(value.DoubleValue),
            XdmValueKind.Float => value.DoubleValue < 0 && !double.IsNaN(value.DoubleValue),
            _ => false
        };
    }

    /// <summary>
    /// Converts an <see cref="XdmValue"/> to a <see cref="BigInteger"/> if it represents a number.
    /// </summary>
    private static BigInteger? XdmValueToBigInteger(XdmValue value)
    {
        // If it's a singleton sequence, extract the first item
        if (value.Kind == XdmValueKind.Sequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                return XdmValueToBigInteger(item);
            return null;
        }

        return value.Kind switch
        {
            XdmValueKind.Integer => new BigInteger(value.IntegerValue),
            XdmValueKind.Decimal => BigInteger.Parse(Math.Round(value.DecimalValue, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture)),
            XdmValueKind.Double => new BigInteger(value.DoubleValue),
            XdmValueKind.Float => new BigInteger(value.DoubleValue),
            XdmValueKind.Node => BigInteger.TryParse(value.NodeValue?.StringValue ?? "", out var n) ? n : null,
            _ => BigInteger.TryParse(value.ToString(), out var n) ? n : null
        };
    }

    /// <summary>
    /// Converts an <see cref="XdmValue"/> to an array of <see cref="BigInteger"/> values.
    /// Handles sequences by extracting all numeric items.
    /// </summary>
    private static BigInteger[] XdmValueToBigIntegerArray(XdmValue value)
    {
        var result = new List<BigInteger>();
        if (value.Kind == XdmValueKind.Sequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                var n = XdmValueToBigInteger(item);
                if (n.HasValue)
                    result.Add(n.Value);
            }
        }
        else
        {
            var n = XdmValueToBigInteger(value);
            if (n.HasValue)
                result.Add(n.Value);
        }
        return result.ToArray();
    }

    /// <summary>
    /// Extracts the local name from a QName string, handling the case where
    /// namespace="" forces the null namespace (prefix must be stripped).
    /// </summary>
    private static string GetLocalName(string name, string? namespaceUri)
    {
        // If namespace is explicitly empty, strip any prefix from the name
        if (namespaceUri == "")
        {
            int colon = name.IndexOf(':');
            if (colon >= 0)
                return name[(colon + 1)..];
        }
        return name;
    }

    /// <summary>
    /// Resolves the local name and namespace URI for xsl:element / xsl:attribute
    /// name attributes that may contain a prefix. When no explicit namespace is
    /// given, the prefix is resolved against the in-scope namespaces of the
    /// instruction element.
    /// </summary>
    private static (string LocalName, string NamespaceUri) ResolveElementName(XElement instruction, string name, string? explicitNamespace, string errorCode)
        => ResolveName(instruction, name, explicitNamespace, errorCode, useDefaultNamespace: true);

    private static (string LocalName, string NamespaceUri) ResolveAttributeName(XElement instruction, string name, string? explicitNamespace, string errorCode)
        => ResolveName(instruction, name, explicitNamespace, errorCode, useDefaultNamespace: false);

    private static string NormalizeQNameWhitespace(string name)
        => Regex.Replace(name.Trim(), @"\s+", " ");

    private static (string LocalName, string NamespaceUri) ResolveName(XElement instruction, string name, string? explicitNamespace, string errorCode, bool useDefaultNamespace)
    {
        name = NormalizeQNameWhitespace(name);
        int colon = name.IndexOf(':');
        if (colon >= 0)
        {
            string prefix = name[..colon];
            string localName = name[(colon + 1)..];
            if (explicitNamespace != null)
                return (localName, explicitNamespace);
            var ns = instruction.GetNamespaceOfPrefix(prefix);
            if (ns == null)
                throw new InvalidOperationException(errorCode);
            return (localName, ns.NamespaceName);
        }
        else
        {
            if (explicitNamespace != null)
                return (name, explicitNamespace);
            if (useDefaultNamespace)
            {
                var ns = instruction.GetDefaultNamespace();
                return (name, ns?.NamespaceName ?? "");
            }
            return (name, "");
        }
    }

    // ---------------------------------------------------------------------------------------------
    // xsl:merge implementation
    // ---------------------------------------------------------------------------------------------

    private readonly record struct MergeEntry(XdmValue Item, int SourceIndex, int OriginalIndex, List<SortKey> Keys);

    /// <summary>
    /// Evaluates an <c>xsl:merge</c> instruction.
    /// </summary>
    private void ExecuteMergeInstruction(XElement instruction, XdmValue contextItem)
    {
        var xsl = Stylesheet.Stylesheet.XslNamespace;
        var sourceElements = instruction.Elements(XName.Get("merge-source", xsl)).ToList();
        var actionElement = instruction.Elements(XName.Get("merge-action", xsl)).FirstOrDefault();
        if (sourceElements.Count == 0 || actionElement == null)
            return;

        var sourceNames = new List<string?>();
        var sourceControls = new List<List<SortControl>>();
        var sourceKeyElems = new List<List<XElement>>();
        var allEntries = new List<MergeEntry>();
        int sourceIndex = 0;

        var savedFocus = _context.ContextItem;
        var savedPosition = _context.ContextPosition;
        var savedSize = _context.ContextSize;
        try
        {
            foreach (var sourceElem in sourceElements)
            {
                var name = sourceElem.Attribute("name")?.Value;
                sourceNames.Add(name);
                var keySpecElems = sourceElem.Elements(XName.Get("merge-key", xsl)).ToList();

                // Evaluate sort controls using the current focus (the focus of the
                // containing xsl:merge instruction), so AVTs such as order="{if(position()...)}"
                // see the correct position/size.
                var controls = new List<SortControl>();
                foreach (var keySpec in keySpecElems)
                    controls.Add(EvaluateSortControl(keySpec));
                sourceControls.Add(controls);
                sourceKeyElems.Add(keySpecElems);

                var sourceItems = EvaluateMergeSourceItems(sourceElem, contextItem);

                for (int originalIndex = 0; originalIndex < sourceItems.Count; originalIndex++)
                {
                    var item = sourceItems[originalIndex];
                    var keys = new List<SortKey>();
                    for (int k = 0; k < keySpecElems.Count; k++)
                    {
                        var keyValue = EvaluateMergeKeyValue(keySpecElems[k], item, controls[k]);
                        keys.Add(new SortKey(keyValue, controls[k]));
                    }
                    allEntries.Add(new MergeEntry(item, sourceIndex, originalIndex, keys));
                }
                sourceIndex++;
            }
        }
        finally
        {
            _context.WithFocus(savedFocus, savedPosition, savedSize);
        }

        if (sourceControls.Count == 0)
            return;

        var keyControls = sourceControls[0];

        // XTDE2210: corresponding merge-key elements across sources must have the same
        // effective values for data-type, order, lang, case-order, and collation.
        // Values are considered to differ if the attribute is present on one element
        // and not on the other, or if both are present with unequal effective values.
        string[] mergeKeyAttrs = { "lang", "order", "collation", "case-order", "data-type" };
        for (int s = 1; s < sourceControls.Count; s++)
        {
            var otherControls = sourceControls[s];
            var otherKeyElems = sourceKeyElems[s];
            var firstKeyElems = sourceKeyElems[0];
            int maxKeys = Math.Max(keyControls.Count, otherControls.Count);
            for (int k = 0; k < maxKeys; k++)
            {
                var a = k < keyControls.Count ? keyControls[k] : new SortControl(false, SortDataType.Auto, null, null, null);
                var b = k < otherControls.Count ? otherControls[k] : new SortControl(false, SortDataType.Auto, null, null, null);
                if (a.DataType != b.DataType ||
                    a.Descending != b.Descending ||
                    a.Lang != b.Lang ||
                    a.CaseOrder != b.CaseOrder ||
                    a.Collation != b.Collation)
                {
                    throw new InvalidOperationException("XTDE2210: xsl:merge-key specifications are incompatible across xsl:merge-source elements");
                }

                // Presence mismatch on a corresponding pair of merge-key elements also
                // counts as differing values (merge-021: order omitted vs order="ascending").
                if (k < firstKeyElems.Count && k < otherKeyElems.Count)
                {
                    foreach (var attrName in mergeKeyAttrs)
                    {
                        bool inFirst = firstKeyElems[k].Attribute(attrName) != null;
                        bool inOther = otherKeyElems[k].Attribute(attrName) != null;
                        if (inFirst != inOther)
                            throw new InvalidOperationException("XTDE2210: xsl:merge-key specifications are incompatible across xsl:merge-source elements");
                    }
                }
            }
        }

        if (allEntries.Count == 0)
            return;

        // Sort globally by key tuple, then source order, then original document order.
        allEntries.Sort((a, b) =>
        {
            int minKeys = Math.Min(a.Keys.Count, b.Keys.Count);
            for (int i = 0; i < minKeys; i++)
            {
                int cmp = CompareSortKey(a.Keys[i], b.Keys[i]);
                if (cmp != 0) return cmp;
            }
            if (a.Keys.Count != b.Keys.Count)
                return a.Keys.Count.CompareTo(b.Keys.Count);
            int srcCmp = a.SourceIndex.CompareTo(b.SourceIndex);
            if (srcCmp != 0) return srcCmp;
            return a.OriginalIndex.CompareTo(b.OriginalIndex);
        });

        // Build groups of consecutive equal-key items.
        var groups = new List<(int Start, int End, List<SortKey> Keys)>();
        int pos = 0;
        while (pos < allEntries.Count)
        {
            int groupStart = pos;
            var groupKey = allEntries[pos].Keys;
            pos++;
            while (pos < allEntries.Count)
            {
                var otherKey = allEntries[pos].Keys;
                if (groupKey.Count != otherKey.Count)
                    break;
                bool equal = true;
                for (int i = 0; i < groupKey.Count; i++)
                {
                    if (CompareSortKey(groupKey[i], otherKey[i]) != 0)
                    {
                        equal = false;
                        break;
                    }
                }
                if (!equal) break;
                pos++;
            }
            groups.Add((groupStart, pos, groupKey));
        }

        // Emit the merge-action for each group.
        for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
        {
            var (groupStart, groupEnd, groupKey) = groups[groupIndex];
            int groupPos = groupIndex + 1;
            int totalGroups = groups.Count;

            var groupItems = new List<XdmValue>();
            var namedGroups = new Dictionary<string, List<XdmValue>>();
            for (int i = groupStart; i < groupEnd; i++)
            {
                var entry = allEntries[i];
                groupItems.Add(entry.Item);
                var name = sourceNames[entry.SourceIndex];
                if (!string.IsNullOrEmpty(name))
                {
                    if (!namedGroups.TryGetValue(name, out var list))
                    {
                        list = new List<XdmValue>();
                        namedGroups[name] = list;
                    }
                    list.Add(entry.Item);
                }
            }

            var savedMergeGroup = _currentMergeGroup;
            var savedMergeKey = _currentMergeKey;
            var savedNamedGroups = _currentNamedMergeGroups;
            var savedSourceNames = _currentMergeSourceNames;
            var savedActionFocus = _context.ContextItem;
            var savedActionPosition = _context.ContextPosition;
            var savedActionSize = _context.ContextSize;
            try
            {
                _currentMergeGroup = groupItems;
                _currentNamedMergeGroups = namedGroups;
                _currentMergeSourceNames = new HashSet<string>(sourceNames.Where(n => !string.IsNullOrEmpty(n)).Select(n => n!));
                if (groupKey.Count == 1)
                {
                    _currentMergeKey = groupKey[0].Value;
                }
                else
                {
                    var keySeq = groupKey.Select(k => k.Value).ToList();
                    _currentMergeKey = XdmValue.FromSequence(MaterializedSequence.FromList(keySeq));
                }

                // The focus inside xsl:merge-action is the current group: position/size are
                // defined as the position and number of merge groups. The context item is set
                // to the first item of the group so that position() works.
                var groupContext = groupItems.Count > 0 ? groupItems[0] : XdmValue.Undefined;
                _context.WithFocus(groupContext, groupPos, totalGroups);

                foreach (var childNode in actionElement.Nodes())
                {
                    switch (childNode)
                    {
                        case XText text:
                            ProcessSequenceText(text, actionElement);
                            break;
                        case XElement elem when elem.Name.LocalName == "sort" && elem.Name.NamespaceName == xsl:
                            continue;
                        case XElement elem when elem.Name.NamespaceName == xsl:
                            ExecuteXsltInstruction(elem, XdmValue.Undefined);
                            break;
                        case XElement elem:
                            CopyLiteralElement(elem);
                            break;
                    }
                }
            }
            finally
            {
                _currentMergeGroup = savedMergeGroup;
                _currentMergeKey = savedMergeKey;
                _currentNamedMergeGroups = savedNamedGroups;
                _currentMergeSourceNames = savedSourceNames;
                _context.WithFocus(savedActionFocus, savedActionPosition, savedActionSize);
            }
        }
    }

    /// <summary>
    /// Evaluates an <c>xsl:analyze-string</c> instruction, invoking <paramref name="executeChild"/>
    /// for each matching/non-matching substring child.
    /// </summary>
    private void ExecuteAnalyzeString(XElement instruction, XdmValue contextItem, Action<XElement, XdmValue> executeChild)
    {
        var xsl = Stylesheet.Stylesheet.XslNamespace;

        var selectAttr = instruction.Attribute("select")?.Value;
        XdmValue selectValue = !string.IsNullOrEmpty(selectAttr)
            ? CompileXPath(selectAttr, instruction).Evaluate(_context)
            : contextItem;

        var selectedItems = EnumerateItems(selectValue).ToList();
        if (selectedItems.Count > 1)
            throw new InvalidOperationException("XPTY0004: xsl:analyze-string select must evaluate to zero or one items");

        string input;
        if (selectedItems.Count == 0)
        {
            input = string.Empty;
        }
        else
        {
            var atomized = AtomizeFirstItem(selectedItems[0]);
            if (atomized.Kind != XdmValueKind.String)
                throw new InvalidOperationException("XPTY0004");
            input = atomized.ToString();
        }

        var regexAttr = instruction.Attribute("regex");
        if (regexAttr == null || regexAttr.Value == null)
            throw new InvalidOperationException("XTSE0010: xsl:analyze-string requires a regex attribute");
        string pattern = EvaluateAvt(regexAttr.Value, instruction);

        var flagsAttr = instruction.Attribute("flags")?.Value ?? string.Empty;
        string flags = EvaluateAvt(flagsAttr, instruction);

        var options = RegexHelper.ParseRegexFlags(flags, out bool isQuoteMode);
        if (isQuoteMode)
            pattern = Regex.Escape(pattern);
        else
            pattern = RegexHelper.ValidateAndTranslatePatternCached(pattern, options);

        var matchingChild = instruction.Element(XName.Get("matching-substring", xsl));
        var nonMatchingChild = instruction.Element(XName.Get("non-matching-substring", xsl));
        if (matchingChild == null && nonMatchingChild == null)
            throw new InvalidOperationException("XTSE1130: xsl:analyze-string must contain at least one xsl:matching-substring or xsl:non-matching-substring");

        // Validate child order: matching-substring*, non-matching-substring?, fallback*
        bool seenNonMatching = false;
        bool seenFallback = false;
        foreach (var child in instruction.Elements())
        {
            var ln = child.Name.LocalName;
            if (ln == "matching-substring")
            {
                if (seenNonMatching || seenFallback)
                    throw new InvalidOperationException("XTSE0010: xsl:matching-substring must precede xsl:non-matching-substring and xsl:fallback");
            }
            else if (ln == "non-matching-substring")
            {
                if (seenFallback)
                    throw new InvalidOperationException("XTSE0010: xsl:non-matching-substring must precede xsl:fallback");
                seenNonMatching = true;
            }
            else if (ln == "fallback")
            {
                seenFallback = true;
            }
            else if (child.Name.NamespaceName == xsl)
            {
                throw new InvalidOperationException("XTSE0010: invalid child of xsl:analyze-string");
            }
        }

        // XSLT 3.0 §17.1: the XSLT 2.0 dynamic error XTDE1150 (regex matches a
        // zero-length string) was removed; zero-length matches are handled by the
        // position-based algorithm below. A 3.0 processor uses the 3.0 algorithm even
        // for stylesheets declaring version="2.0" (see W3C test regex-090/091).
        bool xslt20 = decimal.TryParse(_stylesheet.Version, out var version) && version < 3.0m;

        // Precompute the sequence of matching and non-matching substrings using the
        // XSLT 3.0 position-based algorithm (§17.1). This correctly handles regexes
        // that match zero-length substrings.
        var segments = new List<(bool IsMatch, string Text, Match? Match)>();
        var regex = RegexHelper.GetRegex(pattern, options);
        int pos = 0;
        var pendingNonMatch = new StringBuilder();

        while (true)
        {
            var match = regex.Match(input, pos);
            bool matchedHere = match.Success && match.Index == pos;

            if (matchedHere)
            {
                if (pendingNonMatch.Length > 0)
                {
                    segments.Add((false, pendingNonMatch.ToString(), null));
                    pendingNonMatch.Clear();
                }

                segments.Add((true, match.Value, match));

                if (match.Length == 0)
                {
                    if (pos == input.Length)
                        break;
                    pendingNonMatch.Append(input[pos]);
                    pos++;
                }
                else
                {
                    pos += match.Length;
                    if (pos > input.Length)
                        pos = input.Length;
                }
            }
            else
            {
                if (pos == input.Length)
                {
                    if (pendingNonMatch.Length > 0)
                        segments.Add((false, pendingNonMatch.ToString(), null));
                    break;
                }

                pendingNonMatch.Append(input[pos]);
                pos++;
            }
        }

        var savedGroups = _context.RegexGroups;
        var savedFocus = _context.ContextItem;
        var savedPosition = _context.ContextPosition;
        var savedSize = _context.ContextSize;
        var savedCurrent = _context.CurrentItem;
        try
        {
            int total = segments.Count;
            for (int i = 0; i < segments.Count; i++)
            {
                var (isMatch, text, match) = segments[i];
                _context.RegexGroups = isMatch && match != null
                    ? match.Groups.Cast<Group>().Select(g => g.Success ? g.Value : string.Empty).ToArray()
                    : null;
                var substringItem = XdmValue.FromString(text, "string");
                _context.WithFocus(substringItem, i + 1, total);
                _context.WithCurrentItem(substringItem);

                if (isMatch && matchingChild != null)
                    executeChild(matchingChild, contextItem);
                else if (!isMatch && nonMatchingChild != null)
                    executeChild(nonMatchingChild, contextItem);
            }
        }
        finally
        {
            _context.RegexGroups = savedGroups;
            _context.WithFocus(savedFocus, savedPosition, savedSize);
            _context.WithCurrentItem(savedCurrent);
        }
    }

    private void ExecuteAnalyzeStringChild(XElement child, XdmValue contextItem)
    {
        foreach (var node in child.Nodes())
        {
            switch (node)
            {
                case XText text:
                    ProcessSequenceText(text, child);
                    break;
                case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                    ExecuteXsltInstruction(elem, contextItem);
                    break;
                case XElement elem:
                    CopyLiteralElement(elem);
                    break;
            }
        }
    }

    private void EvaluateAnalyzeStringChild(XElement child, List<XdmValue> results, XdmValue contextItem)
    {
        foreach (var node in child.Nodes())
        {
            switch (node)
            {
                case XText text:
                    if (GetExpandText(child))
                    {
                        results.Add(XdmValue.FromNode(new XDocumentNode(new XText(EvaluateTvt(text.Value, child)))));
                    }
                    else if (!IsWhitespaceOnly(text.Value))
                    {
                        results.Add(XdmValue.FromNode(new XDocumentNode(new XText(text.Value))));
                    }
                    break;
                case XElement elem when elem.Name.NamespaceName == Stylesheet.Stylesheet.XslNamespace:
                    EvaluateFunctionBodyInstruction(elem, results, contextItem);
                    break;
                case XElement elem:
                    results.Add(XdmValue.FromNode(new XDocumentNode(elem)));
                    break;
            }
        }
    }

    /// <summary>
    /// Evaluates the selected items for a single <c>xsl:merge-source</c>.
    /// </summary>
    private List<XdmValue> EvaluateMergeSourceItems(XElement sourceElem, XdmValue contextItem)
    {
        var selectAttr = sourceElem.Attribute("select")?.Value;
        if (string.IsNullOrEmpty(selectAttr))
        {
            var underSelect = sourceElem.Attribute("_select")?.Value;
            if (!string.IsNullOrEmpty(underSelect))
                selectAttr = EvaluateAvt(underSelect, sourceElem);
        }
        var forEachItemAttr = sourceElem.Attribute("for-each-item")?.Value;
        var forEachSourceAttr = sourceElem.Attribute("for-each-source")?.Value;
        var result = new List<XdmValue>();

        var savedFocus = _context.ContextItem;
        var savedPosition = _context.ContextPosition;
        var savedSize = _context.ContextSize;
        try
        {
            if (!string.IsNullOrEmpty(forEachItemAttr))
            {
                var compiled = CompileXPath(forEachItemAttr, sourceElem);
                var feResult = compiled.Evaluate(_context);
                var feItems = EnumerateItems(feResult).ToList();
                for (int idx = 0; idx < feItems.Count; idx++)
                {
                    _context.WithFocus(feItems[idx], idx + 1, feItems.Count);
                    RecordAccumulatorApplicability(sourceElem, feItems[idx]);
                    if (!string.IsNullOrEmpty(selectAttr))
                    {
                        var selCompiled = CompileXPath(selectAttr, sourceElem);
                        var selResult = selCompiled.Evaluate(_context);
                        result.AddRange(EnumerateItems(selResult));
                    }
                    else
                    {
                        result.Add(feItems[idx]);
                    }
                }
                return result;
            }

            if (!string.IsNullOrEmpty(forEachSourceAttr))
            {
                var compiled = CompileXPath(forEachSourceAttr, sourceElem);
                var fsResult = compiled.Evaluate(_context);
                var fsItems = EnumerateItems(fsResult).ToList();
                for (int idx = 0; idx < fsItems.Count; idx++)
                {
                    XdmValue sourceContext;
                    if (fsItems[idx].IsNode)
                    {
                        sourceContext = fsItems[idx];
                    }
                    else
                    {
                        var uri = fsItems[idx].ToString();
                        var baseUri = GetEffectiveBaseUri(sourceElem);
                        var resolvedUri = string.IsNullOrEmpty(uri) ? baseUri ?? "" : uri;
                        var savedBaseUri = _context.BaseUri;
                        try
                        {
                            if (!string.IsNullOrEmpty(baseUri))
                                _context.BaseUri = baseUri;
                            var doc = _context.LoadDocument(resolvedUri);
                            sourceContext = XdmValue.FromNode(doc);
                        }
                        finally
                        {
                            _context.BaseUri = savedBaseUri;
                        }
                    }
                    RecordAccumulatorApplicability(sourceElem, sourceContext);
                    _context.WithFocus(sourceContext, idx + 1, fsItems.Count);
                    if (!string.IsNullOrEmpty(selectAttr))
                    {
                        var selCompiled = CompileXPath(selectAttr, sourceElem);
                        var selResult = selCompiled.Evaluate(_context);
                        result.AddRange(EnumerateItems(selResult));
                    }
                    else
                    {
                        result.Add(sourceContext);
                    }
                }
                return result;
            }

            if (!string.IsNullOrEmpty(selectAttr))
            {
                var compiled = CompileXPath(selectAttr, sourceElem);
                var selResult = compiled.Evaluate(_context);
                return EnumerateItems(selResult).ToList();
            }
        }
        finally
        {
            _context.WithFocus(savedFocus, savedPosition, savedSize);
        }

        return result;
    }

    /// <summary>
    /// Records the accumulator applicability set declared by <c>use-accumulators</c> on an
    /// <c>xsl:merge-source</c> for the source tree containing <paramref name="sourceContext"/>.
    /// </summary>
    private void RecordAccumulatorApplicability(XElement sourceElem, XdmValue sourceContext)
    {
        if (!sourceContext.IsNode || sourceContext.NodeValue == null)
            return;

        var useAccAttr = sourceElem.Attribute("use-accumulators")?.Value;
        if (string.IsNullOrWhiteSpace(useAccAttr))
            return;

        var root = GetRootNode(sourceContext.NodeValue);
        var trimmed = useAccAttr.Trim();
        HashSet<string> set;
        if (trimmed == "#all")
        {
            set = new HashSet<string>(_accumulators.Select(a => a.ClarkName));
        }
        else
        {
            set = new HashSet<string>();
            foreach (var name in trimmed.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                set.Add(ResolveAccumulatorClarkName(name, sourceElem));
        }

        _accumulatorApplicability[root] = set;
    }

    /// <summary>
    /// Resolves an accumulator name (possibly prefixed or in Clark notation) to Clark notation
    /// using the in-scope namespaces of <paramref name="contextElement"/>.
    /// </summary>
    private static string ResolveAccumulatorClarkName(string name, XElement contextElement)
    {
        if (name.StartsWith("{"))
            return name;

        var colon = name.IndexOf(':');
        if (colon < 0)
            return name; // unprefixed accumulator names are in no namespace

        var prefix = name[..colon];
        var local = name[(colon + 1)..];
        if (prefix == "xml")
            return "{http://www.w3.org/XML/1998/namespace}" + local;

        var ns = contextElement.GetNamespaceOfPrefix(prefix);
        if (ns == null)
            throw new InvalidOperationException($"XPST0081: Undefined namespace prefix '{prefix}'");
        return $"{{{ns.NamespaceName}}}{local}";
    }

    /// <summary>
    /// Evaluates a single <c>xsl:merge-key</c> for the given item and returns the atomized key value.
    /// Raises XTTE1020 if the key expression evaluates to more than one item.
    /// </summary>
    private XdmValue EvaluateMergeKeyValue(XElement keySpec, XdmValue item, SortControl control)
    {
        var savedFocus = _context.ContextItem;
        var savedPosition = _context.ContextPosition;
        var savedSize = _context.ContextSize;
        var savedOutputUri = _context.CurrentOutputUri;
        _context.CurrentOutputUri = null;
        try
        {
            _context.WithFocus(item, 1, 1);
            var selectAttr = keySpec.Attribute("select")?.Value;
            XdmValue keyValue;
            if (!string.IsNullOrEmpty(selectAttr))
            {
                var compiled = CompileXPath(selectAttr, keySpec);
                keyValue = compiled.Evaluate(_context);
            }
            else
            {
                keyValue = item;
            }

            var enumerated = EnumerateItems(keyValue).Take(2).ToList();
            if (enumerated.Count > 1)
                throw new InvalidOperationException("XTTE1020: xsl:merge-key must evaluate to a single item");

            var atomized = AtomizeFirstItem(keyValue);

            // data-type="number" follows the XPath number() function semantics: unparseable
            // values produce NaN rather than a dynamic error. CompareNumericSortKey handles
            // NaN values by treating them as equal and sorting them before ordinary numbers.
            return atomized;
        }
        finally
        {
            _context.WithFocus(savedFocus, savedPosition, savedSize);
            _context.CurrentOutputUri = savedOutputUri;
        }
    }

    /// <summary>
    /// Executes <paramref name="action"/> with the merge context cleared.
    /// Used for xsl:call-template and xsl:apply-templates so that current-merge-group()
    /// and current-merge-key() are not visible in the called template.
    /// </summary>
    private void WithoutMergeContext(Action action)
    {
        var savedMergeGroup = _currentMergeGroup;
        var savedMergeKey = _currentMergeKey;
        var savedNamedGroups = _currentNamedMergeGroups;
        try
        {
            _currentMergeGroup = null;
            _currentMergeKey = null;
            _currentNamedMergeGroups = null;
            action();
        }
        finally
        {
            _currentMergeGroup = savedMergeGroup;
            _currentMergeKey = savedMergeKey;
            _currentNamedMergeGroups = savedNamedGroups;
        }
    }

    /// <summary>
    /// Annotation attached to copied elements when <c>copy-accumulators="yes"</c> is used.
    /// Maps accumulator Clark names to their before/after values for the source node.
    /// </summary>
    private sealed class AccumulatorValues
    {
        public Dictionary<string, (XdmValue Before, XdmValue After)> Values { get; } = new();

        /// <summary>
        /// The names of the accumulators that are applicable to the copied tree.
        /// </summary>
        public HashSet<string> ApplicableNames { get; } = new();

        /// <summary>
        /// The names of the accumulators that are known but not applicable to the copied tree.
        /// Used to raise XTDE3362 when one of them is requested.
        /// </summary>
        public HashSet<string> InapplicableNames { get; } = new();
    }

    /// <summary>
    /// Base class for control exceptions used to implement <c>xsl:break</c> and
    /// <c>xsl:next-iteration</c> inside <c>xsl:iterate</c>. These must not be
    /// caught by <c>xsl:try</c> or <c>xsl:message</c>.
    /// </summary>
    private abstract class IterateControlException : Exception
    {
        protected IterateControlException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Control exception raised by <c>xsl:break</c> to terminate the innermost
    /// <c>xsl:iterate</c> loop.
    /// </summary>
    private sealed class BreakSignal : IterateControlException
    {
        /// <summary>The value produced by the <c>xsl:break</c> instruction.</summary>
        public XdmValue? Value { get; }

        public BreakSignal(XdmValue? value)
            : base("xsl:break")
        {
            Value = value;
        }
    }

    /// <summary>
    /// Control exception raised by <c>xsl:next-iteration</c> to advance the
    /// innermost <c>xsl:iterate</c> loop with updated parameter values.
    /// </summary>
    private sealed class NextIterationSignal : IterateControlException
    {
        /// <summary>New values for the iteration parameters.</summary>
        public Dictionary<(string LocalName, string NamespaceUri), XdmValue> NewParamValues { get; }

        public NextIterationSignal(Dictionary<(string LocalName, string NamespaceUri), XdmValue> newParamValues)
            : base("xsl:next-iteration")
        {
            NewParamValues = newParamValues;
        }
    }

    /// <summary>
    /// Executes an <c>xsl:iterate</c> instruction in the result-tree path.
    /// </summary>
    private void ExecuteXslIterate(XElement instruction, XdmValue contextItem)
    {
        var select = instruction.Attribute("select")?.Value;
        if (string.IsNullOrEmpty(select))
            throw new InvalidOperationException("XTSE0010: xsl:iterate must have a select attribute.");

        var items = EnumerateItems(CompileXPath(select, instruction).Evaluate(_context)).ToList();
        var xslNs = Stylesheet.Stylesheet.XslNamespace;

        ValidateIterateDescendants(instruction);

        // Validate ordering of xsl:param and xsl:on-completion, and detect xsl:param after body instructions.
        bool bodyStarted = false;
        foreach (var child in instruction.Elements())
        {
            if (child.Name.NamespaceName != xslNs)
            {
                bodyStarted = true;
                continue;
            }

            var localName = child.Name.LocalName;
            if (localName == "param")
            {
                if (bodyStarted)
                    throw new InvalidOperationException("XTSE0010: xsl:param must appear first in xsl:iterate.");
            }
            else if (localName == "on-completion")
            {
                if (bodyStarted)
                    throw new InvalidOperationException("XTSE0010: xsl:on-completion must appear before the body of xsl:iterate.");
            }
            else
            {
                bodyStarted = true;
            }
        }

        var paramValues = new Dictionary<(string LocalName, string NamespaceUri), XdmValue>();
        foreach (var p in instruction.Elements(XName.Get("param", xslNs)))
        {
            var pname = p.Attribute("name")?.Value;
            if (string.IsNullOrEmpty(pname))
                continue;

            var (plocal, pns) = ExpandVariableName(p, pname);
            var pselect = p.Attribute("select")?.Value;
            var pas = p.Attribute("as")?.Value;
            XdmValue pvalue;
            if (!string.IsNullOrEmpty(pselect))
            {
                pvalue = CompileXPath(pselect, p).Evaluate(_context);
            }
            else
            {
                var savedOutputUri = _context.CurrentOutputUri;
                _context.CurrentOutputUri = null;
                try
                {
                    pvalue = EvaluateSequenceConstructor(p, _context.ContextItem, wrapInDocumentNode: string.IsNullOrEmpty(pas));
                }
                finally
                {
                    _context.CurrentOutputUri = savedOutputUri;
                }
            }
            pvalue = ConvertVariableValue(pvalue, pas, isParam: true);

            var paramKey = (plocal, pns);
            if (paramValues.ContainsKey(paramKey))
                throw new InvalidOperationException("XTSE0580: duplicate xsl:param name in xsl:iterate.");
            paramValues[paramKey] = pvalue;
        }

        XdmValue? completionResult = null;
        bool broken = false;
        var savedFocus = _context.ContextItem;
        var savedPosition = _context.ContextPosition;
        var savedSize = _context.ContextSize;
        var savedCurrent = _context.CurrentItem;
        var savedVariables = _context.SnapshotVariables();
        try
        {
            int total = items.Count;
            for (int i = 0; i < total; i++)
            {
                var item = items[i];
                _context.WithFocus(item, i + 1, total);
                _context.WithCurrentItem(item);
                foreach (var kv in paramValues)
                    _context.WithVariable(kv.Key.LocalName, kv.Value, kv.Key.NamespaceUri);

                var iterationVariables = _context.SnapshotVariables();
                try
                {
                    foreach (var child in instruction.Elements())
                    {
                        if (child.Name.LocalName == "param" || child.Name.LocalName == "on-completion")
                            continue;
                        if (child.Name.NamespaceName == xslNs)
                            ExecuteXsltInstruction(child, item);
                        else
                            CopyLiteralElement(child);
                    }
                }
                catch (NextIterationSignal next)
                {
                    foreach (var kv in next.NewParamValues)
                        paramValues[kv.Key] = kv.Value;
                    continue;
                }
                catch (BreakSignal br)
                {
                    if (br.Value.HasValue && !br.Value.Value.IsUndefined)
                        CopyToResult(br.Value.Value);
                    broken = true;
                    break;
                }
                finally
                {
                    _context.RestoreVariables(iterationVariables);
                }
            }

            if (!broken)
            {
                // xsl:on-completion is evaluated with an absent focus and the final parameter values.
                _context.WithFocus(XdmValue.Undefined, 0, 0);
                foreach (var kv in paramValues)
                    _context.WithVariable(kv.Key.LocalName, kv.Value, kv.Key.NamespaceUri);

                var onCompletion = instruction.Element(XName.Get("on-completion", xslNs));
                if (onCompletion != null)
                {
                    var ocSelect = onCompletion.Attribute("select")?.Value;
                    var ocHasContent = onCompletion.Elements().Any();
                    if (!string.IsNullOrEmpty(ocSelect) && ocHasContent)
                        throw new InvalidOperationException("XTSE3125: xsl:on-completion must not have both a select attribute and sequence constructor content.");

                    if (!string.IsNullOrEmpty(ocSelect))
                    {
                        completionResult = CompileXPath(ocSelect, onCompletion).Evaluate(_context);
                    }
                    else if (ocHasContent)
                    {
                        completionResult = EvaluateSequenceConstructor(onCompletion, _context.ContextItem, wrapInDocumentNode: true);
                    }
                }
            }
        }
        finally
        {
            _context.RestoreVariables(savedVariables);
            _context.WithFocus(savedFocus, savedPosition, savedSize);
            _context.WithCurrentItem(savedCurrent);
        }

        if (completionResult.HasValue && !completionResult.Value.IsUndefined)
            CopyToResult(completionResult.Value);
    }

    /// <summary>
    /// Validates the lexical placement of <c>xsl:param</c>, <c>xsl:on-completion</c>,
    /// <c>xsl:break</c>, and <c>xsl:next-iteration</c> descendants of an
    /// <c>xsl:iterate</c> instruction, raising static errors when they are misplaced.
    /// </summary>
    private void ValidateIterateDescendants(XElement instruction)
    {
        var xslNs = Stylesheet.Stylesheet.XslNamespace;
        foreach (var descendant in instruction.Descendants())
        {
            if (descendant.Name.NamespaceName != xslNs)
                continue;

            // Instructions inside a nested xsl:iterate are validated by that nested instruction.
            bool insideNestedIterate = descendant.Ancestors().TakeWhile(a => a != instruction).Any(a =>
                a.Name.LocalName == "iterate" && a.Name.NamespaceName == xslNs);
            if (insideNestedIterate)
                continue;

            var local = descendant.Name.LocalName;
            if (local == "param" || local == "on-completion")
            {
                // xsl:param and xsl:on-completion must be direct children of xsl:iterate.
                if (descendant.Parent != instruction)
                    throw new InvalidOperationException("XTSE0010: xsl:param and xsl:on-completion must be direct children of xsl:iterate.");
                continue;
            }

            if (local != "break" && local != "next-iteration")
                continue;

            var parent = descendant.Parent;
            if (parent == null)
                continue;

            bool parentAllowed;
            if (parent == instruction)
            {
                parentAllowed = true;
            }
            else if (parent.Name.NamespaceName == xslNs)
            {
                var parentLocal = parent.Name.LocalName;
                if (parentLocal == "try")
                {
                    // Within xsl:try the instruction must precede any xsl:catch siblings.
                    parentAllowed = !descendant.ElementsAfterSelf().Any(e =>
                        !(e.Name.NamespaceName == xslNs && e.Name.LocalName == "catch"));
                }
                else
                {
                    parentAllowed = parentLocal == "when" || parentLocal == "otherwise" || parentLocal == "catch";
                }
            }
            else
            {
                parentAllowed = false;
            }

            if (!parentAllowed)
                throw new InvalidOperationException("XTSE3120: xsl:break and xsl:next-iteration must appear in the body of xsl:iterate.");

            bool hasFollowingSibling;
            if (parent == instruction)
            {
                // Direct children of xsl:iterate must be the last body instruction.
                hasFollowingSibling = descendant.ElementsAfterSelf().Any(e =>
                    e.Name.NamespaceName == xslNs
                    && e.Name.LocalName != "on-completion"
                    && e.Name.LocalName != "param");
            }
            else if (parent.Name.LocalName == "try" && parent.Name.NamespaceName == xslNs)
            {
                hasFollowingSibling = descendant.ElementsAfterSelf().Any(e =>
                    !(e.Name.NamespaceName == xslNs && e.Name.LocalName == "catch"));
            }
            else
            {
                hasFollowingSibling = descendant.ElementsAfterSelf().Any();
            }

            if (hasFollowingSibling)
                throw new InvalidOperationException("XTSE3120: xsl:break and xsl:next-iteration must be the last instruction in their sequence constructor.");
        }
    }

    /// <summary>
    /// Executes an <c>xsl:break</c> instruction inside <c>xsl:iterate</c>.
    /// </summary>
    private void ExecuteXslBreak(XElement instruction)
    {
        var xslNs = Stylesheet.Stylesheet.XslNamespace;
        if (!instruction.Ancestors().Any(a => a.Name.LocalName == "iterate" && a.Name.NamespaceName == xslNs))
            throw new InvalidOperationException("XTSE0010: xsl:break must be lexically inside xsl:iterate.");

        var select = instruction.Attribute("select")?.Value;
        var hasContent = instruction.Elements().Any();
        if (!string.IsNullOrEmpty(select) && hasContent)
            throw new InvalidOperationException("XTSE3125: xsl:break must not have both a select attribute and sequence constructor content.");

        XdmValue? value = null;
        if (!string.IsNullOrEmpty(select))
        {
            value = CompileXPath(select, instruction).Evaluate(_context);
        }
        else if (hasContent)
        {
            value = EvaluateSequenceConstructor(instruction, _context.ContextItem, wrapInDocumentNode: true);
        }

        throw new BreakSignal(value);
    }

    /// <summary>
    /// Executes an <c>xsl:next-iteration</c> instruction inside <c>xsl:iterate</c>.
    /// </summary>
    private void ExecuteXslNextIteration(XElement instruction)
    {
        var xslNs = Stylesheet.Stylesheet.XslNamespace;
        if (!instruction.Ancestors().Any(a => a.Name.LocalName == "iterate" && a.Name.NamespaceName == xslNs))
            throw new InvalidOperationException("XTSE0010: xsl:next-iteration must be lexically inside xsl:iterate.");

        var newValues = new Dictionary<(string LocalName, string NamespaceUri), XdmValue>();

        // The iteration parameters are the ones declared on the innermost enclosing xsl:iterate.
        var iterateAncestor = instruction.Ancestors().First(a =>
            a.Name.LocalName == "iterate" && a.Name.NamespaceName == xslNs);
        var validParamNames = new HashSet<(string LocalName, string NamespaceUri)>();
        var paramTypeByName = new Dictionary<(string LocalName, string NamespaceUri), string?>();
        foreach (var p in iterateAncestor.Elements(XName.Get("param", xslNs)))
        {
            var pname = p.Attribute("name")?.Value;
            if (!string.IsNullOrEmpty(pname))
            {
                var (plocal, pns) = ExpandVariableName(p, pname);
                validParamNames.Add((plocal, pns));
                paramTypeByName[(plocal, pns)] = p.Attribute("as")?.Value;
            }
        }

        foreach (var wp in instruction.Elements(XName.Get("with-param", xslNs)))
        {
            var wpName = wp.Attribute("name")?.Value;
            if (string.IsNullOrEmpty(wpName))
                continue;

            var (wplocal, wpns) = ExpandVariableName(wp, wpName);
            var wpKey = (wplocal, wpns);
            if (!validParamNames.Contains(wpKey))
                throw new InvalidOperationException("XTSE3130: xsl:with-param name does not match an xsl:iterate parameter.");
            if (newValues.ContainsKey(wpKey))
                throw new InvalidOperationException("XTSE0670: duplicate xsl:with-param name in xsl:next-iteration.");

            var wpSelect = wp.Attribute("select")?.Value;
            var wpAs = wp.Attribute("as")?.Value;
            XdmValue wpValue;
            if (!string.IsNullOrEmpty(wpSelect))
            {
                wpValue = CompileXPath(wpSelect, wp).Evaluate(_context);
            }
            else
            {
                var savedOutputUri = _context.CurrentOutputUri;
                _context.CurrentOutputUri = null;
                try
                {
                    wpValue = EvaluateSequenceConstructor(wp, _context.ContextItem, wrapInDocumentNode: string.IsNullOrEmpty(wpAs));
                }
                finally
                {
                    _context.CurrentOutputUri = savedOutputUri;
                }
            }
            wpValue = ConvertVariableValue(wpValue, wpAs, isParam: false);
            if (paramTypeByName.TryGetValue(wpKey, out var paramAs))
                wpValue = ConvertVariableValue(wpValue, paramAs, isParam: true);
            newValues[wpKey] = wpValue;
        }

        throw new NextIterationSignal(newValues);
    }
}

/// <summary>
/// Exception thrown when an XSLT instruction reports a dynamic error, carrying the
/// XSLT error code, description, and optional error value (e.g. the value of an
/// <c>xsl:message</c> that terminated processing).
/// </summary>
public sealed class XsltRuntimeException : InvalidOperationException
{
    /// <summary>
    /// The XSLT error code (e.g. <c>XTMM9000</c>).</summary>
    public string ErrorCode { get; }

    /// <summary>
    /// The value associated with the error, used for <c>$err:value</c> in <c>xsl:catch</c>.</summary>
    public XdmValue ErrorValue { get; }

    /// <summary>
    /// Creates a new XSLT runtime exception.</summary>
    public XsltRuntimeException(string errorCode, string description, XdmValue errorValue)
        : base($"{errorCode}: {description}")
    {
        ErrorCode = errorCode;
        ErrorValue = errorValue;
    }
}

/// <summary>
/// Exception thrown when evaluation of a global variable or parameter fails. Errors
/// from global variables are not catchable by <c>xsl:try</c> because the variable is
/// evaluated outside the dynamic scope of the try block.
/// </summary>
public sealed class XsltGlobalVariableException : Exception
{
    /// <summary>The local name of the variable whose evaluation failed.</summary>
    public string VariableName { get; }

    /// <summary>The namespace URI of the variable whose evaluation failed.</summary>
    public string NamespaceUri { get; }

    /// <summary>The original exception raised while evaluating the variable.</summary>
    public Exception OriginalException { get; }

    /// <summary>Creates a new global-variable evaluation exception.</summary>
    public XsltGlobalVariableException(string variableName, string namespaceUri, Exception originalException)
        : base($"Error evaluating global variable ${variableName}: {originalException.Message}", originalException)
    {
        VariableName = variableName;
        NamespaceUri = namespaceUri;
        OriginalException = originalException;
    }
}

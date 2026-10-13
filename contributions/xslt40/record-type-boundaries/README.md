# Native record-type boundary batch

Submitted as [xslt40-test PR #26](https://github.com/qt4cg/xslt40-test/pull/26). Both native cases now pass with Bosak source 1.2.0; the original 0 pass / 2 fail receipts below remain historical. Acceptance and independent processor confirmation are pending.

## Source and existing coverage

Existing record-001–025 and subtype cases primarily use structural record syntax; the only scanned xsl:record-type occurrence is an older atomic-set package using extensible forms. New cases target current nominative declarations with constructor=yes, explicit empty fields, and static initialization excluded from constructor arguments. They do not change old structural/optional-field expectations or certify all nominative semantics.

Expected results derive from [pinned source e7ee4ab8](https://github.com/qt4cg/qtspecs/blob/e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e/specifications/xslt-40/src/xslt.xml) and the [current section](https://qt4cg.org/specifications/xslt-40/Overview.html#named-record-types). This is a focused review, not exhaustive absence-of-duplicates certification or complete family conformance. No existing expectations changed.

## Input and expected output

Each case starts at xsl:initial-template and requires no source document. Literal values inside the stylesheet supply the input. Status routing, order-export projections and reusable type declarations represent small boundaries arising in real transformations; these are not full production schemas or applications.

### record-026

Named record constructor uses declared field order and retains an explicit empty field.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
 xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:t="urn:order-types"
 xmlns:map="http://www.w3.org/2005/xpath-functions/map" exclude-result-prefixes="#all">
<!-- Named record constructor uses declared field order and retains an explicit empty field. -->
<xsl:record-type name="t:line" constructor="yes"><xsl:field name="sku" as="xs:string"/><xsl:field name="quantity" as="xs:integer"/><xsl:field name="note" as="xs:string?"/></xsl:record-type>
<xsl:template name="xsl:initial-template"><xsl:variable name="line" as="t:line" select="t:line('A',2,())"/><out sku="{$line?sku}" quantity="{$line?quantity}" note="{map:contains($line,'note')}" empty="{empty($line?note)}" typed="{$line instance of t:line}"/></xsl:template>
</xsl:stylesheet>
```

Expected:

```xml
<out sku="A" quantity="2" note="true" empty="true" typed="true"/>
```

Bosak outcome: **fail** — FAIL record-026: XPST0017: Function {urn:order-types}line#3 not found.. See [exact result](record-026-result.txt) and [receipt](record-026-receipt.json).

### record-027

Static field initialization is excluded from constructor parameters and is retained in the record.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
 xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:t="urn:order-types"
 xmlns:map="http://www.w3.org/2005/xpath-functions/map" exclude-result-prefixes="#all">
<!-- Static field initialization is excluded from constructor parameters and is retained in the record. -->
<xsl:record-type name="t:line" constructor="yes"><xsl:field name="sku" as="xs:string"/><xsl:field name="quantity" as="xs:integer"/><xsl:field name="kind" static="yes" as="xs:string" select="'line'"/></xsl:record-type>
<xsl:template name="xsl:initial-template"><xsl:variable name="line" as="t:line" select="t:line('B',3)"/><out sku="{$line?sku}" kind="{$line?kind}" quantity="{$line?quantity}" fields="{map:size($line)}"/></xsl:template>
</xsl:stylesheet>
```

Expected:

```xml
<out sku="B" kind="line" quantity="3" fields="3"/>
```

Bosak outcome: **fail** — FAIL record-027: XPST0017: Function {urn:order-types}line#2 not found.. See [exact result](record-027-result.txt) and [receipt](record-027-receipt.json).

## Reproduction and evidence

The [portable mail patch](0001-native-tests.patch), [evidence manifest](evidence.json) and [submitted PR body](UPSTREAM_PR_BODY.md) preserve the native commit 024e11f3525c1753ccda92561adc220bf584f1f0. It is based on 6205faf9f32ace6338ac245c41f8eb9a1447926f and changes only the existing test-set and two stylesheets. Each patch reproduces its native commit tree; SHA-256 is recorded.

In the configured checkout, select the correct independent branch before running:

```powershell
git -C .upstream/xslt40-test switch bosak/record-type-boundaries
pwsh -File ./validate-xslt40-catalog.ps1 -Files tests/type/record/_record-test-set.xml
pwsh -File ./run-xslt40-tests.ps1 -TestSet record -TestCase record-026 -NoBuild
pwsh -File ./run-xslt40-tests.ps1 -TestSet record -TestCase record-027 -NoBuild
```

Build Bosak first if the local binary is absent. Failed cases exit 1; retain the catalog output instead of changing it to fit the engine. Runner totals can include substring-selected inherited cases; use exact case IDs for the two-case tally. For a fresh suite clone at the recorded base, create a new branch and apply the mail patch with git am; do not apply it again to the existing branch.

## Follow-up

Monitor the upstream review and requested independent cross-check. Record Bosak engine gaps separately; this contribution does not fix them or claim they are upstream defects. Source 1.1.4 remains unpublished, public release 1.1.3.


**Post-implementation — 2026-10-13:** Bosak source candidate 1.2.0 (unpublished) now recognizes xsl:record-type/xsl:field and registers constructor=yes functions. Constructors retain field order and empty entries, convert field values, supply trailing defaults and exclude static fields from arguments. Records carry declaration identity: constructed values satisfy named @as and instance-of tests; same-shaped other types and plain maps do not. Both unchanged native cases record-026/027 pass. The wider record substring sweep is 12 pass/52 fail/1 skip, including inherited structural/subtype cases. Plain-map coercion/casts, recursive record definitions, complete package exposure/function signatures and streaming conformance remain follow-ups. N05 and REQ-126 remain In Progress; original failed receipts and upstream PR #26 remain unchanged, with review/independent confirmation pending. Public release remains 1.1.3; no publication. Next: prioritize the wider named-type/coercion gaps and select body namespace declarations after checking this slice's CI. See [post-fix results](POST_FIX_RESULTS.json).


**Nominative record coercion — 2026-10-13:** Source candidate 1.2.0 (unpublished) now coerces maps to package-local nominative xsl:record-type declarations at variable and function boundaries. Conversion applies field types, missing-field defaults and static initializers, rejects surplus keys and supplied static fields, preserves declaration order and annotates the result with the target identity. Existing matching records are retained. Type checks still reject plain maps and distinguish same-shaped declarations. Nine new regressions cover sensor defaults, function references, cardinality, invalid bindings and identity; the previous valid-map refusal test now rejects surplus fields. Named casts, recursive named records, nested named-type/static namespace resolution and complete package exposure remain follow-ups. N04/REQ-126 remain In Progress; published version remains 1.1.3. See [separate evidence](COERCION_RESULTS.json). Catalogue counts remain unchanged; the named-record plain-map fixture now expects INV-2048. Original native assertions remain unchanged; earlier slice notes are historical. Next: named record cast and nested type resolution boundaries.


**Named record casts and nested references — 2026-10-13:** Source candidate 1.2.0 (unpublished) now supports package-local nominative record cast/castable operations. Casts retain map operands, cast each present field sequence, apply defaults to missing fields, discard surplus keys, protect static fields and attach the target declaration identity. Nested named references in binding/function signatures resolve in their declaration namespace, and nested named record fields reuse casting recursively. Fourteen new regressions cover lexical/type failures, empty/optional inputs, field cardinalities and namespace boundaries. General array/map casts, item-type alias cast expansion, recursive definitions, full package exposure and complete static diagnostics remain follow-ups. N05/REQ-126 remain In Progress; published version remains 1.1.3. See [separate evidence](CAST_RESULTS.json). Catalogue counts/outcomes and original native assertions remain unchanged. Earlier slice notes are historical. Next: remaining recursive/static type-resolution and cast-container boundaries.


**Finite recursive records and container casts — 2026-10-13:** Source candidate 1.2.0 (unpublished) now casts XPath 4.0 arrays/maps by casting member/key/value sequences, retaining container shape and checking cardinality. Map-key collisions raise XQDY0137; wildcard casts retain the supplied container and empty container types reject nonempty inputs. Named record field casts reuse the shared sequence caster. Record field metadata resolves aliases and finite self/mutual named references in the declaration namespace without expanding nominative identities. Sixteen new regressions cover finite linked lists, recursive array casts, nested containers, invalid members and collisions. Arbitrary-depth resource handling, full recursive/static diagnostics, item-type alias cast expansion and package exposure remain follow-ups. N05/REQ-126 remain In Progress; published version remains 1.1.3. See [separate evidence](CONTAINER_RESULTS.json). Catalogue counts/outcomes and original native assertions remain unchanged; earlier slice notes are historical. Next: item-type alias cast expansion and static named-type validation.


## Host static named-type validation — 2026-10-13

Source candidate 1.2.0 (unpublished) validates package-local named types in supported XPath 4.0 instance-of, treat, cast and castable targets when the XSLT host compiles the expression. Unknown direct/nested names raise XPST0051, including AVT expressions; undeclared prefixes retain XPST0081. Declared nominative record references are canonicalized to their expanded names, and record alternatives in choice cast targets raise XPST0080. Ordinary record-valued type-test choices remain valid. The internal host registry survives inline namespace declarations, recognizes built-in/XDM types and consults in-scope simple schema types. Twenty-one new regressions pass. Binding/field declaration validation, unsupported type grammar, complete schema variety/choice semantics and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress; published version stays 1.1.3.

See [static named-type evidence](../item-type-boundaries/STATIC_TYPE_RESULTS.json). Native type-alias remains 8/24/0; unchanged contributed cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Declaration static named-type validation — 2026-10-13

Candidate 1.2.0 now validates supported XPath 4.0 sequence-type names on XSLT @as declarations at stylesheet registration, including unused variables, parameters, function results and record fields. The check runs after package-local aliases and records are registered and traverses import/include modules with each declaration's namespace context. Unknown direct/nested names raise XPST0051; undeclared binding prefixes raise XPST0081; record registration retains XTSE0280 for unbound field prefixes. Forward aliases, finite recursive record references, default type namespaces and quoted enumeration/field names remain valid. Fourteen new regressions cover these boundaries. XPath 3.1 declarations keep their existing path. Unsupported shared-walker grammar, complete schema variety semantics and package type exposure remain follow-ups; N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; no release or gate promotion.

[Verification evidence](../item-type-boundaries/DECLARATION_TYPE_RESULTS.json). Native type-alias: 9 pass/23 fail/0 skip; type-alias-019 newly passes and unchanged contribution cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Node-kind alias grammar — 2026-10-13

Candidate 1.2.0 supports node-kind aliases whose tests have empty parentheses: element(), attribute(), text(), comment(), processing-instruction(), namespace-node() and document-node(). The shared type walker also accepts whitespace and nested comments around empty tests. These types can appear within supported array/map/record/function/choice types; adjacent unknown names now receive static validation instead of being hidden by an unsupported node-kind branch. Sixteen regressions verify node identity, occurrences, structural nesting, unknown names, binding failures and non-atomic choice cast rejection. Named/wildcard/schema-annotated node tests, fn/named-parameter function type grammar, full schema semantics and package type exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Public version remains 1.1.3; candidate 1.2.0 stays unpublished.

[Verification evidence](../item-type-boundaries/NODE_KIND_RESULTS.json). Native type-alias remains 9 pass/23 fail/0 skip; original contribution cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Named node-type alias grammar — 2026-10-13

Candidate 1.2.0 supports named element/attribute node tests, the complete * wildcard, and document-node(element(...)) inside package-local aliases and supported structural sequence types. Node names resolve in their declaration namespace and remain stable under use-site prefix/default namespace rebinding; unprefixed attributes retain the empty namespace. The host statically rejects unbound node prefixes and validates unknown type names beside named node tests. Explicit Q{}Name matching now checks both namespace and case-sensitive local name. Seventeen new regressions cover matching/mismatches, node identity, nested types, record fields, namespace defaults and static errors. Schema-annotated node tests, partial wildcards, processing-instruction targets, fn/named-parameter function grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Public version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../item-type-boundaries/NAMED_NODE_RESULTS.json). Native type-alias remains 9 pass/23 fail/0 skip; original contribution cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Processing-instruction target aliases — 2026-10-13

Candidate 1.2.0 supports processing-instruction targets in item-type aliases and supported structural sequence types. NCName and quoted-string targets retain case, normalize surrounding whitespace and are never expanded as type QNames. The parser validates targeted sequence-type syntax using a shared target reader; runtime matching compares the actual PI target. Malformed targets raise XPST0003 and quoted targets outside the NCName lexical space raise XPTY0004. Twelve new regressions verify identity, target/case mismatch, nesting, optional occurrences and static diagnostics. Partial wildcards, schema-annotated node tests, fn/named-parameter function grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Public version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../item-type-boundaries/PI_TARGET_RESULTS.json). Native type-alias remains 9 pass/23 fail/0 skip; original contribution cases 023/024 pass. Broader failures and independent processor confirmation remain outstanding.


## Partial node wildcard aliases — 2026-10-13

Candidate 1.2.0 supports partial element/attribute name wildcards in aliases and supported structural types: *:Name matches an exact local name in any namespace; prefix:* and Q{uri}* match any local name in the specified namespace. Prefix wildcards retain declaration bindings under use-site rebinding, including document-node(element(...)) nesting. Unbound wildcard prefixes raise XPST0081; adjacent unknown type names retain XPST0051. The ##any declaration default now expands unprefixed element tests to *:Name while unprefixed attributes stay unqualified. Twenty new regressions verify identity, case/namespace mismatches, empty namespaces, nested arrays/documents and diagnostics. Node-name unions, schema-annotated tests, fn/named-parameter function grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../item-type-boundaries/NODE_WILDCARD_RESULTS.json). Native type-alias improves to 11 pass/21 fail/0 skip: wildcard-type-001/002 newly pass and original contribution cases 023/024 still pass. Broader failures and independent processor confirmation remain outstanding.


## Node-name union aliases — 2026-10-13

Candidate 1.2.0 supports node-name unions in element/attribute item-type aliases and supported structural sequence types: element(doctor|nurse) and attribute(price|cost) match any listed name. Each alternative retains declaration namespaces and may use a partial wildcard, including nested document-node(element(...)), arrays and record fields. EQName namespace URIs containing pipe or comma characters remain intact. Empty alternatives raise XPST0003; an undeclared prefix in any alternative raises XPST0081; adjacent unknown type names retain XPST0051. Twenty-two new regressions cover positive/negative matching, identity, declaration prefix rebinding, wildcard combinations, nested types and ##any defaults. Schema-annotated alias traversal, fn/named-parameter function grammar and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../item-type-boundaries/NODE_UNION_RESULTS.json). Native type-alias stays 11 pass/21 fail/0 skip, including wildcard-type-001/002 and original contribution cases 023/024. Broader native failures and independent processor confirmation remain outstanding.


## Schema-annotated node aliases — 2026-10-13

Candidate 1.2.0 supports schema annotations in element/attribute item-type aliases and supported structural sequence types, including node-name unions, nested documents, arrays and record fields. The annotation resolves independently against in-scope schema types and retains declaration namespaces under prefix rebinding; item-type aliases are not substituted into the schema annotation. Built-in and imported simple/complex schema types are accepted. Unknown annotations raise XPST0008; undeclared annotation prefixes raise XPST0081; malformed or empty annotations raise XPST0003. Only element annotations permit the nillable ? marker. The ##any default maps unprefixed schema annotations to the XML Schema namespace. Twenty-eight new regressions cover matching untyped and validated nodes, non-coercion, imported simple/complex types, nillability, namespace identity, nested contracts and unused declarations. fn/named-parameter function grammar, schema-element/schema-attribute alias traversal, document shorthand and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../item-type-boundaries/SCHEMA_NODE_RESULTS.json). Native type-alias stays 11 pass/21 fail/0 skip, including wildcard-type-001/002 and original contribution cases 023/024. Broader native failures and independent processor confirmation remain outstanding.


## Function-type grammar aliases — 2026-10-13

Candidate 1.2.0 accepts fn(...) as a synonym for function(...) and documentary parameter names in supported function-type aliases, direct XPath type operands and XSLT binding signatures. Names do not affect matching or coercion; duplicate expanded names raise XQST0039, including different prefixes bound to the same namespace and unprefixed names paired with Q{} names. Undeclared documentary prefixes raise XPST0081; malformed named parameters raise XPST0003; unknown parameter/result item types retain XPST0051. Named and unnamed parameters may be mixed, with independent name scopes for nested function types. Nested arrays, records and higher-order signatures retain parameter/result sequence occurrences. XPath 3.1 rejects fn type synonyms and named type parameters. Twenty-six new regressions cover callback conversion, wildcard maps/arrays, direct bindings without aliases, direct host evaluation, nesting, namespace identity, diagnostics and version boundaries. Annotated function types, schema-element/schema-attribute alias traversal, document shorthand and package exposure remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../item-type-boundaries/FUNCTION_GRAMMAR_RESULTS.json). Native type-alias stays 11 pass/21 fail/0 skip, including wildcard-type-001/002 and original contribution cases 023/024. Broader native failures and independent processor confirmation remain outstanding.


## Schema-declaration and document shorthand aliases — 2026-10-13

Candidate 1.2.0 supports schema-element(Name), schema-attribute(Name) and document-node(schema-element(Name)) in item-type aliases and supported structural sequence types. Schema declaration names retain their declaration namespace under prefix rebinding and must resolve to imported global elements/attributes; unknown or unavailable declarations raise XPST0008 and undeclared prefixes raise XPST0081. Wildcards, unions and empty declaration names raise XPST0003. Schema-element under ##any uses no namespace for an unprefixed declaration; unprefixed schema attributes always use no namespace. Document shorthand such as document-node(root|other) normalizes to document-node(element(root|other)), including wildcard and EQName alternatives, and reuses single-root matching. Direct host type operands and alias-free XSLT document/schema bindings normalize the new forms. Twenty-nine new regressions cover node identity, namespace mismatches, imported declarations, substitution members, typed attribute matching, rejection of untyped nodes, nested arrays, ##any defaults, empty documents and static validation of unused bindings. Annotated function types, package exposure and broader native type-alias gaps remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../item-type-boundaries/SCHEMA_DECLARATION_RESULTS.json). Native type-alias stays 11 pass/21 fail/0 skip, including wildcard-type-001/002 and original contribution cases 023/024. Broader native failures and independent processor confirmation remain outstanding.


## Package named type exposure — 2026-10-13

Candidate 1.2.0 accepts item-type and record-type components in xsl:expose and xsl:accept. Public or exposed named types enter using-package type registries unless accepted as hidden; private dependencies retain the defining package's resolved alias definitions and record metadata. Accept names resolve namespaces on the accept element. Missing component names, incompatible acceptance, invalid declaration visibility and collisions with local types produce static errors. Seventeen regressions cover public declarations, explicit and wildcard exposure, private and hidden names, prefix scope, record constructors, private alias dependencies rejected collisions and public/private visibility through an intermediate package. Native type-alias improves from 11/21/0 to 15/17/0; newly passing cases are 018, 020, 021 and 022. Original contribution cases 023/024 remain green. Broader native failures and independent processor confirmation remain outstanding; N04/N05 and REQ-126 stay In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../item-type-boundaries/PACKAGE_EXPOSURE_RESULTS.json). The [XSLT 4.0 named type and package rules](https://qt4cg.org/specifications/xslt-40/Overview.html) define these visibility boundaries.


## Fixed namespace type contexts — 2026-10-13

Candidate 1.2.0 resolves fixed-namespaces for named item/record type names, supported nested type references, static declaration validation and the main XPath compilation paths. Supported tokens include #standard, selected root-native or conventional prefixes, explicit prefix=URI bindings and namespace-document references through the existing stylesheet resolver. Bindings are module-local, cached at compilation, unaffected by descendant rebinding and applied in token order. Invalid bindings or unavailable namespace documents raise XTSE0122. Unprefixed type/node names retain their existing default-namespace rules; fixed bindings do not create native result namespaces. Sixteen regressions cover standard prefixes, nested node contracts, explicit/repeated bindings, namespace documents, include isolation and invalid/unavailable bindings. Native type-alias remains 15/17/0: case 016 now passes namespace resolution but still fails its independent inline-function record conversion contract with XPTY0004. No upstream assertion changes. Fixed namespace coverage of other QName-bearing XSLT attributes, pattern compilation and shadow/static processing remains a follow-up; this is not complete fixed-namespaces support. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../item-type-boundaries/FIXED_NAMESPACE_RESULTS.json). The [XSLT 4.0 fixed-namespaces rules](https://qt4cg.org/specifications/xslt-40/Overview.html#fixed-namespaces) define the namespace token and scope rules.


## Inline function named type contracts — 2026-10-13

Candidate 1.2.0 expands host named types in inline function parameter and return contracts before IR lowering. Both function(...) and fn(...) retain resolved atomic, array, structural record and nominative record types, including occurrence indicators and nested member aliases. Signature resolution uses the expression static namespace context, including defaults, EQNames, inline namespace declarations and fixed bindings; the existing VM conversion rules apply to arguments and results. Unknown supported signature types are rejected statically even when the function is unused. Public EnumerationItemTypes registrations also resolve in inline signatures and remain restricted to single enum item types. Omitted annotations and XPath 3.1 instructions keep their existing behavior. Twenty-five regressions cover record/date updates, argument and return coercion, cardinality errors, namespaces, array members, record identity, AVTs, public enumeration boundaries and mixed-version refusal. Native type-alias improves from 15/17/0 to 16/16/0: case 016 now passes without changing its assertion; original contribution cases 023/024 remain green. Broader record matching, annotated function types, type patterns and remaining native failures stay outstanding. N04/N05 and REQ-126 remain In Progress. Published version stays 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../item-type-boundaries/INLINE_FUNCTION_RESULTS.json). The [XPath 4.0 inline function rules](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-inline-func) define parameter and result contracts.


## Direct structural record bindings — 2026-10-13

Candidate 1.2.0 coerces direct structural record(...) and record(*) XSLT binding contracts through the existing XPath 4.0 converter. Supported direct record fields resolve aliases and node/type namespaces in their declaration context. Local/global variables and template parameters preserve record annotations and numeric field conversion; invalid values retain XTTE0570/XTTE0590 binding diagnostics. Required single-record and record-plus conversion now reject empty sequences in both empty-value representations, while optional/star records retain empty-sequence support. Plain maps still do not match annotated record instance tests; record(*) requires an existing annotation. Seventeen regressions cover local/global/default-parameter bindings, occurrence indicators, field aliases and node contracts, empty records, invalid values/extra fields, parameter diagnostics and required inline function returns. The local inline-record40 invoice example now verifies INV-2048|EUR instead of the previously observed XTTE0570. Native type-alias stays 16/16/0: case 050 now gets past XTTE0570 record coercion but still fails its independent result assertion. Upstream assertions remain unchanged. Type patterns, broader native record outcomes and annotated function types remain follow-ups. N04/N05 and REQ-126 stay In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](STRUCTURAL_BINDING_RESULTS.json). The [XPath 4.0 structural record rules](https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-record-types) distinguish coercion from annotated membership.


## Template type patterns — 2026-10-13

Candidate 1.2.0 supports standalone XSLT 4.0 template type patterns ~T with zero or more predicates, including parenthesized forms, named item types, structural records and nominative records. The matcher delegates membership to the existing XPath type tests and preserves numeric predicate, singleton focus and dynamic-error-as-nonmatch behavior. Template patterns compile with the declaring package type registry and applicable static namespaces, including fixed-namespaces. Default priorities cover item/node/function/map/array families, built-in atomic derivation depth, enumerations and supported choice types; predicate patterns have priority 0.5 and explicit priorities are retained. xs:anyAtomicType is now recognized in host-supplied static type contexts. Twenty-nine regressions cover these behaviors, invalid syntax, previous-profile rejection, quoted field names, EQName delimiters and package-private alias isolation. Native type-alias advances from 16/16/0 to 18/14/0: cases 008 and 009 now pass with their upstream assertions unchanged. Ordinary wildcard node patterns, full XSLT 4.0 union-pattern priority semantics, schema-defined atomic priority derivation and named-type/fixed-namespace authoring-validation contexts remain follow-ups. Non-template pattern slots have not gained the template declaring-type registry in this slice. Plain maps still do not acquire record annotations through pattern matching; cases 006/010/013 and case 050 result handling remain independent boundaries. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../item-type-boundaries/TYPE_PATTERN_RESULTS.json). The [XSLT 4.0 type-pattern rules](https://qt4cg.org/specifications/xslt-40/Overview.html) and [default priorities](https://qt4cg.org/specifications/xslt-40/Overview.html) define matching and conflict resolution.


## Wildcard node patterns — 2026-10-13

Candidate 1.2.0 matches XSLT 4.0 element(...) and attribute(...) kind-test patterns using namespace wildcards (p:* and Q{uri}*), local-name wildcards (*:local), and supported name unions. Matching applies in standalone, explicit-axis and path steps while retaining schema type checks and distinct element/attribute default namespaces. The 4.0 priority table gives untyped partial wildcards -0.25, typed all-name wildcards 0, typed partial wildcards 0.125 and typed exact names 0.25; name unions use their highest member priority. Supported node type patterns and document-node wrappers reuse those priorities. Explicit priorities are retained, and predicates raise node-pattern priority to 0.5, including explicit-axis kind tests. Earlier profiles retain their existing matching and priority behavior. Node-type schema annotations now tolerate surrounding whitespace; 4.0 pattern EQName URI rewriting and kind-test argument splitting preserve embedded delimiters. Forty-one regressions cover matching/nonmatching namespaces and types, axes/paths, unions, priorities, malformed names, prior-profile compatibility and EQName delimiters. Native type-alias advances from 18/14/0 to 22/10/0: wildcard-type-003–006 newly pass with upstream assertions unchanged. Full XSLT 4.0 union-pattern priority/next-match semantics, other node-pattern priority forms, schema-defined atomic priorities, authoring-validation named-type/fixed-namespace contexts and non-template declaring-type registries remain follow-ups. Remaining record/result cases and annotated function types remain independent boundaries; plain maps do not gain record annotations through pattern matching. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../item-type-boundaries/WILDCARD_PATTERN_RESULTS.json). The [XSLT 4.0 default priority table](https://qt4cg.org/specifications/xslt-40/Overview.html) in section 6.4.3 defines the supported kind-test priorities.


## Node union patterns — 2026-10-13

Candidate 1.2.0 keeps supported XSLT 4.0 node union patterns as one template rule, using the highest default priority of their alternatives even when a lower-priority alternative is the one that matches. xsl:next-match visits that rule once, and overlapping alternatives do not create competing rule identities. Earlier effective profiles retain separate rules and their original per-alternative next-match behavior. Explicit priorities remain unchanged. Priority calculation recognizes pipe and union keyword operators, recursively computes nested alternatives, and preserves strings, predicates and EQName URIs; parenthesized 4.0 matching retains the declaring static namespace/type context. Empty top-level alternatives are rejected with XTSE0340. Twenty-eight regressions verify maximum priority, single visits, duplicate alternatives, failing ambiguity modes, nested/default namespaces, explicit priorities, version overrides, delimiters and invalid alternatives. Native next-match (including si-next-match) remains 46/8/6, and type-alias remains 22/10/0 with all prior wildcard/type-pattern gains retained. Four next-match 4.0 assertions (021a/023a/025a/039a) reference version-2.0 stylesheets and therefore still select the earlier effective profile; their fixtures and assertions remain unchanged. Processor-level selection for those lower-version native fixtures is a separate harness/API boundary. Other node-pattern priority forms, broader pattern grammar, schema-defined atomic priorities, named-type/fixed-namespace authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../item-type-boundaries/UNION_PATTERN_RESULTS.json). The [XSLT 4.0 node-pattern priorities](https://qt4cg.org/specifications/xslt-40/Overview.html), section 6.4.3, define the single-rule union behavior.


## Attribute and forward-axis priorities — 2026-10-13

Candidate 1.2.0 computes supported XSLT 4.0 attribute shorthand and forward-axis pattern priorities from the underlying node test. Named attributes (@A and attribute::A) and named element axis tests have priority 0; namespace/local wildcards retain -0.25 and untargeted kind tests retain -0.5. Named processing-instruction axis tests have priority 0 and now match their target in 4.0 child/self/descendant-style node-test compilation. Existing 4.0 typed kind-test priorities, explicit priorities, predicate priority 0.5 and single-rule union behavior are retained. Earlier effective profiles keep their original behavior. Axis recognition for priority and 4.0 matching ignores :: inside EQName URIs. Thirty-three regressions compare default priorities against explicit competitors, verify earlier-profile differences, target nonmatches, attribute-union next-match chains and positive/negative EQName URI cases. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior wildcard/type-pattern gains remain green and upstream assertions/fixture versions are unchanged. Intersect/except and bare predicate-pattern priorities, broader pattern grammar, lower-version native fixture processor selection, schema-defined atomic priorities, named-type/fixed-namespace authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../item-type-boundaries/FORWARD_AXIS_PRIORITY_RESULTS.json). The [XSLT 4.0 node-pattern priorities](https://qt4cg.org/specifications/xslt-40/Overview.html), section 6.4.3, assign priorities independently of supported forward-axis prefixes.


## Set and predicate-pattern priorities — 2026-10-13

Candidate 1.2.0 computes supported XSLT 4.0 intersect/except pattern default priorities from the first operand, including mixed operators, parenthesized operands and union precedence. Bare predicate pattern `.` has priority -1; predicate patterns with one or more predicates have priority +1 even when their expressions contain path slashes or slash string literals. Earlier effective profiles retain their existing behavior. Thirty-four regressions compare explicit competitors, check excluded-node and false-predicate nonmatches, atomic bare-dot matching, comments, adjacent punctuation, operator-name elements and EQName URI operator text. This is a priority calculation slice; broader pattern grammar and set-matching semantics remain separate follow-ups. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior wildcard/type-pattern gains remain green and upstream assertions/fixture versions are unchanged. Parenthesized item-pattern static validation, broader pattern grammar, lower-version native fixture processor selection, schema-defined atomic priorities, named-type/fixed-namespace authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../item-type-boundaries/SET_PREDICATE_PRIORITY_RESULTS.json). The [XSLT 4.0 node-pattern priorities](https://qt4cg.org/specifications/xslt-40/Overview.html), section 6.4.3, use first-operand priorities for intersect/except; section 6.4.1 assigns bare-dot and filtered predicate-pattern priorities.


## Parenthesized item patterns — 2026-10-13

Candidate 1.2.0 admits supported parenthesized predicate and type patterns in the effective XSLT 4.0 profile and restricts those outer XNode patterns to nodes. Atomic values, maps and records therefore do not match a parenthesized item pattern; standalone predicate/type patterns retain their item matching. A prior local quoted-record-field regression incorrectly used a parenthesized type pattern for a map; it now uses the standalone type pattern, and new negative regressions verify the node boundary. Nested compilation retains declaring-package aliases, nominative type registries and applicable namespaces. Contained type priorities are recomputed after alias expansion, including wrapped union operands. Earlier profiles retain parenthesized-predicate rejection. Twenty-seven new regressions cover nesting, predicates, node-only membership, atomic/map negatives, aliases, default namespaces, contained priority and invalid direct item unions. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior wildcard/type-pattern gains remain green and upstream assertions/fixture versions are unchanged. Parenthesized outer-filter path classification, broader pattern grammar/set semantics, lower-version native fixture processor selection, schema-defined atomic priorities, named-type/fixed-namespace authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../item-type-boundaries/PARENTHESIZED_ITEM_PATTERN_RESULTS.json). The [XSLT 4.0 node-pattern priorities](https://qt4cg.org/specifications/xslt-40/Overview.html), section 6.4.3, preserve contained-pattern priorities; section 6.3.4 permits parenthesized item patterns within XNode patterns and restricts XNode patterns to nodes.


## Parenthesized outer-filter paths — 2026-10-13

Candidate 1.2.0 classifies supported XSLT 4.0 parenthesized outer filters using path slashes outside predicates, string literals and EQName namespace URIs. Patterns such as `(.[child/..])[true()]` keep candidate-node focus instead of incorrectly evaluating from the document root. Actual path patterns retain sequence-based positional filtering, including nested parentheses, descendant paths and paths whose EQName URIs contain slashes. Earlier effective profiles retain their existing classification. Twenty-five regressions cover candidate predicates, nested predicates, escaped quotes, URI slashes, aliases, real path positions and nonmatches. The initial twenty-one-case baseline had six failures before the fix; all final twenty-five cases pass. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior gains remain green and upstream assertions/fixtures are unchanged. Pattern delimiter scanning around quoted parentheses/brackets, broader grammar/set semantics, lower-version native fixture processor selection, schema-defined atomic priorities, fixed namespace/authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../item-type-boundaries/PARENTHESIZED_FILTER_PATH_RESULTS.json). See [XSLT 4.0 XNode pattern semantics](https://qt4cg.org/specifications/xslt-40/Overview.html), section 6.3.4.2.


## Pattern delimiter scanning — 2026-10-13

Candidate 1.2.0 matches pattern parentheses and brackets outside string literals and EQName namespace URIs, including doubled quote escapes. Predicate extraction, parenthesized compilation and template normalization/priority share the same lexical delimiter matcher. Static function-call validation ignores parentheses inside EQName URIs. Nineteen regressions cover quoted opening/closing delimiters, nested predicates, escaped single/double quotes, URI parentheses, contained priority, false outer predicates and ordinary earlier-profile node patterns. The baseline was 6/13/0 before the fix; all nineteen now pass. Lexical corrections apply to supported patterns across profiles; existing profile-specific grammar and priority rules remain intact. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior gains remain green and upstream assertions/fixtures are unchanged. Top-level operator splitting around quoted delimiters, broader grammar/set semantics, lower-version native fixture processor selection, schema-defined atomic priorities, fixed namespace/authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../item-type-boundaries/PATTERN_DELIMITER_RESULTS.json). See [XSLT 4.0 XNode pattern semantics](https://qt4cg.org/specifications/xslt-40/Overview.html), sections 6.3.1 and 6.3.4.


## Pattern operator scanning — 2026-10-13

Candidate 1.2.0 splits supported top-level pattern operators outside string literals, doubled quote escapes, EQName namespace URIs and nested expressions. The shared scanner handles pipe unions and union/intersect/except keywords without splitting complete node-name tokens such as pre-except-post, pre_intersect_post or pre.union.post. Earlier-profile union rule expansion and type-choice priority splitting reuse the lexical scanner; existing profile-specific priority and grammar rules remain intact. Twenty-three regressions cover quoted delimiters/operator text, nested predicates, escaped quotes, URI punctuation, complete operator-like names, nonmatches and earlier-profile unions. The baseline was 17/6/0 before the fix; all twenty-three now pass. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior gains remain green and upstream assertions/fixtures are unchanged. Mixed intersect/except associativity, broader grammar/set semantics, lower-version native fixture processor selection, schema-defined atomic priorities, fixed namespace/authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../item-type-boundaries/PATTERN_OPERATOR_RESULTS.json). See [XSLT 4.0 XNode pattern semantics](https://qt4cg.org/specifications/xslt-40/Overview.html), sections 6.3.1 and 6.3.4.


## Set-pattern associativity — 2026-10-13

Candidate 1.2.0 evaluates supported mixed intersect/except pattern chains from left to right in the effective XSLT 4.0 profile. Both operators have equal precedence: `root except missing intersect missing` does not match root, while `root except (missing intersect missing)` does. The matcher preserves parenthesized grouping, union precedence, quoted/URI operator text and existing all-path/all-axis expression evaluation. Missing operands in mixed chains raise XTSE0340. Earlier profiles retain their existing dispatch behavior. Eighteen new regressions cover membership, nonmatches, grouping, union precedence, URI/quoted punctuation, path/axis negatives and earlier-profile behavior. The baseline was 10/8/0 before the fix; focused new plus prior operator checks pass 41/0/0. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior gains remain green and upstream assertions/fixtures are unchanged. Forward-axis operand focus in 4.0 set patterns, broader grammar/set semantics, lower-version native fixture processor selection, schema-defined atomic priorities, fixed namespace/authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../item-type-boundaries/SET_PATTERN_ASSOCIATIVITY_RESULTS.json). See [XSLT 4.0 XNode pattern semantics](https://qt4cg.org/specifications/xslt-40/Overview.html), sections 6.3.1 and 6.3.4.


## Set-pattern axis focus — 2026-10-13

Candidate 1.2.0 matches supported forward-axis operands in XSLT 4.0 intersect/except patterns against the candidate node instead of forcing axis-only chains into document-root XPath evaluation. Mixed axis/path operands use their respective pattern matchers; all-actual-path chains retain sequence evaluation and positional selection. The 4.0 single-pattern classifier reuses lexical path detection so slashes inside predicates or string literals do not turn an axis pattern into a path. Earlier profiles retain existing dispatch. Twenty-two regressions cover self/child/descendant-style axes, attributes, predicates, mixed actual paths, all-path positions, EQName URIs, first-operand priority and earlier-profile behavior. The baseline was 9/13/0 before the fix; all twenty-two now pass. Native next-match remains 46/8/6 and type-alias remains 22/10/0; prior gains remain green and upstream assertions/fixtures are unchanged. Fixed namespace resolution in 4.0 template pattern names, broader grammar/set semantics, lower-version native fixture processor selection, schema-defined atomic priorities, authoring contexts, non-template type registries, remaining record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../item-type-boundaries/SET_PATTERN_AXIS_FOCUS_RESULTS.json). See [XSLT 4.0 XNode pattern semantics](https://qt4cg.org/specifications/xslt-40/Overview.html), sections 6.3.1 and 6.3.4.


## Pattern fixed namespaces — 2026-10-13

Candidate 1.2.0 resolves prefixes in effective XSLT 4.0 template patterns using the declaring applicable static namespace bindings. Fixed bindings now survive native rebinding in node names, attributes, axes, unions, set operands, nested type tests and predicates. Existing EQNames and quoted text are preserved. Without fixed bindings, native declarations remain applicable; earlier profiles retain their native lookup. Twenty-one regressions cover these boundaries; the initial nineteen-test baseline was 3/16/0. Native next-match remains 46/8/6 and type-alias remains 22/10/0; upstream assertions and fixtures are unchanged. Broader pattern grammar, omitted-prefix diagnostics, non-template authoring/static contexts, schema-defined atomic priorities, lower-version native fixture processor selection, record/result cases and annotated function types remain follow-ups. N04/N05 and REQ-126 remain In Progress. Published version remains 1.1.3; candidate 1.2.0 remains unpublished.

[Verification evidence](../item-type-boundaries/PATTERN_FIXED_NAMESPACE_RESULTS.json). See [XSLT 4.0 XNode pattern semantics](https://qt4cg.org/specifications/xslt-40/Overview.html), sections 3.6.1 and 6.3.1.

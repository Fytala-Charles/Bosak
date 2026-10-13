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

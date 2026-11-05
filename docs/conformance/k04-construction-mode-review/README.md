# K04-C33 - Construction and typed-copy boundary review

28 October 2026. Review complete against `71233bfc`. **Admission is not qualified.** Normal source, construction/pragma guards, fixtures, baselines and version remain unchanged. Candidate **1.31.0** remains unpublished; public **1.26.3**.

## Complete remaining cohort

The independently loaded normal and copied-prototype harnesses inventory all **31,821 QT3 cases / 428 sets** with actual expression reading, lexical normalizers and dependencies. Exactly **87** construction predicates change across **ten sets**, matching C30's **75 construction-only and 12 mixed-mode** originals. Four cases retain independent exact-XQ10 exclusions under current harness policy; the other 83 are dependency-admissible. All 49 previously admitted copy-only predicates and 43 context predicates retain admission.

The copied complete harness disables only the construction refusal and adds its diagnostic history row. Pragma handling, environment/module/schema preparation, comparisons and every other guard retain normal source; its engine DLLs match the normal DLLs byte-for-byte.

| Fresh bounded check | Passed / failed / skipped |
|---|---|
| Complete normal QT3 gate | **31,257 / 0 / 564** |
| Complete isolated QT3 gate | **31,261 / 79 / 481** |
| Complete normal qt4 gate | **8,939 / 0 / 1,920** |
| All 87 originals, direct normal executor | **3 / 0 / 84** |
| Same originals, direct isolated executor | **4 / 83 / 0** |
| Independent public construction controls | **4 / 4 / 0** |

The isolated gate exits **2**; all 79 original assertion failures and that failed-assertion status are preserved. Exactly 83 old skips execute: four pass and 79 fail, with all **481** retained skip names/reasons unchanged and no added skips. Both normal skip maps match the owner-approved C32 adoption exactly. qt4 retains 230 gated sets and 11 experimental passes.

Three isolated passes accept XPST0003: K-ConstructionProlog-2 and qischema90007/90008. The fourth, version_declaration-005, accepts an earlier XQST0038 unsupported-collation error, independently reproduced before construction parsing. All 83 direct failures report unsupported-prolog XPST0003; four independent dependency exclusions explain the difference from 79 gated failures. Direct normal execution bypasses the runner's dependency admission, so its three parse-error passes do not mean the full normal runner admits them.

Both valid construction modes, the mixed copy/construction query and duplicate declaration fail independently through the public XQuery API. Duplicate construction requires **XQST0067**, not the observed XPST0003. Four malformed/order controls accept XPST0003. Nineteen independent lexical controls pass against both real harness assemblies, retaining pragma refusal and the already adopted context/copy grammar predicates.

## Typed-copy behavior is a separate prerequisite

A generated public-API probe validates an XSD 1.0 source using XmlSchemaSet, then copies the same source through direct element, computed element and document constructors **without a construction declaration**. It records the actual existing default behavior; it is not a modified original test.

| Metadata witness | Source | All three copied results |
|---|---|---|
| Root named complex type | Q{urn:c33}Root | Retained |
| Integer descendant | xs:int | Annotation lost; untyped value |
| IDREFS descendant | Two xs:IDREF items | One untyped item |
| Nillable child | Nilled, empty typed value | Non-nilled, one untyped item |
| ID attribute | xs:ID, is-id true | Type lost; is-id remains true |
| QName attribute | xs:QName | Type lost |

The retained is-id flag in this witness is not proof that its annotation survived: the provider also recognizes a valid unqualified attribute named id. Subsequent implementation regressions should include differently named xs:ID attributes to isolate type-driven flags.

Every copy is a distinct node, and the source's metadata snapshot remains unchanged. The copied root retains its schema annotation while descendants/attributes lose theirs. This hybrid outcome is not a qualified strip or preserve policy. Provider source confirms that root annotations are copied separately while descendant/attribute clone paths preserve selected namespace/name annotations rather than complete schema metadata. See [raw typed snapshots](evidence/typing-results.json).

**Default construction is implementation-defined**, as the immutable XQuery 3.1 REC section 4.6 explicitly states. Do not assume that an omitted declaration means strip. The next implementation must select/document a compatible default and qualify it consistently, in addition to supporting explicit strip/preserve. Preserve/no-preserve QName interactions, nilled, list typed values, IDs and declaration/type metadata need implementation coverage; accepting prolog syntax alone cannot qualify these semantics.

## Bosak.Schema ownership and seam scope

Read-only snapshots of Bosak.Schema's source audit, gap matrix, ADR-006 and S4 design preparation are pinned by document hash and repository HEAD in [schema context](evidence/schema-context.json). These are exact working-tree documentation snapshots; HEAD is informational and is not a claim that every document was committed. The audit describes the current XmlSchemaSet-typed core seam. ADR-006 records the add-on's S1-S3 work and proposes an additive callback seam; S4 design preparation remains gated on owner acceptance. Core [REQ-139](evidence/core-req139-snapshot.txt) is still **Pending**, and its ADR acceptance criterion remains unchecked. Historical gap-matrix baseline statements are not a claim that Bosak.Schema has made no implementation progress.

The observed typed-copy inconsistency is reproducible with the **existing XSD 1.0/core node-annotation interface**, without Bosak.Schema activation or an XSD 1.1 backend. Fixing construction copying must not be conflated with implementing or accepting the proposed XSD 1.1 seam. Bosak.Schema retains XSD 1.1 ownership; no sibling file, core callback contract, package dependency or licensing posture changes here.

## Evidence and next slice

All **276 final receipt-listed diagnostic processes** finish without timeout: 261 original/resource probes, eight public controls, two inventories, three full gates, one typed-copy probe and one early-error parser witness. Normal/probe exits are zero except the expected isolated full-gate exit 2. Four generated-project build logs are separate. C32's 24,010 solution passes and 76 focused passes are inherited, not fresh unit-test runs in this read-only review. Prior C32 review/adoption receipts, original fixtures/assertions, normal source and schema documentation snapshots are preserved.

[Structured reconciliation](evidence/review-analysis.json), [validation](evidence/validate-evidence.py) and [results](RESULTS.json) retain the boundaries. REQ-126 remains Complete under bounded owner acceptance; original dispositions and 48 historical unmet observations remain recorded. No release, publication, tag, push, upstream or Braid change is included.

[C34 should implement a consistent construction-mode contract](NEXT-SLICE.md), first pinning the default compatibility policy and typed-copy design. Keep normal construction refusal and exact approved gates throughout that engine slice; any later admission proposal needs separate qualification and owner disposition.

## Reproduction

Run `prepare-review.py` and `prepare-inventory.py`, build the generated construction harness/driver/inventory Release projects, then run `run-inventory.py`, `run-originals.py`, `run-controls.py` and `run-gates.py`. The isolated full gate must preserve exit 2. The typed probe's source and project snapshots are in the evidence folder; its receipt records the exact command. `analyze-review.py` reconciles every assertion, dependency exclusion and retained skip. `validate-evidence.py` verifies preserved identities and the manifest; regenerated receipts require regenerated raw identities.

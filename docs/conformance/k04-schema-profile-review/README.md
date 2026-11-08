# K04-C41 — Bounded schema-profile reconciliation

29 October 2026. Read-only review base **f0cba471**. The **37 legacy XSD-profile exclusions**, **six schema-preparation errors** and Schema's separately documented XSLT cohort remain distinct. Source, filters, fixtures, assertions, baseline and version are unchanged. Candidate **1.33.0** remains unpublished/public **1.26.3**. No seam approval is requested or inferred.

## Original evidence and current-profile companions

The actual normal QT3 inventory covers **31,821 cases / 428 sets** and exactly reproduces C40's adopted expression identities, dependencies and routing. All original queries, assertions, environments, modules, source documents and schema resources for the bounded cohorts are preserved, including **nine exact-query XSD 1.1 companions** selected by identical expression hashes.

| Bounded diagnostic cohort | Observed outcome |
|---|---|
| 37 excluded XSD 1.0 originals | **15 pass / 22 fail** directly |
| Nine exact-query XSD 1.1 companions | **All nine pass** |
| Six already-admitted substitution11 originals | **Six schema-preparation exceptions** |
| Ten independent validated-node controls | **Six pass / four mismatches** |
| Independent function-subtype control | **One mismatch** |
| Complete normal QT3 | **31,397 / 0 / 424** |
| Complete normal qt4 gate | **8,941 / 0 / 1,918** |

Both full normal skip maps exactly match adopted C40. All 37 older-profile cases retain their Unsupported dependency reasons; the six loader cases retain their separate schema exception reasons. qt4 retains 230 gated sets and 11 experimental passes. All **109 final diagnostic processes** are externally bounded and finish with exit zero/no timeout; assertion failures and preparation exceptions are separately recorded rather than treated as process success. The initial matching probe is preserved separately; its refinement only adds readable QName/local-versus-global metadata, with identical expected/actual boolean outcomes.

Fifteen direct passes do not justify admission of their older XSD-profile declarations. Of the 22 failures, **18** concern scalar, URI, character or regex assertions under a counter-profile setting. Nine exact-query pairs demonstrate opposite required assertions across the 1.0/1.1 metadata boundary and pass under the current 1.1 profile; they do not prove complete XSD 1.1 schema validation. The other **four failures** concern the existing single-head schema matching/subtyping contract and are independently reproduced below. No blanket dependency change is qualified.

## Existing core contract gaps, independent of a 1.1 backend

The unchanged **substitution.xsd** compiles successfully with XmlSchemaSet. Four original assertions fail after successful preparation:

| Original | Expected / actual | Contract implicated |
|---|---|---|
| substitution-003 | false / true | Abstract candidate declaration matched through a substitution head |
| substitution-010 | false / true | Extension blocking ignored in actual substitution membership |
| substitution-011 | false / true | Substitution blocking ignored for the requested head |
| substitution-030 | true / false | schema-element member-set subtyping in function parameter contravariance |

An independent public XPath 3.1 probe validates ten generated inputs against that exact XSD 1.0 resource, with no catalog dependency override. It reproduces abstract-candidate matching failures for A/H and A/A, blocked membership for G/H and E/D, and a function-type containment failure. Six positive/edge controls—including B under abstract head A and nilled C—retain their expected outcomes. All candidate nodes are locally declared within the validated wrapper; explicit QName and local/global declaration observations are recorded. The schema and original test fixtures are not rewritten.

The pinned [XPath 3.1 Schema Element Test and subtype rules](https://www.w3.org/TR/2017/REC-xpath-31-20170321/#id-schema-element-test) require actual substitution membership, non-abstract candidate declarations and blocking constraints. Candidate typing/nillability and schema-element set containment also matter; an abstract head can still have valid concrete members. Current `MatchesSchemaElement` follows the single-head affiliation chain without the required abstract/block checks, and its type check uses the requested head's type. The independent results and current source snapshots support a bounded existing-core repair; they do not depend on accepting the proposed Schema advisor.

**Concrete next recommendation: C42 repair actual schema-element membership and subtype handling over the existing XSD 1.0 model**, with immutable semantic adoption, new public regressions and unchanged normal profile guards. Keep the four older-profile originals excluded; prove the fix with current-profile controls. Any subsequent schema-advisor compatibility decision remains separate.

## Six failures at a different boundary

substitution-020–025 use unchanged **substitution11.xsd**, whose abstract A declaration has `substitutionGroup="s:H1 s:H2"`. All six stop during schema preparation; their query assertions never establish runtime matching behavior. A standalone framework contrast compiles substitution.xsd but rejects substitution11.xsd during **XmlSchemaSet.Add**, with the same invalid-attribute message. Reflection records `XmlSchemaElement.SubstitutionGroup` as one `XmlQualifiedName`.

[XSD 1.1 Structures §3.3](https://www.w3.org/TR/2012/REC-xmlschema11-1-20120405/) permits an affiliation list, whereas the current [.NET property](https://learn.microsoft.com/dotnet/api/system.xml.schema.xmlschemaelement.substitutiongroup) exposes a single name. Supporting this cohort requires a component representation and integration path that handle the list and its semantics. Reducing the fixture to one head would change the test and is not a correction. C41 does not verify that the add-on already supplies this feature, and ADR-006 acceptance alone would not prove these six assertions pass.

## Current Schema readiness and permanent API boundary

Six current working-tree Schema documents are pinned by content hash with sibling HEAD/status. They report S1–S3 and staged S4a/S4b/S5a/S5b progress; the S0 platform-baseline prose must not be interpreted as absence of add-on XSD 1.1 work. **ADR-006 remains Proposed**; S4 preparation still conditions S4c on core owner acceptance. Core **REQ-139 remains Pending**, and its acceptance items remain unchecked and unchanged. No sibling file, global git setting, license state or package dependency is modified.

The current seams are heterogeneous: CompileOptions/EvaluationContext and annotation/validation internals expose XmlSchemaSet and compiled schema types; IXsdValidator is a **stream-based validation facade**; IXdmNode exposes **expanded-QName annotation/declaration views** and nilled/ID properties. A future compatibility proposal must audit these actual surfaces rather than assuming every schema API is a concrete component set. It must define coverage/refusal, authority ordering, annotation lifetime, registration ownership and unregistered behavior, with no core product dependency on the add-on or license bypass.

REQ-139's preserved draft still cites the historical QT3 **31,145/0/676** baseline. Any future concrete compatibility proposal must refresh its source and no-advisor receipts to then-current gates (currently **31,397/0/424** and **8,941/0/1,918**) and account for approved core correctness fixes. C41 reports this mismatch without silently rewriting the draft. The Schema documents' **30-case XSLT mapping** is another cohort, not these six QT3 preparation errors or the 37 older-profile exclusions; it is not re-executed or certified here.

Full solution evidence is explicitly inherited from adopted C40: **24,164 passing tests**, with normal binaries unchanged. REQ-126 remains Complete under bounded acceptance with all 48 historical unmet observations preserved. No implementation, filter/baseline, fixture/assertion, package version, release, upstream or Braid change is included. Run `python docs/conformance/k04-schema-profile-review/evidence/validate-evidence.py` to verify the complete cohort ledger, resources, independent controls and scope. Ten HTML renders use unchanged Fytala CSS; no screenshot/PDF acceptance is claimed.

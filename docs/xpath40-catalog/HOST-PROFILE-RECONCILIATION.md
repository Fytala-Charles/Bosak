<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak host declaration package and profile reconciliation">
  <h1>Host and profile reconciliation — C06</h1>
  <p>26 October 2026 · published Bosak 1.26.3</p>
</div>

**C06 completes the bounded review of S0, S7 and S8-dispatch. Seven closure workpackages remain open.** All 18 retained grammar rows now have bounded review annotations; this is neither exhaustive grammar coverage nor semantic acceptance. [The machine report](HOST-PROFILE-RECONCILIATION.json) preserves source mappings, independent expectations, observations and four follow-up dispositions.

## Evidence and results

All **376 focused existing tests and 3,148 catalog examples pass**. New diagnostics exercise 60 stylesheets under explicit Legacy/Xslt40Draft profiles and 54 public registration/loader combinations: **154 observations, comprising 133 matching controls, two unmet controls and 19 context observations**. The unmet controls retain the required switch comparison error under both hosts. Context observations deliberately do not assert universal static-error timing or adoption of current F&O coercion by the XQuery 3.1 loader.

Engine, tests and executable examples remain identical to published source `08fbdc31ac79baac04cf00f4773ec6d0b2dd247f`; documentation base is `19d9e1a9`. C02's **21,798 full solution passes** and C00's frozen QT3 **31,142/0/679** and gated qt4 **8,436/0/1,894** are inherited, not rerun here. No version, gate, fixture, filter or package publication changes. The catalog remains 76 entries and 3,148 feature +24 chapter checks.

## Host and instruction mapping

| Row | Reviewed forms and evidence | Explicit remaining boundary |
|---|---|---|
| S0 | Local/root versions, processor profile, include/import/use-package, arity/visibility, static context and host registration. Retain all 66 ordinary and 114 direct module examples. Fresh ProcessorProfile, package optional-function, package type, namespace and authoring-module tests supplement them. | Complete static-error detection mode, transitive profile combinations, package version/override resolution, schema/resources and full lexical default-mode contexts need per-contract acceptance. |
| S7 | Inert note content; if test/then/else/content, lazy branches and EBV errors; map select/content/duplicates and type errors; iteration/apply-templates separators, including empty bodies. | Known forms accepted at Legacy version 3 are Bosak extensions, not portable XSLT 3. Streaming, static/package combinations and arbitrary instruction attributes are not certified. |
| S8-dispatch | Array select versus for-each member grouping, empty arrays/members and content refusal; switch sequence matching and selected-empty termination; select text/CDATA, conversion, node identity and child refusal. | H02's comparison mismatch remains. Existing native/fallback tests cover named cases, not all combinations or a complete generic node model. |

Xslt40Draft selects frozen XPath 4 independently of local stylesheet version; Legacy selects XPath 3.1 or frozen 4 from the nearest effective local version. Serialization `xsl:output/@version` does not promote expression grammar. Neither XSLT host enables Experimental `scan`; the standalone Experimental API still resolves it. Unknown-instruction fallback is checked against each processor's ceiling, rather than treating every stylesheet version 4 as forwards compatible.

## Registration and declaration boundaries

The standard library does not register `transform#1`. Public `XsltFunctionLibrary.PopulateTransformOnly(context)` supplies it, and `Populate(context)` also supplies XSLT functions and the XQuery module loader delegate. Fresh `FnTransform_*` tests exercise real transformations. This resolves C01's missing standard-library literal as a **host registration boundary**, not an unsupported function.

`load-xquery-module` differs: standard lookup resolves its dispatch stub even without a loader; invocation then raises `FOQM0001`. Installing the loader without module sources produces `FOQM0002`. The static module-source registry is copied specifically into stylesheet `use-when` contexts, where the configured module returns 12. It is not automatically copied into an arbitrary standalone runtime context. Original incomplete-setup receipts remain separate from the configured controls.

The configured static loader rejects integer 12 supplied to an external `xs:double` with `FOQM0005` under both XSLT hosts. This records the existing XQuery 3.1 loader boundary; XPath 4 calling syntax does not adopt XQuery 4 or the current F&O external-variable coercion rule. C01's function-lookup error-timing and unparsed-text fragment deltas likewise remain explicit current-draft/host policy reviews in H03. Fragment retrieval and arbitrary resource-policy combinations are not claimed tested here.

## Follow-up dispositions

| ID | Finding | Exit condition |
|---|---|---|
| H01 | Default closed-stylesheet compilation defers unknown variable/function errors until execution; guarded branches return 7. The pinned XSLT error section permits a lazy mode, while also requiring a mode that detects all static errors. These probes do not establish that complete mode. | Demonstrate the public complete-detection mode or record its unsupported boundary and scope a separate correction. Keep C04's open API context distinct. |
| H02 | `<xsl:switch select="2"><xsl:when test="true()" select="7"/></xsl:switch>` silently returns empty. Direct `2 = true()` raises `XPTY0004`. Pinned `xsl-switch` requires XPath `=` comparison semantics. Both hosts retain this unmet error control. | Independently correct atomic comparison/error behavior with regression tests and mandatory engine gates. No fix is claimed here. |
| H03 | Loader setup, error timing, external-variable coercion and fragment behavior depend on host and adopted language policy. | Review exact contracts in C09–C13; preserve versioned refusals and distinguish resolvability from invocation support. |
| H04 | Larger package, resource, authoring, static, streaming and native/fallback combinations remain bounded by named evidence. | Apply per-entry applicable evidence or explicit limitations in C09–C13. Preserve delivered fixes and every earlier open defect. |

## Reproduction and provenance

Current XSLT source is the retained immutable revision `b4723ff52fd9d4fabf2b4cd33bd44f21939c512c` in [C03 evidence](evidence/c03-type-review/manifest.json). [Exact section fragments and form mappings](evidence/c06-host-profile-review/production-sources.json) retain source conditions. This is a review pin, not a reconstructed adoption revision; C08 still owns all 28 missing historical pins.

Requests, raw results, generated diagnostic programs, source identities, original authoring/setup corrections and fresh test logs are retained in [the manifest](evidence/c06-host-profile-review/manifest.json). Rebuild and validate with:

```powershell
python docs/xpath40-catalog/build-c06-review.py
python docs/xpath40-catalog/evidence/c06-host-profile-review/validate-evidence.py --local-artifacts
```

Earlier C00–C05 raw evidence and checkpoint outputs are preserved; historical plans are reproduced separately. Rendering receipts cover HTML with unchanged Fytala CSS, not screenshot/PDF acceptance. REQ-126 and AC-01–04 remain partial. Next is [C07 lifecycle/disposition reconciliation](CLOSURE-PLAN.md); skip fixes, Braid and extension work remain deferred.

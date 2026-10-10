<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak REQ-124 consumer acceptance review">
  <br><br>
  <h1>REQ-124 — Consumer Acceptance Review</h1>
  <p>Bosak.Braid findings, expected corrections and closure evidence</p>
</div>

> **Review date:** 2026-10-10. **Requester/reviewer:** Bosak.Braid. **Implementation owner:** Bosak maintainers. **Disposition:** accepted after verification; F1/F2 closed. REQ-124 is Implemented. See section 9 for final evidence. This acceptance does not publish a release.

## 1. Scope and Reviewed Revisions

The owner requested a review of whether delivered Slices A, B and C satisfy the engine-owned REQ-124 scope. The governing contract is the [source-preserving authoring dossier](REQ-124-source-preserving-xslt-authoring.md); the [Bosak registry](FEATURE_REQUESTS.md) owns status and acceptance.

| Evidence | Revision or scope |
|----------|-------------------|
| Reviewed Bosak main | 8e29289b27b0e40f1ee3ae34b5afabdc8414abf2 |
| Slice C implementation | cdf379dcdbff2c3da1ebd6a46cffdabc54498b6b |
| Earlier review corrections / current Braid pin | 16f0ee90ea913605597ce1e5714b196526edffa3 |
| Independent engine verification | Authoring-filtered Release tests: 62 passed, 0 failed |
| Existing Braid consumer evidence | 158 tests passed at the earlier pin; inspection and licensed principal Expression drafts adopted |

Slice C adds AuthoringCapabilities, a public-API consumer sample executed as an engine test, package-placement agreement and lifecycle documentation. These are useful completion evidence. The sample targets an in-memory principal without dependencies, so it cannot detect the compilation resolver issue below. The 158 Braid tests do not establish adoption or compatibility of the new Slice C descriptor: Braid still pins the earlier revision.

No full Bosak solution/conformance rerun or released-package verification was performed in this review. The upstream Slice C handover reports Xslt.Tests 874/874, full-solution success and XSLT smoke 162/0/26; those are maintainer-reported gates, separate from the independently rerun 62 authoring tests.

## 2. Initial Acceptance Recommendation (Superseded by Section 9)

Keep the current Implemented (pending owner acceptance) disposition pending correction of F1 and F2. Both are engine-owned contract issues within REQ-124, rather than new Braid feature requests. Record the response, fix commits and regression evidence against this request before final owner acceptance.

After correction and verification, REQ-124 can be accepted as Implemented for its agreed bounded surface: source-preserving inspection, contextual Expression replacement, isolated candidates, public capability/lifecycle documentation and compatible delivery inside Bosak.Xslt. Braid's graph construction, ordered history, preview/provenance, validated export, commercial activation and desktop acceptance are separate consumer work and must not keep the engine requirement open.

## 3. F1 — Candidate Compilation Bypasses the Supplied Resolver (Closed)

**Priority:** acceptance blocker. **Contract:** dossier sections 7 and 9, AC-08/AC-10 and AC-12 regression policy.

### Observed evidence

[AuthoringEditCandidate.Compile](../src/Bosak.Xslt/Authoring/AuthoringEdit.cs) constructs a new XsltCompiler and calls Compile without configuring UriResolver. [XsltCompiler](../src/Bosak.Xslt/Api/XsltCompiler.cs) selects FileSystemUriResolver when no resolver is supplied. Consequently this public candidate compilation path does not reuse the IAuthoringModuleResolver associated with its authoring snapshot.

The earlier correction in [AuthoringInspector](../src/Bosak.Xslt/Authoring/AuthoringInspector.cs) bridges the inspection resolver for optional derived compilation during inspection. It does not cover AuthoringEditCandidate.Compile. This review established F1 by tracing those source paths; it did not run a filesystem-access reproduction. Existing passing sample tests have no includes/imports and therefore do not exercise this path.

### Impact

A stylesheet inspected and edited using an in-memory, approved or denying resolver can be compiled using different module content or an unintended filesystem source. A captured include can also fail compilation solely because no matching file exists on disk. Candidate diagnostics and the executable returned by Compile can therefore reflect different resource policies or dependencies.

Braid avoids this helper in production and accepts drafts with AttemptCompilation=false. Its test-only fidelity compiler explicitly denies module acquisition. That consumer precaution does not resolve the public engine contract issue.

### Expected change

- Route candidate compilation through the snapshot's effective IAuthoringModuleResolver, using the existing engine-owned bridge or equivalent supported internal implementation.
- Preserve module-relative base URI resolution, xml:base behavior and the XML loading compatibility already established by the inspection compilation bridge.
- Do not retry or silently fall back to filesystem resolution when an explicit resolver refuses, returns no module or fails.
- Preserve deliberate default-filesystem behavior when the snapshot was originally created with the engine's default resolver. This request does not remove that existing default.
- Compile derived state without changing input/candidate source envelopes, source ranges or node correspondence. Document resource policy and failure behavior on Compile.

Resolver authority and resource freezing are separate concerns: this finding requires consistent resolver policy. It does not require a new engine-wide frozen-resource cache; Braid's adapter already owns its captured dependency envelopes.

### Required regression evidence

| Case | Expected assertion |
|------|--------------------|
| In-memory include/import with no corresponding disk module | Inspect, propose an expression edit and call candidate.Compile successfully; output uses supplied dependency bytes |
| Explicit denial with a valid alternative module present on disk | Candidate.Compile fails according to the supplied resolver policy; it never substitutes the disk module |
| Supplied module content differs from an available disk module | Executable behavior follows the supplied module, not the disk alternative |
| Relative nested module reference/base URI | Candidate compilation preserves the resolver bridge's identity/base resolution contract |
| Default filesystem resolver | Existing deliberate default-path compile behavior remains compatible |
| Snapshot isolation | Compilation, including failure, leaves original/candidate bytes and correspondence unchanged |

Use engine-owned temporary fixtures with cleanup for disk-alternative tests. Cover include and import resolution across the suite. Existing bridge tests can supply reusable patterns; add coverage that directly invokes candidate.Compile rather than only inspection's optional compilation.

## 4. F2 — Capability Lists Expose Mutable Global Arrays (Closed)

**Priority:** acceptance blocker for the advertised read-only descriptor. **Contract:** dossier section 10 / AC-11 and Slice C's side-effect-free, read-only capability documentation.

### Observed evidence

[AuthoringCapabilities](../src/Bosak.Xslt/Authoring/AuthoringCapabilities.cs) exposes these static properties as IReadOnlyList, but initializes each with an array:

- FidelityModes
- SupportedEncodings
- DocumentedLimits

IReadOnlyList restricts access through that interface; it does not make the underlying array immutable. A consumer can cast the returned object back to the array type and overwrite its elements. Independent verification of SupportedEncodings reported backing type System.String[] and showed that changing its first element changed the value returned by a subsequent property read. The original value was restored in a finally block in that short-lived review process. No tracked file was modified by the probe.

Equivalent public-API reproduction:

~~~csharp
var exposed = (string[])AuthoringCapabilities.SupportedEncodings;
var original = exposed[0];
try
{
    exposed[0] = "changed-by-consumer";
    // Subsequent reads now advertise a value the engine never supported.
    var changed = AuthoringCapabilities.SupportedEncodings[0];
}
finally
{
    exposed[0] = original;
}
~~~

### Impact

One consumer can corrupt process-wide capability advertising for every other consumer, making discovery depend on mutable shared state. This contradicts the new read-only, side-effect-free descriptor contract. It changes advertised metadata, rather than adding actual parser support.

### Expected change

- Expose all three collections through immutable collections or read-only wrappers over privately retained arrays whose mutable backing storage is never returned.
- Array.AsReadOnly with private backing storage is sufficient; no new collection package or public API signature is required.
- Preserve element values/order, existing IReadOnlyList signatures and exception-free reads. Do not expose an alternate accessor that returns the mutable backing arrays.
- Retain safe concurrent reads and document the collection immutability guarantee.

### Required regression evidence

| Case | Expected assertion |
|------|--------------------|
| All three public collections | Mutable backing arrays are not obtainable by ordinary public casts |
| Mutation through a supported mutable collection interface | Assignment/add/remove is refused, or operates only on an independent copy; subsequent capability reads remain unchanged |
| Repeated reads and consumer copies | Values/order remain consistent; mutating a consumer-created copy cannot affect later reads |
| Existing capability contract tests | Supported encodings, fidelity values and documented limits still match delivered behavior |

Tests should exercise collection behavior, not require a particular wrapper type. Do not mutate global descriptors without guaranteed restoration in regression probes, since other tests may run concurrently.

## 5. Verification Already Performed

The independently rerun command was:

~~~powershell
dotnet test tests/Bosak.Xslt.Tests/Bosak.Xslt.Tests.csproj -c Release --no-restore --filter 'FullyQualifiedName~Authoring'
~~~

Result: 62 passed, 0 failed. The build emitted the existing ElementMap nullable warning and an AuthoringInspectionTests xUnit filtering warning; neither was introduced or fixed by this documentation review.

The descriptor mutation probe confirmed F2 directly. F1 is a source-confirmed resolver mismatch requiring the targeted runtime tests above. Passing current tests is valuable evidence but does not cover either missing regression.

## 6. Ownership and Compatibility

Bosak maintainers own fix design, source changes, tests, supported API decisions and release gating. Braid owns dependency pin/adoption, its resource capture policy, session licensing/revisions and consumer fidelity tests. Keep both findings associated with REQ-124 through the registry and dossier; a new requirement number is unnecessary for these corrections.

No mutable compiler internals, friend access, reflection-based consumer workaround, new licensing dependency or unrelated parser feature is requested. Keep the existing Bosak.Xslt package placement and Apache-2.0 notices. Braid's private product must not become an engine dependency.

## 7. Closure Checklist

- [x] Bosak records its response to F1 and F2 in the REQ-124 dossier/registry.
- [x] F1 corrected; direct candidate compilation uses the supplied resolver with no denial bypass.
- [x] F2 corrected; all three descriptor lists resist consumer mutation.
- [x] Targeted new regressions and all authoring tests pass.
- [x] Bosak runs its applicable full build/unit/conformance gates and records exact commands, revisions and outcomes, including skips or environmental exceptions.
- [x] Documentation/sample/lifecycle claims match the corrected behavior; mark registry acceptance criteria with their evidence.
- [x] Owner accepts the agreed engine scope and removes the pending-owner-acceptance qualification.
- [ ] Braid separately reviews/pins the corrected delivery and reruns its consumer suite; record this adoption without claiming a full P1 product release.

Owner acceptance and Braid adoption can be recorded separately. Remaining Braid functionality is not an additional engine closure criterion.

## 8. Related Records

- [REQ-124 dossier](REQ-124-source-preserving-xslt-authoring.md)
- [Bosak feature request registry](FEATURE_REQUESTS.md)
- [Bosak integration guide](INTEGRATION.md)

## 9. Final Re-Review and Acceptance — 2026-10-10

REQ-124 is accepted as Implemented on the owner's instruction after verification. Both findings are closed:

- **F1:** candidate Compile now uses the shared AuthoringModuleUriResolverBridge for an explicitly supplied resolver, preserving deliberate default-filesystem behavior and refusing fallback after denial. All six targeted resolver/isolation regressions pass, including valid disk alternatives, in-memory imports/includes, nested relative resolution and unchanged snapshot bytes.
- **F2:** all three capability lists are read-only wrappers over privately retained backing arrays. All three new immutability regressions pass; public casts and IList mutation cannot corrupt global advertising, and consumer copies are isolated.

| Revision | Verified relationship |
|----------|-----------------------|
| Initially tested fix | cb98af1ff05a37a59e7b1fe051b024d3436e1cf2 |
| Merged implementation | b5eb1b17249fe458095af2dc53445d459f30db11 |
| Main reviewed at acceptance | 5720f873c8f9e5ec0b3182186bf336d2b5e58a03 |

The checkout moved from the fix branch to merged main during verification. A git diff of src and tests between the tested fix and accepted main returned no changes. Acceptance therefore applies to the merged fixes; the later main commit contains documentation handover updates.

Independent verification commands:

~~~powershell
dotnet test Bosak.sln -c Release --no-restore
dotnet test tests/Bosak.Xslt.Tests/Bosak.Xslt.Tests.csproj -c Release --no-build --no-restore --filter 'FullyQualifiedName~Authoring'
dotnet run --project tests/Bosak.Xslt.Conformance -c Release --no-restore -- tests/xslt30-test/catalog.xml mode
~~~

Results: **3,709 unit tests passed, 0 failed, 0 skipped** (Xslt.Tests 883); **71 authoring tests passed**; **XSLT mode smoke 162 passed, 0 failed, 26 skipped**. Release compilation performed by these commands succeeded. Existing ElementMap CS8629 and AuthoringInspectionTests xUnit2031 warnings remain. Full QT3, qt4 and full XSLT catalog sweeps were not independently rerun during this acceptance; maintainers' existing broader regression records remain separate evidence.

The accepted scope is the agreed additive inspection/contextual Expression edit/candidate/capability/lifecycle surface in Bosak.Xslt. Structural edits, unrestricted visual coverage, browser, optimization, billing and Braid UI/session workflows are outside this engine request. Acceptance does not publish a NuGet release or establish Braid compatibility with an untested new pin. Braid remains on 16f0ee9; reviewed dependency adoption and its consumer suite are the remaining separate checklist item.
*Last updated: 2026-10-10*

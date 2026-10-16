<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala REQ-128 reviewed baseline release readiness">
  <br><br><h1>REQ-128: reviewed baseline readiness</h1>
  <p>16 October 2026 — published 1.8.0</p>
</div>

**1.8.0 published - 2026-10-16:** Ten .NET 10 libraries are published from approved immutable tag `v1.8.0` (`20a5d9be`), adding bounded source-preserving value-of insertion. Release CI passes 8,624 solution tests. Fresh nuget.org-only consumers pass 141 insertion cases, 153 broad regression checks and all 19 unchanged Braid probe outcomes; public archive/cache/output payloads and exact assembly revisions verify. [Publication evidence](RELEASE-1.8.0-PUBLICATION.json). REQ-128 engine delivery is implemented; independent Braid adoption/acceptance remains separate. VS Code 0.2.3 still bundles engine 1.7.1, and Braid retains its existing production pin. Earlier entries below are historical.

Braid's updated consumer report closes F1 for exact PR #312 head **`b0cd2ef8e7600a62464c626f86260612a81c738a`**, with no additional blocker in its focused value-of-only re-review. It independently reports 141 upstream insertion tests and 19 public API probes at that head, with the documented analyzer/warning overrides in its nested environment. This is focused technical clearance; it does not claim general static-context completeness, PR approval, runtime adoption or native UI acceptance.

The original report remains at `D:/Development/Bosak.Braid/docs/BOSAK_REQ128_CONSUMER_REVIEW.md`. The [new receipt](REQ-128-REVIEWED-HEAD-PACKAGES.json) records its current SHA-256 separately from the earlier review receipts. No Braid source, review, probe or production-pin file was edited.

## Exact reviewed-head verification

Ten local 1.8.0 packages were rebuilt at the reviewed head. All ten assemblies carry version `1.8.0.0` and informational revision `1.8.0+b0cd2ef8e7600a62464c626f86260612a81c738a`. A new consumer and package cache, with no project references, pass **141/0/0** public API cases. The nine transitive libraries restored by the consumer byte-match their package archives and fresh cache.

Braid's unchanged 19-probe program was also compiled against those packages from a separate Bosak scratch project. All 19 outcome lines match the reviewed results, including the four typed-test refusals and the successful fidelity controls. Probe output and build files were written only under Bosak's ignored scratch directory. These probes remain exploratory evidence, not a complete Braid acceptance suite.

The exact-head [CI run](https://github.com/Fytala-Charles/Bosak/actions/runs/38793664720) passes 8,624 solution tests, 84 language-server tests and highlighting checks. Previously recorded frozen gates remain QT3 31,142/0/679 and qt4 gated 8,278/0/1,894. No engine source or test changes were made during the reviewed-head readiness step. The later merged-revision verification reran both gates and retained the same totals.

## Integration and publication history

PR #312 was marked ready and merged with explicit user approval on 15 October 2026. Actual merge revision: **`20a5d9be757599848d7ceaa8885e59bea8dbb2ec`**. Its source and test trees exactly match reviewed head `b0cd2ef8`. The earlier automatic approval rejection is retained in the historical reviewed-head receipt; the subsequent explicit approval authorized this merge.

Ten packages were rebuilt from the actual merge into a separate feed and fresh cache. All 141 public API consumer tests pass, all 19 unchanged Braid probe outcomes match, and all nine restored library payloads match archive/cache/output bytes. See [merged-revision package evidence](REQ-128-MERGED-PACKAGES.json). The reviewed-head receipt remains historical and is not rewritten as merged evidence.

The actual merge [CI run](https://github.com/Fytala-Charles/Bosak/actions/runs/38805459550) passes all 8,624 solution tests, 84 language-server tests and highlighting checks. Both conformance gates were rerun: QT3 **31,142/0/679**, qt4 gated **8,278/0/1,894**, with zero gated failures.

The user separately approved publication on 16 October 2026. Immutable v1.8.0 tags the verified merge, and Release workflow 38846326481 published ten packages. Fresh public consumers verify the delivered packages; see RELEASE-1.8.0-PUBLICATION.json. This follow-up branch contains documentation only. Braid pin/runtime adoption remains separate.

REQ-128 engine delivery is Implemented in published 1.8.0; independent Braid adoption/acceptance is pending. Public libraries are 1.8.0; VS Code 0.2.3 still bundles 1.7.1. Braid keeps pin `5bab4799783569d538cec9b03d37a57454f0eebc`. Its runtime adapter, operation discovery, Apply/history and native acceptance follow supported delivery independently.

## Subsequent implementation

The supported creation operation remains one `xsl:value-of` append. Existing-source validation support is not a creation catalogue. The [structural operation roadmap](STRUCTURAL-AUTHORING-ROADMAP.md) records later candidates and the contracts they need; it does not schedule or implement them, register accepted requests or advertise additional capabilities.

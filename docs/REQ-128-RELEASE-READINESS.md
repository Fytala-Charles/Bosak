<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala REQ-128 reviewed baseline release readiness">
  <br><br><h1>REQ-128: reviewed baseline readiness</h1>
  <p>15 October 2026 — unpublished 1.8.0</p>
</div>

Braid's updated consumer report closes F1 for exact PR #312 head **`b0cd2ef8e7600a62464c626f86260612a81c738a`**, with no additional blocker in its focused value-of-only re-review. It independently reports 141 upstream insertion tests and 19 public API probes at that head, with the documented analyzer/warning overrides in its nested environment. This is focused technical clearance; it does not claim general static-context completeness, PR approval, runtime adoption or native UI acceptance.

The original report remains at `D:/Development/Bosak.Braid/docs/BOSAK_REQ128_CONSUMER_REVIEW.md`. The [new receipt](REQ-128-REVIEWED-HEAD-PACKAGES.json) records its current SHA-256 separately from the earlier review receipts. No Braid source, review, probe or production-pin file was edited.

## Exact reviewed-head verification

Ten local 1.8.0 packages were rebuilt at the reviewed head. All ten assemblies carry version `1.8.0.0` and informational revision `1.8.0+b0cd2ef8e7600a62464c626f86260612a81c738a`. A new consumer and package cache, with no project references, pass **141/0/0** public API cases. The nine transitive libraries restored by the consumer byte-match their package archives and fresh cache.

Braid's unchanged 19-probe program was also compiled against those packages from a separate Bosak scratch project. All 19 outcome lines match the reviewed results, including the four typed-test refusals and the successful fidelity controls. Probe output and build files were written only under Bosak's ignored scratch directory. These probes remain exploratory evidence, not a complete Braid acceptance suite.

The exact-head [CI run](https://github.com/Fytala-Charles/Bosak/actions/runs/38793664720) passes 8,624 solution tests, 84 language-server tests and highlighting checks. Previously recorded frozen gates remain QT3 31,142/0/679 and qt4 gated 8,278/0/1,894. No engine source or test changes were made during this readiness step, so those checks were not needlessly repeated.

## Integration and publication gates

PR #312 is still a draft at the reviewed head; the default branch has not changed. Automatic approval review rejected marking the PR ready and merging it because the earlier authorization covered preparation/review rather than shared-default-branch mutation. The rejected command did not execute. Explicit approval of that exact ready/merge action is required before integration can continue.

After authorized integration, rebuild and verify packages from the actual merged revision. The reviewed-head packages above do **not** substitute for that merged-revision gate. Present the resulting immutable candidate for separate publication approval; do not create a release tag or run the publication workflow during preparation.

REQ-128 remains In Progress for unpublished 1.8.0. Public libraries stay 1.7.1; VS Code 0.2.3 still bundles 1.7.1. Braid keeps pin `5bab4799783569d538cec9b03d37a57454f0eebc`. Its runtime adapter, operation discovery, Apply/history and native acceptance follow supported delivery independently.

## Subsequent implementation

The supported creation operation remains one `xsl:value-of` append. Existing-source validation support is not a creation catalogue. The [structural operation roadmap](STRUCTURAL-AUTHORING-ROADMAP.md) records later candidates and the contracts they need; it does not schedule or implement them, register accepted requests or advertise additional capabilities.

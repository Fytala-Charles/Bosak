<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala frozen map-to-element conformance gate repair">
  <h1>Restore the frozen map-to-element gate</h1>
  <p>24 October 2026</p>
</div>

**Final local verification:** Release build succeeds with one existing CS8604 conformance warning. All11,133 solution +84 language-server tests pass, zero failures/skips. Fresh QT3 is31,142/0/679; expanded frozen qt4 is8,434/0/1,896. Native gate skips are distinct from unit-test skips. All full TRX hashes are in the machine receipt; temporary fixture junctions are removed.



## Result

The conformance runner now gates `fn-map-to-element`, the actual native catalog name. Previously it configured `map-to-element`, which matched no catalog entry. [The prior audit](CURRENT-GATE-AUDIT.md) exposed that dormant name and verified the native frozen set separately. This repair changes one runtime membership string and appends the required source history row; it changes no engine or native assertion.

All **224 configured gate names now resolve and execute** against unchanged qt4tests revision `68910080d2d4d4092a1c6a908998ceec64ffc6d4`. The configured count stays224; executed sets increase from223 to224. The restored set contributes **156 passed /0 failed /2 skipped**.

| Manual gate | Before | After |
|---|---|---|
| Frozen qt4tests | 8,278/0/1,894 | **8,434/0/1,896** |
| QT3 | 31,142/0/679 | **31,142/0/679** |

Counts are passed/failed/skipped. The two added skips are one unsupported dependency and one existing XML-name error. The complete qt4 gate retains two overflow-error skips, adds that XML-name error, and has1,893 unsupported-dependency skips. Eleven passes still require the runner's experimental compatibility fallback. This repair does not certify those skipped or fallback boundaries.

The original [expanded frozen run](evidence/map-to-element-gate/frozen-expanded-gate.txt), [skip inventory](evidence/map-to-element-gate/frozen-expanded-gate-skips.txt), [QT3 run](evidence/map-to-element-gate/qt3.txt), control outputs and full-suite hashes are retained in [the machine receipt](MAP-TO-ELEMENT-GATE-REPAIR.json). Existing current-upstream failure receipts remain historical comparisons of the old gate membership; current upstream was not rerun or adopted here.

## Failure enforcement control

Two standalone synthetic catalogs use the exact name `fn-map-to-element`. Both evaluate `map-to-element({'a':'x'})`; the [positive assertion](evidence/map-to-element-gate/controls/happy/map-control.xml) expects `<a>x</a>`, while the [negative assertion](evidence/map-to-element-gate/controls/negative/map-control.xml) intentionally expects `<wrong/>`. These are process controls, not changes to the upstream/native suite or new normative expectations.

| Runner/control | Passed | Failed | Exit |
|---|---:|---:|---:|
| Previous runner, positive | 1 | 0 | 0 |
| Previous runner, negative | 0 | 1 | 0 |
| Corrected runner, positive, gate-only mode | 1 | 0 | 0 |
| Corrected runner, negative, gate-only mode | 0 | 1 | 2 |
| Corrected runner, negative, same full-mode setting as previous runner | 0 | 1 | 2 |

The previous negative run reports an exploratory failure without failing the process. The corrected runner enforces it in both full and gate-only modes. The intentionally failing control is successful verification of exit2; it is separate from the passing solution tests and native gate.

To rerun the published controls from the repository root:

```powershell
$env:BOSAK_QT4_GATE_ONLY = '1'
dotnet run --project tests/Bosak.XPath.Conformance40 -c Release -- docs/xpath40-catalog/evidence/map-to-element-gate/controls/happy
# Expected exit2 and one intentionally mismatched assertion:
dotnet run --project tests/Bosak.XPath.Conformance40 -c Release -- docs/xpath40-catalog/evidence/map-to-element-gate/controls/negative
```

## Scope and next work

The frozen fixture pin, gate expectations, existing skips and engine source are unchanged. Public Bosak stays1.24.0; no release or Braid update is performed. REQ-136 remains consumer-accepted; REQ-126/D2/D3/D4/AC-03 remain partial. The corrected catalog entry now cites the named frozen gate and its limits.

Next, review the smallest independent current-contract discrepancy, the empty-pattern behavior of `index-of-substring`, against its pinned specification before designing a compatibility/baseline change. Keep CSV selection/`get`, map/element option semantics, JNode behavior and date/time changes in separate bounded reviews. Current upstream remains unadopted: the prior audit's45 gated failures plus53 supplemental map-to-element failures must not be relabeled as passing evidence. No upstream contribution or independent processor confirmation is claimed.

# K04-C12: unsupported numbering-sequence proposal

27 October 2026. **Owner approval pending;normal source/baselines unchanged.**

The original format-integer-042 tests decimal fallback for the unrecognized U+FBF4 numbering sequence and declares satisfied=false. Both filters currently refuse that negative dependency,while treating its positive counterpart as admitted. The current engine does not implement this sequence:actual mechanism checks and low-value fallback/recognized-sequence contrasts verify that distinction.

The proposed bounded correction recognizes only this known unsupported sequence,using dependency type/value rather than case names. It preserves all other admission policy. A complete inventory finds the target only in the two negative originals;no positive target declaration gains a skip. The [exact adoption proposal](BASELINE-UPDATE-PROPOSAL.md) contains a reviewable patch for both filters and AGENTS.md.

Complete isolated rehearsals pass **QT3 31,162/0/659** and **qt4 8,587/0/1,892**. In each,only the original format-integer-042 leaves the skip set and all remaining names/reasons match. The normal runners remain **31,161/0/660** and **8,586/0/1,893** pending approval. There is no source,engine,fixture/assertion,schema,GatedSets or version change in this review. Source-identical22,198solution passes are inherited;full normal gates are required after adoption.

[RESULTS.json](RESULTS.json) records two original direct passes,two current normal skips,16expression controls,two mechanism observations,288filter controls and two full rehearsals. There are25small watchdog processes plus two300-second full-run watchdogs,zero timeouts. The profile comparison changes exactly eight outcomes and preserves all unrelated controls. [Skip deltas](evidence/skip-delta.json) prove the exact one-case removal per suite.

The current unsupported-sequence/fallback behavior is already the engine's behavior;this is harness metadata policy,not a new numbering-system implementation. The F&O unrecognized-sequence fallback and catalog negative-dependency semantics retain their prior pinned sources. If engine sequence support changes later,this explicit profile decision must be reviewed with it. This proposal does not claim a complete capability resolver for other dependency kinds.

The [manifest](evidence/manifest.json) protects2,520prior artifacts and all current evidence. Candidate1.28.0 remains unpublished/public1.26.3. REQ-126 stays Complete for its accepted bounded catalog;Bosak.Schema owns staged XSD1.1. No release,push,tag,Braid change or upstream submission occurs.

```powershell
python docs/conformance/k04-sequence-admission/build-review.py
python docs/conformance/k04-sequence-admission/evidence/validate-evidence.py
```

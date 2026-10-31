<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak proposed legacy adoption evidence disposition">
  <h1>Legacy adoption evidence — proposed owner disposition</h1>
  <p>26 October 2026 · proposal only; approval pending</p>
</div>

**This proposal grants no exception until the owner explicitly approves it.** It applies only to the 28 records listed in [C08's research report](ADOPTION-HISTORY-RESEARCH.md), originally inventoried with a null adopted-specification revision. [The closure plan](CLOSURE-PLAN.md) currently grants no waiver and requires owner disposition where historical evidence cannot be recovered.

## Proposed acceptance wording

For these **26 implemented/Experimental legacy records**, accept the documented absence of an exact historical specification-adoption revision as a scoped provenance exception. Keep `adoptedSpecRevision: null`, the original delivery and research receipts, and an explicit statement that the historical snapshot was not recovered. Do not substitute a fixture pin, specification publication date, feature-origin PR or later review commit for the missing adoption revision.

For these **two non-adoption records**—the deferred method-call operator and rejected `=>>` spelling—record that Bosak adopted no implementation of the documented shape. An implementation-adoption pin is therefore not applicable under this disposition. Preserve the original null fields and dossier revision; external proposal/rename history is not certified by that dossier.

All 28 records still require applicable executable/disposition evidence and semantic review against an explicitly pinned review source where relevant. Record intentional differences from that source. The exception does not approve current behavior, fix or waive any engine defect, promote a compatibility profile or gate, or close AC-01–04. C09–C13 and final owner acceptance remain required.

New implementations and future semantic changes must record the exact immutable specification source revision, affected sections and intentional deviations before implementation. This legacy exception must not become a default for new work.

## Effect if approved

C08 can close its historical-research workpackage with **zero recovered adoption pins, 26 explicit legacy exceptions and two non-adoption dispositions**. Five other closure workpackages, C09–C13, remain open. Original provenance and C00–C07 receipts remain unchanged. Missing historical facts remain missing; approval changes their acceptance treatment.

If this proposal is declined, C08 remains open unless original immutable adoption evidence is supplied or another explicit owner policy is agreed. C09–C12 can still proceed because their documented dependencies do not include C08; final closure remains subject to C08/C13.

# XSLT XML-version namespace correction - 12 November 2026

This follow-up is separate from [PR #474](https://github.com/Fytala-Charles/Bosak/pull/474), whose five required CI checks pass. Its source base is `46b3ba0421d99fa2107997229732ff925fc331b5`. Development Core/server metadata is 1.81.4, unpublished; published Core remains 1.81.2 and the extension remains 0.2.6. Original reference and output-isolation qualification receipts are unchanged.

## Problem and correction

All eight remaining XML-version failures reproduce against the sealed 1.81.3 assemblies. A read-only probe shows that the XML 1.1 loader preserves `xmlns:a=""` in provider metadata. Literal-result namespace collection then ignores that metadata and resurrects the ancestor's binding. The attribute-name hint route can do the same. Provisional undeclaration markers can also hide a child's own binding from raw XDM namespace queries even when serialization sees that binding.

Namespace collection now treats declarations and undeclarations as prefix-shadowing boundaries, while allowing a deeper explicit rebinding. Attribute hints use those same bindings. Namespace fixup reconciles provisional markers with the final element bindings and avoids duplicate markers. The XML writer omits redundant identical namespace declarations while preserving genuine rebinding and default namespace resets.

The XML 1.0 duplicate-suppression change is a valid serialization policy choice, not a claim that the specification universally prohibits redundant declarations. Raw XDM namespace bindings and XML 1.0 reconstructed bindings remain distinct: XML 1.0 cannot undeclare a nonempty prefix. The [immutable adoption record](ADOPTION.json) identifies exact specification revisions and sections before implementation. No expected result or upstream fixture is changed, and no skip or schema capability is added.

## Verification

The complete XML-version test set passes **42/42**, resolving all eight original failures. Five new unit cases verify raw bindings in both output XML versions, deeper rebinding, attribute hints, real namespace changes and default namespace resets. The same cases against the original sealed assemblies yield four expected regression failures and one pass; all five pass on the candidate. An intermediate two-failure diagnostic is retained separately.

All **26,354 solution implementation tests** pass, including 8,329 XSLT cases. The separate language-server suite passes 90 cases. Frozen QT3 remains 31,397/0/424 and selected qt4 remains 8,941/0/1,918, including 11 experimental passes. Eleven output-isolation controls preserve golden bytes and reject the three deliberately wrong/missing/stale outputs; sixteen existing profile/error controls pass.

**Full run completed; qualification unsuccessful.** The complete pinned rerun accounts for every original case:

| Cohort | Passed | Failed | Skipped |
|---|---:|---:|---:|
| Original unchanged 77301df7 reference | 10,199 | 22 | 4,380 |
| Output-isolation candidate / PR #474 | 10,211 | 10 | 4,380 |
| Namespace correction, unpublished 1.81.4 | 10,219 | 2 | 4,380 |

All eight XML-version failures are resolved, with no new failures. Only `output-0707` and `try-022` remain. All 4,380 skip identities and explanations are unchanged. The legacy console rounds its pass-rate line to `100.0%` and exits zero despite those failures; neither is accepted as success. The controller correctly records unsuccessful qualification. No full-conformance claim or gate adoption is made.

The packet at `D:/Development/Bosak/.guard-tmp/xslt30-xml-version-qualified-20261112` uses all 234 sets / 14,601 cases at `fddf1cf920087e791f13315d68dfbe874d97dc56`, basic/non-schema-aware Legacy/Xslt30 execution, the existing 1,200-second watchdog and 8 GiB process-job limit. The run completes in 776.080 seconds with zero unselected/not-run cases and zero missing sets. These diagnostic limits are not performance acceptance.

The audit confirms **zero added, changed or removed corpus files**: all 12,886 exact input files, 13,229 inventoried original-checkout files and 32 runtime files remain unchanged. Four changed source files are identified by hash separately from the base commit. The temporary idle reservation is released. See [results](RESULTS.json), [remaining failures](failures.csv), [all skip explanations](skips.csv), [sealed evidence](EVIDENCE.zip), [manifest](MANIFEST.json), and the [prepared dependent PR description](PR-DRAFT.md). Run `python docs/xslt30-xml-version/verify.py` for read-only verification.

Next review this dependent slice after PR #474, then address adaptive output `output-0707` and `try-022` under separate specification-adoption evidence. Preserve both as failures; do not turn them into skips. Correct the legacy rounded-percentage display before relying on it as a gate summary.

## Reproduction

The preparation/sealing helpers refer to this retained Windows workspace and original packets. From this candidate checkout:

```powershell
dotnet build Bosak.sln -c Release
dotnet test Bosak.sln -c Release --no-build
dotnet test tests/Bosak.LanguageServer.Tests -c Release
dotnet run --project tests/Bosak.Xslt.Conformance -c Release -- --processor-profile legacy <pinned-corpus>/catalog.xml xml-version
python docs/xslt30-xml-version/prepare.py <fresh-packet-with-LF-corpus>
python docs/xslt30-qualification/run.py --packet <fresh-packet-with-LF-corpus>
python docs/xslt30-xml-version/report.py <completed-packet>
python docs/xslt30-qualification/audit.py --packet <completed-packet> --original-corpus D:/Development/Bosak/tests/xslt30-test
```

The earlier dossiers retain their own source identities, exclusions and unsuccessful qualification results. No full-conformance claim or standalone gate promotion follows from this maintenance correction.

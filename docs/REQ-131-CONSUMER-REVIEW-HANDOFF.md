# REQ-131: independent Bosak.Braid implementation review handoff

Date: 17 October 2026. Engine source delivery is complete; this requests independent review of the delivered API and evidence. It does not claim consumer integration, native acceptance or public 1.19.0 publication. REQ-130 is independently accepted in published 1.18.0.

**Review returned ? 17 October 2026:** [Unchanged Braid report](REQ-131-CONSUMER-IMPLEMENTATION-REVIEW.md) finds no blockers and reports291/282/298 overlapping checks, plus unchanged accepted1.18 baseline1,015. [Release preparation](RELEASE-1.19.0-READINESS.md) is now the next engine step. Original request below is retained; review does not publish1.19 or accept Braid integration/native behavior.

## Immutable review target

[PR #364](https://github.com/Fytala-Charles/Bosak/pull/364), exact merge **`27691fdb3708bb4ffd7c984abbf3196b7cde5667`**. Final reviewed head `189405f2b7277b32a416323dbb45bb15fbf974c4`; its source/test trees equal the merge. Source version **1.19.0, unpublished**. Review this immutable revision rather than a moving main checkout. The acceptance-record PR changes documentation only.

Read the [implementation](REQ-131-IMPLEMENTATION.md), [contract](REQ-131-TEXT-INSERTION-CONTRACT.md) and [25-group acceptance matrix](REQ-131-TEXT-INSERTION-ACCEPTANCE.md). The source implements bounded append of one literal xsl:text in supported principal-module constructor parents, with PreserveSource default and explicit Readable mode. Unsupported scope refuses; there is no arbitrary XML fragment or subtree API.

## Review priorities

- Independently consume the public `GetTextInsertionTarget` / `ProposeTextInsertion` API, separate typed target/proposal/result/candidate and capabilities. Use a distinct consumer assembly identity and package references only. Null arguments/undefined modes throw; cancellation is a classified Cancelled result. Empty text is valid.
- Verify literal Unicode payloads, including XML metacharacters, braces/XPath-looking input, quotes, `]]>`, CR/LF/TAB, non-BMP scalars, invalid XML characters and isolated surrogates. Generated instructions explicitly set local `expand-text="no"`; no payload execution or external-resource authority is introduced. Empty payload has no synthetic text descriptor.
- Reconcile the compatibility correction: released 1.18.0 already allows existing xsl:text for value-of insertion, including some DOE/whitespace-expansion forms. Preserve those legacy successes. Only the new text operation applies its independently narrower existing-text profile.
- Check original/candidate byte bounds including BOM, UTF-16, escaping amplification and complete Readable replacement footprints, plus cancellation, source/target ownership, denied resources and unchanged input on refusal.
- Verify ordered complete SourceEdits, independent exact-byte replay, full inserted instruction/payload range and actual new subtree, all old-node correspondence and untouched bytes. Text insertion uses an old-ID to candidate-ID dictionary scoped to its two owning snapshots, like value-of insertion. Deletion's descriptor-reference map and separate merge groups are a different schema; do not interchange them or infer cross-snapshot ID identity.
- Cover repeated text/value-of/deletion lifecycles, retained old literal payloads, UTF-8 BOM/no-BOM, UTF-16 LE/BE, PreserveSource/Readable, captured 3.0/4.0 profiles and default/aliased/colliding namespace prefixes.

## Available engine evidence

Final-head CI [39035190436](https://github.com/Fytala-Charles/Bosak/actions/runs/39035190436) and integrated CI [39035670638](https://github.com/Fytala-Charles/Bosak/actions/runs/39035670638) pass **9,810** checks (9,726 solution +84 language-server), zero failed/skipped. Focused source suite **291** =65 text +177 existing insertion/readable +49 deletion. Frozen QT3 **31,142/0/679**, qt4 gated **8,278/0/1,894**, zero gated failures.

Independent package-only source-head, final-head and merged consumers each pass the same **282** checks:61 public text +175 public legacy insertion/readable +45 public deletion +one metadata check. These are overlapping runs, not additive coverage. Ten internal controls remain source-only (four new cancellation, four deletion proof, two legacy traversal); no friend assembly identity spoofing. Ten archives and nine consumed assemblies match each exact revision, package/archive/cache/output bytes, SHA-512 assets and restore-source receipts; dependency audits report no vulnerabilities.

Local review artifacts in the Bosak checkout:

| Artifact | Path |
|---|---|
| Exact-merge local packages | `.guard-tmp/req131-merged-packages/` |
| Public-API consumer fixtures | `.guard-tmp/req131-merged-consumer/` |
| TRX/log | `.guard-tmp/req131-merged-consumer-results/`, `.guard-tmp/req131-merged-consumer-tests.log` |
| Exact-merge provenance | `.guard-tmp/req131-merged-package-provenance.json` |
| Overall source delivery | `.guard-tmp/req131-implementation-delivery.json` |
| Published 1.18 compatibility probe | `.guard-tmp/req131-baseline-probes.log` |

These unsigned local candidate artifacts are not public nuget.org packages. Use a fresh scoped package cache and a feed mapping that resolves Bosak.* exclusively from the exact-merge local feed; keep restore, test and audit under the same NUGET_PACKAGES setting. Record the independently observed revision, archive/assembly hashes, actual totals and any corrections without overwriting original receipts.

## Requested review outcome and subsequent boundary

Record blocking/nonblocking findings, exact tested revision/package provenance, independent test totals, compatibility behavior and the consumer's intended complete evidence adapter. A no-blocking implementation review permits considering 1.19.0 release preparation; publication remains a separate decision. Do not change Braid's accepted 1.18.0 production pin merely to review local candidates.

Consumer integration and matrix T23–T25 follow separately: literal-input presentation, frozen review and explicit Apply under current authority, exact new-instruction selection even when empty, one-step Undo/Redo, refreshed validation/preview, expected output, restricted new-file export and independent reopen. Record actual native variants and hash observations. REQ-129/REQ-130 acceptance does not accept REQ-131.

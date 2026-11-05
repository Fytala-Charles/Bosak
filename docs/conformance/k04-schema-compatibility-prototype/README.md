# C44-R2 — Compatible schema-universe prototype

29 October 2026. Base HEAD `4e523082dea931d9377f34672ff63c57eb5dbd7b`, with the separately retained [cross-system review](../../compatibility/xdm40-2026-10-29/README.md) as an additional uncommitted input. This completes the revised isolated-prototype workpackage. **C45 production/API acceptance remains pending.**

The revised experiment admits compatible independently compiled sessions through an explicit provider-attested view. It keeps the original type, declaration, session and host source token rather than relabeling or reassessing the value in the consuming session. Query/XDM profile, XSD version, component universe/issuer and host source identity are separate.

## Result

- **123/123 controls pass.** All **12 deliberately bad variants** fail targeted controls; 13 final externally bounded processes finish without timeout. The 1,599 final control evaluations include expected mutation failures, not 1,599 passing tests.
- **32 neutral proposed types compile**, with zero warnings/errors. Compared with C44's normalized compiled proposal, eight types are added and five existing proposed types change. No public production type changes.
- A separate **BCL XSD 1.0 witness** validates an instance using a source-only `xsi:type` against a schema superset, fails validation against its compatible subset, compiles the component union, and retains the original PSVI after validating a clone. This is independent evidence for avoiding reassessment, not an XSD 1.1 add-on qualification.
- All **141 previous C44/review files** remain byte-identical. Seven previously qualified product assemblies remain unchanged. No source/test, dependency, version, baseline, skip or sibling modification is made by this slice.

Read the [revised API/compatibility proposal](API-PROPOSAL.md), [qualification and limits](QUALIFICATION.md), and [concrete owner decision](OWNER-DECISION.md). Machine-readable [results](evidence/RESULTS.json), [final run receipts](evidence/prototype-receipts.json), [compiled API diff](evidence/api-diff.json) and [source/binary identities](evidence/source-and-binary-identities.json) accompany them.

## Specification scope and reproduction

[PROTOTYPE-ADOPTION.json](evidence/PROTOTYPE-ADOPTION.json) was written before new source generation. It adopts, **only for this isolated experiment**, the immutable XDM source at qt4cg/qtspecs `3e9f3bddd38f39f8953750306f0c574a2fbd0387`, especially §§4.1.1, 4.1.4–5, PSVI mapping and type/typed-value accessors. The exact XML/hash/source identities remain in the [earlier pin](../../compatibility/xdm40-2026-10-29/evidence/spec-pin.json). The later [BCL witness scope](evidence/BCL-WITNESS-SCOPE.json) was recorded before adding those observations. Neither record adopts product semantics or replaces historical adoption evidence.

The canonical final isolated inputs are [Contracts.cs.txt](evidence/Contracts.cs.txt), [Model.cs.txt](evidence/Model.cs.txt), [Program.cs.txt](evidence/Program.cs.txt), and both project definitions in `evidence/`. To reproduce, copy these five files without `.txt` into `.guard-tmp/k04-schema-compatibility-prototype/`, retaining the recorded relative assembly references; confirm the expected existing Core binary hash before building. Then run:

```powershell
python docs/conformance/k04-schema-compatibility-prototype/evidence/build-prototype.py
# Continue only when the build receipt reports exitCode 0.
python docs/conformance/k04-schema-compatibility-prototype/evidence/run-prototype.py
```

These scripts overwrite their current receipt names. For a replay, direct them to a **new evidence directory/copy**; preserve the original receipts. `Contracts.cs` builds a BCL/Core-only declaration assembly; the executable implements the finite fake provider and alternate carriers. No commercial assembly is referenced or executed. `SchemaAtomicValues` remains an explicitly throwing API-shape placeholder; production `XdmValue` is unchanged.

## Preserved validation and next action

The existing complete engine qualification remains inherited: **24,203 solution passes**, QT3 **31,397/0/424**, qt4 **8,941/0/1,918**, 230 gated sets and 11 experimental passes. No fresh complete engine, real-consumer build/ABI, XSD 1.1 provider, native UI or performance certification is claimed. Candidate **1.33.1** remains unpublished; public **1.26.3** remains current. REQ-139/ADR-006 remain pending; REQ-126's bounded completion and 48 historical observations remain intact.

The next action is owner disposition of the exact revised candidate and guarded C45 scope. If accepted, implement it behind opt-in registration and complete actual provider/consumer/full-engine qualification before freezing or releasing the API. The separate current-draft JNode/record/map/comparison repair queue remains outside this schema slice.

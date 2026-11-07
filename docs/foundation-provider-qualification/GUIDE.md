# Preparing actual-owner workload receipts

Use QUALIFICATION-MATRIX.json to keep Core fixtures, native source, unchanged-binary consumers and actual public-provider lanes separate. PF-MATRIX.json carries the original nine required workload families. Their fixtures, baselines, budgets and accepted candidate runs remain unfilled. Historical receipts must retain their original versions and scopes; a changed Core candidate needs fresh actual-provider evidence.

The candidate fields of OWNER-RUN.template.json already bind the exact 1.81.0 source/binary manifest and eleven engine hashes. Copy the template for an actual reviewed owner run and import authorized fixture/results/configuration/budget artifacts under relative paths inside this dossier. The consumer input manifest is JSON with consumerSourceRevision and candidatePinsSha256 matching the receipt. Each binary manifest supplies consumerArtifacts as a map from artifact name to relative path/sha256 receipts; baseline and replacement consumer sets and bytes must match, with engine assemblies listed separately. Keep source and unchanged-binary cohorts distinct; an unchanged consumer must not be rebuilt. Record all observed candidate engine hashes, including Core/Runtime/Api for the typed host case, source revision, shipped baseline pin, exact query/settings, correctness, raw samples and measurement platform/method.

Run the structural checker only after supplying real artifacts:

```powershell
python docs/foundation-provider-qualification/tools/receipt_preflight.py path/to/owner-run.json
```

Artifact paths inside the receipt resolve relative to this dossier, not to the receipt's containing directory. Exit 0 means the structure/hash checks pass; exit 2 means evidence is missing or mismatched. The checked-in template intentionally refuses. The checker never runs managed workloads, authenticates a human return, accepts budgets or qualifies the provider. Its qualificationAccepted and ownerAcceptanceVerified fields always remain false. Synthetic test budgets exist only in temporary checker fixtures; they are not owner submissions.

Actual acceptance needs representative owner fixtures and agreed thresholds/tolerances, phase-separated cold/warm samples, honest allocation/retention/GC and I/O/IPC/lease methods, correct supported/refused/error/cancellation outcomes and exact loaded identities. Source/unchanged-binary/native and all required profiles must be reviewed separately. Existing local tracing costs do not substitute for these budgets.

The next native lane must first resolve immutable historical consumer/fixture artifacts and its intended current Core dependency lane in an isolated wrapper. The Schema worktree stays read only, including its no-commit/push rule. Existing assessment output can characterize its bounded legacy document route; it does not by itself implement the new public provider contracts. Apply the scoped retained-caller runner and exact candidate manifest to any later retained managed caller; do not deliberately launch unhandled negative .NET diagnostics or modify global reporting settings.

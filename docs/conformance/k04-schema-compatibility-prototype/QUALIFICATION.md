# C44-R2 qualification, evidence and limits

29 October 2026. [RESULTS.json](evidence/RESULTS.json) and [per-process receipts](evidence/prototype-receipts.json) distinguish successful controls from expected mutation failures. The final isolated build exits 0 with zero warnings/errors; each final process has an external 45-second watchdog. No final build/test process times out.

## Executed coverage

The correct model passes **123 controls** covering independent compatible sessions; same-session reuse; subset/superset and rekeyed component handoff; conflicting named/anonymous/transitive definitions; synthetic union coherence; strict issuer/endpoint ownership; anonymous derivation; multi-head, abstract, blocking and nillability rules; original PSVI and atom provenance; explicit source/host/XDM/XSD identities; closed imports/canonical hashes; supported-false/refusal/fault/cancellation; preserve/strip; snapshot and registration lifetime; parallel reads; and unchanged production-interface boundaries.

The parallel control performs 1,024 read/bind operations. It tests this immutable fake view/carrier, not thread safety of an XmlDb snapshot or a commercial provider. The existing candidate binary and consumer contracts are not rebuilt by the experiment.

| Deliberate bad variant | Targeted controls failed | Defect injected |
|---|---:|---|
| token-only | 9 | Require identical session tokens despite proven compatible definitions. |
| qname | 3 | Accept shared names while ignoring changed type/declaration definitions. |
| fingerprint | 2 | Treat equal full fingerprints as admission for an unregistered provider. |
| profile | 4 | Omit or ignore consuming query-profile binding. |
| schema-version | 2 | Omit XSD version from closure identity and coverage checks. |
| root-only | 3 | Ignore the imported closure when building authority identity. |
| storage-id | 2 | Bypass schema ownership based on presence of a host source token. |
| revalidate | 5 | Reassess the source-only actual type in the target instead of retaining PSVI. |
| union | 1 | Skip the finite model's synthetic union-only coherence constraint. |
| rewrite | 3 | Replace the original annotation/session/type with the target expected type. |
| strip | 2 | Strip only the root while retaining descendant/attribute annotations. |
| fallback | 2 | Turn owned Unsupported/NotApplicable into default success/fallback. |

All variants exit 2 because their expected-correct assertions detect the defect; these exits are required evidence, not failures of the correct prototype. The union-only constraint is an explicit **synthetic oracle**, not an implemented XML Schema UPA/content-model validator. Definition comparisons are complete only for the finite descriptor vocabulary in this model.

## Independent XSD 1.0 reassessment witness

The locally authored [common schema](evidence/common.xsd) declares simple type Base and element M of that type. The [extra schema](evidence/extra.xsd) adds Extra as a restriction of Base. The [instance](evidence/instance.xml) selects Extra through `xsi:type`.

The source component set contains common+extra; the target contains the identical common schema alone; the union is common+extra and compiles. The BCL validates the source as Valid with actual type `{urn:c44r2}Extra`. Target validation of a cloned instance is Invalid because Extra is unavailable. The original retains its exact valid actual-type object after that operation. All five witness controls pass. Errors and original/clone validity are preserved under `bclWitness` in the final baseline receipt.

This uses `System.Xml.Schema` XSD 1.0 with resolver disabled and DTD processing prohibited. It implements no new parser/validator and runs no licensed provider. The real witness proves that compatible schema interchange cannot generally be replaced by target reassessment; the finite fake bridge separately demonstrates the proposed retention mechanism. **The two are not presented as an integrated production schema-aware pipeline.**

## Execution history retained separately

The first neutral contract build passed. The first executable build reported CS1673 because a struct lambda captured its instance; copying the annotation into a local corrected the helper. The first run then returned **101/3**: three controls expected an inner ownership code as the outer error. Their corrected assertions require `ProviderFault` with the original ownership error retained as its cause; no ownership check was removed. Original build/run logs and program text are retained.

The next unbounded diagnostic runs passed 104 and then 111 controls. The first externally bounded matrix, preserved under `v1-`, passed 111 controls and detected all twelve variants. Canonical digest/issuer controls expanded the passing matrix to 118 under `v2-`. A first BCL-witness build then failed nullable checking (CS8604); a shell sequence nevertheless ran the old 118-control binary. Those receipts are retained as **`stale-after-bcl-build-*`**, excluded from BCL and final-source qualification. The nullable annotation was corrected. The final build/runner checks the true compiler exit code plus source/binary hashes before execution, preventing a repeat of that stale attempt. Final fresh receipts are the **123-control** matrix.

Do not overwrite the older receipts or infer current-source/BCL coverage from the stale attempt. [Final source and binary hashes](evidence/source-and-binary-identities.json) identify the actual final execution; [build receipt](evidence/build-receipt.json) records the successful build.

## Preservation and inherited engine qualification

[previous-identities.json](evidence/previous-identities.json) pins all 141 original C44 and compatibility-review files before new source generation. Each remains byte-identical. Seven existing product assemblies match the earlier review hashes, which matched C42's preserved binaries; `git diff -- src tests` is empty. The compiled neutral contract references only BCL assemblies and Bosak.XPath.Core. Sources are retained as `.cs.txt`/`.csproj.txt`; executable builds live in ignored scratch.

No new public product type, carrier field, package version, fixture assertion, filter, baseline or consumer dependency changes. No sibling write, activation/key operation, upstream message or release occurs. Complete **24,203 solution passes**, QT3 **31,397/0/424**, qt4 **8,941/0/1,918** (230 sets, 11 experimental passes) remain inherited evidence. Actual core integration, real Schema coverage, unchanged XmlDb/Schema/Braid consumer binaries, public-package adoption and full fresh engine gates remain C45/consumer acceptance work.

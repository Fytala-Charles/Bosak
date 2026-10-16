# Copy forward-compatible attribute audit

Audit date:16 October2026. Engine base: `974b2a31e2688b749e5bc154a869becfc234dc0b` (PR342). Source **1.14.17** remains unchanged and unpublished. This step records a reproduced compatibility defect and a concrete implementation boundary.

## Specification and observed results

The [XSLT4.0 draft forward-compatibility rules](https://qt4cg.org/specifications/xslt-40/Overview.html#forwards) enable forward-compatible behavior when the effective version exceeds4.0 and require disallowed attributes to be ignored. At versions3/4, the unknown unqualified attribute used here requires XTSE0090. The matrix compiles named-template bodies, without evaluating them, so it measures static validation alone.

| Instruction | Version3 | Version4 | Version5 | Version17 |
|---|---|---|---|---|
| Draft4 expected | XTSE0090 | XTSE0090 | accepted | accepted |
| copy | XTSE0090 | XTSE0090 | XTSE0090 | XTSE0090 |
| apply-templates | XTSE0090 | accepted | accepted | accepted |
| sequence | accepted | accepted | accepted | accepted |

The original eight-case copy/sequence probe returned **4pass/4fail**, exit1. The expanded twelve-case probe returned **7pass/5fail**, exit1. These are defect reproductions, not passing conformance results. No fixture syntax/API/assertion corrections were made; expansion adds a known caller of the shared validator. Original and expanded sources and complete logs are committed under [evidence/copy-forward-compatibility](evidence/copy-forward-compatibility/expanded-probe.cs.txt). To reproduce, copy either cs.txt into tests/Bosak.Xslt.Tests/ForwardCompatibilityAuditProbe.cs and run `dotnet test tests/Bosak.Xslt.Tests/Bosak.Xslt.Tests.csproj -c Release --filter FullyQualifiedName~ForwardCompatibilityAuditProbe`; remove the temporary probe afterward. Failing draft assertions are deliberately excluded from the normal passing suite.

## Why this requires a coordinated change

Stylesheet.ValidateInstructionTree validates copy's whitelist unconditionally. Shared ValidateAllowedAttributes suppresses checking when IsForwardsCompatibleElement is true. Both the element helper and stylesheet-wide IsForwardsCompatible use a ceiling of3.0; their XML comments explicitly retain that ceiling for frozen catalog cases declaring4.0/3.3. The sequence instruction does not use the shared whitelist in this path: its version3 acceptance is an independent validation gap, not evidence that the helper evaluates3.0 as forward compatible.

GetXPathCompatibility currently selects XPath40 from effective versions>=4.0 and XPath31 otherwise. The stylesheet version therefore participates in XPath profile selection, while the supported XSLT ceiling remains3.0. Merely replacing copy's check with the existing helper would silently accept its invalid4.0 attributes and break the negative controls added in PR341/342. Merely raising the global ceiling would alter the deliberately preserved frozen lane. Neither is a bounded correction to copy.

## Concrete next implementation

1. Introduce an explicit supported processor profile independently of the stylesheet effective version; specify defaults and API/runner behavior while preserving existing frozen3.0 callers.
2. Define a4.0 profile in which versions3/4 do not trigger forward-compatible suppression and versions>4 do; retain a3.0 profile with its own ceiling. Audit XPath profile selection and fallback effective-version defaults under each profile.
3. Inventory shared and instruction-specific validators. Route copy through the selected ceiling without bypassing checks for known options. Add the missing sequence whitelist, with valid standard/foreign attributes and invalid unqualified controls.
4. Convert the matrix into permanent happy-path and edge-case tests for both profiles. Keep existing copy negative controls. Run the complete solution, QT3 frozen gate and qt4 gate plus native suites before merging engine changes.

This audit does not claim a complete inventory of all instructions. Schema-role, JNode copying and other previously recorded constructor issues remain separate.

## Revisions and verification

Refreshed xslt40-test upstream/master: `91328fbf0fed4a5d2813eb3661808ee53cb961ba`. Refreshed qtspecs master: `72f222929768b411d8e88d96a1a8740483290078`; pinned xslt.xml blob: `59b9bdc79dca23b0c18fdd3e94130bc59037db3e`. No upstream edits or submissions.

Fresh unchanged solution: **9,335 passed / 0 failed / 0 skipped**, including XSLT6,401; separate language-server **84/0/0**. Initial sandbox HTTP-listener and absent-fixture failures, followed by a runtime-only attempt without fixtures, are retained under .artifacts. Final run used local HTTP access and existing schema fixture junctions; all processes completed and both verified junctions were removed. Final passing logs are committed with the reproductions. No implementation change or version bump.

No engine change means no new manual conformance-gate run: PR342's retained receipts remain QT3 **31,142/0/679**, qt4 gated **8,278/0/1,894**; native array155/5/20, record29/35/1, type-alias23/8/1 (all three family exits1). These are prior receipts, not fresh audit passes. Public libraries1.8.0, extension0.2.4/engine1.8.0, REQ statuses and independent Braid acceptance are unchanged.

PR342 exact-head CI run38936211003 passed and merged at `08b02fba4fe5fa73ed34dec2982740eb22bb4486`. Precursor and merge whole trees match `3d390ec99b0c7cbaa4f7055f00011ed813062fa1`; the audit branch is fast-forward aligned.

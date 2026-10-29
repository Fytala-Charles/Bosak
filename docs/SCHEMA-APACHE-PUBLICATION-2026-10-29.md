# Schema Apache/public transition — 29 October 2026

The owner approved Apache-2.0/public availability for the schema engine and explicitly selected retaining billing/key-generation infrastructure privately. The transition is complete on GitHub.

| Boundary | Verified result |
|---|---|
| Public engine | [Fytala-Charles/Bosak.Schema](https://github.com/Fytala-Charles/Bosak.Schema), repository ID 1438210599, public, GitHub license detection Apache-2.0 |
| Preserved infrastructure | [Fytala-Charles/Bosak.Licensing](https://github.com/Fytala-Charles/Bosak.Licensing), original repository ID 1381449647, private; complete original history retained |
| Public engine content | Parentless initial commit `c4058c33f83960c7937989571d6a01f4ae1e4ffc`; publication receipt commit `044d2cb`; no private Git history copied |
| Private metadata | `d5e6e37` documents the split and changes repository metadata URL; private runtime, solution and proprietary license unchanged |
| Local public checkout | `D:/Development/Bosak.Schema.Public` |
| Local private checkout | `D:/Development/Bosak.Licensing` |
| Original locked checkout | `D:/Development/Bosak.Schema` remains intact and points to the private Licensing remote; Windows refused its directory rename because another process has it open |

## What Changed

Schema factories no longer require activation. Existing public type/member signatures are retained, including legacy license compatibility DTOs and a credential-free activation shim. That shim grants no entitlement to other Fytala products. Internal credential verification/issuance, embedded product key, key-generation executable, billing/deployment material, commercial test vectors and private history are excluded from the public tree.

The public schema package uses an Apache-2.0 license expression and includes LICENSE, NOTICE and README. All 32 selected public C# files have Fytala copyright/SPDX and migration histories. Thirteen compilation/model/validation bodies match the original source after excluding headers/final blank lines; no new XSD semantics are introduced. New regressions cover the Apache boundary and valid/invalid assertion/configuration paths.

## Evidence

All 114 public schema tests pass with zero failures/skips. Strict documentation checks pass. The selected initial public snapshot has 61 files and no detected credential patterns or excluded infrastructure paths. A reflected old/new local assembly inventory has 32 public types on each side and no removed types/members; this is a signature check, not full unchanged-consumer ABI acceptance.

The local development package is `0.4.0-alpha.0` with Apache-2.0 expression metadata and license/notice/readme files. No NuGet package or new release tag was published. [Public CI](https://github.com/Fytala-Charles/Bosak.Schema/actions/runs/40851118519) passed on attempt 2 at the content commit. Its first attempt was blocked before execution by account billing limits while private; that historical attempt is preserved.

See the public [transition and publication record](https://github.com/Fytala-Charles/Bosak.Schema/blob/main/docs/APACHE-TRANSITION.md) and [machine-readable receipt](https://github.com/Fytala-Charles/Bosak.Schema/blob/main/docs/PUBLICATION.json). Local detailed build/TRX/package/audit/API receipts remain under ignored `.guard-tmp/schema-apache-evidence`; they contain no live credential operations.

## Architecture and Next Action

The shared schema implementation is now Apache-2.0. Keeping it modular still prevents dependency cycles: it references published XPath API/providers, XSLT and XQuery 1.26.3. Core must not acquire a reverse dependency merely because licensing changed.

Reassess the smallest useful core/session contract before C45. The C44-R2 32-type neutral proposal remains unaccepted; its provider/session/identity correctness evidence still applies, but a proprietary plug-in boundary no longer determines what must be public. Complete XSD 1.1 structural validation, real provider/component coverage, compatible-schema/PSVI integration and consumer source/ABI acceptance remain pending.

No core source, baseline/filter, package version, commercial billing configuration, signing material, live deployment, XmlDb/Braid dependency or Folio license was changed. Core candidate 1.33.1 remains unpublished/public 1.26.3; existing 24,203 solution passes and exact 31,397/0/424 /8,941/0/1,918 gates are inherited, not rerun for this schema-only/license slice. REQ-126 bounded completion/48 observations and pending REQ-139 remain.

Folio remains proprietary. The recommendation is to keep its substantial typography/pagination/PDF formatter commercial for now; implementing XSL-FO does not itself require Apache licensing. No Folio change was authorized or made.

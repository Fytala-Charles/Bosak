# Shared specification coverage programme

19 November 2026. Owner priority: increase specification coverage and practical
application interoperability in **Bosak and Bosak.Schema**. This supersedes the
earlier diagnostic-first priority for subsequent feature selection. Published
beta.1 and the merged diagnostic API remain valid within their declared limits.

## Baseline and evidence discipline

Core baseline `f7b6c9959b46b67919c7f4677ff67e3fbf6bc03d`; Schema baseline
`3ff8cb6d6a0f14bf81f6e0f25a498c301580dcdc`. The identical machine-readable
[matrix](MATRIX.json) is mirrored in both repositories. It contains24 feature
areas, not every normative requirement. The retained365-case Schema catalog is
indexed in [CATALOG.json](CATALOG.json); this is historical passed evidence, not
a fresh run or a W3C conformance total.

No completion percentage is justified yet. Each area distinguishes bounded native
runtime, legacy helper evaluation, descriptor-only support, explicit refusal and
unassessed work. Catalog references identify relevant test classes for audit;
they do not assert every referenced test exercises every normative clause. W3C
case IDs are intentionally empty until selected and reviewed individually.
Passing Unsupported/refusal tests is not positive feature support. Metadata is
not structural validation. Existing .NET/XPath datatype machinery is not evidence
that Schema validates those types under XSD1.1 rules.

The original six dated specification-body hashes/adoption records are preserved.
They cover the existing decimal profile only. New features need their own exact
immutable bodies, sections and intentional deviations before implementation.
The dated Namespaces1.0 Third Edition is a review target, not a new adoption.
Use XSD1.1 Structures and Datatypes together, plus their referenced XPath/XDM/
namespace rules. Do not inherit stale status claims from the historical S0 matrix.

## Ownership and dependency direction

Schema owns schema parsing/assembly, components, structural assessment, datatypes
and facets, XSD assertion context and resulting validation facts. Core owns its
XPath/XQuery/XSLT semantics, neutral XDM/provider/typed-value transport and engine
lifetime guards. Core source projects must not gain a mandatory Schema dependency.
Use current public APIs first. A missing neutral engine capability must be
reproduced and requested under CoreREQ-142, linked to REQ-139/140; do not invent a
friend/reflection bypass or automatically accept general C45/provider/freeze work.

Core's optional integration currently pins older Schema9fd4e6a against Core1.81.17,
with a recorded260-case implementation/legacy lane and nine separate public cases.
That does not qualify current Schema365 or expanded grammar. Refresh its exact
source/test/legal pin only after a reviewed Schema implementation is delivered;
keep original receipts and distinct dependency identities. A success on an untyped
transform does not prove PSVI or schema-aware transform semantics.

## Milestones and exit gates

| Milestone | Deliverable | Exit evidence |
|---|---|---|
| M0: accountable baseline | Refine24areas into clause-level requirements, map exact positive/negative/refusal cases, pin a suitable upstream XSD suite revision and redistribution terms | Every scoped claim has immutable spec/fixture/source identities, route and outcome; unassessed remains explicit |
| M1: basic interoperable decimal documents | Namespace-aware names and zero-or-more assertions in the existing bounded document shape | Prefix/default-namespace, wrong namespace, form defaults/overrides, assertion-context and assertion-free controls; external package consumer plus actual Core provider lane |
| M2: ordinary application schema components | Named simple/complex types, references; coherent string/boolean/integer/decimal and selected facets; required/optional attributes | A namespaced order/invoice with identifiers and currency can be assessed, with invalid cases and exact typed-value expectations |
| M3: structural breadth | Nested sequence/choice/all and occurrence rules, schema composition, defaults/fixed/nil; wider assertion contexts | Component constraints and instance-validation rules independently mapped; schema-aware consumers never bypass missing structural checks |
| M4: XSD1.1 breadth | Conditional types, open content/wildcards, default attributes, lists/unions, derivation/substitution/identity constraints, remaining datatypes/facets | Positive enforcement and negative normative cases; descriptor recording alone cannot close rows |
| M5: interoperable release profile | Full selected application workflows, PSVI/atomization/query/copy/export/reopen ownership where supported, published-package and retained-binary lanes | Clear documented support profile, reproducible suite results, frozen API compatibility within that profile; no general conformance claim beyond demonstrated scope |

Milestones are direction and dependency order, not dates or a full implementation
commitment in one step. Several M2-M4 areas need multiple separately adopted
slices. A coherent useful profile comes before a broad list of partially parsed
features. Existing public orders/sku examples are representative engineering
targets, not production-user acceptance and not already supported by namespaces.

## First implementation slice: M1

Prepare exact adoption for expanded-name equality, targetNamespace, qualified/
unqualified local element forms, matching and assertion name resolution. Preserve
the unqualified profile and capacity limits. Permit schemas with no assertions;
the current mandatory1..8 assertion restriction is an implementation limitation,
not an XSD requirement. Retain the upper bound and explicit refusal for constructs
outside the chosen slice. This is proposed scope; it changes no current behavior.

Before code, decide and record which xpathDefaultNamespace forms the slice admits,
how prefixed assertion child names resolve, and how imported/named components stay
refused. Do not silently strip namespaces, compare prefixes/local names only, or
promise arbitrary XPath. Schema components/facts must retain expanded identity
through original assessment and Core adapters. No general C45 seam is assumed
necessary; establish a concrete missing capability before changing Core.

Minimum paired controls:

1. Same namespace with different prefixes; default namespace and prefix variants.
2. Same local name in wrong namespace; qualified/unqualified local forms and
   form overrides; preserve the previously supported no-namespace documents.
3. Zero assertions, true/false supported assertions, and rejected unsupported
   assertion syntax/namespace context. Full refusal exposes no owned result.
4. Identical expanded type/declaration identities survive actual public Core
   assessment/atomization/read/export paths; schema namespace does not grant a
   foreign schema or node authority. Unqualified compatibility remains intact.
5. Byte/numeric/document quotas, cancellation, disposal, detached XML and caller
   ownership remain explicit. Changed diagnostics/outcomes need a compatibility
   decision, not relabelling old tests as conformant.

## Joint qualification rules

- Source/unit results, W3C normative cases, representative workflows, performance
  observations and binary/package compatibility remain separate catalogs.
- Every slice needs positive, invalid and unsupported boundaries plus at least
  one external package-only consumer without friend access and an actual Core
  integration lane. Use fixed original binaries for replacement controls.
- Schema's1.81.2 package minimum and the optional current-Core source lane are
  separate compatibility targets. Do not suppress exact identity checks or claim
  every higher dependency version from one tested combination.
- Any engine-touching slice runs the complete Core solution and applicable
  conformance gates. Preserve QT3 **31,397/0/424**, pinned standalone XSLT3.0
  **10,221/0/4,380**, and qt4 gated-set rules, names/reasons, corpus/source hashes
  and watchdogs. Changes to skip admission require their own reviewed adoption.
- Independent reference implementations may help find disagreements; only the
  adopted normative source decides behavior. Pin reference version/settings,
  record discrepancies and never treat agreement alone as conformance.
- No release tag/registry push or API freeze is implied. Keep default builds and
  unrelated application workflows stable; publication follows demonstrated profile.

## Priority and pending delivery

Specification coverage is now the feature priority. More release-controller or
packaging-only work should not displace M0/M1. Existing SchemaPR16 is green and
open; leave it intact for separate review/merge authorization. It does not block
coverage planning. Braid production licensing remains an independent paused track.

This is a local planning/baseline-audit step, not an implementation, new semantic
adoption, normative-suite run or fresh engine qualification. No percentages,
runtime test totals or production readiness have been manufactured.

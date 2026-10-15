<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala source-preserving value-of insertion implementation">
  <br><br><h1>REQ-128: engine insertion candidate</h1>
  <p>Unpublished 1.8.0 — 15 October 2026</p>
</div>

**Consumer re-review - 2026-10-15:** Braid closes F1 for exact head b0cd2ef8 with no additional blocker in its focused value-of-only review. Fresh reviewed-head packages pass 141 cases and reproduce all 19 reviewed probe outcomes. [Current readiness and remaining integration gates](REQ-128-RELEASE-READINESS.md). Integration, publication and Braid adoption remain separate; previous entries below retain their original revision context.

**Consumer review F1 - 2026-10-15:** The declared typed-test exclusion now checks step type metadata and nested document/schema tests across original and proposed slots. Untyped node tests retain support. [Correction and verification](REQ-128-CONSUMER-REVIEW-F1.md). Earlier candidate counts/receipts below describe their recorded revisions.

The user authorized the bounded [insertion contract](REQ-128-INSERTION-CONTRACT.md). Charles Korthout owns REQ-128, now In Progress for 1.8.0. This is an additive engine feature. Public libraries remain 1.7.1 and the published VS Code 0.2.3 bundles 1.7.1. Braid production pins, ports, UI, acceptance/history and native checks are separate work after public delivery.

## Consumer flow

Inspect retained source with `AttemptCompilation = false` and an explicit module resolver when inspection itself must avoid external acquisition. Default inspection still has its existing compilation/resolution behavior; insertion does not retroactively control an earlier inspection.

```csharp
var discovered = snapshot.GetValueOfInsertionTarget(parentDescriptor, cancellationToken);
if (!discovered.IsSuccess)
    return; // Surface discovered.Failure.Kind and Message.

var proposal = new AuthoringValueOfInsertionProposal(discovered.Target!, "$v");
var result = snapshot.ProposeValueOfInsertion(proposal, cancellationToken);
if (!result.IsSuccess)
    return; // Surface result.Failure; the input snapshot is untouched.

var candidate = result.Candidate!;
byte[] candidateBytes = candidate.ExportSource();
var newInstruction = candidate.InsertedNode;
var contextualSelect = candidate.SelectSlot;
```

The consumer reviews the candidate against `InputSnapshot`, its own accepted revision, entitlement and source revision. The engine does not Apply, write files, transform input or update history. A consumer must reject its own stale revisions even though a repeated proposal against the original immutable snapshot remains valid attached data.

`OriginalRange` is zero-length in original source. `InsertedRange` covers exactly the new instruction in candidate source. `NodeCorrespondence` maps every original descriptor, including document and ancestors, injectively into the candidate. `InsertedNode` and its actual `SelectSlot` use candidate identities. The map is derived from byte-range provenance, not sibling ordinals; equal-looking siblings cannot be substituted. Exported arrays are defensive copies; publicly inspected descriptor collections and correspondence are read-only.

## Eligibility and limits

An eligible parent is a top-level `xsl:template` or an ordinary literal result element reached through a literal-result-only chain beneath it. It must have an explicit closing tag and no direct significant text, CDATA or effective `xml:space="preserve"`. Existing parameters, variables, comments, PIs and trailing whitespace retain their bytes and order. The insertion occurs immediately before the actual closing tag, with no added indentation or newline.

Only one principal `xsl:stylesheet`/`xsl:transform`, version 3.0 or 4.0, is supported. Includes, imports, packages and schema dependencies refuse. `AuthoringCapabilities.SupportsValueOfInsertion` is true in this source candidate. Its independent `ValueOfInsertionSupportedEncodings` lists UTF-8 and BOM-marked UTF-16 LE/BE. XML declarations must match; XML 1.1 and other inspection encodings are outside insertion coverage. `MaxValueOfInsertionSourceBytes` is 8,388,608, inclusive, for both original and candidate including BOM. Escaped encoded length is checked before allocating emission buffers.

The new instruction reuses a nonempty effective XSLT prefix, preferring `xsl` then ordinal order. Otherwise it declares the first unused `xsl`, `xsl1`, … locally. Existing bindings never change. The expanded select text is escaped for XML, including numeric references for tab/LF/CR; braces are not doubled. Candidate inspection must recover the exact supplied expression.

## Static coverage and resource boundary

Both original and candidate receive whole-snapshot validation plus compilation under a fresh deny-by-default `ControlledResourcePolicy` and rejecting resolver. Candidate inspection explicitly disables implicit compilation and does not reuse caller options or the original resolver. Eligibility and contextual prefix selection parse the retained source through a DTD-prohibited, resolver-free reader. They never trust a derived tree that earlier inspection compilation might have preprocessed. Controlled compilation uses a separate new document, preserving that source-derived eligibility context. No transform is executed.

Insertion adds conservative static proof because the general compiler can defer some function binding until runtime. Every contextual expression/pattern is checked for namespaces and walked through an explicit AST subset; standard signatures and captured stylesheet signatures must be proven. Ordinary scalar/path/conditional/sequence/map/array, for/let/quantified expressions and literal AVTs are covered. Existing AVT expression segments, dynamic calls, arrows/pipelines, inline functions, keyword/inline namespace forms and unproven signatures return `UnsupportedCoverage`. Schema constructors and XSLT-only function signatures are not assumed present merely because runtime compilation accepts them.

Coverage guards apply to actual XSLT annotation namespaces: unqualified attributes of XSLT elements and XSLT-namespace attributes of result elements. Ordinary result attributes such as HTML `type`, and opaque foreign namespace metadata, remain eligible and preserve their original bytes.

The declared XSLT instruction subset is `stylesheet`, `transform`, `output`, `template`, `param`, `variable`, `function`, `value-of`, `sequence`, `text`, `if`, `choose`, `when`, `otherwise`, `for-each`, `comment` and `processing-instruction`. Other instructions, type/component-reference annotations (`as`, `type`, `validation`, `mode`, `default-mode`, `use-attribute-sets`), typed XPath tests/casts and existing non-whitespace text under effective expand-text are outside proven insertion coverage. This closes deferred text-template, component and type binding paths without changing the general compiler.

All `use-when`, evaluated static declarations, XSLT shadow attributes, extension-element-prefixes and fixed/type namespace contexts refuse before compiler preprocessing. This intentionally includes literal use-when forms. A resource acquisition actually encountered during controlled compilation returns `ResourceDenied`; no resolver or disk/network fallback is permitted. Host extension registrations do not extend this insertion contract.

Cancellation is cooperative: target discovery, validation, expression processing, emission, inspection and final return boundaries check the token. Compiler/parsing calls are synchronous and cannot promise hard real-time interruption. Once cancellation is observed no target/candidate is returned. Null arguments throw before cancellation; expression failures carry underlying codes when available without inventing original-source coordinates for new text.

## Verification and delivery

Source base: `5e13e77d988f559bddaf220fcc45e7d0c8a8b078`. The fixture class `AuthoringValueOfInsertionTests` contains 81 cases: exact-output goldens; preceding/global/function scope; old/new source ranges; total injective node correspondence with identical siblings; isolated repeated proposals and defensive exports; foreign equal IDs; mixed/self-closing/instruction-chain refusals; source encodings/BOM/declaration mismatch; non-BMP/trivia/line endings; namespaces/defaults/base URI; XML escaping/braces; invalid originals/expressions; unsupported static acquisition; cancellation; exact eight-MiB candidate and one-byte-over boundaries.

Focused tests pass 81/0/0. Release solution build passes. QT3 remains 31,142/0/679 and qt4 gated remains 8,278/0/1,894, with no gate membership changes. The final reviewed solution passes **8,564/0/0** (XSLT **5,678/0/0**), and the language server passes **84/0/0**. [Machine-readable execution receipt](REQ-128-IMPLEMENTATION.json) records project/TRX and log hashes. Exact-revision package-consumer evidence is recorded separately during preparation. The initial sandbox run could not start database HTTP listener fixtures; it was interrupted and preserved separately, then rerun with local network fixture access. A collection-copy interaction with an existing cancellation test was corrected by freezing children at the inspector production boundary.

The contract's 38 fixture groups span Bosak and Braid. Engine tests do not claim Braid Apply/history, source races, graph selection, controlled consumer export/reopen or native UI acceptance. Mid-processing cancellation is exercised; an exact scheduler-independent race at the final return instruction is not claimed. Compilation checkpoints and the final token check are also reviewed in source.

Retained-source correction: three cases cover prior default compilation with literal use-when, static parameters and shadow attributes. The earlier local package at `9f7daf5a` passes two and incorrectly accepts the shadow case (one intentional baseline failure); the corrected engine passes all three. [Superseded package evidence](REQ-128-PACKAGE-CANDIDATE-9f7daf5a.json) preserves its original 73-case verification and package hashes. Final exact-revision `526924f0126c3c2eb3a401e1b97930883198a9cd` packages pass all **81/0/0** public API cases from a separate consumer assembly with no project references and a fresh cache. All ten library assemblies are 1.8.0.0 with exact informational revision; nine transitive consumer archive/cache/output assemblies byte-match. [Corrected package receipt](REQ-128-PACKAGE-CANDIDATE.json).

Annotation namespace correction: five additional cases preserve ordinary result attributes and opaque foreign metadata. The prior `e3f69c94` package fails all five (intentional baseline); the final engine and package consumer pass all five. [Prior package receipt](REQ-128-PACKAGE-CANDIDATE-e3f69c94.json) is retained separately.

Next delivery gates: review and CI; rebuild/reverify the final integrated revision before requesting publication approval for immutable 1.8.0. No library or extension publication, release tag or Braid production change is authorized by this preparation record.

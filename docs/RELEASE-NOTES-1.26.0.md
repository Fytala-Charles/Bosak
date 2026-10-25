<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.26.0 conditional authoring release notes">
  <h1>Bosak 1.26.0 candidate release notes</h1>
  <p>25 October 2026 - unsigned local candidate; unpublished</p>
</div>

Bosak 1.26.0 adds the coordinated REQ-138 conditional authoring lifecycle for visual/source editors: Wrap a supported literal result element in xsl:if, edit its actual test through the bounded cancellable expression overload, and Unwrap while retaining the content. Engine-issued targets, exact paired edits and complete graph/attribute/reparenting/two-boundary whitespace provenance keep source review and consumer Apply separate. [API, supported profile and limits](REQ-138-IMPLEMENTATION.md).

The additive overload preserves the legacy one-argument editor and existing refusal values. Existing literal Create/Rename/Delete/Move ancestry restrictions remain intact. Parent-context XSLT prefix selection adds no declarations or rebinding; condition escaping preserves logical whitespace. Both modules validate under restricted acquisition policies. Independent source/condition/graph/depth/evidence budgets and cooperative cancellation are explicit.

This candidate also includes the previously unpublished 1.25.2 unary integer, modulo and exact decimal-backed idiv corrections. Their original 384 numeric regressions remain in both source and package regression coverage. `idiv` results remain Int64-bound; ordinary decimal precision/range and generic integer storage/constructor policies remain separate. REQ-126 is not closed.

Fresh immutable-merge validation: 12,116 solution +84 LSP tests pass, including all 57 conditional source cases. QT3 stays 31,142/0/679; frozen qt4 stays 8,436/0/1,894 across 224 gates. Current native Unix stays 22/2/0 with its two known negative-domain failures; no full current-draft conformance is claimed.

Ten unsigned local packages pass 153 broad and 1,493 focused package-only checks with fresh separate caches and exact source/assembly/archive/cache/output provenance. The focused count comprises 1,429 retained checks, 47 public conditional cases and 17 independent Braid probes; counts overlap source/consumer runs. All 19 consumed assembly identities match the merge, and neither fresh transitive audit reports vulnerable packages.

Braid source review found no blocking findings at `41748242`; the registry/punctuation nits are corrected. Signed public-package adoption, Braid conditional integration, atomic Apply/history/worker/export checks and native acceptance remain separate. Public install pins remain 1.25.1 pending publication approval. [Pinned candidate and complete proof](RELEASE-1.26.0-CANDIDATE.md). No tag, public upload or release has been created.

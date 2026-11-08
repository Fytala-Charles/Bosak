# Bounded provider API review — 29 October 2026

Review base: core `a2ade8fd` plus the uncommitted bridge identified by this dossier's original source-identity receipt; Schema public base `044d2cb2e67decd46e03e0f5ee7cb03029f34bd9` plus its native candidate. This is a local implementation/API review, not acceptance of the general session proposal or public API freeze.

## Contract findings

The optional provider carrier avoids a reverse dependency on Schema and leaves IXdmNode/XdmValue layouts intact. Opaque owner/state references retain original authority without equating QNames or digests. The trusted host is responsible for immutable state and for binding the correct assessed value; public attachment is not a validator or an authenticity/security boundary. No thread safety is promised for concurrently edited exported XML.

Whole-document exports own independent XML snapshots. Preserve copies retain original owner/state, strip removes them, and mutation checks reject stale values. Anonymous element-only root atomization and refusal of unsupported declaration/reassessment paths are covered by the 204-case Schema suite. Strong ownership of escaped trees is deliberate; no caller resource is disposed. Shape verification currently allocates a bounded clone/string on access; optimization requires separate evidence and is deferred.

## Correction recorded before implementation

`AttachTo` removes the previous binding before computing the replacement shape. Bounds failure therefore erases the previous binding despite throwing. Compute the complete replacement binding first, then replace the annotation. A regression must verify that an oversized failed replacement retains the previous binding: after restoring the original XML, its original owner/state and decimal remain available. A fresh-node failed attachment must leave the node unregistered.

This is exception-safety correction within the already adopted bounded API, not new XPath/XSD semantics. Normative scope and intentional deviations remain exactly those of [ADOPTION.md](ADOPTION.md): immutable XDM/XPath/XQuery 3.1 and XSLT 3.0 Recommendation editions, sections and dates recorded there. No new structural feature or schema admission is introduced. The unpublished feature candidate stays 1.34.0; there is no published patch to version.

## Checkpoint disposition

The user's instruction to continue the recommendations authorizes local checkpoint commits following review and validation. It does not authorize push, merge, tag, publication or optional-child implementation. Optional decimal children remain a proposed next slice requiring their own normative adoption record and bounded implementation plan.

Final verification passes: 24,209 core cases, 204 Schema cases and both exact gates, with unchanged skip mappings. The review identified one exception-safety correction, now covered by two added provider regressions. No broader API redesign was adopted. Host-authored annotations remain trusted input; their authenticity and deep immutability cannot be established by the opaque carrier.

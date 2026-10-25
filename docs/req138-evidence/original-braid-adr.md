# ADR-025: Bounded conditional wrapper authoring

Date: 2026-10-24. Status: **Proposed; Bosak REQ-138 registered Pending / target TBD / owner Unassigned.**

## Context

Accepted Bosak1.25.0/REQ-137 completes adjacent literal child reordering. A useful next visual operation is making an output element conditional, editing the predicate and later removing that condition while retaining its contents. Existing public Expression editing already handles xsl:if/@test; six public-package baseline probes pass. No public Wrap/Unwrap API was found in published1.25.0 or the textually audited upstream2580823a snapshot declarations.

## Proposed decision

Request one coordinated Wrap/Edit condition/Unwrap lifecycle, delivering the two missing structural operations together and reusing the existing test-slot editor. Engine-owned Wrap inserts one if around a supported literal element using a proven in-scope XSLT prefix, preserving the element bytes. Unwrap removes only eligible wrapper markup, retaining its child and optional ordinary whitespace; it is not whole-subtree Delete. The profile must explicitly handle or refuse both-boundary text merges and namespace/static-context relocation.

Bosak supplies complete source/edit and created/removed/mapped/merged node/attribute evidence, with explicit reparent/depth exceptions; Braid independently verifies it before a frozen review and separate atomic Apply. Wrap selects the new if; Unwrap selects the surviving literal child. Each Changed command is one exact-byte history step; existing byte-identical expression feedback preserves history/redo. No formatting or consumer XML splicing.

## Consequences

This exposes basic conditional output without broad arbitrary structural editing. Wrapper declarations/context attributes, unsupported ancestors/contents and unproved merge/static behavior refuse. Existing element operations must retain their ancestor/profile restrictions instead of gaining unreviewed access through if. Resource/evidence limits, cancellation, license/currentness, native acceptance and signed-release gates remain explicit. Readable formatting, choose/otherwise and arbitrary wrapper removal require separately reviewed scope.

The [complete consumer request and IF01-IF15](CONDITIONAL_WRAPPER_REQUEST.md) are registered as Bosak REQ-138 after explicit owner approval of documentation-only upstream edits. No engine work or runtime command is authorized or implemented. Maintainer scope/API review is next; this ADR remains Proposed.

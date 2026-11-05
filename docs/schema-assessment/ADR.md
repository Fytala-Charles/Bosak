# ADR-008: Bounded native document assessment

**Status:** Proposed; bounded implementation candidate prepared, public API freeze pending.
**Date:** 2026-10-29
**Deciders:** Product owner requested the assessed-document next slice; maintainer/API disposition remains separate.

## Context

The REQ-009 descriptor pipeline evaluates assertions but does not establish complete document structure or export original assessed state. A successful assertion result can therefore accept extra children. The existing core schema seam accepts BCL XSD 1.0 components; removing 1.1 constructs and calling it would not constitute native XSD 1.1 assessment.

## Decision

Prepare a separate immutable `XsdDocumentAssessor` for the explicit `decimal-sequence-assertions-v1` profile. Refuse unsupported schema, expression and representation coverage at compilation/assessment, and check complete admitted structure before assertions. Keep invalid instances distinct from unsupported requests. Do not extend `ICompiledSchema` or `IXsdValidationService`, change core interfaces, or reuse a descriptor result as a structural certificate.

Successful results retain an owned snapshot, original root/local declarations, anonymous/built-in type components, normalized decimal values and no-typed-value/nilled state. Native copies preserve those original components without revalidation. Independent equal schema digests do not authenticate shared authority. Exported XML is a separate caller-owned copy.

Initially project only assessed decimal children into the legacy engine, where the actual type is the universal built-in decimal type. This projection does not convey local declaration authority. Refuse a full typed XML document export because the BCL projection cannot faithfully represent the original anonymous XSD 1.1 root component. A caller can explicitly request an untyped document copy. A future root bridge needs separate capability/ownership qualification.

## Consequences

### Positive

- A real structurally assessed 1.1 document shape and original component state now exist.
- Invalid content and unsupported coverage cannot yield partial successful documents.
- Existing factories/interfaces, package pins and ordinary engine behavior remain intact.
- The typed-value consumer path works with published 1.26.3 and current core source.

### Negative / Accepted Costs

- The first profile excludes namespaces, imports, attributes, occurrences, alternatives, named types, open content and most XPath constructs.
- Decimal representation is bounded; valid but inexact/out-of-range values are Unsupported.
- Full typed-document/anonymous-root interchange remains unavailable; a digest or QName cannot supply the missing component contract.

## Alternatives Considered

| Option | Why not used |
|---|---|
| Treat descriptor assertion success as assessment | Does not enforce structure or lexical validity. |
| Remove assertions and validate through XmlSchemaSet | Silently changes the schema language/assessment semantics. |
| Adopt the entire earlier session proposal immediately | No concrete complete backend exists for that contract; unnecessary for this bounded result. |
| Pretend the anonymous type is xs:anyType | Loses original type/no-typed-value semantics and component authority. |

## References

- [Pre-implementation normative adoption and limits](ADOPTION.md)
- [Assessment API/profile guide](API-GUIDE.md)
- REQ-009 registry in the owning Schema worktree

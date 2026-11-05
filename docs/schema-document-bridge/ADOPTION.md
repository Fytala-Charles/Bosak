# Native document bridge adoption — 29 October 2026

Recorded before implementation. Core base: `a2ade8fd`; Schema base: public `044d2cb`, with the prior uncommitted native candidate pinned by `docs/schema-assessment/MANIFEST.json`. Owner instruction: continue the recommended whole-document/anonymous-root bridge. This is a bounded candidate, not acceptance of the earlier general session proposal or API freeze/publication.

Immutable normative editions:

- XDM 3.1, `REC-xpath-datamodel-31-20170321`, <https://www.w3.org/TR/2017/REC-xpath-datamodel-31-20170321/>, §6.2 element nodes and typed-value/type-annotation properties. Preserve original anonymous type identity, decimal typed values and absent typed value for element-only content. Do not substitute xs:anyType as the actual annotation.
- XPath 3.1, `REC-xpath-31-20170321`, <https://www.w3.org/TR/2017/REC-xpath-31-20170321/>, §2.5 types/atomization and §3.14 sequence type operations. Root atomization must report FOTY0012; universal xs:anyType matching is not a change to the actual anonymous annotation.
- XSLT 3.0, `REC-xslt-30-20170608`, <https://www.w3.org/TR/2017/REC-xslt-30-20170608/>, §3.13 input annotations, §11.9 copying, §25.4 validation. Preserve copies retain original metadata; strip removes it. Legacy reassessment cannot silently downgrade owned 1.1 coverage.
- XQuery 3.1, `REC-xquery-31-20170321`, <https://www.w3.org/TR/2017/REC-xquery-31-20170321/>, §3.9 constructors and construction-mode declaration: copy original annotations or strip them according to the declared mode, without mutating input.

## Deliberate scope

Add one optional immutable XML-provider annotation for the existing finite native profile's anonymous element-only root and built-in decimal children. Carry opaque original owner/state references; leave IXdmNode, XdmValue layout and Core dependencies intact. QName/digest equality grants no component admission. Named-declaration matching, cross-universe handoff and legacy structural reassessment refuse when original authority cannot be represented.

Bind metadata to captured XML content; edits cannot silently reuse cached typed values. Namespace fixup and ignorable element-only whitespace do not alter this profile's assessment. Copies carry original owner/state without revalidation; stripped nodes are ordinary untyped XML. No XSD 1.1 schema is stripped or submitted to XmlSchemaSet.

The new provider API requires a core minor candidate (1.34.0). The Schema feature candidate uses that dependency with an explicit local source-build override; the original public checkout and product consumer pins stay unchanged. Fresh complete solution tests and exact conformance/skip maps are required before handover. No package/tag/push or general API freeze is implied.

# Bounded native document assessment adoption — 29 October 2026

Recorded before implementation. Public Schema base: `044d2cb2e67decd46e03e0f5ee7cb03029f34bd9`. Core integration base: `a2ade8fd`. The owner requested continuing the assessed-document next slice. This authorizes preparing this bounded implementation; it does not accept the earlier general session proposal, freeze an API, publish a package or upgrade a consumer.

## Immutable normative editions

- XSD 1.1 Structures, `REC-xmlschema11-1-20120405`, <https://www.w3.org/TR/2012/REC-xmlschema11-1-20120405/>: §3.3 element declarations/assessment; §3.4 complex types/element-only content; §3.8 model groups/sequences; §3.13 assertions, especially §3.13.4.1 and §3.13.6.2. Adopt native structural assessment for the profile below, followed by assertion evaluation over validated decimal children with a parentless candidate element.
- XSD 1.1 Datatypes, `REC-xmlschema11-2-20120405`, <https://www.w3.org/TR/2012/REC-xmlschema11-2-20120405/>: §3.3.3 decimal, lexical/value mapping and whiteSpace collapse. No exponent, INF, NaN or non-XML whitespace. Valid decimal values that cannot be represented exactly by .NET decimal are Unsupported, never rounded or reported lexically invalid.
- XPath 2.0, `REC-xpath20-20101214`, <https://www.w3.org/TR/2010/REC-xpath20-20101214/>: §3.5 comparisons and §3.6 logical expressions. Use Bosak's explicit XPath20 profile for a statically checked finite assertion grammar. No new core language semantics are adopted.

## Profile and deliberate limitations

`decimal-sequence-assertions-v1`: one no-target-namespace global element, one anonymous element-only complex type, one nonempty ordered sequence of uniquely named local xs:decimal elements, all occurring exactly once, followed by at least one xs:assert. Allow namespace declarations and selected informational/schema-profile attributes only. Refuse every other schema construct/attribute, including imports/includes, named types, attributes, occurrences other than one, alternatives, nil/default/fixed/identity constraints, facets and open content. Do not strip 1.1 constructs or use XmlSchemaSet for structural assessment.

Assertions are bounded combinations of declared child names and exactly representable decimal constants using eq/ne/lt/le/gt/ge, and/or and parentheses. Functions, axes, variables, arithmetic and other XPath constructs are Unsupported. This is an intentional restricted service profile, not complete XSD 1.1 conformance. Bounds: sixteen children, eight assertions, 512 assertion characters, 128 tokens, sixteen parenthesis levels, 32 KiB schema XML, 64 KiB instance XML and 256 decimal lexical characters.

The immutable result owns its snapshot and native declaration/type/element state. Root type identity is anonymous and endpoint-scoped; a digest or QName is not interchange authority. Invalid/unsupported results expose no assessed document. Preserve native PSVI on explicit assessment copies; stripped XML copies expose none. Legacy engine consumption initially exports validated decimal children with built-in decimal annotations. A request for a fully typed document copy is explicitly unsupported because the legacy BCL projection cannot faithfully represent the anonymous XSD 1.1 root component. No silent whole-document projection or compatible-schema admission is introduced.

Schema/instance mutation after compilation/assessment must not affect retained state. Exported XML is caller-owned; assessment validity refers to the immutable retained snapshot, not later edits to exports. Cancellation throws OperationCanceledException and never yields a successful partial result. No external resources, license operation, general core/session API, gate admission or release is authorized by this slice.

Implementation scope clarification before qualification: decimal leaves containing comments or processing instructions are explicitly Unsupported. Root-level comments/processing instructions remain ignorable in element-only content. This avoids changing comment/text order when exporting schema-normalized decimal leaves. CDATA is text in the XML data model and remains supported.

# Open Schema integration scope — 29 October 2026

Recorded before implementation, against core `5f2dec11c16a7287f45f717779d6f9e92534eb30` and public Schema `044d2cb2e67decd46e03e0f5ee7cb03029f34bd9`.

The owner agreed to a bounded real-provider integration on 29 October 2026. This authorizes this development example and qualification, not acceptance of C44's proposed 32-type public contract, an API freeze, release, consumer dependency upgrade or storage migration. Historical pending decision receipts remain unchanged.

## Immutable specification sources and sections

- XSLT 3.0, `REC-xslt-30-20170608`: <https://www.w3.org/TR/2017/REC-xslt-30-20170608/>, §3.14 schema import, §11.9 copying, §19 schema validation and §25.1 schema-aware processors. Exercise existing import/strict validation and preservation/stripping behavior; introduce no new engine semantics.
- XPath 3.1, `REC-xpath-31-20170321`: <https://www.w3.org/TR/2017/REC-xpath-31-20170321/>, §2.5.5 sequence types, §3.12.1 instance-of and §3.5 comparisons. Observe typed atomization and the existing no-schema baseline.
- XML Schema 1.1 Structures, `REC-xmlschema11-1-20120405`: <https://www.w3.org/TR/2012/REC-xmlschema11-1-20120405/>, §3.13 assertions. Exercise the existing staged assertion service separately; no full structural 1.1 assessment is claimed.

## Deliberate limits

The complete document-to-typed-transform example uses an explicit XSD 1.0 schema. The XSD 1.1 assertion service does not export a fully assessed document/PSVI; its successful result cannot certify structural validity. Never strip 1.1 features and route their schema through the 1.0 example. Source integration compiles the actual public Schema files against current core projects, without copying or modifying the sibling checkout. It is a development harness, not a distributable replacement Schema package.

No new core API, `XdmValue` layout, node interface, language profile, schema-session interchange, conformance admission or package version change is needed for this first path. Source builds, unchanged-binary substitution and full consumer product acceptance must be reported separately.

**Later reference correction:** [Verified XSLT section numbers](SPEC-REFERENCE-CORRECTION.md) correct the original numbering above without rewriting this pre-implementation record or changing its scope.

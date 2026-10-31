# Specification reference corrections — 29 October 2026

The pre-implementation [scope record](ADOPTION.md) correctly pinned the dated XSLT recommendation but gave incorrect section numbers for schema import, validation and conformance. Preserve that original record as history. The verified references in `REC-xslt-30-20170608` are:

- §3.15, Importing Schema Components.
- §11.9, Copying Nodes (unchanged).
- §25.4, Validation, including §25.4.1.1 validation attributes and §25.4.1.3 assessment.
- §27.2, Schema-Awareness Conformance Feature.

Verification read the full dated W3C HTML document; its downloaded bytes are identified in the qualification results. The original §19 concerns streaming, not the validation process, and §25.1 is not the schema-awareness conformance feature. This correction changes reference metadata only; it introduces no implementation or semantic change and does not widen the original bounded XSD 1.0 example/XSD 1.1 assertion-test scope.

XPath 3.1's instance-of section is **§3.14.1**, not the original §3.12.1; §3.12 is conditional expressions. The dated recommendation and intended instance-of behavior are unchanged. XPath §2.5.5 and §3.5 and XSD 1.1 §3.13 were confirmed. This later verification is distinct from the original adoption timestamp.

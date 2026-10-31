# K04-C39 — Review independent dependency-profile exclusions

28 October 2026. Pragma adoption closes the reviewed grammar-refusal cohort. Normal QT3 is **31,395/0/426**; all 370 remaining Unsupported dependency cases are independently excluded under the current XPath and XQuery profiles with routing allowed.

Perform a read-only review of those 370 cases using the actual dependency records and singleton filter observations. Distinguish intentional earlier-version applicability, unsupported optional features, negative dependencies on admitted capabilities, frozen legacy profile policy and evidence that merits a bounded correction. Keep overlapping blockers explicit and preserve original assertions/resources. Select the next small slice only after that evidence establishes a real implementation or policy issue; do not broaden exact old-version admission or flip a negative dependency merely to reduce the skip count.

Keep the 56 other skips separate, especially the six existing schema-loader errors. Consult current Bosak.Schema documentation for schema ownership and readiness; ADR-006/REQ-139 remains separately pending, and core XmlSchemaSet limits do not establish XSD 1.1 backend capability. Do not accept or implement the callback seam as part of a skip-filter change.

Keep normal source, guards, dependency filters, fixtures, assertions, version and baseline unchanged during review. Any subsequent baseline update needs its own exact qualified proposal and owner approval. No release, upstream submission or Braid change is included. Candidate 1.33.0 remains unpublished; public 1.26.3.

# K04-C01: proposed next implementation scope

The next bounded slice should correct raw triple-double-quote routing without broadening prolog, version or schema applicability.

Start from the pinned current source and [16 exact candidates](evidence/quote-candidates.json). Verify lexical doubled quotes, direct-constructor attribute/content syntax, comments, CDATA, PI payloads, string constructors and serialization declarations. Preserve every original assertion. The five successful public-XQuery samples are leads; the remaining candidates require complete environment/output-option handling before any pass claim.

Retain the existing context/construction/copy-namespace/pragma guards, dependency predicates and schema-profile exclusions. Keep the ten unmet public-XQuery observations from this audit as negative boundaries. Add meaningful positive and invalid lexical controls before implementing the routing correction.

Prepare a concrete reviewable change and resource-bounded full-runner rehearsal. The current exact QT3 requirement is **31,145/0/676**. Any admission/baseline proposal must state its exact case/skip delta and satisfy the existing owner-disposition rule before normal adoption; a diagnostic pass alone does not authorize that change. Do not assume all 16 candidates are already supported or that a malformed-input error proves a whole construct.

K04-F03/F04 capability-profile work remains separate. Bosak.Schema owns XSD 1.1, and correcting the source comments must not silently enable or disable a profile. No release or upstream submission is included.

# K04-C32 - Exact copy-namespaces baseline proposal

28 October 2026. **Owner approval pending.** Qualification is complete; adoption is not applied.

Approve the [exact two-file patch](PROPOSED-ADOPTION.patch): remove only copy-namespaces from the QT3 unsupported-prolog alternatives, append history row 0.30, and update AGENTS.md's frozen expectation from **31,208/0/613** to **31,257/0/564**, with adoption provenance. The [proposal identities](evidence/proposal-identities.json) pin the source base, exact patch and proposed snapshots.

The complete catalog inventory covers **31,821 cases / 428 sets**. Exactly **49** predicates change across five sets, matching C31's unchanged qualified originals. All 49 original assertions pass directly under the prototype. The complete isolated gate passes **31,257/0/564**: only these 49 Unsupported dependency skips leave the gate, and all **564** retained names/reasons are identical. See [exact names](evidence/skip-delta.json).

Fresh normal gates remain **31,208/0/613** and **8,939/0/1,920** (230 qt4 sets, 11 experimental passes). Thirty-seven independent lexical controls pass under both harnesses. All 87 construction/mixed-mode predicates retain their refusals; the 43 admitted context predicates and independent dependency exclusions remain unchanged. The proposed predicate is identical to the isolated rehearsal; its history row describes adoption instead of diagnostics.

This changes no engine, dependency filter, KnownSkips, GatedSets, fixture, assertion, schema backend, qt4 runner, package version or release. Candidate **1.31.0** remains unpublished; public **1.26.3**. Bosak.Schema retains XSD 1.1 ownership. REQ-126 bounded closure and its historical observations remain intact.

**Why approval is required:** [AGENTS.md](../../../AGENTS.md) currently requires the QT3 gate to stay exactly **31,208/0/613**. Owner disposition is required before changing that explicit baseline requirement.

After explicit approval, apply only these two proposed production files, synchronize the canonical adoption notices, rebuild, run all solution tests and fresh complete normal QT3/qt4 gates, and verify exactly these 49 removals with every retained reason unchanged. If any result differs, preserve the failed receipt and stop adoption. No release is authorized.

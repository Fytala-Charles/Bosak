# Equality override compatibility review

31 October 2026. Read-only review of the complete rebuilt isolated cohort, compared with the sealed seven-assembly 1.35.0 product cohort.

| Observation | Product | Candidate |
|---|---:|---:|
| Exported types in the seven foundation assemblies | 78 | 78 |
| Public declared reflection members | 1,199 | 1,201 |
| XdmValue size on this x64 runtime | 48 bytes | 48 bytes |
| Public diagnostic transport factories | 0 | 0 |

The only declared member additions are `Boolean Equals(System.Object)` and `Int32 GetHashCode()` on `Bosak.XPath.Core.Xdm.XdmValue`. There are no removed declared members. Both methods already existed through inheritance from `System.ValueType`; they remain virtual, non-final, and occupy the same base slots rooted in `System.Object`. All exported types retain the same inherited public reflection signature lists. These are signature/slot observations, not a claim that every reflection attribute is identical.

The observable difference is `MethodInfo.DeclaringType`: the candidate returns XdmValue for these two methods. A consumer enumerating `DeclaredOnly`, counting declared members, inspecting declaring modules or asserting the original ValueType declaration will observe the change. The candidate must therefore not be labeled an unchanged declared manifest. No new provider-facing schema types or helpers are exported.

Ordinary values retain the previous ValueType equality/hash implementation. Evidence-bearing values compare/hash their ordinary fields without treating the evidence marker as data. The full atomic probe exercises equality/hash and value transport across 39 fixtures; the unchanged retained caller supplies binary execution evidence for its 20 cases. Complete unit and conformance results are in the sealed RESULTS.json and original receipts. Passing those cases does not establish every possible consumer reflection assumption, native AOT/architecture behavior or sibling package compatibility.

Recommended disposition: retain both overrides together. They preserve ordinary caller signatures and avoid making private evidence affect default CLR equality and hashing. Any production promotion must explicitly record the two declared-manifest additions and their reflection effect. This is a technical recommendation; this dossier does not accept C45, a public Schema provider SDK, owner performance budgets or the final API freeze. Existing owner qualification requirements remain in force.

The complete rebuild changes assembly bytes compared with earlier mixed-DLL trials. Its source archive, three exact overlays, canonical ten-assembly layout, actual load observations and fresh retained-caller hashes identify this cohort. Earlier mixed-cohort performance measurements remain scoped to their original hashes. The fresh atomic probe records costs for this cohort separately; no cross-cohort latency acceptance is inferred.

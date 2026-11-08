# Exact numeric integration audit

4 November 2026. Base 7ed0659c / 1.46.0. [Pinned call sites](evidence/CONSUMER-AUDIT.json) record 192 direct legacy IntegerValue/DecimalValue references across 13 source files. This inventory distinguishes legacy checked consumers from future exact-domain routing; it does not claim those routes are migrated.

| Area | Disposition in this slice |
|---|---|
| XdmValue instance storage | Retain the six original fields. Small integers remain inline; private typed numeric holders distinguish payloads from evidence markers in the reference slot. |
| Direct numeric EBV | Read exact zero/nonzero without narrowing; legacy integer/decimal fast paths remain. |
| Legacy integer/decimal access | Checked Int64 / 96-bit coefficient and scale <= 28 conversion. Refuse before formatted parsing, rounding or padding. |
| New exact accessors | Full payload reads, including exact views of existing legacy values. Cached holder reads allocate nothing in the qualified fixture. |
| Representation and marker binding | Equivalent fresh holders match; changed numeric payloads fail rebinding even with a copied valid marker. Strip/replacement keeps payload and annotations. CLR equality/hash stay evidence-neutral and representation-based. |
| Core map keys / Runtime comparisons | Retain checked legacy getters. Representable exact values work; wider keys refuse rather than collapse to inline zero. Wider numeric equivalence/ordering is pending. |
| VM and standard library | Existing arithmetic, aggregates, casts, indexing, formatting and date/time constructors remain checked legacy routes. No host numeric enablement is inferred. |
| XSLT | Transform/sort/key/pattern/output/invocation conversions remain checked legacy consumers. Wider typed/all-host producers and admission remain pending. |
| Serialization and XdmValue.ToString | Keep checked legacy formatting; values outside CLR numeric capacity throw. Explicit full integer formatting uses BigIntegerValue; exact decimal formatting requires ExactDecimalValue.ToInvariantString with an output bound. |

[Source compatibility probes](evidence/SOURCE-COMPATIBILITY.json) retain one intended compile refusal: FromDecimal(default) becomes ambiguous with the reviewed overload. Use default(decimal) to retain the old call. The typed-default consumer compiles all four old/new factory forms. This is a known source migration requirement despite retaining old binary signatures; owner source/unchanged-binary qualification and final release SemVer are still pending.

No XdmValue field/layout, public evidence factory or friend grant is added. No public schema provenance or full numeric query profile is delivered here. Rebuilt full solution/frozen gate receipts qualify existing behavior and the bounded carrier/ownership controls, not arbitrary numeric semantics or actual provider/native acceptance.

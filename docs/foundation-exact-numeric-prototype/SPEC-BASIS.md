# Exact numeric prototype basis

31 October 2026, recorded before execution. This isolated experiment follows the [pinned numeric/host basis](../foundation-c45-domains-hosts/SPEC-BASIS.md): XSD 1.1 Datatypes REC-xmlschema11-2-20120405 sections 3.3.3, 3.4.13, 5.1 and 5.4, and XPath 3.1 REC-xpath-31-20170321 sections 3.1.1 and 3.5. No new production specification adoption is claimed.

Exercise finite coefficient digits and nonnegative decimal scale, normalize trailing coefficient zeros, canonicalize zero, compare/hash numeric equivalents, refuse capacity separately from invalid arguments, format only after a length check and narrow to legacy types only when exact. This is representation/accessor work, not arithmetic, lexical parsing, schema validity or all-host execution. Caller-created BigInteger allocation occurs before this prototype receives it; these quotas do not bound upstream parsing memory. No rounding fallback is permitted in the prototype.

The experiment's carrier is a layout model with separate payload/evidence storage, not a replacement for XdmValue or an authenticated issuer. Preserve all qualified engine hashes. Run caught refusal cases under the scoped runner error mode, build only the scratch probe and measure warmed allocations without imposing a consumer performance budget.

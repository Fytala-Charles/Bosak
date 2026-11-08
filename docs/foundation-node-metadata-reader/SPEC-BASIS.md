# Internal metadata-only original facts adoption (TA-13)

5 November 2026. Implementation base `ec6daa0e538fe3c4e2ebc54177a1daeda5834b38`, version 1.61.1; candidate feature version 1.62.0. Recorded before production edits.

Adopt only the original metadata subset of XDM 3.1 Recommendation 21 March 2017, exact edition https://www.w3.org/TR/2017/REC-xpath-datamodel-31-20170321/ sections 5.8 (nilled) and 6.2.2 (element properties), with the optional Boolean behavior basis of F&O 3.1 Recommendation 21 March 2017 https://www.w3.org/TR/2017/REC-xpath-functions-31-20170321/#func-nilled section 2.2. No later draft or errata is adopted. F&O section 2.4 atomization is excluded from this slice.

Internal host policy: explicit metadata-only reads authenticate the original node/facts/input/origin tuple, original type and optional declaration, absence/nilled consistency and live operation. Typed-value observation state is NotRequested (new internal value 3); existing Absent/Unsupported/Supported values and full-read default remain stable. Metadata-only skips ReadTypedValue, all outcome/payload/container/iterator callbacks, regardless of absence. It does not assert that nilled implies an observed empty typed value. Existing rejection of nilled+absent and nilled non-elements remains. Type/evidence guards remain active on every retained observation, with no cross-observation cache.

This implements the immediate prerequisite selected by TA-12, not the public nilled profile. No public descriptor, compiler route, advisor bridge, API, native acceptance, owner budget or freeze is activated. New cases cover poisoned typed callbacks, metadata authenticity, cancellation/close/drain, retained revocation, retry and cold/warm allocation. Full reader regressions remain required.

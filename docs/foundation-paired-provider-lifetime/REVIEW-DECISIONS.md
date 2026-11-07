# Implementation refinements after the TA-21 selection

These are later internal implementation decisions; the pre-edit ADOPTION.json and historical TA-21 policy are preserved unchanged.

- Paired acquisition is transactional. Failure of the first/second Enter or its retention check releases all successfully acquired members before returning the call failure. It faults paired admission for that raw operation; retry needs a fresh raw operation. No provider membership or active lease can silently reopen after failed cleanup.
- Resource membership is immutable and mode-specific. Deduplication affects real leases only and is not evidence that two schema universes match.
- Neutral retained-source admission uses the original input identity and real original snapshot lifetime. Retention precedes the first input callback; target/source guards bracket callback delivery. No fake source wrapper or schema/proof authority is created.
- MC06/MC15 are qualified for internal mechanics, not actual Establish/view/public proof publication. CV01-CV32 and the proposed API result/entry remain separate.
- Escaped provider call failures belong to the call's observed task; raw completion waits for its actual work/cleanup. Supported callers must keep/await authorized use inside the raw callback. No unhandled .NET negative caller or machine-wide error setting is used.

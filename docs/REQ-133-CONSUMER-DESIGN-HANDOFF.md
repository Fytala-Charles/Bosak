# REQ-133 consumer design review handoff

18 October 2026. **Proposed design; Braid review pending.** No replacement implementation, capability or release. Baseline is published 1.20.0 at91f1af0d488bb2ed72b219118eeb1ab3570726bd; documentation base af76b1e6c3b2f22e6a2783ab95cc93d237301454 closes REQ-132 acceptance.

Review the [contract](REQ-133-TEXT-BODY-REPLACEMENT-CONTRACT.md), [28 planned groups](REQ-133-TEXT-BODY-REPLACEMENT-ACCEPTANCE.md) and [unchanged request](REQ-133-literal-text-body-replacement.md), SHA-256 `ba50ab5c3761f0c193e72ffc3467a950b08744d5a7b30c41bf9ba57db987a22e`. [Baseline evidence](REQ-133-DESIGN-BASELINE.json) contains seven existing public inspection/runtime probes, zero replacement tests.

Please assess these concrete proposed decisions:

1. Separate TextBodyReplacement capability/opaque target/proposal. No insertion/deletion handle reuse, formatting mode, self-closing expansion or control-attribute editing.
2. Three exclusive outcomes: Changed with Candidate, NoChange with its own immutable original-owner result, Refused with Failure. No generic IsSuccess that implies Candidate. Equality is ordinal after supported target/original/Unicode validation, before hypothetical canonical-output sizing; no lexical rewrite or history.
3. Paired ordinary xsl:text under captured principal template/LRE coverage, with effective non-expansion and no output escaping. Retain xml:space and allow supported preservation/mixed surrounding text instead of inheriting append-layout restrictions. Additional conservative scope refusals must be explicit.
4. Exactly one body SourceEdit, actual original/replacement spans, unchanged outside bytes and exact encoded payload/caret. Numeric CR/LF/TAB references preserve requested code points; logical input is never interpreted as markup, XPath or an entity reference.
5. Survivor map excludes the explicitly replaced old payload even for nonempty-to-nonempty changes. ReplacedNodes/NewNodes are actual zero-or-one lists with complete disjoint partitions; owner/parent and all outside identities survive. No total old-node-map assumption or merged-text relation is imported.
6. Original and actual final physical source each bounded to8,388,608 bytes; subtract old body before final-size checks, and keep NoChange independent of hypothetical escaping growth. Original source over the bound still refuses.
7. Operation-specific failure enum preserves old APIs. Consumer session/license/revision, frozen complete review, atomic Apply/history, stale/late cancellation and worker/native acceptance remain Braid responsibilities.

Return blocking findings and corrections against the immutable design commit, with any actually executed probe counts and scope limits. A no-blocking design review authorizes neither engine implementation nor publication/native acceptance. Implementation needs subsequent user authorization. No external message has been sent by preparing this handoff.

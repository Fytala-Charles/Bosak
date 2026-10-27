# Proposed K04-C12 unsupported-sequence adoption

**Pending owner approval - 27 October 2026. Normal source and AGENTS.md remain unchanged.**

Decision ID: **K04-sequence-adoption-v1**. Source base7d2e15b3;candidate1.28.0 unpublished,public1.26.3.

Approve the [exact three-file patch](PROPOSED-ADOPTION.patch): update the two dependency filters to treat the reviewed U+FBF4 numbering sequence as unsupported,with positive refusal and negative admission;append dated source history0.17/0.7;update AGENTS.md's exact QT3 expectation from **31,161/0/660** to **31,162/0/659**.

The rule is keyed by dependency type/value,not test name. It admits the original format-integer-042 in both harnesses through its existing negative dependency. No fixture,assertion,known-skip dictionary,GatedSets,engine API/semantics,package version or schema policy changes. Other sequence names and dependency kinds retain existing policy.

## Complete evidence before approval

- The original assertions pass directly in both profiles;the unchanged normal runners currently skip both.
- Sixteen expression controls pass,covering fallback for zero,one,two,ten,original and negative values,with recognized circled/Arabic sequence contrasts.
- Actual private formatter mechanism reports FBF4 unsupported at value1 and a recognized circled sequence supported. This is not merely an out-of-range fallback observation.
- All78,806 catalog cases are inventoried. There are18 numbering-sequence declaration mentions;the only FBF4 declarations are the two original negative cases. No positive FBF4 declaration would acquire a new skip.
- 288before/after filter controls assess424profile outcomes per mode. Only the target positive/negative predicates and complete original042 lists change (eight outcomes). Version/default-language,unknown-feature and other sequence predicates remain unchanged.
- The complete isolated QT3 rehearsal passes **31,162/0/659**. Exactly format-integer-042 leaves the skip set;all659retained names/reasons are identical.
- The complete isolated qt4 gate passes **8,587/0/1,892**. Exactly format-integer-042 leaves the skip set;all1,892retained names/reasons are identical. The226GatedSets and11Experimental passes are unchanged.

All27 diagnostic/rehearsal processes are externally bounded and complete without timeout. The copied harnesses preserve actual Program/runner,fixtures,environments and comparisons. Their executable change is limited to the proposed filter rule;generated markers and history rows are metadata. [RESULTS.json](RESULTS.json),[skip deltas](evidence/skip-delta.json) and the [manifest](evidence/manifest.json) retain concrete evidence. The engine is unchanged;22,198solution passes are inherited explicitly,not newly rerun.

## Approval and acceptance boundary

Approval authorizes only applying this exact patch,updating current documentation,building normal harnesses and running fresh complete normal QT3 and qt4 gates. Adoption is complete only when normal totals are **31,162/0/659** and **8,587/0/1,892**,the original case passes in each,and all retained skip reasons match. Unexpected outcomes prevent adoption;no assertion or unrelated guard may be weakened.

Explicit approval is required because [AGENTS.md](../../../AGENTS.md) mandates the current exact **31,161/0/660** QT3 result. A successful copied-harness rehearsal does not authorize changing that policy. This is the owner disposition requested by this proposal.

Approval does not publish,push,tag or release. Candidate1.28.0 remains unpublished;public1.26.3. REQ-126 remains Complete for its accepted bounded catalog;historical dispositions are not rewritten. Bosak.Schema retains staged XSD1.1 ownership.

# TA-09 executable acceptance

Base commit: `549ad467` (source 1.59.0). Candidate source: **1.60.0**. [Adopted specification](SPEC-BASIS.md) was recorded before implementation; [plan input](PLAN-INPUT.json) pins the preceding sealed milestone.

The 35 new public controls cover integer/decimal arithmetic, wide exact integers, comparisons, descriptor lane/limit separation, singleton-list/originless input, foreign/plain operands, misleading actual types, unsupported grammar, mid-execution authority revocation, close/cancellation, exact division boundaries, intermediate limits, identity separation and warm allocation without payload replay. See [individual results](../foundation-public-typed-numeric/ACCEPTANCE.json). Existing projection, public identity, internal identity, cache, container, association, adapter and ordinary-session controls remain green.

The full solution passes **25,114/0**, Runtime **794**, API **681**. Frozen QT3 remains **31,397/0/424**; gated qt4 **8,941/0/1,918**, including 11 experimental passes. Exact skip names/reasons match the retained profile baseline. The separate LanguageServer build also passes. Original commands, guarded runner settings, exit codes and TRX records are in [execution receipts](evidence/EXECUTION-RECEIPTS.zip). The runner is documented in [R2 runbook](../foundation-r2-closure/RUNBOOK.md); retained Application Control fallback is `run-xslt-tests.ps1`.

All observed engine loads match the candidate binary hashes. Sources, changed-source archive, engine archive and corpus pins are independently checked. Exported declarations, field layout and friend grants match TA-08; internal API implementation files change intentionally to select the separate public profile. Public constructors/signatures remain unchanged.

The public fixture uses the existing contract test assessor/authority, not a qualified native Schema provider. There is no owner acceptance, freeze, publication or accepted performance budget inferred from these controls. Construction, original node facts, compatibility and broader host/native migration remain pending.

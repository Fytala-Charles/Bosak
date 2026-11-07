# Typed original-node atomization policy (TA-16)

5 November 2026. **Policy selected; implementation not started.** Source remains **1.63.1**, with no engine/API/version/binary change. Reserve `bosak-original-node-data-v1` and its separate descriptor for explicit assessor-bound `data($node)` / `fn:data($node)` over original elements/attributes or explicitly bound empty input.

[Selected policy](POLICY.md) | [Machine profile](PROFILE.json) | [Immutable specification basis](SPEC-BASIS.md) | [Implementation sequence](IMPLEMENTATION.md) | [40 cases, all NotRun](ACCEPTANCE-CASES.json) | [Owner/evidence trace](TRACEABILITY.md) | [Status](STATUS.json)

A valid absent element typed value will raise FOTY0012 without invoking its typed getter. Supported empty and nilled-empty typed results remain empty. Nonempty output preserves ordered atomic payloads, declared/actual member types and original assessment: one direct marked scalar or a private materialized sequence for multiple cells. Existing per-scalar quotas, exact keys, transactional installation and zero empty-container indexing remain selected. Repeated immutable nonempty container payloads can reuse resident cells under current authentication.

Materialized traversal reads copied carriers without provider callbacks; it grants no resource lease or operational authority. Original node/facts tuples are proven at extraction. Later cell use authenticates its independently issued container/input/origin/components, not a permanent parent-node identity. Foreign compiled sessions do not gain an implicit import capability. These distinctions are explicit acceptance obligations, not activated implementation.

Fresh unchanged-binary prerequisite controls pass **600/0** (Runtime 562, API 38), with actual loaded engine hashes matching the exact TA-15 candidate. All **40 new AD cases remain NotRun**. Prior **25,336** full tests and frozen gates are historical same-binary qualification, not rerun by this preparation. The current public nilled route still refuses data. No native/owner budget acceptance, C45/freeze, package, push or publication is inferred.

Next implement the separately adopted private original-node/container registration bridge, materialized result and exact public data plan, then qualify AD01-AD40 and full gates. No new public signatures are selected; this does not close later consumer/all-host signature questions. Verify with `python docs/foundation-node-atomization-policy/tools/verify.py`.

# XmlDb exact-package compatibility and integration readiness

7 November 2026. The live clean owner checkout is `D:/Development/Bosal.XmlDb`.
[Plan](PLAN.json) records its exact HEAD, source hashes and two isolated copies
of the existing Debug XDM test output. No owner source, package pin, storage
format, frozen fixture or history was changed.

The same compiled XDM test assembly passes **47/0** with its original engine
layout and **47/0** after replacing available engine DLLs with exact
**1.81.1-preview.c0b926de** package bytes. The 47 case names match the retained
native-lifetime Windows XDM catalog. These are overlapping runs of the same
cases, not 94 distinct controls. Original consumer binaries/configuration and
fixture bytes are retained unchanged; this is an observed existing-binary lane,
not a new source build or a shipped-artifact provenance claim.

[Results](evidence/RESULTS.json), [TRX archive](evidence/TRX-RESULTS.zip) and
[scoped execution receipts](evidence/EXECUTION-RECEIPTS.zip) retain the evidence.
Actual Default-context, noncollectible engine loads match the corresponding
baseline/preview layout hashes. Only engine DLLs already present in the original
layout are replaced. This does not imply that all nine preview packages or all
native Storage/Integration scenarios were exercised.

The source inspection establishes a concrete integration gap:

- `PersistentXdmNode.OwnsNode` is an internal exact-snapshot membership helper;
  arbitrary foreign node getters must not be used to authenticate membership.
- `SnapshotWorkOwner` is an internal actual-resource owner that retains the raw
  task through deferred reads/serialization and delays native release until the
  task returns. A caller timeout does not confirm physical completion or cleanup.
- The Core ordinary route has public input/lifetime admission, but the owner
  helpers are not a shipped public adapter for it. Production placement and
  composition still need review. A friend-test diagnostic must not be presented
  as a production bridge, and ordinary XML must not receive fabricated Schema
  authority to make admission succeed.

[The six-step qualification recipe](QUALIFICATION-RECIPE.json) pins the next
work: archive the exact owner source/configuration/fixtures; resolve isolated
MinVer/tool/dependency inputs and candidate-only package selection; build and
compare all 3,572 Storage /863 Integration /47 XDM cases; separately adopt and
qualify the adapter, raw lifetime and XSLT serialization; then review actual
worker/currentness routing and PF workloads/budgets. Existing process/crash
helpers require safety review before a full integration launch. No full 4,482-case
source/native run is claimed here.

The product pin remains 1.26.2. No new Core API need is demonstrated by this
compatibility slice. Public provider adoption, actual source/worker qualification,
owner budgets, C45/freeze and release acceptance remain open. The
[draft workload proposals](../foundation-owner-workload-draft/README.md) remain
proposals; passing XDM regressions cannot approve them.

Next: prepare the isolated source qualification with explicit dependency and
version provenance, then investigate any genuine source migration failures before
implementing an adapter. Keep prior receipts and live owner instructions intact.
No owner message, commit, push, tag or publication was performed in this slice.

Run `python docs/foundation-xmldb-package-readiness/tools/verify.py` to check the
owner state, exact cases, layout deltas, receipts and actual engine bindings.
Static local links are checked; no new rendered-document approval is claimed.

# Schema production adoption — concrete review choices

Proposed location: the active Apache-2.0 public Schema repository. Keep dependency
direction **Schema integration → public Core contracts/API**. Core must remain
usable without Schema. Decide the production assembly and namespace explicitly;
the experimental `Bosak.Schema.ContainerPrototype` namespace and assembly are
not an approved production naming contract. The current Schema package already
depends on API, Providers, XQuery and XSLT at 1.26.3; qualify a coherent new
package cohort rather than replacing a single DLL in the shipped package graph.

Use these source files as reviewed behavior references, not a blind copy/export:

- [NativeProviderSession.cs](../foundation-native-immutable-guard/provider/NativeProviderSession.cs): public session/result/snapshot facade.
- [NativeStrictAssessment.cs](../foundation-native-immutable-guard/provider/NativeStrictAssessment.cs): captured request, strict bounded assessment and diagnostics.
- [NativeContainerAdapter.cs](../foundation-native-immutable-guard/provider/NativeContainerAdapter.cs): native authority and typed-container mapping.
- [NativeNodeProjection.cs](../foundation-native-immutable-guard/provider/NativeNodeProjection.cs): private output snapshot, facts and availability guard.

Required decisions and acceptance controls:

1. **Public capture and ownership.** The provider owns captured input and newly
   projected output. Only a complete Valid assessment exposes an admissible
   validated root. Invalid and Unsupported results cannot be admitted; absence
   of diagnostics is not success. The original request source identity and fresh
   normalized output snapshot remain distinct.
2. **Profile.** Start with the tested strict, provider-owned request path and
   actual bounded native decimal facts. Lax/requested-type/foreign-input support
   is not implied. Publish supported/refused types, facts and containers
   explicitly; broader list/union/nil/construction routes need their own exact
   specification adoption and qualification before implementation.
3. **Lifetime.** Result close revokes its output; snapshot close releases that
   admission, not the result owner. Session close prevents new admissions.
   Caller XML/schema objects remain caller-owned. Preserve cancellation and
   request mutation checks. Keep immutable output guards callback-safe and avoid
   reintroducing whole-document serialization on repeated availability checks.
   Serial evidence does not establish concurrent dispose/reentry support.
4. **Diagnostics.** Preserve native codes/messages. The tested default cap is
   eight diagnostics and 4,096 aggregate code-plus-message characters; positive
   caller limits may only tighten it. Truncation is independent of result status.
   Decide whether to adopt this cap explicitly rather than treating a prototype
   constant as a product-wide policy.
5. **API/dependencies.** Review public XML documentation, naming, ownership and
   exceptions. Keep native/provider implementation details behind the public
   facade. No new Core public declaration is required by the demonstrated
   assess/admit/query/dispose path. A missing real-owner capability must first
   be recorded and reviewed, not replaced by a private cast or global cache.
6. **Delivery proof.** Build source and package consumers separately. Preserve
   unchanged native and consumer artifact hashes for the binary lane. Record
   loaded Default-context engine/provider identities, actual source revisions,
   package provenance and fixtures. Re-run native, adapter and independent
   public consumer controls on the production placement; the old 451/59/10
   receipts are starting evidence, not automatic acceptance of a new assembly.
7. **Workload/budget.** Select actual PF-02/03 and shared PF-06/08/09 inputs,
   accepted baselines and explicit units/statistics/thresholds/tolerances.
   [PV-Q06](../foundation-workload-1811/README.md) supplies a candidate-only
   one/eight-child decimal subset. It does not qualify all those scenarios.

Each item needs a dated disposition and implementation/qualification evidence.
No item is accepted or waived by this proposal. Preserve sealed bridge,
assessment, prototype and current production receipts when recording adoption.

<div align="center">
<img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak Bosak 1.19.1 release notes">
<h1>Bosak 1.19.1 release notes</h1>
<p>18 October 2026</p>
</div>

**Prepared; unpublished.** Proposed tag `v1.19.1` targets `a9e954d75b3518340ce6da84c9ae06c50ab1f5a4` ([PR #370](https://github.com/Fytala-Charles/Bosak/pull/370)).

This patch corrects XPath 4.0 draft `element-to-map` fallback handling. If an explicit layout fails and the wildcard plan entry supplies no fallback layout, evaluation now reports **FOJS0008** instead of a .NET nullable-value exception. Existing explicit XML/deep-skip fallbacks and ordinary layout inference retain their behavior.

Build maintenance repairs XML documentation and nullable/test diagnostics. Both the solution and separate language-server clean builds have zero warnings and zero errors, without suppressions. Language-server code-lens null arguments retain their existing JSON shape.

No new public API or authoring operation is introduced. The VS Code extension has its own release cycle; this NuGet patch does not update its bundled engine. Existing REQ-130/REQ-131 consumer acceptance remains valid for its recorded versions.

Source CI passes **9,817** tests (9,733 solution and 84 language-server). Frozen QT3 remains **31,142/0/679** and qt4 gated remains **8,278/0/1,894**. Exact-merge local packages pass **153** broad API checks and **8** focused regression/identity checks using fresh caches; these overlap source coverage and are not additive.

See [release readiness](RELEASE-1.19.1-READINESS.md) and [maintenance details](MAINTENANCE-1.19.1.md). Public signed-package verification follows publication; it has not occurred for 1.19.1.

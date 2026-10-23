<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.23.8 release notes">
  <h1>Bosak 1.23.8 release notes</h1>
  <p>23 October 2026</p>
</div>

**Published and verified.** Immutable v1.23.8 source: `6d12bed35dac56a635acc7702645ef9085103c83`. [Public delivery proof](RELEASE-1.23.8-PUBLICATION.md).

Bosak 1.23.8 fixes omitted `mode` on `xsl:apply-templates` inside stylesheet functions. It now uses the current default mode, matching ordinary instruction dispatch. Previously, effective 4.0 execution could raise XTSE0020 while trying to expand an empty QName; some named-default cases selected the wrong mode. Explicit named modes, `#default`, `#unnamed`, `#current`, empty selections and rejection of an explicitly empty mode remain covered. [PR #416](https://github.com/Fytala-Charles/Bosak/pull/416), [scope and 36 regressions](XSLT-FUNCTION-DEFAULT-MODE.md).

Documentation adds 66 ordinary host/version checks and 114 direct include/import/used-package checks, including complete report examples under Legacy and draft hosts. Optional catalog package registration is scoped and cleaned up within the shared registry collection. [Host profiles](xpath40-catalog/HOST-PROFILE-BOUNDARIES.md), [module profiles](xpath40-catalog/MODULE-PROFILE-BOUNDARIES.md), [PR #417](https://github.com/Fytala-Charles/Bosak/pull/417).

No public API is added. The fix does not certify every lexical default-mode, static, contextual-function or package configuration. Multi-hop module graphs, package ranges/visibility/overrides and broader schema/resource/streaming contracts remain separate. REQ-126 and D2/D3/D4/AC-03 remain partial; 28 historical adoption pins remain unavailable. Catalog totals: 76 entries; 3,049 feature +24 chapter checks =3,073; four worked chapters and 117 assigned names.

Verification: 10,936 solution tests; 84 additional language-server CI tests; fresh package-only consumers with 153 broad +430 focused checks, including all 36 new default-mode regressions. Consumer checks overlap source coverage. Ten unsigned candidates have matching source/version/dependencies and archive/cache/output identities, XML docs and source README/license/icon/branding. Both transitive audits report no vulnerable packages. Existing corrected-engine gate receipts remain QT3 31,142/0/679 and qt4 gated8,278/0/1,894; no membership or fixture changes.

Ten signed public packages and fresh nuget.org-only153 broad/430 focused checks are verified, with matching signed archive/cache/output/contentHash provenance and clear transitive audits. Braid's 1.23.7 smoke result has not been received; no Braid dependency or acceptance change is claimed. The VS Code extension retains its separate release cycle.

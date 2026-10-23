<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala REQ-126 module and package profile boundary audit">
  <h1>Direct module/profile boundaries</h1>
  <p>23 October2026 - source 1.23.8 unpublished; public 1.23.7</p>
</div>

## Contract and separate dependency

[The76th catalog entry](xslt-instructions/module-profiles.md) covers one direct include/import/registered-package dependency under Legacy and Xslt40Draft. The selected host propagates; the dependency retains its own effective version and local function overrides. [Six complete report/library pairs](examples/module-profiles/README.md) provide normal, empty, reordered and malformed-data outcomes under both hosts.

The engine prerequisite is **[PR416](https://github.com/Fytala-Charles/Bosak/pull/416)**, head `c172747b13e5bbfe2b342b04d295720299b4387e`, with green CI. That separate [function default-mode fix](../XSLT-FUNCTION-DEFAULT-MODE.md) advances unpublished source to1.23.8. This module PR is stacked on fix/req126-function-default-mode; merge the fix first, then retarget the module PR to refreshed main and verify CI/diff. CI currently targets main PRs only, so the stacked module PR will receive remote CI after that retarget. The module diff contains no engine change, additional version bump or release. Main base before the fix is merged PR415 `ce9604fa85d30ff941b5edb863b5a762e00742a2`.

Current comparison pin: [qtspecs b4723ff5](https://raw.githubusercontent.com/qt4cg/qtspecs/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c/specifications/xslt-40/src/xslt.xml), refreshed on 23 October 2026, include/import/use-package/backwards/forwards/modes sections. XML bytes/hash are retained. This is current comparison, not historical adoption recovery.

## Executable evidence

| Cases | Scope |
|---|---|
|24 | Principal/dependency3.0/4.0 combinations, three relations, both hosts |
|12 | Dependency-local function version promotion/demotion |
|18 | Child/local versions versus inherited forwards-compatible ceiling on bounded apply-templates attributes |
|6 | Static use-when in3.0 dependencies with a 4.0 principal |
|6 | Frozen scan refusal in4.0 dependencies |
|48 | Three relations x two report/library variants x four data fixtures x two hosts |

Total **114**, listed with exact files, profile, package registrations and output/error in [the machine audit](MODULE-PROFILE-BOUNDARIES.json). Principals resolve ../library/module.xsl relative to their app/main.xsl URI. Used packages are registered by name/exact version 1.0 to an absolute XML file URI through the existing public RegisterPackage API. The runner's optional packages metadata is scoped to a transformation, cleaned in finally, and serialized with other package-registry tests through the existing named collection. Cases without metadata preserve their prior setup.

Initial baseline: **70pass/44fail**, including38 missing-registration failures and six function default-mode errors. After scoped registrations: **105pass/9fail**; the remaining failures are the same independent engine defect, now reproduced through packages too. On PR416: **114pass/0fail**, with no expected result weakened or replaced by an error observation. All 2,935 previous manifest rows and their serialized content are retained. The fix has its own36 API regressions and original26/10 baseline, separately audited.

## Verification and limits

All **10,936 solution tests pass**, zero failures/skips, including 7,890 XSLT, all 114 module cases, the36 prerequisite regressions and3,049 feature +24 chapter =3,073 documentation checks. An additional 84 language-server tests pass. Build has zero errors and the existing CS8604 conformance warning. Temporary fixture junctions were removed; original test catalogs unchanged. JSON/XML, new links, source headers,16-field/index and rendered HTML checks are recorded independently; no screenshot/PDF acceptance.

Fresh engine gates in the prerequisite remain **QT3 31,142/0/679** and **qt4 gated8,278/0/1,894**. They are not repeated for the module documentation/runner delta, which changes no engine code. No gate fixture or membership change. Current catalog 76 entries, four chapters and 117 assigned function names; the18-row matrix is updated without inventing another grammar slice. D2/D3/D4 and AC03 remain partial; 28 historical adoption pins remain missing.

The checks do not certify multi-hop graphs, package version ranges, import precedence, visibility/overrides, all lexical default-mode contexts, schema/static parameters, authoring, dynamic evaluate, streaming or remote/resource-policy behavior. Public1.23.7, Braid smoke/acceptance, README install/build commands and protected PR132 remain unchanged. Original baseline/intermediate/fixed receipts, build logs, TRXs and stack stash are retained under `.guard-tmp/req126-module-profile-evidence`. No new package or tag is published.

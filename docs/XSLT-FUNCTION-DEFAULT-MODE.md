<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Function apply-templates default mode">
  <h1>Function apply-templates default mode</h1>
  <p>23 October 2026 - source 1.23.8 unpublished; public 1.23.7</p>
</div>

**Public 1.23.8 - 23 October 2026:** The function default-mode fix and host/module profile coverage are [published and verified](RELEASE-1.23.8-PUBLICATION.md). Current catalog:76 entries/3,073 documentation checks. REQ-126/AC-03 remain partial; Braid smoke/acceptance is separate. Earlier source/unpublished/PR checkpoints below are historical.

## Contract and reproduction

An xsl:apply-templates instruction with no mode attribute uses the default mode; it does not name an empty QName. The module/profile follow-up exposed XTSE0020 from otherwise legal function-body instructions, including an empty selection. The independent [FunctionApplyTemplatesModeTests](../tests/Bosak.Xslt.Tests/FunctionApplyTemplatesModeTests.cs) reproduce this through the public compiler/executable APIs.

Current comparison: [immutable XSLT XML](https://raw.githubusercontent.com/qt4cg/qtspecs/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c/specifications/xslt-40/src/xslt.xml), standard-attributes and modes sections, refreshed against upstream master on23 October2026. Default mode and current mode are distinct; a non-contextual function call uses the unnamed current mode. This is a current comparison pin, not recovery of historical specification adoption. The original baseline is retained.

The36-case baseline is **26pass/10fail**: eight empty-QName exceptions at effective4.0 and two incorrect named-default results at3.0. Cases exercise3.0/4.0 stylesheets under Legacy and Xslt40Draft, omitted mode with unnamed/named defaults, node/empty selections, explicit #default/#unnamed/#current/named modes, and explicit empty mode attributes. The last remain static XTSE0020 errors.

## Bounded correction

TransformEngine.EvaluateFunctionBodyInstruction now chooses CurrentDefaultMode for an omitted mode, matching ordinary instruction dispatch. Explicit modes continue through the existing expansion and validation code. The old empty unnamed-mode sentinel no longer enters lexical QName expansion. Existing function current-mode reset and result accumulation remain intact. With a root default-mode m, omission now selects the m rule; explicit #unnamed/#current retain the unnamed rule in these controls.

All36 regressions pass with their original expected results. This can change a result that previously ignored a named default, as well as remove the spurious XTSE0020. The fix does not certify every lexical default-mode, contextual-function, package/override or fixed-namespace combination. The module/profile documentation work remains separate and its expected successful results are not replaced by error observations.

Source advances1.23.7 to1.23.8 as a compatibility bug fix. No new public API or release is included; public/Braid1.23.7 smoke remains independent (existing recorded dependency/acceptance are not changed here).

## Verification

The [machine audit](XSLT-FUNCTION-DEFAULT-MODE.json) records **10,822 passing solution tests**, including7,776 XSLT, with zero failures/skips;84 additional language-server tests pass. Fresh engine gates remain **QT3 31,142/0/679** and **qt4 gated8,278/0/1,894**. Build has zero errors and the existing CS8604 conformance warning. Source fixture expectations and gate membership are unchanged. Initial red/green, build and execution receipts are retained under `.guard-tmp/req126-function-mode-evidence`; no failed evidence is overwritten. Catalog remains75 entries /2,935 feature +24 chapter =2,959 checks, four chapters and117 assigned names. D2/D3/D4 and AC03 remain partial.

Base: merged PR415 `ce9604fa85d30ff941b5edb863b5a762e00742a2`. Engine fix branch fix/req126-function-default-mode is isolated from docs/req126-module-profiles. The module slice will be based on the verified fix; it covers registration and dependency profiles rather than hiding this engine change in its diff. Pending upstream notes, worktrees/stashes, sweep baselines and protected PR132 remain preserved. README public installation/build commands remain current.

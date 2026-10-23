<div align="center">
  <img src="../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala stylesheet version and host processor profile reference">
  <h1>Stylesheet versions and processor profiles</h1>
  <p>Bosak host policy - bounded executable reference</p>
</div>

## 1. Name and Classification

**Version/profile boundary** - implemented Bosak host policy. Legacy is the default compatibility profile; Xslt40Draft is an opt-in experimental XSLT processor profile whose embedded XPath is frozen 4.0. This is not a new language feature or a promotion to XPath40Experimental. Public/source 1.23.7 already provides this API.

Original profile delivery: [cc07d50f8a55bdca907d398d4916d95cbed36734](https://github.com/Fytala-Charles/Bosak/commit/cc07d50f8a55bdca907d398d4916d95cbed36734). Current comparison: [immutable XSLT XML](https://raw.githubusercontent.com/qt4cg/qtspecs/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c/specifications/xslt-40/src/xslt.xml), sections backwards, backwards-3.0, forwards and standard-attributes, refreshed against upstream master on 23 October 2026. This comparison pin does not recover a historical adopted specification revision. [Delivery audit](../HOST-PROFILE-BOUNDARIES.md).

## 2. Intent

Choose an execution host explicitly and understand how a local version declaration affects XPath syntax and forwards-compatible validation within that host.

## 3. Also Known As

Processor capability/profile, effective stylesheet version, embedded XPath compatibility and forwards-compatible ceiling are separate concepts. XsltProcessorProfile.Xslt40Draft and XPathCompatibility.XPath40Experimental are different API settings; the former does not enable experimental XPath functions.

## 4. History / Supersedes

The [baseline report](../examples/host-profiles/report-baseline.xsl) uses ordinary 3.0 expressions. The [mixed-version report](../examples/host-profiles/report-local40.xsl) keeps a 3.0 root but places version 4.0 on its template and uses the otherwise operator. Both are executed under both Bosak hosts. A local 4.0 block is not a portable way to give an arbitrary XSLT 3.0 processor XPath 4.0 capability; these are Bosak host contracts.

## 5. Motivation

An order report defaults an absent quantity to1 while preserving an explicit zero. Migrating one template allows a compact otherwise expression without changing the root version. A caller still needs to know whether its host treats a lower local version as a syntax boundary. [The complete task](../examples/host-profiles/README.md) gives input, all instructions, exact results and malformed-data behavior.

## 6. Language Alignment

The pinned XSLT 4.0 draft defines effective version from the nearest applicable [xsl:]version attribute, with special rules including xsl:output. It defines no 3.0-versus4.0 compatibility-mode differences for a 4.0 processor. The draft profile's acceptance of frozen 4.0 XPath within version 3.0 is therefore intentional. Bosak Legacy is its preserved compatibility policy, not an assertion that it exactly emulates every W3C3.0 processor: it uses a 3.0 forwards-compatible ceiling while selecting embedded XPath 4.0 at effective versions at least 4.0.

## 7. Applicability

Use this entry when compiling an ordinary stylesheet with XsltCompiler or generating a locally versioned block. This slice excludes includes/imports, used packages, schema/static parameters, authoring/edit APIs, dynamic evaluate, streaming, detailed fallback behavior and versions1.0/2.0. Existing tests elsewhere may cover some of those paths; this entry does not claim their full matrix.

## 8. Structure

| Host | Embedded XPath in the supplied ordinary expression paths | Forwards-compatible ceiling |
|---|---|---|
| Legacy (default) | Effective version below 4.0 selects XPath 3.1; at least 4.0 selects frozen 4.0 |3.0 |
| Xslt40Draft (explicit opt-in) | Frozen4.0 independently of local 3.0/4.0 version |4.0 |

A non-root XSLT instruction uses unqualified version; a literal result element uses xsl:version. The nearest declaration wins. xsl:output/@version selects serialization behavior and does not promote the surrounding stylesheet's XPath level. The ceiling describes capability-sensitive forwards compatibility; it does not enable arbitrary future syntax.

## 9. Participants

[XsltCompiler.ProcessorProfile](../../../src/Bosak.Xslt/Api/XsltCompiler.cs) selects the public host enum [XsltProcessorProfile](../../../src/Bosak.Xslt/Api/XsltProcessorProfile.cs). [Stylesheet](../../../src/Bosak.Xslt/Stylesheet/Stylesheet.cs) resolves GetEffectiveVersion, GetXPathCompatibility and IsForwardsCompatibleElement. The [catalog runner](../../../tests/Bosak.Xslt.Tests/Documentation/CatalogExampleTests.cs) supplies the explicitly requested host to the public compiler, retaining Legacy as the default for all older rows.

## 10. Collaborations

In the mixed-version report, the root 3.0 declaration remains outside the locally4.0 template. The template's selection, per-line expression and total inherit4.0 in Legacy. Draft supplies frozen 4.0 throughout either stylesheet. Static use-when, shadow-produced select expressions, AVTs and ordinary select expressions have separate focused controls; changing a serializer version is not a shortcut for changing their host.

## 11. Consequences

A Legacy 4.0 stylesheet with a local 3.0 instruction refuses 0x10 with XPST0003; Draft accepts it. A3.0 Legacy root with a local 4.0 template accepts it. Under Draft, an unknown unqualified apply-templates attribute at effective4.0 raises XTSE0090; above 4.0 it is ignored. Legacy's tested apply-templates path ignores it above 3.0. These are bounded instruction controls, not a certification of every unknown attribute/instruction. scan remains unavailable with XPST0017 under both XSLT hosts, including Draft. Runtime malformed quantities still raise FORG0001.

## 12. Implementation

Fifty focused host/version cases plus sixteen full report runs execute66 manifest cases through the public compiler. The initial runner forced Legacy and produced56 passes/10 failures against rows requesting Draft. Those failures are retained as a runner limitation, not an engine defect. The runner now accepts explicit processorProfile values legacy/draft40 and rejects unknown spellings; omitted values preserve Legacy. Existing manifest rows and outcomes are unchanged. No engine, public API, version, gate or release changes. [Machine audit](../HOST-PROFILE-BOUNDARIES.json).

## 13. Sample Code

The actual [mixed-version stylesheet](../examples/host-profiles/report-local40.xsl) declares version 3.0 on xsl:stylesheet and version 4.0 on xsl:template. Its quantity expression is `(xs:integer(@qty) ?? 1)`. With [normal.xml](../examples/host-profiles/normal.xml), both profiles produce `A=2|B=1|C=0|lines=3;total=3`, exactly matching the baseline. Empty input yields `lines=0;total=0`; reordered rows change line order; `qty="oops"` raises FORG0001. [Task and fixtures](../examples/host-profiles/README.md) include full context and all outcomes. Host configuration is the public ProcessorProfile property; each manifest row states it explicitly.

## 14. Performance Notes

Host/version selection is compilation policy, not a performance optimization. Both reports parse and sum quantities per reading; no timing/allocation comparison is claimed. Compilation captures the profile; existing named tests separately verify behavior when a compiler is reused.

## 15. Known Uses

Applicable AC-03 evidence: `host-profile-select-root3-legacy`, `host-profile-select-root3-draft40`, `host-profile-select-root4-local3-draft40`, `host-profile-ancestor-root3-wrapper4-local3-legacy`, `host-profile-avt-root3-draft40`, `host-profile-use-when-root3-draft40`, `host-profile-future-attribute-root4-draft40`, `host-profile-report-local40-normal-legacy` and all other cases in [the audit](../HOST-PROFILE-BOUNDARIES.json) and [manifest](../examples/cases.json), run by CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome. This host/compiler/XSLT contract has no applicable XPath-only gate assignment. Named [ProcessorProfileTests](../../../tests/Bosak.Xslt.Tests/ProcessorProfileTests.cs): DraftXPathIndependentOfVersion, LocalVersion, StaticXPathProfile, UnknownAttributeMatrix, CompilationCapturesProfile and DefaultAndInvalidProfile provide independent public-API evidence. The shadow controls generate a 4.0 select expression from a string-valued shadow attribute; they do not certify every4.0 form inside the shadow expression itself. No gate is invented or promoted.

## 16. Related Features

[Numeric literals](../operators-and-syntax/numeric-literals.md), [otherwise](../operators-and-syntax/otherwise-operator.md), [xsl:select](xsl-select.md) and [coverage reconciliation](../COVERAGE-RECONCILIATION.md). Package/import propagation is a subsequent bounded slice, not implied acceptance from this entry. REQ-126 and D2/D3/D4/AC-03 remain partial.

<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala REQ-126 ordinary stylesheet profile boundary audit">
  <h1>Ordinary stylesheet version/profile boundaries</h1>
  <p>23 October 2026 - source/public 1.23.7 unchanged</p>
</div>

## Supplied contract

[Dedicated16-field entry](xslt-instructions/processor-profiles.md) separates Bosak's default Legacy host, opt-in Xslt40Draft capability, nearest effective [xsl:]version, embedded XPath and forwards-compatible ceiling. It does not call the draft XSLT host XPath40Experimental or a complete XSLT 4.0 implementation. The [complete order task](examples/host-profiles/README.md) compares a plain3.0 report with a root 3.0/local 4.0 report under both hosts, with exact normal/empty/reordered/error outcomes.

Base: published main `11343adce014c655a1599fd36d61adf61b627316`. Existing profile implementation: `cc07d50f8a55bdca907d398d4916d95cbed36734`. Current comparison pin: [qtspecs b4723ff5](https://raw.githubusercontent.com/qt4cg/qtspecs/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c/specifications/xslt-40/src/xslt.xml), refreshed on 23 October 2026; backwards/backwards-3.0/forwards/standard-attributes sections. This pin is not historical adoption recovery. The XML copy/hash is retained in the slice evidence.

| Dimension | Supplied evidence |
|---|---|
| Root/local XPath selection | Root3.0/4.0, local 4.0/3.0 under both hosts |
| Ancestor precedence | Literal xsl:version wrappers and nearer instruction version overrides |
| Expression paths | select, AVT, use-when and shadow-produced select controls |
| Serialization separation | xsl:output/@version does not promote surrounding expression syntax |
| Forwards compatibility | Bounded unknown apply-templates attribute cases at3.0/4.0/5.0 and local 3.0/4.1 |
| Frozen versus experimental | scan refuses XPST0017 under both hosts at root 3.0/4.0 |
| Complete work | Two order reports x four fixtures x two hosts =16 report runs |

The 50 focused cases and 16 report runs total 66. The [machine audit](HOST-PROFILE-BOUNDARIES.json) lists every file, host, exact output/error and retained validation result. Shadow controls verify generated4.0 select expressions, not all syntax inside shadow expressions themselves. Module/include/import/package propagation, schema/static-parameter/declaration contexts, dynamic evaluate, authoring/edit paths, streaming and detailed fallback behavior remain separate. Bounded apply-templates checks do not establish every instruction's unknown-attribute policy.

## Runner change and evidence

The original catalog runner always created the default Legacy compiler. The initial66-case baseline was56pass/10fail against Draft requests. These are retained **runner-selection failures**, not an engine defect or a reason to change language expectations. The runner now accepts optional processorProfile values legacy/draft40 and rejects unknown spellings. Omitted fields preserve Legacy for all existing cases. For nonstreaming XSLT rows the old mode metadata is not an XPath selector; the actual stylesheet and compiler profile determine behavior.

With explicit selection all 66 pass. All 2,869 prior manifest rows and their serialized content are preserved. No engine code, public API, source version, gate, upstream fixture or release change is included. Frozen4.0 in a Draft host remains distinct from experimental XPath. The report still defaults missing quantity to1, retains0, preserves source order, sums empty input as0 and refuses invalid integer text with FORG0001.

## Verification and remaining work

**10,786 solution tests pass**, zero failures/skips, including 7,740 XSLT and 2,935 feature +24 chapter =2,959 documentation checks. An additional 84 language-server tests pass. Initial build has zero errors and the existing CS8604 conformance warning; an intermediate sandbox output-write failure is retained, followed by an authorized zero-warning/zero-error build. Test fixtures were connected temporarily and their junction entries removed safely afterward. No source fixtures were changed.

Manual gates are not rerun for this runner/documentation-only slice. Latest published implementation receipts remain QT3 **31,142/0/679**, qt4 gated **8,278/0/1,894**; no gate promotion is inferred. Current catalog 75 entries, four chapters and 117 assigned function names. The18-row source/test matrix is updated without inventing a new REQ-118 grammar slice. REQ-126, D2/D3/D4 and AC03 remain partial; 28 historical adoption pins remain missing.

Evidence directories: `.guard-tmp/req126-host-profile-evidence` for initial/failed/retry/successful builds and TRXs; `.guard-tmp/req126-host-profile-source` for the isolated branch. Source/public1.23.7, README install/build instructions, Braid smoke/acceptance and protected PR132 remain unchanged. Next bounded work: include/import/package propagation, independently of D2 defaults/options and D4 historical pins. Original receipts and pending upstream notes remain preserved.

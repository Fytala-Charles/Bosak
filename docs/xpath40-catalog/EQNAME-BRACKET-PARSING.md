<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala EQName bracket parsing correction">
  <h1>EQName bracket parsing</h1>
  <p>22 October 2026 - source 1.23.5, unpublished</p>
</div>

## Contract and finding

The [XPath 3.1 BracedURILiteral grammar](https://www.w3.org/TR/xpath-31/#doc-xpath31-BracedURILiteral) excludes curly braces from the literal content, not square brackets. Namespace validation may optionally reject invalid URI values; this correction retains Bosak's permissive namespace-literal policy. The [current draft grammar](https://raw.githubusercontent.com/qt4cg/qtspecs/b4723ff52fd9d4fabf2b4cd33bd44f21939c512c/specifications/grammar-40/xpath-grammar.xml), production `BracedURILiteral` for `xslt40-patterns`, has the same square-bracket boundary. Upstream master was refreshed on 22 October 2026 to `b4723ff52fd9d4fabf2b4cd33bd44f21939c512c`; this is a current review pin, not recovered historical adoption. Immutable grammar and XSLT XML copies are retained with validation receipts.

Before the correction, `Q{urn:n[part]}root` was split at the namespace's opening bracket as though it started a predicate. All 32 initial transformation regressions failed on the unmodified engine, in both effective 3.0 and 4.0 profiles. A URI containing an unmatched opening square bracket is also literal content: the closing curly brace ends the namespace literal.

## Implementation

`PatternCompiler.FindPredicateStart` scans outside EQName namespace literals and quoted strings, including doubled quote escapes. Single-step element and attribute dispatch, path ancestor/final-step classification, and predicate extraction use the same boundary. Actual predicates after the EQName still use the existing matcher, sibling focus and delimiter handling. This is an internal XSLT pattern correction; the XPath lexer and public API are unchanged.

End-to-end tests use explicit template priorities to isolate matching from dispatch priority. They cover exact names and nonmatches, unmatched URI square brackets, attributes and explicit axes, kind tests, wildcard names, union and parenthesized patterns, ancestor/final-step predicates, nested EQNames within real predicates, positional and chained predicates, and both 3.0/4.0 profiles.

The newly supported names also exposed a remaining raw bracket check in effective 4.0 default-priority classification. With parsing fixed but the original priority check retained, both new opening-bracket priority cases fail while eight existing URI punctuation cases pass. That check now uses the already URI-masked name, giving an expanded-name test priority 0 rather than predicate priority 0.5. Earlier-profile default-priority classification remains unchanged, as in the 1.23.4 correction; this slice does not claim exhaustive earlier-profile URI-punctuation priorities.

There are **44 new matching cases plus two new 4.0 priority cases**. All **159 focused checks pass**, including existing delimiter, EQName-priority and union/set-priority regressions. Full-suite and gate results are recorded in [the machine-readable audit](EQNAME-BRACKET-PARSING.json). Final validation: **10,516 solution tests pass**, including **7,548 XSLT tests**, with no failures or skips. QT3 stays **31,142/0/679** and qt4 gated stays **8,278/0/1,894**. Release build succeeds with zero errors and ten NU1900 warnings because NuGet vulnerability metadata was unavailable; the authorized restore retry retained those warnings. No successful vulnerability audit is claimed. Temporary worktree fixture junctions were removed after validation, leaving the shared catalogs unchanged.

The original red run, intermediate attempts and successful runs are retained under `.guard-tmp/req126-eqname-parser-evidence`.

## Delivery and remaining scope

Base: `0f1c4b2158235bb92dd62ea3aed645efcd9a8879` (main after PR #408). Source 1.23.5 is unpublished; public release remains 1.23.4. No Braid dependency or acceptance change is included. REQ-126 remains In Progress; D2/D3/D4 and AC-03 remain partial. Catalog manifests and worked reports retain 71 entries, 2,743 feature plus 24 chapter checks, four chapters and 117 assigned function names. These new engine regressions are separate from catalog check counts.

Older opening-bracket limitations in [the 1.23.4 priority delivery](EQNAME-PATTERN-PRIORITY.md) describe the earlier source and remain historical evidence.

# Session Handover

> Session-specific scratchpad. Overwrite at the end of every session.

## Session Date

2026-07-15 (evening)

## What Was Done

- **fn:transform registered in QT3 harness** — `TestExecutor` calls `XsltFunctionLibrary.Populate(ctx)`;
  csproj gained Bosak.Xslt reference. fn-transform set: **33 passed / 54 failed / 37 skipped**
  (was all XPST0017). Remaining 54 = genuine XSLT gaps (stylesheet-node doc, static-base-uri,
  result-document, params). Unit tests still 999/0.
- **Skip-reason inventory** (TestReport prints grouped reasons): 6,667 unsupported dependency;
  **2,106 UriFormatException** (http: URIs resolved as local paths — biggest recoverable pool);
  1,476 XQuery syntax; 460 external-var binding; 138 schema-awareness; 138 invalid assert-count;
  ~90 OverflowException (should be FOAR0002); ~40 JsonReaderException (should be FOJS0001);
  50 assert-permutation; FileNotFoundException (should be FODC0002).

## Next Session Pointers

- Canonical state: `docs/AGENT_HANDOVER.md` (top section).
- **UriFormatException pool (2,106)**: QT3 env resolves `http://www.w3.org/qt3/...` doc/JSON URIs
  as filesystem paths (`D:\...\Conformance\http:\www.w3.org\...`). Map suite http URIs to local
  files (or a stub doc loader) in TestEnvironment/doc-loading code.
- Error-wrapping quick wins: OverflowException→FOAR0002 (~90), JsonReaderException→FOJS0001 (~40),
  FileNotFoundException→FODC0002, XmlException→FODC0006, DivideByZero→FOAR0001.
- Harness assert support: assert-count invalid-value (138), assert-permutation (50),
  external-variable binding (460).
- Failure clusters: fn:unparsed-text (54), fn:parse (32), fn:load-xquery-module (31),
  fn-transform residuals (54, XSLT features), fn:function-lookup (29).
- Full QT3 run: ~7 min background; log to tmp/ (gitignored). Harness exit code 2 = has failures (normal).

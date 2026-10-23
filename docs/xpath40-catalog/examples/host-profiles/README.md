# Order quantity report across host profiles

This synthetic orders vocabulary contains namespaced line elements, delimiter-free IDs and optional integer qty attributes. Missing quantity defaults to1; explicit0 remains0. Negative quantities are not independently validated. There is no external I/O, persistence or streaming claim.

The [baseline](report-baseline.xsl) uses a 3.0 root and explicit if/then/else. The [mixed-version stylesheet](report-local40.xsl) also has a 3.0 root but declares version 4.0 on its template and uses otherwise. Both report the same rows and total under both Bosak hosts. This pair demonstrates a local migration in Bosak; an arbitrary3.0 processor is not promised to support the mixed template.

| Instruction/context | Responsibility |
|---|---|
| stylesheet and namespace bindings | Declare root 3.0, XSLT/Schema and urn:fytala:dispatch vocabularies. |
| output method=text | Produce exact text; serializer version does not select expression syntax. |
| template match=/ | Process one complete input; the mixed template explicitly selects local 4.0. |
| variable lines | Select namespaced lines in source order. |
| for-each / value-of | Emit ID and parsed/defaulted quantity for each line, preserving explicit zero. |
| final value-of / sum | Emit row count and total, with empty input producing zero. |

## Inputs and exact results

### normal

[normal.xml](normal.xml):

```xml
<orders xmlns="urn:fytala:dispatch"><line id="A" qty="2"/><line id="B"/><line id="C" qty="0"/></orders>
```

Both stylesheets under both profiles emit `A=2|B=1|C=0|lines=3;total=3`.

### empty

[empty.xml](empty.xml):

```xml
<orders xmlns="urn:fytala:dispatch"></orders>
```

Both stylesheets under both profiles emit `lines=0;total=0`.

### reordered

[reordered.xml](reordered.xml):

```xml
<orders xmlns="urn:fytala:dispatch"><line id="C" qty="0"/><line id="B"/><line id="A" qty="2"/></orders>
```

Both stylesheets under both profiles emit `C=0|B=1|A=2|lines=3;total=3`.

### invalid

[invalid.xml](invalid.xml):

```xml
<orders xmlns="urn:fytala:dispatch"><line id="A" qty="oops"/></orders>
```

Both stylesheets under both profiles raise `FORG0001` for invalid integer text.

## Execution and profile metadata

CatalogExampleTests.PublishedExample_MatchesDeclaredOutcome compiles the actual file with a public XsltCompiler, sets ProcessorProfile from `processorProfile` (`legacy` or `draft40`), and captures the result. Existing rows without that field retain Legacy. The manifest's `mode` field does not select XPath compatibility for these nonstreaming XSLT runs: the compiler profile and effective stylesheet version do. No external variables are required. `probe.xml` is the minimal root fixture for the50 focused cases.

All66 case IDs, exact input paths and expected output/error strings are in [the audit](../../HOST-PROFILE-BOUNDARIES.json) and [cases.json](../cases.json). The16 report runs are two stylesheets times four fixtures times two hosts. Run the root AGENTS.md build/test commands; use its documented temporary-copy XSLT workaround if Application Control requires it. [The feature entry](../../xslt-instructions/processor-profiles.md) states the capability, effective-version and forwards-compatibility boundaries. Includes/imports/packages, authoring and other hosts are separate follow-ups.

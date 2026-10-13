<div align="center">
  <img src="../../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak worked example: XML manual publishing">
  <br><br><h1>Publish an XML Manual as HTML</h1>
  <p>Templates, mixed content, a nested table of contents, and references readers can trust</p>
</div>

## Reading this chapter

- [The publishing problem](#the-publishing-problem)
- [Files and contract](#files-and-contract)
- [Source-to-target element guide](#source-to-target-element-guide)
- [Complete input and output documents](#complete-input-and-output-documents)
- [XSLT elements and XPath constructs used](#xslt-elements-and-xpath-constructs-used)
- [Preserve the paragraph, not just its fields](#preserve-the-paragraph-not-just-its-fields)
- [Use modes for two readings of the same section](#use-modes-for-two-readings-of-the-same-section)
- [Resolve links and expose missing targets](#resolve-links-and-expose-missing-targets)
- [Run the chapter](#run-the-chapter)
- [Try the next change](#try-the-next-change)

## The publishing problem

A maintenance team maintains a pump manual as structured XML. Operators need a
single HTML page with a contents list, nested headings, readable emphasis, and
links between sections. A field mapper alone is insufficient: a paragraph can
contain text before and after an emphasized word or a cross-reference, and that
order is part of the meaning.

This synthetic vocabulary demonstrates publishing mechanics. It is not DITA,
DocBook, or a full publishing system. Supported content is a title, nested
sections, paragraphs, emphasis, and section references. Layout styling, images,
localization, arbitrary element handling, and multi-page output are outside this
case. Unknown wrappers are shallow-skipped; production vocabularies need an
explicit policy for every element they accept.

## Files and contract

| File | Purpose |
|---|---|
| [manual.xml](fixtures/manual.xml) | Complete manual: safety, maintenance, and a nested filter-replacement section. |
| [manual-to-html.xsl](manual-to-html.xsl) | Complete documented XSLT 3.0 / XPath 3.1 solution. |
| [expected.html](fixtures/expected.html) | Complete expected HTML page; open it in a browser to inspect the reading experience. |
| [broken-reference.xml](fixtures/broken-reference.xml) | A manual referencing a retired section. |
| [expected-broken.html](fixtures/expected-broken.html) | Expected page with the unresolved reference visibly marked. |

The stylesheet requires one title and unique, non-empty section IDs. IDs come
from the source, so reordering sections does not change their anchors. Duplicate
or missing IDs terminate the transformation rather than create ambiguous links.
The sample generates HTML5 in memory through the public capture API. It does not
require XPath 4.0 and does not write files or fetch external resources.


## Source-to-target element guide

The source default namespace is `urn:example:manual`, bound to `m` in XPath. Output HTML elements are unnamespaced. IDs and reference targets are source-owned and copied as supplied, so the producer must choose identifiers suitable for HTML anchors.

| Source element or attribute | HTML result and rule |
|---|---|
| `m:manual` / `m:title` | One document title populates `<head><title>` and the visible `<h1>`; the root requires exactly one title element, not non-blank title text. |
| `m:section/@id` | Becomes `<section id="...">`, a contents link destination, and a cross-reference destination. Non-blank, unique IDs are required. |
| Section `m:title` | Plain-text contents label and heading; the fixture assumes one title per section, but the root guard does not validate this cardinality. |
| Nested `m:section` | Nested contents `<ul>` and nested body `<section>`; headings advance from `h2` to a maximum of `h6`. |
| `m:p` | `<p>` with every child node processed in order. |
| `m:em` | `<em>` preserving its child text and any supported inline nodes. |
| `m:xref/@target` | Existing section becomes `<a href="#...">` with its title as label; absent section becomes a visible diagnostic `<span>`. |
| Text nodes | Copied by the `body` text rule, including spaces around inline elements. |
| Unknown elements in a named mode | `shallow-skip` traverses children without copying the wrapper; it is a fallback policy, not a guarantee of meaningful rendering. |

### Trace one section through both modes

For `replacement`, the `toc` template creates the nested link beneath Maintenance. The `body` template creates `<section id="replacement">` and an `h3`, because there is one ancestor section. The Safety paragraph's `xref` uses the same ID, looks up its section with `key()`, and reads its title. Reordering sections leaves this relationship intact.

The complete output below shows the surrounding HTML page as well as the paragraph. Its explicit `lang="en"` is an example decision, not language detection. There is no secondary output. The broken-reference case still creates a page, while duplicate or missing section IDs cause a terminating message. Unknown wrappers and title cardinalities beyond the root title need an agreed publishing contract before production use.

## Complete input and output documents

These are the complete published fixtures. Formatting is expanded for reading; paragraph text and inline spacing are preserved. The linked files remain the executable source of truth.

### Complete manual input

[manual.xml](fixtures/manual.xml)

<!-- fixture: fixtures/manual.xml -->
```xml
<manual xmlns="urn:example:manual">
  <title>Pump Service Manual</title>
  <section id="safety">
    <title>Safety</title>
    <p>Disconnect <em>power</em> before servicing. See <xref target="replacement"/>.</p>
  </section>
  <section id="maintenance">
    <title>Maintenance</title>
    <section id="replacement">
      <title>Filter replacement</title>
      <p>Replace the filter every 500 hours.</p>
    </section>
  </section>
</manual>
```

### Complete HTML page

[expected.html](fixtures/expected.html)

<!-- fixture: fixtures/expected.html -->
```html
<!DOCTYPE HTML>
<html lang="en">
  <head>
    <title>Pump Service Manual</title>
  </head>
  <body>
    <h1>Pump Service Manual</h1>
    <nav aria-label="Table of contents">
      <ul>
        <li>
          <a href="#safety">Safety</a>
        </li>
        <li>
          <a href="#maintenance">Maintenance</a>
          <ul>
            <li>
              <a href="#replacement">Filter replacement</a>
            </li>
          </ul>
        </li>
      </ul>
    </nav>
    <main>
      <section id="safety">
        <h2>Safety</h2>
        <p>Disconnect <em>power</em> before servicing. See <a href="#replacement">Filter replacement</a>.</p>
      </section>
      <section id="maintenance">
        <h2>Maintenance</h2>
        <section id="replacement">
          <h3>Filter replacement</h3>
          <p>Replace the filter every 500 hours.</p>
        </section>
      </section>
    </main>
  </body>
</html>
```

### Input with an unresolved reference

[broken-reference.xml](fixtures/broken-reference.xml)

<!-- fixture: fixtures/broken-reference.xml -->
```xml
<manual xmlns="urn:example:manual">
  <title>Pump Service Manual</title>
  <section id="safety">
    <title>Safety</title>
    <p>See <xref target="retired-section"/> before servicing.</p>
  </section>
</manual>
```

### HTML page with a visible editorial defect

[expected-broken.html](fixtures/expected-broken.html)

<!-- fixture: fixtures/expected-broken.html -->
```html
<!DOCTYPE HTML>
<html lang="en">
  <head>
    <title>Pump Service Manual</title>
  </head>
  <body>
    <h1>Pump Service Manual</h1>
    <nav aria-label="Table of contents">
      <ul>
        <li>
          <a href="#safety">Safety</a>
        </li>
      </ul>
    </nav>
    <main>
      <section id="safety">
        <h2>Safety</h2>
        <p>See <span class="missing-reference" data-target="retired-section">[Missing section: retired-section]</span> before servicing.</p>
      </section>
    </main>
  </body>
</html>
```

## XSLT elements and XPath constructs used

This inventory covers every XSLT element in the [complete stylesheet](manual-to-html.xsl). Literal result elements build the target vocabulary described above; `{...}` in their attributes are attribute value templates that evaluate XPath. Expressions in `select`, `test`, and `group-by` are XPath directly.

| XSLT element | Responsibility |
|---|---|
| `xsl:apply-templates` | Dispatches selected source nodes to matching templates, optionally in a named mode. |
| `xsl:choose` | Selects one branch, keeping rejection and accepted-data paths explicit. |
| `xsl:element` | Constructs a computed heading name, h2 through h6, from section depth. |
| `xsl:if` | Emits content or raises a message only when its test holds. |
| `xsl:key` | Declares a document-local index; the XPath `key()` function performs its lookup. |
| `xsl:message` | With `terminate="yes"`, refuses structurally unsuitable input rather than presenting a business report. |
| `xsl:mode` | Separates contents-list and body processing and defines shallow-skip fallback behavior. |
| `xsl:otherwise` | Supplies the fallback branch when no `xsl:when` matches. |
| `xsl:output` | Chooses serialization: XML for business reports, HTML5 for the publishing chapter. |
| `xsl:stylesheet` | Declares XSLT 3.0, source/result/helper namespace bindings, and excluded implementation prefixes. |
| `xsl:template` | Associates a source match with construction rules; `/` coordinates the whole transformation. |
| `xsl:text` | Emits exact diagnostic text around the unresolved reference target. |
| `xsl:value-of` | Inserts the string value of an expression into result text. |
| `xsl:variable` | Binds source nodes, validation results, or amounts without changing their values later. |
| `xsl:when` | Tests an ordered branch of `xsl:choose`. |

The main XPath building blocks are:

| Construct | Role in this chapter |
|---|---|
| `node()`, `text()`, `self::`, predicates | Preserve mixed content while omitting the section title from body dispatch after making its heading. |
| `ancestor::`, `count`, `min` | Calculate heading depth and cap it at h6. |
| `normalize-space`, `distinct-values`, `count` | Check non-blank and unique section IDs and exactly one document title. |
| `key`, `exists` | Resolve a section reference or choose its visible missing-target marker; current focus is the source document. |

## Preserve the paragraph, not just its fields

The input includes this paragraph:

```xml
<p xmlns="urn:example:manual">Disconnect <em>power</em> before servicing. See <xref target="replacement"/>.</p>
```

Its target HTML is:

```html
<p>Disconnect <em>power</em> before servicing. See <a href="#replacement">Filter replacement</a>.</p>
```

The paragraph template applies templates to all child nodes in `body` mode.
Separate rules render text, emphasis, and references. Selecting only element
children would lose surrounding text; stripping all whitespace could remove the
space between words. This stylesheet deliberately uses no `xsl:strip-space`, and
the test loads XML with `LoadOptions.PreserveWhitespace`.

## Use modes for two readings of the same section

The `toc` mode renders a section as a contents-list item. It recursively nests
child sections in another list. The `body` mode renders the same source section
as a section wrapper and heading, then processes its content. Modes keep the two
views distinct while sharing source identities and titles.

Top-level section headings use `h2`; nested sections use deeper levels, capped at
`h6`. The document title occupies `h1`. The contents list and body cross-references
both point to the source-owned section IDs. Titles are rendered as plain text in
this reduced vocabulary.

## Resolve links and expose missing targets

`xsl:key` indexes sections by ID. The cross-reference template looks up its
target and uses the target title as the link label. An unresolved reference
produces a `span` with `class="missing-reference"`, the original `data-target`,
and `[Missing section: retired-section]` as visible text. It creates no broken
anchor link.

The broken-reference fixture still transforms successfully. The result contains
a visible editorial defect; a publication host should inspect these markers and
hold the page for correction. Successful serialization is not publication
approval. Duplicate IDs are different: they make navigation ambiguous, so the
root template refuses the document entirely.

## Run the chapter

From the repository root:

```powershell
dotnet test tests/Bosak.Xslt.Tests/Bosak.Xslt.Tests.csproj -c Release --filter FullyQualifiedName~WorkedExampleTests.Manual
```

[WorkedExampleTests](../../../../tests/Bosak.Xslt.Tests/Documentation/WorkedExampleTests.cs)
executes these exact published files. Both complete HTML outputs are compared
with mixed-content whitespace preserved. The tests also check that every emitted
anchor points to a rendered section and that duplicate IDs refuse publication.
The authored HTML uses no void elements; the harness removes the HTML doctype
and compares its XML-compatible element structure. This is a focused example
comparison, not a general-purpose HTML parser or an HTML conformance claim.

The host compiles the stylesheet with its file URI, loads the source preserving
whitespace, and calls `CaptureTransform`. Its explicit base output URI is
`https://example.invalid/reference/principal.xml`; output remains in memory.
The stylesheet's header and template comments record these contracts directly.

## Try the next change

Add a second paragraph containing two references and text between them. Preserve
the full expected paragraph in a fixture, then add a template for a new inline
element such as a product-code span. If unknown elements should cause failure,
replace the shallow-skip policy with that explicit contract and test it.

For a different template-selection task, continue to [order reconciliation](../order-reconciliation/README.md).
The [instruction separator](../../xslt-instructions/instruction-separators.md)
entry shows an optional 4.0 convenience for compact lists; it is not required by
this portable 3.0 publishing solution.

<div align="center">
  <img src="../../../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak worked example: XML manual publishing">
  <br><br><h1>Publish an XML Manual as HTML</h1>
  <p>Templates, mixed content, a nested table of contents, and references readers can trust</p>
</div>

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

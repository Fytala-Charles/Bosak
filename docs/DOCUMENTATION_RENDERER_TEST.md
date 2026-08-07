<p align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="160" alt="Fytala documentation renderer test">
</p>

# Documentation Renderer Test

Use this document to approve a FYTALA documentation-kit version in VS Code's built-in Markdown preview, Markdown Preview Enhanced, and Markdown PDF. It deliberately exercises the elements whose presentation is governed by the kit.

## Typography and links

Body text should use the configured type stack with dark-slate text. A [normal link](https://fytala.com) should remain visually distinct, and `inline code` should be readable without overpowering the paragraph.

> Blockquotes should have a restrained accent and remain readable in both preview and PDF output.

## Table

| Element | Expected result | Reference |
|---|---|---|
| Header | Dark slate with light mint text | `--table-header-bg` |
| Link in header | Inherits the header foreground | [Style guide](DOCUMENTATION_STYLE_GUIDE.md) |
| Alternating row | Subtle neutral contrast | Canonical stylesheet |

## Code

```csharp
public sealed record DocumentationKit(string Version, bool IsPortable);
```

## Mermaid diagram

```mermaid
flowchart LR
    Manifest[Versioned manifest] --> Preview[Markdown preview]
    Manifest --> Enhanced[Enhanced preview]
    Manifest --> Pdf[PDF export]

    classDef prime fill:#F0FFF0,stroke:#518D8F,color:#2F4F4F
    classDef platform fill:#E8F4EE,stroke:#556B2F,color:#2F4F4F
    class Manifest prime
    class Preview,Enhanced,Pdf platform
```

<p class="fytala-figure-caption"><strong>Renderer coverage.</strong> One versioned contract drives all supported local renderers.</p>

## Acceptance checklist

- The logo renders without a decorative frame.
- Table headers use dark slate and light mint, including links and inline code.
- The Mermaid canvas is white while semantic node colors remain visible.
- Code blocks, blockquotes, links, and captions remain legible.
- PDF output preserves local images, page margins, and diagram boundaries.

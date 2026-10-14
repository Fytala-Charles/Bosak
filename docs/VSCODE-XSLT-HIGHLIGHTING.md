<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak VS Code XSLT highlighting correction">
  <br><br><h1>XSLT highlighting correction</h1>
  <p>Published extension 0.1.6 - 14 October 2026</p>
</div>

The XML grammar previously took precedence over XSLT expression rules, while the semantic provider scanned entire XML files as XPath. The correction gives attribute names and literal values separate scopes and places XPath expressions in `meta.embedded.block.xpath`. `name`, `href`, namespace declarations and literal result attributes retain XML value styling. Expression rules support single and double XML quotes and multiline attributes.

For `<xsl:value-of select="concat('a','b')"/>`, the actual VS Code TextMate tokenizer with the installed Dark+ theme produces:

| Text | Classification | Dark+ color |
|---|---|---|
| `select` | XML attribute name | `#9CDCFE` (light blue) |
| `concat` | XPath function | `#DCDCAA` (yellow) |
| `'a'`, `'b'` | XPath strings | `#CE9178` (orange) |

The language server recognizes unprefixed and prefixed functions inside expression-valued XSLT attributes. It masks XPath string contents and nested comments before finding code tokens and emits single-line, nonoverlapping semantic tokens. Ordinary XML values, text, comments, CDATA and processing instructions are excluded from XPath scanning. Doubled quote escapes use XPath syntax rather than backslash escapes.

Verification: all 8,170 solution tests pass (including 5,284 XSLT tests); the separate language-server project passes 77 tests, including 12 focused semantic checks. TextMate tests cover quote styles, function/string/variable scopes, XML exclusions, multiline attributes and doubled quotes; extension TypeScript compiles. CI now runs both the separate language-server tests and the TextMate checks. Engine source and frozen conformance gate membership are unchanged; their release receipts remain authoritative.

The correction retains the existing conventional `xsl:` prefix support. Attribute value templates, text value templates and arbitrary XSLT prefix aliases are separate work. Exact colors depend on the active theme. This follows [VS Code's embedded-language scope guidance](https://code.visualstudio.com/api/language-extensions/syntax-highlight-guide).

Install the local `vscode-bosak-0.1.6.vsix` using **Extensions: Install from VSIX...**, then **Developer: Reload Window**. Confirm the file's language mode is **XSLT**. Library version 1.2.1 remains published; extension 0.1.6 is published on the [VS Code Marketplace](https://marketplace.visualstudio.com/items?itemName=fytala.vscode-bosak). The downloaded Marketplace manifest, grammar, client and server match the successful CI package; [publication evidence](VSCODE-0.1.6-PUBLICATION.json) records revisions and hashes. Updating the installed extension and reloading VS Code enables this release. Evidence is recorded in [the highlighting receipt](VSCODE-XSLT-HIGHLIGHTING.json).

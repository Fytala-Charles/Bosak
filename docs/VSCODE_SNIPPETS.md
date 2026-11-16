# VS Code snippets

Extension **0.3.0** provides 20 snippets for XPath 3.1, XQuery 3.1 and XSLT 3.0. Install or update [Bosak XPath / XSLT](https://marketplace.visualstudio.com/items?itemName=fytala.vscode-bosak) in VS Code.

Open a file in **XPath**, **XQuery** or **XSLT** language mode. Run **Insert Snippet** from the Command Palette, or type a prefix below and invoke completion with Ctrl+Space. Tab advances through placeholders; Shift+Tab goes back. Editing a linked name updates its mirrored uses. The final cursor follows the inserted construct. No global editor settings are changed.

| Language | Prefixes |
|---|---|
| XSLT | `bosak-stylesheet`, `bosak-template`, `bosak-named-template`, `bosak-xslt-function`, `bosak-param`, `bosak-xslt-variable`, `bosak-choose`, `bosak-for-each`, `bosak-group`, `bosak-iterate` |
| XQuery | `bosak-xquery-main`, `bosak-xquery-module`, `bosak-xquery-function`, `bosak-flwor`, `bosak-xquery-variable` |
| XPath | `bosak-if`, `bosak-for`, `bosak-let`, `bosak-map`, `bosak-array` |

Start a new XSLT document with `bosak-stylesheet`. Its namespace bindings support `xsl:`, `xs:` and `local:` names used by the declaration snippets. Insert templates/functions at stylesheet level, parameters/variables where permitted, and choose/iteration instructions inside a template. An XQuery function or variable snippet is a prolog declaration: add a query body after the declaration. A library module is saved/imported as a module rather than run as a main query. An XPath snippet is an expression, not an XML attribute: quote/escape it appropriately when embedding it in XML. Snippets are language-scoped, not syntax-aware; placement remains the author's responsibility.

For example, `bosak-for` starts as `for $item in (1, 2, 3) return $item`. Change the first placeholder to `entry` and both occurrences become `$entry`. Use `bosak-flwor` in XQuery for a version with let/where/order clauses.

The collection is declarative and does not require the language server to start before insertion. Existing completions remain available. No schema-aware, streaming, experimental 4.0 or package/module acquisition capability is implied by a template.

Validation and exact release identities are recorded in the [publication receipt](extension-0.3.0-publication/README.md).

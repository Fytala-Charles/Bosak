# VS Code snippets

The development extension 0.3.0 adds 20 snippets for the existing XPath 3.1, XQuery 3.1 and XSLT 3.0 profiles. Marketplace 0.2.7 does not include this collection yet.

Open a file in **XPath**, **XQuery** or **XSLT** language mode. Run **Insert Snippet** from the Command Palette, or type a prefix below and invoke completion with Ctrl+Space. Tab advances through placeholders; Shift+Tab goes back. Editing a linked name updates its mirrored uses. The final cursor follows the inserted construct. No global editor settings are changed.

| Language | Prefixes |
|---|---|
| XSLT | `bosak-stylesheet`, `bosak-template`, `bosak-named-template`, `bosak-xslt-function`, `bosak-param`, `bosak-xslt-variable`, `bosak-choose`, `bosak-for-each`, `bosak-group`, `bosak-iterate` |
| XQuery | `bosak-xquery-main`, `bosak-xquery-module`, `bosak-xquery-function`, `bosak-flwor`, `bosak-xquery-variable` |
| XPath | `bosak-if`, `bosak-for`, `bosak-let`, `bosak-map`, `bosak-array` |

Start a new XSLT document with `bosak-stylesheet`. Its namespace bindings support `xsl:`, `xs:` and `local:` names used by the declaration snippets. Insert templates/functions at stylesheet level, parameters/variables where permitted, and choose/iteration instructions inside a template. An XQuery function or variable snippet is a prolog declaration: add a query body after the declaration. A library module is saved/imported as a module rather than run as a main query. An XPath snippet is an expression, not an XML attribute: quote/escape it appropriately when embedding it in XML. Snippets are language-scoped, not syntax-aware; placement remains the author's responsibility.

For example, `bosak-for` starts as `for $item in (1, 2, 3) return $item`. Change the first placeholder to `entry` and both occurrences become `$entry`. Use `bosak-flwor` in XQuery for a version with let/where/order clauses.

The collection is declarative and does not require the language server to start before insertion. Existing completions remain available. No schema-aware, streaming, experimental 4.0 or package/module acquisition capability is implied by a template.

## Qualification

Node checks enforce language registration, unique prefixes, ordered placeholders, mirrored names and escaped literal dollars. Twenty default expansions run against the actual Bosak XPath/XQuery/XSLT APIs in language-server tests, including declaration wrappers and an imported XQuery library module. An additional edited-name/sequence case verifies mirror semantics. These use the collection's restricted snippet syntax; they do not claim automated VS Code UI/tab-navigation acceptance.

Before publication, verify the final VSIX contains all three contributed JSON files. For a manual editor review, open one file per language, insert a snippet via the picker and prefix completion, edit a mirrored name, traverse placeholders forward/backward and inspect the final cursor. Test XSLT declaration placement in a stylesheet and confirm snippets remain available when the server is stopped. Public release remains a separate exact-artifact preparation/publication step.

## Source references and scope

This change adds examples and editor metadata; it changes no engine semantics or public API. The immutable language references are the W3C Recommendations listed below:

- [XPath 3.1](https://www.w3.org/TR/2017/REC-xpath-31-20170321/): sections 3.9?3.10 (for/let), 3.12 (conditional), 3.11 (maps/arrays).
- [XQuery 3.1](https://www.w3.org/TR/2017/REC-xquery-31-20170321/): FLWOR expressions, prolog variable/function declarations and module declarations.
- [XSLT 3.0](https://www.w3.org/TR/2017/REC-xslt-30-20170608/): stylesheet declarations, templates/functions, parameters/variables, conditional and iteration instructions. XSLT's Recommendation is dated 8 June 2017.

VS Code's [snippet syntax](https://code.visualstudio.com/docs/editing/userdefinedsnippets#_snippet-syntax) supplies tab stops, defaults, mirrored stops and dollar escaping. This collection intentionally uses that small subset. No new language adoption or deviation is introduced.

Exact source review pins and content hashes are recorded in [SPECIFICATION-SOURCES.json](SPECIFICATION-SOURCES.json).

Local results: **26,488 solution tests and 111 language-server tests passed** (21 new snippet cases). The first solution run lacked pinned QT3 fixtures; after initialization, the exact runtime case set passed. The initial XSLT test expectation was corrected to include standard atomic-value spacing. Build completed with twelve warnings in unchanged source/dependency auditing. See [validation](VALIDATION.json); run `python docs/vscode-snippets/verify.py` for offline receipt checks.

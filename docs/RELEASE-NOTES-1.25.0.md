<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.25.0 release notes">
  <h1>Bosak 1.25.0 release notes</h1>
  <p>24 October 2026</p>
</div>

Bosak 1.25.0 adds **Move up and Move down together** for adjacent supported literal result elements under the same literal parent. The engine preserves complete subtree bytes and the ordinary whitespace separator in one exact exchange, supplies total reordered node/attribute correspondence and selects the actual moved element. Direction-bound snapshot authority and separate Changed/NoChange/refusal results support explicit consumer review and Apply.

The initial profile uses the dependency-free explicit 3.0/4.0 Legacy authoring host, supported no-namespace parent/children with creation-compatible ancestry, constant result attributes and the REQ-136 closed vocabulary. UTF-8 with/without BOM and BOM-marked UTF-16LE/BE are supported. Intervening comments/PIs/instructions/significant text, boundaries and unproved contexts refuse. Interior markup travels with its subtree. NoChange requires exact source-byte equality and retains original selection without a candidate or history authority. Work, source, graph and complete-evidence limits are explicit. [API and profile](REQ-137-IMPLEMENTATION.md).

This release also includes the separately approved Experimental substring correction: `index-of-substring('abc', ())` yields positions 1–4 only in `XPath40Experimental`; frozen `XPath40` retains XPTY0004. XSLT hosts retain the frozen boundary. [Contract evidence](xpath40-catalog/INDEX-OF-SUBSTRING-CONTRACT.md). Newer upstream qt4tests adoption and full XPath/XSLT 4.0 conformance remain separate; REQ-126 is partial.

Install `dotnet add package Bosak.Xslt --version 1.25.0` or `dotnet add package Bosak.XPath.Api --version 1.25.0`. Existing Create/Rename/Delete and other authoring contracts retain their behavior. Braid integration and consumer acceptance are pending; engine tests and source review do not establish UI Apply/history or native acceptance.

[Verified publication and package proof](RELEASE-1.25.0-PUBLICATION.md).

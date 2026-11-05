# Commercial Offerings — Bosak

Bosak's XPath, XSLT and XQuery engine is free, open-source software under the [Apache License 2.0](license.md). [Bosak.Schema](https://github.com/Fytala-Charles/Bosak.Schema) is also Apache-2.0 and requires no activation, trial or subscription. Commercial users can build on the Apache components subject to their license conditions, without a schema access fee.

This document describes what Fytala offers on top of the free core.

## How support works

The free core is community-supported: questions and usage help go to [GitHub Discussions](https://github.com/Fytala-Charles/Bosak/discussions), bugs to [GitHub Issues](https://github.com/Fytala-Charles/Bosak/issues), and engine-correctness work is always done in the open. A commercial support contract adds direct engineering access (email/video), guaranteed response times, and priority triage on top of that — it does not gate the community channels, and it never gates conformance fixes.

## What is free under Apache-2.0

- The complete XPath 3.1 / XSLT 3.0 / XQuery 3.1 engine
- All NuGet packages published from this repository
- Conformance fixes and engine correctness work — always community work, never paywalled
- The VS Code language-server extension
- The public Bosak.Schema source and its supported schema compilation/validation services; see the [Apache publication record](docs/SCHEMA-APACHE-PUBLICATION-2026-10-29.md)

## Commercial offerings

| Offering | What you get |
|----------|--------------|
| **Support contract** | Direct engineering access (email, video), guaranteed response times (SLA), root-cause analysis for engine issues found in your environment |
| **Priority triage** | Your issues and reproductions move to the front of the queue |
| **Roadmap influence** | Votes on upcoming REQ items and release priorities |
| **Consulting & development** | Custom engine features, adapters, performance work, migration from Saxon/XmlCompiledTransform |
| **Indemnification & custom terms** | Available for OEM redistributors on request |

## Product boundaries

The earlier proposal to charge separately for Schema is superseded by its Apache-2.0 transition on 29 October 2026. Schema remains modular, with staged XSD 1.1 coverage and a separate compatibility lifecycle; that technical boundary does not impose an activation or subscription requirement.

Braid and Folio retain their separate commercial/proprietary terms. Billing and key-generation infrastructure remains private in Bosak.Licensing. Commercial services and product agreements are separate from access to the open engine and Schema; this documentation does not change existing customer agreements or live billing configuration.

## Contact

- Email: charles.korthout@fytala.com
- Web: https://www.fytala.com

---

*Bosak is a product of Fytala. "Bosak" and the Fytala name are trademarks of Fytala; the Apache-2.0 license covers the code, not the names.*

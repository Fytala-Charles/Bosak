# NuGet 1.86.0 publication - 25 November 2026

All ten approved Bosak packages were published from the existing preparation artifact without rebuilding. [Publisher run 45663011066](https://github.com/Fytala-Charles/Bosak/actions/runs/45663011066) succeeded; preparation was skipped. The ten packages are available from NuGet.org, including [Bosak.XPath.Core 1.86.0](https://www.nuget.org/packages/Bosak.XPath.Core/1.86.0).

Source commit: `2d353912954f03caa44ba2d6e96a8a6224a077d5`. Preparation run: [45657153789](https://github.com/Fytala-Charles/Bosak/actions/runs/45657153789). Approved unsigned manifest SHA-256: `b7421f5e9524b463493574ba5108b59dc404720769d0ba1d71e60a6b6f24f120`.

Every public package payload matches its approved unsigned archive byte for byte; the only added ZIP entry is NuGet's repository signature. `dotnet nuget verify --all` passed for all ten packages. Signed and unsigned archive hashes, and individual payload hashes, are retained separately in [signed delivery evidence](SIGNED-PAYLOADS.json). The version-specific packaged README also matches the source bytes.

Fresh source consumers restored only from NuGet.org into new empty caches. All 53 smoke checks, 73 guarded calendar/no-friend checks and both packaged README examples passed. All ten loaded smoke-consumer DLL hashes match the approved package DLLs. These checks cover selected XPath, calendar, XQuery, XSLT, streaming/lifetime and loopback BaseX behavior; they do not qualify a live database, general schema-aware execution, performance or complete specification coverage.

The producer preparation passed 26,750 solution tests, frozen QT3 31,397/0/424 and gated qt4 8,941/0/1,918, with exact skip maps checked. Those are producer receipts, separate from the new registry consumer checks. Historical feature dossiers and the current coverage audit remain unchanged.

The exact calendar foundation, transport, explicit comparison, plain interoperability and guarded comparison APIs are now delivered in Core 1.86.0. Native Bosak.Schema dateTime/time/dateTimeStamp admission and a Schema package-pin migration require a separate adopted contract. C45/public API acceptance, default operator/map behavior and the extension's bundled 1.81.17 engine are unchanged. No Git tag or GitHub Release accompanies this publication.

[Approval](APPROVAL.json), [publisher status](PUBLISHER.json), [original manifest](RELEASE-MANIFEST.json), [consumer review](CONSUMER-REVIEW.json), [example results](README-EXAMPLES.json) and [completion](VERIFIED.json) record delivery. EVIDENCE.zip contains source-only consumers, registry-only configurations and raw verification logs; no package binaries or credentials. Run `python docs/release-1.86.0-publication/verify.py` for offline receipt integrity checks; it does not launch .NET or publish packages.

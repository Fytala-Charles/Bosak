# NuGet 1.81.17 publication — 16 November 2026

All ten approved Bosak packages were published to NuGet.org from the existing preparation artifact, without rebuilding. [Publisher run 43867426381](https://github.com/Fytala-Charles/Bosak/actions/runs/43867426381) succeeded with ten successful pushes and no duplicates. The preparation job was skipped.

Source: `6a7618e9dcf70eb8a67d5d1789ff7b787133b52d`; preparation run `43861115818`; exact unsigned manifest SHA-256: `2ad34e65ade407a914dac93b551caf703a9cfc298ecb82921bd750de3dade2ca`.

All ten public flat-container versions and registration records were verified. Every delivered ZIP payload matches the approved artifact; the only added entry is NuGet's repository signature. Delivered signed archive hashes are recorded separately from the original unsigned hashes. `dotnet nuget verify --all` passed for all ten packages.

A fresh consumer restored only from NuGet.org into an empty cache and passed 53 checks on .NET 10.0.11. All ten loaded DLLs match the approved package DLL hashes. Coverage includes XPath, date fallbacks, XQuery, XSLT, streaming identity/lifetimes and a loopback BaseX protocol fixture. This does not qualify live databases, schema-aware execution, representative performance or every XSLT profile.

The original preparation passed 26,488 solution tests and the frozen QT3 31,397/0/424 and gated qt4 8,941/0/1,918 gates. The separately qualified source standalone XSLT result remains 10,221/0/4,380; it was not rerun against this package cohort. Skips and profile limits remain applicable; no complete-conformance claim is made.

Published package READMEs retain their immutable source's historical 1.81.2 installation example. Repository instructions now point to 1.81.17. Packages were not rebuilt to change documentation. VS Code extension 0.2.6 is unchanged. No Git tag, GitHub release or source push accompanies this delivery.

The JSON receipts, original release manifest and source-only EVIDENCE.zip retain the checks. Raw packages and the fresh cache remain in the ignored local publication packet. Run `python docs/release-1.81.17-publication/verify.py` from this checkout for offline evidence verification; it does not launch .NET or publish anything.

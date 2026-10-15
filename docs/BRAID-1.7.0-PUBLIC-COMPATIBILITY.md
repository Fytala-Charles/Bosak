<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Braid verification against public Bosak 1.7.0">
  <br><br><h1>Braid: public Bosak 1.7.0 compatibility</h1>
  <p>Isolated verification, 15 October 2026</p>
</div>

A new snapshot of the current Braid working tree passes **843/0/0** tests against public NuGet 1.7.0: Core 27, Application 291, Infrastructure 314 and Workbench 211. Its 280-file manifest and original dependency pin are preserved. The build has zero warnings and errors. [Public compatibility evidence](BRAID-1.7.0-PUBLIC-COMPATIBILITY.json) is separate from the unchanged [candidate-package receipt](BRAID-1.7.0-COMPATIBILITY.json).

The copy replaces its engine ProjectReference with a public Bosak.Xslt 1.7.0 PackageReference, removes copied source-engine projects/validation, disables copied MinVer evaluation and restores into a new nuget.org-only cache. All nine engine archives match the publication receipt. Restored files and Infrastructure/Workbench output DLLs match the public archives; assembly version is 1.7.0.0 and informational revision is exactly the approved `33efd8e5a2dda2e16e4cdc69df87dbc01a8a1961`. Archive SHA-512 and restore content hashes are checked separately.

Original Braid source/test/docs files and Git status match the pre-copy snapshot. The production pin remains unchanged. This proves automated compatibility for the copied working tree; production adoption, native desktop acceptance and independent consumer acceptance remain separate. Scratch input/logs/cache/TRX are retained under `.guard-tmp/braid170-public-*`.

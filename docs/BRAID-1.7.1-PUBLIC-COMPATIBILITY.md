<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Braid public Bosak 1.7.1 compatibility">
  <br><br><h1>Braid: public Bosak 1.7.1 compatibility</h1>
  <p>Isolated package verification, 15 October 2026</p>
</div>

A fresh 280-file snapshot of the current Braid working tree passes **843/0/0** tests against public nuget.org 1.7.1: Core 27, Application 291, Infrastructure 314 and Workbench 211. Build: zero warnings/errors. Nine signed archives, cache/Infrastructure/Workbench output DLLs and exact approved assembly revision verify. [Evidence](BRAID-1.7.1-PUBLIC-COMPATIBILITY.json).

The isolated copy replaces the Infrastructure engine ProjectReference with a PackageReference, removes copied source-engine projects/guards, disables copied MinVer evaluation and uses a fresh nuget.org-only cache. The original source/test/docs files, Git status and production dependency pin remain unchanged. Assembly version is 1.7.1.0 and informational revision is exactly `1.7.1+35e531c101bcbba13a2533f33a7decca5f18eddd`; signed archive SHA-512 and restore content hashes are checked separately.

This proves automated compatibility for the copied working tree. Production pin adoption, native desktop acceptance and independent consumer acceptance remain separate. Original candidate evidence in RELEASE-1.7.1-PREPARATION.json and earlier public compatibility receipts remain unchanged.

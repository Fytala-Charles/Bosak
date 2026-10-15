<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Braid compatibility with Bosak 1.7.0 candidate">
  <br><br><h1>Braid compatibility: Bosak 1.7.0 candidate</h1>
  <p>Isolated automated verification, 15 October 2026</p>
</div>

The current Braid working-tree snapshot passes **843/0/0** tests against unpublished Bosak 1.7.0 packages from `7978e5d7cc0a3092f466bc4ac70d5deacb339764`: Core 27, Application 291, Infrastructure 314 and Workbench 211. Its 280-file manifest and [compatibility receipt](BRAID-1.7.0-COMPATIBILITY.json) preserve the exact input and results. The earlier 836-test public-1.6.0 run remains a separate immutable receipt; current Workbench tests add seven cases.

The copy replaces its engine project reference with `Bosak.Xslt` 1.7.0, removes engine projects/source-checkout validation from the copied solution and disables copied MinVer evaluation. Bosak packages restore only from the candidate feed; other dependencies restore from nuget.org, all into a fresh cache. Nine engine archives, restored files and Infrastructure/Workbench output DLLs match. Assembly versions are 1.7.0.0 with exact candidate informational revision; raw archive SHA-512 and restore content hashes are verified separately.

The original Braid files and Git status still match the pre-copy snapshot. Its dependency pin remains `5bab4799783569d538cec9b03d37a57454f0eebc`. No files, commits or package pins in the original Braid checkout were changed. This establishes automated compatibility for that snapshot; production adoption, native desktop acceptance and independent consumer approval remain separate.

[Release preparation](RELEASE-NOTES-1.7.0.md) records engine/gate checks. Public libraries remain 1.6.0 and the extension remains 0.2.1 with engine 1.6.0. No engine publication is performed by this verification.

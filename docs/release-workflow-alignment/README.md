# Release metadata and workflow alignment - 8 November 2026

The selected source pins are now Core/server **1.81.2**, extension manifest/lock **0.2.6**, and the explicit ten-package set including the existing Database REST loaders. Normal MSBuild metadata includes NOTICE. Extension NOTICE and Unicode license copies are tracked and checked against their original sources. No engine or command-handler body changed.

## Workflow behavior

Tags and default manual dispatch prepare artifacts only. Publication is a separate dispatch requiring a successful source run and the reviewed `release-manifest.json` SHA-256. It validates the run's workflow/repository/event/source, downloads that run's artifact, checks out its source commit, and verifies versions, complete file set, byte hashes, dependencies and legal assets. NuGet pushes the verified files; vsce publishes the existing VSIX with `--packagePath`. Neither publish job rebuilds.

The registered NuGet workflow filename and `publish` job ID, NUGET_USER and VCSE_PAT secret names remain. Credentials are referenced only in explicit publication steps. No credential values, settings or approval/environment rules were read or changed. Both workflows use the tested SDK10.0.301; VSIX packaging uses Node24.16.0 and pinned vsce4.0.0. Frozen conformance totals/skip reasons and the experimental11 count are checked before NuGet sealing. CI adds recipe-control tests. [Maintainer guide](../RELEASING.md).

## Local validation and evidence

- [Control receipts](LOCAL-CONTROLS.json):14 Python artifact/gate tests, one valid source-run case and nine refusal cases, parsed YAML/tool/publish-isolation checks and a CLI dirty-source refusal with no output manifest.
- [Real artifact controls](real-artifact-controls.json) exercise the verifier against previously qualified ten-package/VSIX bytes and their original immutable source context. They do not reseal those artifacts under the changed source.
- [Stock pack metadata](STOCK-PACK-METADATA.json) shows all ten packable projects evaluate1.81.2 and exactly one packed NOTICE without the preview overlay.
- [Historical public API shape](public-run-metadata-shape.json) confirms the fields used by source-run validation. It is a read-only check of an old public run, not execution of the new hosted workflows.
- [Task-local YAML tooling install](yaml-tool-install.json) pins PyYAML6.0.3 in an ignored validation directory. No global Python installation changed.
- `before/` preserves the original metadata and workflow bytes. Historical candidate/preview packets retain their old source and artifact identities. Live-source preflights pinned to old versions must be interpreted in their original snapshot, not treated as current version checks.

A clean Git checkout root is required for sealing and verification; tracked changes and untracked inputs refuse. Archives must have the exact selected set and safe unique paths. Reviewed hash/source/version/dependency mismatch, missing or altered legal assets, unsafe ZIP paths, wrong publisher and changed gate names/reasons refuse. Line ending transport alone is normalized for cross-platform skip-map comparison, retaining exact names/reasons.

## Qualification boundary

The current version/workflow changes require a new immutable source checkpoint and final-cohort stock-pack qualification. The older locally qualified1.81.2 version-override cohort remains historical. Hosted Actions execution, cross-run artifact download, publisher authentication and service-side publication have not run here. Owner workload/budget/C45/API and final signing/provenance/publication decisions remain open.

No tag, workflow dispatch, publication, owner-repository write, machine permission or active editor profile change occurred. Next: commit the reviewed source/recipe alignment, build and verify stock1.81.2 artifacts from that exact commit, then present the still-open acceptance items separately.

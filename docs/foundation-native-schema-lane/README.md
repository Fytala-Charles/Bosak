# TA-44: native Schema source and unchanged-binary lanes

6 November 2026. Core development remains **1.81.0**. Fresh **451/0 baseline**, **451/0 isolated source**, and **451/0 unchanged-binary** native Schema regression runs pass. They cover the same 451 existing case identities and are not 1,353 distinct tests. All nine observed engine hashes match the intended lane in the default noncollectible load context. The binary candidate changes only nine engine DLLs; every consumer/test/configuration/fixture byte remains baseline-identical.

[Run guide](GUIDE.md), [scope and findings](AUDIT.md), [adoption](ADOPTION.json), [native results](evidence/NATIVE-RESULTS.json), [binary delta](evidence/BINARY-DELTA.json), [loaded identities](evidence/LOADED-BINDINGS.json), [status](STATUS.json), [next plan](PLAN.json) and [verifier](tools/verify.py).

The native assessment worktree and Core production sources/binaries remain unchanged. The old 114-test layout and historical dossiers are untouched. An initial diagnostic restore failure is retained; the isolated retry used cached package versions with command-scoped NuGetAudit=false and built with zero warnings/errors. No production audit policy or vulnerability acceptance changed.

This qualifies the exercised existing native document-assessment regression lanes only. It does not implement the new public provider binding, accept owner/native performance budgets, finish R3/R4, freeze APIs or release packages. Next PV-Q03 maps actual native provider identity/issuance/lifetime to the public contract. No external write, owner message, commit, push or publication.

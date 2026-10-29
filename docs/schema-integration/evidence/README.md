# Reading the qualification evidence

[RESULTS.json](../RESULTS.json) separates source tests, unchanged-binary substitution, published-dependency baselines and corrected reruns. Counts from these scopes are not additive. A run-level failure remains recorded even when its completed cases passed.

Logs are retained as readable UTF-8 with LF endings and trailing whitespace trimmed. Each log entry in the results records both the retained-file hash and the original local raw-output hash. Shell-produced UTF-16 logs are transcoded for review; the original raw files remain under `.guard-tmp`. Skip receipts retain their original bytes and are compared by complete names/reasons against the sealed baseline.

Source manifests record committed consumer revisions and exact C# identities. XmlDb's uncommitted work is excluded. Candidate build routing, Braid's isolated expected-version adjustment and binary-layout corrections are explicit; no original consumer source, adopted dependency pin or storage format was changed. Native UI acceptance and full XSD 1.1/provider certification are outside this evidence.

The local preview inventory describes unsigned development packages and a package-only smoke run. It is not a publication receipt, signature verification or consumer adoption decision.

# Bridge qualification evidence

The TRX files contain the complete core and Schema executions. The initial sandbox core run has 45 database-provider failures because its local HTTP listener could not start; the separately authorized database run passes all 47. The other nine core projects pass, including 8,196 XSLT cases. The aggregate final result is 24,207 passing core cases, with the failed sandbox run retained rather than counted as a successful run.

`qt3-exact.txt`, `qt4-exact.txt` and their skip dumps record the normal gates with exact mappings compared against the sealed integration checkpoint. `source-identities.json` identifies the assessed implementation. `documentation-checks.json` records HTML/local-destination checks with unchanged Fytala CSS. `example.txt` records the full-document example. These scopes overlap; do not add them into a single test count.

An additional temporary-copy XSLT diagnostic was started while the original suite was still running. Its VSTest blame syntax was rejected, so no watchdog was active. That redundant run was canceled after the original suite completed successfully; it contributes no qualification. The successful original XSLT TRX is the authoritative receipt.

Earlier native-assessment and integration dossiers remain sealed and unchanged. No hosted CI, package publication, full XSD 1.1 certification or consumer acceptance is implied.

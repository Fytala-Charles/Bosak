#!/bin/bash
cd /d/Development/Bosak
DLL=tests/Bosak.Xslt.Conformance/bin/Release/net10.0/Bosak.Xslt.Conformance.dll
SETS="source-document validation import-schema type-functions as match streamable strip-type-annotations xpath-default-namespace accumulator strip-space nodetest treat-as type-expr merge catalog notation type sf-avg si-apply-templates si-copy si-copy-of si-document si-element si-lre si-result-document sx-GeneralComp-eq sx-GeneralComp-ge sx-GeneralComp-gt sx-GeneralComp-le sx-GeneralComp-lt sx-GeneralComp-ne si-fork si-map"
for s in $SETS; do
  dotnet "$DLL" tests/xslt30-test/catalog.xml "$s" > ".sweep-baselines/373-basic-$s.log" 2>&1
  echo "### $s"
  grep -E "^  Done" ".sweep-baselines/373-basic-$s.log"
done
for s in si-fork si-map source-document; do
  dotnet "$DLL" tests/xslt30-test/catalog.xml "$s" --schema-aware > ".sweep-baselines/373-schemaaware-$s.log" 2>&1
  echo "### schema-aware $s"
  grep -E "^  Done" ".sweep-baselines/373-schemaaware-$s.log"
done
echo "AFTER DONE"

#!/bin/bash
cd /d/Development/Bosak
DLL=tests/Bosak.Xslt.Conformance/bin/Release/net10.0/Bosak.Xslt.Conformance.dll
SETS="source-document validation import-schema type-functions as match streamable strip-type-annotations xpath-default-namespace accumulator strip-space nodetest treat-as type-expr merge catalog notation type sf-avg si-apply-templates si-copy si-copy-of si-document si-element si-lre si-result-document sx-GeneralComp-eq sx-GeneralComp-ge sx-GeneralComp-gt sx-GeneralComp-le sx-GeneralComp-lt sx-GeneralComp-ne si-fork si-map"
for s in $SETS; do
  echo "### $s"
  dotnet "$DLL" tests/xslt30-test/catalog.xml "$s" > ".sweep-baselines/372-basic-$s.log" 2>&1
  grep -E "^  (Done|SKIP)" ".sweep-baselines/372-basic-$s.log" | tail -3
done
echo "BASELINE DONE"

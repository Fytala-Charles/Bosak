set -euo pipefail
mkdir -p /work/baseline/source /work/candidate/source /work/fixtures /results
for fx_role in baseline candidate; do
  (tar -C /$fx_role --exclude='*/bin' --exclude='*/obj' -cf - .) | tar -C /work/$fx_role/source -xf -
done
dotnet --info > /results/dotnet-info.txt
uname -a > /results/kernel.txt
stat -f -c '%T' /work > /results/filesystem.txt
for fx_role in baseline candidate; do
  cd /work/$fx_role/source
  dotnet restore tests/Bosak.XmlDb.Xdm.Tests/Bosak.XmlDb.Xdm.Tests.csproj --source /feed --packages /work/cache-$fx_role -p:NuGetAudit=false --force-evaluate > /results/$fx_role-restore.txt 2>&1
  dotnet restore qualification/Probe/Fx009Probe.csproj --source /feed --packages /work/cache-$fx_role -p:NuGetAudit=false --force-evaluate > /results/$fx_role-probe-restore.txt 2>&1
  dotnet build tests/Bosak.XmlDb.Xdm.Tests/Bosak.XmlDb.Xdm.Tests.csproj -c Release --no-restore > /results/$fx_role-test-build.txt 2>&1
  dotnet build qualification/Probe/Fx009Probe.csproj -c Release --no-restore > /results/$fx_role-probe-build.txt 2>&1
done
mkdir -p /work/frozen/control/tests /work/frozen/control/probe /work/frozen/replacement/tests /work/frozen/replacement/probe
for fx_mode in control replacement; do
 cp -a /work/baseline/source/tests/Bosak.XmlDb.Xdm.Tests/bin/Release/net10.0/. /work/frozen/$fx_mode/tests/
 cp -a /work/baseline/source/qualification/Probe/bin/Release/net10.0/. /work/frozen/$fx_mode/probe/
done
for fx_kind in tests probe; do
 for fx_package in /engine/Bosak.XPath.*.dll; do cp "$fx_package" /work/frozen/replacement/$fx_kind/; done
done
fx_inventory() { (cd "$1" && find . -type f -print0 | sort -z | xargs -0 sha256sum); }
fx_dirs() { (cd "$1" && find . -type d | sort); }
for fx_mode in control replacement; do
 for fx_kind in tests probe; do fx_inventory /work/frozen/$fx_mode/$fx_kind > /results/layout-$fx_mode-$fx_kind-before.txt; done
done
export DOTNET_STARTUP_HOOKS=/observer/PackageBindingHook.dll
export BOSAK_PACKAGE_BINDING_REPORT_DIRECTORY=/results/seed-bindings
timeout --kill-after=5s 180s dotnet /work/frozen/control/probe/Fx009Probe.dll seed /work/fixtures/existing > /results/seed.txt 2>&1
fx_inventory /work/fixtures/existing > /results/store-before.txt
fx_dirs /work/fixtures/existing > /results/store-dirs-before.txt
fx_vstest=$(find /usr/share/dotnet/sdk -mindepth 2 -maxdepth 2 -name vstest.console.dll | sort | tail -n 1)
for fx_role in source-baseline source-candidate binary-control binary-replacement; do
 case "$fx_role" in
 source-baseline) fx_test=/work/baseline/source/tests/Bosak.XmlDb.Xdm.Tests/bin/Release/net10.0; fx_probe=/work/baseline/source/qualification/Probe/bin/Release/net10.0;;
 source-candidate) fx_test=/work/candidate/source/tests/Bosak.XmlDb.Xdm.Tests/bin/Release/net10.0; fx_probe=/work/candidate/source/qualification/Probe/bin/Release/net10.0;;
 binary-control) fx_test=/work/frozen/control/tests; fx_probe=/work/frozen/control/probe;;
 binary-replacement) fx_test=/work/frozen/replacement/tests; fx_probe=/work/frozen/replacement/probe;;
 esac
 export BOSAK_PACKAGE_BINDING_REPORT_DIRECTORY=/results/$fx_role-tests-bindings
 mkdir -p /results/$fx_role-tests
 timeout --kill-after=5s 240s dotnet "$fx_vstest" "$fx_test/Bosak.XmlDb.Xdm.Tests.dll" '--Framework:.NETCoreApp,Version=v10.0' '--logger:trx;LogFileName=results.trx' "--ResultsDirectory:/results/$fx_role-tests" > /results/$fx_role-tests.txt 2>&1
 export BOSAK_PACKAGE_BINDING_REPORT_DIRECTORY=/results/$fx_role-probe-bindings
 timeout --kill-after=5s 180s dotnet "$fx_probe/Fx009Probe.dll" qualify /work/fixtures/existing /results/$fx_role-observations.json /work/fixtures/$fx_role-scratch > /results/$fx_role-probe.txt 2>&1
 fx_inventory /work/fixtures/existing > /results/$fx_role-store-after.txt
 fx_dirs /work/fixtures/existing > /results/$fx_role-store-dirs-after.txt
 cmp /results/store-before.txt /results/$fx_role-store-after.txt
 cmp /results/store-dirs-before.txt /results/$fx_role-store-dirs-after.txt
 printf '%s passed tests/probe/store preservation\n' "$fx_role"
done
for fx_mode in control replacement; do
 for fx_kind in tests probe; do
 fx_inventory /work/frozen/$fx_mode/$fx_kind > /results/layout-$fx_mode-$fx_kind-after.txt
 cmp /results/layout-$fx_mode-$fx_kind-before.txt /results/layout-$fx_mode-$fx_kind-after.txt
 done
done
printf 'Completed isolated Linux source/package/binary characterization.\n'

tar -C /work/frozen/control -czf /results/frozen-control.tar.gz .
tar -C /work/frozen/replacement -czf /results/frozen-replacement.tar.gz .

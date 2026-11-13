"""Prepare and inspect a pinned copied-runner diagnostic; never changes production qualification."""
import argparse
import collections
import csv
import hashlib
import importlib.util
import json
import math
from pathlib import Path
import shutil
import subprocess
import sys
import xml.etree.ElementTree as ET
import zipfile
import time

ROOT = Path(__file__).resolve().parents[1]
BUILD_SOURCE = '04eea7317ab2f71f7f3644b1ebcc30076b9dbdaf'
CALL = '            var result = RunTestCase(testCase, environments, testSetDir, testSetPath, catalogDir, ns, testSetName);'
WRAPPED = '''            var caseCost = CaseCosts.Begin();
            TestResult result = default;
            bool costCompleted = false;
            try
            {
                result = RunTestCase(testCase, environments, testSetDir, testSetPath, catalogDir, ns, testSetName);
                costCompleted = true;
            }
            finally { CaseCosts.End(caseCost, testSetName, testCase.Attribute("name")?.Value ?? "unknown", costCompleted ? result.ToString() : null); }'''
MAIN = '    static void Main(string[] args)\n    {'
INIT = '\n        if (!CaseCosts.Initialize(Path.Combine(Directory.GetCurrentDirectory(), "..", "CASE-COST-PIN.json"))) return;'
PREFIX = '// Generated reversible case-cost observer copy; not the production conformance runner.\n'
OLD_COMMAND = "command = [shutil.which('dotnet'), str(packet / 'runtime/Bosak.Xslt.Conformance.dll'),"
NEW_COMMAND = "command = [plan['diagnosticDotnet'], 'exec', '--fx-version', plan['diagnosticFrameworkVersion'], '--roll-forward', 'Disable', str(packet / 'runtime/Bosak.Xslt.Conformance.dll'),"


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def write(path, value):
    path.write_bytes((json.dumps(value, indent=2) + '\n').encode())


def load_gate():
    spec = importlib.util.spec_from_file_location('gate', ROOT / 'tools/xslt30_gate.py')
    module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module)
    return module


def instrument(data):
    source = data.decode('utf-8').replace('\r\n', '\n')
    if source.count(CALL) != 1 or source.count(MAIN) != 1:
        raise ValueError('Runner source anchor drift')
    result = PREFIX + source.replace(CALL, WRAPPED).replace(MAIN, MAIN + INIT)
    restored = result.removeprefix(PREFIX).replace(WRAPPED, CALL).replace(MAIN + INIT, MAIN)
    if restored != source:
        raise ValueError('Instrumentation not exactly reversible')
    restored_bytes = restored.replace('\n', '\r\n').encode() if b'\r\n' in data else restored.encode()
    if restored_bytes != data:
        raise ValueError('Runner byte restoration failed')
    return result.encode()


def controller(data):
    text = data.decode('utf-8')
    if text.count(OLD_COMMAND) != 1:
        raise ValueError('Bounded controller source drift')
    changed = text.replace(OLD_COMMAND, NEW_COMMAND)
    if changed.replace(NEW_COMMAND, OLD_COMMAND).encode() != data:
        raise ValueError('Controller restoration failed')
    return changed.encode()


def safe_paths(qualified, corpus, output):
    if output.exists():
        raise ValueError('Existing attempt output; never overwrite evidence')
    if output.is_relative_to(qualified) or output.is_relative_to(corpus):
        raise ValueError('Output must be outside retained runtime and corpus')


def runtime_pin(native):
    names = ['dotnet.exe', 'coreclr.dll', 'clrjit.dll', 'hostfxr.dll', 'hostpolicy.dll']
    selected = {name: native[name] for name in names}
    for name, value in selected.items():
        if digest(Path(value['path'])) != value['sha256']:
            raise ValueError('Native pin drift: ' + name)
    framework = Path(selected['coreclr.dll']['path']).parent
    if framework.name != '10.0.11':
        raise ValueError('This diagnostic was reviewed for installed 10.0.11')
    return dict(frameworkVersion='10.0.11', nativeFiles=selected,
        frameworkDirectory=str(framework), frameworkFiles={f.name: digest(f) for f in sorted(framework.iterdir()) if f.is_file()})


def verify_pin(pin):
    current = runtime_pin(pin['nativeFiles'])
    if current != pin:
        raise ValueError('Full framework inventory changed')


def qualified_hashes():
    dossier = ROOT / 'docs/xslt30-case-cost'
    policy = json.loads((dossier/'BASELINE.json').read_text())
    evidence = dossier / policy['evidence']
    if digest(evidence) != policy['evidenceSha256']:
        raise ValueError('Sealed original qualification changed')
    with zipfile.ZipFile(evidence) as original:
        return json.loads(original.read('gate/RUNTIME-HASHES.json'))


def prepare(qualified, corpus, output, native):
    safe_paths(qualified, corpus, output)
    gate = load_gate()
    policy, baseline_plan, inputs, _ = gate.baseline()
    if gate.inventory(corpus) != inputs:
        raise ValueError('Corpus inventory differs from immutable baseline')
    if json.loads((qualified / 'GATE.json').read_text())['success'] is not True:
        raise ValueError('Need a successful retained original qualification')
    original = json.loads((qualified / 'RUNTIME-HASHES.json').read_text())
    if gate.runtime_hashes(qualified / 'runtime') != original or original != qualified_hashes() or len(original) != 32:
        raise ValueError('Qualified application payload changed')
    engine = {name: value for name, value in original.items() if name.startswith('Bosak.') and name.endswith('.dll') and name != 'Bosak.Xslt.Conformance.dll'}
    if len(engine) != 9:
        raise ValueError('Need exact nine qualified engine assemblies')
    pin = runtime_pin(native)
    output.mkdir(parents=True)
    build = output / 'diagnostic'; build.mkdir()
    program = ROOT / 'tests/Bosak.Xslt.Conformance/Program.cs'
    reference_source = subprocess.check_output(['git','show',BUILD_SOURCE+':tests/Bosak.Xslt.Conformance/Program.cs'],cwd=ROOT)
    if reference_source.decode().replace('\r\n','\n') != program.read_bytes().decode().replace('\r\n','\n'):
        raise ValueError('Runner source differs from qualified binary build source')
    (build / 'Program.cs').write_bytes(instrument(program.read_bytes()))
    helper = ROOT / 'tools/xslt30-case-cost/CaseCosts.cs.in'
    shutil.copyfile(helper, build / 'CaseCosts.cs')
    project = ET.Element('Project', Sdk='Microsoft.NET.Sdk'); props = ET.SubElement(project, 'PropertyGroup')
    for key, value in [('OutputType','Exe'),('TargetFramework','net10.0'),('AssemblyName','Bosak.Xslt.Conformance'),('ImplicitUsings','enable'),('Nullable','enable'),('GenerateDocumentationFile','true')]:
        ET.SubElement(props, key).text = value
    refs = ET.SubElement(project, 'ItemGroup')
    for name in engine:
        reference = ET.SubElement(refs, 'Reference', Include=Path(name).stem)
        ET.SubElement(reference, 'HintPath').text = str(qualified / 'runtime' / name)
    ET.indent(project); ET.ElementTree(project).write(build / 'Diagnostic.csproj', encoding='unicode')
    with (output / 'BUILD.log').open('xb') as log:
        subprocess.run(['dotnet','build',str(build / 'Diagnostic.csproj'),'-c','Release'], stdout=log, stderr=subprocess.STDOUT, check=True)
    produced = build / 'bin/Release/net10.0'
    if any(digest(produced / name) != value for name, value in engine.items()):
        raise ValueError('Diagnostic build changed an engine reference')
    shutil.copytree(qualified / 'runtime', output / 'runtime')
    for suffix in ['dll','pdb','deps.json','runtimeconfig.json']:
        name = 'Bosak.Xslt.Conformance.' + suffix
        shutil.copyfile(produced / name, output / 'runtime' / name)
    config = output / 'runtime/Bosak.Xslt.Conformance.runtimeconfig.json'
    value = json.loads(config.read_text()); value['runtimeOptions']['framework']['version'] = '10.0.11'; value['runtimeOptions']['rollForward'] = 'Disable'; write(config, value)
    shutil.copytree(corpus, output / 'corpus')
    write(output / 'CORPUS-BEFORE.json', inputs)
    write(output / 'CASE-COST-PIN.json', pin)
    write(output / 'BASELINE-RUNTIME-HASHES.json', original)
    write(output / 'RUNTIME-HASHES.json', gate.runtime_hashes(output / 'runtime'))
    plan = dict(baseline_plan)
    plan.update(sourceCommit=None, diagnosticSourceCommit=subprocess.check_output(['git','rev-parse','HEAD'],cwd=ROOT,text=True).strip(),
        sourceIdentity='Copied runner diagnostic, not production qualification', wallTimeoutSeconds=1200, memoryLimitBytes=8589934592,
        diagnosticDotnet=pin['nativeFiles']['dotnet.exe']['path'], diagnosticFrameworkVersion='10.0.11')
    write(output / 'PLAN.json', plan)
    bounded = ROOT / 'docs/xslt30-qualification/run.py'
    (output / 'diagnostic-run.py').write_bytes(controller(bounded.read_bytes()))
    write(output / 'IDENTITY.json', dict(diagnosticOnly=True, qualification=False, engineHashes=engine,
        originalRunnerSourceSha256=digest(program), diagnosticSources={f.name:digest(f) for f in [build/'Program.cs',build/'CaseCosts.cs']},
        productionRunnerUnchanged=True, controllerOriginalSha256=digest(bounded), controllerSha256=digest(output/'diagnostic-run.py'),
        toolSha256=digest(Path(__file__)), changedApplicationFiles=[n for n,h in gate.runtime_hashes(output/'runtime').items() if original.get(n)!=h]))


def reconcile_costs(costs, outcomes):
    expected = {(r['test_set'],r['test']):r for r in outcomes}
    seen = set(); previous = None
    for index, row in enumerate(costs, 1):
        key = (row['testSet'],row['test'])
        if key not in expected or key in seen or row['ordinal'] != index:
            raise ValueError('Unknown, duplicate or reordered cost identity')
        seen.add(key)
        if {'Pass':'Passed','Fail':'Failed','Skip':'Skipped'}.get(row['result']) != expected[key]['status']:
            raise ValueError('Diagnostic case outcome differs')
        for k in ['wallSeconds','cpuSeconds']:
            if row[k] is None or not math.isfinite(row[k]) or row[k] < 0:
                raise ValueError('Missing or invalid timing counter')
        if row['frequency'] <= 0 or row['endWall'] < row['startWall'] or row['endCpuTicks'] < row['startCpuTicks']:
            raise ValueError('Counter reversal')
        if abs(row['wallSeconds']-(row['endWall']-row['startWall'])/row['frequency']) > 1e-9 or abs(row['cpuSeconds']-(row['endCpuTicks']-row['startCpuTicks'])/10000000) > 1e-9:
            raise ValueError('Counter conversion differs')
        if previous and (row['startWall'] < previous['endWall'] or row['startCpuTicks'] < previous['endCpuTicks']):
            raise ValueError('Overlapping or reversed case windows')
        previous = row
    unmeasured = [r for key,r in expected.items() if key not in seen]
    if any(r['status'] != 'Skipped' or r['reason_source'] != 'RunnerSetSkip' for r in unmeasured):
        raise ValueError('Missing executable case timing')
    return unmeasured


def inspect(packet):
    gate = load_gate(); policy, _, inputs, skips = gate.baseline()
    run = json.loads((packet / 'RUN.json').read_text()); plan = json.loads((packet / 'PLAN.json').read_text())
    rows = gate.reconcile(plan, run, (packet/'stdout.log').read_text(encoding='utf-8-sig'), skips, policy)
    costs = [json.loads(line) for line in (packet/'work/case-costs.jsonl').read_text().splitlines()]
    unmeasured = reconcile_costs(costs, rows)
    end = json.loads((packet/'work/case-cost-end.json').read_text())
    if end['rows'] != len(costs) or end['errors']:
        raise ValueError('Incomplete observer termination or counter errors')
    if gate.inventory(packet/'corpus') != inputs or gate.runtime_hashes(packet/'runtime') != json.loads((packet/'RUNTIME-HASHES.json').read_text()):
        raise ValueError('Runtime or corpus drift')
    verify_pin(json.loads((packet/'CASE-COST-PIN.json').read_text()))
    with (packet/'unmeasured-skips.csv').open('w',newline='',encoding='utf-8') as stream:
        writer = csv.DictWriter(stream,fieldnames=list(rows[0]),lineterminator='\n'); writer.writeheader(); writer.writerows(unmeasured)
    totals = collections.Counter()
    for row in costs: totals[row['testSet']] += row['wallSeconds']
    report = dict(diagnosticOnly=True,qualification=False,counts=run['counts'],totalCases=len(rows),timedCases=len(costs),
        unmeasuredSetSkips=len(unmeasured),elapsedSeconds=run['elapsedSeconds'],sumCaseWallSeconds=sum(r['wallSeconds'] for r in costs),
        sumCaseCpuSeconds=sum(r['cpuSeconds'] for r in costs),topCases=sorted(costs,key=lambda r:r['wallSeconds'],reverse=True)[:25],
        topSetWallSeconds=totals.most_common(20),originalCaseOutcomesAndSkipReasonsUnchanged=True,
        limits='Whole-process CPU across threads; no exclusive case CPU, exact overhead estimate, phase or causal attribution. Short CPU deltas may be zero.')
    write(packet/'REPORT.json',report)
    print(json.dumps({k:v for k,v in report.items() if k not in ['topCases','topSetWallSeconds']},indent=2))


def controls(packet):
    pin = json.loads((packet/'CASE-COST-PIN.json').read_text()); verify_pin(pin)
    identity = json.loads((packet/'IDENTITY.json').read_text())
    helper = packet/'diagnostic/CaseCosts.cs'
    if digest(helper) != identity['diagnosticSources']['CaseCosts.cs']:
        raise ValueError('Control helper source drift')
    folder = packet/'controls'; folder.mkdir(exist_ok=False)
    shutil.copyfile(helper,folder/'CaseCosts.cs')
    source = '''// Generated contained diagnostic controls; no unhandled negative cases.
using System.Text.Json;
internal static class Control
{
    static int Main(string[] args)
    {
        try
        {
            if (!CaseCosts.Initialize(args[0])) return 1;
            var first = CaseCosts.Begin();
            double checksum = 0; for (int i=1;i<100000;i++) checksum += Math.Sqrt(i);
            CaseCosts.End(first,"control","positive","Pass");
            var second = CaseCosts.Begin();
            CaseCosts.End(second,"control","fast-skip","Skip");
            CaseCosts.Close();
            if (checksum <= 0 || CaseCosts.Initialize(args[1])) return 1;
            Environment.ExitCode = 0;
            Console.WriteLine("Positive timing, fast skip and caught native-version refusal passed.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
'''
    (folder/'Control.cs').write_bytes(source.encode())
    (folder/'Control.csproj').write_bytes(b'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup></Project>')
    with (folder/'BUILD.log').open('xb') as log:
        subprocess.run(['dotnet','build',str(folder/'Control.csproj'),'-c','Release'],stdout=log,stderr=subprocess.STDOUT,check=True)
    wrong = dict(pin,frameworkVersion='0.0.0');write(folder/'WRONG-PIN.json',wrong)
    work = folder/'work'; work.mkdir()
    assembly = folder/'bin/Release/net10.0/Control.dll'
    command = [pin['nativeFiles']['dotnet.exe']['path'],'exec','--fx-version','10.0.11','--roll-forward','Disable',str(assembly),str(packet/'CASE-COST-PIN.json'),str(folder/'WRONG-PIN.json')]
    spec = importlib.util.spec_from_file_location('bounded',ROOT/'docs/xslt30-qualification/run.py')
    bounded = importlib.util.module_from_spec(spec);spec.loader.exec_module(bounded)
    process = None; job = bounded.BoundedJob(8589934592)
    try:
        with (folder/'stdout.log').open('xb') as out,(folder/'stderr.log').open('xb') as err:
            with bounded.process_error_mode():
                process = subprocess.Popen(command,cwd=work,stdout=out,stderr=err,creationflags=subprocess.CREATE_NO_WINDOW)
            job.assign(process)
            process.wait(timeout=30)
    finally:
        job.close()
        if process and process.poll() is None:process.wait(timeout=15)
    if process.returncode != 0:
        raise ValueError('Contained helper controls failed; inspect retained logs')
    rows = [json.loads(s) for s in (work/'case-costs.jsonl').read_text().splitlines()]
    expected = [dict(test_set='control',test=name,status=status,reason_source='RunnerCaseLine') for name,status in [('positive','Passed'),('fast-skip','Skipped')]]
    if len(rows)!=2 or reconcile_costs(rows,expected):raise ValueError('Control timing ledger differs')
    end = json.loads((work/'case-cost-end.json').read_text());refusal=json.loads((work/'case-cost-refusal.json').read_text())
    if end['errors'] or end['rows']!=2 or 'differs from the diagnostic pin' not in refusal['error']:
        raise ValueError('Caught-refusal control receipt differs')
    write(folder/'RESULTS.json',dict(success=True,controls=3,command=command,assemblySha256=digest(assembly),helperSha256=digest(helper),caughtRefusal=True,unhandledNegativeLaunched=False))
    print('Three bounded helper controls pass; wrong-version refusal caught inside diagnostic.')


def main():
    parser = argparse.ArgumentParser(description=__doc__); sub = parser.add_subparsers(dest='mode',required=True)
    prep = sub.add_parser('prepare'); prep.add_argument('--qualified',type=Path,required=True); prep.add_argument('--corpus',type=Path,required=True); prep.add_argument('--output',type=Path,required=True); prep.add_argument('--native',type=Path,required=True)
    for name in ['run','inspect','controls']:
        child = sub.add_parser(name); child.add_argument('--packet',type=Path,required=True)
    args = parser.parse_args()
    if args.mode == 'prepare':
        prepare(args.qualified.resolve(),args.corpus.resolve(),args.output.resolve(),json.loads(args.native.read_text()))
    elif args.mode == 'inspect': inspect(args.packet.resolve())
    elif args.mode == 'controls': controls(args.packet.resolve())
    else:
        packet = args.packet.resolve(); identity = json.loads((packet/'IDENTITY.json').read_text())
        if (packet/'RUN.json').exists() or digest(packet/'diagnostic-run.py') != identity['controllerSha256']:
            raise ValueError('Existing attempt or changed bounded controller')
        verify_pin(json.loads((packet/'CASE-COST-PIN.json').read_text()))
        subprocess.run([sys.executable,str(packet/'diagnostic-run.py'),'--packet',str(packet)],check=True)
        inspect(packet)


if __name__ == '__main__':
    try: main()
    except Exception as error:
        print('Case-cost diagnostic refused: '+repr(error),file=sys.stderr); raise SystemExit(2)

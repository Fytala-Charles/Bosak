"""Bounded read-only extraction from the two complete, hash-pinned diagnostic traces."""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import shutil
import subprocess
import xml.etree.ElementTree as ET

ROOT=Path(__file__).resolve().parents[1]
DOSSIER=ROOT/'docs/xslt30-gc-correlation'


def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()


def extract(trace,profiler,output):
    policy=json.loads((DOSSIER/'BASELINE.json').read_text())
    if output.exists():raise ValueError('Existing reader attempt; never overwrite receipts')
    if sha(trace) not in {r['sha256'] for r in policy['allowedTraces'].values()}:raise ValueError('Unreviewed, partial or changed input trace')
    expected=json.loads((DOSSIER/'PROFILER-FILES.json').read_text())
    if any(sha(profiler/n)!=value for n,value in expected.items()):raise ValueError('Pinned analysis library closure changed')
    host=Path(policy['dotnet']['path'])
    if sha(host)!=policy['dotnet']['sha256']:raise ValueError('Reviewed dotnet host changed')
    output.mkdir(parents=True);build=output/'reader';build.mkdir()
    shutil.copyfile(trace,output/'raw.nettrace');shutil.copyfile(ROOT/'tools/xslt30-gc/GcEvents.cs.in',build/'GcEvents.cs')
    project=ET.Element('Project',Sdk='Microsoft.NET.Sdk');props=ET.SubElement(project,'PropertyGroup')
    for key,value in [('OutputType','Exe'),('TargetFramework','net8.0'),('ImplicitUsings','enable'),('Nullable','enable'),('GenerateDocumentationFile','true')]:ET.SubElement(props,key).text=value
    refs=ET.SubElement(project,'ItemGroup')
    for library in profiler.glob('*.dll'):
        if library.name=='dotnet-trace.dll':continue
        item=ET.SubElement(refs,'Reference',Include=library.stem);ET.SubElement(item,'HintPath').text=str(library)
    ET.indent(project);ET.ElementTree(project).write(build/'GcEvents.csproj',encoding='unicode')
    with (output/'BUILD.log').open('xb') as log:subprocess.run(['dotnet','build',str(build/'GcEvents.csproj'),'-c','Release'],stdout=log,stderr=subprocess.STDOUT,check=True)
    command=[str(host),'exec','--fx-version','8.0.28','--roll-forward','Disable',str(build/'bin/Release/net8.0/GcEvents.dll'),str(output/'raw.nettrace'),str(output/'gc-events.jsonl'),str(output/'gc-meta.json')]
    spec=importlib.util.spec_from_file_location('bounded',ROOT/'docs/xslt30-qualification/run.py');bounded=importlib.util.module_from_spec(spec);spec.loader.exec_module(bounded)
    process=None;job=bounded.BoundedJob(8589934592)
    try:
        with (output/'stdout.log').open('xb') as out,(output/'stderr.log').open('xb') as err:
            with bounded.process_error_mode():process=subprocess.Popen(command,stdout=out,stderr=err,creationflags=subprocess.CREATE_NO_WINDOW)
            job.assign(process);process.wait(timeout=120)
    finally:
        job.close()
        if process and process.poll() is None:process.wait(timeout=15)
    record=dict(exitCode=process.returncode,command=command,sourceSha256=sha(build/'GcEvents.cs'),readerSha256=sha(build/'bin/Release/net8.0/GcEvents.dll'),traceSha256=sha(trace),
        afterTraceUnchanged=sha(trace)==sha(output/'raw.nettrace'),workloadExecutions=0,watchdogSeconds=120,memoryLimitBytes=8589934592)
    (output/'READER.json').write_bytes((json.dumps(record,indent=2)+'\n').encode())
    if process.returncode!=0:raise ValueError('Read-only analysis refused; caught errors and original receipts retained')
    if any(sha(profiler/n)!=value for n,value in expected.items()):raise ValueError('Analysis libraries changed')


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    for name in ['trace','profiler','output']:parser.add_argument('--'+name,type=Path,required=True)
    args=parser.parse_args()
    try:extract(args.trace.resolve(),args.profiler.resolve(),args.output.resolve())
    except Exception as error:parser.exit(2,'GC extraction refused: '+repr(error)+'\n')

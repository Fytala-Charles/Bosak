"""Replay the retained candidate feed with a fresh private cache and package-only caller."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import xml.etree.ElementTree as ET
import zipfile

HERE = Path(__file__).resolve().parents[1]
ROOT = HERE.parents[1]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--workspace', default=str(ROOT / '.guard-tmp/foundation-package-replay-2026-10-30'))
    args = parser.parse_args()
    work = Path(args.workspace).resolve()
    assert work.is_relative_to((ROOT / '.guard-tmp').resolve())
    assert not work.exists() or not any(work.iterdir()), 'Use a fresh replay workspace'
    work.mkdir(parents=True, exist_ok=True)
    scope = json.loads((HERE / 'SCOPE.json').read_bytes())
    feed = work / 'feed'; feed.mkdir(); cache = work / 'cache'; cache.mkdir()
    with zipfile.ZipFile(HERE / 'evidence/candidate-feed.zip') as z:
        assert all('/' not in n and '\\' not in n and n.endswith('.nupkg') for n in z.namelist())
        z.extractall(feed)
    config = ET.Element('configuration'); sources = ET.SubElement(config,'packageSources'); ET.SubElement(sources,'clear'); ET.SubElement(sources,'add',key='candidate-local',value=str(feed))
    ET.SubElement(ET.SubElement(config,'fallbackPackageFolders'),'clear')
    ET.SubElement(ET.SubElement(config,'config'),'add',key='globalPackagesFolder',value=str(cache))
    config_path = work / 'NuGet.Config'; ET.ElementTree(config).write(config_path,encoding='utf-8',xml_declaration=True)
    project = work / 'PackageCaller.csproj'; shutil.copyfile(HERE / 'tools/PackageCaller.csproj',project)
    caller = work / 'Caller.source.txt'; caller.write_bytes(subprocess.check_output(['git','show',scope['callerRevision']+':docs/foundation-legacy-compatibility/harness/Program.cs'],cwd=ROOT))
    env = os.environ.copy(); env['NUGET_PACKAGES'] = str(cache)
    properties = ['-p:RetainedCallerSource='+str(caller),'-p:NuGetAudit=false']
    commands = [['dotnet','restore',str(project),'--configfile',str(config_path),'--packages',str(cache),'--force','--no-cache',*properties],
                ['dotnet','build',str(project),'-c','Release','--no-restore','--nologo','-o',str(work/'output'),*properties],
                ['dotnet',str(work/'output/LegacyCaller.dll'),str(work/'run.json')]]
    for i,argv in enumerate(commands):
        with (work/(str(i)+'.txt')).open('wb') as f:
            subprocess.run(argv,cwd=ROOT,env=env,stdout=f,stderr=subprocess.STDOUT,check=True)
    run = json.loads((work/'run.json').read_bytes()); primary=json.loads((HERE/'evidence/package-source-run.json').read_bytes())
    assert run['cases']==primary['cases'] and run['passed']==20 and run['failed']==0
    assert run['loadedAssemblies']==primary['loadedAssemblies']
    (work/'replay.json').write_bytes((json.dumps({'passed':20,'failed':0,'sameObservationsAndLoadedHashes':True,'freshPrivateCache':True,'feedSha256':hashlib.sha256((HERE/'evidence/candidate-feed.zip').read_bytes()).hexdigest()},indent=2)+'\n').encode())
    print('Retained feed replay passed all 20 cases with identical package-loaded hashes.')


if __name__=='__main__':
    main()

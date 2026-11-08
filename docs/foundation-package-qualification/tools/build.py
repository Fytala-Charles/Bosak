"""Build a local seven-package candidate and run only package-restored caller dependencies."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import tarfile
import xml.etree.ElementTree as ET
import zipfile

HERE = Path(__file__).resolve().parents[1]
ROOT = HERE.parents[1]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--workspace', default=str(ROOT / '.guard-tmp/foundation-package-qualification-2026-10-30'))
    args = parser.parse_args()
    work = Path(args.workspace).resolve()
    assert work.is_relative_to((ROOT / '.guard-tmp').resolve()), 'Workspace must be inside the ignored task area'
    assert not work.exists() or not any(work.iterdir()), 'Use a fresh workspace; no cleanup or reuse is implicit'
    work.mkdir(parents=True, exist_ok=True)
    scope = json.loads((HERE / 'SCOPE.json').read_bytes())
    feed = work / 'feed'; feed.mkdir()
    cache = work / 'cache'; cache.mkdir()
    env = os.environ.copy(); env['NUGET_PACKAGES'] = str(cache); env['DOTNET_CLI_TELEMETRY_OPTOUT'] = '1'
    commands = []

    def run(name, argv):
        commands.append({'name': name, 'argv': [str(a) for a in argv]})
        print(name, flush=True)
        with (work / (name + '.txt')).open('wb') as output:
            subprocess.run([str(a) for a in argv], cwd=ROOT, env=env, stdout=output, stderr=subprocess.STDOUT, check=True)

    archive = work / 'source.tar'
    run('archive', ['git', 'archive', '--format=tar', '--output=' + str(archive), scope['sourceRevision'], 'src', 'README.md', 'license.md', 'NOTICE', 'assets'])
    source = work / 'source'; source.mkdir()
    with tarfile.open(archive) as t:
        t.extractall(source, filter='data')
    config = ET.Element('configuration')
    feeds = ET.SubElement(config, 'packageSources'); ET.SubElement(feeds, 'clear'); ET.SubElement(feeds, 'add', key='candidate-local', value=str(feed))
    fallback = ET.SubElement(config, 'fallbackPackageFolders'); ET.SubElement(fallback, 'clear')
    values = ET.SubElement(config, 'config'); ET.SubElement(values, 'add', key='globalPackagesFolder', value=str(cache))
    config_path = work / 'NuGet.Config'; ET.ElementTree(config).write(config_path, encoding='utf-8', xml_declaration=True)
    api = source / 'src/Bosak.XPath.Api/Bosak.XPath.Api.csproj'
    properties = ['-p:SourceRevisionId=' + scope['sourceRevision'], '-p:RepositoryCommit=' + scope['sourceRevision'], '-p:NuGetAudit=false']
    run('source-restore', ['dotnet', 'restore', api, '--configfile', config_path, '--packages', cache, *properties])
    run('source-build', ['dotnet', 'build', api, '-c', 'Release', '--no-restore', '--nologo', *properties])
    for name in scope['packages']:
        run('pack-' + name, ['dotnet', 'pack', source / 'src' / name / (name + '.csproj'), '-c', 'Release', '--no-build', '--no-restore', '--nologo', '-o', feed,
                             '-p:CustomBeforeMicrosoftCommonTargets=' + str(HERE / 'tools/CandidateAssets.targets'), '-p:CandidateSourceRoot=' + str(source), *properties])
    consumer = work / 'consumer'; consumer.mkdir()
    shutil.copyfile(HERE / 'tools/PackageCaller.csproj', consumer / 'PackageCaller.csproj')
    caller = consumer / 'RetainedCaller.source.txt'
    caller.write_bytes(subprocess.check_output(['git', 'show', scope['callerRevision'] + ':docs/foundation-legacy-compatibility/harness/Program.cs'], cwd=ROOT))
    caller_properties = ['-p:RetainedCallerSource=' + str(caller), '-p:NuGetAudit=false']
    run('consumer-restore', ['dotnet', 'restore', consumer / 'PackageCaller.csproj', '--configfile', config_path, '--packages', cache, '--force', '--no-cache', *caller_properties])
    output = work / 'consumer-output'
    run('consumer-build', ['dotnet', 'build', consumer / 'PackageCaller.csproj', '-c', 'Release', '--no-restore', '--nologo', '-o', output, *caller_properties])
    run('package-source-run', ['dotnet', output / 'LegacyCaller.dll', work / 'package-source-run.json'])
    replacement = work / 'unchanged-caller'; replacement.mkdir()
    with zipfile.ZipFile(ROOT / 'docs/foundation-legacy-compatibility/evidence/baseline-runtime.zip') as z:
        for name in ['LegacyCaller.dll', 'LegacyCaller.deps.json', 'LegacyCaller.runtimeconfig.json']:
            (replacement / name).write_bytes(z.read(name))
    for name in scope['packages']:
        shutil.copyfile(cache / name.lower() / scope['version'] / 'lib/net10.0' / (name + '.dll'), replacement / (name + '.dll'))
    run('unchanged-binary-run', ['dotnet', replacement / 'LegacyCaller.dll', work / 'unchanged-binary-run.json'])
    (work / 'execution.json').write_bytes((json.dumps({'scope': scope, 'commands': commands, 'sourceArchiveSha256': hashlib.sha256(archive.read_bytes()).hexdigest(), 'callerSourceSha256': hashlib.sha256(caller.read_bytes()).hexdigest()}, indent=2) + '\n').encode())
    print('Completed local build, pack, isolated package restore and both caller modes.', flush=True)


if __name__ == '__main__':
    main()

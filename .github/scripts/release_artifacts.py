"""Validate, seal and recheck the exact release artifacts; never publish them."""
from pathlib import Path, PurePosixPath
import argparse, hashlib, json, re, subprocess, xml.etree.ElementTree as ET, zipfile

ROOT = Path(__file__).resolve().parents[2]
MANIFEST = 'release-manifest.json'

def require(condition, message):
    if not condition:
        raise ValueError(message)

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def config():
    result = json.loads((ROOT / '.github/release-config.json').read_text())
    names = result['packages']
    require(len(names) == len(set(names)) == 10, 'Expected ten unique library packages')
    require(all(re.fullmatch(r'[A-Za-z][A-Za-z0-9.]+', x) for x in names), 'Invalid package ID')
    return result

def versions():
    core = ET.parse(ROOT / 'src/Directory.Build.props').findtext('.//Version')
    extension = json.loads((ROOT / 'vscode-bosak/package.json').read_text())
    lock = json.loads((ROOT / 'vscode-bosak/package-lock.json').read_text())
    require(re.fullmatch(r'\d+\.\d+\.\d+', core or ''), 'Expected pinned stable Core version')
    require(re.fullmatch(r'\d+\.\d+\.\d+', extension['version']), 'Expected pinned extension version')
    require(extension['version'] == lock['version'] == lock['packages']['']['version'], 'Manifest/lock version mismatch')
    require(extension['publisher'] == 'fytala' and extension['name'] == 'vscode-bosak', 'Unexpected extension identity')
    return core, extension['version']

def zip_contents(path):
    with zipfile.ZipFile(path) as archive:
        names = archive.namelist()
        require(len(names) == len(set(names)), 'Duplicate archive entry')
        require(all(not x.startswith(('/', '\\')) and '..' not in PurePosixPath(x).parts and '\\' not in x and ':' not in x for x in names), 'Unsafe archive path')
        return {x: archive.read(x) for x in names if not x.endswith('/')}

def validate(kind, folder, core, extension, source, names):
    require(re.fullmatch(r'[0-9a-f]{40}', source) is not None, 'Invalid source commit')
    expected = {x + '.' + core + '.nupkg' for x in names} if kind == 'nuget' else {'vscode-bosak-' + extension + '.vsix'}
    require(folder.is_dir(), 'Artifact directory missing')
    require(all(x.is_file() and not x.is_symlink() for x in folder.iterdir()), 'Only regular artifact files are permitted')
    actual = {x.name for x in folder.iterdir() if x.name != MANIFEST}
    require(actual == expected, 'Artifact set differs from the selected release')
    artifacts = []
    for filename in sorted(expected):
        path = folder / filename
        files = zip_contents(path)
        if kind == 'nuget':
            package = filename.removesuffix('.' + core + '.nupkg')
            root = ET.fromstring(files[package + '.nuspec'])
            elements = lambda suffix: [e for e in root.iter() if e.tag.split('}')[-1] == suffix]
            require(elements('id')[0].text == package and elements('version')[0].text == core, 'NuGet identity mismatch')
            require(elements('repository')[0].attrib.get('commit') == source, 'NuGet source commit mismatch')
            project = ET.parse(ROOT / 'src' / package / (package + '.csproj'))
            wanted = {Path(e.attrib['Include'].replace('\\', '/')).stem for e in project.findall('.//ProjectReference')}
            dependencies = {e.attrib['id']: e.attrib['version'] for e in elements('dependency')}
            require(wanted <= set(names) and set(dependencies) == wanted and all(x == core for x in dependencies.values()), 'NuGet dependency cohort mismatch')
            require([x for x in files if x.endswith('.dll')] == ['lib/net10.0/' + package + '.dll'], 'NuGet contains an unexpected assembly')
            require(not any(x.endswith('.exe') for x in files), 'NuGet contains an unexpected executable')
            required = ['NOTICE', 'license.md', 'README.md', 'icon.png', 'lib/net10.0/' + package + '.dll', 'lib/net10.0/' + package + '.xml']
            if package == 'Bosak.XPath.Standard':
                required.append('licenses/Unicode17-LICENSE.txt')
            require(all(x in files for x in required), 'NuGet required asset missing')
            require(files['NOTICE'] == (ROOT / 'NOTICE').read_bytes(), 'NOTICE differs from the source commit')
            require(files['license.md'] == (ROOT / 'license.md').read_bytes(), 'License differs from the source commit')
            if package == 'Bosak.XPath.Standard':
                require(files['licenses/Unicode17-LICENSE.txt'] == (ROOT / 'src/Bosak.XPath.Standard/Resources/Unicode17-LICENSE.txt').read_bytes(), 'Unicode license differs from source')
        else:
            package = json.loads(files['extension/package.json'])
            require(package['publisher'] == 'fytala' and package['name'] == 'vscode-bosak' and package['version'] == extension, 'VSIX identity mismatch')
            require(all(x in files for x in ['extension/NOTICE', 'extension/LICENSE.md', 'extension/licenses/Unicode17-LICENSE.txt', 'extension/out/extension.js', 'extension/server/Bosak.LanguageServer.dll']), 'VSIX required asset missing')
            require(files['extension/NOTICE'] == (ROOT / 'NOTICE').read_bytes(), 'VSIX NOTICE differs from source')
            require(files['extension/LICENSE.md'] == (ROOT / 'vscode-bosak/LICENSE.md').read_bytes(), 'VSIX license differs from source')
            require(files['extension/licenses/Unicode17-LICENSE.txt'] == (ROOT / 'src/Bosak.XPath.Standard/Resources/Unicode17-LICENSE.txt').read_bytes(), 'VSIX Unicode license differs from source')
            deps = json.loads(files['extension/server/Bosak.LanguageServer.deps.json'])
            projects = {x for x in deps['libraries'] if x.startswith('Bosak.')}
            bundled = set(names) - {'Bosak.XPath.Providers.Database'} | {'Bosak.LanguageServer'}
            require(projects == {x + '/' + core for x in bundled}, 'VSIX server dependency cohort mismatch')
            actual_bosak = {PurePosixPath(x).name.removesuffix('.dll') for x in files if x.startswith('extension/server/') and PurePosixPath(x).name.startswith('Bosak.') and x.endswith('.dll')}
            require(actual_bosak == bundled, 'VSIX contains missing or unexpected Bosak assemblies')
            require(not any(x.endswith(('.vsix', '.cs', '.ts', '.exe')) or x.startswith(('extension/src/', 'extension/test/')) for x in files), 'VSIX contains excluded source/test/artifact files')
        artifacts.append({'file': filename, 'sha256': sha(path), 'bytes': path.stat().st_size})
    return {'formatVersion': 1, 'kind': kind, 'sourceCommit': source, 'coreVersion': core, 'extensionVersion': extension, 'artifacts': artifacts}

def verify(kind, folder, expected_hash, core, extension, source, names):
    require(re.fullmatch(r'[0-9a-fA-F]{64}', expected_hash or '') is not None, 'Provide the reviewed manifest SHA-256')
    manifest = folder / MANIFEST
    require(manifest.is_file() and not manifest.is_symlink(), 'Manifest missing or symlinked')
    require(sha(manifest) == expected_hash.lower(), 'Reviewed manifest hash mismatch')
    recorded = json.loads(manifest.read_text())
    require(recorded == validate(kind, folder, core, extension, source, names), 'Artifact bytes/metadata differ from the reviewed manifest')
    return recorded

def check_gates(folder):
    for name, rule in config()['gates'].items():
        text = (folder / (name + '.txt')).read_text(encoding='utf-8-sig')
        counts = []
        for label in ['Passed', 'Failed', 'Skipped']:
            match = re.search('^' + label + r':\s+(\d+)', text, re.M)
            require(match is not None, 'Conformance summary missing')
            counts.append(int(match.group(1)))
        require(counts == rule['counts'], 'Frozen conformance totals differ')
        require((folder / (name + '-skips.txt')).read_text(encoding='utf-8-sig').splitlines() == (ROOT / rule['skipMap']).read_text(encoding='utf-8-sig').splitlines(), 'Frozen skip names/reasons differ')
        if 'experimentalPassed' in rule:
            match = re.search(r'^Experimental-level \(XPath40Experimental\) passes:\s+(\d+)\s*$', text, re.M)
            require(match is not None and int(match.group(1)) == rule['experimentalPassed'], 'Experimental count differs')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('command', choices=['version', 'projects', 'check-tag', 'check-gates', 'seal', 'verify'])
    parser.add_argument('kind', nargs='?', choices=['core', 'vscode', 'nuget'])
    parser.add_argument('directory', nargs='?', type=Path)
    parser.add_argument('--expected-sha')
    parser.add_argument('--ref-type', default='branch')
    parser.add_argument('--ref-name', default='')
    args = parser.parse_args()
    cfg = config(); core, extension = versions()
    if args.command == 'version':
        require(args.kind in ['core', 'vscode'], 'Choose core or vscode')
        print(core if args.kind == 'core' else extension); return
    if args.command == 'projects':
        for name in cfg['packages']: print('src/' + name + '/' + name + '.csproj')
        return
    require(args.kind in ['nuget', 'vscode'], 'Choose nuget or vscode')
    if args.command == 'check-tag':
        tracked = subprocess.check_output(['git', 'ls-files', '--', 'vscode-bosak/server', 'vscode-bosak/out', 'vscode-bosak/node_modules'], cwd=ROOT, text=True).strip()
        require(not tracked, 'Generated extension outputs must not be tracked')
        expected = 'v' + core if args.kind == 'nuget' else 'vscode-v' + extension
        require(args.ref_type != 'tag' or args.ref_name == expected, 'Tag does not match the pinned version')
        return
    require(args.directory is not None, 'Artifact directory required')
    if args.command == 'check-gates':
        check_gates(args.directory); return
    top = subprocess.check_output(['git', 'rev-parse', '--show-toplevel'], cwd=ROOT, text=True).strip()
    require(Path(top).resolve() == ROOT, 'Release source must be a Git checkout root, not an untracked nested source directory')
    source = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=ROOT, text=True).strip()
    require(subprocess.run(['git', 'diff', '--quiet', '--ignore-submodules=all', 'HEAD'], cwd=ROOT).returncode == 0, 'Tracked release inputs differ from the checked-out commit')
    require(not subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard'], cwd=ROOT, text=True).strip(), 'Untracked source inputs are present; stage generated outputs outside the checkout or ignore them explicitly')
    if args.command == 'seal':
        manifest = args.directory / MANIFEST
        require(not manifest.exists(), 'Preserve the previous manifest; use a fresh artifact directory')
        data = validate(args.kind, args.directory, core, extension, source, cfg['packages'])
        manifest.write_bytes((json.dumps(data, indent=2, sort_keys=True) + '\n').encode('utf-8'))
        print(sha(manifest))
    else:
        verify(args.kind, args.directory, args.expected_sha, core, extension, source, cfg['packages'])
        print('Verified reviewed artifact bytes, source commit, versions, dependency closure and legal assets.')

if __name__ == '__main__':
    try:
        main()
    except (ValueError, KeyError, OSError, zipfile.BadZipFile, ET.ParseError) as error:
        raise SystemExit(str(error))

"""Control tests for artifact review, version and provenance refusal boundaries."""
from pathlib import Path
import importlib.util, json, tempfile, unittest, zipfile

HERE = Path(__file__).resolve().parent
spec = importlib.util.spec_from_file_location('release_artifacts', HERE / 'release_artifacts.py')
m = importlib.util.module_from_spec(spec); spec.loader.exec_module(m)

class ArtifactReviewTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(); self.root = Path(self.temp.name); self.original = m.ROOT
        self.names = m.config()['packages']; self.source = 'a' * 40; self.core, self.extension = m.versions()
        # Keep real project dependency declarations while using synthetic archive payloads.
        for name in self.names:
            target = self.root / 'src' / name / (name + '.csproj'); target.parent.mkdir(parents=True)
            target.write_bytes((m.ROOT / 'src' / name / (name + '.csproj')).read_bytes())
        for name in ['NOTICE', 'license.md']: (self.root / name).write_bytes((m.ROOT / name).read_bytes())
        for name in ['src/Bosak.XPath.Standard/Resources/Unicode17-LICENSE.txt', 'vscode-bosak/LICENSE.md']:
            target = self.root / name; target.parent.mkdir(parents=True, exist_ok=True); target.write_bytes((m.ROOT / name).read_bytes())
        m.ROOT = self.root; self.folder = self.root / 'artifacts'; self.folder.mkdir()
    def tearDown(self):
        m.ROOT = self.original; self.temp.cleanup()
    def nuget(self, omit_notice=False):
        for name in self.names:
            project = m.ET.parse(self.root / 'src' / name / (name + '.csproj'))
            deps = [Path(x.attrib['Include'].replace('\\', '/')).stem for x in project.findall('.//ProjectReference')]
            metadata = '<package><metadata><id>' + name + '</id><version>' + self.core + '</version><repository commit="' + self.source + '"/><dependencies>' + ''.join('<dependency id="' + x + '" version="' + self.core + '"/>' for x in deps) + '</dependencies></metadata></package>'
            with zipfile.ZipFile(self.folder / (name + '.' + self.core + '.nupkg'), 'w') as z:
                z.writestr(name + '.nuspec', metadata)
                for asset in ['license.md', 'README.md', 'icon.png', 'lib/net10.0/' + name + '.dll', 'lib/net10.0/' + name + '.xml']:
                    z.writestr(asset, (self.root / 'license.md').read_bytes() if asset == 'license.md' else b'control fixture')
                if not omit_notice: z.writestr('NOTICE', (self.root / 'NOTICE').read_bytes())
                if name == 'Bosak.XPath.Standard': z.writestr('licenses/Unicode17-LICENSE.txt', (self.root / 'src/Bosak.XPath.Standard/Resources/Unicode17-LICENSE.txt').read_bytes())
    def vsix(self, publisher='fytala'):
        bundled = set(self.names) - {'Bosak.XPath.Providers.Database'} | {'Bosak.LanguageServer'}
        with zipfile.ZipFile(self.folder / ('vscode-bosak-' + self.extension + '.vsix'), 'w') as z:
            z.writestr('extension/package.json', json.dumps({'publisher': publisher, 'name': 'vscode-bosak', 'version': self.extension}))
            z.writestr('extension/server/Bosak.LanguageServer.deps.json', json.dumps({'libraries': {x + '/' + self.core: {} for x in bundled}}))
            z.writestr('extension/NOTICE', (self.root / 'NOTICE').read_bytes())
            z.writestr('extension/LICENSE.md', (self.root / 'vscode-bosak/LICENSE.md').read_bytes())
            z.writestr('extension/licenses/Unicode17-LICENSE.txt', (self.root / 'src/Bosak.XPath.Standard/Resources/Unicode17-LICENSE.txt').read_bytes())
            for name in ['extension/out/extension.js', *['extension/server/' + x + '.dll' for x in bundled]]: z.writestr(name, b'control fixture')
    def seal(self, kind):
        data = m.validate(kind, self.folder, self.core, self.extension, self.source, self.names)
        (self.folder / m.MANIFEST).write_text(json.dumps(data)); return m.sha(self.folder / m.MANIFEST)
    def test_exact_ten_package_set_verifies(self):
        self.nuget(); digest = self.seal('nuget')
        self.assertEqual(len(m.verify('nuget', self.folder, digest, self.core, self.extension, self.source, self.names)['artifacts']), 10)
    def test_changed_package_is_refused(self):
        self.nuget(); digest = self.seal('nuget'); next(self.folder.glob('*.nupkg')).write_bytes(b'changed')
        with self.assertRaises((ValueError, zipfile.BadZipFile)): m.verify('nuget', self.folder, digest, self.core, self.extension, self.source, self.names)
    def test_extra_nuget_assembly_is_refused(self):
        self.nuget()
        with zipfile.ZipFile(next(self.folder.glob('*.nupkg')), 'a') as z: z.writestr('lib/net10.0/Other.Extra.dll', b'extra')
        with self.assertRaises(ValueError): m.validate('nuget', self.folder, self.core, self.extension, self.source, self.names)
    def test_extra_bosak_vsix_assembly_is_refused(self):
        self.vsix()
        with zipfile.ZipFile(next(self.folder.glob('*.vsix')), 'a') as z: z.writestr('extension/server/Bosak.XPath.Providers.Database.dll', b'extra')
        with self.assertRaises(ValueError): m.validate('vscode', self.folder, self.core, self.extension, self.source, self.names)
    def test_wrong_review_hash_is_refused(self):
        self.nuget(); self.seal('nuget')
        with self.assertRaises(ValueError): m.verify('nuget', self.folder, 'b' * 64, self.core, self.extension, self.source, self.names)
    def test_wrong_source_commit_is_refused(self):
        self.nuget()
        with self.assertRaises(ValueError): m.validate('nuget', self.folder, self.core, self.extension, 'b' * 40, self.names)
    def test_missing_notice_is_refused(self):
        self.nuget(omit_notice=True)
        with self.assertRaises(ValueError): m.validate('nuget', self.folder, self.core, self.extension, self.source, self.names)
    def test_missing_or_extra_package_is_refused(self):
        self.nuget(); first = next(self.folder.glob('*.nupkg')); first.rename(first.with_suffix('.other'))
        with self.assertRaises(ValueError): m.validate('nuget', self.folder, self.core, self.extension, self.source, self.names)
    def test_version_mismatch_is_refused(self):
        self.nuget()
        with self.assertRaises(ValueError): m.validate('nuget', self.folder, '0.0.0', self.extension, self.source, self.names)
    def test_exact_vsix_verifies(self):
        self.vsix(); digest = self.seal('vscode')
        self.assertEqual(len(m.verify('vscode', self.folder, digest, self.core, self.extension, self.source, self.names)['artifacts']), 1)
    def test_wrong_publisher_is_refused(self):
        self.vsix(publisher='someone-else')
        with self.assertRaises(ValueError): m.validate('vscode', self.folder, self.core, self.extension, self.source, self.names)
    def test_frozen_gate_preserves_names_and_reasons_across_line_endings(self):
        directory = self.root / 'gates'; directory.mkdir()
        (self.root / '.github').mkdir()
        (self.root / '.github/release-config.json').write_text(json.dumps({'packages': self.names, 'gates': {'qt3': {'counts': [1, 0, 1], 'skipMap': 'baseline.txt'}}}))
        (self.root / 'baseline.txt').write_bytes(b'case-a: bounded profile\r\n')
        (directory / 'qt3-skips.txt').write_bytes(b'case-a: bounded profile\n')
        (directory / 'qt3.txt').write_text('Passed: 1\nFailed: 0\nSkipped: 1\n')
        m.check_gates(directory)
        (directory / 'qt3-skips.txt').write_text('case-a: different reason\n')
        with self.assertRaises(ValueError): m.check_gates(directory)
    def test_experimental_count_is_exact(self):
        directory = self.root / 'gates'; directory.mkdir()
        (self.root / '.github').mkdir()
        (self.root / '.github/release-config.json').write_text(json.dumps({'packages': self.names, 'gates': {'qt4': {'counts': [1, 0, 0], 'skipMap': 'baseline.txt', 'experimentalPassed': 11}}}))
        (self.root / 'baseline.txt').write_text('')
        (directory / 'qt4-skips.txt').write_text('')
        (directory / 'qt4.txt').write_text('Passed: 1\nFailed: 0\nSkipped: 0\nExperimental-level (XPath40Experimental) passes: 110\n')
        with self.assertRaises(ValueError): m.check_gates(directory)
    def test_duplicate_or_traversal_zip_entries_are_refused(self):
        for names in [('same', 'same'), ('../escape',)]:
            path = self.root / 'unsafe.zip'
            with zipfile.ZipFile(path, 'w') as z:
                for name in names: z.writestr(name, b'fixture')
            with self.assertRaises(ValueError): m.zip_contents(path)

if __name__ == '__main__': unittest.main()

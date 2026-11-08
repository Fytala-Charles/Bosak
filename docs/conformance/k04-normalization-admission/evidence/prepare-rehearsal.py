"""Create exact unadopted patch and isolated complete harnesses from pinned normal source."""
from pathlib import Path
import difflib
import hashlib
import json
import subprocess
import xml.etree.ElementTree as X
from xml.sax.saxutils import escape

E = Path(__file__).resolve().parent
R = E.parents[3]
P = E.parent
pin = json.loads((E / 'REVIEW-PIN.json').read_text(encoding='utf-8'))
base = pin['sourceBase']
names = list(pin['normalSourceIdentities'])
normal = {f:subprocess.check_output(['git','show',base+':'+f],cwd=R).decode('utf-8') for f in names}
proposed = dict(normal)
proposed[names[0]] = normal[names[0]].replace('exactly **31,164/0/657**', 'exactly **31,165/0/656**', 1)
line = ('        // Upstream defect: the expected value in the catalog carries an artifactual leading\n'
        '        // space (formatting), but U+09BE by itself is the only correct result.\n'
        '        ["cbcl-fn-normalize-unicode-006"] = "Upstream defect: expected value has an artifactual leading space",\n')
assert normal[names[1]].count(line) == 1
proposed[names[1]] = normal[names[1]].replace(line, '')
marker = '//                      | Charles Korthout | 1.45 '
lines = proposed[names[1]].splitlines(keepends=True)
index = next(i for i,l in enumerate(lines) if l.startswith(marker)) + 1
lines.insert(index, '//                      | Charles Korthout | 1.46  | 28-10-2026     | Remove owner-approved repaired FULLY-NORMALIZED positive skip only.                    |\n')
proposed[names[1]] = ''.join(lines)
marker = '//                      | Charles Korthout | 0.15 '
lines = proposed[names[2]].splitlines(keepends=True)
index = next(i for i,l in enumerate(lines) if l.startswith(marker)) + 1
lines.insert(index, '//                      | Charles Korthout | 0.16  | 28-10-2026     | Promote owner-approved zero-failure Unicode normalization set (230 gated sets).         |\n')
proposed[names[2]] = ''.join(lines).replace('        "fn-parse-integer",\n', '        "fn-parse-integer",\n        "fn-normalize-unicode",\n', 1)
patch = ''.join(''.join(difflib.unified_diff(normal[f].splitlines(True), proposed[f].splitlines(True), fromfile='a/'+f, tofile='b/'+f)) for f in names)
(P / 'PROPOSED-ADOPTION.patch').write_bytes(patch.encode('utf-8'))
copied = {}
for suite, project, runner in [('qt3','Bosak.XPath.Conformance',names[1]), ('qt4','Bosak.XPath.Conformance40',names[2])]:
    target = R / ('.guard-tmp/k04-normalization-harness-'+suite)
    target.mkdir(exist_ok=True)
    sources = list((R/'tests'/project).glob('*.cs'))
    if suite == 'qt4':
        sources += [R/'tests/Bosak.XPath.Conformance'/f for f in ['TestCase.cs','TestEnvironment.cs','ResultComparer.cs','TestReport.cs','TestOutcome.cs']]
    for source in sources:
        relative = source.relative_to(R).as_posix()
        content = proposed[runner].encode('utf-8') if relative == runner else source.read_bytes()
        (target/source.name).write_bytes(content)
        copied[(target/source.name).relative_to(R).as_posix()] = {'normalSource':relative,'proposed':relative==runner,'sha256':hashlib.sha256(content).hexdigest()}
    (E/(suite+'-prototype-runner.cs.txt')).write_bytes(proposed[runner].encode('utf-8'))
    directory = R/'tests'/project/'bin/Release/net10.0'
    refs = ''.join('<Reference Include="'+escape(p.stem)+'"><HintPath>'+escape(str(p))+'</HintPath></Reference>' for p in sorted(directory.glob('*.dll')) if p.stem not in ['Bosak.XPath.Conformance','Bosak.XPath.Conformance40'])
    csproj = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><AssemblyName>'+project+'</AssemblyName><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><LangVersion>preview</LangVersion></PropertyGroup><ItemGroup>'+refs+'</ItemGroup></Project>'
    (target/'Probe.csproj').write_text(csproj,encoding='utf-8')
    (E/(suite+'-prototype.csproj.txt')).write_text(csproj,encoding='utf-8')
(E/'copied-source-identities.json').write_text(json.dumps(copied,indent=2)+'\n',encoding='utf-8')
ns = {'t':'http://www.w3.org/2010/09/qt-fots-catalog'}
inventory, hashes = {}, {}
for suite in ['qt3','qt4']:
    root = R/'tests'/(suite+'tests')
    catalog = root/'catalog.xml'
    hashes[catalog.relative_to(R).as_posix()] = hashlib.sha256(catalog.read_bytes()).hexdigest()
    ref = next(n for n in X.parse(catalog).getroot().findall('t:test-set',ns) if n.get('name') == 'fn-normalize-unicode')
    path = root/ref.get('file')
    hashes[path.relative_to(R).as_posix()] = hashlib.sha256(path.read_bytes()).hexdigest()
    doc = X.parse(path).getroot()
    inventory[suite] = [{'name':case.get('name'),'expression':case.findtext('t:test',namespaces=ns),'dependencies':[dict(n.attrib) for n in doc.findall('t:dependency',ns)+case.findall('t:dependency',ns)],'assertion':X.tostring(case.find('t:result',ns),encoding='unicode')} for case in doc.findall('t:test-case',ns)]
assert len(inventory['qt3']) == 48 and len(inventory['qt4']) == 50
(E/'normalization-inventory.json').write_text(json.dumps({'cases':inventory,'fixtureHashes':hashes},indent=2)+'\n',encoding='utf-8')
print('Exact three-file proposal and two isolated complete harnesses prepared; normal sources unchanged.')

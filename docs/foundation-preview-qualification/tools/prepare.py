"""Prepare isolated package-binary overlays over sealed unchanged test/harness layouts."""
from pathlib import Path
import hashlib,json,shutil,subprocess,tempfile,zipfile

P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
prior=R/'docs/foundation-captured-identity-production/evidence'
packages=read(R/'docs/foundation-preview-packages/evidence/PACKAGES.json')
baseline=read(prior/'EXECUTION-LAYOUTS.json');units=read(prior/'UNIT-RESULTS.json')['projects']
work=R/'.guard-tmp/preview-qualification-1811';assert not work.exists();work.mkdir()
fixture_root=work/'retry/tests';fixture_root.mkdir(parents=True)
quote=lambda value:"'"+str(value).replace("'","''")+"'"
subprocess.run(['powershell','-NoProfile','-Command',
    'New-Item -ItemType Junction -Path '+quote(fixture_root/'qt3tests')+' -Value '+quote(R/'tests/qt3tests')+' | Out-Null'],check=True)
E.mkdir(parents=True,exist_ok=True)
plans={};engines={r['id']:r for r in packages['packages']}
for key,base in baseline.items():
    if key=='retained-foundation':continue # Already replayed against exact package hashes in PV-Q08.
    target=Path(tempfile.mkdtemp(prefix='BosakPreview1811Xslt-')) if key=='units/bosak.xslt.tests' else work/key
    if key in ['units/bosak.xpath.runtime.tests','units/bosak.xquery.tests','units/bosak.xpath.standard.tests']:
        target=fixture_root/key.split('/')[1]/'bin/Release/net10.0'
    target.mkdir(parents=True,exist_ok=True);origin=Path(base['path']);omitted=[]
    for name,h in base['files'].items():
        assert sha(origin/name)==h,(key,name)
        if Path(name).name in {n+'.pdb' for n in engines}:omitted.append(name);continue
        dest=target/name;dest.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(origin/name,dest)
    replaced={}
    for name,package in engines.items():
        if not (target/(name+'.dll')).exists():continue
        with zipfile.ZipFile(package['path']) as z:
            for ext in ['dll','xml']:
                if ext=='xml' and not (target/(name+'.xml')).exists():continue
                (target/(name+'.'+ext)).write_bytes(z.read('lib/net10.0/'+name+'.'+ext))
        replaced[name]=sha(target/(name+'.dll'))
        assert replaced[name]==packages['loadedEngines'][name]
    files={p.relative_to(target).as_posix():sha(p) for p in sorted(target.rglob('*')) if p.is_file()}
    if key.startswith('units/'):
        row=next(x for x in units if x['project']==key.split('/')[1]);binary=target/Path(row['dll']).name;expected=row['expected']
    elif key in ['qt3','qt4']:
        binary=target/('Bosak.XPath.Conformance'+('40' if key=='qt4' else '')+'.dll');expected=None
    else:
        names={'languageServer':('Bosak.LanguageServer.Tests.dll',84),'native451':('Bosak.Schema.Tests.dll',451),
               'nativeAdapter':('Bosak.Schema.NativeContainerPrototype.Tests.dll',59),'consumer':('Bosak.Schema.PublicProviderConsumer.Tests.dll',10)}
        name,expected=names[key];binary=target/name
        if not binary.exists():
            # Resolve the sealed test assembly from its own dependency runtime configuration.
            choices=[p for p in target.glob('*.runtimeconfig.json') if p.with_name(p.name.replace('.runtimeconfig.json','.dll')).exists() and 'testhost' not in p.name.lower()]
            assert len(choices)==1,(key,[p.name for p in choices]);binary=choices[0].with_name(choices[0].name.replace('.runtimeconfig.json','.dll'))
    assert binary.exists(),binary
    plans[key]=dict(path=str(target),binary=str(binary),expected=expected,baselineKey=key,
        replacedEngines=replaced,omittedOldEngineSymbols=omitted,files=files)
plan=dict(date='2026-11-07',previewVersion=packages['previewVersion'],candidateCommit=packages['candidateCommit'],
    baselineLayoutsSha256=sha(prior/'EXECUTION-LAYOUTS.json'),packageManifestSha256=sha(R/'docs/foundation-preview-packages/MANIFEST.json'),
    work=str(work),layouts=plans,ownerAccepted=False,apiFrozen=False,publicationAuthorized=False,
    scope='Unchanged compiled test/harness/native/consumer assemblies with exact package DLL/XML overlays; nonpackaged products remain separately pinned.')
(P/'PLAN.json').write_text(json.dumps(plan,indent=2)+'\n',encoding='utf-8')
print('Prepared',len(plans),'hash-pinned isolated package overlay layouts; production evidence unchanged.')

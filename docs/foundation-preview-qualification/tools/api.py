"""Inventory all eleven products with nine package assemblies and two pinned exclusions."""
from pathlib import Path
import hashlib,json,shutil,subprocess,sys,zipfile

P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
packages=read(R/'docs/foundation-preview-packages/evidence/PACKAGES.json')
pins=read(R/'docs/foundation-captured-identity-production/evidence/SOURCE-AND-BINARY-PINS.json')
work=R/'.guard-tmp/preview-qualification-1811';root=work/'api-root';root.mkdir()
for name,row in pins['engines'].items():
    folder=root/'src'/name;binary=folder/'bin/Release/net10.0';binary.mkdir(parents=True)
    shutil.copyfile(R/'src'/name/(name+'.csproj'),folder/(name+'.csproj'))
    package=next((x for x in packages['packages'] if x['id']==name),None)
    if package:
        with zipfile.ZipFile(package['path']) as z:(binary/(name+'.dll')).write_bytes(z.read('lib/net10.0/'+name+'.dll'))
    else:shutil.copyfile(row['path'],binary/(name+'.dll'))
    if not package:
        for support in Path(row['path']).parent.glob('*.dll'):
            if not support.name.startswith('Bosak.') and not (binary/support.name).exists():shutil.copyfile(support,binary/support.name)
audit=work/'api-audit';audit.mkdir()
tool=R/'docs/foundation-xslt-source-membership/tools/Inventory.cs.txt'
shutil.copyfile(tool,audit/'Program.cs')
(audit/'Audit.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup></Project>')
def run(name,command):
    subprocess.run([sys.executable,str(R/'docs/foundation-r2-closure/tools/run.py'),'--log',
        str(R/f'docs/foundation-r2-closure/evidence/execution/preview-qualification-{name}.log'),
        '--timeout-seconds','180','--',*command],cwd=R,check=True)
run('api-build',['dotnet','build',str(audit/'Audit.csproj'),'-c','Release','-p:NuGetAudit=false','--ignore-failed-sources'])
run('api-inventory',['dotnet',str(audit/'bin/Release/net10.0/Audit.dll'),str(root),str(E/'public-api.json'),
    pins['engines']['Bosak.Xslt']['path']])
before=read(R/'docs/foundation-captured-identity-production/evidence/public-api-complete.json');after=read(E/'public-api.json')
for old,new in zip(before['assemblies'],after['assemblies'],strict=True):
    for key in ['project','assembly','identity','references','friendAssemblies','publicTypes','declarationCount','declarations']:
        assert old[key]==new[key],(old['assembly'],key)
    name=new['assembly'];expected=packages['loadedEngines'].get(name,pins['engines'][name]['sha256'])
    assert new['sha256'].lower()==expected,name
assert len(after['assemblies'])==11 and sum(r['publicTypes'] for r in after['assemblies'])==318 and sum(r['declarationCount'] for r in after['assemblies'])==6465
(E/'API-COMPARISON.json').write_text(json.dumps(dict(publicTypes=318,declarationRows=6465,products=11,
    packaged=9,unchangedNonpackaged=['Bosak.LanguageServer','Bosak.XPath.Providers.Database'],
    observedMetadataDelta=False,inventoryToolSha256=hashlib.sha256(tool.read_bytes()).hexdigest(),
    scope='Public/protected declarations, defaults, enum constants, attributes/value layouts, references/friends and identities; not behavioral or owner/API-freeze acceptance.'),indent=2)+'\n')
print('All 318 types/6465 API rows and references/friends/identities match; nine packaged products plus two separately pinned exclusions.')

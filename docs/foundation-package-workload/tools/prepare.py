"""Prepare a fresh-cache package workload and owner inputs over the qualified preview."""
from pathlib import Path
import copy,hashlib,json,shutil,xml.etree.ElementTree as ET

P=Path(__file__).resolve().parents[1];R=P.parents[1]
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
old=R/'docs/foundation-workload-1811';packages=read(R/'docs/foundation-preview-packages/evidence/PACKAGES.json')
version=packages['previewVersion'];assert version=='1.81.1-preview.c0b926de'
work=R/'.guard-tmp/package-workload-1811';assert not work.exists();work.mkdir()
cache=work/'cache';cache.mkdir();assert not list(cache.iterdir())
feed=Path(read(R/'docs/foundation-preview-packages/PLAN.json')['feed'])
config=work/'NuGet.Config';config.write_text(f'<configuration><packageSources><clear/><add key="preview" value="{feed}"/></packageSources><fallbackPackageFolders><clear/></fallbackPackageFolders></configuration>')
(P/'workload').mkdir(parents=True);(P/'evidence').mkdir();(P/'returns').mkdir()
shutil.copytree(old/'fixtures',P/'fixtures')
protocol=read(old/'PROTOCOL.json');protocol['previewVersion']=version
(P/'PROTOCOL.json').write_text(json.dumps(protocol,indent=2)+'\n',encoding='utf-8')
source=(old/'workload/Program.cs').read_text(encoding='utf-8');lines=source.splitlines()
i=next(i for i,line in enumerate(lines) if line.startswith('//                      | Charles Korthout | 0.2'))
lines.insert(i+1,'//                      | Charles Korthout | 0.3   | 07-11-2026     | Record qualified preview package cohort; timed workload body unchanged. |')
source='\n'.join(lines)+'\n';source=source.replace('sourceVersion = "1.81.1", role',f'sourceVersion = "1.81.1", previewVersion = "{version}", role')
(P/'workload/Program.cs').write_text(source,encoding='utf-8')
tree=ET.parse(old/'workload/Workload.csproj');root=tree.getroot()
for group in root.findall('ItemGroup'):
    for reference in list(group):
        if reference.tag=='Reference' and reference.attrib['Include'] in packages['loadedEngines']:group.remove(reference)
    ET.SubElement(group,'PackageReference',dict(Include='Bosak.Xslt',Version='['+version+']'))
props=root.find('PropertyGroup')
ET.SubElement(props,'RestoreFallbackFolders').text=''
ET.SubElement(props,'RestoreAdditionalProjectFallbackFolders').text=''
tree.write(P/'workload/Workload.csproj',encoding='utf-8',xml_declaration=True)
for file in ['compare.py','test_compare.py']:shutil.copyfile(old/'tools'/file,P/'tools'/file)
cohort=read(old/'COHORT.json');cohort['previewVersion']=version
cohort['scope']='Exact preview-package candidate-only measurements; fresh private package cache, unchanged native/provider DLLs and timed method. No baseline or budget acceptance.'
cohort['references']={row['id']:dict(path=str(cache/row['id'].lower()/version/'lib/net10.0'/(row['id']+'.dll')),sha256=packages['loadedEngines'][row['id']]) for row in packages['packages']}
cohort.update(work=str(work),cache=str(cache),config=str(config),feed=str(feed),
    packageManifestSha256=sha(R/'docs/foundation-preview-packages/MANIFEST.json'),
    regressionManifestSha256=sha(R/'docs/foundation-preview-qualification/MANIFEST.json'))
(P/'COHORT.json').write_text(json.dumps(cohort,indent=2)+'\n',encoding='utf-8')
pins=read(R/'docs/foundation-captured-identity-production/evidence/SOURCE-AND-BINARY-PINS.json')
pins['candidateCommit']=packages['candidateCommit'];pins['previewVersion']=version
pins['nonpackagedProducts']=['Bosak.LanguageServer','Bosak.XPath.Providers.Database']
for name,row in cohort['references'].items():pins['engines'][name].update(row)
pin_path=P/'CANDIDATE-PINS.json';pin_path.write_text(json.dumps(pins,indent=2)+'\n',encoding='utf-8')
api_path=R/'docs/foundation-preview-qualification/evidence/public-api.json'
template=read(R/'docs/foundation-provider-adoption/returns/Core.template.json')
template['candidate']=dict(sourceVersion='1.81.1',pinsSha256=sha(pin_path),apiSha256=sha(api_path),
    engines={name:row['sha256'].lower() for name,row in pins['engines'].items()})
for owner in ['Core','Schema','XmlDb','Braid']:
    row=copy.deepcopy(template);row['ownerRole']=owner
    (P/'returns'/(owner+'.template.json')).write_text(json.dumps(row,indent=2)+'\n',encoding='utf-8')
context=dict(candidatePinsSha256=sha(pin_path),apiSha256=sha(api_path),
    checkerSha256=sha(R/'docs/foundation-provider-qualification/tools/receipt_preflight.py'),
    previewVersion=version,ownerAccepted=False,budgetsAccepted=False,requirementsWaived=0)
(P/'OWNER-CONTEXT.json').write_text(json.dumps(context,indent=2)+'\n',encoding='utf-8')
print('Prepared exact package workload/fresh private cache and four unfilled owner review inputs.')

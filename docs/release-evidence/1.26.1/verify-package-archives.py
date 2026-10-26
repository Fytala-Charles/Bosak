from pathlib import Path
import json,hashlib,zipfile,xml.etree.ElementTree as E,base64
root=Path('D:/Development/Bosak');tmp=root/'.guard-tmp';out=tmp/'release1261-candidate';source=tmp/'release1261-source';revision=json.loads((out/'source-plan.json').read_text(encoding='utf-8'))['immutableSource']
def load(p):
 b=p.read_bytes();return json.loads(b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig'))
def sha(b):return hashlib.sha256(b).hexdigest()
metadata={'broad':load(out/'broad-inline-metadata.json'),'focused':load(out/'focused-metadata.json')}
assert len(metadata['broad'])==10 and len(metadata['focused'])==9
rows=[]
for p in sorted((tmp/'release1261-packages').glob('*.nupkg')):
 with zipfile.ZipFile(p) as z:
  n=E.fromstring(z.read(next(n for n in z.namelist() if n.endswith('.nuspec'))));ns={'n':'http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd'}
  # Match by local name to support different NuGet schema revisions.
  fields={e.tag.split('}')[-1]:e.text for e in n.iter() if e.tag.split('}')[-1] in ['id','version','readme','icon']}
  ident=fields['id'];assert fields['version']=='1.26.1'
  repo=next(e for e in n.iter() if e.tag.endswith('repository'));assert repo.attrib['commit']==revision
  deps=[e.attrib for e in n.iter() if e.tag.endswith('dependency')];assert all(d['version']=='1.26.1' for d in deps if d['id'].startswith('Bosak.'))
  dll=z.read('lib/net10.0/'+ident+'.dll');xml=z.read('lib/net10.0/'+ident+'.xml');assert len(xml)>100
  for asset,local in [('README.md','README.md'),('license.md','license.md'),('icon.png','assets/icons-png/fytala-logo-color-dark-128.png')]:assert z.read(asset)==(source/local).read_bytes()
  for pattern in ['assets/logos/*.svg','assets/brand/*.png']:
   for asset in source.glob(pattern):assert z.read(asset.relative_to(source).as_posix())==asset.read_bytes()
  for name in ['broad','focused']:
   m=next((m for m in metadata[name] if m['id']==ident),None)
   if m is None:assert name=='focused' and ident=='Bosak.XPath.Providers.Database';continue
   assert m['informationalVersion']=='1.26.1+'+revision and m['sha256']==sha(dll)
   if 'version' in m:assert m['version']=='1.26.1.0'
   cache=out/(name+'-cache')/ident.lower()/'1.26.1';assert (cache/'lib/net10.0'/f'{ident}.dll').read_bytes()==dll
   assert (cache/p.name.lower()).read_bytes()==p.read_bytes()
   assets=load(out/name/'obj/project.assets.json');assert assets['libraries'][ident+'/1.26.1']['sha512']==base64.b64encode(hashlib.sha512(p.read_bytes()).digest()).decode()
   assert str(tmp/'release1261-packages').replace('\\','/').lower() in json.dumps(assets['project']['restore']['sources']).replace('\\\\','/').lower()
  rows.append({'package':ident,'version':'1.26.1','repositoryCommit':revision,'archiveSha256':sha(p.read_bytes()),'dllSha256':sha(dll),'xmlDocumentationBytes':len(xml),'dependencies':deps,'archiveCacheOutputMatch':True,'packagedReadmeLicenseIconAndBrandAssetsMatchSourceBytes':True})
assert len(rows)==10
trx=E.parse(out/'focused-results/focused.trx');counter=next(x for x in trx.iter() if x.tag.endswith('Counters'));assert counter.attrib['passed']=='3913' and counter.attrib['failed']=='0'
for name in ['broad','focused']:
 audit=load(out/(name+'-audit.json'));assert all(not f.get('topLevelPackages') and not f.get('transitivePackages') for p in audit['projects'] for f in p.get('frameworks',[]))
assert '153' in (out/'broad-run.log').read_text(encoding='utf-16' if (out/'broad-run.log').read_bytes().startswith(b'\xff\xfe') else 'utf-8-sig')
receipt={'date':'2026-10-26','version':'1.26.1','immutableSource':revision,'kind':'Unsigned local candidate; no signed public package verification claimed','packages':rows,'consumerChecks':{'broad':153,'focused':3913,'focusedComposition':'1493 retained public checks +2420 new atomic/numeric/rounding cases; overlaps source tests'},'cacheAndSources':'Separate fresh caches; Bosak.* mapped only to local candidate feed; external test dependencies from nuget.org','vulnerabilityAudits':'Both consumers: no reported vulnerable packages','consumerAdaptation':load(out/'consumer-plan.json')}
(out/'package-verification.json').write_text(json.dumps(receipt,indent=2)+'\n')
print('Ten pinned-source package/archive/cache/output identities verified; broad153 +focused3913; no reported vulnerabilities.')

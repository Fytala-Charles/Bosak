"""Verify loaded binaries, exact frozen totals/skip maps and immutable corpora."""
from pathlib import Path
import json,hashlib,subprocess,re
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
i=json.loads((E/'QUALIFICATION-INPUTS.json').read_text());units=json.loads((E/'ALL-UNITS.json').read_text());assert len(units)==10 and sum(int(r['counters']['passed']) for r in units)==24390 and all(r['counters']['failed']=='0' for r in units)
allowed={}
for row in i['records']:
 for name,h in row['hashes'].items():allowed.setdefault(name.removesuffix('.dll'),set()).add(h)
bindings=[]
for row in i['records']:
 label=row['name'] if row['kind']=='unit' else 'qt3' if row['name']=='Bosak.XPath.Conformance' else 'qt4';seen=set();files=list((E/(label+'-bindings')).glob('*.json'));assert files,label
 for file in files:
  data=json.loads(file.read_text());assert not data['errors'],file
  for v in data['assemblies']:
   assert v['name'] in allowed and v['sha256'] in allowed[v['name']],(label,v['name']);seen.add(v['name'])
 assert row['name'] in seen,(label,seen)
 bindings.append({'runner':row['name'],'processReceipts':len(files),'loadedAssemblies':sorted(seen)})
for name,v in i['corpora'].items():
 assert subprocess.check_output(['git','rev-parse','HEAD'],cwd=R/'tests'/name,text=True).strip()==v['revision']
 assert subprocess.check_output(['git','status','--porcelain'],cwd=R/'tests'/name,text=True)==v['status']
for label,expected in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
 text=(E/(label+'.txt')).read_text(encoding='utf-8');actual=tuple(int(re.search(n+r':\s*(\d+)',text).group(1)) for n in ['Passed','Failed','Skipped']);assert actual==expected,(label,actual)
 assert (E/(label+'-skips.txt')).read_bytes()==(R/'docs/foundation-xslt-source-membership/evidence'/(label+'-skips.txt')).read_bytes(),label
assert 'Experimental-level (XPath40Experimental) passes: 11' in (E/'qt4.txt').read_text()
(E/'FULL-QUALIFICATION.json').write_text(json.dumps({'unitsPassed':24390,'unitProjects':10,'qt3':[31397,0,424],'qt4Gated':[8941,0,1918],'qt4ExperimentalPasses':11,'skipMapsByteIdentical':True,'corporaUnchanged':True,'loadedHashesVerified':True,'bindingRuns':bindings,'solutionBuildWarnings':5,'productionPromoted':False},indent=2)+'\n')
print('Verified 24,390 tests, both exact gates/skip maps, immutable corpora and all runner loaded hashes.')

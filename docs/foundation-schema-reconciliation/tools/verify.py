"""Verify existing-source facts without inferring adapter or owner acceptance."""
from pathlib import Path
import json,hashlib,zipfile
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
i=json.loads((E/'INPUTS.json').read_text());assert not i['productionChanged'] and not i['bcapAdapterImplemented'] and not i['nativeOwnerAcceptance'] and i['ownerWorktreeUnchanged']
assert i['nativeRevision']=='c3624998eca8c83018e121fa84af611f834a0501' and sha(P/'tools/NativeFacts.cs.txt')==i['probeSourceSha256']
a=json.loads((E/'native-facts-0.json').read_text());b=json.loads((E/'native-facts-1.json').read_text());assert a==b and a['passed'] and a['checks']==14
for binding in a['bindings']:
 expected=i['observedNativeAssemblySha256'] if binding['name']=='Bosak.Schema' else i['historicalDependencies'][binding['name']]['sha256'];assert binding['sha256']==expected
 if binding['name']!='Bosak.Schema':assert binding['version']=='1.34.2.0'
if (E/'ARTIFACTS.json').exists():
 artifact=json.loads((E/'ARTIFACTS.json').read_text());assert sha(Path(artifact['path']))==artifact['sha256']
 with zipfile.ZipFile(artifact['path']) as archive:
  for name,h in i['nativeSources'].items():
   member='Schema/'+name.removeprefix('src/Bosak.Schema/');assert hashlib.sha256(archive.read(member)).hexdigest()==h,name
else:
 for name,h in i['nativeSources'].items():assert sha(R/'.worktrees/Bosak.Schema.Assessment'/name)==h,name
if (P/'MANIFEST.json').exists():
 for name,h in json.loads((P/'MANIFEST.json').read_text()).items():assert sha(P/name)==h,name
print('Verified fourteen native source facts twice on explicitly observed historical DLLs; no BCAP adapter, owner acceptance or production change.')

"""Verify TA-16 finite policy selection and unchanged-candidate prerequisite controls."""
from pathlib import Path
import hashlib,json,subprocess,zipfile,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
sha=lambda b:hashlib.sha256(b).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))
i=read(P/'INPUTS.json');s=read(P/'STATUS.json');profile=read(P/'PROFILE.json');cases=read(P/'ACCEPTANCE-CASES.json')
assert i['baseCommit']==s['baseCommit'] and i['sourceVersion']==s['sourceVersion']=='1.63.1'
assert sha((R/i['priorManifest']).read_bytes())==i['priorManifestSha256']
for name,digest in read(R/i['priorManifest']).items():assert sha((R/i['priorManifest']).parent.joinpath(name).read_bytes())==digest,name
for name,digest in i['reviewInputs'].items():assert sha((R/name).read_bytes())==digest,name
pins=read(R/'docs/foundation-public-nilled-cost/evidence/SOURCE-AND-BINARY-PINS.json')
for name,digest in pins['sources'].items():assert sha((R/name).read_bytes())==digest,name
for name,row in pins['engines'].items():assert sha(Path(row['path']).read_bytes())==row['sha256'],name
assert s['policySelected'] and s['existingControlsPassed']==600 and s['newAcceptanceCasesNotRun']==40
for name in ['implementationStarted','dataProfileActivated','newSignatureGapDemonstrated','fullGatesRerun','sourceChanged','binariesChanged','nativeProviderQualified','ownerAccepted','budgetsAccepted','apiFrozen','publicationAuthorized']:assert s[name] is False,name
assert profile['status']=='SelectedForImplementation' and profile['activated'] is False
assert profile['acceptedTrimmedSource']==['data($node)','fn:data($node)']
assert profile['behaviorProfile']=='bosak-original-node-data-v1' and profile['descriptorFormat']=='bosak-original-node-data-descriptor-v1'
assert profile['acceptedDirectInputKinds']==['Undefined','Node'] and profile['acceptedNodeKinds']==['Element','Attribute']
assert profile['output']=={'zero':'Undefined','one':'DirectRegisteredAtomic','many':'PrivateMaterializedOrderedTransportSequence','lazyProviderCallbacks':False,'orderAndDuplicatesPreserved':True}
assert profile['cache']['key']==['containerReference','originReference','ordinal'] and profile['cache']['unit']=='ResidentAtomicScalarEntries'
assert profile['cache']['emptySlots']==0 and not profile['cache']['persistentEmptyIndex'] and not profile['cache']['nodeFactsCache']
assert profile['cache']['allOrNothingCommit'] and not profile['cache']['providerCallbacksUnderLock']
assert profile['provenance']['preserveDeclaredActualAndOriginalAssessment'] and not profile['provenance']['perpetualExtractionNodeIdentity'] and not profile['provenance']['crossSessionImport']
assert profile['errors']['absentElement']=='InvalidOperationException/FOTY0012 prefix'
assert not profile['publicSignaturesAdded'] and not profile['allHostOrImplicitAtomizationActivated']
assert profile['requiresPrivateStagedNodeContainerBridge'] and profile['requiresMaterializedSequenceOutput']
assert cases['implementationStatus']=='NotStarted' and [row['id'] for row in cases['cases']]==['AD'+str(n).zfill(2) for n in range(1,41)]
assert all(row['trigger'] and row['expected'] and row['status']=='NotRun' for row in cases['cases'])
controls=read(E/'CONTROLS.json');actual=[]
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for label,expected in [('runtime',562),('api',38)]:
  name='atomization-policy-'+label+'.log';receipt=json.loads(z.read(name+'.json'))
  assert receipt['exitCode']==0 and receipt['failure'] is None and sha(z.read(name))==receipt['logSha256']
  selected=[row for row in controls['trx'] if row['group']==label];assert len(selected)==1
  row=selected[0];raw=z.read(row['path']);assert sha(raw)==row['sha256']
  r=ET.fromstring(raw);c=r.find('.//{*}Counters').attrib
  assert int(c['passed'])==row['passed']==expected and c['failed']==c['notExecuted']=='0'
  actual.extend(n.attrib for n in r.findall('.//{*}UnitTestResult'))
assert actual==controls['results'] and controls['passed']==600 and controls['failed']==0 and all(row['outcome']=='Passed' for row in actual)
assert any('PublicOriginalNilledTests.NP16_NP17_NP24_CompileRefusal' in row['testName'] and 'data($node)' in row['testName'] for row in actual)
for label,expected in controls['loadedBindings'].items():
 seen=set()
 for f in (E/'bindings'/label).glob('*.json'):
  b=read(f);assert not b['errors']
  for a in b['assemblies']:
   if a['name'] in pins['engines']:
    assert a['sha256']==pins['engines'][a['name']]['sha256'] and a['context']=='Default' and not a['collectible']
    seen.add(a['name'])
 assert sorted(seen)==expected and {'Bosak.XPath.Core','Bosak.XPath.Api','Bosak.XPath.Runtime'}<=seen
subprocess.run(['python','-B',str(R/'docs/foundation-public-nilled-cost/tools/verify.py')],check=True,capture_output=True)
if (P/'MANIFEST.json').exists():
 m=read(P/'MANIFEST.json');assert set(m)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
 for name,digest in m.items():assert sha((P/name).read_bytes())==digest,name
print('Verified TA-16: finite inactive data policy, 600 exact unchanged-binary controls, 40 AD cases NotRun; no source/version/native/freeze/publication change.')

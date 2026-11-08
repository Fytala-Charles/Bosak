"""Verify TA-18 policy selection, exact unchanged candidate and prerequisite controls."""
from pathlib import Path
import hashlib,json,subprocess,zipfile,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
sha=lambda b:hashlib.sha256(b).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))
i=read(P/'INPUTS.json');s=read(P/'STATUS.json');profile=read(P/'PROFILE.json')
assert i['baseCommit']==s['baseCommit'] and i['sourceVersion']==s['sourceVersion']=='1.64.0'
assert sha((R/i['priorManifest']).read_bytes())==i['priorManifestSha256']
for name,digest in read(R/i['priorManifest']).items():assert sha((R/i['priorManifest']).parent.joinpath(name).read_bytes())==digest,name
for name,digest in i['reviewInputs'].items():assert sha((R/name).read_bytes())==digest,name
pins=read(R/'docs/foundation-public-node-data/evidence/SOURCE-AND-BINARY-PINS.json')
for name,digest in pins['sources'].items():assert sha((R/name).read_bytes())==digest,name
for name,row in pins['engines'].items():assert sha(Path(row['path']).read_bytes())==row['sha256'],name
assert s['policySelected'] and s['existingControlsPassed']==747 and s['fullUnitTestsHistorical']==25453
for key in ['implementationStarted','constructorProfileActivated','providerOperationAdmissionImplemented','publicSignaturesAdded','newRequiredProviderMembers','broaderPersistentSignatureNeedsClosed','fullGatesRerun','sourceChanged','binariesChanged','nativeProviderQualified','ownerAccepted','budgetsAccepted','apiFrozen','publicationAuthorized']:assert s[key] is False,key
assert profile['status']=='SelectedForImplementation' and not profile['activated']
assert profile['acceptedTrimmedSource']==['$constructed'] and profile['consumer']['variable']=='constructed'
assert profile['behaviorProfile']=='bosak-constructed-atomic-v1' and profile['descriptorFormat']=='bosak-constructed-atomic-descriptor-v1'
assert not profile['sourceSnapshotsAllowed'] and not profile['fabricatedInputAllowed'] and not profile['ownsTypedValueWithNullInputAllowed']
assert profile['providerAdmission']=='Exact bound advisor must implement existing IQueryOperationLifetime'
assert profile['input']=={'kind':'Unmarked lexical String','sourceOrConstructedMarkersAllowed':False,'userSchemaTypeAnnotationAllowed':False}
assert profile['output']['families']==['String','Boolean','Integer','Decimal'] and profile['output']['originalAssessment']=='MustBeNull'
assert profile['output']['receiptScope']=='Issuing raw operation only' and profile['requiresProviderOperationAdmissionPrerequisite']
assert profile['lifetime']['successfulProviderAdmissionsPerRawOperation']==1 and profile['lifetime']['tailCallsDelayCleanupAndDrain']
assert profile['lifetime']['callbackWindowEndRevokesDelivery'] and not profile['lifetime']['providerCallsUnderGate']
assert not profile['lifetime']['disposeProviderResources'] and not profile['lifetime']['waitCancellationReleasesRawWork']
assert profile['quotas']['sourceCacheSlotsConsumed']==0 and not profile['quotas']['persistentReceiptRegistry']
assert not profile['quotas']['aggregateCallOrByteBudgetPromised'] and not profile['quotas']['providerLexicalParseBudgetPromised']
assert not profile['publicSignaturesAdded'] and not profile['newRequiredProviderMembers'] and not profile['broaderPersistentSignatureNeedsClosed']
assert profile['episode']['firstCallFixesRawEpisode'] and profile['episode']['exactDateTimeOffsetRepresentation']
assert profile['episode']['timezoneMinutesMin']==-840 and profile['episode']['timezoneMinutesMax']==840
for filename,prefix,count in [('LIFETIME-CASES.json','LC',16),('ACCEPTANCE-CASES.json','SC',40)]:
 cases=read(P/filename);assert cases['implementationStatus']=='NotStarted'
 assert [row['id'] for row in cases['cases']]==[prefix+str(n).zfill(2) for n in range(1,count+1)]
 assert all(row['status']=='NotRun' and row['trigger'] and row['expected'] for row in cases['cases'])
assert s['newLifetimeCasesNotRun']==16 and s['newConstructionCasesNotRun']==40
controls=read(E/'CONTROLS.json');actual=[]
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for label,expected in [('runtime',709),('api',38)]:
  name='construction-policy-'+label+'.log';receipt=json.loads(z.read(name+'.json'))
  assert receipt['exitCode']==0 and receipt['failure'] is None and sha(z.read(name))==receipt['logSha256']
  selected=[row for row in controls['trx'] if row['group']==label];assert len(selected)==1
  row=selected[0];raw=z.read(row['path']);assert sha(raw)==row['sha256']
  root=ET.fromstring(raw);c=root.find('.//{*}Counters').attrib
  assert int(c['passed'])==row['passed']==expected and c['failed']==c['notExecuted']=='0'
  actual.extend(n.attrib for n in root.findall('.//{*}UnitTestResult'))
assert actual==controls['results'] and controls['passed']==747 and controls['failed']==0 and all(row['outcome']=='Passed' for row in actual)
for pattern in ['SchemaOperationLeaseSetTests.','SchemaQuerySessionTests.','PublicOriginalDataTests.','PublicOrdinarySessionTests.']:
 assert any(pattern in row['testName'] for row in actual),pattern
for label,expected in controls['loadedBindings'].items():
 seen=set()
 for f in (E/'bindings'/label).glob('*.json'):
  b=read(f);assert not b['errors']
  for a in b['assemblies']:
   if a['name'] in pins['engines']:
    assert a['sha256']==pins['engines'][a['name']]['sha256'] and a['context']=='Default' and not a['collectible']
    seen.add(a['name'])
 assert sorted(seen)==expected and {'Bosak.XPath.Api','Bosak.XPath.Core','Bosak.XPath.Runtime'}<=seen
subprocess.run(['python','-B',str(R/'docs/foundation-public-node-data/tools/verify.py')],check=True,capture_output=True)
if (P/'MANIFEST.json').exists():
 m=read(P/'MANIFEST.json');assert set(m)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
 for name,digest in m.items():assert sha((P/name).read_bytes())==digest,name
print('Verified TA-18: finite inactive construction/lifetime policy, 747 unchanged-binary controls; LC16/SC40 NotRun. No source/version/freeze/publication change.')

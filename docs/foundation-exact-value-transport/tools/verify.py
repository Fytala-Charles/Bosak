"""Verify exact exact numeric carriers source, receipts and regression gates without rebuilding."""
from pathlib import Path
import hashlib,json,zipfile,xml.etree.ElementTree as ET,subprocess,re
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
s=json.loads((P/'STATUS.json').read_text());pins=json.loads((E/'SOURCE-AND-BINARY-PINS.json').read_text());assert s['sourceVersion']==pins['sourceVersion']=='1.47.0' and s['unitTestsPassed']==24591
for name,h in pins['sources'].items():assert sha(R/name)==h,name
with zipfile.ZipFile(E/'ENGINE-BINARIES.zip') as z:
 for name,row in pins['engines'].items():assert sha(Path(row['path']))==row['sha256']==hashlib.sha256(z.read(name+'.dll')).hexdigest(),name
with zipfile.ZipFile(E/'CHANGED-SOURCE.zip') as z:
 for name in z.namelist():assert hashlib.sha256(z.read(name)).hexdigest()==pins['sources'][name],name
for name,row in pins['corpora'].items():
 assert subprocess.check_output(['git','rev-parse','HEAD'],cwd=R/'tests'/name,text=True).strip()==row['revision']
 assert subprocess.check_output(['git','status','--porcelain'],cwd=R/'tests'/name,text=True).strip()==row['status']
u=json.loads((E/'UNIT-RESULTS.json').read_text());assert len(u['projects'])==10 and sum(x['passed'] for x in u['projects'])==24591
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for row in u['projects']:
  raw=z.read(row['trx']);assert hashlib.sha256(raw).hexdigest()==row['sha256'];c=ET.fromstring(raw).find('.//{*}Counters').attrib;assert int(c['passed'])==row['passed'] and c['failed']=='0' and c['notExecuted']=='0'
 log=json.loads(z.read('exact-value-units.log.json'));assert log['exitCode']==0 and log['failure'] is None
 a=json.loads((P/'ACCEPTANCE.json').read_text());assert a['exactControlsPassed']==29 and a['publicContractControlsPassed']==3 and len(a['retainedPublicResults'])==38 and len(a['retainedPrimitiveResults'])==43 and all(x['outcome']=='Passed' for x in a['exactResults']+a['contractResults']+a['retainedPublicResults']+a['ordinaryResults'])
 for label,expected in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
  log=z.read('exact-value-'+label+'.log').decode('utf-8-sig');assert tuple(int(re.search(k+r':\s+(\d+)',log).group(1)) for k in ['Passed','Failed','Skipped'])==expected
for f in (E/'bindings').rglob('*.json'):
 b=json.loads(f.read_text());assert not b['errors']
 for a in b['assemblies']:
  if a['name'] in pins['engines']:assert a['sha256']==pins['engines'][a['name']]['sha256'],a
subprocess.run(['python',str(P/'tools/audit.py')],check=True,capture_output=True)
for label in ['qt3','qt4']:assert (E/(label+'-skips.txt')).read_bytes()==(R/'docs/foundation-bcap-profile/evidence'/(label+'-skips.txt')).read_bytes()
for key in ['numericChannelSupported','nativeProviderQualified','c45Accepted','apiFrozen','publicationAuthorized']:assert s[key] is False
if (P/'MANIFEST.json').exists():
 for name,h in json.loads((P/'MANIFEST.json').read_text()).items():assert sha(P/name)==h,name
assert s['xdmValueExactTransportImplemented'] and s['legacyNarrowingChecked'] and s['reviewedNumericWitnessMatches'] and s['xdmValueFieldLayoutUnchanged'] and not s['numericChannelSupported'] and not s['schemaOperationsSupported']
cost=json.loads((E/'ALLOCATION-OBSERVATIONS.json').read_text());assert not cost['budgetsAccepted'] and len(cost['bytesPerOperation'])==10 and cost['xdmValueBytes']==48
for name in ['legacy-inline-factory','exact-inline-factory','mark-inline','read-wide-integer','read-exact-decimal','read-checked-legacy-decimal']:assert cost['bytesPerOperation'][name]==0
calls=json.loads((E/'CONSUMER-AUDIT.json').read_text());assert calls['references']==192 and len(calls['files'])==13 and not calls['queryNumericProfileEnabled']
for row in calls['files']:assert sha(R/row['path'])==row['sha256']
compat=json.loads((E/'SOURCE-COMPATIBILITY.json').read_text());assert not compat['sourceCompatibilityUnconditional'] and compat['existingBinarySignaturesRetained'] and compat['candidateCoreSha256']==pins['engines']['Bosak.XPath.Core']['sha256']
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for name,code in [('baseline',0),('candidate',1),('typed',0)]:
  row=compat['builds'][name];receipt=z.read('exact-value-compat-'+name+'.log.json');log=z.read('exact-value-compat-'+name+'.log')
  assert hashlib.sha256(receipt).hexdigest()==row['receiptSha256'] and hashlib.sha256(log).hexdigest()==row['logSha256'] and json.loads(receipt)['exitCode']==code
  if name=='candidate':assert b'CS0121' in log
print('Verified exact numeric carriers: 24591 tests, 32 new controls, reviewed witness/unchanged fields, allocation and known source-default migration receipts. Wider numeric/native/freeze work pending.')

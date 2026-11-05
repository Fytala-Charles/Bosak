"""Verify exact quotient source, receipts and regression gates without rebuilding."""
from pathlib import Path
import hashlib,json,zipfile,xml.etree.ElementTree as ET,subprocess,re
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
s=json.loads((P/'STATUS.json').read_text());pins=json.loads((E/'SOURCE-AND-BINARY-PINS.json').read_text());assert s['sourceVersion']==pins['sourceVersion']=='1.50.0' and s['unitTestsPassed']==24714
for name,h in pins['sources'].items():assert sha(R/name)==h,name
with zipfile.ZipFile(E/'ENGINE-BINARIES.zip') as z:
 for name,row in pins['engines'].items():assert sha(Path(row['path']))==row['sha256']==hashlib.sha256(z.read(name+'.dll')).hexdigest(),name
with zipfile.ZipFile(E/'CHANGED-SOURCE.zip') as z:
 for name in z.namelist():assert hashlib.sha256(z.read(name)).hexdigest()==pins['sources'][name],name
for name,row in pins['corpora'].items():
 assert subprocess.check_output(['git','rev-parse','HEAD'],cwd=R/'tests'/name,text=True).strip()==row['revision']
 assert subprocess.check_output(['git','status','--porcelain'],cwd=R/'tests'/name,text=True).strip()==row['status']
u=json.loads((E/'UNIT-RESULTS.json').read_text());assert len(u['projects'])==10 and sum(x['passed'] for x in u['projects'])==24714
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for row in u['projects']:
  raw=z.read(row['trx']);assert hashlib.sha256(raw).hexdigest()==row['sha256'];c=ET.fromstring(raw).find('.//{*}Counters').attrib;assert int(c['passed'])==row['passed'] and c['failed']=='0' and c['notExecuted']=='0'
 log=json.loads(z.read('exact-quotient-units.log.json'));assert log['exitCode']==1 and log['failure'] is None
 initialFailed=[]
 for name in z.namelist():
  if name.startswith('exact-quotient-unit-trx/') and name.endswith('.trx'):
   initialFailed.extend(x for x in ET.fromstring(z.read(name)).iter() if x.tag.endswith('UnitTestResult') and x.attrib.get('outcome')=='Failed')
 assert initialFailed and all('Database.Tests.' in x.attrib['testName'] and 'HttpListenerException' in ''.join(x.itertext()) for x in initialFailed)
 retry=json.loads(z.read('exact-quotient-database.log.json'));assert retry['exitCode']==0 and retry['failure'] is None
 a=json.loads((P/'ACCEPTANCE.json').read_text());assert a['newControls']==29 and a['retainedArithmeticControlsPassed']==41 and all(x['outcome']=='Passed' for x in a['retainedArithmeticResults']) and a['numericCoreControlsPassed']==4 and a['publicNumericControlsPassed']==23 and len(a['retainedPublicResults'])==38 and all(x['outcome']=='Passed' for x in a['coreResults']+a['numericResults']+a['retainedPublicResults']+a['ordinaryResults'])
 for label,expected in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
  log=z.read('exact-quotient-'+label+'.log').decode('utf-8-sig');assert tuple(int(re.search(k+r':\s+(\d+)',log).group(1)) for k in ['Passed','Failed','Skipped'])==expected
for f in (E/'bindings').rglob('*.json'):
 b=json.loads(f.read_text());assert not b['errors']
 for a in b['assemblies']:
  if a['name'] in pins['engines']:assert a['sha256']==pins['engines'][a['name']]['sha256'],a
subprocess.run(['python',str(P/'tools/audit.py')],check=True,capture_output=True)
for label in ['qt3','qt4']:assert (E/(label+'-skips.txt')).read_bytes()==(R/'docs/foundation-bcap-profile/evidence'/(label+'-skips.txt')).read_bytes()
for key in ['fullNumericProfileQualified','nativeProviderQualified','c45Accepted','apiFrozen','publicationAuthorized']:assert s[key] is False
if (P/'MANIFEST.json').exists():
 for name,h in json.loads((P/'MANIFEST.json').read_text()).items():assert sha(P/name)==h,name
assert s['numericChannelSupported'] and s['operationDigitScaleLimitsEnforced'] and s['publicMetadataUnchanged'] and not s['legacyNumericRoutesChanged'] and not s['fullNumericProfileQualified'] and not s['schemaOperationsSupported']
print('Verified exact quotient: 24714 tests, 29 new controls, exact frozen gates and unchanged public API. Full numeric/schema/all-host/native/freeze work remains pending.')

assert s["profile"]=="bosak-exact-quotient-v3" and s["exactQuotientImplemented"] and s["exactRemainderImplemented"] and not s["decimalDivisionImplemented"]

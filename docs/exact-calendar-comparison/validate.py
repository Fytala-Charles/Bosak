from pathlib import Path
import json,hashlib,gzip,re,xml.etree.ElementTree as E
D=Path(__file__).resolve().parent;R=D.parents[1]
for n,h in json.loads((D/'INPUTS.json').read_text(encoding='utf-8')).items():assert hashlib.sha256((R/n).read_bytes()).hexdigest()==h,n
for row in json.loads((D/'PRIOR-SEALS.json').read_text(encoding='utf-8')):
 p=R/row['packet'];assert hashlib.sha256(p.read_bytes()).hexdigest()==row['sha256']
 for n,h in json.loads(p.read_text(encoding='utf-8')).items():assert hashlib.sha256((p.parent/n).read_bytes()).hexdigest()==h,n
s=json.loads((D/'STATUS.json').read_text(encoding='utf-8'));assert s['maximumContracts']==20 and len(s['contracts'])==20 and all(x['status']=='implemented-qualified' for x in s['contracts'])
q=json.loads((D/'QUALIFICATION.json').read_text(encoding='utf-8'));assert q['solutionPassed']==26634 and q['packagePassed']==44 and q['newSourceCases']==44;assert q['qt3']=={'passed':31397,'failed':0,'skipped':424};assert q['gatedQt4']=={'passed':8941,'failed':0,'skipped':1918};assert len(q['coreApiDelta']['addedRows'])==15 and len(q['coreApiDelta']['newTypes'])==3 and not q['coreApiDelta']['removedOrChangedRows'];assert len(q['packages'])==9 and all(x['sourceMatches'] and x['consumerMatches'] is not False for x in q['packages']);assert not q['published'] and not q['schemaNativeAdmissionsChanged'] and not q['legacyMapAndExpressionDispatchChanged']
ns={'t':'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
for f in (D/'evidence').glob('*.trx.gz'):assert E.fromstring(gzip.decompress(f.read_bytes())).find('t:ResultSummary/t:Counters',ns).get('failed')=='0'
ids=set()
for n in ['tests/Bosak.XPath.Core.Tests/ExactCalendarComparisonTests.cs','tests/Bosak.XPath.Core.Tests/ExactCalendarComparisonAuthorityTests.cs','tests/Bosak.XPath.Runtime.Tests/ExactCalendarPublicComparisonTests.cs']:ids.update(re.findall(r'public void (C[0-9]{2})_',(R/n).read_text(encoding='utf-8')))
assert ids=={f'C{i:02}' for i in range(1,21)}
print('Twenty comparison contracts,26634solution/44package passes, additive API delta and exact conformance gates verify; prior seals intact.')

from pathlib import Path
import json,hashlib,gzip,xml.etree.ElementTree as E,re
D=Path(__file__).resolve().parent;R=D.parents[1]
for n,h in json.loads((D/'INPUTS.json').read_text()).items():assert hashlib.sha256((R/n).read_bytes()).hexdigest()==h,n
s=json.loads((D/'STATUS.json').read_text());assert s['maximumContracts']==20 and [x['id'] for x in s['contracts']]==[f'C{i:02}' for i in range(1,21)];assert all(x['status']=='implemented-qualified' for x in s['contracts'])
a=json.loads((D/'ADOPTION.json').read_text());assert a['recordedBeforeImplementation'] and len(a['contracts'])==20
q=json.loads((D/'QUALIFICATION.json').read_text());assert q['solutionDistinctPassingAfterEnvironmentRetry']==26539;assert q['qt3']=={'passed':31397,'failed':0,'skipped':424};assert q['gatedQt4']['failed']==0 and q['focusedAndPackageCases']==51;assert q['package']['sourcePackageRuntimeIdentical'] and not q['package']['published']
ns={'t':'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
for f in (D/'evidence').glob('*.trx.gz'):
 root=E.fromstring(gzip.decompress(f.read_bytes()));c=root.find('t:ResultSummary/t:Counters',ns)
 if not f.name.startswith('solution-results-'):assert c.get('failed')=='0'
tests=(R/'tests/Bosak.XPath.Core.Tests/XdmExactCalendarTests.cs').read_text(encoding='utf-8');assert len(set(re.findall(r'public void (C[0-9]{2})_',tests)))==20
print('Twenty implemented contract groups, exact input hashes, retained retry evidence, 51 package cases and unchanged conformance gates verify.')

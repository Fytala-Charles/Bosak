from pathlib import Path
import json,hashlib,gzip,re,xml.etree.ElementTree as E
D=Path(__file__).resolve().parent;R=D.parents[1]
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
for n,h in json.loads((D/'INPUTS.json').read_text()).items():assert sha(R/n)==h,n
for n,row in json.loads((D/'GIT-INPUTS.json').read_text()).items():
 b=(R/n).read_bytes();normalized=b if row['checkoutMatchesRepository'] else b.replace(b'\r\n',b'\n');assert hashlib.sha256(normalized).hexdigest()==row['repositorySha256'],n
for row in json.loads((D/'PRIOR-SEALS.json').read_text()):
 p=R/row['packet'];assert sha(p)==row['sha256']
 for n,h in json.loads(p.read_text()).items():assert sha(p.parent/n)==h,n
s=json.loads((D/'STATUS.json').read_text());assert s['maximumContracts']==20 and len(s['contracts'])==20 and all(x['status']=='implemented-qualified' for x in s['contracts'])
q=json.loads((D/'QUALIFICATION.json').read_text());assert sum(x['passed'] for x in q['lanes']['final-runtime-reviewed'])==2038 and sum(x['passed'] for x in q['lanes']['package-reviewed'])==73;assert q['solutionPassed']==26750 and q['packagePassed']==73 and q['newSourceCases']==72
assert q['qt3']==dict(passed=31397,failed=0,skipped=424) and q['gatedQt4']==dict(passed=8941,failed=0,skipped=1918)
a=q['coreApiDelta'];assert a['preservedRows']==617 and not a['addedRows'] and not a['newTypes'] and not a['removedOrChangedRows']
a=q['apiDelta'];assert len(a['addedRows'])==1 and '|CompileCalendarComparison|' in a['addedRows'][0] and not a['newTypes'] and not a['removedOrChangedRows']
assert len(q['packages'])==9 and all(x['sourceMatches'] and x['consumerMatches'] is not False for x in q['packages'])
assert not q['published'] and not q['schemaNativeAdmissionsChanged'] and not q['legacyMapAndExpressionDispatchChanged']
ns={'t':'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
for f in (D/'evidence').glob('*.trx.gz'):assert E.fromstring(gzip.decompress(f.read_bytes())).find('t:ResultSummary/t:Counters',ns).get('failed')=='0',f.name
for name,expected in [('qt3',(31821,31397,0,424)),('qt4',(10859,8941,0,1918))]:
 text=gzip.decompress((D/'evidence'/(name+'.log.gz')).read_bytes()).decode('utf-8')
 values=tuple(int(re.findall(r'^'+key+r':\s+([0-9]+)\s*$',text,re.M)[-1]) for key in ['Total','Passed','Failed','Skipped']);assert values==expected,(name,values)
 oldBytes=gzip.decompress((R/'docs/exact-calendar-comparison/evidence'/(name+'.log.gz')).read_bytes());old=oldBytes.decode('utf-16' if oldBytes.startswith(b'\xff\xfe') else 'utf-8-sig')
 def reasons(t):
  start=t.index('Skip reasons (grouped):');return '\n'.join(t[start:].split('Elapsed:')[0].strip().splitlines())
 assert reasons(text)==reasons(old),name+' grouped skip reasons changed'
ids=set()
for name in ['tests/Bosak.XPath.Runtime.Tests/GuardedCalendarQueryTests.cs']:
 t=(R/name).read_text(encoding='utf-8');assert t.startswith('// =====') and 'SPDX-License-Identifier: Apache-2.0' in t[:2200];ids.update(re.findall(r'public (?:async Task|void) (C[0-9]{2})_',t))
assert ids=={f'C{i:02}' for i in range(1,21)}
assert len(json.loads((D/'PRIOR-SEALS.json').read_text()))==5
print('20guarded-calendar contracts;26750solution/73package passes;617Core rows and old Api rows preserved; one factory added; frozen gates/prior5seals verify.')

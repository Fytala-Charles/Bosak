from pathlib import Path
import json,hashlib,zipfile,re,xml.etree.ElementTree as ET,subprocess
r=Path(__file__).resolve().parents[2];d=Path(__file__).resolve().parent
for name,sha in json.loads((d/'MANIFEST.json').read_text()).items():assert hashlib.sha256((d/name).read_bytes()).hexdigest()==sha,name
result=json.loads((d/'RESULTS.json').read_text());assert result['success'] and result['solutionPassed']==26435 and result['languageServerPassed']==90 and not result['sourceOfExcessKnown']
assert result['originalActualBytes']==4000120 and result['newUpperBoundBytes']==4001024 and not result['boundaryExamples']['11200000']
with zipfile.ZipFile(d/'EVIDENCE.zip') as z:
 failure=json.loads(z.read('ORIGINAL-FAILURE.json'));assert len(failure['failed'])==1 and '4000120' in failure['failed'][0]['message']
 tree=ET.fromstring(z.read('original-runtime.trx'));ns={'t':'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'};failed=[t for t in tree.findall('.//t:UnitTestResult',ns) if t.attrib.get('outcome')=='Failed'];assert len(failed)==1 and '4000120' in failed[0].findtext('.//t:Message',namespaces=ns)
 checks=json.loads(z.read('CHECKS.json'));assert len(checks)==4 and all(c['exitCode']==0 for c in checks)
 text=z.read('UNIT.log').decode('utf-8-sig');assert sum(map(int,re.findall(r'Passed:\s+(\d+)',text)))==26435 and not re.search(r'Failed:\s+[1-9]',text)
 assert 'Passed:    90' in z.read('LSP.log').decode('utf-8-sig')
 current=(r/'tests/Bosak.XPath.Runtime.Tests/MatcherClosureTests.cs').read_bytes().replace(b'\r\n',b'\n');assert z.read('source/after-tests.cs.txt').replace(b'\r\n',b'\n')==current
 assert b'Assert.Equal(0, bytes)' in current and b'Assert.InRange(bytes, 0L, 40L * 100000 + 1024)' in current
 assert z.read('source/before-tests.cs.txt')==subprocess.check_output(['git','show',result['base']+':tests/Bosak.XPath.Runtime.Tests/MatcherClosureTests.cs'],cwd=r)
assert subprocess.check_output(['git','diff',result['base'],'HEAD','--','src','tests/Bosak.Xslt.Conformance','Bosak.sln','global.json','.github'],cwd=r,text=True).strip()==''
print('Original failed TRX and corrected full-test receipts verified; source scope unchanged; no .NET launched.')

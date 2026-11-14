from pathlib import Path
r=Path(__file__).resolve().parents[2];d=Path(__file__).resolve().parent
import json,hashlib,zipfile,re,subprocess,xml.etree.ElementTree as ET
for name,sha in json.loads((d/'MANIFEST.json').read_text()).items():assert hashlib.sha256((d/name).read_bytes()).hexdigest()==sha,name
result=json.loads((d/'RESULTS.json').read_text());assert result['success'] and result['fixedAllowanceWithdrawn'] and result['solutionPassed']==26435 and result['languageServerPassed']==90
with zipfile.ZipFile(d/'EVIDENCE.zip') as z:
 check=json.loads(z.read('CHECKS.json'));assert len(check)==4 and all(x['exitCode']==0 for x in check)
 text=z.read('UNIT.log').decode('utf-8-sig');assert sum(map(int,re.findall(r'Passed:\s+(\d+)',text)))==26435 and not re.search(r'Failed:\s+[1-9]',text)
 assert 'Passed:    90' in z.read('LSP.log').decode('utf-8-sig')
 for name,sha in result['sourceHashes'].items():
  snapshot=z.read('source/'+name);assert hashlib.sha256(snapshot).hexdigest()==sha
  assert snapshot.replace(b'\r\n',b'\n')==(r/name).read_bytes().replace(b'\r\n',b'\n')
 before=json.loads(z.read('FAILED-PR493.json'));assert len(before['failed'])==2 and any('5512' in f['message'] for f in before['failed']) and any('4007040' in f['message'] for f in before['failed'])
 tree=ET.fromstring(z.read('original-failed-runtime.trx'));ns={'t':'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'};assert len([x for x in tree.findall('.//t:UnitTestResult',ns) if x.attrib['outcome']=='Failed'])==2
 production=json.loads(z.read('PRODUCTION-WORKER.json'));assert production['success'];assert production['sourceSha256']==result['sourceHashes']['tests/Bosak.XPath.AllocationChecks/Program.cs']
 for report in production['reports']:
  assert report['success'] and report['iterations']==100000 and report['warmup']==10000
  assert all(x['bytes']==(4000000 if x['name'].startswith('convert/') else 0) for x in report['rows'])
 calibration=json.loads(z.read('CALIBRATION-VERIFIED.json'));assert calibration['success'] and calibration['sameCompatibleWorkerAcrossVersions'] and len({x['workerSha256'] for x in calibration['runs']})==1
 for item in calibration['runs']:
  report=item['report'];assert report['success'];rows={x['name']:x['bytes'] for x in report['rows']}
  if item['label']=='current':assert all(v==(4000000 if k.startswith('convert/') else 0) for k,v in rows.items())
  elif item['label']=='baseline-1.81.9':assert rows['=']==rows['!=']==0 and all(rows[k]==100800000 for k in ['<','<=','>','>='])
  else:assert all(v==(11200000 if k.startswith('convert/') else 7200000) for k,v in rows.items())
 assert 'Expected comparisons or matching.' in z.read('calibration/current-invalid.stderr').decode('utf-8')
 assert json.loads(z.read('WORKER-XML-CHECK.json'))['unchanged']
assert subprocess.check_output(['git','diff',result['base'],'HEAD','--','src','tests/Bosak.Xslt.Conformance','global.json','.github'],cwd=r,text=True).strip()==''
print('Both original failures, full passing suite, all worker rows and retained-engine regression controls verified; no .NET launched.')

"""Read-only replay of the observational error-set audit, not a gate admission."""
import collections,csv,hashlib,importlib.util,io,json,re,xml.etree.ElementTree as ET,zipfile
from pathlib import Path
here=Path(__file__).resolve().parent;root=here.parents[1]
def module(name,path):
 spec=importlib.util.spec_from_file_location(name,path);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);return m
gate=module('gate',root/'tools/xslt30_gate.py');reference=module('reference',root/'docs/xslt30-qualification/report.py')
policy,full_plan,inputs,full_skips=gate.baseline()
manifest=json.loads((here/'MANIFEST.json').read_text(encoding='utf-8'))
def check(data,expected):assert len(data)==expected['bytes'] and hashlib.sha256(data).hexdigest()==expected['sha256']
for name,expected in manifest['files'].items():check((here/name).read_bytes(),expected)
with zipfile.ZipFile(here/'EVIDENCE.zip') as archive:
 assert set(archive.namelist())==set(manifest['archiveEntries'])
 for name,expected in manifest['archiveEntries'].items():check(archive.read(name),expected)
 read=lambda name:json.loads(archive.read(name))
 plan,run,report,audit,initial=[read(n) for n in ['PLAN.json','RUN.json','REPORT.json','AUDIT.json','AUDIT-INPUT.json']]
 selected=[s for s in full_plan['sets'] if 'error' in s['name'].lower()]
 assert len(selected)==1 and selected[0]['name']=='error' and selected==plan['sets'] and plan['totalCases']==579
 assert plan['sourceCommit']==run['sourceCommit']=='df16de7f39a556c6f9afea3eee9f61ef9bbc602e'
 assert plan['corpusCommit']==policy['corpusCommit'] and run['command'][-1]=='error'
 assert run['status']=='Completed' and run['completeCatalogCounts'] and run['success']
 assert run['timeoutSeconds']==1200 and run['memoryLimitBytes']==8589934592
 for name in ['stdout.log','stderr.log']:assert hashlib.sha256(archive.read(name)).hexdigest()==run[name+'Sha256']
 rows=reference.parse(plan,archive.read('stdout.log').decode('utf-8-sig'),True)
 assert len(rows)==579 and {r['test'] for r in rows}==set(selected[0]['cases'])
 counts={k:sum(r['status']==k for r in rows) for k in ['Passed','Failed','Skipped']}
 assert counts==report['counts']=={'Passed':513,'Failed':0,'Skipped':66}
 assert all(run['counts'][k]==v for k,v in counts.items())
 recorded=list(csv.DictReader(io.StringIO(archive.read('outcomes.csv').decode('utf-8'))))
 assert {(r['test'],r['status']) for r in rows}=={(r['test'],r['status']) for r in recorded}
 assert all(r['original_gate_status']=='Skipped' for r in recorded)
 old={r['test']:r for r in full_skips if r['test_set']=='error'}
 assert len(old)==579 and all(r['original_gate_reason']==old[r['test']]['reason'] for r in recorded)
 skipped=[r for r in recorded if r['status']=='Skipped']
 assert len(skipped)==66 and dict(collections.Counter(r['reason'] for r in skipped))=={g['reason']:g['count'] for g in report['skipReasonGroups']}
 assert read('CORPUS-BEFORE.json')==inputs==read('CORPUS-AFTER.json')
 assert not any(audit['corpusDifferences'].values()) and all(audit[k] for k in ['sourceCorpusUnchanged','runtimeUnchanged','gatePolicyUnchanged'])
 assert initial['adoptedGatePolicy']==policy and report['adoptedGateCounts']=={'Passed':10221,'Failed':0,'Skipped':4380}
 assert report['adoptedGateUnchanged'] and report['measurementComplete']
 assert hashlib.sha256(archive.read('ERROR-TEST-SET.xml')).hexdigest()==selected[0]['sha256']
 original=archive.read('QUALIFIED-CONTROLLER.py').decode('utf-8').replace('\r\n','\n')
 adapted=archive.read('run-selected.py').decode('utf-8').replace('\r\n','\n')
 before="'--processor-profile', 'legacy', str(packet / 'corpus/catalog.xml')]"
 after="'--processor-profile', 'legacy', str(packet / 'corpus/catalog.xml'), 'error']"
 assert original.count(before)==1 and original.replace(before,after)==adapted
 assert hashlib.sha256(archive.read('run-selected.py')).hexdigest()==initial['controllerAdaptationSha256']
 oracle=read('ORACLE-REVIEW.json');source=archive.read('RUNNER-SOURCE.cs')
 assert hashlib.sha256(source).hexdigest()==oracle['sourceSha256']
 assert source.replace(b'\r\n',b'\n')==(root/'tests/Bosak.Xslt.Conformance/Program.cs').read_bytes().replace(b'\r\n',b'\n')
 assert len(oracle['codeAliasPairs'])==22 and len(oracle['messageLabelPairs'])==7 and oracle['reviewCandidateCount']==71
 assert len(oracle['reviewCandidates'])==71
 assert all((c['test'],'Passed') in {(r['test'],r['status']) for r in rows} for c in oracle['reviewCandidates'])
print('Verified all 579 identities: 513 Legacy passes / 0 failures / 66 explained skips; unchanged inputs, runtime and adopted gate.')
print('No exact-code/error-stage proof or gate admission: 71 potential alias-review cases remain unclassified.')

"""Read-only replay of observed identities; never changes gate acceptance."""
import collections,csv,hashlib,importlib.util,io,json,re,zipfile
from pathlib import Path
here=Path(__file__).resolve().parent
manifest=json.loads((here/'MANIFEST.json').read_text(encoding='utf-8'))
def check(data,expected):assert len(data)==expected['bytes'] and hashlib.sha256(data).hexdigest()==expected['sha256']
for name,expected in manifest['files'].items():check((here/name).read_bytes(),expected)
spec=importlib.util.spec_from_file_location('classifier',here/'classifier.py');classifier=importlib.util.module_from_spec(spec);spec.loader.exec_module(classifier)
with zipfile.ZipFile(here/'EVIDENCE.zip') as archive:
 assert set(archive.namelist())==set(manifest['archiveEntries'])
 for name,expected in manifest['archiveEntries'].items():check(archive.read(name),expected)
 read=lambda name:json.loads(archive.read(name))
 report=read('REPORT.json');run=read('measurement/RUN.json');plan=read('measurement/PLAN.json')
 assert run['success'] and run['sourceCommit'] is None and plan['totalCases']==579
 assert run['command'][-1]=='error' and run['processorProfile']=='Legacy' and run['catalogProfile']=='Xslt30' and not run['schemaAware']
 assert run['timeoutSeconds']==1200 and run['memoryLimitBytes']==8589934592
 for name in ['stdout.log','stderr.log']:assert hashlib.sha256(archive.read('measurement/'+name)).hexdigest()==run[name+'Sha256']
 assert read('measurement/CORPUS-BEFORE.json')==read('measurement/CORPUS-AFTER.json')
 identity=read('DIAGNOSTIC-IDENTITY.json');hashes=read('measurement/RUNTIME-HASHES.json')
 assert len(identity['engineHashes'])==9
 assert all(hashes[n]==h for n,h in identity['engineHashes'].items())
 assert all(hashlib.sha256(archive.read('diagnostic/'+n)).hexdigest()==h for n,h in identity['diagnosticSources'].items())
 restored=archive.read('diagnostic/Program.cs').decode('utf-8').replace('\r\n','\n')
 restored=restored.replace('partial class Program','class Program',1)
 restored=restored.replace('        if (args.Length == 1 && args[0] == "--oracle-controls") { AuditRunControls(); return; }\n','',1)
 restored=''.join(line for line in restored.splitlines(True) if not (line.lstrip().startswith(('_auditCase =','_auditPhase =','AuditWrite("normal-result",','AuditException(ex);')) or 'Charles Korthout | 3.82' in line))
 restored=re.sub(r'return AuditMatch\(expectedCode, ex, "[^"]+", (true|false|matches)\);',r'return \1;',restored)
 assert restored==archive.read('original/Program.cs').decode('utf-8').replace('\r\n','\n')
 parity=read('INSTRUMENTATION-VERIFICATION.json')
 assert all(parity[k] for k in ['observerRemovalExactlyRestoresOriginalSource','all579LegacyStatusesIdentical','all66RawSkipReasonsIdentical'])
 controls=read('CONTROLS.json');assert controls['success'] and controls['exitCode']==0 and len(controls['controls'])==16
 for name,digest in controls['hashes'].items():assert hashlib.sha256(archive.read(name)).hexdigest()==digest
 control_events=[json.loads(s[6:]) for s in archive.read('CONTROLS.stderr.log').decode('utf-8-sig').splitlines() if s.startswith('AUDIT ')]
 observed_controls=[e for e in control_events if e['kind']=='control'];assert len(observed_controls)==16
 for event in observed_controls:
  d=event['data'];assert d['legacyExpected']==d['legacyActual'] and classifier.compare(d['expected'],d['error'])==d['exactExpected']
 rows=list(csv.DictReader(io.StringIO(archive.read('outcomes.csv').decode('utf-8'))))
 old=list(csv.DictReader(io.StringIO(archive.read('original/outcomes.csv').decode('utf-8'))))
 assert len(rows)==579 and len({r['test'] for r in rows})==579
 assert {(r['test'],r['status'],r['reason']) for r in rows}=={(r['test'],r['status'],r['reason']) for r in old}
 counts={k:sum(r['status']==k for r in rows) for k in ['Passed','Failed','Skipped']}
 assert counts==report['legacyCounts']=={'Passed':513,'Failed':0,'Skipped':66}
 events=[json.loads(s[6:]) for s in archive.read('measurement/stderr.log').decode('utf-8-sig').splitlines()]
 assert dict(collections.Counter(e['kind'] for e in events))==report['events']=={'exception':504,'match':517,'normal-result':9}
 exceptions=read('exceptions.json');assert len(exceptions)==504
 by_name={r['test']:r for r in rows}
 for item in exceptions:
  row=by_name[item['test']];error=item['exceptionChain'][0]
  assert classifier.combine(item['allowedErrorCodes'],error)==row['catalog_code_assessment']
  assert classifier.identity(error)==item['primaryIdentity']
  raw=[e for e in events if e['caseName']==item['test']]
  caught=[e for e in raw if e['kind']=='exception'];assert len(caught)==1 and caught[0]['data']['chain']==item['exceptionChain']
  accepted=[e['data'] for e in raw if e['kind']=='match' and e['data']['accepted']];assert accepted==[item['acceptedLegacyMatch']]
  assert row['legacy_branch']==accepted[0]['branch'] and row['api_boundary']==caught[0]['phase']
 assert dict(collections.Counter(r['catalog_code_assessment'] for r in rows))==report['catalogCodeAssessments']
 assert report['catalogCodeAssessments']=={'reported-code-exact':445,'skipped':66,'normal-result':9,'uncoded':23,'reported-code-mismatch':31,'qname-exact':5}
 assert report['observedApiBoundaries']=={'compile-call':255,'execute-call':249,'assertion':9}
 assert sum(r['api_boundary']=='execute-call' and r['static_labeled_alternatives_only']=='True' for r in rows)==55
 resolution=read('CANDIDATE-RESOLUTION.json');assert resolution['previousPotentialCandidates']==71 and resolution['observedAliasOrLabelBranches']==54 and resolution['previousCandidatesNotUsingThoseBranches']==17
 assert set(resolution['confirmedNames'])=={r['test'] for r in rows if r['legacy_branch'] in ['legacy-code-alias','legacy-message-label']}
 regex=read('REGEX-REVIEW.json');assert regex['reviewOnly'] and not regex['implementationStarted']
 for case in regex['cases']:
  row=by_name[case['test']];assert row['actual_code']==case['observed'] and row['allowed_error_codes']==case['expected']
  assert hashlib.sha256(archive.read('fixtures/'+case['test']+'.xsl')).hexdigest()==case['fixtureSha256']
 assert not report['normativeDefectVerdicts'] and report['gateUnchanged']
 for label in ['HOSTED-MAIN-DIAGNOSTIC','HOSTED-PR479']:
  receipt=read(label+'.json');assert receipt['success'] and receipt['counts']=={'Passed':10221,'Failed':0,'Skipped':4380}
  assert 'sha256:'+hashlib.sha256(archive.read(label+'.zip')).hexdigest()==receipt['artifact']['digest']
print('Verified all579 unchanged outcomes; 16 controls; 54 observed tolerance branches; 31 differing codes,23 uncoded reports and55 phase-review observations.')
print('No production change, admission or blanket conformance verdict; same-commit full-gate success retained without rewriting the original timeout.')

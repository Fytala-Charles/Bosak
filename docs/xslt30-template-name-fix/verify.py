"""Replay scoped reporting regressions, original-case identities and the unchanged pinned gate."""
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
 assert dict(collections.Counter(e['kind'] for e in events))==report['events']=={'exception':504,'match':521,'normal-result':9}
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
 assert report['catalogCodeAssessments']=={'reported-code-exact':451,'skipped':66,'normal-result':9,'uncoded':23,'reported-code-mismatch':25,'qname-exact':5}
 assert report['observedApiBoundaries']=={'compile-call':255,'execute-call':249,'assertion':9}
 assert sum(r['api_boundary']=='execute-call' and r['static_labeled_alternatives_only']=='True' for r in rows)==55
 comparison=read('CASE-COMPARISON.json')
 assert set(comparison['onlyChangedReportedIdentities'])=={'error-0280d','error-0280f'}
 prior=list(csv.DictReader(io.StringIO(archive.read('previous-observer/outcomes.csv').decode('utf-8'))))
 previous={r['test']:r for r in prior}
 changed={n for n,row in by_name.items() if any(row[k]!=previous[n][k] for k in ['actual_code','identity_kind','identity_origin','catalog_code_assessment','legacy_branch'])}
 assert changed=={'error-0280d','error-0280f'}
 assert set(comparison['casesNewlyMatchingWithoutAliases'])=={'error-0280d','error-0280f'}
 assert len(comparison['alreadyAcceptedAlternativeCases'])==0
 for name,code in [('error-0280d','XTSE0280'),('error-0280f','XTSE0280')]:
  row=by_name[name];assert row['actual_code']==code and row['catalog_code_assessment']=='reported-code-exact' and row['legacy_branch']=='message-code-substring'
 import xml.etree.ElementTree as ET
 definition=archive.read('fixtures/_error-test-set.xml')
 assert hashlib.sha256(definition).hexdigest()==read('measurement/PLAN.json')['sets'][0]['sha256']
 ns={'t':'http://www.w3.org/2012/10/xslt-test-catalog'}
 catalog={c.get('name'):c for c in ET.fromstring(definition).findall('t:test-case',ns)}
 adoption=read('ADOPTION.json')
 for name in changed:
  expected=[e.get('code') for e in catalog[name].find('t:result',ns).iter('{'+ns['t']+'}error')]
  assert ' | '.join(expected)==by_name[name]['allowed_error_codes'] and by_name[name]['actual_code'] in expected
  fixture=archive.read('fixtures/'+name+'.xsl')
  seal=adoption['fixtureSha256'][name+'.xsl']
  assert hashlib.sha256(fixture).hexdigest()==seal
  if name in comparison['alreadyAcceptedAlternativeCases']:
   assert by_name[name]['catalog_code_assessment']==previous[name]['catalog_code_assessment']=='reported-code-exact'
   assert by_name[name]['legacy_branch']==previous[name]['legacy_branch']=='message-code-substring'
 baseline=read('BASELINE-IDENTITY.json')
 assert hashlib.sha256(archive.read('source/TemplateNameErrorCodeTests.cs')).hexdigest()==baseline['regressionSourceSha256']
 review=read('IMPLEMENTATION-REVIEW-2.json');assert review['beforeRevisedImplementation'] and review['noScopeExpansion']
 assert hashlib.sha256(archive.read('CAUGHT-PROBE.log')).hexdigest()==review['caughtProbeSha256']
 for case in exceptions:
  if case['test'] in ['error-0280d','error-0280f']:
   assert len(case['exceptionChain'])==2 and case['exceptionChain'][0]['message'].startswith('XTSE0280:') and case['exceptionChain'][1]['message'].startswith('XPST0081:')
 assert baseline['allNineMatchQualified1817'] and baseline['testExitCode']==1
 assert all(read('baseline-qualified/RUNTIME-HASHES.json')[n]==h for n,h in baseline['engineHashes'].items())
 assert len(baseline['engineHashes'])==9
 def transcript(name):
  data=archive.read(name);return data.decode('utf-16') if data.startswith((b'\xff\xfe',b'\xfe\xff')) else data.decode('utf-8-sig')
 assert re.search(r'Failed:\s+10, Passed:\s+18, Skipped:\s+0',transcript('BASELINE-TEST.log'))
 assert re.search(r'Failed:\s+0, Passed:\s+28, Skipped:\s+0',transcript('FOCUSED-TEST-2.log'))
 tests=read('TEST-SUMMARY.json');assert tests['solutionTests']==26422 and tests['xsltTests']==8397 and tests['languageServerTests']==90
 assert tests['allCasesPassInSingleSolutionInvocation']
 unit=transcript('UNIT.log');projects=re.findall(r'(Passed!|Failed!)\s+- Failed:\s+(\d+), Passed:\s+(\d+), Skipped:\s+(\d+), Total:\s+(\d+).*?- ([\w.]+)\.dll',unit)
 assert len(projects)==10 and sum(int(r[4]) for r in projects)==26422
 assert all(r[0]=='Passed!' and r[1]=='0' and r[3]=='0' for r in projects)
 assert re.search(r'Failed:\s+0, Passed:\s+90, Skipped:\s+0',transcript('LSP.log'))
 for label,passed,skipped in [('QT3',31397,424),('QT4',8941,1918)]:
  text=transcript(label+'.log')
  for key,value in [('Passed',passed),('Failed',0),('Skipped',skipped)]:assert re.search(r'^'+key+r':\s*'+str(value)+r'\s*$',text,re.M)
  assert transcript(label+'-SKIPS.txt').replace('\r\n','\n')==transcript('frozen-baseline/'+label+'-SKIPS.txt').replace('\r\n','\n')
 assert 'Experimental-level (XPath40Experimental) passes: 11' in transcript('QT4.log')
 qualified=read('qualification/GATE.json');full=read('qualification/RUN.json');plan=read('qualification/PLAN.json')
 assert qualified['success'] and full['success'] and full['status']=='Completed' and full['completeCatalogCounts']
 assert full['sourceCommit']==plan['sourceCommit']==qualified['sourceCommit']=='de15d03c18884b4951fa9b9883fce42512fdeba0'
 assert full['corpusCommit']=='fddf1cf920087e791f13315d68dfbe874d97dc56'
 assert full['timeoutSeconds']==1200 and full['memoryLimitBytes']==8589934592
 for name in ['stdout.log','stderr.log']:assert hashlib.sha256(archive.read('qualification/'+name)).hexdigest()==full[name+'Sha256']
 assert read('qualification/CORPUS-BEFORE.json')==read('qualification/CORPUS-AFTER.json')
 assert qualified['runtimeUnchanged'] and qualified['sourceCorpusUnchanged'] and not any(qualified['corpusDifferences'].values())
 spec=importlib.util.spec_from_file_location('gate',here.parents[1]/'tools/xslt30_gate.py');gate=importlib.util.module_from_spec(spec);spec.loader.exec_module(gate)
 policy,_,_,skips=gate.baseline()
 replay=gate.reconcile(plan,full,transcript('qualification/stdout.log'),skips,policy)
 saved=list(csv.DictReader(io.StringIO(archive.read('qualification/outcomes.csv').decode('utf-8'))))
 assert [{k:('' if v is None else str(v)) for k,v in row.items()} for row in replay]==saved and len(saved)==14601
 assert qualified['counts']=={'Passed':10221,'Failed':0,'Skipped':4380}
 candidate=read('qualification/RUNTIME-HASHES.json')
 assert all(candidate[n]==h for n,h in identity['engineHashes'].items())
 source=read('SOURCE-IDENTITY.json')
 for name,entry in source['files'].items():assert hashlib.sha256(archive.read('source/'+name)).hexdigest()==entry['workingBytesSha256']
 assert source['sourceCommit']==full['sourceCommit']
 parent=read('HOSTED-PARENT-PR.json');assert parent['success'] and parent['counts']==qualified['counts']
 assert 'sha256:'+hashlib.sha256(archive.read('HOSTED-PARENT-PR.zip')).hexdigest()==parent['artifact']['digest']
 failure=read('PARENT-MAIN-FAILURE.json');assert failure['run']['status']=='TimedOut' and not failure['run']['success'] and not failure['gate']['success']
 assert failure['run']['timeoutSeconds']==1200 and failure['run']['memoryLimitBytes']==8589934592 and not failure['observedFailLines']
 assert failure['noRetryLaunched'] and 'sha256:'+hashlib.sha256(archive.read('PARENT-MAIN-FAILURE.zip')).hexdigest()==failure['artifact']['digest']
 assert read('ADOPTION.json')['beforeImplementation'] and read('ADOPTION.json')['intentionalDeviations']==[]
print('Verified 28 reporting tests and 10-failure/18-pass baseline; all 26,422 solution cases and 90 LSP; exact frozen gates; unchanged 14,601-case standalone gate; two original cases report specific codes without aliases.')
print('Remaining 25 code differences and 23 uncoded reports require review. No case admission, exclusion change, package publication or complete-conformance claim.')

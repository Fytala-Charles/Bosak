from pathlib import Path
import json,hashlib,zipfile,base64,subprocess,shutil,xml.etree.ElementTree as E
r=Path('D:/Development/Bosak');t=r/'.guard-tmp';s=t/'release1262-source';e=t/'release1262-candidate';d=r/'docs/release-evidence/1.26.2';d.mkdir(parents=True,exist_ok=True)
rev='6a3372fc3b91e733b71a426cf40f26feaaeb3bcb';publicrev='65417cdb3bff184ca9ff1f624582f55ca4681db9'
def git(*args,cwd=s):return subprocess.check_output(['git',*args],cwd=cwd,text=True).strip()
def load(p):
 b=p.read_bytes();return json.loads(b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
assert git('rev-parse','HEAD')==rev and not git('status','--porcelain')
assert not git('diff',publicrev,rev,'--','src/Bosak.Xslt')
plan=load(e/'source-plan.json');assert plan['immutableSource']==rev and plan['sourceTree']==git('rev-parse','HEAD^{tree}')
assert all(x['exitCode']==0 for x in load(e/'validation-exits.json'))
results={};names={}
with zipfile.ZipFile(d/'test-receipts.zip','w',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
 for lane,expected,count in [('full',18737,10),('lsp',84,1),('focused-results',8114,1)]:
  rows=[];names[lane]=[];files=sorted((e/lane).glob('*.trx'));assert len(files)==count
  for p in files:
   xml=E.parse(p);c=xml.find('.//{*}Counters');counts={k:int(c.get(k,0)) for k in ['total','passed','failed','notExecuted']}
   assert counts['total']==counts['passed'] and counts['failed']==counts['notExecuted']==0
   names[lane].extend(n.get('testName','') for n in xml.findall('.//{*}UnitTestResult'))
   rows.append(dict(path=p.relative_to(r).as_posix(),sha256=sha(p),**counts));z.writestr(lane+'/'+p.name,p.read_bytes())
  assert sum(x['passed'] for x in rows)==expected
  results[lane]=dict(passed=expected,failed=0,skipped=0,receipts=rows)
coverage=[]
for row in plan['addedSourceFiles']:
 name=Path(row['file']).stem;a=sum('.'+name+'.' in n for n in names['full']);b=sum('.'+name+'.' in n for n in names['focused-results'])
 assert a==b==row['expectedCases'] and sha(s/row['source'])==sha(e/'focused'/row['file'])==row['sha256']
 coverage.append(dict(**row,sourcePassed=a,packagePassed=b))
assert sum(x['sourcePassed'] for x in coverage)==4201
assert sum('AuthoringConditionalWrapperTests.' in n for n in names['focused-results'])==47
assert sum('BraidConditionalProbes.' in n for n in names['focused-results'])==17
retained=[]
for row in load(t/'release1261-candidate/source-plan.json')['addedSourceFiles']:
 assert sha(s/row['source'])==sha(e/'focused'/row['file'])==row['sha256']
 assert sum('.'+Path(row['file']).stem+'.' in n for n in names['focused-results'])==row['expectedCases']
 retained.append(row)
identities=[]
for kind,count in [('broad',10),('focused',9)]:
 meta=load(e/(kind+'-metadata.json'));assert len(meta)==count
 assets=load(e/kind/'obj/project.assets.json');assert all(x['type']!='project' for x in assets['libraries'].values())
 assert Path(assets['project']['restore']['packagesPath']).resolve()==(e/(kind+'-cache')).resolve()
 project=E.parse(next((e/kind).glob('*.csproj')));assert not project.findall('.//ProjectReference') and not project.findall('.//Reference')
 for m in meta:
  assert m['version']=='1.26.2.0' and m['informationalVersion']=='1.26.2+'+rev
  pid=m['id'];cache=e/(kind+'-cache')/pid.lower()/'1.26.2';archive=t/'release1262-packages'/(pid+'.1.26.2.nupkg');metadata=load(cache/'.nupkg.metadata')
  assert Path(metadata['source']).resolve()==(t/'release1262-packages').resolve()
  hash512=base64.b64encode(hashlib.sha512(archive.read_bytes()).digest()).decode()
  assert assets['libraries'][pid+'/1.26.2']['sha512']==metadata['contentHash']==hash512
  assert (cache/(pid.lower()+'.1.26.2.nupkg.sha512')).read_text(encoding='utf-8-sig').strip()==hash512
  assert (cache/archive.name.lower()).read_bytes()==archive.read_bytes()
  with zipfile.ZipFile(archive) as z:
   assert '.signature.p7s' not in z.namelist();dll=z.read('lib/net10.0/'+pid+'.dll')
   assert hashlib.sha256(dll).hexdigest()==m['sha256']
   assert dll==(cache/'lib/net10.0'/(pid+'.dll')).read_bytes()==(e/kind/'bin/Release/net10.0'/(pid+'.dll')).read_bytes()
   assert E.fromstring(z.read('lib/net10.0/'+pid+'.xml')).findtext('assembly/name')==pid
  identities.append(dict(consumer=kind,**m,source=metadata['source'],unsignedArchiveSha512=hash512,archiveCacheOutputMatch=True))
 audit=load(e/(kind+'-audit.json'));assert audit['projects'] and not audit.get('problems') and not audit.get('errors')
 assert not any(f.get('topLevelPackages') or f.get('transitivePackages') for p in audit['projects'] for f in p.get('frameworks',[]))
 raw=(e/(kind+'-restore.log')).read_bytes();text=raw.decode('utf-16' if raw.startswith(b'\xff\xfe') else 'utf-8-sig');assert 'NU1900' not in text
packages=load(e/'package-verification.json');assert len(packages['packages'])==10 and packages['consumerChecks']['focused']==8114
gate=load(e/'gate-verification.json');assert gate['base']==rev and (e/'frozen-gate.log').read_text(encoding='utf-8-sig').count('  Starting:')==224
preflight=load(e/'public-package-preflight.json');assert len(preflight)==10 and all(x['httpStatus']==200 and x['public1261Present'] and not x['candidate1262Present'] for x in preflight)
vuln=load(e/'nuget-preflight.json');assert all(x['httpStatus']==200 and not x.get('error') for x in vuln)
fixtures={name:git('rev-parse','HEAD',cwd=r/path) for name,path in [('qt3','tests/qt3tests'),('frozenQt4','tests/qt4tests'),('currentQt4','.upstream/qt4tests')]}
for p in e.iterdir():
 if p.is_file() and p.name not in ['verified-proof.json']:shutil.copyfile(p,d/p.name)
shutil.copytree(e/'public-indexes',d/'public-indexes',dirs_exist_ok=True)
for kind in ['broad','focused']:
 shutil.copyfile(e/(kind+'-audit.json'),d/(kind+'-audit.raw'))
 (d/(kind+'-audit.json')).write_text(json.dumps(load(e/(kind+'-audit.json')),indent=2)+'\n',encoding='utf-8')
(d/'test-results.json').write_text(json.dumps(results,indent=2)+'\n',encoding='utf-8')
(d/'added-case-coverage.json').write_text(json.dumps(coverage,indent=2)+'\n',encoding='utf-8')
with zipfile.ZipFile(d/'package-consumer-source.zip','w',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
 for kind in ['broad','focused','metadata']:
  for p in sorted((e/kind).rglob('*')):
   if p.is_file() and not any(x in ['bin','obj','TestResults'] for x in p.relative_to(e/kind).parts):z.writestr(p.relative_to(e).as_posix(),p.read_bytes())
for name,target in [('setup-release1262-candidate.py','setup-candidate.py'),('correct-release1262-consumer-headers.py','retarget-consumers.py'),('build-release1262-candidate.py','build-and-test-source.py'),('test-release1262-packages.ps1','run-package-consumers.ps1'),('verify-release1262-gates.py','run-gates.py'),('audit-release1262-candidate.py','verify-package-archives.py'),('finalize-release1262-proof.py','verify-candidate-proof.py')]:shutil.copyfile(t/name,d/target)
proof=dict(date='2026-10-26',status='CANDIDATE_VALIDATED_UNPUBLISHED',version='1.26.2',publicVersion='1.26.1',sourceRevision=rev,sourceTree=plan['sourceTree'],isolatedSourceClean=True,sourceTests=results['full'],languageServerTests=results['lsp'],sourceTotalIncludingLanguageServer=18821,packageOnlyConsumers=dict(broadPassed=153,focused=results['focused-results'],retainedFocusedCases=3913,addedPublicCases=4201,sourceFilesCopiedByteIdentically=coverage,retained1261SourceFilesByteIdentical=retained,noProjectOrBinaryReferences=True,freshSeparateCaches=True,countsOverlapSourceTests=True,publicConditionalCases=47,independentBraidConditionalProbes=17),packages=packages['packages'],assemblyIdentities=identities,conformance=gate,fixtureRevisions=fixtures,officialNugetPreflight=preflight,vulnerabilityIndexPreflight=vuln,dependencyAudits='Neither fresh consumer audit reports vulnerable packages; official vulnerability index HTTP200; no NU1900.',consumerPlan=plan,artifacts=dict(packages=str(t/'release1262-packages'),consumers=str(e),consumerSourceArchive='docs/release-evidence/1.26.2/package-consumer-source.zip'),receiptArchive='docs/release-evidence/1.26.2/test-receipts.zip',publicationBoundary=dict(unsignedLocalPackages=True,tagCreated=False,githubReleaseCreated=False,nugetPushPerformed=False,publicSignatureVerificationPerformed=False,remoteCiVerified=False),req126='Partial; D2/D3/D4/applicable-evidence AC-03 remain open',braid='Owner reports smoke completed; outcome not supplied; no new acceptance inferred',authoringImplementationUnchangedSince1261=True,priorDiagnosticCorrection=dict(actualRoundingDiscrepancies=600,invalidExpectations=12,receipt='docs/xpath40-catalog/evidence/rounding-kernel/historical-oracle-correction.json'),knownLimits=['Fixed-precision .NET decimal and decimal-backed integer storage remain bounded.','General wide string constructors/casts and wide abs remain separate.','Existing Int64 idiv result bounds remain.','Schema-list cardinality and generalized function metadata/coercion/BC-dynamic conversion remain separate.','DateTimeOffset-based Unix calendar remains years 1-9999; extended-year construction is a separate capability.','Current native Unix remains 22/2/0 with known negative-domain cases 120/121; no full current-draft conformance.','Unsigned package consumers do not establish signed public packages, Braid integration or native acceptance.'])
if (e/'documentation-render-receipts.json').exists():
 proof['documentationRender']=load(e/'documentation-render-receipts.json');assert len(proof['documentationRender']['pages'])==11
proof['validationCommands']=[dict(phase='source-build',cwd=str(s),command=['dotnet','build','Bosak.sln','-c','Release']),dict(phase='solution-tests',cwd=str(s),command=['dotnet','test','Bosak.sln','-c','Release','--no-build','--logger','trx','--results-directory',str(e/'full')]),dict(phase='lsp-tests',cwd=str(s),command=['dotnet','test','tests/Bosak.LanguageServer.Tests','-c','Release','--logger','trx','--results-directory',str(e/'lsp')]),dict(phase='pack',cwd=str(s),command=['dotnet','pack','Bosak.sln','-c','Release','--no-build','-o',str(t/'release1262-packages')]),dict(phase='consumers-and-audits',cwd=str(r),command=['powershell','-NoProfile','-ExecutionPolicy','Bypass','-File','.guard-tmp/test-release1262-packages.ps1'])]
proof['evidenceHashes']={p.relative_to(d).as_posix():sha(p) for p in sorted(d.rglob('*')) if p.is_file()}
(r/'docs/RELEASE-1.26.2-CANDIDATE.json').write_text(json.dumps(proof,indent=2)+'\n',encoding='utf-8');(e/'verified-proof.json').write_text(json.dumps(proof,indent=2)+'\n',encoding='utf-8')
print('Candidate verified: 18,821 source/LSP, 153 broad /8,114 focused, ten packages /19 identities, unchanged frozen gates.')

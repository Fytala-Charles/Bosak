from pathlib import Path
import json,hashlib,zipfile,base64,subprocess,shutil,xml.etree.ElementTree as E
r=Path('D:/Development/Bosak');t=r/'.guard-tmp';s=t/'release1261-source';e=t/'release1261-candidate';rev='65417cdb3bff184ca9ff1f624582f55ca4681db9'
dest=r/'docs/release-evidence/1.26.1';assert not dest.exists() or not any(dest.iterdir());dest.mkdir(parents=True,exist_ok=True)
def git(*args,cwd=s):return subprocess.check_output(['git',*args],cwd=cwd,text=True).strip()
def load(p):
 raw=p.read_bytes();return json.loads(raw.decode('utf-16' if raw.startswith(b'\xff\xfe') else 'utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
assert git('rev-parse','HEAD')==rev and not git('status','--porcelain')
assert not git('diff','1f06efb4d938d3d8e12eb273e632a8fe15110041',rev,'--','src/Bosak.Xslt')
plan=load(e/'source-plan.json');assert plan['immutableSource']==rev and plan['sourceTree']==git('rev-parse','HEAD^{tree}')
results={};names={}
for folder,expected,fileCount in [('full',14536,10),('lsp',84,1),('focused-results',3913,1)]:
 rows=[];seen=[];files=sorted((e/folder).glob('*.trx'));assert len(files)==fileCount
 for p in files:
  tree=E.parse(p);c=tree.find('.//{*}Counters');counts={k:int(c.get(k,0)) for k in ['total','passed','failed','notExecuted']}
  assert counts['total']==counts['passed'] and counts['failed']==counts['notExecuted']==0
  seen.extend(n.get('testName','') for n in tree.findall('.//{*}UnitTestResult'))
  rows.append(dict(path=str(p.relative_to(r)).replace('\\','/'),sha256=sha(p),**counts))
 assert sum(x['passed'] for x in rows)==expected
 results[folder]=dict(passed=expected,failed=0,skipped=0,receipts=rows);names[folder]=seen
coverage=[]
for row in plan['addedSourceFiles']:
 name=Path(row['file']).stem;sourceCount=sum('.'+name+'.' in n for n in names['full']);packageCount=sum('.'+name+'.' in n for n in names['focused-results'])
 assert sourceCount==packageCount==row['expectedCases'],(name,sourceCount,packageCount)
 assert sha(s/row['source'])==sha(e/'focused'/row['file'])==row['sha256']
 coverage.append(dict(**row,sourcePassed=sourceCount,packagePassed=packageCount))
assert sum(x['sourcePassed'] for x in coverage)==2420
assert sum('AuthoringConditionalWrapperTests.' in n for n in names['full'])==57
assert sum('AuthoringConditionalWrapperTests.' in n for n in names['focused-results'])==47
assert sum('BraidConditionalProbes.' in n for n in names['focused-results'])==17
identities=[]
for kind,count in [('broad',10),('focused',9)]:
 meta=load(e/(kind+'-metadata.json'));assert len(meta)==count
 assets=load(e/kind/'obj/project.assets.json');assert all(x['type']!='project' for x in assets['libraries'].values())
 assert Path(assets['project']['restore']['packagesPath']).resolve()==(e/(kind+'-cache')).resolve()
 project=next((e/kind).glob('*.csproj'));tree=E.parse(project)
 assert not tree.findall('.//ProjectReference') and not tree.findall('.//Reference')
 for m in meta:
  assert m['version']=='1.26.1.0' and m['informationalVersion']=='1.26.1+'+rev
  pid=m['id'];cache=e/(kind+'-cache')/pid.lower()/'1.26.1';archive=t/'release1261-packages'/(pid+'.1.26.1.nupkg');metadata=load(cache/'.nupkg.metadata')
  assert Path(metadata['source']).resolve()==(t/'release1261-packages').resolve()
  hash512=base64.b64encode(hashlib.sha512(archive.read_bytes()).digest()).decode()
  assert assets['libraries'][pid+'/1.26.1']['sha512']==metadata['contentHash']==hash512
  assert (cache/(pid.lower()+'.1.26.1.nupkg.sha512')).read_text(encoding='utf-8-sig').strip()==hash512
  assert (cache/archive.name.lower()).read_bytes()==archive.read_bytes()
  with zipfile.ZipFile(archive) as z:
   assert '.signature.p7s' not in z.namelist()
   dll=z.read('lib/net10.0/'+pid+'.dll');assert hashlib.sha256(dll).hexdigest()==m['sha256']
   assert dll==(cache/'lib/net10.0'/(pid+'.dll')).read_bytes()==(e/kind/'bin/Release/net10.0'/(pid+'.dll')).read_bytes()
   assert E.fromstring(z.read('lib/net10.0/'+pid+'.xml')).findtext('assembly/name')==pid
  identities.append(dict(consumer=kind,**m,source=metadata['source'],unsignedArchiveSha512=hash512,archiveCacheOutputMatch=True))
 audit=load(e/(kind+'-audit.json'));assert audit['projects'] and not audit.get('problems') and not audit.get('errors')
 assert not any(f.get('topLevelPackages') or f.get('transitivePackages') for p in audit['projects'] for f in p.get('frameworks',[]))
 raw=(e/(kind+'-restore.log')).read_bytes();restore=raw.decode('utf-16' if raw.startswith(b'\xff\xfe') else 'utf-8-sig');assert 'NU1900' not in restore
packages=load(e/'package-verification.json');assert len(packages['packages'])==10 and packages['consumerChecks']['focused']==3913
gate=load(e/'gate-verification.json');assert gate['base']==rev and (e/'frozen-gate.log').read_text(encoding='utf-8-sig').count('  Starting:')==224
preflight=load(e/'public-package-preflight.json');assert len(preflight)==10 and all(x['httpStatus']==200 and x['public1260Present'] and not x['candidate1261Present'] for x in preflight)
vuln=load(e/'nuget-preflight.json');assert all(x['httpStatus']==200 and not x.get('error') for x in vuln)
fixtures={name:git('rev-parse','HEAD',cwd=r/path) for name,path in [('qt3','tests/qt3tests'),('frozenQt4','tests/qt4tests'),('currentQt4','.upstream/qt4tests')]}
for name in ['source-plan.json','consumer-plan.json','package-verification.json','gate-verification.json','nuget-preflight.json','public-package-preflight.json','vulnerability-index.json','public-xslt-versions.json','validation-exits.json']:
 shutil.copyfile(e/name,dest/name)
shutil.copytree(e/'public-indexes',dest/'public-indexes')
for name in ['build.log','source-tests.log','lsp-tests.log','pack.log','qt3.log','frozen-gate.log','frozen-gate-skips.txt','current-native-after.log','broad-restore.log','focused-restore.log','broad-run.log','focused-tests.log','broad-metadata.json','focused-metadata.json','broad-inline-metadata.json']:
 shutil.copyfile(e/name,dest/name)
for kind in ['broad','focused']:(dest/(kind+'-audit.json')).write_text(json.dumps(load(e/(kind+'-audit.json')),indent=2)+'\n')
(dest/'test-results.json').write_text(json.dumps(results,indent=2)+'\n')
(dest/'added-case-coverage.json').write_text(json.dumps(coverage,indent=2)+'\n')
with zipfile.ZipFile(dest/'package-consumer-source.zip','w',compression=zipfile.ZIP_DEFLATED,compresslevel=9) as z:
 for kind in ['broad','focused','metadata']:
  for p in sorted((e/kind).rglob('*')):
   if p.is_file() and not any(x in ['bin','obj','TestResults'] for x in p.relative_to(e/kind).parts):z.writestr(p.relative_to(e).as_posix(),p.read_bytes())
shutil.copyfile(t/'audit-release1261-candidate.py',dest/'verify-package-archives.py')
shutil.copyfile(Path(__file__),dest/'verify-candidate-proof.py')
proof=dict(date='2026-10-26',status='CANDIDATE_VALIDATED_UNPUBLISHED',version='1.26.1',publicVersion='1.26.0',sourceRevision=rev,sourceTree=plan['sourceTree'],isolatedSourceClean=True,sourceTests=results['full'],languageServerTests=results['lsp'],sourceTotalIncludingLanguageServer=14620,packageOnlyConsumers=dict(broadPassed=153,focused=results['focused-results'],retainedFocusedCases=1493,addedPublicCases=2420,sourceFilesCopiedByteIdentically=coverage,noProjectOrBinaryReferences=True,freshSeparateCaches=True,countsOverlapSourceTests=True,publicConditionalCases=47,independentBraidConditionalProbes=17),packages=packages['packages'],assemblyIdentities=identities,conformance=gate,fixtureRevisions=fixtures,officialNugetPreflight=preflight,vulnerabilityIndexPreflight=vuln,dependencyAudits='Neither fresh consumer audit reports vulnerable packages; official vulnerability index HTTP200; no NU1900.',consumerPlan=plan,artifacts=dict(packages=str(t/'release1261-packages'),consumers=str(e),consumerSourceArchive='docs/release-evidence/1.26.1/package-consumer-source.zip'),publicationBoundary=dict(unsignedLocalPackages=True,tagCreated=False,githubReleaseCreated=False,nugetPushPerformed=False,publicSignatureVerificationPerformed=False,remoteCiVerified=False),req126='Partial; D2/D3/D4/applicable-evidence AC-03 remain open',req138='Public 1.26.0 bounded Braid acceptance remains unchanged; no 1.26.1 Braid adoption/native acceptance inferred.',authoringImplementationUnchangedSince1260=True,knownLimits=['Fixed-precision .NET decimal (28-29 significant digits).','Generic wide-integer storage/casts, wide abs and extreme rounding precision are not certified by these fixes.','Existing Int64 idiv result and precision payload bounds remain.','Schema-list cardinality and generalized function metadata/coercion/BC-dynamic conversion remain separate.','Early date/duration and backwards-compatible arithmetic dispatch are outside the empty numeric/range correction.','DateTimeOffset-based Unix calendar remains years 1-9999; extended-year construction is a separate capability.','Current native Unix remains 22/2/0 with known negative-domain cases 120/121; no full current-draft conformance.','Unsigned package consumers do not establish signed public packages, Braid integration or native acceptance.'])
proof['evidenceHashes']={p.relative_to(dest).as_posix():sha(p) for p in sorted(dest.rglob('*')) if p.is_file()}
(r/'docs/RELEASE-1.26.1-CANDIDATE.json').write_text(json.dumps(proof,indent=2)+'\n')
(e/'verified-proof.json').write_text(json.dumps(proof,indent=2)+'\n')
print('Candidate verified: 14,620 source/LSP, 153 broad /3913 focused, ten packages /19 consumed identities, both gates unchanged.')

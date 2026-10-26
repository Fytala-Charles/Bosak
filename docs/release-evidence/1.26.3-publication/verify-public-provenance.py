from pathlib import Path
import json,hashlib,base64,zipfile,subprocess,xml.etree.ElementTree as E
r=Path('D:/Development/Bosak/.guard-tmp'); p=r/'release1263-publication'
def read(f):
 b=f.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig')
def sha(b):return hashlib.sha256(b).hexdigest()
attempt=Path(json.loads(read(p/'final-attempt.json'))['directory'])
records={x['id']:x for x in json.loads(read(p/'public-downloads.json'))}
hashes={x['archive']:x['contentHash'] for x in json.loads(read(p/'nuget-content-hashes.json'))}
proof={}
for kind,expected in [('broad-public',10),('focused-public',9)]:
 project=attempt/kind; cache=attempt/(kind+'-cache')
 assets=json.loads(read(project/'obj/project.assets.json'));assert all(x['type']!='project' for x in assets['libraries'].values())
 rows=[]
 for key,lib in assets['libraries'].items():
  pid=key.split('/')[0]
  if pid not in records:continue
  assert key==pid+'/1.26.3' and lib['type']=='package'
  loc=cache/lib['path'];meta=json.loads(read(loc/'.nupkg.metadata'));assert meta['source']=='https://api.nuget.org/v3/index.json'
  arc=loc/(pid.lower()+'.1.26.3.nupkg');b=arc.read_bytes();assert sha(b)==records[pid]['sha256']
  raw=base64.b64encode(hashlib.sha512(b).digest()).decode();assert read(arc.with_name(arc.name+'.sha512')).strip()==raw
  assert hashes[pid+'.1.26.3.nupkg']==lib['sha512']==meta['contentHash']
  with zipfile.ZipFile(arc) as z:
   dll=z.read('lib/net10.0/'+pid+'.dll');assert sha(dll)==records[pid]['assemblySha256']
   assert dll==(loc/('lib/net10.0/'+pid+'.dll')).read_bytes()==(project/('bin/Release/net10.0/'+pid+'.dll')).read_bytes()
  rows.append({'package':key,'source':meta['source'],'archiveSha256':sha(b),'rawSignedArchiveSha512':raw,'nugetContentHash':lib['sha512'],'archiveCacheOutputMatch':True})
 assert len(rows)==expected
 versions=json.loads(read(p/(kind+'-versions.json')));assert len(versions)==expected
 assert all(v['version']=='1.26.3.0' and v['informationalVersion']=='1.26.3+08fbdc31ac79baac04cf00f4773ec6d0b2dd247f' and v['sha256']==records[v['id']]['assemblySha256'] for v in versions)
 audit=json.loads(read(p/(kind+'-audit.json')));assert audit['version']==1 and audit.get('projects') and not audit.get('problems')
 assert not any(f.get('topLevelPackages') or f.get('transitivePackages') for pr in audit['projects'] for f in pr.get('frameworks',[]))
 proof[kind]={'packages':rows,'assemblyMetadata':versions,'reportedVulnerabilities':0}
assert '153 checks passed' in read(attempt/'broad-public-run.log')
trx=E.parse(next((attempt/'focused-results').glob('*.trx')));ns={'t':'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'};c=trx.find('.//t:Counters',ns);assert c.get('total')==c.get('passed')=='11175' and c.get('failed')=='0'
modeCases=[e for e in trx.iter() if e.tag.endswith('UnitTestResult') and 'FunctionApplyTemplatesModeTests.' in e.get('testName','')]
assert len(modeCases)==36 and all(e.get('outcome')=='Passed' for e in modeCases)
lifecycleCases=[x for x in trx.iter() if x.tag.endswith('UnitTestResult') and 'AuthoringLiteralElementLifecycleTests.' in x.get('testName','')]
assert len(lifecycleCases)==73 and all(x.get('outcome')=='Passed' for x in lifecycleCases)
reorder=[x for x in trx.iter() if x.tag.endswith('UnitTestResult') and 'AuthoringLiteralElementReorderingTests.' in x.get('testName','')]
assert len(reorder)==71 and all(x.get('outcome')=='Passed' for x in reorder)
substring=[x for x in trx.iter() if x.tag.endswith('UnitTestResult') and 'SubstringExperimental' in x.get('testName','')]
assert len(substring)==40 and all(x.get('outcome')=='Passed' for x in substring)
proof.update(publicReorderTests=71,publicSubstringTests=40,publicElementLifecycleTests=73,version='1.26.3',revision='08fbdc31ac79baac04cf00f4773ec6d0b2dd247f',broadChecks=153,publicFocusedTests=11175,publicLifecycleTests=117,publicSwitchTests=32,publicIndexOfTests=27,publicPriorityTests=96,publicEqNameBracketTests=44,publicRecordGateTests=44,publicNumericBoundaryTests=34,publicFunctionDefaultModeTests=36,nugetOrgOnly=True,receiptDirectory=str(attempt))

newClasses={'UnixDateTimeContractTests':52,'UnixDateTimeExperimentalTests':25,'UnixDateTimeExperimentalHostBoundaryTests':4,'UnixDateTimeWideIntegerBoundaryTests':24,'IntegerPromotionTests':42,'UnixDateTimeWideRangeTests':32,'IntegerCastContractTests':72,'FloatingIntegerBoundaryTests':72,'UnsignedDecimalCastTests':48,'UnsignedFloatingCastTests':60}
for name,count in newClasses.items():
 cases=[x for x in trx.iter() if x.tag.endswith('UnitTestResult') and name+'.' in x.get('testName','')]
 assert len(cases)==count and all(x.get('outcome')=='Passed' for x in cases),(name,len(cases))
proof['newNumericUnixCases']=newClasses


conditional=[x for x in trx.iter() if x.tag.endswith('UnitTestResult') and 'AuthoringConditionalWrapperTests.' in x.get('testName','')]
independent=[x for x in trx.iter() if x.tag.endswith('UnitTestResult') and 'BraidConditionalProbes.' in x.get('testName','')]
assert len(conditional)==47 and len(independent)==17 and all(x.get('outcome')=='Passed' for x in conditional+independent)
proof['publicConditionalCases']=47;proof['independentConditionalProbes']=17;proof['excludedInternalSourceCases']=10
newArithmetic={'IntegerUnaryTests':48,'DivisionModuloAuditTests':72,'IntegerModuloTests':84,'IntegerDivisionRangeAuditTests':48,'ExactIntegerDivisionTests':132}
for name,count in newArithmetic.items():
 cases=[x for x in trx.iter() if x.tag.endswith('UnitTestResult') and name+'.' in x.get('testName','')]
 assert len(cases)==count and all(x.get('outcome')=='Passed' for x in cases),(name,len(cases))
proof['carriedForwardNumericCases']=newArithmetic
proof['focusedComposition']={'retainedAuthoringPatternBaseline':614,'numericAndUnixCases':815,'conditionalCases':64,'countsOverlapSourceAndConsumerRuns':True}


added={'UnaryCoercionTests':79,'UnaryEmptyAtomizationTests':64,'ArrayAtomizationAuditTests':72,'ArraySingletonAtomizationTests':111,'BinaryEmptyAtomizationTests':214,'AtomicFunctionConversionTests':366,'RoundingFunctionConversionTests':1514}
for name,count in added.items():
 cases=[x for x in trx.iter() if x.tag.endswith('UnitTestResult') and name+'.' in x.get('testName','')]
 assert len(cases)==count and all(x.get('outcome')=='Passed' for x in cases),(name,len(cases))
proof['addedPublicCases']=added
proof['focusedComposition']={'retainedPublicCases':1493,'addedAtomicConversionCases':2420,'countsOverlapSourceAndConsumerRuns':True}


rounding={'RoundingPrecisionBoundaryTests':1321,'RoundingKernelTests':2880}
for name,count in rounding.items():
 cases=[x for x in trx.iter() if x.tag.endswith('UnitTestResult') and name+'.' in x.get('testName','')]
 assert len(cases)==count and all(x.get('outcome')=='Passed' for x in cases),(name,len(cases))
proof['addedRoundingPublicCases']=rounding
proof['focusedComposition']={'retainedPublicCases':3913,'addedRoundingCases':4201,'countsOverlapSourceAndConsumerRuns':True}


integerCases={'AbsIntegerPreservationTests':1323,'FloorCeilingIntegerPreservationTests':936,'FloorCeilingArgumentContractTests':802}
for name,count in integerCases.items():
 cases=[x for x in trx.iter() if x.tag.endswith('UnitTestResult') and name+'.' in x.get('testName','')]
 assert len(cases)==count and all(x.get('outcome')=='Passed' for x in cases),(name,len(cases))
proof['addedIntegerPublicCases']=integerCases
proof['focusedComposition']={'retainedPublicCases':8114,'addedIntegerPreservationAndAuditCases':3061,'countsOverlapSourceAndConsumerRuns':True}

(p/'public-consumer-provenance.json').write_text(json.dumps(proof,indent=2)+'\n',encoding='utf-8')
print('Verified 153 broad /11175 focused; 71 reorder /40 substring cases; all signed archive/cache/output/assembly identities, NuGet content hashes and audits match.')


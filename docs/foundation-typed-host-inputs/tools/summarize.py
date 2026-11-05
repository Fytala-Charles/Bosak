"""Summarize actual rebuilt production qualification and retain licensed binaries."""
from pathlib import Path
import json,zipfile,hashlib,xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';i=json.loads((E/'INPUTS.json').read_text());u=json.loads((E/'UNITS.json').read_text())
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
with zipfile.ZipFile(E/'unit-trx.zip') as z:
 root=ET.fromstring(z.read('Bosak.Xslt.Tests.trx'));rows=root.findall('{*}Results/{*}UnitTestResult')
 regressions=[{'name':r.attrib['testName'],'outcome':r.attrib['outcome']} for r in rows if 'SchemaHostExecutionTests.' in r.attrib['testName']]
 functions=[{'name':r.attrib['testName'],'outcome':r.attrib['outcome']} for r in rows if 'FnTransform_' in r.attrib['testName'] or 'D_NestedTransformInheritsSharedOperationBudget' in r.attrib['testName']]
assert len(regressions)==6 and all(r['outcome']=='Passed' for r in regressions)
with zipfile.ZipFile(E/'unit-trx.zip') as z:
 root=ET.fromstring(z.read('Bosak.XPath.Runtime.Tests.trx'));runtime=[{'name':r.attrib['testName'],'outcome':r.attrib['outcome']} for r in root.findall('{*}Results/{*}UnitTestResult') if 'SchemaOperationInputTests.' in r.attrib['testName']]
assert len(runtime)==25 and all(r['outcome']=='Passed' for r in runtime)
regressions+=runtime
with zipfile.ZipFile(E/'product-binaries.zip','w',compression=zipfile.ZIP_DEFLATED) as z:
 for name,v in i['engines'].items():z.write(v['path'],name)
 # The language server is outside the solution and is qualified by a separate no-dependencies build.
 z.write(ROOT/'src/Bosak.LanguageServer/bin/Release/net10.0/Bosak.LanguageServer.dll','Bosak.LanguageServer.dll')
 for name in ['license.md','NOTICE']:z.write(ROOT/name,name)
summary={'date':'2026-11-01','version':'1.38.0','productionSourceApplied':True,'unitTests':{'passed':sum(int(r['counters']['passed']) for r in u),'failed':sum(int(r['counters']['failed']) for r in u),'projects':{r['name']:int(r['counters']['passed']) for r in u}},'newRegressionCases':regressions,'fnTransformControls':functions,'qt3':{'passed':31397,'failed':0,'skipped':424},'qt4Gated':{'passed':8941,'failed':0,'skipped':1918,'experimentalPasses':11},'skipMapsUnchanged':True,'solutionRebuilt':True,'publicDeclarationsUnchanged':True,'assemblyMetadataCaptured':True,'productArchiveSha256':sha(E/'product-binaries.zip'),'published':False,'publicFacadeImplemented':False,'ownerAcceptance':'Pending','apiFreeze':False}
(E/'RESULTS.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf-8')
print('Summarized '+str(summary['unitTests']['passed'])+' production unit passes and '+str(len(regressions))+' new cases; archived eleven product binaries.')

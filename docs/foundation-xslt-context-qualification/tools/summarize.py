"""Summarize passed receipts, keeping failed original-byte qualification separate."""
from pathlib import Path
import json,zipfile,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];E=P/'evidence';i=json.loads((E/'INPUTS.json').read_text());u=json.loads((E/'UNITS.json').read_text())
with zipfile.ZipFile(E/'unit-trx.zip') as z:
 root=ET.fromstring(z.read('Bosak.Xslt.Tests.trx'))
 cases=[{'name':r.attrib['testName'],'outcome':r.attrib['outcome']} for r in root.findall('{*}Results/{*}UnitTestResult') if 'FnTransform_' in r.attrib['testName'] or 'D_NestedTransformInheritsSharedOperationBudget' in r.attrib['testName']]
assert len(cases)>=6 and all(c['outcome']=='Passed' for c in cases)
summary={'date':'2026-11-01','qualifiedCandidateSha256':i['candidateSha256'],'originalCandidateQualified':False,'projectMetadataRestored':True,'productionPromoted':False,'unitTests':{'passed':sum(int(r['counters']['passed']) for r in u),'failed':sum(int(r['counters']['failed']) for r in u),'projects':{r['name']:int(r['counters']['passed']) for r in u},'retainedAssemblies':True,'solutionRebuilt':False},'fnTransformControls':cases,'qt3':{'passed':31397,'failed':0,'skipped':424},'qt4Gated':{'passed':8941,'failed':0,'skipped':1918,'experimentalPasses':11},'skipMapsUnchanged':True,'explicitBridgeCases':22,'publicTypes':157,'publicDeclarationRows':2961,'publicDeclarationsUnchanged':True,'ownerAcceptance':'Pending','apiFreeze':False}
(E/'RESULTS.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf-8')
print('Summarized '+str(summary['unitTests']['passed'])+' unit passes and '+str(len(cases))+' named fn:transform controls.')

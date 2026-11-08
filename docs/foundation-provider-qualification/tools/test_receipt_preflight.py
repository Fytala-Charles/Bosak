"""Synthetic structural checker controls, not owner/native workload receipts."""
import copy,hashlib,json,tempfile,unittest
from pathlib import Path
from receipt_preflight import check
class ReceiptTests(unittest.TestCase):
 def setUp(self):
  self.temp=tempfile.TemporaryDirectory();self.addCleanup(self.temp.cleanup);self.root=Path(self.temp.name)
  self.engines={n:{'sha256':'a'*64} for n in ['Bosak.XPath.Core','Bosak.XPath.Runtime','Bosak.XPath.Api']};self.pins={'sourceVersion':'test-fixture','engines':self.engines}
  binary=self.root/'consumer.dll';binary.write_bytes(b'synthetic unchanged consumer');binary_ref={'path':binary.name,'sha256':hashlib.sha256(binary.read_bytes()).hexdigest()}
  self.refs={}
  for name in ['input','build','fixture','results','correctness','baseline','replacement','budget']:
   p=self.root/(name+'.json');p.write_text(json.dumps({'syntheticTestFixture':True,'ownerRole':'Schema','scenario':'PF-03','candidatePinsSha256':'pins','consumerSourceRevision':'b'*40,'consumerArtifacts':{'consumer.dll':binary_ref}}),encoding='utf-8');self.refs[name]={'path':p.name,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()}
  self.data={'scenario':'PF-03','ownerRole':'Schema','candidate':{'sourceVersion':'test-fixture','pinsSha256':'pins','apiSha256':'api','engines':{n:'a'*64 for n in self.engines}},'consumer':{'cohort':'source','sourceRevision':'b'*40,'baselinePin':'synthetic','inputManifest':self.refs['input'],'buildLog':self.refs['build']},'loadedEngines':{n:'a'*64 for n in self.engines},'fixtures':[self.refs['fixture']],'queryAndSettings':{'synthetic':True},'rawResults':self.refs['results'],'correctnessReceipt':self.refs['correctness'],'measurement':{'environment':{'os':'synthetic','runtime':'synthetic','cpu':'synthetic','configuration':'synthetic','tool':'synthetic'},'method':'synthetic','warmupSamples':1,'measuredSamples':2,'metrics':['allocation']},'budget':{'receipt':self.refs['budget'],'thresholds':{'synthetic':1},'tolerance':0}}
 def run_check(self):return check(self.data,self.root,self.pins,'pins','api')
 def test_complete_structure_never_claims_acceptance(self):
  r=self.run_check();self.assertTrue(r['structureReady']);self.assertFalse(r['qualificationAccepted']);self.assertFalse(r['ownerAcceptanceVerified'])
 def test_stale_candidate(self):
  self.data['candidate']['pinsSha256']='old';self.assertFalse(self.run_check()['structureReady'])
 def test_missing_engine(self):
  self.data['candidate']['engines'].pop('Bosak.XPath.Core');self.assertFalse(self.run_check()['structureReady'])
 def test_wrong_loaded_engine(self):
  self.data['loadedEngines']['Bosak.XPath.Api']='c'*64;self.assertFalse(self.run_check()['structureReady'])
 def test_missing_budget(self):
  self.data.pop('budget');self.assertFalse(self.run_check()['structureReady'])
 def test_wrong_budget_binding(self):
  p=self.root/'budget.json';p.write_text(json.dumps({'ownerRole':'Braid','candidatePinsSha256':'old'}),encoding='utf-8');self.data['budget']['receipt']['sha256']=hashlib.sha256(p.read_bytes()).hexdigest();self.assertFalse(self.run_check()['structureReady'])
 def test_tampered_fixture(self):
  (self.root/'fixture.json').write_text('changed',encoding='utf-8');self.assertFalse(self.run_check()['structureReady'])
 def test_path_escape(self):
  self.data['fixtures']=[{'path':'../escape.json','sha256':'a'*64}];self.assertIn('fixture 0: path escapes receipt root',self.run_check()['errors'])
 def test_unchanged_binary_cannot_rebuild(self):
  self.data['consumer'].update(cohort='unchanged-binary',rebuilt=True,baselineBinaryManifest=self.refs['baseline'],replacementBinaryManifest=self.refs['replacement']);self.assertFalse(self.run_check()['structureReady'])
 def test_distinct_binary_cohort_structure(self):
  self.data['consumer'].update(cohort='unchanged-binary',rebuilt=False,baselineBinaryManifest=self.refs['baseline'],replacementBinaryManifest=self.refs['replacement']);self.assertTrue(self.run_check()['structureReady'])
 def test_missing_samples(self):
  self.data['measurement']['measuredSamples']=0;self.assertFalse(self.run_check()['structureReady'])
 def test_wrong_scenario(self):
  self.data['scenario']='PF-99';self.assertFalse(self.run_check()['structureReady'])
 def test_non_object_receipt(self):
  self.assertFalse(check([],self.root,self.pins,'pins','api')['structureReady'])
 def test_empty_platform(self):
  self.data['measurement']['environment']={};self.assertFalse(self.run_check()['structureReady'])
 def test_negative_tolerance(self):
  self.data['budget']['tolerance']=-1;self.assertFalse(self.run_check()['structureReady'])
 def test_swapped_consumer_revision(self):
  self.data['consumer']['sourceRevision']='e'*40;self.assertFalse(self.run_check()['structureReady'])
 def test_changed_consumer_bytes(self):
  p=self.root/'changed.dll';p.write_bytes(b'different synthetic consumer');replacement=self.root/'replacement.json';d=json.loads(replacement.read_text(encoding='utf-8'));d['consumerArtifacts']['consumer.dll']={'path':p.name,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()};replacement.write_text(json.dumps(d),encoding='utf-8');self.refs['replacement']['sha256']=hashlib.sha256(replacement.read_bytes()).hexdigest();self.data['consumer'].update(cohort='unchanged-binary',rebuilt=False,baselineBinaryManifest=self.refs['baseline'],replacementBinaryManifest=self.refs['replacement']);self.assertFalse(self.run_check()['structureReady'])
 def test_empty_metrics(self):
  self.data['measurement']['metrics']=[];self.assertFalse(self.run_check()['structureReady'])
if __name__=='__main__':unittest.main(verbosity=2)

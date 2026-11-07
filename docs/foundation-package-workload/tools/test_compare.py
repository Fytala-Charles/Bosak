"""Synthetic controls for rejecting incompatible method comparisons without executing managed negatives."""
import copy,unittest
from compare import compare

class ComparisonControls(unittest.TestCase):
    def setUp(self):
        self.p={'format':'proposal','scenario':'subset','schemaSha256':'s','expression':'data($node)','behaviorProfile':'profile','limits':{'cache':8},'measurementScope':'whole-cycle','repetitions':3,'warmup':2,'iterations':5,'workloads':[{'id':'one','children':1,'instanceSha256':'i'}]}
        self.a={'format':'bosak-owner-measurement-v1','protocolSha256':'digest','runtime':'10','architecture':'X64','osDescription':'Windows','role':'baseline','ownerAccepted':False,'budgetsAccepted':False,'rows':[{'workload':'one','repetition':i,'children':1,'iterations':5,'warmup':2,'latencySamplesMs':[1,2,3,4,5],'allocatedBytes':50,'allocatedBytesPerCycle':10} for i in range(3)]}
        self.b=copy.deepcopy(self.a);self.b['role']='candidate'
    def run_compare(self,mode='baseline',p=None):return compare(self.p,self.a,'digest',p or self.p,self.b,'digest',mode)
    def test_matching_method_is_diagnostic_only(self):
        r=self.run_compare();self.assertFalse(r['budgetsAccepted']);self.assertFalse(r['binaryQualificationPerformedByComparator'])
    def test_candidate_is_not_baseline(self):
        self.a['role']='candidate'
        with self.assertRaises(ValueError):self.run_compare()
    def test_repeatability_mode_is_explicit(self):
        self.a['role']='candidate';self.assertEqual('repeatability',self.run_compare('repeatability')['mode'])
    def test_changed_method_fields_refuse(self):
        for key in ['schemaSha256','expression','behaviorProfile','limits','measurementScope','workloads']:
            with self.subTest(key=key):
                p=copy.deepcopy(self.p);p[key]='changed'
                with self.assertRaises((ValueError,TypeError)):self.run_compare(p=p)
    def test_environment_mismatch_refuses(self):
        self.b['runtime']='other'
        with self.assertRaises(ValueError):self.run_compare()
    def test_missing_repetition_refuses(self):
        self.b['rows'].pop()
        with self.assertRaises(ValueError):self.run_compare()
    def test_duplicate_repetition_refuses(self):
        self.b['rows'][1]['repetition']=0
        with self.assertRaises(ValueError):self.run_compare()
    def test_wrong_protocol_hash_refuses(self):
        self.b['protocolSha256']='wrong'
        with self.assertRaises(ValueError):self.run_compare()
    def test_nonfinite_latency_refuses(self):
        self.b['rows'][0]['latencySamplesMs'][0]=float('nan')
        with self.assertRaises(ValueError):self.run_compare()
    def test_missing_samples_refuse(self):
        self.b['rows'][0]['latencySamplesMs'].pop()
        with self.assertRaises(ValueError):self.run_compare()
    def test_wrong_work_unit_refuses(self):
        self.b['rows'][0]['children']=8
        with self.assertRaises(ValueError):self.run_compare()
    def test_wrong_warmup_refuses(self):
        self.b['rows'][0]['warmup']=0
        with self.assertRaises(ValueError):self.run_compare()
    def test_bad_allocation_refuses(self):
        self.b['rows'][0]['allocatedBytesPerCycle']=11
        with self.assertRaises(ValueError):self.run_compare()
    def test_acceptance_claim_refuses(self):
        self.b['ownerAccepted']=True
        with self.assertRaises(ValueError):self.run_compare()

if __name__=='__main__':unittest.main()

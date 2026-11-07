"""Negative controls prevent a scope reconciliation from erasing release obligations."""
import unittest,copy
import verify as v
class ScopeControls(unittest.TestCase):
 def setUp(self):
  self.args=[v.read(v.P/name) for name in ['REQUIREMENTS.json','PLAN.json','DECLARATIONS.json','STATUS.json','HOST-CASES.json']]+[v.read(v.R/'docs/foundation-r2-closure/REQUIREMENT-DISPOSITIONS.json'),v.read(v.R/'docs/foundation-r2-closure/CLOSURE.json')]
 def test_valid(self):v.validate(*self.args)
 def refuse(self,change):
  args=copy.deepcopy(self.args);change(args)
  with self.assertRaises(AssertionError):v.validate(*args)
 def test_missing_requirement(self):self.refuse(lambda a:a[0]['requirements'].pop())
 def test_duplicate_requirement(self):self.refuse(lambda a:a[0]['requirements'].__setitem__(1,a[0]['requirements'][0]))
 def test_waiver(self):self.refuse(lambda a:a[0]['requirements'][0].__setitem__('requirementWaived',True))
 def test_subset_is_not_completion(self):self.refuse(lambda a:a[0]['requirements'][0].__setitem__('fullyImplemented',True))
 def test_cycle(self):self.refuse(lambda a:a[1]['work'][0]['dependsOn'].append('PK-01'))
 def test_missing_exit(self):self.refuse(lambda a:a[1]['work'][0].__setitem__('exitEvidence',''))
 def test_inventory_count(self):self.refuse(lambda a:a[2].__setitem__('presentTypes',47))
 def test_duplicate_type(self):self.refuse(lambda a:a[2]['rows'].__setitem__(1,a[2]['rows'][0]))
 def test_no_inferred_owner_acceptance(self):self.refuse(lambda a:a[3].__setitem__('ownerAccepted',True))
 def test_no_false_r3_closure(self):self.refuse(lambda a:a[3].__setitem__('r3Complete',True))
 def test_no_unrun_host_qualification(self):self.refuse(lambda a:a[4]['cases'][0].__setitem__('status','Qualified'))
 def test_preserve_requirement_text(self):self.refuse(lambda a:a[0]['requirements'][0]['baselineRequirement'].__setitem__('requestedReturn','Dropped'))
if __name__=='__main__':unittest.main()

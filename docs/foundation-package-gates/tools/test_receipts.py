"""Exercise qualification refusal boundaries without changing sealed test/binding files."""
import copy
import unittest
from unittest.mock import patch
import verify


class GateReceiptTests(unittest.TestCase):
    def setUp(self):
        self.results=verify.load(verify.HERE/'RESULTS.json')

    def test_complete_qualification(self):
        self.assertEqual(verify.validate(self.results),(24225,2,7))

    def test_missing_project_refuses(self):
        self.results['runners'].pop()
        with self.assertRaises(AssertionError):verify.validate(self.results)

    def test_changed_gate_count_refuses(self):
        next(r for r in self.results['runners'] if r['kind']=='gate')['counts'][0]-=1
        with self.assertRaises(AssertionError):verify.validate(self.results)

    def test_inflated_unit_total_refuses(self):
        self.results['unitTests']['passed']+=1
        with self.assertRaises(AssertionError):verify.validate(self.results)

    def test_false_freeze_refuses(self):
        self.results['publicationOrFreezeAccepted']=True
        with self.assertRaises(AssertionError):verify.validate(self.results)

    def test_unknown_observer_refuses(self):
        self.results['runners'][0]['observerSha256']='0'*64
        with self.assertRaises(AssertionError):verify.validate(self.results)

    def test_wrong_source_built_product_refuses(self):
        row=next(r for r in self.results['runners'] if r['sourceBuiltProducts'])
        row['sourceBuiltProducts'][0]['sha256']='0'*64
        with self.assertRaises(AssertionError):verify.validate(self.results)

    def test_wrong_actual_loaded_package_refuses(self):
        original=verify.load
        def altered(path):
            result=original(path)
            if 'processId' in result and 'assemblies' in result:
                for assembly in result['assemblies']:
                    if assembly['name']=='Bosak.XPath.Core' and assembly['context']=='Default':
                        result=copy.deepcopy(result)
                        next(a for a in result['assemblies'] if a['name']=='Bosak.XPath.Core' and a['context']=='Default')['sha256']='0'*64
                        break
            return result
        with patch.object(verify,'load',altered):
            with self.assertRaisesRegex(AssertionError,'Wrong loaded package hash'):verify.validate(self.results)


if __name__=='__main__':unittest.main()

"""Exercise review-tool boundaries without changing engine code or sealed receipts."""
import copy
import unittest
from verify_manifest import HERE, load, validate


class ManifestChecks(unittest.TestCase):
    def setUp(self):
        self.manifest = load(HERE / 'evidence/member-manifest.json')
        self.closure = load(HERE / 'evidence/signature-closure.json')
        self.cases = load(HERE / 'compatibility-cases.json')

    def check(self):
        return validate(self.manifest, self.closure, self.cases)

    def test_complete_observed_baseline(self):
        self.assertEqual(self.check(), (5766, 40))

    def test_overloads_have_distinct_exact_ids(self):
        entries = [e for e in self.manifest['entries'] if e['type'].endswith('.XdmValue') and e['symbol'] == 'FromBoolean' and e['kind'] == 'M']
        self.assertEqual(len(entries), 2)
        self.assertEqual(len({e['id'] for e in entries}), 2)

    def test_parameter_only_runtime_dependency_is_retained(self):
        entries = [e for e in self.manifest['entries'] if e['type'].endswith('.XPath31Expression') and e['symbol'] == 'Evaluate' and e['kind'] == 'M']
        self.assertTrue(any('Bosak.XPath.Runtime.Vm.EvaluationContext' in e['signatureTypes'] for e in entries))
        self.assertIn('Bosak.XPath.Runtime.Resources.IControlledResourceAuthority', self.closure['reachableTypes'])

    def test_required_and_default_node_slots_differ(self):
        entries = {e['symbol']: e for e in self.manifest['entries'] if e['type'].endswith('.IXdmNode') and e['kind'] == 'M'}
        self.assertEqual(entries['get_NodeKind']['interfaceSlot'], 'Required')
        self.assertEqual(entries['get_HasNoTypedValue']['interfaceSlot'], 'DefaultImplementation')

    def test_missing_entry_refuses(self):
        self.manifest['entries'].pop()
        with self.assertRaises(AssertionError):
            self.check()

    def test_duplicate_entry_refuses(self):
        self.manifest['entries'][-1] = copy.deepcopy(self.manifest['entries'][0])
        with self.assertRaises(AssertionError):
            self.check()

    def test_lost_parameter_dependency_refuses(self):
        entry = next(e for e in self.manifest['entries'] if e['type'].endswith('.XPath31Expression') and 'Bosak.XPath.Runtime.Vm.EvaluationContext' in e['signatureTypes'])
        entry['signatureTypes'].remove('Bosak.XPath.Runtime.Vm.EvaluationContext')
        with self.assertRaisesRegex(AssertionError, 'Signature dependency mismatch'):
            self.check()

    def test_incomplete_closure_refuses(self):
        self.closure['reachableTypes'].remove('Bosak.XPath.Runtime.Resources.IControlledResourceAuthority')
        with self.assertRaisesRegex(AssertionError, 'Incomplete transitive closure'):
            self.check()

    def test_false_freeze_refuses(self):
        self.manifest['freezeAccepted'] = True
        with self.assertRaises(AssertionError):
            self.check()

    def test_fabricated_case_result_refuses(self):
        self.cases['cases'][0]['status'] = 'Passed'
        with self.assertRaises(AssertionError):
            self.check()


if __name__ == '__main__':
    unittest.main()

"""Test retained-caller verifier refusal boundaries using in-memory receipt mutations."""
import unittest
from verify import inputs, validate


class LegacyReceiptTests(unittest.TestCase):
    def setUp(self):
        self.runs, self.bundles = inputs()

    def test_valid_cross_version_evidence(self):
        self.assertEqual(validate(self.runs, self.bundles), (20, 3, 7))

    def test_changed_result_refuses(self):
        self.runs['unchanged-binary']['cases'][10]['value']['text'] = 'changed'
        with self.assertRaisesRegex(AssertionError, 'Case observations differ'):
            validate(self.runs, self.bundles)

    def test_wrong_loaded_version_refuses(self):
        self.runs['unchanged-binary']['loadedAssemblies'][0]['identity'] = 'Bosak.XPath.Api, Version=1.34.1.0, Culture=neutral, PublicKeyToken=null'
        with self.assertRaisesRegex(AssertionError, 'Wrong loaded version'):
            validate(self.runs, self.bundles)

    def test_changed_caller_hash_refuses(self):
        self.runs['unchanged-binary']['callerSha256'] = '0' * 64
        with self.assertRaisesRegex(AssertionError, 'Caller hash mismatch'):
            validate(self.runs, self.bundles)

    def test_changed_original_reference_version_refuses(self):
        self.runs['baseline']['requestedBosakReferences'][0] = self.runs['baseline']['requestedBosakReferences'][0].replace('1.34.1.0','1.34.2.0')
        with self.assertRaises(AssertionError):
            validate(self.runs, self.bundles)

    def test_changed_bundle_hash_refuses(self):
        self.bundles['bundles'][0]['sha256'] = '0' * 64
        with self.assertRaisesRegex(AssertionError, 'Bundle hash mismatch'):
            validate(self.runs, self.bundles)

    def test_failed_case_refuses(self):
        self.runs['recompiled-source']['failed'] = 1
        with self.assertRaises(AssertionError):
            validate(self.runs, self.bundles)

    def test_missing_case_refuses(self):
        self.runs['baseline']['cases'].pop()
        with self.assertRaises(AssertionError):
            validate(self.runs, self.bundles)


if __name__ == '__main__':
    unittest.main()

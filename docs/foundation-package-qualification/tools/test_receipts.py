"""Exercise package qualification verifier refusal boundaries without changing fixtures."""
import unittest
from verify import inputs, validate


class PackageEvidenceTests(unittest.TestCase):
    def setUp(self):
        self.report, self.assets, self.runs = inputs()

    def check(self):
        return validate(self.report, self.assets, self.runs)

    def test_complete_candidate(self):
        self.assertEqual(self.check(), (7, 20, 2, 2205))

    def test_extra_restore_dependency_refuses(self):
        self.assets['libraries']['Unexpected/1.0.0'] = {}
        with self.assertRaisesRegex(AssertionError, 'Unexpected restore dependency'):
            self.check()

    def test_restore_hash_mismatch_refuses(self):
        self.assets['libraries']['Bosak.XPath.Api/1.34.2']['sha512'] = 'changed'
        with self.assertRaisesRegex(AssertionError, 'Restore package hash mismatch'):
            self.check()

    def test_stale_loaded_source_revision_refuses(self):
        self.runs['package-source-run']['loadedAssemblies'][0]['informationalVersion'] = '1.34.2+stale'
        with self.assertRaisesRegex(AssertionError, 'Wrong loaded source revision'):
            self.check()

    def test_changed_observation_refuses(self):
        self.runs['unchanged-binary-run']['cases'][10]['value']['text'] = 'changed'
        with self.assertRaisesRegex(AssertionError, 'Caller observations changed'):
            self.check()

    def test_package_hash_mismatch_refuses(self):
        self.report['packages'][0]['sha256'] = '0'*64
        with self.assertRaisesRegex(AssertionError, 'Package hash mismatch'):
            self.check()

    def test_wrong_package_commit_refuses(self):
        self.report['packages'][0]['repositoryCommit'] = 'stale'
        with self.assertRaisesRegex(AssertionError, 'Package source pin mismatch'):
            self.check()

    def test_second_package_source_refuses(self):
        self.assets['project']['restore']['sources']['https://example.invalid/feed'] = {}
        with self.assertRaisesRegex(AssertionError, 'Restore isolation lost'):
            self.check()


if __name__ == '__main__':
    unittest.main()

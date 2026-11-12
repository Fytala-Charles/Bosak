"""Receipt-parser controls: retain reasons and refuse incomplete or ambiguous inventories."""
import importlib.util
from pathlib import Path
import unittest

spec = importlib.util.spec_from_file_location('summary', Path(__file__).with_name('generate-summary.py'))
summary = importlib.util.module_from_spec(spec)
spec.loader.exec_module(summary)


class SkipReceiptTests(unittest.TestCase):
    def test_named_groups_retain_original_reasons(self):
        self.assertEqual(summary.parse_skips('=== 1 skipped: Platform limit ===\ncase-a\n\n'
                                            '=== 1 skipped: Harness error ===\ncase-b\n'),
                         [('case-a', 'Platform limit'), ('case-b', 'Harness error')])

    def test_truncated_group_is_refused(self):
        with self.assertRaisesRegex(ValueError, 'count mismatch'):
            summary.parse_skips('=== 2 skipped: Unsupported dependency ===\ncase-a\n')

    def test_duplicate_name_is_refused(self):
        with self.assertRaisesRegex(ValueError, 'Duplicate'):
            summary.parse_skips('=== 2 skipped: Unsupported dependency ===\ncase-a\ncase-a\n')

    def test_missing_reason_is_refused(self):
        with self.assertRaisesRegex(ValueError, 'without a reason'):
            summary.parse_skips('case-a\n')


if __name__ == '__main__':
    unittest.main()

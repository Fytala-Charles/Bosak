"""Refuse lost/duplicate outcomes and distinguish silent skips from interrupted cases."""
import importlib.util
from pathlib import Path
import unittest

spec = importlib.util.spec_from_file_location('qualification_report', Path(__file__).with_name('report.py'))
report = importlib.util.module_from_spec(spec)
spec.loader.exec_module(report)


class ReportTests(unittest.TestCase):
    def plan(self):
        return {'sets': [{'name': 'sample', 'file': 'sample.xml', 'cases': ['a', 'b', 'c']}]}

    def test_silent_skip_reconciles_with_completed_set(self):
        text = ('  Starting: sample ...\n  RUN  a\n  PASS a\n  RUN  b\n'
                '  RUN  c\n  FAIL c: result mismatch\n'
                '  Done: sample (3 tests, 1 passed, 1 failed, 1 skipped)\n')
        rows = report.parse(self.plan(), text, True)
        self.assertEqual([r['status'] for r in rows], ['Passed', 'Skipped', 'Failed'])

    def test_whole_set_skip_names_every_original_case(self):
        rows = report.parse(self.plan(), '  SKIP sample: Requires unsupported feature (3 tests)\n', True)
        self.assertEqual([r['test'] for r in rows], ['a', 'b', 'c'])
        self.assertTrue(all(r['status'] == 'Skipped' for r in rows))

    def test_interruption_is_not_reported_as_a_skip(self):
        rows = report.parse(self.plan(), '  Starting: sample ...\n  RUN  a\n  PASS a\n  RUN  b\n', False)
        self.assertEqual([r['status'] for r in rows], ['Passed', 'Incomplete', 'NotRun'])

    def test_inconsistent_set_counts_are_refused(self):
        with self.assertRaisesRegex(ValueError, 'outcome mismatch'):
            report.parse(self.plan(), '  Starting: sample ...\n  RUN  a\n  PASS a\n'
                         '  Done: sample (3 tests, 3 passed, 0 failed, 0 skipped)\n', True)

    def test_duplicate_outcome_is_refused(self):
        with self.assertRaisesRegex(ValueError, 'duplicate'):
            report.parse(self.plan(), '  Starting: sample ...\n  PASS a\n  PASS a\n', False)

    def test_named_policy_comments_remain_scoped_to_their_group(self):
        source = ('HashSet<string> SkipTests = new()\n    {\n'
                  '        // Original recursion rationale\n        "a", "b",\n'
                  '        // Separate formatting rationale\n        "c",\n    };\n')
        notes = report.named_skip_notes(source)
        self.assertEqual(notes, {'a': 'Original recursion rationale', 'b': 'Original recursion rationale',
                                 'c': 'Separate formatting rationale'})


if __name__ == '__main__':
    unittest.main()

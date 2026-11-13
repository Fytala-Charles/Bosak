"""Copied-runner reversibility, identity and timing refusal controls; no .NET negatives."""
import copy
import importlib.util
from pathlib import Path
import tempfile
import unittest

spec = importlib.util.spec_from_file_location('cost', Path(__file__).with_name('xslt30_case_cost.py'))
cost = importlib.util.module_from_spec(spec); spec.loader.exec_module(cost)


class CaseCostTests(unittest.TestCase):
    def setUp(self):
        self.row = dict(ordinal=1,testSet='set',test='case',result='Pass',startWall=100,endWall=200,
            frequency=100,wallSeconds=1.0,startCpuTicks=0,endCpuTicks=5000000,cpuSeconds=0.5)
        self.outcomes = [dict(test_set='set',test='case',status='Passed',reason_source='RunnerCaseLine')]

    def check(self, rows=None, outcomes=None):
        return cost.reconcile_costs(rows if rows is not None else [self.row], outcomes if outcomes is not None else self.outcomes)

    def test_positive_and_zero_cpu_short_case(self):
        self.assertEqual([], self.check())
        self.row.update(endCpuTicks=0,cpuSeconds=0)
        self.assertEqual([], self.check())

    def test_set_exclusions_remain_unmeasured(self):
        skip = dict(test_set='excluded',test='x',status='Skipped',reason_source='RunnerSetSkip',reason='Original unsupported feature')
        self.assertEqual([skip],self.check(outcomes=self.outcomes+[skip]))

    def test_missing_pass_is_refused(self):
        with self.assertRaisesRegex(ValueError,'Missing executable'):self.check(rows=[])

    def test_missing_case_level_skip_is_refused(self):
        self.outcomes[0].update(status='Skipped')
        with self.assertRaisesRegex(ValueError,'Missing executable'):self.check(rows=[])

    def test_duplicate_unknown_and_ordinal_refused(self):
        for change in [dict(test='unknown'),dict(ordinal=2)]:
            with self.subTest(change=change),self.assertRaises(ValueError):self.check(rows=[dict(self.row,**change)])
        with self.assertRaises(ValueError):self.check(rows=[self.row,dict(self.row,ordinal=2)])

    def test_outcome_change_refused(self):
        self.row['result']='Fail'
        with self.assertRaisesRegex(ValueError,'outcome'):self.check()

    def test_nonfinite_missing_and_negative_counters_refused(self):
        for value in [None,-1,float('nan'),float('inf')]:
            with self.subTest(value=value),self.assertRaises(ValueError):self.check(rows=[dict(self.row,cpuSeconds=value)])

    def test_counter_conversion_refused(self):
        self.row['wallSeconds']=2
        with self.assertRaisesRegex(ValueError,'conversion'):self.check()

    def test_overlapping_windows_refused(self):
        second=dict(self.row,ordinal=2,test='other',startWall=150,endWall=250)
        outcomes=self.outcomes+[dict(self.outcomes[0],test='other')]
        with self.assertRaisesRegex(ValueError,'Overlapping'):self.check(rows=[self.row,second],outcomes=outcomes)

    def test_production_source_reversibility_and_copyright(self):
        original=(cost.ROOT/'tests/Bosak.Xslt.Conformance/Program.cs').read_bytes()
        result=cost.instrument(original).decode()
        self.assertIn('SPDX-License-Identifier: Apache-2.0',result)
        restored=result.removeprefix(cost.PREFIX).replace(cost.WRAPPED,cost.CALL).replace(cost.MAIN+cost.INIT,cost.MAIN)
        self.assertEqual(original.decode().replace('\r\n','\n'),restored)

    def test_anchor_drift_refused(self):
        with self.assertRaisesRegex(ValueError,'anchor'):cost.instrument(b'class Different {}')

    def test_bounded_controller_only_changes_host_command(self):
        original=(cost.ROOT/'docs/xslt30-qualification/run.py').read_bytes()
        changed=cost.controller(original)
        self.assertEqual(original,changed.decode().replace(cost.NEW_COMMAND,cost.OLD_COMMAND).encode())
        self.assertIn(b"'--roll-forward', 'Disable'",changed)
        self.assertIn(b"'--fx-version'",changed)

    def test_bounded_controller_drift_refused(self):
        with self.assertRaisesRegex(ValueError,'drift'):cost.controller(b'not the reviewed controller')

    def test_existing_output_and_nested_outputs_refused(self):
        with tempfile.TemporaryDirectory() as folder:
            root=Path(folder);marker=root/'marker';marker.write_bytes(b'original')
            with self.assertRaises(ValueError):cost.safe_paths(root/'q',root/'c',root)
            self.assertEqual(b'original',marker.read_bytes())
            with self.assertRaises(ValueError):cost.safe_paths(root/'q',root/'c',root/'q/new')
            with self.assertRaises(ValueError):cost.safe_paths(root/'q',root/'c',root/'c/new')
            cost.safe_paths(root/'q',root/'c',root/'fresh')

    def test_sealed_qualified_identity_has_nine_engines(self):
        hashes=cost.qualified_hashes()
        self.assertEqual(32,len(hashes))
        self.assertEqual(9,len([n for n in hashes if n.startswith('Bosak.') and n.endswith('.dll') and n!='Bosak.Xslt.Conformance.dll']))


if __name__=='__main__':unittest.main()

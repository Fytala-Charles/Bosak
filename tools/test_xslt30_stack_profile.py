"""Pure subset/stack refusal controls; no negative .NET launches."""
import copy
import importlib.util
from pathlib import Path
import unittest

spec=importlib.util.spec_from_file_location('profile',Path(__file__).with_name('xslt30_stack_profile.py'))
profile=importlib.util.module_from_spec(spec);spec.loader.exec_module(profile)


class StackProfileTests(unittest.TestCase):
    def setUp(self):
        self.data=dict(shared=dict(frames=[dict(name='Program.RunTestCase'),dict(name='Engine.Compare'),dict(name='Other.Wait')]),
            profiles=[dict(type='evented',unit='milliseconds',startValue=0,endValue=5,events=[
                dict(type='O',frame=0,at=0),dict(type='O',frame=1,at=1),dict(type='C',frame=1,at=4),dict(type='C',frame=0,at=5)]),
                dict(type='sampled',unit='milliseconds',samples=[[2]],weights=[10])])
        self.row=dict(ordinal=1,testSet='sx-GeneralComp-gt',test='example',result='Pass',wallSeconds=1,cpuSeconds=0)

    def test_case_stack_selection_excludes_unrelated_waiting_thread(self):
        result=profile.stack_report(self.data)
        self.assertEqual(15,result['sampledThreadWeight']);self.assertEqual(5,result['caseStackWeight'])
        self.assertEqual('Engine.Compare',result['caseExclusive'][0]['frame']);self.assertEqual(.6,result['caseExclusive'][0]['share'])
        self.assertIn('not on-CPU',result['interpretation'])

    def test_sampled_case_stacks_and_recursion_deduplicate_inclusive_weight(self):
        self.data['profiles']=[dict(type='sampled',unit='seconds',samples=[[0,1,1]],weights=[2])]
        result=profile.stack_report(self.data)
        self.assertEqual(1,result['caseInclusive'][0]['share'])
        self.assertEqual(2,result['caseExclusive'][0]['weight'])

    def test_zero_duration_is_allowed_with_real_case_coverage(self):
        self.data['profiles'][1]['weights']=[0]
        self.assertEqual(5,profile.stack_report(self.data)['sampledThreadWeight'])

    def test_unknown_negative_and_nonfinite_weights_refused(self):
        for value in [-1,float('inf'),float('nan')]:
            data=copy.deepcopy(self.data);data['profiles'][1]['weights']=[value]
            with self.subTest(value=value),self.assertRaises(ValueError):profile.stack_report(data)

    def test_unknown_frame_refused(self):
        self.data['profiles'][1]['samples']=[[99]]
        with self.assertRaisesRegex(ValueError,'frame'):profile.stack_report(self.data)

    def test_event_time_reversal_refused(self):
        self.data['profiles'][0]['events'][2]['at']=-1
        with self.assertRaisesRegex(ValueError,'reversal'):profile.stack_report(self.data)

    def test_unbalanced_stack_refused(self):
        self.data['profiles'][0]['events'][2]['frame']=0
        with self.assertRaisesRegex(ValueError,'Unbalanced'):profile.stack_report(self.data)

    def test_unclosed_stack_refused(self):
        self.data['profiles'][0]['events'].pop()
        with self.assertRaisesRegex(ValueError,'Unclosed'):profile.stack_report(self.data)

    def test_mismatched_sample_weights_refused(self):
        self.data['profiles'][1]['weights']=[]
        with self.assertRaisesRegex(ValueError,'mismatch'):profile.stack_report(self.data)

    def test_mixed_units_and_missing_case_ancestry_refused(self):
        self.data['profiles'][1]['unit']='seconds'
        with self.assertRaises(ValueError):profile.stack_report(self.data)
        self.data['profiles']=self.data['profiles'][1:]
        with self.assertRaises(ValueError):profile.stack_report(self.data)

    def test_selected_outcomes_and_zero_cpu_accepted(self):
        profile.validate_cases([self.row],{'example':'Pass'})

    def test_missing_duplicate_changed_or_wrong_set_refused(self):
        variants=[[],[self.row,self.row],[dict(self.row,result='Fail')],[dict(self.row,testSet='different')]]
        for rows in variants:
            with self.subTest(rows=rows),self.assertRaises(ValueError):profile.validate_cases(rows,{'example':'Pass'})

    def test_invalid_counter_refused(self):
        self.row['cpuSeconds']=None
        with self.assertRaisesRegex(ValueError,'counter'):profile.validate_cases([self.row],{'example':'Pass'})

    def test_both_runtime_commands_pinned_and_only_one_selected_set(self):
        tool,command=profile.commands(Path('packet'),{'profiler':'tool.dll'},{'nativeFiles':{'dotnet.exe':{'path':'dotnet.exe'}}})
        self.assertEqual(['exec','--fx-version','8.0.28','--roll-forward','Disable'],tool[1:6])
        self.assertEqual('sx-GeneralComp-gt',command[-1])
        self.assertIn('dotnet-common,dotnet-sampled-thread-time',command)
        self.assertIn('10.0.11',command);self.assertEqual(1,command.count('--'))

    def test_sealed_reference_selection_is_exact(self):
        baseline=profile.baseline()
        self.assertEqual(54,len(baseline['expectedResults']))
        self.assertEqual(set(baseline['targetSet']['cases']),set(baseline['expectedResults']))
        self.assertEqual(180,baseline['wallTimeoutSeconds']);self.assertEqual(8589934592,baseline['memoryLimitBytes'])


if __name__=='__main__':unittest.main()

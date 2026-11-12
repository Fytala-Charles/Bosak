"""Gate acceptance/refusal controls; no unhandled .NET negative processes."""
import io
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch
import zipfile

import xslt30_gate as gate


class GateTests(unittest.TestCase):
    def setUp(self):
        self.policy={'counts':{'Passed':2,'Failed':0,'Skipped':1}}
        self.plan={'sets':[{'name':'sample','file':'sample.xml','cases':['a','b','c']}]}
        self.run={'status':'Completed','success':True,'completeCatalogCounts':True,
                  'counts':self.policy['counts']}
        self.text='  Starting: sample ...\n  RUN a\n  PASS a\n  RUN b\n  PASS b\n  SKIP c: excluded\n  Done: sample (3 tests, 2 passed, 0 failed, 1 skipped)\n'
        self.skips=[dict(test_set='sample',test='c',reason='Readable explanation',recorded_reason='excluded',reason_source='RunnerCaseLine')]

    def reconcile(self):
        return gate.reconcile(self.plan,self.run,self.text,self.skips,self.policy)

    def test_complete_case_ledger_and_original_explanation(self):
        rows=self.reconcile()
        self.assertEqual(len(rows),3)
        self.assertEqual(rows[-1]['reason'],'Readable explanation')

    def test_failed_legacy_zero_exit_is_refused(self):
        self.run.update(success=False,runnerExitCode=0)
        with self.assertRaises(ValueError): self.reconcile()

    def test_timeout_is_refused_even_with_success_flag(self):
        self.run['status']='TimedOut'
        with self.assertRaises(ValueError): self.reconcile()

    def test_false_aggregate_success_cannot_hide_case_failure(self):
        self.text=self.text.replace('PASS a','FAIL a: wrong').replace('2 passed, 0 failed','1 passed, 1 failed')
        with self.assertRaisesRegex(ValueError,'counts'): self.reconcile()

    def test_missing_outcome_cannot_hide_behind_summary(self):
        self.text=self.text.replace('  RUN b\n  PASS b\n','')
        with self.assertRaises(ValueError): self.reconcile()

    def test_same_counts_with_changed_exclusion_are_refused(self):
        self.text=self.text.replace('PASS a','SKIP a: excluded').replace('SKIP c: excluded','PASS c')
        with self.assertRaisesRegex(ValueError,'exclusions'): self.reconcile()

    def test_printed_exclusion_reason_drift_is_refused(self):
        self.text=self.text.replace('SKIP c: excluded','SKIP c: different')
        with self.assertRaisesRegex(ValueError,'reason'): self.reconcile()

    def test_duplicate_outcomes_are_refused(self):
        self.text=self.text.replace('  PASS a\n','  PASS a\n  PASS a\n')
        with self.assertRaises(ValueError): self.reconcile()

    def test_ignored_generated_fixture_is_in_inventory(self):
        with tempfile.TemporaryDirectory() as name:
            root=Path(name)
            (root/'.git').mkdir()
            (root/'.git/index').write_text('metadata')
            (root/'.gitignore').write_text('*.out\n')
            (root/'wrong.out').write_text('generated')
            self.assertEqual(set(gate.inventory(root)),{'.gitignore','wrong.out'})

    def test_fixture_write_retained_and_refused(self):
        with tempfile.TemporaryDirectory() as name:
            packet=Path(name)
            corpus=packet/'corpus'; corpus.mkdir()
            (corpus/'expected.xml').write_text('before')
            before=gate.inventory(corpus)
            (corpus/'expected.xml').write_text('overwritten')
            (packet/'runtime').mkdir()
            gate.write_json(packet/'RUNTIME-HASHES.json',{})
            result=gate.finish(packet,self.policy,self.plan,before,self.skips,corpus)
            self.assertFalse(result['success'])
            with zipfile.ZipFile(packet/'GENERATED-OUTPUTS.zip') as archive:
                self.assertEqual(archive.read('expected.xml'),b'overwritten')
            self.assertFalse(json.loads((packet/'GATE.json').read_text())['success'])

    def test_existing_packet_is_refused_without_overwrite(self):
        with tempfile.TemporaryDirectory() as name:
            packet=Path(name)
            (packet/'RUN.json').write_text('original')
            with self.assertRaisesRegex(ValueError,'Existing packet'):
                gate.prepare(packet,packet,packet/'unused.dll')
            self.assertEqual((packet/'RUN.json').read_text(),'original')

    def test_qualified_full_transcript_replays(self):
        policy,plan,inputs,skips=gate.baseline()
        with zipfile.ZipFile(gate.ROOT/policy['evidence']) as archive:
            rows=gate.reconcile(plan,json.loads(archive.read('RUN.json')),
                archive.read('stdout.log').decode('utf-8-sig'),skips,policy)
        self.assertEqual(len(rows),14601)
        self.assertEqual(len(inputs),12886)


if __name__=='__main__':
    unittest.main()

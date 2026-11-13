"""Telemetry refusal/outcome controls; no negative .NET processes are launched."""
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest
from unittest import mock

spec = importlib.util.spec_from_file_location('telemetry', Path(__file__).with_name('xslt30_telemetry.py'))
telemetry = importlib.util.module_from_spec(spec)
spec.loader.exec_module(telemetry)


class TelemetryTests(unittest.TestCase):
    def test_existing_output_refused_without_modification(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            marker = root / 'receipt'
            marker.write_bytes(b'original')
            with self.assertRaisesRegex(ValueError, 'Existing telemetry'):
                telemetry.validate_paths(root / 'corpus', root / 'a.dll', root)
            self.assertEqual(b'original', marker.read_bytes())

    def test_corpus_nested_output_refused(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            with self.assertRaisesRegex(ValueError, 'outside'):
                telemetry.validate_paths(root, root / 'bin/a.dll', root / 'new')

    def test_runtime_nested_output_refused(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            with self.assertRaisesRegex(ValueError, 'outside'):
                telemetry.validate_paths(root / 'corpus', root / 'bin/a.dll', root / 'bin/new')

    def test_wrong_assembly_refused(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            wrong = root / 'bin/Other.dll'
            wrong.parent.mkdir()
            wrong.write_bytes(b'not an assembly')
            with self.assertRaisesRegex(ValueError, 'unmodified'):
                telemetry.validate_paths(root / 'corpus', wrong, root / 'new')

    def test_fresh_paths_allowed(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            assembly = root / 'bin/Bosak.Xslt.Conformance.dll'
            assembly.parent.mkdir()
            assembly.write_bytes(b'mocked launcher only')
            telemetry.validate_paths(root / 'corpus', assembly, root / 'new')

    def test_process_tree_refuses_cycles_and_unrelated_pid(self):
        self.assertTrue(telemetry.belongs_to(3, 1, {3: 2, 2: 1}))
        self.assertFalse(telemetry.belongs_to(3, 1, {3: 2, 2: 3}))
        self.assertFalse(telemetry.belongs_to(3, 1, {3: 9}))

    def test_command_preserves_production_entrypoint_and_full_selection(self):
        command = telemetry.gate_command(Path('corpus'), Path('runner.dll'), Path('fresh'))
        self.assertEqual([sys.executable, str(telemetry.ROOT / 'tools/xslt30_gate.py'), '--corpus', 'corpus', '--assembly', 'runner.dll', '--output', 'fresh'], command)
        self.assertNotIn('--filter', command)
        self.assertNotIn('--timeout', command)

    def classify(self, **changes):
        values = dict(receipt=dict(status='Completed', gateControllerExitCode=0), result=dict(success=True),
            run=dict(success=True), app={'same': 'hash'}, hashes={'same': 'hash'}, source_hashes={'same': 'hash'},
            sample_count=2, module_names=telemetry.REQUIRED_NATIVE)
        values.update(changes)
        return telemetry.outcome(**values), values['receipt']

    def test_success_requires_gate_and_telemetry(self):
        code, receipt = self.classify()
        self.assertEqual(0, code)
        self.assertTrue(receipt['gateSuccess'] and receipt['telemetryCaptured'])

    def test_timeout_is_never_promoted_by_good_samples(self):
        code, receipt = self.classify(run=dict(success=False, status='TimedOut'), result=dict(success=False))
        self.assertEqual(1, code)
        self.assertFalse(receipt['gateSuccess'])
        self.assertTrue(receipt['telemetryCaptured'])

    def test_zero_controller_exit_does_not_override_gate_failure(self):
        self.assertEqual(1, self.classify(result=dict(success=False))[0])

    def test_controller_failure_is_never_a_pass(self):
        self.assertEqual(1, self.classify(receipt=dict(status='ControllerFailure', gateControllerExitCode=0))[0])

    def test_missing_receipt_is_not_a_pass(self):
        self.assertEqual(1, self.classify(result=None)[0])

    def test_missing_native_hashes_marks_telemetry_incomplete(self):
        code, receipt = self.classify(module_names={'dotnet.exe'})
        self.assertEqual(2, code)
        self.assertTrue(receipt['gateSuccess'])
        self.assertFalse(receipt['telemetryCaptured'])

    def test_runtime_and_source_drift_are_refused(self):
        self.assertEqual(2, self.classify(hashes={'changed': 'hash'})[0])
        self.assertEqual(2, self.classify(source_hashes={'changed': 'hash'})[0])

    def test_insufficient_samples_do_not_claim_capture(self):
        self.assertEqual(2, self.classify(sample_count=1)[0])

    def test_metadata_allowlist_excludes_secrets(self):
        self.assertNotIn('GITHUB_TOKEN', telemetry.ENVIRONMENT_KEYS)
        self.assertNotIn('AWS_SECRET_ACCESS_KEY', telemetry.ENVIRONMENT_KEYS)
        with tempfile.TemporaryDirectory() as folder, mock.patch.dict(os.environ, {'GITHUB_TOKEN': 'must-not-appear', 'ImageOS': 'test-image'}):
            fake = mock.Mock()
            fake.observer_job.return_value = {'scope': 'observer'}
            with mock.patch.object(telemetry.platform, 'platform', return_value='mock Windows'), mock.patch.object(telemetry.subprocess, 'run', return_value=mock.Mock(stdout=b'host info', returncode=0)):
                record = telemetry.host_record(fake, Path(folder))
            self.assertNotIn('must-not-appear', json.dumps(record))
            self.assertEqual('test-image', record['environment']['ImageOS'])

    @unittest.skipUnless(sys.platform == 'win32', 'Windows native sampler')
    def test_live_python_child_counters_and_ancestry(self):
        # Harmless Python allocation/CPU work, never an unhandled .NET negative case.
        child = subprocess.Popen([sys.executable, '-c', 'import time; data=bytearray(2000000); sum(range(100000)); time.sleep(3)'])
        sampler = None
        try:
            api = telemetry.Windows()
            self.assertTrue(telemetry.belongs_to(child.pid, os.getpid(), api.parents()))
            sampler = telemetry.ProcessSampler(api, child.pid)
            row = sampler.sample()
            self.assertGreater(row['workingSetBytes'], 0)
            self.assertGreaterEqual(row['kernelCpuSeconds'], 0)
            self.assertGreaterEqual(row['io']['read_bytes'], 0)
            self.assertGreater(sampler.context()['affinityCpuCount'], 0)
        finally:
            if sampler:
                sampler.close()
            child.wait(timeout=10)


if __name__ == '__main__':
    unittest.main()

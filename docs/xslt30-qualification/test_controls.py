"""Exercise controller preflight with mocked Git; no .NET negative process is launched."""
import hashlib
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest
from unittest import mock

spec = importlib.util.spec_from_file_location('qualification_run', Path(__file__).with_name('run.py'))
run = importlib.util.module_from_spec(spec)
spec.loader.exec_module(run)


class PreflightTests(unittest.TestCase):
    def fixture(self, directory):
        packet = Path(directory)
        (packet / 'runtime').mkdir()
        (packet / 'runtime/runner.dll').write_bytes(b'caught diagnostic fixture; never executed')
        plan = dict(wallTimeoutSeconds=1200, memoryLimitBytes=8 * 1024**3, corpusCommit='pinned')
        (packet / 'PLAN.json').write_text(json.dumps(plan), encoding='utf-8')
        digest = hashlib.sha256((packet / 'runtime/runner.dll').read_bytes()).hexdigest()
        (packet / 'RUNTIME-HASHES.json').write_text(json.dumps({'runner.dll': digest}), encoding='utf-8')
        return packet

    def test_clean_pinned_input_is_admitted_without_launch(self):
        with tempfile.TemporaryDirectory() as directory:
            packet = self.fixture(directory)
            with mock.patch.object(run.os, 'name', 'nt'), mock.patch.object(run.subprocess, 'check_output', side_effect=['pinned\n', '']):
                self.assertEqual(run.preflight(packet)['corpusCommit'], 'pinned')

    def test_existing_attempt_is_refused(self):
        with tempfile.TemporaryDirectory() as directory:
            packet = self.fixture(directory)
            (packet / 'RUN.json').write_text('{}', encoding='utf-8')
            with mock.patch.object(run.os, 'name', 'nt'), self.assertRaisesRegex(ValueError, 'Existing attempt'):
                run.preflight(packet)

    def test_changed_binary_is_refused_before_git_or_launch(self):
        with tempfile.TemporaryDirectory() as directory:
            packet = self.fixture(directory)
            (packet / 'runtime/runner.dll').write_bytes(b'changed')
            with mock.patch.object(run.os, 'name', 'nt'), mock.patch.object(run.subprocess, 'check_output') as git:
                with self.assertRaisesRegex(ValueError, 'hash mismatch'):
                    run.preflight(packet)
                git.assert_not_called()

    def test_dirty_corpus_is_refused(self):
        with tempfile.TemporaryDirectory() as directory:
            packet = self.fixture(directory)
            with mock.patch.object(run.os, 'name', 'nt'), mock.patch.object(run.subprocess, 'check_output', side_effect=['pinned\n', ' M catalog.xml']):
                with self.assertRaisesRegex(ValueError, 'Corpus differs'):
                    run.preflight(packet)


if __name__ == '__main__':
    unittest.main()

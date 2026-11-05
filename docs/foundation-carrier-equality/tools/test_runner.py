"""Non-crashing preflight checks; negative cases must never launch .NET."""
from pathlib import Path
import contextlib,io,unittest
from unittest.mock import patch
import importlib.util,json,tempfile
spec=importlib.util.spec_from_file_location('run_caller',Path(__file__).resolve().parents[3]/'tools/run-retained-foundation-caller.py')
run_caller=importlib.util.module_from_spec(spec);spec.loader.exec_module(run_caller)

class RunnerTests(unittest.TestCase):
    def test_missing_argument_never_launches(self):
        with patch.object(run_caller.subprocess,'run') as launch,contextlib.redirect_stderr(io.StringIO()):
            with self.assertRaises(SystemExit):run_caller.main(['--layout','.'])
            launch.assert_not_called()
    def test_missing_binary_never_launches(self):
        output=run_caller.ROOT/'.guard-tmp/nonexistent-caller-output.json'
        with patch.object(run_caller.subprocess,'run') as launch,contextlib.redirect_stderr(io.StringIO()):
            self.assertEqual(1,run_caller.main(['--layout',str(run_caller.ROOT),'--output',str(output)]))
            launch.assert_not_called()
    def test_existing_evidence_never_launches(self):
        with patch.object(run_caller.subprocess,'run') as launch,contextlib.redirect_stderr(io.StringIO()):
            self.assertEqual(1,run_caller.main(['--layout','.', '--output',str(run_caller.ROOT/'README.md')]))
            launch.assert_not_called()
    def test_candidate_manifest_must_name_exact_assemblies(self):
        with tempfile.TemporaryDirectory(dir=run_caller.ROOT/'.guard-tmp') as temp:
            folder=Path(temp);assert folder.resolve().is_relative_to((run_caller.ROOT/'.guard-tmp').resolve());manifest=folder/'qualification.json'
            manifest.write_text(json.dumps({'assemblies':[{'name':'../outside','sha256':'0'*64}]*7}),encoding='utf-8')
            with patch.object(run_caller.subprocess,'run') as launch,contextlib.redirect_stderr(io.StringIO()):
                self.assertEqual(1,run_caller.main(['--layout',str(run_caller.ROOT),'--output',str(folder/'out.json'),'--candidate-evidence',str(manifest)]))
                launch.assert_not_called()
    def test_error_mode_is_restored(self):
        if run_caller.os.name!='nt':return
        import ctypes
        kernel=ctypes.WinDLL('kernel32');kernel.GetErrorMode.restype=ctypes.c_uint
        before=kernel.GetErrorMode()
        with run_caller.process_error_mode():self.assertEqual(3,kernel.GetErrorMode()&3)
        self.assertEqual(before,kernel.GetErrorMode())

if __name__=='__main__':unittest.main()

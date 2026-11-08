"""Non-crashing preflight checks; negative cases must never launch .NET."""
from pathlib import Path
import contextlib,io,unittest
from unittest.mock import patch
import run_caller

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
    def test_error_mode_is_restored(self):
        if run_caller.os.name!='nt':return
        import ctypes
        kernel=ctypes.WinDLL('kernel32');kernel.GetErrorMode.restype=ctypes.c_uint
        before=kernel.GetErrorMode()
        with run_caller.process_error_mode():self.assertEqual(3,kernel.GetErrorMode()&3)
        self.assertEqual(before,kernel.GetErrorMode())

if __name__=='__main__':unittest.main()

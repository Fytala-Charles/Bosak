"""Pre-launch trace refusal controls; never invokes .NET."""
import importlib.util
from pathlib import Path
import tempfile
import unittest
from unittest import mock

spec=importlib.util.spec_from_file_location('extractor',Path(__file__).with_name('xslt30_gc_extract.py'))
extractor=importlib.util.module_from_spec(spec);spec.loader.exec_module(extractor)


class ExtractorTests(unittest.TestCase):
    def test_existing_output_preserves_prior_evidence_without_launch(self):
        with tempfile.TemporaryDirectory() as directory,mock.patch.object(extractor.subprocess,'run') as launch:
            root=Path(directory);marker=root/'original';marker.write_bytes(b'retained')
            with self.assertRaisesRegex(ValueError,'Existing'):extractor.extract(root/'trace',root/'tool',root)
            self.assertEqual(b'retained',marker.read_bytes());launch.assert_not_called()

    def test_changed_or_partial_trace_refused_before_library_or_launch(self):
        with tempfile.TemporaryDirectory() as directory,mock.patch.object(extractor.subprocess,'run') as launch:
            root=Path(directory);trace=root/'changed.nettrace';trace.write_bytes(b'partial input')
            with self.assertRaisesRegex(ValueError,'Unreviewed'):extractor.extract(trace,root/'tool',root/'fresh')
            self.assertFalse((root/'fresh').exists());launch.assert_not_called()


if __name__=='__main__':unittest.main()

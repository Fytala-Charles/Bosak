"""GC-domain and sampled-allocation controls; no .NET processes."""
import importlib.util
from pathlib import Path
import unittest

spec=importlib.util.spec_from_file_location('gc_analysis',Path(__file__).with_name('xslt30_gc_analysis.py'))
gc=importlib.util.module_from_spec(spec);spec.loader.exec_module(gc)


def event(name,at,thread=1,**payload):
    return dict(name=name,milliseconds=at,processId=1,threadId=thread,payload=dict(ClrInstanceID='9',**payload))


class GcAnalysisTests(unittest.TestCase):
    def test_background_collection_elapsed_is_not_pause(self):
        rows=[event('GC/Start',0,Count='1',Depth='2',Type='BackgroundGC',Reason='AllocSmall'),
            event('GC/SuspendEEStart',2,Reason='SuspendForGC'),event('GC/SuspendEEStop',3),
            event('GC/RestartEEStart',5),event('GC/RestartEEStop',6),event('GC/Stop',10,Count='1')]
        r=gc.analyze(rows)
        self.assertEqual(10,r['collectionElapsedUnionMs']);self.assertEqual(4,r['gcSuspensionWindowUnionMs']);self.assertEqual(2,r['gcFullySuspendedPhaseUnionMs'])

    def test_concurrent_suspend_requests_pair_by_initiating_thread(self):
        rows=[event('GC/SuspendEEStart',0,thread=1,Reason='SuspendOther'),event('GC/SuspendEEStart',1,thread=2,Reason='SuspendForGC'),
            event('GC/RestartEEStop',2,thread=1),event('GC/RestartEEStop',5,thread=2)]
        r=gc.analyze(rows);self.assertEqual(4,r['gcSuspensionWindowUnionMs']);self.assertEqual(2,r['otherSuspensionWindowUnionMs']);self.assertFalse(r['unmatched'])

    def test_unmatched_edges_are_visible(self):
        r=gc.analyze([event('GC/Start',0,Count='1',Depth='0',Type='NonConcurrentGC',Reason='AllocSmall')])
        self.assertEqual(0,r['collections']);self.assertEqual('GCStartWithoutStop',r['unmatched'][0]['kind'])

    def test_duplicate_collection_start_refused(self):
        row=event('GC/Start',0,Count='1',Depth='0',Type='NonConcurrentGC',Reason='AllocSmall')
        with self.assertRaises(ValueError):gc.analyze([row,row])

    def test_negative_or_nonfinite_time_refused(self):
        for at in [-1,float('nan'),float('inf')]:
            with self.subTest(at=at),self.assertRaises(ValueError):gc.analyze([event('Other',1),event('Other',at)])

    def test_interval_union_does_not_double_count_overlaps(self):
        self.assertEqual(7,gc.duration([(0,5),(2,7)]));self.assertEqual([],gc.union([]))

    def test_reverse_interval_refused(self):
        with self.assertRaises(ValueError):gc.union([(2,1)])

    def test_allocation_threshold_type_does_not_own_interval_bytes(self):
        row=event('GC/AllocationTick',1,TypeName='SmallThresholdObject',AllocationAmount64='100000')
        row['engineFrames']=['System.List`1[Bosak.Value].Add','Bosak.Provider.Wrap']
        r=gc.analyze([row]);self.assertEqual(100000,r['reportedAllocationIntervalBytes'])
        self.assertEqual([('SmallThresholdObject',1)],r['thresholdCrossingTypes'])
        self.assertEqual([('Bosak.Provider.Wrap',1)],r['sampledAllocationEngineContexts'])
        self.assertNotIn('bytesByType',r)

    def test_missing_allocation_events_is_not_zero_allocations_claim(self):
        r=gc.analyze([]);self.assertEqual(0,r['allocationTicks']);self.assertIn('approximate',r['interpretation'])

    def test_finalizer_object_count_separate_from_elapsed(self):
        r=gc.analyze([event('GC/FinalizersStart',0),event('GC/FinalizersStop',4,Count='100')])
        self.assertEqual(100,r['finalizerReportedObjects']);self.assertEqual(4,r['finalizerElapsedUnionMs'])

    def test_suspension_phase_reversal_refused(self):
        rows=[event('GC/SuspendEEStart',0,Reason='SuspendForGC'),event('GC/RestartEEStart',1),event('GC/SuspendEEStop',2),event('GC/RestartEEStop',3)]
        with self.assertRaises(ValueError):gc.analyze(rows)

    def test_temporal_overlap_is_not_thread_cpu(self):
        data=dict(shared=dict(frames=[dict(name='Bosak.XPath.Runtime!Engine'),dict(name='PollGCWorker'),dict(name='CPU_TIME')]),
            profiles=[dict(type='evented',unit='milliseconds',startValue=0,events=[
                dict(type='O',frame=0,at=0),dict(type='O',frame=1,at=0),dict(type='O',frame=2,at=0),
                dict(type='C',frame=2,at=10),dict(type='C',frame=1,at=10),dict(type='C',frame=0,at=10)])])
        r=gc.correlate(data,[(2,5),(4,7)])
        self.assertEqual(10,r['sampledWeightsMs']['gcPoll']);self.assertEqual(5,r['overlapWithGcSuspensionMs']['gcPoll'])
        self.assertIn('not CPU attribution',r['interpretation'])


if __name__=='__main__':unittest.main()

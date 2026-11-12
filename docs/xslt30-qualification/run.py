"""Bound one unchanged source-built XSLT catalog attempt; retain failures and partial runs."""
import argparse
import contextlib
import ctypes
from ctypes import wintypes
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import time


@contextlib.contextmanager
def process_error_mode():
    kernel = ctypes.WinDLL('kernel32', use_last_error=True)
    kernel.GetErrorMode.restype = wintypes.UINT
    kernel.SetErrorMode.argtypes = [wintypes.UINT]
    previous = kernel.GetErrorMode()
    kernel.SetErrorMode(previous | 0x0001 | 0x0002 | 0x8000)
    try:
        yield
    finally:
        kernel.SetErrorMode(previous)


class BasicLimits(ctypes.Structure):
    _fields_ = [('process_time', ctypes.c_int64), ('job_time', ctypes.c_int64),
                ('flags', wintypes.DWORD), ('minimum_working_set', ctypes.c_size_t),
                ('maximum_working_set', ctypes.c_size_t), ('active_process_limit', wintypes.DWORD),
                ('affinity', ctypes.c_size_t), ('priority', wintypes.DWORD), ('scheduling', wintypes.DWORD)]


class IoCounters(ctypes.Structure):
    _fields_ = [(name, ctypes.c_uint64) for name in
                ['read_operations', 'write_operations', 'other_operations', 'read_bytes', 'write_bytes', 'other_bytes']]


class ExtendedLimits(ctypes.Structure):
    _fields_ = [('basic', BasicLimits), ('io', IoCounters),
                ('process_memory', ctypes.c_size_t), ('job_memory', ctypes.c_size_t),
                ('peak_process_memory', ctypes.c_size_t), ('peak_job_memory', ctypes.c_size_t)]


class BoundedJob:
    def __init__(self, memory_limit):
        self.kernel = ctypes.WinDLL('kernel32', use_last_error=True)
        self.kernel.CreateJobObjectW.argtypes = [ctypes.c_void_p, wintypes.LPCWSTR]
        self.kernel.CreateJobObjectW.restype = wintypes.HANDLE
        self.kernel.SetInformationJobObject.argtypes = [wintypes.HANDLE, ctypes.c_int, ctypes.c_void_p, wintypes.DWORD]
        self.kernel.AssignProcessToJobObject.argtypes = [wintypes.HANDLE, wintypes.HANDLE]
        self.kernel.TerminateJobObject.argtypes = [wintypes.HANDLE, wintypes.UINT]
        self.kernel.CloseHandle.argtypes = [wintypes.HANDLE]
        self.handle = self.kernel.CreateJobObjectW(None, None)
        if not self.handle:
            raise ctypes.WinError(ctypes.get_last_error())
        limits = ExtendedLimits()
        limits.basic.flags = 0x2000 | 0x0200  # Kill process tree on close; aggregate committed-memory limit.
        limits.job_memory = memory_limit
        if not self.kernel.SetInformationJobObject(self.handle, 9, ctypes.byref(limits), ctypes.sizeof(limits)):
            error = ctypes.WinError(ctypes.get_last_error())
            self.close()
            raise error

    def assign(self, process):
        if not self.kernel.AssignProcessToJobObject(self.handle, wintypes.HANDLE(int(process._handle))):
            raise ctypes.WinError(ctypes.get_last_error())

    def terminate(self):
        if not self.kernel.TerminateJobObject(self.handle, 124):
            raise ctypes.WinError(ctypes.get_last_error())

    def close(self):
        if self.handle:
            self.kernel.CloseHandle(self.handle)
            self.handle = None


def preflight(packet):
    if os.name != 'nt':
        raise ValueError('This recorded Windows qualification requires Windows job/error-mode controls')
    if (packet / 'RUN.json').exists():
        raise ValueError('Existing attempt receipt; use a fresh packet, never overwrite an original run')
    plan = json.loads((packet / 'PLAN.json').read_text(encoding='utf-8'))
    if not (1 <= plan['wallTimeoutSeconds'] <= 1200) or plan['memoryLimitBytes'] != 8 * 1024**3:
        raise ValueError('Unexpected qualification limits')
    manifest = json.loads((packet / 'RUNTIME-HASHES.json').read_text(encoding='utf-8'))
    for name, expected in manifest.items():
        path = (packet / 'runtime' / name).resolve()
        if not path.is_relative_to(packet / 'runtime') or hashlib.sha256(path.read_bytes()).hexdigest() != expected:
            raise ValueError('Runtime hash mismatch: ' + name)
    corpus = packet / 'corpus'
    revision = subprocess.check_output(['git', '-C', str(corpus), 'rev-parse', 'HEAD'], text=True).strip()
    status = subprocess.check_output(['git', '-C', str(corpus), 'status', '--porcelain'], text=True)
    if revision != plan['corpusCommit'] or status.strip():
        raise ValueError('Corpus differs from the clean pinned input')
    return plan


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--packet', required=True, type=Path)
    args = parser.parse_args()
    packet = args.packet.resolve()
    plan = preflight(packet)
    work = packet / 'work'
    work.mkdir(exist_ok=False)
    command = [shutil.which('dotnet'), str(packet / 'runtime/Bosak.Xslt.Conformance.dll'),
               '--processor-profile', 'legacy', str(packet / 'corpus/catalog.xml')]
    receipt = dict(status='Prepared', command=command, sourceCommit=plan['sourceCommit'],
                   corpusCommit=plan['corpusCommit'], timeoutSeconds=plan['wallTimeoutSeconds'],
                   memoryLimitBytes=plan['memoryLimitBytes'], errorModeScope='Python runner and its child only',
                   processorProfile='Legacy', catalogProfile='Xslt30', schemaAware=False, success=False)

    def save():
        temporary = packet / 'RUN.json.tmp'
        temporary.write_text(json.dumps(receipt, indent=2) + '\n', encoding='utf-8')
        temporary.replace(packet / 'RUN.json')

    save()
    process = None
    job = BoundedJob(plan['memoryLimitBytes'])
    started = time.monotonic()
    try:
        with (packet / 'stdout.log').open('wb') as stdout, (packet / 'stderr.log').open('wb') as stderr:
            with process_error_mode():
                process = subprocess.Popen(command, cwd=work, stdout=stdout, stderr=stderr,
                                           creationflags=subprocess.CREATE_NO_WINDOW)
            try:
                job.assign(process)
            except Exception:
                process.kill()
                process.wait(timeout=10)
                raise
            receipt.update(status='Running', processId=process.pid, startedUtc=time.strftime('%Y-%m-%dT%H:%M:%SZ', time.gmtime()))
            save()
            next_progress = 0
            while process.poll() is None:
                elapsed = time.monotonic() - started
                if elapsed >= plan['wallTimeoutSeconds']:
                    receipt['status'] = 'TimedOut'
                    job.terminate()
                    process.wait(timeout=15)
                    break
                if elapsed >= next_progress:
                    last = work / 'last_test.txt'
                    case = last.read_text(encoding='utf-8-sig', errors='replace') if last.exists() else '(startup)'
                    print(f'Elapsed {elapsed:.0f}s; current test: {case}', flush=True)
                    next_progress = elapsed + 20
                time.sleep(0.25)
            receipt['runnerExitCode'] = process.returncode
            if receipt['status'] == 'Running':
                receipt['status'] = 'Completed'
    except Exception as error:
        receipt.update(status='ControllerFailure', error=repr(error))
    finally:
        job.close()
        if process is not None and process.poll() is None:
            process.wait(timeout=15)
        receipt['elapsedSeconds'] = round(time.monotonic() - started, 3)
        text = (packet / 'stdout.log').read_text(encoding='utf-8-sig', errors='replace')
        counts = {}
        for key in ['Total', 'Passed', 'Failed', 'Skipped', 'Unselected', 'Selected', 'Selected skipped', 'Missing test sets']:
            match = re.search(r'^' + re.escape(key) + r':\s*(\d+)\s*$', text, re.M)
            if match:
                counts[key] = int(match[1])
        receipt['counts'] = counts
        receipt['completeCatalogCounts'] = (counts.get('Total') == plan['totalCases']
            and counts.get('Unselected') == 0 and counts.get('Missing test sets') == 0)
        receipt['success'] = (receipt['status'] == 'Completed' and receipt.get('runnerExitCode') == 0
            and receipt['completeCatalogCounts'] and counts.get('Failed') == 0)
        for name in ['stdout.log', 'stderr.log']:
            receipt[name + 'Sha256'] = hashlib.sha256((packet / name).read_bytes()).hexdigest()
        save()
    print(json.dumps(receipt, indent=2), flush=True)
    return 0 if receipt['success'] else 1


if __name__ == '__main__':
    raise SystemExit(main())

"""Observe one unchanged Windows XSLT gate; never retry or reinterpret qualification."""
import argparse
import ctypes
from ctypes import wintypes
import datetime
import hashlib
import importlib.util
import json
import locale
import os
from pathlib import Path
import platform
import shutil
import subprocess
import sys
import time

ROOT = Path(__file__).resolve().parents[1]
INTERVAL = 1.0
CONTROLLER_TIMEOUT = 1800  # Outer lifetime safeguard; the unchanged child cap is 1200s.
ENVIRONMENT_KEYS = ('ImageOS', 'ImageVersion', 'RUNNER_OS', 'RUNNER_ARCH', 'RUNNER_NAME',
    'RUNNER_ENVIRONMENT', 'GITHUB_RUN_ID', 'GITHUB_RUN_ATTEMPT', 'GITHUB_SHA', 'GITHUB_REF',
    'PROCESSOR_IDENTIFIER', 'NUMBER_OF_PROCESSORS', 'DOTNET_ROOT', 'DOTNET_ROLL_FORWARD',
    'DOTNET_TieredCompilation', 'DOTNET_ReadyToRun', 'DOTNET_gcServer', 'DOTNET_GCHeapHardLimit')
NATIVE_NAMES = {'dotnet.exe', 'coreclr.dll', 'clrjit.dll', 'hostfxr.dll', 'hostpolicy.dll', 'clrgc.dll', 'clrgcexp.dll'}
REQUIRED_NATIVE = {'dotnet.exe', 'coreclr.dll', 'clrjit.dll'}


def utc():
    return datetime.datetime.now(datetime.timezone.utc).isoformat()


def write_json(path, value):
    temporary = path.with_suffix(path.suffix + '.tmp')
    temporary.write_bytes((json.dumps(value, indent=2) + '\n').encode())
    temporary.replace(path)


def read_json(path):
    try:
        return json.loads(path.read_text(encoding='utf-8-sig'))
    except (FileNotFoundError, json.JSONDecodeError):
        return None


def file_identity(path):
    data = path.read_bytes()
    return dict(path=str(path.resolve()), bytes=len(data), sha256=hashlib.sha256(data).hexdigest())


def load_module(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


class FileTime(ctypes.Structure):
    _fields_ = [('low', wintypes.DWORD), ('high', wintypes.DWORD)]

    def ticks(self):
        return (self.high << 32) | self.low


class MemoryCounters(ctypes.Structure):
    _fields_ = [('cb', wintypes.DWORD), ('page_faults', wintypes.DWORD)] + [
        (name, ctypes.c_size_t) for name in ('peak_working_set', 'working_set', 'peak_paged_pool',
        'paged_pool', 'peak_nonpaged_pool', 'nonpaged_pool', 'commit', 'peak_commit', 'private_commit')]


class IoCounters(ctypes.Structure):
    _fields_ = [(name, ctypes.c_uint64) for name in
               ('read_ops', 'write_ops', 'other_ops', 'read_bytes', 'write_bytes', 'other_bytes')]


class ProcessEntry(ctypes.Structure):
    _fields_ = [('size', wintypes.DWORD), ('usage', wintypes.DWORD), ('pid', wintypes.DWORD),
        ('heap', ctypes.c_size_t), ('module', wintypes.DWORD), ('threads', wintypes.DWORD),
        ('parent', wintypes.DWORD), ('priority', wintypes.LONG), ('flags', wintypes.DWORD),
        ('exe', wintypes.WCHAR * 260)]


class Windows:
    def __init__(self):
        self.kernel = ctypes.WinDLL('kernel32', use_last_error=True)
        self.psapi = ctypes.WinDLL('psapi', use_last_error=True)
        functions = [
            (self.kernel, 'OpenProcess', [wintypes.DWORD, wintypes.BOOL, wintypes.DWORD], wintypes.HANDLE),
            (self.kernel, 'CloseHandle', [wintypes.HANDLE], wintypes.BOOL),
            (self.kernel, 'GetProcessTimes', [wintypes.HANDLE] + [ctypes.POINTER(FileTime)] * 4, wintypes.BOOL),
            (self.kernel, 'GetProcessIoCounters', [wintypes.HANDLE, ctypes.POINTER(IoCounters)], wintypes.BOOL),
            (self.kernel, 'GetProcessAffinityMask', [wintypes.HANDLE, ctypes.POINTER(ctypes.c_size_t), ctypes.POINTER(ctypes.c_size_t)], wintypes.BOOL),
            (self.kernel, 'IsProcessInJob', [wintypes.HANDLE, wintypes.HANDLE, ctypes.POINTER(wintypes.BOOL)], wintypes.BOOL),
            (self.kernel, 'WaitForSingleObject', [wintypes.HANDLE, wintypes.DWORD], wintypes.DWORD),
            (self.kernel, 'CreateToolhelp32Snapshot', [wintypes.DWORD, wintypes.DWORD], wintypes.HANDLE),
            (self.kernel, 'Process32FirstW', [wintypes.HANDLE, ctypes.POINTER(ProcessEntry)], wintypes.BOOL),
            (self.kernel, 'Process32NextW', [wintypes.HANDLE, ctypes.POINTER(ProcessEntry)], wintypes.BOOL),
            (self.kernel, 'QueryInformationJobObject', [wintypes.HANDLE, ctypes.c_int, ctypes.c_void_p, wintypes.DWORD, ctypes.c_void_p], wintypes.BOOL),
            (self.psapi, 'GetProcessMemoryInfo', [wintypes.HANDLE, ctypes.POINTER(MemoryCounters), wintypes.DWORD], wintypes.BOOL),
            (self.psapi, 'EnumProcessModulesEx', [wintypes.HANDLE, ctypes.c_void_p, wintypes.DWORD, ctypes.POINTER(wintypes.DWORD), wintypes.DWORD], wintypes.BOOL),
            (self.psapi, 'GetModuleFileNameExW', [wintypes.HANDLE, wintypes.HMODULE, wintypes.LPWSTR, wintypes.DWORD], wintypes.DWORD)]
        for library, name, args, result in functions:
            fn = getattr(library, name)
            fn.argtypes, fn.restype = args, result

    @staticmethod
    def require(value):
        if not value:
            raise ctypes.WinError(ctypes.get_last_error())

    def parents(self):
        handle = self.kernel.CreateToolhelp32Snapshot(2, 0)
        if handle == ctypes.c_void_p(-1).value:
            raise ctypes.WinError(ctypes.get_last_error())
        try:
            row = ProcessEntry()
            row.size = ctypes.sizeof(row)
            result = {}
            present = self.kernel.Process32FirstW(handle, ctypes.byref(row))
            while present:
                result[row.pid] = row.parent
                present = self.kernel.Process32NextW(handle, ctypes.byref(row))
            return result
        finally:
            self.kernel.CloseHandle(handle)

    def observer_job(self):
        # NULL queries this observer's immediate job, not the child's nested job.
        values = (wintypes.DWORD * 2)()
        ok = self.kernel.QueryInformationJobObject(None, 15, ctypes.byref(values), ctypes.sizeof(values), None)
        return (dict(scope='Observer immediate job only', cpuRateControlFlags=values[0], rawCpuRateOrWeight=values[1])
                if ok else dict(scope='Observer immediate job only', unavailable=ctypes.get_last_error()))


def belongs_to(pid, ancestor, parents):
    seen = set()
    while pid and pid not in seen:
        if pid == ancestor:
            return True
        seen.add(pid)
        pid = parents.get(pid, 0)
    return False


class ProcessSampler:
    def __init__(self, api, pid):
        self.api, self.pid = api, pid
        self.handle = api.kernel.OpenProcess(0x0400 | 0x0010 | 0x100000, False, pid)
        api.require(self.handle)

    def close(self):
        if self.handle:
            self.api.kernel.CloseHandle(self.handle)
            self.handle = None

    def active(self):
        return self.api.kernel.WaitForSingleObject(self.handle, 0) == 258

    def times(self):
        creation, exit_time, kernel, user = FileTime(), FileTime(), FileTime(), FileTime()
        self.api.require(self.api.kernel.GetProcessTimes(self.handle, ctypes.byref(creation), ctypes.byref(exit_time), ctypes.byref(kernel), ctypes.byref(user)))
        return dict(creationFileTime=creation.ticks(), exitFileTime=exit_time.ticks() if not self.active() else None,
                    kernelCpuSeconds=kernel.ticks() / 10000000, userCpuSeconds=user.ticks() / 10000000)

    def sample(self):
        record = self.times()
        memory, io = MemoryCounters(), IoCounters()
        memory.cb = ctypes.sizeof(memory)
        self.api.require(self.api.psapi.GetProcessMemoryInfo(self.handle, ctypes.byref(memory), memory.cb))
        self.api.require(self.api.kernel.GetProcessIoCounters(self.handle, ctypes.byref(io)))
        record.update(workingSetBytes=memory.working_set, reportedPeakWorkingSetBytes=memory.peak_working_set,
            privateCommitBytes=memory.private_commit, reportedPeakCommitBytes=memory.peak_commit,
            pageFaults=memory.page_faults, io={name: getattr(io, name) for name, _ in IoCounters._fields_})
        return record

    def context(self):
        process_mask, system_mask, in_job = ctypes.c_size_t(), ctypes.c_size_t(), wintypes.BOOL()
        self.api.require(self.api.kernel.GetProcessAffinityMask(self.handle, ctypes.byref(process_mask), ctypes.byref(system_mask)))
        self.api.require(self.api.kernel.IsProcessInJob(self.handle, None, ctypes.byref(in_job)))
        return dict(pid=self.pid, processAffinityMask=process_mask.value, systemAffinityMask=system_mask.value,
                    affinityCpuCount=process_mask.value.bit_count(), inAnyJob=bool(in_job.value),
                    childJobCpuEntitlement='Not available through observer NULL-job query', **self.times())

    def modules(self):
        modules = (wintypes.HMODULE * 4096)()
        needed = wintypes.DWORD()
        self.api.require(self.api.psapi.EnumProcessModulesEx(self.handle, modules, ctypes.sizeof(modules), ctypes.byref(needed), 3))
        if needed.value > ctypes.sizeof(modules):
            raise ValueError('Module snapshot exceeds fixed bound')
        paths = []
        for module in modules[:needed.value // ctypes.sizeof(wintypes.HMODULE)]:
            buffer = ctypes.create_unicode_buffer(32768)
            length = self.api.psapi.GetModuleFileNameExW(self.handle, module, buffer, len(buffer))
            self.api.require(length)
            path = Path(buffer.value)
            if path.name.lower() in NATIVE_NAMES:
                paths.append(path)
        return paths


def validate_paths(corpus, assembly, output):
    if output.exists():
        raise ValueError('Existing telemetry output; use a fresh attempt, never overwrite receipts')
    if output.is_relative_to(corpus) or output.is_relative_to(assembly.parent):
        raise ValueError('Telemetry output must be outside source corpus and runtime')
    if not assembly.is_file() or assembly.name != 'Bosak.Xslt.Conformance.dll':
        raise ValueError('Build the unmodified conformance runner first')


def host_record(api, output):
    records = {}
    for label, args in [('info', ['--info']), ('runtimes', ['--list-runtimes']), ('sdks', ['--list-sdks'])]:
        result = subprocess.run(['dotnet', *args], stdout=subprocess.PIPE, stderr=subprocess.STDOUT, timeout=30)
        (output / ('dotnet-' + label + '.log')).write_bytes(result.stdout)
        records[label] = result.returncode
    return dict(capturedUtc=utc(), platform=platform.platform(), python=sys.version, pythonExecutable=file_identity(Path(sys.executable)),
        logicalCpuCount=os.cpu_count(), pointerBits=ctypes.sizeof(ctypes.c_void_p) * 8,
        locale=locale.getlocale(), timezone=time.tzname, environment={k: os.environ[k] for k in ENVIRONMENT_KEYS if k in os.environ},
        observerJob=api.observer_job(), dotnetCommands=records,
        limits='Affinity is observable; VM entitlement and child nested-job CPU caps are not established.')


def gate_command(corpus, assembly, packet):
    return [sys.executable, str(ROOT / 'tools/xslt30_gate.py'), '--corpus', str(corpus), '--assembly', str(assembly), '--output', str(packet)]


def outcome(receipt, result, run, app, hashes, source_hashes, sample_count, module_names):
    receipt['gateSuccess'] = bool(receipt['status'] == 'Completed' and receipt.get('gateControllerExitCode') == 0
        and result and result['success'] and run and run['success'])
    receipt['applicationMatchesGate'] = hashes == app
    receipt['sourceApplicationUnchanged'] = source_hashes == app
    receipt['telemetryCaptured'] = bool(sample_count >= 2 and REQUIRED_NATIVE.issubset(module_names)
        and hashes == app and source_hashes == app)
    return 0 if receipt['gateSuccess'] and receipt['telemetryCaptured'] else (1 if not receipt['gateSuccess'] else 2)


def run_once(corpus, assembly, output):
    validate_paths(corpus, assembly, output)
    gate = load_module('production_gate', ROOT / 'tools/xslt30_gate.py')
    bounded = load_module('bounded_control', ROOT / 'docs/xslt30-qualification/run.py')
    policy, _, _, _ = gate.baseline()
    if policy['wallTimeoutSeconds'] != 1200 or policy['memoryLimitBytes'] != 8589934592:
        raise ValueError('Unexpected production bounds')
    app = gate.runtime_hashes(assembly.parent)
    if len(app) != 32:
        raise ValueError('Expected exact 32-file application payload')
    output.mkdir(parents=True)
    try:
        api = Windows()
        write_json(output / 'HOST.json', host_record(api, output))
    except Exception as error:
        write_json(output / 'TELEMETRY.json', dict(status='HostCaptureFailure', gateSuccess=False,
            telemetryCaptured=False, controllerError=repr(error), qualificationLaunched=False))
        return 2
    write_json(output / 'APPLICATION-BEFORE.json', app)
    write_json(output / 'OBSERVER.json', dict(source=file_identity(Path(__file__)), sampleIntervalSeconds=INTERVAL,
        productionBounds=dict(seconds=1200, bytes=8589934592), controllerLifetimeSeconds=CONTROLLER_TIMEOUT,
        interpretation='Diagnostic observer overhead is unmeasured; not an untouched performance benchmark.', retries=0))
    packet = output / 'gate'
    command = gate_command(corpus, assembly, packet)
    receipt = dict(status='Prepared', gateSuccess=False, telemetryCaptured=False, command=command, errors=[])
    sampler, process, lifetime = None, None, None
    samples, modules = [], {}
    started = time.monotonic()
    try:
        # Reuse the reviewed job structs, but impose only kill-on-close, no outer memory/CPU cap.
        lifetime = bounded.BoundedJob.__new__(bounded.BoundedJob)
        lifetime.kernel = api.kernel
        api.kernel.CreateJobObjectW.argtypes, api.kernel.CreateJobObjectW.restype = [ctypes.c_void_p, wintypes.LPCWSTR], wintypes.HANDLE
        api.kernel.SetInformationJobObject.argtypes = [wintypes.HANDLE, ctypes.c_int, ctypes.c_void_p, wintypes.DWORD]
        api.kernel.AssignProcessToJobObject.argtypes = [wintypes.HANDLE, wintypes.HANDLE]
        lifetime.handle = api.kernel.CreateJobObjectW(None, None)
        api.require(lifetime.handle)
        limits = bounded.ExtendedLimits()
        limits.basic.flags = 0x2000
        api.require(api.kernel.SetInformationJobObject(lifetime.handle, 9, ctypes.byref(limits), ctypes.sizeof(limits)))
        with (output / 'gate-controller.log').open('xb') as log, (output / 'samples.jsonl').open('xb') as stream:
            with bounded.process_error_mode():
                process = subprocess.Popen(command, stdout=log, stderr=subprocess.STDOUT, creationflags=subprocess.CREATE_NO_WINDOW)
            lifetime.assign(process)
            receipt.update(status='Running', controllerPid=process.pid)
            next_sample, next_modules = 0.0, 0.0
            while process.poll() is None:
                elapsed = time.monotonic() - started
                if elapsed >= CONTROLLER_TIMEOUT:
                    receipt['status'] = 'ControllerTimedOut'
                    break
                run = read_json(packet / 'RUN.json')
                if run and sampler is None and run['status'] == 'Running':
                    if not belongs_to(run['processId'], process.pid, api.parents()):
                        raise ValueError('Recorded child is not in this controller process tree')
                    sampler = ProcessSampler(api, run['processId'])
                    write_json(output / 'CHILD.json', sampler.context())
                if sampler and sampler.active() and elapsed >= next_sample:
                    began = time.monotonic()
                    try:
                        row = sampler.sample()
                        marker = packet / 'work/last_test.txt'
                        row.update(observedUtc=utc(), observerElapsedSeconds=elapsed,
                            lastRecordedCase=marker.read_text(encoding='utf-8-sig').strip() if marker.exists() else None)
                        row['collectionSeconds'] = time.monotonic() - began
                        stream.write((json.dumps(row) + '\n').encode()); stream.flush(); samples.append(row)
                    except OSError as error:
                        receipt['errors'].append(dict(kind='sample', observedUtc=utc(), error=str(error)))
                    next_sample = elapsed + INTERVAL
                if sampler and sampler.active() and elapsed >= next_modules and not REQUIRED_NATIVE.issubset(modules):
                    try:
                        for path in sampler.modules():
                            key = path.name.lower()
                            if key not in modules:
                                modules[key] = dict(observedLoaded=True, **file_identity(path))
                                if key == 'coreclr.dll':
                                    sibling = path.parent / 'System.Private.CoreLib.dll'
                                    if sibling.exists():
                                        modules['system.private.corelib.dll'] = dict(observedLoaded=False, relation='Sibling of observed coreclr.dll', **file_identity(sibling))
                        write_json(output / 'NATIVE-MODULES.json', modules)
                    except OSError as error:
                        receipt['errors'].append(dict(kind='module-snapshot', observedUtc=utc(), error=str(error)))
                    next_modules = elapsed + 10
                time.sleep(0.1)
            if process.poll() is not None:
                receipt.update(status='Completed', gateControllerExitCode=process.returncode)
            if sampler:
                write_json(output / 'FINAL-TIMES.json', sampler.times())
    except Exception as error:
        receipt.update(status='ControllerFailure', controllerError=repr(error))
    finally:
        if lifetime:
            lifetime.close()
        if process and process.poll() is None:
            process.wait(timeout=15)
        if sampler:
            sampler.close()
        receipt['observerElapsedSeconds'] = time.monotonic() - started
        receipt['samples'] = len(samples)
        receipt['observedNativeNames'] = sorted(modules)
        receipt['missingRequiredNativeNames'] = sorted(REQUIRED_NATIVE - modules.keys())
        result, run = read_json(packet / 'GATE.json'), read_json(packet / 'RUN.json')
        hashes = read_json(packet / 'RUNTIME-HASHES.json')
        exit_code = outcome(receipt, result, run, app, hashes, gate.runtime_hashes(assembly.parent), len(samples), modules)
        write_json(output / 'TELEMETRY.json', receipt)
    print(json.dumps(receipt, indent=2))
    return exit_code


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--corpus', required=True, type=Path)
    parser.add_argument('--output', required=True, type=Path)
    parser.add_argument('--assembly', type=Path, default=ROOT / 'tests/Bosak.Xslt.Conformance/bin/Release/net10.0/Bosak.Xslt.Conformance.dll')
    args = parser.parse_args()
    if sys.platform != 'win32':
        parser.error('Windows process telemetry requires Windows')
    if ctypes.sizeof(ctypes.c_void_p) != 8:
        parser.error('This qualification requires a 64-bit observer')
    return run_once(args.corpus.resolve(), args.assembly.resolve(), args.output.resolve())


if __name__ == '__main__':
    try:
        raise SystemExit(main())
    except Exception as error:
        print('Telemetry refused: ' + repr(error), file=sys.stderr)
        raise SystemExit(2)

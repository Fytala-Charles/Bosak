"""Run only the qualified retained caller, with preflight and scoped Windows error handling."""
from pathlib import Path
import argparse,contextlib,hashlib,json,os,subprocess,sys,zipfile
ROOT=Path(__file__).resolve().parents[3]

def preflight(layout,output):
    layout=Path(layout).resolve();output=Path(output).resolve()
    if not output.is_relative_to(ROOT):raise ValueError('Output must stay inside the Bosak workspace.')
    if not output.parent.is_dir():raise ValueError('Create the output directory before launching the caller.')
    if output.exists():raise ValueError('Use a fresh output path; existing evidence will not be overwritten.')
    prior=json.loads((ROOT/'docs/foundation-legacy-compatibility/evidence/unchanged-binary.json').read_text(encoding='utf-8'))
    expected=json.loads((ROOT/'docs/foundation-production-admission/evidence/candidate-public-surface.json').read_text(encoding='utf-8'))
    files={'LegacyCaller.dll':prior['callerSha256'],**{a['name']+'.dll':a['sha256'] for a in expected['assemblies']}}
    for name,digest in files.items():
        file=layout/name
        if not file.is_file() or hashlib.sha256(file.read_bytes()).hexdigest()!=digest:raise ValueError('Unexpected or missing binary: '+name)
    with zipfile.ZipFile(ROOT/'docs/foundation-legacy-compatibility/evidence/unchanged-binary-runtime.zip') as archive:
        for name in ['LegacyCaller.deps.json','LegacyCaller.runtimeconfig.json']:
            file=layout/name
            if not file.is_file() or hashlib.sha256(file.read_bytes()).digest()!=hashlib.sha256(archive.read(name)).digest():raise ValueError('Unexpected caller configuration: '+name)
    return ['dotnet',str(layout/'LegacyCaller.dll'),str(output)]

@contextlib.contextmanager
def process_error_mode():
    # Fresh runner process only; CreateProcess children inherit this process error mode.
    if os.name!='nt':yield;return
    import ctypes
    kernel=ctypes.WinDLL('kernel32',use_last_error=True)
    kernel.GetErrorMode.restype=ctypes.c_uint
    kernel.SetErrorMode.argtypes=[ctypes.c_uint];kernel.SetErrorMode.restype=ctypes.c_uint
    previous=kernel.GetErrorMode();kernel.SetErrorMode(previous|0x0001|0x0002)
    try:yield
    finally:kernel.SetErrorMode(previous)

def main(argv=None):
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--layout',required=True,type=Path)
    parser.add_argument('--output',required=True,type=Path)
    args=parser.parse_args(argv)
    try:
        command=preflight(args.layout,args.output)
        with process_error_mode():result=subprocess.run(command,capture_output=True,text=True,timeout=60)
        print(result.stdout,end='');print(result.stderr,end='',file=sys.stderr)
        if result.returncode:return 1
        report=json.loads(args.output.resolve().read_text(encoding='utf-8'))
        if report['passed']!=20 or report['failed']!=0:return 1
        return 0
    except (ValueError,KeyError,OSError,subprocess.TimeoutExpired) as error:
        print('Caller runner refused or failed: '+str(error),file=sys.stderr);return 1

if __name__=='__main__':raise SystemExit(main())

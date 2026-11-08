from pathlib import Path
import hashlib,json,shutil
r=Path.cwd();out=r/'docs/binary-compatibility-execution';(out/'evidence').mkdir(parents=True,exist_ok=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
inventory=json.loads((r/'docs/binary-compatibility-scope/evidence/binary-inventory.json').read_text())
for group in inventory['groups'].values():
 for row in group['files']:assert sha(r/group['workspaceRelativeDirectory']/row['name'])==row['sha256'],row['name']
common=['Bosak.XPath.'+x+'.dll' for x in ['Core','Parser','Compiler','Runtime','Standard','Providers','Api']]
closures={'xmldb':common,'braid':common+['Bosak.XQuery.dll','Bosak.Xslt.dll']}
manifests={}
for consumer in ['xmldb','braid']:
 original=r/f'.guard-tmp/{consumer}-baseline'
 for mode in ['baseline','replacement']:
  dest=r/f'.guard-tmp/binary-{consumer}-{mode}'
  assert not dest.exists(),'Use fresh unique workspace rather than overwriting an existing run.'
  shutil.copytree(original,dest,ignore=shutil.ignore_patterns('obj','.git','TestResults'))
  if mode=='replacement':
   candidate=r/('.guard-tmp/xmldb-candidate/probe/bin/Release/net10.0' if consumer=='xmldb' else '.guard-tmp/braid-candidate/src/Bosak.Braid.Desktop/bin/Release/net10.0')
   for p in dest.rglob('*.dll'):
    if p.name in closures[consumer]:shutil.copyfile(candidate/p.name,p)
  manifests[f'{consumer}.{mode}']={p.relative_to(dest).as_posix():sha(p) for p in sorted(dest.rglob('*')) if p.is_file()}
 baseline=manifests[f'{consumer}.baseline'];replacement=manifests[f'{consumer}.replacement'];assert baseline.keys()==replacement.keys()
 changes=[name for name in baseline if baseline[name]!=replacement[name]]
 assert changes and all(Path(name).name in closures[consumer] for name in changes)
 manifests[f'{consumer}.changes']=changes
(out/'evidence/layout-manifests.json').write_text(json.dumps(manifests,indent=2)+'\n',encoding='utf-8',newline='\n')
(out/'evidence/replacement-allowlist.json').write_text(json.dumps(closures,indent=2)+'\n',encoding='utf-8',newline='\n')
hook=r/'.guard-tmp/binary-load-hook';hook.mkdir(exist_ok=True)
(hook/'Hook.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup></Project>')
header=(r/'.guard-tmp/xmldb-baseline/probe/Program.cs').read_text().split('using System.Text;')[0].replace('Probes persistent XML provider compatibility and snapshot lifetime in isolation.','Records actual assembly loads for isolated binary compatibility qualification.')
(hook/'StartupHook.cs').write_text(header+'''using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

/// <summary>Records engine and consumer assembly loads without referencing their APIs.</summary>
public static class StartupHook
{
    private static readonly object Gate = new();
    /// <summary>Registers a per-process metadata-only assembly-load observer.</summary>
    public static void Initialize()
    {
        AppDomain.CurrentDomain.AssemblyLoad += (_, args) => Record(args.LoadedAssembly);
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) Record(assembly);
    }
    private static void Record(Assembly assembly)
    {
        var name = assembly.GetName();
        if (!(name.Name?.StartsWith("Bosak.", StringComparison.Ordinal) ?? false)) return;
        var directory = Environment.GetEnvironmentVariable("BINARY_QUAL_TRACE_DIR");
        if (directory is null || assembly.IsDynamic || assembly.Location.Length == 0) return;
        Directory.CreateDirectory(directory);
        var row = JsonSerializer.Serialize(new { processId=Environment.ProcessId, processPath=Environment.ProcessPath, fullName=name.FullName, location=assembly.Location, sha256=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location))).ToLowerInvariant() });
        lock(Gate) File.AppendAllText(Path.Combine(directory, Environment.ProcessId + ".jsonl"), row + "\\n");
    }
}
''',encoding='utf-8',newline='\n')
print('Fresh baseline/replacement layouts frozen; every delta is an allowlisted engine DLL; load observer prepared.')

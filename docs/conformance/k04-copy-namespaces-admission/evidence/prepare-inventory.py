from pathlib import Path
E=Path(__file__).resolve().parent;R=E.parents[3];T=R/'.guard-tmp/k04-copy-admission-inventory';T.mkdir(exist_ok=True)
for source,target in [('inventory-program.cs.txt','Program.cs'),('inventory-project.csproj.txt','Probe.csproj')]:
 (T/target).write_bytes((E/source).read_bytes())

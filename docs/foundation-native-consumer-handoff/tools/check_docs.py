"""Check current Markdown source hashes and file destinations; no HTML rendering."""
from pathlib import Path
import hashlib,json,re
P=Path(__file__).resolve().parents[1];R=P.parents[1]
receipt=json.loads((P/'evidence/DOCUMENTATION-CHECKS.json').read_text(encoding='utf-8'))
count=0
for row in receipt['pages']:
    path=R/row['source']
    assert hashlib.sha256(path.read_bytes()).hexdigest()==row['sourceSha256'],row['source']
    source=path.read_text(encoding='utf-8')
    source=re.sub(r'```[\s\S]*?```','',source)
    source=re.sub(r'`[^`]*`','',source)
    for link in re.findall(r'\[[^\[\]\n]*\]\(([^)\n]+)\)',source):
        link=link.strip('<>').split('#')[0]
        if not link or re.match(r'[A-Za-z][A-Za-z0-9+.-]*:',link) or link.startswith('//'):
            continue
        # Historical executable examples are not Markdown destinations.
        if link=='-1' or link.startswith('?x('):
            continue
        assert (path.parent/link).exists(),(row['source'],link)
        count+=1
assert count==receipt['localLinks'] and not receipt['htmlRendered']
print(f'Checked {len(receipt["pages"])} Markdown sources and {count} local file destinations; no HTML rendering.')

from pathlib import Path
import struct,json,hashlib
R=Path(__file__).resolve().parents[4];D=R/'.guard-tmp/k04-normalization-spec';output=R/'src/Bosak.XPath.Standard/Resources/UnicodeNormalization17.bin'
rows={int(line.split(';')[0],16):line.split(';') for line in (D/'UnicodeData.txt').read_text(encoding='utf-8').splitlines()}
excluded=set()
for line in (D/'DerivedNormalizationProps.txt').read_text(encoding='utf-8').splitlines():
 parts=line.split('#')[0].strip().split(';')
 if len(parts)<2 or parts[1].strip()!='Full_Composition_Exclusion':continue
 bounds=parts[0].strip().split('..');excluded.update(range(int(bounds[0],16),int(bounds[-1],16)+1))
ccc={cp:int(r[3]) for cp,r in rows.items() if int(r[3])}
decomp={cp:[int(v,16) for v in r[5].split()] for cp,r in rows.items() if r[5] and not r[5].startswith('<')}
pairs={(v[0],v[1]):cp for cp,v in decomp.items() if len(v)==2 and cp not in excluded};assert all(rows[a][3]=='0' for a,b in pairs)
b=bytearray(b'BUN17\0\0\0');b+=struct.pack('<I',len(ccc))
for cp,value in sorted(ccc.items()):b+=struct.pack('<IB',cp,value)
b+=struct.pack('<I',len(decomp))
for cp,values in sorted(decomp.items()):
 b+=struct.pack('<IB',cp,len(values))
 for v in values:b+=struct.pack('<I',v)
b+=struct.pack('<I',len(pairs))
for (a,c),value in sorted(pairs.items()):b+=struct.pack('<III',a,c,value)
output.write_bytes(b)
print(json.dumps(dict(combiningClasses=len(ccc),decompositions=len(decomp),compositions=len(pairs),bytes=len(b),sha256=hashlib.sha256(b).hexdigest())))

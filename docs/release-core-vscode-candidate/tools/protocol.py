import subprocess, json, threading, queue, zipfile, hashlib, os, time
from pathlib import Path

archive=Path('docs/release-core-vscode-candidate/vscode-bosak-0.2.6-candidate.vsix')
root=Path('D:\\Development\\Bosak\\.guard-tmp\\core-vscode-candidate-20261108\\extracted')
with zipfile.ZipFile(archive) as z:
    z.extractall(root)
    manifest=json.loads(z.read('extension/package.json'))
    assert manifest['version']=='0.2.6'
    script=z.read('extension/out/extension.js').decode()
    assert 'command: serverCommand' in script and 'args: serverArguments' in script
    assert not any(n.endswith('Bosak.LanguageServer.exe') for n in z.namelist())
    deps=json.loads(z.read('extension/server/Bosak.LanguageServer.deps.json'))
    names=[k for k in deps['libraries'] if k.startswith('Bosak.')]
    assert len(names)==10
p=subprocess.Popen(['dotnet',str(root/'extension/server/Bosak.LanguageServer.dll')],stdin=subprocess.PIPE,stdout=subprocess.PIPE,stderr=subprocess.PIPE,env=dict(os.environ,BOSAK_LSP_DEBUG='1'))
stderr=[]
def errors():
    for line in p.stderr: stderr.append(line.decode(errors='replace'))
threading.Thread(target=errors,daemon=True).start()
messages=queue.Queue()
received=[]
def read():
    while True:
        headers={}
        while True:
            line=p.stdout.readline()
            if not line: return
            if line==b'\r\n': break
            key,value=line.decode().split(':',1)
            headers[key.lower()]=value.strip()
        message=json.loads(p.stdout.read(int(headers['content-length'])))
        received.append(message)
        messages.put(message)
threading.Thread(target=read,daemon=True).start()
def send(message):
    raw=json.dumps(dict(jsonrpc='2.0',**message)).encode()
    p.stdin.write(f'Content-Length: {len(raw)}\r\n\r\n'.encode()+raw)
    p.stdin.flush()
def response(identifier):
    while True:
        try: message=messages.get(timeout=30)
        except queue.Empty:
            print('Server status:',p.poll(),flush=True)
            Path('docs/release-core-vscode-candidate/protocol-stderr.txt').write_text(''.join(stderr),encoding='utf-8')
            Path('docs/release-core-vscode-candidate/protocol-messages.json').write_text(json.dumps(received,indent=2),encoding='utf-8')
            raise
        if message.get('id')==identifier and 'method' not in message:
            assert 'error' not in message,message
            return message['result']
        if 'id' in message and 'method' in message:
            send(dict(id=message['id'],result=[] if message['method']=='workspace/configuration' else None))
results=[]
try:
    send(dict(id=1,method='initialize',params=dict(processId=os.getpid(),rootUri='file:///C:/vscode025-initial/',workspaceFolders=[],capabilities=dict(textDocument=dict(diagnostic=dict(dynamicRegistration=False))))))
    initialized=response(1)
    assert initialized['serverInfo']['version'].startswith('1.81.1-preview.c0b926de')
    Path('docs/release-core-vscode-candidate/protocol-initialize.json').write_text(json.dumps(initialized,indent=2),encoding='utf-8')
    send(dict(method='initialized',params={}))
    time.sleep(1)
    send(dict(id=100,method='textDocument/hover',params=dict(textDocument=dict(uri='file:///C:/vscode025-initial/test.xpath'),position=dict(line=0,character=0))))
    print('Hover response',response(100),flush=True)
    declaration="<xsl:function name='divide'><xsl:param name='a'/><xsl:param name='b' required='no' select='3'/><xsl:sequence select='$a div $b'/></xsl:function>"
    for i,(expression,expected) in enumerate([('divide(a := 9)',False),('divide(?, b := 3)(9)',False),('divide(unknown := 9)',True)],2):
        uri=f'file:///C:/vscode025-initial/case{i}.xsl'
        text="<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='4.0'>"+declaration+"<xsl:template match='/'><xsl:value-of select='"+expression+"'/></xsl:template></xsl:stylesheet>"
        send(dict(method='textDocument/didOpen',params=dict(textDocument=dict(uri=uri,languageId='xslt',version=1,text=text))))
        time.sleep(.2)
        send(dict(id=i,method='textDocument/diagnostic',params=dict(textDocument=dict(uri=uri),identifier='bosak')))
        result=response(i)
        assert bool(result['items'])==expected,result
        if expected: assert any('XPST0017' in d['message'] for d in result['items'])
        results.append(dict(expression=expression,diagnostics=result['items']))
    send(dict(id=99,method='shutdown',params=None)); response(99)
    send(dict(method='exit',params=None))
    p.wait(timeout=10)
finally:
    if p.poll() is None: p.kill()
    Path('docs/release-core-vscode-candidate/protocol-stderr.txt').write_text(''.join(stderr),encoding='utf-8')
    Path('docs/release-core-vscode-candidate/protocol-messages.json').write_text(json.dumps(received,indent=2),encoding='utf-8')
receipt=dict(extensionVersion='0.2.6',engineVersion='1.81.1-preview.c0b926de',archiveSha256=hashlib.sha256(archive.read_bytes()).hexdigest(),bundledProjects=names,packagedStdioChecks=results)
Path('docs/release-core-vscode-candidate/PROTOCOL.json').write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf-8')
print('VSIX manifest, ten bundled project versions, executable launch and three stdio diagnostic checks passed')

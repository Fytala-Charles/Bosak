const vscode = require('vscode');
const fs = require('node:fs');
const path = require('node:path');
exports.run = async () => {
    const extension = vscode.extensions.getExtension('fytala.vscode-bosak');
    if (!extension) throw new Error('Candidate extension missing');
    if (extension.packageJSON.version !== '0.2.6') throw new Error('Wrong installed version');
    if (!extension.extensionPath.includes('extensions')) throw new Error('Not the installed payload');
    if (vscode.workspace.getConfiguration('bosak').get('server.path')) throw new Error('Unexpected server override');
    await extension.activate();
    await new Promise(resolve => setTimeout(resolve, 3000));
    const folder = path.resolve(__dirname, 'fixtures');
    fs.mkdirSync(folder, { recursive: true });
    const results = [];
    for (const [name, expression, invalid] of [['valid', 'divide(a := 9)', false], ['invalid', 'divide(unknown := 9)', true]]) {
        const text = `<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='4.0'><xsl:function name='divide'><xsl:param name='a'/><xsl:param name='b' required='no' select='3'/><xsl:sequence select='$a div $b'/></xsl:function><xsl:template match='/'><xsl:value-of select='${expression}'/></xsl:template></xsl:stylesheet>`;
        const file = path.join(folder, `${name}.xsl`);
        fs.writeFileSync(file, text);
        const document = await vscode.workspace.openTextDocument(file);
        await vscode.window.showTextDocument(document);
        if (document.languageId !== 'xslt') throw new Error('Unexpected language mode');
        fs.writeFileSync(path.join(__dirname, 'progress.json'),JSON.stringify({name,uri:document.uri.toString(),scheme:document.uri.scheme,patternScore:vscode.languages.match([{pattern:'**/*.xsl'}],document),tabs:vscode.window.tabGroups.all.flatMap(g=>g.tabs.map(t=>({type:t.input.constructor.name,uri:t.input.uri?.toString()})))}));
        let diagnostics = [];
        for (let i = 0; i < 60; i++) {
            await new Promise(resolve => setTimeout(resolve, 500));
            diagnostics = vscode.languages.getDiagnostics(document.uri);
            if (invalid && diagnostics.some(d => d.message.includes('XPST0017'))) break;
            if (!invalid && i >= 5) break;
        }
        if (invalid !== diagnostics.some(d => d.message.includes('XPST0017'))) throw new Error(`Unexpected diagnostics for ${name}: ${JSON.stringify(diagnostics)}`);
        if (!invalid && diagnostics.length) throw new Error('Valid stylesheet has diagnostics');
        results.push({ name, languageId: document.languageId, diagnostics: diagnostics.map(d => d.message) });
        if (invalid) {
            const edit = new vscode.WorkspaceEdit();
            edit.replace(document.uri,new vscode.Range(document.positionAt(0),document.positionAt(document.getText().length)),text.replace(expression,'divide(a := 9)'));
            await vscode.workspace.applyEdit(edit);
            for (let i=0;i<40;i++) {
                await new Promise(resolve=>setTimeout(resolve,250));
                if (!vscode.languages.getDiagnostics(document.uri).length) break;
            }
            if (vscode.languages.getDiagnostics(document.uri).length) throw new Error('Corrected document still has diagnostics');
            results.push({ name: 'corrected-live', diagnostics: [] });
        }
    }
    for (const language of ['xpath', 'xquery']) {
        const file = path.join(folder, `ordinary.${language === 'xquery' ? 'xq' : 'xpath'}`);
        fs.writeFileSync(file, '1 +');
        const doc = await vscode.workspace.openTextDocument(file);
        await vscode.window.showTextDocument(doc);
        if (doc.languageId !== language) throw new Error('Wrong language mode: '+doc.languageId);
        for (let i=0;i<60 && !vscode.languages.getDiagnostics(doc.uri).length;i++) await new Promise(r=>setTimeout(r,500));
        if (!vscode.languages.getDiagnostics(doc.uri).length) throw new Error('Missing invalid '+language+' diagnostic');
        const edit = new vscode.WorkspaceEdit();
        edit.replace(doc.uri,new vscode.Range(doc.positionAt(0),doc.positionAt(doc.getText().length)),'1 + 1');
        await vscode.workspace.applyEdit(edit);
        for(let i=0;i<60;i++){await new Promise(r=>setTimeout(r,250));if(!vscode.languages.getDiagnostics(doc.uri).length)break;}
        if(vscode.languages.getDiagnostics(doc.uri).length)throw new Error('Uncleared '+language+' diagnostic');
        results.push({name:language+'-invalid-and-live-correction',languageId:doc.languageId,diagnostics:[]});
    }
    fs.writeFileSync("D:\\Development\\Bosak\\docs\\release-core-vscode-candidate\\EDITOR.json", JSON.stringify({ extensionVersion: extension.packageJSON.version, results }, null, 2));
};

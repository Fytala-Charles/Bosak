const vscode = require('vscode');
const fs = require('node:fs');
const path = require('node:path');
const assert = require('node:assert/strict');
exports.run = async () => {
 const extension = vscode.extensions.getExtension('fytala.vscode-bosak');
 assert.equal(extension?.packageJSON.version, '0.2.6');
 assert(extension.extensionPath.includes('final-fresh'));
 assert(!vscode.workspace.getConfiguration('bosak').get('server.path'));
 await extension.activate();
 const commands = await vscode.commands.getCommands(true);
 for (const id of ['bosak.evaluateXPath','bosak.runXQuery','bosak.evaluateXQuery','bosak.transformXslt','bosak.runInitialTemplate']) assert(commands.includes(id),id);
 const folder=path.join(__dirname,'fixtures');fs.mkdirSync(folder,{recursive:true});
 const results=[];
 async function open(name,text,language){
  const file=path.join(folder,name);fs.writeFileSync(file,text);
  const doc=await vscode.workspace.openTextDocument(file);await vscode.window.showTextDocument(doc);assert.equal(doc.languageId,language);
  await new Promise(r=>setTimeout(r,600));return doc;
 }
 async function execute(command,expected,...args){
  await vscode.commands.executeCommand(command,...args);
  for(let i=0;i<100;i++){
   const doc=vscode.window.activeTextEditor?.document;
   if(doc?.uri.scheme==='untitled' && doc.getText().trim()===expected){results.push({command,expected,actual:doc.getText().trim(),outputLanguage:doc.languageId,passed:true});return;}
   await new Promise(r=>setTimeout(r,100));
  }
  throw new Error('Missing expected output for '+command+': '+expected+'; active='+vscode.window.activeTextEditor?.document.getText());
 }
 try{
  await open('sum.xpath','sum((1, 2, 3))','xpath');await new Promise(r=>setTimeout(r,3000));
  await execute('bosak.evaluateXPath','6');
  await open('loop.xq','string-join(for $i in 1 to 3 return concat("q", $i), "|")','xquery');
  await execute('bosak.runXQuery','q1|q2|q3');
  await open('alias.xq','6 * 7','xquery');await execute('bosak.evaluateXQuery','42');
  const source=path.join(folder,'input.xml');fs.writeFileSync(source,'<root><n>11</n><n>12</n></root>');
  const style=await open('transform.xsl',`<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"><xsl:output omit-xml-declaration="yes"/><xsl:template match="/"><result><xsl:value-of select="sum(/root/n)"/></result></xsl:template></xsl:stylesheet>`,'xslt');
  await execute('bosak.transformXslt','<result>23</result>',style.uri.toString(),source);
  const initial=await open('initial.xsl',`<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"><xsl:output method="text"/><xsl:template name="start"><xsl:text>initial-42</xsl:text></xsl:template></xsl:stylesheet>`,'xslt');
  await execute('bosak.runInitialTemplate','initial-42',initial.uri.toString(),'start');
 }finally{
  fs.writeFileSync("D:\\Development\\Bosak\\docs\\release-vscode-command-smoke\\COMMANDS.json",JSON.stringify({extensionVersion:extension.packageJSON.version,extensionPath:extension.extensionPath,defaultBundledServer:true,results},null,2));
 }
};

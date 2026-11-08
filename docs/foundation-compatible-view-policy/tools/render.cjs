// Render the current candidate documentation; pass a markdown-it module location if needed.
const fs=require('fs'),path=require('path'),crypto=require('crypto');
const md=require(process.argv[2]||'markdown-it')({html:true});
const root=path.resolve(__dirname,'../../..'),p=path.resolve(__dirname,'..');
const out=path.join(root,'.guard-tmp/compatible-view-policy-render');fs.mkdirSync(out,{recursive:true});
const files=['README.md','ROADMAP.md','docs/ARCHITECTURE.md','docs/FEATURE_REQUESTS.md','docs/INTEGRATION.md','docs/AGENT_HANDOVER.md','docs/IMPLEMENTATION_STATUS.md','docs/DEVELOPMENT_HISTORY.md','docs/DOCUMENTATION_RENDERER_TEST.md','docs/foundation-r2-consolidation/README.md','docs/release-planning/README.md','docs/foundation-compatible-view-policy/README.md','docs/foundation-compatible-view-policy/POLICY.md','docs/foundation-compatible-view-policy/SIGNATURE-PROPOSAL.md','docs/foundation-compatible-view-policy/packaging/RECIPE.md','docs/foundation-compatible-view-policy/packaging/MIGRATION.md'];
const css=path.join(root,'assets/css/fytala-markdown.css');let links=0;const pages=[];
for(const name of files){
 const full=path.join(root,name),source=fs.readFileSync(full,'utf8');
 for(const token of md.parse(source,{}).flatMap(t=>[t,...(t.children||[])])){
  const link=token.type==='link_open'?token.attrGet('href'):token.type==='image'?token.attrGet('src'):null;
  if(!link||/^[a-zA-Z][a-zA-Z0-9+.-]*:/.test(link)||link.startsWith('#')||link.startsWith('//'))continue;
  if(!fs.existsSync(path.resolve(path.dirname(full),decodeURIComponent(link.split('#')[0]))))throw Error('Missing link '+name+': '+link);
  links++;
 }
 const html=`<!doctype html><meta charset="utf-8"><link rel="stylesheet" href="file:///${css.replaceAll('\\','/')}"><main>${md.render(source)}</main>`;
 fs.writeFileSync(path.join(out,name.replaceAll('/','_')+'.html'),html);
 pages.push({source:name,sourceSha256:crypto.createHash('sha256').update(source).digest('hex'),htmlSha256:crypto.createHash('sha256').update(html).digest('hex')});
}
fs.writeFileSync(path.join(p,'evidence/DOCUMENTATION-CHECKS.json'),JSON.stringify({date:'2026-11-05',pages,localLinks:links,cssSha256:crypto.createHash('sha256').update(fs.readFileSync(css)).digest('hex'),scope:'HTML rendering and local destinations; no screenshot/PDF claim'},null,2)+'\n');
console.log(`Rendered ${pages.length} pages; ${links} local links checked.`);

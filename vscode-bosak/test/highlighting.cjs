const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const { Registry, parseRawGrammar } = require('vscode-textmate');
const onig = require('vscode-oniguruma');

(async () => {
  const wasm = fs.readFileSync(require.resolve('vscode-oniguruma/release/onig.wasm'));
  await onig.loadWASM(wasm.buffer.slice(wasm.byteOffset, wasm.byteOffset + wasm.byteLength));
  const registry = new Registry({
    onigLib: Promise.resolve({createOnigScanner: p => new onig.OnigScanner(p), createOnigString: s => new onig.OnigString(s)}),
    loadGrammar: async scope => {
      const name = {'text.xml.xsl':'xslt', 'source.xpath':'xpath'}[scope];
      if (!name) return null;
      const filename = path.join(__dirname, '../syntaxes', name + '.tmLanguage.json');
      return parseRawGrammar(fs.readFileSync(filename, 'utf8'), filename);
    }
  });
  const grammar = await registry.loadGrammar('text.xml.xsl');
  function scopes(text, word) {
    const at = text.indexOf(word);
    assert.ok(at >= 0, word);
    return grammar.tokenizeLine(text).tokens.find(t => t.startIndex <= at && t.endIndex > at).scopes;
  }
  for (const text of [`<xsl:value-of select="concat('a','b')"/>`, `<xsl:value-of select='concat("a","b")'/>`]) {
    assert.ok(scopes(text, 'select').includes('entity.other.attribute-name.xml'));
    assert.ok(scopes(text, 'concat').includes('support.function.xpath'));
    assert.ok(scopes(text, text.includes("'a'") ? "'a'" : '"a"').some(s => s.startsWith('string.quoted.')));
    assert.ok(scopes(text, 'concat').includes('meta.embedded.block.xpath'));
    assert.ok(!scopes(text, 'concat').some(s => s.startsWith('string.quoted.')));
  }
  for (const text of [`<xsl:include href="fn:concat(123)"/>`, `<result select="concat('a','b')"/>`, `<!-- <xsl:value-of select="concat('a','b')"/> -->`]) {
    assert.ok(!scopes(text, 'concat').includes('support.function.xpath'));
  }
  let state;
  const lines = ['<xsl:value-of', ` select="concat(`, ` 'a', 'b')"/>`];
  const results = lines.map(l => {const r = grammar.tokenizeLine(l, state); state = r.ruleStack; return r.tokens;});
  assert.ok(results[1].some(t => t.scopes.includes('support.function.xpath')));
  assert.ok(results[2].some(t => t.scopes.includes('string.quoted.single.xpath')));
  const doubled = `<xsl:value-of select="concat('it''s','b')"/>`;
  assert.ok(scopes(doubled, "''").includes('constant.character.escape.xpath'));
  const variable = `<xsl:value-of select="$name + fn:concat('a','b')"/>`;
  assert.ok(scopes(variable, '$name').includes('variable.other.xpath'));
  console.log('TextMate highlighting checks passed: quotes, function/string scopes, XML exclusions, multiline and doubled quotes.');
  registry.dispose();
})().catch(e => {console.error(e); process.exitCode = 1;});

const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');

const root = path.resolve(__dirname, '..');
const manifest = JSON.parse(fs.readFileSync(path.join(root, 'package.json'), 'utf8'));
assert.deepEqual(manifest.contributes.snippets.map(s => s.language).sort(), ['xpath', 'xquery', 'xslt']);
const prefixes = new Set();
let count = 0;
for (const contribution of manifest.contributes.snippets) {
  const snippets = JSON.parse(fs.readFileSync(path.join(root, contribution.path), 'utf8'));
  for (const [name, snippet] of Object.entries(snippets)) {
    assert.match(snippet.prefix, /^bosak-[a-z-]+$/, name);
    assert.ok(!prefixes.has(snippet.prefix), `Duplicate prefix ${snippet.prefix}`);
    prefixes.add(snippet.prefix);
    assert.ok(snippet.description.length > 10, name);
    assert.ok(Array.isArray(snippet.body) && snippet.body.every(x => typeof x === 'string'), name);
    const body = snippet.body.join('\n');
    // This collection deliberately uses only defaults, mirrored numeric stops and escaped dollars.
    const token = /\\[\\$}]|\$\{(\d+):([^{}]*)\}|\$(\d+)/g;
    const defaults = new Map();
    let zero = 0;
    for (const match of body.matchAll(token)) {
      if (match[1]) {
        const n = Number(match[1]);
        assert.ok(n > 0 && !defaults.has(n), `${name}: duplicate/default zero stop`);
        defaults.set(n, match[2]);
      }
      if (match[3] === '0') zero++;
    }
    assert.equal(zero, 1, `${name}: one final cursor`);
    assert.deepEqual([...defaults.keys()].sort((a, b) => a - b), Array.from({length: defaults.size}, (_, i) => i + 1));
    for (const match of body.matchAll(token)) {
      if (match[3] && match[3] !== '0') assert.ok(defaults.has(Number(match[3])), `${name}: dangling mirror`);
    }
    assert.ok(!body.replace(token, '').includes('$'), `${name}: unescaped dollar or unsupported placeholder`);
    count++;
  }
}
assert.equal(count, 20);
console.log('Snippet registration, 20 unique prefixes, placeholder order/mirrors and literal-dollar escaping passed.');

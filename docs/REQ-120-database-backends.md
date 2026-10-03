<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak engine">
  <br><br>
  <h1>REQ-120 Dossier — Database Backends (Phase 5 Scoping)</h1>
  <p>How XML database adapters plug into the Bosak provider seam, and what to build first</p>
</div>

> **Status:** Scoped (2026-10-03) — Slices 1–3 landed (Slice 3: collection seam + foreign-provider friction, 2026-10-03) · **REQ:** [`REQ-120`](./FEATURE_REQUESTS.md) · **Deciders:** Charles Korthout (Fytala) · **Basis:** seam audit of `main` @ `3cc4a7f` (published v0.12.2-beta surface)

---

## 1. What the docs already promise

- `README.md` Roadmap Phase 5 — "Database backends — XML database adapters" (📋 Planned).
- `docs/ARCHITECTURE.md` §Extensibility — "Database backends | 🔮 Planned | `IXdmNode` implementations over XML databases."
- `docs/FEATURE_REQUESTS.md` §9 roadmap row 6 — Pending, `TBD` REQ.

This dossier scopes that promise against the seam as it actually exists.

## 2. Seam reality (audit 2026-10-03)

### 2.1 The frozen contract

`IXdmNode` (33 members, frozen per `docs/API_FREEZE.md`) is the provider contract. Mandatory for any node provider: `NodeKind`, name accessors, `StringValue`, **`TypedValue`** (no default), **`ToXmlString()`** (no default), `Parent`, `Document`, `Children`/`Attributes`/`Axis`, `IsSameNode`, `DocumentOrder`, `BaseUri`/`DocumentUri`. Optional facets (typed-value absence, schema info, ID/IDREF, DTD, unparsed entities) have default interface implementations — a provider can skip them.

`XDocumentNode` is the reference implementation: per-`XObject` wrapper cache (`ConditionalWeakTable`), identity on the underlying object, composite `DocumentOrder` (per-document order map + global creation sequence for cross-document sorting), `RegisterTree` eager numbering.

### 2.2 Document resolution — what is public today

`EvaluationContext` is the blessed extension point (`API_FREEZE.md`), and the two hooks that matter for databases are already public and sufficient:

| Hook | Purpose |
|------|---------|
| `DocumentLoader` (`Func<string,IXdmNode>?`) | URI → node for `fn:doc`/`fn:document`/`xsl:source-document`(non-streamed)/`fn:transform`/merge inputs. **The DB-scheme hook.** |
| `StreamingDocumentLoader` (`Func<string,IXdmNode>?`) | streamable `xsl:source-document` — bounded-memory record-at-a-time straight off a DB cursor. Never set in-engine; pure host hook. |
| `ResourceUriMapper` | published-URI → local redirect (authored docs). |
| `RegisterDocument(uri, node)` / `LoadDocument(uri)` | pre-cache / cached load with FODC0002/0005 mapping, cache keyed (URI, policy). |

A scheme-dispatching loader (`uri.StartsWith("basex://") ? FetchAndWrap(uri) : default`) makes every consumer work unchanged: fn:doc, fn:document, collection items, xsl:source-document (both modes), xsl:merge-source (which funnels through fn:doc), fn:transform stylesheet locations.

### 2.3 What is NOT public today

- `fn:collection` URI schemes — `Collections`/`CollectionValues` are **internal**. A `db://coll` scheme needs a new **additive** public seam on `EvaluationContext` (SemVer minor; additive changes to the frozen context are allowed).
- Engine friction points that special-case `XDocumentNode` (relevant only to foreign *node-level* providers, not to URI adapters): `FunctionLibrary.LoadDocumentFragment` (:8410 cast), `LoadDocument`'s `RegisterTree` skip for foreign providers (cross-tree ordering degrades), `TransformEngine.IsNodeAttached`, raw-`XElement` whitespace stripping, `fn:copy-of` fallback. All fixable; `TransformEngine` is internal (freely evolvable), `EvaluationContext` is additive-only.

### 2.4 Established idioms

- Fully synchronous pipeline; HTTP via sync-over-async `HttpClient` (`fn:unparsed-text`, FunctionLibrary 5.x) is the established idiom.
- No query/update push-down surface exists (no XQJ-like API, no update facility) — Phase 5 is about **data access**, not XQuery push-down.
- No in-memory `IXdmNode` test doubles exist yet — a DB-adapter suite would be the first; streaming tests show the harness pattern; HTTP adapters can stub with `HttpListener`.

## 3. Ranked adapter shapes

| Shape | Seam fit | Engine changes | Verdict |
|-------|----------|----------------|---------|
| **A. REST/HTTP URI-scheme adapters** (`DocumentLoader` + `StreamingDocumentLoader` dispatch; BaseX REST, eXist REST/RESTXQ, MarkLogic REST) | Everything needed exists and is public/frozen | **None** | **Build first** |
| **B. Native protocol clients** (BaseX socket, eXist XML-RPC) | Same two hooks, different client library | None for the loader; needs a connection/credentials/options surface and per-DB packaging | Later, on demand |
| **C. DB-native `IXdmNode` providers** (MarkLogic-style node-level laziness) | Full frozen 33-member contract incl. `DocumentOrder` composite semantics | Engine friction fixes (§2.3) + collection seam | Only with a committed customer |
| **D. Query/update push-down** | No surface exists | New public API family | Out of Phase 5 scope |

## 4. Recommended scope

### Slice 1 — REST adapter spike (1 session)

- One scheme-dispatching `DatabaseDocumentLoader` (BaseX REST first — simplest wire shape) + streaming variant via `XmlStreamingProvider.Load(responseStream)`.
- Proves the seam end-to-end with **zero engine changes**.
- Tests with a local `HttpListener` stub (first in-memory `IXdmNode` consumer tests in the tree); auth via URI userinfo or per-scheme options delegate.
- Deliverable: spike branch, test suite, go/no-go for Slice 2.

### Slice 2 — `Bosak.XPath.Providers.Database` package (1–2 sessions)

- Generalize to scheme registry: `basex://`, `exist://`, `marklogic://` REST configs (endpoint, credentials, timeouts).
- New package in the 9-assembly layout (10th package), Apache-2.0 like the other providers.
- INTEGRATION.md §2.2 extended with a "database document loaders" section; README Roadmap Phase 5 → In Progress.

### Slice 3 — collection seam + foreign-provider friction (2–3 sessions, engine)

- Additive public `EvaluationContext` collection hook (e.g. `CollectionLoader`/`CollectionUriMapper`) so `fn:collection("basex://db/coll")` resolves server-side.
- Fix the `XDocumentNode` special-cases from §2.3 so foreign providers are first-class (also benefits future XmlDocument adapter).
- Gates: unit + QT3 + both sweeps bit-identical; SemVer minor.

### Deferred (recorded, not scheduled)

- Native protocol clients (Slice B) — until a customer needs lower overhead than REST.
- DB-native node providers (Slice C) — until a committed design partner.
- XQuery push-down — separate REQ family if ever.

## 5. Open owner decisions

1. **First database:** BaseX recommended (OSS, REST, Docker-testable). eXist equally viable. MarkLogic needs a commercial license — defer.
2. **Package ownership:** providers package in the Apache-2.0 core repo (consistent with streaming provider), vs. a commercial Bosak.Schema tier — REST adapters fit the open core story; keep them Apache-2.0.
3. **CI strategy:** `HttpListener` stubs in CI vs. Dockerized DB containers (heavier; only if stub fidelity becomes an issue).
4. **Credential surface:** URI-embedded vs. options delegate vs. both — decide at Slice 2.

## 6. Risks

- **Sync-over-async** is the established idiom but can deadlock in UI/ASP.NET sync contexts with certain DB clients; document `ConfigureAwait(false)` in the adapter.
- **REST fidelity:** DB REST endpoints differ in pagination/serialization details; keep per-DB quirks behind the scheme registry, not in shared code.
- **Ordering guarantees:** documents fetched per-URI are fine; collection member ordering across requests needs the Slice 3 seam to be explicit about document order (creation-sequence model).

## 7. References

- `src/Bosak.XPath.Core/Xdm/IXdmNode.cs` (frozen contract) · `docs/API_FREEZE.md`
- `src/Bosak.XPath.Runtime/Vm/EvaluationContext.cs` (`DocumentLoader` :353, `StreamingDocumentLoader` :362)
- `src/Bosak.XPath.Providers/` (`XDocumentNode`, `XmlStreamingProvider`)
- `src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs` (fn:doc/collection entries)
- `docs/ARCHITECTURE.md` §Extensibility · `docs/INTEGRATION.md` §2.2

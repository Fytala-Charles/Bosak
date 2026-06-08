# Bosak XPath / XSLT — VS Code Extension

Language support for **XPath 3.1** and **XSLT 3.0** powered by the Bosak engine.

## Features

- **Syntax highlighting** for XPath (`.xpath`) and XSLT (`.xsl`, `.xslt`) files
- **Realtime diagnostics** — compile errors for XPath expressions and XSLT stylesheets
- **Auto-completion** — XPath functions, axes, keywords and XSLT instructions
- **Context-menu commands** — Evaluate XPath, Run XSLT Transformation *(WIP)*

---

## Prerequisites

| Tool | Version | Notes |
|------|---------|-------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0+ | Required to build and run the language server |
| [Node.js](https://nodejs.org/) | 18.x+ | Required to build the extension |
| [VS Code](https://code.visualstudio.com/) | 1.80+ | |

---

## Installation

### Option 1 — Development / Sideload (recommended for contributors)

1. **Build the language server**
   ```bash
   cd /path/to/Bosak
   dotnet build src/Bosak.LanguageServer/Bosak.LanguageServer.csproj
   ```

2. **Install extension dependencies**
   ```bash
   cd vscode-bosak
   npm install
   ```

3. **Compile the extension**
   ```bash
   npm run compile
   ```

4. **Open in VS Code** and press `F5`  
   This opens a new **Extension Development Host** window with Bosak loaded.

### Option 2 — Install from VSIX

1. **Build the extension package**
   ```bash
   cd vscode-bosak
   npm install
   npm run compile
   npx vsce package
   ```

2. **Install the VSIX in VS Code**
   - Open VS Code → Extensions view (`Ctrl+Shift+X`)
   - Click **⋯** (More Actions) → **Install from VSIX…**
   - Select `vscode-bosak-0.1.0.vsix`

### Option 3 — Custom server path

If the extension cannot find the language server automatically, set the path manually:

```json
// .vscode/settings.json
{
  "bosak.server.path": "D:/Development/Bosak/src/Bosak.LanguageServer/bin/Debug/net10.0/Bosak.LanguageServer.exe"
}
```

---

## Usage

### Opening files

Create or open files with the following extensions:

| Language | Extensions |
|----------|------------|
| XPath | `.xpath` |
| XSLT | `.xsl`, `.xslt` |

The extension activates automatically when an XPath or XSLT file is opened.

### Diagnostics

Errors appear in the **Problems** panel (`Ctrl+Shift+M`) as you type:

- **XPath files** — parser errors when the expression is invalid
- **XSLT files** — XML well-formedness errors, missing root element (`xsl:stylesheet` / `xsl:transform`), and invalid XPath in `select`, `test`, `match`, and `use-when` attributes

### Completions

Press `Ctrl+Space` to trigger suggestions:

- In **XPath** files: standard `fn:*` functions, axes (`child::`, `descendant::`, …), and keywords (`for`, `let`, `if`, …)
- In **XSLT** files: all of the above plus XSLT instructions (`xsl:template`, `xsl:apply-templates`, …)

### Commands

| Command | Context | Shortcut |
|---------|---------|----------|
| **Bosak: Evaluate XPath Expression** | Editor context menu (XPath files) | — |
| **Bosak: Run XSLT Transformation** | Editor context menu (XSLT files) | — |

> These commands are placeholders and will be wired to the evaluation engine in a future release.

---

## Configuration

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `bosak.server.path` | `string \| null` | `null` | Absolute path to the `Bosak.LanguageServer` executable or DLL. When `null`, the extension searches the workspace for a built server. |
| `bosak.trace.server` | `string` | `"off"` | Traces LSP communication between VS Code and the server. Values: `"off"`, `"messages"`, `"verbose"`. |

### Enable verbose logging

```json
{
  "bosak.trace.server": "verbose"
}
```

Open **Output** panel (`Ctrl+Shift+U`) → select **Bosak XPath / XSLT** to view the LSP traffic.

---

## Troubleshooting

### "Bosak language server not found"

The extension could not locate `Bosak.LanguageServer.exe` (or `.dll`). Make sure:

1. You have built the .NET solution:
   ```bash
   dotnet build Bosak.sln
   ```
2. The server binary exists at:
   ```
   src/Bosak.LanguageServer/bin/Debug/net10.0/Bosak.LanguageServer.exe
   ```
3. Or set `bosak.server.path` explicitly in VS Code settings.

### No diagnostics showing

1. Check that the file has the correct extension (`.xpath`, `.xsl`, `.xslt`).
2. Open **Output** → **Bosak XPath / XSLT** and look for connection errors.
3. Enable `"bosak.trace.server": "verbose"` to inspect LSP messages.

### Server crashes on startup

Run the server standalone to see the error:

```bash
dotnet src/Bosak.LanguageServer/bin/Debug/net10.0/Bosak.LanguageServer.dll
```

If it exits immediately, check for missing dependencies (`dotnet --info`).

---

## Development

### Project layout

```
vscode-bosak/
├── package.json              # Extension manifest
├── tsconfig.json             # TypeScript config
├── src/
│   └── extension.ts          # Client entry point
├── syntaxes/
│   ├── xpath.tmLanguage.json # XPath TextMate grammar
│   └── xslt.tmLanguage.json  # XSLT TextMate grammar
└── language-configuration.json # Bracket/symbol config
```

The language server itself lives in `../src/Bosak.LanguageServer/` (C# / .NET 10).

### Rebuild after server changes

```bash
dotnet build src/Bosak.LanguageServer/Bosak.LanguageServer.csproj
```

Then reload the VS Code Extension Development Host (`Ctrl+R` or **Developer: Reload Window**).

---

## License

Dual Usage License — See the root `LICENSE.md` for details.

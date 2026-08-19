import * as path from 'path';
import * as vscode from 'vscode';
import {
    LanguageClient,
    LanguageClientOptions,
    ServerOptions,
    TransportKind,
} from 'vscode-languageclient/node';

let client: LanguageClient | undefined;

export function activate(context: vscode.ExtensionContext): void {
    const serverModule = getServerPath(context);
    if (!serverModule) {
        vscode.window.showErrorMessage(
            'Bosak language server not found. Please build the solution first (`dotnet build`).'
        );
        return;
    }

    const serverOptions: ServerOptions = {
        run: {
            module: serverModule,
            transport: TransportKind.stdio,
        },
        debug: {
            module: serverModule,
            transport: TransportKind.stdio,
            options: { env: { ...process.env, BOSAK_LSP_DEBUG: '1' } },
        },
    };

    const clientOptions: LanguageClientOptions = {
        documentSelector: [
            { scheme: 'file', language: 'xpath' },
            { scheme: 'file', language: 'xslt' },
            { scheme: 'file', language: 'xquery' },
            { scheme: 'file', pattern: '**/*.xsl' },
            { scheme: 'file', pattern: '**/*.xslt' },
            { scheme: 'file', pattern: '**/*.xq' },
            { scheme: 'file', pattern: '**/*.xqy' },
            { scheme: 'file', pattern: '**/*.xquery' },
        ],
        synchronize: {
            fileEvents: [
                vscode.workspace.createFileSystemWatcher('**/*.xsl'),
                vscode.workspace.createFileSystemWatcher('**/*.xslt'),
                vscode.workspace.createFileSystemWatcher('**/*.xpath'),
                vscode.workspace.createFileSystemWatcher('**/*.xq'),
                vscode.workspace.createFileSystemWatcher('**/*.xqy'),
                vscode.workspace.createFileSystemWatcher('**/*.xquery'),
            ],
        },
    };

    client = new LanguageClient(
        'bosak',
        'Bosak XPath / XSLT',
        serverOptions,
        clientOptions
    );

    client.start();

    // Register commands
    context.subscriptions.push(
        vscode.commands.registerCommand('bosak.evaluateXPath', evaluateXPath),
        vscode.commands.registerCommand('bosak.transformXslt', transformXslt),
        vscode.commands.registerCommand('bosak.runXQuery', runXQuery)
    );
}

export function deactivate(): Thenable<void> | undefined {
    return client?.stop();
}

function getServerPath(context: vscode.ExtensionContext): string | undefined {
    const config = vscode.workspace.getConfiguration('bosak');
    const customPath = config.get<string | null>('server.path');
    if (customPath) {
        return customPath;
    }

    // 1. Try bundled server inside the extension
    const bundledCandidates = [
        path.join(context.extensionPath, 'server', 'Bosak.LanguageServer.exe'),
        path.join(context.extensionPath, 'server', 'Bosak.LanguageServer.dll'),
    ];

    for (const candidate of bundledCandidates) {
        try {
            const fs = require('fs');
            if (fs.existsSync(candidate)) {
                return candidate;
            }
        } catch {
            // ignore
        }
    }

    // 2. Try to find the built server relative to the workspace
    const workspaceRoot = vscode.workspace.workspaceFolders?.[0]?.uri.fsPath;
    if (!workspaceRoot) {
        return undefined;
    }

    const candidates = [
        path.join(workspaceRoot, 'src', 'Bosak.LanguageServer', 'bin', 'Debug', 'net10.0', 'Bosak.LanguageServer.exe'),
        path.join(workspaceRoot, 'src', 'Bosak.LanguageServer', 'bin', 'Release', 'net10.0', 'Bosak.LanguageServer.exe'),
        path.join(workspaceRoot, 'src', 'Bosak.LanguageServer', 'bin', 'Debug', 'net10.0', 'Bosak.LanguageServer.dll'),
        path.join(workspaceRoot, 'src', 'Bosak.LanguageServer', 'bin', 'Release', 'net10.0', 'Bosak.LanguageServer.dll'),
    ];

    for (const candidate of candidates) {
        try {
            const fs = require('fs');
            if (fs.existsSync(candidate)) {
                return candidate;
            }
        } catch {
            // ignore
        }
    }

    return undefined;
}

async function evaluateXPath(): Promise<void> {
    const editor = vscode.window.activeTextEditor;
    if (!editor || editor.document.languageId !== 'xpath') {
        vscode.window.showInformationMessage('Open an XPath file to evaluate.');
        return;
    }
    if (!client) {
        vscode.window.showErrorMessage('Bosak language server is not running.');
        return;
    }

    try {
        const result = await client.sendRequest<{ result?: string; error?: string }>(
            'bosak/evaluateXPath',
            { textDocument: { uri: editor.document.uri.toString() } }
        );
        if (result.error) {
            vscode.window.showErrorMessage(`XPath evaluation failed: ${result.error}`);
        } else {
            const doc = await vscode.workspace.openTextDocument({
                content: result.result ?? '',
                language: 'text'
            });
            await vscode.window.showTextDocument(doc, { preview: true });
        }
    } catch (err) {
        vscode.window.showErrorMessage(`XPath evaluation failed: ${err}`);
    }
}

async function runXQuery(): Promise<void> {
    const editor = vscode.window.activeTextEditor;
    if (!editor || editor.document.languageId !== 'xquery') {
        vscode.window.showInformationMessage('Open an XQuery file to run.');
        return;
    }
    if (!client) {
        vscode.window.showErrorMessage('Bosak language server is not running.');
        return;
    }

    try {
        const result = await client.sendRequest<{ result?: string; error?: string }>(
            'bosak/evaluateXQuery',
            { textDocument: { uri: editor.document.uri.toString() } }
        );
        if (result.error) {
            vscode.window.showErrorMessage(`XQuery evaluation failed: ${result.error}`);
        } else {
            const doc = await vscode.workspace.openTextDocument({
                content: result.result ?? '',
                language: 'text'
            });
            await vscode.window.showTextDocument(doc, { preview: true });
        }
    } catch (err) {
        vscode.window.showErrorMessage(`XQuery evaluation failed: ${err}`);
    }
}

async function transformXslt(): Promise<void> {
    const editor = vscode.window.activeTextEditor;
    if (!editor || editor.document.languageId !== 'xslt') {
        vscode.window.showInformationMessage('Open an XSLT file to transform.');
        return;
    }
    if (!client) {
        vscode.window.showErrorMessage('Bosak language server is not running.');
        return;
    }

    const picked = await vscode.window.showOpenDialog({
        canSelectFiles: true,
        canSelectFolders: false,
        canSelectMany: false,
        filters: { 'XML files': ['xml'], 'All files': ['*'] },
        openLabel: 'Select source XML document'
    });
    if (!picked || picked.length === 0) {
        return;
    }

    try {
        const result = await client.sendRequest<{ result?: string; error?: string }>(
            'bosak/transformXslt',
            {
                textDocument: { uri: editor.document.uri.toString() },
                sourcePath: picked[0].fsPath
            }
        );
        if (result.error) {
            vscode.window.showErrorMessage(`XSLT transformation failed: ${result.error}`);
        } else {
            const doc = await vscode.workspace.openTextDocument({
                content: result.result ?? '',
                language: 'xml'
            });
            await vscode.window.showTextDocument(doc, { preview: true });
        }
    } catch (err) {
        vscode.window.showErrorMessage(`XSLT transformation failed: ${err}`);
    }
}

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
            { scheme: 'file', pattern: '**/*.xsl' },
            { scheme: 'file', pattern: '**/*.xslt' },
        ],
        synchronize: {
            fileEvents: [
                vscode.workspace.createFileSystemWatcher('**/*.xsl'),
                vscode.workspace.createFileSystemWatcher('**/*.xslt'),
                vscode.workspace.createFileSystemWatcher('**/*.xpath'),
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
        vscode.commands.registerCommand('bosak.transformXslt', transformXslt)
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

    const expression = editor.document.getText();
    // TODO: Invoke Bosak evaluation via LSP custom message or CLI
    vscode.window.showInformationMessage(`Evaluate: ${expression.substring(0, 50)}...`);
}

async function transformXslt(): Promise<void> {
    const editor = vscode.window.activeTextEditor;
    if (!editor || editor.document.languageId !== 'xslt') {
        vscode.window.showInformationMessage('Open an XSLT file to transform.');
        return;
    }

    const xsltPath = editor.document.uri.fsPath;
    // TODO: Prompt for source XML and invoke transformation
    vscode.window.showInformationMessage(`Transform: ${xsltPath}`);
}

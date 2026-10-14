const assert = require('node:assert/strict');
const Module = require('node:module');
const path = require('node:path');
const originalLoad = Module._load;
const extension = path.resolve(__dirname, '../out/extension.js');
(async () => {
for (const customPath of ['C:/bosak/Bosak.LanguageServer.dll', 'C:/bosak/Bosak.LanguageServer.exe']) {
    let options;
    let clientOptions;
    let stateChanged;
    let instance;
    const vscode = {
        workspace: {
            getConfiguration: () => ({ get: () => customPath }),
            createFileSystemWatcher: () => ({})
        },
        commands: { registerCommand: () => ({}) },
        CancellationTokenSource: class { token = {}; dispose() {} }
    };
    class Client {
        constructor(_id, _name, server, configuration) { options = server; clientOptions = configuration; instance = this; }
        fillInitializeParams() {}
        start() {}
        onNotification() {}
        isRunning() { return false; }
        onDidChangeState(callback) { stateChanged = callback; return { dispose() {} }; }
    }
    Module._load = function (request, ...args) {
        if (request === 'vscode') return vscode;
        if (request === 'vscode-languageclient/node') return { LanguageClient: Client, TransportKind: { stdio: 0 }, State: { Running: 2, Stopped: 1 } };
        return originalLoad.call(this, request, ...args);
    };
    try {
        delete require.cache[extension];
        require(extension).activate({ extensionPath: '/candidate', subscriptions: [] });
        const dll = customPath.endsWith('.dll');
        assert.equal(options.run.command, dll ? 'dotnet' : customPath);
        assert.deepEqual(options.run.args, dll ? [customPath] : []);
        assert.equal(options.run.transport, 0);
        assert.equal(options.debug.command, options.run.command);
        assert.equal(options.debug.options.env.BOSAK_LSP_DEBUG, '1');
        assert.equal(options.run.module, undefined);
        const initialize = { capabilities: { textDocument: { diagnostic: { dynamicRegistration: true }, synchronization: { dynamicRegistration: true } } } };
        instance.fillInitializeParams(initialize);
        assert.equal(initialize.capabilities.textDocument.diagnostic.dynamicRegistration, false);
        assert.equal(initialize.capabilities.textDocument.synchronization.dynamicRegistration, false);
        let registered = false;
        const registration = clientOptions.middleware.handleRegisterCapability({ registrations: [] }, async () => { registered = true; });
        assert.equal(registered, false);
        stateChanged({ newState: 2 });
        await registration;
        assert.equal(registered, true);
    } finally {
        Module._load = originalLoad;
    }
}
console.log('Language-server launch checks passed: dotnet DLL, executable apphost and startup registration.');
})().catch(error => { console.error(error); process.exitCode = 1; });

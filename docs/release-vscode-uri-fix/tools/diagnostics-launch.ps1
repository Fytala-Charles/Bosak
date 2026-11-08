$savedElectronMode = $env:ELECTRON_RUN_AS_NODE
$savedIpcHook = $env:VSCODE_IPC_HOOK_CLI
Remove-Item Env:ELECTRON_RUN_AS_NODE -ErrorAction SilentlyContinue
Remove-Item Env:VSCODE_IPC_HOOK_CLI -ErrorAction SilentlyContinue
try {
$editorArgs = @('--user-data-dir','D:\Development\Bosak\.guard-tmp\core-vscode-uri-fix-20261108\final-fresh\profile','--extensions-dir','D:\Development\Bosak\.guard-tmp\core-vscode-uri-fix-20261108\final-fresh\extensions','--extensionDevelopmentPath=D:\Development\Bosak\.guard-tmp\core-vscode-uri-fix-20261108\diagnostic-harness','--extensionTestsPath=D:\Development\Bosak\.guard-tmp\core-vscode-uri-fix-20261108\diagnostic-harness\test.cjs','--skip-welcome','--skip-release-notes','--disable-updates','--new-window')
$editorProbe = Start-Process -FilePath 'C:\Program Files\Microsoft VS Code\Code.exe' -ArgumentList $editorArgs -WindowStyle Hidden -PassThru -RedirectStandardOutput 'D:\Development\Bosak\docs\release-vscode-uri-fix\diagnostic-stdout.txt' -RedirectStandardError 'D:\Development\Bosak\docs\release-vscode-uri-fix\diagnostic-stderr.txt'
$editorProbe.Id | Out-File -FilePath 'D:\Development\Bosak\docs\release-vscode-uri-fix\diagnostic-pid.txt' -Encoding ascii
$editorProbe.Id
} finally { $env:ELECTRON_RUN_AS_NODE = $savedElectronMode; $env:VSCODE_IPC_HOOK_CLI = $savedIpcHook }

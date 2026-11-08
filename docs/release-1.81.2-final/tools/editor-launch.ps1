$savedElectronMode = $env:ELECTRON_RUN_AS_NODE
$savedIpcHook = $env:VSCODE_IPC_HOOK_CLI
Remove-Item Env:ELECTRON_RUN_AS_NODE -ErrorAction SilentlyContinue
Remove-Item Env:VSCODE_IPC_HOOK_CLI -ErrorAction SilentlyContinue
try {
$editorArgs = @('--user-data-dir','D:\Development\Bosak\.guard-tmp\release-1812-final-1d69d059\final-fresh\profile','--extensions-dir','D:\Development\Bosak\.guard-tmp\release-1812-final-1d69d059\final-fresh\extensions','--extensionDevelopmentPath=D:\Development\Bosak\.guard-tmp\release-1812-final-1d69d059\editor-harness','--extensionTestsPath=D:\Development\Bosak\.guard-tmp\release-1812-final-1d69d059\editor-harness\test.cjs','--skip-welcome','--skip-release-notes','--disable-updates','--new-window')
$editorProbe = Start-Process -FilePath 'C:\Program Files\Microsoft VS Code\Code.exe' -ArgumentList $editorArgs -WindowStyle Hidden -PassThru -RedirectStandardOutput 'D:\Development\Bosak\docs\release-1.81.2-final\evidence\editor-stdout.txt' -RedirectStandardError 'D:\Development\Bosak\docs\release-1.81.2-final\evidence\editor-stderr.txt'
$editorProbe.Id | Out-File -FilePath 'D:\Development\Bosak\docs\release-1.81.2-final\evidence\editor-pid.txt' -Encoding ascii
$editorProbe.Id
} finally { $env:ELECTRON_RUN_AS_NODE = $savedElectronMode; $env:VSCODE_IPC_HOOK_CLI = $savedIpcHook }

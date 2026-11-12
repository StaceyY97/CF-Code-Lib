# 启动 rave-cf-writer agent 的 PowerShell 脚本
Write-Host "启动 rave-cf-writer agent..." -ForegroundColor Green
Write-Host ""
Write-Host "Agent 配置文件: .kiro/agents/rave-cf-writer.json"
Write-Host "技能文件: rave-cf-generator/SKILL.md"
Write-Host ""
Write-Host "要使用此 agent，请:"
Write-Host "1. 在 Kiro 聊天中说: '#启动 CF writer'"
Write-Host "2. 或者直接描述您的 CF 需求，hook 会自动触发"
Write-Host "3. 或者在命令行运行: kiro run agent://rave-cf-writer"
Write-Host ""

# 显示 agent 的简要说明
Get-Content .kiro/agents/rave-cf-writer.json | ConvertFrom-Json | Select-Object name, description | Format-List

Write-Host ""
Write-Host "Agent 已准备就绪，随时可以开始编写 CF 代码！" -ForegroundColor Cyan
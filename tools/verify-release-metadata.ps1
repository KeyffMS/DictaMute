$ErrorActionPreference = 'Stop'

$requestPath = Join-Path $PSScriptRoot '..\release\release-request.json'
$projectPath = Join-Path $PSScriptRoot '..\src\DictaMute\DictaMute.csproj'

if (-not (Test-Path $requestPath)) { throw "Missing release/release-request.json" }
if (-not (Test-Path $projectPath)) { throw "Missing DictaMute.csproj" }

$request = Get-Content $requestPath -Raw | ConvertFrom-Json
[xml]$project = Get-Content $projectPath -Raw
$projectVersion = [string]$project.Project.PropertyGroup.Version

if ([string]::IsNullOrWhiteSpace($request.version)) { throw "Release version is empty." }
if ([string]::IsNullOrWhiteSpace($request.tag)) { throw "Release tag is empty." }
if ($request.channel -notin @('alpha', 'beta', 'rc', 'stable')) { throw "Unsupported release channel: $($request.channel)" }
if ($projectVersion -ne $request.version) {
    throw "release-request version $($request.version) does not match project version $projectVersion."
}

$expectedTag = if ($request.channel -eq 'stable') {
    "v$($request.version)"
} else {
    "v$($request.version)-$($request.channel)"
}
if ($request.tag -ne $expectedTag) {
    throw "Release tag $($request.tag) does not match expected $expectedTag."
}

$notes = Join-Path (Join-Path $PSScriptRoot '..') $request.notesFile
if (-not (Test-Path $notes)) { throw "Missing release notes file: $($request.notesFile)" }

$requiredRepoFiles = @('LICENSE', 'PRIVACY.md', 'SECURITY.md', 'THIRD-PARTY-NOTICES.md', 'docs/BRAND.md', 'docs/RELEASING.md')
foreach ($file in $requiredRepoFiles) {
    $path = Join-Path (Join-Path $PSScriptRoot '..') $file
    if (-not (Test-Path $path)) { throw "Missing release authority file: $file" }
}

if ($request.publish -and -not $request.manualWindowsSmokeAccepted) {
    throw "publish=true requires manualWindowsSmokeAccepted=true."
}
if ($request.publish -and -not $request.releaseImmutabilityConfirmed) {
    throw "publish=true requires releaseImmutabilityConfirmed=true."
}

Write-Host "Release metadata OK: $($request.tag), publish=$($request.publish)"

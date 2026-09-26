$ErrorActionPreference = 'Stop'

$root = Join-Path $PSScriptRoot '..'
$brandPath = Join-Path $root 'docs\BRAND.md'
$websitePath = Join-Path $root 'docs\WEBSITE-CONTENT.md'

foreach ($path in @($brandPath, $websitePath)) {
    if (-not (Test-Path $path)) { throw "Missing public content authority: $path" }
}

$brand = Get-Content $brandPath -Raw
$website = Get-Content $websitePath -Raw

$canonical = 'https://aiteracja.pl/dictamute/'
if (-not $brand.Contains($canonical)) { throw 'Brand authority is missing canonical URL.' }
if (-not $website.Contains($canonical)) { throw 'Website contract is missing canonical URL.' }

$requiredRoutes = @(
    '/dictamute/',
    '/dictamute/features/',
    '/dictamute/download/',
    '/dictamute/docs/',
    '/dictamute/how-it-works/',
    '/dictamute/faq/',
    '/dictamute/releases/',
    '/dictamute/privacy/',
    '/dictamute/legal/',
    '/dictamute/security/',
    '/dictamute/support/'
)
foreach ($route in $requiredRoutes) {
    if (-not $website.Contains($route)) { throw "Website contract is missing route: $route" }
}

$requiredAuthorities = @('docs/BRAND.md', 'PRIVACY.md', 'SECURITY.md', 'docs/RELEASING.md')
foreach ($authority in $requiredAuthorities) {
    if (-not $website.Contains($authority)) { throw "Website contract does not reference authority: $authority" }
}

if (-not $website.Contains('It does not perform speech recognition')) {
    throw 'Website contract is missing the speech-recognition boundary.'
}
if (-not $website.Contains('GitHub Releases remain authoritative')) {
    throw 'Website contract is missing the release-authority rule.'
}

Write-Host 'Public content contract OK.'

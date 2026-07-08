[CmdletBinding()]
param(
	[string] $Filter = "*",
	[string] $Job = "Dry",
	[string] $Artifacts = "",
	[Parameter(ValueFromRemainingArguments = $true)]
	[string[]] $BenchmarkArgs = @()
)

$ErrorActionPreference = "Stop"

$projectDir = Split-Path -Parent $PSCommandPath
if ([string]::IsNullOrWhiteSpace($Artifacts))
{
	$Artifacts = Join-Path $projectDir "..\..\_bin\BenchmarkDotNet.Artifacts"
}

Push-Location $projectDir
try
{
	dotnet build .\Bench.KSoft.BCL.csproj --configuration Release
	if ($LASTEXITCODE -ne 0)
	{
		exit $LASTEXITCODE
	}

	$runArguments = @(
		"run",
		"--project", ".\Bench.KSoft.BCL.csproj",
		"--configuration", "Release",
		"--no-build",
		"--",
		"--filter", $Filter,
		"--job", $Job,
		"--noOverwrite",
		"--artifacts", $Artifacts
	) + $BenchmarkArgs

	dotnet @runArguments
	if ($LASTEXITCODE -ne 0)
	{
		exit $LASTEXITCODE
	}
}
finally
{
	Pop-Location
}

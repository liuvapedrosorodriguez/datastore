Param(
    [Parameter(Mandatory=$true)]
    [Alias('u')]
	[string] $vstsUser,
    [Parameter(Mandatory=$true)]
    [Alias('p')]
    [string] $vstsPassword
)

$vstsCredentials = @($vstsUser,$vstsPassword)
$vstsRootUri = "https://anavarro9731.visualstudio.com/defaultcollection/powershell/_apis/git/repositories/powershell/items?api-version=1.0&scopepath="
$modules = @(
    "build",
    "prepare-new-version",
    "build-and-test",
    "pack-and-publish"
)

# Set Project Root Folder
$global:projectRoot = $PSScriptRoot
Push-Location $PSScriptRoot

Write-Host "Importing Custom Modules"

# Base64-encodes the Personal Access Token (PAT) appropriately
$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $vstsCredentials[0],$vstsCredentials[1])))

# Set Modules Path for Session
$env:PSModulePath = $env:PSModulePath + ";$PSScriptRoot\.psModules\"

# Download and Import all Modules
foreach ($module in $modules) {
    New-Item ".\.psModules\$module\" -ItemType Directory -Verbose
    Invoke-RestMethod -Uri "$vstsRootUri$module.psm1" -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} -ContentType "text/plain; charset=UTF-8" -OutFile ".\.psModules\$module\$module.psm1"
    Import-Module $module -Verbose -Global -Force
}

#Remove folder once modules are loaded
Remove-Item ".\.psModules\" -Verbose -Recurse


#expose this function like a CmdLet
function global:Run {


	Param(
		[switch]$PrepareNewVersion,
		[switch]$BuildAndTest,
		[switch]$PackAndPublish,
        [Alias('o')]
        [string] $originUrl,
        [Alias('k')]
        [string] $nugetApiKey
	)
	
	if ($PrepareNewVersion) {
        Prepare-NewVersion -projects @(
            "DataStore",		      
            "DataStore.Providers.CosmosDb",
            "DataStore.Interfaces",
            "DataStore.Interfaces.LowLevel",
            "DataStore.Models"
        )
	}

	if ($BuildAndTest) {
		Build-And-Test -testPackages @(
            "DataStore.Tests" 
		)
	}

    if ($PackAndPublish) {
        Pack-And-Publish -projectsToPublish @(
            "DataStore",		      
            "DataStore.Providers.CosmosDb",
            "DataStore.Interfaces",
            "DataStore.Interfaces.LowLevel",
            "DataStore.Models"       	         	    			
        ) -unlistedProjects @(
            "DataStore.Interfaces.LowLevel",
            "DataStore.Models" 
        ) `
        -nugetFeedUri "https://www.nuget.org/api/v2/package" `
        -nugetSymbolFeedUri "https://www.nuget.org/api/v2/package" `
        -nugetApiKey $nugetApiKey `
        -originUrl $originUrl
    }
}
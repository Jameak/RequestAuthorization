[CmdletBinding()]
param (
	[ValidateSet('local', 'ci')]
	[string]$Env = 'local'
)
Set-StrictMode -Version 1

function Exec([scriptblock] $cmd) {
    $expandedCmdForLog = $ExecutionContext.InvokeCommand.ExpandString($cmd)
    Write-Host "Executing: $expandedCmdForLog"
    & $cmd
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

function RunLinterAndStandardTests{
    Param
    (
         [Parameter(Mandatory=$true, Position=0)]
         [string] $Configuration,
         [Parameter(Mandatory=$true, Position=1)]
         [AllowEmptyString()]
         [string] $AdditionalConfiguration
    )
    
    if ($Env -eq 'ci') {
        Exec { & dotnet format style --verify-no-changes }
        Exec { & dotnet format analyzers --verify-no-changes }
    } else {
        Exec { & dotnet format --verify-no-changes }
    }

    Exec { & dotnet build -c $Configuration $AdditionalConfiguration  }
    Exec { & dotnet test -c $Configuration $AdditionalConfiguration --logger trx --no-build --no-restore }
}

$configValue = 'Debug'
$msbuildConstant = ''

if ($Env -eq 'ci') {
    $configValue = 'Release'
    $msbuildConstant = '/p:ExtraDefineConstants="IS_CI_TEST_BUILD"'
}

RunLinterAndStandardTests -Configuration $configValue -AdditionalConfiguration $msbuildConstant
PrepareAndRunNugetTests -Configuration $configValue -AdditionalConfiguration $msbuildConstant

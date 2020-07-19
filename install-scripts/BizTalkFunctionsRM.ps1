$BizTalkPath = (Get-ItemProperty  "hklm:SOFTWARE\Wow6432Node\Microsoft\Biztalk Server\3.0\" -ErrorAction SilentlyContinue).InstallPath
$FrameworkPath = (Get-ItemProperty  "hklm:SOFTWARE\Wow6432Node\Microsoft\.NETFramework\" -ErrorAction SilentlyContinue).InstallRoot
#$SDKPath=(Get-ItemProperty  "hklm:SOFTWARE\Wow6432Node\Microsoft\.NETFramework\" -ErrorAction SilentlyContinue).$SdkKeyName
	
#if ($SDKPath -eq $null)
#{
#	$SDKPath=(Get-ItemProperty  "hklm:SOFTWARE\Wow6432Node\Microsoft\Microsoft SDKs\Windows\v7.1").InstallationFolder
#}
#if ($SDKPath -eq $null)
#{
#	$SDKPath=(Get-ItemProperty  "hklm:SOFTWARE\Wow6432Node\Microsoft\Microsoft SDKs\Windows\v7.0A").InstallationFolder
#}

function ImportBTSResource
{
	param([string]$appName, [string]$resType, [string]$srcPath)
	$gacOptions="/Options:GacOnAdd,GacOnInstall"
	BTSTask.exe AddResource /Source:$srcPath /ApplicationName:$appName /Type:$resType /Overwrite $gacOptions
}

function GetMSBuildPath
{
    $MSBuildPath = Join-Path $FrameworkPath "v4.0.30319\MSBuild.exe"
    return $MSBuildPath
}

#function GacAssembly
#{
#    param([string]$assembly)
#    
#    if($SDKPath -eq $null)
#    {
#        $gacutil = ".\gacutil.exe"
#    }
#    else
#    {
#        $gacutil = join-path $SDKPath "bin\NETFX 4.0 Tools\gacutil.exe"
#    }
#    
#    Write-Host $gacutil
#    $command = "-if"
#    &$gacutil $command $assembly

#}
#Function to install MSI
function InstallMSI{
    param([string]$msi,[string]$installPath)

    $log = $msi + ".log"

    $Arguments += "/i `"$msi`""
    $Arguments += " /log `"$log`""
    $Arguments += " /passive"
    $Arguments += " INSTALLDIR=`"$installPath`""

    #Write-Host "Installing Msi with $Arguments." -ForegroundColor DarkGray;
    #Write-Host ""

    Start-Process "msiexec.exe" $Arguments -Wait -Passthru
    #Write "Exitcode: $exitCode" -ForegroundColor DarkGray;
    #Write ""

    #return $exitCode

}

#Function to uninstall MSI
function UninstallMSI{
    param([string]$msi)

    $app = Get-WmiObject -Class Win32_Product -Filter "PackageName = '$msi'"
    $app.Uninstall()
    
    #Write-Host "Msi uninstalled." -ForegroundColor Green;

}

#Function to select specific environment settings file, there is no need to export it as BTDF exports it anyway
function ExportEnvironmentSettings
{
    param([string]$installPath, [string]$environment)
    
    $args = "`"" + (Join-Path $installPath "Deployment\EnvironmentSettings\SettingsFileGenerator.xml") + "`"" + " Deployment\EnvironmentSettings"
    $SettingsExporter = ("`"" + (Join-Path $installPath "\Deployment\Framework\DeployTools\EnvironmentSettingsExporter.exe") + "`"")
    
    #Write-Host "SettingsExporter: $SettingsExporter" -ForegroundColor DarkGray;
    #Write-Host "Arguments: $args" -ForegroundColor DarkGray;
    #Write-Host ""
    
    #Write-Host "Exporting Environment Settings file..." -ForegroundColor DarkGray;
    
    #$exitCode = (Start-Process -FilePath $SettingsExporter -ArgumentList $args -Wait -Passthru).ExitCode
    #if($exitCode -eq 0)
    #{
        $settingsFile = "Deployment\EnvironmentSettings\{0}_Settings.xml" -f $environment
        $envSettingsFile = join-path $installPath $settingsFile
        
    #}
    
    #Write-Host ""
    #Write-Host "Env File: " $envSettingsFile -ForegroundColor DarkGray;
    
    return $envSettingsFile
    
    

}


#Function to deploy BTS application form BTDF deployment proj
function DeployApplication
{
    param([string]$installPath, [bool]$deployToMgmtDB,[string]$environment, [bool]$skipUnDeploy, [string]$deploymentProjFileName)

    if($environment -ne "NA")
    {
        $envSettingsFile = ExportEnvironmentSettings $installPath $environment
        
        #Write-Host "ENV_SETTINGS " $envSettingsFile -ForegroundColor DarkGray;
        #Set-Item Env:ENV_SETTINGS -Value $envSettingsFile
        #Set-Item Env:BT_DEPLOY_MGMT_DB -Value $deployToMgmtDB
    }


	if($deploymentProjFileName -eq "")
	{
		$projFile = $installPath + "\Deployment\Deployment.btdfproj"
	}
	else
	{
		$projFile = $installPath + "\Deployment\" + $deploymentProjFileName
    }

	#Write-Host "projFile: " $projFile -ForegroundColor DarkGray;

	$logfile = $installPath + "\DeployResults\DeployResults.txt"

    $Arguments += "`"$projFile`""
    $Arguments += " /p:DeployBizTalkMgmtDB=$deployToMgmtDB;Configuration=Server;SkipUndeploy=$skipUnDeploy;InstallDir=`"$installPath`""
    $Arguments += " /target:Deploy"
    $Arguments += " /l:FileLogger,Microsoft.Build.Engine;logfile=`"$logfile`""
    
    if($envSettingsFile -ne $null)
    {
        $Arguments += " /p:ENV_SETTINGS=`"$envSettingsFile`""
    }
    
    #Write ""
    #Write-Host $Arguments -ForegroundColor DarkGray;

    $MSBuildPath = GetMSBuildPath

    #Write-Host "MSBuildPath: " $MSBuildPath -ForegroundColor DarkGray;
    #Write-Host ""

    #Start-Process -FilePath $MSBuildPath -ArgumentList $Arguments -Wait -Passthru
    &cmd /c "$MSBuildPath $Arguments"

}

#function to undeploy the biztalk project
function UndeployApplication
{
    param(
      [Parameter(Position=0,HelpMessage="Path wherein the app is installed")]
      [Alias("path")]
      [string]$ApplicationInstallPath,
      
      [Parameter(Position=1,HelpMessage="DeployBizTalkMgmtDB e.g. true if its first server to undeploy or false otherwise, optional by default true")]
      [Alias('firstserver')]
      [bool]$deployToMgmtDb = $true,
	  
	  [Parameter(Position=2,HelpMessage="Optionally, Proivde name of btdf deployment.proj file")]
	  [string]$deploymentProjFileName


      )
    $ErrorActionPreference = "Stop"
    
	if($deploymentProjFileName -eq "")
	{
		$projFile = $ApplicationInstallPath + "\Deployment\Deployment.btdfproj"
	}
	else
	{
		$projFile = $ApplicationInstallPath + "\Deployment\" + $deploymentProjFileName
		
    }
    $logfile = $ApplicationInstallPath + "\DeployResults\DeployResults.txt"

    $Arguments += "`"$projFile`""    
    $Arguments += " /p:DeployBizTalkMgmtDB=$deployToMgmtDB;Configuration=Server;InstallDir=`"$ApplicationInstallPath`""
    $Arguments += " /target:Undeploy"
    $Arguments += " /l:FileLogger,Microsoft.Build.Engine;logfile=`"$logfile`""
    
    #Write ""
    #Write-Host $Arguments -ForegroundColor DarkGray;

    $MSBuildPath = GetMSBuildPath

    #Write-Host "MSBuildPath: " $MSBuildPath -ForegroundColor DarkGray;
    #Write-Host ""    
    
    #Start-Process -FilePath $MSBuildPath -ArgumentList $Arguments -Wait -Passthru
    &cmd /c "$MSBuildPath $Arguments"
    
    #Write-Host "Undeployed finished" -ForegroundColor Green;
    
}

#function to do both install and deploy
function InstallAndDeployBizTalkApp
{
    param(
      [Parameter(Position=0,Mandatory=$true,HelpMessage="Msi file should be existing")]
      [ValidateScript({Test-Path $_})]
      [Alias("msi")]
      [string]$MsiFile,
       
      [Parameter(Position=1,Mandatory=$true,HelpMessage="Path wherein the resource file will be installed")]
      [Alias("path")]
      [string]$ApplicationInstallPath,
       
      [Parameter(Position=2,Mandatory=$true,HelpMessage="Only valid parameters are Local,Dev,Test and Prod")]
      [Alias("env")]
      #[ValidateSet("DEV","QA","PROD","NA","PRODG1","PRODG2","PRODG3","PRODG4","QOL")]
      [string]$Environment,
      
      [Parameter(Position=3, HelpMessage="DeployBizTalkMgmtDB e.g. true if its last server to deploy or false otherwise, optional by default true")]
      [Alias("lastserver")]
      [bool]$deployToMgmtDb=$true,
      
      [Parameter(Position=4, HelpMessage="Skip Undeploy e.g. false if undeploy is required before deploy otherwise true, optional by default true")]
      [bool]$skipUndeploy=$true,

	   [Parameter(Position=5,HelpMessage="Optionally, Proivde name of btdf deployment.proj file")]
	   [string]$deploymentProjFileName
      )
      
    $ErrorActionPreference = "Stop"
    
    #Write-Host "Installing msi..." -ForegroundColor DarkGray;
     
    InstallMSI $MsiFile $ApplicationInstallPath
 
    #Write-Host "Waiting 30 seconds before deploying the app" -ForegroundColor DarkGray;
    start-sleep -seconds 30
    
    #Write-Host "Deploying assemblies..." -ForegroundColor DarkGray;
    DeployApplication $ApplicationInstallPath $deployToMgmtDb $Environment $skipUndeploy $deploymentProjFileName
        

    #Write-Host "Finished installing and deploying" -ForegroundColor Green;   

}

function UndeployAndUnInstallBizTalkApp
{
       param(
      [Parameter(Position=0,Mandatory=$true,HelpMessage="Msi file should be existing")]
      [Alias("msi")]
      [string]$MsiFile,
             
      [Parameter(Position=1,HelpMessage="Path wherein the app is installed")]
      [ValidateScript({Test-Path $_})]
      [Alias("path")]
      [string]$ApplicationInstallPath,
      
      [Parameter(Position=2,HelpMessage="DeployBizTalkMgmtDB e.g. true if its first server to undeploy or false otherwise, optional by default true")]
      [Alias('firstserver')]
      [bool]$deployToMgmtDb = $true,
	  
	  [Parameter(Position=3,HelpMessage="Optionally, Proivde name of btdf deployment.proj file")]
	  [string]$deploymentProjFileName
      ) 
      
      $ErrorActionPreference = "Stop"
    
      #Write-Host "undeploy assemblies..." -ForegroundColor DarkGray;  
      
      UndeployApplication $ApplicationInstallPath $deployToMgmtDb $deploymentProjFileName
      
      #Write-Host "uninstalling msi..." -ForegroundColor DarkGray;  
      
      UninstallMSI $MsiFile
      
      #Write-Host "Finished uninstalling and undeploying" -ForegroundColor Green;   
      
}

function InstallPipelineComponent
{
    param
    (
        [Parameter(Position=0, Mandatory=$true, HelpMessage="Pipeline component not valid")]
        [Alias('componentname')]
        [string]$PipelineComponent,
        
        [Parameter(Position=1, Mandatory=$true, HelpMessage="Souce directory of pipeline component not valid")]
        [ValidateScript({Test-Path $_})]
        [Alias('source')]
        [string]$SourcePath
    )

    $destination = join-path $BizTalkPath "Pipeline Components\$PipelineComponent"
    $source = join-path $SourcePath $PipelineComponent
    
    Copy -path $source -destination $destination
    GacAssembly $source

}
# SIG # Begin signature block
# MIIFxQYJKoZIhvcNAQcCoIIFtjCCBbICAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUyHWe6s3upJyz22wCywHDxep3
# ZLugggNOMIIDSjCCAjagAwIBAgIQYK2JbMozEaVNxUi+zt1t5jAJBgUrDgMCHQUA
# MCwxKjAoBgNVBAMTIVBvd2VyU2hlbGwgTG9jYWwgQ2VydGlmaWNhdGUgUm9vdDAe
# Fw0xNjA2MjQxNDA2MDlaFw0zOTEyMzEyMzU5NTlaMCYxJDAiBgNVBAMTG1Bvd2Vy
# U2hlbGwgRUlTIFNjcmlwdHMgQ2VydDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCC
# AQoCggEBAOc7tI/jlRnjfOrj4kiqWTluAIDXY+xVjd3rg0wgGVD2gzsJ7a1MTU4D
# SZAenSY/pAAp6CTSsYucswXZnmESwENzUFarD/+EWummhD9aWKp/LuGB7TJ8L+Wb
# GXP/36jgpLKpvDC2McdbfXmv95IbMERc8kCF6R2WTrAw4GGq+Hyy6XHRuN7hOa72
# 1rtF4f5YVkV5fH3+7mqoZ0ZWcz7Iq2F5NRkN5M2henJxnGUqhSne3/6UzxEQyiQk
# QzOaaRUH825A1S92vw21qWxvU61nU+UWxJAwo5JxbncKogPj2H6lCPuT/WTsjvey
# CjQedoUqK96Asjbm5lb7sioXd7sEujsCAwEAAaN2MHQwEwYDVR0lBAwwCgYIKwYB
# BQUHAwMwXQYDVR0BBFYwVIAQjaDar8Xa3+DbbTfpVMoIjKEuMCwxKjAoBgNVBAMT
# IVBvd2VyU2hlbGwgTG9jYWwgQ2VydGlmaWNhdGUgUm9vdIIQ4ydvyFP0y6lEKtKW
# f/akajAJBgUrDgMCHQUAA4IBAQBfu6pSOb8+Y2zpKshGzqoWLgqFbMjqlTVr6ZwB
# K1NaAwdwIPhZV2CMq9BFoky3QxuRldWLbPjEpLnMoX9XoZ0VQUu3DZ9Go+UOlwzE
# oEB0i8fnPV2ipVkhXqQ862DSnuMXIGaSd9gWlGsjXxBp94ctvEijMy+SSzttmi1C
# hoGBO8UafWDnzLEwILQe5uvVbtpZC19lYQ9vYcnBRWa40tmbM5+PeYlvNNy6N9/c
# mUpuEAASeR7URAKv075V3IPnz9FdNS6XLrUwUM6zf6muB465ArjtnMA49CW/KOlj
# EMnmB6eHBHaMPUfEC4rRspMI/RBU1P7etBbM03ekhUgbNkHqMYIB4TCCAd0CAQEw
# QDAsMSowKAYDVQQDEyFQb3dlclNoZWxsIExvY2FsIENlcnRpZmljYXRlIFJvb3QC
# EGCtiWzKMxGlTcVIvs7dbeYwCQYFKw4DAhoFAKB4MBgGCisGAQQBgjcCAQwxCjAI
# oAKAAKECgAAwGQYJKoZIhvcNAQkDMQwGCisGAQQBgjcCAQQwHAYKKwYBBAGCNwIB
# CzEOMAwGCisGAQQBgjcCARUwIwYJKoZIhvcNAQkEMRYEFNZ5Idz8FvZSilVbrIj1
# uNNvtKAXMA0GCSqGSIb3DQEBAQUABIIBALxyORutA0GY0jYEqQvQWyIJl7FnwUFu
# W+0oZmDZ1QEQ+NleSd6qChb2S8/3fTNH0Kq9YQnDKpQdtqnOLajgxnmIY5k5HuAr
# n7SQq56ulYONBQI/b9CdzcDJ0xz27QhwM1ZFiBG8DkJ4mCbTLMPeFJZQZLWgnYF1
# /MTPqLAWhTBFYH9A81fTtZ3fylSzVJFpbmwvPeVdqNAGRvCE1Awz5GjgR/ZaDNGY
# oLsc0oGRmICAnaZhci2+qwBCFPxMKi9aj3CWxRw/m/20Lzl+bC2qsv+Q8gdEAGNM
# k98dfJsB1041KBOn8IefDqq4ln9WyT8d3lWaTALQ7I1r676k/b75xm8=
# SIG # End signature block

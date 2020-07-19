param
(
   [Parameter(Position=0,Mandatory=$true, HelpMessage="MSI name to be installed")]
   [string]$msiName,
   [Parameter(Position=1,Mandatory=$true, HelpMessage="location of msi")]
   [ValidateScript({Test-Path $_})]
   [string]$msiLocation,
   [Parameter(Position=2,Mandatory=$true, HelpMessage="environment you are installing to, e.g. DEV, QA, or NA if no env specific binding are included in deployment.")]
   [string]$environment,
   [Parameter(Position=3,HelpMessage="Server location where MSI to be installed. Default is 'E:\Program Files (x86)\BizTalkApps\<MsiName>\1.0")]
   [string]$targetLocation='',
   [Parameter(Position=4,HelpMessage="if its is last server for deployment then set to true for biztalk mgmt deployment")]
   [bool]$lastserver=$false,
   [Parameter(Position=5,HelpMessage="if you want to redeploy, then set this to false")]
   [bool]$skipUndeploy=$true,
   [Parameter(Position=6,HelpMessage="if all server does not have biztalk functions rm locally then")]
   [string]$serverRunningScript=''

)


if($PSScriptRoot -eq '')
{
    $bizTalkFunctionsPath = "\\$serverRunningScript\BuildDrop\BizTalkFunctions\BizTalkFunctionsRM.ps1"
}
else
{
    $bizTalkFunctionsPath = $PSScriptRoot + "\BizTalkFunctionsRM.ps1"
}

. $bizTalkFunctionsPath


if($targetLocation -eq '')
{
    $targetLocation = "E:\Program Files (x86)\BizTalkApps\" + $msiName + "\1.0"
}

$msiNameWithExt = $msiName + '.msi'

$msiNameWithPath = join-path $msiLocation $msiNameWithExt

InstallAndDeployBizTalkApp -msi $msiNameWithPath -path $targetLocation -env $environment -lastserver $lastserver -skipUndeploy $skipUndeploy
# SIG # Begin signature block
# MIIFxQYJKoZIhvcNAQcCoIIFtjCCBbICAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQU0yJ49GRh4WNUuy16xwrVETfY
# xzSgggNOMIIDSjCCAjagAwIBAgIQYK2JbMozEaVNxUi+zt1t5jAJBgUrDgMCHQUA
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
# CzEOMAwGCisGAQQBgjcCARUwIwYJKoZIhvcNAQkEMRYEFKwjzDsskZwyLnoRGyQE
# 4oQFCXrqMA0GCSqGSIb3DQEBAQUABIIBAHuY5x3vCDWCmceU+FkcU7w/uaKtiR3w
# sYOgccm1XDRK1EjlonSMmE0JDhbCFCcJNkjCLPqDfHcaZaX4HS0RLv45U4ocgVhg
# LMYACQnOEP7iNCEsYWDe4+95OZW3PnLcoE6vyqzjVxE139G8TGKMllH3KNUb+fot
# z+JVVXqoAhkXdEytvbsUJHPKF3vJNcthEu7MZXER6k0qqVCsdzgZublF31DB+YL6
# z48ZuC+WOoYMDNNCw9B4dipl+BqnroFHgXWkDvkcrRUvN0aBmPpjK0/ci8m2riQF
# Zu0S3+pno+/kK3+/DuwQjGYxk6bPPj5wMPBQQ0oVJKA6+/m2ICi81E8=
# SIG # End signature block

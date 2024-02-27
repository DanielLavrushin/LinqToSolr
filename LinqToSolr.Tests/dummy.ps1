$solrContainer = "linqtosolr"
$solrCoreName = "dummy"
$solrCofigs = Join-Path -Path $PSScriptRoot -ChildPath "solr"

$securityJsonPath = "$PSScriptRoot\solr\security.json"
$solrAdminPassword = "12345" 
$solrAdminUsername = "admin"
# password: 12345
$hashedPassword = "vUklmD45nYAxD3RGyQuOKOnO5gShG4EuRKXhmUuPCF8= aDh2MmIzdWhtM2ZieHh3OQ=="

$securityJsonContent = @"
{
  "authentication":{
    "blockUnknown": true,
    "class":"solr.BasicAuthPlugin",
    "credentials":{"admin":"$hashedPassword"}
  },
  "authorization":{
    "class":"solr.RuleBasedAuthorizationPlugin",
    "permissions":[{"name":"security-edit","role":"admin"}],
    "user-role":{"solr":"admin"}
  }
}
"@

# Save the content to the security.json file

# Encode credentials in Base64 for Basic Authentication
$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $solrAdminUsername, $solrAdminPassword)))

# Now, when making Invoke-RestMethod calls, include the Authorization header
$headers = @{
    Authorization = "Basic $base64AuthInfo"
}


docker rm $solrContainer --force
Remove-Item -Recurse -Force "$PSScriptRoot\solr\" -ErrorAction SilentlyContinue
New-Item -Path "$PSScriptRoot\" -Name "solr" -ItemType "directory"
$securityJsonContent | Out-File -FilePath $securityJsonPath -Encoding UTF8 -ErrorAction SilentlyContinue
Write-Host "security.json has been created at $securityJsonPath" -ForegroundColor Green 

$cmd = "docker run -d --name $solrContainer -p 8983:8983 -v $($solrCofigs -replace '\\', '/'):/var/solr/data solr solr-precreate $solrCoreName"
Write-Host $cmd -ForegroundColor Green
Invoke-Expression $cmd

Start-Sleep 2

$jsonBody = Get-Content -Path "$PSScriptRoot\dummy-schema.json" -Raw
Invoke-RestMethod -Uri "http://localhost:8983/solr/dummy/schema" -Method Post -ContentType "application/json" -Body $jsonBody  -Headers $headers

$jsonFilePath = "$PSScriptRoot\dummy-data.json"
$uri = "http://localhost:8983/solr/$solrCoreName/update/json/docs?commit=true"
Invoke-RestMethod -Uri $uri -Method Post -ContentType "application/json" -InFile $jsonFilePath -Headers $headers

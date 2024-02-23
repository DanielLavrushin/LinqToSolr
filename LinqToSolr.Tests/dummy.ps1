$solrContainer = "linqtosolr"
$solrCoreName = "dummy"
$solrCofigs = Join-Path -Path $PSScriptRoot -ChildPath "solr"

docker rm $solrContainer --force
Remove-Item -Recurse -Force "$PSScriptRoot\solr\$solrCoreName"
$cmd = "docker run -d --name $solrContainer -p 8983:8983 -v $($solrCofigs -replace '\\', '/'):/var/solr/data solr"
Write-Host $cmd -ForegroundColor Green
Invoke-Expression $cmd

Start-Sleep 2
docker exec $solrContainer solr create_core -c dummy


$jsonBody = Get-Content -Path "$PSScriptRoot\dummy-schema.json" -Raw
Invoke-RestMethod -Uri "http://localhost:8983/solr/dummy/schema" -Method Post -ContentType "application/json" -Body $jsonBody

$jsonFilePath = "$PSScriptRoot\dummy-data.json"
$uri = "http://localhost:8983/solr/$solrCoreName/update/json/docs?commit=true"
Invoke-RestMethod -Uri $uri -Method Post -ContentType "application/json" -InFile $jsonFilePath

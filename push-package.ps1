[string]$version = $env:VERSION
[string]$nugetApiKey = $env:NUGET_API_KEY
[string]$notes = "Automated build for version $version"

if ([string]::IsNullOrWhiteSpace($version) -or [string]::IsNullOrWhiteSpace($nugetApiKey))
{
    write-error "missing required environment variables"
    exit -1
}

set-location src
dotnet restore
dotnet pack -c Release -p:PackageVersion=$version --output nupkg

if ($version.Split('.').Length -lt 4)
{
    # Release
    gh release create $version nupkg/*.nupkg --notes $notes
    dotnet nuget push nupkg/*.nupkg --api-key $nugetApiKey --source https://api.nuget.org/v3/index.json
}
else
{
    # Pre-Release
    gh release create $version nupkg/*.nupkg --prerelease --notes $notes
}

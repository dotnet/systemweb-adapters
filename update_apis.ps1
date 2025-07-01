## Running this script will update the type forwards and .NET Standard APIs

param($config = "release")

$project = "src\Microsoft.AspNetCore.SystemWebAdapters\Microsoft.AspNetCore.SystemWebAdapters.csproj"

Write-Host "Generating reference source"
dotnet build --no-incremental $project -c $config -f net8.0 /p:GenerateStandard=true

Write-Host "Generating type forwards"
dotnet build --no-incremental $project -c $config -f net8.0 /p:GenerateTypeForwards=true

Write-Host "Verifying all APIs are up to date"

# Script will have an error if there are git changes
$output = git status src --porcelain

if($output){
  Write-Host "There are changes in the git repository. Please run locally and then commit the changes."
  Write-Host "Changes detected:"
  Write-Host $output
  exit 1
}

Write-Host "No changes detected for APIs"
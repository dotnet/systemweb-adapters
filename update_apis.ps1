## Running this script will update the type forwards and .NET Standard APIs

param($config = "release")

$project = "src\Microsoft.AspNetCore.SystemWebAdapters\Microsoft.AspNetCore.SystemWebAdapters.csproj"

dotnet build --no-incremental $project -c $config -f net8.0 /p:GenerateStandard=true
dotnet build --no-incremental $project -c $config -f net8.0 /p:GenerateTypeForwards=true

# Script will have an error if there are git changes
if(git status --porcelain){
  exit 1
}

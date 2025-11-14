param(
    [string]$RootPath = $PSScriptRoot
)

Write-Host "Searching for web.config files in: $RootPath" -ForegroundColor Cyan

# Find all web.config files
$webConfigFiles = Get-ChildItem -Path $RootPath -Filter "web.config" -Recurse -File

if ($webConfigFiles.Count -eq 0) {
    Write-Host "No web.config files found." -ForegroundColor Yellow
    exit
}

Write-Host "Found $($webConfigFiles.Count) web.config file(s)" -ForegroundColor Green

foreach ($file in $webConfigFiles) {
    Write-Host "`nProcessing: $($file.FullName)" -ForegroundColor Cyan
    
    try {
        # Load the XML content
        [xml]$xml = Get-Content -Path $file.FullName -Raw
        
        # Find the runtime node
        $runtimeNode = $xml.SelectSingleNode("//configuration/runtime")
        
        if ($null -eq $runtimeNode) {
            Write-Host "  No <runtime> element found - skipping" -ForegroundColor Gray
            continue
        }
        
        # Check if runtime has child nodes
        if ($runtimeNode.HasChildNodes) {
            Write-Host "  Removing $($runtimeNode.ChildNodes.Count) child element(s) from <runtime>" -ForegroundColor Yellow
            
            # Remove all child nodes
            $runtimeNode.RemoveAll()
            
            # Save the modified XML
            $xml.Save($file.FullName)
            Write-Host "  Successfully cleaned <runtime> element" -ForegroundColor Green
        }
        else {
            Write-Host "  <runtime> element is already empty - skipping" -ForegroundColor Gray
        }
    }
    catch {
        Write-Host "  ERROR: Failed to process file - $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`nCompleted processing all web.config files." -ForegroundColor Cyan

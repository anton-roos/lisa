# CodeCounter.ps1
# PowerShell script to count lines of code in C# and Razor files

param (
    [string]$rootPath = (Split-Path -Parent (Get-Location).Path),
    [string[]]$excludeFolders = @("bin", "obj", "node_modules", "wwwroot\lib", ".git", "Migrations"),
    [switch]$includeEmptyLines = $false,
    [switch]$excludeComments = $false,
    [switch]$detailed = $false
)

# Helper function to check if path should be excluded
function ShouldExclude {
    param ([string]$path)
    
    foreach ($folder in $excludeFolders) {
        if ($path -like "*\$folder\*") {
            return $true
        }
    }
    return $false
}

# Function to count lines in a file
function CountLinesInFile {
    param (
        [string]$filePath,
        [bool]$countEmptyLines = $true,
        [bool]$skipComments = $false
    )
    
    $content = Get-Content -Path $filePath
    $lineCount = 0
    $commentCount = 0
    $emptyLineCount = 0
    
    foreach ($line in $content) {
        $trimmedLine = $line.Trim()
        
        # Check if line is empty
        if ([string]::IsNullOrWhiteSpace($trimmedLine)) {
            $emptyLineCount++
            if ($countEmptyLines) {
                $lineCount++
            }
            continue
        }
        
        # Check if line is a comment
        if ($skipComments -and 
            ($trimmedLine.StartsWith("//") -or 
             $trimmedLine.StartsWith("/*") -or 
             $trimmedLine.StartsWith("*") -or 
             $trimmedLine.StartsWith("*/"))) {
            $commentCount++
            continue
        }
        
        $lineCount++
    }
    
    return @{
        TotalLines = $lineCount
        CommentCount = $commentCount
        EmptyLineCount = $emptyLineCount
    }
}

# Main execution
Write-Host "Counting lines of code in $rootPath..." -ForegroundColor Cyan

$csharpFiles = Get-ChildItem -Path $rootPath -Include "*.cs" -Recurse | 
    Where-Object { -not (ShouldExclude $_.FullName) }

$razorFiles = Get-ChildItem -Path $rootPath -Include "*.razor", "*.cshtml" -Recurse | 
    Where-Object { -not (ShouldExclude $_.FullName) }

$csharpStats = @{
    Count = $csharpFiles.Count
    TotalLines = 0
    CommentCount = 0
    EmptyLineCount = 0
    Files = @()
}

$razorStats = @{
    Count = $razorFiles.Count
    TotalLines = 0
    CommentCount = 0
    EmptyLineCount = 0
    Files = @()
}

# Process C# files
Write-Host "Processing $($csharpFiles.Count) C# files..." -ForegroundColor Yellow
foreach ($file in $csharpFiles) {
    $result = CountLinesInFile -filePath $file.FullName -countEmptyLines (-not $excludeEmptyLines) -skipComments $excludeComments
    
    $csharpStats.TotalLines += $result.TotalLines
    $csharpStats.CommentCount += $result.CommentCount
    $csharpStats.EmptyLineCount += $result.EmptyLineCount
    
    if ($detailed) {
        $csharpStats.Files += [PSCustomObject]@{
            Name = $file.Name
            Path = $file.FullName.Replace($rootPath, "")
            Lines = $result.TotalLines
        }
    }
}

# Process Razor files
Write-Host "Processing $($razorFiles.Count) Razor files..." -ForegroundColor Yellow
foreach ($file in $razorFiles) {
    $result = CountLinesInFile -filePath $file.FullName -countEmptyLines (-not $excludeEmptyLines) -skipComments $excludeComments
    
    $razorStats.TotalLines += $result.TotalLines
    $razorStats.CommentCount += $result.CommentCount
    $razorStats.EmptyLineCount += $result.EmptyLineCount
    
    if ($detailed) {
        $razorStats.Files += [PSCustomObject]@{
            Name = $file.Name
            Path = $file.FullName.Replace($rootPath, "")
            Lines = $result.TotalLines
        }
    }
}

# Calculate total
$totalFiles = $csharpFiles.Count + $razorFiles.Count
$totalLines = $csharpStats.TotalLines + $razorStats.TotalLines

# Output results
Write-Host "`n=== CODE STATISTICS ===`n" -ForegroundColor Green

Write-Host "C# Files:" -ForegroundColor Cyan
Write-Host "  Count: $($csharpStats.Count) files"
Write-Host "  Lines: $($csharpStats.TotalLines) lines"
if ($excludeEmptyLines) {
    Write-Host "  (Excluded $($csharpStats.EmptyLineCount) empty lines)"
}
if ($excludeComments) {
    Write-Host "  (Excluded $($csharpStats.CommentCount) comment lines)"
}

Write-Host "`nRazor/CSHTML Files:" -ForegroundColor Cyan
Write-Host "  Count: $($razorStats.Count) files"
Write-Host "  Lines: $($razorStats.TotalLines) lines"
if ($excludeEmptyLines) {
    Write-Host "  (Excluded $($razorStats.EmptyLineCount) empty lines)"
}
if ($excludeComments) {
    Write-Host "  (Excluded $($razorStats.CommentCount) comment lines)"
}

Write-Host "`nTotal Statistics:" -ForegroundColor Green
Write-Host "  Files: $totalFiles"
Write-Host "  Total Lines: $totalLines`n"

# Display detailed file statistics if requested
if ($detailed) {
    Write-Host "`nDetailed C# File Statistics:" -ForegroundColor Magenta
    $csharpStats.Files | Sort-Object -Property Lines -Descending | Format-Table -AutoSize

    Write-Host "`nDetailed Razor File Statistics:" -ForegroundColor Magenta
    $razorStats.Files | Sort-Object -Property Lines -Descending | Format-Table -AutoSize
}

# Export to CSV if requested
$exportCsv = Read-Host "Would you like to export results to CSV? (y/n)"
if ($exportCsv -eq "y") {
    $csvPath = Join-Path -Path $PWD.Path -ChildPath "CodeStats_$(Get-Date -Format 'yyyyMMdd_HHmmss').csv"
    
    if ($detailed) {
        $csvData = @()
        
        foreach ($file in $csharpStats.Files) {
            $csvData += [PSCustomObject]@{
                Type = "C#"
                FileName = $file.Name
                FilePath = $file.Path
                LineCount = $file.Lines
            }
        }
        
        foreach ($file in $razorStats.Files) {
            $csvData += [PSCustomObject]@{
                Type = "Razor"
                FileName = $file.Name
                FilePath = $file.Path
                LineCount = $file.Lines
            }
        }
        
        $csvData | Export-Csv -Path $csvPath -NoTypeInformation
    }
    else {
        # Simple summary
        $csvData = @(
            [PSCustomObject]@{
                Type = "C#"
                FileCount = $csharpStats.Count
                LineCount = $csharpStats.TotalLines
            },
            [PSCustomObject]@{
                Type = "Razor"
                FileCount = $razorStats.Count
                LineCount = $razorStats.TotalLines
            },
            [PSCustomObject]@{
                Type = "Total"
                FileCount = $totalFiles
                LineCount = $totalLines
            }
        )
        
        $csvData | Export-Csv -Path $csvPath -NoTypeInformation
    }
    
    Write-Host "Results exported to $csvPath" -ForegroundColor Green
}
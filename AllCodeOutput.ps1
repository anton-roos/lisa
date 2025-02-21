# Ask the user to select a folder
$folder = Read-Host "Enter the folder path to search for .cs and .razor files"

# Validate the folder path
if (-not (Test-Path $folder -PathType Container)) {
    Write-Host "Invalid folder path. Please enter a valid directory."
    exit
}

# Define the output file path
$outputFile = "AllCodeOutput.txt"

# Clear the output file if it already exists
if (Test-Path $outputFile) {
    Remove-Item $outputFile
}

# Retrieve all .cs and .razor files recursively from the chosen folder and process each file
Get-ChildItem -Path $folder -Recurse -Include *.cs,*.razor,*.csproj -File | ForEach-Object {
    # Write a header with the file name using the format operator
    "{0}" -f $_.Name | Out-File $outputFile -Append

    # Write the content of the file to the output file
    Get-Content $_.FullName | Out-File $outputFile -Append

    # Optionally add a blank line to separate entries
    "" | Out-File $outputFile -Append
}

# Optionally, display the total number of lines in the output file
$totalLines = (Get-Content $outputFile | Measure-Object -Line).Lines
Write-Host "Total lines in $outputFile : $totalLines"

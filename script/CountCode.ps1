(Get-ChildItem -Recurse -Include *.cs,*.razor -File | Get-Content | Measure-Object -Line).Lines

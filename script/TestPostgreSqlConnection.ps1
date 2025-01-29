$connectionString = "Host=41.76.111.105;Port=5432;Database=Lisa;Username=postgres;Password=@dm!n-DcegRem?!*1;;"
$conn = New-Object Npgsql.NpgsqlConnection($connectionString)
$conn.Open()
Write-Host "Connected Successfully"
$conn.Close()

param($Request)

[HttpResponse]@{
    StatusCode = 200
    Body       = "Hello, Functions Framework from PowerShell."
    Headers    = @{
        'Content-Type' = 'text/plain'
    }
}
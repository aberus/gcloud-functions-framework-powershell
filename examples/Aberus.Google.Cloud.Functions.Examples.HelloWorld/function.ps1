param($Request)

[HttpResponse]@{
    StatusCode = 200
    Body       = "Hello, world!"
    Headers    = @{'Content-Type' = 'text/plain' }
}
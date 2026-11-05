param(
    [ValidateSet('timeout', 'unauthorized', 'normal')][string]$Mode = 'timeout',
    [int]$Port = 17108,
    [string]$Upstream = 'https://localhost:17107/'
)

# Local QA harness: relay API calls and alter only a successful create-order response.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Net.Http
$listener = [System.Net.HttpListener]::new()
$listener.Prefixes.Add("http://localhost:$Port/")
$handler = [System.Net.Http.HttpClientHandler]::new()
$handler.AllowAutoRedirect = $false
$handler.UseCookies = $false
$client = [System.Net.Http.HttpClient]::new($handler)
$client.Timeout = [TimeSpan]::FromSeconds(60)
$base = [Uri]::new($Upstream)
$created = 0
try {
    $listener.Start()
    Write-Host "QA proxy ready on localhost:$Port ($Mode)"
    while ($listener.IsListening) {
        $context = $listener.GetContext()
        $incoming = $context.Request
        $target = [Uri]::new($base, $incoming.Url.PathAndQuery.TrimStart('/'))
        $request = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::new($incoming.HttpMethod), $target)
        try {
            if ($incoming.Headers['Authorization']) {
                $request.Headers.TryAddWithoutValidation('Authorization', $incoming.Headers['Authorization']) | Out-Null
            }
            if ($incoming.HasEntityBody) {
                $buffer = [System.IO.MemoryStream]::new()
                $incoming.InputStream.CopyTo($buffer)
                $request.Content = [System.Net.Http.ByteArrayContent]::new($buffer.ToArray())
                if ($incoming.ContentType) { $request.Content.Headers.TryAddWithoutValidation('Content-Type', $incoming.ContentType) | Out-Null }
                $buffer.Dispose()
            }
            $reply = $client.SendAsync($request).GetAwaiter().GetResult()
            try {
                $isCreate = $incoming.HttpMethod -eq 'POST' -and $incoming.Url.AbsolutePath -eq '/api/orders'
                if ($isCreate -and [int]$reply.StatusCode -eq 201) {
                    $created++
                    Write-Host "Upstream committed create POST #$created at $(Get-Date -Format o); fault=$Mode"
                    if ($Mode -eq 'timeout') { Start-Sleep -Seconds 30 }
                    if ($Mode -eq 'unauthorized') {
                        $context.Response.StatusCode = 401
                        $context.Response.Close()
                        continue
                    }
                }
                $body = $reply.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
                $context.Response.StatusCode = [int]$reply.StatusCode
                if ($reply.Content.Headers.ContentType) { $context.Response.ContentType = $reply.Content.Headers.ContentType.ToString() }
                $context.Response.ContentLength64 = $body.Length
                $context.Response.OutputStream.Write($body, 0, $body.Length)
                $context.Response.Close()
            }
            finally { $reply.Dispose() }
        }
        catch {
            Write-Warning "Proxy request failed: $($_.Exception.Message)"
            try { $context.Response.StatusCode = 502; $context.Response.Close() } catch {}
        }
        finally { $request.Dispose() }
    }
}
finally {
    $client.Dispose()
    $listener.Close()
}

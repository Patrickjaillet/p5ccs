using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace P5CCS.Engine.Http;

public sealed class LocalSketchServer : IDisposable
{
    private static readonly Dictionary<string, string> AssetContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"] = "image/gif",
        [".svg"] = "image/svg+xml",
        [".webp"] = "image/webp",
        [".bmp"] = "image/bmp",
        [".mp3"] = "audio/mpeg",
        [".wav"] = "audio/wav",
        [".ogg"] = "audio/ogg",
        [".m4a"] = "audio/mp4",
        [".mp4"] = "video/mp4",
        [".webm"] = "video/webm",
        [".json"] = "application/json",
        [".csv"] = "text/csv",
        [".txt"] = "text/plain",
        [".xml"] = "application/xml",
        [".ttf"] = "font/ttf",
        [".otf"] = "font/otf",
        [".woff"] = "font/woff",
        [".woff2"] = "font/woff2",
        [".glsl"] = "text/plain",
        [".vert"] = "text/plain",
        [".frag"] = "text/plain",
    };

    private readonly HttpListener _listener;
    private readonly Func<string> _sketchSourceProvider;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _listenTask;

    public LocalSketchServer(Func<string> sketchSourceProvider)
    {
        _sketchSourceProvider = sketchSourceProvider;
        Port = GetFreeLoopbackPort();
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://127.0.0.1:{Port}/");
    }

    public int Port { get; }

    public Uri BaseUri => new($"http://127.0.0.1:{Port}/");

    public string? AssetDirectory { get; set; }

    public void Start()
    {
        if (_listener.IsListening)
        {
            return;
        }

        _listener.Start();
        _cancellationTokenSource = new CancellationTokenSource();
        _listenTask = Task.Run(() => ListenLoopAsync(_cancellationTokenSource.Token));
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
        if (_listener.IsListening)
        {
            _listener.Stop();
        }
    }

    public void Dispose()
    {
        Stop();
        _listener.Close();
    }

    private async Task ListenLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            HttpListenerContext context;
            try
            {
                context = await _listener.GetContextAsync();
            }
            catch (Exception) when (cancellationToken.IsCancellationRequested || !_listener.IsListening)
            {
                break;
            }

            _ = HandleRequestAsync(context);
        }
    }

    private Task HandleRequestAsync(HttpListenerContext context)
    {
        var path = context.Request.Url?.AbsolutePath ?? "/";

        byte[] body;
        string contentType;
        var statusCode = 200;

        switch (path)
        {
            case "/":
                body = Encoding.UTF8.GetBytes(EmbeddedEngineResource.ReadText("P5CCS.Engine.Resources.index.html"));
                contentType = "text/html; charset=utf-8";
                break;
            case "/p5.min.js":
                body = EmbeddedEngineResource.ReadBytes("P5CCS.Engine.Resources.p5.min.js");
                contentType = "text/javascript; charset=utf-8";
                break;
            case "/p5.sound.min.js":
                body = EmbeddedEngineResource.ReadBytes("P5CCS.Engine.Resources.p5.sound.min.js");
                contentType = "text/javascript; charset=utf-8";
                break;
            case "/bridge.js":
                body = Encoding.UTF8.GetBytes(EmbeddedEngineResource.ReadText("P5CCS.Engine.Resources.bridge.js"));
                contentType = "text/javascript; charset=utf-8";
                break;
            case "/sketch.js":
                body = Encoding.UTF8.GetBytes(_sketchSourceProvider());
                contentType = "text/javascript; charset=utf-8";
                break;
            default:
                if (TryReadAssetFile(path, out var assetBody, out var assetContentType))
                {
                    body = assetBody;
                    contentType = assetContentType;
                }
                else
                {
                    body = Encoding.UTF8.GetBytes("Not Found");
                    contentType = "text/plain; charset=utf-8";
                    statusCode = 404;
                }

                break;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = contentType;
        context.Response.Headers.Add("Cache-Control", "no-store");

        try
        {
            context.Response.ContentLength64 = body.Length;
            context.Response.OutputStream.Write(body, 0, body.Length);
        }
        catch (HttpListenerException)
        {
        }
        finally
        {
            context.Response.OutputStream.Close();
        }

        return Task.CompletedTask;
    }

    internal static string? ResolveAssetPath(string? assetDirectory, string requestPath)
    {
        if (string.IsNullOrEmpty(assetDirectory))
        {
            return null;
        }

        var relativePath = Uri.UnescapeDataString(requestPath).TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        if (string.IsNullOrEmpty(relativePath))
        {
            return null;
        }

        string fullPath;
        string assetRootFullPath;
        try
        {
            assetRootFullPath = Path.GetFullPath(assetDirectory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            fullPath = Path.GetFullPath(Path.Combine(assetRootFullPath, relativePath));
        }
        catch (ArgumentException)
        {
            return null;
        }

        return fullPath.StartsWith(assetRootFullPath, StringComparison.OrdinalIgnoreCase) ? fullPath : null;
    }

    private bool TryReadAssetFile(string requestPath, out byte[] body, out string contentType)
    {
        body = Array.Empty<byte>();
        contentType = "application/octet-stream";

        var fullPath = ResolveAssetPath(AssetDirectory, requestPath);
        if (fullPath is null || !File.Exists(fullPath))
        {
            return false;
        }

        body = File.ReadAllBytes(fullPath);
        contentType = AssetContentTypes.TryGetValue(Path.GetExtension(fullPath), out var mapped)
            ? mapped
            : "application/octet-stream";
        return true;
    }

    private static int GetFreeLoopbackPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}

using System.Net;
using System.Net.Sockets;
using System.Text;

namespace P5CCS.Engine.Http;

public sealed class LocalSketchServer : IDisposable
{
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
            case "/bridge.js":
                body = Encoding.UTF8.GetBytes(EmbeddedEngineResource.ReadText("P5CCS.Engine.Resources.bridge.js"));
                contentType = "text/javascript; charset=utf-8";
                break;
            case "/sketch.js":
                body = Encoding.UTF8.GetBytes(_sketchSourceProvider());
                contentType = "text/javascript; charset=utf-8";
                break;
            default:
                body = Encoding.UTF8.GetBytes("Not Found");
                contentType = "text/plain; charset=utf-8";
                statusCode = 404;
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

    private static int GetFreeLoopbackPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}

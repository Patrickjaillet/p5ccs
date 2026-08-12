using P5CCS.Engine.Http;

namespace P5CCS.Engine.Tests.Http;

public class LocalSketchServerTests
{
    [Fact]
    public async Task IndexRoute_ReturnsHtmlReferencingBridgeAndP5js()
    {
        using var server = new LocalSketchServer(() => "console.log('test');");
        server.Start();
        using var client = new HttpClient();

        var html = await client.GetStringAsync(server.BaseUri);

        Assert.Contains("/p5.min.js", html);
        Assert.Contains("/bridge.js", html);
        Assert.Contains("/sketch.js", html);
    }

    [Fact]
    public async Task P5jsRoute_ReturnsEmbeddedRuntime()
    {
        using var server = new LocalSketchServer(() => string.Empty);
        server.Start();
        using var client = new HttpClient();

        var script = await client.GetStringAsync(new Uri(server.BaseUri, "p5.min.js"));

        Assert.Contains("p5.js", script[..200]);
    }

    [Fact]
    public async Task SketchRoute_ReturnsCurrentSourceFromProvider()
    {
        var source = "function setup() { createCanvas(800, 450); }";
        using var server = new LocalSketchServer(() => source);
        server.Start();
        using var client = new HttpClient();

        var script = await client.GetStringAsync(new Uri(server.BaseUri, "sketch.js"));

        Assert.Equal(source, script);
    }

    [Fact]
    public async Task UnknownRoute_Returns404()
    {
        using var server = new LocalSketchServer(() => string.Empty);
        server.Start();
        using var client = new HttpClient();

        var response = await client.GetAsync(new Uri(server.BaseUri, "does-not-exist"));

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public void BaseUri_IsBoundToLoopbackOnly()
    {
        using var server = new LocalSketchServer(() => string.Empty);

        Assert.Equal("127.0.0.1", server.BaseUri.Host);
    }
}

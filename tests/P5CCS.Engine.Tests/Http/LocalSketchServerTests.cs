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
    public async Task P5SoundRoute_ReturnsEmbeddedSoundAddon()
    {
        using var server = new LocalSketchServer(() => string.Empty);
        server.Start();
        using var client = new HttpClient();

        var script = await client.GetStringAsync(new Uri(server.BaseUri, "p5.sound.min.js"));

        Assert.Contains("p5.sound", script[..200]);
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

    [Fact]
    public async Task AssetRoute_WithConfiguredDirectory_ServesFileWithCorrectContentType()
    {
        var tempDir = Directory.CreateTempSubdirectory("p5ccs-assets-");
        try
        {
            await File.WriteAllTextAsync(Path.Combine(tempDir.FullName, "data.json"), "{\"ok\":true}");

            using var server = new LocalSketchServer(() => string.Empty) { AssetDirectory = tempDir.FullName };
            server.Start();
            using var client = new HttpClient();

            var response = await client.GetAsync(new Uri(server.BaseUri, "data.json"));
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
            Assert.Equal("{\"ok\":true}", body);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task AssetRoute_WithNestedSubdirectory_ServesFile()
    {
        var tempDir = Directory.CreateTempSubdirectory("p5ccs-assets-");
        try
        {
            var nested = Directory.CreateDirectory(Path.Combine(tempDir.FullName, "images"));
            await File.WriteAllBytesAsync(Path.Combine(nested.FullName, "sprite.png"), new byte[] { 1, 2, 3 });

            using var server = new LocalSketchServer(() => string.Empty) { AssetDirectory = tempDir.FullName };
            server.Start();
            using var client = new HttpClient();

            var response = await client.GetAsync(new Uri(server.BaseUri, "images/sprite.png"));

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    [Fact]
    public void ResolveAssetPath_TraversalSequenceEscapingAssetDirectory_ReturnsNull()
    {
        var tempDir = Directory.CreateTempSubdirectory("p5ccs-assets-");
        try
        {
            var resolved = LocalSketchServer.ResolveAssetPath(tempDir.FullName, "../secret.txt");

            Assert.Null(resolved);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    [Fact]
    public void ResolveAssetPath_EncodedTraversalSequence_ReturnsNull()
    {
        var tempDir = Directory.CreateTempSubdirectory("p5ccs-assets-");
        try
        {
            var resolved = LocalSketchServer.ResolveAssetPath(tempDir.FullName, "%2e%2e/secret.txt");

            Assert.Null(resolved);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    [Fact]
    public void ResolveAssetPath_ValidNestedRelativePath_ReturnsPathWithinAssetDirectory()
    {
        var tempDir = Directory.CreateTempSubdirectory("p5ccs-assets-");
        try
        {
            var resolved = LocalSketchServer.ResolveAssetPath(tempDir.FullName, "images/sprite.png");

            Assert.NotNull(resolved);
            Assert.StartsWith(Path.GetFullPath(tempDir.FullName), resolved, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task AssetRoute_WithoutConfiguredDirectory_Returns404()
    {
        using var server = new LocalSketchServer(() => string.Empty);
        server.Start();
        using var client = new HttpClient();

        var response = await client.GetAsync(new Uri(server.BaseUri, "anything.json"));

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
}

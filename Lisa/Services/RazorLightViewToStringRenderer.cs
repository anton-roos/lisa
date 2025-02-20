using RazorLight;

public class RazorLightViewToStringRenderer
{
    private readonly RazorLightEngine _engine;

    // Inject the IWebHostEnvironment to get the project root.
    public RazorLightViewToStringRenderer(IWebHostEnvironment env)
    {
        // Combine the ContentRootPath with the relative path to your views.
        // This should resolve to "C:\Users\farro\Source\Lisa\Lisa\Components\Pages"
        string viewsPath = Path.Combine(env.ContentRootPath, "Components", "Pages");

        _engine = new RazorLightEngineBuilder()
            .UseFileSystemProject(viewsPath)
            .UseMemoryCachingProvider()
            .Build();
    }

    public async Task<string> RenderViewToStringAsync<TModel>(string viewPath, TModel model)
    {
        // viewPath should be relative to the "Components/Pages" folder.
        // For your _ProgressFeedback view, pass "Shared/_ProgressFeedback.cshtml"
        return await _engine.CompileRenderAsync(viewPath, model);
    }
}

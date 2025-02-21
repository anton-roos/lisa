using RazorLight;

public class RazorLightViewToStringRenderer
{
    private readonly RazorLightEngine _engine;

    // Inject the IWebHostEnvironment to get the project root.
    public RazorLightViewToStringRenderer(IWebHostEnvironment env, IConfiguration configuration)
    {
        // Use a configuration setting or fallback to the default folder.
        string viewsFolder = configuration["RazorViewsFolder"] ?? Path.Combine("Components", "Pages");
        string viewsPath = Path.Combine(env.ContentRootPath, viewsFolder);

        if (!Directory.Exists(viewsPath))
        {
            throw new DirectoryNotFoundException($"Root directory '{viewsPath}' not found. Please ensure that your views are deployed correctly.");
        }

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

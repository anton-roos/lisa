using RazorLight;

namespace Lisa.Services;

public static class RazorLightViewToStringRenderer
{
    private static readonly RazorLightEngine _engine = new RazorLightEngineBuilder()
        .UseEmbeddedResourcesProject(typeof(RazorLightViewToStringRenderer))
        .UseMemoryCachingProvider()
        .Build();

    public static async Task<string> RenderViewToStringAsync<TModel>(string viewKey, TModel model)
    {
        return await _engine.CompileRenderAsync(viewKey, model);
    }
}
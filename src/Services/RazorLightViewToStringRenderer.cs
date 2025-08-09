using RazorLight;
using System.Reflection;

namespace Lisa.Services;

public static class RazorLightViewToStringRenderer
{
    private static readonly RazorLightEngine _engine = new RazorLightEngineBuilder()
        .UseEmbeddedResourcesProject(Assembly.GetExecutingAssembly(), "Lisa.Templates")
        .UseMemoryCachingProvider()
        .Build();

    public static async Task<string> RenderViewToStringAsync<TModel>(string viewKey, TModel model)
    {
        return await _engine.CompileRenderAsync(viewKey, model);
    }
}
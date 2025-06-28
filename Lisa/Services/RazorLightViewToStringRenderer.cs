using RazorLight;

public class RazorLightViewToStringRenderer
{
    private readonly RazorLightEngine _engine;

    public RazorLightViewToStringRenderer()
    {
        _engine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(typeof(RazorLightViewToStringRenderer))
            .UseMemoryCachingProvider()
            .Build();
    }

    public async Task<string> RenderViewToStringAsync<TModel>(string viewKey, TModel model)
    {
        return await _engine.CompileRenderAsync(viewKey, model);
    }
}
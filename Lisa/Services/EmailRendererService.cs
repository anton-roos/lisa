using RazorLight;

namespace Lisa.Services
{
    public class EmailRendererService
    {
        private readonly RazorLightEngine _engine;

        public EmailRendererService()
        {
            _engine = new RazorLightEngineBuilder()
                .UseMemoryCachingProvider()
                .Build();
        }

        /// <summary>
        /// Renders a Razor template stored as a string.
        /// </summary>
        /// <typeparam name="T">The type of the model used in the template.</typeparam>
        /// <param name="templateKey">A unique key for caching (for example, the EmailTemplate.Id).</param>
        /// <param name="templateContent">The template content (HTML with Razor syntax).</param>
        /// <param name="model">The model with properties that match your template placeholders.</param>
        /// <returns>Rendered HTML as a string.</returns>
        public async Task<string> RenderTemplateAsync<T>(string templateKey, string templateContent, T model)
        {
            return await _engine.CompileRenderStringAsync(templateKey, templateContent, model);
        }
    }
}

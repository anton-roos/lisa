using Lisa.Enums;

namespace Lisa.Services
{
    public class EmailRendererService
    {
        private readonly ILogger<EmailRendererService> _logger;

        public EmailRendererService(ILogger<EmailRendererService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Renders a Razor template stored as a string.
        /// </summary>
        /// <typeparam name="T">The type of the model used in the template.</typeparam>
        /// <param name="templateKey">A unique key for caching (for example, the EmailTemplate.Id).</param>
        /// <param name="templateContent">The template content (HTML with Razor syntax).</param>
        /// <param name="model">The model with properties that match your template placeholders.</param>
        /// <returns>Rendered HTML as a string.</returns>
        public async Task<string> RenderTemplateAsync(Template template, string templateContent)
        {
            try
            {
                Task.Delay(1000).Wait(); // Simulate a long-running operation
                return "Here we need to render template";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render template {template}.", template.ToString());
                return string.Empty;
            }
        }
    }
}

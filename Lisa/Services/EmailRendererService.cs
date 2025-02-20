using Lisa.Enums;
using Lisa.Models.ViewModels;

namespace Lisa.Services
{
    public class EmailRendererService
    (
        ILogger<EmailRendererService> logger,
        ProgressFeedbackService progressFeedbackService,
        UserService userService,
        RazorLightViewToStringRenderer razorViewToStringRenderer
    )
    {
        private readonly RazorLightViewToStringRenderer _razorViewToStringRenderer = razorViewToStringRenderer;
        private readonly ILogger<EmailRendererService> _logger = logger;
        private readonly ProgressFeedbackService _progressFeedbackService = progressFeedbackService;
        private readonly UserService _userService = userService;

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
                await Task.Delay(1000); // Simulate a long-running operation
                return "Here we need to render template";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render template {template}.", template.ToString());
                return string.Empty;
            }
        }

        public async Task<string> RenderProgressFeedbackAsync(Guid learnerId)
        {
            try
            {
                // Retrieve the necessary data using your services.
                var feedback = await _progressFeedbackService.GetProgressFeedbackAsync(learnerId);
                var principals = await _userService.GetLearnerPrincipal(learnerId);
                var model = new ProgressFeedbackViewModel
                {
                    Feedback = feedback,
                    Principals = principals
                };

                // Define the path to your shared partial view.
                string viewPath = "Shared/_ProgressFeedback.cshtml";

                // Render the view to a string using your Razor view renderer.
                string renderedHtml = await _razorViewToStringRenderer.RenderViewToStringAsync(viewPath, model);
                return renderedHtml;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render progress feedback for learner {learnerId}.", learnerId);
                return string.Empty;
            }
        }
    }
}

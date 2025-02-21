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

        public async Task<string> RenderProgressFeedbackAsync(Guid learnerId)
        {
            try
            {
                var feedback = await _progressFeedbackService.GetProgressFeedbackAsync(learnerId);
                var principals = await _userService.GetLearnerPrincipal(learnerId);
                var model = new ProgressFeedbackViewModel
                {
                    Feedback = feedback,
                    Principals = principals
                };

                string viewKey = "Lisa.Components.Pages.Shared._ProgressFeedback.cshtml";

                string renderedHtml = await _razorViewToStringRenderer.RenderViewToStringAsync(viewKey, model);
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

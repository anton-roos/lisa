using Lisa.Models.Entities;
using Lisa.Models.ViewModels;

namespace Lisa.Services;
public class EmailRendererService
(
    ILogger<EmailRendererService> logger,
    ProgressFeedbackService progressFeedbackService,
    UserService userService,
    LearnerService learnerService,
    SchoolService schoolService,
    RazorLightViewToStringRenderer razorViewToStringRenderer
)
{
    private readonly RazorLightViewToStringRenderer _razorViewToStringRenderer = razorViewToStringRenderer;
    private readonly ILogger<EmailRendererService> _logger = logger;
    private readonly ProgressFeedbackService _progressFeedbackService = progressFeedbackService;
    private readonly UserService _userService = userService;
    private readonly LearnerService _learnerService = learnerService;
    private readonly SchoolService _schoolService = schoolService;

    public async Task<string> RenderProgressFeedbackAsync(Guid learnerId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var feedback = fromDate.HasValue || toDate.HasValue
                ? await _progressFeedbackService.GetProgressFeedbackAsync(learnerId, fromDate, toDate)
                : await _progressFeedbackService.GetProgressFeedbackAsync(learnerId);

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

    public async Task<string> RenderTestAsync(Guid learnerId)
    {
        School? school;
        var principals = await _userService.GetLearnerPrincipal(learnerId);
        var learner = await _learnerService.GetByIdAsync(learnerId);
        if (learner is not null && learner.CareGroup is not null)
        {
            school = await _schoolService.GetSchoolAsync(learner.CareGroup.SchoolId);
        }
        else
        {
            school = null;
        }

        var model = new TestEmailViewModel
        {
            Learner = learner,
            Principals = principals,
            School = school
        };

        string viewKey = "Lisa.Components.Pages.Shared._TestEmail.cshtml";

        string renderedHtml = await _razorViewToStringRenderer.RenderViewToStringAsync(viewKey, model);
        return renderedHtml;
    }

    public async Task<string> RenderNewsletterAsync(Guid schoolId)
    {
        await Task.Delay(1000);
        return string.Empty;
    }

}
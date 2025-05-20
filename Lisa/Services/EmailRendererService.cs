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
    public async Task<string> RenderProgressFeedbackAsync(Guid learnerId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var feedback = fromDate.HasValue || toDate.HasValue
                ? await progressFeedbackService.GetProgressFeedbackAsync(learnerId, fromDate, toDate)
                : await progressFeedbackService.GetProgressFeedbackAsync(learnerId);

            var principals = await userService.GetLearnerPrincipal(learnerId);
            var model = new ProgressFeedbackViewModel
            {
                Feedback = feedback,
                Principals = principals
            };

            var viewKey = "Lisa.Components.Pages.Shared._ProgressFeedback.cshtml";

            var renderedHtml = await razorViewToStringRenderer.RenderViewToStringAsync(viewKey, model);
            return renderedHtml;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to render progress feedback for learner {learnerId}.", learnerId);
            return string.Empty;
        }
    }

    public async Task<string> RenderTestAsync(Guid learnerId)
    {
        School? school;
        var principals = await userService.GetLearnerPrincipal(learnerId);
        var learner = await learnerService.GetByIdAsync(learnerId);
        if (learner is not null && learner.CareGroup is not null)
        {
            school = await schoolService.GetSchoolAsync(learner.CareGroup.SchoolId);
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

        var viewKey = "Lisa.Components.Pages.Shared._TestEmail.cshtml";

        var renderedHtml = await razorViewToStringRenderer.RenderViewToStringAsync(viewKey, model);
        return renderedHtml;
    }

    public async Task<string> RenderNewsletterAsync(Guid schoolId)
    {
        await Task.Delay(1000);
        return string.Empty;
    }

}
using Lisa.Models.Entities;
using Lisa.Models.ViewModels;

namespace Lisa.Services;

public class TemplateRenderService
(
    ILogger<TemplateRenderService> logger,
    ProgressFeedbackService progressFeedbackService,
    UserService userService,
    LearnerService learnerService,
    SchoolService schoolService
)
{
    public async Task<string> RenderBulkProgressFeedbackAsync(List<Guid> learnerIds, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var feedback = fromDate.HasValue || toDate.HasValue
                ? await progressFeedbackService.GetProgressFeedbackForLearnersAsync(learnerIds, fromDate, toDate)
                : await progressFeedbackService.GetProgressFeedbackForLearnersAsync(learnerIds);

            var placeHolders = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("{{fromDate}}",
                    fromDate.HasValue ? fromDate.Value.ToString("dd MMM yyyy") : "All time"),

                new KeyValuePair<string, string>("{{toDate}}",
                    toDate.HasValue ? toDate.Value.ToString("dd MMM yyyy") : "All time"),
            };

            var viewKey = "_ProgressFeedbackTemplate.cshtml";

            var renderedHtml = await RazorLightViewToStringRenderer.RenderViewToStringAsync(viewKey, feedback);
            foreach (var placeholder in placeHolders)
            {
                if (renderedHtml.Contains(placeholder.Key))
                {
                    renderedHtml = renderedHtml.Replace(placeholder.Key, placeholder.Value);
                }
            }
            return renderedHtml;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to download progress feedback");
            return string.Empty;
        }
    }

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

            var viewKey = "_ProgressFeedback.cshtml";

            var renderedHtml = await RazorLightViewToStringRenderer.RenderViewToStringAsync(viewKey, model);
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
        try
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

            var viewKey = "_TestEmail.cshtml";

            var renderedHtml = await RazorLightViewToStringRenderer.RenderViewToStringAsync(viewKey, model);
            return renderedHtml;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to render test email for learner {learnerId}.", learnerId);
            return string.Empty;
        }
    }

    public async Task<string> RenderNewsletterAsync(Guid schoolId)
    {
        try
        {
            // TODO: Implement newsletter rendering
            await Task.Delay(1000);
            return string.Empty;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to render newsletter for school {schoolId}.", schoolId);
            return string.Empty;
        }
    }
}
using Lisa.Models.Entities;
using Lisa.Models.ViewModels;
using System.Xml.Linq;
using System;
using System.Collections.Generic;

namespace Lisa.Services;
public class TemplateRenderService
(
    ILogger<EmailRendererService> logger,
    ProgressFeedbackService progressFeedbackService,
    UserService userService,
    RazorLightViewToStringRenderer razorViewToStringRenderer
)
{
    public async Task<string> RenderProgressFeedbackAsync(List<Guid> learnerIds, DateTime? fromDate = null, DateTime? toDate = null)
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

            var viewKey = "Lisa.Templates._ProgressFeedbackTemplate.cshtml";

            var renderedHtml = await razorViewToStringRenderer.RenderViewToStringAsync(viewKey, feedback);
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
}
using Lisa.Models.Entities;

namespace Lisa.Models.EmailModels;

public interface ICampaignTemplateProcessor
{
    /// <summary>
    /// Determines if this processor can handle the provided template.
    /// </summary>
    bool CanProcess(EmailTemplate template);

    /// <summary>
    /// Generates the HTML content for the email campaign.
    /// </summary>
    Task<string> GenerateHtmlAsync(CommunicationCommand command);

    /// <summary>
    /// Performs any additional actions required for the campaign.
    /// For example, sending progress reports immediately.
    /// </summary>
    Task ProcessAdditionalActionsAsync(CommunicationCommand command);
}

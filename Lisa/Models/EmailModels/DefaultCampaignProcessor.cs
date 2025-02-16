using Lisa.Models.Entities;

namespace Lisa.Models.EmailModels;

public class DefaultCampaignProcessor : ICampaignTemplateProcessor
{
    public bool CanProcess(EmailTemplate template) => true;

    public Task<string> GenerateHtmlAsync(CommunicationRequest request)
    {
        return Task.FromResult("<p>No content available</p>");
    }

    public Task ProcessAdditionalActionsAsync(CommunicationRequest request)
    {
        return Task.CompletedTask;
    }
}

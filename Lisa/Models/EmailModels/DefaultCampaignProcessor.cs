using Lisa.Models.Entities;

namespace Lisa.Models.EmailModels;

public class DefaultCampaignProcessor : ICampaignTemplateProcessor
{
    public bool CanProcess(EmailTemplate template) => true;

    public Task<string> GenerateHtmlAsync(CommunicationCommand command)
    {
        return Task.FromResult("<p>No content available</p>");
    }

    public Task ProcessAdditionalActionsAsync(CommunicationCommand command)
    {
        return Task.CompletedTask;
    }
}

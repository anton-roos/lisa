using Lisa.Enums;

namespace Lisa.Models.EmailModels;

public class DefaultCampaignProcessor : ICampaignTemplateProcessor
{
    public bool CanProcess(Template template) => true;

    public Task<string> GenerateHtmlAsync(CommunicationCommand command)
    {
        return Task.FromResult("<p>No content available</p>");
    }

    public Task ProcessAdditionalActionsAsync(CommunicationCommand command)
    {
        return Task.CompletedTask;
    }
}

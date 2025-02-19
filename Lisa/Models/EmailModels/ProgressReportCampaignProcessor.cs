using Hangfire;
using Lisa.Enums;
using Lisa.Models.Entities;
using Lisa.Services;

namespace Lisa.Models.EmailModels;

public class ProgressReportCampaignProcessor(

    IBackgroundJobClient backgroundJobClient) : ICampaignTemplateProcessor
{
    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    public bool CanProcess(Template template) =>
        template != Template.None;

    public async Task<string> GenerateHtmlAsync(CommunicationCommand command)
    {
        Task.Delay(1000).Wait();
        return "";
    }

    public Task ProcessAdditionalActionsAsync(CommunicationCommand command)
    {
        _backgroundJobClient.Enqueue<EmailCampaignService>(service =>
            service.SendProgressReportsAsync(command.SchoolId));

        return Task.CompletedTask;
    }
}

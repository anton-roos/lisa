using Hangfire;
using Lisa.Models.Entities;
using Lisa.Services;

namespace Lisa.Models.EmailModels;

public class ProgressReportCampaignProcessor : ICampaignTemplateProcessor
{
    private readonly EmailRendererService _emailRendererService;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ResultService _resultService;

    public ProgressReportCampaignProcessor(
        EmailRendererService emailRendererService,
        IBackgroundJobClient backgroundJobClient,
        ResultService resultService)
    {
        _emailRendererService = emailRendererService;
        _backgroundJobClient = backgroundJobClient;
        _resultService = resultService;
    }

    public bool CanProcess(EmailTemplate template) =>
        template.Name?.Equals("Progress Report", StringComparison.OrdinalIgnoreCase) == true;

    public async Task<string> GenerateHtmlAsync(CommunicationCommand command)
    {
        var resultSets = await _resultService.GetResultsByFiltersAsync(command.SchoolId, null, null, null, command.LearnerId);
        var progressReportModel = new ProgressFeedback
        {
            LearnerName = "Default Child",
            ResultsBySubject = []
        };


        if (resultSets == null || resultSets.Count == 0)
        {
            return "<p>No results available</p>";
        }

        foreach (var resultSet in resultSets)
        {
            //progressReportModel.ResultsBySubject.AddRange(resultSet.Results);
        }
        
        
        var renderedHtml = await _emailRendererService.RenderTemplateAsync(
            $"template-{command.TemplateId}",
            command.EmailTemplate.Content,
            progressReportModel);

        return string.IsNullOrWhiteSpace(renderedHtml)
            ? "<p>No content available</p>"
            : renderedHtml;
    }

    public Task ProcessAdditionalActionsAsync(CommunicationCommand command)
    {
        // Use IBackgroundJobClient to enqueue the job without directly depending on EmailCampaignService.
        _backgroundJobClient.Enqueue<EmailCampaignService>(service =>
            service.SendProgressReportsAsync(command.SchoolId, command.TemplateId));

        return Task.CompletedTask;
    }
}

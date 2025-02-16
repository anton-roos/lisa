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

    public async Task<string> GenerateHtmlAsync(CommunicationRequest request)
    {
        var resultSets = await _resultService.GetResultsByFiltersAsync(request.SchoolId, null, null, null, request.LearnerId);
        var progressReportModel = new ProgressReportModel
        {
            ChildName = "Default Child",
            Results = []
        };


        if (resultSets == null || resultSets.Count == 0)
        {
            return "<p>No results available</p>";
        }

        foreach (var resultSet in resultSets)
        {
            progressReportModel.Results.AddRange(resultSet.Results);
        }
        
        
        var renderedHtml = await _emailRendererService.RenderTemplateAsync(
            $"template-{request.TemplateId}",
            request.EmailTemplate.Content,
            progressReportModel);

        return string.IsNullOrWhiteSpace(renderedHtml)
            ? "<p>No content available</p>"
            : renderedHtml;
    }

    public Task ProcessAdditionalActionsAsync(CommunicationRequest request)
    {
        // Use IBackgroundJobClient to enqueue the job without directly depending on EmailCampaignService.
        _backgroundJobClient.Enqueue<EmailCampaignService>(service =>
            service.SendProgressReportsAsync(request.SchoolId, request.TemplateId));

        return Task.CompletedTask;
    }
}

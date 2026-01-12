using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Lisa.Models.AcademicPlanning;

namespace Lisa.Infrastructure.AcademicPlanning
{
    public class AcademicPlanPdfExporter
    {
        public byte[] Export(TeachingPlan plan)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"School Grade ID: {plan.SchoolGradeId}");
                        col.Item().Text($"Subject ID: {plan.SubjectId}");
                        col.Item().Text($"Status: {plan.Status}");
                        col.Item().Text($"Created: {plan.CreatedAt:yyyy-MM-dd}");

                        foreach (var week in plan.Weeks.OrderBy(w => w.WeekNumber))
                        {
                            col.Item().PaddingTop(10).Text($"Week {week.WeekNumber}");
                            
                            if (week.StartDate.HasValue)
                            {
                                col.Item().Text($"Period: {week.StartDate.Value:yyyy-MM-dd} to {week.EndDate?.ToString("yyyy-MM-dd") ?? "N/A"}");
                            }
                            
                            if (!string.IsNullOrEmpty(week.Notes))
                            {
                                col.Item().Text($"Notes: {week.Notes}");
                            }

                            foreach (var period in week.Periods.OrderBy(p => p.PeriodNumber))
                            {
                                col.Item().PaddingLeft(10).Column(periodCol =>
                                {
                                    periodCol.Item().Text($"Period {period.PeriodNumber}: {period.Topic ?? "No topic"}");
                                    if (!string.IsNullOrEmpty(period.Resources))
                                        periodCol.Item().PaddingLeft(10).Text($"Resources: {period.Resources}");
                                    if (!string.IsNullOrEmpty(period.Assessment))
                                        periodCol.Item().PaddingLeft(10).Text($"Assessment: {period.Assessment}");
                                    if (!string.IsNullOrEmpty(period.Homework))
                                        periodCol.Item().PaddingLeft(10).Text($"Homework: {period.Homework}");
                                });
                            }
                        }
                    });
                });
            }).GeneratePdf();
        }
    }
}
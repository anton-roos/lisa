using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Lisa.Models.AcademicPlanning;

namespace Lisa.Infrastructure.AcademicPlanning
{
    public class AcademicPlanPdfExporter
    {
        public byte[] Export(AcademicPlanExportData exportData)
        {
            var plan = exportData.Plan;
            
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);
                    page.Content().Column(col =>
                    {
                        // Header Section
                        col.Item().PaddingBottom(10).Row(row =>
                        {
                            row.RelativeItem().Text("Year:").FontSize(10).FontFamily(Fonts.Arial);
                            row.RelativeItem().Text(exportData.Year.ToString()).FontSize(10).FontFamily(Fonts.Arial).Bold();
                            row.RelativeItem().Text("Term:").FontSize(10).FontFamily(Fonts.Arial);
                            row.RelativeItem().Text(plan.Term.ToString()).FontSize(10).FontFamily(Fonts.Arial).Bold();
                            row.RelativeItem().Text("Grade:").FontSize(10).FontFamily(Fonts.Arial);
                            row.RelativeItem().Text(exportData.GradeName).FontSize(10).FontFamily(Fonts.Arial).Bold();
                            row.RelativeItem().Text("Subject:").FontSize(10).FontFamily(Fonts.Arial);
                            row.RelativeItem().Text(exportData.SubjectName).FontSize(10).FontFamily(Fonts.Arial).Bold();
                            row.RelativeItem().Text("Teacher:").FontSize(10).FontFamily(Fonts.Arial);
                            row.RelativeItem().Text(exportData.TeacherName).FontSize(10).FontFamily(Fonts.Arial).Bold();
                        });

                        col.Item().PaddingTop(10).PaddingBottom(5).Text("Academic Planning Template").FontSize(14).Bold();
                        
                        // Table Header
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); // Topic as per ATP
                                columns.RelativeColumn(1); // Week
                                columns.RelativeColumn(1); // Period
                                columns.RelativeColumn(2); // Lesson Topic
                                columns.RelativeColumn(1.5f); // Sub-Topic
                                columns.RelativeColumn(2); // Lesson detail
                                columns.RelativeColumn(1.5f); // Class work
                                columns.RelativeColumn(1.5f); // Home work
                                columns.RelativeColumn(1); // % Planned
                                columns.RelativeColumn(1); // % Actual
                                columns.RelativeColumn(1.2f); // Date Planned
                                columns.RelativeColumn(1.2f); // Date Actual
                            });

                            // Header Row
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Topic as per ATP").FontSize(9).Bold();
                                header.Cell().Element(CellStyle).Background(Colors.Grey.Lighten3).Text("Week").FontSize(9).Bold();
                                header.Cell().Element(CellStyle).Background(Colors.Grey.Lighten3).Text("Period").FontSize(9).Bold();
                                header.Cell().Element(CellStyle).Text("Lesson Topic").FontSize(9).Bold();
                                header.Cell().Element(CellStyle).Text("Sub-Topic").FontSize(9).Bold();
                                header.Cell().Element(CellStyle).Text("Lesson detail").FontSize(9).Bold();
                                header.Cell().Element(CellStyle).Text("Class work").FontSize(9).Bold();
                                header.Cell().Element(CellStyle).Text("Home work").FontSize(9).Bold();
                                header.Cell().Element(CellStyle).Text("% Planned").FontSize(9).Bold();
                                header.Cell().Element(CellStyle).Text("% Actual").FontSize(9).Bold();
                                header.Cell().Element(CellStyle).Text("Date Planned").FontSize(9).Bold();
                                header.Cell().Element(CellStyle).Text("Date Actual").FontSize(9).Bold();
                            });

                            // Data Rows
                            var weeks = plan.Weeks.OrderBy(w => w.WeekNumber).ToList();
                            int? currentWeek = null;
                            
                            foreach (var week in weeks)
                            {
                                var periods = week.Periods.OrderBy(p => p.PeriodNumber).ToList();
                                
                                foreach (var period in periods)
                                {
                                    table.Cell().Element(CellStyle).Text("").FontSize(8); // Topic as per ATP
                                    
                                    // Week - only show on first period
                                    if (currentWeek != week.WeekNumber)
                                    {
                                        table.Cell().Element(CellStyle).Background(Colors.Grey.Lighten3)
                                            .Text(week.WeekNumber.ToString()).FontSize(8).AlignCenter();
                                        currentWeek = week.WeekNumber;
                                    }
                                    else
                                    {
                                        table.Cell().Element(CellStyle).Background(Colors.Grey.Lighten3).Text("").FontSize(8);
                                    }
                                    
                                    table.Cell().Element(CellStyle).Background(Colors.Grey.Lighten3)
                                        .Text(period.PeriodNumber.ToString()).FontSize(8).AlignCenter();
                                    table.Cell().Element(CellStyle).Text(period.Topic ?? "").FontSize(8);
                                    table.Cell().Element(CellStyle).Text(period.SubTopic ?? "").FontSize(8);
                                    
                                    table.Cell().Element(CellStyle).Text(period.LessonDetailDescription ?? "").FontSize(8);
                                    table.Cell().Element(CellStyle).Text(period.ClassWorkDescription ?? "").FontSize(8);
                                    table.Cell().Element(CellStyle).Text(period.Homework ?? "").FontSize(8);
                                    
                                    table.Cell().Element(CellStyle).Text(
                                        period.PercentagePlanned.HasValue ? period.PercentagePlanned.Value.ToString("0") : ""
                                    ).FontSize(8).AlignCenter();
                                    
                                    table.Cell().Element(CellStyle).Text(
                                        period.PercentageCompleted.HasValue ? period.PercentageCompleted.Value.ToString("0") : ""
                                    ).FontSize(8).AlignCenter();
                                    
                                    table.Cell().Element(CellStyle).Text(
                                        period.DatePlanned.HasValue ? period.DatePlanned.Value.ToString("yyyy-MM-dd") : ""
                                    ).FontSize(8).AlignCenter();
                                    
                                    table.Cell().Element(CellStyle).Text(
                                        period.DateCompleted.HasValue ? period.DateCompleted.Value.ToString("yyyy-MM-dd") : ""
                                    ).FontSize(8).AlignCenter();
                                }
                            }
                        });
                    });
                });
            }).GeneratePdf();
        }
        
        private static IContainer CellStyle(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten1)
                .Padding(4)
                .AlignMiddle();
        }
        
        // Legacy method for backward compatibility
        public byte[] Export(TeachingPlan plan)
        {
            return Export(new AcademicPlanExportData
            {
                Plan = plan,
                SchoolName = string.Empty,
                GradeName = string.Empty,
                SubjectName = string.Empty,
                TeacherName = string.Empty,
                Year = DateTime.UtcNow.Year
            });
        }
    }
}

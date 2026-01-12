using System.Linq;
using ClosedXML.Excel;
using Lisa.Models.AcademicPlanning;

namespace Lisa.Infrastructure.AcademicPlanning
{
    public class AcademicPlanExcelExporter
    {
        public byte[] Export(TeachingPlan plan)
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Academic Plan");
            
            sheet.Cell(1, 1).Value = "School Grade ID";
            sheet.Cell(1, 2).Value = plan.SchoolGradeId.ToString(); 
            sheet.Cell(2, 1).Value = "Subject ID";
            sheet.Cell(2, 2).Value = plan.SubjectId; 
            sheet.Cell(3, 1).Value = "Status";
            sheet.Cell(3, 2).Value = plan.Status.ToString(); 

            int row = 5;

            foreach (var week in plan.Weeks.OrderBy(w => w.WeekNumber))
            {
                sheet.Cell(row, 1).Value = $"Week {week.WeekNumber}";
                if (week.StartDate.HasValue)
                {
                    sheet.Cell(row, 2).Value = $"({week.StartDate.Value:yyyy-MM-dd} to {week.EndDate?.ToString("yyyy-MM-dd") ?? "N/A"})";
                }
                row++;

                sheet.Cell(row, 1).Value = "Period";
                sheet.Cell(row, 2).Value = "Topic";
                sheet.Cell(row, 3).Value = "Resources";
                sheet.Cell(row, 4).Value = "Assessment";
                sheet.Cell(row, 5).Value = "Homework";
                row++;

                foreach (var period in week.Periods.OrderBy(p => p.PeriodNumber))
                {
                    sheet.Cell(row, 1).Value = period.PeriodNumber;
                    sheet.Cell(row, 2).Value = period.Topic ?? string.Empty;
                    sheet.Cell(row, 3).Value = period.Resources ?? string.Empty;
                    sheet.Cell(row, 4).Value = period.Assessment ?? string.Empty;
                    sheet.Cell(row, 5).Value = period.Homework ?? string.Empty;
                    row++;
                }

                row += 2;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
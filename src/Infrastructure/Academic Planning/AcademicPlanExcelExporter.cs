using System.Linq;
using ClosedXML.Excel;
using Lisa.Models.AcademicPlanning;

namespace Lisa.Infrastructure.AcademicPlanning
{
    public class AcademicPlanExcelExporter
    {
        public byte[] Export(AcademicPlanExportData exportData)
        {
            var plan = exportData.Plan;
            
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Academic Plan");
            
            // Define styles
            var headerStyle = workbook.Style;
            headerStyle.Font.Bold = true;
            headerStyle.Font.FontSize = 11;
            
            var lightBlueFill = XLColor.FromHtml("#D9E1F2");
            var headerFill = XLColor.FromHtml("#D9E1F2");
            
            // Row 1: Header information
            sheet.Cell(1, 1).Value = "Year:";
            sheet.Cell(1, 2).Value = exportData.Year;
            sheet.Cell(1, 3).Value = "Term:";
            sheet.Cell(1, 4).Value = plan.Term;
            sheet.Cell(1, 5).Value = "Grade:";
            sheet.Cell(1, 6).Value = exportData.GradeName;
            sheet.Cell(1, 7).Value = "Subject:";
            sheet.Cell(1, 8).Value = exportData.SubjectName;
            sheet.Cell(1, 9).Value = "Teacher:";
            sheet.Cell(1, 10).Value = exportData.TeacherName;
            
            // Row 2: Column headers - "Topic as per ATP" spanning A-D
            sheet.Range(2, 1, 2, 4).Merge().Value = "Topic as per ATP";
            sheet.Range(2, 1, 2, 4).Style.Font.Bold = true;
            sheet.Range(2, 1, 2, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Range(2, 1, 2, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            
            // Column headers in exact order matching the form:
            // Period, Lesson Topic, Sub-Topic, Lesson Detail, Class Work, Homework, Planned %, Actual %, Date Planned, Actual Date
            int col = 5; // Start from column E
            
            sheet.Cell(2, col).Value = "Week";
            sheet.Cell(2, col).Style.Fill.BackgroundColor = lightBlueFill;
            sheet.Cell(2, col).Style.Font.Bold = true;
            sheet.Cell(2, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            col++;
            
            sheet.Cell(2, col).Value = "Period";
            sheet.Cell(2, col).Style.Fill.BackgroundColor = lightBlueFill;
            sheet.Cell(2, col).Style.Font.Bold = true;
            sheet.Cell(2, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            col++;
            
            sheet.Cell(2, col).Value = "Lesson Topic";
            sheet.Cell(2, col).Style.Font.Bold = true;
            sheet.Cell(2, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            col++;
            
            sheet.Cell(2, col).Value = "Sub-Topic";
            sheet.Cell(2, col).Style.Font.Bold = true;
            sheet.Cell(2, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            col++;
            
            sheet.Cell(2, col).Value = "Lesson Detail";
            sheet.Cell(2, col).Style.Font.Bold = true;
            sheet.Cell(2, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            col++;
            
            sheet.Cell(2, col).Value = "Class Work";
            sheet.Cell(2, col).Style.Font.Bold = true;
            sheet.Cell(2, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            col++;
            
            sheet.Cell(2, col).Value = "Homework";
            sheet.Cell(2, col).Style.Font.Bold = true;
            sheet.Cell(2, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            col++;
            
            sheet.Cell(2, col).Value = "Planned %";
            sheet.Cell(2, col).Style.Font.Bold = true;
            sheet.Cell(2, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            col++;
            
            sheet.Cell(2, col).Value = "Actual %";
            sheet.Cell(2, col).Style.Font.Bold = true;
            sheet.Cell(2, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            col++;
            
            sheet.Cell(2, col).Value = "Date Planned";
            sheet.Cell(2, col).Style.Font.Bold = true;
            sheet.Cell(2, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            col++;
            
            sheet.Cell(2, col).Value = "Actual Date";
            sheet.Cell(2, col).Style.Font.Bold = true;
            sheet.Cell(2, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            
            // Row 3: Repeat headers for frozen pane
            sheet.Cell(3, 5).Value = "Week";
            sheet.Cell(3, 5).Style.Fill.BackgroundColor = lightBlueFill;
            sheet.Cell(3, 5).Style.Font.Bold = true;
            sheet.Cell(3, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            
            sheet.Cell(3, 6).Value = "Period";
            sheet.Cell(3, 6).Style.Fill.BackgroundColor = lightBlueFill;
            sheet.Cell(3, 6).Style.Font.Bold = true;
            sheet.Cell(3, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            
            // Apply borders to all header cells in row 3
            for (int c = 7; c <= 15; c++)
            {
                sheet.Cell(3, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
            
            int currentRow = 4;
            
            // Group periods by week
            var weeks = plan.Weeks.OrderBy(w => w.WeekNumber).ToList();
            
            int? lastWeekNumber = null;
            int? weekStartRow = null;
            
            foreach (var week in weeks)
            {
                var periods = week.Periods.OrderBy(p => p.PeriodNumber).ToList();
                
                foreach (var period in periods)
                {
                    // Week column (E) - merge cells for same week
                    if (lastWeekNumber != week.WeekNumber)
                    {
                        // If we had a previous week, merge its cells
                        if (weekStartRow.HasValue && lastWeekNumber.HasValue)
                        {
                            var weekEndRow = currentRow - 1;
                            if (weekEndRow >= weekStartRow.Value)
                            {
                                sheet.Range(weekStartRow.Value, 5, weekEndRow, 5).Merge();
                                sheet.Cell(weekStartRow.Value, 5).Style.Fill.BackgroundColor = lightBlueFill;
                                sheet.Cell(weekStartRow.Value, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                sheet.Cell(weekStartRow.Value, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            }
                        }
                        
                        // Start new week
                        weekStartRow = currentRow;
                        lastWeekNumber = week.WeekNumber;
                        sheet.Cell(currentRow, 5).Value = week.WeekNumber;
                        sheet.Cell(currentRow, 5).Style.Fill.BackgroundColor = lightBlueFill;
                        sheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        sheet.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    
                    // Period column (F)
                    sheet.Cell(currentRow, 6).Value = period.PeriodNumber;
                    sheet.Cell(currentRow, 6).Style.Fill.BackgroundColor = lightBlueFill;
                    sheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    sheet.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    
                    // Lesson Topic (G) - Column 7
                    sheet.Cell(currentRow, 7).Value = period.Topic ?? string.Empty;
                    sheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    sheet.Cell(currentRow, 7).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    sheet.Cell(currentRow, 7).Style.Alignment.WrapText = true;
                    
                    // Sub-Topic (H) - Column 8
                    sheet.Cell(currentRow, 8).Value = period.SubTopic ?? string.Empty;
                    sheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    sheet.Cell(currentRow, 8).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    sheet.Cell(currentRow, 8).Style.Alignment.WrapText = true;
                    
                    // Lesson Detail (I) - Column 9
                    sheet.Cell(currentRow, 9).Value = period.LessonDetailDescription ?? string.Empty;
                    sheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    sheet.Cell(currentRow, 9).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    sheet.Cell(currentRow, 9).Style.Alignment.WrapText = true;
                    
                    // Class Work (J) - Column 10
                    sheet.Cell(currentRow, 10).Value = period.ClassWorkDescription ?? string.Empty;
                    sheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    sheet.Cell(currentRow, 10).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    sheet.Cell(currentRow, 10).Style.Alignment.WrapText = true;
                    
                    // Homework (K) - Column 11
                    sheet.Cell(currentRow, 11).Value = period.Homework ?? string.Empty;
                    sheet.Cell(currentRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    sheet.Cell(currentRow, 11).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    sheet.Cell(currentRow, 11).Style.Alignment.WrapText = true;
                    
                    // Planned % (L) - Column 12
                    if (period.PercentagePlanned.HasValue)
                    {
                        sheet.Cell(currentRow, 12).Value = period.PercentagePlanned.Value;
                        sheet.Cell(currentRow, 12).Style.NumberFormat.Format = "0";
                    }
                    sheet.Cell(currentRow, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    sheet.Cell(currentRow, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    
                    // Actual % (M) - Column 13
                    if (period.PercentageCompleted.HasValue)
                    {
                        sheet.Cell(currentRow, 13).Value = period.PercentageCompleted.Value;
                        sheet.Cell(currentRow, 13).Style.NumberFormat.Format = "0";
                    }
                    sheet.Cell(currentRow, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    sheet.Cell(currentRow, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    
                    // Date Planned (N) - Column 14
                    if (period.DatePlanned.HasValue)
                    {
                        sheet.Cell(currentRow, 14).Value = period.DatePlanned.Value;
                        sheet.Cell(currentRow, 14).Style.NumberFormat.Format = "yyyy-mm-dd";
                    }
                    sheet.Cell(currentRow, 14).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    sheet.Cell(currentRow, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    
                    // Actual Date (O) - Column 15
                    if (period.DateCompleted.HasValue)
                    {
                        sheet.Cell(currentRow, 15).Value = period.DateCompleted.Value;
                        sheet.Cell(currentRow, 15).Style.NumberFormat.Format = "yyyy-mm-dd";
                    }
                    sheet.Cell(currentRow, 15).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    sheet.Cell(currentRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    
                    // Set row height for better text visibility
                    sheet.Row(currentRow).Height = 60; // Increased height for text areas
                    
                    currentRow++;
                }
            }
            
            // Merge the last week's cells
            if (weekStartRow.HasValue && lastWeekNumber.HasValue)
            {
                var weekEndRow = currentRow - 1;
                if (weekEndRow >= weekStartRow.Value)
                {
                    sheet.Range(weekStartRow.Value, 5, weekEndRow, 5).Merge();
                    sheet.Cell(weekStartRow.Value, 5).Style.Fill.BackgroundColor = lightBlueFill;
                    sheet.Cell(weekStartRow.Value, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    sheet.Cell(weekStartRow.Value, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
            }
            
            // Set column widths - optimized for the new field order
            sheet.Column(1).Width = 2; // A (Topic as per ATP start)
            sheet.Column(2).Width = 2;
            sheet.Column(3).Width = 2;
            sheet.Column(4).Width = 2;
            sheet.Column(5).Width = 8; // Week
            sheet.Column(6).Width = 8; // Period
            sheet.Column(7).Width = 20; // Lesson Topic
            sheet.Column(8).Width = 15; // Sub-Topic
            sheet.Column(9).Width = 30; // Lesson Detail (increased for better visibility)
            sheet.Column(10).Width = 30; // Class Work (increased for better visibility)
            sheet.Column(11).Width = 30; // Homework (increased for better visibility)
            sheet.Column(12).Width = 12; // Planned %
            sheet.Column(13).Width = 12; // Actual %
            sheet.Column(14).Width = 15; // Date Planned
            sheet.Column(15).Width = 15; // Actual Date
            
            // Freeze panes at row 3
            sheet.SheetView.FreezeRows(3);
            
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
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

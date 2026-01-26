using Lisa.Infrastructure.AcademicPlanning;
using Lisa.Models.AcademicPlanning;
using QuestPDF.Fluent;

public class AcademicPlanExportService
{
    private readonly AcademicPlanExcelExporter _excel;
    private readonly AcademicPlanPdfExporter _pdf;

    public AcademicPlanExportService(
        AcademicPlanExcelExporter excel,
        AcademicPlanPdfExporter pdf)
    {
        _excel = excel;
        _pdf = pdf;
    }

    public byte[] ExportExcel(TeachingPlan plan) => _excel.Export(plan);
    public byte[] ExportExcel(AcademicPlanExportData exportData) => _excel.Export(exportData);
    
    public byte[] ExportPdf(TeachingPlan plan) => _pdf.Export(plan);
    public byte[] ExportPdf(AcademicPlanExportData exportData) => _pdf.Export(exportData);
    
    public byte[] ExportToPdf(TeachingPlan plan)
    {
        return _pdf.Export(plan); 
    }
}
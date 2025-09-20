using System;
using System.IO;
using OfficeOpenXml;

class DebugExcel
{
    static void Main(string[] args)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        
        string[] files = {
            @"..\..\Input\01552_25_03641_v1_0_newCARASI_HEV_J3U_LatAm_P2F2.xlsx",
            @"..\..\Input\01552_21_02627_v31_1_oldCARASI_eVCU_CTEPh2022_W1C9.xlsx"
        };
        
        foreach (string file in files)
        {
            if (!File.Exists(file)) continue;
            
            Console.WriteLine($"\n=== {Path.GetFileName(file)} ===");
            
            using (var package = new ExcelPackage(new FileInfo(file)))
            {
                Console.WriteLine($"Worksheets: {package.Workbook.Worksheets.Count}");
                
                foreach (var sheet in package.Workbook.Worksheets)
                {
                    Console.WriteLine($"- Sheet: '{sheet.Name}' ({sheet.Dimension?.Rows ?? 0} rows)");
                    
                    // Show first few rows to understand structure
                    if (sheet.Dimension != null && sheet.Dimension.Rows > 0)
                    {
                        Console.WriteLine("  First row:");
                        for (int col = 1; col <= Math.Min(10, sheet.Dimension.Columns); col++)
                        {
                            var value = sheet.Cells[1, col].Text;
                            Console.WriteLine($"    [{col}]: '{value}'");
                        }
                    }
                }
            }
        }
        
        Console.WriteLine("\nPress any key...");
        Console.ReadKey();
    }
}

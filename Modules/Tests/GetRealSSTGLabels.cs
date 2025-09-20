using System;
using System.IO;
using System.Linq;
using OfficeOpenXml;

class GetRealSSTGLabels
{
    static void Main(string[] args)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        
        string file = @"..\..\Input\01552_25_03641_v1_0_newCARASI_HEV_J3U_LatAm_P2F2.xlsx";
        
        if (!File.Exists(file))
        {
            Console.WriteLine("File not found!");
            return;
        }
        
        Console.WriteLine("=== REAL SSTG LABELS ===");
        
        using (var package = new ExcelPackage(new FileInfo(file)))
        {
            var interfacesSheet = package.Workbook.Worksheets["Interfaces"];
            if (interfacesSheet == null)
            {
                Console.WriteLine("Interfaces sheet not found!");
                return;
            }
            
            Console.WriteLine($"Total rows: {interfacesSheet.Dimension.Rows}");
            
            // Get first 20 SSTG labels (column 5)
            Console.WriteLine("\nFirst 20 SSTG Labels:");
            for (int row = 2; row <= Math.Min(21, interfacesSheet.Dimension.Rows); row++)
            {
                var sstgLabel = interfacesSheet.Cells[row, 5].Text;
                if (!string.IsNullOrEmpty(sstgLabel))
                {
                    Console.WriteLine($"'{sstgLabel}'");
                }
            }
        }
        
        Console.WriteLine("\nPress any key...");
        Console.ReadKey();
    }
}

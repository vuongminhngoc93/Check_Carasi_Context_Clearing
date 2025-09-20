using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OfficeOpenXml;

class QuickBenchmark
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== QUICK EPPlus PERFORMANCE TEST ===");
        Console.WriteLine("Testing Excel file loading speed...\n");
        
        // Find any Excel file in current directory or Input folder
        string[] searchPaths = { @"..\..\Input", "Input", ".", @"..\..\..\Input" };
        string testFile = null;
        
        foreach (string path in searchPaths)
        {
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, "*.xlsx", SearchOption.AllDirectories)
                    .Concat(Directory.GetFiles(path, "*.xls", SearchOption.AllDirectories))
                    .Take(1);
                    
                if (files.Any())
                {
                    testFile = files.First();
                    break;
                }
            }
        }
        
        if (testFile == null)
        {
            Console.WriteLine("No Excel files found for testing.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return;
        }
        
        Console.WriteLine($"Testing file: {Path.GetFileName(testFile)}");
        Console.WriteLine($"File size: {new FileInfo(testFile).Length / 1024} KB\n");
        
        // Test variables to search for
        var testVariables = new List<string>
        {
            "VehicleSpeed", "EngineRPM", "ThrottlePosition", "BrakePosition", "SteeringAngle",
            "FuelLevel", "CoolantTemp", "OilPressure", "BatteryVoltage", "AirIntakeTemp",
            "ExhaustTemp", "TurboBoost", "LambdaSensor", "CatalystTemp", "EGRPosition",
            "VVTPosition", "FuelPressure", "ManifoldPressure", "AirFlowRate", "FuelFlowRate",
            "WheelSpeedFL", "WheelSpeedFR", "WheelSpeedRL", "WheelSpeedRR", "LateralAccel",
            "LongitudinalAccel", "YawRate", "PitchRate", "RollRate", "GPS_Latitude",
            "GPS_Longitude", "GPS_Altitude", "GPS_Speed", "GPS_Heading", "Odometer",
            "TripDistance", "AmbientTemp", "CabinTemp", "ACCompressor", "FanSpeed",
            "HeaterCore", "DefoggerStatus", "WiperSpeed", "HeadlightStatus", "BrakeLight",
            "TurnSignalLeft", "TurnSignalRight", "HazardLights", "ParkingBrake", "GearPosition"
        };
        
        try
        {
            Console.WriteLine("Testing EPPlus performance...");
            var sw = Stopwatch.StartNew();
            
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo(testFile)))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    Console.WriteLine("No worksheets found in Excel file.");
                    return;
                }
                
                Console.WriteLine($"Worksheet loaded: {worksheet.Name}");
                Console.WriteLine($"Dimensions: {worksheet.Dimension?.Rows ?? 0} rows x {worksheet.Dimension?.Columns ?? 0} columns");
                
                // Load all data into memory for faster searching
                var allData = new List<string[]>();
                if (worksheet.Dimension != null)
                {
                    for (int row = 1; row <= worksheet.Dimension.Rows; row++)
                    {
                        var rowData = new string[worksheet.Dimension.Columns];
                        for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                        {
                            rowData[col - 1] = worksheet.Cells[row, col].Text ?? "";
                        }
                        allData.Add(rowData);
                    }
                }
                
                sw.Stop();
                Console.WriteLine($"✓ File loaded in {sw.ElapsedMilliseconds} ms");
                
                // Test searching performance
                sw.Restart();
                int foundCount = 0;
                
                foreach (string variable in testVariables)
                {
                    bool found = allData.Any(row => 
                        row.Any(cell => cell.IndexOf(variable, StringComparison.OrdinalIgnoreCase) >= 0));
                    
                    if (found) foundCount++;
                }
                
                sw.Stop();
                Console.WriteLine($"✓ Searched {testVariables.Count} variables in {sw.ElapsedMilliseconds} ms");
                Console.WriteLine($"✓ Found {foundCount} variables");
                Console.WriteLine($"✓ Average: {(double)sw.ElapsedMilliseconds / testVariables.Count:F2} ms per variable");
                
                // Performance summary
                Console.WriteLine("\n=== PERFORMANCE SUMMARY ===");
                Console.WriteLine($"Total time: {sw.ElapsedMilliseconds} ms");
                Console.WriteLine($"Search speed: {testVariables.Count * 1000.0 / sw.ElapsedMilliseconds:F0} variables/second");
                
                if (sw.ElapsedMilliseconds < 100)
                {
                    Console.WriteLine("🚀 EXCELLENT performance! EPPlus is very fast for this dataset.");
                }
                else if (sw.ElapsedMilliseconds < 500)
                {
                    Console.WriteLine("✅ GOOD performance! EPPlus shows significant improvement over OLEDB.");
                }
                else
                {
                    Console.WriteLine("⚠️ Performance could be optimized further.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}

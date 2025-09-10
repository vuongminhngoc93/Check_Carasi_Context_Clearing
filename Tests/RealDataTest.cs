using System;
using System.IO;

namespace Check_carasi_DF_ContextClearing.Tests
{
    class RealDataTest
    {
        // Note: This class provides test methods but doesn't have Main entry point
        // to avoid conflicts with SimpleTestRunner
        
        public static void TestInputFolderData()
        {
            Console.WriteLine("=== Testing Real Input Folder Data ===");
            
            string inputFolder = @"..\Input";
            string fullInputPath = Path.GetFullPath(inputFolder);
            
            Console.WriteLine("Input folder: " + fullInputPath);
            
            if (!Directory.Exists(fullInputPath))
            {
                Console.WriteLine("ERROR: Input folder not found at " + fullInputPath);
                return;
            }
            
            // Get all files in Input folder
            string[] files = Directory.GetFiles(fullInputPath);
            Console.WriteLine("Found " + files.Length + " files in Input folder:");
            
            int excelFiles = 0;
            int totalSize = 0;
            
            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                string extension = Path.GetExtension(file).ToLower();
                
                Console.WriteLine("  - " + Path.GetFileName(file) + " (" + fileInfo.Length + " bytes)");
                
                if (extension == ".xlsx" || extension == ".xls")
                {
                    excelFiles++;
                    Console.WriteLine("    → Excel file detected");
                }
                
                totalSize += (int)fileInfo.Length;
            }
            
            Console.WriteLine("\nSummary:");
            Console.WriteLine("  Total files: " + files.Length);
            Console.WriteLine("  Excel files: " + excelFiles);
            Console.WriteLine("  Total size: " + totalSize + " bytes");
            Console.WriteLine("  Input folder ready for processing!");
        }
        
        public static void TestFileAccessibility()
        {
            Console.WriteLine("=== Testing File Accessibility ===");
            
            string inputFolder = @"..\Input";
            string fullInputPath = Path.GetFullPath(inputFolder);
            
            if (!Directory.Exists(fullInputPath))
            {
                Console.WriteLine("Input folder not found!");
                return;
            }
            
            string[] files = Directory.GetFiles(fullInputPath);
            int accessibleFiles = 0;
            
            foreach (string file in files)
            {
                try
                {
                    // Test if file can be read
                    using (FileStream fs = File.OpenRead(file))
                    {
                        byte[] buffer = new byte[10];
                        fs.Read(buffer, 0, 10);
                        accessibleFiles++;
                        Console.WriteLine("  ✓ " + Path.GetFileName(file) + " - accessible");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("  ✗ " + Path.GetFileName(file) + " - error: " + ex.Message);
                }
            }
            
            Console.WriteLine("Accessible files: " + accessibleFiles + "/" + files.Length);
        }
    }
}

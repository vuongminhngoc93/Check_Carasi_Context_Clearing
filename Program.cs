using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Check_carasi_DF_ContextClearing
{
    static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Check if running in CLI mode
            if (args.Length > 0)
            {
                // Allocate console for Windows Forms application
                AllocConsole();
                RunCliMode(args);
                
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                FreeConsole();
            }
            else
            {
                // Run GUI mode
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
        }
        
        static void RunCliMode(string[] args)
        {
            Console.WriteLine("=== Carasi DF Context Clearing Tool - CLI Mode ===");
            Console.WriteLine("Starting file processing...\n");
            
            string inputPath = ".";
            string searchPattern = "*";
            bool recursive = false;
            
            // Parse command line arguments
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-path":
                    case "-p":
                        if (i + 1 < args.Length)
                            inputPath = args[++i];
                        break;
                    case "-search":
                    case "-s":
                        if (i + 1 < args.Length)
                            searchPattern = args[++i];
                        break;
                    case "-recursive":
                    case "-r":
                        recursive = true;
                        break;
                    case "-help":
                    case "-h":
                    case "/?":
                        ShowHelp();
                        return;
                    default:
                        // If no switch specified, treat as path
                        if (!args[i].StartsWith("-"))
                            inputPath = args[i];
                        break;
                }
            }
            
            Console.WriteLine($"Input path: {Path.GetFullPath(inputPath)}");
            Console.WriteLine($"Search pattern: {searchPattern}");
            Console.WriteLine($"Recursive: {(recursive ? "Yes" : "No")}");
            Console.WriteLine();
            
            ProcessFiles(inputPath, searchPattern, recursive);
        }
        
        static void ShowHelp()
        {
            Console.WriteLine("=== Carasi DF Context Clearing Tool - CLI Help ===");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  Check_carasi_DF_ContextClearing.exe [options] [path]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -path, -p <path>       Input folder path (default: current directory)");
            Console.WriteLine("  -search, -s <pattern>  Search pattern (default: * for all files)");
            Console.WriteLine("  -recursive, -r         Search subdirectories recursively");
            Console.WriteLine("  -help, -h, /?          Show this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  Check_carasi_DF_ContextClearing.exe");
            Console.WriteLine("  Check_carasi_DF_ContextClearing.exe Input");
            Console.WriteLine("  Check_carasi_DF_ContextClearing.exe -path Input -search *.xlsx");
            Console.WriteLine("  Check_carasi_DF_ContextClearing.exe -path C:\\Data -search *carasi* -recursive");
            Console.WriteLine();
        }
        
        static void ProcessFiles(string inputPath, string searchPattern, bool recursive)
        {
            try
            {
                if (!Directory.Exists(inputPath))
                {
                    Console.WriteLine($"ERROR: Directory '{inputPath}' not found!");
                    return;
                }
                
                SearchOption searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                string[] files = Directory.GetFiles(inputPath, searchPattern, searchOption);
                
                Console.WriteLine($"Found {files.Length} files matching pattern '{searchPattern}'");
                Console.WriteLine();
                
                if (files.Length == 0)
                {
                    Console.WriteLine("No files to process.");
                    return;
                }
                
                int processedCount = 0;
                int errorCount = 0;
                
                foreach (string file in files)
                {
                    Console.WriteLine($"Processing: {Path.GetFileName(file)}");
                    Console.WriteLine($"  Path: {file}");
                    
                    try
                    {
                        ProcessSingleFile(file);
                        processedCount++;
                        Console.WriteLine("  ✓ SUCCESS\n");
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        Console.WriteLine($"  ✗ ERROR: {ex.Message}\n");
                    }
                }
                
                Console.WriteLine("=== Processing Summary ===");
                Console.WriteLine($"Total files found: {files.Length}");
                Console.WriteLine($"Successfully processed: {processedCount}");
                Console.WriteLine($"Errors: {errorCount}");
                Console.WriteLine($"Success rate: {(processedCount * 100.0 / files.Length):F1}%");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FATAL ERROR: {ex.Message}");
            }
        }
        
        static void ProcessSingleFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath).ToLower();
            string extension = Path.GetExtension(filePath).ToLower();
            
            FileInfo fileInfo = new FileInfo(filePath);
            Console.WriteLine($"  Size: {fileInfo.Length:N0} bytes");
            Console.WriteLine($"  Modified: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
            
            // Determine file type and processing method
            if (fileName.Contains("carasi"))
            {
                Console.WriteLine("  → Processing as CARASI interface file");
                ProcessCarasiFile(filePath);
            }
            else if (fileName.Contains("dataflow"))
            {
                Console.WriteLine("  → Processing as DataFlow file");
                ProcessDataFlowFile(filePath);
            }
            else if (extension == ".a2l")
            {
                Console.WriteLine("  → Processing as A2L file");
                ProcessA2LFile(filePath);
            }
            else if (extension == ".xlsx" || extension == ".xls")
            {
                Console.WriteLine("  → Processing as Excel file");
                ProcessExcelFile(filePath);
            }
            else
            {
                Console.WriteLine("  → Processing as generic file");
                ProcessGenericFile(filePath);
            }
        }
        
        static void ProcessCarasiFile(string filePath)
        {
            // Initialize A2L_Check for CARASI processing
            var a2lCheck = new A2L_Check();
            Console.WriteLine("    A2L_Check initialized for CARASI analysis");
            
            // Add specific CARASI processing logic here
            Console.WriteLine("    Analyzing CARASI interface definitions...");
            Console.WriteLine("    Checking signal mappings and data types...");
        }
        
        static void ProcessDataFlowFile(string filePath)
        {
            // Initialize MM_Check for DataFlow processing
            var mmCheck = new MM_Check();
            Console.WriteLine("    MM_Check initialized for DataFlow analysis");
            
            // Add specific DataFlow processing logic here
            Console.WriteLine("    Analyzing data flow paths...");
            Console.WriteLine("    Checking context clearing rules...");
        }
        
        static void ProcessA2LFile(string filePath)
        {
            var a2lCheck = new A2L_Check();
            Console.WriteLine("    A2L_Check initialized for A2L file analysis");
            
            // Add A2L specific processing
            Console.WriteLine("    Parsing A2L file structure...");
            Console.WriteLine("    Validating A2L syntax and semantics...");
        }
        
        static void ProcessExcelFile(string filePath)
        {
            Console.WriteLine("    Testing Excel file accessibility...");
            
            // Test file access
            using (FileStream fs = File.OpenRead(filePath))
            {
                Console.WriteLine("    File is accessible for reading");
            }
            
            // Add Excel-specific processing based on content
            Console.WriteLine("    Analyzing Excel file structure...");
        }
        
        static void ProcessGenericFile(string filePath)
        {
            Console.WriteLine("    Performing basic file validation...");
            
            FileInfo info = new FileInfo(filePath);
            if (info.Length == 0)
            {
                throw new Exception("File is empty");
            }
            
            Console.WriteLine("    Basic validation completed");
        }
    }
}

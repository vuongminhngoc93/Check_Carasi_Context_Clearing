using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace Check_carasi_DF_ContextClearing
{
    /// <summary>
    /// High-Performance Excel Parser using EPPlus
    /// Replaces OLEDB-based Excel_Parser for better performance and reliability
    /// Author: Vuong Minh Ngoc (MS/EJV)
    /// Version: 2.0.0 - EPPlus Migration
    /// </summary>
    public class EPPlusExcelParser : IDisposable
    {
        #region Constants
        private const string CARASI_INTERFACES_SHEET = "Interfaces";
        private const string CARASI_DICTIONARY_SHEET = "Dictionary"; 
        private const string DATAFLOW_MAPPING_SHEET = "Mapping";
        private const string CARASI_LABEL_COLUMN = "SSTG label";
        private const string DATAFLOW_F2_COLUMN = "F2";
        private const string DATAFLOW_F17_COLUMN = "F17";
        #endregion

        #region Private Fields
        private ExcelPackage excelPackage;
        private string filePath;
        private string fileName;
        private DataTable templateDataTable;
        
        // Cached data for performance
        private Dictionary<string, bool> carasiCache;
        private Dictionary<string, bool> dataflowCache;
        private bool isDataLoaded = false;
        
        // Sheet data cache
        private DataTable interfacesData;
        private DataTable dictionaryData;
        private DataTable mappingData;
        #endregion

        #region Properties
        public string FilePath => filePath;
        public string FileName => fileName;
        public bool IsCarasiFile => fileName.ToLower().Contains("carasi");
        public bool IsDataflowFile => fileName.ToLower().Contains("dataflow");
        #endregion

        #region Constructor & Disposal
        public EPPlusExcelParser(string filePath, DataTable templateDataTable = null)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Excel file not found: {filePath}");

            this.filePath = filePath;
            this.fileName = Path.GetFileName(filePath);
            this.templateDataTable = templateDataTable?.Clone();
            
            // Initialize EPPlus with license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            // Open Excel file
            this.excelPackage = new ExcelPackage(new FileInfo(filePath));
            
            // Initialize caches
            this.carasiCache = new Dictionary<string, bool>();
            this.dataflowCache = new Dictionary<string, bool>();
        }

        public void Dispose()
        {
            excelPackage?.Dispose();
            interfacesData?.Dispose();
            dictionaryData?.Dispose();
            mappingData?.Dispose();
            carasiCache?.Clear();
            dataflowCache?.Clear();
        }
        #endregion

        #region Public Methods - Existence Check
        /// <summary>
        /// Check if variable exists in Carasi Interfaces sheet
        /// OPTIMIZED: Direct worksheet search - much faster than DataTable
        /// </summary>
        public bool IsExistCarasi(string variable)
        {
            if (string.IsNullOrEmpty(variable))
                return false;

            // Check cache first
            if (carasiCache.ContainsKey(variable))
                return carasiCache[variable];

            // Direct worksheet search - skip EnsureDataLoaded()
            var worksheet = excelPackage.Workbook.Worksheets[CARASI_INTERFACES_SHEET];
            if (worksheet?.Dimension != null)
            {
                // Direct search in SSTG label column (column 5)
                for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                {
                    var cellValue = worksheet.Cells[row, 5].Text;
                    if (string.Equals(variable, cellValue, StringComparison.OrdinalIgnoreCase))
                    {
                        carasiCache[variable] = true;
                        return true;
                    }
                }
            }

            carasiCache[variable] = false;
            return false;
        }

        /// <summary>
        /// Check if variable exists in Dataflow Mapping sheet (F2 or F17 columns)
        /// Much faster than OLEDB query with OR condition
        /// </summary>
        public bool IsExistDataflow(string variable)
        {
            if (string.IsNullOrEmpty(variable))
                return false;

            // Check cache first
            if (dataflowCache.ContainsKey(variable))
                return dataflowCache[variable];

            EnsureDataLoaded();
            
            bool exists = false;
            
            if (mappingData != null)
            {
                // Fast DataTable search with multiple columns (replaces OR query)
                var rows = mappingData.AsEnumerable()
                    .Where(row => row.Field<string>(DATAFLOW_F2_COLUMN) == variable ||
                                 row.Field<string>(DATAFLOW_F17_COLUMN) == variable);
                
                exists = rows.Any();
            }

            // Cache result for future queries
            dataflowCache[variable] = exists;
            return exists;
        }

        /// <summary>
        /// Batch check multiple variables - OPTIMIZED for speed
        /// Direct worksheet search without loading entire DataTable
        /// </summary>
        public Dictionary<string, bool> BatchCheckCarasi(IEnumerable<string> variables)
        {
            var results = new Dictionary<string, bool>();
            var uncachedVariables = new List<string>();

            // First check cache
            foreach (string variable in variables)
            {
                if (string.IsNullOrEmpty(variable))
                {
                    results[variable] = false;
                    continue;
                }

                if (carasiCache.ContainsKey(variable))
                {
                    results[variable] = carasiCache[variable];
                }
                else
                {
                    uncachedVariables.Add(variable);
                }
            }

            // Direct worksheet search for uncached variables - MUCH FASTER!
            if (uncachedVariables.Count > 0)
            {
                var worksheet = excelPackage.Workbook.Worksheets[CARASI_INTERFACES_SHEET];
                if (worksheet?.Dimension != null)
                {
                    // Find SSTG label column (column 5)
                    int sstgLabelColumn = 5;
                    var foundVariables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    // Direct search in worksheet - skip DataTable loading!
                    for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                    {
                        var cellValue = worksheet.Cells[row, sstgLabelColumn].Text;
                        if (!string.IsNullOrEmpty(cellValue) && 
                            uncachedVariables.Any(v => string.Equals(v, cellValue, StringComparison.OrdinalIgnoreCase)))
                        {
                            foundVariables.Add(cellValue);
                        }
                        
                        // Early exit if all variables found
                        if (foundVariables.Count >= uncachedVariables.Count)
                            break;
                    }

                    // Set results and cache
                    foreach (string variable in uncachedVariables)
                    {
                        bool exists = foundVariables.Contains(variable);
                        results[variable] = exists;
                        carasiCache[variable] = exists; // Cache for future
                    }
                }
                else
                {
                    // Sheet not found - mark all as false
                    foreach (string variable in uncachedVariables)
                    {
                        results[variable] = false;
                        carasiCache[variable] = false;
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Batch check multiple variables for Dataflow
        /// </summary>
        public Dictionary<string, bool> BatchCheckDataflow(IEnumerable<string> variables)
        {
            var results = new Dictionary<string, bool>();
            var uncachedVariables = new List<string>();

            // First check cache
            foreach (string variable in variables)
            {
                if (string.IsNullOrEmpty(variable))
                {
                    results[variable] = false;
                    continue;
                }

                if (dataflowCache.ContainsKey(variable))
                {
                    results[variable] = dataflowCache[variable];
                }
                else
                {
                    uncachedVariables.Add(variable);
                }
            }

            // Batch process uncached variables
            if (uncachedVariables.Count > 0)
            {
                EnsureDataLoaded();
                
                if (mappingData != null)
                {
                    // Single LINQ query for all variables with OR condition - SUPER FAST!
                    var existingVariables = mappingData.AsEnumerable()
                        .Where(row => uncachedVariables.Contains(row.Field<string>(DATAFLOW_F2_COLUMN)) ||
                                     uncachedVariables.Contains(row.Field<string>(DATAFLOW_F17_COLUMN)))
                        .SelectMany(row => new[] { row.Field<string>(DATAFLOW_F2_COLUMN), 
                                                  row.Field<string>(DATAFLOW_F17_COLUMN) })
                        .Where(val => !string.IsNullOrEmpty(val))
                        .ToHashSet();

                    foreach (string variable in uncachedVariables)
                    {
                        bool exists = existingVariables.Contains(variable);
                        results[variable] = exists;
                        dataflowCache[variable] = exists; // Cache for future
                    }
                }
            }

            return results;
        }
        #endregion

        #region Public Methods - Data Retrieval
        /// <summary>
        /// Get interface data for specific variable (replaces search_Variable)
        /// </summary>
        public DataTable GetInterfaceData(string variable)
        {
            if (string.IsNullOrEmpty(variable))
                return null;

            EnsureDataLoaded();

            if (IsCarasiFile && interfacesData != null)
            {
                var filteredRows = interfacesData.AsEnumerable()
                    .Where(row => row.Field<string>(CARASI_LABEL_COLUMN) == variable);

                if (filteredRows.Any())
                {
                    return filteredRows.CopyToDataTable();
                }
            }
            else if (IsDataflowFile && mappingData != null)
            {
                var filteredRows = mappingData.AsEnumerable()
                    .Where(row => row.Field<string>(DATAFLOW_F2_COLUMN) == variable ||
                                 row.Field<string>(DATAFLOW_F17_COLUMN) == variable);

                if (filteredRows.Any())
                {
                    return filteredRows.CopyToDataTable();
                }
            }

            return null;
        }

        /// <summary>
        /// Get dictionary data for Carasi variable
        /// </summary>
        public DataTable GetDictionaryData(string variable)
        {
            if (string.IsNullOrEmpty(variable) || !IsCarasiFile)
                return null;

            EnsureDataLoaded();

            if (dictionaryData != null)
            {
                var filteredRows = dictionaryData.AsEnumerable()
                    .Where(row => row.Field<string>(CARASI_LABEL_COLUMN) == variable);

                if (filteredRows.Any())
                {
                    return filteredRows.CopyToDataTable();
                }
            }

            return null;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Load Excel data into memory for fast searching
        /// This is done only once and cached for multiple searches
        /// </summary>
        private void EnsureDataLoaded()
        {
            if (isDataLoaded)
                return;

            try
            {
                // Load Carasi sheets
                if (IsCarasiFile)
                {
                    interfacesData = LoadWorksheetToDataTable(CARASI_INTERFACES_SHEET);
                    dictionaryData = LoadWorksheetToDataTable(CARASI_DICTIONARY_SHEET);
                }
                
                // Load Dataflow sheets
                if (IsDataflowFile)
                {
                    mappingData = LoadWorksheetToDataTable(DATAFLOW_MAPPING_SHEET);
                }

                isDataLoaded = true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load Excel data: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Convert Excel worksheet to DataTable for fast in-memory searching
        /// Much faster than multiple OLEDB queries
        /// </summary>
        private DataTable LoadWorksheetToDataTable(string worksheetName)
        {
            var worksheet = excelPackage.Workbook.Worksheets[worksheetName];
            if (worksheet == null)
                return null;

            var dataTable = new DataTable(worksheetName);
            
            // Get dimensions
            var start = worksheet.Dimension.Start;
            var end = worksheet.Dimension.End;
            
            // Create columns from first row (headers)
            for (int col = start.Column; col <= end.Column; col++)
            {
                var headerValue = worksheet.Cells[start.Row, col].Value?.ToString() ?? $"Column{col}";
                dataTable.Columns.Add(headerValue, typeof(string));
            }

            // Load data rows
            for (int row = start.Row + 1; row <= end.Row; row++)
            {
                var dataRow = dataTable.NewRow();
                bool hasData = false;

                for (int col = start.Column; col <= end.Column; col++)
                {
                    var cellValue = worksheet.Cells[row, col].Value?.ToString() ?? string.Empty;
                    dataRow[col - start.Column] = cellValue;
                    
                    if (!string.IsNullOrEmpty(cellValue))
                        hasData = true;
                }

                // Only add row if it has some data
                if (hasData)
                    dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }
        #endregion

        #region Static Utility Methods
        /// <summary>
        /// Create EPPlus parser with auto-detection of file type
        /// </summary>
        public static EPPlusExcelParser Create(string filePath, DataTable templateDataTable = null)
        {
            return new EPPlusExcelParser(filePath, templateDataTable);
        }

        /// <summary>
        /// Batch process multiple files and variables - Ultimate performance!
        /// </summary>
        public static Dictionary<string, Dictionary<string, bool>> BatchProcessFiles(
            IEnumerable<string> filePaths, 
            IEnumerable<string> variables)
        {
            var results = new Dictionary<string, Dictionary<string, bool>>();

            foreach (string filePath in filePaths)
            {
                try
                {
                    using (var parser = new EPPlusExcelParser(filePath))
                    {
                        Dictionary<string, bool> fileResults;
                        
                        if (parser.IsCarasiFile)
                        {
                            fileResults = parser.BatchCheckCarasi(variables);
                        }
                        else if (parser.IsDataflowFile)
                        {
                            fileResults = parser.BatchCheckDataflow(variables);
                        }
                        else
                        {
                            fileResults = variables.ToDictionary(v => v, v => false);
                        }

                        results[filePath] = fileResults;
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue processing other files
                    Console.WriteLine($"Error processing {filePath}: {ex.Message}");
                    results[filePath] = variables.ToDictionary(v => v, v => false);
                }
            }

            return results;
        }
        #endregion
    }
}

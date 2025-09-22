using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace Check_carasi_DF_ContextClearing
{
    public partial class UC_ContextClearing : UserControl
    {
        private bool isValidnewCarasi = false;
        private bool isValidoldCarasi = false;
        private bool isValidnewDataflow = false;
        private bool isValidoldDataflow = false;

        private string nameOfnewCarasi = string.Empty;
        private string nameOfoldCarasi = string.Empty;
        private string nameOfnewDataflow = string.Empty;
        private string nameOfoldDataflow = string.Empty;
        private string link2Folder = string.Empty;
        
        // A2L INTEGRATION: A2L file path for unified search
        private string a2lFilePath = string.Empty;
        public string A2LFilePath { get => a2lFilePath; set => a2lFilePath = value; }

        public string NameOfnewCarasi { get => nameOfnewCarasi; set => nameOfnewCarasi = value; }
        public string NameOfoldCarasi { get => nameOfoldCarasi; set => nameOfoldCarasi = value; }
        public string NameOfnewDataflow { get => nameOfnewDataflow; set => nameOfnewDataflow = value; }
        public string NameOfoldDataflow { get => nameOfoldDataflow; set => nameOfoldDataflow = value; }
        public string Link2Folder { get => link2Folder; set { if (value!= "" && folder_verifying(value)) link2Folder = value; } }

        internal Excel_Parser NewCarasi { get => newCarasi; set => newCarasi = value; }
        internal Excel_Parser OldCarasi { get => oldCarasi; set => oldCarasi = value; }
        internal Excel_Parser NewDF { get => newDF; set => newDF = value; }
        internal Excel_Parser OldDF { get => oldDF; set => oldDF = value; }

        // Control properties for PropertyDifferenceHighlighter
        public UC_Carasi OldCarasiControl => UC_OldCarasi;
        public UC_Carasi NewCarasiControl => UC_Newcarasi;
        public UC_dataflow OldDataflowControl => UC_OldDF;
        public UC_dataflow NewDataflowControl => UC_NewDF;

        // Properties for performance optimization features
        public bool IsHighlightingEnabled { get; set; } = true;

        // Performance optimization methods
        public async Task WarmupCacheAsync(List<string> variables)
        {
            try 
            {
                // PERFORMANCE: Pre-warm ExcelParserManager cache with file paths
                if (!string.IsNullOrEmpty(nameOfnewCarasi) && File.Exists(nameOfnewCarasi))
                {
                    var parser = ExcelParserManager.GetParser(nameOfnewCarasi, null);
                    System.Diagnostics.Debug.WriteLine($"WARMUP: Cached NewCarasi parser for {variables?.Count ?? 0} variables");
                }
                
                if (!string.IsNullOrEmpty(nameOfoldCarasi) && File.Exists(nameOfoldCarasi))
                {
                    var parser = ExcelParserManager.GetParser(nameOfoldCarasi, null);
                    System.Diagnostics.Debug.WriteLine($"WARMUP: Cached OldCarasi parser for {variables?.Count ?? 0} variables");
                }
                
                if (!string.IsNullOrEmpty(nameOfnewDataflow) && File.Exists(nameOfnewDataflow))
                {
                    var parser = ExcelParserManager.GetParser(nameOfnewDataflow, null);
                    System.Diagnostics.Debug.WriteLine($"WARMUP: Cached NewDataflow parser for {variables?.Count ?? 0} variables");
                }
                
                if (!string.IsNullOrEmpty(nameOfoldDataflow) && File.Exists(nameOfoldDataflow))
                {
                    var parser = ExcelParserManager.GetParser(nameOfoldDataflow, null);
                    System.Diagnostics.Debug.WriteLine($"WARMUP: Cached OldDataflow parser for {variables?.Count ?? 0} variables");
                }
                
                // A2L WARMUP: Pre-warm A2LParserManager cache
                if (!string.IsNullOrEmpty(a2lFilePath) && File.Exists(a2lFilePath))
                {
                    var a2lParser = A2LParserManager.GetParser(a2lFilePath);
                    System.Diagnostics.Debug.WriteLine($"WARMUP: Cached A2L parser for {variables?.Count ?? 0} variables");
                }
                
                await Task.Delay(10); // Small delay to prevent blocking
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WARMUP WARNING: {ex.Message}");
            }
        }

        public void ResetBatchSearchResources()
        {
            // PERFORMANCE: Clear caches but keep managers intact
            System.Diagnostics.Debug.WriteLine("PERFORMANCE: Resetting batch search resources");
            
            // CLEANUP: Let managers handle their own cleanup
            A2LParserManager.CleanupStaleCache(); // Clean up A2L cache
            System.GC.Collect(); // Suggest garbage collection for memory optimization
        }

        /// <summary>
        /// PERFORMANCE: Batch search multiple variables using ExcelParserManager
        /// Returns comprehensive results for all variables across all files
        /// </summary>
        public async Task<Dictionary<string, object>> BatchSearchVariablesAsync(
            List<string> variables, 
            IProgress<int> progress = null)
        {
            var results = new Dictionary<string, object>();
            
            if (variables == null || !variables.Any())
                return results;
                
            try
            {
                // PERFORMANCE: Use ExcelParserManager for optimal caching
                progress?.Report(10);
                
                var tasks = new List<Task>();
                var semaphore = new SemaphoreSlim(Environment.ProcessorCount, Environment.ProcessorCount);
                
                for (int i = 0; i < variables.Count; i++)
                {
                    var variable = variables[i];
                    var index = i;
                    
                    tasks.Add(Task.Run(async () =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            // Search in all files using cached parsers
                            var searchResult = await SearchVariableAcrossFilesAsync(variable);
                            results[variable] = searchResult;
                            
                            // Update progress
                            var progressPercent = (int)((index + 1) / (double)variables.Count * 90) + 10;
                            progress?.Report(progressPercent);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));
                }
                
                await Task.WhenAll(tasks);
                progress?.Report(100);
                
                System.Diagnostics.Debug.WriteLine($"PERFORMANCE: Batch searched {variables.Count} variables");
                return results;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BATCH SEARCH ERROR: {ex.Message}");
                return results;
            }
        }

        /// <summary>
        /// PERFORMANCE: Search single variable across all files using cached parsers
        /// </summary>
        private async Task<object> SearchVariableAcrossFilesAsync(string variable)
        {
            await Task.Delay(1); // Make async for consistency
            
            var result = new
            {
                Variable = variable,
                NewCarasi = SearchInFile(variable, nameOfnewCarasi),
                OldCarasi = SearchInFile(variable, nameOfoldCarasi),
                NewDataflow = SearchInFile(variable, nameOfnewDataflow),
                OldDataflow = SearchInFile(variable, nameOfoldDataflow),
                A2L = SearchInA2L(variable, a2lFilePath), // UNIFIED SEARCH: Include A2L results
                SearchTime = DateTime.Now
            };
            
            return result;
        }
        
        /// <summary>
        /// PERFORMANCE: Search in single file using ExcelParserManager
        /// </summary>
        private object SearchInFile(string variable, string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                    return null;
                    
                // PERFORMANCE: Use cached parser from ExcelParserManager
                var parser = ExcelParserManager.GetParser(filePath, null);
                parser.search_Variable(variable);
                
                return new
                {
                    Found = parser.Interfaces?.Count > 0 || parser.Dictionary?.Count > 0,
                    InterfaceCount = parser.Interfaces?.Count ?? 0,
                    DictionaryCount = parser.Dictionary?.Count ?? 0
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SEARCH ERROR in {filePath}: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// A2L SEARCH: Search in A2L file using A2LParserManager
        /// </summary>
        private object SearchInA2L(string variable, string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                    return null;
                    
                // PERFORMANCE: Use cached parser from A2LParserManager
                var searchResult = A2LParserManager.FindVariable(filePath, variable);
                
                return new
                {
                    Found = searchResult.Found,
                    FoundInMeasurements = searchResult.FoundInMeasurements,
                    FoundInCharacteristics = searchResult.FoundInCharacteristics,
                    Summary = searchResult.GetSummary(),
                    MeasurementName = searchResult.Measurement?.Name,
                    CharacteristicName = searchResult.Characteristic?.Name
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"A2L SEARCH ERROR in {filePath}: {ex.Message}");
                return null;
            }
        }

        private Excel_Parser newCarasi;
        private Excel_Parser oldCarasi;
        private Excel_Parser newDF;
        private Excel_Parser oldDF;

        public UC_ContextClearing()
        {
            InitializeComponent();
            
            // LAYOUT FIX: Set proper panel proportions on initialization
            SetupPanelLayout();
        }
        
        /// <summary>
        /// LAYOUT: Configure 4-panel layout with proper proportions
        /// Top/Bottom: 40%/60%, Left/Right: 50%/50%
        /// </summary>
        private void SetupPanelLayout()
        {
            // Wait for control to be fully loaded
            this.Load += (sender, e) => ApplyLayoutProportions();
            this.Resize += (sender, e) => ApplyLayoutProportions();
        }
        
        private void ApplyLayoutProportions()
        {
            if (this.Width > 0 && this.Height > 0)
            {
                // Calculate proportions based on actual control size
                int totalHeight = this.Height - 60; // Account for top info panel
                int totalWidth = this.Width - 20; // Account for margins
                
                // 40% top, 60% bottom
                int topHeight = (int)(totalHeight * 0.40);
                
                // 50% left, 50% right
                int leftWidth = (int)(totalWidth * 0.50);
                
                // Apply to split containers
                if (splitContainer1.Height > 0)
                    splitContainer1.SplitterDistance = topHeight;
                    
                if (splitContainer2.Width > 0)
                    splitContainer2.SplitterDistance = leftWidth;
                    
                if (splitContainer3.Width > 0)
                    splitContainer3.SplitterDistance = leftWidth;
                    
                System.Diagnostics.Debug.WriteLine($"LAYOUT: Applied proportions - Top: {topHeight}px, Left: {leftWidth}px");
            }
        }

        private StringBuilder build_string(string[] array)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != null)
                {
                    sb.AppendFormat("{0}    {1}", i.ToString(), array[i]);
                    sb.AppendLine();
                    sb.AppendLine();
                }
            }
            return sb;
        }

        public void _setValueMM(bool isContain, string[] Lines)
        {
            tb_MM_Infor.Text = "";
            if (isContain)
            {
                tb_MM_Infor.Text = build_string(Lines).ToString();
                label1.BackColor = Color.Green;
            }
            else
            {
                tb_MM_Infor.Text = "Not Found in Extern ARXML! Please check again! ";
                label1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            }
        }

        public void _setValueA2L(bool isContain, string[] Lines)
        {
            tb_A2L_Infor.Text = "";
            if (isContain)
            {
                tb_A2L_Infor.Text = build_string(Lines).ToString();
            }
            else
            {
                tb_A2L_Infor.Text = "Not Found in A2L!!";
            }
        }


        private bool folder_verifying(string link)
        {
            bool isValid = false;
            
            // Reset validation flags
            isValidnewCarasi = false;
            isValidoldCarasi = false;
            isValidnewDataflow = false;
            isValidoldDataflow = false;
            
            if(System.IO.Directory.Exists(link))
            {
                string[] files = System.IO.Directory.GetFiles(link);
                foreach (var file in files)
                {
                    if (file.ToLower().Contains("newcarasi") && (file.ToLower().EndsWith(".xls") || file.ToLower().EndsWith(".xlsx")))
                    {
                        nameOfnewCarasi = file;
                        isValidnewCarasi = true;
                    }
                    if (file.ToLower().Contains("oldcarasi") && (file.ToLower().EndsWith(".xls") || file.ToLower().EndsWith(".xlsx")))
                    {
                        nameOfoldCarasi = file;
                        isValidoldCarasi = true;
                    }
                    if (file.ToLower().Contains("newdataflow") && (file.ToLower().EndsWith(".xls") || file.ToLower().EndsWith(".xlsx")))
                    {
                        nameOfnewDataflow = file;
                        isValidnewDataflow = true;
                    }
                    if (file.ToLower().Contains("olddataflow") && (file.ToLower().EndsWith(".xls") || file.ToLower().EndsWith(".xlsx")))
                    {
                        nameOfoldDataflow = file;
                        isValidoldDataflow = true;
                    }
                }
                
                // Return true if we have all required files
                isValid = isValidnewCarasi && isValidnewDataflow && isValidoldCarasi && isValidoldDataflow;
            }

            return isValid;
        }

        public void __checkVariable(ref ToolStripProgressBar toolStripProgressBar, string Interface2search)
        {
            if(newCarasi ==null && oldCarasi == null && newDF == null && oldDF == null)
            {
                MessageBox.Show("Something is wrong! Please contact Vuong Minh Ngoc to fix it!!");
            }
            else
            {
                int offset = 0;
                if (toolStripProgressBar.Value != 0) offset = toolStripProgressBar.Value;

                string new_variable = string.Empty;

                Cursor.Current = Cursors.WaitCursor;
                toolStripProgressBar.Value = 0 + offset;
                
                // CLEAR PREVIOUS HIGHLIGHTING: Reset colors before new search
                if (IsHighlightingEnabled)
                {
                    try
                    {
                        PropertyDifferenceHighlighter.ClearAllHighlighting(this);
                        System.Diagnostics.Debug.WriteLine("HIGHLIGHTING: Cleared previous color highlighting");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"CLEAR HIGHLIGHTING ERROR: {ex.Message}");
                    }
                }
                
                //Verify String Name
                if (folder_verifying(link2Folder))
                {
                    if (Interface2search != "")
                        new_variable = Interface2search;
                    else
                        MessageBox.Show("Please insert the Interface Name", "Warning!");


                    //Run searching Excel file
                    if (new_variable != string.Empty)
                    {
                        toolStripProgressBar.Value = 50 + offset;
                        //Create Excel Parsers
                        //-------------------------------CARASI-----------------------------------------------//
                        Doing_serching(newCarasi, UC_Newcarasi, new_variable, nameOfnewCarasi);
                        toolStripProgressBar.Value = 60 + offset;

                        Doing_serching(oldCarasi, UC_OldCarasi, new_variable, nameOfoldCarasi);
                        toolStripProgressBar.Value = 70 + offset;
                        //-------------------------------CARASI-----------------------------------------------//

                        //-------------------------------DATAFLOW---------------------------------------------//
                        Doing_serching(newDF, "New", new_variable, nameOfnewDataflow);
                        toolStripProgressBar.Value = 80 + offset;

                        Doing_serching(oldDF, "Old", new_variable, nameOfoldDataflow);
                        toolStripProgressBar.Value = 90 + offset;
                        //-------------------------------DATAFLOW---------------------------------------------//
                        
                        // HIGHLIGHT DIFFERENCES: Apply color comparison after all searches complete
                        if (IsHighlightingEnabled)
                        {
                            try
                            {
                                PropertyDifferenceHighlighter.HighlightAllPropertyDifferences(this, false);
                                System.Diagnostics.Debug.WriteLine($"HIGHLIGHTING: Applied color comparison for variable '{new_variable}'");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"HIGHLIGHTING ERROR: {ex.Message}");
                            }
                        }
                    }
                }
                toolStripProgressBar.Value = 100;
                Cursor.Current = Cursors.Default;
            }
        }

        //Doing Searching in DataFlow
        private void Doing_serching(Excel_Parser _Parser, string __NewOrOld, string new_variable, string nameOfFile)
        {
            string name = _Parser.Lb_NameOfFile;
            _Parser.search_Variable(new_variable);

            if (__NewOrOld == "New")
            {
                var a = _Parser.Dataview_df_Properties;
                UC_NewDF.setValue_UC(_Parser.Lb_NameOfFile, a);
                // AUTO-SELECT: setValue_UC now automatically selects first cell and shows row data
                System.Diagnostics.Debug.WriteLine($"DATAFLOW: NewDF loaded with auto-select for variable '{new_variable}'");
            }
            else
            {
                var a1 = _Parser.Dataview_df_Properties;
                UC_OldDF.setValue_UC(_Parser.Lb_NameOfFile, a1);
                // AUTO-SELECT: setValue_UC now automatically selects first cell and shows row data
                System.Diagnostics.Debug.WriteLine($"DATAFLOW: OldDF loaded with auto-select for variable '{new_variable}'");
            }
        }

        //Doing Searching in CARASI
        private void Doing_serching(Excel_Parser _Parser, UC_Carasi __uC, string new_variable, string nameOfFile)
        {
            _Parser.search_Variable(new_variable);
            __uC.setValue_UC(new_variable, _Parser.Interfaces, _Parser.Dictionary, gethint(_Parser), _Parser.Lb_NameOfFile);
        }


        private string gethint(Excel_Parser _Parser)
        {
            string __hint = "";
            DataView carasi_dictionary_dataview = _Parser.Dictionary;
            DataView carasi_Interface_dataview = _Parser.Interfaces;
            bool __execptionCase = false;

            for (int i = 0; i < carasi_Interface_dataview.Count; i++)
            {
                string funcName = string.Empty;
                if (carasi_Interface_dataview[i][1] != null)
                {
                    funcName = carasi_Interface_dataview[i][1].ToString().ToLower().Length > 7 ?
                               carasi_Interface_dataview[i][1].ToString().ToLower().Substring(0, 5) : carasi_Interface_dataview[i][1].ToString().ToLower();
                    switch (funcName)
                    {
                        case "gdgar": //gdgar
                            __hint = " .This is Diagnostic Interface! Should be type 4 or 6! Please check carefully!";
                            __execptionCase = true;
                            break;
                        case "stub_": // STUB
                            __hint += " .This is STUB mapping, should be type 10, check STUB Init also ! Please check carefully!";
                            __execptionCase = true;
                            break;
                        case "mm_ev": //mm_evcu
                            string __description = carasi_Interface_dataview[i][1] != null ? carasi_Interface_dataview[i][1].ToString().ToLower() : "";

                            if (__description != null && __description.ToLower().Contains("parameter exchanged on can network"))
                                __hint = " .This is Gateway signal to exchang data, Type 19 ! Please check carefully!";
                            else
                            {
                                string I_O = carasi_Interface_dataview[i][3] != null ? carasi_Interface_dataview[i][3].ToString().ToLower() : "";
                                if (I_O == "input")
                                    __hint = " .This is T3 to Autosar mapping, Type 12 ! Please check carefully!";
                                else if (I_O == "output")
                                    __hint = " .This is Autosar to T3 mapping, Type 12 ! Please check carefully!";
                                else if (I_O == "calib")
                                    __hint = ". This is a Calibration.";
                                else if (I_O == "local")
                                    __hint = ". This is a local signal.";
                                else
                                { }
                            }
                            __execptionCase = true;
                            break;

                        default:
                            break;
                    }
                }
            }
            __hint += !__execptionCase ? ".This is T3 only. type 19,21 or 8 if request from Function team! Please check carefully!" : "";

            for (int i = 0; i < carasi_dictionary_dataview.Count; i++)
            {
                if(carasi_dictionary_dataview[i][17] != null && carasi_dictionary_dataview[i][17].ToString().ToLower().Equals("real32"))
                {
                    __hint += ". Chosing Floating Point 32bit!";
                }
                if(carasi_dictionary_dataview[i][14] != null && carasi_dictionary_dataview[i][14].ToString().ToLower().Equals("boolean"))
                {
                    __hint += ". Boolean Type!";
                }
                if (carasi_dictionary_dataview[i][14] != null && carasi_dictionary_dataview[i][14].ToString().ToLower() != "boolean")
                {
                    if (carasi_dictionary_dataview[i][6] != null && carasi_dictionary_dataview[i][6].ToString().ToLower() != "-")
                        __hint += ". Maybe we need VERB compu!";
                }
            }

            return __hint;
        }

        /// <summary>
        /// A2L BATCH SEARCH: Batch search multiple variables in A2L file
        /// Returns comprehensive A2L results for all variables
        /// </summary>
        public async Task<Dictionary<string, object>> BatchSearchA2LVariablesAsync(
            List<string> variables, 
            IProgress<int> progress = null)
        {
            var results = new Dictionary<string, object>();
            
            if (variables == null || !variables.Any() || string.IsNullOrEmpty(a2lFilePath))
                return results;
                
            try
            {
                // PERFORMANCE: Use A2LParserManager for batch search
                progress?.Report(10);
                
                var a2lResults = A2LParserManager.BatchSearchVariables(a2lFilePath, variables);
                
                // Convert A2LSearchResult to display format
                foreach (var variable in variables)
                {
                    if (a2lResults.TryGetValue(variable, out A2LSearchResult searchResult))
                    {
                        results[variable] = new
                        {
                            Variable = variable,
                            Found = searchResult.Found,
                            FoundInMeasurements = searchResult.FoundInMeasurements,
                            FoundInCharacteristics = searchResult.FoundInCharacteristics,
                            Summary = searchResult.GetSummary(),
                            MeasurementName = searchResult.Measurement?.Name,
                            CharacteristicName = searchResult.Characteristic?.Name,
                            SearchTime = DateTime.Now
                        };
                    }
                    else
                    {
                        results[variable] = new
                        {
                            Variable = variable,
                            Found = false,
                            Summary = "Not Found in A2L!",
                            SearchTime = DateTime.Now
                        };
                    }
                }
                
                progress?.Report(100);
                System.Diagnostics.Debug.WriteLine($"A2L BATCH SEARCH: Searched {variables.Count} variables in A2L");
                return results;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"A2L BATCH SEARCH ERROR: {ex.Message}");
                return results;
            }
        }
        
        /// <summary>
        /// UNIFIED SEARCH DEMO: Demonstrate unified Excel + A2L search capabilities
        /// This method shows how to search across all data sources simultaneously
        /// </summary>
        public async Task<object> UnifiedSearchDemoAsync(string variable)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"UNIFIED SEARCH: Starting comprehensive search for '{variable}'");
                
                // EXCEL SEARCH: Search across all 4 Excel files
                var excelResults = await SearchVariableAcrossFilesAsync(variable);
                
                // A2L SEARCH: Search in A2L file if available
                object a2lResult = null;
                if (!string.IsNullOrEmpty(a2lFilePath))
                {
                    a2lResult = SearchInA2L(variable, a2lFilePath);
                }
                
                // UNIFIED RESULT: Combine all search results
                var unifiedResult = new
                {
                    SearchVariable = variable,
                    SearchTime = DateTime.Now,
                    ExcelResults = excelResults,
                    A2LResult = a2lResult,
                    
                    // SUMMARY: Quick overview
                    Summary = new
                    {
                        TotalSources = 5, // 4 Excel + 1 A2L
                        ExcelHits = CountExcelHits(excelResults),
                        A2LHit = a2lResult != null && ((dynamic)a2lResult).Found,
                        TotalHits = CountExcelHits(excelResults) + (a2lResult != null && ((dynamic)a2lResult).Found ? 1 : 0)
                    }
                };
                
                System.Diagnostics.Debug.WriteLine($"UNIFIED SEARCH: Found {unifiedResult.Summary.TotalHits}/{unifiedResult.Summary.TotalSources} matches for '{variable}'");
                return unifiedResult;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UNIFIED SEARCH ERROR: {ex.Message}");
                return new { Error = ex.Message, Variable = variable };
            }
        }
        
        /// <summary>
        /// HELPER: Count Excel search hits
        /// </summary>
        private int CountExcelHits(object excelResults)
        {
            try
            {
                if (excelResults == null) return 0;
                
                var results = (dynamic)excelResults;
                int hits = 0;
                
                if (results.NewCarasi != null && ((dynamic)results.NewCarasi).Found) hits++;
                if (results.OldCarasi != null && ((dynamic)results.OldCarasi).Found) hits++;
                if (results.NewDataflow != null && ((dynamic)results.NewDataflow).Found) hits++;
                if (results.OldDataflow != null && ((dynamic)results.OldDataflow).Found) hits++;
                
                return hits;
            }
            catch
            {
                return 0;
            }
        }

    }
}

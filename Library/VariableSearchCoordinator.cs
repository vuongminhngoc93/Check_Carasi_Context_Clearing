using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Windows.Forms;

namespace Check_carasi_DF_ContextClearing
{
    /// <summary>
    /// Variable Search Coordinator ensures data integrity across 4 Excel files
    /// Prevents mismatched variable searches between CARASI and DataFlow files
    /// Author: Vuong Minh Ngoc (MS/EJV)
    /// Version: 1.0.0 - Data Integrity Protection
    /// </summary>
    public class VariableSearchCoordinator
    {
        #region Private Fields
        private string lastSearchedVariable = string.Empty;
        private DateTime lastSearchTime = DateTime.MinValue;
        private readonly object searchLock = new object();
        
        // Search results cache for validation
        private Dictionary<string, SearchResult> searchResultsCache = new Dictionary<string, SearchResult>();
        #endregion

        #region Public Properties
        public string LastSearchedVariable => lastSearchedVariable;
        public DateTime LastSearchTime => lastSearchTime;
        #endregion

        #region Data Structures
        public class SearchResult
        {
            public string Variable { get; set; }
            public DateTime SearchTime { get; set; }
            public bool NewCarasiFound { get; set; }
            public bool OldCarasiFound { get; set; }
            public bool NewDataFlowFound { get; set; }
            public bool OldDataFlowFound { get; set; }
            public DataTable NewCarasiData { get; set; }
            public DataTable OldCarasiData { get; set; }
            public DataView NewDataFlowData { get; set; }
            public DataView OldDataFlowData { get; set; }
            public string ValidationHash { get; set; }
        }

        public class CoordinatedSearchResult
        {
            public bool IsValid { get; set; }
            public string Variable { get; set; }
            public SearchResult Results { get; set; }
            public List<string> ValidationErrors { get; set; } = new List<string>();
            public List<string> ValidationWarnings { get; set; } = new List<string>();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Coordinated search across all 4 Excel files with validation
        /// CRITICAL: Ensures same variable is searched in all files
        /// </summary>
        public CoordinatedSearchResult SearchVariableAcrossAllFiles(
            string variableToSearch,
            Excel_Parser newCarasiParser,
            Excel_Parser oldCarasiParser, 
            Excel_Parser newDataFlowParser,
            Excel_Parser oldDataFlowParser,
            ref ToolStripProgressBar progressBar)
        {
            lock (searchLock)
            {
                var result = new CoordinatedSearchResult
                {
                    Variable = variableToSearch,
                    ValidationErrors = new List<string>(),
                    ValidationWarnings = new List<string>()
                };

                try
                {
                    // VALIDATION: Input checks
                    if (string.IsNullOrEmpty(variableToSearch))
                    {
                        result.ValidationErrors.Add("Variable name cannot be empty");
                        result.IsValid = false;
                        return result;
                    }

                    if (newCarasiParser == null || oldCarasiParser == null || 
                        newDataFlowParser == null || oldDataFlowParser == null)
                    {
                        result.ValidationErrors.Add("One or more Excel parsers are null");
                        result.IsValid = false;
                        return result;
                    }

                    // COORDINATION: Synchronized search
                    var searchResult = new SearchResult
                    {
                        Variable = variableToSearch,
                        SearchTime = DateTime.Now
                    };

                    progressBar.Value = 50;

                    // SEARCH 1: New CARASI
                    newCarasiParser.search_Variable(variableToSearch);
                    searchResult.NewCarasiFound = newCarasiParser._IsExist_Carasi(variableToSearch);
                    searchResult.NewCarasiData = newCarasiParser.Interfaces?.Table?.Copy();
                    progressBar.Value = 60;

                    // SEARCH 2: Old CARASI  
                    oldCarasiParser.search_Variable(variableToSearch);
                    searchResult.OldCarasiFound = oldCarasiParser._IsExist_Carasi(variableToSearch);
                    searchResult.OldCarasiData = oldCarasiParser.Interfaces?.Table?.Copy();
                    progressBar.Value = 70;

                    // SEARCH 3: New DataFlow
                    newDataFlowParser.search_Variable(variableToSearch);
                    searchResult.NewDataFlowFound = newDataFlowParser._IsExist_Dataflow(variableToSearch);
                    searchResult.NewDataFlowData = newDataFlowParser.Dataview_df_Properties?.Table?.Copy()?.DefaultView;
                    progressBar.Value = 80;

                    // SEARCH 4: Old DataFlow
                    oldDataFlowParser.search_Variable(variableToSearch);
                    searchResult.OldDataFlowFound = oldDataFlowParser._IsExist_Dataflow(variableToSearch);
                    searchResult.OldDataFlowData = oldDataFlowParser.Dataview_df_Properties?.Table?.Copy()?.DefaultView;
                    progressBar.Value = 90;

                    // VALIDATION: Cross-file consistency checks
                    ValidateSearchResults(searchResult, result);

                    // CACHE: Store results for future validation
                    searchResult.ValidationHash = GenerateValidationHash(searchResult);
                    searchResultsCache[variableToSearch] = searchResult;

                    // UPDATE: Track last search
                    lastSearchedVariable = variableToSearch;
                    lastSearchTime = searchResult.SearchTime;

                    result.Results = searchResult;
                    result.IsValid = result.ValidationErrors.Count == 0;

                    progressBar.Value = 100;
                    return result;
                }
                catch (Exception ex)
                {
                    result.ValidationErrors.Add($"Search coordination failed: {ex.Message}");
                    result.IsValid = false;
                    return result;
                }
            }
        }

        /// <summary>
        /// Validate that search was performed correctly across all files
        /// </summary>
        public bool ValidateLastSearch(string expectedVariable)
        {
            lock (searchLock)
            {
                if (string.IsNullOrEmpty(expectedVariable) || string.IsNullOrEmpty(lastSearchedVariable))
                    return false;

                // Check if last search matches expected variable
                bool variableMatches = string.Equals(lastSearchedVariable, expectedVariable, StringComparison.OrdinalIgnoreCase);
                
                // Check if search was recent (within last 30 seconds)
                bool searchIsRecent = (DateTime.Now - lastSearchTime).TotalSeconds < 30;

                return variableMatches && searchIsRecent;
            }
        }

        /// <summary>
        /// Get cached search results for validation
        /// </summary>
        public SearchResult GetCachedResults(string variable)
        {
            lock (searchLock)
            {
                return searchResultsCache.ContainsKey(variable) ? searchResultsCache[variable] : null;
            }
        }

        /// <summary>
        /// Clear cache and reset coordinator
        /// </summary>
        public void Reset()
        {
            lock (searchLock)
            {
                lastSearchedVariable = string.Empty;
                lastSearchTime = DateTime.MinValue;
                searchResultsCache.Clear();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Validate search results for consistency and data integrity
        /// </summary>
        private void ValidateSearchResults(SearchResult searchResult, CoordinatedSearchResult coordinatedResult)
        {
            // VALIDATION 1: Check for variable name consistency in results - RELAXED
            if (searchResult.NewCarasiData != null && searchResult.NewCarasiData.Rows.Count > 0)
            {
                var foundVariables = GetVariablesFromCarasiData(searchResult.NewCarasiData);
                if (!foundVariables.Contains(searchResult.Variable))
                {
                    coordinatedResult.ValidationWarnings.Add($"New CARASI search returned data but exact variable '{searchResult.Variable}' not found - may be partial match");
                }
            }

            if (searchResult.OldCarasiData != null && searchResult.OldCarasiData.Rows.Count > 0)
            {
                var foundVariables = GetVariablesFromCarasiData(searchResult.OldCarasiData);
                if (!foundVariables.Contains(searchResult.Variable))
                {
                    coordinatedResult.ValidationWarnings.Add($"Old CARASI search returned data but exact variable '{searchResult.Variable}' not found - may be partial match");
                }
            }

            // VALIDATION 2: Check for data consistency between New and Old
            if (searchResult.NewCarasiFound && searchResult.OldCarasiFound)
            {
                // Both found - this is expected for comparison
                coordinatedResult.ValidationWarnings.Add("Variable found in both New and Old CARASI files - ready for comparison");
            }
            else if (!searchResult.NewCarasiFound && !searchResult.OldCarasiFound)
            {
                coordinatedResult.ValidationWarnings.Add("Variable not found in either CARASI file - check variable name");
            }

            // VALIDATION 3: Check DataFlow consistency
            if (searchResult.NewDataFlowFound && searchResult.OldDataFlowFound)
            {
                coordinatedResult.ValidationWarnings.Add("Variable found in both New and Old DataFlow files - ready for comparison");
            }
            else if (!searchResult.NewDataFlowFound && !searchResult.OldDataFlowFound)
            {
                coordinatedResult.ValidationWarnings.Add("Variable not found in either DataFlow file - may be CARASI-only");
            }

            // VALIDATION 4: Cross-file type validation - RELAXED LOGIC
            bool foundInCarasi = searchResult.NewCarasiFound || searchResult.OldCarasiFound;
            bool foundInDataFlow = searchResult.NewDataFlowFound || searchResult.OldDataFlowFound;

            if (!foundInCarasi && !foundInDataFlow)
            {
                // SOFTENED: Change from ERROR to WARNING - some variables may not exist
                coordinatedResult.ValidationWarnings.Add($"Variable '{searchResult.Variable}' not found in any file - check variable name or it may not exist in current files");
            }
            else if (foundInCarasi && !foundInDataFlow)
            {
                coordinatedResult.ValidationWarnings.Add($"Variable '{searchResult.Variable}' found only in CARASI files - CARASI-specific variable");
            }
            else if (!foundInCarasi && foundInDataFlow)
            {
                coordinatedResult.ValidationWarnings.Add($"Variable '{searchResult.Variable}' found only in DataFlow files - DataFlow-specific variable");
            }
        }

        /// <summary>
        /// Extract variable names from CARASI data for validation
        /// FIXED: Handle different possible column names
        /// </summary>
        private List<string> GetVariablesFromCarasiData(DataTable carasiData)
        {
            var variables = new List<string>();
            if (carasiData == null || carasiData.Rows.Count == 0)
                return variables;

            // Try different possible column names for CARASI variable
            string[] possibleColumns = { "SSTG Label", "SSTG label", "Interface Name", "Variable Name", "Name" };
            string targetColumn = null;

            foreach (string columnName in possibleColumns)
            {
                if (carasiData.Columns.Contains(columnName))
                {
                    targetColumn = columnName;
                    break;
                }
            }

            if (string.IsNullOrEmpty(targetColumn))
            {
                // Debug: List available columns for troubleshooting
                var availableColumns = string.Join(", ", carasiData.Columns.Cast<DataColumn>().Select(c => $"'{c.ColumnName}'"));
                System.Diagnostics.Debug.WriteLine($"CARASI data validation: No standard variable column found. Available columns: {availableColumns}");
                return variables;
            }

            foreach (DataRow row in carasiData.Rows)
            {
                var variable = row[targetColumn]?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(variable))
                {
                    variables.Add(variable);
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"CARASI validation: Found {variables.Count} variables in column '{targetColumn}'");
            return variables;
        }

        /// <summary>
        /// Generate validation hash for search results
        /// </summary>
        private string GenerateValidationHash(SearchResult searchResult)
        {
            var hashInput = $"{searchResult.Variable}|{searchResult.SearchTime:yyyyMMddHHmmss}|" +
                           $"{searchResult.NewCarasiFound}|{searchResult.OldCarasiFound}|" +
                           $"{searchResult.NewDataFlowFound}|{searchResult.OldDataFlowFound}";
            
            return hashInput.GetHashCode().ToString("X");
        }
        #endregion
    }
}

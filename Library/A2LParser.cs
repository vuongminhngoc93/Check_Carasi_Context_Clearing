using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Check_carasi_DF_ContextClearing
{
    /// <summary>
    /// HIGH-PERFORMANCE A2L Parser with optimized parsing for large files
    /// Supports MEASUREMENT, CHARACTERISTIC, COMPU_METHOD, and RECORD_LAYOUT sections
    /// </summary>
    public class A2LParser
    {
        // CORE PROPERTIES
        public string FilePath { get; private set; }
        public string ProjectName { get; private set; }
        public string ModuleName { get; private set; }
        public bool IsValid { get; private set; }
        public DateTime LastModified { get; private set; }
        public TimeSpan ParseTime { get; private set; }
        
        // PARSED DATA COLLECTIONS
        public Dictionary<string, A2LMeasurement> Measurements { get; private set; }
        public Dictionary<string, A2LCharacteristic> Characteristics { get; private set; }
        public Dictionary<string, A2LCompuMethod> CompuMethods { get; private set; }
        public Dictionary<string, A2LRecordLayout> RecordLayouts { get; private set; }
        
        // STATISTICS
        public int TotalMeasurements => Measurements?.Count ?? 0;
        public int TotalCharacteristics => Characteristics?.Count ?? 0;
        public int TotalCompuMethods => CompuMethods?.Count ?? 0;
        public int TotalRecordLayouts => RecordLayouts?.Count ?? 0;
        public bool ParsedSuccessfully => IsValid && (TotalMeasurements > 0 || TotalCharacteristics > 0);
        
        // CONSTRUCTOR
        public A2LParser(string filePath)
        {
            FilePath = filePath;
            Measurements = new Dictionary<string, A2LMeasurement>(StringComparer.OrdinalIgnoreCase);
            Characteristics = new Dictionary<string, A2LCharacteristic>(StringComparer.OrdinalIgnoreCase);
            CompuMethods = new Dictionary<string, A2LCompuMethod>(StringComparer.OrdinalIgnoreCase);
            RecordLayouts = new Dictionary<string, A2LRecordLayout>(StringComparer.OrdinalIgnoreCase);
            
            ParseFile();
        }
        
        // MAIN PARSING METHOD
        private void ParseFile()
        {
            try
            {
                var startTime = DateTime.Now;
                
                if (!File.Exists(FilePath))
                {
                    IsValid = false;
                    return;
                }
                
                LastModified = File.GetLastWriteTime(FilePath);
                
                // OPTIMIZATION: Read file in chunks for large A2L files
                using (var reader = new StreamReader(FilePath, Encoding.UTF8, true, bufferSize: 65536))
                {
                    ParseA2LContent(reader);
                }
                
                IsValid = true;
                ParseTime = DateTime.Now - startTime;
                
                System.Diagnostics.Debug.WriteLine($"A2L PARSING: {Path.GetFileName(FilePath)}");
                System.Diagnostics.Debug.WriteLine($"  Parse Time: {ParseTime.TotalMilliseconds:F0}ms");
                System.Diagnostics.Debug.WriteLine($"  Measurements: {TotalMeasurements:N0}");
                System.Diagnostics.Debug.WriteLine($"  Characteristics: {TotalCharacteristics:N0}");
            }
            catch (Exception ex)
            {
                IsValid = false;
                System.Diagnostics.Debug.WriteLine($"A2L PARSE ERROR: {ex.Message}");
            }
        }
        
        /// <summary>
        /// PERFORMANCE: Parse A2L content with optimized regex patterns
        /// Fixed to handle variable names on separate lines after /begin blocks
        /// </summary>
        private void ParseA2LContent(StreamReader reader)
        {
            string line;
            var currentBlock = A2LBlockType.None;
            var blockContent = new StringBuilder();
            string blockName = string.Empty;
            bool expectingVariableName = false;
            
            // REGEX PATTERNS: Pre-compiled for performance
            var beginPattern = new Regex(@"^\s*/begin\s+(\w+)\s*(.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var endPattern = new Regex(@"^\s*/end\s+(\w+)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var projectPattern = new Regex(@"^\s*/begin\s+PROJECT\s+(\w+)\s+""([^""]*)""\s*$", RegexOptions.Compiled);
            var modulePattern = new Regex(@"^\s*/begin\s+MODULE\s+(\w+)\s*""([^""]*)""\s*$", RegexOptions.Compiled);
            
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                
                // Skip empty lines and comments
                if (string.IsNullOrEmpty(line) || line.StartsWith("/*"))
                    continue;
                
                // PARSE PROJECT INFO
                var projectMatch = projectPattern.Match(line);
                if (projectMatch.Success)
                {
                    ProjectName = projectMatch.Groups[2].Value;
                    continue;
                }
                
                // PARSE MODULE INFO
                var moduleMatch = modulePattern.Match(line);
                if (moduleMatch.Success)
                {
                    ModuleName = moduleMatch.Groups[2].Value;
                    continue;
                }
                
                // PARSE BLOCK STRUCTURE
                var beginMatch = beginPattern.Match(line);
                if (beginMatch.Success)
                {
                    var blockType = beginMatch.Groups[1].Value.ToUpper();
                    
                    if (blockType == "MEASUREMENT")
                    {
                        currentBlock = A2LBlockType.Measurement;
                        blockName = ""; // Name will be on next line
                        blockContent.Clear();
                        expectingVariableName = true;
                    }
                    else if (blockType == "CHARACTERISTIC")
                    {
                        currentBlock = A2LBlockType.Characteristic;
                        blockName = ""; // Name will be on next line
                        blockContent.Clear();
                        expectingVariableName = true;
                    }
                    else if (blockType == "COMPU_METHOD")
                    {
                        currentBlock = A2LBlockType.CompuMethod;
                        blockName = beginMatch.Groups[2].Value.Split(' ').FirstOrDefault()?.Trim() ?? "";
                        blockContent.Clear();
                    }
                    else if (blockType == "RECORD_LAYOUT")
                    {
                        currentBlock = A2LBlockType.RecordLayout;
                        blockName = beginMatch.Groups[2].Value.Split(' ').FirstOrDefault()?.Trim() ?? "";
                        blockContent.Clear();
                    }
                    else
                    {
                        currentBlock = A2LBlockType.None;
                    }
                    
                    continue;
                }
                
                // CAPTURE VARIABLE NAME: First non-empty line after /begin MEASUREMENT or /begin CHARACTERISTIC
                if (expectingVariableName && (currentBlock == A2LBlockType.Measurement || currentBlock == A2LBlockType.Characteristic))
                {
                    // Extract variable name (should be first word on the line)
                    blockName = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim() ?? "";
                    expectingVariableName = false;
                    blockContent.AppendLine(line);
                    continue;
                }
                
                // HANDLE BLOCK END
                var endMatch = endPattern.Match(line);
                if (endMatch.Success)
                {
                    var blockType = endMatch.Groups[1].Value.ToUpper();
                    
                    if (currentBlock == A2LBlockType.Measurement && blockType == "MEASUREMENT")
                    {
                        var measurement = ParseMeasurement(blockName, blockContent.ToString());
                        if (measurement != null && !string.IsNullOrEmpty(measurement.Name))
                        {
                            Measurements[measurement.Name] = measurement;
                        }
                    }
                    else if (currentBlock == A2LBlockType.Characteristic && blockType == "CHARACTERISTIC")
                    {
                        var characteristic = ParseCharacteristic(blockName, blockContent.ToString());
                        if (characteristic != null && !string.IsNullOrEmpty(characteristic.Name))
                        {
                            Characteristics[characteristic.Name] = characteristic;
                        }
                    }
                    else if (currentBlock == A2LBlockType.CompuMethod && blockType == "COMPU_METHOD")
                    {
                        var compuMethod = ParseCompuMethod(blockName, blockContent.ToString());
                        if (compuMethod != null && !string.IsNullOrEmpty(compuMethod.Name))
                        {
                            CompuMethods[compuMethod.Name] = compuMethod;
                        }
                    }
                    else if (currentBlock == A2LBlockType.RecordLayout && blockType == "RECORD_LAYOUT")
                    {
                        var recordLayout = ParseRecordLayout(blockName, blockContent.ToString());
                        if (recordLayout != null && !string.IsNullOrEmpty(recordLayout.Name))
                        {
                            RecordLayouts[recordLayout.Name] = recordLayout;
                        }
                    }
                    
                    // RESET STATE
                    currentBlock = A2LBlockType.None;
                    blockContent.Clear();
                    blockName = string.Empty;
                    expectingVariableName = false;
                    continue;
                }
                
                // COLLECT BLOCK CONTENT
                if (currentBlock != A2LBlockType.None)
                {
                    blockContent.AppendLine(line);
                }
            }
        }
        
        /// <summary>
        /// PARSING: Extract MEASUREMENT block data
        /// </summary>
        private A2LMeasurement ParseMeasurement(string name, string content)
        {
            try
            {
                var lines = content.Split('\n').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToArray();
                
                if (lines.Length < 3)
                    return null;
                
                var measurement = new A2LMeasurement
                {
                    Name = name,
                    Description = ExtractQuotedString(content),
                    DataType = ExtractDataType(content),
                    Address = ExtractAddress(content),
                    RecordLayout = ExtractRecordLayout(content),
                    MinValue = ExtractMinValue(content),
                    MaxValue = ExtractMaxValue(content)
                };
                
                return measurement;
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// PARSING: Extract CHARACTERISTIC block data
        /// </summary>
        private A2LCharacteristic ParseCharacteristic(string name, string content)
        {
            try
            {
                var lines = content.Split('\n').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToArray();
                
                if (lines.Length < 3)
                    return null;
                
                var characteristic = new A2LCharacteristic
                {
                    Name = name,
                    Description = ExtractQuotedString(content),
                    Type = ExtractCharacteristicType(content),
                    Address = ExtractAddress(content),
                    RecordLayout = ExtractRecordLayout(content),
                    MinValue = ExtractMinValue(content),
                    MaxValue = ExtractMaxValue(content)
                };
                
                return characteristic;
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// PARSING: Extract COMPU_METHOD block data
        /// </summary>
        private A2LCompuMethod ParseCompuMethod(string name, string content)
        {
            try
            {
                return new A2LCompuMethod
                {
                    Name = name,
                    Description = ExtractQuotedString(content),
                    Type = ExtractCompuMethodType(content),
                    Unit = ExtractUnit(content)
                };
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// PARSING: Extract RECORD_LAYOUT block data  
        /// </summary>
        private A2LRecordLayout ParseRecordLayout(string name, string content)
        {
            try
            {
                return new A2LRecordLayout
                {
                    Name = name,
                    DataSize = ExtractDataSize(content),
                    NoAxisPts = ExtractNoAxisPts(content)
                };
            }
            catch
            {
                return null;
            }
        }
        
        // UTILITY METHODS FOR EXTRACTION
        private string ExtractQuotedString(string content)
        {
            var match = Regex.Match(content, @"""([^""]*)""");
            return match.Success ? match.Groups[1].Value : "";
        }
        
        private string ExtractDataType(string content)
        {
            var lines = content.Split('\n').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToArray();
            return lines.Length > 2 ? lines[2] : "";
        }
        
        private string ExtractCharacteristicType(string content)
        {
            var lines = content.Split('\n').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToArray();
            return lines.Length > 2 ? lines[2] : "";
        }
        
        private string ExtractCompuMethodType(string content)
        {
            var lines = content.Split('\n').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToArray();
            return lines.Length > 2 ? lines[2] : "";
        }
        
        private string ExtractAddress(string content)
        {
            var match = Regex.Match(content, @"0x[0-9A-Fa-f]+");
            return match.Success ? match.Value : "";
        }
        
        private string ExtractRecordLayout(string content)
        {
            var lines = content.Split('\n').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToArray();
            return lines.Length > 4 ? lines[4] : "";
        }
        
        private double ExtractMinValue(string content)
        {
            var lines = content.Split('\n').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToArray();
            if (lines.Length > 5 && double.TryParse(lines[5], out double value))
                return value;
            return 0.0;
        }
        
        private double ExtractMaxValue(string content)
        {
            var lines = content.Split('\n').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToArray();
            if (lines.Length > 6 && double.TryParse(lines[6], out double value))
                return value;
            return 0.0;
        }
        
        private string ExtractUnit(string content)
        {
            var match = Regex.Match(content, @"UNIT\s+""([^""]*)""");
            return match.Success ? match.Groups[1].Value : "";
        }
        
        private int ExtractDataSize(string content)
        {
            var match = Regex.Match(content, @"DATA_SIZE\s+(\d+)");
            return match.Success && int.TryParse(match.Groups[1].Value, out int size) ? size : 0;
        }
        
        private int ExtractNoAxisPts(string content)
        {
            var match = Regex.Match(content, @"NO_AXIS_PTS_(\w+)\s+(\d+)");
            return match.Success && int.TryParse(match.Groups[2].Value, out int pts) ? pts : 0;
        }
        
        // SEARCH METHODS
        public A2LSearchResult FindVariable(string variableName)
        {
            var result = new A2LSearchResult { VariableName = variableName };
            
            // SEARCH MEASUREMENTS
            if (Measurements.TryGetValue(variableName, out A2LMeasurement measurement))
            {
                result.FoundInMeasurements = true;
                result.Measurement = measurement;
            }
            
            // SEARCH CHARACTERISTICS  
            if (Characteristics.TryGetValue(variableName, out A2LCharacteristic characteristic))
            {
                result.FoundInCharacteristics = true;
                result.Characteristic = characteristic;
            }
            
            result.Found = result.FoundInMeasurements || result.FoundInCharacteristics;
            return result;
        }
        
        public Dictionary<string, A2LSearchResult> FindVariables(List<string> variableNames)
        {
            var results = new Dictionary<string, A2LSearchResult>();
            
            foreach (var variableName in variableNames)
            {
                var result = FindVariable(variableName);
                results[variableName] = result;
            }
            
            return results;
        }
        
        public List<string> FindVariablesByPattern(string pattern)
        {
            var matches = new List<string>();
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            
            matches.AddRange(Measurements.Keys.Where(name => regex.IsMatch(name)));
            matches.AddRange(Characteristics.Keys.Where(name => regex.IsMatch(name)));
            
            return matches.Distinct().ToList();
        }
    }
    
    // ENUMS AND CLASSES
    public enum A2LBlockType
    {
        None,
        Measurement,
        Characteristic,
        CompuMethod,
        RecordLayout
    }
    
    public class A2LMeasurement
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; }
        public string Address { get; set; }
        public string RecordLayout { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
    }
    
    public class A2LCharacteristic
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Address { get; set; }
        public string RecordLayout { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
    }
    
    public class A2LCompuMethod
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Unit { get; set; }
    }
    
    public class A2LRecordLayout
    {
        public string Name { get; set; }
        public int DataSize { get; set; }
        public int NoAxisPts { get; set; }
    }
    
    public class A2LSearchResult
    {
        public string VariableName { get; set; }
        public bool Found { get; set; }
        public bool FoundInMeasurements { get; set; }
        public bool FoundInCharacteristics { get; set; }
        public A2LMeasurement Measurement { get; set; }
        public A2LCharacteristic Characteristic { get; set; }
        
        public string GetSummary()
        {
            if (!Found)
                return "Not Found in A2L!";
                
            var summary = new StringBuilder();
            
            if (FoundInMeasurements && Measurement != null)
            {
                summary.AppendLine($"MEASUREMENT: {Measurement.Name}");
                summary.AppendLine($"  Description: {Measurement.Description}");
                summary.AppendLine($"  DataType: {Measurement.DataType}");
                summary.AppendLine($"  Address: {Measurement.Address}");
                summary.AppendLine($"  Range: {Measurement.MinValue} - {Measurement.MaxValue}");
            }
            
            if (FoundInCharacteristics && Characteristic != null)
            {
                summary.AppendLine($"CHARACTERISTIC: {Characteristic.Name}");
                summary.AppendLine($"  Description: {Characteristic.Description}");
                summary.AppendLine($"  Type: {Characteristic.Type}");
                summary.AppendLine($"  Address: {Characteristic.Address}");
                summary.AppendLine($"  Range: {Characteristic.MinValue} - {Characteristic.MaxValue}");
            }
            
            return summary.ToString();
        }
    }
}

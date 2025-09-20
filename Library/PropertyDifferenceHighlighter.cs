using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Check_carasi_DF_ContextClearing
{
    /// <summary>
    /// PERFORMANCE-OPTIMIZED Property Difference Highlighting System
    /// Provides visual comparison between Old vs New Carasi and Dataflow properties
    /// Features: Color-coded highlighting, batch operations, efficient algorithms
    /// </summary>
    public static class PropertyDifferenceHighlighter
    {
        #region Color Scheme Constants
        
        /// <summary>
        /// Professional color scheme for property highlighting - BOLD COLORS for better visibility
        /// RED: Different values, GREEN: Same values, WHITE: Default
        /// </summary>
        private static readonly Color DifferentColor = Color.FromArgb(255, 200, 200); // Darker red for visibility
        private static readonly Color SameColor = Color.FromArgb(200, 255, 200);      // Darker green for visibility
        private static readonly Color DefaultColor = Color.White;                     // Default white
        
        #endregion

        #region Property Mappings - Updated for Real Control Names
        
        /// <summary>
        /// CARASI MAPPING: Maps Old Carasi textboxes to New Carasi textboxes
        /// Based on ACTUAL UC_Carasi Designer control names
        /// </summary>
        private static readonly Dictionary<string, string> CarasiPropertyMappings = new Dictionary<string, string>
        {
            // REAL Carasi Properties from UC_Carasi.Designer.cs
            { "tb_Type", "tb_Type" },                           // Data type
            { "tb_Unit", "tb_Unit" },                           // Unit
            { "tb_Min", "tb_Min" },                             // Minimum value
            { "tb_Max", "tb_Max" },                             // Maximum value
            { "tb_Res", "tb_Res" },                             // Resolution
            { "tb_Init", "tb_Init" },                           // Initial value
            { "tb_stub_InitValue", "tb_stub_InitValue" },       // Stub Init Value
            { "tb_Description", "tb_Description" },             // Description
            { "tb_Conversion", "tb_Conversion" },               // Conversion
            { "tb_TypeMM", "tb_TypeMM" },                       // Type MM
            // RichTextBox controls for Input/Output/CM changes
            { "richTextBox_Input", "richTextBox_Input" },       // Input connections
            { "richTextBox_Ouput", "richTextBox_Ouput" },       // Output connections (note typo in original)
            { "rtb_computeDetails", "rtb_computeDetails" }      // CM Details (Compute Details)
        };

        /// <summary>
        /// DATAFLOW MAPPING: Maps Old Dataflow textboxes to New Dataflow textboxes
        /// Based on ACTUAL UC_dataflow Designer control names - supports detail textboxes
        /// </summary>
        private static readonly Dictionary<string, string> DataflowPropertyMappings = new Dictionary<string, string>
        {
            // PSA Properties (left side)
            { "tb_PSAunit", "tb_PSAunit" },                     // PSA Unit
            { "tb_PSAMin", "tb_PSAMin" },                       // PSA Min
            { "tb_PSAMax", "tb_PSAMax" },                       // PSA Max
            { "tb_PSARes", "tb_PSARes" },                       // PSA Resolution
            { "tb_PSAInit", "tb_PSAInit" },                     // PSA Init
            { "tb_PSAType", "tb_PSAType" },                     // PSA Type
            { "tb_PSAOffset", "tb_PSAOffset" },                 // PSA Offset
            { "tb_PSAConversion", "tb_PSAConversion" },         // PSA Conversion
            { "tb_RTE_direction", "tb_RTE_direction" },         // RTE Direction
            { "tb_Description", "tb_Description" },             // Description
            
            // RB Properties (right side) 
            { "tb_RB_Unit", "tb_RB_Unit" },                     // RB Unit
            { "tb_RB_Min", "tb_RB_Min" },                       // RB Min
            { "tb_RB_Max", "tb_RB_Max" },                       // RB Max
            { "tb_RB_Res", "tb_RB_Res" },                       // RB Resolution
            { "tb_RB_init", "tb_RB_init" },                     // RB Init
            { "tb_RB_type", "tb_RB_type" },                     // RB Type
            { "tb_RB_offset", "tb_RB_offset" },                 // RB Offset
            { "tb_RB_conversion", "tb_RB_conversion" },         // RB Conversion
            { "tb_RB_PseudoCode", "tb_RB_PseudoCode" },         // RB PseudoCode
            { "tb_RB_MappingType", "tb_RB_MappingType" }        // RB MappingType
        };

        #endregion

        #region Main Highlighting Methods

        /// <summary>
        /// PERFORMANCE: Main highlighting method - compares and highlights all property differences
        /// Uses optimized batch operations to avoid UI flickering
        /// </summary>
        public static void HighlightAllPropertyDifferences(UC_ContextClearing contextClearing, bool showTooltips = false)
        {
            try
            {
                // BATCH OPERATION: Suspend UI updates during highlighting for better performance
                SuspendUIUpdates(contextClearing);

                // CARASI COMPARISON: Old vs New Carasi properties
                HighlightCarasiDifferences(contextClearing.OldCarasiControl, contextClearing.NewCarasiControl, showTooltips);

                // DATAFLOW COMPARISON: Old vs New Dataflow properties  
                HighlightDataflowDifferences(contextClearing.OldDataflowControl, contextClearing.NewDataflowControl, showTooltips);

                // PERFORMANCE: Resume UI updates after all highlighting is complete
                ResumeUIUpdates(contextClearing);

                System.Diagnostics.Debug.WriteLine($"Property highlighting completed successfully - Carasi: {CarasiPropertyMappings.Count} properties, Dataflow: {DataflowPropertyMappings.Count} properties");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in HighlightAllPropertyDifferences: {ex.Message}");
                ResumeUIUpdates(contextClearing); // Ensure UI updates are resumed even on error
            }
        }

        /// <summary>
        /// CARASI HIGHLIGHTING: Compare and highlight differences between Old and New Carasi controls
        /// Supports both TextBox and RichTextBox controls for Input/Output changes
        /// </summary>
        private static void HighlightCarasiDifferences(UC_Carasi oldCarasi, UC_Carasi newCarasi, bool showTooltips = false)
        {
            try
            {
                foreach (var mapping in CarasiPropertyMappings)
                {
                    string oldControlName = mapping.Key;
                    string newControlName = mapping.Value;

                    // Find controls in both UC_Carasi (TextBox or RichTextBox)
                    Control oldControl = FindControlByName(oldCarasi, oldControlName);
                    Control newControl = FindControlByName(newCarasi, newControlName);

                    if (oldControl != null && newControl != null)
                    {
                        // Get text from either TextBox or RichTextBox
                        string oldText = GetControlText(oldControl);
                        string newText = GetControlText(newControl);

                        // Compare values and apply highlighting
                        bool areEqual = string.Equals(oldText?.Trim(), newText?.Trim(), StringComparison.OrdinalIgnoreCase);
                        
                        Color highlightColor = areEqual ? SameColor : DifferentColor;
                        
                        // Apply color to both controls
                        oldControl.BackColor = highlightColor;
                        newControl.BackColor = highlightColor;

                        // DEBUG: Log highlighting for debugging
                        if (showTooltips)
                        {
                            System.Diagnostics.Debug.WriteLine($"Carasi {oldControlName}: {(areEqual ? "MATCH" : "DIFFERENT")} - Old:'{oldText}' vs New:'{newText}'");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error highlighting Carasi differences: {ex.Message}");
            }
        }

        /// <summary>
        /// DATAFLOW HIGHLIGHTING: Compare and highlight differences between Old and New Dataflow controls
        /// Compares actual DataGridView selected row data instead of textbox display values
        /// </summary>
        private static void HighlightDataflowDifferences(UC_dataflow oldDataflow, UC_dataflow newDataflow, bool showTooltips = false)
        {
            try
            {
                // Get selected row data from both DataGridViews
                var oldRowData = GetDataflowSelectedRowData(oldDataflow);
                var newRowData = GetDataflowSelectedRowData(newDataflow);
                
                if (oldRowData == null || newRowData == null)
                {
                    System.Diagnostics.Debug.WriteLine("Cannot compare dataflow - no row selected in one or both dataflows");
                    return;
                }

                // Map DataGridView column indices to TextBox properties for comparison
                var columnMappings = new Dictionary<string, int>
                {
                    { "tb_Description", 2 },        // Description column
                    { "tb_RTE_direction", 15 },     // RTE Direction column
                    { "tb_RB_MappingType", 29 },    // RB MappingType column  
                    { "tb_RB_PseudoCode", 31 },     // RB PseudoCode column
                    { "tb_PSAType", 5 },            // PSA Type column
                    { "tb_PSAunit", 6 },            // PSA Unit column
                    { "tb_PSAConversion", 7 },      // PSA Conversion column
                    { "tb_PSARes", 8 },             // PSA Resolution column
                    { "tb_PSAMin", 9 },             // PSA Min column
                    { "tb_PSAMax", 10 },            // PSA Max column
                    { "tb_PSAOffset", 11 },         // PSA Offset column
                    { "tb_PSAInit", 12 },           // PSA Init column
                    { "tb_RB_type", 19 },           // RB Type column
                    { "tb_RB_Unit", 20 },           // RB Unit column
                    { "tb_RB_conversion", 21 },     // RB Conversion column
                    { "tb_RB_Res", 22 },            // RB Resolution column
                    { "tb_RB_Min", 23 },            // RB Min column
                    { "tb_RB_Max", 24 },            // RB Max column
                    { "tb_RB_offset", 25 },         // RB Offset column
                    { "tb_RB_init", 28 }            // RB Init column
                };

                // Map DataGridView column indices to Label properties for comparison
                var labelMappings = new Dictionary<string, int>
                {
                    { "lb_Condition", 34 },         // SC (Safety Condition) column
                    { "lb_Producer", 82 },          // Producer column
                    { "lb_consumer", 83 },          // Consumer column  
                    { "lb_Task", 37 }               // Task column
                };

                foreach (var mapping in columnMappings)
                {
                    string controlName = mapping.Key;
                    int columnIndex = mapping.Value;

                    // Find textbox controls in both UC_dataflow
                    Control oldControl = FindControlByName(oldDataflow, controlName);
                    Control newControl = FindControlByName(newDataflow, controlName);

                    if (oldControl != null && newControl != null)
                    {
                        // Get actual data from DataGridView rows instead of textbox values
                        string oldValue = oldRowData.Length > columnIndex ? 
                            oldRowData[columnIndex]?.ToString()?.Trim() ?? "" : "";
                        string newValue = newRowData.Length > columnIndex ? 
                            newRowData[columnIndex]?.ToString()?.Trim() ?? "" : "";

                        // Compare actual DataGridView data
                        bool areEqual = string.Equals(oldValue, newValue, StringComparison.OrdinalIgnoreCase);
                        
                        Color highlightColor = areEqual ? SameColor : DifferentColor;
                        
                        // Apply color to both controls
                        oldControl.BackColor = highlightColor;
                        newControl.BackColor = highlightColor;

                        // DEBUG: Log comparison of actual data
                        if (showTooltips)
                        {
                            System.Diagnostics.Debug.WriteLine($"Dataflow {controlName}: {(areEqual ? "MATCH" : "DIFFERENT")} - Old:'{oldValue}' vs New:'{newValue}'");
                        }
                    }
                }

                // Compare Label controls (SC, Producer, Consumer, Task)
                foreach (var mapping in labelMappings)
                {
                    string controlName = mapping.Key;
                    int columnIndex = mapping.Value;

                    // Find label controls in both UC_dataflow
                    Control oldControl = FindControlByName(oldDataflow, controlName);
                    Control newControl = FindControlByName(newDataflow, controlName);

                    if (oldControl != null && newControl != null)
                    {
                        // Get actual data from DataGridView rows
                        string oldValue = oldRowData.Length > columnIndex ? 
                            oldRowData[columnIndex]?.ToString()?.Trim() ?? "" : "";
                        string newValue = newRowData.Length > columnIndex ? 
                            newRowData[columnIndex]?.ToString()?.Trim() ?? "" : "";

                        // Compare actual DataGridView data
                        bool areEqual = string.Equals(oldValue, newValue, StringComparison.OrdinalIgnoreCase);
                        
                        Color highlightColor = areEqual ? SameColor : DifferentColor;
                        
                        // Apply color to both label controls
                        oldControl.BackColor = highlightColor;
                        newControl.BackColor = highlightColor;

                        // DEBUG: Log comparison of label data
                        if (showTooltips)
                        {
                            System.Diagnostics.Debug.WriteLine($"Dataflow Label {controlName}: {(areEqual ? "MATCH" : "DIFFERENT")} - Old:'{oldValue}' vs New:'{newValue}'");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error highlighting Dataflow differences: {ex.Message}");
            }
        }

        /// <summary>
        /// Get selected row data from UC_dataflow DataGridView
        /// Returns null if no row is selected
        /// </summary>
        private static object[] GetDataflowSelectedRowData(UC_dataflow dataflow)
        {
            try
            {
                // Find DataGridView in UC_dataflow
                DataGridView dataGridView = FindControlByName(dataflow, "dataGridView_DF") as DataGridView;
                if (dataGridView == null)
                {
                    System.Diagnostics.Debug.WriteLine("DataGridView not found in UC_dataflow");
                    return null;
                }

                // Check if there's a selected row
                if (dataGridView.CurrentRow == null || dataGridView.CurrentRow.Index < 0)
                {
                    System.Diagnostics.Debug.WriteLine("No row selected in DataGridView");
                    return null;
                }

                // Get all cell values from the selected row
                var selectedRow = dataGridView.CurrentRow;
                object[] rowData = new object[selectedRow.Cells.Count];
                for (int i = 0; i < selectedRow.Cells.Count; i++)
                {
                    rowData[i] = selectedRow.Cells[i].Value;
                }

                return rowData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting dataflow selected row data: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// UTILITY: Find TextBox control by name within a UserControl
        /// Searches recursively through all child controls
        /// </summary>
        private static TextBox FindTextBoxByName(Control parentControl, string controlName)
        {
            try
            {
                // Direct search in immediate children
                foreach (Control control in parentControl.Controls)
                {
                    if (control is TextBox textBox && textBox.Name == controlName)
                    {
                        return textBox;
                    }
                    
                    // Recursive search in container controls
                    if (control.HasChildren)
                    {
                        TextBox found = FindTextBoxByName(control, controlName);
                        if (found != null)
                        {
                            return found;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error finding TextBox '{controlName}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// UTILITY: Find ANY control (TextBox, RichTextBox, etc.) by name within a UserControl
        /// Searches recursively through all child controls
        /// </summary>
        private static Control FindControlByName(Control parentControl, string controlName)
        {
            try
            {
                // Direct search in immediate children
                foreach (Control control in parentControl.Controls)
                {
                    if (control.Name == controlName)
                    {
                        return control;
                    }
                    
                    // Recursive search in container controls
                    if (control.HasChildren)
                    {
                        Control found = FindControlByName(control, controlName);
                        if (found != null)
                        {
                            return found;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error finding Control '{controlName}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// UTILITY: Get text from control regardless of type (TextBox, RichTextBox, etc.)
        /// </summary>
        private static string GetControlText(Control control)
        {
            try
            {
                if (control is TextBox textBox)
                {
                    return textBox.Text;
                }
                else if (control is RichTextBox richTextBox)
                {
                    return richTextBox.Text;
                }
                else
                {
                    return control.Text; // Fallback for other controls
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting text from control: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// PERFORMANCE: Suspend UI layout updates during batch highlighting
        /// </summary>
        private static void SuspendUIUpdates(UC_ContextClearing contextClearing)
        {
            try
            {
                contextClearing.SuspendLayout();
                contextClearing.OldCarasiControl?.SuspendLayout();
                contextClearing.NewCarasiControl?.SuspendLayout();
                contextClearing.OldDataflowControl?.SuspendLayout();
                contextClearing.NewDataflowControl?.SuspendLayout();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error suspending UI updates: {ex.Message}");
            }
        }

        /// <summary>
        /// PERFORMANCE: Resume UI layout updates after batch highlighting
        /// </summary>
        private static void ResumeUIUpdates(UC_ContextClearing contextClearing)
        {
            try
            {
                contextClearing.OldDataflowControl?.ResumeLayout(false);
                contextClearing.NewDataflowControl?.ResumeLayout(false);
                contextClearing.NewCarasiControl?.ResumeLayout(false);
                contextClearing.OldCarasiControl?.ResumeLayout(false);
                contextClearing.ResumeLayout(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resuming UI updates: {ex.Message}");
            }
        }

        #endregion

        #region Clear Highlighting Methods

        /// <summary>
        /// CLEANUP: Clear all property highlighting from UC_ContextClearing
        /// Restores default white background to all textboxes
        /// </summary>
        public static void ClearAllHighlighting(UC_ContextClearing contextClearing)
        {
            try
            {
                contextClearing.SuspendLayout();
                
                // Clear Carasi highlighting
                ClearUserControlHighlighting(contextClearing.OldCarasiControl, CarasiPropertyMappings.Keys);
                ClearUserControlHighlighting(contextClearing.NewCarasiControl, CarasiPropertyMappings.Keys);
                
                // Clear Dataflow highlighting - using column mappings control names + labels
                var dataflowControlNames = new[]
                {
                    "tb_Description", "tb_RTE_direction", "tb_RB_MappingType", "tb_RB_PseudoCode",
                    "tb_PSAType", "tb_PSAunit", "tb_PSAConversion", "tb_PSARes", "tb_PSAMin", "tb_PSAMax", "tb_PSAOffset", "tb_PSAInit",
                    "tb_RB_type", "tb_RB_Unit", "tb_RB_conversion", "tb_RB_Res", "tb_RB_Min", "tb_RB_Max", "tb_RB_offset", "tb_RB_init",
                    "lb_Condition", "lb_Producer", "lb_consumer", "lb_Task"  // Add labels for SC, Producer, Consumer, Task
                };
                ClearUserControlHighlighting(contextClearing.OldDataflowControl, dataflowControlNames);
                ClearUserControlHighlighting(contextClearing.NewDataflowControl, dataflowControlNames);
                
                contextClearing.ResumeLayout(false);
                
                System.Diagnostics.Debug.WriteLine("Property highlighting cleared successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing highlighting: {ex.Message}");
                contextClearing.ResumeLayout(false); // Ensure layout is resumed
            }
        }

        /// <summary>
        /// UTILITY: Clear highlighting from specific UserControl - supports all control types
        /// </summary>
        private static void ClearUserControlHighlighting(Control userControl, IEnumerable<string> controlNames)
        {
            try
            {
                if (userControl == null) return;

                foreach (string controlName in controlNames)
                {
                    Control control = FindControlByName(userControl, controlName);
                    if (control != null)
                    {
                        control.BackColor = DefaultColor;
                        // No tooltip clearing needed - using debug logging instead
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing UserControl highlighting: {ex.Message}");
            }
        }

        #endregion
    }
}

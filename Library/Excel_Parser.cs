using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Check_carasi_DF_ContextClearing
{
    public struct Carasi_Interface
    {
        public string name;
        public string[] input;
        public string output;
        public string description;

        //Properties
        public string unit;
        public string minValue;
        public string maxValue;
        public string resolution;
        public string initialisation;
        public string swType;
        public string mmType;
        public string conversion;
        public string comments;
        public string computeDetails;
    }

    public struct Dataflow_Interface
    {
        public string status;
        public string description;
        public string Rte_Direction;
        public string MappingType;
        public string Pseudo_code;
        public string System_constant;
        public string FC_Name;
        public string Producer;
        public string Consumers;

        //Properties PSA
        public string PSAname;
        public string PSAswType;
        public string PSAunit;
        public string PSAconversion;
        public string PSAresolution;
        public string PSAminValue;
        public string PSAmaxValue;
        public string PSAoffset;
        public string PSAinitialisation;

        //Properties Bosch
        public string Boschname;
        public string BoschswType;
        public string Boschunit;
        public string Boschconversion;
        public string Boschresolution;
        public string BoschminValue;
        public string BoschmaxValue;
        public string Boschoffset;
        public string Boschinitialisation;
    }


    class Excel_Parser
    {
        const int MAX_NUMBER_OF_INTERFACE = 10000;
        const String MacroModule_Interface_sheet = "Interfaces";
        const String MacroModule_Dictionary_sheet = "Dictionary";
        const String DataFlow_Interface_sheet = "Mapping";
        const String MacroModule_signals = "carasi";
        const String Dataflow_signals = "dataflow";

        Lib_OLEDB_Excel __excel;
        DataTable dt_template = new DataTable();

        public Excel_Parser( string FileLink, DataTable dt_template)
        {
            linkOfFile = FileLink;
            lb_NameOfFile = linkOfFile.Split('\\').Last();
            __excel = new Lib_OLEDB_Excel(FileLink);
            this.dt_template = dt_template.Clone();
        }

        public void Dispose()
        {
            //Close all Connection
            __excel.Dispose();
        }

        public void search_Variable(string var)
        {
            clearData();

            lb_Name = var;
            
            if(lb_NameOfFile.ToLower().Contains(MacroModule_signals))
            {
                carasi_Parser(var);
            }
            else if (lb_NameOfFile.ToLower().Contains(Dataflow_signals))
            {
                dataflow_Parser(var);
            }
            else
            {
                MessageBox.Show("Please make sure 'carasi' contain in Carasi file name or 'dataflow' contain in Dataflow file name...", "Warning!");
            }
        }

        public bool _IsExist_Carasi(string var)
        {
            DataTable dt = __excel.ReadTable("Interfaces$", "[" + "SSTG label" + "]='" + var + "'");
            return (dt.Rows.Count > 0);
        }

        // BATCH OPTIMIZATION: Check multiple variables in single query
        public Dictionary<string, bool> _IsExist_Carasi_Batch(List<string> variables)
        {
            var results = new Dictionary<string, bool>();
            
            if (variables == null || variables.Count == 0)
                return results;

            // Initialize all variables as false
            foreach (string var in variables)
            {
                results[var] = false;
            }

            // Build WHERE IN clause for batch query
            var quotedVars = variables.Select(v => "'" + v.Replace("'", "''") + "'");
            string whereClause = "[SSTG label] IN (" + string.Join(",", quotedVars) + ")";
            
            DataTable dt = __excel.ReadTable("Interfaces$", whereClause);
            
            // Mark found variables as true
            foreach (DataRow row in dt.Rows)
            {
                string foundVar = row["SSTG label"].ToString();
                if (results.ContainsKey(foundVar))
                {
                    results[foundVar] = true;
                }
            }
            
            return results;
        }

        public bool _IsExist_Dataflow(string var)
        {
            DataTable dt = __excel.ReadTable("Mapping$", "[" + "F2" + "]='" + var + "'" + " OR " + "[" + "F17" + "]='" + var + "'");
            return (dt.Rows.Count > 0);
        }

        // BATCH OPTIMIZATION: Check multiple variables in Dataflow
        public Dictionary<string, bool> _IsExist_Dataflow_Batch(List<string> variables)
        {
            var results = new Dictionary<string, bool>();
            
            if (variables == null || variables.Count == 0)
                return results;

            // Initialize all variables as false
            foreach (string var in variables)
            {
                results[var] = false;
            }

            // Build WHERE clause for batch query (F2 OR F17 fields)
            var conditions = new List<string>();
            foreach (string var in variables)
            {
                string escapedVar = var.Replace("'", "''");
                conditions.Add("([F2]='" + escapedVar + "' OR [F17]='" + escapedVar + "')");
            }
            string whereClause = string.Join(" OR ", conditions);
            
            DataTable dt = __excel.ReadTable("Mapping$", whereClause);
            
            // Mark found variables as true
            foreach (DataRow row in dt.Rows)
            {
                string f2Value = row["F2"].ToString();
                string f17Value = row["F17"].ToString();
                
                if (results.ContainsKey(f2Value))
                    results[f2Value] = true;
                if (results.ContainsKey(f17Value))
                    results[f17Value] = true;
            }
            
            return results;
        }

        private void dataflow_Parser(string var)
        {
            DataTable dt =  dt_template.Clone();
            df_Properties = __excel.ReadTable("Mapping$", "[" + "F2" + "]='" + var + "'" + " OR " + "[" + "F17" + "]='" + var + "'");
            
            foreach (DataRow dr in df_Properties.Rows)
            {
                dt.Rows.Add(dr.ItemArray);
            }

            df_Properties = dt;
            dataview_df_Properties = df_Properties.DefaultView;
        }

        private void carasi_Parser(string var)
        {
            carasi_Interfaces = __excel.ReadTable("Interfaces$", "[" + "SSTG label" + "]='" + var + "'");
            carasi_dictionary = __excel.ReadTable("Dictionary$", "[" + "SSTG label" + "]='" + var + "'");

            dictionary = carasi_dictionary.DefaultView;
            interfaces = carasi_Interfaces.DefaultView;
        }

        private void clearData()
        {
            lb_Name = string.Empty;
            df_Properties.Clear();
            carasi_Interfaces.Clear();
            carasi_dictionary.Clear();

            dataview_df_Properties = null;
            dictionary = new DataView();
            interfaces = new DataView();
        }

        private string linkOfFile = string.Empty;
        private string lb_Name = string.Empty;
        private string lb_NameOfFile = string.Empty;
        private string computeDetails = string.Empty;

        private DataTable df_Properties = new DataTable();
        private DataView dataview_df_Properties = new DataView();
        private Carasi_Interface variable = new Carasi_Interface();

        private DataTable carasi_Interfaces = new DataTable(); 
        private DataTable carasi_dictionary = new DataTable();
        private DataView dictionary = new DataView();
        private DataView interfaces = new DataView();


        public string Lb_Name { get => lb_Name; }
        public string Lb_NameOfFile { get => lb_NameOfFile; }

        public DataTable DF_Properties { get => df_Properties;}
        public DataView Dataview_df_Properties { get => dataview_df_Properties;}
        public string ComputeDetails { get => computeDetails;}
        public Carasi_Interface Variable { get => variable; }
        public DataTable Carasi_Interfaces { get => carasi_Interfaces; }
        public DataTable Carasi_dictionary { get => carasi_dictionary; }
        public DataView Dictionary { get => dictionary; }
        public DataView Interfaces { get => interfaces; }
    }
}

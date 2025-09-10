using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        public string NameOfnewCarasi { get => nameOfnewCarasi; set => nameOfnewCarasi = value; }
        public string NameOfoldCarasi { get => nameOfoldCarasi; set => nameOfoldCarasi = value; }
        public string NameOfnewDataflow { get => nameOfnewDataflow; set => nameOfnewDataflow = value; }
        public string NameOfoldDataflow { get => nameOfoldDataflow; set => nameOfoldDataflow = value; }
        public string Link2Folder { get => link2Folder; set { if (value!= "" && folder_verifying(value)) link2Folder = value; } }

        internal Excel_Parser NewCarasi { get => newCarasi; set => newCarasi = value; }
        internal Excel_Parser OldCarasi { get => oldCarasi; set => oldCarasi = value; }
        internal Excel_Parser NewDF { get => newDF; set => newDF = value; }
        internal Excel_Parser OldDF { get => oldDF; set => oldDF = value; }

        private Excel_Parser newCarasi;
        private Excel_Parser oldCarasi;
        private Excel_Parser newDF;
        private Excel_Parser oldDF;

        public UC_ContextClearing()
        {
            InitializeComponent();
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
            }
           
            if (isValidnewCarasi && isValidnewDataflow && isValidoldCarasi && isValidoldDataflow)
                isValid = true;
            else if (isValid == false)
                MessageBox.Show("Link is not exist!!", "Warning!");
            else
            {
                MessageBox.Show("Please check again content of Folder! Should have 4 files NewCarasi, OldCarasi, NewDF, OldDF !!", "Warning!");
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
            }
            else
            {
                var a1 = _Parser.Dataview_df_Properties;
                UC_OldDF.setValue_UC(_Parser.Lb_NameOfFile, a1);
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

    }
}

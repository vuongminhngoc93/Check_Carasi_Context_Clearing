using System;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Check_carasi_DF_ContextClearing
{
    public partial class UC_Carasi : UserControl
    {

        public UC_Carasi()
        {
            InitializeComponent();
            cleanUp();
        }

        //private void OnDispose(object sender, EventArgs e)
        //{
        //    // do stuff on dispose

        //}

        public void SetToolTip(ToolTip toolTip)
        {
            foreach (Control _control in this.Controls)
            {
                toolTip1.SetToolTip(_control, _control.Text);
            }
        }

        private StringBuilder build_string(string[] array)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                if(array[i] != null)
                {
                    sb.AppendFormat("{0}    {1}", i.ToString(), array[i]);
                    sb.AppendLine();
                    sb.AppendLine();
                }
            }
            return sb;
        }

        private void cleanUp()
        {
            foreach (Control c in this.tableLayoutPanel1.Controls)
            {
                if (c.GetType().Name == "TextBox")
                {
                    c.Text = "";
                }
                if (c.GetType().Name == "RichTextBox")
                {
                    c.Text = "";
                }
            }
            lb_Name.Text = "Name of Interface";
            lb_hint.Text = "Hint: ";
        }

        public void setValue_UC(string lb_Name, DataView carasi_Interface, DataView carasi_Dictionary, string __hint, string __nameOfFile)
        {
            cleanUp();
            string[] input = new string[100]; 
            //loop all rows
            for (int i = 0; i < carasi_Interface.Count; i++)
            {
                this.lb_Name.Text = lb_Name;

                if (carasi_Interface[i][3] != null && carasi_Interface[i][3].ToString().ToLower() == "input")
                {
                    string tmp = carasi_Interface[i][1] != null ? carasi_Interface[i][1].ToString() : "";
                    string des = carasi_Interface[i][5] != null ? carasi_Interface[i][5].ToString() : "";

                    input[i] = tmp;
                    this.tb_Description.Text = des;
                }
                else if (carasi_Interface[i][3] != null && carasi_Interface[i][3].ToString().ToLower() == "output")
                {
                    string tmp = carasi_Interface[i][1] != null ? carasi_Interface[i][1].ToString() : "Please check Function name in Carasi";
                    string des = carasi_Interface[i][5] != null ? carasi_Interface[i][5].ToString() : "Please check Function name in Carasi";

                    this.richTextBox_Ouput.Text = tmp;
                    this.tb_Description.Text = des;
                }
                else if (carasi_Interface[i][3] != null && carasi_Interface[i][3].ToString().ToLower() == "calib")
                {
                    string tmp = carasi_Interface[i][1] != null ? carasi_Interface[i][1].ToString() : "Please check Function name in Carasi";
                    string des = carasi_Interface[i][5] != null ? carasi_Interface[i][5].ToString() : "Please check Function name in Carasi";

                    input[i] = tmp;
                    this.tb_Description.Text = "CALIB  :" + des;
                }
                else if (carasi_Interface[i][3] != null && carasi_Interface[i][3].ToString().ToLower() == "local")
                {
                    string tmp = carasi_Interface[i][1] != null ? carasi_Interface[i][1].ToString() : "Please check Function name in Carasi";
                    string des = carasi_Interface[i][5] != null ? carasi_Interface[i][5].ToString() : "Please check Function name in Carasi";

                    input[i] = tmp;
                    this.tb_Description.Text = "LOCAL  :" + des;
                }
                else
                {
                    MessageBox.Show("Wrong Carasi format! No output or input found! ", "ERROR!");
                }

                if (carasi_Interface[i][0] != null && carasi_Interface[i][0].ToString().ToLower() == "stub")
                {
                    this.tb_stub_InitValue.Text = carasi_Interface[i][8] != null ? carasi_Interface[i][8].ToString() : "";
                }
            }

            //loop all rows
            if (carasi_Dictionary.Count > 0)
            {
                this.tb_Unit.Text = carasi_Dictionary[0][4] != null ? carasi_Dictionary[0][4].ToString() : "";
                this.rtb_computeDetails.Text = carasi_Dictionary[0][6] != null ? carasi_Dictionary[0][6].ToString() : "";
                this.tb_Min.Text = carasi_Dictionary[0][7] != null ? carasi_Dictionary[0][7].ToString() : "";
                this.tb_Max.Text = carasi_Dictionary[0][8] != null ? carasi_Dictionary[0][8].ToString() : "";
                this.tb_Res.Text = carasi_Dictionary[0][9] != null ? carasi_Dictionary[0][9].ToString() : "";
                this.tb_Init.Text = carasi_Dictionary[0][10] != null ? carasi_Dictionary[0][10].ToString() : "";
                this.tb_Type.Text = carasi_Dictionary[0][14] != null ? carasi_Dictionary[0][14].ToString() : "";
                this.tb_Conversion.Text = carasi_Dictionary[0][15] != null ? carasi_Dictionary[0][15].ToString() : "";

                this.tb_TypeMM.Text = carasi_Dictionary[0][17] != null ? carasi_Dictionary[0][17].ToString() : "";
            }

            this.richTextBox_Input.Text = build_string(input).ToString();
            this.lb_hint.Text = "Hint: " + __hint;
            this.lb_NameOfFile.Text = __nameOfFile;

        }
    }
}

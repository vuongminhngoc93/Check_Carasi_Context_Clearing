using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Check_carasi_DF_ContextClearing
{
    public partial class UC_dataflow : UserControl
    {
        // EVENT: To notify parent UC_ContextClearing when cell is clicked for highlighting
        public event System.Action OnCellClickedForHighlighting;

        public UC_dataflow()
        {
            InitializeComponent();
            flag_IsInternal = false;
            dt_temp = null;
            internal_DF = null;
            link_Of_DF_file = string.Empty;
        }

        private string link_Of_DF_file = string.Empty;
        private DataTable dt_temp;
        private Excel_Parser internal_DF;
        private bool flag_IsInternal = false;

        public string Link_Of_DF_file { get => link_Of_DF_file; set { link_Of_DF_file = value; run_internal(); } }
        public DataTable Dt_temp { get => dt_temp; set => dt_temp = value; }
        public bool Flag_IsInternal { get => flag_IsInternal; set => flag_IsInternal = value; }

        private void run_internal()
        {
            if (dt_temp != null)
            {
                flag_IsInternal = true;
                internal_DF = new Excel_Parser(link_Of_DF_file, dt_temp);
            }
        }

        public void __checkVariable(string Interface2search)
        {
            if (flag_IsInternal && internal_DF!= null)
            {
                string new_variable = string.Empty;

                Cursor.Current = Cursors.WaitCursor;
                //Verify String Name
                if (Interface2search != "")
                    new_variable = Interface2search;
                else
                    MessageBox.Show("Please insert the Interface Name", "Warning!");

                //Run searching Excel file
                if (new_variable != string.Empty)
                {
                    Doing_serching(internal_DF, new_variable);
                }
                Cursor.Current = Cursors.Default;
            }
            else
                MessageBox.Show("There is no new DF Viewer opened!");
        }

        private void Doing_serching(Excel_Parser _Parser, string new_variable)
        {
            _Parser.search_Variable(new_variable);
            this.setValue_UC(_Parser.Lb_NameOfFile, _Parser.Dataview_df_Properties);
        }

        /*************************************************************************************************************************************/
        public void setValue_UC(string lb_NameOfFile, DataView dt)
        {
            dataGridView_DF.AutoGenerateColumns = true;
            
            // FIX: Create a COPY of DataView to avoid reference issues when Excel_Parser clears data
            if (dt != null)
            {
                // Create a new DataTable from the DataView to break the reference
                DataTable copyTable = dt.ToTable();
                dataGridView_DF.DataSource = copyTable;
                System.Diagnostics.Debug.WriteLine($"DATAVIEW FIX: Created independent copy for {lb_NameOfFile} with {copyTable.Rows.Count} rows");
            }
            else
            {
                dataGridView_DF.DataSource = null;
            }
            
            this.lb_NameOfFile.Text = lb_NameOfFile;
            
            // AUTO-SELECT: Basic first cell selection
            if (dt != null && dt.Count > 0)
            {
                try
                {
                    // Direct selection without BeginInvoke to avoid conflicts
                    if (dataGridView_DF.Rows.Count > 0)
                    {
                        dataGridView_DF.CurrentCell = dataGridView_DF.Rows[0].Cells[0];
                        dataGridView_DF.ClearSelection();
                        dataGridView_DF.Rows[0].Selected = true;
                        dataGridView_DF_CellClick(dataGridView_DF, new DataGridViewCellEventArgs(0, 0));
                        System.Diagnostics.Debug.WriteLine($"AUTO-SELECT: Selected first cell in {lb_NameOfFile}");
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"AUTO-SELECT ERROR: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// AUTO-SELECT: Public method to select first cell and trigger cell click event
        /// This will show first row data in textboxes below the DataGridView
        /// </summary>
        public void SelectFirstCell()
        {
            if (dataGridView_DF.Rows.Count > 0)
            {
                // Select first cell
                dataGridView_DF.CurrentCell = dataGridView_DF.Rows[0].Cells[0];
                dataGridView_DF.ClearSelection();
                dataGridView_DF.Rows[0].Selected = true;
                
                // Trigger cell click event to populate textboxes
                dataGridView_DF_CellClick(dataGridView_DF, new DataGridViewCellEventArgs(0, 0));
            }
        }

        private void dataGridView_DF_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex > -1)
            {
                this.lb_status.Text = dataGridView_DF.Rows[e.RowIndex].Cells[0].Value.ToString();
                if (this.lb_status.Text == "Remove") this.lb_status.BackColor = Color.Red;
                else if (this.lb_status.Text == "Add") this.lb_status.BackColor = Color.Red;
                else if (this.lb_status.Text == "Change") this.lb_status.BackColor = Color.Red;
                else this.lb_status.BackColor = Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));

                this.tb_Description.Text = dataGridView_DF.Rows[e.RowIndex].Cells[2].Value.ToString();
                this.tb_RTE_direction.Text = dataGridView_DF.Rows[e.RowIndex].Cells[15].Value.ToString();
                this.tb_RB_MappingType.Text = dataGridView_DF.Rows[e.RowIndex].Cells[29].Value.ToString();
                this.tb_RB_PseudoCode.Text = dataGridView_DF.Rows[e.RowIndex].Cells[31].Value.ToString();
                this.lb_Condition.Text = "SC: \n"+ dataGridView_DF.Rows[e.RowIndex].Cells[34].Value.ToString();
                string FC_Name = dataGridView_DF.Rows[e.RowIndex].Cells[89].Value.ToString();
                
                this.lb_Producer.Text = "Producer: \n" + dataGridView_DF.Rows[e.RowIndex].Cells[82].Value.ToString();
                if (lb_status.Text != "" && this.lb_Producer.Text == "Producer: \n") this.lb_Producer.BackColor = Color.Red;
                else this.lb_Producer.BackColor = Color.AntiqueWhite;

                this.lb_consumer.Text = "Consumer: \n" + dataGridView_DF.Rows[e.RowIndex].Cells[83].Value.ToString();
                if (lb_status.Text != "" && this.lb_consumer.Text == "Consumer: \n") this.lb_consumer.BackColor = Color.Red;
                else this.lb_consumer.BackColor = Color.AntiqueWhite;

                this.lb_Task.Text = "Task: \n" + dataGridView_DF.Rows[e.RowIndex].Cells[37].Value.ToString();
                this.lb_PSAName.Text = dataGridView_DF.Rows[e.RowIndex].Cells[1].Value.ToString();
                this.tb_PSAType.Text = dataGridView_DF.Rows[e.RowIndex].Cells[5].Value.ToString();
                this.tb_PSAunit.Text = dataGridView_DF.Rows[e.RowIndex].Cells[6].Value.ToString();
                this.tb_PSAConversion.Text = dataGridView_DF.Rows[e.RowIndex].Cells[7].Value.ToString();
                this.tb_PSARes.Text = dataGridView_DF.Rows[e.RowIndex].Cells[8].Value.ToString();
                this.tb_PSAMin.Text = dataGridView_DF.Rows[e.RowIndex].Cells[9].Value.ToString();
                this.tb_PSAMax.Text = dataGridView_DF.Rows[e.RowIndex].Cells[10].Value.ToString();
                this.tb_PSAOffset.Text = dataGridView_DF.Rows[e.RowIndex].Cells[11].Value.ToString();
                this.tb_PSAInit.Text = dataGridView_DF.Rows[e.RowIndex].Cells[12].Value.ToString();
                this.lb_BoschName_FCName.Text = dataGridView_DF.Rows[e.RowIndex].Cells[16].Value.ToString() + "  : FC = " + FC_Name;
                this.tb_RB_type.Text = dataGridView_DF.Rows[e.RowIndex].Cells[19].Value.ToString();
                this.tb_RB_Unit.Text = dataGridView_DF.Rows[e.RowIndex].Cells[20].Value.ToString();
                this.tb_RB_conversion.Text = dataGridView_DF.Rows[e.RowIndex].Cells[21].Value.ToString();
                this.tb_RB_Res.Text = dataGridView_DF.Rows[e.RowIndex].Cells[22].Value.ToString();
                this.tb_RB_Min.Text = dataGridView_DF.Rows[e.RowIndex].Cells[23].Value.ToString();
                this.tb_RB_Max.Text = dataGridView_DF.Rows[e.RowIndex].Cells[24].Value.ToString();
                this.tb_RB_offset.Text = dataGridView_DF.Rows[e.RowIndex].Cells[25].Value.ToString();
                this.tb_RB_init.Text = dataGridView_DF.Rows[e.RowIndex].Cells[28].Value.ToString();
                
                // HIGHLIGHT TRIGGER: Notify parent UC_ContextClearing to re-run highlighting
                // This ensures color comparison happens when user clicks different rows
                try
                {
                    OnCellClickedForHighlighting?.Invoke();
                    System.Diagnostics.Debug.WriteLine($"CELL CLICK: Triggered highlighting for row {e.RowIndex}");
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"CELL CLICK HIGHLIGHT ERROR: {ex.Message}");
                }
            }
        }
    }
}

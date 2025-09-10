using Check_carasi_DF_ContextClearing.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using OutlookApp = Microsoft.Office.Interop.Outlook.Application;
using System.Reflection;

namespace Check_carasi_DF_ContextClearing
{
    public partial class Form1 : Form
    {
        private string link2Folder = string.Empty;
        private string nameOfnewCarasi = string.Empty;
        private string nameOfoldCarasi = string.Empty;
        private string nameOfnewDataflow = string.Empty;
        private string nameOfoldDataflow = string.Empty;

        public string Link2Folder { get => link2Folder; set => link2Folder = value; }
        public string NameOfnewCarasi { get => nameOfnewCarasi; set => nameOfnewCarasi = value; }
        public string NameOfoldCarasi { get => nameOfoldCarasi; set => nameOfoldCarasi = value; }
        public string NameOfnewDataflow { get => nameOfnewDataflow; set => nameOfnewDataflow = value; }
        public string NameOfoldDataflow { get => nameOfoldDataflow; set => nameOfoldDataflow = value; }

        /*Project Information
         * projectName : Project name, Ex: [eVCU] V1C3, ...
         * pverRQ1 : RQ1 Id for Pver Release
         * dateForDD : DD Collection Deadline for DF
         * dateForBugfix : Bugfix Collection Deadline
         */
        private string projectName = string.Empty;
        private string pverRQ1 = string.Empty;
        private string dateForDD = string.Empty;
        private string dateForBugfix = string.Empty;
        private string reserve = string.Empty;
        /************************************************/

        private bool isValidnewCarasi = false;
        private bool isValidoldCarasi = false;
        private bool isValidnewDataflow = false;
        private bool isValidoldDataflow = false;

        DataTable dt_template = new DataTable();
        string tempPath = string.Empty;

        UC_dataflow internalUC;
        Form DF_viewer = new Form();
        MM_Check _mmCheck = null;
        A2L_Check _a2lCheck = null;
        RichTextBox Newlist;  //list of something to search, make sure create new RichTextBox() before using it


        public Form1()
        {
            InitializeComponent();
            lb_version.Text = VersionLabel;
            buildHeader();
        }

        public string VersionLabel
        {
            get
            {
                if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                {
                    Version ver = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion;
                    return string.Format("Product Name: {4}, Version: {0}.{1}.{2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision, Assembly.GetEntryAssembly().GetName().Name);
                }
                else
                {
                    var ver = Assembly.GetExecutingAssembly().GetName().Version;
                    return string.Format("Product Name: {4}, Version: {0}.{1}.{2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision, Assembly.GetEntryAssembly().GetName().Name);
                }
            }
        }

        private void buildHeader()
        {
            tempPath = System.IO.Path.GetTempPath();
            System.IO.File.WriteAllBytes(tempPath + "template.xlsx", Properties.Resources.template_dataflow);
            Lib_OLEDB_Excel template = new Lib_OLEDB_Excel(tempPath + "template.xlsx");
            dt_template = template.ReadTable("Sheet1$");
        }

        private void btn_Link2Folder_Click(object sender, EventArgs e)
        {
            var a = Settings.Default.LinkOfFolder;
            folderBrowserDialog1.SelectedPath = Settings.Default.LinkOfFolder != null ? Settings.Default.LinkOfFolder: @"C:\";
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if(result == DialogResult.OK)
            {
                bool IsvalidFolder = folder_verifying(folderBrowserDialog1.SelectedPath);
                if (IsvalidFolder)
                {
                    link2Folder = folderBrowserDialog1.SelectedPath;
                    tb_Link2Folder.Text = folderBrowserDialog1.SelectedPath;
                    Settings.Default.LinkOfFolder = link2Folder;
                    Settings.Default.Save();
                }
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
            else if (!isValid)
                MessageBox.Show("Link is not exist!", "Warning!");
            else
            {
                MessageBox.Show("Please check again content of Folder! Should have 4 files NewCarasi, OldCarasi, NewDF, OldDF !!", "Warning!");
            }
            
            return isValid;
        }

        private void btn_Run_Click(object sender, EventArgs e)
        {
            toolStripProgressBar1.Value = 0;
            if (link2Folder == "")
            {
                link2Folder = tb_Link2Folder.Text;
                Settings.Default.LinkOfFolder = link2Folder;
                Settings.Default.Save();
            }

            if (DF_viewer.Visible == true)
            {
                if (internalUC.Flag_IsInternal)
                {
                    if (tb_Interface2search.Text != "")
                    {
                        internalUC.__checkVariable(tb_Interface2search.Text);
                        toolStripProgressBar1.Value = 10;
                    }
                    else
                        MessageBox.Show("Please insert the Interface Name", "Warning!");
                }
            }
            else
            {
                internalUC = new UC_dataflow();
            }

            if (folder_verifying(link2Folder))
            {
                UC_ContextClearing UC_doing = new UC_ContextClearing();
                foreach (Control c in tabControl1.SelectedTab.Controls)
                {
                    if (c.GetType().Name == "UC_ContextClearing")
                    {
                        UC_doing = (UC_ContextClearing)c;
                        UC_doing.NewCarasi = new Excel_Parser(nameOfnewCarasi, dt_template);
                        UC_doing.OldCarasi = new Excel_Parser(nameOfoldCarasi, dt_template);
                        UC_doing.NewDF = new Excel_Parser(nameOfnewDataflow, dt_template);
                        UC_doing.OldDF = new Excel_Parser(nameOfoldDataflow, dt_template);

                        UC_doing.Link2Folder = link2Folder;
                        if (tb_Interface2search.Text != "")
                        {
                            UC_doing.__checkVariable(ref toolStripProgressBar1, tb_Interface2search.Text);
                            /* Check and update Macro Module searching Feature */
                            if(_mmCheck != null && _mmCheck.IsValidLink)
                            {
                                string[] result = new string[150];
                                bool a = _mmCheck.IsExistInMM(tb_Interface2search.Text, ref result);
                                UC_doing._setValueMM(a, result);
                            }

                            /* Check and update A2L searching Feature */
                            if (_a2lCheck != null && _a2lCheck.IsValidLink)
                            {
                                string[] result = new string[150];
                                bool a = _a2lCheck.IsExistInA2L(tb_Interface2search.Text, ref result);
                                UC_doing._setValueA2L(a, result);
                            }

                            tabControl1.SelectedTab.Text = tb_Interface2search.Text;
                        }
                        else
                            MessageBox.Show("Please insert the Interface Name", "Warning!");

                        //Disposed all Excel Parser
                        UC_doing.NewCarasi.Dispose();
                        UC_doing.OldCarasi.Dispose();
                        UC_doing.NewDF.Dispose();
                        UC_doing.OldDF.Dispose();
                    }
                }
            }
        }

        private void btn_toolStrip_NewTab_Click(object sender, EventArgs e)
        {
            try
            {
                if (tabControl1.TabPages.Count > 0)
                {
                    TabPage newTab = new TabPage("TabPage" + (tabControl1.TabPages.Count + 1).ToString());
                    UC_ContextClearing newUC = new UC_ContextClearing();
                    newUC.Dock = DockStyle.Fill;
                    newTab.Controls.Add(newUC);
                    tabControl1.TabPages.Add(newTab);
                    tabControl1.SelectedTab = newTab;
                }
                else
                {
                    TabPage newTab = new TabPage("TabPage1");
                    UC_ContextClearing newUC = new UC_ContextClearing();
                    newUC.Dock = DockStyle.Fill;
                    newTab.Controls.Add(newUC);
                    tabControl1.TabPages.Add(newTab);
                    tabControl1.SelectedTab = newTab;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show( "Reached Max GDI!" + '\n' + ex.Message);
            }
        }

        private void btn_toolStrip_daleteTab_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count > 1)
            {
                DialogResult result = MessageBox.Show("Do you want to delete the Tab?", "Warning!", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    foreach (Control c in tabControl1.SelectedTab.Controls)
                    {
                        if (c.GetType().Name == "UC_ContextClearing")
                        {
                            c.Dispose();
                        }
                    }
                    tabControl1.TabPages.Remove(tabControl1.SelectedTab);
                }
            }
            else
                MessageBox.Show("There is only one Tab here! we cannot delete it!");
        }

        private void closeAllTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count > 1)
            {
                DialogResult result = MessageBox.Show("Do you want to delete the All Tabs?", "Warning!", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    foreach (TabPage a in tabControl1.TabPages)
                    {
                        foreach (Control c in a.Controls)
                        {
                            if (c.GetType().Name == "UC_ContextClearing")
                            {
                                c.Dispose();
                            }
                        }
                        tabControl1.TabPages.Remove(a);
                    }
                    btn_toolStrip_NewTab.PerformClick();
                }
            }
            else
                MessageBox.Show("There is only one Tab here! we cannot delete it!");
        }

        private void btn_toolStrip_MacroModule_Click(object sender, EventArgs e)
        {
            //folderBrowserDialog1.SelectedPath = Settings.Default.LinkOfReview;
            //DialogResult result = folderBrowserDialog1.ShowDialog();
            //if (result == DialogResult.OK)
            //{
            //    Review_IM_change __review = new Review_IM_change(folderBrowserDialog1.SelectedPath);
            //    DataTable[] a =  __review.AllData;
            //    Settings.Default.LinkOfReview = folderBrowserDialog1.SelectedPath;
            //    Settings.Default.Save();
            //}

            _mmCheck = new MM_Check();

            folderBrowserDialog1.SelectedPath = Settings.Default.LinkOfFolder != null ? Settings.Default.LinkOfFolder : @"C:\";
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                _mmCheck.Link_Of_MM = folderBrowserDialog1.SelectedPath;
                bool IsvalidFolder = _mmCheck.IsValidLink;
                if (IsvalidFolder)
                {
                    Settings.Default.LinkOfFolder = folderBrowserDialog1.SelectedPath;
                    Settings.Default.Save();
                }
            }

        }

        private void a2LCheckToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string link_A2L = string.Empty;

            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = @"D:\",
                Title = "Browse A2L File",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "a2l",
                Filter = "a2l files (*.a2l)|*.a2l",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                _a2lCheck = new A2L_Check();
                link_A2L = openFileDialog1.FileName;
                _a2lCheck.Link_Of_A2L = link_A2L;
            }
        }

        private void tb_Interface2search_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
                btn_Run.PerformClick();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void estimationCheckToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string _linkOfEstimation = string.Empty;
            if (link2Folder == "")
            {
                link2Folder = tb_Link2Folder.Text;
                Settings.Default.LinkOfFolder = link2Folder;
                Settings.Default.Save();
            }

            if (folder_verifying(link2Folder))
            {
                /**************************************************************************************************************/
                OpenFileDialog openFileDialog1 = new OpenFileDialog
                {
                    InitialDirectory = Settings.Default.LinkOfFolder != null ? Settings.Default.LinkOfFolder : @"C:\",
                    Title = "Browse Estimation Files",

                    CheckFileExists = true,
                    CheckPathExists = true,

                    DefaultExt = "xlsx",
                    Filter = "xlsx files (*.xlsx)|*.xlsx",
                    FilterIndex = 2,
                    RestoreDirectory = true,

                    ReadOnlyChecked = true,
                    ShowReadOnly = true
                };

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    _linkOfEstimation = openFileDialog1.FileName;

                    Lib_OLEDB_Excel _listOfInterfaces = new Lib_OLEDB_Excel(_linkOfEstimation);
                    DataTable dt_listInterfaces = _listOfInterfaces.ReadTable("Dataflow consolidated$");
                    /**************************************************************************************************************/
                    Excel_Parser _RevewNewCarasi = new Excel_Parser(nameOfnewCarasi, dt_template);
                    Excel_Parser _RevewOldCarasi = new Excel_Parser(nameOfoldCarasi, dt_template);
                    /**************************************************************************************************************/

                    dt_listInterfaces.Columns.Add();

                    // BATCH OPTIMIZATION: Collect all variables first
                    var allVariables = new List<string>();
                    foreach (DataRow dr in dt_listInterfaces.Rows)
                    {
                        allVariables.Add(dr[0].ToString());
                    }

                    // BATCH OPTIMIZATION: Single batch query for each parser instead of individual queries
                    var newCarasiResults = _RevewNewCarasi._IsExist_Carasi_Batch(allVariables);
                    var oldCarasiResults = _RevewOldCarasi._IsExist_Carasi_Batch(allVariables);

                    // Apply results to datatable
                    foreach (DataRow dr in dt_listInterfaces.Rows)
                    {
                        string variable = dr[0].ToString();
                        bool isNew = newCarasiResults.ContainsKey(variable) && newCarasiResults[variable];
                        bool isOld = oldCarasiResults.ContainsKey(variable) && oldCarasiResults[variable];
                        
                        if (isNew && isOld)
                        {
                            dr[3] = "Old & New. Please Check";
                        }
                        else if (isNew && !isOld)
                        {
                            dr[3] = "New Requirement to ADD";
                        }
                        else if (!isNew && isOld)
                        {
                            dr[3] = "New Requirement to REMOVE";
                        }
                        else if (!isNew && !isOld)
                        {
                            dr[3] = "Not in both New and Old! Maybe Internal Request";
                        }
                        else
                        {

                        }
                    }

                    var lines = new List<string>();

                    string[] columnNames = dt_listInterfaces.Columns.Cast<DataColumn>()
                        .Select(column => column.ColumnName)
                        .ToArray();

                    var header = string.Join(",", columnNames.Select(name => $"\"{name}\""));
                    lines.Add(header);

                    var valueLines = dt_listInterfaces.AsEnumerable()
                        .Select(row => string.Join(",", row.ItemArray.Select(val => $"\"{val}\"")));

                    lines.AddRange(valueLines);

                    File.WriteAllLines(link2Folder + @"\\ExtimationReview.csv", lines);
                    MessageBox.Show("DONE!");
                }
            }
            else
            {
                MessageBox.Show("Please choosing Link of Project first! Click Browser and choose your folder with Carasi and DataFlow!");
            }

        }

        private void toolStripMenuItem_DDRequest_Click(object sender, EventArgs e)
        {
            string _PrjName = "Br1Bis V2C3.1 SW";
            string _PverName = "PVER: RQONE02649748 - PVER : VC1CP019_Br1bis / C010C_1";
            string _DD_Deadline = "09.03.2022";

            OutlookApp outlookApp = new OutlookApp();
            MailItem mailItem = outlookApp.CreateItem(OlItemType.olMailItem);

            mailItem.Subject = "[STLA][eVCU] " + _PrjName + "- DD request for Adapter development";
            mailItem.CC = " VUONG MINH NGOC (MS/EJV63-PS) <NGOC.VUONGMINH@vn.bosch.com>; Huynh Trong Hieu (MS/EJV63-PS) <Hieu.HuynhTrong@vn.bosch.com>; " +
                "Huynh Nhat Thanh (MS/EJV63-PS) <Thanh.HuynhNhat2@vn.bosch.com>; Aloysius Claud Pinto (MS/EEP31-PS) <Aloysius.ClaudPinto@in.bosch.com>; " +
                "Shrinidhi Chandrashekhar Magadal (MS/EEP31-PS) <Magadal.ShrinidhiChandrashekhar@in.bosch.com>";

            mailItem.HTMLBody = "<html><body> " +
                "<p><span> Hi all, </span></p>" +
                "<p> Below are the Issues planned for " + _PrjName + " : <b>" + _PverName + "</b><br>" +
                "Could you all please provide me DDs or info on any adapter implementations needed for your modules by " +
                "<b><span style = 'background:yellow;mso-highlight:yellow'>" + _DD_Deadline + "</span></b>" + "(EOD)?" + "<br>" +
                "Thanks 😊 And have a nice day all!" + "<br> <p>" +
                buildBody_DDRequest() +
                "<p></p>" +
                "<b><span style = 'background:yellow;mso-highlight:yellow'> Please ignore if its not relevant to you !</span></b> </body></html>" +
                "<p> Trân trọng / Best regards, </p>";
            //Set a high priority to the message
            mailItem.Importance = OlImportance.olImportanceHigh;
            mailItem.Display(true);
        }

        private string buildBody_DDRequest()
        {
            return "<p><b> RIU8KOR - Krishnaprasath Suryanarayanan (MS/EEP1-PS)</b> <br>" +
                   "&emsp; RQONE02812710 - 01552_20_00296_v3.0 - Actuator tests </t> </ p>";
        }

        private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void toolStripMenuItem_AddExtraDF_Click(object sender, EventArgs e)
        {
            internalUC = new UC_dataflow();
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = Settings.Default.LinkOfFolder != null ? Settings.Default.LinkOfFolder : @"C:\",
                Title = "Browse A new DF File",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "xls",
                Filter = "xls files (*.xls)|*.xls",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                internalUC.Dt_temp = dt_template;
                internalUC.Link_Of_DF_file = openFileDialog1.FileName;

                internalUC.Dock = DockStyle.Fill;
                DF_viewer.Controls.Add(internalUC);
                DF_viewer.Show(this);
            }
        }

        /* Search multi interfaces in 1 Project in 1 shot */
        private void searchListInterfacesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form NewlistInterface = new Form();
            Newlist = new RichTextBox();
            Newlist.Dock = DockStyle.Fill;
            Newlist.Text = "";

            NewlistInterface.Controls.Add(Newlist);
            NewlistInterface.Show();

            //Call event to run searching
            NewlistInterface.FormClosed += new FormClosedEventHandler(searchList_of_Interface);

        }

        private void searchInterfaceInMultiBranchesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form NewlistInterface = new Form();
            Newlist = new RichTextBox();
            Newlist.Dock = DockStyle.Fill;
            Newlist.Text = "";

            NewlistInterface.Controls.Add(new TableLayoutPanel());
            NewlistInterface.Controls.Add(Newlist);
            NewlistInterface.Show();
        }

        /* Event to search multi interfaces per 1 shot */
        private void searchList_of_Interface(object sender, FormClosedEventArgs e)
        {
            var a = Newlist.Text;
            string[] listOfInterfaces = Newlist.Text.Split('\n');

            for (int i = 0; i < listOfInterfaces.Length - 1; i++)
            {
                btn_toolStrip_NewTab.PerformClick();
                tb_Interface2search.Text = listOfInterfaces[i];
                btn_Run.PerformClick();
            }
        }


    }
}

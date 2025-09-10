using Check_carasi_DF_ContextClearing.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        
        // Flag to track batch operations to avoid double warnings
        private bool isBatchOperation = false;

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
            
            // Initialize status display
            UpdateTabMemoryStatus();
            
            // Setup lightweight modern styling (no heavy custom drawing)
            SetupLightweightModernStyling();
        }
        
        /// <summary>
        /// Lightweight modern styling for better performance
        /// </summary>
        private void SetupLightweightModernStyling()
        {
            // Use built-in Windows styling instead of custom drawing for performance
            tabControl1.Appearance = TabAppearance.Normal;
            tabControl1.SizeMode = TabSizeMode.Normal;
            
            // Setup tab selection optimization (keep this for performance)
            tabControl1.SelectedIndexChanged += OnTabSelectionChanged;
            
            // Setup modern toolbar styling (lightweight)
            toolStrip1.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            toolStrip1.RenderMode = ToolStripRenderMode.Professional;
            
            // Setup button hover effects (lightweight)
            SetupButtonHoverEffects();
            
            // Apply modern fonts only (no heavy visual effects)
            ApplyModernFonts();
            
            // Add Performance menu
            SetupPerformanceMenu();
        }
        
        /// <summary>
        /// Setup Performance monitoring menu
        /// </summary>
        private void SetupPerformanceMenu()
        {
            try
            {
                // Create Performance dropdown button
                var perfDropDown = new ToolStripDropDownButton
                {
                    Text = "📊 Performance",
                    ToolTipText = "Performance monitoring and analysis"
                };
                
                // Show Performance Report menu item
                var showReportItem = new ToolStripMenuItem
                {
                    Text = "📈 Show Performance Report",
                    ToolTipText = "Display detailed performance analysis"
                };
                showReportItem.Click += ShowPerformanceReport_Click;
                
                // Open Log File menu item
                var openLogItem = new ToolStripMenuItem
                {
                    Text = "📄 Open Log File",
                    ToolTipText = "Open CSV log file for detailed analysis"
                };
                openLogItem.Click += OpenLogFile_Click;
                
                // Clear Metrics menu item
                var clearMetricsItem = new ToolStripMenuItem
                {
                    Text = "🗑️ Clear Metrics",
                    ToolTipText = "Reset all performance data"
                };
                clearMetricsItem.Click += ClearMetrics_Click;
                
                // Add items to dropdown
                perfDropDown.DropDownItems.Add(showReportItem);
                perfDropDown.DropDownItems.Add(openLogItem);
                perfDropDown.DropDownItems.Add(new ToolStripSeparator());
                perfDropDown.DropDownItems.Add(clearMetricsItem);
                
                // Add to toolbar
                toolStrip1.Items.Add(new ToolStripSeparator());
                toolStrip1.Items.Add(perfDropDown);
            }
            catch 
            {
                // Ignore menu setup errors
            }
        }
        
        /// <summary>
        /// REMOVED: Custom tab drawing was causing performance issues
        /// Using built-in Windows tab styling for better speed
        /// </summary>
        private void TabControl1_DrawItem_DISABLED(object sender, DrawItemEventArgs e)
        {
            // This method is disabled for performance optimization
            // The application now uses native Windows tab rendering
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
            // START PERFORMANCE MEASUREMENT
            PerformanceLogger.StartTimer("Search_Operation", $"Interface: {tb_Interface2search.Text}");
            
            // RESOURCE PROTECTION: Check if we're approaching resource limits
            // (Skip this check if we're in a batch operation and already warned)
            if (tabControl1.TabPages.Count >= 58 && !isBatchOperation) // Warning before hitting limit
            {
                DialogResult result = MessageBox.Show(
                    $"You have {tabControl1.TabPages.Count} tabs open. Continuing may cause performance issues or crashes.\n\n" +
                    "Recommended: Close some tabs first.\n\n" +
                    "Do you want to continue anyway?",
                    "Resource Warning", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Warning);
                
                if (result == DialogResult.No)
                {
                    PerformanceLogger.StopTimer("Search_Operation", "Cancelled by user due to resource warning");
                    return; // User chose to stop
                }
            }

            // MEMORY CLEANUP: Force cleanup before heavy operations
            if (tabControl1.TabPages.Count > 50)
            {
                PerformanceLogger.StartTimer("Memory_Cleanup");
                CleanupResourcesIfNeeded();
                PerformanceLogger.StopTimer("Memory_Cleanup");
            }

            toolStripProgressBar1.Value = 0;
            
            // Show modern animated progress
            ShowModernProgress(true, "Preparing search...");
            
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
                        PerformanceLogger.StartTimer("Internal_UC_Check");
                        internalUC.__checkVariable(tb_Interface2search.Text);
                        PerformanceLogger.StopTimer("Internal_UC_Check");
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
                PerformanceLogger.StartTimer("Excel_Parser_Creation");
                UC_ContextClearing UC_doing = new UC_ContextClearing();
                foreach (Control c in tabControl1.SelectedTab.Controls)
                {
                    if (c.GetType().Name == "UC_ContextClearing")
                    {
                        UC_doing = (UC_ContextClearing)c;
                        
                        // RESOURCE MONITORING: Check before creating multiple Excel_Parser instances
                        if (tabControl1.TabPages.Count >= 58)
                        {
                            this.Text = $"Context Clearing - HIGH RESOURCE USAGE ({tabControl1.TabPages.Count} tabs)";
                        }
                        
                        UC_doing.NewCarasi = new Excel_Parser(nameOfnewCarasi, dt_template);
                        UC_doing.OldCarasi = new Excel_Parser(nameOfoldCarasi, dt_template);
                        UC_doing.NewDF = new Excel_Parser(nameOfnewDataflow, dt_template);
                        UC_doing.OldDF = new Excel_Parser(nameOfoldDataflow, dt_template);
                        PerformanceLogger.StopTimer("Excel_Parser_Creation");

                        UC_doing.Link2Folder = link2Folder;
                        if (tb_Interface2search.Text != "")
                        {
                            PerformanceLogger.StartTimer("Variable_Check", $"Searching: {tb_Interface2search.Text}");
                            UC_doing.__checkVariable(ref toolStripProgressBar1, tb_Interface2search.Text);
                            PerformanceLogger.StopTimer("Variable_Check");
                            
                            // UPDATE STATUS: Refresh status after search to show cache/pool activity
                            UpdateTabMemoryStatus();
                            
                            /* Check and update Macro Module searching Feature */
                            if(_mmCheck != null && _mmCheck.IsValidLink)
                            {
                                PerformanceLogger.StartTimer("MM_Check");
                                string[] result = new string[150];
                                bool a = _mmCheck.IsExistInMM(tb_Interface2search.Text, ref result);
                                UC_doing._setValueMM(a, result);
                                PerformanceLogger.StopTimer("MM_Check");
                            }

                            /* Check and update A2L searching Feature */
                            if (_a2lCheck != null && _a2lCheck.IsValidLink)
                            {
                                PerformanceLogger.StartTimer("A2L_Check");
                                string[] result = new string[150];
                                bool a = _a2lCheck.IsExistInA2L(tb_Interface2search.Text, ref result);
                                UC_doing._setValueA2L(a, result);
                                PerformanceLogger.StopTimer("A2L_Check");
                            }

                            tabControl1.SelectedTab.Text = tb_Interface2search.Text;
                            
                            // Hide progress animation and show completion
                            ShowModernProgress(false);
                        }
                        else
                        {
                            ShowModernProgress(false);
                            MessageBox.Show("Please insert the Interface Name", "Warning!");
                        }

                        //Disposed all Excel Parser
                        PerformanceLogger.StartTimer("Excel_Parser_Disposal");
                        UC_doing.NewCarasi.Dispose();
                        UC_doing.OldCarasi.Dispose();
                        UC_doing.NewDF.Dispose();
                        UC_doing.OldDF.Dispose();
                        PerformanceLogger.StopTimer("Excel_Parser_Disposal");
                    }
                }
            }
            
            // COMPLETE PERFORMANCE MEASUREMENT
            long totalTime = PerformanceLogger.StopTimer("Search_Operation", $"Completed for: {tb_Interface2search.Text}");
            
            // Update title with performance info
            this.Text = $"Context Clearing - Last search: {totalTime}ms";
        }

        private void btn_toolStrip_NewTab_Click(object sender, EventArgs e)
        {
            PerformanceLogger.StartTimer("Create_New_Tab");
            try
            {
                // TAB LIMIT: Prevent creating more than 60 tabs to avoid crash
                const int MAX_TABS = 60;
                if (tabControl1.TabPages.Count >= MAX_TABS)
                {
                    MessageBox.Show($"Maximum {MAX_TABS} tabs reached. Please close some tabs before creating new ones.", 
                                   "Tab Limit Reached", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    PerformanceLogger.StopTimer("Create_New_Tab", "Failed - Max tabs reached");
                    return;
                }

                if (tabControl1.TabPages.Count > 0)
                {
                    TabPage newTab = new TabPage("TabPage" + (tabControl1.TabPages.Count + 1).ToString());
                    UC_ContextClearing newUC = new UC_ContextClearing();
                    newUC.Dock = DockStyle.Fill;
                    newTab.Controls.Add(newUC);
                    tabControl1.TabPages.Add(newTab);
                    tabControl1.SelectedTab = newTab;
                    
                    // Update status after creating tab
                    UpdateTabMemoryStatus();
                }
                else
                {
                    TabPage newTab = new TabPage("TabPage1");
                    UC_ContextClearing newUC = new UC_ContextClearing();
                    newUC.Dock = DockStyle.Fill;
                    newTab.Controls.Add(newUC);
                    tabControl1.TabPages.Add(newTab);
                    tabControl1.SelectedTab = newTab;
                    
                    // Update status after creating tab
                    UpdateTabMemoryStatus();
                }
                
                long creationTime = PerformanceLogger.StopTimer("Create_New_Tab", $"Success - Total tabs: {tabControl1.TabPages.Count}");
                
                // Log if tab creation is getting slow
                if (creationTime > 100)
                {
                    PerformanceLogger.LogDuration("Slow_Tab_Creation", creationTime, $"Tab creation took {creationTime}ms");
                }
            }
            catch (System.Exception ex)
            {
                PerformanceLogger.StopTimer("Create_New_Tab", $"Exception: {ex.Message}");
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
                    
                    // MEMORY CLEANUP: Clean resources after closing tabs
                    CleanupResourcesIfNeeded();
                    
                    // UPDATE STATUS: Refresh status display after tab removal
                    UpdateTabMemoryStatus();
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
            var mailItem = outlookApp.CreateItem(0); // OlItemType.olMailItem = 0

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
            mailItem.Importance = 2; // OlImportance.olImportanceHigh = 2
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
            
            // SET BATCH FLAG: Indicate we're in batch operation
            isBatchOperation = true;

            for (int i = 0; i < listOfInterfaces.Length - 1; i++)
            {
                // RESOURCE PROTECTION: Check tab limits BEFORE creating new tab
                const int MAX_TABS = 60;
                if (tabControl1.TabPages.Count >= MAX_TABS)
                {
                    MessageBox.Show($"Maximum {MAX_TABS} tabs reached during batch search.\n" +
                                   $"Processed {i} interfaces out of {listOfInterfaces.Length - 1}.\n" +
                                   "Please close some tabs and try again.", 
                                   "Tab Limit Reached", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break; // Stop the batch processing
                }

                // Additional warning before approaching limit
                if (tabControl1.TabPages.Count >= 58)
                {
                    DialogResult result = MessageBox.Show(
                        $"You have {tabControl1.TabPages.Count} tabs open. Continuing batch search may cause issues.\n\n" +
                        $"Processed {i} interfaces out of {listOfInterfaces.Length - 1}.\n\n" +
                        "Do you want to continue the batch search?",
                        "Resource Warning", 
                        MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Warning);
                    
                    if (result == DialogResult.No)
                    {
                        break; // Stop the batch processing
                    }
                }

                btn_toolStrip_NewTab.PerformClick();
                tb_Interface2search.Text = listOfInterfaces[i].Trim(); // Trim whitespace
                btn_Run.PerformClick();
                
                // UPDATE STATUS: Refresh status after each search to show cache/pool changes
                UpdateTabMemoryStatus();
                
                // Brief pause to see the updates
                System.Windows.Forms.Application.DoEvents();
            }
            
            // RESET BATCH FLAG: Batch operation completed
            isBatchOperation = false;
            
            // FINAL STATUS UPDATE: Ensure final status is displayed
            UpdateTabMemoryStatus();
        }

        /// <summary>
        /// Performance optimized virtual tab rendering - Only render visible tabs
        /// This significantly improves performance when dealing with 50+ tabs
        /// </summary>
        private void OptimizeTabPerformance()
        {
            // Only render visible tabs to improve performance
            tabControl1.SuspendLayout();
            
            foreach (TabPage tab in tabControl1.TabPages)
            {
                if (tab != tabControl1.SelectedTab)
                {
                    // Suspend non-visible tab controls to save memory
                    foreach (Control control in tab.Controls)
                    {
                        control.SuspendLayout();
                    }
                }
                else
                {
                    // Resume layout for active tab
                    foreach (Control control in tab.Controls)
                    {
                        control.ResumeLayout();
                    }
                }
            }
            
            tabControl1.ResumeLayout();
        }
        
        /// <summary>
        /// Smart tab switching with animation and performance optimization
        /// </summary>
        private void OnTabSelectionChanged(object sender, EventArgs e)
        {
            PerformanceLogger.StartTimer("Tab_Switch");
            
            // Optimize performance by only rendering visible tab
            OptimizeTabPerformance();
            
            // Update status when tab changes
            UpdateTabMemoryStatus();
            
            long switchTime = PerformanceLogger.StopTimer("Tab_Switch", $"Switched to tab: {tabControl1.SelectedTab?.Text}");
            
            // Log if tab switching is getting slow
            if (switchTime > 50)
            {
                PerformanceLogger.LogDuration("Slow_Tab_Switch", switchTime, $"Tab switch took {switchTime}ms with {tabControl1.TabPages.Count} tabs");
            }
        }

        // MEMORY CLEANUP: Clean up resources when reaching limits
        private void CleanupResourcesIfNeeded()
        {
            if (tabControl1.TabPages.Count > 50)
            {
                try
                {
                    // Force garbage collection
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    
                    // Clear query cache to free memory
                    Excel_Parser.ClearQueryCache();
                    
                    // Clear connection pool
                    Lib_OLEDB_Excel.CleanupConnectionPool();
                }
                catch { /* Ignore cleanup errors */ }
            }
        }

        // STATUS UPDATE: Update tab and memory status
        private void UpdateTabMemoryStatus()
        {
            try
            {
                int tabCount = tabControl1.TabPages.Count;
                int cacheSize = Excel_Parser.GetCacheSize();
                int poolSize = Lib_OLEDB_Excel.GetPoolSize();
                
                // Update ToolStrip status labels in menubar
                toolStripLabelTabCount.Text = $"Tabs: {tabCount}/60";
                toolStripLabelCache.Text = $"Cache: {cacheSize}/50";
                toolStripLabelPool.Text = $"Pool: {poolSize}/10";
                
                // Update memory status with color coding
                if (tabCount > 55)
                {
                    toolStripLabelTabCount.ForeColor = System.Drawing.Color.Red;
                    toolStripLabelMemory.Text = "Memory: HIGH";
                    toolStripLabelMemory.ForeColor = System.Drawing.Color.Red;
                }
                else if (tabCount > 45)
                {
                    toolStripLabelTabCount.ForeColor = System.Drawing.Color.Orange;
                    toolStripLabelMemory.Text = "Memory: WARN";
                    toolStripLabelMemory.ForeColor = System.Drawing.Color.Orange;
                }
                else
                {
                    toolStripLabelTabCount.ForeColor = System.Drawing.Color.Black;
                    toolStripLabelMemory.Text = "Memory: OK";
                    toolStripLabelMemory.ForeColor = System.Drawing.Color.Green;
                }
                
                // Color coding for cache and pool warnings
                if (cacheSize > 40)
                {
                    toolStripLabelCache.ForeColor = System.Drawing.Color.Orange;
                }
                else
                {
                    toolStripLabelCache.ForeColor = System.Drawing.Color.Black;
                }
                
                if (poolSize > 8)
                {
                    toolStripLabelPool.ForeColor = System.Drawing.Color.Orange;
                }
                else
                {
                    toolStripLabelPool.ForeColor = System.Drawing.Color.Black;
                }
                
                // Keep simple version info in version label (no color changes)
                lb_version.Text = VersionLabel;
                lb_version.ForeColor = System.Drawing.Color.Black; // Always keep version label black
            }
            catch { /* Ignore status update errors */ }
        }

        /// <summary>
        /// Setup modern button hover effects and animations
        /// </summary>
        private void SetupButtonHoverEffects()
        {
            // Setup hover effects for Browse button
            btn_Link2Folder.MouseEnter += (s, e) => {
                btn_Link2Folder.BackColor = System.Drawing.Color.FromArgb(102, 187, 106);
                btn_Link2Folder.Cursor = Cursors.Hand;
            };
            btn_Link2Folder.MouseLeave += (s, e) => {
                btn_Link2Folder.BackColor = System.Drawing.Color.FromArgb(76, 175, 80);
                btn_Link2Folder.Cursor = Cursors.Default;
            };
            
            // Setup hover effects for Search button
            btn_Run.MouseEnter += (s, e) => {
                btn_Run.BackColor = System.Drawing.Color.FromArgb(66, 165, 245);
                btn_Run.Cursor = Cursors.Hand;
            };
            btn_Run.MouseLeave += (s, e) => {
                btn_Run.BackColor = System.Drawing.Color.FromArgb(33, 150, 243);
                btn_Run.Cursor = Cursors.Default;
            };
            
            // Setup modern input field focus effects
            tb_Link2Folder.Enter += (s, e) => {
                tb_Link2Folder.BackColor = System.Drawing.Color.FromArgb(248, 248, 248);
            };
            tb_Link2Folder.Leave += (s, e) => {
                tb_Link2Folder.BackColor = System.Drawing.Color.White;
            };
            
            tb_Interface2search.Enter += (s, e) => {
                tb_Interface2search.BackColor = System.Drawing.Color.FromArgb(248, 248, 248);
            };
            tb_Interface2search.Leave += (s, e) => {
                tb_Interface2search.BackColor = System.Drawing.Color.White;
            };
        }

        /// <summary>
        /// Setup modern progress bar with smooth animations
        /// </summary>
        private void SetupModernProgressBar()
        {
            // Style the progress bar with modern colors
            toolStripProgressBar1.Style = ProgressBarStyle.Continuous;
            toolStripProgressBar1.MarqueeAnimationSpeed = 30;
        }
        
        /// <summary>
        /// Show animated progress with modern styling
        /// </summary>
        private void ShowModernProgress(bool isActive, string message = "")
        {
            if (isActive)
            {
                toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
                toolStripProgressBar1.MarqueeAnimationSpeed = 30;
                
                // Update status message with emoji
                if (!string.IsNullOrEmpty(message))
                {
                    toolStripLabelMemory.Text = $"⚡ {message}";
                    toolStripLabelMemory.ForeColor = System.Drawing.Color.FromArgb(255, 152, 0);
                }
            }
            else
            {
                toolStripProgressBar1.Style = ProgressBarStyle.Continuous;
                toolStripProgressBar1.Value = 0;
                
                // Reset status
                UpdateTabMemoryStatus();
            }
            
            System.Windows.Forms.Application.DoEvents();
        }

        /// <summary>
        /// Apply lightweight modern visual effects (optimized for performance)
        /// </summary>
        private void ApplyLightweightVisualEffects()
        {
            // Keep essential styling only
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = System.Drawing.Color.FromArgb(250, 250, 250);
            
            // REMOVED: Heavy custom painting for performance
            // Only keep essential double buffering
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
        }
        
        /// <summary>
        /// Apply consistent modern fonts throughout the application
        /// </summary>
        private void ApplyModernFonts()
        {
            var modernFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            var modernBoldFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            
            // Apply to main controls
            foreach (Control control in this.Controls)
            {
                ApplyFontRecursively(control, modernFont);
            }
            
            // Special styling for labels
            lb_link2Folder.Font = modernBoldFont;
            lb_NameOfInterface.Font = modernBoldFont;
            lb_version.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular);
        }
        
        /// <summary>
        /// Recursively apply font to all child controls
        /// </summary>
        private void ApplyFontRecursively(Control parent, System.Drawing.Font font)
        {
            try
            {
                parent.Font = font;
                foreach (Control child in parent.Controls)
                {
                    ApplyFontRecursively(child, font);
                }
            }
            catch 
            {
                // Ignore font application errors for some controls
            }
        }

        /// <summary>
        /// Show Performance Report event handler
        /// </summary>
        private void ShowPerformanceReport_Click(object sender, EventArgs e)
        {
            try
            {
                string report = PerformanceLogger.GetPerformanceSummary();
                
                // Create a form to display the report
                Form reportForm = new Form
                {
                    Text = "🚀 Performance Analysis Report",
                    Size = new Size(800, 600),
                    StartPosition = FormStartPosition.CenterParent,
                    MinimizeBox = false,
                    MaximizeBox = true,
                    Font = new Font("Segoe UI", 9F)
                };
                
                RichTextBox reportTextBox = new RichTextBox
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    Text = report,
                    Font = new Font("Consolas", 9F),
                    BackColor = Color.FromArgb(248, 248, 248)
                };
                
                Button saveButton = new Button
                {
                    Text = "💾 Save Report",
                    Dock = DockStyle.Bottom,
                    Height = 35,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(33, 150, 243),
                    ForeColor = Color.White
                };
                
                saveButton.Click += (s, args) =>
                {
                    SaveFileDialog saveDialog = new SaveFileDialog
                    {
                        Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                        DefaultExt = "txt",
                        FileName = $"Performance_Report_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                    };
                    
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(saveDialog.FileName, report);
                        MessageBox.Show("Performance report saved successfully!", "Report Saved", 
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                };
                
                reportForm.Controls.Add(reportTextBox);
                reportForm.Controls.Add(saveButton);
                reportForm.ShowDialog(this);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error showing performance report: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Open Log File event handler
        /// </summary>
        private void OpenLogFile_Click(object sender, EventArgs e)
        {
            try
            {
                string logFilePath = PerformanceLogger.GetLogFilePath();
                
                if (File.Exists(logFilePath))
                {
                    // Try to open with default CSV application (Excel, etc.)
                    System.Diagnostics.Process.Start(logFilePath);
                }
                else
                {
                    MessageBox.Show("Log file not found. No performance data has been recorded yet.", 
                                  "Log File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error opening log file: {ex.Message}\n\nLog path: {PerformanceLogger.GetLogFilePath()}", 
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Clear Metrics event handler
        /// </summary>
        private void ClearMetrics_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to clear all performance metrics?\n\nThis action cannot be undone.",
                    "Clear Performance Metrics",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    PerformanceLogger.ClearMetrics();
                    MessageBox.Show("Performance metrics cleared successfully!", "Metrics Cleared",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error clearing metrics: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
    
    /// <summary>
    /// Modern ToolStrip Renderer for professional appearance
    /// </summary>
    public class ModernToolStripRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            // Create gradient background for toolbar
            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                e.AffectedBounds,
                System.Drawing.Color.FromArgb(250, 250, 250),
                System.Drawing.Color.FromArgb(235, 235, 235),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }
        }
        
        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Selected || e.Item.Pressed)
            {
                using (var brush = new System.Drawing.SolidBrush(
                    System.Drawing.Color.FromArgb(230, 230, 230)))
                {
                    e.Graphics.FillRectangle(brush, new Rectangle(Point.Empty, e.Item.Size));
                }
            }
        }
        
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = System.Drawing.Color.FromArgb(66, 66, 66);
            base.OnRenderItemText(e);
        }
    }
}

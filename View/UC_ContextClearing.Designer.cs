
namespace Check_carasi_DF_ContextClearing
{
    partial class UC_ContextClearing
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.UC_Newcarasi = new Check_carasi_DF_ContextClearing.UC_Carasi();
            this.UC_OldCarasi = new Check_carasi_DF_ContextClearing.UC_Carasi();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.UC_NewDF = new Check_carasi_DF_ContextClearing.UC_dataflow();
            this.UC_OldDF = new Check_carasi_DF_ContextClearing.UC_dataflow();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.tb_MM_Infor = new System.Windows.Forms.RichTextBox();
            this.tb_A2L_Infor = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.splitContainer1, 3);
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 52);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Size = new System.Drawing.Size(872, 517);
            this.splitContainer1.SplitterDistance = 258;
            this.splitContainer1.TabIndex = 2;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.UC_Newcarasi);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.UC_OldCarasi);
            this.splitContainer2.Size = new System.Drawing.Size(872, 258);
            this.splitContainer2.SplitterDistance = 436;
            this.splitContainer2.TabIndex = 0;
            // 
            // UC_Newcarasi
            // 
            this.UC_Newcarasi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UC_Newcarasi.Location = new System.Drawing.Point(0, 0);
            this.UC_Newcarasi.Name = "UC_Newcarasi";
            this.UC_Newcarasi.Size = new System.Drawing.Size(436, 258);
            this.UC_Newcarasi.TabIndex = 8;
            // 
            // UC_OldCarasi
            // 
            this.UC_OldCarasi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UC_OldCarasi.Location = new System.Drawing.Point(0, 0);
            this.UC_OldCarasi.Name = "UC_OldCarasi";
            this.UC_OldCarasi.Size = new System.Drawing.Size(432, 258);
            this.UC_OldCarasi.TabIndex = 9;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.UC_NewDF);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.UC_OldDF);
            this.splitContainer3.Size = new System.Drawing.Size(872, 259);
            this.splitContainer3.SplitterDistance = 436;
            this.splitContainer3.TabIndex = 0;
            // 
            // UC_NewDF
            // 
            this.UC_NewDF.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UC_NewDF.Dt_temp = null;
            this.UC_NewDF.Flag_IsInternal = false;
            this.UC_NewDF.Link_Of_DF_file = "";
            this.UC_NewDF.Location = new System.Drawing.Point(0, 0);
            this.UC_NewDF.Name = "UC_NewDF";
            this.UC_NewDF.Size = new System.Drawing.Size(436, 259);
            this.UC_NewDF.TabIndex = 15;
            // 
            // UC_OldDF
            // 
            this.UC_OldDF.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UC_OldDF.Dt_temp = null;
            this.UC_OldDF.Flag_IsInternal = false;
            this.UC_OldDF.Link_Of_DF_file = "";
            this.UC_OldDF.Location = new System.Drawing.Point(0, 0);
            this.UC_OldDF.Name = "UC_OldDF";
            this.UC_OldDF.Size = new System.Drawing.Size(432, 259);
            this.UC_OldDF.TabIndex = 16;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 255F));
            this.tableLayoutPanel1.Controls.Add(this.splitContainer1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tb_MM_Infor, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tb_A2L_Infor, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.566434F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 91.43356F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(878, 572);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(144, 49);
            this.label1.TabIndex = 4;
            this.label1.Text = "MM Info:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tb_MM_Infor
            // 
            this.tb_MM_Infor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tb_MM_Infor.Location = new System.Drawing.Point(153, 3);
            this.tb_MM_Infor.Name = "tb_MM_Infor";
            this.tb_MM_Infor.ReadOnly = true;
            this.tb_MM_Infor.Size = new System.Drawing.Size(467, 43);
            this.tb_MM_Infor.TabIndex = 3;
            this.tb_MM_Infor.Text = "";
            // 
            // tb_A2L_Infor
            // 
            this.tb_A2L_Infor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tb_A2L_Infor.Location = new System.Drawing.Point(626, 3);
            this.tb_A2L_Infor.Name = "tb_A2L_Infor";
            this.tb_A2L_Infor.Size = new System.Drawing.Size(249, 43);
            this.tb_A2L_Infor.TabIndex = 5;
            this.tb_A2L_Infor.Text = "";
            // 
            // UC_ContextClearing
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "UC_ContextClearing";
            this.Size = new System.Drawing.Size(878, 572);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private UC_Carasi UC_Newcarasi;
        private UC_Carasi UC_OldCarasi;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private UC_dataflow UC_NewDF;
        private UC_dataflow UC_OldDF;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.RichTextBox tb_MM_Infor;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox tb_A2L_Infor;
    }
}

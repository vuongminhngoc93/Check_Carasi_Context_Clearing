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
    public partial class PopUp_ProjectInfo : Form
    {
        string _PrjName;
        string _PverName;
        string _DD_Deadline;

        public PopUp_ProjectInfo()
        {
            InitializeComponent();
        }

        public PopUp_ProjectInfo(ref string _PrjName, ref string _PverName, ref string _DD_Deadline)
        {
            InitializeComponent();
            _PrjName = this._PrjName;
            _PverName = this._PverName;
            _DD_Deadline = this._DD_Deadline;
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            _DD_Deadline = tb_DD_Date.Text;
            _PverName = tb_PverRQ1.Text;
            _PrjName = tb_PrjName.Text;
            this.Close();
        }
    }
}

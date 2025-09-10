using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Check_carasi_DF_ContextClearing
{
    class A2L_Check
    {
        private string link_Of_A2L = string.Empty;
        private bool isValidLink = true;

        public string Link_Of_A2L { get => link_Of_A2L; set { link_Of_A2L = value; A2L_setup(); } }
        public bool IsValidLink { get => isValidLink; set => isValidLink = value; }

        public A2L_Check()
        {
            link_Of_A2L = string.Empty;
            isValidLink = true;
        }
        
        private void A2L_setup ()
        {

        }

        public bool IsExistInA2L(string keyword, ref string[] result)
        {
            bool _isContain = false;
            int index = 0;



            return _isContain;
        }
    }
}

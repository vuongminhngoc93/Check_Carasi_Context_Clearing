using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Check_carasi_DF_ContextClearing
{
    public class A2L_Check
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
            // Validate A2L file path
            if (string.IsNullOrEmpty(link_Of_A2L))
            {
                isValidLink = false;
                return;
            }
            
            // Check if file exists and has correct extension
            if (File.Exists(link_Of_A2L) && Path.GetExtension(link_Of_A2L).ToLower() == ".a2l")
            {
                isValidLink = true;
            }
            else
            {
                isValidLink = false;
            }
        }

        public bool IsExistInA2L(string keyword, ref string[] result)
        {
            bool _isContain = false;
            
            // Validate inputs
            if (string.IsNullOrEmpty(keyword))
            {
                result = new string[0];
                return false;
            }
            
            if (string.IsNullOrEmpty(link_Of_A2L) || !File.Exists(link_Of_A2L))
            {
                result = new string[0];
                isValidLink = false;
                return false;
            }
            
            try
            {
                // Read A2L file and search for keyword
                string[] lines = File.ReadAllLines(link_Of_A2L);
                List<string> matchedLines = new List<string>();
                
                foreach (string line in lines)
                {
                    if (line.Contains(keyword))
                    {
                        matchedLines.Add(line.Trim());
                        _isContain = true;
                    }
                }
                
                result = matchedLines.ToArray();
                isValidLink = true;
                return _isContain;
            }
            catch (Exception)
            {
                // Handle file reading errors
                result = new string[0];
                isValidLink = false;
                return false;
            }
        }
    }
}

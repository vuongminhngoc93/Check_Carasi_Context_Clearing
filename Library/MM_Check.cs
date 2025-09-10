using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Check_carasi_DF_ContextClearing
{
    public class MM_Check
    {
        private string link_Of_MM = string.Empty;
        private string link_Of_ARXML = string.Empty;
        private bool isValidLink = false;

        public string Link_Of_MM { get => link_Of_MM; set { link_Of_MM = value; isValid(); } }
        public bool IsValidLink { get => isValidLink; set => isValidLink = value; }
        public string Link_Of_ARXML { get => link_Of_ARXML; set => link_Of_ARXML = value; }

        public MM_Check()
        {
            link_Of_MM = string.Empty;
            link_Of_ARXML = string.Empty;
            isValidLink = false;
        }

        public static List<string> GetDirectories(string path, string searchPattern = "*",  SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
                return Directory.GetDirectories(path, searchPattern).ToList();

            var directories = new List<string>(GetDirectories(path, searchPattern));

            for (var i = 0; i < directories.Count; i++)
                directories.AddRange(GetDirectories(directories[i], searchPattern));

            return directories;
        }

        private static List<string> GetDirectories(string path, string searchPattern)
        {
            try
            {
                return Directory.GetDirectories(path, searchPattern).ToList();
            }
            catch (UnauthorizedAccessException)
            {
                return new List<string>();
            }
        }

        private void isValid()
        {
            // Reset validation state
            isValidLink = false;
            
            // Check if link_Of_MM is valid path
            if (string.IsNullOrEmpty(link_Of_MM) || !Directory.Exists(link_Of_MM))
            {
                return; // Early exit if path is invalid
            }

            try
            {
                var a = GetDirectories(link_Of_MM, "arxml");
                if (a.Count > 0)
                {
                    
                    string[] files = Directory.GetFiles(a[0].ToString(), "*.arxml");

                    foreach (string file in files)
                    {
                        if (file.EndsWith("_Extern.arxml"))
                        {
                            isValidLink = true;
                            link_Of_ARXML = file;
                            break;
                        }
                        else { }
                            //MessageBox.Show("There is an 'arxml' folder inside! But no Extern Arxml file!");
                    }
                }
                else
                {
                    isValidLink = false;
                    //MessageBox.Show("There is an 'arxml' folder inside! But no Extern Arxml file!");
                }
            }
            catch (Exception)
            {
                // Handle any file system exceptions gracefully
                isValidLink = false;
            }
        }

        private void isValid_New()
        {
            var a = GetDirectories(link_Of_MM, "arxml");
            if (a.Count > 0)
            {

                string[] files = Directory.GetFiles(a[0].ToString(), "*.arxml");

                foreach (string file in files)
                {
                    if (file.EndsWith(".arxml"))
                    {
                        isValidLink = true;
                        link_Of_ARXML = file;
                        break;
                    }
                    else { }
                    //MessageBox.Show("There is an 'arxml' folder inside! But no Extern Arxml file!");
                }
            }
            else
            {
                isValidLink = false;
                MessageBox.Show("There is an 'arxml' folder inside! But no Extern Arxml file!");
            }
        }

        public bool IsExistInMM(string keyword, ref string[] result)
        {
            bool _isContain = false;
            int index = 0;

            using (FileStream inFile = new FileStream(link_Of_ARXML, FileMode.Open, FileAccess.Read))
            using (var sr = new StreamReader(inFile))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (String.IsNullOrEmpty(line)) continue;
                    if (line.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        result[index++] = line;
                        _isContain = true;
                    }
                }
            }

            return _isContain;
        }

        public bool IsExistInMM_Intern(string keyword, ref string[] result)
        {
            bool _isContain = false;
            int index = 0;

            using (FileStream inFile = new FileStream(link_Of_ARXML, FileMode.Open, FileAccess.Read))
            using (var sr = new StreamReader(inFile))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (String.IsNullOrEmpty(line)) continue;
                    if (line.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        result[index++] = line;
                        _isContain = true;
                    }
                }
            }

            return _isContain;
        }

    }
}

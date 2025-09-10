using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Check_carasi_DF_ContextClearing
{
    class Review_IM_change
    {
        const int MAX_NO_OF_REVIEW_FILES = 20;

        public Review_IM_change(string linkOffolderOfReviews)
        {
            AllData = new DataTable[MAX_NO_OF_REVIEW_FILES];
            if (verify_link(linkOffolderOfReviews))
            {
                this.linkOfFolder = linkOffolderOfReviews;
                getAll();
            }
            else
                MessageBox.Show("There is no valid Files in the link!", "Warning!");
        }

        private bool verify_link(string linkOffolderOfReviews)
        {
            bool isValid = false;
            string[] files = System.IO.Directory.GetFiles(linkOffolderOfReviews);
            foreach (var file in files)
            {
                if ((file.ToLower().EndsWith(".xls") || file.ToLower().EndsWith(".xlsx")))
                {
                    Lib_OLEDB_Excel __excel = new Lib_OLEDB_Excel(file);
                    if (__excel.ReadTable("Mapping$") != null)
                    {
                        isValid = true;
                        break;
                    }
                    else { }
                }
            }
            return isValid;
        }

        private void getAll()
        {
            string[] files = System.IO.Directory.GetFiles(linkOfFolder);
            int index = 0;
            foreach (var file in files)
            {
                if ((file.ToLower().EndsWith(".xls") || file.ToLower().EndsWith(".xlsx")))
                {
                    Lib_OLEDB_Excel __excel = new Lib_OLEDB_Excel(file);
                    if (__excel.ReadTable("Mapping$") != null)
                    {
                        AllData[index] = __excel.ReadTable("Mapping$");
                        AllData[index].TableName = file.Split('\\').Last();

                        int indexColumn = 0;
                        foreach (DataColumn column in AllData[index].Columns)
                        {
                            string cName = AllData[index].Rows[1][column.ColumnName].ToString();
                            if (cName != "")
                            {
                                if (AllData[index].Columns.Contains(cName))
                                {
                                    column.ColumnName = cName + " (" + (++indexColumn).ToString() + ")";
                                }
                                else
                                    column.ColumnName = cName;
                            }
                        }
                        DataRow dr = AllData[index].Rows[0];
                        if (dr != null) dr.Delete();
                        DataRow dr1 = AllData[index].Rows[1];
                        if (dr1 != null) dr1.Delete();
                        AllData[index].AcceptChanges();

                        index++;
                    }
                    else { }
                }
            }
        }

        public string linkOfFolder { get; private set; }
        public DataTable[] AllData { get; private set; }
    }
}

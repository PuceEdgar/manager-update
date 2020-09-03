using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace MNGUPDATEPRD
{
    class company
    {
        DataTable t;

        static DataTable noneExistingCompany = GetEmptyTable();

        public company()
        {
            SqlConnection dbConnection;
            SqlDataAdapter dataAdapter;
            String serviceName = "company";

            //string c = "Integrated Security=True;Data Source=" + "WSP2971A" + ";Initial Catalog=" + "ORSXML";
            string c = ConfigurationSettings.AppSettings["OrmisDBConnect"];

            try
            {
                dbConnection = new SqlConnection(c);
                dbConnection.Open();
                DataSet companyDs = new DataSet();

                dataAdapter = new SqlDataAdapter("SELECT [CompanyId] as hrId,[Old_Company_Code] as financeId, useteam FROM [OrsXml].[dbo].[LOCALID_COMPANIES_GH]", dbConnection);
                t = new DataTable();

                dataAdapter.Fill(t);

            }
            catch (Exception ex)
            {
                //myLog.Write(ex.Message, "HR2ORM", LogTypes.ErrorMessage, "HR2ORM Error", serviceName);
                throw;
            }
        }

        // Convert HR Online company id to finance Id

        public String hr2finance(String hrId)
        {
            String serviceName = "hr2finance";

            try
            {
                DataRow[] r = t.Select("HrId = " + hrId);

                DataRow row = r[0];

                return row[1].ToString();
            }
            catch (Exception e)
            {
                Remember(hrId);
                return (hrId);
            }
        }

        // Return if team level should be used for this company

        public String useTeam(String hrId)
        {
            String serviceName = "useTeam";

            try
            {
                DataRow[] r = t.Select("HrId = " + hrId);

                DataRow row = r[0];

                return row[2].ToString();
            }
            catch (Exception e)
            {
                return ("N");
            }
        }


        void Remember(String hrId)
        {
            String serviceName = "Remember";

            try
            {
                DataRow[] r = noneExistingCompany.Select("CompanyId = " + hrId);

                if (r.Length > 0)
                {
                    r[0]["Counter"] = (int)r[0]["Counter"] + 1;
                }
                else
                {
                    noneExistingCompany.Rows.Add(hrId, 1);
                }
            }
            catch (Exception e)
            {
                //myLog.Write(e.Message, "HR2ORM", LogTypes.InfoMessage, "HR2ORM Error", serviceName);
                throw;
            }
        }

        static DataTable GetEmptyTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("CompanyId", typeof(string));
            table.Columns.Add("Counter", typeof(int));
            return table;
        }

    }
}

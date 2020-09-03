using MNGUPDATEPRD.Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace MNGUPDATEPRD
{
    public static class DBLogic
    { 
        private static readonly string file = ConfigurationManager.AppSettings["OutputFilename"];

        public static void ExtractData()
        {
            Logger.LogEvent("**************** STARTING MANAGER UPDATE ****************");
            try
            {                
                using (var sqlConnection = SqlDBUtil.GetConnection())
                {
                    using (var oracleConnection = OracleDBUtil.GetConnection())
                    {  
                        Logger.LogEvent(" Trying to connect to server..");
                        sqlConnection.Open();
                        Logger.LogEvent(" Connected to server");
                        
                        SqlDBUtil.TruncateTable(sqlConnection);                        
                        SqlDBUtil.ExecuteInsert(sqlConnection, oracleConnection);
                        
                        Logger.LogEvent(" Extract and import part is DONE!");
                        Logger.WriteLogToFile();
                    }
                }
               
            }
            catch (OracleException oraEx)
            {
                EmailUtility.SendEmail("ORACLE EXCEPTION: " + "\r\n" + oraEx.Message, "Import Failed!");
                Logger.LogException(oraEx.Message);
                Logger.WriteLogToFile();
                throw;
            }
            catch (Exception ex)
            {
                EmailUtility.SendEmail("Other EXCEPTION while retrieving data from Oracle db: " + "\r\n" + ex.Message, "Import Failed!");
                Logger.LogException(ex.Message);
                Logger.WriteLogToFile();
                throw;
            }
        }

        

        public static XmlDocument CreateXmlDocument()
        {
            var xmlDoc = new XmlDocument();
            try
            {
                using (var sqlConnection = SqlDBUtil.GetConnection())
                {
                    sqlConnection.Open();
                    var sqlcmd = new SqlCommand("[maint].[__UpdateManagers]", sqlConnection);
                    sqlcmd.CommandTimeout = 240;
                    sqlcmd.CommandType = System.Data.CommandType.StoredProcedure;

                    Logger.LogEvent(" Executing stored procedure...");
                    using (XmlReader reader = sqlcmd.ExecuteXmlReader())
                    {
                       

                        while (reader.Read())
                        {
                            xmlDoc.Load(reader);
                        }
                        xmlDoc.Save(file);

                        XmlDeclaration xmldecl;
                        xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);

                        XmlElement root = xmlDoc.DocumentElement;
                        xmlDoc.InsertBefore(xmldecl, root);

                        Logger.LogEvent(" File created!");
                        Logger.Log.Append(DateUtility.getTime() + " Path: " + file + "\r\n");
                    }

                    Logger.LogEvent(" Finished executing sp on server!");
                    Logger.WriteLogToFile();
                }
                return xmlDoc;
            }
            catch (SqlException sqlEx)
            {
                EmailUtility.SendEmail("ORMIS DB/SQL EXCEPTION: " + "\r\n" + sqlEx.Message, "Import Failed!");
                Logger.LogException(sqlEx.Message);
                Logger.WriteLogToFile();
                throw;
            }

            catch (Exception ex)
            {
                EmailUtility.SendEmail("Other ORMIS DB/SQL EXCEPTION while executing stored procedure: " + "\r\n" + ex.Message, "Import Failed!");
                Logger.LogException(ex.Message);
                Logger.WriteLogToFile();                
                throw;
            }
        }
    }
}

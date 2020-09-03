using MNGUPDATEPRD.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MNGUPDATEPRD
{
    public static class ManagerImporter
    {
        private static readonly string file = ConfigurationManager.AppSettings["OutputFilename"];
        public static void ImportManagers(XmlDocument xmlDoc)
        {
            try
            {
                Logger.LogEvent(" Checking if file exists");
                string bat = ConfigurationManager.AppSettings["BAT"];
                if (File.Exists(file))
                {
                    string contents = XmlUtil.BeautifyXml(xmlDoc);
                    Logger.LogEvent(" File exists!");
                    Logger.LogEvent(" Starting import");
                    File.WriteAllText(file, contents, Encoding.UTF8);

                    try
                    {
                        StringBuilder sbOut = new StringBuilder();
                        ExecuteImport(sbOut);
                    }
                    catch (Exception ex)
                    {
                        EmailUtility.SendEmail("Error executing Optial Data Import. " + "\r\n" + ex.Message, "Import Failed!");
                        Logger.LogEvent(" Error executing Optial Data Import: " + ex.Message);
                    }

                    if (File.Exists(bat))
                    {
                        try
                        {
                            Logger.LogEvent(" Bat found! Starting bat execution");
                            Process.Start(bat);
                            Logger.LogEvent(" Finished bat execution");
                        }
                        catch (Exception ex)
                        {
                            Logger.LogEvent(" Error executing bat file: ");
                            Logger.LogException(ex.Message);
                            Logger.WriteLogToFile();
                        }
                    }
                }

                Logger.Log.Append("**************** FINISHED MANAGER UPDATE ****************" + "\r\n" + "\r\n");
                Logger.WriteLogToFile();
            }
            catch (Exception e)
            {
                EmailUtility.SendEmail("Error executing import part: " + "\r\n" + e.Message, "Import Failed!");
                Logger.LogException(e.Message);
                Logger.WriteLogToFile();
            }
        }

        private static void ExecuteImport(StringBuilder sbOut)
        {
            var process = new Process();
            process.StartInfo.FileName = "CMD.exe";
            process.StartInfo.Arguments = ConfigurationManager.AppSettings["Command"];
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.OutputDataReceived += (sender, ar) => sbOut.Append(ar.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            if (sbOut.Length > 0)
            {
                EmailUtility.SendEmail("Error executing Optial Data Import. " + "\r\n" + sbOut.ToString(), "Import Failed!");
                Logger.LogEvent(" Error executing Optial Data Import. ");
                Logger.Log.Append(sbOut.ToString());
            }
        }
    }
}

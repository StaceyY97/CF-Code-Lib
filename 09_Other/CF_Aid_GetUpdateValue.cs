using Medidata.Core.Common;
using Medidata.Core.Common.Utilities;
using Medidata.Core.Objects;
using Medidata.Interfaces;
using Medidata.Interfaces.Clinical;
using Medidata.Interfaces.Refs;
using Medidata.RaveWebServices.Outbound;
using Medidata.Utilities;
using Medidata.Utilities.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
namespace CustomFunctions
{
using Medidata.Core.Objects.DCFs;
#region System Class - Do Not Modify
using Medidata.CustomFunctions.Debug;
using Mono.CSharp;
using System.Diagnostics;
using System.Windows.Forms;
using static log4net.Appender.ColoredConsoleAppender;
/// <summary>
/// Runs the CustomFunction Development Utility.
/// </summary>
public class _SystemClass
{
/// <summary>
/// Initializes a new instance of the <see cref="_SystemClass"/> class.
/// </summary>
public _SystemClass()
{
}
/// <summary>
/// The main entry point for the application.
/// </summary>
[STAThread]
static void Main()
{
AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
System.Windows.Forms.Application.Run(new MainForm());
}
/// <summary>
/// Handles the UnhandledException event of the CurrentDomain control.
/// </summary>
/// <param name="sender">The source of the event.</param>
/// <param name="e">The <see cref="System.UnhandledExceptionEventArgs"/> instance containing the event data.</param>
private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
{
Exception exception = (Exception)e.ExceptionObject;
if (!CacheManager.IsThreadAbortException(exception)) System.Windows.Forms.MessageBox.Show(exception.ToString());
}
}
#endregion
public class CF_Aid_GetUpdateValue : CustomFunctionBase
{
public object Eval(object ThisObject)
{
object[] inputParameters = (object[])ThisObject;
            DataPoint enterDP = (DataPoint)inputParameters[0];
            string[] FieldOrder = (string[])inputParameters[1];

            StringBuilder tableHtml = new StringBuilder();

            string tableStyle = @"style='width: 100%; border-collapse: collapse; font-family: Arial, sans-serif; font-size: 14px;'";
            string headerStyle = @"style='background-color: #2c6aa0; color: white; padding: 10px 12px; text-align: left; font-weight: bold;'";
            string cellStyle = @"style='padding: 8px 12px; border: 1px solid #ddd;'";
            string fieldCellStyle = @"style='padding: 8px 12px; border: 1px solid #ddd; background-color: #f5f9fc; font-weight: bold; width: 25%;'";
            string valueCellStyle = @"style='padding: 8px 12px; border: 1px solid #ddd; width: 25%;'";

tableHtml.Append("<h3 style='font-family: Arial, sans-serif; color: #2c6aa0; margin-bottom: 10px;'>Update Items:</h3>");
            tableHtml.Append("<table " + tableStyle + ">");
            tableHtml.Append("<thead>");
            tableHtml.Append("<tr>");
            tableHtml.Append("<th " + headerStyle + ">Field</th>");
            tableHtml.Append("<th " + headerStyle + ">Change Type</th>");
            tableHtml.Append("<th " + headerStyle + ">Value Before</th>");
            tableHtml.Append("<th " + headerStyle + ">Value Current</th>");
            tableHtml.Append("</tr>");
            tableHtml.Append("</thead>");
            tableHtml.Append("<tbody>");

            bool hasData = false;

            foreach (string OID in FieldOrder)
            {
                try
                {
                    DataPoint dp = enterDP.Record.DataPoints.FindByFieldOID(OID);
                    if (dp != null && checkChangeStatus(dp))
                    {
                        hasData = true;

                        string fieldName = convertCKB(dp);
                        string changeValue = GetChangeValue(dp).Trim();
                        string currentValue = GetCurrentValue(dp).Trim();
                        string changeType = "";

                        if (changeValue == currentValue)
                        {
                            changeType = "Query / Field Status Changed";
                        }
                        else
                        {
                            changeType = "Value Updated";
                        }

                        tableHtml.Append("<tr>");
                        tableHtml.Append("<td " + fieldCellStyle + ">" + fieldName + "</td>");
                        tableHtml.Append("<td " + valueCellStyle + ">" + changeType + "</td>");
                        tableHtml.Append("<td " + valueCellStyle + ">" + changeValue + "</td>");
                        tableHtml.Append("<td " + valueCellStyle + ">" + currentValue + "</td>");
                        tableHtml.Append("</tr>");
                    }
                }
                catch { }
            }
            if (!hasData)
            {
                tableHtml.Append("<tr>");
                tableHtml.Append("<td colspan='4' " + cellStyle + " style='text-align: center; color: #666;'>No field changes detected</td>");
                tableHtml.Append("</tr>");
            }

            tableHtml.Append("</tbody>");
            tableHtml.Append("</table>");

            return tableHtml.ToString();
        }
        bool checkChangeStatus(DataPoint dp) => dp != null && dp.Active && dp.IsObjectChanged && !dp.Interaction.TrueUser.FullName.Trim().ToUpper().StartsWith("SYSTEM");
        string GetUV(DataPoint targetDP, string codedValue) => GetCKBUV(targetDP, codedValue) == "" ?
targetDP.Field.Variable.DataDictionary?.Entries.FirstOrDefault(e => e.Key.Equals(codedValue))?.Value ?? codedValue
: GetCKBUV(targetDP, codedValue);
        string GetCKBUV(DataPoint targetDP, string codedValue) => string.Compare(targetDP.Field.ControlType, "CHECKBOX", true) == 0 ?
        codedValue == "1" ? "Checked" : codedValue == "0" ? "Unchecked" : "" : "";

        string AEACN_OP = "Action taken with OP-3136: ", AEACN_FU = "Action taken with Fulvestrant: ", AEACN_PA = "Action taken with Palazestrant: ";
        string convertCKB(DataPoint dp)
        {
            if (dp.Field.OID.StartsWith("AEACN") && dp.Field.OID != "AEACNOTH" && dp.Field.OID != "AEACNSP")
            {
                string end = dp.Field.OID.Substring(5);
                if (int.TryParse(end, out int num))
                {
                    string pretext = (num >= 1 && num <= 5) ? AEACN_OP : ((num >= 6 && num <= 10) ? AEACN_FU : ((num >= 11 && num <= 15) ? AEACN_PA : ""));
                    return pretext += dp.Field.PreText;
                }
                return "";
            }
            else return dp.Field.PreText;
        }
        string GetChangeValue(DataPoint dp)
        {
            foreach (Audit audit in Audits.Load(dp))
            {
                if (audit.SubCategory == AuditSubCategory.Entered || audit.SubCategory == AuditSubCategory.EnteredWithChangeCode)
                {
                    string auditValue = GetValueFromAudit(audit.Readable.Trim());
                    return GetCKBUV(dp, auditValue) == "" ? auditValue : GetCKBUV(dp, auditValue);
                }
            }
            return "Empty";

        }

        string GetValueFromAudit(string audit)
        {
            int first_Quote = audit.IndexOf("\'");
            int second_Quote = audit.Substring(first_Quote + 1).IndexOf("\'");
            string value = audit.Substring(first_Quote + 1, second_Quote);
            if (value.IndexOf("(") > 0) value = value.Substring(0, value.IndexOf("(") - 1).Trim();
            return value;
        }

        string GetCurrentValue(DataPoint dp)
        {
            if (dp.Data == string.Empty) return "Empty";
            return dp.StandardValue() is DateTime ? dp.Data : GetUV(dp, dp.Data);
}
}
}

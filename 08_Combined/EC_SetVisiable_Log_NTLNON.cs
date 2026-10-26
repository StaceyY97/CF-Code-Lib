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
public class EC_SetVisiable_Log_NTLNON : CustomFunctionBase
{
public object Eval(object ThisObject)
{
Dictionary<string, List<string>> form_Match = new Dictionary<string, List<string>>()
{
   {"NTL", new List<string>{ "NTLNON", "0","NTLSTAT" } }
};
            List<string> avoidFolder = new List<string>() { "SCREEN_2", "RESCREEN_2" };
            string NELable = "TLSUMNE";

            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpAction = afp.ActionDataPoint;
            DataPage currentPage = dpAction.Record.DataPage;
            string instanceOID = currentPage.Instance.Folder.OID;
            if (!form_Match.ContainsKey(currentPage.Form.OID)) return null;

            List<string> manageFields = form_Match[currentPage.Form.OID];
            DataPoint checkDP = currentPage.MasterRecord.DataPoints.FindByFieldOID(manageFields[0]);
            string convertToUservalue(string enterCode) => enterCode == "Y" ? "Yes" : enterCode == "N" ? "No" : "";
            string GetUserValueByCodedValue(DataPoint targetDP, string codedValue) => GetCKBUserValue(targetDP, codedValue) == "" ?
                targetDP.Field.Variable.DataDictionary?.Entries.FirstOrDefault(e => e.Key.Equals(codedValue))?.Value ?? codedValue
                : GetCKBUserValue(targetDP, codedValue);
            string GetCKBUserValue(DataPoint targetDP, string codedValue) => string.Compare(targetDP.Field.ControlType, "CHECKBOX", true) == 0 ? codedValue == "1" ? "Checked" : codedValue == "0" ? "Unchecked" : "" : "";

            bool doVisiable = checkDP != null && checkDP.Data == manageFields[1] && checkDP.IsVisible && !avoidFolder.Contains(instanceOID);
            Instance curIns = dpAction.Record.Instance;
            DataPage NTL = curIns?.DataPages.FindByFormOID("NTL");
            if (NTL != null)
            {
                foreach (Record rc in NTL.Records)
                {
                    if (rc == null || !rc.Active || rc.RecordPosition == 0) continue;
                    DataPoint manageDP = rc.DataPoints.FindByFieldOID("NTLSTAT");
                    if (manageDP != null) manageDP.IsVisible = doVisiable;
                    DoDynamicWithQuery(manageDP, doVisiable);
                }
            }

            void DoDynamicWithQuery(DataPoint DP, bool isVis)
            {
                if (DP != null)
                {
                    string querytext =
                    $"\"{checkDP.Field.PreText}\" is not recorded as \"{GetUserValueByCodedValue(checkDP, manageFields[1])}\" or " +
                    $"Current Folder is Sreening 2 / Re-Screening 2, but \"{DP.Field.PreText}\" is present. Please verify and update, else clarify.";

                    if (isVis)
                    {
                        DP.IsVisible = true;
                        CustomFunction.PerformQueryAction(querytext, 1, false, false, DP, false, afp.CheckID, afp.CheckHash);
                    }
                    else
                    {
                        bool status = DP.Field.ControlType.ToUpper() != "CHECKBOX" ? !string.IsNullOrEmpty(DP.Data) : DP.Data == "1";
                        DP.IsVisible = status;
                        CustomFunction.PerformQueryAction(querytext, 1, false, false, DP, status, afp.CheckID, afp.CheckHash);
                    }
                }
            }

            return null;
}
}
}

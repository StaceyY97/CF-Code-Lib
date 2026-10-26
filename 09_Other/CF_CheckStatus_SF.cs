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
public class CF_CheckStatus_SF : CustomFunctionBase
{
public object Eval(object ThisObject)
{
object[] inputParameters = (object[])ThisObject;
            DataPoint dpAction = (DataPoint)inputParameters[0];
            string checkType = (string)inputParameters[1];
            Subject SUBJ = dpAction.Record.Subject;

            DataPoints allRSYN = CustomFunction.FetchAllDataPointsForOIDPath("RSYN", "RS", null, SUBJ);
            DataPoints allIEYN = CustomFunction.FetchAllDataPointsForOIDPath("IEYN", "IE", null, SUBJ);

            List<string> checkList = folderSCR;

            foreach (DataPoint RSYN in allRSYN)
            {
                if (RSYN != null && RSYN.Active && RSYN.Data == "Y")
                {
                    checkList = RSYN.Record.Instance.Folder.OID != "D1" ? folderRESCR_Pre : new List<string> { "RESCREEN_1", "RESCREEN_2", "D1_2" };
                    break;
                }
            }
            string SFFolder = isIEMeet_SF(allIEYN, checkList);

            if (checkType == "SF") return SFFolder != "";
            else
            {
                if (SFFolder != "")
                {
                    DataPoints CheckDPs_SCR = CustomFunction.FetchAllDataPointsForOIDPath("RSYN", "RS", null, SUBJ);
                    if (checkList.Contains("D1_2")) ManageIndex_D1();
                    foreach (DataPoint dp in CheckDPs_SCR)
                    {
                        if (dp != null && dp.Active && dp.Data == "Y" && foldersForEnroll.Contains(dp.Record.Instance.Folder.OID))
                        {
                            if (foldersForEnroll.IndexOf(dp.Record.Instance.Folder.OID) > foldersForEnroll.IndexOf(SFFolder)) return true;
                        }
                    }
                    return false;
                }
                else return true;
            }
        }
        List<string> folderSCR = new List<string>()
{
    "SCREEN_1", "SCREEN_2", "D1"
}
        ;
        List<string> folderRESCR_Pre = new List<string>()
{
    "RESCREEN_1", "RESCREEN_2","D1"
}
        ;
        List<string> foldersForEnroll = new List<string>()
{
   "SCREEN_1", "SCREEN_2", "RESCREEN_1", "RESCREEN_2","D1","D1_2"
}
        ;

        string isIEMeet_SF(DataPoints allIEYN, List<string> aplList) //return true when subject meet IE
        {
            foreach (DataPoint dp in allIEYN)
            {
                if (dp != null && dp.Active && aplList.Contains(dp.Record.Instance.Folder.OID))
                {
                    if (dp.Data != "Y" && dp.Data != string.Empty)
                    {
                        DataPoint SFDate = dp.Record.DataPoints.FindByFieldOID("IEFDAT");
                        if (SFDate != null && SFDate.Active && SFDate.Data != string.Empty && SFDate.StandardValue() is DateTime) return dp.Record.Instance.Folder.OID;
                    }
                }
            }
            return "";
        }

        void ManageIndex_D1()
        {
            int indexOfD1 = foldersForEnroll.IndexOf("D1");
            int indexOfRescreen1 = foldersForEnroll.IndexOf("RESCREEN_1");
            if (indexOfD1 != -1 && indexOfRescreen1 != -1)
            {
                foldersForEnroll.RemoveAt(indexOfD1);
                if (indexOfD1 < indexOfRescreen1)
                {
                    indexOfRescreen1--;
                }
                foldersForEnroll.Insert(indexOfRescreen1, "D1");
            }
}
}
}

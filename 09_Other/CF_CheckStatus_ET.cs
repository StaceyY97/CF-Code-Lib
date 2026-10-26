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
public class CF_CheckStatus_ET : CustomFunctionBase
{
public object Eval(object ThisObject)
{
object[] inputParameters = (object[])ThisObject;
    DataPoint dpAction = (DataPoint)inputParameters[0];
    string type = (string)inputParameters[1];
    Subject SUBJ = dpAction.Record.Subject;

    bool doMeet = false;

    DataPoints allCOUNT = CustomFunction.FetchAllDataPointsForOIDPath("CONTYN", null, null, SUBJ);

    if (type == "RCPET")
    {
        DataPoint VISYN = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { SUBJ, "", "WK16", "VIS2", "VIS2YN" });
        if (checkDPN(VISYN)) return true;

        doMeet = CheckRS(SUBJ) ? CheckELDAT(dpAction, "D1_2") : CheckELDAT(dpAction, "D1");
        if (doMeet) return doMeet;

        List<string> foldersForRCP = new List<string>
        {
            "WK2",
            "WK4",
            "WK8",
"WK12"
        };
        if (CheckRS(SUBJ)) foldersForRCP.Add("D1_2");
        else foldersForRCP.Add("D1");
        if (CheckCOUNT(allCOUNT, foldersForRCP)) return true;
        else return doMeet;

    }
    else if (type == "OLEET")
    {
        DataPoint VISYN = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { SUBJ, "", "OLEWK104", "VIS", "VISYN" });
        if (checkDPN(VISYN)) return true;

        List<string> foldersForOLE = new List<string>
        {
            "OLED1",
            "OLEWK2",
            "OLEWK6",
            "OLEWK12",
            "OLEWK36",
"OLEWK24",
            "OLEWK48",
            "OLEWK60",
            "OLEWK72",
            "OLEWK84"
        };
        CheckCOUNT(allCOUNT, foldersForOLE);
        if (CheckCOUNT(allCOUNT, foldersForOLE)) return true;
        else return doMeet;
    }

    return doMeet;
}

bool CheckELDAT(DataPoint dpAction, string folderOID)
{
    DataPoint ELDAT = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { dpAction.Record.Subject, "", folderOID, "ENR", "ELDAT" });
    DataPoint COUNT = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { dpAction.Record.Subject, "", folderOID, "CONT", "CONTYN" });
    return checkDTNotEmpty(ELDAT) && checkDPN(COUNT);
}
bool CheckCOUNT(DataPoints allCOUNT, List<string> folderOID)
{
    foreach (DataPoint dp in allCOUNT)
    {
        if(checkDPN(dp) && folderOID.Contains(dp.Record.Instance.Folder.OID)) return true;
    }
    return false;
}

bool checkDPY(DataPoint dp) => dp != null && dp.Active && dp.Data == "Y";
bool checkDPN(DataPoint dp) => dp != null && dp.Active && dp.Data == "N";
bool checkDTNotEmpty(DataPoint dp) => dp != null && dp.Active && dp.StandardValue() is DateTime && dp.Data != string.Empty;

 bool CheckRS(Subject SUBJ)
 {
     bool isRSSubj = false;
     DataPoints allRSYN = CustomFunction.FetchAllDataPointsForOIDPath("RSYN", "RS", "D1", SUBJ);
     foreach (DataPoint RSYN in allRSYN) if (checkDPY(RSYN)&&RSYN.Record.Instance.Folder.OID=="D1") { isRSSubj = true; break; }
     return isRSSubj;
}
}
}

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
public class EC_Contra_Compare_To_Base : CustomFunctionBase
{
public object Eval(object ThisObject)
{
const string QUERY_TEXT = "The number of log lines triggered at Screening 2 or Rescreening 2 should remain consistent for the following visits. Please verify and update, else clarify.";
    const int MARKING_GROUP = 1;
    const bool ANSWER_ON_CHANGE = false;
    const bool CLOSE_ON_CHANGE = false;
    bool doquery = false;

    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dp = afp.ActionDataPoint;
    if (dp == null) return null;
    Subject subj = dp.Record.Subject;
    Instance curIns = dp.Record.DataPage.Instance;
    DataPage curPage = dp.Record.DataPage;
    string checkFieldOID = dp.Field.OID;
    string[] baseline = new string[] { "RESCREEN_2", "SCREEN_2" };

    DataPoints queryDPs= new DataPoints();
    if(baseline.Contains(curIns.Folder.OID)) queryDPs = CustomFunction.FetchAllDataPointsForOIDPath(checkFieldOID, curPage.Form.OID,null,subj);
    else queryDPs.Add(dp);

    foreach(DataPoint queryDP in queryDPs)
    {
        if (queryDP == null) continue;
        doquery = false;
        int checkRC = GetActiveRC(queryDP.Record.DataPage.Records);
        foreach (string visit in baseline)
        {
            DataPage CheckPage = subj.Instances.FindByFolderOID(visit)?.DataPages.FindByFormOID(dp.Record.DataPage.Form.OID);
            if (CheckPage != null)
            {
                int compareRC = GetActiveRC(CheckPage.Records);
                doquery = checkRC > compareRC;
                break;
            }
        }
        CustomFunction.PerformQueryAction(QUERY_TEXT, MARKING_GROUP, ANSWER_ON_CHANGE, CLOSE_ON_CHANGE, queryDP, doquery, afp.CheckID, afp.CheckHash);

    }

    return null;
}
int GetActiveRC(Records allRC)
{
    int activeRecords = allRC.Count;
    foreach (Record rc in allRC)
    {
        if (rc != null && !rc.Active) activeRecords = activeRecords - 1;
    }
    return activeRecords;
}
}
}

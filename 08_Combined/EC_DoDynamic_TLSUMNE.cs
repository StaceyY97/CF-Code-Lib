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
public class EC_DoDynamic_TLSUMNE : CustomFunctionBase
{
public object Eval(object ThisObject)
{
ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint enterDP = afp.ActionDataPoint;
    if (enterDP == null) return null;

    DataPoint triggerDP = enterDP.Record.DataPoints.FindByFieldOID("TLYN");
    string folderOID = enterDP.Record.Instance?.Folder.OID;
bool triggerMeet = triggerDP != null && triggerDP.Active && triggerDP.Data == "Y"&& folderOID != "SCREEN_2"&& folderOID != "RESCREEN_2";

    string[] queryFields = { "TLSUMNE" };
    DataPoints queryDPs = new DataPoints();
    foreach (string fieldOID in queryFields)
    {
        DataPoint tempDP = enterDP.Record.DataPoints.FindByFieldOID(fieldOID);
        if (tempDP != null) queryDPs.Add(tempDP);
    }

    if (queryDPs.Count == 0) return null;

    ManageSteps_DP(enterDP, "TLYN", "IsEqualTo", "Y");
    foreach (DataPoint queryDP in queryDPs)
    {
        string tempQuery = (string)CustomFunction.PerformCustomFunction("CF_QueryBuilder", enterDP.Field.CRFVersion.ID, new object[] { triggerDPs, relation, querytext, enterDP }) + ", but \"" + (queryDP?.Field?.PreText ?? "field") + "\"" + " is present. Please verify and update, else clarify.";
        bool shouldBeVisible = triggerMeet || queryDP.Data!=string.Empty ;
        bool shouldBeQuery = !triggerMeet && queryDP.Data!=string.Empty;
        CustomFunction.PerformQueryAction(tempQuery, 1, false, false, queryDP, shouldBeQuery, afp.CheckID, afp.CheckHash);
        queryDP.IsVisible = shouldBeVisible;
    }

    return null;
}
private string querytext = "";
private Dictionary<int, List<object>> triggerDPs = new Dictionary<int, List<object>>();
private Dictionary<int, string> relation = new Dictionary<int, string>();
private void ManageSteps_DP(DataPoint DP, string fieldOID_TriggerDP, string relation, string compareValue)
{
    var checkDP = DP.Record.DataPoints.FindByFieldOID(fieldOID_TriggerDP);
    var compareOB = compareValue;
    var tempFunc = relation;
    triggerDPs[0] = new List<object> { checkDP, compareOB, tempFunc };
}
}
}

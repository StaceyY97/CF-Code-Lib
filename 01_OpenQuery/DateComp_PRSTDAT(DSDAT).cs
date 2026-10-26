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
public class DateComp_PRSTDAT_DSDAT : CustomFunctionBase
{
public object Eval(object ThisObject)
{
const string QUERY_TEXT = "\"Start Date\" is before earliest \"Consent Date\". Please review and correct, else clarify.";
const int MARKING_GROUP_ID = 1;
const bool ANSWER_ON_CHANGE = false;
const bool CLOSE_ON_CHANGE = false;
bool openQuery = false;

ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
DataPoint dpAction = afp.ActionDataPoint, dpt = null;
Subject currentSubject = dpAction.Record.Subject;

DataPoints dptsLB = new DataPoints();
DateTime dtMax = DateTime.MinValue;
if (dpAction.Field.OID == "PRSTDAT")
{
    dptsLB.Add(dpAction);
}
else
    dptsLB = CustomFunction.FetchAllDataPointsForOIDPath("PRSTDAT", "PR", null, currentSubject);

dtMax = GetDate(currentSubject);

for (int i = 0; i < dptsLB.Count; i++)
{
    openQuery = false;
    if (!dptsLB[i].Active||dptsLB[i].Data.Contains("UN")) continue;
    if (dptsLB[i].StandardValue() is DateTime && dtMax != DateTime.MinValue && dtMax > Convert.ToDateTime(dptsLB[i].StandardValue()))
        openQuery = true;



    CustomFunction.PerformQueryAction(QUERY_TEXT, MARKING_GROUP_ID, ANSWER_ON_CHANGE, CLOSE_ON_CHANGE, dptsLB[i], openQuery, afp.CheckID, afp.CheckHash);
}
return null;
    }

    DateTime GetDate(Subject currentSubject)
{
    DateTime dt = DateTime.MinValue;
    DataPoints dpts = CustomFunction.FetchAllDataPointsForOIDPath("DSDAT", "DSINF", "SCRN", currentSubject);

    for (int i = 0; i < dpts.Count; i++)
    {
        if (!dpts[i].Active || !(dpts[i].StandardValue() is DateTime)) continue;

        if (dt == DateTime.MinValue || dt > Convert.ToDateTime(dpts[i].StandardValue()))
            dt = Convert.ToDateTime(dpts[i].StandardValue());

    }
    return dt;
}
}
}

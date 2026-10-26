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
public class EC_CheckDP_EXSTDAT_EXENDAT : CustomFunctionBase
{
public object Eval(object ThisObject)
{
const bool closeOnChange = false;
const bool answerOnChange = false;
const int markingGroup = 1;
string queryMessage = " The \"Start Date\" is equal or prior to the \"Stop Date\" in the previous record. Please review and correct as appropriate. Dosing records should be recorded in chronological order.";
bool doQuery = false;
string queryMessage_2 = " The \"Start Date\" is more than 1 day after the \"Stop Date\" in the previous record. Please review, correct as appropriate, and ensure Study Drug Dosing entries are not missing and entered in sequential order. ";
bool doQuery_2 = false;


ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
DataPoint dpAction = afp.ActionDataPoint;
DataPage currentPage = dpAction.Record.DataPage;
Records allPageRC = currentPage.Records;

Dictionary<int, DataPoint> STDATMatch = new Dictionary<int, DataPoint>();
Dictionary<int, DataPoint> ENDATMatch = new Dictionary<int, DataPoint>();

foreach (Record rc in allPageRC)
{
    if (rc == null || !rc.Active || rc.RecordPosition == 0) continue;
    DataPoint STDAT = rc.DataPoints.FindByFieldOID("EXSTDAT");
    if (STDAT != null && STDAT.Active && STDAT.StandardValue() is DateTime && rc.RecordPosition != 1) STDATMatch.Add(rc.RecordPosition, STDAT);
    DataPoint ENDAT = rc.DataPoints.FindByFieldOID("EXENDAT");
    if (ENDAT != null && ENDAT.Active && ENDAT.StandardValue() is DateTime) ENDATMatch.Add(rc.RecordPosition, ENDAT);
}

foreach (int index in STDATMatch.Keys)
{
    if (index == 1) continue;
    if (ENDATMatch.ContainsKey(index - 1))
    {
        DateTime dtST = (DateTime)STDATMatch[index].StandardValue();
        DateTime dtEN = (DateTime)ENDATMatch[index - 1].StandardValue();
        //doQuery = dtST <= dtEN;
        //CustomFunction.PerformQueryAction(queryMessage, markingGroup, false, false, STDATMatch[index], doQuery, afp.CheckID, afp.CheckHash);
        TimeSpan window_Up = new TimeSpan(1, 0, 0, 0);
        doQuery_2 = dtST > dtEN && (dtST - dtEN) > window_Up|| dtST <= dtEN;
        CustomFunction.PerformQueryAction(queryMessage_2, markingGroup, false, false, STDATMatch[index], doQuery_2, afp.CheckID, afp.CheckHash);
        //string otstr = doQuery ? "1=TRUE\n" : "1=FALSE\n";
        //string otstr_2 = doQuery_2 ? "2=TRUE\n" : "2=FALSE\n";
        //if (dpAction.Record.Subject.Name == "QY004") Message.SendEmail("qian.yang@edetek.com", "test_qy@mdsol.com", "test_qy", otstr + otstr_2 + index.ToString());
    }
}

return null;
}
}
}

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
public class EC_CheckDP_LBTEST_Duplicated : CustomFunctionBase
{
public object Eval(object ThisObject)
{
const bool closeOnChange = false;
const bool answerOnChange = false;
const int markingGroup = 1;
string queryMessage = "Tests are duplicated. Please verify and update, else clarify.";
bool doQuery = false;

ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
DataPoint dpAction = afp.ActionDataPoint;
DataPage currentPage = dpAction.Record.DataPage;
Records allPageRC = currentPage.Records;
string matchField = dpAction.Field.OID;

DataPoints CheckDPs = new DataPoints();
foreach (Record rc in allPageRC)
{
    if (rc == null || !rc.Active || rc.RecordPosition == 0) continue;
    DataPoint checkDP = rc.DataPoints.FindByFieldOID(matchField);
    if (checkDP != null && checkDP.Active) CheckDPs.Add(checkDP);
}

if (CheckDPs.Count > 1)
{
    for (int i = CheckDPs.Count - 1; i > 0; i--)
    {
        doQuery = false;
        for (int j = i - 1; j >= 0; j--)
        {
            doQuery=CheckDPs[i].Data != string.Empty && CheckDPs[j].Data != string.Empty && CheckDPs[i].Data == CheckDPs[j].Data;
            if(doQuery) break;
        }
        CustomFunction.PerformQueryAction(queryMessage, markingGroup, false, false, CheckDPs[i], doQuery, afp.CheckID, afp.CheckHash);
    }
}

return null;
}
}
}

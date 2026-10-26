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
public class Contra_BLTPT_TLATPT_NTLATPT : CustomFunctionBase
{
public object Eval(object ThisObject)
{
string QUERY_TEXT = $"\"Timepoint\" is not consistant with Timepoint recorded in  Target and(or) Non-Target Lesion Assessment (RECIST) Form. Please review and correct, else clarify.";
const int MARKING_GROUP_ID = 1;
const bool ANSWER_ON_CHANGE = false;
const bool CLOSE_ON_CHANGE = false;
bool openQuery = false;


ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
DataPoint dpAction = afp.ActionDataPoint, dpt = null;
Subject currentSubject = dpAction.Record.Subject;
Instance currentIns = dpAction.Record.Instance;

string[] checkOIDs = new string[] { "TLATPT", "NTLATPT" };
string queryOID = "BLTPT", formOID_QR = "BL";

DataPoint GetDataPoint(string fieldOID, string formOID) => currentIns?.DataPages?.FindByFormOID(formOID)?.MasterRecord?.DataPoints?.FindByFieldOID(fieldOID);
bool CheckDPStatus(DataPoint dp) => dp != null && dp.Active && dp.Data.Trim() != string.Empty;

DataPoint queryDP = GetDataPoint(queryOID, formOID_QR);
if (CheckDPStatus(queryDP))
{
    foreach (string checkOID in checkOIDs)
    {
        openQuery = false;
        string formOID = checkOID.StartsWith("T") ? "TLA" : "NTL";
        DataPoint checkDP = GetDataPoint(checkOID, formOID);
        if (CheckDPStatus(checkDP))
        {
            openQuery = queryDP.Data.Trim() != checkDP.Data.Trim();
            break;
        }
    }
    CustomFunction.PerformQueryAction(QUERY_TEXT, MARKING_GROUP_ID, ANSWER_ON_CHANGE, CLOSE_ON_CHANGE, queryDP, openQuery, afp.CheckID, afp.CheckHash);
}


return null;
}
}
}

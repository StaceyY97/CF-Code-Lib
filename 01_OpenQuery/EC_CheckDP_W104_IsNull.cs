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
public class EC_CheckDP_W104_IsNull : CustomFunctionBase
{
public object Eval(object ThisObject)
{
const string QUERY_TEXT = "Did the subject complete the last scheduled visit in OLE (Week 104) is Yes. However, Did the visit occur? at Week 104 is No  or OLE Week 104 (Day 728) is not present. Please verify and update or clarify.";
const int MARKING_GROUP = 1;
const bool ANSWER_ON_CHANGE = false;
const bool CLOSE_ON_CHANGE = false;
bool doQuery = false;

ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
DataPoint dpAction = afp.ActionDataPoint;
Subject SUBJ = dpAction.Record.Subject;

if (dpAction.Data == "Y")
{
    DataPoint checkDP = GetSimpleDP("VISYN", "VIS", "OLEWK104");
DataPoint checkDP_2 = GetSimpleDP("CONTYN", "CONT", "OLEWK84");
    doQuery = checkDP == null || !checkDP.Active || checkDP?.Data == "N"||checkDP_2 ==null||checkDP_2?.Data!="Y";
}

CustomFunction.PerformQueryAction(QUERY_TEXT, MARKING_GROUP, ANSWER_ON_CHANGE, CLOSE_ON_CHANGE, dpAction, doQuery, afp.CheckID, afp.CheckHash);

DataPoint GetSimpleDP(string fieldOID, string formOID, string folderOID) => SUBJ.Instances.FindByFolderOID(folderOID)?.DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints.FindByFieldOID(fieldOID);
return null;
}
}
}

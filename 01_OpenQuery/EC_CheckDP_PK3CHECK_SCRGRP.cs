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
public class EC_CheckDP_PK3CHECK_SCRGRP : CustomFunctionBase
{
public object Eval(object ThisObject)
{
const string QUERY_TEXT = "Lanreotide and Octreotide PK is checked. However, this subject is in the Naive/Not Currently Treated screening group. Please review and update, else clarify.";
    const int MARKING_GROUP = 1;
    const bool ANSWER_ON_CHANGE = false;
    const bool CLOSE_ON_CHANGE = false;
    bool doQuery = false;

    List<string> folderList = new List<string>
{
    "UNA"
}
;

    Dictionary<string, List<string>> matchCodes = new Dictionary<string, List<string>>(){ { "1", new List<string> { "NAIVE/NOT CURRENTLY TREATED" } } };

    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpAction = afp.ActionDataPoint;
    Subject subject = dpAction.Record.Subject;

    if (dpAction.Field.OID == "PK3CHECK"&& folderList.Contains(dpAction.Record.Instance.Folder.OID))
    {
        if (matchCodes.ContainsKey(dpAction.Data))
        {
            doQuery = IsConsisitant(dpAction, matchCodes[dpAction.Data]);
            CustomFunction.PerformQueryAction(QUERY_TEXT, MARKING_GROUP, ANSWER_ON_CHANGE, CLOSE_ON_CHANGE, dpAction, doQuery, afp.CheckID, afp.CheckHash);
        }
    }
    else
    {
        DataPoints all_PK3TYPE = CustomFunction.FetchAllDataPointsForOIDPath("PK3CHECK", "UNA", null, subject);
        foreach (DataPoint dp in all_PK3TYPE)
        {
            if (dp == null || !dp.Active || !folderList.Contains(dp.Record.Instance.Folder.OID)) continue;
            if (matchCodes.ContainsKey(dp.Data))
            {
                doQuery = IsConsisitant(dp, matchCodes[dp.Data]);
                CustomFunction.PerformQueryAction(QUERY_TEXT, MARKING_GROUP, ANSWER_ON_CHANGE, CLOSE_ON_CHANGE, dp, doQuery, afp.CheckID, afp.CheckHash);
            }
        }
    }

    return null;
}

bool IsConsisitant(DataPoint dpAction, List<string> matchCode)
{
    string FolderOID_RE = "RESCREEN_1", FolderOID_1 = "SCREEN_1";
    string FormOID = "MH2";
    string FieldOID = "SCRGRP";

    DataPoint checkDP_RES = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { dpAction.Record.Subject, "", FolderOID_RE, FormOID, FieldOID });

    if (checkDP_RES == null || !checkDP_RES.Active)
    {
        DataPoint checkDP_SCR = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { dpAction.Record.Subject, "", FolderOID_1, FormOID, FieldOID });
        return checkDP_SCR != null && checkDP_SCR.Active && matchCode.Contains(checkDP_SCR.Data);
    }
    else return matchCode.Contains(checkDP_RES.Data);
}
}
}

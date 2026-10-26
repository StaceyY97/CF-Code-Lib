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
public class EC_GetGroup : CustomFunctionBase
{
public object Eval(object ThisObject)
{
ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpAction = afp.ActionDataPoint;
    string CurentIns = dpAction.Record.Instance.Folder.OID;

    string enterValue = "";

    string matchedFolder = CurentIns.EndsWith("1")?CurentIns.Replace("1", "2") : CurentIns;
    string triggerFolder = CurentIns.EndsWith("1")?CurentIns : CurentIns.Replace("2", "1");

    DataPoint dp_enter = dpAction.Record.Subject.Instances.FindByFolderOID(matchedFolder)?.DataPages.FindByFormOID("IE2")?.MasterRecord.DataPoints.FindByFieldOID("IE_PTS");
    if (dp_enter == null) return null;

    DataPoint checkDP_GRP = dpAction.Record.Subject.Instances.FindByFolderOID(triggerFolder)?.DataPages.FindByFormOID("MH2")?.MasterRecord.DataPoints.FindByFieldOID("SCRGRP");
    if (checkDP_GRP == null || !checkDP_GRP.Active || checkDP_GRP.Data == string.Empty) enterValue = "";

    if (checkDP_GRP != null && checkDP_GRP.Active&&checkDP_GRP.Data == "NAIVE/NOT CURRENTLY TREATED") enterValue = "NAIVE/NOT CURRENTLY TREATED";
    else
    {
        DataPoint checkDP_SRLTREAT = dpAction.Record.Subject.Instances.FindByFolderOID(triggerFolder)?.DataPages.FindByFormOID("MH2")?.MasterRecord.DataPoints.FindByFieldOID("SRLTREAT");
        if (checkDP_SRLTREAT == null || !checkDP_SRLTREAT.Active || checkDP_SRLTREAT.Data == string.Empty) enterValue = "";
        else if((checkDP_GRP.Data.StartsWith("LONG") && checkDP_SRLTREAT.Data.StartsWith("LONG")) || (checkDP_GRP.Data.StartsWith("SHORT") && checkDP_SRLTREAT.Data.StartsWith("SHORT"))) enterValue = "WASHOUT OF OCTREOTIDE";
        else if (checkDP_GRP.Data.StartsWith("LONG") && checkDP_SRLTREAT.Data == "LANREOTIDE") enterValue = "WASHOUT OF LANREOTIDE";
        else enterValue = "";
    }
    EnterData(dp_enter, enterValue);
    return null;
}

public void EnterData(DataPoint dp, string val)
{
    if (dp != null && dp.Active && string.Compare(dp.Data, val, true) != 0)
    {
        dp.UnFreeze();
        dp.Enter(val, string.Empty, 0);
    }
    dp.Freeze();
}
}
}

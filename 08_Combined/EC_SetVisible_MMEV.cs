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
public class EC_SetVisible_MMEV : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
 * Created By :Qian Yang/18Apr2025
 * Custom Function :EC_SetVisible_MMEV
 * Description: Set fields visible in condition
 * Edit Checks :Merge_OLE_D1_W12, AddForm_OLED1_EGPK_W12
 *********************************************************************************************************/
    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dp = afp.ActionDataPoint;
    Subject sj = dp.Record.Subject;

    List<string> manageFields_1 = new List<string>
{
    "MMEV_FNUM", "MMEV_ACT"
}
    ;
    List<string> manageFields_2 = new List<string>
{
    "MMEV_FNUM1", "MMEV_WOR"
}
    ;

    Instance currentIns = dp.Record.Instance;
    if (currentIns?.Folder == null) return null;

    string checkFolder = currentIns.Folder.OID.StartsWith("R") ? "RESCREEN_1" : "SCREEN_1";
    string GetUserValueByCodedValue(DataPoint targetDP, string codedValue) => targetDP?.Field?.Variable?.DataDictionary?.Entries?.FirstOrDefault(e => e.Key.Equals(codedValue))?.Value ?? string.Empty;
    DataPoint checkDP = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dp.Field.CRFVersion.ID, new object[] { sj, "", checkFolder, "MH2", "SCRGRP" });
    if (checkDP != null && checkDP.Active)
    {
        bool openFirst = checkDP.Data == "NAIVE/NOT CURRENTLY TREATED";
        bool openSecond = !openFirst && checkDP.Data != string.Empty;

        foreach (string field in manageFields_1)
        {
            DataPoint manageDP_1 = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dp.Field.CRFVersion.ID, new object[] { sj, "", currentIns.Folder.OID, "MMEV", field });

            if (manageDP_1 != null)
            {
                string querytext = "\"" + checkDP.Field.PreText + "\" is not recorded as " + "\"" + GetUserValueByCodedValue(checkDP, "NAIVE/NOT CURRENTLY TREATED") + "\", but \"" + manageDP_1.Field.PreText + "\" is present. Please verify and update, else clarify.";
                manageVisibleAndQuery(manageDP_1, openFirst, querytext);
            }
        }
        foreach (string field in manageFields_2)
        {
            DataPoint manageDP_1 = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dp.Field.CRFVersion.ID, new object[] { sj, "", currentIns.Folder.OID, "MMEV", field });
            if (manageDP_1 != null)
            {
                string querytext = "\"" + checkDP.Field.PreText + "\" is recorded as " + "\"" + GetUserValueByCodedValue(checkDP, "NAIVE/NOT CURRENTLY TREATED") + "\" or empty, but \"" + manageDP_1.Field.PreText + "\" is present. Please verify and update, else clarify.";
                manageVisibleAndQuery(manageDP_1, openSecond, querytext);
            }
        }
    }
    void manageVisibleAndQuery(DataPoint manageDP, bool shouldBeVisible, string querytext)
    {
        manageDP.IsVisible = shouldBeVisible||manageDP.Data!=string.Empty;
        bool hasQuery = !shouldBeVisible&&manageDP.Data!=string.Empty;
        CustomFunction.PerformQueryAction(querytext, 7, false, false, manageDP, hasQuery, afp.CheckID, afp.CheckHash);        
    }
    return null;
}
}
}

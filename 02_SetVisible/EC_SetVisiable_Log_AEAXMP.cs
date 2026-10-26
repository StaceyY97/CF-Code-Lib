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
public class EC_SetVisiable_Log_AEAXMP : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
 * Created By :Qian Yang/01Jul2025
 * Custom Function :EC_SetVisiable_LogForm_AEAXMP
 * Description: Set log lines visible or invisible in condition, please change and field mentioned in dictionary form_Match should be the first level of form dynamic and raise query if invisible field entered
 * Edit Checks :SetVisable_Log
 *********************************************************************************************************/

    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpAction = afp.ActionDataPoint;
    Subject subject = dpAction.Record.Subject;

    DataPoint triggerDP_1 = GetSimpleDP("SEXSCR", "DM", "SCRN");
    DataPoint triggerDP_2 = GetSimpleDP("CANTYPE", "DSENR", "SCRN");
    bool shouleBeVisible = false;
    if (triggerDP_1 != null && !triggerDP_1.Active && triggerDP_2 != null && !triggerDP_2.Active)
    {
        shouleBeVisible = (triggerDP_1.Data == "M" && (triggerDP_2.Data == "2" || triggerDP_2.Data == "3")) || (triggerDP_1.Data == "F" && triggerDP_2.Data == "1");
    }
    manageDPs("AEAXMP");


    DataPoint GetSimpleDP(string fieldOID, string formOID, string folderOID) =>
subject.Instances.FindByFolderOID(folderOID)?.DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints.FindByFieldOID(fieldOID);

    void manageDPs(string oid)
    {
        DataPoints allManageDPs = CustomFunction.FetchAllDataPointsForOIDPath(oid, "AE", "CMF", subject);
        for (int i = 0; i < allManageDPs.Count; i++)
        {
            DataPoint SAE = allManageDPs[i].Record.DataPoints.FindByFieldOID("AESER");
            if (allManageDPs[i] != null) allManageDPs[i].IsVisible = shouleBeVisible && SAE != null && SAE.Data == "Y";
        }
    }
    return null;
}
}
}

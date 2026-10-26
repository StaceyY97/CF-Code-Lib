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
public class EC_SetVisiable_Log_BACKYN : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
     * Created By :Qian Yang/02Jul2025
     * Custom Function :EC_SetVisiable_Log_singleTrigger
     * Description: Set log lines visible or invisible in condition, please change and field mentioned in dictionary form_Match should be the first level of form dynamic and raise query if invisible field entered
     * Edit Checks :SetVisable_Log
     *********************************************************************************************************/
        Dictionary<string, List<string[]>> form_Match = new Dictionary<string, List<string[]>>()
{
    { "BACKYN",new List<string[]>{ new string[] { "N" }, new string[] { "AEDLT" } } }
};

        ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
        DataPoint dpAction = afp.ActionDataPoint;
        Subject subject = dpAction.Record.Subject;

        if (form_Match.ContainsKey(dpAction?.Field.OID))
        {
            bool shouleBeVisible = form_Match[dpAction.Field.OID][0].Contains(dpAction.Data);
            manageDPs(form_Match[dpAction.Field.OID][1], shouleBeVisible);
        }
        else
        {
            foreach (string oid in form_Match.Keys)
            {
                DataPoint triggerDP = GetSimpleDP(oid, "DSENR", "SCRN");
                if (triggerDP == null && !triggerDP.Active) continue;
                bool shouleBeVisible = form_Match[triggerDP?.Field.OID][0].Contains(triggerDP ?.Data);
                manageDPs(form_Match[oid][1], shouleBeVisible);
            }
        }

        DataPoint GetSimpleDP(string fieldOID, string formOID, string folderOID) =>
    subject.Instances.FindByFolderOID(folderOID)?.DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints.FindByFieldOID(fieldOID);
        DataPage GetPage(string formOID, string folderOID) => subject.Instances.FindByFolderOID(folderOID)?.DataPages.FindByFormOID(formOID);

        void manageDPs(string[] manageList, bool shouleBeVisible)
        {
            DataPage AE = GetPage("AE", "CMF");
            if (AE != null)
            {
                Records allPgRecords = AE.Records;
                foreach (Record record in allPgRecords)
                {
                    if (record == null || record.RecordPosition == 0) continue;

                    foreach (string oid in manageList)
                    {
                        DataPoint manageDP = record.DataPoints.FindByFieldOID(oid);
                        if (manageDP != null)manageDP.IsVisible = shouleBeVisible;
                    }
                }
            }
        }
        return null;
}
}
}

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
public class EC_SetVisable_NEWYN_Y : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:**************************
        * CF Name: DER_TUBASE_NEW
        * Programmer Name:Qian Yang(17Nov2025)
        * Last Modified: NA.
        * Reason Modified: NA.
        * Description: Populate baseline information to following assessments and do visitble to Logline fields
        **/

        ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
        DataPoint dp = afp.ActionDataPoint;
        Subject curSub = dp.Record.Instance.Subject;

        Instance currentIns = dp.Record.Instance;
        string folderOID = currentIns.Folder.OID;
        
        DataPoint trigger = GetSimpleDP(triggerField, manageForm, folderOID, curSub);
        if (trigger == null || !trigger.Active) return null;
        bool isVisible = trigger?.Data == triggerMatch;
        Records allPageRC = trigger.Record.DataPage.Records;
        foreach (Record rc in allPageRC)
        {
            if (rc == null || !rc.Active | rc.RecordPosition == 0) return null;
            DoVisible(isVisible, rc);
            //if (isVisible && Baseline.ContainsKey(rc.RecordPosition)) DoPopulate(rc, Baseline[rc.RecordPosition]);

        }
        return false;
    }

    DataPoint GetSimpleDP(string field_OID, string formOID, string folder_OID, Subject curSub) =>
curSub?.Instances.FindByFolderOID(folder_OID)?.DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints.FindByFieldOID(field_OID);

    ///below should be defined by programmer
    string manageForm = "NEW";

    string triggerField = "NEWYN";

    string triggerMatch = "Y";

    string[] visibleFields = new string[] { "NEWID", "NEWSITE", "NEWDESC", "NEWMETH", "NEWDAT" };


    ///above should be defined by programmer

    void DoVisible(bool isVisible, Record rc)
    {
        if (rc != null && rc.Active && rc.RecordPosition != 0)
        {
            foreach (string field in visibleFields)
            {
                DataPoint manageDP = rc.DataPoints.FindByFieldOID(field);
                if (manageDP != null) manageDP.IsVisible = isVisible;
            }
        }
}
}
}

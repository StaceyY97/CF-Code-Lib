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
public class LBTM_014 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dp = afp.ActionDataPoint;
    Subject sj = dp.Record.Subject;
    bool openQuery = false;
    string queryText = "Sex equals Female. However, 'PSA' is present. Please review & update.";
    bool isFemale;

    if (dp.Data != string.Empty)
    {
        DataPoint gender = GetDataPoint(sj, "", "DM", "DM", "SEX");
        if (gender == null) return null;
        if (gender.Data == string.Empty) return null;
        isFemale = (string.Compare(gender.Data.Trim(), "F", true) == 0);

        if (isFemale)
        {
            if (!Number.IsValidNumber(dp.Data))
            {
                if (string.Compare(dp.Data.Trim(), "N/A", true) == 0 || string.Compare(dp.Data.Trim(), "ND", true) == 0) openQuery = false;
                else openQuery = true;
            }
            else openQuery = true;
        }
    }
    else  openQuery = false;

    CustomFunction.PerformQueryAction(queryText, 1, false, false, dp, openQuery, afp.CheckID, afp.CheckHash);
    return null;      

}

DataPoint GetDataPoint(Subject currentSubj, string parentFolderOID, string folderOID, string formOID, string feildOID)
{
    DataPoint dt = null;
    Instance Folder = null;
    if (currentSubj != null && currentSubj.Active)
    {
        if (parentFolderOID != "")
        {
            Instance tempIns = currentSubj.Instances.FindByFolderOID(parentFolderOID);
            if (tempIns == null) return dt;
            Folder = tempIns.Instances.FindByFolderOID(folderOID);
            if (Folder == null) return dt;
        }
        else
        {
            Folder = currentSubj.Instances.FindByFolderOID(folderOID);
            if (Folder == null) return dt;
        }
        DataPage tempPage = Folder.DataPages.FindByFormOID(formOID);
        if (tempPage == null) return dt;
        DataPoint tempDP = tempPage.MasterRecord.DataPoints.FindByFieldOID(feildOID);
        if (tempDP == null) return dt;
        if (tempDP.Data != string.Empty)
        {
            return tempDP;
        }
    }
    return dt;
}
}
}

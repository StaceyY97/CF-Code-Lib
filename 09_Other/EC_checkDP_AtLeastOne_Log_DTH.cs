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
public class EC_checkDP_AtLeastOne_Log_DTH : CustomFunctionBase
{
public object Eval(object ThisObject)
{
DataPoint dpAction = (DataPoint)ThisObject;

        string[] Folders = new string[] { "LOGS", "LOGS" }, Form = new string[] { "AE", "AE2" }, Field = new string[] { "AEOUT", "AE2OUT" };
        List<string> Matched_Code = new List<string>
{
    "FATAL"
}
        ;

        if (dpAction == null) return false;

        for (int i = 0; i < Folders.Count(); i++)
        {
            DataPoints allFatal = CustomFunction.FetchAllDataPointsForOIDPath(Field[i], Form[i], Folders[i], dpAction.Record.Subject);
            foreach (DataPoint dp in allFatal)
            {
                if (dp == null || !dp.Active || dp.Record.RecordPosition == 0) continue;
                if (Matched_Code.Contains(dp.Data)) return true;
            }
        }

        DataPoint GetSimpleDP(string fieldOID, string formOID, string folderOID) =>
    dpAction.Record.Subject.Instances.FindByFolderOID(folderOID)?.DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints.FindByFieldOID(fieldOID);


        DataPoint tempDP_EOTRCP = GetSimpleDP("DSREAS", "DS", "EOT");
        DataPoint tempDP_EOTOLE = GetSimpleDP("DSREAS2", "DS2", "EOT");

        return (tempDP_EOTOLE != null && tempDP_EOTOLE.Active && tempDP_EOTOLE.Data == "DEATH") || (tempDP_EOTRCP != null && tempDP_EOTRCP.Active && tempDP_EOTRCP.Data == "DEATH");
}
}
}

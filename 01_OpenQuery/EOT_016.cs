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
public class EOT_016 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*Date:07-Feb-2024
        Custom Fuction Name:EOT_16
        Related Edit Check:EOT_16
        Discription:"Open query for comparing a simple form field to a log form field."
        */

        ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint dp = afp.ActionDataPoint;
        Subject sj = dp.Record.Subject;
        bool openQuery = false;
        string queryText = "Reason for treatment discontinuation was reported as 'Withdrawal by Subject'; however, Consent Withdrawal form is missing. Please verify & update.";

        Instance insEOT = sj.Instances.FindByFolderOID("EOT");
        if (insEOT == null) return null;
        DataPage pageEOT = insEOT.DataPages.FindByFormOID("EOT");
        if (pageEOT == null) return null;
        DataPoint pointEOT = pageEOT.MasterRecord.DataPoints.FindByFieldOID("TTREAS");
        if (pointEOT.Active && pointEOT.Data != string.Empty && pointEOT.Data == "7")
        {
            openQuery = true;
            DataPoints CWCONSs = CustomFunction.FetchAllDataPointsForOIDPath("CWCONS", "CWD", null, sj);
            if (CWCONSs != null)
            {
                foreach (DataPoint CWCONS in CWCONSs)
                {

                    if (CWCONS.Active && CWCONS.Data != "")
                    {
                        openQuery = false;
                        break;
                    }
                }
            }

        }
        CustomFunction.PerformQueryAction(queryText, 1, false, false, pointEOT, openQuery, afp.CheckID, afp.CheckHash);
        return null;
}
}
}

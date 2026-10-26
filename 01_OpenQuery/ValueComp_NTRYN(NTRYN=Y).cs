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
public class ValueComp_NTRYN_NTRYN_Y : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/**HEADER INFORMATION:***********
        Check name : ValueComp_NTRYN(NTRYN=Y)
        Description: NTRYN=Yes and NTLSDAT is NULL
        Calling Edit Checks: TA003_CF,TA003_CF_CROSS
        ***********************************************************************/
const string QUERY_TEXT = "\"Are there any non target lesions?\" is \"Yes\", however, none of \"Date of Scan (dd-mon-yyyy)\" field in Non-Target Lesion (RECIST 1.1 - Screening) form is present. Please review and correct, else clarify.";
const int MARKING_GROUP_ID = 1;
const bool ANSWER_ON_CHANGE = false;
const bool CLOSE_ON_CHANGE = false;
bool openQuery = false;

ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
DataPoint dpAction = afp.ActionDataPoint, dpt = null;
Subject currentSubject = dpAction.Record.Subject;


ArrayList al = new ArrayList()
        {
            "NTLSDAT"
        }
;
Records rcds = new Records();
bool flag = false;
if (dpAction.Data == "Y")
{
    openQuery = true;
    DataPage dpg = dpAction.Record.Instance.DataPages.FindByFormOID("NTLS");
    if (dpg != null && dpg.Active)
        rcds = dpg.Records;
    if (dpg.IsBitSet(Status.IsTouched))
    {
        openQuery = false;
        for (int i = 0; i < rcds.Count; i++)
        {
            if (!rcds[i].Active || rcds[i].RecordPosition == 0) continue;
            flag = true;
            for (int j = 0; j < al.Count; j++)
            {
                dpt = rcds[i].DataPoints.FindByFieldOID(al[j].ToString());
                if (dpt != null && dpt.Active && dpt.ChangeCount > 0 && (dpt.Data.Trim() == string.Empty || dpt.IsDataPointNonConformant))
                {
                    openQuery = true;
                    break;
                }
            }

        }
        if (!flag) openQuery = true;
    }

}
CustomFunction.PerformQueryAction(QUERY_TEXT, MARKING_GROUP_ID, ANSWER_ON_CHANGE, CLOSE_ON_CHANGE, dpAction, openQuery, afp.CheckID, afp.CheckHash);

return null;
}
}
}

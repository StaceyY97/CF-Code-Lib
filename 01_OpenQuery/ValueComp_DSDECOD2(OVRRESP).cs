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
public class ValueComp_DSDECOD2_OVRRESP : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/* Custom Function: ValueComp_DSDECOD2(OVRRESP)
       Edit Check(s): ValueComp_DSDECOD2(OVRRESP)
       Description: DSDECOD2 = Progressive Disease per RECIST v1.1 and OVRRESP <> Progressive Disease (PD)
       */
const string QUERY_TEXT = "\"PRIMARY reason for Fulvestrant discontinuation\" is \"Progressive disease as determined by RECIST 1.1\"; however, there is not an RECIST 1.1 Response Assessment with an Overall Response of \"Progressive Disease (PD)\". Please review and correct, else clarify.";
const int MARKING_GROUP_ID = 1;
const bool ANSWER_ON_CHANGE = false;
const bool CLOSE_ON_CHANGE = false;
bool openQuery = false;

ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
DataPoint dpt_Action = afp.ActionDataPoint;
Subject Current_Subject = dpt_Action.Record.Subject;

if (dpt_Action.Data == "3")
{
    openQuery = true;
    DataPoints dpsAE = CustomFunction.FetchAllDataPointsForOIDPath("OVRRESP", "RS", "TASSMT1", Current_Subject);
    for (int i = 0; i < dpsAE.Count; i++)
    {
        if (dpsAE[i] == null || !dpsAE[i].Active) continue;
        if (dpsAE[i] != null && dpsAE[i].Data == "4")

        {
            openQuery = false;
            break;
        }
    }
}
CustomFunction.PerformQueryAction(QUERY_TEXT, MARKING_GROUP_ID, ANSWER_ON_CHANGE, CLOSE_ON_CHANGE, dpt_Action, openQuery, afp.CheckID, afp.CheckHash);

return null;
}
}
}

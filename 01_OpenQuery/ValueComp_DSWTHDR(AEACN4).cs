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
public class ValueComp_DSWTHDR_AEACN4 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*Custom Function: ValueComp_DSWTHDR(AEACN4)
        Edit Check(s): ValueComp_DSWTHDR(AEACN4)
        Description: DSDECOD1 = ADVERSE EVENT and no AETERM is present where AEACN = DRUG WITHDRAWN
        */
const string QUERY_TEXT = "\"Reason for Withdrawal of consent\" is \"Adverse Event\"; however, there is not an Adverse Event with Action Taken with OP-3136 of \"Drug Withdrawn\". Please review and correct, else clarify.";
const int MARKING_GROUP_ID = 1;
const bool ANSWER_ON_CHANGE = false;
const bool CLOSE_ON_CHANGE = false;
bool openQuery = false;

ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
DataPoint dpt_Action = afp.ActionDataPoint;
Subject Current_Subject = dpt_Action.Record.Subject;

if (dpt_Action.Data == "4")
{
    openQuery = true;
    DataPoints dpsAE = CustomFunction.FetchAllDataPointsForOIDPath("AETERM", "AE", "CMF", Current_Subject);
    for (int i = 0; i < dpsAE.Count; i++)
    {
        if (dpsAE[i] == null || !dpsAE[i].Active || dpsAE[i].Record.RecordPosition == 0) continue;
        DataPoint dpAEACN = dpsAE[i].Record.DataPoints.FindByFieldOID("AEACN4");
        if (dpsAE[i] != null && dpsAE[i].Data != string.Empty && dpAEACN != null && dpAEACN.Active && dpAEACN.Data == "1")

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

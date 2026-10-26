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
public class EC_CheckDP_ICFDAT : CustomFunctionBase
{
public object Eval(object ThisObject)
{
const string QUERY_TEXT = "Consent date is prior to Informed Consent date at Screening 1 / Rescreening 1. Please verify and update, else clarify.";
    const int MARKING_GROUP = 1;
    const bool ANSWER_ON_CHANGE = false;
    const bool CLOSE_ON_CHANGE = false;
    bool doQuery = false;

    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpAction = afp.ActionDataPoint;
    Subject subject = dpAction.Record.Subject;

    DataPoint DSCETDAT_D1 = dpAction;

    if (checkDTNotEmpty(DSCETDAT_D1))
    {
        DataPoint compareDT;
        DataPoint IEICDAT_RES1 = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subject, "", "RESCREEN_1", "DSIC", "IEICDAT" });
        if (checkDTNotEmpty(IEICDAT_RES1 )) compareDT = IEICDAT_RES1;
        else 
        {
            DataPoint IEICDAT_S1 = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subject, "", "SCREEN_1", "DSIC", "IEICDAT" });
            compareDT = IEICDAT_S1;
        }
//if (dpAction.Record.Subject.Name == "QL1114") Message.SendEmail("qian.yang@edetek.com", "test_qy@mdsol.com", "test_qy", DSCETDAT_D1.Data.ToString() + "_"+ compareDT.Data.ToString());
        doQuery = DataCompare(DSCETDAT_D1, compareDT);
        CustomFunction.PerformQueryAction(QUERY_TEXT, MARKING_GROUP, ANSWER_ON_CHANGE, CLOSE_ON_CHANGE, DSCETDAT_D1, doQuery, afp.CheckID, afp.CheckHash);

    }
    
    return null;
}
bool checkDTNotEmpty(DataPoint dp) => dp != null && dp.Active && dp.StandardValue() is DateTime && dp.Data != string.Empty;
bool DataCompare(DataPoint MH2DP, DataPoint VISDAT)
{
    if (checkDTNotEmpty(MH2DP) && checkDTNotEmpty(VISDAT))
    {
        return (DateTime)MH2DP.StandardValue() < (DateTime)VISDAT.StandardValue();
    }
    return false;
}
}
}

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
public class EC_CheckDP_IEYN_SCANDAT2 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
const string QUERY_TEXT = "Did the subject meet all eligibility criteria per Investigator's confirmation? is Yes. However, documentation of positive somatostatin receptor tumor status by PET or somatostatin receptor scintigraphy prior to Day 1(or Day 1 (2)) is not available. Please verify and update, else clarify.";
    const int MARKING_GROUP = 1;
    const bool ANSWER_ON_CHANGE = false;
    const bool CLOSE_ON_CHANGE = false;
    bool doQuery = false;

    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpAction = afp.ActionDataPoint;
    Subject subject = dpAction.Record.Subject;

    DataPoint VISDAT_D1_2 = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subject, "", "D1_2", "VIS", "VISDAT" });
    if (checkDTNotEmpty(VISDAT_D1_2))
    {
        DataPoint VIDYN_D1_2 = VISDAT_D1_2.Record.DataPage.MasterRecord.DataPoints.FindByFieldOID("VISYN");
        if (checkDPY(VIDYN_D1_2))
        {
            DataPoint IEYN_D1_2 = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subject, "", "D1_2", "IE", "IEYN" });
            if (checkDPY(IEYN_D1_2) )
            {
                DataPoint SCANDAT2_RS1 = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subject, "", "RESCREEN_1", "MH2", "SCANDAT2" });
                doQuery = DataCompare(SCANDAT2_RS1, VISDAT_D1_2 );
                CustomFunction.PerformQueryAction(QUERY_TEXT, MARKING_GROUP, ANSWER_ON_CHANGE, CLOSE_ON_CHANGE, IEYN_D1_2, doQuery, afp.CheckID, afp.CheckHash);
            }
        }
    }
    else
    {
        DataPoint IEYN_D1 = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subject, "", "D1", "IE", "IEYN" });
        DataPoint VISDAT_D1 = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subject, "", "D1", "VIS", "VISDAT" });

        if (checkDPY(IEYN_D1))
        {
            DataPoint VISDAT_RS1 = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subject, "", "RESCREEN_1", "VIS", "VISDAT" });
            if (checkDTNotEmpty(VISDAT_RS1))
            {
                DataPoint SCANDAT2_RS1 = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subject, "", "RESCREEN_1", "MH2", "SCANDAT2" });
                doQuery = DataCompare(SCANDAT2_RS1, VISDAT_D1);
                CustomFunction.PerformQueryAction(QUERY_TEXT, MARKING_GROUP, ANSWER_ON_CHANGE, CLOSE_ON_CHANGE, IEYN_D1, doQuery, afp.CheckID, afp.CheckHash);
            }
            else
            {
                DataPoint SCANDAT2_S1 = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subject, "", "SCREEN_1", "MH2", "SCANDAT2" });
                doQuery = DataCompare(SCANDAT2_S1, VISDAT_D1);
                CustomFunction.PerformQueryAction(QUERY_TEXT, MARKING_GROUP, ANSWER_ON_CHANGE, CLOSE_ON_CHANGE, IEYN_D1, doQuery, afp.CheckID, afp.CheckHash);
            }
        }
    }
    return null;
}
bool checkDPY(DataPoint dp) => dp != null && dp.Active && dp.Data == "Y";
bool checkDTNotEmpty(DataPoint dp) => dp != null && dp.Active && dp.StandardValue() is DateTime && dp.Data != string.Empty;
bool DataCompare(DataPoint MH2DP, DataPoint VISDAT)
{
    if (checkDTNotEmpty(MH2DP) && checkDTNotEmpty(VISDAT))
    {
        return (DateTime)MH2DP.StandardValue() >= (DateTime)VISDAT.StandardValue();
    }

    return false;
}
}
}

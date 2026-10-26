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
public class DateComp_CND01_VISDAT_CN_1D01_DOV : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/***********************************************
    * Programmer Name: YueXu (19 Dec 2024)
    * CF Name: DateComp_CND01.VISDAT(CN-1D01.DOV)
    * Edit check(s):DateComp_CND01.VISDAT(CN-1D01.DOV)
    * Description: Open / Close query on dpAction and
    * Modification History:
    **************************************************************************/
    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpt_Action = afp.ActionDataPoint;
    System.Collections.Generic.List<string> lsFOLDEROID = new System.Collections.Generic.List<string>()
{
    "C04D01", "C05D01", "C06D01", "C07D01", "C08D01", "C09D01", "C10D01", "C11D01", "C12D01", "C13D01", "C14D01", "C15D01", "C16D01",
    "C17D01", "C18D01", "C19D01", "C20D01", "C21D01", "C22D01", "C23D01", "C24D01", "C25D01", "C26D01", "C27D01", "C28D01", "C29D01", "C30D01"
}
    ;
    int index = lsFOLDEROID.IndexOf(dpt_Action.Record.DataPage.Instance.Folder.OID);
    if (index == -1) return null;
    subject = dpt_Action.Record.DataPage.Subject;
    string next_oid = index<26?lsFOLDEROID[index + 1]:"";
    if (index != 0)
    {
        string previous_oid = lsFOLDEROID[index - 1];
        Check(dpt_Action, previous_oid, afp);
    }
    if (next_oid != "")
    {
        DataPoints dps_next = CustomFunction.FetchAllDataPointsForOIDPath(field_oid, form_oid, next_oid, subject);
        if (dps_next != null && dps_next.Count > 0) Check(dps_next[0], dpt_Action.Record.DataPage.Instance.Folder.OID, afp);
    }

    return null;
}
const string field_oid = "VISDAT";
const string form_oid = "VISIT";
bool queryvalue = false;
Subject subject = null;
string querytext = null;
void Check(DataPoint dp_query, string previous_oid, ActionFunctionParams afp)
{
    if (dp_query != null && dp_query.Active)
    {
        DataPoints dps_previous = CustomFunction.FetchAllDataPointsForOIDPath(field_oid, form_oid, previous_oid, subject);
        querytext = dp_query.Record.Instance.Folder.OID + " \"Visit Date\" is not 23-33 days after " + previous_oid + " \"Visit Date\". Please review and correct, else clarify.";
        if (dp_query.StandardValue() is DateTime && dps_previous != null && dps_previous.Count > 0 && dps_previous[0] != null && dps_previous[0].Active && dps_previous[0].StandardValue() is DateTime)
        {
            DateTime pre_visdat = (DateTime)dps_previous[0].StandardValue();
            DateTime max_date = pre_visdat.AddDays(33);
            DateTime min_date = pre_visdat.AddDays(23);
            DateTime query_dat = (DateTime)dp_query.StandardValue();
            if (query_dat < min_date || query_dat > max_date) queryvalue = true;

        }
    }

    CustomFunction.PerformQueryAction(querytext, 1, false, false, dp_query, queryvalue, afp.CheckID, afp.CheckHash);
    queryvalue = false;
    querytext = null;
}
}
}

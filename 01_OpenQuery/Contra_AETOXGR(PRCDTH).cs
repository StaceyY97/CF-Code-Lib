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
public class Contra_AETOXGR_PRCDTH : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/***********************************************
            * Programmer Name: YueXu (17 Dec 2024)
            * CF Name: Contra_AETOXGR(PRCDTH)
            * Edit check(s):Contra_AETOXGR(PRCDTH)1,Contra_AETOXGR(PRCDTH)2
            * Description: "CTCAE Grading" is selected as "Grade 5: Fatal", however Death form is missing or "Primary cause of death" on death form does not equal "Adverse Event". Please review and correct, else clarify.
            * Modification History:
            **************************************************************************/

            const string form_oid1 = "DEATH";
            const string form_oid2 = "AE";
            const string field_oid = "PRCDTH";
            const string query_field_oid = "AETOXGR";
            const string folder_oid = "CMF";
            const string AE = "ADVERSE EVENT";
            bool check = false;
            bool queryvalue = false;
            string querytext = "\"CTCAE Grading\" is selected as \"Grade 5: Fatal\", however Death form is missing or \"Primary cause of death\" on death form does not equal \"Adverse Event\". Please review and correct, else clarify.";

            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpt_Action = afp.ActionDataPoint;
            Subject Current_Subject = dpt_Action.Record.Subject;
            DataPoints dps_query = CustomFunction.FetchAllDataPointsForOIDPath(query_field_oid, form_oid2, folder_oid, Current_Subject);
            if (!(dps_query != null && dps_query.Count > 0)) return null;
            DataPoints dps = CustomFunction.FetchAllDataPointsForOIDPath(field_oid, form_oid1, folder_oid, Current_Subject);
            if (dps != null && dps.Count > 0 && dps[0].Active && dps[0].Data == AE) check = true;
            foreach (DataPoint dp_query in dps_query)
            {
                if (dp_query != null && dp_query.Active && dp_query.Data == "5" && check == false)
                {
                    queryvalue = true;
                }
                    CustomFunction.PerformQueryAction(querytext, 1, false, false, dp_query, queryvalue, afp.CheckID, afp.CheckHash);
                    queryvalue = false;
            }
            return null;
}
}
}

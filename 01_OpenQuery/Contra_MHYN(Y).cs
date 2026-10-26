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
public class Contra_MHYN_Y : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/***********************************************
            * Programmer Name: YueXu (27 SEP 2023)
            * CF Name: Contra_MHYN(Y)
            * Edit check(s):Contra_MHYN(Y)1,Contra_MHYN(Y)2
            * Description: At least one "Medical History Term" is present when "Has the subject experienced any past and/or concomitant diseases, illnesses or past surgeries?" is "Yes"
            * Modification History:
            **************************************************************************/

            const string form_oid = "MH";
            const string field_oid = "MHTERM";
            const string query_field_oid = "MHYN";
            bool queryvalue = false;
            string querytext = "\"Has the subject experienced any past and/or concomitant diseases, illnesses or past surgeries?\" is \"Yes\", however, no Medical History Terms are recorded. Please verify and update, else clarify.";

            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpt_Action = afp.ActionDataPoint;
            Subject Current_Subject = dpt_Action.Record.Subject;
            DataPoint dp_query = dpt_Action.Record.DataPage.MasterRecord.DataPoints.FindByFieldOID(query_field_oid);
            if (dp_query == null) return null;
            if (dp_query.Data == "Y")
            {
                queryvalue = true;
                foreach (Record rcd in dpt_Action.Record.DataPage.Records)
                {
                    if (rcd.RecordPosition == 0) continue;
                    DataPoint dp = rcd.DataPoints.FindByFieldOID(field_oid);
                    if (dp != null && dp.Active && dp.Data != string.Empty)
                    {
                        queryvalue = false;
                        break;
                    }
                }

            }
            CustomFunction.PerformQueryAction(querytext, 1, false, false, dp_query, queryvalue, afp.CheckID, afp.CheckHash);
            return null;
}
}
}

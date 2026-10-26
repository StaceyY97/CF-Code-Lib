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
public class ValueComp_PRCDTH_AESDTH : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/***********************************************
            * Programmer Name: YueXu (17 Dec 2024)
            * CF Name: ValueComp_PRCDTH(AESDTH)
            * Edit check(s):ValueComp_PRCDTH(AESDTH)1,ValueComp_PRCDTH(AESDTH)
            * Description: "Primary cause of death" is "Adverse Event", however, there is not an Adverse Event where "Did the serious adverse event result in death?" is "Yes". Please review and correct, else clarify.
            * Modification History:
            **************************************************************************/

            const string form_oid1 = "AE";
            const string form_oid2 = "DEATH";
            const string field_oid = "AESDTH";
            const string query_field_oid = "PRCDTH";
            const string folder_oid = "CMF";
            bool queryvalue = false;
            string querytext = "\"Primary cause of death\" is \"Adverse Event\", however, there is not an Adverse Event where \"Did the serious adverse event result in death?\" is \"Yes\". Please review and correct, else clarify.";

            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpt_Action = afp.ActionDataPoint;
            Subject Current_Subject = dpt_Action.Record.Subject;
            DataPoints dps_query = CustomFunction.FetchAllDataPointsForOIDPath(query_field_oid, form_oid2, folder_oid, Current_Subject);
            if (!(dps_query != null && dps_query.Count > 0 && dps_query[0].Active)) return null;
            DataPoints dps = CustomFunction.FetchAllDataPointsForOIDPath(field_oid, form_oid1, folder_oid, Current_Subject);
            if (dps_query[0].Data == "ADVERSE EVENT" && dps != null)
            {
                queryvalue = true;
                foreach (DataPoint dp in dps)
                {
                    if (dp != null && dp.Active && dp.Data =="Y")
                    {
                        queryvalue = false;
                        break;
                    }
                }

            }
            CustomFunction.PerformQueryAction(querytext, 1, false, false, dps_query[0], queryvalue, afp.CheckID, afp.CheckHash);
            return null;
}
}
}

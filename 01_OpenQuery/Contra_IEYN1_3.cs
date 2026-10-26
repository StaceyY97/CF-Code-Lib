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
public class Contra_IEYN1_3 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*Custom Function: Contra_IEYN1_3
        Edit Check: Contra_IEYN1_3, Contra_IEYN1_3_A
        Description: IEYN1 = N and IETMPT <> 'RESCREEN1'*/

            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpAction = afp.ActionDataPoint;

            int markingGroup = 1;
            bool requiredResponse = false, manualClose = false, fireQuery = false;
            string queryText = "\"Did subject meet eligibility criteria during Rescreening 1?\" is \"No\", however Rescreening 1 failed criteria is missing. Please review and correct, else clarify.";

            string strNo = "N";
            if (string.Compare(dpAction.Data, strNo, true) == 0)
            {
                DataPage dpg = dpAction.Record.DataPage;
                Records rds = dpg != null && dpg.Active ? dpg.Records : new Records();

                for (int i = 0; i < rds.Count; i++)
                {
                    fireQuery = true;
                    if (rds[i] == null || !rds[i].Active || rds[i].RecordPosition == 0) continue;
                    DataPoint dpt_IETMPT = rds[i].DataPoints.FindByFieldOID("IETMPT");
                    if (dpt_IETMPT != null && dpt_IETMPT.Active)
                    {
                        if (dpt_IETMPT.Data == "RESCREEN1")
                        {
                            fireQuery = false;
                            break;
                        }

                    }
              
                }
            }
            CustomFunction.PerformQueryAction(queryText, markingGroup, requiredResponse, manualClose, dpAction, fireQuery, afp.CheckID, afp.CheckHash);
            return null;
}
}
}

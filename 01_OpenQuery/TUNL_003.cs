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
public class TUNL_003 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*

          Programmer Name: MXZ/17Jan2024
         * Custom Function: TUNL_002
         * Edit Check(s): TUNL_003
         * Descriptions: We have limit of 10 New Lesions. Fire query on extra active Logline if more than 10 active loglines.
         **************/

            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint Dpt_Action = afp.ActionDataPoint;
            Subject current_subj = Dpt_Action.Record.Subject;
            string QueryMsg = "According to Protocol, maximum number of New Target Lesions that can be entered is 10. Please verify & update.";
            int count = 0;
            DataPoint Dpt_LESNO = null;
            Records Rcds_TUNL = Dpt_Action.Record.DataPage.Records;
            bool fireQuery = false;
            if (Rcds_TUNL != null && Rcds_TUNL.Count > 0)
            {
                for (int i = 1; i < Rcds_TUNL.Count; i++)
                {
                    if (Rcds_TUNL[i].Active)
                    {
                        count++;
                        Dpt_LESNO = Rcds_TUNL[i].DataPoints.FindByFieldOID("LESNO");
                        if (Dpt_LESNO != null && Dpt_LESNO.Active && Dpt_LESNO.ChangeCount > 0)
                        {

                            fireQuery = count > 10;

                            CustomFunction.PerformQueryAction(QueryMsg, 1, false, false, Dpt_LESNO, fireQuery, afp.CheckID, afp.CheckHash);
                        }
                    }
                }
            }

            return null;
}
}
}

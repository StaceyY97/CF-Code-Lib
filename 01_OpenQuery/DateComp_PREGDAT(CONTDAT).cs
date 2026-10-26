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
public class DateComp_PREGDAT_CONTDAT : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*******************************************************
            * Created By : YueXu 2Jan2025
            * Custom Function : DateComp_PREGDAT(CONTDAT)
            * Edit Checks : DateComp_PREGDAT(CONTDAT)
            * Descreiption: QUERY
            ***************************/
            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpt_Action = afp.ActionDataPoint;
            Subject subject = dpt_Action.Record.DataPage.Subject;
            string query = "\"Date of Pregnancy Test (dd-mon-yyyy)\" is the same with none of \"Date of Follow-up (dd-mon-yyyy)\" in Safety Follow-up form. Please review and correct, else clarify.";
            bool doquery = true;
            DataPoints dps_VISDAT = CustomFunction.FetchAllDataPointsForOIDPath("PREGDAT", "PREG", dpt_Action.Record.DataPage.Instance.Folder.OID, subject);
            DataPoints dps_CONTDAT = CustomFunction.FetchAllDataPointsForOIDPath("CONTDAT", "FUP", dpt_Action.Record.DataPage.Instance.Folder.OID, subject);
            if (dps_VISDAT != null && dps_VISDAT.Count > 0 && dps_VISDAT[0].Active )
            {
                if (dps_CONTDAT != null)
                {
                    foreach (DataPoint dp in dps_CONTDAT)
                    {
                        if (dp.Active && dp.Data == dps_VISDAT[0].Data && dp.Data != "")
                        {
                            doquery = false;
                            break;
                        }
                    }
                    if (dps_VISDAT[0].Data=="") doquery = false;

                }

                CustomFunction.PerformQueryAction(query, 1, false, false, dps_VISDAT[0], doquery, afp.CheckID, afp.CheckHash);
            }

            return null;
}
}
}

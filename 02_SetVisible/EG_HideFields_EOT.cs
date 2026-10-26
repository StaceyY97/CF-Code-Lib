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
public class EG_HideFields_EOT : CustomFunctionBase
{
public object Eval(object ThisObject)
{
ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dp_SVYN = afp.ActionDataPoint;
            if (dp_SVYN.Active && dp_SVYN.Data == "Y")
            {
                Subject subject = dp_SVYN.Record.Subject;
                DataPoints dps_Cohort = CustomFunction.FetchAllDataPointsForOIDPath("COHORT", "ENROLL", "SCR", subject);
                if (dps_Cohort.Count != 1) return null;
                if (dps_Cohort[0].Data != "1" && dps_Cohort[0].Data != string.Empty)
                {

                    DataPage DP_EG = dp_SVYN.Record.DataPage.Instance.DataPages.FindByFormOID("EG");
                    DataPoint dp_EGPERF1 = DP_EG.MasterRecord.DataPoints.FindByFieldOID("EGPERF1");
                    DataPoint dp_EGCOMP = DP_EG.MasterRecord.DataPoints.FindByFieldOID("EGCOMP");
                    dp_EGPERF1.IsVisible = false;
                    dp_EGCOMP.IsVisible = false;

                }
            }
            return null;
}
}
}

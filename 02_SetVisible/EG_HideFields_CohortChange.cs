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
public class EG_HideFields_CohortChange : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/* Developer: Xiaorui Zhang
 * Date: 10 Jan 2024
 * Custom Function: CF_ET_HideFields
 * Edit Check:
 * Descrption: Hide 2 fields for ECG form in part2.
 */
             ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dp_Cohort = afp.ActionDataPoint;
            Subject subject = dp_Cohort.Record.Subject;
            if (dp_Cohort.Active && dp_Cohort.Data != "1" && dp_Cohort.Data != string.Empty)

            {


                Instances Instances = subject.Instances;
                try
                {
                    Instance inst_SCR = Instances.FindByFolderOID("SCR");
                    DataPage DP_SCREG = inst_SCR.DataPages.FindByFormOID("EG");
                    DataPoint dp_SCREGPERF1 = DP_SCREG.MasterRecord.DataPoints.FindByFieldOID("EGPERF1");
                    DataPoint dp_SCREGCOMP = DP_SCREG.MasterRecord.DataPoints.FindByFieldOID("EGCOMP");
                    dp_SCREGPERF1.IsVisible = false;
                    dp_SCREGCOMP.IsVisible = false;
                }
                catch
                {
                }

                try
                {
                    Instance inst_UNS = Instances.FindByFolderOID("UNS");
                    DataPages DP_UNSEG = inst_UNS.DataPages;
                    foreach (var i in DP_UNSEG)
                    {
                        if (i.Form.OID == "EG")
                        {
                            DataPoint dp_UNSEGPERF1 = i.MasterRecord.DataPoints.FindByFieldOID("EGPERF1");
                            DataPoint dp_UNSEGCOMP = i.MasterRecord.DataPoints.FindByFieldOID("EGCOMP");
                            dp_UNSEGPERF1.IsVisible = false;
                            dp_UNSEGCOMP.IsVisible = false;

                        }
                    }


                    
                }
                catch
                {
                }
                try
                {
                    Instance inst_EOT = Instances.FindByFolderOID("EOT");
                    DataPage DP_EOTEG = inst_EOT.DataPages.FindByFormOID("EG");
                    DataPoint dp_EOTEGPERF1 = DP_EOTEG.MasterRecord.DataPoints.FindByFieldOID("EGPERF1");
                    DataPoint dp_EOTEGCOMP = DP_EOTEG.MasterRecord.DataPoints.FindByFieldOID("EGCOMP");
                    dp_EOTEGPERF1.IsVisible = false;
                    dp_EOTEGCOMP.IsVisible = false;
                }
                catch
                {
                }
            }
            if (dp_Cohort.Active && dp_Cohort.Data == "1")
            {
                Instances Instances = subject.Instances;
                try
                {
                    Instance inst_SCR = Instances.FindByFolderOID("SCR");
                    DataPage DP_SCREG = inst_SCR.DataPages.FindByFormOID("EG");
                    DataPoint dp_SCREGPERF1 = DP_SCREG.MasterRecord.DataPoints.FindByFieldOID("EGPERF1");
                    DataPoint dp_SCREGCOMP = DP_SCREG.MasterRecord.DataPoints.FindByFieldOID("EGCOMP");
                    dp_SCREGPERF1.IsVisible = true;
                    dp_SCREGCOMP.IsVisible = true;
                }
                catch
                {
                }
                try
                {
                        Instance inst_UNS = Instances.FindByFolderOID("UNS");
                        DataPages DP_UNSEG = inst_UNS.DataPages;
                        foreach (var i in DP_UNSEG)
                        {
                            if (i.Form.OID == "EG")
                            {
                                DataPoint dp_UNSEGPERF1 = i.MasterRecord.DataPoints.FindByFieldOID("EGPERF1");
                                DataPoint dp_UNSEGCOMP = i.MasterRecord.DataPoints.FindByFieldOID("EGCOMP");
                                dp_UNSEGPERF1.IsVisible = true;
                                dp_UNSEGCOMP.IsVisible = true;

                            }
                        }
                    }
                catch
                {
                }
                try
                {
                    Instance inst_EOT = Instances.FindByFolderOID("EOT");
                    DataPage DP_EOTEG = inst_EOT.DataPages.FindByFormOID("EG");
                    DataPoint dp_EOTEGPERF1 = DP_EOTEG.MasterRecord.DataPoints.FindByFieldOID("EGPERF1");
                    DataPoint dp_EOTEGCOMP = DP_EOTEG.MasterRecord.DataPoints.FindByFieldOID("EGCOMP");
                    dp_EOTEGPERF1.IsVisible = true;
                    dp_EOTEGCOMP.IsVisible = true;
                }
                catch
                {
                }
            }
            return null;
}
}
}

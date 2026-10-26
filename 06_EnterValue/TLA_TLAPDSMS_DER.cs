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
public class TLA_TLAPDSMS_DER : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/* Custom Function: TLA_TLAPDSMS_DER
    Edit Checks: AUTO_TLAPDSMS */

    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpThis = afp.ActionDataPoint;
    Subject subj = dpThis.Record.Subject;
    DataPoint dp_enter = null, dp_enter1 = null, dp_TLANOEV = null, dp_TLAPDSMS = null, dp_TLALOCS = null;
    bool flag = false;
    double sum = 0;

    if (dpThis != null && dpThis.Active)
    {
        DataPage dpg = dpThis.Record.DataPage;
        if (dpg != null && dpg.Active)
        {
            dp_TLAPDSMS = dpg.MasterRecord.DataPoints.FindByFieldOID("TLAPDSMS");

            Records rds = dpg.Records;
            if (rds != null && rds.Count > 0 && dp_TLAPDSMS != null)
            {
                for (int i = 0; i < rds.Count; i++)
                {
                    if (rds[i].Active && i > 0)
                    {
                        if (rds[i].RecordPosition == 0) { continue; }

                        dp_TLANOEV = rds[i].DataPoints.FindByFieldOID("TLANOEV");
                        if (dp_TLANOEV != null && dp_TLANOEV.Active && dp_TLANOEV.Data == "1")
                        {
                            flag = true;
                            break;
                        }


                        dp_TLALOCS = rds[i].DataPoints.FindByFieldOID("TLALOCS");
                        if (dp_TLALOCS != null && dp_TLALOCS.UserValue() != null && dp_TLALOCS.Data != null && dp_TLALOCS.Active)
                        {

                            if (dp_TLALOCS.Data == "4")
                            {
                                dp_enter1 = rds[i].DataPoints.FindByFieldOID("TLASDIS");
                                if (dp_enter1 != null && dp_enter1.Active && double.TryParse(dp_enter1.Data, out double total1))
                                    sum = sum + Convert.ToDouble(total1);
                            }
                            else
                            {
                                dp_enter = rds[i].DataPoints.FindByFieldOID("TLALDIS");
                                if (dp_enter != null && dp_enter.Active && double.TryParse(dp_enter.Data, out double total))
                                    sum = sum + Convert.ToDouble(total);
                            }
                        }

                    }
                }
                if (flag)
                {
                    EnterData(dp_TLAPDSMS, "NE", true);
                }
                else
                {
                    if (sum > 0.0)
                    {
                        sum = Math.Round(sum, 1);
                        EnterData(dp_TLAPDSMS, sum.ToString(), true);
                    }
                    if (sum <= 0.0)
                        EnterData(dp_TLAPDSMS, "", true);
                }
            }
        }
    }
    return null;
}
void EnterData(DataPoint dp, string val, bool doFreeze)
{
    if (dp != null && dp.Active && string.Compare(dp.Data.Trim(), val, true) != 0)
    {
        dp.Unlock();
        dp.UnFreeze();
        if (dp.LockDisplayStatus == LockDisplayStatusEnum.Unlocked)
        {
            dp.Enter(val, string.Empty, 0);
            if (doFreeze) dp.Freeze();
        }
    }
}
}
}

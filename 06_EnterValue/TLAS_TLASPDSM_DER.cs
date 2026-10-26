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
public class TLAS_TLASPDSM_DER : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/* Custom Function: TLAS_TLASPDSM_DER
          Edit Checks: AUTO_TLASPDSM */

            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpThis = afp.ActionDataPoint;
            DataPoint dp_enter = null, dp_enter1 = null, dp_TLASPDSM = null;
            double sum = 0;

            if (dpThis != null && dpThis.Active)
            {
                DataPage dpg = dpThis.Record.DataPage;

                if (dpg != null && dpg.Active)
                {
                    dp_TLASPDSM = dpg.MasterRecord.DataPoints.FindByFieldOID("TLASPDSM");
                    bool frozen1 = false;

                    Records rds = dpg.Records;
                    if (rds != null && rds.Count > 0 && dp_TLASPDSM != null)
                    {
                        for (int i = 0; i < rds.Count; i++)
                        {
                            if (rds[i].Active)
                            {
                                dp_enter = rds[i].DataPoints.FindByFieldOID("TLASLDIS");
                                dp_enter1 = rds[i].DataPoints.FindByFieldOID("TLASSDIS");

                                if (dp_enter != null && dp_enter.Active && dp_enter.IsVisible&&double.TryParse(dp_enter.Data, out double total))
                                    sum = sum + Convert.ToDouble(total);
                                if (dp_enter1 != null && dp_enter1.Active && dp_enter1.IsVisible&&double.TryParse(dp_enter1.Data, out double total1))
                                    sum = sum + Convert.ToDouble(total1);
                            }
                        }
                        if (sum > 0)
                        {
                            sum = Math.Round(sum, 1);
                            if (dp_TLASPDSM != null && dp_TLASPDSM.Active && dp_TLASPDSM.Data != sum.ToString())
                            {
                                dp_TLASPDSM.UnFreeze();
                                dp_TLASPDSM.Enter(sum.ToString(), null, 0);
                                dp_TLASPDSM.Freeze();
                            }

                                
                        }
                        else
                        {
                            dp_TLASPDSM.UnFreeze();
                            dp_TLASPDSM.Enter(String.Empty, null, 0);
                            dp_TLASPDSM.Freeze();
                        }
                    }
                }
            }
            return null;
}
}
}

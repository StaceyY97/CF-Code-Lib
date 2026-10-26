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
public class AUTO_SAE3DAT : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/***********************************************
     * Programmer Name: YueXu (25 Dec 2024)
     * CF Name: AUTO_SAE3DAT
     * Edit check(s):AUTO_SAE3DAT1, AUTO_SAE3DAT2
     * Description: 
     * Modification History:
     **************************************************************************/
    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpt_Action = afp.ActionDataPoint;
    Subject subject = dpt_Action.Record.DataPage.Subject;
    DataPoints dps = CustomFunction.FetchAllDataPointsForOIDPath("EXSTDAT", "EX", "SDA", subject);
    String value = Getmin(dps);
    DataPoints dps_SAE3DAT = CustomFunction.FetchAllDataPointsForOIDPath("SAE3DAT", "AE", "CMF", subject);
    foreach (DataPoint dp in dps_SAE3DAT)
    {
        if (dp != null && dp.Active && dp.IsVisible) EnterData(dp, value, true);
        if (dp != null && dp.Active && !dp.IsVisible) EnterData(dp, "", true);
    }
        return null;
}

String Getmin(DataPoints dps)
{
    DateTime dt = DateTime.MaxValue;
    string str_dp = "";
    foreach (DataPoint dp in dps)
    {
        if (dp!=null && dp.Active && dp.StandardValue() is DateTime)
        {
            DateTime dt_dp = (DateTime)dp.StandardValue();
            if (dt_dp < dt)
            {
                dt = dt_dp;
                str_dp = dp.Data;
            }
        }
    }
    return str_dp;
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

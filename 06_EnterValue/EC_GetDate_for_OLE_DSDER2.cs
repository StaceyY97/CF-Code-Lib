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
public class EC_GetDate_for_OLE_DSDER2 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
* Created By :Qian Yang/23Dec2024
* Custom Function :EC_GetDate_for_OLE_DSDER2
* Description: Get and enter the earlist date between two fields.
* Edit Checks :Get_DSDER2, Get_DSDER2_2, Merge_OLE_EOT
*********************************************************************************************************/
 ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint dpAction = afp.ActionDataPoint;
    Subject sj = dpAction.Record.Subject;
    Instance currentFolder = dpAction.Record.Instance;

    DataPoint checkD1 = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { sj, "", "EOT", "DS2", "DSDAT2" });
    DataPoint checkDP_2 = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { sj, "", "OLEET", "VIS", "VISDAT" });

    DataPoint enterDP = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { sj, "", "EOT", "DS2", "DSDER2" });

    if (checkD1 != null && checkD1.Active)
    {
        if (checkDP_2 == null || !checkDP_2.Active || checkDP_2.Data == string.Empty) EnterData(enterDP, checkD1.Data);
        else if (checkD1.Data == string.Empty) EnterData(enterDP, checkDP_2.Data);
        else
        {
            if (checkDP_2.StandardValue() is DateTime && checkD1.StandardValue() is DateTime)
            {
                if ((DateTime)checkD1.StandardValue() > (DateTime)checkDP_2.StandardValue()) EnterData(enterDP, checkDP_2.Data);
                else EnterData(enterDP, checkD1.Data);
            }
        }
    }

    return null;

}

        public void EnterData(DataPoint dp, string val)
        {
            if (dp != null && dp.Active &&!dp.IsDataPointLocked &&string.Compare(dp.Data, val, true) != 0)
            {
                dp.UnFreeze();
                dp.Enter(val, string.Empty, 0);
                dp.Freeze();
            }
}
}
}

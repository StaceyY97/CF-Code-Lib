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
public class AUTO_PCGLTO : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*******************************************************
         * Created By : YueXu 26Nov2024
         * Custom Function : AUTO_PCGLTO
         * Edit Checks : Related ECs
         * Descreiption: Derive value
         ***************************/

            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpAction = afp.ActionDataPoint;
            Subject subject = dpAction.Record.Subject;
            Instance curIns = dpAction.Record.DataPage.Instance;
            string FIELD_1 = "PCGLPR";
            string FIELD_2 = "PCGLSC";
            string FIELD_3 = "PCGLTO";
            DataPoint dp1 = dpAction.Record.DataPoints.FindByFieldOID(FIELD_1);
            DataPoint dp2 = dpAction.Record.DataPoints.FindByFieldOID(FIELD_2);
            DataPoint dp3 = dpAction.Record.DataPoints.FindByFieldOID(FIELD_3);
            if (dp3 == null || !dp3.Active) return null;
            if (dp1 != null && dp2 != null && dp1.Active && dp2.Active && dp1.Data != "6" && dp1.Data != "7" && dp1.Data != "" && dp2.Data != "6" && dp2.Data != "7" && dp2.Data != "")
            {
                int int_dp1 = int.Parse(dp1.Data);
                int int_dp2 = int.Parse(dp2.Data);
                int sum = int_dp1 + int_dp2;
                EnterData(dp3, sum.ToString(), true);
            }
            else EnterData(dp3, "", true);
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

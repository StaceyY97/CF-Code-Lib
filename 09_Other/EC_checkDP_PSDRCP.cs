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
public class EC_checkDP_PSDRCP : CustomFunctionBase
{
public object Eval(object ThisObject)
{
//Created Date:17Feb2025
DataPoint dpAction = (DataPoint)ThisObject;

DataPoint tempDP_Group1_1 = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { dpAction.Record.Subject, "", "SCREEN_1", "PDIARY", "PDLAYN" });
DataPoint tempDP_Group1_2 = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { dpAction.Record.Subject, "", "RESCREEN_1", "PDIARY", "PDLAYN" });
DataPoint tempDP_Group2_1 = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { dpAction.Record.Subject, "", "D1", "VIS", "VISDAT" });
DataPoint tempDP_Group2_2 = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { dpAction.Record.Subject, "", "D1_2", "VIS", "VISDAT" });

return ((tempDP_Group1_1 != null && tempDP_Group1_1.Active && tempDP_Group1_1.Data == "Y") || 
    (tempDP_Group1_2 != null && tempDP_Group1_2.Active && tempDP_Group1_2.Data == "Y")) &&
    ((tempDP_Group2_1 != null && tempDP_Group2_1.Active && tempDP_Group2_1.Data != String.Empty&& tempDP_Group2_1.StandardValue() is DateTime)||
    (tempDP_Group2_2 != null && tempDP_Group2_2.Active && tempDP_Group2_2.Data != String.Empty && tempDP_Group2_2.StandardValue() is DateTime));
}
}
}

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
public class EC_checkDP_OLEFU_DAT_FILL : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
* Created By :Qian Yang/21Dec2024
* Custom Function :EC_checkDP_OLEFU_DAT_FILL
* Description: "Date of Treatment Discontinuation/Completion" is filled on End of Treatment - OLE form
* Edit Checks :Merge_OLE_FU, Merge_OLE_FU_2
*********************************************************************************************************/

    DataPoint dpAction = (DataPoint)ThisObject;
    Subject sj = dpAction.Record.Subject;
    Instance currentFolder = dpAction.Record.Instance;

    DataPoint checkDP;
    if (currentFolder.Folder.OID == "OLEET") checkDP = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { sj, "", "EOT", "DS2", "DSDAT2" });
    else if (currentFolder.Folder.OID == "EOT") checkDP = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { sj, "", "OLEET", "VIS", "VISDAT" });
    else return false;

    return checkDP != null && checkDP.Active && checkDP.Data != String.Empty;
}
}
}

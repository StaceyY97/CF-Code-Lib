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
public class CF_RCP_To_OLE_MapList : CustomFunctionBase
{
public object Eval(object ThisObject)
{
object[] inputParameters = (object[]) ThisObject;

    Dictionary<string, List<string>> deriveFields = new Dictionary<string, List<string>>
{
    { "AE2" ,new List<string> {"AE2PID", "AE2TERM", "AE2STDAT", "AE2RELCS", "AE2SER", "AE2SERCR_DEATH", "AE2SERCR_LT",
        "AE2SERCR_HOSP", "AE2SERCR_DIS", "AE2SERCR_BD", "AE2SERCR_SIGME", "AE2SEV", "AE2REL",
        "AE2REL_AT", "AE2ACN", "AE2ACNOTHYN", "AE2CONTRT", "AE2ACNOTH_PR", "AE2DIS"  }},
    { "CM2" ,new List<string> {"CM2PID", "CM2TRT", "CM2STDAT", "CM2DU_AE", "CM2DU_MH", "CM2DU_CS", "CM2DU_PROPH",
        "CM2DU_AT", "CM2DU", "CM2OTH1", "CM2PRO1", "CM2AT1", "CM2RM", "CM2DOSE", "CM2DOSU",
        "CM2OTH2", "CM2ROUTE", "CM2OTH3", "CM2DOSFRQ", "CM2OTH4" }},
    { "NP2" ,new List<string> {"NP2PID", "NP2TERM", "NP2STDAT", "NP2INDC", "NP2INDC2", "NP2INDC3", "NP2INDC4",
        "NP2INDC5", "NP2OTH"  }}
};

return deriveFields;
}
}
}

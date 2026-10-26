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
public class CF_SAE_FieldLevel : CustomFunctionBase
{
public object Eval(object ThisObject)
{
object[] inputParameters = (object[]) ThisObject;
                    Dictionary<string, List<string>> S = new Dictionary<string, List<string>>()
{
    { "AESER", new List<string> { "AETERM", "AESTDAT", "AEONGO", "AERELCS", "AESER", "AESEV", "AEREL", "AEACN", "AEACNOTHYN", "AEOUT", "AEDIS","AEREL_AT" }},
    {"AEENDAT", new List<string> { "AEENDAT" } },
    { "AESERCR_DEATH", new List<string> { "AESERCR_DEATH", "AESERCR_LT", "AESERCR_HOSP", "AESERCR_DIS", "AESERCR_BD", "AESERCR_SIGME" }},
    {"AECONTRT", new List<string> { "AECONTRT", "AEACNOTH_PR" } },
    {"AEOUT_SEQUELAE_SP", new List<string> { "AEOUT_SEQUELAE_SP" } },
    {"AEREAS1", new List<string> { "AEREAS1", "AEREAS3", "AEREAS4", "AEREAS5","AEREAS6"} },
    {"AEREAS2", new List<string> { "AEREAS2", "AEREAS7", "AEREAS8", "AEREAS9", "AEREAS10"}},
    { "AE2SER", new List<string> { "AE2_PID","AE2TERM", "AE2STDAT", "AE2ONGO", "AE2RELCS", "AE2SER", "AE2SEV", "AE2REL", "AE2ACN", "AE2ACNOTHYN", "AE2OUT", "AE2DIS","AE2REL_AT" }},
    {"AE2ENDAT", new List<string> { "AE2ENDAT" } },
    { "AE2SERCR_DEATH", new List<string> { "AE2SERCR_DEATH", "AE2SERCR_LT", "AE2SERCR_HOSP", "AE2SERCR_DIS", "AE2SERCR_BD", "AE2SERCR_SIGME" }},
    {"AE2CONTRT", new List<string> { "AE2CONTRT", "AE2ACNOTH_PR" } },
    {"AE2OUT_SEQUELAE_SP", new List<string> { "AE2OUT_SEQUELAE_SP" } },
    {"AE2REAS1", new List<string> { "AE2REAS1", "AE2REAS3", "AE2REAS4", "AE2REAS5","AE2REAS6"} },
    {"AE2REAS2", new List<string> { "AE2REAS2", "AE2REAS7", "AE2REAS8", "AE2REAS9", "AE2REAS10"}},
{"AE2REL_ATR", new List<string> { "AE2REL_ATR", "AE2REL_ATR2", "AE2REL_ATR3", "AE2REL_ATR4", "AE2REL_ATR5"}},
{"AEREL_ATR", new List<string> { "AEREL_ATR", "AEREL_ATR2", "AEREL_ATR3", "AEREL_ATR4", "AEREL_ATR5"}},
};

        return S;
}
}
}

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
public class CF_Aid_GetDSL_Key : CustomFunctionBase
{
public object Eval(object ThisObject)
{
object[] inputParameters = (object[])ThisObject;
        string entetField = (string)inputParameters[0];

        List<string> AERealted = new List<string>
        {
           "AETERM","AE2TERM","DSAE", "DSAE1", "DSAE2", "DSAE3", "OAAE", "OAAE2", "OAAE3", "OAAE4", "OAAE5",
            "CMAE1", "CMAE2", "CMAE3", "CMAE4", "CMAE5", "NPAE", "NPAE2", "NPAE3", "NPAE4", "NPAE5", "EOSAE1", "EOSAE2", "DTHAE",
            "CM2AE1", "CM2AE2", "CM2AE3", "CM2AE4", "CM2AE5", "NP2AE", "NP2AE2", "NP2AE3", "NP2AE4", "NP2AE5"
        }
;

        List<string> MHRealted = new List<string>
        {
            "MHTERM","CMMH", "CMMH2", "CMMH3", "CMMH4", "CMMH5", "OAMH", "OAMH2", "OAMH3", "OAMH4", "OAMH5",
            "NPMH", "NPMH2", "NPMH3", "NPMH4", "NPMH5","NP2MH", "NP2MH2", "NP2MH3", "NP2MH4", "NP2MH5",
"CM2MH", "CM2MH2", "CM2MH3", "CM2MH4", "CM2MH5"
        }
        ;

        List<string> CMRealted = new List<string>
        {
           "CMTRT","CM2TRT", "AEREAS1", "AEREAS3", "AEREAS4", "AEREAS5", "AEREAS6", "AEREL_ATR", "AEREL_ATR2", "AEREL_ATR3", "AEREL_ATR4", "AEREL_ATR5",
             "AE2REAS1", "AE2REAS3", "AE2REAS4", "AE2REAS5", "AE2REAS6", "AE2REL_ATR", "AE2REL_ATR2", "AE2REL_ATR3", "AE2REL_ATR4", "AE2REL_ATR5"
        }
        ;

        List<string> NPRealted = new List<string>
        {
            "NPTERM","NP2TERM","AEREAS2", "AEREAS7", "AEREAS8", "AEREAS9", "AEREAS10",
             "AE2REAS2", "AE2REAS7", "AE2REAS8", "AE2REAS9", "AE2REAS10"
        }
        ;
        Dictionary<string, List<string>> allDSLs = new Dictionary<string, List<string>>
        {
            {"AE", AERealted },
            {"MH", MHRealted },
            {"CM", CMRealted },
            {"NP",NPRealted }
        };


        foreach (string matchKey in allDSLs.Keys)
        {
            if (allDSLs[matchKey].Contains(entetField)) return new Dictionary<string, List<string>> { { matchKey, allDSLs[matchKey] } };
        }

        return null;
}
}
}

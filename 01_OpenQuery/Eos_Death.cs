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
public class Eos_Death : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:**************************
 * CF Name: Eos_Death
* Programmer Name:TCW(31 Dec 2024)
* Last Modified: Not Applicable
* EC:Contra_EOSRSN(DEATH missed)2
* Reason Modified: Not Applicable
* Description: Death form must be entered when "Primary Reason for End of Study" equals "Death".**/

ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
DataPoint dp = afp.ActionDataPoint;
Subject subj = dp.Record.Subject;
Instance curIns = dp.Record.DataPage.Instance;


DataPage Death;
string[] Death_Fields = { "DTHDAT", "PRCDTH", "AUTOPERF", "DTHCERT" };
bool fireQuery = true;
string querytext = "Primary Reason for End of Study\" is recorded as \"Death\"; however, Death form is missing. Please review and correct, else clarify.";

if (dp != null && dp.Active && dp.UserValue() != null && dp.Data != null && dp.Data == "DEATH")
{
    Instance Common = subj.Instances.FindByFolderOID("CMF");
    if (Common != null && Common.Active)
    {
        Death = Common.DataPages.FindByFormOID("DEATH");
        if (Death != null && Death.Active)
        {
            foreach (string field in Death_Fields)
            {
                DataPoint Death_dp = Death.MasterRecord.DataPoints.FindByFieldOID(field);
                if (Death_dp != null && Death_dp.Data != null && !(Death_dp.EntryStatus == EntryStatusEnum.EnteredEmpty) && Death_dp.Data != string.Empty)
                {
                    fireQuery = false;
                    break;
                }
            }
        }
    }
}
else { fireQuery = false; }
CustomFunction.PerformQueryAction(querytext, 1, false, false, dp, fireQuery, afp.CheckID, afp.CheckHash);
return null;
}
}
}

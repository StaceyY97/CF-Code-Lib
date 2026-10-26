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
public class Contra_AEDIS_EOS : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:*********
* CF Name: Contra_AEDIS_EOS
* EC:Contra_AEDIS_EOS
* Programmer Name:TianciWen (2 Jan 2025)
* Last Modified: Not Applicable
* Reason Modified: Not Applicable
* Description: "Did the adverse event cause the subject to be discontinued from the study?" is Yes and End of study form is not filled. **/

ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
DataPoint dp = afp.ActionDataPoint;
Subject subj = dp.Record.Subject;
Instance curIns = dp.Record.DataPage.Instance;


DataPage EOS_FORM;
string[] Eos_Fields = { "EOSDAT", "EOSRSN" };
bool fireQuery = true;
string querytext = "\"Did the adverse event cause the subject to be discontinued from the study?\" is recorded as \"Yes\", however, End of study form is missing. Please review and correct, else clarify.";

if (dp != null && dp.Active && dp.UserValue() != null && dp.Data != null && dp.Data == "Y")
{
    Instance EOS_FOLDER = subj.Instances.FindByFolderOID("EOS");
    if (EOS_FOLDER != null && EOS_FOLDER.Active)
    {
        EOS_FORM = EOS_FOLDER.DataPages.FindByFormOID("EOS");
        if (EOS_FORM != null && EOS_FORM.Active)
        {
            foreach (string field in Eos_Fields)
            {
                DataPoint Eos_dp = EOS_FORM.MasterRecord.DataPoints.FindByFieldOID(field);
                if (Eos_dp != null && Eos_dp.Data != null && !(Eos_dp.EntryStatus == EntryStatusEnum.EnteredEmpty) && Eos_dp.Data != string.Empty)
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

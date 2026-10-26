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
public class UpdateFolderN_TASSMT1 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:**************************
  * CF Name: UpdateFolderN_TASSMT1
  * Programmer Name:TCW(06Jan 2025)/QY(05Sep2025)/QY(10Dec2025)
  * Last Modified: 10Dec2025
  * Reason Modified: Add Bone TPTP for naming range
  * Description: update folder name by value entered into TPT**/
  ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
  DataPoint dp = afp.ActionDataPoint;
  Subject subj = dp.Record.Subject;
  Instance currentIns = dp.Record.DataPage.Instance;

  if (dp == null) return null;

  DataPoint GetSimpleDP(string fieldOID, string formOID) => currentIns.DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints.FindByFieldOID(fieldOID);
  string GetSuffix(DataPoint tempDP) => tempDP.Field.OID.EndsWith("O") ? tempDP.Data : tempDP.Data == "OTH" ? "" : tempDP.UserValue().ToString();

  List<string> sourceFields = new List<string> { "TLATPTO", "TLATPT", "NTLATPTO", "NTLATPT", "BLTPTO", "BLTPT" };

       bool matchOne = false;
       for (int i = 0; i < sourceFields.Count; i++)
       {
           string formOID = sourceFields[i].StartsWith("T") ? "TLA" : "NTL";
           DataPoint tempDP = GetSimpleDP(sourceFields[i], formOID);
           if (tempDP != null && tempDP.Active&& tempDP.IsVisible && tempDP.Data != string.Empty)
           {
               renameFolder(currentIns, GetSuffix(tempDP));
               matchOne = true;
               break;
           }
       }
       if (!matchOne) renameFolder(currentIns, "");
       renameFolder(currentIns?.ParentInstance, "");

       void renameFolder(Instance ins, string suffix)
       {
           if (ins != null && ins.Active)
           {
               ins.SetInstanceName(suffix);
           }
       }
       return null;
}
}
}

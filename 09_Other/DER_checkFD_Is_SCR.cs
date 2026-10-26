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
public class DER_checkFD_Is_SCR : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
* Created By :Qian Yang/16Jan2025
* Custom Function :DER_checkFD_Is_SCR
* Description:Check is current folder Screening 1 or Screening 2 or Day 1
* Edit Checks :Derive_SDRSDER1
*********************************************************************************************************/

 DataPoint dpAction = (DataPoint)ThisObject;
 Instance currentFolder = dpAction.Record.Instance;

 DateTime dtLast = DateTime.MinValue;
 List<string> checkFolders = new List<string>() { "SCREEN_1", "SCREEN_2", "D1" };
 DataPoints checkDPs = new DataPoints();
 if (dpAction != null && dpAction.Active && dpAction.StandardValue() is DateTime&&checkFolders.Contains(currentFolder.Folder.OID)) checkDPs.Add(dpAction);


 foreach (string folder in checkFolders)
 {
     if (folder != currentFolder.Folder.OID)
     {
         DataPoint tempDP = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { dpAction.Record.Subject, "", folder, "IE", "IEFDAT" });
         if (tempDP != null &&tempDP.Active && tempDP.StandardValue() is DateTime) checkDPs.Add(tempDP);
     }
 }

 DataPoint dp_enter = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { dpAction.Record.Subject, "", "SCREEN_1", "SDRS", "SDRSDER1" });
 if (checkDPs.Count == 0) return dp_enter != null ?"": null;

 if (dp_enter != null && dp_enter.Active)
 {
     foreach (DataPoint dp in checkDPs)
     {
         dtLast = (DateTime)dp.StandardValue()> dtLast? (DateTime)dp.StandardValue(): dtLast;
     }
 }
 else return null;

 return dtLast;
}
}
}

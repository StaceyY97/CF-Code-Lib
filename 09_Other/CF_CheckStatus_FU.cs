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
public class CF_CheckStatus_FU : CustomFunctionBase
{
public object Eval(object ThisObject)
{
object[] inputParameters = (object[]) ThisObject;
DataPoint dpAction = (DataPoint)inputParameters[0];
      Instance currentIns = dpAction.Record.Instance;

      List<string> Folders = new List<string>
  {
      "WK16", "ET", "OLEET", "OLEWK104"
  }
      ;
      if (!Folders.Contains(currentIns.Folder.OID)) return false;

      List<string> Folders_VISDAT = new List<string>
  {
       "ET", "OLEET", "OLEWK104"
  }
   ;
      List<string> Folders_VIS2ROYN = new List<string>
  {
      "WK16"
  }
;

      bool enterCheck = false;
      switch (dpAction.Field.OID)
      {
          case "VISDAT":
              enterCheck = Folders_VISDAT.Contains(currentIns.Folder.OID) && dpAction.Active && dpAction.StandardValue() is DateTime && dpAction.Data != string.Empty;
              break;
          case "VIS2ROYN":
              enterCheck = Folders_VIS2ROYN.Contains(currentIns.Folder.OID)&&dpAction.Active && dpAction.Data == "N";
              break;
      }
      return enterCheck;
}
}
}

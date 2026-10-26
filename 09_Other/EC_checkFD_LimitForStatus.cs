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
public class EC_checkFD_LimitForStatus : CustomFunctionBase
{
public object Eval(object ThisObject)
{
DataPoint dpAction = (DataPoint) ThisObject;
        Instance currentFolder = dpAction.Record.Instance;
if(dpAction .Field.OID == "CONTYN"){
List<string> Folders_CONTYN = new List<string> { "D1","D1_2","WK2", "WK4", "WK8","WK12","OLED1", "OLEWK2","OLEWK6", "OLEWK12", "OLEWK24", "OLEWK36", "OLEWK48","OLEWK60", "OLEWK72", "OLEWK84"};
return Folders_CONTYN.Contains(currentFolder.Folder.OID);
}
else if(dpAction .Field.OID == "VISDAT")
{
List<string> Folders_VISDAT = new List<string> { "OLED1","WK16", "ET", "OLEET", "OLEWK104"};
return Folders_VISDAT .Contains(currentFolder.Folder.OID);
}

return false;
}
}
}

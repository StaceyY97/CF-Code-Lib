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
public class EX_022 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
 DataPoint dpAction = afp.ActionDataPoint;
 Subject subject = dpAction.Record.Subject;
 const string folderOID = "C1D1";
 const string parentFolderOID = "TRT";
 const string formOID = "EX";

 if (dpAction.Record.DataPage.Instance.Folder.OID != "SCR") return null;

 Instance objIns_Parent = subject.Instances.FindByFolderOID(parentFolderOID);
 if (objIns_Parent == null || !objIns_Parent.Active) return null;
 Instance objIns = objIns_Parent.Instances.FindByFolderOID(folderOID);
 if (objIns == null || !objIns.Active) return null;
 DataPage objDPage = objIns.DataPages.FindByFormOID(formOID);
 if (objDPage == null || !objDPage.Active) return null;
 string[] actionFeilds = { "EXDSDEL", "EXDSRED", "EXDSINC" };

 foreach (string actionDP_OID in actionFeilds)
 {
     DataPoint actionDP = objDPage.MasterRecord.DataPoints.FindByFieldOID(actionDP_OID);
     if (actionDP != null && actionDP.IsVisible == true) actionDP.IsVisible = false;
 }
 return null;
}
}
}

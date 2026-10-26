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
public class EG_007 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
    Developer    : Qian Yang
    Version Date : 04Mar2024 
    Fuction Name : EGTPT_Disable  
    Edit Check   : Discription:"Disable a single log field in some folders"
    ******************************************************************/

    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpAction = afp.ActionDataPoint;
    Subject subject = dpAction.Record.Subject;
    ///define value input field
    string formOid = "EG";
    string fieldOid_TimePoint = "EGTPT";
    string[] applyFolder = new string[] { "SCR", "EOT", "UNS" };
    ArrayList apFolders = new ArrayList(applyFolder);
 
    foreach(string folderOID in applyFolder)
    {
        DataPoints allTPTs = CustomFunction.FetchAllDataPointsForOIDPath(fieldOid_TimePoint, formOid, folderOID, subject);
        if (allTPTs == null) return null;

        foreach (DataPoint EGTPT in allTPTs)
        {
            EnterData(EGTPT, "");
        }
    }

    return true;
}

void EnterData(DataPoint dp, string val)
{
    if (dp != null && dp.Active)
    {
        if (dp.LockStatus != LockStatusEnum.Locked)
        {
            dp.Enter(val, string.Empty, 0);
            dp.Freeze();
        }
    }
}
}
}

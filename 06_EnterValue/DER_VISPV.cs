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
public class DER_VISPV : CustomFunctionBase
{
public object Eval(object ThisObject)
{
ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
DataPoint dpAction = afp.ActionDataPoint;
Subject subject = dpAction?.Record.Subject;

if (dpAction == null && subject == null) return null;

DataPoints dps_filled = dpAction.Field.OID == "VISDAT" ? 
    new DataPoints() { dpAction}
    :CustomFunction.FetchAllDataPointsForOIDPath("VISDAT", "VISIT",null,subject);

foreach(DataPoint dp_fill in dps_filled)
{
     if(dp_fill == null) continue;
    string curVersion = string.Empty;
    if(DoCheck(dp_fill,true))
    {
        curVersion = (string)CustomFunction.PerformCustomFunction("CF_GetICFVersion", dpAction.Field.CRFVersion.ID, new object[] { subject, (DateTime)dp_fill.StandardValue() });
    }
    EnterToDP_SameRC(dp_fill, "VISPV", curVersion);
}

void EnterToDP_SameRC(DataPoint dp, string pvOID, string enterValue)
{
    DataPoint dpPV = dp.Record.DataPoints.FindByFieldOID(pvOID);
    if (dpPV != null && string.Compare(dpPV.Data, enterValue, true) != 0)
    {
        dpPV.UnFreeze();
        dpPV.Enter(enterValue, string.Empty, 0);
        dpPV.Freeze();
    }
}

bool DoCheck(DataPoint dp, bool IsDate, bool checkEmpty = true) => IsDate ? IsValidDp(dp, checkEmpty) && IsValidDate(dp) : IsValidDp(dp, checkEmpty);
bool IsValidDp(DataPoint dp, bool checkEmpty) => dp != null && dp.Active && dp.IsVisible && (checkEmpty ? dp.Data != string.Empty : true);
bool IsValidDate(DataPoint dp) => dp != null && dp.Active && dp.IsVisible && (dp.StandardValue() is DateTime || dp.StandardValue() is TimeSpan);
return null;
}
}
}

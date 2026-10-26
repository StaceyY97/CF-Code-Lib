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
public class EC_DoDynamic_PRIMAG : CustomFunctionBase
{
public object Eval(object ThisObject)
{
ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint dp = afp.ActionDataPoint;
        Subject sj = dp.Record.Subject;
List<string> allowedIns = new List<string>{"OLEWK2","OLEWK24","OLEWK48","OLEWK72","OLEWK104","OLEET","UNA"};
if(!allowedIns.Contains(dp.Record.Instance?.Folder.OID))return null;

        string GetUserValueByCodedValue(DataPoint targetDP, string codedValue) => targetDP?.Field.Variable.DataDictionary?.Entries.FirstOrDefault(e => e.Key.Equals(codedValue)) ?.Value ?? string.Empty;
        DataPoint checkDP = dp.Record.DataPoints?.FindByFieldOID("PRIYN");
        DataPoint manageDP = dp.Record.DataPoints?.FindByFieldOID("PRIDISPR");
        if (manageDP != null)
        {
            bool shouldVisible = checkDP.Data == "Y";
            string querytext = "\"" + checkDP.Field.PreText + "\" is not recorded as " + "\"" + GetUserValueByCodedValue(checkDP, "Y") + "\", but \"" + manageDP.Field.PreText + "\" is present. Please verify and update, else clarify.";
            manageVisibleAndQuery(shouldVisible, querytext);
        }
        void manageVisibleAndQuery(bool shouldVisible, string querytext)
        {
            manageDP.IsVisible = shouldVisible || manageDP.Data != string.Empty;
            bool hasQuery = !shouldVisible && manageDP.Data != string.Empty;
            CustomFunction.PerformQueryAction(querytext, 1, false, false, manageDP, hasQuery, afp.CheckID, afp.CheckHash);
        }
        return null;
}
}
}

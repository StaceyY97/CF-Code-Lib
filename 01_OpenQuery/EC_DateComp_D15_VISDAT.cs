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
public class EC_DateComp_D15_VISDAT : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:**************************
* CF Name: EC_DateComp_D15_VISDAT
* Programmer Name:Qian Yang 04Jul025
* Last Modified: NA
* Reason Modified: NA
* Description: Compare D15 Date with D1 date.
* **/

        ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
        DataPoint dpAction = afp.ActionDataPoint;
        Subject subject = dpAction.Record.Subject;
        bool fireQuery = false;
        string querytext = "";

        DataPoint GetSimpleDP(string fieldOID, string formOID, string folderOID) =>
    subject.Instances.FindByFolderOID(folderOID)?.DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints.FindByFieldOID(fieldOID);

        Instance currentIns = dpAction?.Record.Instance;
        if (int.TryParse(currentIns.Folder.OID.Substring(1, 2), out int cycleNum))
        {
            if (currentIns.Folder.OID.EndsWith("1")|| cycleNum<=4) return null;
            querytext = $"Cycle {cycleNum.ToString()} Day 15 \"Visit Date\" is not 12-16 days after Cycle {cycleNum.ToString()} Day 1 \"Visit Date\". Please review and correct, else clarify.";

            string cycleNum_str = cycleNum < 10 ? ("0"+ cycleNum.ToString()) : cycleNum.ToString();
            DataPoint D1_VISDAT = GetSimpleDP("VISDAT", "VISIT", "C" + cycleNum_str + "D01");
            if (D1_VISDAT != null && D1_VISDAT.Active && D1_VISDAT.StandardValue() is DateTime && (dpAction.StandardValue() is DateTime))
            {
                DateTime dt1 = (DateTime)dpAction.StandardValue();
                DateTime dt2 = (DateTime)D1_VISDAT.StandardValue();
                fireQuery = dt1 < dt2.AddDays(12) || dt1 > dt2.AddDays(16);
            }
        }

        if (querytext != "") CustomFunction.PerformQueryAction(querytext, 1, false, false, dpAction, fireQuery, afp.CheckID, afp.CheckHash);
        return null;
}
}
}

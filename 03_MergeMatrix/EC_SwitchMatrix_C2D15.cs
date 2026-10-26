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
public class EC_SwitchMatrix_C2D15 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:**************************
* CF Name: EC_SwitchMatrix_C2D15
* Programmer Name:Qian Yang 16Jun2025
* Last Modified: Not Applicable
* Reason Modified: Not Applicable
* Description: Switch Matrix based on part and cycles of status 
* **/

ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
DataPoint ActionDP = afp.ActionDataPoint;
Subject subject = ActionDP.Record.Subject;

DataPoint GetSimpleDP(string fieldOID, string formOID, string folderOID) =>
    subject.Instances.FindByFolderOID(folderOID)?.DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints.FindByFieldOID(fieldOID);        

string[] allManageMatrix = new string[] { "C2D15","CYCLE2DAY15"};
string[] matrixMatch = new string[] { };

DataPoint status_C1D22 = GetSimpleDP("STATUS", "STATUS","C01D22");
DataPoint status_C1D16 = GetSimpleDP("STATUS", "STATUS", "C01D16");
DataPoint part = GetSimpleDP("STPART", "DSENR", "SCRN");

switch (part?.Data)
{
    case "1"://Part 1A
        if(status_C1D22!=null&& status_C1D22.Data=="Y") matrixMatch = new string[] { "C2D15" };
        break;
    case "2"://Part 2A
        if((status_C1D16 != null && status_C1D16.Data == "Y")) matrixMatch = new string[] { "C2D15","CYCLE2DAY15"};
        break;
    case "3"://Part 1B
        if (status_C1D22 != null && status_C1D22.Data == "Y") matrixMatch = new string[] { "C2D15" };
        break;
    case "4"://Part 2B
        if (status_C1D22 != null && status_C1D22.Data == "Y") matrixMatch = new string[] { "C2D15" };
        break;
case "5"://Part 1C
        if (status_C1D22 != null && status_C1D22.Data == "Y") matrixMatch = new string[] { "C2D15" };
        break;
    default:
        break;
}

foreach (string matrixOID in matrixMatch)
{
    Matrix manageMatrix = Matrix.FetchByOID(matrixOID, subject.CRFVersion.ID);
    subject.MergeMatrix(manageMatrix);
}

foreach(string matrixOID in allManageMatrix)
{
    if (!matrixMatch.Contains(matrixOID))
    {
        Matrix manageMatrix = Matrix.FetchByOID(matrixOID, subject.CRFVersion.ID);
        subject.UnMergeMatrix(manageMatrix);
    }
}

return null;
}
}
}

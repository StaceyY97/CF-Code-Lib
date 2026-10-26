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
public class DYN_CF_DoMerge_C2D1 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:**************************
        * CF Name: DYN_DoMerge_C2D1
        * Programmer Name:Qian Yang(12Nov2025)
        * Last Modified: NA.
        * Reason Modified: NA.
        * Description: Add C2D1, based on value of cohort and Next YN     
**/


///below should be defined by programmer
        System.Collections.Generic.Dictionary<string, string> cohortsMatch = new System.Collections.Generic.Dictionary<string, string>()
        {
            {"1", "C1D15"},

            {"2A", "C1D15"},

            {"1B", "C1D22"},

            {"2B","C1D22"},

            {"2D","C1D22"},

            {"3A", "C1D22"},

            {"1C", "C1D8"},

            {"2C", "C1D8"},

            {"2E","C1D8"},

            {"3B", "C1D8"},
        };

        string[] cohort = new string[] { "COHORT", "ENROLL", "SCR" };
        string[] IsContinue = new string[] { "DSCONT", "VISNX" };
        string InsOID_Parent = "TRT";
        string matchValue = "Y";

        ///above should be defined by programmer


        DataPoint dp = (DataPoint)ThisObject;
        if (dp == null) return null;

        string fieldOID = dp.Field.OID;
        Subject curSub = dp.Record.Instance.Subject;
        Instance currentIns = dp.Record.Instance;
        string folderOID = currentIns.Folder.OID;
        Instance parentIns = curSub.Instances.FindByFolderOID(InsOID_Parent);


        if (curSub == null || parentIns == null) return null;

        string subjectCohort = fieldOID == IsContinue[0] ? GetSimpleDP(cohort[0], cohort[1], cohort[2])?.Data : dp.Data;

        try
        {
            if (cohortsMatch.ContainsKey(subjectCohort))
            {
                string checkFodler = cohortsMatch[subjectCohort];
                if (folderOID == checkFodler)
                {
                    return dp.Data == matchValue;
                }
                else
                {
                    DataPoint checkDP = GetSimpleDP_parent(IsContinue[0], IsContinue[1], checkFodler, InsOID_Parent);
                    return checkDP?.Data == matchValue;
                }
            }

        }
        catch { }
        return false;

        DataPoint GetSimpleDP(string field_OID, string formOID, string folder_OID) =>
curSub?.Instances.FindByFolderOID(folder_OID)?.DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints.FindByFieldOID(field_OID);
        DataPoint GetSimpleDP_parent(string field_OID, string formOID, string folder_OID, string parent) =>
    curSub?.Instances.FindByFolderOID(parent)?.Instances.FindByFolderOID(folder_OID).DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints.FindByFieldOID(field_OID);
}
}
}

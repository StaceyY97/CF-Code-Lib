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
public class CF_SetTarget_Cohort : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/// <summary>
        /// CF Name: CF_SetTarget_Cohort
        /// Programmer Name: QianYang(08 Dec 2025)
        /// Last Modified: Not Applicable
        /// Reason Modified: Not Applicable
        /// Description: Set target date of all aviliable instances by value of cohort
        /// </summary>

        ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
        DataPoint dptAction = afp.ActionDataPoint;
        subject = dptAction.Record.Subject;

        DataPoint cohort = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dptAction.Field.CRFVersion.ID, new object[] { subject, "", "SCR", "ENROLL", "COHORT" });
        Instance TRT = subject.Instances.FindByFolderOID("TRT");
        if (TRT == null) return null;

        Instances allTRTIns = TRT.Instances;
        if (allTRTIns.Count < 1) return null;

        DataPoint DOV_C1D1 = GetSimpleDP(index_DOV_C1D1[0], index_DOV_C1D1[1], index_DOV_C1D1[2], index_DOV_C1D1[3]);
        if (DOV_C1D1 != null && DOV_C1D1.Active && DOV_C1D1.Data != string.Empty && DOV_C1D1.StandardValue() is DateTime) targetZero = (DateTime)DOV_C1D1.StandardValue();
        if (targetZero == DateTime.MinValue) return null;

        if (Q2W.Contains(cohort.Data)) { cycles = targetCycles_Q2W; days = targetDays_Q2W; }
        else if (Q3W.Contains(cohort.Data)) { cycles = targetCycles_Q3W; days = targetDays_Q3W; }
        else if (Q3W_1.Contains(cohort.Data)) { cycles = targetCycles_Q3W1; days = targetDays_Q3W1; }
        else if (Q4W.Contains(cohort.Data)) { cycles = targetCycles_Q4W; days = targetDays_Q4W; }
        else return null;

        for (int i = 0; i < cycles.Length; i++) Cycle_Day_Match[cycles[i]] = days[i];

        foreach (Instance checkIns in allTRTIns)
        {
            if (checkIns == null || !checkIns.Active) continue;

            string folderOID = checkIns.Folder.OID;
            int index_C = folderOID.IndexOf(FolderPrefix_cycle);
            int index_D = folderOID.IndexOf(FolderPrefix_day);
            if (int.TryParse(folderOID.Substring(index_C + 1, index_D - index_C - 1), out int cycle_Number) && int.TryParse(folderOID.Substring(index_D + 1), out int day_Number))
            {
                DateTime window = GetWindow(day_Number, cycle_Number);
                if (window != DateTime.MinValue) checkIns.Target = window;
            }
        }
        return null;
    }


    /// <summary>
    /// Parameters to be define by programmer
    /// </summary>
    ArrayList Q3W = new ArrayList(new string[] {  "2A" });
    ArrayList Q3W_1 = new ArrayList(new string[] { "1"});

    ArrayList Q2W = new ArrayList(new string[] { "1C", "2C", "2E", "3B" });
    ArrayList Q4W = new ArrayList(new string[] { "1B", "2B", "2D", "3A" });
    string FolderPrefix_cycle = "C", FolderPrefix_day = "D";

    int[] targetCycles_Q3W = new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35 };
    int[] targetDays_Q3W = new int[] { 21, 42, 63, 84, 105, 126, 147, 168, 189, 210, 231, 252, 273, 294, 315, 336, 357, 378, 399, 420, 441, 462, 483, 504, 525, 546, 567, 588, 609, 630, 651, 672, 693, 714 };
    int[] targetCycles_Q3W1 = new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35 , 36, 37, 38, 39, 40, 41, 42, 43, 44,
        45, 46, 47, 48, 49, 50, 51, 52, 53 };
    int[] targetDays_Q3W1 = new int[] { 21, 42, 63, 84, 105, 126, 147, 168, 189, 210, 231, 252, 273, 294, 315, 336, 357, 378, 399, 420, 441, 462, 483, 504, 525, 546, 567, 588, 609, 630, 651, 672, 693, 714,
        735, 756, 777, 798, 819, 840, 861, 882, 903, 924, 945, 966, 987, 1008, 1029, 1050, 1071, 1092 };
    int[] targetCycles_Q2W = new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45,
        46, 47, 48, 49, 50, 51, 52, 53 };
    int[] targetDays_Q2W = new int[]{ 14, 28, 42, 56, 70, 84, 98, 112, 126, 140, 154, 168, 182, 196, 210, 224, 238, 252, 266, 280, 294, 308, 322, 336, 350, 364, 378, 392, 406, 420, 434, 448, 462, 476,
490, 504, 518, 532, 546, 560, 574, 588, 602, 616, 630, 644, 658, 672, 686, 700, 714, 728 };
    int[] targetCycles_Q4W = new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27 };
    int[] targetDays_Q4W = { 28, 56, 84, 112, 140, 168, 196, 224, 252, 280, 308, 336, 364, 392, 420, 448, 476, 504, 532, 560, 588, 616, 644, 672, 700, 728 };

    public static readonly string[] index_DOV_C1D1 = new string[] { "VISDAT", "VISIT", "C1D1", "TRT" };

    /// <summary>
    /// DO NOT MODIOFY: Public objects to be used across CF
    /// </summary>
    Subject subject = null;
    int[] cycles = new int[] { };
    int[] days = new int[] { };
    System.Collections.Generic.Dictionary<int, int> Cycle_Day_Match = new System.Collections.Generic.Dictionary<int, int>();
    DateTime targetZero = DateTime.MinValue;
    DataPoint GetSimpleDP(string fieldOID, string formOID, string folderOID, string parent) =>
    subject?.Instances?.FindByFolderOID(parent)?.Instances?.FindByFolderOID(folderOID)?.DataPages?.FindByFormOID(formOID)?.MasterRecord?.DataPoints?.FindByFieldOID(fieldOID);

    DateTime GetWindow(int day_Number, int cycle_Number)
    {
        DateTime date_Return = DateTime.MinValue;
        if (targetZero != DateTime.MinValue && Cycle_Day_Match.ContainsKey(cycle_Number))
        {
            int addDays = Cycle_Day_Match[cycle_Number] + day_Number - 1;
            date_Return = targetZero.Add(new TimeSpan(addDays, 0, 0, 0));
        }
        return date_Return;
}
}
}

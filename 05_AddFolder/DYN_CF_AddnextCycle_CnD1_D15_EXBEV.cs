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
public class DYN_CF_AddnextCycle_CnD1_D15_EXBEV : CustomFunctionBase
{
public object Eval(object ThisObject)
{
ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint dp = afp.ActionDataPoint;
        if (dp == null) return null;

        curSub = dp.Record.Instance.Subject;
        Instance currentIns = dp.Record.Instance;
        string folderOID = currentIns.Folder.OID;
        string fieldOID = dp.Field.OID;
        parentIns = curSub.Instances.FindByFolderOID(InsOID_Parent);

        if (curSub == null || parentIns == null) return null;

        CVID = curSub.CRFVersionID;
        SMID = dp.Record.SubjectMatrixID;
        string subjectCohort = fieldOID == IsContinue[0] ? GetSimpleDP(cohort[0], cohort[1], cohort[2])?.Data : dp.Data;

        if (cohortsMatch.ContainsKey(subjectCohort))
        {
            CycleInfor[] checkCycles = cohortsMatch[subjectCohort];
            if (checkCycles.Length > 0)
            {
                for (int i = 0; i < checkCycles.Length; i++)
                {
                    int lowRange_Check = checkCycles[i].ExtrsFolderToCheck != string.Empty ? checkCycles[i].CycleRange[0] + 1 : checkCycles[i].CycleRange[0];
                    int upperRange_Check = checkCycles[i].CycleRange[1];
                    int lowRange_Add = checkCycles[i].IsAddNextCycle ? checkCycles[i].CycleRange[0] + 1 : checkCycles[i].CycleRange[0];
                    int upperRange_Add = checkCycles[i].IsAddNextCycle ? upperRange_Check + 1 : upperRange_Check;
                    string[] targetForms = checkCycles[i].Forms;

                    System.Collections.Generic.List<string> InsToCheck = GetInsList(lowRange_Check, upperRange_Check, checkCycles[i].DayNumberToCheck, checkCycles[i].ExtrsFolderToCheck);
                     System.Collections.Generic.List<string> InsToAdd = GetInsList(lowRange_Add, upperRange_Add, checkCycles[i].DayNumberToAdd, string.Empty);

                    if (fieldOID == IsContinue[0])
                    {
                        if (InsToCheck.Contains(folderOID))
                        {
                            bool doAdd = dp.Data == matchValue;
                            string targetFodler = InsToAdd[InsToCheck.IndexOf(folderOID)];
                            Instance targetIns = parentIns.Instances.FindByFolderOID(targetFodler);
if (curSub.Name == "2051-QY07") Message.SendEmail("qian.yang@edetek.com", "test_qy_2@mdsol.com", "test_qy", "Check_1_"+(targetIns==null).ToString());
                            for (int j = 0; j < targetForms.Length; j++) CustomFunction.PerformCustomFunction("CF_Aid_AddForm", dp.Field.CRFVersion.ID, new object[] { targetIns, doAdd, targetForms[j] });
                        }
                    }
                    else
                    {
                        foreach (string Ins in InsToCheck)
                        {
                            DataPoint checkDP = GetSimpleDP(IsContinue[0], IsContinue[1], Ins, InsOID_Parent);
                            if (checkDP != null)
                            {
                                bool doAdd = checkDP.Data == matchValue;
                                string targetFodler = InsToAdd[InsToCheck.IndexOf(Ins)];
                                Instance targetIns = parentIns.Instances.FindByFolderOID(targetFodler);
                                for (int j = 0; j < targetForms.Length; j++) CustomFunction.PerformCustomFunction("CF_Aid_AddForm", dp.Field.CRFVersion.ID, 
                                    new object[] { targetIns, doAdd, targetForms[j] });
                            }
                        }
                    }
                }


            }
        }
        if (fieldOID != IsContinue[0])
        {
            CycleInfor[] invCycle = new CycleInfor[0] { };
            if (cohortsMatch.ContainsKey(subjectCohort)) { invCycle = cohortsMatch[subjectCohort]; }
            System.Collections.Generic.Dictionary<string, int[]> checkCycleDic = new System.Collections.Generic.Dictionary<string, int[]> { };
            for (int i = 0; i < invCycle.Length; i++) checkCycleDic.Add(invCycle[i].DayNumberToAdd, invCycle[i].CycleRange);
            CustomFunction.PerformCustomFunction("CF_Aid_CleanExtraCycle", dp.Field.CRFVersion.ID, new object[] { curSub, checkCycleDic, formsType_Commen });
        }

        return null;

    }
    /// Below Basical parameters, need to define by programmer
    public struct CycleType
    {
        /// <summary>
        /// 3-26
        /// </summary>
        public static readonly int[] cycleType_1 = new int[] { 3, 26 };
        /// <summary>
        /// 3-52
        /// </summary>
        public static readonly int[] cycleType_2 = new int[] { 3, 52 };
        /// <summary>
        /// 4-27
        /// </summary>
        public static readonly int[] cycleType_3 = new int[] { 4, 27 };
    }

    public static readonly string[] formsType_Commen = new string[] { "EXBEV" };

    System.Collections.Generic.Dictionary<string, CycleInfor[]> cohortsMatch = new System.Collections.Generic.Dictionary<string, CycleInfor[]>()
    {
        {"2D", new CycleInfor[] {new CycleInfor { CycleRange = CycleType.cycleType_1, DayNumberToCheck = "D15", DayNumberToAdd = "D1"},
                                new CycleInfor { CycleRange = CycleType.cycleType_3, DayNumberToCheck = "D1", DayNumberToAdd = "D15",IsAddNextCycle = false } } },

        {"3A", new CycleInfor[] {new CycleInfor { CycleRange = CycleType.cycleType_1, DayNumberToCheck = "D15", DayNumberToAdd = "D1" },
                                new CycleInfor { CycleRange = CycleType.cycleType_3, DayNumberToCheck = "D1", DayNumberToAdd = "D15" ,IsAddNextCycle = false } } },

        {"2E", new CycleInfor[] {new CycleInfor { CycleRange = CycleType.cycleType_2, DayNumberToCheck = "D1", DayNumberToAdd = "D1",ExtrsFolderToCheck = "C3D8" } } },

        {"3B", new CycleInfor[] {new CycleInfor { CycleRange = CycleType.cycleType_2, DayNumberToCheck = "D1", DayNumberToAdd = "D1", ExtrsFolderToCheck = "C3D8"} } },

    };

    public static readonly string[] cohort = new string[] { "COHORT", "ENROLL", "SCR" };
    public static readonly string[] IsContinue = new string[] { "DSCONT", "VISNX" };
    string FolderPrefix_cycle = "C", FolderPrefix_day = "D";
    string InsOID_Parent = "TRT";
    string matchValue = "Y";
    /// Above Basical parameters, need to define by programmer

    Subject curSub;
    int CVID, SMID;
    Instance parentIns;
    DataPoint GetSimpleDP(string fieldOID, string formOID, string folderOID) =>
curSub?.Instances?.FindByFolderOID(folderOID)?.DataPages?.FindByFormOID(formOID)?.MasterRecord?.DataPoints?.FindByFieldOID(fieldOID);
    DataPoint GetSimpleDP(string fieldOID, string formOID, string folderOID, string parent) =>
curSub?.Instances?.FindByFolderOID(parent)?.Instances?.FindByFolderOID(folderOID)?.DataPages?.FindByFormOID(formOID)?.MasterRecord?.DataPoints?.FindByFieldOID(fieldOID);
        
    System.Collections.Generic.List<string> GetInsList(int lowRange, int upperRange, string dayNumber, string extraInsToCheck = "")
    {
        System.Collections.Generic.List<string> output = new System.Collections.Generic.List<string>();
        if (extraInsToCheck != "") output.Add(extraInsToCheck);
        for (int i = lowRange; i <= upperRange; i++)
        {
            output.Add("C" + i.ToString() + dayNumber);
        }
        return output;
    }
    public class CycleInfor
    {
        public string[] Forms { get; set; } = formsType_Commen;
        public int[] CycleRange { get; set; }
        public string DayNumberToCheck { get; set; }
        public string ExtrsFolderToCheck { get; set; } = string.Empty;
        public string DayNumberToAdd { get; set; }

        public bool IsAddNextCycle { get; set; } = true;
}
}
}

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
public class DYN_CF_AddnextCycle_CnD1_D15 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dp = afp.ActionDataPoint;
            if (dp == null) return null;
            int CFID = dp.Field.CRFVersion.ID;

            curSub = dp.Record.Instance.Subject;
            Instance currentIns = dp.Record.Instance;
            string folderOID = currentIns.Folder.OID;
            string fieldOID = dp.Field.OID;
            CVID = curSub.CRFVersionID; SMID = dp.Record.SubjectMatrixID;

            string cohort = GetSimpleDP("COHORT", "ENROLL", "SCR")?.Data;
            if (cohortsMatch.ContainsKey(cohort))
            {
                cycinfo[] cycs = cohortsMatch[cohort];
                if (cycs.Length > 0)
                {
                    if (fieldOID == "DSCONT")
                    {
                        for (int i = 0; i < cycs.Length; i++)
                        {
                            cycinfo cycle = cycs[i];
                            if (cycle.DayNumberForC3 != folderOID && !folderOID.EndsWith(cycle.DayNumber)) continue;
                            int index_C = folderOID.IndexOf("C"), index_D = folderOID.IndexOf("D");
                            if (int.TryParse(folderOID.Substring(index_C + 1, index_D - index_C - 1), out int cNum) && cNum >= cycle.CycleRange[0] && cNum <= cycle.CycleRange[1])
                            {
                                bool doContinue = cNum == 3 ? cycle.DayNumberForC3 == folderOID : true;
                                if (doContinue) DoMerge(dp, cycle, cNum,folderOID);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < cycs.Length; i++)
                        {
                            cycinfo cycle = cycs[i];
                            for (int cycleNumber = cycle.CycleRange[0]; cycleNumber < cycle.CycleRange[1] + 1; cycleNumber++)
                            {
                                string checkFolder = cycleNumber == 3 && cycle.DayNumberForC3 != string.Empty ? cycle.DayNumberForC3 : "C" + cycleNumber.ToString() + cycle.DayNumber;
                                DataPoint checkDP = GetSimpleDP("DSCONT", "VISNX", checkFolder, "TRT");
                                if (checkDP != null) DoMerge(checkDP, cycle, cycleNumber,checkFolder);
                            }
                        }
                        System.Collections.Generic.Dictionary<string, int[]> checkCycleDic = new System.Collections.Generic.Dictionary<string, int[]> { };
                        for (int i = 0; i < cycs.Length; i++) checkCycleDic.Add(cycs[i].FolderSuf, cycs[i].CycleRange);
                        CustomFunction.PerformCustomFunction("CF_Aid_CleanExtraCycle", CFID, new object[] { curSub, checkCycleDic, formsType_Commen });
                    }
                }
            }
            void DoMerge(DataPoint checkDP, cycinfo cycle, int cycleNumber,string str_flder)
            {
                int nextCycle = cycle.FolderSuf == "D15" ? cycleNumber : cycleNumber + 1;
                string targetFolder = "C" + nextCycle.ToString() + cycle.FolderSuf;
                string[] targetForms = cycleNumber == cycle.CycleRange[1] ? cycle.Forms_End : cycle.Forms;
                bool doAdd = checkDP.Data == "Y";
                if (doAdd&&str_flder == "C3D15")
                {
                    DataPoint dp_VISPV = GetSimpleDP("VISPV", "VISIT", str_flder,"TRT");
                    doAdd = int.TryParse(dp_VISPV?.Data.Replace("V", ""), out int vNum) &&  vNum != 0 && vNum < 7;
if (curSub.Name == "C501-QY01") Message.SendEmail("qian.yang@edetek.com", "test_qy_2@mdsol.com", "test_qy", doAdd.ToString());
                }
                CustomFunction.PerformCustomFunction("CF_Aid_AddnextCycle", CFID, new object[] { curSub, SMID, "TRT", targetFolder, doAdd, targetForms });
            }
            return null;
        }
        /// Below Basical parameters, need to define by programmer
        public static readonly int[] cycleType_1 = new int[] { 3, 34 };
        public static readonly int[] cycleType_2 = new int[] { 3, 26 };
        public static readonly int[] cycleType_3 = new int[] { 3, 52 };
        public static readonly int[] cycleType_4 = new int[] { 4, 27 };

        public static readonly string[] formsType_Commen = new string[] { "VISIT", "EX", "VISNX" };
        public static readonly string[] formsType_Commen_End = new string[] { "VISIT", "EX" };
        public static readonly string[] formsType_D15 = new string[] { "VISIT", "VISNX", "EXBEV" };
        public static readonly string[] formsType_D15_End = new string[] { "VISIT", "EXBEV" };
        public static readonly string[] formsType_EXBEV = new string[] { "VISIT", "EX", "EXBEV", "VISNX" };
        public static readonly string[] formsType_EXBEV_End = new string[] { "VISIT", "EX", "EXBEV" };


        System.Collections.Generic.Dictionary<string, cycinfo[]> cohortsMatch = new System.Collections.Generic.Dictionary<string, cycinfo[]>()
{
    {"1", new cycinfo[] { new cycinfo { DayNumberForC3 = "C3D15", CycleRange = cycleType_3 } }},

    {"2A", new cycinfo[] {new cycinfo { DayNumberForC3 = "C3D15", CycleRange = cycleType_1 } }},

    {"1B", new cycinfo[] {new cycinfo { DayNumberForC3 = "C3D8" } }},

    {"2B", new cycinfo[] {new cycinfo {  DayNumberForC3 = "C3D8"} }},

    {"2D", new cycinfo[] {new cycinfo { FolderSuf = "D15",  Forms =formsType_D15 ,Forms_End = formsType_D15_End,CycleRange = cycleType_4 },
        new cycinfo {DayNumberForC3 = "C3D15",  Forms =formsType_EXBEV ,DayNumber = "D15", Forms_End = formsType_EXBEV } }},

    {"3A", new cycinfo[] {new cycinfo { FolderSuf = "D15",  Forms =formsType_D15 ,Forms_End = formsType_D15_End,CycleRange = cycleType_4},
        new cycinfo {DayNumberForC3 = "C3D15",  Forms =formsType_EXBEV ,DayNumber = "D15", Forms_End = formsType_EXBEV } }},

    {"1C", new cycinfo[] {new cycinfo { DayNumberForC3 = "C3D8", CycleRange = cycleType_3 } }},

    {"2C", new cycinfo[] {new cycinfo { DayNumberForC3 = "C3D8", CycleRange = cycleType_3 } }},

    {"2E", new cycinfo[] {new cycinfo { DayNumberForC3 = "C3D8", CycleRange = cycleType_3, Forms = formsType_EXBEV , Forms_End = formsType_EXBEV_End } }},

    {"3B", new cycinfo[] {new cycinfo { DayNumberForC3 = "C3D8", CycleRange = cycleType_3, Forms = formsType_EXBEV, Forms_End = formsType_EXBEV_End } }},
};

        /// Above Basical parameters, need to define by programmer
        Subject curSub;
        int CVID, SMID;
        DataPoint GetSimpleDP(string fieldOID, string formOID, string folderOID) =>
    curSub?.Instances?.FindByFolderOID(folderOID)?.DataPages?.FindByFormOID(formOID)?.MasterRecord?.DataPoints?.FindByFieldOID(fieldOID);
        DataPoint GetSimpleDP(string fieldOID, string formOID, string folderOID, string parent) =>
    curSub?.Instances?.FindByFolderOID(parent)?.Instances?.FindByFolderOID(folderOID)?.DataPages?.FindByFormOID(formOID)?.MasterRecord?.DataPoints?.FindByFieldOID(fieldOID);
        public class cycinfo
        {
            public int[] CycleRange { get; set; } = cycleType_2;
            public string DayNumber { get; set; } = "D1";
            public string DayNumberForC3 { get; set; } = string.Empty;
            public string FolderSuf { get; set; } = "D1";
            public string[] Forms { get; set; } = formsType_Commen;
            public string[] Forms_End { get; set; } = formsType_Commen_End;
}
}
}

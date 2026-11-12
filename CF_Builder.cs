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
    using Medidata.Interfaces.Users;
    using Mono.CSharp;
    using System.Diagnostics;
    using System.Security.Cryptography;
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

    /// <summary>
    /// Sample CustomFunction.
    /// </summary>
    public class CustomFunction1 : CustomFunctionBase
    {
        public object Eval(object ThisObject)
        {
            IDataPoint dp = Rave.Context.ActionDataPoint;

            if (dp == null) return null;
            CrfVersionRef CFID = dp.Field.CrfVersionRef;

            curSub = dp.Record.Instance.Subject;
            SMID = dp.Record.SubjectMatrixRef;

            string cohort = GetSimpleDP("COHORT", "ENROLL", "SCR")?.Data;
            if (cohortsMatch.ContainsKey(cohort))
            {
                cycinfo[] cycs = cohortsMatch[cohort];
                if (cycs.Length > 0)
                {
                    // Same flow for every trigger field: walk each cycle, find its visit,
                    // decide whether the next visit is triggered, then add it.
                    for (int i = 0; i < cycs.Length; i++)
                    {
                        cycinfo cycle = cycs[i];
                        for (int cycleNumber = cycle.CycleRange[0]; cycleNumber < cycle.CycleRange[1] + 1; cycleNumber++)
                        {
                            string checkFolder = cycleNumber == 3 && cycle.DayNumberForC3 != string.Empty ? cycle.DayNumberForC3 : "C" + cycleNumber.ToString() + cycle.DayNumber;
                            IDataPoint checkDP = GetSimpleDP("DSCONT", "VISNX", checkFolder, "TRT");
                            if (checkDP != null) DoMerge(checkDP, cycle, cycleNumber, checkFolder);
                        }
                    }
                    // Remove cycles beyond the cohort's ranges
                    Dictionary<string, int[]> checkCycleDic = new Dictionary<string, int[]> { };
                    for (int i = 0; i < cycs.Length; i++) checkCycleDic.Add(cycs[i].FolderSuf, cycs[i].CycleRange);
                    Rave.PerformCustomFunction("CF_Aid_CleanExtraCycle", CFID, new object[] { curSub, checkCycleDic, formsType_Commen });
                }
            }
            void DoMerge(IDataPoint checkDP, cycinfo cycle, int cycleNumber, string str_flder)
            {
                int nextCycle = cycle.FolderSuf == "D15" ? cycleNumber : cycleNumber + 1;
                string targetFolder = "C" + nextCycle.ToString() + cycle.FolderSuf;
                string[] targetForms = cycleNumber == cycle.CycleRange[1] ? cycle.Forms_End : cycle.Forms;
                bool doAdd = checkDP.Data == "Y";
                if (doAdd && new List<string> { "1B", "2B", "2D", "3A" }.Contains(cohort) && (str_flder == "C3D15" || str_flder == "C3D8"))
                {
                    IDataPoint dp_VISPV = GetSimpleDP("PRTVR", "DSCONS", "SCR");
                    doAdd = int.TryParse(dp_VISPV?.Data.Replace("V", ""), out int vNum) && vNum != 0 && vNum < 7;
                }
                Rave.PerformCustomFunction("CF_Aid_AddnextCycle", CFID, new object[] { curSub, SMID, "TRT", targetFolder, doAdd, targetForms });
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


        Dictionary<string, cycinfo[]> cohortsMatch = new Dictionary<string, cycinfo[]>()
{
    {"1", new cycinfo[] { new cycinfo { CycleRange = cycleType_3, DayNumber = "D1", DayNumberForC3 = "C3D15", FolderSuf = "D1", Forms = formsType_Commen, Forms_End = formsType_Commen_End } }},

    {"2A", new cycinfo[] { new cycinfo { CycleRange = cycleType_1, DayNumber = "D1", DayNumberForC3 = "C3D15", FolderSuf = "D1", Forms = formsType_Commen, Forms_End = formsType_Commen_End } }},

    {"1B", new cycinfo[] { new cycinfo { CycleRange = cycleType_2, DayNumber = "D1", DayNumberForC3 = "C3D8", FolderSuf = "D1", Forms = formsType_Commen, Forms_End = formsType_Commen_End } }},

    {"2B", new cycinfo[] { new cycinfo { CycleRange = cycleType_2, DayNumber = "D1", DayNumberForC3 = "C3D8", FolderSuf = "D1", Forms = formsType_Commen, Forms_End = formsType_Commen_End } }},

    {"2D", new cycinfo[] { new cycinfo { CycleRange = cycleType_4, DayNumber = "D1", DayNumberForC3 = string.Empty, FolderSuf = "D15", Forms = formsType_D15, Forms_End = formsType_D15_End },
        new cycinfo { CycleRange = cycleType_2, DayNumber = "D15", DayNumberForC3 = "C3D15", FolderSuf = "D1", Forms = formsType_EXBEV, Forms_End = formsType_EXBEV } }},

    {"3A", new cycinfo[] { new cycinfo { CycleRange = cycleType_4, DayNumber = "D1", DayNumberForC3 = string.Empty, FolderSuf = "D15", Forms = formsType_D15, Forms_End = formsType_D15_End },
        new cycinfo { CycleRange = cycleType_2, DayNumber = "D15", DayNumberForC3 = "C3D15", FolderSuf = "D1", Forms = formsType_EXBEV, Forms_End = formsType_EXBEV } }},

    {"1C", new cycinfo[] { new cycinfo { CycleRange = cycleType_3, DayNumber = "D1", DayNumberForC3 = "C3D8", FolderSuf = "D1", Forms = formsType_Commen, Forms_End = formsType_Commen_End } }},

    {"2C", new cycinfo[] { new cycinfo { CycleRange = cycleType_3, DayNumber = "D1", DayNumberForC3 = "C3D8", FolderSuf = "D1", Forms = formsType_Commen, Forms_End = formsType_Commen_End } }},

    {"2E", new cycinfo[] { new cycinfo { CycleRange = cycleType_3, DayNumber = "D1", DayNumberForC3 = "C3D8", FolderSuf = "D1", Forms = formsType_EXBEV, Forms_End = formsType_EXBEV_End } }},

    {"3B", new cycinfo[] { new cycinfo { CycleRange = cycleType_3, DayNumber = "D1", DayNumberForC3 = "C3D8", FolderSuf = "D1", Forms = formsType_EXBEV, Forms_End = formsType_EXBEV_End } }},

    {"2F", new cycinfo[] { new cycinfo { CycleRange = cycleType_3, DayNumber = "D1", DayNumberForC3 = "C3D8", FolderSuf = "D1", Forms = formsType_Commen, Forms_End = formsType_Commen_End } }},

    {"2I", new cycinfo[] { new cycinfo { CycleRange = cycleType_2, DayNumber = "D15", DayNumberForC3 = "C3D22", FolderSuf = "D1", Forms = formsType_Commen, Forms_End = formsType_Commen_End } }},

    {"2G", new cycinfo[] { new cycinfo { CycleRange = cycleType_1, DayNumber = "D15", DayNumberForC3 = "C3D15", FolderSuf = "D1", Forms = formsType_Commen, Forms_End = formsType_Commen_End } }},

    {"2K", new cycinfo[] { new cycinfo { CycleRange = cycleType_3, DayNumber = "D1", DayNumberForC3 = "C3D8", FolderSuf = "D1", Forms = formsType_Commen, Forms_End = formsType_Commen_End } }},

    {"2J", new cycinfo[] { new cycinfo { CycleRange = cycleType_2, DayNumber = "D15", DayNumberForC3 = "C3D22", FolderSuf = "D1", Forms = formsType_Commen, Forms_End = formsType_Commen_End } }},
};

        /// Above Basical parameters, need to define by programmer
        ISubject curSub;
        SubjectMatrixRef SMID;
        IDataPoint GetSimpleDP(string fieldOID, string formOID, string folderOID) =>
    curSub?.Instances?.FindByFolderOid(folderOID)?.DataPages?.FindByFormOid(formOID)?.MasterRecord?.AllDataPoints?.FindByFieldOid(fieldOID);
        IDataPoint GetSimpleDP(string fieldOID, string formOID, string folderOID, string parent) =>
    curSub?.Instances?.FindByFolderOid(parent)?.Instances?.FindByFolderOid(folderOID)?.DataPages?.FindByFormOid(formOID)?.MasterRecord?.DataPoints?.FindByFieldOid(fieldOID);
        public class cycinfo
        {
            public int[] CycleRange { get; set; }
            public string DayNumber { get; set; }
            public string DayNumberForC3 { get; set; }
            public string FolderSuf { get; set; }
            public string[] Forms { get; set; }
            public string[] Forms_End { get; set; }
        }
    }
}

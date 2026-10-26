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
public class DYN_CF_AddnextCycle_SFMn : CustomFunctionBase
{
public object Eval(object ThisObject)
{
///<summary>
    ///CF: DYN_CF_AddnextCycle_SFMn Qian Yang 26Aug2026
    ///Add next SFM visit by prior visit
    ///</summary>

    enterDP = Rave.Context.ActionDataPoint;
    if (enterDP == null) return null;

    curSub = enterDP.Record.Subject;
    IDataPoint dp_cohort = GetDP("COHORT", "ENROLL", "SCR", curSub);
    CohortType type = CohortType.None;
    if (IsValidDp(dp_cohort, true)) type = GetType(dp_cohort.Data);

    if (type != CohortType.None)
    {
        IReadOnlyList<IDataPoint> dps_visnx = curSub.GetDataPointsByOidPath("DSCONT", "VISNX", null);

        int last_visyn = -1;
        List<string> Folder_seq = Cohorts_Match[type];

        foreach (IDataPoint dp_visnx in dps_visnx)
        {
            if (!IsValidDp(dp_visnx, true) || !dp_visnx.Record.Instance.Folder.OID.StartsWith(InsOID_Parent)) continue;

            int idx = Folder_seq.IndexOf(dp_visnx.Record.Instance?.Name.Trim());
            if (idx >= 0 && dp_visnx.Data == str_doAdd_trigger) last_visyn = Math.Max(last_visyn, idx);
            if (curSub.Name == "C501-QY02") Rave.Messaging.SendEmail("qian.yang@edetek.com", "test_qy_2@mdsol.com", "test_qy", $"Check_1_{dp_visnx.Record.Instance?.Name}_{dp_visnx.Data}_{idx >= 0}_{dp_visnx.Data == str_doAdd_trigger}");
        }
        if (last_visyn < Folder_seq.Count - 1)
        {
            string[] targetForms = last_visyn == Folder_seq.Count - 2 ? forms_End : forms_Common;
            AddFolder(Folder_seq, last_visyn + 1, targetForms);
        }
    }
    return null;
}
Dictionary<CohortType, List<string>> Cohorts_Match = new Dictionary<CohortType, List<string>>()
{
    {
        CohortType.Q3W,
        new List<string>
        {
            "Safety Monitoring Cycle 1 Day 8",
            "Safety Monitoring Cycle 1 Day 15",
            "Safety Monitoring Cycle 2 Day 8",
            "Safety Monitoring Cycle 2 Day 15",
            "Safety Monitoring Cycle 3 Day 8",
            "Safety Monitoring Cycle 3 Day 15",
            "Safety Monitoring Cycle 4 Day 8",
            "Safety Monitoring Cycle 4 Day 15"
        }
    },
    {
        CohortType.Q2W,
        new List<string>
        {
            "Safety Monitoring Cycle 1 Day 8",
            "Safety Monitoring Cycle 2 Day 8",
            "Safety Monitoring Cycle 3 Day 8",
            "Safety Monitoring Cycle 4 Day 8",
            "Safety Monitoring Cycle 5 Day 8",
            "Safety Monitoring Cycle 6 Day 8"
        }
    },
    {
        CohortType.Q4W,
        new List<string>
        {
            "Safety Monitoring Cycle 1 Day 8",
            "Safety Monitoring Cycle 1 Day 15",
            "Safety Monitoring Cycle 1 Day 22",
            "Safety Monitoring Cycle 2 Day 8",
            "Safety Monitoring Cycle 2 Day 15",
            "Safety Monitoring Cycle 2 Day 22",
            "Safety Monitoring Cycle 3 Day 8",
            "Safety Monitoring Cycle 3 Day 15",
            "Safety Monitoring Cycle 3 Day 22"
        }
    }
};

string InsOID_Parent = "SFM", InsOID = "SFMN", header = "Safety Monitoring Cycle";
string[] forms_Common = new string[] { "TELC", "VISNX" };
string[] forms_End = new string[] { "TELC" };

string str_doAdd_trigger = "Y";

IDataPoint enterDP = null;
ISubject curSub = null;
IDataPoint GetDP(string field_Oid, string form_Oid, string folder_Oid, ISubject subj) => subj.Instances.FindByFolderOid(folder_Oid)?.DataPages.FindByFormOid(form_Oid)?.MasterRecord.AllDataPoints.FindByFieldOid(field_Oid);
bool IsValidDp(IDataPoint dp, bool checkEmpty) => dp != null && dp.Active && dp.IsVisibleRaw && (checkEmpty ? dp.Data != string.Empty : true);
enum CohortType { None = 0, Q3W, Q2W, Q4W }
CohortType GetType(string str_cohort)
{
    Dictionary<CohortType, string[]> type_match = new Dictionary<CohortType, string[]>()
    {
        {CohortType.Q3W,new string[] { "1", "2A" }},
        {CohortType.Q2W,new string[] { "1C", "2C", "2E", "3B" } },
        {CohortType.Q4W ,new string[] { "1B", "2B", "2D", "3A" }}
    };
    foreach (CohortType type in type_match.Keys)
    {
        if (type_match[type].Contains(str_cohort.ToUpper())) return type;
    }
    return CohortType.None;
}

void AddFolder(List<string> Folder_seq, int index_target, string[] forms)
{
    SubjectMatrixRef mtxRef = enterDP.Record.SubjectMatrixRef;
    IInstance ins_Parent = curSub.Instances.FindByFolderOid(InsOID_Parent);
    IReasonCode activativeReason = enterDP.Interaction.Role?.ActivationReasons.FirstOrDefault();
    IReasonCode inactivativeReason = enterDP.Interaction.Role?.InactivationReasons.FirstOrDefault();

    if (ins_Parent != null)
    {
        int repeat = 1;
        IReadOnlyList<IInstance> inses = ins_Parent.Instances.OrderBy(ins => { string name = (ins?.Name ?? "").Trim(); int idx = Folder_seq.IndexOf(name); return idx >= 0 ? idx : int.MaxValue; }).ToList();

        for (int i = 0; i < index_target + 1; i++)
        {
            string folder_target = Folder_seq[i];
            string suffix = folder_target.Replace(header, "").Trim();
            bool found = false;

            foreach (IInstance ins in inses)
            {
                if (ins == null) continue;

                string str_insName = ins.Name.Trim().ToUpper();
                if (str_insName == header.Trim().ToUpper())
                {
                    if (!found) ins.SetInstanceName(suffix);
                }
                else if (str_insName == folder_target.Trim().ToUpper())
                {
                    if (!found)
                    {
                        found = true;
                        if (!ins.Active) { ins.Activate(activativeReason); }
                    }
                }
                else if (!Folder_seq.Contains(ins.Name)) ins.SetInstanceName("");

                if (found) { ins.InstanceRepeatNumber = repeat++; break; }
            }
            if (!found&& index_target != 0)
            {
                IInstance ins_target = ins_Parent.AddInstance(curSub.CrfVersion.Folders.FindByOid(InsOID), mtxRef);
                if (ins_target != null)
                {
                    ins_target.SetInstanceName(suffix);
                    foreach (string str_fm_oid in forms)
                    {
                        ins_target.AddDataPage(curSub.CrfVersion.Forms.FindByOid(str_fm_oid), mtxRef);
                    }
                }
            }
        }
        foreach (IInstance ins in ins_Parent.Instances)
        {
            if (ins?.Name == header || ins?.Folder.OID == "SFM8") continue;
            if (!Folder_seq.Contains(ins?.Name) || (Folder_seq.IndexOf(ins?.Name) > index_target)) { ins.InActivate(inactivativeReason);}
        }
    }
}
}
}

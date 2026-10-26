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
public class EG_TIMEPOINTS2 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpAction = afp.ActionDataPoint;
    Subject subject = dpAction.Record.Subject;
    string Yes = "Y", formOid = "EG", fieldOid_TimePoint = "EGTPT", fieldOid_EGNUM = "EGNUM";

    string[] strFolder = new string[] { "C1D1", "C1D2", "C1D3", "C1D8", "C2D1", "C3D1", "C3D2", "C3D3" };
    string[] strEGTPT = new string[] { "1|1|1|2|2|2|3|3|3", "4|4|4", "5|5|5", "6|6|6", "1|1|1|2|2|2|3|3|3", "1|1|1|2|2|2|3|3|3", "4|4|4", "5|5|5" };
    string[] strEGNUM = new string[] { "1|2|3|1|2|3|1|2|3", "1|2|3", "1|2|3", "1|2|3", "1|2|3|1|2|3|1|2|3", "1|2|3|1|2|3|1|2|3", "1|2|3", "1|2|3" };
    string[] conditionIns = new string[] { "C1D1", "C2D1", "C3D1" };

    ArrayList arFolder = new ArrayList(strFolder), arFolder_condition = new ArrayList(conditionIns);
    Instance inst = dpAction.Record.Instance;
    if ((!arFolder.Contains(inst.Folder.OID) && !arFolder_condition.Contains(inst.Folder.OID) && inst.Folder.OID != "SCR")) return null;

    DataPoint condition = inst.Folder.OID == "SCR" ? dpAction : subject.Instances?.FindByFolderOID("SCR")?.DataPages?.FindByFormOID("DSCONS")?.MasterRecord?.DataPoints?.FindByFieldOID("PRTVR");
    DataPoints conditions = CustomFunction.FetchAllDataPointsForOIDPath("REPROT", "RECONS", "CONSENT", subject);
    conditions.Add(condition);
    bool conditionMet = false;
    foreach(DataPoint checkDP in conditions)
    {
        if(checkDP != null && checkDP.Active && checkDP.Data!=string.Empty&&int.TryParse(checkDP.Data.Substring(checkDP.Data.Length-1), out int versionNumber) && versionNumber >= 6)
        {
            conditionMet = true;
            break;
        }
    }
    if (conditionMet)
    {
        foreach (string ins in conditionIns)
        {
            strEGTPT[arFolder.IndexOf(ins)] = "1|1|1|2|2|2|3|3|3|7|7|7";
            strEGNUM[arFolder.IndexOf(ins)] = "1|2|3|1|2|3|1|2|3|1|2|3";
        }
    }
    ArrayList arEGNUM = new ArrayList(strEGNUM), arTimePoint = new ArrayList(strEGTPT);

    if (dpAction.Field.OID == "PRTVR")
    {
        Instance ins_Cycle = subject.Instances?.FindByFolderOID("TRT");
        if (ins_Cycle == null) return null;

        foreach (string fld in conditionIns)
        {
            DataPoint VISYN = ins_Cycle.Instances?.FindByFolderOID(fld)?.DataPages?.FindByFormOID("VISIT")?.MasterRecord?.DataPoints?.FindByFieldOID("VISYN");
            if (VISYN == null) continue;
            bool doAdd = VISYN.Data == Yes;
            int index = arFolder.IndexOf(fld);
            if (index > -1)
            {
                string strEGNUMS = arEGNUM[index].ToString();
                string[] EGNUMs = strEGNUMS.ToString().Split('|');
                string TPTList = arTimePoint[index].ToString();
                string[] TPTs = TPTList.Split('|');
                DataPage ECGPage = VISYN.Record.Instance?.DataPages?.FindByFormOID(formOid);
                if (ECGPage != null && ECGPage.Active)
                {
                    AddlogLines(ECGPage, fieldOid_TimePoint, TPTs, fieldOid_EGNUM, EGNUMs, doAdd);
                }
            }
        }
    }
    else if (inst.Folder.OID != "SCR")
    {
        newDpage = inst.DataPages?.FindByFormOID(formOid);
        MakeEGSUPVisible(newDpage, "EGSUP", inst);
        bool doAdd = dpAction.Data == Yes;
        int index = arFolder.IndexOf(inst.Folder.OID);
        if (index > -1)
        {
            string strEGNUMS = arEGNUM[index].ToString();
            string[] EGNUMs = strEGNUMS.ToString().Split('|');
            string TPTList = arTimePoint[index].ToString();
            string[] TPTs = TPTList.Split('|');
            if (newDpage != null)
            {
                AddlogLines(newDpage, fieldOid_TimePoint, TPTs, fieldOid_EGNUM, EGNUMs, doAdd);
            }
        }
    }
    return null;
}
void MakeEGSUPVisible(DataPage newDpage, string field, Instance Inst)
{
    if (newDpage != null && newDpage.Active && Inst.Folder.OID == "C1D1")
    {
        DataPoint dpt = newDpage.MasterRecord.DataPoints.FindByFieldOID(field);
        if (dpt != null && dpt.IsVisible == false) dpt.IsVisible = true;
    }
}
DataPage newDpage = null;
void AddlogLines(DataPage dpg, string fieldOid_TimePoint, string[] timePoints, string fieldOid_EGNUM, string[] EGNUMs, bool doAdd)
{
    if (dpg != null && dpg.Active)
    {
        if (doAdd)
        {
            for (int i = 0; i < timePoints.Length; i++)
            {
                Record rd = null;
                rd = dpg.Records.FindByRecordPosition(i + 1);

                if (rd == null)
                    rd = dpg.AddLogRecord();

                if (rd != null)
                {
                    rd.Active = true;
                    DataPoint dpTimePoint = rd.DataPoints.FindByFieldOID(fieldOid_TimePoint);
                    EnterData(dpTimePoint, timePoints[i]);
                    dpTimePoint.Freeze();
                    DataPoint dpEGNUM = rd.DataPoints.FindByFieldOID(fieldOid_EGNUM);
                    EnterData(dpEGNUM, EGNUMs[i]);
                    dpEGNUM.Freeze();
                }
            }

            for (int j = dpg.Records.Count - 1; j > timePoints.Length; j--)
            {
                Record rc = dpg.Records.FindByRecordPosition(j);
                if (rc != null)
                {
                    DataPoint dpTimePoint = rc.DataPoints.FindByFieldOID(fieldOid_TimePoint);
                    EnterData(dpTimePoint, string.Empty);
                    DataPoint dpEGNUM = rc.DataPoints.FindByFieldOID(fieldOid_EGNUM);
                    EnterData(dpEGNUM, string.Empty);
                    rc.Active = false;
                }
            }
        }
        else
        {
            for (int j = dpg.Records.Count - 1; j > 0; j--)
            {
                Record rc = dpg.Records.FindByRecordPosition(j);
                if (rc != null)
                {
                    DataPoint dpTimePoint = rc.DataPoints.FindByFieldOID(fieldOid_TimePoint);
                    EnterData(dpTimePoint, string.Empty);
                    DataPoint dpEGNUM = rc.DataPoints.FindByFieldOID(fieldOid_EGNUM);
                    EnterData(dpEGNUM, string.Empty);
                    rc.Active = false;
                }
            }
        }
    }
}
void EnterData(DataPoint dp, string val)
{
    if (dp != null && dp.Active)
    {
        dp.UnFreeze();
        dp.Enter(val, string.Empty, 0);
    }
}
}
}

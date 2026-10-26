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
public class EG_TIMEPOINTS1 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpAction = afp.ActionDataPoint;
    Subject subject = dpAction.Record.Subject;

    ///define value input field
    string formOid = "EG";
    string fieldOid_TimePoint = "EGTPT";
    string fieldOid_EGNUM = "EGNUM";
    string Yes = "Y", check = "1";
    bool isAdd = false;

    DataPoint cohort = GetDataPoint(subject, "", "SCR", "ENROLL", "COHORT");
    if (cohort == null) return null;
    if (!cohort.Active || cohort.Data == string.Empty) return null;

    string[] strFolder;
    string[] strEGTPT;
    string[] strEGNUM;
    strFolder = new string[] { "C1D1", "C1D2", "C1D3", "C1D8", "C2D1", "C3D1", "C3D2", "C3D3" };
    strEGTPT = new string[] { "1|1|1|2|2|2|3|3|3|7|7|7", "4|4|4", "5|5|5", "6|6|6", "1|1|1|2|2|2|3|3|3|7|7|7", "1|1|1|2|2|2|3|3|3|7|7|7", "4|4|4", "5|5|5" };
    strEGNUM = new string[] { "1|2|3|1|2|3|1|2|3|1|2|3", "1|2|3", "1|2|3", "1|2|3", "1|2|3|1|2|3|1|2|3|1|2|3", "1|2|3|1|2|3|1|2|3|1|2|3", "1|2|3", "1|2|3" };

    //switch (cohort.Data.ToUpper())
    //{
    //    case "1":
    //        strFolder = new string[] { "C1D1", "C1D2", "C1D3", "C1D8", "C2D1", "C3D1", "C3D2", "C3D3" };
    //        strEGTPT = new string[] { "1|1|1|2|2|2|3|3|3|7|7|7", "4|4|4", "5|5|5", "6|6|6", "1|1|1|2|2|2|3|3|3|7|7|7", "1|1|1|2|2|2|3|3|3|7|7|7", "4|4|4", "5|5|5" };
    //        strEGNUM = new string[] { "1|2|3|1|2|3|1|2|3|1|2|3", "1|2|3", "1|2|3", "1|2|3", "1|2|3|1|2|3|1|2|3|1|2|3", "1|2|3|1|2|3|1|2|3|1|2|3", "1|2|3", "1|2|3" };
    //        break;
    //    default:
    //        strFolder = new string[] { "C1D1", "C3D1" };
    //        strEGTPT = new string[] { "1|1|1|2|2|2|3|3|3", "1|1|1|2|2|2|3|3|3" };
    //        strEGNUM = new string[] { "1|2|3|1|2|3|1|2|3", "1|2|3|1|2|3|1|2|3" };
    //        break;
    //}

    if (strFolder == null || strEGTPT == null || strEGNUM == null) return null;
    ArrayList arFolders = new ArrayList(strFolder);
    ArrayList arTimePoint = new ArrayList(strEGTPT);
    ArrayList arEGNUM = new ArrayList(strEGNUM);

    //get enter folder OID of EC enter field, End CF if folder not contain in the CF defined list
    Instance inst = dpAction.Record.Instance;
    if (inst == null || (inst != null && !arFolders.Contains(inst.Folder.OID))) return null;

    //check enter field OID, if EC is excuted by cohort, then initial the ECG page first
    if (dpAction.Field.OID == "COHORT")
    {
        DataPage entryPG = inst.DataPages.FindByFormOID(formOid);
        AddTimepoints(entryPG, fieldOid_TimePoint, string.Empty);
    }
    //if excuted by VISYN
    else
    {
        //get cohort datapoint
        DataPoint trigDP_Cohort = subject.Instances.FindByFolderOID("SCR").DataPages.FindByFormOID("ENROLL").MasterRecord.DataPoints.FindByFieldOID("COHORT");
        //check if VISYN==Y and Cohort == 1, add TPT if meet
        if (string.Compare(dpAction.Data.ToUpper(), Yes, true) == 0 && string.Compare(trigDP_Cohort.Data.ToUpper(), check, true) == 0) isAdd = true;
        //initial EG page
        AddForms(inst, formOid, isAdd, subject);
        //make visiable for one of  the EG page fields (should  confirm whether to use this part)
        MakeEGSUPVisible(newDpage, "EGSUP", inst);
        //get CF defined sequence of folder OID,apply this seq to code seq and record position(EGNUMs)
        int index = arFolders.IndexOf(inst.Folder.OID);
        if (index > -1)
        {
            string strEGNUMS = arEGNUM[index].ToString();
            string[] EGNUMs = strEGNUMS.ToString().Split('|');
            string TPTList = arTimePoint[index].ToString();
            string[] TPTs = TPTList.Split('|');
            if (newDpage != null && newDpage.Active)
            {
                AddlogLines(newDpage, fieldOid_TimePoint, TPTs, fieldOid_EGNUM, EGNUMs);
            }
        }
    }

    return null;
}

DataPoint GetDataPoint(Subject currentSubj, string parentFolderOID, string folderOID, string formOID, string feildOID) => currentSubj?.Instances.FindByFolderOID(parentFolderOID)?.Instances.FindByFolderOID(folderOID)?
    .DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints.FindByFieldOID(feildOID);

void MakeEGSUPVisible(DataPage newDpage, string field, Instance Inst)
{
    if (newDpage != null && newDpage.Active && Inst.Folder.OID == "C1D1")
    {
        DataPoint dpt = newDpage.MasterRecord.DataPoints.FindByFieldOID(field);
        if (dpt != null && dpt.IsVisible == false) dpt.IsVisible = true;
    }
}
DataPage newDpage = null;
void AddlogLines(DataPage dpg, string fieldOid_TimePoint, string[] timePoints, string fieldOid_EGNUM, string[] EGNUMs)
{
    if (dpg != null && dpg.Active)
    {
        for (int i = 0; i < timePoints.Length; i++)
        {
            Record rd = null;
            rd = dpg.Records.FindByRecordPosition(i + 1);

            if (rd == null)
                rd = dpg.AddLogRecord();

            if (rd != null)
            {
                DataPoint dpTimePoint = rd.DataPoints.FindByFieldOID(fieldOid_TimePoint);
                EnterData(dpTimePoint, timePoints[i]);
                DataPoint dpEGNUM = rd.DataPoints.FindByFieldOID(fieldOid_EGNUM);
                EnterData(dpEGNUM, EGNUMs[i]);
            }
        }
    }
}
void AddTimepoints(DataPage dpg, string fieldOid_TimePoint, string val)
{
    if (dpg != null && dpg.Active)
    {
        Records rcds = dpg.Records;
        for (int i = 0; i < rcds.Count; i++)
        {
            if (!rcds[i].Active || rcds[i].RecordPosition == 0) continue;

            DataPoint dpTimePoint = rcds[i].DataPoints.FindByFieldOID(fieldOid_TimePoint);
            EnterData(dpTimePoint, val);

        }
    }
}
void EnterData(DataPoint dp, string val)
{
    if (dp != null && dp.Active)
    {
        if (dp.LockStatus != LockStatusEnum.Locked)
        {
            dp.Enter(val, string.Empty, 0);
            dp.Freeze();
        }
    }
}
void AddForms(Instance ins, string formOID, bool doAdd, Subject subject)
{
    string fieldOid_Req = "EGPERF";
    //get the EG page of current folder
    newDpage = ins.DataPages.FindByFormOID(formOID);
    if (ins != null && ins.Active)
    {
        if (doAdd)
        {
            //if didn't find the ECG page of current folder, add one
            if (newDpage == null)
            {
                //Form newForm = Form.FetchByOID(formOID, ins.Subject.CRFVersion.ID);
                //newDpage = new DataPage(ins, newForm, ins.SubjectMatrixID);
                //ins.DataPages.Add(newDpage);
            }
            //if find make sure it is active
            else
            {
                newDpage.Active = true;
            }
        }
        //if no need to add TPT
        else if (newDpage != null)
        {
            //if the datapoint of EG page you want to check is never entered, inacive the form (should  confirm whether to use this part)
            //DataPoint dpReq = newDpage.MasterRecord.DataPoints.FindByFieldOID(fieldOid_Req);
            //if (dpReq != null && dpReq.ChangeCount == 0)
            //{
            //newDpage.Active = false;
            //}
        }
    }
}
}
}

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
public class EG_TIMEPOINTS3 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
        * Created By : Rananjay
        * Custom Function : EG_TIMEPOINTS1
        * Edit Checks : EG_TIMEPOINTS1
        EG_TIMEPOINTS1_1
        * Description :

        *********************************************************************************************************/
        ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint dpAction = afp.ActionDataPoint;
        Subject subject = dpAction.Record.Subject;
        string formOid = "EG";
        string fieldOid_TimePoint = "EGTPT";
        string fieldOid_EGNUM = "EGNUM";

        string[] strFolder =
        {
            "SCR", "EOT", "UNS"
        }
        ;

        string[] strEGTPT =
        {
            "", "", ""
        }
        ;
        string[] strEGNUM =
        {
            "1", "1", "1"
        }
        ;
        ArrayList arFolders = new ArrayList(strFolder);
        ArrayList arTimePoint = new ArrayList(strEGTPT);
        ArrayList arEGNUM = new ArrayList(strEGNUM);
        Instance inst = dpAction.Record.Instance;

        if (inst == null || (inst != null && !arFolders.Contains(inst.Folder.OID))) return null;

        string Yes = "Y", check = "1";
        bool isAdd = false;
        if (inst != null && arFolders.Contains(inst.Folder.OID))
        {
            if (dpAction.Field.OID == "H_NOW")
            {
                AddTimepoints(dpAction.Record.DataPage, fieldOid_TimePoint, string.Empty);
            }

            else
            {
                if (string.Compare(dpAction.Data.ToUpper(), Yes, true) == 0 || string.Compare(dpAction.Data.ToUpper(), check, true) == 0) isAdd = true;
                newDpage = inst.DataPages.FindByFormOID(formOid); 
//AddForms(inst, formOid, isAdd, subject);

                int index = arFolders.IndexOf(inst.Folder.OID);
                if (index > -1)
                {
                    string[] timePoints =
                    {
                        ""
                    }
                    ;
                    string strEGNUMS = arEGNUM[index].ToString();
                    string[] EGNUMs = strEGNUMS.ToString().Split('|');
                    if (newDpage != null && newDpage.Active)
                    {
                        AddlogLines(newDpage, fieldOid_TimePoint, timePoints);
                    }
                }
            }


        }
        return null;

    }

    void MakeEGSUPVisible(DataPage newDpage, string field, Instance Inst)
    {
        if (newDpage != null && newDpage.Active && Inst.Folder.OID == "SCR")
        {

            DataPoint dpt = newDpage.MasterRecord.DataPoints.FindByFieldOID(field);

            if (dpt != null && dpt.IsVisible == false) dpt.IsVisible = true;

        }
    }
    DataPage newDpage = null;
    void AddlogLines(DataPage dpg, string fieldOid_TimePoint, string[] timePoints)
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
        if (dp != null)
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
        newDpage = ins.DataPages.FindByFormOID(formOID);
        if (ins != null && ins.Active)
        {
            if (doAdd)
            {
                if (newDpage == null)
                {
                    Form newForm = Form.FetchByOID(formOID, ins.Subject.CRFVersion.ID);
                    newDpage = new DataPage(ins, newForm, ins.SubjectMatrixID);
                    ins.DataPages.Add(newDpage);
                }
                else
                {
                    newDpage.Active = true;
                }
            }
            else if (newDpage != null)
            {

                DataPoint dpReq = newDpage.MasterRecord.DataPoints.FindByFieldOID(fieldOid_Req);
                if (dpReq != null && dpReq.ChangeCount == 0)
                {
                    newDpage.Active = false;

                }
            }
        }
}
}
}

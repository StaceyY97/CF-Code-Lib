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
public class VS2_TIMEPOINTS1 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
        * Created By : Rananjay
        * Custom Function : VS2_TIMEPOINTS1
        * Edit Checks : VS2_TIMEPOINTS1
        VS2_TIMEPOINTS1_1
        * Description :

        *********************************************************************************************************/
        ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint dpAction = afp.ActionDataPoint;
        Subject subject = dpAction.Record.Subject;
        string formOid = "VS2";
        string fieldOid_TimePoint = "VSTPT1";


        string[] strFolder =
        {
            "C1D1", "C2D1"
        }
        ;

        string[] strEGTPT =
        {
            "PRE-INFUSION|15MIN|30MIN|45MIN|60MIN|END OF INFUSION (EOI)|EOI+60MIN|EOI+2H|EOI+3H|EOI+4H|EOI+5H|EOI+6H", "PRE-INFUSION|15MIN|30MIN|45MIN|60MIN|END OF INFUSION (EOI)|EOI+60MIN|EOI+2H|EOI+3H|EOI+4H|EOI+5H|EOI+6H"
        }
        ;

        ArrayList arFolders = new ArrayList(strFolder);
        ArrayList arTimePoint = new ArrayList(strEGTPT);

        Instance instDPAction = dpAction.Record.Instance;

        if (instDPAction == null || (instDPAction != null && instDPAction.Folder.OID != "SCR" && !arFolders.Contains(instDPAction.Folder.OID))) return null;

        DataPoints dptsVISYN = new DataPoints();

        if (dpAction.Field.OID == "VISYN") dptsVISYN.Add(dpAction);

        else
        dptsVISYN = CustomFunction.FetchAllDataPointsForOIDPath("VISYN", "VISIT", null, subject);



        string strCOHORT = GetCohort(subject, "SCR", "ENROLL", "COHORT");
        string Yes = "Y";
        bool isAdd = false, cohort = false;

        for (int i = 0; i < dptsVISYN.Count; i++)
        {
            if (!dptsVISYN[i].Active || dptsVISYN[i].Record.RecordPosition != 0) continue;

            Instance inst = dptsVISYN[i].Record.DataPage.Instance;

            isAdd = false;
            cohort = false;
            if (inst != null && arFolders.Contains(inst.Folder.OID))
            {
                if (inst.Folder.OID == "C1D8")
                {
                    if (strCOHORT == "1|2A|1B") cohort = true;
                }

                else
                {
                    cohort = true;
                }
                if (string.Compare(dptsVISYN[i].Data.ToUpper(), Yes, true) == 0 && cohort) isAdd = true;
                AddForms(inst, formOid, isAdd, subject);

                int index = arFolders.IndexOf(inst.Folder.OID);
                if (index > -1)
                {

                    string strTimePoint = arTimePoint[index].ToString();
                    string[] timePoints = strTimePoint.ToString().Split('|');


                    if (newDpage != null && newDpage.Active)
                    {
                        AddlogLines(newDpage, fieldOid_TimePoint, timePoints);
                    }
                }
            }
        }
        return null;

    }

    string GetCohort(Subject subject, string folder, string form, string field)
    {
        Instance inst = subject.Instances.FindByFolderOID(folder);

        if (inst != null && inst.Active)
        {
            DataPage dpg = inst.DataPages.FindByFormOID(form);
            if (dpg != null && dpg.Active)
            {
                DataPoint dpt = dpg.MasterRecord.DataPoints.FindByFieldOID(field);

                if (dpt != null && dpt.Data != string.Empty) return dpt.Data;
            }
        }
        return string.Empty;
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
    void EnterData(DataPoint dp, string val)
    {
        if (dp != null && dp.Active && string.Compare(dp.Data.Trim(), val, true) != 0)
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
        string fieldOid_Req = "VSPERF";
        newDpage = ins.DataPages.FindByFormOID(formOID);
        if (ins != null && ins.Active)
        {
            if (doAdd)
            {
                if (newDpage == null)
                {
                    //Form newForm = Form.FetchByOID(formOID, ins.Subject.CRFVersion.ID);
                    //newDpage = new DataPage(ins, newForm, ins.SubjectMatrixID);
                    //ins.DataPages.Add(newDpage);
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
                    //newDpage.Active = false;

                }
            }
        }
}
}
}

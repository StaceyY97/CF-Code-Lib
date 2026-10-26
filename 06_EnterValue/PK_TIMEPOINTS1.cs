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
public class PK_TIMEPOINTS1 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
     * Created By :MXZ/18Jan2024
     * Custom Function :PK_TIMEPOINTS1
     * Edit Checks :PK_TIMEPOINTS1
     PK_TIMEPOINTS1_1

     *********************************************************************************************************/
    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpAction = afp.ActionDataPoint;
    Subject subject = dpAction.Record.Subject;
    string formOid = "PK";
    string fieldOid_TimePoint = "PKTP";

    string[] strFolder =
    {
        "C1D1", "C1D2", "C1D3", "C1D8", "C1D15", "C2D1", "C2D3", "C3D1", "C3D2", "C3D3", "C3D8", "C3D15",  "C4D1", "C6D1", "C8D1","C16D1","C24D1","C32D1","C40D1","C48D1"
    }
    ;

    string[] strPKTPT =
    {
        "PRE-INFUSION|EOI+15MIN|EOI+1HR|EOI+6HR", "EOI+24HR", "EOI+48HR", "DAY 8", "DAY 15", "PRE-INFUSION", "EOI+48HR", "PRE-INFUSION|EOI+15MIN|EOI+1HR|EOI+6HR",
        "EOI+24HR", "EOI+48HR", "DAY 8", "DAY 15",  "PRE-INFUSION", "PRE-INFUSION", "PRE-INFUSION","PRE-INFUSION","PRE-INFUSION","PRE-INFUSION","PRE-INFUSION","PRE-INFUSION"
    }
    ;

    ArrayList arFolders = new ArrayList(strFolder);
    ArrayList arTimePoint = new ArrayList(strPKTPT);

    Instance instDPAction = dpAction.Record.Instance;

    if (instDPAction == null || (instDPAction != null && instDPAction.Folder.OID != "SCR" && !arFolders.Contains(instDPAction.Folder.OID))) return null;

    DataPoints dptsVISYN = new DataPoints();

    if (dpAction.Field.OID == "VISYN") dptsVISYN.Add(dpAction);

    else
        dptsVISYN = CustomFunction.FetchAllDataPointsForOIDPath("VISYN", "VISIT", null, subject);

    string Yes = "Y";
    bool isAdd = false;


    for (int i = 0; i < dptsVISYN.Count; i++)
    {
        if (!dptsVISYN[i].Active || dptsVISYN[i].Record.RecordPosition != 0) continue;

        Instance inst = dptsVISYN[i].Record.DataPage.Instance;

       // isAdd = false;


        if (inst != null && arFolders.Contains(inst.Folder.OID))
        {
            //if (string.Compare(dptsVISYN[i].Data.ToUpper(), Yes, true) == 0) isAdd = DoAdd(dptsVISYN[i]);
           // AddForms(inst, formOid, true, subject);

newDpage = inst.DataPages.FindByFormOID(formOid);
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
    string fieldOid_Req = "PKYN";
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
               // newDpage.Active = true;
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

System.Collections.Generic.List<string> manageList = new System.Collections.Generic.List<string>() { "C1D2", "C1D3", "C1D8", "C3D2", "C3D3", "C3D8", "C3D15" };
System.Collections.Generic.List<string> aviodList = new System.Collections.Generic.List<string>() { "C1D1", "C1D15", "C2D1", "C2D3", "C3D1", "C4D1", "C6D1", "C8D1", "C16D1", "C24D1", "C32D1" };
//manageList -- Cycles(visits) those need to confirm version and Cohort
//aviodList  -- Cycles(visits) those can be manage ignoring Version and Cohort

bool DoAdd(DataPoint dptsVISYN)
{
    string folderName = dptsVISYN.Record.Instance.Folder.OID;

    if (aviodList.Contains(folderName)) return true;
    else
    {
        if (manageList.Contains(folderName))
        {
            Subject subject = dptsVISYN.Record.Subject;
            DataPoint version = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dptsVISYN.Field.CRFVersion.ID, new object[] { subject, "", "SCR", "DSCONS", "PRTVR" });
            DataPoint cohort = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dptsVISYN.Field.CRFVersion.ID, new object[] { subject, "", "SCR", "ENROLL", "COHORT" });
            if (version != null && version.Active && version.Data == "V2") return true;
            else
            {
                if (version.Data != string.Empty && cohort != null && cohort.Active && cohort.Data == "1") return true;
                else return false;
            }
        }
        else return false;
    }
}
}
}

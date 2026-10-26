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
public class AddForm_CTDNA_TIMEPOINT : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:**************************
* CF Name: AddForm_CTDNA_TIMEPOINT
* Programmer Name:YueXu 14Nov2024/Qian Yang 17Jun2025
* Last Modified: 17Jun2025
* Reason Modified: PV3.0 add timpoint to in conditions
* Description: Add form and set default timepoints.
* **/

    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpAction = afp.ActionDataPoint;
    Subject subject = dpAction.Record.Subject;
    Instance curIns = dpAction.Record.DataPage.Instance;
    string formOid = "CTDNA";
    string fieldOid_TimePoint = "CTDNATPT";
    bool doAdd = false;
    string[] CTDNATPT;
    string[] CTDNATPT1 = { "PRE-DOSE" };
    string[] CTDNATPT2 = { "END OF TREATMENT" };

    System.Collections.Generic.List<string> lsFOLDEROID = new System.Collections.Generic.List<string>() { "C01D01", "C03D01", "C07D01", "EOT", "C07D01" };//"C01D15", "C11D01", "C15D01", "C19D01", "C23D01", "C27D01"
    System.Collections.Generic.List<string> FOLDEROID = new System.Collections.Generic.List<string>() {  "C07D01" };//"C01D15", "C11D01", "C15D01", "C19D01", "C23D01", "C27D01"

    if (curIns != null && dpAction.Field.OID == "STPART")
    {
        foreach (string i in FOLDEROID)
        {
            DataPoints dptsVISAT2 = CustomFunction.FetchAllDataPointsForOIDPath("VISDAT", "VISIT", i, subject);
            if (dptsVISAT2 != null && dptsVISAT2.Count > 0 && dptsVISAT2[0].Active && dptsVISAT2[0].StandardValue() is DateTime)
            {
                Instance ins_C2 = subject.Instances.FindByFolderOID(i);
                if (checkPartWithCycleName(dpAction,i))
                {
                    doAdd = true;
                    CTDNATPT = CTDNATPT1;
                    AddForms(ins_C2, formOid, doAdd, subject);
                    DataPoint dp = newDpage.MasterRecord.DataPoints.FindByFieldOID(fieldOid_TimePoint);
                    EnterData(dp, CTDNATPT[0], true);
                }
                else AddForms(ins_C2, formOid, doAdd, subject);
            }
        }

    }
    else if (curIns != null && lsFOLDEROID.Contains(curIns.Folder.OID))
    {
        if (!(dpAction.StandardValue() is DateTime))
        {
            AddForms(curIns, formOid, doAdd, subject);
        }
        else if (FOLDEROID.Contains(curIns.Folder.OID))
        {
            DataPoints dptsSTPART = CustomFunction.FetchAllDataPointsForOIDPath("STPART", "DSENR", "SCRN", subject);
            if (dptsSTPART != null && dptsSTPART[0].Active && checkPartWithCycleName(dptsSTPART[0], curIns.Folder.OID))
            {
                doAdd = true;
                CTDNATPT = CTDNATPT1;
                AddForms(curIns, formOid, doAdd, subject);
                DataPoint dp = newDpage.MasterRecord.DataPoints.FindByFieldOID(fieldOid_TimePoint);
                EnterData(dp, CTDNATPT[0], true);
            }
            else AddForms(curIns, formOid, doAdd, subject);
        }

        else
        {
            doAdd = true;
            CTDNATPT = curIns.Folder.OID == "EOT" ? CTDNATPT2 : CTDNATPT1;
            AddForms(curIns, formOid, doAdd, subject);
            DataPoint dp = newDpage.MasterRecord.DataPoints.FindByFieldOID(fieldOid_TimePoint);
            EnterData(dp, CTDNATPT[0], true);
        }
    }

    bool checkPartWithCycleName(DataPoint part, string cycleName)
    {
        bool isMeet = false;
        string partName = part.Data;
        if (int.TryParse(cycleName.Substring(1, 2), out int cycleNum))
        {
            isMeet = cycleNum > 7 ? (partName == "4" || partName == "5") : partName == "3" || partName == "4"|| (cycleNum == 1 && partName == "2") || (cycleNum == 7);
        }
        return isMeet;
    }

    return null;
}
DataPage newDpage = null;


void EnterData(DataPoint dp, string val, bool doFreeze)
{
    if (dp != null && dp.Active && string.Compare(dp.Data.Trim(), val, true) != 0)
    {
        dp.Unlock();
        dp.UnFreeze();
        if (dp.LockDisplayStatus == LockDisplayStatusEnum.Unlocked)
        {
            dp.Enter(val, string.Empty, 0);
            if (doFreeze) dp.Freeze();
        }
    }
}

void AddForms(Instance ins, string formOID, bool doAdd, Subject subject)
{
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
            newDpage.Active = false;
        }
    }
}
}
}

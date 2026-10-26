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
public class DYN_CF_AddnextCycle_CnD1 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:**************************
      * CF Name: CF_AddnextCycle
      * Programmer Name:Gopi Namballa(20 JAN 2022)
      * Last Modified: Krishna Dandamudi [20 FEB 2022] Qian Yang[06Nov2024]
      * Reason Modified: Updated to remove the next visit form from visit C52D1 folder.
      * Description: If on Next Visit Form "Will Subject continue to next visit is Yes Then add next Folder **/

        ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
        DataPoint dpAct = afp.ActionDataPoint;
        Subject cSUB = dpAct.Record.Subject;
        Instance curIns = dpAct.Record.DataPage.Instance;
        int CRFVERSIONID = cSUB.CRFVersionID, SUBJECTMATRIXID = dpAct.Record.SubjectMatrixID;
        Instance parentIns = (curIns != null && curIns.Active) ? curIns.ParentInstance : null;

        string strNextFolder = string.Empty;

        if (curIns != null && lsFOLDEROID.Contains(curIns.Folder.OID) && curIns.Folder.OID != CYCLE35)
        {
            int iindex = lsFOLDEROID.IndexOf(curIns.Folder.OID);
            strNextFolder = lsFOLDEROID[iindex + 1];
        }

        if (strNextFolder.Trim() == string.Empty) return null;
        if (dpAct.Data == STRYES)
            AddFolder(cSUB, CRFVERSIONID, SUBJECTMATRIXID, parentIns, strNextFolder, true);
        else
            AddFolder(cSUB, CRFVERSIONID, SUBJECTMATRIXID, parentIns, strNextFolder, false);

        return null;
    }
    const string CYCLE35 = "C35D1", STRYES = "Y";
    System.Collections.Generic.List<string> lsFOLDEROID = new System.Collections.Generic.List<string>()
{
    "C3D15", "C4D1", "C5D1", "C6D1", "C7D1", "C8D1", "C9D1", "C10D1", "C11D1", "C12D1", "C13D1", "C14D1", "C15D1", "C16D1",
    "C17D1", "C18D1", "C19D1", "C20D1", "C21D1", "C22D1", "C23D1", "C24D1", "C25D1", "C26D1", "C27D1", "C28D1", "C29D1", "C30D1", "C31D1","C32D1", "C33D1", "C34D1", "C35D1"
}
    ;
    System.Collections.Generic.List<string> lsFORMOID = new System.Collections.Generic.List<string>()
{
    "VISIT", "EX", "VISNX"
}
    ;
    System.Collections.Generic.List<string> lsFORMOIDV = new System.Collections.Generic.List<string>()
{
    "VISIT", "EX"
}
    ;
    private void AddFolder(Subject curSub, int CVID, int SMID, Instance parent, string strNextFolder, bool addFolder)
    {
        if (parent != null)
        {
            Instance nextIns = parent.Instances.FindByFolderOID(strNextFolder);
            Folder fld = Folder.FetchByOID(strNextFolder, CVID);
            Instances childIns = parent.Instances;
            if (addFolder)
            {
                if (nextIns == null)
                {
                    if (fld != null)
                    {
                        nextIns = new Instance(parent, fld, SMID);
                        if(GetTimeZero(curSub) != DateTime.MinValue) nextIns.SetTimeZero(GetTimeZero(curSub));
                        AddForms(nextIns, curSub, strNextFolder, true);
                    }
                }
                else if (nextIns != null)
                    AddForms(nextIns, curSub, strNextFolder, true);
            }
            else
            {
                if (nextIns != null)
                    AddForms(nextIns, curSub, strNextFolder, false);
            }
            if (nextIns != null)
                nextIns.SetInstanceName("");
        }
    }
    private void AddForms(Instance nxtIns, Subject curSub, string strNextFolder, bool addForm)
    {
        if (nxtIns != null)
        {

            if (strNextFolder != "C35D1")

            {
                foreach (string formOID in lsFORMOID)
                {
                    if (addForm)
                        AddForm(nxtIns, true, formOID);
                    else
                        AddForm(nxtIns, false, formOID);
                }
            }
            else
            {
                foreach (string formOID in lsFORMOIDV)
                {
                    if (addForm)
                        AddForm(nxtIns, true, formOID);
                    else
                        AddForm(nxtIns, false, formOID);
                }
            }
        }
    }

    private void AddForm(Instance inst, bool IsAdd, string sFORMOID)
    {
        if (inst != null)
        {
            DataPage dpg = inst.DataPages.FindByFormOID(sFORMOID);
            if (IsAdd)
            {
                if (dpg == null)
                {
                    Form newForm = Form.FetchByOID(sFORMOID, inst.Subject.CRFVersion.ID);
                    dpg = new DataPage(inst, newForm, inst.SubjectMatrixID);
                    inst.DataPages.Add(dpg);
                }
                else if (dpg != null && !dpg.Active) activateForm(dpg);
            }
            else
            {
                if (dpg != null && dpg.Active && !dpg.IsBitSet(Status.IsTouched)) dpg.Active = false;
            }
if (dpg != null&& sFORMOID == "EX") CustomFunction.PerformCustomFunction("CF_Aid_ManageVisible", dpg.Form.CRFVersion.ID, new object[] { dpg });
        }
    }
    private void activateForm(DataPage dpg)
    {
        Audits auds = Audits.Load(dpg);
        if (auds.Count > 0)
        {
            for (int i = 0; i < auds.Count; i++)
            {
                if (auds[i].SubCategory == AuditSubCategory.InactivatedWithReasonCode)
                    break;
                else if (!dpg.Active)
                {
                    dpg.Active = true;
                    break;
                }
            }
        }
    }

    DateTime GetTimeZero(Subject currentSubj)
    {
        DateTime dt = DateTime.MinValue;
        if(currentSubj != null)
        {
            Instance TRT = currentSubj.Instances.FindByFolderOID("TRT");
            if (TRT == null) return dt;
            Instance C1D1 = TRT.Instances.FindByFolderOID("C1D1");
            if (C1D1 == null) return dt;
            DataPage VISIT = C1D1.DataPages.FindByFormOID("VISIT");
            if(VISIT == null) return dt;
            DataPoint  timeZ = VISIT.MasterRecord.DataPoints.FindByFieldOID("VISDAT");
            if (timeZ == null) return dt;
            if(timeZ.Data != string.Empty && timeZ.StandardValue() is DateTime)
            {
                dt = (DateTime)timeZ.StandardValue();
            }               
        }
        return dt;
}
}
}

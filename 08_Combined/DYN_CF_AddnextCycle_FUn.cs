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
public class DYN_CF_AddnextCycle_FUn : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:**************************
        * CF Name: CF_AddnextCycle_FUn
        * Programmer Name:Gopi Namballa(25 JAN 2022)
        * Last Modified: Krishna Dandamudi [20 FEB 2022]
        * Reason Modified: Updated to remove the next visit form from visit FU13 folder.
        * Description: If on Next Visit Form "Will Subject continue to next visit is Yes Then add next Folder **/

        ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint dpAct = afp.ActionDataPoint;
        Subject cSUB = dpAct.Record.Subject;
        Instance curIns = dpAct.Record.DataPage.Instance;
        int CRFVERSIONID = cSUB.CRFVersionID, SUBJECTMATRIXID = dpAct.Record.SubjectMatrixID;
        Instance parentIns = (curIns != null && curIns.Active) ? curIns.ParentInstance : null;
        string strNextFolder = string.Empty;

        if (curIns !=null && lsFOLDEROID.Contains(curIns.Folder.OID) && curIns.Folder.OID != FU13)
        {
            int iindex = lsFOLDEROID.IndexOf(curIns.Folder.OID);
            strNextFolder = lsFOLDEROID[iindex + 1];
        }
        if(strNextFolder.Trim() == string.Empty) return null;
        if (dpAct.Data == STRYES)
        AddFolder(cSUB, CRFVERSIONID, SUBJECTMATRIXID, parentIns, strNextFolder, true);
        else
        AddFolder(cSUB, CRFVERSIONID, SUBJECTMATRIXID, parentIns, strNextFolder, false);

        return null;
    }
    const string FU13 = "FU13", STRYES = "Y";
    System.Collections.Generic.List<string> lsFOLDEROID = new System.Collections.Generic.List<string>()
    {
        "FU1","FU2", "FU3", "FU4", "FU5", "FU6", "FU7", "FU8", "FU9", "FU10", "FU11", "FU12", "FU13"
    }
    ;
 
 
 
    System.Collections.Generic.List<string> lsFORMOID = new System.Collections.Generic.List<string>()
    {
        "VISIT", "VISNX"
    }
    ;
    System.Collections.Generic.List<string> lsFORMOIDV = new System.Collections.Generic.List<string>()
    {
        "VISIT"
    }
    ;  
    private void AddFolder(Subject curSub, int CVID, int SMID, Instance parent, string strNextFolderOID, bool addFolder)
    {
        if(parent!=null)
        {
            Instance nextIns = parent.Instances.FindByFolderOID(strNextFolderOID);
            Folder fld = Folder.FetchByOID(strNextFolderOID, CVID);
            Instances childIns = parent.Instances;
            if (addFolder)
            {
                if (nextIns == null)
                {
                    if (fld != null)
                    {
                        nextIns = new Instance(parent, fld, SMID);
                        AddForms(nextIns, curSub, strNextFolderOID, true);
                    }
                }
                else if (nextIns != null)
                AddForms(nextIns, curSub, strNextFolderOID, true);
            }
            else
            {
                if (nextIns != null)
                AddForms(nextIns, curSub, strNextFolderOID, false);
            }
            if (nextIns != null)
            nextIns.SetInstanceName("");
        }
    }
        private void AddForms(Instance nxtIns, Subject curSub, string strNextFolder, bool addForm)
        {
            if (nxtIns != null)
            {
                if (strNextFolder != "FU13")
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
}
}

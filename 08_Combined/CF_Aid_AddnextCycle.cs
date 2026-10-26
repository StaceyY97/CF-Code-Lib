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
public class CF_Aid_AddnextCycle : CustomFunctionBase
{
public object Eval(object ThisObject)
{
object[] inputParameters = (object[])ThisObject;
    curSub = (Subject)inputParameters[0];
    SMID = (int)inputParameters[1];
    string InsOID_Parent = (string)inputParameters[2];
    string targetFolder = (string)inputParameters[3];
    bool doAdd = (bool)inputParameters[4];
    string[] targetForms = (string[])inputParameters[5];

    CVID = curSub?.CRFVersionID ?? 0;
    parentIns = curSub?.Instances?.FindByFolderOID(InsOID_Parent);
    try
    {
        if (parentIns != null && !string.IsNullOrEmpty(targetFolder) && targetForms != null)
        {
            AddFolder(parentIns, targetFolder, doAdd, targetForms);
        }
    }
    catch { }
    return null;
}

Subject curSub;
int CVID, SMID;
Instance parentIns;

DateTime GetTimeZero(Subject currentSubj)
{
    DateTime dt = DateTime.MinValue;
    if (currentSubj != null)
    {
        Instance TRT = currentSubj.Instances?.FindByFolderOID("TRT");
        if (TRT == null) return dt;

        Instance C1D1 = TRT.Instances?.FindByFolderOID("C1D1");
        if (C1D1 == null) return dt;

        DataPage VISIT = C1D1.DataPages?.FindByFormOID("VISIT");
        if (VISIT == null) return dt;

        DataPoint timeZ = VISIT.MasterRecord?.DataPoints?.FindByFieldOID("VISDAT");
        if (timeZ == null) return dt;

        if (timeZ.Data != string.Empty && timeZ.StandardValue() is DateTime)
        {
            dt = (DateTime)timeZ.StandardValue();
        }
    }
    return dt;
}

private void AddFolder(Instance parent, string strNextFolder, bool addFolder, string[] lsFORMOID)
{
    if (parent != null && !string.IsNullOrEmpty(strNextFolder) && lsFORMOID != null)
    {
        Instance nextIns = parent.Instances?.FindByFolderOID(strNextFolder);
        Folder fld = Folder.FetchByOID(strNextFolder, CVID);

        if (addFolder)
        {
            if (nextIns == null)
            {
                if (fld != null)
                {
                    nextIns = new Instance(parent, fld, SMID);
                    DateTime timeZero = GetTimeZero(curSub);
                    if (timeZero != DateTime.MinValue)
                    {
                        nextIns.SetTimeZero(timeZero);
                    }
                    AddForms(nextIns, true, lsFORMOID);
                }
            }
            else if (nextIns != null)
            {
                AddForms(nextIns, true, lsFORMOID);
            }
        }
        else
        {
            if (nextIns != null)
            {
                AddForms(nextIns, false, lsFORMOID);
            }
        }

        if (nextIns != null)
        {
            nextIns.SetInstanceName("");
        }
    }
}

private void AddForms(Instance nxtIns, bool addForm, string[] lsFORMOID)
{
    if (nxtIns != null && lsFORMOID != null)
    {
        foreach (string formOID in lsFORMOID)
        {
            if (!string.IsNullOrEmpty(formOID))
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
    if (inst != null && !string.IsNullOrEmpty(sFORMOID))
    {
        DataPage dpg = inst.DataPages?.FindByFormOID(sFORMOID);
        if (IsAdd)
        {
            if (dpg == null)
            {
                Form newForm = Form.FetchByOID(sFORMOID, inst.Subject?.CRFVersion?.ID ?? 0);
                if (newForm != null)
                {
                    dpg = new DataPage(inst, newForm, inst.SubjectMatrixID);
                    inst.DataPages?.Add(dpg);
                }
            }
            else if (dpg != null && !dpg.Active)
            {
                activateForm(dpg);
            }
        }
        else
        {
            if (dpg != null && dpg.Active&&!dpg.IsBitSet(Status.IsTouched))
            {
                dpg.Active = false;
            }
        }
        
        if (dpg != null && sFORMOID == "EX" && dpg.Form?.CRFVersion?.ID != null)
        {
            CustomFunction.PerformCustomFunction("CF_Aid_ManageVisible", dpg.Form.CRFVersion.ID, new object[] { dpg });
        }
    }
}

private void activateForm(DataPage dpg)
{
    if (dpg == null) return;

    Audits auds = Audits.Load(dpg);
    if (auds?.Count > 0)
    {
        for (int i = 0; i < auds.Count; i++)
        {
            if (auds[i]?.SubCategory == AuditSubCategory.InactivatedWithReasonCode)
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

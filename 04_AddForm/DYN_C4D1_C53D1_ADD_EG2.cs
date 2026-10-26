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
public class DYN_C4D1_C53D1_ADD_EG2 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*Developer Name: Sandeep Kanike, Edetek
      Date:11-NOV-2025
      CF Name: DYN_C4D1_C53D1_ADD_EG2
      Edit check name: DYN_C4D1_C53D1_ADD_EG2, DYN_C4D1_C53D1_ADD_EG2_CROSS, DYN_C4D1_C53D1_ADD_EG2_CROSS1
      Description: If [(DSCONS.PRTVR<> null, Version 2.0 Amendment 1, Version 3.0 Amendment 2, Version 4.0 Amendment 3, and
      Version 5.0 Amendment 4) or RECONS.REPROT=Version 6.0 Amendment 5] and CnD1.VISYN=Y then add EG2 form to the corresponding CnD1 folder*/

    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpAction = afp.ActionDataPoint;
    Subject subject = dpAction.Record.Subject;
    string Folder_SCR = "SCR", Form_DSCONS = "DSCONS", Field_PRTVR = "PRTVR";
    string Folder_CONSENT = "CONSENT", Form_RECONS = "RECONS", Field_REPROT = "REPROT";
    string Form_VISIT = "VISIT", Field_VISYN = "VISYN";
    DataPoint Dp_VISYN = null;
    Instance Ins_TRT = null;
    DataPage Dpg_Visit = null;
    bool Flag_DSCONS = false, Flag_RECONS = false;
    bool Addform = false;
    string FormOID = "EG2";
    Instance Ins_Day1 = null;

    DataPoint Dp_PRTVR = GetDatapoint(Folder_SCR, Form_DSCONS, Field_PRTVR, subject);
   
    Flag_DSCONS = Getlogic(Dp_PRTVR);
    Flag_RECONS = GetDatapointlog(Folder_CONSENT, Form_RECONS, Field_REPROT, subject);

    Ins_TRT = subject.Instances.FindByFolderOID("TRT");
    if (Ins_TRT != null)
    {
        Instances Inss_Day1 = Ins_TRT.Instances;
        if (Inss_Day1.Count > 0)
        {
            for (int i = 0; i < Inss_Day1.Count; i++)
            {
                Addform = false;
                Ins_Day1 = Inss_Day1[i];
                if (Ins_Day1 != null)
                {
                    int FolderOID = Getnumber(Ins_Day1);
                    
                    if (FolderOID >= 4 && FolderOID <= 53)
                    {
                        Dpg_Visit = Ins_Day1.DataPages.FindByFormOID(Form_VISIT);
                        if (Dpg_Visit != null)
                        {
                            Dp_VISYN = Dpg_Visit.MasterRecord.DataPoints.FindByFieldOID(Field_VISYN);
                            if (Dp_VISYN != null && Dp_VISYN.Data.Trim() == "Y" && (Flag_DSCONS || Flag_RECONS))
                            {
                               Addform = true;
                            }
                        }
AddForm(Ins_Day1, FormOID, Addform);
                    }
                    
                }
            }
        }
    }
    return null;
}
bool Getlogic(DataPoint Dp)
{
    if(Dp!=null && string.Compare(Dp.Data.Trim(), string.Empty, true)!=0 &&
        string.Compare(Dp.Data.Trim(), "V2", true) != 0 && string.Compare(Dp.Data.Trim(), "V3", true) != 0
        && string.Compare(Dp.Data.Trim(), "V4", true) != 0 && string.Compare(Dp.Data.Trim(), "V5", true) != 0)
    {
        return true;
    }
    return false;
}
bool GetDatapointlog(string Folder_CONSENT, string Form_RECONS, string Field_REPROT, Subject sub)
{
    Records Rds = new Records();
    Record Rd = null;
    Instance Ins = sub.Instances.FindByFolderOID(Folder_CONSENT);
    if (Ins != null)
    {
        DataPage Dpg = Ins.DataPages.FindByFormOID(Form_RECONS);
        if (Dpg != null)
        {
            Rds = Dpg.Records;
            if(Rds.Count>0)
            {
                for(int i=0;i<Rds.Count;i++)
                {
                    Rd = Rds[i];
                    if (Rd == null || !Rd.Active || Rd.RecordPosition == 0) continue;

                    DataPoint Dp = Rd.DataPoints.FindByFieldOID(Field_REPROT);
                    if(Dp!=null && string.Compare(Dp.Data.Trim(), "V6", true)==0)
                    {
                        return true;
                    }
                }
            }
        }
    }
    return false;
}
int Getnumber(Instance Day1)
{
    string FolderOID = string.Empty;
    string Substring = string.Empty;
    if (Day1 != null)
    {
        FolderOID = Day1.Folder.OID.ToString();
        int Firstindexval = FolderOID.IndexOf("C");
        int LastIndexval = FolderOID.LastIndexOf("D");
        Substring = FolderOID.Substring(LastIndexval + 1);
        
        if (Firstindexval != -1 && LastIndexval != -1 && LastIndexval - Firstindexval > 0 && Substring == "1")
        {
            FolderOID = FolderOID.Substring(Firstindexval + 1, LastIndexval - Firstindexval - 1);
            if (FolderOID != string.Empty)
            return Convert.ToInt32(FolderOID.ToString());
        }
    }
    return 0;
}
DataPoint GetDatapoint(string Folder, string Form, string Field, Subject sub)
{
    Instance Ins = sub.Instances.FindByFolderOID(Folder);
    if (Ins != null)
    {
        DataPage Dpg = Ins.DataPages.FindByFormOID(Form);
        if (Dpg != null)
        {
            DataPoint Dp = Dpg.MasterRecord.DataPoints.FindByFieldOID(Field);
            if (Dp != null)
            {
                return Dp;
            }
        }
    }
    return null;
}
void AddForm(Instance inst, string formOID, bool doAdd)
{
    DataPage newPage = null;
    if (inst != null)
    {
        newPage = inst.DataPages.FindByFormOID(formOID);
        if (doAdd)
        {
            if (newPage == null)
            {
                Form newForm = Form.FetchByOID(formOID, inst.Subject.CRFVersion.ID);
                newPage = new DataPage(inst, newForm, inst.SubjectMatrixID);
                inst.DataPages.Add(newPage);
            }
            else
            {
                if (newPage != null && !newPage.Active)
                    newPage.Active = true;
            }
        }
        else if (newPage != null && !newPage.IsBitSet(Status.IsTouched))
            newPage.Active = false;
    }
}
}
}

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
public class AddForm_BL_TASSMT1 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*******************************************************
 * Created By : YueXu 26Nov2024/Qian Yang 20Oct2025
 * Custom Function : AddForm_BL_TASSMT1
 * Edit Checks : Related ECs
 * Descreiption: Add form.
 ***************************/

        ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
        DataPoint dpAction = afp.ActionDataPoint;
        Subject subject = dpAction.Record.Subject;
        Instance curIns = dpAction.Record.DataPage.Instance;
        string formOid;
        string formOid_1 = "BL"; // post cycle3
        string formOid_2 = "BL1"; //cycle 3
        string formOid_3 = "RSCOM";
        bool doAdd_RSCOM; //when cancer type equals 2, then doAdd_RSCOM equals true

        if (dpAction.Field.OID == "TLATPT"|| dpAction.Field.OID == "NTLATPT")
        {
            DataPoints dps_CANTYPE = CustomFunction.FetchAllDataPointsForOIDPath("CANTYPE", "DSENR", "SCRN", subject);
            doAdd_RSCOM = dps_CANTYPE != null && dps_CANTYPE.Count > 0 && dps_CANTYPE[0].Active && dps_CANTYPE[0].Data == "2";
DataPoint TPT = dpAction.Field.OID == "TLATPT"&&dpAction.Active&&dpAction.Data!=string.Empty? dpAction: curIns.DataPages.FindByFormOID("NTL").MasterRecord.DataPoints.FindByFieldOID("NTLATPT");
            ManageForms(curIns, TPT, doAdd_RSCOM);
            AddForms(curIns, formOid_3, doAdd_RSCOM, subject);
        }
        else
        {
            Instance ins_TASSMT = subject.Instances.FindByFolderOID("TASSMT");
            if (ins_TASSMT != null && ins_TASSMT.Active)
            {
                doAdd_RSCOM = dpAction.Data == "2";
                foreach (Instance ins in ins_TASSMT.Instances)
                {
                    DataPage dpg_TLA = ins.DataPages.FindByFormOID("TLA")!=null? ins.DataPages.FindByFormOID("TLA"): ins.DataPages.FindByFormOID("NTL");
                    if (dpg_TLA != null && dpg_TLA.Active)
                    {
                        DataPoint dp_TLATPT = dpg_TLA.MasterRecord.DataPoints.FindByFieldOID("TLATPT")!=null? dpg_TLA.MasterRecord.DataPoints.FindByFieldOID("TLATPT"): dpg_TLA.MasterRecord.DataPoints.FindByFieldOID("NTLATPT");
                        ManageForms(ins, dp_TLATPT, doAdd_RSCOM);
                    }
                    else
                    {
                        AddForms(ins, formOid_1, false, subject);
                        AddForms(ins, formOid_2, false, subject);
                    }
                    AddForms(ins, formOid_3, doAdd_RSCOM, subject);
                }
            }
        }

        void ManageForms(Instance ins,DataPoint dp_TLATPT,bool IsCancer)
        {
            bool doAdd = false;
            if (IsCancer && dp_TLATPT != null && dp_TLATPT.Active && dp_TLATPT.Data != "")
            {
                doAdd = true;
                if (dp_TLATPT.Data == "C3")
                {
                    AddForms(ins, formOid_2, doAdd, subject);
                    AddForms(ins, formOid_1, !doAdd, subject);
                }
                else
                {
                    AddForms(ins, formOid_1, doAdd, subject);
                    AddForms(ins, formOid_2, !doAdd, subject);
                }
            }
            else
            {
                AddForms(ins, formOid_1, doAdd, subject);
                AddForms(ins, formOid_2, doAdd, subject);
            }
        }
        return null;
    }
    DataPage newDpage = null;


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

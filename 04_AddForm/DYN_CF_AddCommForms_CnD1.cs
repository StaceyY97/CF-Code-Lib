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
public class DYN_CF_AddCommForms_CnD1 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:**************************
        * CF Name: CF_SubseqCycle_AddCommForms
        * Programmer Name:Yuanyuan(08 JAN 2024)
        * Last Modified: Not Applicable
        * Reason Modified: Not Applicable
        * Description: To check for cycles **/

        ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint dpAct = afp.ActionDataPoint;
        Subject cSUB = dpAct.Record.Subject;
        Instance curIns = dpAct.Record.DataPage.Instance;
        bool isAdd = false;

if(!lsFOLDEROID.Contains(curIns.Folder.OID))return null;

        if(curIns != null && dpAct.Data== STRYES)
        isAdd = true;
        if (curIns != null && lsFOLDEROID.Contains(curIns.Folder.OID))
        {
            AddForms(curIns, cSUB, isAdd);
        }

        return null;
    }
    const string STRYES = "Y";
    System.Collections.Generic.List<string> lsFOLDEROID = new System.Collections.Generic.List<string>()
    {
        "C4D1", "C5D1", "C6D1", "C7D1", "C8D1", "C9D1", "C10D1", "C11D1", "C12D1", "C13D1", "C14D1", "C15D1", "C16D1",
        "C17D1", "C18D1", "C19D1", "C20D1", "C21D1", "C22D1", "C23D1", "C24D1", "C25D1", "C26D1", "C27D1", "C28D1", "C29D1", "C30D1", "C31D1",
        "C32D1", "C33D1", "C34D1", "C35D1", "C36D1", "C37D1", "C38D1", "C39D1", "C40D1", "C41D1", "C42D1", "C43D1", "C44D1", "C45D1", "C46D1", "C47D1", "C48D1", "C49D1", "C50D1", "C51D1", "C52D1", "C53D1"
    }
    ;
    System.Collections.Generic.List<string> lsFORMOID = new System.Collections.Generic.List<string>()
    {
        "PE2", "ECOG", "VS3", "LBHEM", "LBCHEM", "LBURIN"
    }
    ;
    private void AddForms(Instance nxtIns, Subject curSub, bool addForm)
    {
        if (nxtIns != null)
        {
            foreach (string formOID in lsFORMOID)
            {
                if (addForm)
                AddForm(nxtIns, true, formOID);
                else
                AddForm(nxtIns, false, formOID);
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

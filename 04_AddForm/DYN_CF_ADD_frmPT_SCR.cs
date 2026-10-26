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
public class DYN_CF_ADD_frmPT_SCR : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:**************************
        * CF Name: DYN_CF_ADD_frmPT_SCR
        * EC: DYN_SCR_ADD_PT_MULTIPLE
        * Programmer Name: Nickel Ke (03Mar2022)
        * Last Modified:
        * Reason Modified:
        * Description: Trigger new PT form when PTADDYN = Yes and only one new PT form is allowed **/
        ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint dptAction = afp.ActionDataPoint;
        Subject sub = dptAction.Record.Subject;
        Instance inst = dptAction.Record.DataPage.Instance;
        if (inst == null || inst.Folder.OID != "SCR") return null;
        string formOID = "PT";
        int vrsId = sub.CRFVersionID;
        int nextform = dptAction.Record.DataPage.PageRepeatNumber + 1;
        DataPage NextDataPage = CheckFormPresent(nextform, inst, formOID);
        bool isAdd = false;
        if (dptAction.Active && dptAction.Data == "Y")
        {
            isAdd = true;
        }
        AddForms(inst, NextDataPage, formOID, isAdd, sub);

        return null;
    }
    DataPage CheckFormPresent(int nextform, Instance inst, string formOID)
    {

        DataPages dpgs = inst.DataPages.FilterDataPagesByFormOID("PT");
        for (int i = 0; i < dpgs.Count; i++)
        {
            if (dpgs[i].PageRepeatNumber == nextform) return dpgs[i];
        }
        return null;
    }
    void AddForms(Instance ins, DataPage newDpage, string formOID, bool doAdd, Subject subject)
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
        else if (newDpage != null && !newDpage.IsBitSet(Status.IsTouched) && newDpage.Active == true)
        {
            newDpage.Active = false;

        }
}
}
}

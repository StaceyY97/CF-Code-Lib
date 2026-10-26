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
public class EC_AddForm_D15 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:**************************
    * CF Name: EC_AddForm_D15
    * Programmer Name:Qian Yang 16Jun2025
    * Last Modified: Not Applicable
    * Reason Modified: Not Applicable
    * Description:  Manage form in CnD15(n>=5) visits 
    * **/

    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpAction = afp.ActionDataPoint;
    Instance curIns = dpAction.Record.DataPage.Instance;
    Subject subject = dpAction.Record.Subject;
    bool doAdd = false;
    string[] form_oid =
    {
        "VS2"
    }
    ;
    bool IsInsContained() => int.TryParse(curIns?.Folder.OID.Substring(1, 2), out int CycleNum)&& CycleNum>=5 && int.TryParse(curIns?.Folder.OID.Substring(4, 2), out int DayNum) && DayNum==15;

    if (!IsInsContained()) return null;

    if (dpAction.StandardValue() is DateTime )
    {
        doAdd = true;
        foreach (string i in form_oid)
        {
            AddForms(curIns, i, doAdd, subject);
            newDpage = null;
        }
    }
    else 
    {
        foreach (string i in form_oid)
        {
            AddForms(curIns, i, doAdd, subject);
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

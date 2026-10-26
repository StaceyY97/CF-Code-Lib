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
public class CF_Aid_AddForm : CustomFunctionBase
{
public object Eval(object ThisObject)
{
object[] inputParameters = (object[])ThisObject;
    Instance currIns = (Instance)inputParameters[0];
    bool doAdd = (bool)inputParameters[1];
    string formOID = (string)inputParameters[2];

    try
    {
        AddForm(currIns, doAdd, formOID);
    }
    catch { }
    return null;

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
            if (dpg != null && dpg.Active && !dpg.IsBitSet(Status.IsTouched))
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

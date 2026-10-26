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
public class EC_AddForm_PREG : CustomFunctionBase
{
public object Eval(object ThisObject)
{
ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpAction = afp.ActionDataPoint;
    Instance currentFolder = dpAction.Record.Instance;

    string match = "F";
    bool doAdd;
//Message.SendEmail("qian.yang@edetek.com", "test_qy@mdsol.com", "test_qy", "ec entered: " +currentFolder.Folder.Name );
    DataPoint tempDP_SCR = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { dpAction.Record.Subject, "", "SCREEN_1", "DM", "DMSEX" });
    DataPoint tempDP_RSCR = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { dpAction.Record.Subject, "", "RESCREEN_1", "DM", "DMSEX" });

    if (tempDP_RSCR != null && tempDP_RSCR.Active && tempDP_RSCR.Data != string.Empty) doAdd = tempDP_RSCR.Data == match;
    else doAdd = tempDP_SCR != null && tempDP_SCR.Active && tempDP_SCR.Data == match;

    if (dpAction.Field.OID == "DMSEX")
    {
        Instances allSubjIns = dpAction.Record.Subject.Instances;
        foreach (Instance tempIns in allSubjIns)
        {
            if (tempIns == null || !tempIns.Active || tempIns.Folder.OID != "UNA") continue;
            bool isChecked = false;
            if (doAdd)
            {
                DataPage uns = tempIns.DataPages.FindByFormOID("UNA");
                if (uns != null && uns.Active)
                {
                    DataPoint pregCheck = uns.MasterRecord.DataPoints.FindByFieldOID("PREGCHECK");
                    isChecked = pregCheck != null && pregCheck.Active && pregCheck.Data == "1";
                }
            }
            AddForms(tempIns, "PREG", doAdd && isChecked, dpAction.Record.Subject);
        }

    }
    else
    {
        bool isChecked = false;
        if (doAdd) isChecked = dpAction.Active && dpAction.Data == "1";
        AddForms(currentFolder, "PREG", doAdd & isChecked, dpAction.Record.Subject);
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
            ins.DataPages.Remove(newDpage);
        }
    }
}
}
}

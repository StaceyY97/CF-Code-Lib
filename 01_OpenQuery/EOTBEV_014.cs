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
public class EOTBEV_014 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*Developer Name: Sandeep Kanike, Edetek
    Date:06-NOV-2025
    CF Name: EOTBEV_014
    Edit check name: EOTBEV_014, EOTBEV_014_CROSS
    Description: If TTREAS=Withdrawal by Subject then should have at least one of the CWD logline*/

    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpAction = afp.ActionDataPoint;
    Subject subject = dpAction.Record.Subject;

    string Field_TTREAS = "TTREAS", Form_EOTBEV = "EOTBEV", Folder_EOT = "EOT";
    string Form_CWD = "CWD", Folder_CONSENT = "CONSENT";
    bool Openquery = false;
    Records Rds = new Records(); DataPoint Dp_CWCONS = null;
    string QT = "Reason for treatment discontinuation was reported as 'Withdrawal by Subject'; however, Consent Withdrawal form is missing. Please verify & update.";

    DataPoint Dp_TTREAS = GetDatapoint(Folder_EOT, Form_EOTBEV, Field_TTREAS, subject);

    DataPage Dpg_CWD = GetDatapage(Form_CWD, Folder_CONSENT, subject);

    if (Dp_TTREAS != null && Dp_TTREAS.Active && string.Compare(Dp_TTREAS.Data.Trim(), "7", true) == 0)
    {
        Openquery = true;
        if(Dpg_CWD!=null)
        {
            Rds = Dpg_CWD.Records;
            if(Rds.Count>0)
            {
                for(int i=0;i<Rds.Count;i++)
                {
                    if (Rds[i] == null || !Rds[i].Active || Rds[i].RecordPosition == 0) continue;

                    Dp_CWCONS = Rds[i].DataPoints.FindByFieldOID("CWCONS");
                    if(Dp_CWCONS!=null && string.Compare(Dp_CWCONS.Data.Trim(), string.Empty, true)!=0)
                    {
                        Openquery = false;
                        break;
                    }
                }
            }
        }
    }
    if (Dp_TTREAS != null)
        CustomFunction.PerformQueryAction(QT, 1, false, false, Dp_TTREAS, Openquery, afp.CheckID, afp.CheckHash);
    return null;
}
DataPage GetDatapage(string Form, string Folder, Subject sub)
{
    Instance ins = sub.Instances.FindByFolderOID(Folder);
    if (ins != null)
    {
        DataPage Dpg = ins.DataPages.FindByFormOID(Form);
        if (Dpg != null)
        {
            return Dpg;
        }
    }
    return null;
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
}
}

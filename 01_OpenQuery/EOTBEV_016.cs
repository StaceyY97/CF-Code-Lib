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
public class EOTBEV_016 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*Developer Name: Sandeep Kanike, Edetek
          Date:06-NOV-2025
          CF Name: EOTBEV_016
          Edit check name: EOTBEV_016, EOTBEV_016_CROSS
          Description: If TTREAS= Death then DEATH form should be completed & Date of death must equal to Treatment discontinuation decision date, when
< EOTBEV.TTDAT & EOTBEV.TTREAS are not null & DEATH.DTHDAT is null or not equal to EOTBEV.TTDAT >*/

        ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
        DataPoint dpAction = afp.ActionDataPoint;
        Subject subject = dpAction.Record.Subject;

        string Field_TTREAS = "TTREAS", Form_EOTBEV = "EOTBEV", Folder_EOT = "EOT";
        string Field_DTHDAT = "DTHDAT", Form_DEATH = "DEATH", Folder_DTH = "DTH";
        DataPage Dpg_DEATH = null;
        DataPoint DpTTDAT = null;
        bool OpenQuery = false;
        string QT = "Reason for treatment discontinuation was reported as Death; however, DEATH form is missing OR Treatment discontinuation decision date is not same as Date of Death. Please verify & update.";

        DataPoint Dp_TTREAS = GetDatapoint(Folder_EOT, Form_EOTBEV, Field_TTREAS, subject);
        DataPoint Dp_DTHDAT = GetDatapoint(Folder_DTH, Form_DEATH, Field_DTHDAT, subject);
        Instance Ins = subject.Instances.FindByFolderOID("DTH");
        if(Ins!=null)
        {
            Dpg_DEATH = Ins.DataPages.FindByFormOID("DEATH");
        }
        if (Dp_TTREAS != null)
            DpTTDAT = Dp_TTREAS.Record.DataPage.MasterRecord.DataPoints.FindByFieldOID("TTDAT");

        if (Dp_TTREAS != null && Dp_TTREAS.Active && string.Compare(Dp_TTREAS.Data.Trim(), "10", true) == 0 &&
            (ValidDP(DpTTDAT, Dp_DTHDAT) || (Dpg_DEATH == null)))
        {
            OpenQuery = true;
        }
        if (Dp_TTREAS != null)
            CustomFunction.PerformQueryAction(QT, 1, false, false, Dp_TTREAS, OpenQuery, afp.CheckID, afp.CheckHash);
        return null;
    }
    bool ValidDP(DataPoint DpTTDAT, DataPoint Dp_DTH)
    {
        if(DpTTDAT != null && Dp_DTH!=null && DpTTDAT.StandardValue() is DateTime && Dp_DTH.StandardValue() is DateTime
            && Convert.ToDateTime(Dp_DTH.StandardValue())!= Convert.ToDateTime(DpTTDAT.StandardValue()))
        {
            return true;
        }
        return false;
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

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
public class EOTBEV_012 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*Developer Name: Sandeep Kanike, Edetek
  Date:06-NOV-2025
  CF Name: EOTBEV_012
  Edit check name: EOTBEV_012, EOTBEV_012_CROSS
  Description: If EOTBEV.TTREAS & is PD and PDSP=Disease Progression per RECIST V1.1 then at least one of the  Response Assessment form must 
  have Overall response= 'Disease Progression per RECIST V1.1'.*/

    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpAction = afp.ActionDataPoint;
    Subject subject = dpAction.Record.Subject;

    string Field_PDSP = "PDSP", Form_EOTBEV = "EOTBEV", Folder_EOT = "EOT";
    string Field_TTREAS = "TTREAS";

    string FieldOID_RAOVER = "RAOVER", Form_TURA = "TURA";
    string QT = "If Reason is Progressive Disease, specify type of progression' was reported as 'Disease Progression per RECIST V1.1'; however, \"Overall response\" is not equal on any Response Assessment form. Please verify & update.";
    bool flag = false;

    DataPoint Dp_PDSP = GetDatapoint(Folder_EOT, Form_EOTBEV, Field_PDSP, subject);
    DataPoint Dp_TTREAS = GetDatapoint(Folder_EOT, Form_EOTBEV, Field_TTREAS, subject);

    DataPoints Dps_RAOVER = CustomFunction.FetchAllDataPointsForOIDPath(FieldOID_RAOVER, Form_TURA, null, subject);
    bool OpenQuery = GetCondition(Dps_RAOVER);
    if (Dp_PDSP != null && Dp_PDSP.Active && string.Compare(Dp_PDSP.Data.Trim().ToUpper(), "DISEASE PROGRESSION PER RECIST V1.1", true) == 0 && Dp_TTREAS != null && Dp_TTREAS.Active && Dp_TTREAS.Data!=string.Empty && string.Compare(Dp_TTREAS.Data.Trim(), "2", true) == 0 && OpenQuery)flag = true;

    if (Dp_PDSP != null)
        CustomFunction.PerformQueryAction(QT, 1, false, false, Dp_PDSP, flag, afp.CheckID, afp.CheckHash);

    return null;
}
bool GetCondition(DataPoints Dps)
{
    DataPoint Dp = null;
    if (Dps.Count > 0)
    {
        for (int i = 0; i < Dps.Count; i++)
        {
            Dp = Dps[i];
            if (Dp != null && Dp.Active && string.Compare(Dp.Data.Trim(), "4", true) == 0)
            {
                return false;
            }
        }
    }
    return true;
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

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
public class EOTBEV_006 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*Developer Name: Sandeep Kanike, Edetek
      Date:20-NOV-2025
      CF Name: EOTBEV_006
      Edit check name: EOTBEV_006, EOTBEV_006_CROSS
      Description:  TTREAS= Adverse Event & AEID is provided. Same AE must have "Drug Withdrawn" as  
    'Action Taken with Bevacizumab'.*/

    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpAction = afp.ActionDataPoint;
    Subject subject = dpAction.Record.Subject;

    string Field_TTREAS = "TTREAS", Field_AEID = "AEID", Form_EOTBEV = "EOTBEV", Folder_EOT = "EOT";

    Records Rds = new Records();
    Record Rd = null;
    DataPoint Dp_AEACNBEV4 = null;
    
    bool OpenQuery = false, flag = false;

    string QT = "Primary reason for treatment discontinuation is 'Adverse Event'; however, 'Action Taken with Bevacizumab' is not selected as \"Drug Withdrawn\" for the same AE. Please verify & update.";

    DataPoint Dp_TTREAS = GetDatapoint(Folder_EOT, Form_EOTBEV, Field_TTREAS, subject);
    DataPoint Dp_AEID = GetDatapoint(Folder_EOT, Form_EOTBEV, Field_AEID, subject);

    if (Dp_TTREAS != null && Dp_AEID != null && string.Compare(Dp_TTREAS.Data.Trim(), string.Empty, true) != 0 &&
        string.Compare(Dp_TTREAS.Data.Trim(), "3", true) == 0 &&
        string.Compare(Dp_AEID.Data.Trim(), string.Empty, true) != 0)
    {
        flag = true;
    }

    int Repos = GetRecordno(Dp_AEID);

    Rds = GetRecords(subject);

    if (Rds.Count > 0)
    {
        Rd = Rds.FindByRecordPosition(Repos);
        if (Rd != null)
        {
            Dp_AEACNBEV4 = Rd.DataPoints.FindByFieldOID("AEACNBEV4");
            if (Dp_AEACNBEV4 != null && string.Compare(Dp_AEACNBEV4.Data.Trim(), "1", true) != 0 && flag)
            {
                OpenQuery = true;
            }
        }
        if (Dp_AEID!= null)
            CustomFunction.PerformQueryAction(QT, 1, false, false, Dp_AEID, OpenQuery, afp.CheckID, afp.CheckHash);
    }
    return null;
}
int GetRecordno(DataPoint Dp_AEID)
{
    int split = 0;
    if(Dp_AEID!=null && string.Compare(Dp_AEID.Data.Trim(), string.Empty, true)!=0)
    {
        if(Dp_AEID.Data.Trim().Contains("_"))
        {
            string[] Arr = Dp_AEID.Data.Trim().Split(new char[]  { '_' });
            if (Arr[0]!=string.Empty)
            {
                split = Convert.ToInt32(Arr[0].ToString());
                if(split!=0)
                {
                    return split;
                }
            }
        }
    }
    return split;
}
Records GetRecords(Subject sub)
{
    Records Rds_Col = new Records();
    Instance Ins_AE = sub.Instances.FindByFolderOID("AE");
    if(Ins_AE!=null)
    {
        DataPage Dpg_AE = Ins_AE.DataPages.FindByFormOID("AE");
        if(Dpg_AE!=null)
        {
            Rds_Col = Dpg_AE.Records;
            if(Rds_Col.Count>0)
            {
                return Rds_Col;
            }
        }
    }
    return Rds_Col;
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

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
public class EOTBEV_004 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*Developer Name: Sandeep Kanike, Edetek
      Date:06-NOV-2025
      CF Name: EOTBEV_004
      Edit check name: EOTBEV_004, EOTBEV_004_CROSS
      Description: Treatment discontinuation decision date should be equal to or after Last Dose Dtae (Maximum Infusion Stop Date on Bevacizumab Administration form). When
     <EOTBEV.TTDAT & EXBEV.EXENDAT are not null>.*/

    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpAction = afp.ActionDataPoint;
    Subject subject = dpAction.Record.Subject;

    string FormOID_EXBEV = "EXBEV", FieldOID_EXENDAT = "EXENDAT";
    string FormOID_EOTBEV = "EOTBEV", FieldOID_TTDAT = "TTDAT", FolderOID_EOT = "EOT";
    DataPoint Dp_TTDAT = null, Dp_EXENDAT = null;
    bool OpenQuery = false;

    string QT = "Treatment discontinuation decision date is prior to the last Infusion Stop Date on Bevacizumab Administration form. Please verify & update.";

    DataPoints Dps_EXENDAT = CustomFunction.FetchAllDataPointsForOIDPath(FieldOID_EXENDAT, FormOID_EXBEV, null, subject);

    Dp_TTDAT = GetDatapoint(FolderOID_EOT, FormOID_EOTBEV, FieldOID_TTDAT, subject);

    Dp_EXENDAT = GetlatestDp(Dps_EXENDAT);

    if (Dp_EXENDAT != null && Dp_TTDAT != null && Dp_EXENDAT.StandardValue() is DateTime &&
        Dp_TTDAT.StandardValue() is DateTime && Convert.ToDateTime(Dp_TTDAT.StandardValue()) <
        Convert.ToDateTime(Dp_EXENDAT.StandardValue()))
    {
        OpenQuery = true;
    }
    if (Dp_TTDAT != null)
        CustomFunction.PerformQueryAction(QT, 1, false, false, Dp_TTDAT, OpenQuery, afp.CheckID, afp.CheckHash);

    return null;
}


DataPoint GetDatapoint(string FolderOID, string FormOID, string FieldOID, Subject sub)
{
    Instance Ins_EOT = sub.Instances.FindByFolderOID(FolderOID);
    if(Ins_EOT!=null)
    {
        DataPage Dpg_EOTBEV = Ins_EOT.DataPages.FindByFormOID(FormOID);
        if(Dpg_EOTBEV!=null)
        {
            DataPoint Dp_TTDAT = Dpg_EOTBEV.MasterRecord.DataPoints.FindByFieldOID(FieldOID);
            if(Dp_TTDAT!=null)
            {
                return Dp_TTDAT;
            }
        }
    }
    return null;
}
DataPoint GetlatestDp(DataPoints Dps_EXENDAT)
{
    DataPoint Dp_Date = null, Dp_latest = null;
    DateTime Dt = DateTime.MinValue;
    if(Dps_EXENDAT.Count>0)
    {
        for(int i=0;i< Dps_EXENDAT.Count;i++)
        {
            Dp_Date = Dps_EXENDAT[i];
            if(Dp_Date != null && Dp_Date.Active && Dp_Date.StandardValue() is DateTime)
            {
                DateTime Dt1 = Convert.ToDateTime(Dp_Date.StandardValue());
                if(Dt == DateTime.MinValue || Dt1 > Dt)
                {
                    Dt = Dt1;
                    Dp_latest = Dp_Date;
                }
            }
        }
    }
    return Dp_latest;
}
}
}

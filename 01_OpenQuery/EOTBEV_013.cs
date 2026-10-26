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
public class EOTBEV_013 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*Developer Name: Sandeep Kanike, Edetek
          Date:06-NOV-2025
          CF Name: EOTBEV_013
          Edit check name: EOTBEV_013, EOTBEV_013_CROSS
          Description: If TTREAS=Pregnancy then "Result" on latest Pregnancy Test form must be 'Positive'. When
<EOTBEV.TTREAS & LBPREG.LBORRES are not null>  */

        ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
        DataPoint dpAction = afp.ActionDataPoint;
        Subject subject = dpAction.Record.Subject;

        string Field_TTREAS = "TTREAS", Form_EOTBEV = "EOTBEV", Folder_EOT = "EOT";
        string Field_LBHCG = "LBHCG", Field_LBDAT = "LBDAT", Form_LBPREG = "LBPREG";
        DataPoint Dp_LBHCG = null, Dp_LBDAT = null;
        bool Openquery = false;
        string QT = "Reason for treatment discontinuation was reported as 'Pregnancy'; however, \"Result\" on latest Pregnancy Test form is 'Negative'. Please verify & update.";

        DataPoint Dp_TTREAS = GetDatapoint(Folder_EOT, Form_EOTBEV, Field_TTREAS, subject);

        DataPoints Dps_LBDAT = CustomFunction.FetchAllDataPointsForOIDPath(Field_LBDAT, Form_LBPREG, null, subject);

        if (Dps_LBDAT.Count > 0)
        {
            Dp_LBDAT = GetlatestDate(Dps_LBDAT);
            if (Dp_LBDAT != null)
            {
                Dp_LBHCG = Dp_LBDAT.Record.DataPage.MasterRecord.DataPoints.FindByFieldOID(Field_LBHCG);
                if (Dp_TTREAS != null && Dp_LBHCG != null && string.Compare(Dp_TTREAS.Data.Trim(), "4", true) == 0 &&
                    string.Compare(Dp_LBHCG.Data.Trim(), "NEGATIVE", true) == 0)
                {
                    Openquery = true;
                }
            }
            if (Dp_TTREAS != null)
                CustomFunction.PerformQueryAction(QT, 1, false, false, Dp_TTREAS, Openquery, afp.CheckID, afp.CheckHash);
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
    DataPoint GetlatestDate(DataPoints Dps)
    {
        DataPoint Dp_Date = null, Dp_latest = null;
        DateTime Dt = DateTime.MinValue;
        if (Dps.Count > 0)
        {
            for (int i = 0; i < Dps.Count; i++)
            {
                Dp_Date = Dps[i];
                if (Dp_Date != null && Dp_Date.Active && Dp_Date.StandardValue() is DateTime)
                {
                    DateTime Dt1 = Convert.ToDateTime(Dp_Date.StandardValue());
                    if (Dt == DateTime.MinValue || Dt1 > Dt)
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

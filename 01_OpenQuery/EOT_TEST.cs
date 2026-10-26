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
public class EOT_TEST : CustomFunctionBase
{
public object Eval(object ThisObject)
{
//Updated on 09-01-2026

        ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
        DataPoint dpAction = afp.ActionDataPoint;
        Subject subject = dpAction.Record.Subject;

        bool fireQuery = false;
        string querytext = "'If Reason is Progressive Disease, specify type of progression' was reported as " +
        "'Disease Progression per RECIST V1.1'; however, \"Overall response\" is not equal on any Response Assessment form." +
        " Please verify & update.";

        DataPoint dpPDSP = null, Dp_TTREAS = null;
        DataPoints dpsPDSP = CustomFunction.FetchAllDataPointsForOIDPath("PDSP", "EOT", "EOT", subject);
        if (dpsPDSP.Count > 0)
        {
            dpPDSP = dpsPDSP[0];
            if (dpPDSP != null)
            {
                Dp_TTREAS = dpPDSP.Record.DataPage.MasterRecord.DataPoints.FindByFieldOID("TTREAS");
            }
        }
        else return null;
        //return null when EOT form is invisible

        DataPoints dpsRAOVER = CustomFunction.FetchAllDataPointsForOIDPath("RAOVER", "TURA", null, subject);
        bool OpenQuery = GetCondition(dpsRAOVER);

        if (dpPDSP != null && dpPDSP.Active & dpPDSP.Data.Trim() == "DISEASE PROGRESSION PER RECIST V1.1" &&
Dp_TTREAS != null && Dp_TTREAS.Active && string.Compare(Dp_TTREAS.Data.Trim(), "2", true) == 0 && OpenQuery)
        {
            fireQuery = true;
        }
        if (dpPDSP != null)
            CustomFunction.PerformQueryAction(querytext, 1, false, false, dpPDSP, fireQuery, afp.CheckID, afp.CheckHash);
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
}
}

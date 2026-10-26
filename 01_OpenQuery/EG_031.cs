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
public class EG_031 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*

        Programmer Name: Rananay Singh/14Apr2022
        * Custom Function: EG_031
        * Edit Check(s): EG_031, EG_031_1
        * Descriptions: 'Pre-infusion' samples should be collected prior to the Infusion Start Time (EXSTTIM) for the same visit.
        When <EG.EGTPT ='Pre-dose' and EX.EXSTTIM & EG.EGTIM are not null>.

        **************/
        ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint dpAction = afp.ActionDataPoint;
        Subject subject = dpAction.Record.Subject;
        Instance InstCurrent = dpAction.Record.Instance;
        bool fireQuery = false;
        string querytext = " 'Pre-infusion' sample is not collected prior to the Infusion Start Time. Please verify & update.";
        DataPoints dpts_LBTIM = new DataPoints();
        string qTfield = "EGTIM", qDfield = "EGDAT", qTPTfield = "EGTPT", sTfield = "EXSTTIM", sDfield = "EXSTDAT";
        string qForm = "EG", SForm = "EX";
        string timepoint = "1";
        if (dpAction.Field.OID == qTfield) dpts_LBTIM.Add(dpAction);

        else
        dpts_LBTIM = CustomFunction.FetchAllDataPointsForOIDPath(qTfield, qForm, null, subject);


        for (int i = 0; i < dpts_LBTIM.Count; i++)
        {
            if (!dpts_LBTIM[i].Active || dpts_LBTIM[i].Record.RecordPosition == 0) continue;

            fireQuery = false;

            if (dpts_LBTIM[i].StandardValue() is TimeSpan)
            {
                DataPoint dpLBDAT = dpts_LBTIM[i].Record.DataPage.MasterRecord.DataPoints.FindByFieldOID(qDfield);
                DataPoint dpLBTPT = dpts_LBTIM[i].Record.DataPoints.FindByFieldOID(qTPTfield);
                if (dpLBDAT != null && dpLBTPT != null && dpLBTPT.Data == timepoint && dpLBDAT.StandardValue() is DateTime)
                {
                    DateTime dt1 = ((DateTime) dpLBDAT.StandardValue()).Add((TimeSpan) dpts_LBTIM[i].StandardValue());
                    DateTime dt2 = GetEXDateTime(dpts_LBTIM[i], sTfield, sDfield, SForm);

                    if (dt2 != DateTime.MinValue && (dt1 >= dt2)) fireQuery = true;
                }
            }
            CustomFunction.PerformQueryAction(querytext, 1, false, false, dpts_LBTIM[i], fireQuery, afp.CheckID, afp.CheckHash);
        }

        return null;
    }

    DateTime GetEXDateTime(DataPoint dpt, string sTfield, string sDfield, string SForm)
    {
        DateTime dt = DateTime.MinValue;

        Instance inst_curr = dpt.Record.Instance;
        if (inst_curr != null && inst_curr.Active)
        {
            DataPage dpgSRC = inst_curr.DataPages.FindByFormOID(SForm);
            if (dpgSRC != null && dpgSRC.Active)
            {
                DataPoint dptTime = dpgSRC.MasterRecord.DataPoints.FindByFieldOID(sTfield);
                DataPoint dptDate = dpgSRC.MasterRecord.DataPoints.FindByFieldOID(sDfield);

                if (dptTime != null && dptDate != null && dptTime.StandardValue() is TimeSpan && dptDate.StandardValue() is DateTime)
                {
                    dt = ((DateTime) dptDate.StandardValue()).Add((TimeSpan) dptTime.StandardValue());
                }
            }
        }
        return dt;
}
}
}

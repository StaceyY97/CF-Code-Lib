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
public class EG_035 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*

        Programmer Name: Rananay Singh/14Apr2022
        * Custom Function: EG_035
        * Edit Check(s): EG_035, EG_035_1
        * Descriptions: '24 Hours Post-EOI' samples should be collected within 24 (±6) hour after the Infusion Stop Time (EXSTTIM) of the same Cycle.
        When <EG.EGTPT =''24 Hours Post-EOI'' and EX.EXSTTIM & EG.EGTIM are not null>.
        **************/
        ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint dpAction = afp.ActionDataPoint;
        Subject subject = dpAction.Record.Subject;
        Instance InstCurrent = dpAction.Record.Instance;
        bool fireQuery = false;
        string querytext = "'24 Hours Post-EOI' sample is not collected within 24 (±6) hours after the Infusion Stop Time. Please verify & update.";
        DataPoints dpts_LBTIM = new DataPoints();
        string qTfield = "EGTIM", qDfield = "EGDAT", qTPTfield = "EGTPT", sTfield = "EXENTIM", sDfield = "EXENDAT";
        string qForm = "EG", SForm = "EX";
        string timepoint = "5";

        System.Collections.Generic.List<string> FOlders = new System.Collections.Generic.List<string>
        {
            "C1D2", "C3D2"
        }
        ;



        if (dpAction.Field.OID == qTfield) dpts_LBTIM.Add(dpAction);

        else
        dpts_LBTIM = CustomFunction.FetchAllDataPointsForOIDPath(qTfield, qForm, null, subject);


        for (int i = 0; i < dpts_LBTIM.Count; i++)
        {
            if (!dpts_LBTIM[i].Active || dpts_LBTIM[i].Record.RecordPosition == 0 || dpts_LBTIM[i].Record.Instance == null || !dpts_LBTIM[i].Record.Instance.Active || !FOlders.Contains(dpts_LBTIM[i].Record.Instance.Folder.OID)) continue;

            fireQuery = false;

            if (dpts_LBTIM[i].StandardValue() is TimeSpan)
            {
                DataPoint dpLBDAT = dpts_LBTIM[i].Record.DataPage.MasterRecord.DataPoints.FindByFieldOID(qDfield);
                DataPoint dpLBTPT = dpts_LBTIM[i].Record.DataPoints.FindByFieldOID(qTPTfield);
                if (dpLBDAT != null && dpLBTPT != null && dpLBTPT.Data == timepoint && dpLBDAT.StandardValue() is DateTime)
                {
                    DateTime dt1 = ((DateTime) dpLBDAT.StandardValue()).Add((TimeSpan) dpts_LBTIM[i].StandardValue());
                    DateTime dt2 = GetEXDateTime(dpts_LBTIM[i], sTfield, sDfield, SForm);

                    if (dt2 != DateTime.MinValue && (dt1 < dt2.AddHours(18) || dt1 > dt2.AddHours(30))) fireQuery = true;
                }
            }
            CustomFunction.PerformQueryAction(querytext, 1, false, false, dpts_LBTIM[i], fireQuery, afp.CheckID, afp.CheckHash);
        }

        return null;
    }

    DateTime GetEXDateTime(DataPoint dpt, string sTfield, string sDfield, string SForm)
    {
        DateTime dt = DateTime.MinValue;
        string folderOID = string.Empty;
        Instance inst_curr = dpt.Record.Instance;
        if (inst_curr != null && inst_curr.Active)
        {
            if (inst_curr.Folder.OID == "C1D2") folderOID = "C1D1";
            else if (inst_curr.Folder.OID == "C3D2") folderOID = "C3D1";
            if (folderOID != string.Empty)
            {
                Instance parent = inst_curr.ParentInstance;
                if (parent != null && parent.Active)
                {
                    Instance instCycle = parent.Instances.FindByFolderOID(folderOID);
                    if (instCycle != null && instCycle.Active)
                    {
                        DataPage dpgSRC = instCycle.DataPages.FindByFormOID(SForm);
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
                }
            }
        }
        return dt;
}
}
}

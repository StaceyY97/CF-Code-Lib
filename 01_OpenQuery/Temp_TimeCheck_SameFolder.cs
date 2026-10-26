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
public class ADA_008 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
        /** HEADER INFORMATION:**************************
        * Programmer Name:Stacey Yang
        * Description: compare time in specific time range, all fields should be in the same visit, works for both log and non-log forms
        * **/
        
        ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint dpAction = afp.ActionDataPoint;
        Subject subject = dpAction.Record.Subject;
        Instance InstCurrent = dpAction.Record.Instance;

        bool fireQuery = false;
        string querytext = "Sample is not collected up to 30 minutes prior to the Infusion Start Time. Please verify & update.";
        DataPoints dpts_LBTIM = new DataPoints();
        string qTfield = "ADATIM", qDfield = "ADADAT", sTfield = "EXSTTIM", sDfield = "EXSTDAT";
        string qForm = "ADA", SForm = "EX";
        if (dpAction.Field.OID == qTfield) dpts_LBTIM.Add(dpAction);
        else dpts_LBTIM = CustomFunction.FetchAllDataPointsForOIDPath(qTfield, qForm, InstCurrent.Folder.OID, subject);


        for (int i = 0; i < dpts_LBTIM.Count; i++)
        {
            if (dpts_LBTIM[i] == null || !dpts_LBTIM[i].Active) continue;
            fireQuery = false;

            if (isValuedTime(dpts_LBTIM[i]))
            {
                DataPoint dpLBDAT = dpts_LBTIM[i].Record.DataPoints.FindByFieldOID(qDfield);
                DataPoint Compare_DT = GetDataPointByOID(dpts_LBTIM[i], sDfield);
                DataPoint Compare_TM = GetDataPointByOID(dpts_LBTIM[i], sTfield);

                if (isValuedDate(dpLBDAT) && isValuedDate(Compare_DT) && isValuedTime(Compare_TM))
                {
                    DateTime dt1_Query = ConvertToDate(dpLBDAT, dpts_LBTIM[i]);
                    DateTime dt2_Compare = ConvertToDate(Compare_DT, Compare_TM);

                    if (dt1_Query != DateTime.MinValue && dt2_Compare != DateTime.MinValue && (dt1_Query.AddMinutes(30) > dt2_Compare)) fireQuery = true;
                }
            }
            CustomFunction.PerformQueryAction(querytext, 1, false, false, dpts_LBTIM[i], fireQuery, afp.CheckID, afp.CheckHash);
        }

        DataPoint GetDataPointByOID(DataPoint dp, string OID) => dp.Record.Instance.DataPages.FindByFormOID(SForm) ?.MasterRecord.DataPoints.FindByFieldOID(OID);
        bool isValuedDate(DataPoint checkDT) => checkDT != null && checkDT.Active && checkDT.StandardValue() is DateTime && checkDT.Data != string.Empty;
        bool isValuedTime(DataPoint checkDT) => checkDT != null && checkDT.Active && checkDT.StandardValue() is TimeSpan && checkDT.Data != string.Empty;
        DateTime ConvertToDate(DataPoint date, DataPoint time) => (isValuedDate(date) && isValuedTime(time)) ? ((DateTime) date.StandardValue()).Add((TimeSpan) time.StandardValue()) : DateTime.MinValue;

        return null;
}
}
}

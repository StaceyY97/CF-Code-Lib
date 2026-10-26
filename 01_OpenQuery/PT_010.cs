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
public class PT_010 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*
        Programmer Name: MXZ/18Jan2024
        * Custom Function: PT_010
        * Edit Check(s): PT_014, PT_014_A
        * Descriptions: If Setting= Metastatic then Start Date must be equal to or after Date of metastatic diagnosis on Disease History form
When <SCR.DH.DHMETDAT & SCR.PT.PTSTDAT are not null>
        Note: Consider only known date parts.
        If Day is unknown but Month & Year are same do not fire query.
        If Day & Month are unknown but Year is same do not fire query.
        **************/
        ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint dpEnter = afp.ActionDataPoint;
        Subject subject = dpEnter.Record.Subject;
        bool fireQuery = false;
        string querytext = "Setting equals 'Metastatic'; however, Start Date is prior to the Date of metastatic diagnosis recorded on the Disease History form. Please verify & update.";

        string sDfield = "DHMETDAT", SForm = "DH";
        string triggerfield = "PTSET", triggerForm = "PT";

        //check conditon
        Instance sameIns = dpEnter.Record.DataPage.Instance;
        string sameFolder = sameIns.Folder.OID;
        DataPage trggerPG = sameIns.DataPages.FindByFormOID(triggerForm);
        if (trggerPG == null) return null;
        DataPoint conditionDPs = trggerPG.MasterRecord.DataPoints.FindByFieldOID(triggerfield);
        if (conditionDPs == null) return null;
        bool trigger = false;
        if (conditionDPs.Data == string.Empty || string.Compare(conditionDPs.Data, "META", true) != 0) trigger = true;

        //get all start date,and close all query when condition not meet,close CF
        DataPoints allLogDate = CustomFunction.FetchAllDataPointsForOIDPath("PTSTDAT", triggerForm, sameFolder, subject);
        if (allLogDate == null) return null;
        if (trigger)
        {
            foreach (DataPoint dpClose in allLogDate)
            {
                CustomFunction.PerformQueryAction(querytext, 1, false, false, dpClose, false, afp.CheckID, afp.CheckHash);
            }
            return null;
        }

        //get the compare date
        DataPoint compareDate = dpEnter.Record.DataPage.Instance.DataPages.FindByFormOID(SForm).MasterRecord.DataPoints.FindByFieldOID(sDfield);
        if (compareDate == null || !compareDate.Active) return null;
        if (!(compareDate.StandardValue() is DateTime)) return null;
        DateTime dt2 = (DateTime) compareDate.StandardValue();

        if (dpEnter.Field.OID == "PTSTDAT")
        {
            fireQuery = DoQuery(dpEnter, compareDate, dt2);
            CustomFunction.PerformQueryAction(querytext, 1, false, false, dpEnter, fireQuery, afp.CheckID, afp.CheckHash);
        }
        else
        {
            foreach (DataPoint dpAction in allLogDate)
            {
                fireQuery = DoQuery(dpAction, compareDate, dt2);
                CustomFunction.PerformQueryAction(querytext, 1, false, false, dpAction, fireQuery, afp.CheckID, afp.CheckHash);
            }
        }
        return null;
    }

    bool CheckCondition1(DataPoint dpt_QueryField, DataPoint compareDate, DateTime dt2, DateTime dt1)
    {
        bool doquery = false;
        if (dt1.Year.ToString().Contains("UNK") || dt2.Year.ToString().Contains("UNK")) return doquery;
        if (dt1.Year < dt2.Year) doquery = true;
        if (!doquery && dt1.Year == dt2.Year)
        {
            if (!dpt_QueryField.Data.ToUpper().Contains("UNK") && !compareDate.Data.Contains("UNK"))
            {
                if (dt1.Month < dt2.Month) doquery = true;
                if (!doquery && dt1.Month == dt2.Month)
                {
                    if (!dpt_QueryField.Data.ToUpper().StartsWith("UN") && !compareDate.Data.ToUpper().StartsWith("UN"))
                    doquery = !(dt1.Day >= dt2.Day);
                }
            }
        }
        return doquery;
    }

    bool DoQuery(DataPoint dpEnter, DataPoint compareDate, DateTime dt2)
    {
        bool fireQuery = false;
        if (dpEnter.StandardValue() is DateTime)
        {
            DateTime dt1 = (DateTime) dpEnter.StandardValue();
            fireQuery = CheckCondition1(dpEnter, compareDate, dt2, dt1);
        }
        return fireQuery;
}
}
}

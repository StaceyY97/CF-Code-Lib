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
public class DH_014 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*

        Programmer Name: Lixia shuai/18Jan2024
        * Custom Function: DH_010
        * Edit Check(s): DH_014, DH_014_A
        * Descriptions: Date of locally advanced diagnosis should be prior to Informed Consent Date. When
        <SCR.DH.DHDIGDAT & SCR.DSCONS.DSDAT are not null>

        Note: Consider only known date parts.
        If Day is unknown but Month & Year are same do not fire query.
        If Day & Month are unknown but Year is same do not fire query.


        **************/
            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpAction = afp.ActionDataPoint;
            Subject subject = dpAction.Record.Subject;

            bool fireQuery = false;
            string querytext = "Date of locally advanced diagnosis is equal to or after Informed Consent Date. Please review & update.";

            string sDfield = "DSDAT";
            string SForm = "DSCONS", Sfolder = "SCR";

            DateTime dt2 = GetEXDate(sDfield, SForm, Sfolder, subject);


            if (dpAction.StandardValue() is DateTime && !dpAction.Data.ToUpper().Contains("UNKN"))
            {
                DateTime dt1 = (DateTime)dpAction.StandardValue();

                if (dt2 != DateTime.MinValue)
                {
                    fireQuery = CheckCondition1(dpAction, dt2, dt1);

                }
            }
            CustomFunction.PerformQueryAction(querytext, 1, false, false, dpAction, fireQuery, afp.CheckID, afp.CheckHash);

            return null;
        }

        bool CheckCondition1(DataPoint dpt_QueryField, DateTime dt2, DateTime dt1)
        {
            bool doquery = false;
            if (dt1.Year > dt2.Year) doquery = true;

            if (!doquery && dt1.Year == dt2.Year)
            {
                if (!dpt_QueryField.Data.ToUpper().Contains("UNK"))
                {
                    if (dt1.Month > dt2.Month) doquery = true;
                    if (!doquery && dt1.Month == dt2.Month)
                    {
                        if (!dpt_QueryField.Data.ToUpper().StartsWith("UN"))
                            doquery = dt1.Day >= dt2.Day;
                    }
                }
            }
            return doquery;
        }
        bool CheckCondition(DataPoint dpt_QueryField, DateTime dt2, DateTime dt1)
        {
            bool doquery = false;
            if (!dpt_QueryField.Data.ToUpper().StartsWith("UN") && !dpt_QueryField.Data.ToUpper().Contains("UNK"))
            {
                if (dt1 >= dt2) doquery = true;
            }
            else if (dpt_QueryField.Data.ToUpper().StartsWith("UN") && dpt_QueryField.Data.ToUpper().Contains("UNK"))
            {
                if (dt1.Year > dt2.Year) doquery = true;
            }
            else if (dpt_QueryField.Data.ToUpper().StartsWith("UN") && !dpt_QueryField.Data.ToUpper().Contains("UNK"))
            {
                if (dt1.Month > dt2.Month) doquery = true;
            }
            return doquery;
        }

        DateTime GetEXDate(string sDfield, string SForm, string Sfolder, Subject sub)
        {
            DataPoints dpts = CustomFunction.FetchAllDataPointsForOIDPath(sDfield, SForm, Sfolder, sub);
            for (int i = 0; i < dpts.Count; i++)
            {
                if (!dpts[i].Active || dpts[i].Record.RecordPosition != 0) continue;

                if (dpts[i].StandardValue() is DateTime) return (DateTime)dpts[i].StandardValue();
            }
            return DateTime.MinValue;
}
}
}

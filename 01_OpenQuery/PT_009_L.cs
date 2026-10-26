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
public class PT_009_L : CustomFunctionBase
{
public object Eval(object ThisObject)
{
ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint dpEnter = afp.ActionDataPoint;
        Subject subject = dpEnter.Record.Subject;
        bool fireQuery = false;
        string querytext = "Setting equals 'Locally Advanced'; however, Start Date is prior to the Date of locally advanced diagnosis recorded on the Disease History form. Please verify & update.";
        string sDfield = "DHDIGDAT", SForm = "DH";
        string sDfield_1 = "DHCAT";
        string triggerfield_1 = "PTSTDAT", triggerfield_2 = "PTSET", triggerForm = "PT";
        string InsSCR = "SCR";
        string strSET = "LOCA";

        if (dpEnter.Field.OID == triggerfield_1)
        {
            if (dpEnter.StandardValue() is DateTime && dpEnter.Active)
            {
                DataPoint dp_PTSET = dpEnter.Record.DataPage.MasterRecord.DataPoints.FindByFieldOID(triggerfield_2);
                if (dp_PTSET.Active && dp_PTSET.Data == strSET)
                {
                    DataPoint dp_DAT = GetDataPoint(subject, "", InsSCR, SForm, sDfield);
                    if (dp_DAT != null && dp_DAT.Active && dp_DAT.StandardValue() is DateTime && dp_DAT.IsVisible)
                    {
                        DateTime dt_DAT = (DateTime) dp_DAT.StandardValue();
                        DateTime dt_PTSTDAT = (DateTime) dpEnter.StandardValue();
                        fireQuery = CheckCondition1(dpEnter, dp_DAT, dt_DAT, dt_PTSTDAT);
                    }
                }
            }
            CustomFunction.PerformQueryAction(querytext, 1, false, false, dpEnter, fireQuery, afp.CheckID, afp.CheckHash);
            //Enter field is "PTSTDAT", close or open query accroding to value of setting field in the same PT form and value in sDfield field
        }


        if (dpEnter.Field.OID == sDfield)
        {
         
            if (dpEnter.StandardValue() is DateTime && dpEnter.Active)
            {
                DateTime dt_DAT = (DateTime) dpEnter.StandardValue();
                DataPages dtps_PT = dpEnter.Record.Instance.DataPages.FilterDataPagesByFormOID(triggerForm);
                if (dtps_PT != null && dtps_PT.Count != 0) // if no PT form, no action will taken
                {
                    foreach (DataPage dtp_PT in dtps_PT)
                    {

                        DataPoint dp_PTSET = dtp_PT.MasterRecord.DataPoints.FindByFieldOID(triggerfield_2);
                        bool bool_PTSET = (dp_PTSET != null && dp_PTSET.Active && dp_PTSET.Data == strSET);
                        foreach (Record log in dtp_PT.Records)
                        {
                            fireQuery = false;
                            if (log.RecordPosition == 0) continue;
                            DataPoint dp_PTSTDAT = log.DataPoints.FindByFieldOID(triggerfield_1);
                            if (dp_PTSTDAT != null && dp_PTSTDAT.Active && dp_PTSTDAT.StandardValue() is DateTime && bool_PTSET ) //If the condition is not met, query will be closed
                            {
                                DateTime dt_PTSTDAT = (DateTime) dp_PTSTDAT.StandardValue();
                                fireQuery = CheckCondition1(dp_PTSTDAT, dpEnter, dt_DAT, dt_PTSTDAT);
                            }
                            CustomFunction.PerformQueryAction(querytext, 1, false, false, dp_PTSTDAT, fireQuery, afp.CheckID, afp.CheckHash);
                        }
                    }
                }
            }
            //Enter field is "DHDIGDAT"
            //If DHDIGDAT is a complete date and status is active, find all PT form and all loglines in each PT form. Open query to PTSTDAT when setting is LOCA, dates is complete format and active, and PTSTDAT is greater than DHDIGDAT.
            else
            {
                DataPoints dpts_PTSTDAT = CustomFunction.FetchAllDataPointsForOIDPath(triggerfield_1, triggerForm, InsSCR, subject);
                foreach (DataPoint dp_PTSTDAT in dpts_PTSTDAT)
                {
                    CustomFunction.PerformQueryAction(querytext, 1, false, false, dp_PTSTDAT, fireQuery, afp.CheckID, afp.CheckHash);
                }
            }
            //If DHDIGDAT is an incomplete date or status is inactive, close related query on all PTSTDAT.
        }

        if (dpEnter.Field.OID == triggerfield_2)
        {

            DataPoint dp_DAT = GetDataPoint(subject, "", InsSCR, SForm, sDfield);
            bool bool_PTSET = (dpEnter.Active && dpEnter.Data == strSET && dp_DAT != null && dp_DAT.Active && dp_DAT.StandardValue() is DateTime && dp_DAT.IsVisible);
            foreach (Record log in dpEnter.Record.DataPage.Records)
            {

                if (log.RecordPosition == 0) continue;
                else
                {
                    DataPoint dp_PTSTDAT = log.DataPoints.FindByFieldOID(triggerfield_1);
                    if (bool_PTSET)
                    {
                        if (dp_PTSTDAT != null && dp_PTSTDAT.Active && dp_PTSTDAT.StandardValue() is DateTime && bool_PTSET)
                        {
                            DateTime dt_PTSTDAT = (DateTime) dp_PTSTDAT.StandardValue();
                            DateTime dt_DAT = (DateTime) dp_DAT.StandardValue();
                            fireQuery = CheckCondition1(dp_PTSTDAT, dpEnter, dt_DAT, dt_PTSTDAT);
                        }
                    }
                    CustomFunction.PerformQueryAction(querytext, 1, false, false, dp_PTSTDAT, fireQuery, afp.CheckID, afp.CheckHash);
                }
            }
        }
        //If enter field is "PTSET", find all PTSTDAT in the same form.
        return null;
    }

    bool CheckCondition1(DataPoint dp_PTSTDAT, DataPoint dp_DAT, DateTime dt_DAT, DateTime dt_PTSTDAT)
    {
        bool doquery = false;
        if (dp_PTSTDAT.Data.ToUpper().Contains("UNKN") || dp_DAT.Data.ToUpper().Contains("UNKN")) return doquery;
        if (dt_PTSTDAT.Year < dt_DAT.Year) doquery = true;
        if (!doquery && dt_PTSTDAT.Year == dt_DAT.Year)
        {
            if (!dp_PTSTDAT.Data.ToUpper().Contains("UNK") && !dp_DAT.Data.Contains("UNK"))
            {
                if (dt_PTSTDAT.Month < dt_DAT.Month) doquery = true;
                if (!doquery && dt_PTSTDAT.Month == dt_DAT.Month)
                {
                    if (!dp_PTSTDAT.Data.ToUpper().StartsWith("UN") && !dp_DAT.Data.ToUpper().StartsWith("UN"))
                    doquery = !(dt_PTSTDAT.Day >= dt_DAT.Day);
                }
            }
        }
        return doquery;
    }

    DataPoint GetDataPoint(Subject currentSubj, string parentFolderOID, string folderOID, string formOID, string feildOID)
    {
        DataPoint dt = null;
        Instance Folder = null;
        if (currentSubj != null && currentSubj.Active)
        {
            if (parentFolderOID != "")
            {
                Instance tempIns = currentSubj.Instances.FindByFolderOID(parentFolderOID);
                if (tempIns == null) return dt;
                Folder = tempIns.Instances.FindByFolderOID(folderOID);
                if (Folder == null) return dt;
            }
            else
            {
                Folder = currentSubj.Instances.FindByFolderOID(folderOID);
                if (Folder == null) return dt;
            }
            DataPage tempPage = Folder.DataPages.FindByFormOID(formOID);
            if (tempPage == null) return dt;
            DataPoint tempDP = tempPage.MasterRecord.DataPoints.FindByFieldOID(feildOID);
            if (tempDP == null) return dt;
            if (tempDP.Data != string.Empty)
            {
                return tempDP;
            }
        }
        return dt;
}
}
}

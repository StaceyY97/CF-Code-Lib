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
public class PT_009 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/***********************************************************************************
  Programmer Name: Qian Yang/27Feb2024
  * Custom Function: PT_009
  * Edit Check(s): PT_009, PT_009_A
  * Descriptions: Open and close two different queries in different sittuation on a same Datapoint.
 ************************************************************************************/
ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpEnter = afp.ActionDataPoint;
    Subject subject = dpEnter.Record.Subject;
    bool fireQuery = false;
    string querytext1 = "Setting equals 'Locally Advanced'; however, Start Date is prior to the Date of locally advanced diagnosis recorded on the Disease History form. Please verify & update.";
    string querytext2 = "Setting equals 'Metastatic'; however, Start Date is prior to the Date of metastatic diagnosis recorded on the Disease History form. Please verify & update.";
    string querytext = "";

    string sDfield_1 = "DHDIGDAT", sDfield_2 = "DHMETDAT", SForm = "DH", triggerForm = "PT";
    string sDfield = "PTSTDAT", triggerfield = "PTSET";

    Instance sameIns = dpEnter.Record.DataPage.Instance;
    DataPoint conditionDP = GetDataPoint(subject, "", sameIns.Folder.OID, triggerForm, triggerfield);
    if (conditionDP == null) return null;

    //Get all query fire dp
    DataPoints allLogDate = CustomFunction.FetchAllDataPointsForOIDPath("PTSTDAT", triggerForm, sameIns.Folder.OID, subject);
    if (allLogDate == null) return null;

    if (conditionDP.IsObjectChanged)
    {
        if (conditionDP.Data == string.Empty || (conditionDP.Data.ToUpper() != "LOCA" && conditionDP.Data.ToUpper() != "META"))
        {
            foreach (DataPoint temDP in allLogDate)
            {
                CustomFunction.PerformQueryAction(querytext1, 1, false, false, dpEnter, false, afp.CheckID, afp.CheckHash);
                CustomFunction.PerformQueryAction(querytext2, 1, false, false, dpEnter, false, afp.CheckID, afp.CheckHash);
            }
        }
        else if (conditionDP.Data.ToUpper() != "LOCA")
        {
            foreach (DataPoint temDP in allLogDate)
            {
                CustomFunction.PerformQueryAction(querytext1, 1, false, false, dpEnter, false, afp.CheckID, afp.CheckHash);
            }
        }
        else if (conditionDP.Data.ToUpper() != "META")
        {
            foreach (DataPoint temDP in allLogDate)
            {
                CustomFunction.PerformQueryAction(querytext2, 1, false, false, dpEnter, false, afp.CheckID, afp.CheckHash);
            }
        }
    }

    switch (conditionDP.Data)
    {
        case "LOCA":
            sDfield = sDfield_1;
            querytext = querytext1;
            break;
        case "META":
            sDfield = sDfield_2;
            querytext = querytext2;
            break;
    }

    if (sDfield == "" || querytext == "") return null;
    //get the compare date 
    DataPoint compareDate = GetDataPoint(subject, "", sameIns.Folder.OID, SForm, sDfield); ;
    if (compareDate == null || !compareDate.Active) return null;
    if (!(compareDate.StandardValue() is DateTime)) return null;
    DateTime dt2 = (DateTime)compareDate.StandardValue();

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
        DateTime dt1 = (DateTime)dpEnter.StandardValue();
        fireQuery = CheckCondition1(dpEnter, compareDate, dt2, dt1);
    }
    return fireQuery;
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

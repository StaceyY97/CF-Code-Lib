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
public class CF_BM_And_Flushing_Scoring : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*
        Created : Oanh Nguyen, JIRA# USTS-78900, KMP-9798
        Date : 22 Jan 2025
        Custom Function : CF_BM_And_Flushing_Scoring, CF_BM_And_Flushing_Scoring_CROSS
        Edit Check(s) : EC_BM_And_Flushing_Scoring, EC_BM_And_Flushing_Scoring_CROSS, EC_BM_And_Flushing_Scoring_CROSS1, EC_BM_And_Flushing_Scoring_CROSS2
        Description : 14-day average scores for Bowel Movement Frequency and Flushing Frequency of "Screening 1" folder and "Rescreening 1" folder
    */
    var afp = (ActionFunctionParams)ThisObject;
    return PreliminaryCheck(afp) ? Main(afp) : null;
}

// -- VARIABLES DECLARATION ------
const string FOLDER_OID_SCREEN_1 = "SCREEN_1",
        FOLDER_OID_RESCREEN_1 = "RESCREEN_1",
        FORM_OID_VIS = "VIS",
        FORM_OID_SDRS = "SDRS",
        FORM_OID_PDIARY = "PDIARY",
        FIELD_OID_VIS = "VISDAT",
        FIELD_OID_SDRSREC = "SDRSREC",
        FIELD_OID_SCRDOV = "SCRDOV",
        FIELD_OID_SDRSDER1 = "SDRSDER1",
        FIELD_OID_SDRSDAT = "SDRSDAT",
        CHECKED = "1",
        UNCHECKED = "0",
        FORMAT_DATE = "dd MMM yyyy",
        PREFIX_Rescreening = "R";
// -- END VARIABLES DECLARATION --

bool PreliminaryCheck(ActionFunctionParams afp)
{
    return true;
}

object Main(ActionFunctionParams afp)
{
    DataPoint dpAction = afp.ActionDataPoint;
    Subject subj = dpAction.Record.Subject;
    string folderOID = dpAction.Record.DataPage.Instance.Folder.OID;

    if (folderOID == null || (folderOID != FOLDER_OID_SCREEN_1 && folderOID != FOLDER_OID_RESCREEN_1)) 
        return null;

    bool isRescreening = folderOID == FOLDER_OID_RESCREEN_1;
    string form = isRescreening ? PREFIX_Rescreening + FORM_OID_SDRS : FORM_OID_SDRS;
    string checkField = isRescreening ? PREFIX_Rescreening + FIELD_OID_SDRSREC : FIELD_OID_SDRSREC;

    if (IsPerformAction(subj, folderOID, form, checkField))
    {
        PerformProcessing(subj, folderOID, form, checkField, isRescreening, dpAction);
    }

    return null;
}

bool IsValid(StatusObjectBase obj)
    => obj != null && obj.Active;
  
bool IsPerformAction(Subject subj, string folderOID, string form, string checkField)
{
    DataPoint dpVisitDate = FindDataPointInFolder(subj, folderOID, FORM_OID_VIS, FIELD_OID_VIS);
    DataPoint dpRecalculateCheck = FindDataPointInFolder(subj, folderOID, form, checkField);

    return IsValidDate(dpVisitDate) || IsQualifiedCondition(dpRecalculateCheck, CHECKED);
}

bool IsValidDate(DataPoint dp) =>
        dp != null && ConvertToDateTime(dp) != DateTime.MinValue;

bool IsQualifiedCondition(DataPoint dp, string valueCode) =>
        dp != null && dp.IsVisible && dp.CodedValue()?.ToString() == valueCode;

DateTime ConvertToDateTime(DataPoint dp) =>
    dp != null && dp.StandardValue() is DateTime dt
        ? dt
        : DateTime.MinValue;

DateTime ConvertTimeZone(int idTimeZone, DateTime dt)
{
    Timezone timezone = Timezone.Fetch(idTimeZone);
    TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(timezone.KeyName);
    return TimeZoneInfo.ConvertTimeFromUtc(dt, cstZone);
}

DataPoint FindDataPointInFolder(Subject sub, string folderOID, string formOID, string fieldOID)
{
    Instance ins = sub.Instances.FindByFolderOID(folderOID);
    DataPage dpg = ins?.DataPages.FindByFormOID(formOID);

    return dpg?.MasterRecord.DataPoints.FindByFieldOID(fieldOID);
}

void EnterData(DataPoint dp, string value)
{
    if (IsValid(dp) && dp.LockStatus != LockStatusEnum.Locked)
        dp.Enter(value, string.Empty, 0);
}

void PerformProcessing(Subject subj, string folderOID, string form, string checkField, bool isRescreening, DataPoint dp)
{
    string failureDateField = isRescreening ? PREFIX_Rescreening + FIELD_OID_SDRSDER1 : FIELD_OID_SDRSDER1;
    string derivedDateField = isRescreening ? PREFIX_Rescreening + FIELD_OID_SCRDOV : FIELD_OID_SCRDOV;

    DataPoint dpRecalculateCheck = FindDataPointInFolder(subj, folderOID, form, checkField);
    EnterData(dpRecalculateCheck, UNCHECKED);

    DataPoint dpECOADScreening1Date = FindDataPointInFolder(subj, folderOID, FORM_OID_PDIARY, derivedDateField);
    DataPoint dpECOADScreenFailureDate = FindDataPointInFolder(subj, folderOID, form, failureDateField);

    if (IsValidDate(dpECOADScreening1Date) || IsValidDate(dpECOADScreenFailureDate))
        return;

    string recentDateField = isRescreening ? PREFIX_Rescreening + FIELD_OID_SDRSDAT : FIELD_OID_SDRSDAT;

    DataPoint dpRecentlyDate = FindDataPointInFolder(subj, folderOID, form, recentDateField);
    string currentDate = ConvertTimeZone(dp.Interaction.TrueUser.TimeZone, DateTime.UtcNow).ToString(FORMAT_DATE);
    EnterData(dpRecentlyDate, currentDate);

    CustomFunction.PerformCustomFunction("CF_BM_And_Flushing_Scoring_CROSS", subj.CRFVersion.ID, new object[] { subj, dp });
}
}
}

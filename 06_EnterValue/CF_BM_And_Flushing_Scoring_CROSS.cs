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
public class CF_BM_And_Flushing_Scoring_CROSS : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*
        Created : Oanh Nguyen, JIRA# USTS-78900, KMP-9798
        Date : 22 Jan 2025
        Custom Function : CF_BM_And_Flushing_Scoring_CROSS
        Edit Check(s) : N/A
        Description : This custom function calculate BM and flushing score
    */

    object[] tObject = (object[])ThisObject;

    if (tObject.Length != 2)
        return null;

    Subject subj = tObject[0] as Subject;
    DataPoint dpAction = tObject[1] as DataPoint;

    if (subj == null || dpAction == null)
        return null;

    string folderOID = dpAction.Record.DataPage.Instance.Folder.OID;
    bool isRescreening = folderOID == FOLDER_OID_RESCREEN_1;
    string form = isRescreening ? PREFIX_Rescreening + FORM_OID_SDRS : FORM_OID_SDRS;
    string checkField = isRescreening ? PREFIX_Rescreening + FIELD_OID_SDRSREC : FIELD_OID_SDRSREC;

    Instance ins = FindInstance(subj, folderOID);
    DataPage dpgCSSDiary = FindDataPage(ins, FORM_OID_DIARY);
    DataPoint dpVisitDate = FindDataPointInFolder(subj, folderOID, FORM_OID_VIS, FIELD_OID_VIS);

    if (!IsValidDate(dpVisitDate))
        return null;

    DateTime dtVisitDate = ConvertToDateTime(dpVisitDate);
    DateTime dtEndDate = dtVisitDate.AddDays(13);
    Records recs = dpgCSSDiary.Records;
    Records loglines = new Records();

    for (int i = 1; i < recs.Count; i++)
    {
        Record rec = recs.FindByRecordPosition(i);
        if (rec == null)
            return null;

        DataPoint dpOpenDate = FindDataPoint(rec, FIELD_OID_META_OPEN_DATE);
        DateTime dtOpenDate = ConvertToDateTime(dpOpenDate);
        bool isQualifiedDate = dtOpenDate.Date >= dtVisitDate.Date && dtOpenDate.Date <= dtEndDate.Date;

        if (IsValidDate(dpOpenDate)
                && IsLoglineCompleted(rec)
                && isQualifiedDate
                && rec.Active)
                    loglines.Add(rec);
    }

    int logLinesCount = loglines.Count;
    string bMAField = isRescreening ? PREFIX_Rescreening + FIELD_OID_SDRSABM : FIELD_OID_SDRSABM;
    string fAField = isRescreening ? PREFIX_Rescreening + FIELD_OID_SDRSAFE : FIELD_OID_SDRSAFE;
    string iNDField = isRescreening ? PREFIX_Rescreening + FIELD_OID_SDRSIND : FIELD_OID_SDRSIND;
    DataPoint dpIND = FindDataPointInFolder(subj, folderOID, form, iNDField);

    if (logLinesCount <= 9)
        EnterData(dpIND, CHECKED);
    else
    {
        EnterData(dpIND, UNCHECKED);

        double bowelTotal = 0;
        double flusingTotal = 0;

        foreach (var item in loglines)
        {
            DataPoint dpBowel = FindDataPoint(item, FIELD_OID_BOWEL_FREQ);
            DataPoint dpFlusing = FindDataPoint(item, FIELD_OID_FLUSHING_FREQ);
            bowelTotal = SumValue(bowelTotal, ConvertToInt(dpBowel));
            flusingTotal = SumValue(flusingTotal, ConvertToInt(dpFlusing));
        }

        string bowelAvg = CalculateScore(bowelTotal, logLinesCount).ToString();
        string flusingAvg = CalculateScore(flusingTotal, logLinesCount).ToString();
        DataPoint dpBowelAvg = FindDataPointInFolder(subj, folderOID, form, bMAField);
        DataPoint dpFlushingAvg = FindDataPointInFolder(subj, folderOID, form, fAField);

        EnterData(dpBowelAvg, bowelAvg);
        EnterData(dpFlushingAvg, flusingAvg);
    }

    return null;
}

// -- VARIABLES DECLARATION ------
const string FOLDER_OID_RESCREEN_1 = "RESCREEN_1",
        FORM_OID_VIS = "VIS",
        FORM_OID_SDRS = "SDRS",
        FIELD_OID_VIS = "VISDAT",
        FIELD_OID_SDRSREC = "SDRSREC",
        CHECKED = "1",
        UNCHECKED = "0",
        FORM_OID_DIARY = "DIARY",
        FIELD_OID_SDRSIND = "SDRSIND",
        FIELD_OID_SDRSABM = "SDRSABM",
        FIELD_OID_SDRSAFE = "SDRSAFE",
        FIELD_OID_META_OPEN_DATE = "META_OPEN_DATE",
        FIELD_OID_META_COMPLETED_AT = "META_COMPLETED_AT",
        FIELD_OID_BOWEL_FREQ = "BOWEL_FREQ",
        FIELD_OID_FLUSHING_FREQ = "FLUSHING_FREQ",
        PREFIX_Rescreening = "R";
// -- END VARIABLES DECLARATION --
  
bool IsValid(StatusObjectBase obj)
    => obj != null && obj.Active;

bool IsValidDate(DataPoint dp) =>
        dp != null && ConvertToDateTime(dp) != DateTime.MinValue;   

bool IsLoglineCompleted(Record record) =>
        ConvertToDateTime(FindDataPoint(record, FIELD_OID_META_COMPLETED_AT)) != DateTime.MinValue;

int ConvertToInt(DataPoint dp) =>
        dp != null && int.TryParse(dp.Data, out int result)
            ? result
            : int.MinValue;

double CalculateScore(double sum, int average) => Math.Round(sum / average, 1);

double SumValue(double sum, int currentValue) => 
    currentValue == int.MinValue ? sum : sum + currentValue;

DateTime ConvertToDateTime(DataPoint dp) =>
    dp != null && dp.StandardValue() is DateTime dt
        ? dt
        : DateTime.MinValue;

DataPage FindDataPage(Instance ins, string formOID) => ins?.DataPages.FindByFormOID(formOID);

Instance FindInstance(Subject subj, string folderOID) =>
    subj.Instances.FindByFolderOID(folderOID);

DataPoint FindDataPoint(Record record, string fieldOID) =>
        record?.DataPoints.FindByFieldOID(fieldOID);

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
}
}

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
public class CF_Paper_Symptom : CustomFunctionBase
{
public object Eval(object ThisObject)
{
return Main((object[])ThisObject);
}

// -- VARIABLES DECLARATION ------
const string FOLDER_OID_D1 = "D1",
            FORM_OID_VIS = "VIS",
            FIELD_OID_VISDAT = "VISDAT",
            FOLDER_OID_WK16 = "WK16",
            FOLDER_OID_OLED1 = "OLED1",
            FOLDER_OID_D1_2 = "D1_2",
            FIELD_OID_PDDAT = "PDDAT",
            FIELD_OID_META_COMPLETED_AT = "PDTIM",
FIELD_OID_BOWEL_FREQ = "BOWEL_FREQ_P3", FIELD_OID_FLUSHING_FREQ = "FLUSHING_FREQ_P3";
DateTime dtNow;

// -- END VARIABLES DECLARATION --

object Main(object[] TObject)
{
    //if (!(TObject is object[])) return false;
    return IsSendEmail((Subject)TObject[0], (DataPoint)TObject[1]);
}

bool IsCurrentDate(DataPoint dpTmps) => ConvertToDateTime(dpTmps).Date == dtNow.Date;

/* Check if StatusObjectBase is not null and active */
bool IsValid(StatusObjectBase obj) => obj != null && obj.Active;
bool IsConformateDate(DataPoint dp) => ConvertToDateTime(dp) != DateTime.MinValue;
bool IsConformateTIM(DataPoint dp) => dp.StandardValue() is TimeSpan;
DataPoint FindDataPoint(Record record, string fieldOID) => record?.DataPoints.FindByFieldOID(fieldOID);
DateTime ConvertToDateTime(DataPoint dp) => (dp != null && dp.IsVisible && dp.StandardValue() is DateTime) ? (DateTime)dp.StandardValue() : DateTime.MinValue;
DataPoint FindDataPoint(Subject subj, string folderOID, string formOID, string fieldOID) =>
    subj.Instances.FindByFolderOID(folderOID)?.DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints.FindByFieldOID(fieldOID);

//DateTime GetWholeDT(DataPoint enterDAT)
//{
//    DataPoint EnterTIM = FindDataPoint(enterDAT.Record, FIELD_OID_META_COMPLETED_AT);
//    if (IsValid(enterDAT) && IsConformateDate(enterDAT) && IsValid(EnterTIM) && IsConformateTIM(EnterTIM))
//    {
//        return (DateTime)enterDAT.StandardValue() + (TimeSpan)EnterTIM.StandardValue();
//    }
//    return DateTime.MinValue;
//}
/*  Check if alert needs to be sent or not */
bool IsSendEmail(Subject subj, DataPoint dpTmps)
{
    if (!IsValid(dpTmps) || !IsConformateDate(dpTmps) || !dpTmps.IsObjectChanged) return false;

    if (FIELD_OID_BOWEL_FREQ == "" || FIELD_OID_FLUSHING_FREQ == "") return false;

    var idTimeZone = dpTmps.Interaction.TrueUser.TimeZone;
    TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(Timezone.Fetch(idTimeZone).KeyName);
    dtNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cstZone);

    DataPoint dpD1 = FindDataPoint(subj, FOLDER_OID_D1, FORM_OID_VIS, FIELD_OID_VISDAT);
    DataPoint dpD1_2 = FindDataPoint(subj, FOLDER_OID_D1_2, FORM_OID_VIS, FIELD_OID_VISDAT);
    //DataPoint dpWK16 = FindDataPoint(subj, FOLDER_OID_WK16, "VIS2", "VIS2DAT");
    //DataPoint dpOLED1 = FindDataPoint(subj, FOLDER_OID_OLED1, FORM_OID_VIS, FIELD_OID_VISDAT);
    //DataPoint dpRCPET = FindDataPoint(subj, "ET", FORM_OID_VIS, FIELD_OID_VISDAT);


    bool isMetConditionDate = (IsConformateDate(dpD1) || IsConformateDate(dpD1_2));
    //&& !IsConformateDate(dpWK16)&& !IsConformateDate(dpOLED1)&& !IsConformateDate(dpRCPET )

    if (!isMetConditionDate) return false;

    DataPoint dpFLUSHING_FREQ = FindDataPoint(dpTmps.Record, FIELD_OID_FLUSHING_FREQ);

    if (IsMetPastTwoConsecutiveLogs(dpTmps)) return true;

    //IsCurrentDate(dpTmps)

    return double.TryParse(dpFLUSHING_FREQ?.Data, out double valueFlushing) && valueFlushing >= 3 && dpTmps.Record.RecordPosition > 1;

    //IsCurrentDate(dpTmps) 
}

bool IsMetPastTwoConsecutiveLogs(DataPoint dpTmps)
{
    DataPoint dpBOWEL = FindDataPoint(dpTmps.Record, FIELD_OID_BOWEL_FREQ);

    if (!(double.TryParse(dpBOWEL?.Data, out double valueBowel) && valueBowel >= 4)) return false;

    DataPage dpgDiary = dpTmps.Record.DataPage;
    DateTime wholeDT_Current = ConvertToDateTime(dpTmps);
    for (int i = dpTmps.Record.RecordPosition - 1; i >= 1; i--)
    {
        Record rec = dpgDiary.Records[i];

        if (rec == null || !rec.Active) continue;

        DataPoint dpTmpsPast_DAT = FindDataPoint(rec, FIELD_OID_PDDAT);
        DateTime wholeDT_pre = ConvertToDateTime(dpTmpsPast_DAT);

        bool validGapDay = Math.Abs((wholeDT_Current - wholeDT_pre).Days) == 1;

        if (IsConformateDate(dpTmpsPast_DAT) && validGapDay)
        {
            DataPoint dpBOWELPast = FindDataPoint(dpgDiary.Records[i], FIELD_OID_BOWEL_FREQ);
            return double.TryParse(dpBOWELPast?.Data, out double valueBOWELPast) && valueBOWELPast >= 4;
        }
    }
    return false;
}
}
}

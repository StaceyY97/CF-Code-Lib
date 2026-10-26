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
public class EC_SendEmail_RES_RSMMYN : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
* Created By :Qian Yang/09Dec2025
* Custom Function :SendEmail_RES_RSMMYN
* Description: If RSMMYN is update to Y or any of  fields "RS_ECG / RS_HEMA1 / RS_CHEM1 / RS_URIN1 / RS_LPD1 / RS_SER / RS_HORM1 / RS_HBA1C1 / RS_PRIMAG / RS_FA / RS_OA / RS_TTE" Updated when RSMMYN=Y Then send mail 
* Edit Checks :SendEmail_RES_RSMMYN
*********************************************************************************************************/
        var afp = (ActionFunctionParams)ThisObject;
        return Main(afp);
    }

    // -- VARIABLES DECLARATION ------
    const string FIELD_OID_SUBJECT = "SUBJID";

    const string EML_FROM = "Re-Screening_Alert@mdsol.com";
    readonly string[] EML_TO_ROLES =
    {
    "Data Manager",
    "Medical Monitor",
    "Medical Monitor – Restricted Data Entry"
};
    string[] checkDP_1 = new string[] { "RS", "RSMMYN", "Y", "1" };
    string[] checkDPs_RS1 = new string[] { "RS_ECG", "RS_HEMA1", "RS_CHEM1", "RS_URIN1", "RS_LPD1", "RS_SER", "RS_HORM1", "RS_HBA1C1" };
    string[] checkDPs_RS2 = new string[] { "RS_PRIMAG", "RS_FA", "RS_OA", "RS_TTE" };

    string[] bodyDP = new string[] { "VIS", "VISDAT" };

    enum E_ALERT_TYPE
    {
        NONE = 0, // No alert
        INITIAL
    };

    readonly Dictionary<E_ALERT_TYPE, string> EML_SUBJECT = new Dictionary<E_ALERT_TYPE, string>()
{
    {
        E_ALERT_TYPE.INITIAL, "{0}CRN00808-12 Assessments done at the initial Screening to be reused at Rescreening"
    }
}
    ;

    readonly Dictionary<E_ALERT_TYPE, string> EML_BODY = new Dictionary<E_ALERT_TYPE, string>()
{
    {
        E_ALERT_TYPE.INITIAL,
        @"The site {0} confirmed subject {1} at {2} on {3}, the below assessments do not need to be repeated at Rescreening:
        <ul><b>Rescreening 1 Assessments:</b>        
        {4}
        <br><b>Rescreening 2 Assessments:</b>    
        {5}
        </ul><br>
        Data Management, please proceed to confirm approval by the Medial Monitor and ensure these forms are available for the site to enter data in the EDC.
        <br><br>Thank you"
    }
}
    ;
    // -- END VARIABLES DECLARATION --

    object Main(ActionFunctionParams afp)
    {
        DataPoint dpTmps = afp.ActionDataPoint;
        Subject subj = dpTmps.Record.Subject;

        bool isSendEmail = IsSendEmail(dpTmps);
        if (!isSendEmail) return null;

        string emailTo = (string)CustomFunction.PerformCustomFunction("CF_Aid_GetTo_ByRole", dpTmps.Field.CRFVersion.ID, new object[] { new List<string>() { "qian.yang@edetek.com", "yuxin.zeng@edetek.com" }, subj, EML_TO_ROLES });
        string emailSubject = GetEmailSubject(subj, E_ALERT_TYPE.INITIAL);
        string emailBody = GetEmailBody(subj, dpTmps);

        if (string.IsNullOrEmpty(emailTo) && string.IsNullOrEmpty(emailBody))
            return null;

        SendEmail(emailTo, EML_FROM, emailSubject, emailBody);

        return null;
    }

    public void SendEmail(string to, string from, string subject, string body)
    {
        const string HTML_TAG = "<.*?>";

        var mail = new System.Net.Mail.MailMessage();
        mail.From = new System.Net.Mail.MailAddress(from);
        mail.To.Add(to);
        mail.Subject = subject;
        mail.Body = Regex.Replace(body, HTML_TAG, string.Empty);

        var mimeType = new System.Net.Mime.ContentType("text/html");
        var alternate = System.Net.Mail.AlternateView.CreateAlternateViewFromString(body, mimeType);
        mail.AlternateViews.Add(alternate);
        var smtp = new System.Net.Mail.SmtpClient("smtp.mdsol.com");
        smtp.Send(mail);
    }

    /* Check if subject is in PROD or not */
    bool IsProduction(Subject subj) => subj.StudySite.Study.Environment.Trim().Equals("PROD", StringComparison.OrdinalIgnoreCase);

    /* Append study environment for non-PROD with parentheses */
    string GetEnvironment(Subject subj) => IsProduction(subj) ? "" : string.Format("{0} ", subj.StudySite.Study.Environment.Trim().ToUpper());

    /* Returns Email subject string */
    string GetEmailSubject(Subject subj, E_ALERT_TYPE type)
    {
        if (!EML_SUBJECT.ContainsKey(type))
            return string.Empty;

        var param = new List<string>();
        param.Add(GetEnvironment(subj));

        return string.Format(EML_SUBJECT[type], param.ToArray());
    }

    /* Returns Email body string */
    string GetEmailBody(Subject subj, DataPoint dpTmps)
    {
        DataPoint dpSUBJECT = subj.PrimaryDataPage.MasterRecord.DataPoints.FindByFieldOID(FIELD_OID_SUBJECT);

        string subjNumber = dpSUBJECT?.Data;

        if (string.IsNullOrEmpty(subjNumber))
            return string.Empty;

        string studySiteNum = subj.StudySite.StudySiteNumber.Trim();
        string visit = dpTmps.Record.Instance?.Name;
        string VISDAT = GetDataPoint_SameFodler(dpTmps, bodyDP[0], bodyDP[1])?.Data;
        string assessRS1 = GetAssessment(checkDPs_RS1, dpTmps);
        string assessRS2 = GetAssessment(checkDPs_RS2, dpTmps);

        var param = new List<string>()
    {
        studySiteNum,
        subjNumber,
        visit,
        VISDAT,
        assessRS1,
        assessRS2
    };

        var type = E_ALERT_TYPE.INITIAL;
        return string.Format(EML_BODY[type], param.ToArray());
    }

    string GetAssessment(string[] checkList, DataPoint dpTmps)
    {
        string result = string.Empty;
        List<string> list = new List<string>();
        foreach (string checkOID in checkList)
        {
            DataPoint checkDP = GetDataPoint_SameForm(dpTmps, checkOID);
            if (checkDP != null && checkDP.Active && checkDP.Data == "1") list.Add(GetPretextWithoutLable(checkDP.Field.PreText));
        }
        foreach (string text in list) result += $"<li>{text}</li>";
        return result;
    }
    string GetPretextWithoutLable(string PreText) => PreText.IndexOf(":") > 0 ? PreText.Substring(PreText.IndexOf(":") + 1).Replace("</b><br/><br/>", "") : PreText;

    DataPoint GetDataPoint_SameFodler(DataPoint dpTmps, string formOID, string fieldOID) => dpTmps.Record.Instance?.DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints?.FindByFieldOID(fieldOID);
    DataPoint GetDataPoint_SameForm(DataPoint dpTmps, string fieldOID) => dpTmps?.Record?.DataPage?.MasterRecord?.DataPoints?.FindByFieldOID(fieldOID);
    bool IsSendEmail(DataPoint dpTmps)
    {
        if (dpTmps != null && dpTmps.Active && dpTmps.Data == "Y")
        {
            if (dpTmps.IsObjectChanged) return true;
            else
            {
                foreach (string checkDP_OID in checkDPs_RS1)
                {
                    DataPoint checkDp = GetDataPoint_SameForm(dpTmps, checkDP_OID);
                    if (checkDp != null && checkDp.Active && checkDp.IsObjectChanged) return true;
                }
                foreach (string checkDP_OID in checkDPs_RS2)
                {
                    DataPoint checkDp = GetDataPoint_SameForm(dpTmps, checkDP_OID);
                    if (checkDp != null && checkDp.Active && checkDp.IsObjectChanged) return true;
                }
            }
        }
        return false;
}
}
}

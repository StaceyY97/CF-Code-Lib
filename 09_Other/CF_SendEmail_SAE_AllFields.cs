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
public class CF_SendEmail_SAE_AllFields : CustomFunctionBase
{
public object Eval(object ThisObject)
{
ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
        DataPoint enterDP = afp.ActionDataPoint;
        subj = enterDP.Record.Subject;
        Main(enterDP);
        return null;
    }

    const string from = "SAE_Alert@mdsol.com";
    Subject subj;
    string siteNumber, subjName, country, environment, studyName;
    string[] FieldOrder = new string[] { };
    string body_temp;

    void Main(DataPoint enterDP)
    {
        FieldOrder = GetFieldOrder(subj);
        Records allPageRC = enterDP.Record.DataPage.Records;
        foreach (Record rc in allPageRC)
        {
            if (rc == null || !rc.Active || rc.RecordPosition == 0) continue;
            DataPoint SAE = rc.DataPoints.FindByFieldOID("AESER");
            string type = ShouldSendMail(SAE);
            if (type != string.Empty)
            {
                InitialParameters();
                string To = GetTo(enterDP).Replace(";", ",");
                string Topic = GetTopic(type);
                body_temp = (string)CustomFunction.PerformCustomFunction("CF_Aid_GetMailBody", subj.CRFVersion.ID, new object[] { null });
                string body = GetBody(SAE, type);
                if (To != string.Empty && from != string.Empty && Topic != string.Empty && body != string.Empty) SendEmail(To, from, Topic, body);
                break;
            }
        }

    }

    string ShouldSendMail(DataPoint SAE)
    {
        if (SAE != null && SAE.Active)
        {
            if (SAE.Data == "Y")
            {
                if (checkChangeStatus(SAE))
                {
                    if (IsPreviousYes(SAE)) return "Re-Activated";
                    else return "New";
                }

                else
                {
                    DataPoints allDataPoints = SAE.Record.DataPoints;
                    foreach (DataPoint checkDP in allDataPoints)
                    {
                        if (checkDP != null && checkChangeStatus(checkDP) && checkDP.Field.OID != "H_NOW") return "Updated";
                    }
                    return string.Empty;
                }
            }
            else if (SAE.Data == "N")
            {
                if (checkChangeStatus(SAE) && IsPreviousYes(SAE)) return "Downgrade";
            }
            else return string.Empty;
        }
        return string.Empty;

    }

    void InitialParameters()
    {
        siteNumber = subj.StudySite.StudySiteNumber;
        subjName = subj.Name;
        country = subj.StudySite.Site.Country;
        environment = subj.StudySite.Study.Environment.ToUpper();
        studyName = subj.StudySite.Study.Name;
    }

    string[] GetFieldOrder(Subject subject) => (string[])CustomFunction.PerformCustomFunction("CF_Aid_AEFieldsOrder", subject.CRFVersion.ID, new object[] { null });
    bool checkChangeStatus(DataPoint dp) => dp != null && dp.Active && dp.IsObjectChanged && !dp.Interaction.TrueUser.FullName.Trim().ToUpper().StartsWith("SYSTEM");
    string GetTo(DataPoint enterDP) => (string)CustomFunction.PerformCustomFunction("CF_Aid_GetTo", enterDP.Field.CRFVersion.ID, new object[] { environment });
    string GetTopic(string type) => $"[{environment}] **{type} SAE_Alert** (Study {studyName} / Country {country} / Site {siteNumber} / Patient {subjName})";

    DataPoint GetDP(DataPoint dp, string OID) => dp.Record.DataPoints.FindByFieldOID(OID);

    string ConvertDP(DataPoint enterDP, string OID)
    {
        DataPoint manageDP = null;
        try
        {
            if (OID == string.Empty) manageDP = enterDP;
            else manageDP = enterDP?.Record.DataPoints.FindByFieldOID(OID);

            if (manageDP?.Field.ControlType == "CheckBox") return manageDP.Data == "1" ? "Yes" : "No";
            else return manageDP?.UserValue().ToString();
        }
        catch { return string.Empty; }
    }
    string GetAction(DataPoint dp)
    {
        string action = "<br>";

        foreach (string OID in new string[] { "AEACN2", "AEACN3", "AEACN1", "AEACN5", "AEACN4" })
        {
            try
            {
                if (ConvertDP(dp, OID) == "Yes") action += GetDP(dp, OID)?.Field.PreText + " <br>";
            }
            catch { }
        }
        return action == "<br>" ? "None" : action;
    }

    string GetBody(DataPoint enterDP, string type)
    {
        List<string> body = new List<string>()
{
    type,
    studyName,
    country,
    siteNumber,
    subjName,
    enterDP?.Record.RecordPosition.ToString(),
    ConvertDP(enterDP, "AETERM"),
    ConvertDP(enterDP, "AESTDAT"),
    ConvertDP(enterDP, "AEENDAT"),
    ConvertDP(enterDP, "AEOUT"),
    GetAction(enterDP),
    ConvertDP(enterDP, "AEREL"),
    ConvertDP(enterDP, "AESER"),
    ConvertDP(enterDP, "AETOXGR"),
    type=="New"?"Created":type=="Re-Activated"?"Re-Activated":"Updated",
    enterDP.Interaction.TrueUser.FullName,
    type == "Updated" ? (string)CustomFunction.PerformCustomFunction("CF_Aid_GetUpdateValue", subj.CRFVersion.ID, new object[] { enterDP,FieldOrder }) : string.Empty
};

        return string.Format(body_temp, body.ToArray());
    }
    bool IsPreviousYes(DataPoint dp)
    {
        foreach (Audit audit in Audits.Load(dp))
        {
            if ((audit.SubCategory == AuditSubCategory.Entered || audit.SubCategory == AuditSubCategory.EnteredWithChangeCode) && audit.Readable.ToUpper().IndexOf("YES (Y)") > 0) return true;
        }
        return false;
    }

    void SendEmail(string to, string from, string subject, string body)
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
}
}

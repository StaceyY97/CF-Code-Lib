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
public class CF_Symptom_Email_Alert : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*
                Created : Kieu Vo, JIRA# USTS-78712
                Date : 12 Dec 2024 
                Custom Function : CF_Symptom_Email_Alert
                Edit Check(s) : EC_Symptom_Email_Alert
                Description : Send an email to users with the Clinical Research Coordinator- No Subject Create, Restricted CRA, 
                                Medical Monitor, Medical Monitor - Restricted Data Entry, Investigator - No Subject Create roles when the condition is met
                    Condition :  Carcinoid Syndrome Symptom Diary is submitted with a conformant date in the Date and time of completion field,
                                and if Date of Visit field in the Day 1 folder has a conformant date and 
                                ( if the Date of Visit field in the Week 16 (Day 112) folder is EMPTY 
                                AND if the Date of Visit field in the OLE Day 1 folder is EMPTY), 
                                find the past two consecutive Carcinoid Syndrome Symptom Diary log lines 
                                where the Date and time of completion equals the date the custom function runs 
                                and the second log line Date and time of completion should be for the date the custom function runs -1 day
                Modification: Medidata, 02 FEB 2026, JIRA# USTS-98637
                Modification Done: Update email recipient roles
            */
            var afp = (ActionFunctionParams)ThisObject;
            return PreliminaryCheck(afp) ? Main(afp) : null;
        }

        // -- VARIABLES DECLARATION ------
        const string FIELD_OID_SUBJECT = "SUBJID";

        const string EML_FROM = "noreply@mdsol.com";
        readonly string[] EML_TO_ROLES =
        {
            "Clinical Research Coordinator- No Subject Create",
            "Investigator - No Subject Create"
        };

        enum E_ALERT_TYPE
        {
            NONE = 0, // No alert
            INITIAL
        };

        readonly Dictionary<E_ALERT_TYPE, string> EML_SUBJECT = new Dictionary<E_ALERT_TYPE, string>()
        {
            { E_ALERT_TYPE.INITIAL, "{0}CRN00808-12: Flushing Episode or Bowel Movements Email Alert" }
        };

        readonly Dictionary<E_ALERT_TYPE, string> EML_BODY = new Dictionary<E_ALERT_TYPE, string>()
        {
            {
                E_ALERT_TYPE.INITIAL,
                @"Below is a participant who in the past 2 days has reported a Flushing score of >=3, or a Bowel Movement score of >= 4:
                <ul>
              <li> Site Number: {0}</li>
              <li> Subject ID: {1}</li>
             </ul>   
               Please follow up with these participants as necessary.
                <br><br>Thank you,<br>CAREFNDR study team</p>"
            }
        };
        // -- END VARIABLES DECLARATION --    

        bool PreliminaryCheck(ActionFunctionParams afp)
        {
            return Array.IndexOf(
                new string[] { "Coder Import", "CoderImport" },
                afp.ActionDataPoint.Interaction.TrueUser.Login) < 0;
        }

        object Main(ActionFunctionParams afp)
        {
            DataPoint dpTmps = afp.ActionDataPoint;
            Subject subj = dpTmps.Record.Subject;

            bool isSendEmail = (bool)CustomFunction.PerformCustomFunction(
                "CF_Symptom_Email_Alert_CROSS",
                subj.CRFVersion.ID,
                new object[] { subj, dpTmps });

            if (!isSendEmail)
                return null;

            string emailTo = GetEmailTo(subj);
            string emailSubject = GetEmailSubject(subj, E_ALERT_TYPE.INITIAL);
            string emailBody = GetEmailBody(subj);

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
        bool IsProduction(Subject subj)
            => subj.StudySite.Study.Environment.Trim().Equals("PROD", StringComparison.OrdinalIgnoreCase);

        /* Append study environment for non-PROD with parentheses */
        string GetEnvironment(Subject subj)
            => IsProduction(subj) ? "" : string.Format("{0} ", subj.StudySite.Study.Environment.Trim().ToUpper());

        /* Returns Email subject string */
        string GetEmailSubject(Subject subj, E_ALERT_TYPE type)
        {
            if (!EML_SUBJECT.ContainsKey(type))
                return string.Empty;

            var param = new List<string>
            {
                GetEnvironment(subj)
            };

            return string.Format(EML_SUBJECT[type], param.ToArray());
        }


        /* Returns Email body string */
        string GetEmailBody(Subject subj)
        {
            DataPoint dpSUBJECT = subj.PrimaryDataPage
                .MasterRecord
                .DataPoints.FindByFieldOID(FIELD_OID_SUBJECT);

            string subjNumber = dpSUBJECT?.Data;

            if (string.IsNullOrEmpty(subjNumber))
                return string.Empty;

            string studySiteNum = subj.StudySite.StudySiteNumber.Trim();

            var param = new List<string>()
            {
                studySiteNum,
                subjNumber
            };

            var type = E_ALERT_TYPE.INITIAL;
            return string.Format(EML_BODY[type], param.ToArray());
        }

        /* Returns a deliminated string of email addresses with ';'*/
        string GetEmailTo(Subject subj)
        {
            var toAddresses = new List<string>();

            Users users = subj.StudySite.Users;

            for (int i = 0; i < users.Count; i++)
            {
                User user = users[i];
                Role role = user.UsersRoleInStudy(subj.StudySite.Study);
                if (!user.Active
                    || role == null
                    || Array.IndexOf(EML_TO_ROLES, role.RoleName) < 0)
                    continue;

                string[] singleAddresses = user.Email.Split(new char[] { ';', ',', ' ' },
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (var singleAddress in singleAddresses)
                {
                    if (!toAddresses.Contains(singleAddress))
                        toAddresses.Add(singleAddress);
                }
            }

            return string.Join(",", toAddresses);
}
}
}

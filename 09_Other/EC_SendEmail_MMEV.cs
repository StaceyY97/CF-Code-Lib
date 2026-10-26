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
public class EC_SendEmail_MMEV : CustomFunctionBase
{
public object Eval(object ThisObject)
{
var afp = (ActionFunctionParams)ThisObject;
            return PreliminaryCheck(afp) ? Main(afp) : null;
        }

        // -- VARIABLES DECLARATION ------
        const string FIELD_OID_SUBJECT = "SUBJID";

        const string EML_FROM = "MM_Alert@mdsol.com";
        readonly string[] EML_TO_ROLES =
        {
        "Clinical Research Coordinator- No Subject Create",
       "Investigator - No Subject Create",
        "Restricted CRA"
    }
        ;
        string[] checkDP_1 = new string[] { "MMEV", "MMEVAGR", "1", "1" };
        string[] checkDP_2 = new string[] { "MMEV", "MMEVNAGR", "0", "1" };
        string[] bodyDP = new string[] { "VIS", "VISDAT" };
        string[] bodyDP_2 = new string[] { "MMEV", "MMEVDAT" };
        string[] bodyDP_3 = new string[] { "IE2", "IE_PTS" };
        string[] bodyDP_4 = new string[] { "IE2", "IE_BMNUM" };


        enum E_ALERT_TYPE
        {
            NONE = 0, // No alert
            INITIAL
        };

        readonly Dictionary<E_ALERT_TYPE, string> EML_SUBJECT = new Dictionary<E_ALERT_TYPE, string>()
{
    {
        E_ALERT_TYPE.INITIAL, "{0}CRN00808-12 Medical Monitor supports Investigator's Assessment that subject meets eligibility criteria"
    }
}
        ;

        readonly Dictionary<E_ALERT_TYPE, string> EML_BODY = new Dictionary<E_ALERT_TYPE, string>()
{
    {
        E_ALERT_TYPE.INITIAL,
        @"The Medical Monitor supports the Investigator's Assessment that the participant below meets eligibility criteria:
        <ul>
        <li> Site Number: {0}</li>
        <li> Subject ID: {1}</li>
        <li> Visit: {2}</li>
        <li> Date of Visit: {3}</li>
        <li> Date of Verification: {4}</li>
        <li> Prior Treatment Status: {5}</li>
        <li> Number of BMs: {6}</li>

        </ul><br>
        Please proceed to schedule the randomization visit.
        Please complete Stratification factors in IRT at the time of randomization: Prior Treatment Status and Number of BMs.
        <br><br>Thank you"
    }
}
        ;
        // -- END VARIABLES DECLARATION --

        bool PreliminaryCheck(ActionFunctionParams afp)
        {
            return Array.IndexOf(new string[] { "Coder Import", "CoderImport" }, afp.ActionDataPoint.Interaction.TrueUser.Login) < 0;
        }

        object Main(ActionFunctionParams afp)
        {
            DataPoint dpTmps = afp.ActionDataPoint;
            Subject subj = dpTmps.Record.Subject;

            bool isSendEmail = IsSendEmail(dpTmps);
            if (!isSendEmail) return null;

            string emailTo = GetEmailTo(subj);
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

            string placeHolder_1 = dpSUBJECT?.Data;

            if (string.IsNullOrEmpty(placeHolder_1))
                return string.Empty;

            string placeHolder_0 = subj.StudySite.StudySiteNumber.Trim();
            string placeHolder_2 = dpTmps.Record.Instance?.Name;
            string placeHolder_3 = GetDataPoint_SameFodler(dpTmps, bodyDP[0], bodyDP[1])?.Data;
            string placeHolder_4 = GetDataPoint_SameFodler(dpTmps, bodyDP_2[0], bodyDP_2[1])?.Data;
            string placeHolder_5 = GetDataPoint_SameFodler(dpTmps, bodyDP_3[0], bodyDP_3[1])?.Data;
            string placeHolder_6 = GetDataPoint_SameFodler(dpTmps, bodyDP_4[0], bodyDP_4[1])?.Data;

            var param = new List<string>()
    {
        placeHolder_0,
        placeHolder_1,
        placeHolder_2,
        placeHolder_3,
        placeHolder_4,
        placeHolder_5,
        placeHolder_6
    };

            var type = E_ALERT_TYPE.INITIAL;
            return string.Format(EML_BODY[type], param.ToArray());
        }

        /* Returns a deliminated string of email addresses with ';'*/
        string GetEmailTo(Subject subj)
        {
            var toAddresses = new List<string>() { "qian.yang@edetek.com" };

            Users users = subj.StudySite.Users;

            for (int i = 0; i < users.Count; i++)
            {
                User user = users[i];
                Role role = user.UsersRoleInStudy(subj.StudySite.Study);
                if (!user.Active || role == null || Array.IndexOf(EML_TO_ROLES, role.RoleName) < 0) continue;

                string[] singleAddresses = user.Email.Split(new char[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var singleAddress in singleAddresses)
                {
                    if (!toAddresses.Contains(singleAddress)) toAddresses.Add(singleAddress);
                }
            }

            return string.Join(",", toAddresses);
        }


        DataPoint GetDataPoint_SameFodler(DataPoint dpTmps, string formOID, string fieldOID) => dpTmps.Record.Instance?.DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints?.FindByFieldOID(fieldOID);

        bool DoCheck(DataPoint Dp, string[] checks) => checks[(checks.Length) - 1] == "1" ? Dp.Data == checks[2] : Dp.Data != checks[2];
        bool IsSendEmail(DataPoint dpTmps)
        {
            DataPoint Dp_1 = GetDataPoint_SameFodler(dpTmps, checkDP_1[0], checkDP_1[1]);
            DataPoint Dp_2 = GetDataPoint_SameFodler(dpTmps, checkDP_2[0], checkDP_2[1]);
            if (Dp_1 == null || !Dp_1.Active || Dp_2 == null || !Dp_2.Active || (!Dp_1.IsObjectChanged && !Dp_2.IsObjectChanged)) return false;
            if (DoCheck(Dp_1, checkDP_1)) return DoCheck(Dp_2, checkDP_2);
            else return false;
}
}
}

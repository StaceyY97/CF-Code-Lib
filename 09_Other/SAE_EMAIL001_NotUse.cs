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
public class SAE_EMAIL001_NotUse : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*Created by: TianciWen/23Dec2024
CF: SAE_EMAIL001
EC: SAE_EMAIL001
Log: Added Deleted, Reactivated and Downgrade emails
    Reference function CF_Aid_GetTo
    */

    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint AESER = afp.ActionDataPoint;
    Subject sub = AESER.Record.Subject;

    if (AESER.Interaction.TrueUser != null && AESER.Interaction.TrueUser.Login != null)
        if (string.Compare(AESER.Interaction.TrueUser.Login.Trim(), "CoderImport", true) == 0)
            return null;

    bool NewEmail = true, sendEmail = false;

    string Email = (string)CustomFunction.PerformCustomFunction("CF_Aid_GetTo", AESER.Field.CRFVersion.ID, new object[] { sub.StudySite.Study.Environment.ToUpper() });

    string[] fieldlist =
    {
    "AETERM", "AESTDAT", "AEENDAT", "AEREL", "AEOUT", "AETOXGR" , "AEACN1", "AEACN2", "AEACN3", "AEACN4", "AEACN5"
}
    ;
    bool flag = false;
    string NewLine = Environment.NewLine;
    for (int i = 0; i < fieldlist.Length; i++)
    {
        DataPoint dp = AESER.Record.DataPoints.FindByFieldOID(fieldlist[i]);
        if (dp.IsObjectChanged)
        {
            flag = true;
            break;
        }
    }

    if (AESER != null)
    {
        string status = string.Empty;
        if (AESER.IsObjectChanged)
        {
            if (AESER.Data == "Y")
            {
                sendEmail = true;
                status = "New";
            }
            else if (AESER.Data == "N" && IsPreviousYes(AESER))
            {
                sendEmail = true;
                status = "Downgrade";
                NewEmail = false;
            }
        }
        else if (AESER.Data == "Y" && !AESER.IsObjectChanged && flag == true)
        {
            sendEmail = true;
            status = "Updated";
        }
        string UpdateTo = status;
        if (status == "Updated")
        {
            UpdateTo = NewLine + NewLine + "Updated Items:" + NewLine;
            string[] fieldlist2 =
            {

            "AETERM", "AESTDAT", "AEENDAT", "AEREL", "AEOUT", "AETOXGR" , "AEACN1", "AEACN2", "AEACN3", "AEACN4", "AEACN5"

        }
            ;
            for (int j = 0; j < fieldlist2.Length; j++)
            {
                DataPoint dp = AESER.Record.DataPoints.FindByFieldOID(fieldlist2[j]);
                if (dp.IsObjectChanged)
                {
                    string str = AESER.Record.DataPoints.FindByFieldOID(fieldlist2[j]).Field.PreText;
                    if (dp.Field.OID.EndsWith("DAT"))
                    {
                        while (true)
                        {
                            int i1 = str.IndexOf("(");
                            if (i1 == -1)
                                break;
                            int i2 = str.IndexOf(")");
                            if (i2 == -1)
                                break;
                            str = str.Remove(i1, i2 - i1 + 1);
                        }
                    }

                    UpdateTo = UpdateTo + str + ", ";
                }
            }

            UpdateTo = UpdateTo.Substring(0, UpdateTo.Length - 2);
            NewEmail = false;
        }
        if (!AESER.Active && IsPreviousYes(AESER))
        {
            sendEmail = true;
            status = "Deleted";
            NewEmail = false;
        }
        else if (AESER.Active && IsPrevAuditInActivated(AESER) && IsPreviousYes(AESER))
        {
            sendEmail = true;
            status = "Reactivated";
            NewEmail = false;
        }
        string strCountry = AESER.Record.Subject.StudySite.Site.Country;
        string strSiteNum = AESER.Record.Subject.StudySite.Site.Number;
        string strSUBJID = AESER.Record.Subject.PrimaryName.ToString();
        StringBuilder strSubject = new StringBuilder("");
        StringBuilder strMessage = new StringBuilder("");


        if (NewEmail == true)
        {
            strSubject.Append("** SAE Alert - New **");
            strSubject.Append("(Study " + "OP-3136-101" + " / Site " + strSiteNum + " / Patient " + strSUBJID + ")");
        }
        else
        {
            strSubject.Append("** SAE Alert - Update **");
            strSubject.Append("(Study " + "OP-3136-101" + " / Site " + strSiteNum + " / Patient " + strSUBJID + ")");
        }

        if (sendEmail)
        {
            strMessage.Append("Status of case: " + status);

            strMessage.Append(NewLine + NewLine + "Study: " + "OP-3136-101");
            strMessage.Append(NewLine + "Country: " + strCountry);
            strMessage.Append(NewLine + "Site number: " + strSiteNum);
            strMessage.Append(NewLine + "Subject number: " + strSUBJID);
            strMessage.Append(NewLine + "AE Log Line Number: " + AESER.Record.RecordPosition.ToString());
            strMessage.Append(NewLine + "AE Term: " + AESER.Record.DataPoints.FindByFieldOID("AETERM").Data.ToString());
            strMessage.Append(NewLine + "AE Start Date: " + AESER.Record.DataPoints.FindByFieldOID("AESTDAT").Data.ToString());
            strMessage.Append(NewLine + "AE End Date: " + AESER.Record.DataPoints.FindByFieldOID("AEENDAT").Data.ToString());
            strMessage.Append(NewLine + "Outcome: " + AESER.Record.DataPoints.FindByFieldOID("AEOUT").Data.ToString());
            strMessage.Append(NewLine + "Relationship to Study Drug: " + AESER.Record.DataPoints.FindByFieldOID("AEREL").Data.ToString());
            strMessage.Append(NewLine + "Action Taken with Study Drug: " + ReturnStudyDrug(AESER));
            strMessage.Append(NewLine + "Serious?: " + AESER.Data.ToString());
            strMessage.Append(NewLine + "CTCAE Grade: " + AESER.Record.DataPoints.FindByFieldOID("AETOXGR").Data.ToString() + NewLine);

            strMessage.Append(NewLine + "SAE Created/Updated By: " + AESER.Interaction.TrueUser.FirstName + " " + AESER.Interaction.TrueUser.LastName + " [" + AESER.Interaction.TrueUser.Login + "]" + NewLine);
            strMessage.Append(UpdateTo);
            strMessage.Append(NewLine + NewLine + NewLine + "*** This message was generated by an automated system, please DO NOT reply to this message. ***");
            Message.SendEmail(Email, "SAE_Alert@mdsol.com", strSubject.ToString(), strMessage.ToString());
        }
    }

    return null;
}
bool IsPreviousYes(DataPoint dp)
{
    Audits auditValues = Audits.Load(dp);
    for (int i = 0; i < auditValues.Count; i++)
    {
        Audit obj_AuditValue;
        obj_AuditValue = auditValues[i];
        if (obj_AuditValue.SubCategory == AuditSubCategory.Entered || obj_AuditValue.SubCategory == AuditSubCategory.EnteredWithChangeCode)
        {
            if ((obj_AuditValue.Readable.ToUpper().IndexOf("YES (Y)") > 0))
            {
                return true;
            }
        }
    }
    return false;
}
bool IsPrevAuditInActivated(DataPoint dpAction)
{
    Audits ads = Audits.Load(dpAction);
    for (int j = 0; j < ads.Count; j++)
    {
        Audit ad = ads[j];
        if (ad.SubCategory == AuditSubCategory.Inactivated || ad.SubCategory == AuditSubCategory.InactivatedWithReasonCode)
        {
            return true;
        }
        break;
    }
    return false;
}
string ReturnStudyDrug(DataPoint dpAction)
{
    StringBuilder sb = new StringBuilder();
    string[] fieldOIDs = { "AEACN1", "AEACN2", "AEACN3", "AEACN4", "AEACN5" };
    bool first = true;

    foreach (string oid in fieldOIDs)
    {
        DataPoint dp = dpAction.Record.DataPoints.FindByFieldOID(oid);
        if (dp != null && dp.Data == "1")
        {
            if (!first)
            {
                sb.Append(",");
            }
            sb.Append(dp.Field.PreText);
            first = false;
        }
    }
    return sb.ToString();
}
}
}

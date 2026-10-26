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
public class EC_SendEmail_AESAE : CustomFunctionBase
{
public object Eval(object ThisObject)
{
//V0.0.6
    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpAction = afp.ActionDataPoint;
    Subject subj = dpAction.Record.Subject;

    Dictionary<string, List<string>> S = (Dictionary<string, List<string>>)CustomFunction.PerformCustomFunction("CF_SAE_FieldLevel", dpAction.Field.CRFVersion.ID, new object[] { dpAction });

    string siteNumber = subj.StudySite.StudySiteNumber;
    string subjName = subj.Name;
    string country = subj.StudySite.Site.Country;

    const string from = "SAE_Alert@mdsol.com";
    string to = (string)CustomFunction.PerformCustomFunction("CF_Email_Recipients", dpAction.Field.CRFVersion.ID, new object[] { subj.StudySite.Study.Environment, "PART" });
    string topic = "[" + subj.StudySite.Study.Environment.ToUpper() + "] " + "**SAE_Alert** (Study " + subj.StudySite.Study.Name + " / Country " + country.ToString() + " / Site " + siteNumber + " / Patient " + subjName + ")";

    StringBuilder body = new StringBuilder("");

    if (dpAction.Field.OID == "AESER")
    {
        if (dpAction.Data == "Y")
        {
            if (dpAction.IsObjectChanged)
            {
                to = (string)CustomFunction.PerformCustomFunction("CF_Email_Recipients", dpAction.Field.CRFVersion.ID, new object[] { subj.StudySite.Study.Environment, "All" });
                SForNewSAE(dpAction, subj, to, from, topic, body);
            }
            else
            {
                if (GetFirstChangeField(dpAction, S) != null)
                {
                    DataPoint firstChange = GetFirstChangeField(dpAction, S);
                    if (S[dpAction.Field.OID].Contains(firstChange.Field.OID)) SForUpdatedSAE(dpAction, subj, to, from, topic, body);
                }
            }
        }
        else if (dpAction.Data == "N" && dpAction.IsObjectChanged&&IsPreviousYes(dpAction)) SForUpdatedSAE(dpAction, subj, to, from, topic, body);
    }
    else//Other field
    {
        //if (subj.Name == "IT888-Y07") Message.SendEmail("qian.yang@edetek.com", "test_qy@mdsol.com", "test_qy", dpAction.Field.OID + " other entered");
        DataPoint DP_SAR = dpAction.Record.DataPoints.FindByFieldOID("AESER");
        if (DP_SAR != null && DP_SAR.Data == "Y")
        {
            //if (subj.Name == "IT888-Y07" ) Message.SendEmail("qian.yang@edetek.com", "test_qy@mdsol.com", "test_qy", dpAction.Field.OID + "_" + dpAction.Data + " check 1");
            DataPoint firstChange = GetFirstChangeField(dpAction, S);
            //if (subj.Name == "IT888-Y07" && firstChange != null) Message.SendEmail("qian.yang@edetek.com", "test_qy@mdsol.com", "test_qy", dpAction.Field.OID + "_" + dpAction.Data + " check 2 "+ firstChange.Field.OID);
            //else
            {
                //if (subj.Name == "IT888-Y07") Message.SendEmail("qian.yang@edetek.com", "test_qy@mdsol.com", "test_qy", dpAction.Field.OID + "_" + dpAction.Data + " check 3");
            }
            if (firstChange != null && S[dpAction.Field.OID].Contains(firstChange.Field.OID)) SForUpdatedSAE(dpAction, subj, to, from, topic, body);
        }
        //else{ if (subj.Name == "IT888-Y07"&&DP_SAR !=null) Message.SendEmail("qian.yang@edetek.com", "test_qy@mdsol.com", "test_qy", dpAction.Field.OID + "_" + dpAction.Data + " check 4 "+ DP_SAR .Data);}
    }
    return null;
}

DataPoint GetFirstChangeField(DataPoint inputDP, Dictionary<string, List<string>> inputDic)
{
    DataPoints allDataPoints = inputDP.Record.DataPoints;
    foreach (string key in inputDic.Keys)
    {
        foreach (string dpOID in inputDic[key])
        {
            DataPoint dp = allDataPoints.FindByFieldOID(dpOID);
            if (dp!=null && dp.IsObjectChanged)
            {
                if ((inputDP.Field.OID == "AECONTRT" && dp.Field.OID == "AEACNOTHYN") || (inputDP.Field.OID == "AESERCR_DEATH" && (dp.Field.OID == "AEOUT" || dp.Field.OID == "AESER"))) continue;
                return dp;
            }
        }
    }
    return null;
}
void SForNewSAE(DataPoint dp, Subject subj, string to, string from, string topic, StringBuilder body)
{
if (dp.Interaction.TrueUser.FullName.Trim().ToUpper() != "System User".Trim().ToUpper()){
    body = BodyCreat(dp, body, subj);
    body.Append(Environment.NewLine + "SAE Created By: " + dp.Interaction.TrueUser.FullName);
    body.Append(Environment.NewLine + Environment.NewLine + "*** This message was generated by an automated system, please DO NOT reply to this message. ***" + Environment.NewLine + Environment.NewLine + "Note: This does not require a query to fire to the site  - only an email to the address with the study name, subject country, site number, subject number, AE log line number, AE term, AE start date, name of individual who created/updated SAE information.");
    Message.SendEmail(to, from, topic, body.ToString());
}
}

void SForUpdatedSAE(DataPoint dp, Subject subj, string to, string from, string topic, StringBuilder body)
{
if (dp.Interaction.TrueUser.FullName.Trim().ToUpper() != "System User".Trim().ToUpper()){
    body = BodyCreat(dp, body, subj);
    body.Append(Environment.NewLine + "SAE Updated By: " + dp.Interaction.TrueUser.FullName);
    body.Append(Environment.NewLine + "Field(s) / Field Query(s) Updated: \n\t" + GetUpdateString(dp));
    body.Append(Environment.NewLine + Environment.NewLine + "*** This message was generated by an automated system, please DO NOT reply to this message. ***" + Environment.NewLine + Environment.NewLine + "Note: This does not require a query to fire to the site  - only an email to the address with the study name, subject country, site number, subject number, AE log line number, AE term, AE start date, name of individual who created/updated SAE information.");
    Message.SendEmail(to, from, topic, body.ToString());
}
}

StringBuilder BodyCreat(DataPoint dp, StringBuilder body, Subject subj)
{
    body.Append("Study: " + subj.StudySite.Study.Name);
    body.Append(Environment.NewLine + "Country: " + subj.StudySite.Site.Country.ToString());
    body.Append(Environment.NewLine + "Site number: " + subj.StudySite.StudySiteNumber);
    body.Append(Environment.NewLine + "Subject number: " + dp.Record.Subject.Name);
 body.Append(Environment.NewLine + "Form Name: " + dp.Record.DataPage.Name);
    body.Append(Environment.NewLine + "AE Log Line Number: " + dp.Record.RecordPosition);
    body.Append(Environment.NewLine + "AE Term: " + dp.Record.DataPoints.FindByFieldOID("AETERM").UserValue());
    body.Append(Environment.NewLine + "AE Start Date: " + dp.Record.DataPoints.FindByFieldOID("AESTDAT").UserValue());
    body.Append(Environment.NewLine + "Is SAE: " + dp.Record.DataPoints.FindByFieldOID("AESER").UserValue());
    body.Append(Environment.NewLine + "AE Severity: " + dp.Record.DataPoints.FindByFieldOID("AESEV").UserValue());
    return body;
}

string GetUpdateString(DataPoint dpAction)
{
    string updateStr = "";
    DataPoints allDataPoints = dpAction.Record.DataPoints;
    foreach (DataPoint dp in allDataPoints)
    {
        if (dp != null && dp.IsObjectChanged && dp.Record.RecordPosition != 0 && dp.Field.OID != "H_NOW" && dp.Field.OID != "AEYN") updateStr = updateStr + dp.Field.PreText + ": " + dp.UserValue() + "\n\t";
    }
    return updateStr;
}

bool IsPreviousYes(DataPoint dp)
    {
        Audits auditValues = Audits.Load(dp);
        for (int i = 0; i < auditValues.Count; i++)
        {
            Audit obj_AuditValue = auditValues[i];
            if (obj_AuditValue.SubCategory == AuditSubCategory.Entered || obj_AuditValue.SubCategory == AuditSubCategory.EnteredWithChangeCode)
            {
                if ((obj_AuditValue.Readable.ToUpper().IndexOf("Y") > 0))return true;
else return false;
            }
        }
        return false;
}
}
}

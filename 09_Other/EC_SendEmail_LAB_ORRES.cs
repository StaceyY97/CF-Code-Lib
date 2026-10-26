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
public class EC_SendEmail_LAB_ORRES : CustomFunctionBase
{
public object Eval(object ThisObject)
{
ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpAction = afp.ActionDataPoint;
            DataPage currentPage = dpAction.Record.DataPage;

            List<string> checkLab_prior = new List<string>()
{
    "HBA1C2ORRES", "EGFR", "ALANINE AMINOTRANSFERASE ALT", "ASPARTATE AMINOTRANSFERASE AST", "TOTAL BILIRUBIN"
}
            ;
            List<string> checkLab_after = new List<string>()
{
    "AMYLASE", "LIPASE", "TOTAL BILIRUBIN"
}
            ;
            if (!checkLab_prior.Contains(dpAction.Data) && !checkLab_after.Contains(dpAction.Data) && !checkLab_prior.Contains(dpAction.Field.OID)) return null;
            Subject subj = dpAction.Record.Subject;

            string siteNumber = subj.StudySite.Site.Number;
            string subjName = subj.Name;
            string country = subj.StudySite.Site.Country;

            const string from = "Lab_Alert@mdsol.com";
            string to = (string)CustomFunction.PerformCustomFunction("CF_Email_Recipients", dpAction.Field.CRFVersion.ID, new object[] { subj.StudySite.Study.Environment, "PART" });
            string topic = "[" + subj.StudySite.Study.Environment.ToUpper() + "] " + "**Safety Lab Alert** (Study " + subj.StudySite.Study.Name + " / Country " + country.ToString() + " / Site " + siteNumber + " / Patient " + subjName + ")";
            StringBuilder body = new StringBuilder("");

            DataPoint dp_rand = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subj, "", "D1", "ECOA", "ECOADER2" });
            DataPoint LBDATE = dpAction.Record.DataPoints.FindByFieldOID(currentPage.Form.OID + "DAT");
            if (checkDate(dp_rand, LBDATE))
            {
                if (checkLab_after.Contains(dpAction.Data))
                {
                    DataPoint ORRES = dpAction.Record.DataPoints.FindByFieldOID(currentPage.Form.OID + "ORRES");
                    DataPoint ULN = dpAction.Record.DataPoints.FindByFieldOID(currentPage.Form.OID + "NRHI");
                    bool doSend = ORRES.IsObjectChanged || ULN.IsObjectChanged|| LBDATE.IsObjectChanged;
                    if (doSend)
                    {
                        switch (dpAction.Data)
                        {
                            case "AMYLASE":
                                doSend = checkORRES(ORRES, ULN, 3);
                                break;
                            case "LIPASE":
                                doSend = checkORRES(ORRES, ULN, 3);
                                break;
                            case "TOTAL BILIRUBIN":
                                doSend = checkORRES(ORRES, ULN, 2);
                                break;
                        }
                    }
                    if (doSend)
                    {
                        GetBody(dpAction, body, subj);
                        Message.SendEmail(to, from, topic, body.ToString());
                    }
                }
            }
            else
            {
                if (checkLab_prior.Contains(dpAction.Data) || checkLab_prior.Contains(dpAction.Field.OID))
                {
                    DataPoint ORRES = dpAction.Record.DataPoints.FindByFieldOID(currentPage.Form.OID + "ORRES");
                    DataPoint ULN = dpAction.Record.DataPoints.FindByFieldOID(currentPage.Form.OID + "NRHI");
                    bool doSend = ORRES.IsObjectChanged || (ULN != null && ULN.IsObjectChanged)|| LBDATE.IsObjectChanged;
                    if (doSend)
                    {
                        switch (dpAction.Data)
                        {
                            case "EGFR":
                                DataPoint UNIT = dpAction.Record.DataPoints.FindByFieldOID(currentPage.Form.OID + "RESU");
                                if (string.Compare(UNIT.Data, "mL/min/1.73 m2", true) == 0) doSend = ORRES.Data != string.Empty && ValueTrans(ORRES.Data) < 30;
else doSend = false;                                
break;
                            case "ALANINE AMINOTRANSFERASE ALT":
                                doSend = checkORRES(ORRES, ULN, 2);
                                break;
                            case "ASPARTATE AMINOTRANSFERASE AST":
                                doSend = checkORRES(ORRES, ULN, 2);
                                break;
                            case "TOTAL BILIRUBIN":
                                doSend = checkORRES(ORRES, ULN, 1.5);
                                break;
                            default:
                                if (dpAction.Field.OID == "HBA1C2ORRES") doSend = ValueTrans(dpAction.Data) >= 8.5;
                                break;
                        }
                    }                    
                    if (doSend)
                    {
                        GetBody(dpAction, body, subj);
                        Message.SendEmail(to, from, topic, body.ToString());
                    }
                }
            }
            return null;
        }

        bool checkDate(DataPoint rand, DataPoint LBDAT)
        {
            if (rand != null && LBDAT != null && rand.StandardValue() is DateTime && LBDAT.StandardValue() is DateTime)
            {
                return (DateTime)rand.StandardValue() <= (DateTime)LBDAT.StandardValue();
            }
            return false;
        }

        bool checkORRES(DataPoint ORRES, DataPoint ULN, double mult)
        {
            double orres = ValueTrans(ORRES.Data);
            double uln = ValueTrans(ULN.Data);
            if (orres != 0 && uln != 0) return orres > mult * uln;
            return false;
        }

        double ValueTrans(string inputString)
        {
            double valueOut = 0;
            if (string.IsNullOrEmpty(inputString)) return valueOut;
            if (Number.IsValidNumber(inputString))
            {
                valueOut = double.Parse(inputString);
            }
            return valueOut;
        }
        void GetBody(DataPoint dp, StringBuilder body, Subject subj)
        {
            body.Append(Environment.NewLine + Environment.NewLine + "Safety Lab is reported as Out of Range" + Environment.NewLine + "Study: CRN00808-12");
            body.Append(Environment.NewLine + "Country: " + dp.Record.Subject.StudySite.Site.Country.ToString());
            body.Append(Environment.NewLine + "Site number: " + dp.Record.Subject.StudySite.Site.Number);
            body.Append(Environment.NewLine + "Subject number: " + dp.Record.Subject.Name);
            body.Append(Environment.NewLine + "Visit Name: " + dp.Record.Instance.Name);
            body.Append(Environment.NewLine + "Form Name: " + dp.Record.DataPage.Name);
            if (dp.Field.OID != "HBA1C2ORRES") body.Append(Environment.NewLine + "Test Name: " + dp.UserValue());
            else body.Append(Environment.NewLine + "Test Name: " + dp.Record.DataPage.Name);
            body.Append(Environment.NewLine + Environment.NewLine + "*** This message was generated by an automated system, please DO NOT reply to this message. ***");
}
}
}

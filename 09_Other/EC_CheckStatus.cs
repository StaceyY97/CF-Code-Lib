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
public class EC_CheckStatus : CustomFunctionBase
{
public object Eval(object ThisObject)
{
//1: screening RS.RSYN = Y
    //2: SF SCR?SCR.IEs atLeastOne = Y:RESCR?RESCR.IEs atLeastOne = Y
    //4: ET SCR?D1.ENR.ELDAT !="&&"CONTYN=="N":RESCR? D1_2.ENR.ELDAT!=""&&"CONTYN=="N" / W2 to W8(OLED1 to W84) "CONTYN=="N" / W12 "CONTYN=="N"
    ///W16(W104).VIS2YN = "N"
    //5: completed EOS.EOSYN = Y and EOS.EOSDAT !=""
    //6: Randomized SCR?D1.ENR.ELDAT !="":RESCR? D1_2.ENR.ELDAT!=""
    //7: FU  ET/ OLEET/ OLEWK104 VISDAT !="" / W16.VIS2ROYN = "N"
    //8: Discontinued EOS.EOSYN = N and EOS.EOSDAT !=""
    //11: OLE OLED1.VISDAT!=""


    Dictionary<string, int> statusDic = new Dictionary<string, int>
{
    { "SCR", 1 },
    { "SF", 2 },
    { "ET", 4 },
    { "COMP", 5 },
    { "ENR", 6 },
    { "FU", 7 },
    { "DIS", 8 },
    { "OLE", 11 }
};

    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpAction = afp.ActionDataPoint;
    string fieldOID = dpAction.Field.OID;
    string realStatus = "SCR";
    if (checkSubjectStatus(dpAction, "SF")) realStatus =  "SF";
    int currentIndex = statusSequence.IndexOf(realStatus);
    for (int i = currentIndex + 1; i < statusSequence.Count; i++)
    {
        string nextStatus = statusSequence[i];
//if (dpAction.Record.Subject.Name == "QY007") Message.SendEmail("qian.yang@edetek.com", "test_qy@mdsol.com", "test_qy",i.ToString() + "_" + nextStatus);
        if (checkSubjectStatus(dpAction, nextStatus))
        {
            realStatus = ConvertStatus(nextStatus);
            if (nextStatus == "DIS") break;
        }
        else if (nextStatus == "RCPET")
        {
            for (int j = i + 1; j < statusSequence.Count; j++)
            {
                string nextStatus_sec = statusSequence[j];
                if (checkSubjectStatus(dpAction, nextStatus_sec))
                {
                    realStatus = ConvertStatus(nextStatus_sec);
                    if (nextStatus_sec == "DIS") break;
                }
                else if (nextStatus_sec == "OLE"|| nextStatus_sec == "OLEET") 
                {
                    CheckFromFU(dpAction, realStatus);
                }
                else { break; }
            }
        }
        else if (nextStatus == "OLE" || nextStatus == "OLEET") CheckFromFU(dpAction, realStatus);
        else
        {
            if (checkSubjectStatus(dpAction, "DIS")) { realStatus = "DIS"; }
break;
        }
    }

    if (statusDic.ContainsKey(realStatus)) dpAction.Record.Subject.SubjectStatus = statusDic[realStatus];

    return null;
}

List<string> statusSequence = new List<string>() {  "SF","SCR","ENR", "RCPET", "OLE", "OLEET", "FU", "COMP", "DIS" };


bool checkDPY(DataPoint dp) => dp != null && dp.Active && dp.Data == "Y";
bool checkDPN(DataPoint dp) => dp != null && dp.Active && dp.Data == "N";
bool checkDTNotEmpty(DataPoint dp) => dp != null && dp.Active && dp.StandardValue() is DateTime && dp.Data != string.Empty;
string ConvertStatus(string checkStatus) => checkStatus == "RCPET" || checkStatus == "OLEET" ? "ET" : checkStatus;
bool checkSubjectStatus(DataPoint dpAction, string status)
{
    bool isMeet = false;
    Subject subj = dpAction.Record.Subject;
    switch (status)
    {
        case "SF":
            //Check SF
            isMeet = (bool)CustomFunction.PerformCustomFunction("CF_CheckStatus_SF", dpAction.Field.CRFVersion.ID, new object[] { dpAction,"SF" });
            break;
        case "SCR":
            //Check RS
            isMeet = (bool)CustomFunction.PerformCustomFunction("CF_CheckStatus_SF", dpAction.Field.CRFVersion.ID, new object[] { dpAction,"SCR" });
            break;
        case "ENR":
            DataPoints CheckDPs_ENR = CustomFunction.FetchAllDataPointsForOIDPath("ELDAT", "ENR", null, subj);
            foreach (DataPoint dp in CheckDPs_ENR) if (checkDTNotEmpty(dp)) { isMeet = true; break; }
            break;
        case "RCPET":
            //Check RCP ET
            isMeet = (bool)CustomFunction.PerformCustomFunction("CF_CheckStatus_ET", dpAction.Field.CRFVersion.ID, new object[] { dpAction, "RCPET" });
            break;
        case "OLE":
            //Check OLE
            DataPoint VISDAT_OLE = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subj, "", "OLED1", "VIS", "VISDAT" });
            isMeet = checkDTNotEmpty(VISDAT_OLE);
            break;
        case "OLEET":
            //Check OLE ET
            isMeet = (bool)CustomFunction.PerformCustomFunction("CF_CheckStatus_ET", dpAction.Field.CRFVersion.ID, new object[] { dpAction, "OLEET" });
            break;
        case "FU":
            //Check FU
            DataPoints CheckDPs_FU = CustomFunction.FetchAllDataPointsForOIDPath("VISDAT", "VIS", null, subj);
            DataPoint VIS2ROYN = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subj, "", "WK16", "VIS2", "VIS2ROYN" });
            CheckDPs_FU.Add(VIS2ROYN);
            bool IsDataPointFU(DataPoint dp) => dp != null && dp.Active && (bool)CustomFunction.PerformCustomFunction("CF_CheckStatus_FU", dp.Field.CRFVersion.ID, new object[] { dp });
            foreach (DataPoint dp in CheckDPs_FU) if (IsDataPointFU(dp)) { isMeet = true; break; }
            break;
        case "COMP":
            //Check Completed and Discontinue
            DataPoint EOSYN_COM = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subj, "", "EOS", "EOS", "EOSYN" });
            DataPoint EOSDAT_COM = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subj, "", "EOS", "EOS", "EOSDAT" });
            isMeet = checkDPY(EOSYN_COM) && checkDTNotEmpty(EOSDAT_COM);
            break;
        case "DIS":
            //Check Completed and Discontinue
            DataPoint EOSYN_DIS = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subj, "", "EOS", "EOS", "EOSYN" });
            DataPoint EOSDAT_DIS = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { subj, "", "EOS", "EOS", "EOSDAT" });
            isMeet = checkDPN(EOSYN_DIS) && checkDTNotEmpty(EOSDAT_DIS);
            break;
    }
    return isMeet;

}

void CheckFromFU(DataPoint dpAction,string realStatus)
{
    for (int k = statusSequence.IndexOf("FU"); k < statusSequence.Count; k++)
    {
        string nextStatus = statusSequence[k];
        if (checkSubjectStatus(dpAction, nextStatus))
        {
            realStatus = ConvertStatus(nextStatus);
            if (nextStatus == "DIS") break;
        }
        else { break; }
    }
}
}
}

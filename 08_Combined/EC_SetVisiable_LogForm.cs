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
public class EC_SetVisiable_LogForm : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
     * Created By :Qian Yang/17Apr2025
     * Custom Function :EC_SetVisiable_LogForm
     * Description: Set log lines visible or invisible in condition, please change and field mentioned in dictionary form_Match should be the first level of form dynamic and raise query if invisible field entered
     * Edit Checks :SetVisable_Log
     *********************************************************************************************************/
                   Dictionary<string, List<string>> form_Match = new Dictionary<string, List<string>>()
{
    { "CHEM1",new List<string>{"CHEM1OUT","Y","CHEM1TESTCD","CHEM1CLSIG"} },
    { "COAG1",new List<string>{"COAG1OUT","Y","COAG1TEST","COAG1CLSIG" } },
    { "ECG",new List<string>{"EGSTAT","Y", "EGND" } },
    { "HEMA1",new List<string>{"HEMA1OUT","Y", "HEMA1TESTCD", "HEMA1CLSIG" } },
    { "LPD1",new List<string>{"LPOUT","Y", "LBTESTCD", "LBCLSIG1" } },
    { "MH",new List<string>{"MHYN","Y","MHTERM","MHSTDAT","MHONGO"} },
    { "CM",new List<string> {"CMYN","Y","CMTRT","CMSTDAT","CMONGO","CMDU_AE","CMDU_MH","CMDU_CS","CMDU_PROPH","CMRM","CMDU","CMDOSE","CMDOSU","CMROUTE","CMDOSFRQ","CMDU_AT"} },
    { "CM2",new List<string> {"CM2YN","Y","CM2PID","CM2TRT","CM2STDAT","CM2ONGO","CM2DU_AE","CM2DU_MH","CM2DU_CS","CM2DU_PROPH","CM2RM","CM2DU","CM2DOSE","CM2DOSU","CM2ROUTE","CM2DOSFRQ","CM2DU_AT"} },
    { "SER",new List<string>{"SEROUT","Y", "SERTESTCD" } },
    { "EX",new List<string>{"EXYN","Y", "EXREAS","EXSTDAT","EXONGO","EXMISSED"} },
    { "EXO",new List<string>{"EXOYN","Y", "EXOREAS","EXOSTDAT","EXOONGO","EXOMISSED"} },
    { "HORM1",new List<string>{"HORM1OUT","Y", "HORM1TESTCD", "HORM1CLSIG" } },
    { "IE",new List<string>{"IEYN","N", "IECAT", "IETESTCD" } },
    { "AE",new List<string>{"AEYN","Y", "AETERM","AESTDAT","AEONGO","AERELCS","AESER","AESEV","AEREL","AEACN","AEACNOTHYN","AEOUT","AEDIS","AEREL_AT"} },
    { "AE2",new List<string>{"AE2YN","Y", "AE2_PID","AE2TERM","AE2STDAT","AE2ONGO","AE2RELCS","AE2SER","AE2SEV","AE2REL","AE2ACN","AE2ACNOTHYN","AE2OUT","AE2DIS","AE2REL_AT"} },
    { "NP",new List<string>{"NPYN","Y", "NPTERM","NPSTDAT","NPONGO","NPINDC","NPINDC2","NPINDC3","NPINDC4","NPINDC5" } },
    { "NP2",new List<string>{"NP2YN","Y", "NP2PID", "NP2TERM","NP2STDAT","NP2ONGO","NP2INDC","NP2INDC2","NP2INDC3","NP2INDC4","NP2INDC5" } },
    { "URIN1",new List<string>{"URIN1OUT","Y", "URIN1TESTCD", "URIN1CLSIG" } },
    { "AT",new List<string>{"ATYN","Y", "ATTREAT", "ATSTDAT", "ATENDAT", "ATRDISC", "ATBRLOT" } },
    { "RT",new List<string>{"RTYN","Y", "RTINT", "RTLOC", "RTSTDAT", "RTENDAT" } },
    { "SG",new List<string>{"SGYN","Y", "SGREAS", "SGPROC", "SGPRLOC", "SGPRDAT" } },
    { "PH",new List<string>{"PHYN","Y","PHRECDAT", "PHPROV"} },
    { "TL",new List<string>{ "TLYN", "Y", "TLID", "TLSITE", "TLDESC", "TLMETH", "TLDAT", "TLDIAM", "TLDIAMTYP" } },
{ "NTL",new List<string>{ "NTLNON", "0", "NTLID", "NTLSITE", "NTLDESC", "NTLMETH", "NTLDAT" } },
    { "NEW",new List<string>{ "NEWYN", "Y", "NEWID", "NEWSITE", "NEWDESC", "NEWMETH", "NEWDAT" } }

};


            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpAction = afp.ActionDataPoint;
            DataPage currentPage = dpAction.Record.DataPage;
            if (!form_Match.ContainsKey(currentPage.Form.OID)) return null;

            List<string> manageFields = form_Match[currentPage.Form.OID];
            DataPoint checkDP = currentPage.MasterRecord.DataPoints.FindByFieldOID(manageFields[0]);
string convertToUservalue(string enterCode) => enterCode == "Y" ? "Yes" : enterCode == "N" ? "No" : "";
        string GetUserValueByCodedValue(DataPoint targetDP, string codedValue) => GetCKBUserValue(targetDP,codedValue)==""?
            targetDP.Field.Variable.DataDictionary?.Entries.FirstOrDefault(e => e.Key.Equals(codedValue))?.Value ?? codedValue
            : GetCKBUserValue(targetDP, codedValue);
        string GetCKBUserValue(DataPoint targetDP, string codedValue) => string.Compare(targetDP.Field.ControlType, "CHECKBOX", true) == 0 ? codedValue == "1" ? "Checked" : codedValue == "0" ? "Unchecked" : "":"";

            if (checkDP != null)
            {
                bool doVisiable = checkDP.Data == manageFields[1] && checkDP.IsVisible;
                Records allPageRC = currentPage.Records;
                foreach (Record rc in allPageRC)
                {
                    if (rc == null || rc.RecordPosition == 0) continue;
                    for (int i = 2; i < manageFields.Count; i++)
                    {
                        string checkOID = manageFields[i];
                        DataPoint manageDP = rc.DataPoints.FindByFieldOID(checkOID);
                        if (manageDP != null)
                        {
                            string querytext ="\""+ checkDP.Field.PreText + "\" is not recorded as " +"\""+ GetUserValueByCodedValue(checkDP,manageFields[1]) + "\", but \"" + manageDP.Field.PreText + "\" is present. Please verify and update, else clarify.";
                            if (doVisiable)
                            {
                                manageDP.IsVisible = true;
                                CustomFunction.PerformQueryAction(querytext, 1, false, false, manageDP, false, afp.CheckID, afp.CheckHash);
                            }
                            else
                            {
bool status = manageDP.Field.ControlType.ToUpper() != "CHECKBOX" ? !string.IsNullOrEmpty(manageDP.Data) : manageDP.Data == "1";
manageDP.IsVisible = status;
CustomFunction.PerformQueryAction(querytext, 1, false, false, manageDP, status, afp.CheckID, afp.CheckHash);
                            }
                        }
                    }
                }
            }
            return null;
}
}
}

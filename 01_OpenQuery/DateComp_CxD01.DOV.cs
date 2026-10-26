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
public class DateComp_CxD01_DOV : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/**HEADER INFORMATION:***********
        Check name : DateComp_CxD01.DOV
        Description: Check date of Assessment within 5 days(0-5) form related visit date, related visit date shoud be drfined by timepoint of current assessment 
        ***********************************************************************/

string QUERY_TEXT = "Assessment is linked to \"Cycle {0}\", however, \"{1}\" is not within +/-5 days from \"Cycle {2}\" Visit Date. Please review and correct, else clarify.";
    const int MARKING_GROUP_ID = 1;
    const bool ANSWER_ON_CHANGE = false;
    const bool CLOSE_ON_CHANGE = false;
    bool openQuery = false;

    string FolderOID_TU = "TASSMT1", ParentFormOID_TU = "TASSMT";
    string[] TPTs = new string[] { "TLATPT", "NTLATPT" };
    CFField VISDAT = new CFField() { OID = "VISDAT", FormOID = "VISIT" };
    CFField[] CFFields = new CFField[]
    {
new CFField() { OID = "TLADAT", FormOID = "TLA",FolderOID = FolderOID_TU, ParentFormOID =ParentFormOID_TU},
new CFField() { OID ="RSPDAT", FormOID = "RS" ,FolderOID = FolderOID_TU, ParentFormOID =ParentFormOID_TU},
new CFField() { OID ="RSDAT", FormOID = "RSCOM" ,FolderOID = FolderOID_TU, ParentFormOID =ParentFormOID_TU},
new CFField() { OID ="NEWDAT", FormOID = "NEW",FolderOID = FolderOID_TU, ParentFormOID =ParentFormOID_TU},
new CFField() { OID ="NTLDAT",FormOID = "NTL" ,FolderOID = FolderOID_TU, ParentFormOID =ParentFormOID_TU},
new CFField() { OID ="ICPDAT",FormOID = "ICP" ,FolderOID = FolderOID_TU, ParentFormOID =ParentFormOID_TU},
new CFField() { OID ="BLDAT",FormOID = "BL",FolderOID = FolderOID_TU, ParentFormOID =ParentFormOID_TU},
new CFField() { OID ="BLDAT",FormOID = "BL1",FolderOID = FolderOID_TU, ParentFormOID =ParentFormOID_TU}
    };

    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpAction = afp.ActionDataPoint;
    Subject currentSubject = dpAction.Record.Subject;

    if (dpAction == null || !dpAction.Active || currentSubject == null) return null;

    if (dpAction.Field.OID == VISDAT.OID)
    {
        string InsOID_Cycle = dpAction.Record.Instance.Folder.OID;
        if (InsOID_Cycle.EndsWith("D01"))
        {
            if (int.TryParse(InsOID_Cycle.Substring(1, 2), out int cycleNum)) ManageQuries(cycleNum, dpAction);
        }
    }
    else if (TPTs.Contains(dpAction.Field.OID))
    {
        DataPoint TPT = GetTargetTPT(dpAction.Record.Instance);
        if (TPT != null && TPT.Data.Contains("C"))
        {
            string cycleStr = TPT.Data.Replace("C", "").Trim();

            if (int.TryParse(cycleStr, out int cycleNum))
            {
                string compareIns_OID = cycleNum < 10 ? $"C0{cycleNum.ToString()}D01" : $"C{cycleNum.ToString()}D01";
                DataPoint compareDP = currentSubject.Instances.FindByFolderOID(compareIns_OID)?.DataPages.FindByFormOID(VISDAT.FormOID)?.MasterRecord.DataPoints.FindByFieldOID(VISDAT.OID);
                ManageQuries(cycleNum, compareDP);
            }
        }

    }
    else
    {
        string queryText = QUERY_TEXT;
        int cycleNum = GetCyceNum(dpAction);
        if (cycleNum > 0)
        {
            queryText = string.Format(queryText, cycleNum, dpAction.Field.PreText,cycleNum);
            string compareIns_OID = cycleNum < 10 ? $"C0{cycleNum.ToString()}D01" : $"C{cycleNum.ToString()}D01";
            DataPoint compareDP = currentSubject.Instances.FindByFolderOID(compareIns_OID)?.DataPages.FindByFormOID(VISDAT.FormOID)?.MasterRecord.DataPoints.FindByFieldOID(VISDAT.OID);
            openQuery = DoCompare(compareDP, dpAction);
        }
        ManageQuery(queryText, dpAction, openQuery);
    }

    void ManageQuery(string queryText, DataPoint QueryDP, bool doQuery)
    {
        CustomFunction.PerformQueryAction(queryText, MARKING_GROUP_ID, ANSWER_ON_CHANGE, CLOSE_ON_CHANGE, QueryDP, doQuery, afp.CheckID, afp.CheckHash);
        ManageExtraQuery(QueryDP, queryText);
    }

    void ManageQuries(int cycleNum, DataPoint compareDP)
    {
        DataPoints QueryDPs = new DataPoints();
        foreach (CFField cfField in CFFields)
        {
            DataPoints tempDPs = CustomFunction.FetchAllDataPointsForOIDPath(cfField.OID, cfField.FormOID, cfField.FolderOID, currentSubject);
            foreach (DataPoint dp in tempDPs)
            {
                if (dp != null && dp.Active && GetCyceNum(dp) == cycleNum) QueryDPs.Add(dp);
            }
        }
        foreach (DataPoint dp in QueryDPs)
        {
            if (dp == null || !dp.Active) continue;
            string queryText = string.Format(QUERY_TEXT, cycleNum, dp.Field.PreText,cycleNum);
            ManageQuery(queryText, dp, DoCompare(compareDP, dp));
        }
    }

    void ManageExtraQuery(DataPoint dp, string queryText)
    {
        string escapedPattern = "^" + Regex.Escape(QUERY_TEXT) + "$";
        if (dp != null && dp.Active)
        {
            Queries allDPQueries = dp.Queries;
            foreach (Query q in allDPQueries)
            {
                if (q.QueryText != queryText && q.IsOpen)
                {
                    if (Regex.IsMatch(QUERY_TEXT, escapedPattern)) q.Close();
                }
            }
        }
    }

    DataPoint GetTargetTPT(Instance currentIns)
    {
        DataPoint GetSimpleDP(string fieldOID, string formOID) => currentIns.DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints.FindByFieldOID(fieldOID);

        for (int i = 0; i < TPTs.Length - 1; i++)
        {
            string formOID = TPTs[i].StartsWith("T") ? "TLA" : "NTL";
            DataPoint tempDP = GetSimpleDP(TPTs[i], formOID);
            if (tempDP != null && tempDP.Active && tempDP.IsVisible && tempDP.Data != string.Empty)
            {
                return tempDP;
            }
        }
        return null;
    }

    return null;
}

bool DoCompare(DataPoint compareDP, DataPoint targetDP)
{
    if (compareDP != null && compareDP.Active && compareDP.StandardValue() is DateTime && targetDP != null && targetDP.Active && targetDP.StandardValue() is DateTime)
    {
        DateTime compareDT = Convert.ToDateTime(compareDP.StandardValue());
        DateTime dpDate = Convert.ToDateTime(targetDP.StandardValue());
        if (dpDate < compareDT.AddDays(-5) || dpDate > compareDT.AddDays(5)) return true;
    }
    return false;
}

int GetCyceNum(DataPoint dp)
{
    string insName_tu = dp?.Record.Instance.Name;
    Match match = Regex.Match(insName_tu, @"[Cc]ycle?\s+(\d+)", RegexOptions.IgnoreCase);
    if (match.Success && int.TryParse(match.Groups[1].Value, out int cycleMtach)) return cycleMtach;
    else return 0;
}

class CFField
{
    public string OID { set; get; }
    public string FormOID { set; get; }
    public string FolderOID { set; get; }
    public string ParentFormOID { set; get; } = string.Empty;
}
}
}

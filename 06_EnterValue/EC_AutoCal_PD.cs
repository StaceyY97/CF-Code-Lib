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
public class EC_AutoCal_PD : CustomFunctionBase
{
public object Eval(object ThisObject)
{
ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpAction = afp.ActionDataPoint;
            Subject subj = dpAction.Record.Subject;

            if (dpAction == null || !dpAction.Active || !(dpAction.StandardValue() is DateTime)) return null;

            string checkTYPE = GetPDType(dpAction);

            matchDic = checkTYPE == "SCREEN_1" ? OID_For_SCR : checkTYPE == "RESCREEN_1" ? OID_For_RESCR : null;

            if (matchDic != null)
            {
                //1.set field “check this box to recalculate score” = 0 (unchecked)
                enterforCKB(subj, "0", 0);

                //3.check SF, if current subject status is SF, or target part is Screening but at least one RESYN = Y, stop CF. (i.e. do not continue to run CF steps)
               if(subj.SubjectStatus==2|| IsRSForS1(subj,checkTYPE)) return null;
                //4.set field “Date most recently calculated” = the date the CF is run (today's date)
                enterNow(subj);

                //6.define a 14 days range form current visit1 Date of Visit date, and get all log lines that with date of assessment in that range(contain)
                DataPoint DOV = dpAction;
                DateTime LowLimit = (DateTime)DOV.StandardValue();
                DateTime UpperLimit = LowLimit + new TimeSpan(13, 0, 0, 0);
                List<DateTime> dateRange = new List<DateTime>() { LowLimit, UpperLimit };
                GetInnerRC(subj, dateRange);

                //7.If total lines <= 9 then set Insufficient data: 9 or less records = 1
                if (allMatchRC != null && allMatchRC.Count <= 9) { enterforCKB(subj, "1", 2); return null; }
                //8.If total lines >=10 then do calculate
                else
                {
enterforCKB(subj, "0", 2);
                    //calculate Bowel Movement Frequency
                    double BOWAva = GetAvarage(subj, 1);
                    enterAva(subj, BOWAva.ToString(), 3);

                    //calculate Flushing Frequency
                    double FLUAva = GetAvarage(subj, 2);
                    enterAva(subj, FLUAva.ToString(), 4);
                }
            }
            return null;

        }
        Dictionary<string, List<string>> OID_For_SCR = new Dictionary<string, List<string>>()
        {
            { "TargetFolder" , new List<string> { "PDIARY" } },
            { "TargetForm" , new List<string> { "PSDRS" } },
            { "CheckFields_Target" , new List<string> { "PSDRSREC", "PSDRSDAT", "PSDRSIND", "PSDRSABM", "PSDRSAFE" } },
            { "MatchForm" , new List<string> { "DIARY_P1" } },
            { "CheckFields_Match" , new List<string> { "PDDAT", "BOWEL_FREQ_P1", "FLUSHING_FREQ_P1" } }
        }
        ;
        Dictionary<string, List<string>> OID_For_RESCR = new Dictionary<string, List<string>>()
         {
            { "TargetFolder" , new List<string> { "PDIARY" } },
            { "TargetForm" , new List<string> { "PRSDRS" } },
            { "CheckFields_Target" , new List<string> { "PRSDRSREC", "PRSDRSDAT", "PRSDRSIND", "PRSDRSABM", "PRSDRSAFE" } },
            { "MatchForm" , new List<string> { "DIARY_P2" } },
            { "CheckFields_Match" , new List<string> { "PDDAT", "BOWEL_FREQ_P2", "FLUSHING_FREQ_P2" } },
        }
        ;
        Dictionary<string, List<string>> matchDic;

        Records allMatchRC = new Records();
        string GetPDType(DataPoint dpAction)
        {
            if (dpAction.Field.OID == "VISDAT") return dpAction.Record.Instance.Folder.OID;
            else return string.Empty;
        }

        void enterforCKB(Subject subj, string enterValue, int index)
        {
            if (index <= matchDic["CheckFields_Target"].Count - 1)
            {
                DataPoint enterCKB = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", subj.CRFVersion.ID, new object[] { subj, "",
                matchDic["TargetFolder"][0], matchDic["TargetForm"][0], matchDic["CheckFields_Target"][index] });
                if (enterCKB != null && enterCKB.Active) EnterData(enterCKB, enterValue);
                if(index == 0)enterCKB.UnFreeze();
            }
        }

        bool IsRSForS1(Subject subj, string checkType)
        {
            if(checkType== "RESCREEN_1")return false;

            DataPoints allRSYN = CustomFunction.FetchAllDataPointsForOIDPath("RSYN", "RS", null, subj);
            foreach (DataPoint RSYN in allRSYN)
            {
                if (RSYN != null && RSYN.Active && RSYN.Data == "Y") return true;
            }
            return false;
        }
        void enterNow(Subject subj)
        {
            DataPoint enterDAT = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", subj.CRFVersion.ID, new object[] { subj, "",
               matchDic["TargetFolder"][0], matchDic["TargetForm"][0], matchDic["CheckFields_Target"][1]});
            if (enterDAT != null && enterDAT.Active)
            {
                Dictionary<int, string> monthMatch = new Dictionary<int, string>()
{
    { 1, "Jan" },
    { 2, "Feb" },
    { 3, "Mar" },
    { 4, "Apr" },
    { 5, "May" },
    { 6, "Jun" },
    { 7, "Jul" },
    { 8, "Aug" },
    { 9, "Sep" },
    { 10, "Oct" },
    { 11, "Nov" },
    { 12, "Dec" }
};
                DateTime now = DateTime.Now;
                string now_Enter = now.Day.ToString() + " " + monthMatch[now.Month] + " " + now.Year.ToString();
                EnterData(enterDAT, now_Enter);
            }
        }

        void GetInnerRC(Subject subj, List<DateTime> range)
        {
            DataPoints PDDAT_target = CustomFunction.FetchAllDataPointsForOIDPath(matchDic["CheckFields_Match"][0], matchDic["MatchForm"][0], matchDic["TargetFolder"][0], subj);
            foreach (DataPoint dp in PDDAT_target)
            {
                if (dp == null || !dp.Active || dp.Record.RecordPosition == 0 || !(dp.StandardValue() is DateTime)) continue;
                DateTime checkDT = (DateTime)dp.StandardValue();
                if (checkDT >= range[0] && checkDT <= range[1]) allMatchRC.Add(dp.Record);
            }
        }

        double GetAvarage(Subject subj, int index)
        {
            double sumAll = 0;
            int time = 0;
            if (index <= matchDic["CheckFields_Match"].Count - 1)
            {
                foreach (Record rc in allMatchRC)
                {
                    DataPoint enterBOW = rc.DataPoints.FindByFieldOID(matchDic["CheckFields_Match"][index]);
                    if (enterBOW != null && enterBOW.Active && enterBOW.Data != string.Empty)
                    {                    
                        double BOW;
                        if (double.TryParse(enterBOW.Data, out BOW))
                        {
                            sumAll += BOW;
                            time += 1;
                        }
                    }
                }
            }
            return time != 0 ? Math.Round(sumAll / time, 1) : 0;
        }

        void enterAva(Subject subj,string enterValue,int index)
        {
            DataPoint enterBOW = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", subj.CRFVersion.ID, new object[] { subj, "",
                        matchDic["TargetFolder"][0], matchDic["TargetForm"][0], matchDic["CheckFields_Target"][index] });
            EnterData(enterBOW, enterValue);
        }

        public void EnterData(DataPoint dp, string val)
        {
            if (dp != null && dp.Active && string.Compare(dp.Data, val, true) != 0)
            {
                dp.UnFreeze();
                dp.Enter(val, string.Empty, 0);
                dp.Freeze();
            }
}
}
}

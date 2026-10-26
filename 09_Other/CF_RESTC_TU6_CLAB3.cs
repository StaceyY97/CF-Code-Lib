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
public class CF_RESTC_TU6_CLAB3 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:**************************
        * CF Name: CF_RESTC_TU6_CLAB3_1
        * Programmer Name:Qian Yang/19Nov2025
        * Last Modified:NA
        * Reason Modified: NA
        * Description: Add forms in conditions **/

        ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
        dp = afp.ActionDataPoint;
        //below should be defined by programmer
        string[] formArr = new string[] { "CLAB3" };
        string targetFolder = "TU6";

        TriggerInfor trigger_1 = new TriggerInfor
        {
            FieldIndex = new string[] { "PRTVR", "DSCONS", "SCR" },
            ConditionValues_Convers = new string[] { "V2", "V3" },
            CheckNotEmpty = true
        };

        TriggerInfor trigger_2 = new TriggerInfor
        {
            FieldIndex = new string[] { "COHORT", "ENROLL", "SCR" },
            ConditionValues = new string[] { "1", "2A" },
        };

        TriggerInfor[] allowedTrigger = new TriggerInfor[] { trigger_1, trigger_2 };

        TriggerInfor trigger_3 = new TriggerInfor
        {
            FieldIndex = new string[] { "COHORT", "ENROLL", "SCR" },
            ConditionValues = new string[] { "1C", "2C", "2E", "3B" },
        };

        TriggerInfor[] allowedTrigger_2 = new TriggerInfor[] { trigger_3 };

        string EnterDPvalue = "Y";
        //above should be defined by programmer

        Instance currIns = dp.Record.Instance;
        string folderOID = currIns.Folder.OID;

        if (folderOID == targetFolder)
        {
             bool isMatch = IsMatchTrigger(allowedTrigger_2)||(DoSwitch() &&IsMatchTrigger(allowedTrigger));
            bool doAdd = dp?.Data == EnterDPvalue && isMatch;
            for (int i = 0; i < formArr.Length; i++) CustomFunction.PerformCustomFunction("CF_Aid_AddForm", dp.Field.CRFVersion.ID, new object[] { currIns, doAdd, formArr[i] });
        }

        return null;
    }

    DataPoint dp = null;
    DataPoint GetSimpleDP(string fieldOID, string formOID, string foldOID) =>
dp.Record.Subject?.Instances?.FindByFolderOID(foldOID)?.DataPages?.FindByFormOID(formOID)?.MasterRecord?.DataPoints?.FindByFieldOID(fieldOID);
    DataPoint GetSimpleDP(string fieldOID, string formOID, string foldOID, string foldOID_Parent) =>
dp.Record.Subject?.Instances?.FindByFolderOID(foldOID_Parent)?.Instances.FindByFolderOID(foldOID).DataPages?.FindByFormOID(formOID)?.MasterRecord?.DataPoints?.FindByFieldOID(fieldOID);

    bool DoSwitch()
    {
try{
        string[] switchField = new string[] { "VISDAT", "VISIT", "TU6", "TU" };
        string[] switchField_1 = new string[] { "VISDAT", "VISIT", "C3D1", "TRT" };
        DataPoint switchDP_Pre = GetSimpleDP(switchField_1[0], switchField_1[1], switchField_1[2], switchField_1[3]);
        DataPoint switchDP_After = GetSimpleDP(switchField[0], switchField[1], switchField[2], switchField[3]);
        if (switchDP_Pre != null && switchDP_After != null && switchDP_Pre.StandardValue() is DateTime && switchDP_After.StandardValue() is DateTime)
        {
            DateTime dtPre = (DateTime)switchDP_Pre.StandardValue();
            DateTime dtAfter = (DateTime)switchDP_After.StandardValue();
            return dtAfter > dtPre && dtPre.Add(new TimeSpan(14, 0, 0, 0)) < dtAfter;
        }
}
catch{}
        return false;
    }
    bool IsMatchTrigger(TriggerInfor[] allowedTrigger)
    {
        bool isAllMeet = true;
try{
        foreach (TriggerInfor trigger in allowedTrigger)
        {
            DataPoint triggerDP = GetSimpleDP(trigger.FieldIndex[0], trigger.FieldIndex[1], trigger.FieldIndex[2]);
            if (!trigger.IsConditionMet(triggerDP))
            {
                isAllMeet = false;
                break;
            }
        }
}
catch{}
        return isAllMeet;
    }
    public class TriggerInfor
    {
        public string[] FieldIndex { get; set; }
        public string[] ConditionValues { get; set; }
        public string[] ConditionValues_Convers { get; set; }
        public bool CheckNotEmpty { get; set; } = false;

        public bool IsConditionMet(DataPoint triggerDP)
        {
            if (triggerDP == null || !triggerDP.Active) return false;
            if (CheckNotEmpty)
            {
                ArrayList trigegrValue = new ArrayList(ConditionValues_Convers);
                return !trigegrValue.Contains(triggerDP.Data) && triggerDP.Data != string.Empty;
            }
            else
            {
                ArrayList trigegrValue = new ArrayList(ConditionValues);
                return triggerDP != null && triggerDP.Active && trigegrValue.Contains(triggerDP.Data);
            }
        }
}
}
}

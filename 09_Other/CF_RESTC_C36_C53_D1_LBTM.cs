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
public class CF_RESTC_C36_C53_D1_LBTM : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:**************************
        * CF Name: CF_RESTC_C36_C53_D1_LBTM
        * Programmer Name:Qian Yang/19Nov2025
        * Last Modified:NA
        * Reason Modified: NA
        * Description: Add forms in conditions **/

        ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
        dp = afp.ActionDataPoint;

        //below should be defined by programmer
        string[] formArr = new string[]{ "LBTM" };
        System.Collections.Generic.List<int> allowedCycle = new System.Collections.Generic.List<int>() { 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53 };

        TriggerInfor trigger_1 = new TriggerInfor
        {
            FieldIndex = new string[] { "COHORT", "ENROLL", "SCR" },
            ConditionValues = new string[] {  "1C", "2C", "2E", "3B" },
        };

        TriggerInfor[] allowedTrigger = new TriggerInfor[] { trigger_1 };

        string folderEnds = "D1";
        string EnterDPvalue = "Y";

        //above should be defined by programmer


        Instance currIns = dp.Record.Instance;
        string folderOID = currIns.Folder.OID;
        int index_C = folderOID.IndexOf("C");
        int index_D = folderOID.IndexOf("D");

        if (folderOID.EndsWith(folderEnds) && int.TryParse(folderOID.Substring(index_C + 1, index_D - index_C - 1), out int cycleNumber) && allowedCycle.Contains(cycleNumber))
        {
            bool doAdd = dp.Data == EnterDPvalue && IsMatchTrigger(allowedTrigger);
            for (int i = 0; i < formArr.Length; i++) CustomFunction.PerformCustomFunction("CF_Aid_AddForm", dp.Field.CRFVersion.ID, new object[] { currIns, doAdd, formArr[i] });
        }

        return null;
    }

    DataPoint dp  = null;
    DataPoint GetSimpleDP(string fieldOID, string formOID, string foldOID) =>
dp.Record.Subject?.Instances?.FindByFolderOID(foldOID)?.DataPages?.FindByFormOID(formOID)?.MasterRecord?.DataPoints?.FindByFieldOID(fieldOID);

    bool IsMatchTrigger(TriggerInfor[] allowedTrigger)
    {
        bool isAllMeet = true;
        foreach (TriggerInfor trigger in allowedTrigger)
        {
            DataPoint triggerDP = GetSimpleDP(trigger.FieldIndex[0], trigger.FieldIndex[1], trigger.FieldIndex[2]);
            if (!trigger.IsConditionMet(triggerDP))
            {
                isAllMeet = false;
                break;
            }
        }
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
                return !trigegrValue.Contains(triggerDP.Data) &&triggerDP.Data!=string.Empty;
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

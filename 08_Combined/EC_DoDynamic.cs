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
public class EC_DoDynamic : CustomFunctionBase
{
public object Eval(object ThisObject)
{
try{
afp = (ActionFunctionParams)ThisObject;
        enterDP = afp.ActionDataPoint;
        // Get all check action details
        if (afp.CheckID <= 0) return null;
        Checks allCKS = afp.ActionDataPoint.Record.Subject.CRFVersion?.Checks;
        if (allCKS == null) return null;
        //if (enterDP.Record.Subject.Name == "QY005" && enterDP.Field.OID == "EXDOSE") Message.SendEmail("qian.yang@edetek.com", "test_qy@mdsol.com", "test_qy",enterDP.Record.RecordPosition.ToString());
        foreach (Check tempCK in allCKS)
        {
            if (tempCK?.ID == afp.CheckID)
            {
                manageCK = tempCK;
                ManageSteps_DP(manageCK);
                break;
            }
        }
        string[] unitDic = { "Height", "Weight", "TEMPUNIT", "AGE", "HORM2UNIT", "HORM2UNIT2" };
        if (triggerDPs?.Count > 0)
        {
            querytext = (string)CustomFunction.PerformCustomFunction("CF_QueryBuilder", enterDP.Field.CRFVersion.ID, new object[] { triggerDPs, relation, querytext, enterDP }) + ", but \"" + (enterDP?.Field?.PreText ?? "field") + "\"" + " is present. Please verify and update, else clarify.";
            GetStepResultAndUpdateQuery();
            bool meet = IsMeet();
            //if condition meet or value or unit(if any) is not empty or checkbox is checked set DP visible(Any one entered)
            bool hasNoValue_Unmeet = unitDic.Contains(enterDP.Field.Variable.UnitDictionary?.Name) ? string.IsNullOrEmpty(enterDP.Data) && (enterDP.UnitDictionaryEntry == null || enterDP.UnitDictionaryEntry?.UserUnitString == "") :
    enterDP.Field.ControlType.ToUpper() != "CHECKBOX" ? string.IsNullOrEmpty(enterDP.Data) : enterDP.Data == "0";
            bool shouldBeVisible = meet || !hasNoValue_Unmeet;
            enterDP.IsVisible = shouldBeVisible;
            //if DP Should be visible but condition not meet,then raise consistant type query
            bool shouldBeQuey = !meet && shouldBeVisible;
            CustomFunction.PerformQueryAction(querytext, 1, false, false, enterDP, shouldBeQuey, afp.CheckID, afp.CheckHash);
            //for fields used unit dictionary only: if condition meet but value or/and unit is/are empty, raise required query(Any on not entered)
            if (unitDic.Contains(enterDP.Field.Variable.UnitDictionary?.Name))
            {
                bool hasNoValue_meet = string.IsNullOrEmpty(enterDP.Data) || enterDP.UnitDictionaryEntry == null || enterDP.UnitDictionaryEntry?.UserUnitString == "";
                bool shouldREQ = meet && hasNoValue_meet;
                CustomFunction.PerformQueryAction("Data is required. Please complete.", 1, false, false, enterDP, shouldREQ, afp.CheckID, afp.CheckHash);
            }
        }
}
catch{}
        return null;
    }

    // Define basic parameters
    private ActionFunctionParams afp;
    private DataPoint enterDP;
    private Check manageCK;
    private DataPoint triggerDP;
    private int AllStepCount = 0;
    private string querytext = "";

    private Dictionary<int, List<object>> triggerDPs = new Dictionary<int, List<object>>();
    private Dictionary<int, string> relation = new Dictionary<int, string>();
    private List<bool> stepResult = new List<bool>();

    List<string> FUNC_TYPE = new List<string>
        {
            "IsEqualTo",
            "IsNotEqualTo",
            "IsNotEmpty",
            "IsGreaterThan",
            "IsLessThan",
            "IsGreaterThanOrEqualTo",
            "IsLessThanOrEqualTo",
            "Contains"
        };

    List<string> REl_TYPE = new List<string>()
        {
            "And",
            "Or"
        };

    Dictionary<string, Func<string, string, bool>> Comparers = new Dictionary<string, Func<string, string, bool>>()
{
    { "IsEqualTo", (a, b) => a.ToUpper() == b.ToUpper() },
    { "IsNotEqualTo", (a, b) => a.ToUpper() != b.ToUpper() },
    { "IsNotEmpty", (a, _) => !string.IsNullOrEmpty(a) },
    { "IsGreaterThan", (a, b) => double.TryParse(a, out var numA) && double.TryParse(b, out var numB) && numA > numB },
    { "IsLessThan", (a, b) => double.TryParse(a, out var numA) && double.TryParse(b, out var numB) && numA < numB },
    { "IsGreaterThanOrEqualTo", (a, b) => double.TryParse(a, out var numA) && double.TryParse(b, out var numB) && numA >= numB },
    { "IsLessThanOrEqualTo", (a, b) => double.TryParse(a, out var numA) && double.TryParse(b, out var numB) && numA <= numB },
    { "Contains", (a, b) => a.ToUpper().Contains(b.ToUpper()) }

};

    Dictionary<string, Func<bool, bool, bool>> checkBool = new Dictionary<string, Func<bool, bool, bool>>()
{
    { "And", (a, b) => a && b },
    { "Or", (a, b) => a || b },
};
    private DataPoint GetMatchDP(CheckStep DPstep)
    {
        if (DPstep == null || enterDP?.Record?.Subject == null) return null;

        DataPoints allSubjectDPs = enterDP?.Record?.DataPoints;
        if (allSubjectDPs == null) return null;

        foreach (DataPoint dp in allSubjectDPs) { if (dp != null && dp.Active && DPstep.DoesDataPointMatch(dp)) return dp; }
        allSubjectDPs = enterDP?.Record.DataPage.MasterRecord.DataPoints;
        if (allSubjectDPs == null) return null;
        foreach (DataPoint dp in allSubjectDPs) { if (dp != null && dp.Active && DPstep.DoesDataPointMatch(dp)) return dp; }

        return null;
    }

    private void ManageSteps_DP(Check manageCK)
    {
        if (manageCK?.Actions == null || manageCK?.Steps == null) return;
        var (allActions, allSteps) = (manageCK.Actions, manageCK.Steps);
        AllStepCount = allSteps.Count - allActions.Count * 3;
        if (AllStepCount < 2) return;

        for (int i = 0, funcIndex = 1, relIndex = 1; i < AllStepCount; i++)
        {
            var tempStep = allSteps[i];
            if (tempStep == null) continue;
            string tempFunc = tempStep.Function;
            if (FUNC_TYPE.Contains(tempStep.Function) && i >= 1)
            {
                var DPstep = tempFunc == "IsNotEmpty" ? allSteps[i - 1] : i >= 2 ? allSteps[i - 2] : null;
                var compareStep = tempFunc != "IsNotEmpty" && i >= 2 ? allSteps[i - 1] : null;
                var checkDP = GetMatchDP(DPstep);
                var compareOB = !string.IsNullOrEmpty(compareStep?.StaticValue) ? compareStep?.StaticValue : GetMatchDP(compareStep)?.Data ?? "";
                if (checkDP != null) triggerDPs[funcIndex++] = new List<object> { checkDP, compareOB, tempFunc };
            }
            if (REl_TYPE.Contains(tempStep.Function)) relation[relIndex++] = tempStep.Function;
        }
    }
    private bool IsMeet() =>
        stepResult == null || stepResult.Count == 0 ? false :
        stepResult.Count == 1 ? stepResult[0] :
        stepResult.Skip(1).Aggregate(stepResult[0], (current, next) => relation.TryGetValue(stepResult.IndexOf(next), out var relType) && checkBool.TryGetValue(relType, out var comparer)
         ? comparer(current, next)
         : current);
    private void GetStepResultAndUpdateQuery()
    {
        if (triggerDPs == null) return;
        foreach (var kvp in triggerDPs)
        {
            if (kvp.Value?.Count > 2 && kvp.Value[0] is DataPoint)
            {
                triggerDP = (DataPoint)kvp.Value[0];
                string compareValue = kvp.Value[1]?.ToString() ?? "";
                int index = Convert.ToInt32(kvp.Key);
                if (triggerDP != null) stepResult.Add(Comparers.TryGetValue((string)kvp.Value[2], out var comparer) ? comparer(triggerDP.Data, compareValue) : false);

            }
        }
}
}
}

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
public class DER_TLSUM_TLDIAM : CustomFunctionBase
{
public object Eval(object ThisObject)
{
ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dp = afp.ActionDataPoint;
    if (dp == null) return null;
    Subject subj = dp.Record.Subject;
    Instance curIns = dp.Record.DataPage.Instance;
    DataPage curPage = dp.Record.DataPage;

    string dataFields = "TLDIAM", sum = "TLSUM",checkField = "TLDIAMNE",NELable = "TLSUMNE";

    DataPoint dpSum = curPage.MasterRecord.DataPoints.FindByFieldOID(sum);
    if (dpSum == null) return null;

    double sumValue = 0;
    bool atLeastOneValue = false;
    bool atLeastOneNE = false;
    foreach (Record rc in curPage.Records)
    {
        if (rc == null || !rc.Active || rc.RecordPosition == 0) continue;

        DataPoint dpDiam = rc.DataPoints.FindByFieldOID(dataFields);
        DataPoint NE = rc.DataPoints.FindByFieldOID(checkField);
        if (!atLeastOneValue) atLeastOneValue = dpDiam != null && dpDiam.Active && dpDiam.Data != string.Empty && double.TryParse(dpDiam.Data.ToString(), out double a);
        if (!atLeastOneNE) atLeastOneNE = NE != null && NE.Active && NE.Data == "1";
        if (dpDiam != null) sumValue += ValueTrans(dpDiam.Data);
    }

    if (atLeastOneValue&&!atLeastOneNE) EnterData(dpSum, sumValue.ToString());
    else EnterData(dpSum, string.Empty);

 DataPoint dpNE_Lable= curPage.MasterRecord.DataPoints.FindByFieldOID(NELable);
        if (dpNE_Lable != null&&dpNE_Lable.IsVisible) 
{
  if (atLeastOneNE)EnterData(dpNE_Lable, "1");
else EnterData(dpNE_Lable, "0");
}

    return null;
}

double ValueTrans(string inputString)
{
    double valueOut = 0;
    if (string.IsNullOrEmpty(inputString)) return valueOut;
    if (Number.IsValidNumber(inputString))
    {
        try { valueOut = double.Parse(inputString); }
        catch { }
    }
    return valueOut;
}
void EnterData(DataPoint enterDP, string enterValue)
{
    if (enterDP != null)
    {
        enterDP?.UnFreeze();
        enterDP?.Enter(enterValue, string.Empty, 0);
        enterDP?.Freeze();
    }
}
}
}

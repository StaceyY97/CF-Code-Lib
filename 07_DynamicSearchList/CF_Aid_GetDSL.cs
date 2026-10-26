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
public class CF_Aid_GetDSL : CustomFunctionBase
{
public object Eval(object ThisObject)
{
//Aid CF, will be referenced by CF: CF_DynamicSearchList

    object[] inputParameters = (object[])ThisObject;
    string FOLDEROID = (string)inputParameters[0];
    string FORMOID = (string)inputParameters[1];
    string FIELDOID1 = (string)inputParameters[2];
    string FIELDOID2 = (string)inputParameters[3];
    Subject subj = (Subject)inputParameters[4];

    preInfo = FORMOID.EndsWith("2") ? "OLE - " : string.Empty;

    const int FIELD_LENGTH = 50;
    KeyValueCollection kvcList = new KeyValueCollection();
    DataPoints dpsAE_S01 = CustomFunction.FetchAllDataPointsForOIDPath(FIELDOID1, FORMOID, FOLDEROID, subj);

    for (int i = 0; i < dpsAE_S01.Count; i++)
    {
        if (dpsAE_S01[i] != null && dpsAE_S01[i].Record.Active)
        {
            DataPoint dpAeStartDate = dpsAE_S01[i].Record.DataPoints.FindByFieldOID(FIELDOID2);
            if (dpsAE_S01[i].Data.ToString() != string.Empty)
            {
                string temp = dpAeStartDate.Data;
                string entry = BuildEntry(dpsAE_S01[i].Record.RecordPosition, dpsAE_S01[i].Data, temp, FIELD_LENGTH);
                KeyValue kvAE = new KeyValue(entry, entry);
                kvcList.Add(kvAE);
            }
        }
    }
    return kvcList;
}
string preInfo;

private string BuildEntry(int recPos, string term, string startDate, int targetFieldLength)
{
    string aeRecPos = recPos.ToString();
    string strTARGET = term.Replace("\r", "").Replace("\n", "");
    string aeTerm = strTARGET.Trim();

    if (aeTerm.Length > targetFieldLength)
    {
        aeTerm = aeTerm.Substring(0, targetFieldLength);
    }

    string entry = preInfo+aeRecPos.ToString() + " - " + aeTerm;

    if (startDate != string.Empty)
    {
        entry += (" - " + startDate);
    }

    return entry;
}
}
}

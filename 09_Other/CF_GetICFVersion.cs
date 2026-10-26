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
public class CF_GetICFVersion : CustomFunctionBase
{
public object Eval(object ThisObject)
{
//Aid CF: CF_GetICFVersion
    //Get ICF Version by comparing visit date with version date, return string
    //Qian Yang/ 14Aug2026

    object[] inputParameters = (object[])ThisObject;
    Subject subject = (Subject)inputParameters[0];
    DateTime visitDate = (DateTime)inputParameters[1];

    if (subject == null || visitDate == null) return string.Empty;

    List<string[]> ICFs = new List<string[]>()
    {
        new string[] { "DSDAT", "DSCONS", "SCR", "PRTVR"},
        new string[] { "RESTDAT", "RECONS", "CONSENT","REPROT" }
    };
    string protocolVersion = "";

    List<CF_Pair> Pair = new List<CF_Pair>();

    foreach (string[] OIDs in ICFs)
    {
        string fieldOID_Date = OIDs[0],
            formOID = OIDs[1],
            folderOID = OIDs[2] == string.Empty ? null : OIDs[2],
            fieldOID_Version = OIDs[3];
        DataPoints tempICF = CustomFunction.FetchAllDataPointsForOIDPath(fieldOID_Date, formOID, folderOID, subject);
        List<CF_Pair> temPair = ConvetToPair(tempICF, fieldOID_Version);
        foreach (CF_Pair pr in temPair) Pair.Add(pr);
    }
    Pair = Pair.OrderBy(x => x.VersionDate).ToList();

    foreach (CF_Pair tempPR in Pair)
    {
        if (visitDate >= tempPR.VersionDate)
        {
            double Vnumber = tempPR.VersionNumber;
            protocolVersion = Vnumber == 0 ? protocolVersion : $"V{Vnumber}";
        }
        else break;
    }

    return protocolVersion;
}
bool DoCheck(DataPoint dp, bool IsDate, bool checkEmpty = true) => IsDate ? IsValidDp(dp, checkEmpty) && IsValidDate(dp) : IsValidDp(dp, checkEmpty);
bool IsValidDp(DataPoint dp, bool checkEmpty) => dp != null && dp.Active && dp.IsVisible && (checkEmpty ? dp.Data != string.Empty : true);
bool IsValidDate(DataPoint dp) => dp != null && dp.Active && dp.IsVisible && (dp.StandardValue() is DateTime || dp.StandardValue() is TimeSpan);

List<CF_Pair> ConvetToPair(DataPoints dps, string versionOID)
{
    List<CF_Pair> pairList = new List<CF_Pair>();
    foreach (DataPoint dp in dps)
    {
        if (DoCheck(dp, true))
        {
            DataPoint version = dp.Record.DataPoints.FindByFieldOID(versionOID);
            if (DoCheck(version, false))
            {
                int versionNumber = int.TryParse(version.Data.Replace("V",""), out versionNumber) ? versionNumber : 0;
                CF_Pair pair = new CF_Pair() { VersionNumber = versionNumber, VersionDate = (DateTime)dp.StandardValue() };
                pairList.Add(pair);
            }
        }
    }
    return pairList;
}

class CF_Pair
{
    public int VersionNumber { get; set; }
    public DateTime VersionDate { get; set; }
}
}
}

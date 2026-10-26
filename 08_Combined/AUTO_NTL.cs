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
public class AUTO_NTL : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:*********
    * CF Name: AUTO_NTL
    * Programmer Name:YueXu (15 Nov 2024)
    * Last Modified: Not Applicable
    * Reason Modified: Not Applicable
    * Description: Derive value and visible atrribute **/

    ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
    DataPoint dpAction = afp.ActionDataPoint;
    Subject subject = dpAction.Record.Subject;
    Instance Inst = dpAction.Record.DataPage.Instance;

    Instance inst = subject.Instances.FindByFolderOID("SCRN");  
    if (inst == null) return null;

    DataPage Source_page = inst.DataPages.FindByFormOID("NTLS");

    DataPoints dps = CustomFunction.FetchAllDataPointsForOIDPath("NTSALNS", "NTLS", "SCRN", subject);  
    DataPoints dps1 = CustomFunction.FetchAllDataPointsForOIDPath("NTLALNS", "NTL", "TASSMT1", subject); 
    if (dps != null && dps.Count > 0 && Source_page != null && Source_page.Active) 
    {
        for (int i = 0; i < dps1.Count; i++) 
        {
            DataPage dpageDest = dps1[i].Record.DataPage;
            if (dpageDest != null && dpageDest.Active)
            {
                CopyData(Source_page, dpageDest);
            }
        }
    }
    return null;
}
void CopyData(DataPage dpageSource, DataPage dpageDest)
{
    if (dpageSource != null && dpageDest != null)
    {
        for (int i = 0; i < dpageSource.Records.Count; i++)
        {
            Record rd = dpageSource.Records[i]; 
            Record destRd = GetRecord(dpageDest, rd.RecordPosition);
            if (destRd != null)
            {
                CopyRecord(rd, destRd);
                destRd.Active = rd.Active;
            }
        }
        if (dpageDest.Records.Count > dpageSource.Records.Count)
        {
            for (int i = dpageSource.Records.Count; i < dpageDest.Records.Count; i++)
            {
                Record rd = dpageDest.Records[i];
                rd.Active = false;
            }
        }
    }
}
Dictionary<string, string> myDictionary = new Dictionary<string, string>
{
{"NTSALNS", "NTLALNS"},
{"NTLSALC", "NTLALC"},
{"NTLSABLC","NTLABLC"},
{"NTLSCLC", "NTLCLC"},
{"NTLSLLC","NTLLLC"},
{"NTLSPLC",  "NTLPLC"},
{"NTLSSLC", "NTLSLC"},
{"NTLSBLC","NTLBLC"},
{"NTLSAOLC" , "NTALAOLC"},
{"NTLSODES", "NTLAODES"},
{"NTLSATTY", "NTLLATTY"},
{ "NTLSATYP","NTLATYPO"}
};
void CopyRecord(Record rdSource, Record rdDest)
{
    if (rdSource != null && rdDest != null)
    {
        if (rdSource.RecordPosition != 0)
        {
            foreach (KeyValuePair<string, string> pair in myDictionary)
            {
                DataPoint dpSrc = rdSource.DataPoints.FindByFieldOID(pair.Key);
                DataPoint dpDest = rdDest.DataPoints.FindByFieldOID(pair.Value);
                if (dpSrc != null && dpDest != null)
                {
                    dpDest.Freeze(false);
                    EnterData(dpSrc, dpDest);
                    dpDest.Active = dpSrc.Active;
                    dpDest.IsVisible = dpSrc.IsVisible;
                    dpDest.Freeze(true);
                }
            }
        }
    }
}
Record GetRecord(DataPage dpage, int RecPosition)
{
    for (int i = 0; i <= RecPosition - dpage.Records.Count; i++)
    {
        dpage.AddLogRecord();
    }
    for (int i = 0; i < dpage.Records.Count; i++)
    {
        if (dpage.Records[i].RecordPosition == RecPosition)
            return dpage.Records[i];
    }
    return null;
}
void EnterData(DataPoint dpSource, DataPoint dpDest)
{
    if (dpSource != null && dpDest != null && dpSource.ChangeCount > 0)
    {
        if (dpDest.LockStatus != LockStatusEnum.Locked)
            dpDest.Enter(dpSource.Data, string.Empty, 0);
    }
}
}
}

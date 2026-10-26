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
public class DSL_EOTBEV_AEID : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*Developer Name: Sandeep Kanike, Edetek
  Date:06-NOV-2025
  CF Name: DSL_EOTBEV_AEID
  Edit check name: DSL_EOTBEV_AEID, DSL_EOTBEV_AEID_CROSS
  Description: Add Dynamic Search List (AE.AETERM and AE.AESTDAT) to EOTBEV.AEID field*/

Medidata.Core.Objects.DynamicSearchParams Dsp = (Medidata.Core.Objects.DynamicSearchParams)ThisObject;
DataPoint Dpaction = Dsp.DataPoint;
Subject sub = Dpaction.Record.Subject;

Records rds = new Records();
Record rd = null;
DataPoint Dp_AETERM = null, Dp_AESTDAT = null;
string AEValue = string.Empty;
Medidata.Utilities.KeyValueCollection KVC = new Medidata.Utilities.KeyValueCollection();

Instance Ins = sub.Instances.FindByFolderOID("AE");
if (Ins != null)
{
    DataPage Dpg = Ins.DataPages.FindByFormOID("AE");
    if (Dpg != null)
    {
        rds = Dpg.Records;
        if (rds.Count > 0)
        {
            for (int i = 0; i < rds.Count; i++)
            {
                rd = rds[i];
                if (rd == null || !rd.Active || rd.RecordPosition == 0) continue;

                Dp_AETERM = rd.DataPoints.FindByFieldOID("AETERM");
                Dp_AESTDAT = rd.DataPoints.FindByFieldOID("AESTDAT");

                if (Dp_AETERM != null && Dp_AESTDAT != null &&
                    string.Compare(Dp_AETERM.Data.Trim(), string.Empty, true) != 0 &&
                    Dp_AESTDAT.StandardValue() is DateTime)
                {
                    AEValue = Dp_AETERM.Record.RecordPosition.ToString();
                    AEValue += "-";

                    if (Dp_AESTDAT.Data.IndexOf("UNK") > 0 || Dp_AESTDAT.Data.IndexOf("UN") == 0)
                        AEValue += ("-" + Dp_AESTDAT.UserValue().ToString());
                    else
                    {
                        DateTime dt = Convert.ToDateTime(Dp_AESTDAT.StandardValue());
                        AEValue += ("-" + dt.ToString("dd MMM yyyy").ToUpper());
                    }
                    AEValue = AEValue.Trim();

                    if (AEValue != string.Empty)
                    {
                        Medidata.Utilities.KeyValue KV1 = new Medidata.Utilities.KeyValue(Dp_AETERM.Record.RecordPosition.ToString(), AEValue);
                        KVC.Add(KV1);
                    }
                }
            }
        }
    }
}
return KVC;
}
}
}

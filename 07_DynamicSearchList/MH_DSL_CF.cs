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
public class MH_DSL_CF : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/**************************HEADER INFORMATION******************************
        Programmer Name: Athiyappan Ganesan (04 SEP 2023)
        Custom Function: MH_DSL_CF
        EditCheck : CM_CMMHDSL_MH_DSL_CF 
                    CM_CMMHDSL2_MH_DSL_CF 
                    CM_CMMHDSL3_MH_DSL_CF
                    CM_CMMHDSL4_MH_DSL_CF
                    CM_CMMHDSL5_MH_DSL_CF
        **************************************************************************/

        string MHTERM = "MHTERM";
        string MHSTDAT = "MHSTDAT";
        Medidata.Core.Objects.DynamicSearchParams DSP = (Medidata.Core.Objects.DynamicSearchParams) ThisObject;
        Subject subject = DSP.DataPoint.Record.Subject;
        DataPoint dpt_MHTERM = null, dpt_MHSTDAT = null;

        Medidata.Utilities.KeyValueCollection KVC = new Medidata.Utilities.KeyValueCollection();
        try
        {

            DataPage dpageMH1 = GetAEDataPage(subject);
            if (dpageMH1 != null && dpageMH1.Active)
            {

                for (int i = 0; i < dpageMH1.Records.Count; i++)
                {
                    if (dpageMH1.Records[i].RecordPosition == 0 || !dpageMH1.Records[i].Active)
                    continue;
                    dpt_MHTERM = dpageMH1.Records[i].DataPoints.FindByFieldOID(MHTERM);
                    dpt_MHSTDAT = dpageMH1.Records[i].DataPoints.FindByFieldOID(MHSTDAT);

                    if (dpt_MHTERM.ChangeCount > 0 && dpt_MHTERM != null && dpt_MHSTDAT != null)
                    {
                        string ds1 = dpt_MHTERM.Record.RecordPosition.ToString();
                        if (string.Compare(dpt_MHTERM.Data.Trim(), string.Empty, true) != 0)
                        {
                            ds1 += (dpt_MHTERM.Data.Trim().Length >= 60 ? " - " + dpt_MHTERM.Data.Trim().Substring(0, 60) : " - " + dpt_MHTERM.Data.Trim());

                        }
                        ds1 += " - " + dpt_MHSTDAT.UserValue();
                        if (string.Compare(ds1.Trim(), string.Empty) != 0)
                        {
                            Medidata.Utilities.KeyValue KV1 = new Medidata.Utilities.KeyValue(ds1, ds1);
                            KVC.Add(KV1);
                        }
                    }
                }
            }
            return KVC;
        }
        catch
        {
            return KVC;
        }
    }
    DataPage GetAEDataPage(Subject subject)
    {
        Instance inst = subject.Instances.FindByFolderOID("SCRN");
        if (inst != null && inst.Active)
        {
            DataPage dpg = inst.DataPages.FindByFormOID("MH");
            if (dpg != null && dpg.Active)
            return dpg;
            else
            return null;
        }
        return null;
}
}
}

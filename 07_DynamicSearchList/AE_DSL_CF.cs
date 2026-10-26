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
public class AE_DSL_CF : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/**************************HEADER INFORMATION******************************
        Programmer Name: Athiyappan Ganesan (04 SEP 2023)
        Custom Function: AE_DSL_CF
        EditCheck : AE_DSL_CF
        **************************************************************************/

        string AETERM = "AETERM";
        string AESTDAT = "AESTDAT";
        Medidata.Core.Objects.DynamicSearchParams DSP = (Medidata.Core.Objects.DynamicSearchParams) ThisObject;
        Subject subject = DSP.DataPoint.Record.Subject;

        Medidata.Utilities.KeyValueCollection KVC = new Medidata.Utilities.KeyValueCollection();
        try
        {

            DataPage dpageAe = GetAEDataPage(subject);
            if (dpageAe != null && dpageAe.Active)
            {

                for (int i = 0; i < dpageAe.Records.Count; i++)
                {
                    if (dpageAe.Records[i].RecordPosition == 0 || !dpageAe.Records[i].Active)
                    continue;
                    DataPoint dpAETERM = dpageAe.Records[i].DataPoints.FindByFieldOID(AETERM);
                    DataPoint dpAEDT = dpageAe.Records[i].DataPoints.FindByFieldOID(AESTDAT);

                    if (dpAETERM.ChangeCount > 0 && dpAETERM != null && dpAEDT != null)
                    {
                        string ds1 = dpAETERM.Record.RecordPosition.ToString();
                        if (string.Compare(dpAETERM.Data.Trim(), string.Empty, true) != 0)
                        {
                            ds1 += (dpAETERM.Data.Trim().Length >= 60 ? " - " + dpAETERM.Data.Trim().Substring(0, 60) : " - " + dpAETERM.Data.Trim());

                        }
                        ds1 += " - " + dpAEDT.UserValue();
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
        Instance inst = subject.Instances.FindByFolderOID("CMF");
        if (inst != null && inst.Active)
        {
            DataPage dpg = inst.DataPages.FindByFormOID("AE");
            if (dpg != null && dpg.Active)
            return dpg;
            else
            return null;
        }
        return null;
}
}
}

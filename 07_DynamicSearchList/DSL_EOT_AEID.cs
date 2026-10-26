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
public class DSL_EOT_AEID : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*Created: Rananjay 
      Date: 
      Custom Function: DSL_EOT_AEID
      Edit Checks: DSL_EOT_AEID
      Description: Add Dynamic Search List (AE recordPosition, AE.AETERM and AE.AESTDAT)
      */


            Medidata.Core.Objects.DynamicSearchParams DSP = (Medidata.Core.Objects.DynamicSearchParams)ThisObject;
            DataPoint dpDSL = DSP.DataPoint;
            Subject subject = dpDSL.Record.Subject;
            if (!dpDSL.Record.DataPage.Active) return null;
            string form_OID = "AE";
            string folder_OID = "AE";
            string field_AETERM_OID = "AETERM";
            string field_AESTDAT_OID = "AESTDAT";
            string field_CMINDC_OID = "TTREAS";
            string AEKeyValue = string.Empty;

            string strAE = "3";

            Medidata.Utilities.KeyValueCollection KVC = new Medidata.Utilities.KeyValueCollection();

            DataPoint dptCMINDC = dpDSL.Record.DataPoints.FindByFieldOID(field_CMINDC_OID);

            if (dptCMINDC != null && dptCMINDC.Active && string.Compare(dptCMINDC.Data, strAE, true) == 0)
            {

                DataPage dpgAE = GetAEDataPage(form_OID, folder_OID, subject);

                if (dpgAE != null)
                {
                    for (int i = 0; i < dpgAE.Records.Count; i++)
                    {
                        if (dpgAE.Records[i].RecordPosition == 0 || !dpgAE.Records[i].Active) continue;
                        DataPoint dpAETERM = dpgAE.Records[i].DataPoints.FindByFieldOID(field_AETERM_OID);
                        DataPoint dpAESTDAT = dpgAE.Records[i].DataPoints.FindByFieldOID(field_AESTDAT_OID);


                        if (dpAETERM != null && string.Compare(dpAETERM.Data.Trim(), string.Empty, true) != 0)
                        {

                            AEKeyValue = dpAETERM.Record.RecordPosition.ToString();
                            AEKeyValue += "_" + (dpAETERM.Data.Trim().Length > 80 ? dpAETERM.Data.Substring(0, 80).Trim() : dpAETERM.Data.Trim());
                            if (dpAESTDAT.Data != string.Empty)
                                AEKeyValue += "_" + dpAESTDAT.Data;

                            Medidata.Utilities.KeyValue KV1 = new Medidata.Utilities.KeyValue(AEKeyValue, AEKeyValue);
                            KVC.Add(KV1);
                        }

                    }
                }
            }
            return KVC;
        }

        DataPage GetAEDataPage(string FormOID, string FolderOID, Subject subject)
        {
            Instance inst = subject.Instances.FindByFolderOID(FolderOID);

            if (inst != null && inst.Active)
            {
                DataPage dpg = inst.DataPages.FindByFormOID(FormOID);

                if (dpg != null && dpg.Active) return dpg;
            }
            return null;
}
}
}

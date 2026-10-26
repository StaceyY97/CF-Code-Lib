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
public class CM_CMMHDSL_MH_DSL_CF : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/**************************HEADER INFORMATION******************************
        Programmer Name: Athiyappan Ganesan (04 SEP 2023)
        Custom Function: CM_CMMHDSL_MH_DSL_CF
        EditCheck : CM_CMMHDSL_MH_DSL_CF
        **************************************************************************/ 
 
        ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        Subject subject = afp.ActionDataPoint.Record.Subject;


        //Variable Declaration Start -------

        string formOid_MH = "MH", fieldOidTerm = "MHTERM", fieldOid_StartDate = "MHSTDAT", foldoid_MH = "SCRN";

        ArrayList arr = new ArrayList();

        //Variable Declaration End -------

        DataPoints dpsMHTERM = CustomFunction.FetchAllDataPointsForOIDPath(fieldOidTerm, formOid_MH, foldoid_MH, subject);


        for (int i = 0; i < dpsMHTERM.Count; i++)
        {

            DataPoint dpMHTERM = dpsMHTERM[i];
            if (dpMHTERM == null || !dpMHTERM.Active || dpMHTERM.Record.RecordPosition == 0) continue;


            DataPoint dpMHSTDAT = dpMHTERM.Record.DataPoints.FindByFieldOID(fieldOid_StartDate);

            if (dpMHSTDAT != null && string.Compare(dpMHTERM.Data.Trim(), string.Empty, true) != 0)
            {
                string ds1 = dpMHTERM.Record.RecordPosition.ToString() + " - " + (dpMHTERM.Data.Trim().Length >= 60 ? dpMHTERM.Data.Trim().Substring(0, 60) : dpMHTERM.Data.Trim());
                if (dpMHSTDAT.UserValue() != null)
                ds1 = ds1 + " - " + dpMHSTDAT.UserValue().ToString();

                if (string.Compare(ds1.Trim(), string.Empty, true) != 0)
                {
                    arr.Add(ds1.Trim().ToUpper());
                }
            }
        }
        string fieldOid = "CMMHDSL", formOid = "CM", folderOid = "CMF";
        //DSL Fields
        DataPoints dpsDSL = CustomFunction.FetchAllDataPointsForOIDPath(fieldOid, formOid, null, subject);

        SetNonConformant(dpsDSL, arr, afp);

        return null;
    }
    void SetNonConformant(DataPoints dps, ArrayList arrLst, ActionFunctionParams afp)
    {
        bool isNonConf = false;
        for (int i = 0; i < dps.Count; i++)
        {
            isNonConf = false;
            if (dps[i] == null || !dps[i].Active) continue;
            DataPoint dp = dps[i];
            if (dp != null && dp.Active)
            {
                if (string.Compare(dp.Data.Trim(), string.Empty, true) != 0 && !arrLst.Contains(dp.Data.Trim().ToUpper()))
                isNonConf = true;
                dp.SetNonConformant(isNonConf);
                CustomFunction.PerformQueryAction("The selected Medical History value has been modified on the Medical History form. Please re-select the correct event.", 1, true, true, dp, isNonConf, afp.CheckID, afp.CheckHash);

            }
        }
}
}
}

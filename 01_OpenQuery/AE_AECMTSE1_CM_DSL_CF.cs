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
public class AE_AECMTSE1_CM_DSL_CF : CustomFunctionBase
{
public object Eval(object ThisObject)
{
        /**************************HEADER INFORMATION******************************
        Programmer Name: Athiyappan Ganesan (04 SEP 2023)
        Custom Function: AE_AECMTSE1_CM_DSL_CF
        EditCheck : AE_AECMTSE1_CM_DSL_CF
        **************************************************************************/

        ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        Subject subject = afp.ActionDataPoint.Record.Subject;


        //Variable Declaration Start -------

        string formOid_CM = "CM", fieldOid_CMTRT = "CMTRT", fieldOid_StartDate = "CM1SDAT", foldoid_CM = "CMF";


        ArrayList arr = new ArrayList();

        //Variable Declaration End -------

        DataPoints dps = CustomFunction.FetchAllDataPointsForOIDPath(fieldOid_CMTRT, formOid_CM, foldoid_CM, subject);


        for (int i = 0; i < dps.Count; i++)
        {

            DataPoint dpCMTRT = dps[i];
            if (dpCMTRT == null || !dpCMTRT.Active || dpCMTRT.Record.RecordPosition == 0) continue;


            DataPoint dpDAT = dpCMTRT.Record.DataPoints.FindByFieldOID(fieldOid_StartDate);

            if (dpDAT != null && string.Compare(dpCMTRT.Data.Trim(), string.Empty, true) != 0)
            {
                string ds1 = dpCMTRT.Record.RecordPosition.ToString() + " - " + (dpCMTRT.Data.Trim().Length > 60 ? dpCMTRT.Data.Trim().Substring(0, 60) : dpCMTRT.Data.Trim());
                if (dpDAT.UserValue() != null)
                ds1 = ds1 + " - " + dpDAT.UserValue().ToString();

                if (string.Compare(ds1.Trim(), string.Empty, true) != 0)
                {
                    arr.Add(ds1.Trim().ToUpper());
                }
            }
        }
        string fieldOid = "AECMTSE1", formOid = "AE", folderOid = "CMF";
        //DSL Fields
        DataPoints dpsDSL = CustomFunction.FetchAllDataPointsForOIDPath(fieldOid, formOid, folderOid, subject);

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
                CustomFunction.PerformQueryAction("The selected Prior and Concomitant Medications value has been modified on the Prior and Concomitant Medications form, please re-select the correct data.", 1, true, true, dp, isNonConf, afp.CheckID, afp.CheckHash);
            }
        }
}
}
}

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
public class DSL_CM_CMAENOX_CROSS : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*Created: Rananjay 
       Date: 
       Custom Function: DSL_CM_CMAENOX_CROSS
       Edit Checks: DSL_CM_CMAENOX_CROSS, DSL_CM_CMAENOX_CROSS1
       Description: set CMAENO1, CMAENO2, CMAENO3, CMAENO4 and CMAENO5 field to non-conformant if the AETERM and AESTDAT changed on AE page
       */


            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpAction = afp.ActionDataPoint;
            Subject subject = dpAction.Record.Subject;
            string form_OIDAE = "AE";
            string folder_OIDAE = "AE";
            string field_AETERM_OID = "AETERM";
            string field_AESTDAT_OID = "AESTDAT";
            string[] FieldOIDsDSL = { "CMAENO1", "CMAENO2", "CMAENO3", "CMAENO4", "CMAENO5" };
            string form_OIDDSL = "CM";

            DataPage dpgAE = GetDataPage(form_OIDAE, folder_OIDAE, subject);
            ArrayList DSLValues = new ArrayList();
            DSLValues = GetDSLValues(dpgAE, field_AETERM_OID, field_AESTDAT_OID, DSLValues);

            

            for (int k = 0; k < FieldOIDsDSL.Length; k++)
            {
                DataPoints dpsEVAETRM = CustomFunction.FetchAllDataPointsForOIDPath(FieldOIDsDSL[k], form_OIDDSL, null, subject);
                for (int i = 0; i < dpsEVAETRM.Count; i++)
                {
                    if (dpsEVAETRM[i].Active && string.Compare(dpsEVAETRM[i].Data.Trim(), string.Empty, true) != 0)
                    {
                        if (!DSLValues.Contains(dpsEVAETRM[i].Data.Trim()))
                            dpsEVAETRM[i].SetNonConformant(true);
                        else
                            dpsEVAETRM[i].SetNonConformant(false);
                    }
                }
            }
            return null;
        }

        private ArrayList GetDSLValues(DataPage dpgAE, string field_AETERM_OID, string field_AESTDAT_OID, ArrayList DSLValues)
        {

            string AEKeyValue = string.Empty;
            if (dpgAE != null)
            {
                for (int i = 0; i < dpgAE.Records.Count; i++)
                {
                    if (dpgAE.Records[i].RecordPosition == 0 || !dpgAE.Records[i].Active) continue;
                    DataPoint dpAETERM = dpgAE.Records[i].DataPoints.FindByFieldOID(field_AETERM_OID);
                    DataPoint dpAESTDAT = dpgAE.Records[i].DataPoints.FindByFieldOID(field_AESTDAT_OID);

                    AEKeyValue = string.Empty;
                    if (dpAETERM != null && string.Compare(dpAETERM.Data.Trim(), string.Empty, true) != 0)
                    {

                        AEKeyValue = dpAETERM.Record.RecordPosition.ToString();
                        AEKeyValue += "_" + (dpAETERM.Data.Trim().Length > 80 ? dpAETERM.Data.Substring(0, 80).Trim() : dpAETERM.Data.Trim());
                        if (dpAESTDAT.Data != string.Empty)
                            AEKeyValue += "_" + dpAESTDAT.Data;

                        if (AEKeyValue != string.Empty)
                            DSLValues.Add(AEKeyValue);
                    }

                }
            }

            return DSLValues;
        }

        DataPage GetDataPage(string FormOID, string FolderOID, Subject subject)
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

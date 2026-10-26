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
public class EC_AutoCal_LBAGE : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
        * Created By :Qian Yang/08Jan2025
        * Custom Function :EC_AutoCal_LBAGE
        * Description:Calculate age of collection
        * Edit Checks :AutoCal_CHEM2AGE,AutoCal_COAG2AGE,AutoCal_LPDAGE2,AutoCal_HEMA2AGE,AutoCal_HBA1CAGE2,AutoCal_HORMAGE2,AutoCal_URIN2AGE,AutoCal_BIRTHDAT
        *********************************************************************************************************/

            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpAction = afp.ActionDataPoint;
            Subject sj = dpAction.Record.Subject;
            DataPage currentPage = dpAction.Record.DataPage;
                        
            string Age = "";
            int year = 0, yearLB = 0;

            if (dpAction.Field.OID == "BIRTHDAT")
            {
                Instances allINS = sj.Instances;
                foreach (Instance inst in allINS)
                {
                    if (inst == null || inst.Folder.OID != "UNA") continue;
                    foreach (string formOID in manageForms.Keys)
                    {
                        DataPage managePage = inst.DataPages.FindByFormOID(formOID);
                        if (managePage == null) continue;
                        DataPoint enterDP = managePage.MasterRecord.DataPoints.FindByFieldOID(manageForms[formOID][1]);
                        if (enterDP != null)
                        {
                            DataPoint check_DP = managePage.MasterRecord.DataPoints.FindByFieldOID(manageForms[formOID][0]);
                            if (check_DP != null && check_DP.Active && check_DP.Data != string.Empty&& dpAction.Data != string.Empty && int.TryParse(dpAction.Data, out year))
                            {
                                yearLB = getLBYear(check_DP);
                                Age = (yearLB - year).ToString();
                            }
                            EnterData(enterDP, Age);
                        }

                    }
                }

            }
            else
            {
                if (manageForms.ContainsKey(currentPage.Form.OID))
                {
                    DataPoint enterDP = dpAction.Record.DataPage.MasterRecord.DataPoints.FindByFieldOID(manageForms[currentPage.Form.OID][1]);
                    if (enterDP != null)
                    {
                        DataPoint birth = (DataPoint)CustomFunction.PerformCustomFunction("CF_GetDataPoint", dpAction.Field.CRFVersion.ID, new object[] { sj, "", "SCREEN_1", "DM", "BIRTHDAT" });
                        if (birth != null && birth.Active && birth.Data != string.Empty && int.TryParse(birth.Data, out year)&& dpAction.Data!=string.Empty)
                        {
                            yearLB = getLBYear(dpAction);
                            Age = (yearLB - year).ToString();
                        }
                        EnterData(enterDP, Age);
                    }
                }
            }
            return null;
        }

        Dictionary<string, List<string>> manageForms = new Dictionary<string, List<string>>()
        {
            { "HEMA2", new List<string> { "HEMA2DAT", "HEMA2AGE" } },
            { "CHEM2", new List<string> { "CHEM2DAT", "CHEM2AGE" } },
            { "URIN2", new List<string> { "URIN2DAT", "URIN2AGE" } },
            { "LPD2", new List<string> { "LPDDAT2", "LPDAGE2" } },
            { "COAG2", new List<string> { "COAG2DAT", "COAG2AGE" } },
            { "HORM2", new List<string> { "HORM2DAT", "HORMAGE2" } },
            { "HBA1C2", new List<string> { "HBA1C2DAT", "HBA1CAGE2" } }
        };

        int getLBYear(DataPoint LBDAT)
        {
            if(LBDAT!=null&&LBDAT.StandardValue() is DateTime)
            {
                DateTime dt = (DateTime)LBDAT.StandardValue();
                return dt.Year;
            }
            return 0;
        }

        public void EnterData(DataPoint dp, string val)
        {
            if (dp != null && dp.Active && !dp.IsDataPointLocked && string.Compare(dp.Data, val, true) != 0)
            {
                dp.UnFreeze();
                dp.Enter(val, string.Empty, 0);
            }
}
}
}

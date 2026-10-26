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
public class AUTO_ACTNMDER : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:**************************
            * CF Name: AUTO_ACTNMDER
            * Programmer Name:YX(18 Nov 2024)
            * Last Modified: Not Applicable
            * Reason Modified: Not Applicable
            * Description: Derive the value of ACTNMDER**/

            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dp = afp.ActionDataPoint;
            Subject subject = dp.Record.Subject;
            string field_oid = "";
            if (dp.Field.OID == "CANTYPE")
            {
                field_oid = dp.Data == "1" ? "ACTNAME" : (dp.Data == "2" ? "ACTNAME2" : (dp.Data == "3" ? "ACTNAME3" : ""));
                DataPage dpage_ACT = dp.Record.DataPage.Instance.DataPages.FindByFormOID("ACT");
                if (dpage_ACT != null && dpage_ACT.Active)
                {
                    foreach (Record rd in dpage_ACT.Records)
                    {
                        if (rd.Active)
                        {
                            Derive(rd, field_oid);
                        }
                    }
                }
            }
            else
            {
                DataPoints dps_CANTYPE = CustomFunction.FetchAllDataPointsForOIDPath("CANTYPE", "DSENR", "SCRN", subject);
                if (dps_CANTYPE != null && dps_CANTYPE.Count > 0 && dps_CANTYPE[0].Active)
                {
                    field_oid = dps_CANTYPE[0].Data == "1" ? "ACTNAME" : (dps_CANTYPE[0].Data == "2" ? "ACTNAME2" : (dps_CANTYPE[0].Data == "3" ? "ACTNAME3" : ""));
                }
                Derive(dp.Record, field_oid);
            }

            return null;
        }

        void EnterData(DataPoint dp, string val, bool doFreeze)
        {
            if (dp != null && dp.Active && string.Compare(dp.Data.Trim(), val, true) != 0)
            {
                dp.Unlock();
                dp.UnFreeze();
                if (dp.LockDisplayStatus == LockDisplayStatusEnum.Unlocked)
                {
                    dp.Enter(val, string.Empty, 0);
                    if (doFreeze) dp.Freeze();
                }
            }
        }

        void Derive(Record rd, string field_oid)
        {
            DataPoint dp_OTHER = rd.DataPoints.FindByFieldOID("ACTNAMES");
            DataPoint dp_ACTNMDER = rd.DataPoints.FindByFieldOID("ACTNMDER");
            if (dp_ACTNMDER != null && dp_ACTNMDER.Active && dp_ACTNMDER.IsVisible)
            {
                if (field_oid != "")
                {
                    DataPoint dp_ACTNAME = rd.DataPoints.FindByFieldOID(field_oid);
                    if (dp_ACTNAME != null && dp_ACTNAME.Active && dp_ACTNAME.IsVisible && dp_ACTNAME.Data != "")
                    {
                        if (dp_ACTNAME.Data == "99")
                        {
                            if (dp_OTHER != null && dp_OTHER.Active && dp_OTHER.IsVisible)
                            {
                                EnterData(dp_ACTNMDER, dp_OTHER.Data.ToString(), true);
                            }
                            else EnterData(dp_ACTNMDER, "", true);
                        }
                        else EnterData(dp_ACTNMDER, dp_ACTNAME.UserValue().ToString(), true);
                    }
                    else
                    {
                        EnterData(dp_ACTNMDER, "", true);
                    }
                }
                else EnterData(dp_ACTNMDER, "", true);
            }
}
}
}

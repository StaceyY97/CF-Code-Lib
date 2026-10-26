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
public class DYN_EGFR : CustomFunctionBase
{
public object Eval(object ThisObject)
{
///<summary>
            ///CF: DYN_EGFR _Qian Yang 24Aug2026
            ///add LBCHEM.EGFR field to corresponding LBCHEM form by value of cohort and current version
            ///</summary>
            ///

            IDataPoint dpAction = Rave.Context.ActionDataPoint;
            if (dpAction == null) return null;

            ISubject curSub = dpAction.Record.Subject;

            List<string> cohorts = new List<string> { "2D", "2E", "3A", "3B" };

            IReadOnlyList<IDataPoint> Dps_LBPERF = curSub.GetDataPointsByOidPath("LBPERF", "LBCHEM", null);

            IDataPoint Dp_COHORT = GetSimpleDP("COHORT", "ENROLL", "SCR");

            if (cohorts.Contains(Dp_COHORT?.Data ?? ""))
            {
                for (int i = 0; i < Dps_LBPERF.Count; i++)
                {
                    IDataPoint Dp_LBPERF = Dps_LBPERF[i];
                    if (Dp_LBPERF != null)
                    {
                        bool shouldVisible = false;
                        if (Dp_LBPERF.Data.Trim() == "Y")
                        {
                            IInstance curIns = Dp_LBPERF.Record.Instance;
                            IDataPoint dp_VISPV = GetSimpleDP_sameIns("VISPV", "VISIT", curIns);
                            if (curIns.Folder.OID == "SCR") shouldVisible = dp_VISPV?.Data == "V6";
                            else shouldVisible = LessThan_V6(dp_VISPV);
                        }
                        IDataPoint Dp_EGFR = Dp_LBPERF.Record.AllDataPoints.FindByFieldOid("EGFR");
                        if (Dp_EGFR != null) Dp_EGFR.IsVisibleRaw = shouldVisible;
                    }
                }
            }
            IDataPoint GetSimpleDP(string fieldOID, string formOID, string folderOID) =>
curSub?.Instances?.FindByFolderOid(folderOID)?.DataPages?.FindByFormOid(formOID)?.MasterRecord?.DataPoints?.FindByFieldOid(fieldOID);
            IDataPoint GetSimpleDP_sameIns(string fieldOID, string formOID, IInstance ins) => ins?.DataPages?.FindByFormOid(formOID)?.MasterRecord?.DataPoints?.FindByFieldOid(fieldOID);
            bool LessThan_V6(IDataPoint dp) => int.TryParse(dp?.Data, out int vNum) && vNum != 0 && vNum < 6;
            return null;
}
}
}

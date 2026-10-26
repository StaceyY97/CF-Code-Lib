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
public class DYN_Add_C4D1_Q4W : CustomFunctionBase
{
public object Eval(object ThisObject)
{
///<summary>
            ///CF: DYN_Add_C4D1_Q4W _Qian Yang 19Aug2026
            ///Add C4D1 by C3D15 or C3D22 by value from curretn version
            ///</summary>

            string InsOID_Parent = "TRT", targetFolder = "C4D1";
            string[] forms_Common = new string[] { "VISIT", "VISNX", "EX", "EXBEV" };
            List<string> cohorts = new List<string> { "2D", "3A" };

            string str_doAdd_trigger = "Y";

            IDataPoint enterDP = Rave.Context.ActionDataPoint;
            if (enterDP == null) return null;

            ISubject curSub = enterDP.Record.Subject;

            bool doAdd = false;
            IDataPoint dp_Cohort = GetSimpleDP("COHORT", "ENROLL", "SCR");
            if (cohorts.Contains(dp_Cohort?.Data))
            {
                IDataPoint dp_VISPV_15 = GetSimpleDP_parent("VISPV", "VISIT", "C3D15", "TRT");
                IDataPoint dp_VISNX_15 = GetSimpleDP_parent("DSCONT", "VISNX", "C3D15", "TRT");
                if ((LessThan_V7(dp_VISPV_15) && dp_VISNX_15?.Data == "Y")) doAdd = true;

                if (!doAdd)
                {
                    IDataPoint dp_VISPV_22 = GetSimpleDP_parent("VISPV", "VISIT", "C3D22", "TRT");
                    IDataPoint dp_VISNX_22 = GetSimpleDP_parent("DSCONT", "VISNX", "C3D22", "TRT");

                    if ((GreaterThanOrEqualTo_V7(dp_VISPV_22) && dp_VISNX_22?.Data == "Y")) doAdd = true;
                }
            }
            Rave.PerformCustomFunction("CF_Aid_AddnextCycle", curSub.CrfVersionRef, new object[] { curSub,((DataPoint)enterDP).Record.SubjectMatrixID, InsOID_Parent, targetFolder, doAdd, forms_Common});

            IDataPoint GetSimpleDP(string fieldOID, string formOID, string folderOID) =>
curSub?.Instances?.FindByFolderOid(folderOID)?.DataPages?.FindByFormOid(formOID)?.MasterRecord?.DataPoints?.FindByFieldOid(fieldOID);
            IDataPoint GetSimpleDP_parent(string fieldOID, string formOID, string folderOID, string parent) =>
            curSub?.Instances?.FindByFolderOid(parent)?.Instances?.FindByFolderOid(folderOID)?.DataPages?.FindByFormOid(formOID)?.MasterRecord?.DataPoints?.FindByFieldOid(fieldOID);

            bool LessThan_V7(IDataPoint dp) => int.TryParse(dp?.Data.Replace("V", ""), out int vNum) && vNum != 0 && vNum < 7;
            bool GreaterThanOrEqualTo_V7(IDataPoint dp) => int.TryParse(dp?.Data.Replace("V", ""), out int vNum) && vNum >= 7;

            return null;
}
}
}

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
public class EC_SwitchMatrix_Cn_n_5 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:**************************
* CF Name: EC_SwitchMatrix_Cn(n>=5)
* Programmer Name:Qian Yang 02Sep2025
* Last Modified: Not Applicable
* Reason Modified: Not Applicable
* Description: Switch Matrix based on part and cycles of status 
* **/

       ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
       DataPoint ActionDP = afp.ActionDataPoint;
       subject = ActionDP.Record.Subject;

       DataPoint GetSimpleDP(string fieldOID, string formOID, string folderOID) =>
           subject.Instances.FindByFolderOID(folderOID)?.DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints.FindByFieldOID(fieldOID);

       DataPoints statusCollect = new DataPoints();
       if (ActionDP.Field.OID == "STATUS") statusCollect.Add(ActionDP);
       else statusCollect = CustomFunction.FetchAllDataPointsForOIDPath("STATUS", "STATUS", null, subject);

       foreach (DataPoint status in statusCollect)
       {
           if (status == null||!status.Active) continue;

           //initial parameters for every status to avoid re-use of last value
           matrix_D1 = "";
           matrix_D15 = "";
           matrix_STATUS = "";
           status_D1 = null;
           status_D15 = null;

           //get parameters for current status
           getParameters();

           if (matrix_D1 == "" || matrix_D15 == "" || matrix_STATUS == "" || (status_D1 == null && status_D15 == null)) continue;

           part = GetSimpleDP("STPART", "DSENR", "SCRN");

           //do merge and unmerge
           mergeMatrix(matrix_D1, matrix_D15, matrix_STATUS, status_D1, status_D15);

           void getParameters()
           {
               string currentIns = status?.Record.Instance?.Folder.OID;
               if (int.TryParse(currentIns?.Length >= 6 ? currentIns.Substring(1, 2) : "null", out int cycleNum))
               {
                   if (cycleNum >= 4)
                   {
                       string cycleNumber_Prior = cycleNum < 10 ? "0" + cycleNum.ToString() : cycleNum.ToString();
                       string cycleNumber_Current = (cycleNum + 1) < 10 ? "0" + (cycleNum + 1).ToString() : ((cycleNum + 1)).ToString();

                       matrix_D1 = "CYCLE" + (cycleNum + 1).ToString();
                       matrix_D15 = "C" + cycleNumber_Current + "2B";
                       matrix_STATUS = "CYCLE" + (cycleNum + 1).ToString() + "DAY1";

                       status_D1 = GetSimpleDP("STATUS", "STATUS", "C" + cycleNumber_Prior + "D01");
                       status_D15 = GetSimpleDP("STATUS", "STATUS", "C" + cycleNumber_Prior + "D15");
                   }
               }
               ;
           }
       }

       return null;
   }

   Subject subject;
   DataPoint part = null, status_D1 = null, status_D15 = null;
   string matrix_D1 = "", matrix_D15 = "", matrix_STATUS = "";


   void mergeMatrix(string matrix_D1, string matrix_D15, string matrix_STATUS, DataPoint status_D1, DataPoint status_D15)
   {
       if (matrix_D1 != "" && matrix_D15 != "" && matrix_STATUS != "" && (status_D1 != null || status_D15 != null) && part != null)
       {
           string[] allManageMatrix = new string[] { matrix_D1, matrix_D15, matrix_STATUS };
           string[] matrixMatch = new string[] { };

           bool isMeet_D1 = status_D1?.Data == "Y" || (matrix_D15.Substring(1, 2) == "05" ? status_D15?.Data == "Y" : false);
           bool isMeet_D15 = status_D15?.Data == "Y";

           switch (part?.Data)
           {
               case "1"://Part 1A
                   if (isMeet_D1) matrixMatch = new string[] { matrix_D1, matrix_STATUS };
                   break;
               case "2"://Part 2A
                   if (isMeet_D1) matrixMatch = new string[] { matrix_D1, matrix_STATUS };
                   break;
               case "3"://Part 1B
                   if (isMeet_D1) matrixMatch = new string[] { matrix_D1, matrix_STATUS };
                   break;
               case "4"://Part 2B
                   if (isMeet_D15) matrixMatch = new string[] { matrix_D1, matrix_D15 };
                   break;
case "5"://Part 1C
                   if (isMeet_D1) matrixMatch = new string[] { matrix_D1, matrix_STATUS };
                   break;
               default:
                   break;
           }

           foreach (string matrixOID in matrixMatch)
           {
               Matrix manageMatrix = Matrix.FetchByOID(matrixOID, subject.CRFVersion.ID);
               if (manageMatrix != null) subject.MergeMatrix(manageMatrix);
           }

           foreach (string matrixOID in allManageMatrix)
           {
               if (!matrixMatch.Contains(matrixOID))
               {
                   Matrix manageMatrix = Matrix.FetchByOID(matrixOID, subject.CRFVersion.ID);
                   if (manageMatrix != null) subject.UnMergeMatrix(manageMatrix);
               }
           }
       }
}
}
}

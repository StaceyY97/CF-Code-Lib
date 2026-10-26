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
public class AddForm_RNA_TIMEPOINT : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:**************************
* Programmer Name:YueXu 14Nov2024/Qian Yang 16Jun2025/Qian Yang 02Sep2025
* Last Modified: 02Sep2025
* Description: Add form and set default timepoints.
* **/

       ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
       DataPoint dpAction = afp.ActionDataPoint;
       Subject subject = dpAction.Record.Subject;

       /**Following are parameters should be be configured based the study requirement**/
       ///Note: Mapping logic: Visit OID-Part codelist- Timepoints list
       string formOid = "RNA";//The form timepoints to be added
       string fieldOid_TimePoint = "RNATPT";//The timepoint field OID
       string partFieldOID = "STPART", partFormOID = "DSENR", partFolderOID = "SCRN";

       //Define timepoints lists first, please list all possible combinations
       string[] TPT1 = { "PREDOSE", "4HR POSTDOSE" };
       string[] TPT2 = { "PREDOSE" };
       string[] TPT3 = { "END OF TREATMENT" };
       string[] TPT4 = { "4HR POSTDOSE"};

       //Dicname: Visit Name; Key: Allowed Part codelist values; Value: Timepoints list defined above
       List<string> AllPartCode = (List<string>)CustomFunction.PerformCustomFunction("CF_Aid_AllPartCode", dpAction.Field.CRFVersion.ID, new object[] { null });

       Dictionary<List<string>, string[]> map_C1D1 = new Dictionary<List<string>, string[]>
   {
       { new List<string> { "1" }, TPT1 },
       { new List<string> { "2","4" },  TPT2 },
       { new List<string> { "3","5" },  TPT1 }
   };
       Dictionary<List<string>, string[]> map_C1D15 = new Dictionary<List<string>, string[]>
   {
       { new List<string> { "1","2","3","4","5"},  TPT2 }
   };
       Dictionary<List<string>, string[]> map_Others = new Dictionary<List<string>, string[]>
   {
       { AllPartCode,  TPT2 }
   };
       Dictionary<List<string>, string[]> map_EOT = new Dictionary<List<string>, string[]>
   {
       { AllPartCode,  TPT3 }
   };

       //Key: Folder OID; Value: Map dictionary. Do not change the Dic Name
       Dictionary<string, Dictionary<List<string>, string[]>> FolderOID_WT_Condition = new Dictionary<string, Dictionary<List<string>, string[]>>()
  {
      { "C01D01", map_C1D1},
      { "C01D02", map_Others},
      { "C01D15", map_C1D15},
      { "C03D01", map_Others},
      { "EOT", map_EOT}
  };
       /**Below are parameters should be be configured based the study requirement **/

       ///Main body. Do not change the code below
       DataPoint PART;
       Instance curIns;

       if (dpAction.Field.OID == partFieldOID)
       {
           PART = dpAction;
           foreach (string key in FolderOID_WT_Condition.Keys)
           {
               curIns = subject.Instances.FindByFolderOID(key);
               if (curIns != null)
               {
                   object ManageTPT = (object)CustomFunction.PerformCustomFunction("CF_Aid_ManageTPT", dpAction.Field.CRFVersion.ID, new object[] { subject, curIns, FolderOID_WT_Condition[key], PART, formOid, fieldOid_TimePoint});
               }
           }
       }
       else if (FolderOID_WT_Condition.ContainsKey(dpAction.Record.Instance?.Folder.OID))
       {
           PART = GetSimpleDP(partFieldOID, partFormOID, partFolderOID);
           curIns = dpAction.Record.Instance;
           object ManageTPT = (object)CustomFunction.PerformCustomFunction("CF_Aid_ManageTPT", dpAction.Field.CRFVersion.ID, new object[] { subject, curIns, FolderOID_WT_Condition[curIns.Folder.OID], PART, formOid, fieldOid_TimePoint});
       }

       DataPoint GetSimpleDP(string fieldOID, string formOID, string folderOID) => subject.Instances.FindByFolderOID(folderOID)?.DataPages.FindByFormOID(formOID)?.MasterRecord.DataPoints.FindByFieldOID(fieldOID);
       return null;
}
}
}

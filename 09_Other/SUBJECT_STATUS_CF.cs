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
public class SUBJECT_STATUS_CF : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*Custom Function: SUBJECT_STATUS_CF
Description: Set the subject status as indicated below:

1. If SUBJECT is not empty, then set subject status to "Screening - '1'"
2. If At least one of fields (IEYN. IEYN1, IEYN2), and none of field equals Y then set subject status to "Screen Failed - '2'"
3. If DSENR.DSDAT is not empty, set subject status to "Enrolled - '3'"
4. If EX.EXSTDAT is not empty, then set subject status to "On-Treatment - '10'"
5. If DSCOMP1.DSDECOD1 is not null, then set subject status to "Off-Treatment - '11'"
6. If DSCOMP1.DSCONT = Y, then set subject status to "Follow-up - '7'"
7. If EOS.EOSRSN is not empty, then set subject status to "End of Study - '12'"*/

   ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
   DataPoint dpaction = afp.ActionDataPoint;
   Subject subject = dpaction.Record.Subject;
   bool flagEX = false;
   bool flagIE = false;

   DataPoints dpsSUBJECT = CustomFunction.FetchAllDataPointsForOIDPath("SUBJECT", "SUBJECT", null, subject);
   DataPoints dpsIEYN = CustomFunction.FetchAllDataPointsForOIDPath("IEYN", "IE", "SCRN", subject);
   DataPoints dpsIEYN1 = CustomFunction.FetchAllDataPointsForOIDPath("IEYN1", "IE", "SCRN", subject);
   DataPoints dpsIEYN2 = CustomFunction.FetchAllDataPointsForOIDPath("IEYN2", "IE", "SCRN", subject);
   DataPoints dpsEXSTDAT = CustomFunction.FetchAllDataPointsForOIDPath("EXSTDAT", "EX", "SDA", subject);
   DataPoints dpsDSDECOD1 = CustomFunction.FetchAllDataPointsForOIDPath("DSDECOD1", "DSCOMP1", "EOT", subject);
   DataPoints dpsDSRAND = CustomFunction.FetchAllDataPointsForOIDPath("DSDAT","DSENR", "SCRN", subject);
   DataPoints dpsDSCONT = CustomFunction.FetchAllDataPointsForOIDPath("DSCONT", "DSCOMP1", "EOT", subject);
   DataPoints dpsEOSRSN = CustomFunction.FetchAllDataPointsForOIDPath("EOSRSN", "EOS", "EOS", subject);

   if (dpsEXSTDAT.Count > 0)
   {
       for (int i = 0; i < dpsEXSTDAT.Count; i++)
       {
           if (dpsEXSTDAT[i] != null && dpsEXSTDAT[i].Active && dpsEXSTDAT[i].StandardValue() is DateTime)
           {
               flagEX = true;
               break;
           }
       }
   }

   if (dpsIEYN.Count > 0 && dpsIEYN[0] != null && dpsIEYN1.Count > 0 && dpsIEYN1[0] != null && dpsIEYN2.Count > 0 && dpsIEYN2[0] != null)
   {
       flagIE = (dpsIEYN[0].Data != "" || dpsIEYN1[0].Data != "" || dpsIEYN2[0].Data != "") && (dpsIEYN[0].Data != "Y" && dpsIEYN1[0].Data != "Y" && dpsIEYN2[0].Data != "Y");
   }



   if (dpsEOSRSN.Count > 0 && dpsEOSRSN[0] != null && dpsEOSRSN[0].Active && dpsEOSRSN[0].Data != string.Empty)
   {
       subject.SubjectStatus = 12;
       //End of Study
   }

   else if (dpsDSCONT.Count > 0 && dpsDSCONT[0] != null && dpsDSCONT[0].Active && dpsDSCONT[0].Data == "Y")
   {
       subject.SubjectStatus = 7;
       //In-Safety Follow-up
   }
   else if (dpsDSDECOD1.Count > 0 && dpsDSDECOD1[0] != null && dpsDSDECOD1[0].Active && dpsDSDECOD1[0].Data != string.Empty)
   {
       subject.SubjectStatus = 11;
       //Off - Treatment
   }
   else if (flagEX)
   {
       subject.SubjectStatus = 10;
       //On - Treatment
   }
   else if (dpsDSRAND.Count > 0 && dpsDSRAND[0] != null && dpsDSRAND[0].Active && dpsDSRAND[0].StandardValue() is DateTime)
   {
       subject.SubjectStatus = 3;
       //Enrolled
   }
   else if (flagIE )
   {
       subject.SubjectStatus = 2;
       //Screen Failure
   }
   else if (dpsSUBJECT.Count > 0 && dpsSUBJECT[0] != null && dpsSUBJECT[0].Active && dpsSUBJECT[0].Data != string.Empty)
   {
       subject.SubjectStatus = 1;
       //Screened
   }
   else
   {
       subject.SubjectStatus = 1;
       //Entered- Default
   }

   return null;
}
}
}

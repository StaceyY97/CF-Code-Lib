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
public class EC_checkDP_EqualTo_VISDAT : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
        * Created By :Qian Yang/10Jan2025
        * Custom Function :EC_checkDP_EqualTo_VISDAT
        * Description:Check is date of assessment equal to date of visit
        * Edit Checks : DateComp_BIO1DAT(^=VISDAT),DateComp_CHEM1DAT(^=VISDAT),DateComp_EGDAT(^=VISDAT),DateComp_LBDAT(^=VISDAT),DateComp_HEMA1DAT(^=VISDAT),DateComp_HORM1DAT(^=VISDAT),DateComp_SERDAT(^=VISDAT),DataComp_URIN1DAT(^=VISDAT),DateComp_PEDAT(^=VISDAT),DateComp_PK3DAT(^=VISDAT),DateComp_OADAT1(^=VISDAT)
        *********************************************************************************************************/

            DataPoint dpAction = (DataPoint)ThisObject;
            Instance currentFolder = dpAction.Record.Instance;

            string folderOID_comment = "VIS", fieldOID_comment = "VISDAT", folderOID_W1216 = "VIS2", fieldOID_W1216 = "VIS2DAT";
            bool openQuery = false;

            if (currentFolder.Folder.OID == "SCREEN_1" || currentFolder.Folder.OID == "RESCREEN_1"||!(dpAction.StandardValue() is DateTime)) return openQuery;


            string checkForm = (currentFolder.Folder.OID == "WK12" || currentFolder.Folder.OID == "WK16") ? folderOID_W1216 : folderOID_comment;
            string checkField = (currentFolder.Folder.OID == "WK12" || currentFolder.Folder.OID == "WK16") ? fieldOID_W1216 : fieldOID_comment;
            DataPage VIS = currentFolder.DataPages.FindByFormOID(checkForm);
            if (VIS != null&& VIS.Active) 
            {
                DataPoint VISDAT = VIS.MasterRecord.DataPoints.FindByFieldOID(checkField);
                if (VISDAT != null&& VISDAT.Active&&VISDAT.StandardValue() is DateTime)
                {
                    openQuery = !((DateTime)dpAction.StandardValue() == (DateTime)VISDAT.StandardValue());
                }
            }

            return openQuery;
}
}
}

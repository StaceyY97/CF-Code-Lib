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
public class TUTL_027 : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/*****************************************************************
          * Created By : MXZ/16Jan2024
          * Custom Function : TUTL_027
          * Edit Checks : TUTL_027
          * Description : Default Lesion status to "Evaluated" at Screening Visit

          *********************************************************************************************************/
            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dpAction = afp.ActionDataPoint;
            Subject subject = dpAction.Record.Subject;
            string formOid = "TUTL";
            string fieldOid = "TUSTAT";


            string[] strFolder =
            {
           "SCR"
        }
            ;


            ArrayList arFolders = new ArrayList(strFolder);

            Instance inst = dpAction.Record.Instance;

            if (inst == null || (inst != null && !arFolders.Contains(inst.Folder.OID))) return null;

            DataPage dpg = inst.DataPages.FindByFormOID(formOid);

            if (dpg != null && dpg.Active)
            {
                Records rcds = dpg.Records;
                for (int j = 0; j < rcds.Count; j++)
                {
                    if (!rcds[j].Active || rcds[j].RecordPosition == 0) continue;


                    DataPoint dpt = rcds[j].DataPoints.FindByFieldOID(fieldOid);

                    EnterData(dpt, "E");
                }
            }
            return null;

        }

        void EnterData(DataPoint dp, string val)
        {
            if (dp != null && dp.Active)
            {
                if (dp.LockStatus != LockStatusEnum.Locked)
                {
                    dp.Enter(val, string.Empty, 0);
                    dp.Freeze();
                }
            }
}
}
}

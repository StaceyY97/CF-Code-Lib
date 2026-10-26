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
public class DYN_TUTL_SETVISIBLE : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:**************************
           * CF Name: DYN_TUTL_SETVISIBLE
           * EC: DYN_TUTL_SETVISIBLE
           * Programmer Name: Rananjay
           * Last Modified:
           * Reason Modified:
           * Description:  **/
            ActionFunctionParams afp = (ActionFunctionParams)ThisObject;
            DataPoint dptTUSTAT = afp.ActionDataPoint;
            Subject sub = dptTUSTAT.Record.Subject;
            Instance inst = dptTUSTAT.Record.DataPage.Instance;
            if (inst == null || (inst.Folder.OID == "SCR")) return null;

            string strVal = dptTUSTAT.Data;
            //Coalesced
            string[] Fields1 =
                {
            "COAL1"
        }
            ;
            //SPLIT
            string[] Fields2 =
        {
            "SP1LTD", "SP1TSTM", "SP1TSTMD", "SP2LTD", "SP2TSTM", "SP2TSTMD","SP3LTD", "SP3TSTM", "SP3TSTMD"
        }
            ;
            //Too small to measure
            string[] Fields3 =
        {
            "TSTMD"
        }
            ;

            Makedpvisible(dptTUSTAT, Fields1, strVal == "C");
            Makedpvisible(dptTUSTAT, Fields2, strVal == "S");
            Makedpvisible(dptTUSTAT, Fields3, strVal == "T");
            


            return null;
        }
        private void Makedpvisible(DataPoint dpTUSTAT, string[] fields, bool FlagCheck1)
        {

            if (FlagCheck1 == true)
            {

                Makedpvisible1(dpTUSTAT, fields, true);
            }

            else if (FlagCheck1 == false && IsPresent(dpTUSTAT, fields))
            {
                Makedpvisible1(dpTUSTAT, fields, false);
            }
        }
        private void Makedpvisible1(DataPoint dpTUSTAT, string[] fields, bool FlagCheck1)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                DataPoint Dpt = dpTUSTAT.Record.DataPoints.FindByFieldOID(fields[i]);
                if (Dpt != null)
                {

                    Dpt.IsVisible = FlagCheck1;
                }
            }
        }
        bool IsPresent(DataPoint dpTUSTAT, string[] fields)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                DataPoint Dpt = dpTUSTAT.Record.DataPoints.FindByFieldOID(fields[i]);
                if (Dpt != null)
                {
                    if (Dpt.Field.ControlType.ToUpper() == "CHECKBOX")
                    {
                        if (Dpt.Data == "1") return false;
                    }
                    else
                    {
                        if (Dpt.Data != string.Empty)
                            return false;
                    }
                }
            }
            return true;
}
}
}

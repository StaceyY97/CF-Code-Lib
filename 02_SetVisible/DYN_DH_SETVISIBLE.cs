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
public class DYN_DH_SETVISIBLE : CustomFunctionBase
{
public object Eval(object ThisObject)
{
/** HEADER INFORMATION:**************************
        * CF Name: DYN_DH_SETVISIBLE
        * EC: DYN_DH_SETVISIBLE
        * Programmer Name: MXZ
        * Last Modified:
        * Reason Modified:
        * Description: **/
        ActionFunctionParams afp = (ActionFunctionParams) ThisObject;
        DataPoint dptDHTYPE = afp.ActionDataPoint;
        Subject sub = dptDHTYPE.Record.Subject;
        Instance inst = dptDHTYPE.Record.DataPage.Instance;
        if (inst == null || inst.Folder.OID != "SCR") return null;
        string strVal = dptDHTYPE.Data;
        //Breast Cancer
        string[] Fields1 =
        {
            "BCER", "BCPR", "BCHER2"
        }
        ;
        //Non-Small Cell Lung Cancer (NSCLC)
        string[] Fields2 =
        {
            "NSCLCHIST", "NSCLSOTH", "NSCLCGENMUT", "NSCLCEGFR", "EGFRTYPE", "EGFROTH", "NSCLCALK", "NSCLCROS1", "NSCLCBRAF", "NSCLCKRAS", "NSCLCMET", "NSCLCRET", "NSCLCNTRK", "NSCLCOTH"
        }
        ;
        //Colorectal Cancer (CRC)
        string[] Fields3 =
        {
            "CRCKRAS", "KRASTYPE", "MSI", "MMR", "CRCLOC", "CRCTP53", "KRASTYPOTH"
        }
        ;
        //Ovarian Cancer
        string[] Fields4 =
        {
            "OVLOC", "OVLOCOTH", "OVHISTYPE", "OVHISOTH"
        }
        ;
        //Head and Neck Squamous Cell Carcinoma
        string[] Fields5 =
        {
            "HNSCCLOC", "LOCOTH"
        }
        ;
        //Esophageal Cancer
        string[] Fields8 =
        {
            "ESPHER2","ESPDL"
        }
        ;
        //Gastric Cancer/Gastroesophageal Junction (GEJ) Carcinoma
        string[] Fields7 =
        {
            "GCTTYPE", "GCTTYPEOTH", "GCHTYPE", "GCSTA", "GCHER", "GCPDL", "GCCLDN"
        }
        ;
        //Endometrial Cancer
        string[] Fields9 =
        {
            "ECHTYPE", "ECGRAD", "MMSTS", "FIGOSTS", "PDLSTS"
        }
        ;
        //Renal Cell Cancer
        string[] Fields11 =
        {
            "HISTYPE1", "HISTYPE1OTH", "TP53"
        }
        ;
//Pancreatic Cancer
string[] Fields12 =
        {
            "MUTTYPE", "MUTTYPEOTH", "PANMSI","PANMMR"
        }
        ;
//Biliary Tract Cancer
string[] Fields13 =
        {
            "BTLOC", "BTHER2", "BTMSI","BTMMR","BTFGFR","BTIDH1","BTIDH2"
        }
        ;


        Makedpvisible(dptDHTYPE, Fields1, strVal == "2");
        Makedpvisible(dptDHTYPE, Fields2, strVal == "9");
        Makedpvisible(dptDHTYPE, Fields3, strVal == "4");
        Makedpvisible(dptDHTYPE, Fields4, strVal == "10");
        Makedpvisible(dptDHTYPE, Fields5, strVal == "8");
        Makedpvisible(dptDHTYPE, Fields7, strVal == "7");
        Makedpvisible(dptDHTYPE, Fields8, strVal == "6");
        Makedpvisible(dptDHTYPE, Fields11, strVal == "12");
        Makedpvisible(dptDHTYPE, Fields9, strVal == "5");
Makedpvisible(dptDHTYPE, Fields12, strVal == "11");
Makedpvisible(dptDHTYPE, Fields13, strVal == "14");


        return null;
    }
    private void Makedpvisible(DataPoint dpDHTYPE, string[] fields, bool FlagCheck1)
    {

        if (FlagCheck1 == true)
        {

            Makedpvisible1(dpDHTYPE, fields, true);
        }

        else if(FlagCheck1 == false && IsPresent(dpDHTYPE, fields))
        {
            Makedpvisible1(dpDHTYPE, fields, false);
        }
    }
    private void Makedpvisible1(DataPoint dpDHTYPE, string[] fields, bool FlagCheck1)
    {
        for (int i = 0; i < fields.Length; i++)
        {
            DataPoint Dpt = dpDHTYPE.Record.DataPoints.FindByFieldOID(fields[i]);
            if (Dpt != null)
            {

                Dpt.IsVisible = FlagCheck1;
            }
        }
    }
    bool IsPresent(DataPoint dpDHTYPE, string[] fields)
    {
        for (int i = 0; i < fields.Length; i++)
        {
            DataPoint Dpt = dpDHTYPE.Record.DataPoints.FindByFieldOID(fields[i]);
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

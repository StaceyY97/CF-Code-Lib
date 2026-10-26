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
public class EC_DSL_PDWK : CustomFunctionBase
{
public object Eval(object ThisObject)
{
DataPoint dp = ((DynamicSearchParams)ThisObject).DataPoint;
    Subject subj = dp.Record.Subject;

    KeyValueCollection kvcList = new KeyValueCollection();

    Dictionary<string, List<int>> weekDayDict = new Dictionary<string, List<int>>()
{
    { "1", new List<int> { 1, 2, 3, 4, 5, 6, 7 } },
    { "2", new List<int> { 8, 9, 10, 11, 12, 13, 14 } },
    { "3", new List<int> { 15, 16, 17, 18, 19, 20, 21 } },
    { "4", new List<int> { 22, 23, 24, 25, 26, 27, 28 } },
    { "5", new List<int> { 29, 30, 31, 32, 33, 34, 35 } },
    { "6", new List<int> { 36, 37, 38, 39, 40, 41, 42 } },
    { "9", new List<int> { 63 } },
    { "10", new List<int> { 64, 65, 66, 67, 68, 69, 70 } },
    { "11", new List<int> { 71, 72, 73, 74, 75, 76, 77 } },
    { "12", new List<int> { 78, 79, 80, 81, 82, 83, 84 } },
    { "13", new List<int> { 85, 86, 87, 88, 89, 90, 91 } },
    { "14", new List<int> { 92, 93, 94, 95, 96, 97, 98 } },
    { "21", new List<int> { 147 } },
    { "22", new List<int> { 148, 149, 150, 151, 152, 153, 154 } },
    { "23", new List<int> { 155, 156, 157, 158, 159, 160, 161 } },
    { "24", new List<int> { 162, 163, 164, 165, 166, 167, 168 } },
    { "25", new List<int> { 169, 170, 171, 172, 173, 174, 175 } },
    { "26", new List<int> { 176, 177, 178, 179, 180, 181, 182 } },
    { "33", new List<int> { 231 } },
    { "34", new List<int> { 232, 233, 234, 235, 236, 237, 238 } },
    { "35", new List<int> { 239, 240, 241, 242, 243, 244, 245 } },
    { "36", new List<int> { 246, 247, 248, 249, 250, 251, 252 } },
    { "37", new List<int> { 253, 254, 255, 256, 257, 258, 259 } },
    { "38", new List<int> { 260, 261, 262, 263, 264, 265, 266 } },
    { "45", new List<int> { 315 } },
    { "46", new List<int> { 316, 317, 318, 319, 320, 321, 322 } },
    { "47", new List<int> { 323, 324, 325, 326, 327, 328, 329 } },
    { "48", new List<int> { 330, 331, 332, 333, 334, 335, 336 } },
    { "49", new List<int> { 337, 338, 339, 340, 341, 342, 343 } },
    { "50", new List<int> { 344, 345, 346, 347, 348, 349, 350 } },
    { "57", new List<int> { 399 } },
    { "58", new List<int> { 400, 401, 402, 403, 404, 405, 406 } },
    { "59", new List<int> { 407, 408, 409, 410, 411, 412, 413 } },
    { "60", new List<int> { 414, 415, 416, 417, 418, 419, 420 } },
    { "61", new List<int> { 421, 422, 423, 424, 425, 426, 427 } },
    { "62", new List<int> { 428, 429, 430, 431, 432, 433, 434 } },
    { "69", new List<int> { 483 } },
    { "70", new List<int> { 484, 485, 486, 487, 488, 489, 490 } },
    { "71", new List<int> { 491, 492, 493, 494, 495, 496, 497 } },
    { "72", new List<int> { 498, 499, 500, 501, 502, 503, 504 } },
    { "73", new List<int> { 505, 506, 507, 508, 509, 510, 511 } },
    { "74", new List<int> { 512, 513, 514, 515, 516, 517, 518 } },
    { "81", new List<int> { 567 } },
    { "82", new List<int> { 568, 569, 570, 571, 572, 573, 574 } },
    { "83", new List<int> { 575, 576, 577, 578, 579, 580, 581 } },
    { "84", new List<int> { 582, 583, 584, 585, 586, 587, 588 } },
    { "85", new List<int> { 589, 590, 591, 592, 593, 594, 595 } },
    { "86", new List<int> { 596, 597, 598, 599, 600, 601, 602 } },
    { "101", new List<int> { 707 } },
    { "102", new List<int> { 708, 709, 710, 711, 712, 713, 714 } },
    { "103", new List<int> { 715, 716, 717, 718, 719, 720, 721 } },
    { "104", new List<int> { 722, 723, 724, 725, 726, 727, 728 } },
    { "105", new List<int> { 729, 730, 731, 732, 733, 734, 735 } },
    { "106", new List<int> { 736, 737, 738, 739, 740, 741, 742 } }
};

    switch (dp.Record.DataPage.Form.OID)
    {
        case "DIARY_P1":
            addDSL(kvcList, 11, false);
            break;
        case "DIARY_P2":
            addDSL(kvcList, 11, false);
            break;
        case "DIARY_P3":
            addDSL(kvcList, 17, false);
            addDSL(kvcList, 17, true);
            break;
        case "DIARY_P4":
            foreach (string key in weekDayDict.Keys)
            {
                KeyValue kvWK = new KeyValue(key, key);
                kvcList.Add(kvWK);
            }
            addDSL(kvcList, 17, true);
            break;
    }
    return kvcList;
}

void addDSL(KeyValueCollection kvcList, int lenth, bool isAddl)
{
    for (int i = 1; i < lenth + 1; i++)
    {
        string pretext;
        if (isAddl) pretext = "Additional Copy " + i.ToString();
        else pretext = i.ToString();
        KeyValue kvWK = new KeyValue(pretext, pretext);
        kvcList.Add(kvWK);
    }
}
}
}

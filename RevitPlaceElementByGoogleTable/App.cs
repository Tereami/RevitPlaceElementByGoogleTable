using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace RevitPlaceElementByGoogleTable
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class App : IExternalApplication
    {
        public static string assemblyPath;

        public Result OnStartup(UIControlledApplication application)
        {
            assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            string tabName = "BIM-STARTER TEST";
            try { application.CreateRibbonTab(tabName); } catch { }

            RibbonPanel panel1 = application.CreateRibbonPanel(tabName, "ARCH");

            _ = panel1.AddItem(new PushButtonData(
                 "btnPlacebtTable",
                 "Разместить по таблице",
                 assemblyPath,
                 "RevitPlaceElementByGoogleTable.Command")
                 );

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}

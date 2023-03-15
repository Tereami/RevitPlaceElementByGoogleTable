using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace RevitPlaceElementByGoogleTable
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public string paramName = "Таблица размещения оборудования";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            List<string> info = new List<string>();

            Document doc = commandData.Application.ActiveUIDocument.Document;
            Selection sel = commandData.Application.ActiveUIDocument.Selection;
            var selids = sel.GetElementIds();

            List<Room> rooms = new List<Room>();
            foreach (ElementId elid in selids)
            {
                Room r = doc.GetElement(elid) as Room;
                if (r is null) continue;
                rooms.Add(r);
            }

            if (rooms.Count == 0)
            {
                TaskDialog.Show("Error", "Выберите помещения для размещения оборудования");
                return Result.Failed;
            }
            info.Add("Выбрано помещений " + rooms.Count.ToString());

            ProjectInfo pinfo = doc.ProjectInformation;
            Parameter tableUrlParam = pinfo.LookupParameter(paramName);
            if (tableUrlParam == null || !tableUrlParam.HasValue)
            {
                TaskDialog.Show("Ошибка", "Не указана ссылка на таблицу в параметре "
                    + paramName + " в Информации о проекте");
                return Result.Failed;
            }

            //string url = "https://docs.google.com/spreadsheets/d/1XkAi-CVdAOJQbPA1r2VgFVnGOmtFBc-eozk0-b1VOWM";
            string url = tableUrlParam.AsString();
            info.Add("URL таблицы: " + url);
            Dictionary<string, List<string>> roomsData = GoogleUtils.GetInfo(url);

            //проверю помещения
            HashSet<string> roomNamesSet = new HashSet<string>();
            foreach(Room r in rooms)
            {
                string roomName = r.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();
                roomNamesSet.Add(roomName);
            }
            foreach(string roomName in roomNamesSet)
            {
                if(!roomsData.ContainsKey(roomName))
                    info.Add("В таблице не найдено помещение " + roomName);
            }

            //соберу список оборудования
            HashSet<string> furnitureNames = new HashSet<string>();
            foreach (List<string> furnitures in roomsData.Values)
            {
                foreach (string furnitureName in furnitures)
                {
                    furnitureNames.Add(furnitureName);
                }
            }

            //проверю, есть ли нужные семейства и типоразмеры в проекте
            Dictionary<string, FamilySymbol> furnitureStorage = new Dictionary<string, FamilySymbol>();
            foreach (string furnitureName in furnitureNames)
            {
                try
                {
                    FamilySymbol symb = DocumentGetter.GetFamilySymbol(doc, furnitureName);
                    furnitureStorage.Add(furnitureName, symb);
                }
                catch(Exception ex)
                {
                    info.Add(ex.Message);
                }
            }


            //можно начинать размещать оборудование
            using(Transaction t = new Transaction(doc))
            {
                t.Start("Размещение оборудования по помещениям");
                info.Add("Старт размещения оборудования");
                int count = 0;

                foreach(Room r in rooms)
                {
                    LocationPoint locPoint = r.Location as LocationPoint;
                    if (locPoint == null)
                    {
                        info.Add("Не удалось получить точку размещения помещения id" + r.Id.IntegerValue.ToString());
                        continue;
                    }
                    XYZ point = locPoint.Point;

                    Level lev = r.Level;
                    string roomName = r.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();
                    if (!roomsData.ContainsKey(roomName)) continue;
                    List<string> furnitures = roomsData[roomName];
                    foreach(string furnitureName in furnitures)
                    {
                        if (!furnitureStorage.ContainsKey(furnitureName))
                            continue;
                        FamilySymbol symb = furnitureStorage[furnitureName];
                        if (!symb.IsActive)
                            symb.Activate();
                        var strtype = Autodesk.Revit.DB.Structure.StructuralType.NonStructural;
                        doc.Create.NewFamilyInstance(point, symb, lev, strtype);
                        count++;
                    }
                }

                info.Add("Успешно размещено оборудования: " + count);
                t.Commit();
            }

            FormResult form = new FormResult(info);
            form.ShowDialog();

            return Result.Succeeded;
        }
    }
}

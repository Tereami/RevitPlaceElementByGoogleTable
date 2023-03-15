using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitPlaceElementByGoogleTable
{
    public static class DocumentGetter
    {
        public static FamilySymbol GetFamilySymbol(Document doc, string furnitureName)
        {
            string familyName = null;
            string symbolName = null;
            if (furnitureName.Contains(":"))
            {
                familyName = furnitureName.Split(':')[0].Trim();
                symbolName = furnitureName.Split(':')[1].Trim();
            }
            else
            {
                familyName = furnitureName;
            }

            List<Family> families = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfClass(typeof(Family))
                .Cast<Family>()
                .Where(i => i.Name == familyName)
                .ToList();

            if (families.Count == 0)
                throw new Exception("Не удалось найти семейство " + familyName);

            Family fam = families[0];
            ISet<ElementId> symbolsIds = fam.GetFamilySymbolIds();
            if (symbolsIds.Count == 0)
                throw new Exception("Нет типоразмеров у семейства " + familyName);

            FamilySymbol famSymb = null;
            if (symbolName == null || symbolsIds.Count == 1)
            {
                famSymb = doc.GetElement(symbolsIds.First()) as FamilySymbol;
            }
            else
            {
                foreach (ElementId symbId in symbolsIds)
                {
                    FamilySymbol curFamSymb = doc.GetElement(symbId) as FamilySymbol;
                    if (curFamSymb.Name == symbolName)
                    {
                        famSymb = curFamSymb;
                        break;
                    }
                }
            }

            if (famSymb == null)
            {
                throw new Exception("Не удалось найти типоразмер семейства " + furnitureName);
            }

            return famSymb;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;


namespace RevitPlaceElementByGoogleTable
{
    public static class GoogleUtils
    {
        public static Dictionary<string, List<string>> GetInfo(string TableUrl)
        {
            string data = ReadUrl(TableUrl, Encoding.UTF8);

            List<string[]> cells = ParseTableToRows(data);

            CheckTableIsNormal(cells);

            int columnsCount = cells[0].Length;

            Dictionary<string, List<string>> roomsData = new Dictionary<string, List<string>>();
            for (int i = 0; i < columnsCount; i++)
            {
                string roomName = cells[0][i];
                List<string> furnitureNames = new List<string>();
                for (int j = 1; j < cells.Count; j++)
                {
                    string furnitureName = cells[j][i];
                    if (furnitureName.Length < 2) 
                        continue;
                    furnitureNames.Add(furnitureName);
                }
                if(roomsData.ContainsKey(roomName))
                {
                    throw new Exception("Дублировано имя помещения " + roomName);
                }
                roomsData.Add(roomName, furnitureNames);
            }

            return roomsData;
        }
        
        public static string ReadUrl(string url, Encoding encoding)
        {
            Uri uri = new Uri(url);
            string data = "";

            using (WebClient wc = new WebClient())
            {
                wc.Encoding = encoding;
                data = wc.DownloadString(uri);
            }

            return data;
        }

        public static List<string[]> ParseTableToRows(string data)
        {
            string[] lines = data.Split(
                new string[] {
                    "\r\n",
                    "\n\r",
                    "\n",
                    "\r" },
                StringSplitOptions.None);

            List<string[]> rows = new List<string[]>();
            for (int i = 2; i < lines.Length; i++)
            {
                string row = lines[i];
                if (row.StartsWith("try")) break;

                if (row.Contains("\""))
                    row = row.Split('\"')[0];

                if (!row.Contains(",")) break;

                string[] rowCells = row.Split(',');
                rows.Add(rowCells);
            }

            return rows;
        }
        
        public static void CheckTableIsNormal(List<string[]> rows)
        {
            int columnsCount = rows[0].Length;
            for (int i = 1; i < rows.Count; i++)
            {
                string[] curRow = rows[i];
                if (curRow.Length != columnsCount)
                {
                    string line = string.Join(",", curRow);
                    string msg = "Неверное количество элементов в строке  " + line
                        + ". Заполните пустые ячейки символом х";
                    System.Windows.Forms.MessageBox.Show(msg);
                    throw new Exception(msg);
                }
            }
        }
    }
}

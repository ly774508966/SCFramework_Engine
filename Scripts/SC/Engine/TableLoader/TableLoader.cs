using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SCFramework
{
    public partial class TableLoader
    {
        public static CsvTable ParseCsvFile(string fileName)
        {
            string path = "Data/" + fileName;
            var data = Resources.Load<TextAsset>(path);
            CsvTable table = CsvTable.Parse(data.text, true);
            return table;
        }

        public static T LoadTable<T>(string fileName) where T : IDataTable, new()
        {
            T t = new T();
            t.Load(ParseCsvFile(fileName));
            return t;
        }

        public void LoadAllTable()
        {
            //StartCoroutine(loadConfigDataTable());
        }

        public float Schedule
        {
            //get { return (float)(m_LoadIndex) / (float)(m_TotalCount); }
            get { return 1.0f; }
        }

    }
}


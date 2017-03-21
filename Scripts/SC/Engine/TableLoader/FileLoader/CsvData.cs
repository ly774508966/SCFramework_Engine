using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SCFramework
{
	
	public class CsvData
	{
	    private CsvData()
	    {
	        _bInited = false;
	    }

	    public static CsvData Instance()
	    {
	        if (_instance == null)
	            _instance = new CsvData();
	        return _instance;
	    }

	    public void Init()
	    {
	        if(!_bInited)
	        {
	            //Helper.CallProcess("/Assets/Resources/CSV/CSVEncoding.exe", "CSV Encoding");

	            var data = Resources.Load<TextAsset>("Table/Config");
	            var config = CsvTable.Parse(data.text, true);

	            foreach(var val in config.Table)
	            {
	                int columnIndex = config.FindColumnIndex("Name");
	                if (columnIndex == -1)
	                    continue;
	                string tableName = val[columnIndex].AsString();

	                columnIndex = config.FindColumnIndex("Path");
	                if (columnIndex == -1)
	                    continue;
					//string path = val[columnIndex].AsString();

	                columnIndex = config.FindColumnIndex("HasHeader");
					if (columnIndex == -1)
						continue;
	                bool hasHeader = val[columnIndex].AsInt() == 1;

	                CsvTable table = null;
	                try
	                {
	                    table = _LoadTable("Table/" + tableName, hasHeader);
	                    if (table == null)
	                    {
	                        Log.i(tableName + "load error");
	                        continue;
	                    }
	                }
	                catch
	                {
	                    Log.i(tableName + "load error");
	                }

	                _tables.Add(tableName, table);

	            }
	            _bInited = true;
	        }
	    }

	    CsvTable _LoadTable(string path, bool hasHeader)
	    {
			var data = Resources.Load<TextAsset>(path);
	        return CsvTable.Parse(data.text, hasHeader);
	    }

	    /// <summary>
	    /// 查找数据表.
	    /// </summary>
	    /// <param name="name">表名</param>
	    /// <returns></returns>
		public CsvTable FindTable(string name)
		{
			if (_tables.ContainsKey (name))
				return _tables [name];
			return null;
		}

	    
	    public static Vector3[] ConvertToPoints(string points)
	    {
	        List<Vector3> ret = new List<Vector3>();
	        string[] vals = points.Split(new char[]{'|'});

	        foreach(var v in vals)
	        {
	            ret.Add(ConvertToPoint(v));
	        }

	        return ret.ToArray();
	    }

	    public static Vector3 ConvertToPoint(string point)
	    {
	        point = point.Trim(new char[] { ' ', '(', ')' });
	        string[] vals = point.Split(new char[]{','});
	        Vector3 ret = Vector3.zero;

	        if(vals.Length == 3)
	        {
	            float.TryParse(vals[0], out ret.x);
	            float.TryParse(vals[1], out ret.y);
	            float.TryParse(vals[2], out ret.z);
	        }

	        return ret;
	    }

	    public static int[] ConvertToIntArray(string val)
	    {
	        var s = val.Trim();

	        if (s.Length <= 2)
	            return null;
	        if(s[0] != '[' || s[s.Length - 1] != ']')
	        {
	            return null;
	        }

	        s = s.Trim(new char[] { '[', ']' });
	        string[] sVal = s.Split(new char[] { ',' });
	        if (sVal.Length == 0)
	            return null;

	        int[] ret = new int[sVal.Length];
	        for(int i = 0; i < sVal.Length; i++)
	        {
	            int.TryParse(sVal[i], out ret[i]);
	        }

	        return ret;
	    }

	    public static uint[] ConvertToUIntArray(string val)
	    {
	        var s = val.Trim();

	        if (s.Length <= 2)
	            return null;
	        if (s[0] != '[' || s[s.Length - 1] != ']')
	        {
	            return null;
	        }

	        s = s.Trim(new char[] { '[', ']' });
	        string[] sVal = s.Split(new char[] { ',' });
	        if (sVal.Length == 0)
	            return null;

	        uint[] ret = new uint[sVal.Length];
	        for (int i = 0; i < sVal.Length; i++)
	        {
	            uint.TryParse(sVal[i], out ret[i]);
	        }

	        return ret;
	    }

	    public static float[] ConvertToFloatArray(string val)
	    {
	        var s = val.Trim();

	        if (s.Length <= 2)
	            return null;
	        if (s[0] != '[' || s[s.Length - 1] != ']')
	        {
	            return null;
	        }

	        s = s.Trim(new char[] { '[', ']' });
	        string[] sVal = s.Split(new char[] { ',' });
	        if (sVal.Length == 0)
	            return null;

	        float[] ret = new float[sVal.Length];
	        for (int i = 0; i < sVal.Length; i++)
	        {
	            float.TryParse(sVal[i], out ret[i]);
	        }

	        return ret;
	    }
	    

	    static CsvData _instance;

	    bool _bInited;

	    Dictionary<string, CsvTable> _tables = new Dictionary<string, CsvTable>();

	}
}

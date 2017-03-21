using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SCFramework
{
	public sealed class Value
	{
		public Value(string val)
		{
			_value = val;
			if (_value == null)
				_value = "";
		}
	
		public string AsString()
		{
			return _value;
		}

		public int AsInt()
		{
			int ret = 0;
			int.TryParse (_value, out ret);
			return ret;
		}

		public long AsLong()
		{
			long ret = 0;
			long.TryParse (_value, out ret);
			return ret;
		}

		public short AsShort()
		{
			short ret = 0;
			short.TryParse (_value, out ret);
			return ret;
		}

		public uint AsUInt()
		{
			uint ret = 0;
			uint.TryParse (_value, out ret);
			return ret;
		}

		public ushort AsUShort()
		{
			ushort ret = 0;
			ushort.TryParse (_value, out ret);
			return ret;
		}

		public ulong AsULong()
		{
			ulong ret = 0;
			ulong.TryParse (_value, out ret);
			return ret;
		}

		public float AsFloat()
		{
			float ret = 0;
			float.TryParse (_value, out ret);
			return ret;
		}

		public double AsDouble()
		{
			double ret = 0;
			double.TryParse (_value, out ret);
			return ret;
		}

        public bool AsBool()
        {
            bool ret = false;
            bool.TryParse(_value, out ret);
            return ret;
        }

		string _value;
	}

	public class CsvTable
	{
		public CsvTable()
		{

		}

		public static CsvTable Parse(string text)
		{
			return Parse (new StringReader (text), false);
		}

		public static CsvTable Parse(string text, bool hasHeader)
		{
			return Parse (new StringReader (text), hasHeader);
		}

		public static CsvTable Parse(TextReader reader, bool hasHeader)
		{
			CsvTable csv = new CsvTable ();
			int line_num = 0;

			while (reader.Peek() != -1) 
			{
				string line = reader.ReadLine();
				if(hasHeader && line_num == 0)
				{
					string[] columns = _ParseLine(line);
					if(columns == null)

						return null;
					int columns_index = 0;
					foreach(var c in columns)
					{
						csv._header[c] = columns_index++;
					}
				}
				else if(hasHeader && line_num == 1)
				{
					string[] columns = _ParseLine(line);
					if(columns == null)
						return null;

					foreach(var c in columns)
					{
						csv._type.Add(c);
					}
				}
				else
				{
					string[] cells = _ParseLine(line);
                    if (cells == null)
                        continue;

					List<Value> row = new List<Value>();
					foreach(var c in cells)
					{
						row.Add(new Value(c));
					}

					if(hasHeader)
					{
						if(row.Count != csv._header.Count)
							continue;
					}

                    if (row.Count == 0)
                        continue;

                    csv._index.Add(row[0].AsString(), csv._table.Count);
					csv._table.Add(row);
				}

				line_num++;
			}

			return csv;
		}

		public List<List<Value>> Table
		{
			get { return _table; }
		}

		public Dictionary<string, int> Header
		{
			get { return _header; }
		}

		public List<string> typeList
		{
			get { return _type; }
		}

		public List<Value> Find(int id)
		{
			if(_index.ContainsKey(id.ToString()))
			{
				return _table[_index[id.ToString()]];
			}
			else
			{
				return null;
			}
		}

		public Dictionary<string, Value> FindRow(string InColName, int InColValue)
        {
            var colIndex = FindColumnIndex(InColName);
            if (colIndex < 0)
                return null;

            foreach(var row in _table)
            {
                if (row[colIndex].AsInt() == InColValue)
                {
                    var id = row[0].AsInt();
                    return FindRow(id);
                }
            }

            return null;
        }

		public Dictionary<string, Value> FindRow(int id)
		{
			List<Value> row = Find (id);
			if (row == null)
				return null;
			if (_header.Count != row.Count)
				return null;

			Dictionary<string, Value> ret = new Dictionary<string, Value> ();
			foreach (var pair in _header)
			{
				ret.Add(pair.Key, row[pair.Value]);
			}

			return ret;
		}



        public int FindColumnIndex(string name)
        {
            if (_header == null || _header.Count == 0)
                return -1;

            if(_header.ContainsKey(name))
            {
                return _header[name];
            }

            return -1;
        }

		static string[] _ParseLine(string line)
		{
			List<string> ret = new List<string> ();
			string data = "";
			for(int i = 0; i < line.Length; i++)
			{
				char ch = line[i];
				if(ch == ',')
				{
					ret.Add(data);
					data = "";
					continue;
				}
				else if(ch == '"')
				{
					do
					{
						bool bOk = _Next(line, ref i, ref ch);
						if(bOk)
						{
							if(ch == '\\')
							{
								bOk = _Next (line, ref i, ref ch);
								if(!bOk)
									return null;
								if( ch == ',' || ch == '"' )
									data += ch;
								else
									return null;
							}
							else if(ch == '"')
							{
								break;
							}
							else
							{
								data += ch;
							}
						}
						else
						{
							if(ch == '"')
							{
								break;
							}
							else
								return null;
						}
					}while(true);
				}
				else
				{
					data += ch;
				}
			}

			ret.Add (data);
			return ret.ToArray ();
		}

		static bool _Next(string line, ref int i, ref char ch)
		{
			++i;
			if (i < line.Length)
			{
				ch = line[i];
				return true;
			}

			return false;
		}

		Dictionary<string, int> _index = new Dictionary<string, int> ();
		Dictionary<string, int> _header = new Dictionary<string, int>();
		List<string>			_type = new List<string>();
		List<List<Value>> _table = new List<List<Value>>();
	}
	
}

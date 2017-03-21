using UnityEngine;
using System.Collections;

namespace SCFramework
{

	public class BaseDataTable : IDataTable
	{
		public virtual void Load(CsvTable table){}
		public virtual void OnLoadFinish(){}
		public virtual bool OnAllTableLoadFinish() { return false; }
	}
}

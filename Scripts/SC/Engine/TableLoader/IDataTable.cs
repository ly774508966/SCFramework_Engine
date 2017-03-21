using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SCFramework
{
    public interface IDataTable
    {
        void Load(CsvTable table);
        void OnLoadFinish();
        bool OnAllTableLoadFinish();
    }
}

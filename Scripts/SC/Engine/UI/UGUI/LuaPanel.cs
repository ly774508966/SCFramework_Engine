using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SCFramework
{
    public class LuaPanel : AbstractPanel
    {
        [SerializeField]
        private string m_LuaSciptName;

        protected override void OnUIInit()
        {
            InitLuaBind();
        }

        protected void InitLuaBind()
        {

        }
    }
}

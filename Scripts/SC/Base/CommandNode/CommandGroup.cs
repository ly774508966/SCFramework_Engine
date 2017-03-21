﻿using System;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace SCFramework
{
    public class CommandGroup : CommandNode
    {
        private List<CommandNode> m_GuideCommandList;
        private int m_ComplateCommandCount;

        public void Add(CommandNode command)
        {
            if (m_GuideCommandList == null)
            {
                m_GuideCommandList = new List<CommandNode>();
            }
            m_GuideCommandList.Add(command);
        }

        public override void Start()
        {
            if (m_GuideCommandList == null)
            {
                FinishCommand();
            }
            m_ComplateCommandCount = m_GuideCommandList.Count;
            foreach (var item in m_GuideCommandList)
            {
                item.SetCommandNodeEventListener(OnSubCommondComplate);
                item.Start();
            }
        }

        private void OnSubCommondComplate(CommandNode command)
        {
            --m_ComplateCommandCount;
            if (m_ComplateCommandCount == 0)
            {
                FinishCommand();
            }
        }
    }

}

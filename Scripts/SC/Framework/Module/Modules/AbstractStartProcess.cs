﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace SCFramework
{
    public class AbstractStartProcess : AbstractMonoModule
    {
        private ExecuteNodeContainer m_ExecuteContainer;

        public ExecuteNodeContainer executeContainer
        {
            get
            {
                return m_ExecuteContainer;
            }
        }
        public float totalSchedule
        {
            get
            {
                if (m_ExecuteContainer == null)
                {
                    return 0;
                }

                return m_ExecuteContainer.totalSchedule;
            }
        }

        public void Append(ExecuteNode node)
        {
            if (node == null)
            {
                return;
            }

            if (m_ExecuteContainer == null)
            {
                m_ExecuteContainer = new ExecuteNodeContainer();
            }
            m_ExecuteContainer.Append(node);
        }

        protected override void OnAwakeCom()
        {
            InitExechuteContainer();
        }

        public override void OnComStart()
        {
            if (m_ExecuteContainer == null)
            {
                return;
            }

            m_ExecuteContainer.On_ExecuteContainerEndEvent += OnAllExecuteNodeEnd;
            m_ExecuteContainer.Start();
        }

        public override void OnComUpdate(float dt)
        {
            if (m_ExecuteContainer == null)
            {
                return;
            }

            m_ExecuteContainer.Update();
        }

        protected virtual void InitExechuteContainer()
        {

        }

        protected virtual void OnAllExecuteNodeEnd()
        {
            Log.i("#BaseStartProcess: OnAllExecuteNodeEnd");
            m_ExecuteContainer.On_ExecuteContainerEndEvent -= OnAllExecuteNodeEnd;
            m_ExecuteContainer = null;
            Destroy(gameObject);
        }
    }
}

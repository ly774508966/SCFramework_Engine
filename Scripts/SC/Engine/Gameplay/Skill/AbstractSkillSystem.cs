﻿using System;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace SCFramework
{
    public class AbstractSkillSystem
    {
        #region 技能释放过滤器
        public interface SkillReleaseFilter
        {
            //过滤器的排序
            int FilterSort
            {
                get;
            }

            bool CheckSkillReleaseAble(ISkill skill, ISkillReleaser releaser);
        }

        #endregion

        private int                         m_NextSkillID = 0;
        //对技能列表的增删操作优化空间比较大
        private List<SkillReleaseFilter>    m_SkillFilterList;  //技能释放过滤器列表
        private List<ISkill>                m_SkillList;    //释放技能列表

        public AbstractSkillSystem()
        {
            m_SkillFilterList = new List<SkillReleaseFilter>();
            m_SkillList = new List<ISkill>();
        }

        #region Public Func
        public bool ReleaseSkill(ISkill skill, ISkillReleaser releaser)
        {
            if (skill == null)
            {
                return false;
            }

            for(int i = m_SkillFilterList.Count - 1; i >= 0; --i)
            {
                if(!m_SkillFilterList[i].CheckSkillReleaseAble(skill, releaser))
                {
                    return false;
                }
            }

            m_SkillList.Add(skill);

            skill.skillInfo = CreateSkillInfo(NextSkillID);

            skill.DoSkillRelease(this, releaser);

            return true;
        }

        public void RemoveSkill(ISkill skill)
        {
            if (skill == null || skill.skillInfo == null)
            {
                return;
            }

            skill.skillInfo.skillState = SkillInfo.eSkillState.kRemove;
        }

        public void RemoveSkillByReleaser(ISkillReleaser releaser)
        {
            if(releaser == null)
            {
                return;
            }

            for(int i = m_SkillList.Count - 1; i >= 0; --i)
            {
                if (m_SkillList[i].skillReleaser == releaser)
                {
                    m_SkillList[i].skillInfo.skillState = SkillInfo.eSkillState.kRemove;
                }
            }
        }

        public void Update(float time)
        {
            for (int i = m_SkillList.Count - 1; i >= 0; --i)
            {
                ISkill skill = m_SkillList[i];
                if (skill.skillInfo.skillState == SkillInfo.eSkillState.kRemove)
                {
                    m_SkillList.RemoveAt(i);
                    skill.DoSkillRemove();
                    continue;
                }

                skill.DoSkillUpdate(time);
            }
        }

        #endregion

        #region Protected Func
        protected int NextSkillID
        {
            get { return ++m_NextSkillID; }
        }

        protected SkillInfo CreateSkillInfo(int skillID, SkillInfo.eSkillState defaultState = SkillInfo.eSkillState.kUnInit)
        {
            SkillInfo info = new SkillInfo();
            info.skillID = skillID;
            info.skillState = defaultState;
            return info;
        }
        #endregion
    }
}

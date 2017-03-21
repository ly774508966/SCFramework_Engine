﻿using System;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace SCFramework
{
    public interface ISkillReleaser
    {
        AbstractActor actor
        {
            get;
        }
        void OnSkillRelease(ISkill skill);
        void OnSkillRemove(ISkill skill);
    }
}

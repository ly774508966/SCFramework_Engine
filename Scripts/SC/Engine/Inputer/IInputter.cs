﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SCFramework
{
    public interface IInputter
    {
        bool isEnabled
        {
            get;
            set;
        }
        void RegisterKeyCodeMonitor(KeyCode code, Run begin, Run end, Run press);
        void Release();
        void LateUpdate();
    }
}


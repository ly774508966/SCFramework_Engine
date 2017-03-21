using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SCFramework
{
    //面板管理的内部事件
    public enum ViewEvent
    {
        MIN = 0,
        Action_ClosePanel,
        OnPanelOpen,
        OnPanelClose,
        DumpTest,
    }

    public interface IView
    {

    }

    public interface IViewDelegate
    {

    }
}

﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace SCFramework
{
    public class AlphaGradient : MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.UI.Graphic m_Target;
        void Awake()
        {
            if (m_Target == null)
            {
                m_Target = GetComponent<Graphic>();
            }
        }

        float m_MaxStep = 0.2f;
        private void Update()
        {
            float childCount = transform.parent.childCount;
            float step = 1.0f / childCount;
            step = Mathf.Min(m_MaxStep, step);
            float precent = 1 - (childCount - transform.GetSiblingIndex()) * step;
            precent = Mathf.Max(0.2f, Mathf.Min(precent, 1.0f));
            Color c = m_Target.color;
            c.a = precent;
            m_Target.color = c;
        }
    }
}

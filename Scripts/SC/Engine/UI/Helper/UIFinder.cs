﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SCFramework
{
    public class UIFinder
    {
        public static T Find<T>(GameObject parent, string name) where T : MonoBehaviour
        {
            Transform target = parent.transform.Find(name);
            if (target == null)
            {
                Log.e("Error Not Find Compoment:" + name);
                return null;
            }

            return target.gameObject.GetComponent<T>();
        }


        public static T Find<T>(Transform parent, string name) where T : MonoBehaviour
        {
            Transform target = parent.Find(name);
            if (target == null)
            {
                Log.e("Error Not Find Compoment:" + name);
                return null;
            }
            return target.gameObject.GetComponent<T>();
        }

        public static Transform FindTransform(Transform parent, string name)
        {
            if (parent == null)
            {
                return null;
            }

            Transform target = parent.transform.Find(name);
            if (target == null)
            {
                Log.e("Error Not Find Obj:" + name);
                return null;
            }
            return target;
        }

        public static T FindInChild<T>(Transform parent, string childName, string nodeName) where T : MonoBehaviour
        {
            Transform child = FindTransform(parent.transform, childName);
            if (child == null)
            {
                return null;
            }

            Transform target = child.Find(nodeName);
            if (target == null)
            {
                Log.e("Error Not Find Compoment:" + nodeName);
                return null;
            }
            return target.gameObject.GetComponent<T>();
        }

        public static T FindInChildChildren<T>(Transform parent, string childName, bool includeInactive) where T : MonoBehaviour
        {
            Transform child = FindTransform(parent.transform, childName);
            if (child == null)
            {
                Log.e("Error Not Find:" + childName);
                return null;
            }

            T target = child.GetComponentInChildren<T>(includeInactive);
            return target;
        }

        public static T FindInChild<T>(GameObject parent, string childName, string nodeName) where T : MonoBehaviour
        {
            return FindInChild<T>(parent.transform, childName, nodeName);
        }
    }
}

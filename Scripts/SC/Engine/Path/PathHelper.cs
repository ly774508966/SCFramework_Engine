﻿using System;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SCFramework
{
    public class PathHelper
    {

        public static string FileNameWithoutSuffix(string name)
        {
            if (name == null)
            {
                return null;
            }

            int endIndex = name.LastIndexOf('.');
            if (endIndex > 0)
            {
                return name.Substring(0, endIndex);
            }
            return name;
        }

        public static string FullAssetPath2Name(string fullPath)
        {
            string name = FileNameWithoutSuffix(fullPath);
            if (name == null)
            {
                return null;
            }

            int endIndex = name.LastIndexOf('/');
            if (endIndex > 0)
            {
                return name.Substring(endIndex + 1);
            }
            return name;
        }
    }
}

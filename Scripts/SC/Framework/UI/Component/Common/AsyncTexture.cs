﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace SCFramework
{
    public class AsyncTexture : MonoBehaviour
    {
        [SerializeField]
        private RawImage m_RawImage;
        [SerializeField]
        private string m_TexturePath;
        [SerializeField]
        private bool m_AutoHide = true;

        private ResLoader m_ResLoader;

        private void Awake()
        {
            if (m_RawImage == null)
            {
                m_RawImage = GetComponent<RawImage>();
            }

            LoadTexture(m_TexturePath);
        }

        public string texturePath
        {
            get { return m_TexturePath; }
            set
            {
                if (m_TexturePath == value)
                {
                    return;
                }

                LoadTexture(value);
            }
        }

        public Sprite sprite
        {
            set
            {
                if (m_RawImage != null)
                {
                    if (value != null)
                    {
                        m_RawImage.texture = value.texture;
                        m_RawImage.enabled = true;
                    }
                    else
                    {
                        m_RawImage.texture = null;
                        m_RawImage.enabled = false;
                    }
                }
            }
        }

        public void LoadTexture(string path)
        {
            if (m_RawImage == null)
            {
                return;
            }

            m_RawImage.texture = null;
            if (m_AutoHide)
            {
                m_RawImage.enabled = false;
            }
            if (m_ResLoader != null)
            {
                m_ResLoader.ReleaseAllRes();
            }

            m_TexturePath = path;

            if (string.IsNullOrEmpty(m_TexturePath))
            {
                return;
            }

            if (m_ResLoader == null)
            {
                m_ResLoader = new ResLoader();
            }

            m_ResLoader.Add2Load(m_TexturePath, OnResLoadFinish);
            m_ResLoader.LoadAsync();
        }

        private void OnResLoadFinish(bool result, IRes res)
        {
            if (!result || res == null)
            {
                return;
            }


            if (!res.name.Equals(m_TexturePath))
            {
                return;
            }

            UnityEngine.Object obj = res.asset;

            if (obj == null)
            {
                return;
            }

            m_RawImage.texture = obj as Texture;
            m_RawImage.enabled = true;
        }
    }
}

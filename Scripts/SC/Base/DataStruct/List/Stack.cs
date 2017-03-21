﻿using System;

namespace SCFramework
{
    
    public class SCStack<T>
    {
        private SCLinkedList<T> m_List;

        public void Push(T data)
        {
            if (m_List == null)
            {
                m_List = new SCLinkedList<T>();
            }
            m_List.InsertHead(data);
        }

        public T Pop()
        {
            if (m_List == null)
            {
                return default(T);
            }

            T result = m_List.HeadData;
            m_List.RemoveHead();
            return result;
        }

        public T Top()
        {
            if (m_List == null)
            {
                return default(T);
            }

            T result = m_List.HeadData;
            return result;
        }

        public bool IsEmpty
        {
            get
            {
                if (m_List == null)
                {
                    return true;
                }

                return m_List.IsEmpty;
            }
        }
    }
    
}



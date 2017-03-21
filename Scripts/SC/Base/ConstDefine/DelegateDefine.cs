using System;
using System.Collections;
using System.Collections.Generic;

namespace SCFramework
{
    public delegate void DTableOnParse(byte[] data);
    public delegate void Run();
    public delegate void Run<T>(T v);
    public delegate void Run<T, K>(T v1, K v2);
    
}

﻿using System;

namespace CloudCoin
{
    public class MultiDetectRequest
    {
        public int[] nn ;
        public int[] sn;
        public String[][] an = new String[Config.NodeCount][];
        public String[][] pan = new String[Config.NodeCount][];
        public int[] d;
        public int timeout;
    }
}

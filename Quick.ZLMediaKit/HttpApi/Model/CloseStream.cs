﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Quick.ZLMediaKit.HttpApi.Model
{
    public class CloseStreamResult : CloseBaseResult
    {
        /// <summary>
        /// 被关闭的流个数，可能小于count_hit
        /// </summary>
        public int Count_Closed { get; set; }
    }
}

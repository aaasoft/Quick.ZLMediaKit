using System;
using System.Collections.Generic;
using System.Text;

namespace Quick.ZLMediaKit.HttpApi
{
    public class ZLMediaKitClientOptions
    {
        public string ApiUrl { get; set; } = "http://127.0.0.1/index/api";
        public string ApiSecret { get; set; } = "035c73f7-bb6b-4889-a715-d9eb2d1925cc";
        /// <summary>
        /// 超时时间
        /// </summary>
        public int Timeout { get; set; } = 2000;
    }
}

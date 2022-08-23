using System;
using System.Collections.Generic;
using System.Text;

namespace Quick.ZLMediaKit.HttpApi.Model
{
    public class ServerConfigResult
    {
        public ApiCodeEnum Code { get; set; }

        /// <summary>
        /// 配置项变更个数
        /// </summary>
        public int Changed { get; set; }

        public string Msg { get; set; }
    }
}

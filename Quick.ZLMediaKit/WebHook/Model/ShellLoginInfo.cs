using System;
using System.Collections.Generic;
using System.Text;

namespace Quick.ZLMediaKit.WebHook.Model
{
    public class ShellLoginInfo : EventBase
    {
        /// <summary>
        /// TCP链接唯一ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// telnet 终端ip
        /// </summary>
        public string Ip { get; set; }
        /// <summary>
        /// telnet 终端登录用户密码
        /// </summary>
        public bool Passwd { get; set; }
        /// <summary>
        /// telnet 终端端口号
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// telnet 终端登录用户名
        /// </summary>
        public string User_name { get; set; }
    }

    public class ShellLonginResult : ResultBase
    {
        public ShellLonginResult()
        {
            this.Code = -1;
            this.Msg = "禁止访问";
        }
    }
}

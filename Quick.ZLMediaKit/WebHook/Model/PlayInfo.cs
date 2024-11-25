﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Quick.ZLMediaKit.WebHook.Model
{
    public class PlayInfo : EventBase
    {
        /// <summary>
        /// 流应用名
        /// </summary>
        public string App { get; set; }
        /// <summary>
        /// TCP链接唯一ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 播放器ip
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// 播放url参数
        /// </summary>
        public string Params { get; set; }

        /// <summary>
        /// 播放器端口号
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 播放的协议，可能是rtsp、rtmp、http
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// 流ID
        /// </summary>
        public string Stream { get; set; }

        /// <summary>
        /// 流虚拟主机
        /// </summary>
        public string Vhost { get; set; }

    }

    public class PlayInfoResult : ResultBase
    {
        
    }
}

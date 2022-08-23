using System;
using System.Collections.Generic;
using System.Text;

namespace Quick.ZLMediaKit.WebHook.Model
{
    public class PublishInfo : EventBase
    {
        /// <summary>
        /// 流应用名
        /// </summary>
        public string App { get; set; }

        /// <summary>
        /// Tcp链接唯一Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 推流器Ip
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// 推流Url参数
        /// </summary>
        public string Params { get; set; }

        /// <summary>
        /// 推流器端口号
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 推流的协议，可能是rtsp、rtmp、rtp
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// 流Id
        /// </summary>
        public string Stream { get; set; }

        /// <summary>
        /// 流虚拟主机
        /// </summary>
        public string VHost { get; set; }
    }

    public class PublishResult : ResultBase
    {
        public PublishResult() { }
        public PublishResult(string err)
        {
            this.Code = -1;
            this.Msg = err;
            this.EnableHls = false;
            this.EnableMP4 = false;
            this.EnableRtxp = false;
        }

        /// <summary>
        /// 是否转换成hls协议
        /// </summary>
        /// <value>Default Value : True</value>
        public bool EnableHls { get; set; } = true;

        /// <summary>
        /// 是否允许mp4录制
        /// </summary>
        /// <value>Default Value : False</value>
        public bool EnableMP4 { get; set; } = false;

        /// <summary>
        /// 是否允许转rtsp或rtmp
        /// </summary>
        /// <value>Default Value : True</value>
        public bool EnableRtxp { get; set; } = true;
    }
}

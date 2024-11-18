﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl.Http;
using Quick.ZLMediaKit.HttpApi.Model;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Quick.ZLMediaKit.HttpApi
{
    public class ZLMediaKitClient
    {
        private ZLMediaKitClientOptions options;
        
        public ZLMediaKitClient(ZLMediaKitClientOptions options)
        {
            this.options = options;
        }

        private IFlurlRequest BaseRequest()
        {
            return options.ApiUrl
                .WithTimeout(options.Timeout)
                .SetQueryParam("secret", options.ApiSecret);
        }

        /// <summary>
        /// 获取各epoll(或select)线程负载以及延时
        /// </summary>
        /// <returns></returns>
        public Task<ResultListBase<ThreadsLoad>> GetThreadsLoad()
        {
            return BaseRequest()
                .AppendPathSegment("getThreadsLoad")
                .GetJsonAsync<ResultListBase<ThreadsLoad>>();
        }

        /// <summary>
        /// 获取各后台epoll(或select)线程负载以及延时
        /// </summary>
        /// <returns></returns>
        public Task<ResultListBase<ThreadsLoad>> GetWorkThreadsLoad()
        {
            return BaseRequest()
                .AppendPathSegment("getWorkThreadsLoad")
                .GetJsonAsync<ResultListBase<ThreadsLoad>>();
        }

        /// <summary>
        /// 获取服务器配置
        /// </summary>
        /// <returns></returns>
        public Task<ResultBase<ServerConfig>> GetServerConfig()
        {
            return BaseRequest()
                .AppendPathSegment("getServerConfig")
                .GetJsonAsync<ResultListBase<Dictionary<string, object>>>()
                .ContinueWith(task =>
                {
                    var result = task.Result;
                    if (result.Code != ApiCodeEnum.Success) return new ResultBase<ServerConfig> { Code = result.Code, Msg = result.Msg, Data = null };
                    var config = result.Data.FirstOrDefault();
                    return new ResultBase<ServerConfig>
                    {
                        Code = result.Code,
                        Msg = result.Msg,
                        Data = new ServerConfig
                        {
                            Api = GetModel<ServerConfig.ApiConfig>(config, ServerConfig.ApiConfig.PrefixName),
                            Ffmpeg = GetModel<ServerConfig.FfmpegConfig>(config, ServerConfig.FfmpegConfig.PrefixName),
                            General = GetModel<ServerConfig.GeneralConfig>(config, ServerConfig.GeneralConfig.PrefixName),
                            Hls = GetModel<ServerConfig.HlsConfig>(config, ServerConfig.HlsConfig.PrefixName),
                            Hook = GetModel<ServerConfig.HookConfig>(config, ServerConfig.HookConfig.PrefixName),
                            Http = GetModel<ServerConfig.HttpConfig>(config, ServerConfig.HttpConfig.PrefixName),
                            Multicast = GetModel<ServerConfig.MulticastConfig>(config, ServerConfig.MulticastConfig.PrefixName),
                            Record = GetModel<ServerConfig.RecordConfig>(config, ServerConfig.RecordConfig.PrefixName),
                            Rtmp = GetModel<ServerConfig.RtmpConfig>(config, ServerConfig.RtmpConfig.PrefixName),
                            Rtp = GetModel<ServerConfig.RtpConfig>(config, ServerConfig.RtpConfig.PrefixName),
                            Rtc = GetModel<ServerConfig.RtcConfig>(config, ServerConfig.RtcConfig.PrefixName),
                            RtpProxy = GetModel<ServerConfig.RtpProxyConfig>(config, ServerConfig.RtpProxyConfig.PrefixName),
                            Rtsp = GetModel<ServerConfig.RtspConfig>(config, ServerConfig.RtspConfig.PrefixName),
                            Shell = GetModel<ServerConfig.ShellConfig>(config, ServerConfig.ShellConfig.PrefixName)
                        }
                    };
                });
        }

        private JsonSerializerOptions WebJsonSerializerOptions = new JsonSerializerOptions() 
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
             NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
        private T GetModel<T>(Dictionary<string, object> dicts, string name) where T : new()
        {
            var objDict = dicts.Where(w => w.Key.StartsWith(name)).ToDictionary(f => f.Key.Replace(name, ""), v => v.Value);
            var json = JsonSerializer.Serialize(objDict);
            return JsonSerializer.Deserialize<T>(json, WebJsonSerializerOptions);
        }

        /// <summary>
        /// 动态修改ZLM配置
        /// </summary>
        /// <param name="serverConfig"></param>
        /// <returns></returns>
        public Task<ServerConfigResult> SetServerConfig(ServerConfig serverConfig)
        {
            return GetServerConfig().ContinueWith(task =>
            {
                var currentConfig = task.Result.Data;
                var para = serverConfig.GetUpdateDicts(currentConfig);
                var resultTask = BaseRequest().AppendPathSegment("setServerConfig")
                    .SetQueryParams(para)
                    .GetJsonAsync<ServerConfigResult>();
                resultTask.Wait();
                return resultTask.Result;
            });
        }


        /// <summary>
        /// 启服务器,只有Daemon方式才能重启，否则是直接关闭！
        /// </summary>
        /// <returns></returns>
        public Task<ResultBase> RestartServer()
        {
            return BaseRequest()
                .AppendPathSegment("restartServer")
                .GetJsonAsync<ResultBase>();
        }

        /// <summary>
        /// 获取流列表，可选筛选参数
        /// </summary>
        /// <param name="schema">筛选协议，例如 rtsp或rtmp</param>
        /// <param name="vhost">筛选虚拟主机，例如__defaultVhost__</param>
        /// <param name="app">筛选应用名，例如 live</param>
        /// <returns></returns>
        public Task<ResultListBase<MediaInfo>> GetMediaList(string schema = null, string vhost = null, string app = null)
        {
            return BaseRequest()
                .AppendPathSegment("getMediaList")
                .SetQueryParam("schema", schema)
                .SetQueryParam("vhost", vhost)
                .SetQueryParam("app", app)
                .GetJsonAsync<ResultListBase<MediaInfo>>();
        }

        /// <summary>
        /// 关闭流(目前所有类型的流都支持关闭)
        /// </summary>
        /// <param name="schema">协议，例如 rtsp或rtmp</param>
        /// <param name="vhost">虚拟主机，例如__defaultVhost__</param>
        /// <param name="app">应用名，例如 live</param>
        /// <param name="stream">流id，例如 test</param>
        /// <param name="force">是否强制关闭(有人在观看是否还关闭)</param>
        /// <returns>result 
        /// <para>0 成功</para>
        /// <para>-1 关闭失败</para>
        /// <para>-2 该流不存在</para>
        /// </returns>
        /// <remarks>已过期，请使用 <see cref="CloseStreams(string, string, string, string, bool)"/>接口替代</remarks>
        [Obsolete("已过期，请使用close_streams接口替换")]
        public Task<ResultBase> CloseStream(string schema, string vhost, string app, string stream, bool force)
        {
            return BaseRequest()
                .AppendPathSegment("close_stream")
                .SetQueryParam("schema", schema)
                .SetQueryParam("vhost", vhost)
                .SetQueryParam("app", app)
                .SetQueryParam("stream", stream)
                .SetQueryParam("force", force ? 1 : 0)
                .GetJsonAsync<ResultBase>();
        }

        /// <summary>
        /// 关闭流(目前所有类型的流都支持关闭)
        /// </summary>
        /// <param name="schema">协议，例如 rtsp或rtmp</param>
        /// <param name="vhost">虚拟主机，例如__defaultVhost__</param>
        /// <param name="app">应用名，例如 live</param>
        /// <param name="stream">流id，例如 test</param>
        /// <param name="force">是否强制关闭(有人在观看是否还关闭)</param>
        /// <returns></returns>
        public Task<CloseStreamResult> CloseStreams(string schema = null, string vhost = null, string app = null, string stream = null, bool force = false)
        {
            return BaseRequest()
                .AppendPathSegment("close_streams")
                .SetQueryParam("schema", schema)
                .SetQueryParam("vhost", vhost)
                .SetQueryParam("app", app)
                .SetQueryParam("stream", stream)
                .SetQueryParam("force", force ? 1 : 0)
                .GetJsonAsync<CloseStreamResult>();
        }

        /// <summary>
        /// 获取所有TcpSession列表(获取所有tcp客户端相关信息)
        /// </summary>
        /// <param name="local_port">筛选本机端口，例如筛选rtsp链接：554</param>
        /// <param name="peer_ip">筛选客户端ip</param>
        /// <returns></returns>
        public Task<ResultListBase<ClientSession>> GetAllSession(int? local_port = null, string peer_ip = null)
        {
            return BaseRequest()
                .AppendPathSegment("getAllSession")
                .SetQueryParam("local_port", local_port)
                .SetQueryParam("peer_ip", peer_ip)
                .GetJsonAsync<ResultListBase<ClientSession>>();
        }

        /// <summary>
        /// 断开tcp连接，比如说可以断开rtsp、rtmp播放器等
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<ResultBase> KickSession(string id)
        {
            return BaseRequest()
                .AppendPathSegment("kick_session")
                .SetQueryParam("id", id)
                .GetJsonAsync<ResultBase>();
        }

        /// <summary>
        /// 断开tcp连接，比如说可以断开rtsp、rtmp播放器
        /// </summary>
        /// <param name="local_port"></param>
        /// <param name="peer_ip"></param>
        /// <returns></returns>
        public Task<CloseBaseResult> KickSessions(int? local_port = null, string peer_ip = null)
        {
            return BaseRequest()
                .AppendPathSegment("kick_sessions")
                .SetQueryParam("local_port", local_port)
                .SetQueryParam("peer_ip", peer_ip)
                .GetJsonAsync<CloseBaseResult>();
        }

        /// <summary>
        /// 动态添加rtsp/rtmp/hls拉流代理(只支持H264/H265/aac/G711负载)
        /// </summary>
        /// <param name="vhost">添加的流的虚拟主机，例如__defaultVhost__</param>
        /// <param name="app">添加的流的应用名，例如live</param>
        /// <param name="stream">添加的流的id名，例如test</param>
        /// <param name="url">拉流地址，例如rtmp://live.hkstv.hk.lxdns.com/live/hks2</param>
        /// <param name="enable_hls">是否转hls</param>
        /// <param name="enable_mp4">是否mp4录制</param>
        /// <param name="enable_rtsp">是否转rtsp协议</param>
        /// <param name="enable_rtmp">是否转rtmp/flv协议</param>
        /// <param name="enable_ts">是否转http-ts/ws-ts协议</param>
        /// <param name="enable_fmp4">是否转http-fmp4/ws-fmp4协议</param>
        /// <param name="enable_audio">转协议时是否开启音频</param>
        /// <param name="rtp_type">rtsp拉流时，拉流方式，0：tcp，1：udp，2：组播</param>
        /// <param name="timeout_sec">拉流超时时间，单位秒，float类型</param>
        /// <param name="retry_count">拉流重试次数,不传此参数或传值<=0时，则无限重试</param>
        /// <returns></returns>
        public Task<ResultBase<StreamProxy>> AddStreamProxy(
            string vhost,
            string app,
            string stream,
            string url,
            bool enable_hls = false,
            bool enable_mp4 = false,
            bool enable_rtsp = false,
            bool enable_rtmp = false,
            bool enable_ts = false,
            bool enable_fmp4 = false,
            bool enable_audio = true,
            int rtp_type = 0,
            float timeout_sec = 10,
            int retry_count = 0
            )
        {
            return BaseRequest()
                .AppendPathSegment("addStreamProxy")
                .SetQueryParam("vhost", vhost)
                .SetQueryParam("app", app)
                .SetQueryParam("stream", stream)
                .SetQueryParam("url", url)
                .SetQueryParam("enable_hls", enable_hls ? 1 : 0)
                .SetQueryParam("enable_mp4", enable_mp4 ? 1 : 0)
                .SetQueryParam("enable_rtsp", enable_rtsp ? 1 : 0)
                .SetQueryParam("enable_rtmp", enable_rtmp ? 1 : 0)
                .SetQueryParam("enable_ts", enable_ts ? 1 : 0)
                .SetQueryParam("enable_fmp4", enable_fmp4 ? 1 : 0)
                .SetQueryParam("enable_audio", enable_audio ? 1 : 0)
                .SetQueryParam("rtp_type", rtp_type)
                .SetQueryParam("timeout_sec", timeout_sec)
                .SetQueryParam("retry_count", retry_count)
                .GetJsonAsync<ResultBase<StreamProxy>>();
        }

        /// <summary>
        /// 关闭拉流代理
        /// </summary>
        /// <param name="key">addStreamProxy接口返回的key</param>
        /// <returns></returns>
        /// <remarks>流注册成功后，也可以使用close_streams接口替代</remarks>
        public Task<ResultBase<DeleteStreamProxy>> DelStreamProxy(string key)
        {
            return BaseRequest()
                .AppendPathSegment("delStreamProxy")
                .SetQueryParam("key", key)
                .GetJsonAsync<ResultBase<DeleteStreamProxy>>();
        }

        /// <summary>
        /// 通过fork FFmpeg进程的方式拉流代理，支持任意协议
        /// </summary>
        /// <param name="src_url">FFmpeg拉流地址,支持任意协议或格式(只要FFmpeg支持即可)</param>
        /// <param name="dst_url">FFmpeg rtmp推流地址，一般都是推给自己，<para><c>例如:<b>rtmp://127.0.0.1/live/stream_form_ffmpeg</b></c></para></param>
        /// <param name="timeout_ms">FFmpeg推流成功超时时间</param>
        /// <param name="enable_hls">是否开启hls录制</param>
        /// <param name="enable_mp4">是否开启mp4录制</param>
        /// <param name="ffmpeg_cmd_key">FFmpeg命令参数模板，置空则采用配置项:ffmpeg.cmd</param>
        /// <returns></returns>
        public Task<ResultBase<StreamProxy>> AddFFmpegSource(string src_url, string dst_url, int timeout_ms = 5000, bool enable_hls = false, bool enable_mp4 = false, string ffmpeg_cmd_key = null)
        {
            return BaseRequest().AppendPathSegment("addFFmpegSource")
                .SetQueryParam("src_url", src_url, true)
                .SetQueryParam("dst_url", dst_url, true)
                .SetQueryParam("timeout_ms", timeout_ms)
                .SetQueryParam("enable_hls", enable_hls ? 1 : 0)
                .SetQueryParam("enable_mp4", enable_mp4 ? 1 : 0)
                .SetQueryParam("ffmpeg_cmd_key", enable_mp4 ? 1 : 0)
                .GetJsonAsync<ResultBase<StreamProxy>>();
        }

        /// <summary>
        /// 添加rtsp/rtmp推流(addStreamPusherProxy)
        /// </summary>
        /// <param name="schema">推流协议，支持rtsp、rtmp，大小写敏感</param>
        /// <param name="vhost">已注册流的虚拟主机，一般为__defaultVhost__</param>
        /// <param name="app">已注册流的应用名，例如live</param>
        /// <param name="stream">已注册流的id名，例如test</param>
        /// <param name="dst_url">推流地址，需要与schema字段协议一致</param>
        /// <param name="rtp_type">rtsp推流时，推流方式，0：tcp，1：udp</param>
        /// <param name="timeout_sec">推流超时时间，单位秒，float类型</param>
        /// <param name="retry_count">推流重试次数,不传此参数或传值<=0时，则无限重试</param>
        /// <returns></returns>
        public Task<ResultBase<StreamProxy>> AddStreamPusherProxy(string schema, string vhost, string app, string stream, string dst_url, int rtp_type = 0, int timeout_sec = 10, int retry_count = 0)
        {
            return BaseRequest().AppendPathSegment("addStreamPusherProxy")
               .SetQueryParam("schema", schema, true)
               .SetQueryParam("vhost", vhost, true)
               .SetQueryParam("app", app)
               .SetQueryParam("stream", stream)
               .SetQueryParam("dst_url", dst_url)
               .SetQueryParam("rtp_type", rtp_type)
               .SetQueryParam("timeout_sec", timeout_sec)
               .SetQueryParam("retry_count", retry_count)
               .GetJsonAsync<ResultBase<StreamProxy>>();
        }

        /// <summary>
        /// 关闭推流(delStreamPusherProxy)
        /// </summary>
        /// <param name="key">addStreamPusherProxy接口返回的key</param>
        /// <returns></returns>
        public Task<ResultBase<DeleteStreamProxy>> DelStreamPusherProxy(string key)
        {
            return BaseRequest().AppendPathSegment("delStreamPusherProxy")
                .SetQueryParam("key", key)
                .GetJsonAsync<ResultBase<DeleteStreamProxy>>();
        }


        /// <summary>
        /// 关闭ffmpeg拉流代理
        /// </summary>
        /// <param name="key">addStreamProxy接口返回的key</param>
        /// <returns></returns>
        /// <remarks>流注册成功后，也可以使用close_streams接口替代</remarks>
        public Task<ResultBase<DeleteStreamProxy>> DelFFmpegSource(string key)
        {
            return BaseRequest().AppendPathSegment("delFFmpegSource")
                .SetQueryParam("key", key)
                .GetJsonAsync<ResultBase<DeleteStreamProxy>>();
        }

        /// <summary>
        /// 下载可执行文件
        /// </summary>
        /// <returns></returns>
        public Task<System.IO.Stream> DownloadBin()
        {
            return BaseRequest()
                .AppendPathSegment("downloadBin")
                .GetStreamAsync();
        }

        /// <summary>
        /// 判断直播流是否在线
        /// </summary>
        /// <param name="schema">协议，例如 rtsp或rtmp</param>
        /// <param name="vhost">虚拟主机，例如__defaultVhost__</param>
        /// <param name="app">应用名，例如 live</param>
        /// <param name="stream">流id，例如 obs</param>
        /// <returns></returns>
        /// <remarks>已过期，请使用 <see cref="GetMediaList(string, string, string)"/>接口替代</remarks>
        [Obsolete("已过期，请使用getMediaList接口替代")]
        public Task<ResultBase> IsMediaOnline(string schema, string vhost, string app, string stream)
        {
            return BaseRequest().AppendPathSegment("isMediaOnline")
                .SetQueryParam("schema", schema)
                .SetQueryParam("vhost", vhost)
                .SetQueryParam("app", app)
                .SetQueryParam("stream", stream)
                .GetJsonAsync<ResultBase>();
        }

        /// <summary>
        /// 获取流相关信息
        /// </summary>
        /// <param name="schema">协议，例如 rtsp或rtmp</param>
        /// <param name="vhost">虚拟主机，例如__defaultVhost__</param>
        /// <param name="app">应用名，例如 live</param>
        /// <param name="stream">流id，例如 obs</param>
        /// <returns></returns>
        /// <remarks>已过期，请使用 <see cref="GetMediaList(string, string, string)"/>接口替代</remarks>
        [Obsolete("已过期，请使用getMediaList接口替代")]
        public Task<ResultBase<MediaInfo>> GetMediaInfo(string schema, string vhost, string app, string stream)
        {
            return BaseRequest().AppendPathSegment("getMediaInfo")
                .SetQueryParam("schema", schema)
                .SetQueryParam("vhost", vhost)
                .SetQueryParam("app", app)
                .SetQueryParam("stream", stream)
                .GetJsonAsync<ResultBase<MediaInfo>>();
        }


        /// <summary>
        /// 获取rtp代理时的某路ssrc rtp信息
        /// </summary>
        /// <param name="stream_id">RTP的ssrc，16进制字符串或者是流的id(openRtpServer接口指定)</param>
        /// <returns></returns>
        public Task<RtpInfoResult> GetRtpInfo(string stream_id)
        {
            return BaseRequest().AppendPathSegment("getRtpInfo")
                .SetQueryParam("stream_id", stream_id)
                .GetJsonAsync<RtpInfoResult>();
        }

        /// <summary>
        /// 搜索文件系统，获取流对应的录像文件列表或日期文件夹列表
        /// </summary>
        /// <param name="vhost">流的虚拟主机名</param>
        /// <param name="app">流的应用名</param>
        /// <param name="stream">流的ID</param>
        /// <param name="period">流的录像日期，格式为2020-02-01,如果不是完整的日期，那么是搜索录像文件夹列表，否则搜索对应日期下的mp4文件列表</param>
        /// <returns></returns>
        public Task<ResultBase<RecordFile>> GetMp4RecordFile(string vhost, string app, string stream, string period)
        {
            return BaseRequest().AppendPathSegment("getMp4RecordFile")
                .SetQueryParam("vhost", vhost)
                .SetQueryParam("app", app)
                .SetQueryParam("stream", stream)
                .SetQueryParam("period", period)
                .GetJsonAsync<ResultBase<RecordFile>>();
        }

        /// <summary>
        /// 开始录制hls或MP4
        /// </summary>
        /// <param name="type">0为hls，1为mp4</param>
        /// <param name="vhost">流的虚拟主机名</param>
        /// <param name="app">流的应用名</param>
        /// <param name="stream">流的ID</param>
        /// <param name="customized_path">录像保存目录</param>
        /// <param name="max_second">mp4录像切片时间大小,单位秒，置0则采用配置项</param>
        /// <returns></returns>
        public Task<ResultBase> StartRecord(int type, string vhost, string app, string stream, string customized_path = null, int max_second = 0)
        {
            return BaseRequest().AppendPathSegment("startRecord")
                .SetQueryParam("type", type)
                .SetQueryParam("vhost", vhost)
                .SetQueryParam("app", app)
                .SetQueryParam("stream", stream)
                .SetQueryParam("customized_path", customized_path)
                .SetQueryParam("max_second", max_second)
                .GetJsonAsync<ResultBase>();
        }

        /// <summary>
        /// 开始录制hls或MP4
        /// </summary>
        /// <param name="type">0为hls，1为mp4</param>
        /// <param name="vhost">流的虚拟主机名</param>
        /// <param name="app">流的应用名</param>
        /// <param name="stream">流的ID</param>
        /// <returns></returns>
        public Task<ResultBase> StopRecord(int type, string vhost, string app, string stream)
        {
            return BaseRequest().AppendPathSegment("stopRecord")
                .SetQueryParam("type", type)
                .SetQueryParam("vhost", vhost)
                .SetQueryParam("app", app)
                .SetQueryParam("stream", stream)
                .GetJsonAsync<ResultBase>();
        }

        /// <summary>
        /// 获取流录制状态
        /// </summary>
        /// <param name="type">0为hls，1为mp4</param>
        /// <param name="vhost">流的虚拟主机名</param>
        /// <param name="app">流的应用名</param>
        /// <param name="stream">流的ID</param>
        /// <returns></returns>
        public Task<ResultBase> IsRecording(int type, string vhost, string app, string stream)
        {
            return BaseRequest().AppendPathSegment("isRecording")
                .SetQueryParam("type", type)
                .SetQueryParam("vhost", vhost)
                .SetQueryParam("app", app)
                .SetQueryParam("stream", stream)
                .GetJsonAsync<ResultBase>();
        }

        /// <summary>
        /// 获取截图或生成实时截图并返回
        /// </summary>
        /// <param name="url">需要截图的url，可以是本机的，也可以是远程主机的</param>
        /// <param name="timeout_sec">截图失败超时时间，防止FFmpeg一直等待截图</param>
        /// <param name="expire_sec">截图的过期时间，该时间内产生的截图都会作为缓存返回</param>
        /// <returns>jpeg格式的图片，可以在浏览器直接打开</returns>
        public Task<byte[]> GetSnap(string url, int timeout_sec = 5, int expire_sec = 5)
        {
            return BaseRequest().AppendPathSegment("getSnap")
                .SetQueryParam("url", url)
                .SetQueryParam("timeout_sec", timeout_sec)
                .SetQueryParam("expire_sec", expire_sec)
                .GetBytesAsync();
        }

        /// <summary>
        /// 创建GB28181 RTP接收端口，如果该端口接收数据超时，则会自动被回收(不用调用closeRtpServer接口)
        /// </summary>
        /// <param name="port">接收端口，0则为随机端口</param>
        /// <param name="enable_tcp">启用UDP监听的同时是否监听TCP端口</param>
        /// <param name="stream_id">该端口绑定的流ID，该端口只能创建这一个流(而不是根据ssrc创建多个)</param>
        /// <returns></returns>
        public Task<OpenRtpServerResult> OpenRtpServer(int port = 0, bool enable_tcp = false, string stream_id = null)
        {
            return BaseRequest().AppendPathSegment("openRtpServer")
                .SetQueryParam("port", port)
                .SetQueryParam("enable_tcp", enable_tcp)
                .SetQueryParam("stream_id", stream_id)
                .GetJsonAsync<OpenRtpServerResult>();

        }

        /// <summary>
        /// 关闭GB28181 RTP接收端口
        /// </summary>
        /// <param name="stream_id"></param>
        /// <returns></returns>
        public Task<CloseBaseResult> CloseRtpServer(string stream_id)
        {
            return BaseRequest().AppendPathSegment("closeRtpServer")
               .SetQueryParam("stream_id", stream_id)
               .GetJsonAsync<CloseBaseResult>();
        }

        /// <summary>
        /// 获取openRtpServer接口创建的所有RTP服务器
        /// </summary>
        /// <returns></returns>
        public Task<ResultListBase<RtpServer>> ListRtpServer()
        {
            return BaseRequest().AppendPathSegment("listRtpServer")
              .GetJsonAsync<ResultListBase<RtpServer>>();
        }

        /// <summary>
        /// 作为GB28181客户端，启动ps-rtp推流，支持rtp/udp方式；该接口支持rtsp/rtmp等协议转ps-rtp推流。第一次推流失败会直接返回错误，成功一次后，后续失败也将无限重试
        /// </summary>
        /// <param name="vhost">虚拟主机，例如__defaultVhost__</param>
        /// <param name="app">应用名，例如 live</param>
        /// <param name="stream">流id，例如 test</param>
        /// <param name="ssrc">推流的rtp的ssrc</param>
        /// <param name="dst_url">目标ip或域名</param>
        /// <param name="dst_port">目标端口</param>
        /// <param name="is_udp">是否为udp模式,否则为tcp模式</param>
        /// <param name="src_port">使用的本机端口，为0或不传时默认为随机端口</param>
        /// <returns></returns>
        public Task<StartSendRtpResult> StartSendRtp(string vhost, string app, string stream, string ssrc, string dst_url, int dst_port, bool is_udp, int src_port = 0)
        {
            return BaseRequest().AppendPathSegment("startSendRtp")
                .SetQueryParam("vhost", vhost)
                .SetQueryParam("app	", app)
                .SetQueryParam("stream", stream)
                .SetQueryParam("ssrc	", ssrc)
                .SetQueryParam("dst_url", dst_url)
                .SetQueryParam("dst_port", dst_port)
                .SetQueryParam("is_udp", is_udp ? 1 : 0)
                .SetQueryParam("src_port", src_port)
                .GetJsonAsync<StartSendRtpResult>();
        }
        /// <summary>
        /// 停止GB28181 ps-rtp推流
        /// </summary>
        /// <param name="secret">api操作密钥(配置文件配置)，如果操作ip是127.0.0.1，则不需要此参数</param>
        /// <param name="vhost">虚拟主机，例如__defaultVhost__</param>
        /// <param name="app">应用名，例如 live</param>
        /// <param name="stream">流id，例如 test</param>
        /// <param name="ssrc"></param>
        /// <returns></returns>
        public Task<ResultBase> StopSendRtp(string secret, string vhost, string app, string stream, string ssrc = null)
        {
            return BaseRequest().AppendPathSegment("stopSendRtp")
                .SetQueryParam("secret", secret)
                .SetQueryParam("vhost", vhost)
                .SetQueryParam("app	", app)
                .SetQueryParam("stream", stream)
                .SetQueryParam("ssrc", ssrc)
                .GetJsonAsync<ResultBase>();
        }

        /// <summary>
        /// 停止RTP代理超时检查
        /// </summary>
        /// <param name="streamid"></param>
        /// <returns></returns>
        public Task<ResumeRtp> PauseRtpCheck(string streamid)
        {
            return BaseRequest().AppendPathSegment("pauseRtpCheck")
                .SetQueryParam("stream_id", streamid)
                .GetJsonAsync<ResumeRtp>();
        }

        /// <summary>
        /// 恢复RTP代理超时检查
        /// </summary>
        /// <param name="streamid"></param>
        /// <returns></returns>
        public Task<ResumeRtp> ResumeRtpCheck(string streamid)
        {
            return BaseRequest().AppendPathSegment("resumeRtpCheck")
                .SetQueryParam("stream_id", streamid)
                .GetJsonAsync<ResumeRtp>();
        }

        /// <summary>
        /// 获取ZLMediaKit对象统计
        /// </summary>
        /// <returns></returns>
        public Task<ResultBase<StatisticInfo>> GetStatistic()
        {
            return BaseRequest().AppendPathSegment("getStatistic")
                .GetJsonAsync<ResultBase<StatisticInfo>>();
        }
    }
}

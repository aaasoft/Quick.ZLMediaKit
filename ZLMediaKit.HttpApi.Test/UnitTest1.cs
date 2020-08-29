using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ZLMediaKit.HttpApi;
using System;
using System.Xml.XPath;
using ZLMediaKit.HttpApi.Dtos;
using System.IO;

namespace ZLMediaKit.HttpApi.Test
{
    [TestFixture]
    public class Tests
    {
        private readonly ZLHttpClient ZLHttpClient;
        public Tests()
        {
            
            
        }
        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddZLMediaKitHttpClient(option => {
                option.ApiPort = 80;
                option.IpAddress = "172.30.62.231";
            });
        }

        [Test]
        public void get_thread_load()
        {
            var result = new ZLHttpClient().GetThreadsLoad().Result;
            Assert.AreEqual(ZLMediaKit.HttpApi.Dtos.ApiCodeEnum.Success, result.Code, message: "GetWorkThreadsLoad ��֤��ͨ��");
        }

        [Test]
        public void get_work_threads_load()
        {
            var result = new ZLHttpClient().GetWorkThreadsLoad().Result;
            Assert.AreEqual(ZLMediaKit.HttpApi.Dtos.ApiCodeEnum.Success, result.Code, message: "GetWorkThreadsLoad ��֤��ͨ��");
        }

        [Test]
        public void get_server_config()
        {
            var result = new ZLHttpClient().GetServerConfig().Result;
            Assert.IsNotNull(result, message: "GetServerConfig ���Բ�ͨ����δ��ȡ�����");
            Assert.IsNotNull(result.Data, message: "GetServerConfig ���Բ�ͨ����δ��ȡ�����");
            Assert.IsNotNull(result.Data.Api, message: "api �������ȡʧ��");
            Assert.IsNotNull(result.Data.Ffmpeg, message: "ffmpeg �������ȡʧ��");
            Assert.IsNotNull(result.Data.General, message: "general �������ȡʧ��");
            Assert.IsNotNull(result.Data.Hls, message: "hls �������ȡʧ��");
            Assert.IsNotNull(result.Data.Hook, message: "Hook �������ȡʧ��");
            Assert.IsNotNull(result.Data.Http, message: "Http �������ȡʧ��");
            Assert.IsNotNull(result.Data.Multicast, message: "Multicast �������ȡʧ��");
            Assert.IsNotNull(result.Data.Record, message: "Record �������ȡʧ��");
            Assert.IsNotNull(result.Data.Rtmp, message: "Rtmp �������ȡʧ��");
            Assert.IsNotNull(result.Data.Rtp, message: "Rtp �������ȡʧ��");
            Assert.IsNotNull(result.Data.RtpProxy, message: "RtpProxy �������ȡʧ��");
            Assert.IsNotNull(result.Data.Rtsp, message: "Rtsp �������ȡʧ��");
            Assert.IsNotNull(result.Data.Shell, message: "Shell �������ȡʧ��");
        }

        [Test]
        public void set_server_config()
        {
            var client = new ZLHttpClient();
            var result = client.GetServerConfig().Result.Data;
            result.Api.ApiDebug = true;
            result.Ffmpeg.Log = $"./ffmpeg/log/{DateTime.Now.Ticks}.log";
            var serverconfig = client.SetServerConfig(result).Result;
            Assert.AreEqual(ApiCodeEnum.Success, serverconfig.Code, message: "�޸�ServerConfig����ʧ��");
            Assert.AreEqual(1, serverconfig.Changed, message: "�޸�����Ԥ�ڲ���");
        }
        [Test]
        public void re_start_server()
        {
            var client = new ZLHttpClient();
            var result = client.RestartServer().Result ;
            Assert.AreEqual(result.Code, ApiCodeEnum.Success, $"����ʧ��:{result.Msg}");
        }

        [Test]
        public void add_stream_proxy()
        {
            var client = new ZLHttpClient();
            var result = client.AddStreamProxy("__defaultVhost__", "live", "test", "rtsp://admin:admin123@21.36.59.47/h264/ch1/main/av_stream", true, true).Result;
            Assert.AreEqual(result.Code, ApiCodeEnum.Success, $"��Ӳ��Ŵ���ʧ�ܣ�{result.Msg}");
            Assert.IsNotNull(result.Data.Key, message: "�������Ŵ���ʧ��");
        }


        [Test]
        public void GetMediaList()
        {
            var client = new ZLHttpClient();
            var result = client.GetMediaList().Result;
            Assert.AreEqual(result.Code, ApiCodeEnum.Success, $"GetMediaList :{result.Msg}");
        }

        [Test]
        public void getMediaInfo()
        {
            var client = new ZLHttpClient();
            var result = client.GetMediaInfo("rtsp", "__defaultVhost__","live","test").Result;
            Assert.AreEqual(result.Code, ApiCodeEnum.Success, $"GetMediaList :{result.Msg}");
        }



    }
}
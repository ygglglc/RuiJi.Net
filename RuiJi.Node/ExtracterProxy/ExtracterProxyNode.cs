﻿using Newtonsoft.Json;
using RuiJi.Core;
using RuiJi.Core.Utils;
using RuiJi.Node.Extracter;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZooKeeperNet;

namespace RuiJi.Node.ExtracterProxy
{
    public class ExtracterProxyNode : NodeBase
    {
        public ExtracterProxyNode(string baseUrl, string zkServer) : base(baseUrl, zkServer)
        {
            
        }

        protected override void OnStartup()
        {
            var stat = zooKeeper.Exists("/live_nodes/proxy/" + BaseUrl, false);
            if (stat == null)
                zooKeeper.Create("/live_nodes/proxy/" + BaseUrl, "extracter proxy".GetBytes(), Ids.OPEN_ACL_UNSAFE, CreateMode.Ephemeral);

            //create crawler proxy config in zookeeper
            stat = zooKeeper.Exists("/config/proxy/" + BaseUrl, false);
            if (stat == null)
            {
                var d = new 
                {
                    type = "extracter"
                };

                zooKeeper.Create("/config/proxy/" + BaseUrl, JsonConvert.SerializeObject(d).GetBytes(), Ids.OPEN_ACL_UNSAFE, CreateMode.Persistent);
            }

            LoadLiveExtracter();
        }

        public ExtracterConfig GetExtracterConfig(string baseUrl)
        {
            var b = zooKeeper.GetData("/config/crawler/" + baseUrl, false, null);
            var r = System.Text.Encoding.UTF8.GetString(b);
            var d = JsonConvert.DeserializeObject<ExtracterConfig>(r);

            return d;
        }

        protected void LoadLiveExtracter()
        {
            ExtracterManager.Instance.Clear();

            var nodes = zooKeeper.GetChildren("/live_nodes/extracter", new LiveExtracterWatcher(this));

            foreach (var node in nodes)
            {
                ExtracterManager.Instance.AddServer(node);
            }
        }

        protected override void Process(WatchedEvent @event)
        {

        }

        private void ProcessConfig(WatchedEvent @event, string[] segments)
        {
            if (segments.Length == 3)
            {
                var baseUrl = segments[2];

                switch (@event.Type)
                {
                    case EventType.NodeDataChanged:
                        {
                            var d = GetExtracterConfig(baseUrl);
                            ExtracterManager.Instance.AddServer(baseUrl);

                            break;
                        }
                    case EventType.NodeDeleted:
                        {
                            break;
                        }
                }
            }
        }

        class LiveExtracterWatcher : IWatcher
        {
            ExtracterProxyNode node;

            public LiveExtracterWatcher(ExtracterProxyNode node)
            {
                this.node = node;
            }

            public void Process(WatchedEvent @event)
            {
                switch (@event.Type)
                {
                    case EventType.NodeChildrenChanged:
                        {
                            node.LoadLiveExtracter();
                            Console.WriteLine("detected extracter node change");
                            break;
                        }
                }
            }
        }
    }
}
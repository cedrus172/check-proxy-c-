using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckingProxy
{
    public class ProxyMgr
    {
        public static string[] listStringProxy;

        public static List<ProxyInfo> ListProxy = new List<ProxyInfo>();

        public static bool Init()
        {
            bool result = false;
            try
            {
                ListProxy.Clear();
                string path = Directory.GetCurrentDirectory() + "\\proxyList.txt";
                listStringProxy = File.ReadAllLines(path);
                foreach (string proxy in listStringProxy)
                {
                    var proxyInfo = proxy.Split(':');
                    if (proxyInfo.Length < 3)
                    {
                        ProxyInfo prx = new ProxyInfo() { IP = proxyInfo[0], Port = Convert.ToInt32(proxyInfo[1]), isUsing = false, isAuth = false, Username = "", Password = "" };
                        ListProxy.Add(prx);
                    }
                    else
                    {
                        ProxyInfo prx = new ProxyInfo() { IP = proxyInfo[0], Port = Convert.ToInt32(proxyInfo[1]), isUsing = false, isAuth = true, Username = proxyInfo[2], Password = proxyInfo[3] };
                        ListProxy.Add(prx);
                    }

                }
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public static ProxyInfo GetProxyRandom()
        {
            var proxy = ListProxy.FindAll(a => a.isUsing == false);
            if (proxy.Count > 0)
            {
                var rnd = new Random().Next(0, proxy.Count);
                return proxy[rnd];
            }
            else
            {
                var rnd = new Random().Next(0, ListProxy.Count);
                return ListProxy[rnd];
            }

        }
    }
}

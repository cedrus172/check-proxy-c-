using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CheckingProxy
{
    internal class Program
    {
        private static int ProxyLive = 0;
        private static int ProxyDie = 0;

        private static List<string> ListProxyDie = new List<string>();
        private static List<string> ListProxyLive = new List<string>();
        private static List<string> ListProxyLiveFast = new List<string>();
        private static List<string> ListProxyLiveSlow = new List<string>();
        private static List<string> ListIPClear = new List<string>();

        private static List<ProxyInfo> ProxiesLiveFast = new List<ProxyInfo>();
        private static List<ProxyInfo> ProxiesLiveSlow = new List<ProxyInfo>();

        private static string pathLive = Directory.GetCurrentDirectory() + "\\listLive.txt";
        private static string pathDie = Directory.GetCurrentDirectory() + "\\listDie.txt";
        private static string pathLiveFast = Directory.GetCurrentDirectory() + "\\listLiveFast.txt";
        private static string pathLiveSlow = Directory.GetCurrentDirectory() + "\\listLiveSlow.txt";
        private static string pathIPClear = Directory.GetCurrentDirectory() + "\\ipClear.txt";

        public static string URLCheck = "";
        static void Main(string[] args)
        {
            ListIPClear = File.ReadAllLines(pathIPClear).ToList();
            foreach (string ip in ListIPClear)
            {
                Console.WriteLine("IP Clear : " + ip);
            }

            List<ProxyInfo> proxyInfos = new List<ProxyInfo>();

            if (ProxyMgr.Init())
            {
                Console.WriteLine("Init Proxy => Success !");
                Console.WriteLine("Totals Proxy : " + ProxyMgr.ListProxy.Count);
                Console.WriteLine("Enter a URL");
                var url = Console.ReadLine();


                foreach (var proxy in ProxyMgr.ListProxy)
                {
                    if (!ListIPClear.Contains(proxy.IP))
                    {
                        proxyInfos.Add(proxy);
                    }

                }

                StartCheck(url, proxyInfos);

            }
            else
            {
                Console.WriteLine("Init Proxy => Fail ! Please check Proxy List");

            }


            switch (Console.ReadLine())
            {
                case "checkall":
                    StartCheck(URLCheck, proxyInfos);
                    break;
                case "checkfast":
                    StartCheck(URLCheck, ProxiesLiveFast);
                    break;
            }
        }

        public static void Reset()
        {
            ProxyDie = 0;
            ProxyLive = 0;
            ListProxyDie.Clear();
            ListProxyLive.Clear();
            ListProxyLiveFast.Clear();
            ListProxyLiveSlow.Clear();

        }

        public static void StartCheck(string url, List<ProxyInfo> listProxy)
        {
            Reset();
            Console.WriteLine("Checking proxy with URL : " + url);
            URLCheck = url;
           // List<Task> listTask = new List<Task>();
            Parallel.ForEach(listProxy, proxy =>
            {
                CheckProxy(proxy);

            });
         //   Task.WaitAll(listTask.ToArray());
            /*  foreach (ProxyInfo proxy in listProxy)
              {
                  var task = new Task(() =>
                  {
                      CheckProxy(proxy);

                  });
                  task.Start();
                  listTask.Add(task);
              }
              Task.WaitAll(listTask.ToArray());
            */
            Console.WriteLine($"Total Live : {ProxyLive} - Total Die : {ProxyDie} - Total Fast : {ListProxyLiveFast.Count} - Total Slow : {ListProxyLiveSlow.Count}");

            System.IO.File.WriteAllLines(pathLiveFast, ListProxyLiveFast.ToArray());
            System.IO.File.WriteAllLines(pathLiveSlow, ListProxyLiveSlow.ToArray());
            System.IO.File.WriteAllLines(pathLive, ListProxyLive.ToArray());
            System.IO.File.WriteAllLines(pathDie, ListProxyDie.ToArray());


        }


        public static void CheckProxy(ProxyInfo proxy)
        {
            try
            {
                var watcher = Stopwatch.StartNew();
                var client = new RestClient(URLCheck);

                if (!proxy.isAuth)
                {
                    client.Proxy = new System.Net.WebProxy(proxy.IP, proxy.Port) { BypassProxyOnLocal = false };
                }
                else
                {
                    client.Proxy = new System.Net.WebProxy(proxy.IP, proxy.Port) { BypassProxyOnLocal = false, UseDefaultCredentials = false, Credentials = new System.Net.NetworkCredential(proxy.Username, proxy.Password) };

                }
                string token = "eyJhbGciOiJIUzI1NiIsImtpZCI6ImRlbmlleC5jb20iLCJ0eXAiOiJKV1QifQ.eyJ1aWQiOiIxNjA0MzkwNjEwNjE1IiwiZF9pZCI6IkJyb3dzZXIgb24gV2luZG93cyAxMC9jMzU2YWFhYi1kZDlhLTQyMmQtYTc4OS1hZjAyNGQyMDFjMGQiLCJlbWFpbCI6ImNlZHJ1c2lzbWVAZ21haWwuY29tIiwic2lkIjoiYzM1NmFhYWItZGQ5YS00MjJkLWE3ODktYWYwMjRkMjAxYzBkIiwic3lzdGVtIjoiZGVuaWV4LXdlYiIsImV4cCI6MTY0NzcyOTgwNCwiaXNzIjoiZGVuaWV4LmNvbSIsImF1ZCI6ImRlbmlleC13ZWIifQ.1YUX9sox8NzELR-IR0shvHe3BFiTmELs-GhF0u3mYP0";
                client.Timeout = 10000;
                client.UseSynchronizationContext = true;
                var request = new RestRequest("/", Method.GET);
                request.RequestFormat = DataFormat.Json;
                request.AddHeader("Content-type", "application/json");
                request.AddHeader("Cache-Control", "no-store,no-cache");
                request.AddHeader("Accept", "application/json, text/plain, */*");
                //  request.AddHeader("Authorization", "Bearer " + token);
                //   request.AddJsonBody(new { betAccountType = "LIVE", betAmount = "1", betType = "UP" });

                var response = client.Execute(request);
                watcher.Stop();
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{proxy.IP}:{proxy.Port} => DIE");
                    ListProxyDie.Add($"{proxy.IP}:{proxy.Port}:{proxy.Username}:{proxy.Password}");
                    ProxyDie++;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{proxy.IP}:{proxy.Port} => LIVE |" + watcher.ElapsedMilliseconds);
                    if (watcher.ElapsedMilliseconds < 1000)
                    {
                        ListProxyLiveFast.Add($"{proxy.IP}:{proxy.Port}:{proxy.Username}:{proxy.Password}");
                        ProxiesLiveFast.Add(proxy);
                    }
                    else
                    {
                        ListProxyLiveSlow.Add($"{proxy.IP}:{proxy.Port}:{proxy.Username}:{proxy.Password} - {watcher.ElapsedMilliseconds}");
                        ProxiesLiveSlow.Add(proxy);
                    }
                    ListProxyLive.Add($"{proxy.IP}:{proxy.Port}:{proxy.Username}:{proxy.Password}");
                    ProxyLive++;
                }
                request = null;
                client.ClearHandlers();
                client = null;

            }
            catch (Exception ex)
            {

            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace SexNXDemo
{
    /// <summary>
    /// 利用分布式锁实现防止重复提交和秒杀业务 demo
    /// </summary>
    public class Program
    {
        private static readonly string redisConnectionStr = "192.168.1.199:26379,password=123456,connectTimeout=5000,allowAdmin=false,defaultDatabase=1";

        /// <summary>
        /// 防重复提交demo
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //防止重复提交
            //TestRepeatSubmitDemo();

            //秒杀，防超卖
            TestSeckillDemo();


            Console.ReadLine();
        }

        /// <summary>
        /// 
        /// </summary>
        private static void TestRepeatSubmitDemo()
        {
            //客户端请求防重复key，实际业务因根据客户端的token,args,apiPath等诸多参数确定唯一key
            var key = "mdkey001";
            var value = "test1";
            var expireSeconds = 100;
            var csredis = new CSRedis.CSRedisClient(redisConnectionStr);
            csredis.M5_SetNxRemove(key, value);
            for (int i = 0; i < 200; i++)
            {
                //设置锁过期时间，未过期内禁止重复请求
                var lockSuccess = csredis.M5_LockPE(key, value, expireSeconds);
                Console.WriteLine(lockSuccess ? $"{i}请求成功" : $" {i} 请求失败，禁止重复请求");
                Thread.Sleep(30);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static void TestSeckillDemo()
        {
            //模拟线程数
            var thredNumber = 32;
            //模拟单线程请求数
            var requestNumber = 5;
            //秒杀库存数
            var stockNumber = 10;
            
            //秒杀成功队列key
            var key = "order1";
            //分布式锁key
            var nxKey = "order1NX";

            var csredis = new CSRedis.CSRedisClient(redisConnectionStr);
            csredis.Del(key);
            var isEnd = false;

            List<ManualResetEvent> manualEvents = new List<ManualResetEvent>();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < thredNumber; i++)
            {
                ManualResetEvent mre = new ManualResetEvent(false);
                manualEvents.Add(mre);
                Param pra = new Param();
                pra.mrEvent = mre;
                pra.praData = i;

                ThreadPool.QueueUserWorkItem((object state) =>
                {
                    Param pra = (Param)state;
                    var sets = pra.praData;

                    for (int i = 0; i < requestNumber; i++)
                    {
                        Thread.Sleep(50);

                        if (isEnd)
                        {
                            Console.WriteLine($"线程{sets} - 用户{i} 秒杀失败，抢完了。");
                            continue;
                        }

                        //设置客户端的标识，用于解锁
                        var nxSelfMarkvalue = $"thred{sets}_user{i}";
                        //加锁
                        var setnxResult = csredis.M5_LockPE(nxKey, nxSelfMarkvalue, 1000);
                        if (setnxResult)
                        {
                            var len = csredis.LLen(key);
                            //库存不足
                            if (len >= stockNumber)
                            {
                                isEnd = true;
                                stopwatch.Stop();
                                Console.WriteLine($"线程{sets} - 用户{i} 秒杀失败，抢完了。");
                            }
                            else
                            {
                                var value = $"线程{sets}-用户{i}";
                                csredis.LPush(key, value);
                                //解锁 nxSelfMarkvalue，防止误解锁
                                csredis.M5_UnLock(nxKey, nxSelfMarkvalue);
                                Console.WriteLine($"线程{sets} - 用户{i} 秒杀成功。");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"线程{sets} - 用户{i} 系统繁忙，请稍后再试。 秒杀失败。");
                        }

                    }

                    pra.mrEvent.Set();
                    // Console.WriteLine($"线程{sets}已抢购完毕！");

                }, pra);
            }

            WaitHandle.WaitAll(manualEvents.ToArray());
            var lenALL = csredis.LLen(key);
            Console.WriteLine($"\r\n秒杀成功人数：{lenALL} 人,用时：{stopwatch.ElapsedMilliseconds} 毫秒.");
            Console.WriteLine($"\r\n是否超售：{(lenALL > stockNumber ? "是":"否")}");
            Console.WriteLine("\r\n秒杀成功人员名单：");
            for (int i = 0; i < stockNumber; i++)
            {
                Console.WriteLine(csredis.RPop(key));
            }
        }
    }

    public class Param
    {
        public ManualResetEvent mrEvent;
        public int praData;
    }
}

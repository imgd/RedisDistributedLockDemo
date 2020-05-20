using CSRedis;
using System;
using System.Collections.Generic;
using System.Text;

namespace SexNXDemo
{
    /// <summary>
    /// Redis分布式锁可参照：https://github.com/2881099/csredis/issues/104
    /// </summary>
    public static class RedisDistributedLockExtension
    {

        #region 推荐使用

        /*
         *  也可以使用CSRedisClient内置方法 lock 和 unlock        
         */


        /// <summary>
        /// 加锁
        /// </summary>
        /// <param name="client">redis客户端连接</param>
        /// <param name="key">锁key</param>
        /// <param name="value">锁值</param>
        /// <param name="expireSeconds">缓存时间 单位/秒 默认1秒</param>
        /// <returns></returns>
        public static bool M5_Lock(this CSRedisClient client, string key, object value, int expireSeconds = 1)
        {
            //注意未设置锁的过期时间不解锁就成了死锁了

            var script = @"local isNX = redis.call('SETNX', KEYS[1], ARGV[1])
                           if isNX == 1 then
                               redis.call('EXPIRE', KEYS[1], ARGV[2])
                               return 1
                           end
                           return 0";

            return client.Eval(script, key, value, expireSeconds)?.ToString() == "1";
        }

        /// <summary>
        /// 加锁毫秒级
        /// </summary>
        /// <param name="client">redis客户端连接</param>
        /// <param name="key">锁key</param>
        /// <param name="value">锁值</param>
        /// <param name="expireMilliSeconds">缓存时间 单位/毫秒 默认1000毫秒</param>
        /// <returns></returns>
        public static bool M5_LockPE(this CSRedisClient client, string key, object value, int expireMilliSeconds = 1000)
        {
            var script = @"local isNX = redis.call('SETNX', KEYS[1], ARGV[1])
                           if isNX == 1 then
                               redis.call('PEXPIRE', KEYS[1], ARGV[2])
                               return 1
                           end
                           return 0";

            return client.Eval(script, key, value, expireMilliSeconds)?.ToString() == "1";
        }

        /// <summary>
        /// 解锁
        /// </summary>
        /// <param name="client">redis客户端连接</param>
        /// <param name="key">锁key</param>
        /// <param name="selfMark">对应加锁客户端标识</param>
        /// <returns></returns>
        public static bool M5_UnLock(this CSRedisClient client, string key, string selfMark)
        {
            var script = @"local getLock = redis.call('GET', KEYS[1])
                            if getLock == ARGV[1] then
                              redis.call('DEL', KEYS[1])
                              return 1
                            end
                            return 0";

            return client.Eval(script, key, selfMark)?.ToString() == "1";
        }

        #endregion

        #region 不推荐使用

        /// <summary>
        /// 加锁
        /// </summary>
        /// <param name="client"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expireSeconds"></param>
        /// <returns></returns>
        public static bool M5_Set(this CSRedisClient client, string key, object value, int expireSeconds = 1)
        {
            var result = client.Set(key, value, expireSeconds, RedisExistence.Nx);
            return result;
        }

        /// <summary>
        /// 加锁 毫秒级
        /// </summary>
        /// <param name="client"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expireMilliSeconds"></param>
        /// <returns></returns>
        [Obsolete("StartPipe 不适合需要返回值作为判断的场景。参见：https://github.com/2881099/csredis/issues/297", false)]
        public static bool M5_SetPE(this CSRedisClient client, string key, object value, int expireMilliSeconds = 1000)
        {
            var pl = client.StartPipe();
            var result = client.Set(key, value, -1, RedisExistence.Nx);
            if (result)
            {
                client.PExpire(key, expireMilliSeconds);
            }
            pl.EndPipe();
            return result;
        }

        /// <summary>
        /// 加锁 毫秒级
        /// </summary>
        /// <param name="client"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expireMilliSeconds"></param>
        /// <returns></returns>
        [Obsolete("StartPipe 不适合需要返回值作为判断的场景。参见：https://github.com/2881099/csredis/issues/297", false)]
        public static bool M5_SetNxPE(this CSRedisClient client, string key, object value, int expireMilliSeconds = 1000)
        {
            var pl = client.StartPipe();
            var result = client.SetNx(key, value);
            if (result)
            {
                client.PExpire(key, expireMilliSeconds);
            }
            pl.EndPipe();

            return result;
        }

        /// <summary>
        /// 加锁
        /// </summary>
        /// <param name="client"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expireSeconds"></param>
        /// <returns></returns>
        [Obsolete("StartPipe 不适合需要返回值作为判断的场景。参见：https://github.com/2881099/csredis/issues/297", false)]
        public static bool M5_SetNx(this CSRedisClient client, string key, object value, int expireSeconds = 1)
        {
            var pl = client.StartPipe();
            var result = client.SetNx(key, value);
            if (result)
            {
                client.Expire(key, expireSeconds);
            }
            pl.EndPipe();

            return result;
        }

        public static void M5_SetNxRemove(this CSRedisClient client, string key, string selfMark)
        {
            //判断是否是自己的锁
            if (client.Get(key) == selfMark)
            {
                client.Del(key);
            }
        }

        #endregion
    }
}

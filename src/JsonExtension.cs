using System;
using Newtonsoft.Json;

namespace Honasoft.CommonLibrary.Extensions
{
    /// <summary>
    /// json 序列化、反序列化操作 扩展类
    /// </summary>
    public static class JsonExtension
    {
        #region Newtonsoft.Json 实现
        /// <summary>
        /// 对象序列化成Json字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns> 
        public static string M5_ObjectToJson(this object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// 对象序列化成Json字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns> 
        public static string M5_ObjectToJson(this object obj, Formatting format)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            return JsonConvert.SerializeObject(obj, format);
        }

        /// <summary>
        /// 对象尝试序列化成Json字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns> 
        public static ConvertJsonResult M5_TryObjectToJson(this object obj)
        {
            var tryConvertResult = new ConvertJsonResult
            {
                IsConvertSuccess = false,
                ConvertData = string.Empty
            };

            if (obj == null)
                return tryConvertResult;

            try
            {
                tryConvertResult.ConvertData = JsonConvert.SerializeObject(obj);
                tryConvertResult.IsConvertSuccess = true;
            }
            catch (Exception) { }

            return tryConvertResult;
        }



        /// <summary>
        /// 对象尝试序列化成Json字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns> 
        public static ConvertJsonResult M5_TryObjectToJson(this object obj, Formatting format)
        {
            var tryConvertResult = new ConvertJsonResult
            {
                IsConvertSuccess = false,
                ConvertData = string.Empty
            };

            if (obj == null)
                return tryConvertResult;

            try
            {
                tryConvertResult.ConvertData = JsonConvert.SerializeObject(obj, format);
                tryConvertResult.IsConvertSuccess = true;
            }
            catch (Exception) { }

            return tryConvertResult;

        }


        /// <summary>
        /// Json字符串序列化成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T M5_JsonToObject<T>(this string obj) where T : class
        {
            if (obj == null)
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(obj);
        }

        /// <summary>
        /// Json字符串尝试序列化成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ConvertJsonResult<T> M5_TryJsonToObject<T>(this string obj) where T : class
        {
            var tryConvertResult = new ConvertJsonResult<T>
            {
                IsConvertSuccess = false,
                ConvertData = default(T)
            };

            if (obj == null)
                return tryConvertResult;

            try
            {
                tryConvertResult.ConvertData = JsonConvert.DeserializeObject<T>(obj);
                tryConvertResult.IsConvertSuccess = true;
            }
            catch (Exception) { }

            return tryConvertResult;
        }


#if !NET35
        /// <summary>
        /// JSON dynamic 对象 序列化成实体对象
        /// </summary>
        /// <typeparam name="T">需要返回的实例类型</typeparam>
        /// <param name="json">需要反序列化的json字符串</param>
        /// <returns></returns>
        public static T M5_JsonToObject<T>(dynamic json) where T : class
        {
            if (json == null)
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(Convert.ToString(json));
        }

        /// <summary>
        /// JSON dynamic 对象 尝试序列化成实体对象
        /// </summary>
        /// <typeparam name="T">需要返回的实例类型</typeparam>
        /// <param name="json">需要反序列化的json字符串</param>
        /// <returns></returns>
        public static ConvertJsonResult<T> M5_TryJsonToObject<T>(dynamic json) where T : class
        {
            var tryConvertResult = new ConvertJsonResult<T>
            {
                IsConvertSuccess = false,
                ConvertData = default(T)
            };

            if (json == null)
                return tryConvertResult;

            try
            {
                tryConvertResult.ConvertData = JsonConvert.DeserializeObject<T>(Convert.ToString(json));
                tryConvertResult.IsConvertSuccess = true;
            }
            catch (Exception) { }

            return tryConvertResult;
        }

#endif

        #endregion

    }

    public class ConvertJsonResult
    {
        public bool IsConvertSuccess { get; set; }

        public string ConvertData { get; set; }
    }
    public class ConvertJsonResult<T>
    {
        public bool IsConvertSuccess { get; set; }

        public T ConvertData { get; set; }
    }
}

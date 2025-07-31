using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDPro.Utilities
{
    public static class DateTimeUtil
    {

        /// <summary>
        /// 日期比较:<=
        /// </summary>
        /// <param name="fromD"></param>
        /// <param name="endD"></param>
        /// <returns></returns>
        public static bool Earlier(this DateTime? fromD, DateTime? endD)
        {
            if (fromD == null || endD == null)
            {
                return false;
            }
            else
            {
                //小于0 t1 早于 t2。0 t1 与t2 相同。大于0 t1 晚于 t2 
                return DateTime.Compare((DateTime)fromD, (DateTime)endD) <= 0;
            }
        }

        /// <summary>
        /// 将dateTime格式转换为Unix时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        [Obsolete]
        public static long DateTimeToUnixTime(DateTime dateTime, bool isMinseconds = false)
        {
            TimeSpan ts = (dateTime - TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)));

            return Convert.ToInt64(isMinseconds ? ts.TotalMilliseconds : ts.TotalSeconds);
        }



        /// <summary>
        /// 将Unix时间戳转换为dateTime格式
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        [Obsolete]
        public static DateTime UnixTimeToDateTime(int time)
        {
            if (time < 0)
                throw new ArgumentOutOfRangeException("time is out of range");

            return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(time);
        }

        public static long GetTimestamp(bool isMinseconds = false)
        {
            DateTime dateTime = DateTimeOffset.Now.LocalDateTime ;

            if (isMinseconds)
            {
                return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
            }
            return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();

        }


        public static long GetTimestamp(DateTime dateTime, bool isMinseconds = false)
        {

            if (isMinseconds)
            {
                return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
            }
            return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();

        }


        public static DateTime GetDateTime( long HDProtamp, bool isMinseconds = false)
        {

            if (isMinseconds)
            {
              return  DateTimeOffset.FromUnixTimeMilliseconds(HDProtamp).LocalDateTime;

            }

            DateTimeOffset d = DateTimeOffset.FromUnixTimeSeconds(HDProtamp);

            return d.LocalDateTime;

        }


    }
}

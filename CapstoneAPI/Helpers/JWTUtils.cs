using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Helpers
{
    public static class JWTUtils
    {
        public static string CalculateTimeAgo(DateTime recordDate)
        {
            var ts = new TimeSpan(DateTime.UtcNow.Ticks - recordDate.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 1 * Consts.MINUTE)
                return ts.Seconds + " giây trước";

            if (delta < 60 * Consts.MINUTE)
                return ts.Minutes + " phút trước";

            if (delta < 24 * Consts.HOUR)
                return ts.Hours + " giờ trước";

            if (delta < 30 * Consts.DAY)
                return ts.Days + " ngày trước";

            if (delta < 12 * Consts.MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months + " tháng trước";
            }
            else
            {
                int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years + " năm trước";
            }
        }
    }
}

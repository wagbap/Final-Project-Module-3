namespace eConsultas_MVC.Utils
{
    public class TimeAgo    
    {


        public static string GetTimeAgo(DateTime timestamp)
        {
            var currentTime = DateTime.Now;
            var difference = currentTime - timestamp;
            var minutesAgo = difference.TotalMinutes;

            string timeAgo = string.Empty;

            if (minutesAgo < 60)
            {
                timeAgo = $"{Math.Round(minutesAgo)} mins ago";
            }
            else if (minutesAgo < 1440) // Less than 24 hours (1 day)
            {
                var hoursAgo = Math.Floor(minutesAgo / 60);
                timeAgo = $"{hoursAgo} {(hoursAgo == 1 ? "hour" : "hours")} ago";
            }
            else
            {
                var daysAgo = Math.Floor(minutesAgo / 1440);
                timeAgo = $"{daysAgo} {(daysAgo == 1 ? "day" : "days")} ago";
            }

            return timeAgo;
        }
    }
}

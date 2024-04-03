using System;

namespace Mapp.DataAccess
{
    public interface IDateTimeManager
    {
        DateTime Today { get; }
        DateTime Now { get; }
    }

    public class DateTimeManager : IDateTimeManager
    {
        public DateTime Today => DateTime.Today;

        public DateTime Now => DateTime.Now;
    }

}
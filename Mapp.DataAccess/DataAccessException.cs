using System;

namespace Shmap.DataAccess
{
    public class DataAccessException:Exception
    {
        public DataAccessException()
        {
        }

        public DataAccessException(string message)
            : base(message)
        {
        }

        public DataAccessException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }

    public class SettingsDataAccessException : DataAccessException
    {
        public SettingsDataAccessException()
        {
        }

        public SettingsDataAccessException(string message)
            : base(message)
        {
        }

        public SettingsDataAccessException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}
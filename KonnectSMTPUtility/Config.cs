namespace KonnectSMTPUtility
{
   
    public class Config
    {

        public Logging Logging { get; set; }
        public Socket Socket { get; set; }
        public Smtp Smtp { get; set; }
        

    }

    public class Logging
    {
        public LogLevel LogLevel { get; set; }
    }
    public class LogLevel
    {
        public string Default { get; set; }
        public string Microsoft { get; set; }
        public string Microsoft_Hosting_Lifetime { get; set; }
    }

    public class Socket
    {
        public string Host { get; set; }


    }
    public class Smtp
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }


}

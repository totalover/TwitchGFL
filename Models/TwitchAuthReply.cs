using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchGFL.Models
{
    public class TwitchAuthReply
    {
        public DateTime RecivedTime { get; set; }
        public string device_code { get; set; }
        public int expires_in { get; set; }
        public int interval { get; set; }
        public string user_code { get; set; }
        public string verification_uri { get; set; }

        public TwitchAuthReply() 
        {
            RecivedTime = DateTime.Now;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckingProxy
{
    public class ProxyInfo
    {
        public string IP { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public bool isAuth { get; set; }
        public bool isUsing { get; set; }
    }
}

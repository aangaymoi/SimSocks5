using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Configuration;
using Helpers;
using System.Windows.Forms;
using System.Threading;

namespace Socks5
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            int p = ConfigurationManager.AppSettings["p"].ToInt();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "/p")
                {
                    p = args[++i].ToInt();
                }
            }

            Socks5 s = new Socks5(p)
            {
                FilterFules = ConfigurationManager.AppSettings["rules"]
            };
            s.StartNew();

            ManualResetEvent mre = new ManualResetEvent(false);
            mre.WaitOne();
        }
    }
}

using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
/*using Admin;*/

namespace Helpers
{
    struct TcpKeepAlive
    {
        public int OnOff;
        public int KeepAliveTime;
        public int KeepAliveInterval;

        public unsafe byte[] Buffer
        {
            get
            {
                var buf = new byte[sizeof(TcpKeepAlive)];
                fixed (void* p = &this) Marshal.Copy(new IntPtr(p), buf, 0, buf.Length);
                return buf;
            }
        }
    };

    static class SocketExtensions
    {
        public static void Listen(this Socket acceptSocket, int p, AsyncCallback callback)
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, p);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                acceptSocket.Bind(localEndPoint);
                acceptSocket.Listen(int.MaxValue);
                acceptSocket.BeginAccept(callback, acceptSocket);
            }
            catch { }
        }

        /// <summary>
        /// Is socket connected?
        /// </summary>
        /// <param name="s">Socket</param>
        /// <returns>bool</returns>
        public static bool IsConnected(this Socket s)
        {
            try
            {
                return !(s.Available == 0 && s.Poll(1, SelectMode.SelectRead));
            }
            catch (SocketException) { return false; }
        }

        /// <summary>
        /// SetKeepAliveValues
        /// </summary>
        /// <param name="s">Socket</param>
        /// <param name="On_Off">bool</param>
        /// <param name="KeepaLiveTime">uint</param>
        /// <param name="KeepaLiveInterval">uint</param>
        /// <returns></returns>
        public static int SetKeepAliveValues(
            this Socket s
            , bool onOff
            , int keepAliveTime
            , int keepAliveInterval)
        {
            int Result = -1;

            unsafe
            {
                TcpKeepAlive KeepAliveValues = new TcpKeepAlive();

                KeepAliveValues.OnOff = 1;
                KeepAliveValues.KeepAliveTime = keepAliveTime;
                KeepAliveValues.KeepAliveInterval = keepAliveInterval;

                Result = s.IOControl(IOControlCode.KeepAliveValues, KeepAliveValues.Buffer, null);
            }

            return Result;
        }

        /// <summary>
        /// public static void ConfigureTcpSocket(this Socket s)
        /// </summary>
        /// <param name="s">Socket</param>
        public static void ConfigureTcpSocket(this Socket s)
        {
            // Don't allow another socket to bind to this port.
            /*s.ExclusiveAddressUse = true;*/

            // The socket will linger for 10 seconds after  
            // Socket.Close is called.
            /*s.LingerState = new LingerOption(true, 0);*/

            // Disable the Nagle Algorithm for this tcp socket.
            s.NoDelay = true;

            // Set the receive buffer size to 8k
            /*s.ReceiveBufferSize = 8192;*/

            // Set the timeout for synchronous receive methods to  
            // 1 second (1000 milliseconds.)
            /*s.ReceiveTimeout = 1000;*/

            // Set the send buffer size to 8k.
            /*s.SendBufferSize = 8192;*/

            // Set the timeout for synchronous send methods 
            // to 1 second (1000 milliseconds.)			
            /*s.SendTimeout = 1000;*/

            // Set the Time To Live (TTL) to 42 router hops.
            /*s.Ttl = 42;*/
        }

        /// <summary>
        /// public static void SetKACOption(this Socket s)
        /// </summary>
        /// <param name="s">Socket</param>
        public static void SetKACOption(this Socket s)
        {
            s.SetKACOption(30000, 1000);
        }

        /// <summary>
        /// public static void SetKACOption(this Socket s, int keepAliveTime, int keepAliveInterval)
        /// </summary>
        /// <param name="s"></param>
        /// <param name="keepAliveTime"></param>
        /// <param name="keepAliveInterval"></param>
        public static void SetKACOption(this Socket s, int keepAliveTime, int keepAliveInterval)
        {
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);
            s.SetKeepAliveValues(true, keepAliveTime, keepAliveInterval);
            s.ConfigureTcpSocket();
        }

        #region -- Socks5 --

        //public static IAsyncResult BeginConnect(this Socket s
        //    , Config config
        //    , StateObject state
        //    , AsyncCallback socks5
        //    , AsyncCallback callback)
        //{
        //    return s.BeginConnect(config.HasSocks5, config.Socks5Ep, config.DestEp, state, socks5, callback);
        //}

        //public static IAsyncResult BeginConnect(this Socket s
        //    , bool hasSocks5
        //    , IPEndPoint socks5ep
        //    , IPEndPoint ep
        //    , StateObject state
        //    , AsyncCallback socks5
        //    , AsyncCallback callback)
        //{
        //    if (hasSocks5)
        //    {
        //        return s.BeginConnect(socks5ep, socks5, state);
        //    }

        //    return s.BeginConnect(ep, callback, state);
        //} 
        
        #endregion

        /// <summary>
        /// public static void ShutdownClose(this Socket s)
        /// </summary>
        /// <param name="s">Socket</param>
        public static void ShutdownClose(this Socket s)
        {
            if (s != null)
            {
                try
                {
                    s.Shutdown(SocketShutdown.Both);
                    s.Close();
                }
                catch /*(System.Exception ex)*/
                {
                }

                s = null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Helpers;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Socks5
{
    public class Socks5
    {
        //private ManualResetEvent _allDoneSocks5;
        int _port;

        /// <summary>
        /// public Socks5(int port)
        /// </summary>
        /// <param name="port">int</param>
        public Socks5(int port)
        {
            _port = port;
            //_allDoneSocks5 = new ManualResetEvent(false);
        }

        Socket _acceptSocket = new Socket(
                AddressFamily.InterNetwork
                , SocketType.Stream
                , ProtocolType.Tcp);

        public void StartNew()
        {
            _acceptSocket.Listen(_port, OnNewSocks5Accept);
        }

        /// <summary>
        /// public void OnSocks5Accept(IAsyncResult ar)
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        public void OnNewSocks5Accept(IAsyncResult ar)
        {
            try
            {
                Socket clientConnection = _acceptSocket.EndAccept(ar);
                if (clientConnection != null)
                {
                    clientConnection.SetKACOption();

                    // Create the state object.
                    Socks5State state = new Socks5State();
                    state.ClientSocket = clientConnection;

                    try
                    {
                        clientConnection.BeginReceive(state.ClientBuffer, 0, 3, 0, OnSocks5RecvAuth, state);
                    }
                    catch
                    {
                        clientConnection.ShutdownClose();
                        state.Dispose();
                    }
                }
            }
            catch {}

            try
            {
                _acceptSocket.BeginAccept(OnNewSocks5Accept, _acceptSocket);
            }
            catch
            {
                _acceptSocket.ShutdownClose();
            }
        }

        /// <summary>
        /// public void Start()
        /// </summary>
//        public void Start()
//        {
//            /*Create a TCP/IP socket.*/
//            Socket listener = new Socket(
//                AddressFamily.InterNetwork
//                , SocketType.Stream
//                , ProtocolType.Tcp);

//            /*Bind the socket to the local endpoint and listen for incoming connections.*/
//            try
//            {
//                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, _port);

//                listener.Bind(localEndPoint);
//                listener.Listen(int.MaxValue);

//                while (true)
//                {
//                    _allDoneSocks5.Reset();
//                    listener.BeginAccept(new AsyncCallback(OnSocks5Accept), listener);

//                    _allDoneSocks5.WaitOne();
//                }
//            }
//            catch (Exception e)
//            {
//                MessageBox.Show(e.ToString(), "Warning :-'(", MessageBoxButtons.OK, MessageBoxIcon.Warning);
//            }
//        }

//        /// <summary>
//        /// public void OnSocks5Accept(IAsyncResult ar)
//        /// </summary>
//        /// <param name="ar">IAsyncResult</param>
//        public void OnSocks5Accept(IAsyncResult ar)
//        {
//            try
//            {
//                // Signal the main thread to continue.
//                _allDoneSocks5.Set();

//                // Get the socket that handles the client request.
//                Socket listener = (Socket)ar.AsyncState;
//                Socket clientConnection = listener.EndAccept(ar);

//                /*handler.SetKACOption(30 * 60 * 1000, 1000);*/
//                clientConnection.SetKACOption();

//                // Create the state object.
//                Socks5State state = new Socks5State();
//                state.ClientSocket = clientConnection;

//                try
//                {
//                    clientConnection.BeginReceive(state.ClientBuffer, 0, 3, 0, OnSocks5RecvAuth, state);
//                    return;
//                }
//                catch { }

//                clientConnection.ShutdownClose();
//                state.Dispose();
//            }
//#if DEBUG
//            catch (System.Exception ex)
//            {
//                if (ex is ObjectDisposedException)
//                {
//                    return;
//                }

//                Console.WriteLine(ex.ToString());
//            }
//#else
//            catch { }
//#endif
//        }

        /// <summary>
        /// public void OnSocks5RecvAuth(IAsyncResult ar)
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        public void OnSocks5RecvAuth(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                Socks5State state = (Socks5State)ar.AsyncState;
                Socket clientConnection = state.ClientSocket;

                SocketError errorCode;

                // Read data from the client socket.
                int bytesRead = clientConnection.EndReceive(ar, out errorCode);

                if (errorCode == SocketError.Success)
                {
                    if (bytesRead == 3)
                    {
                        try
                        {
                            /*if (state.Buffer[0] == 0x5 && state.Buffer[1] == 0x1 && state.Buffer[2] == 0x0)*/
                            if (state.ClientBuffer[0] == 0x5)
                            {
                                if (state.ClientBuffer[1] == 0x1)
                                {
                                    clientConnection.BeginSend(new byte[] { 0x5, 0x0 /*No authenticate*/ }, 0, 2, 0, OnSocks5SendAuth, state);
                                    return;
                                }
                                else if (state.ClientBuffer[1] == 0x2)
                                {
                                    clientConnection.BeginReceive(state.ClientBuffer, 0, 1, 0, OnSocks5RecvAuthSkip, state);
                                    return;
                                }
                            }
                        }
                        catch { }
                    }
                }
                
                clientConnection.ShutdownClose();
                state.Dispose();
            }
#if DEBUG
            catch (System.Exception ex)
            {
                if (ex is ObjectDisposedException)
                {
                    return;
                }

                Console.WriteLine(ex.ToString());
            }
#else
            catch { }
#endif
        }

        public void OnSocks5RecvAuthSkip(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                Socks5State state = (Socks5State)ar.AsyncState;
                Socket clientConnection = state.ClientSocket;

                SocketError errorCode;

                // Read data from the client socket.
                int bytesRead = clientConnection.EndReceive(ar, out errorCode);

                if (errorCode == SocketError.Success)
                {
                    if (bytesRead == 1)
                    {
                        try
                        {
                            clientConnection.BeginSend(new byte[] { 0x5, 0x0 /*No authenticate*/ }, 0, 2, 0, OnSocks5SendAuth, state);
                            return;
                        }
                        catch { }
                    }
                }

                clientConnection.ShutdownClose();
                state.Dispose();
            }
#if DEBUG
            catch (System.Exception ex)
            {
                if (ex is ObjectDisposedException)
                {
                    return;
                }

                Console.WriteLine(ex.ToString());
            }
#else
            catch { }
#endif
        }

        /// <summary>
        /// public void OnSocks5SendAuth(IAsyncResult ar)
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        public void OnSocks5SendAuth(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                Socks5State state = (Socks5State)ar.AsyncState;
                Socket clientConnection = state.ClientSocket;

                SocketError errorCode;

                // Read data from the client socket.
                int bytesSent = clientConnection.EndSend(ar, out errorCode);

                if (errorCode == SocketError.Success)
                {
                    if (bytesSent == 2)
                    {
                        try
                        {
                            clientConnection.BeginReceive(state.ClientBuffer, 0, Socks5State.RECV_BUFFER_SZ, 0, OnSocks5RecvRQ, state);
                            return;
                        }
                        catch { }
                    }
                }

                clientConnection.ShutdownClose();
                state.Dispose();
            }
#if DEBUG
            catch (System.Exception ex)
            {
                if (ex is ObjectDisposedException)
                {
                    return;
                }

                Console.WriteLine(ex.ToString());
            }

#else
            catch { }
#endif
        }

        public string FilterFules { get; set; }

        /// <summary>
        /// public void OnSocks5RecvRQ(IAsyncResult ar)
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        public void OnSocks5RecvRQ(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                Socks5State state = (Socks5State)ar.AsyncState;
                Socket clientConnection = state.ClientSocket;

                SocketError errorCode;

                // Read data from the client socket.
                int bytesRead = clientConnection.EndReceive(ar, out errorCode);

                if (errorCode == SocketError.Success)
                {
                    if (bytesRead > 0)
                    {
                        try
                        {
                            /*Check request*/
                            if (isValidQuery(bytesRead, state.ClientBuffer))
                            {
                                switch (state.ClientBuffer[1])
                                {
                                    /*CONNECT*/
                                    case 1:
                                        {
                                            IPAddress remoteIP = null;
                                            int remotePort = 0;
                                            string remoteHost = "";

                                            bool isIP = state.ClientBuffer[3] == 1;
                                            bool isDM = state.ClientBuffer[3] == 3;

                                            /*IPv4*/
                                            if (isIP)
                                            {
                                                remoteHost = String.Format("{0}.{1}.{2}.{3}", state.ClientBuffer[4]
                                                                           , state.ClientBuffer[5]
                                                                           , state.ClientBuffer[6]
                                                                           , state.ClientBuffer[7]);

                                                remoteIP = IPAddress.Parse(remoteHost);
                                                remotePort = state.ClientBuffer[8] * 256 + state.ClientBuffer[9];
                                            }
                                            /*Domain*/
                                            else if (isDM)
                                            {
                                                remoteHost = Encoding.ASCII.GetString(state.ClientBuffer, 5, state.ClientBuffer[4]);
                                                remotePort = state.ClientBuffer[4] + 5;
                                                remotePort = state.ClientBuffer[remotePort] * 256 + state.ClientBuffer[remotePort + 1];
                                            }
                                            /*IPV6*/
                                            else
                                            {
                                                /*No more implement.*/
                                                break;
                                            }

                                            if (!FilterFules.IsNullOrEmpty())
                                            {
                                                /*Apply filter*/
                                                /*[\w]*.mozilla.org|[\w]*.mozilla.com*/
                                                if (Regex.IsMatch(remoteHost, FilterFules))
                                                {
                                                    break;
                                                }
                                            }

                                            if (isDM)
                                            {
                                                try { 
                                                    remoteIP = Dns.GetHostAddresses(remoteHost)[0];
                                                }
                                                catch {
                                                    /*GetHostAddresses failed -> nothing to do*/
                                                    break;
                                                }
                                            }

                                            /*BeginConnect*/
                                            Socket remoteConnection = new Socket(remoteIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                                            /*remoteConnection.SetKACOption(30 * 60 * 1000, 1000);*/
                                            remoteConnection.SetKACOption();

                                            state.RemoteSocket = remoteConnection;

                                            remoteConnection.BeginConnect(new IPEndPoint(remoteIP, remotePort) , OnRemoteConnected, state);
                                        }
                                        return;

                                    /*BIND*/
                                    case 2:
                                        {
                                        }
                                        break;

                                    /*ASSOCIATE*/
                                    case 3:
                                        {
                                        }
                                        break;

                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                /*No more handle.*/
                            }
                        }
                        catch { }
                    }
                }

                state.RemoteSocket.ShutdownClose();
                state.ClientSocket.ShutdownClose();
                state.Dispose();
            }

#if DEBUG
            catch (System.Exception ex)
            {
                if (ex is ObjectDisposedException)
                {
                    return;
                }

                Console.WriteLine(ex.ToString());
            }
#else
            catch { }
#endif
        }

        /// <summary>
        /// private bool isValidQuery(int sz, byte[] query)
        /// </summary>
        /// <param name="sz">int</param>
        /// <param name="query">byte[]</param>
        /// <returns>bool</returns>
        private bool isValidQuery(int sz, byte[] query)
        {
            try
            {
                switch (query[3])
                {
                    case 1: /*IPv4 address*/
                        return (sz == 10);

                    case 3: /*Domain name*/
                        return (sz == query[4] + 7);

                    case 4: /*IPv6 address*/
                        /*Not supported*/
                        return false;

                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// public void OnRemoteConnected(IAsyncResult ar)
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        public void OnRemoteConnected(IAsyncResult ar)
        {
            Socks5State state = (Socks5State)ar.AsyncState;
            Socket clientConnection = state.ClientSocket;
            Socket remoteConnection = state.RemoteSocket;

            byte[] toSend = new byte[] { 5, 1 /*Connect failed*/, 0, 1 /*IPv4*/, 0, 0, 0, 0, 0, 0 };

            try
            {
                remoteConnection.EndConnect(ar);
                toSend = new byte[] { 5, 0, 0, 1 /*IPv4*/,
                                      (byte)(((IPEndPoint)remoteConnection.LocalEndPoint).Address.Address % 256.0),
                                      (byte)(Math.Floor((((IPEndPoint)remoteConnection.LocalEndPoint).Address.Address % 65536.0) / 256)),
                                      (byte)(Math.Floor((((IPEndPoint)remoteConnection.LocalEndPoint).Address.Address % 16777216.0) / 65536)),
                                      (byte)(Math.Floor(((IPEndPoint)remoteConnection.LocalEndPoint).Address.Address / 16777216.0)),
                                      (byte)(Math.Floor(((IPEndPoint)remoteConnection.LocalEndPoint).Port / 256.0)),
                                      (byte)(((IPEndPoint)remoteConnection.LocalEndPoint).Port % 256)
                                    };
            }
#if DEBUG
            catch (System.Exception ex)
            {
                if (ex is ObjectDisposedException)
                {
                    return;
                }

                Console.WriteLine(ex.ToString());
            }
#else
            catch { }
#endif

            try
            {
                clientConnection.BeginSend(toSend, 0, toSend.Length
                                           , SocketFlags.None, OnSocks5ResponseRQ, state);
                return;
            }
#if DEBUG
            catch (System.Exception ex)
            {
                if (ex is ObjectDisposedException)
                {
                    return;
                }

                Console.WriteLine(ex.ToString());
            }

#else
            catch { }
#endif
            state.Dispose();
        }

        /// <summary>
        /// public void OnSocks5ResponseRQ(IAsyncResult ar)
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        public void OnSocks5ResponseRQ(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                Socks5State state = (Socks5State)ar.AsyncState;
                Socket clientConnection = state.ClientSocket;
                Socket remoteConnection = state.RemoteSocket;

                SocketError errorCode;

                // Read data from the client socket.
                int bytesSent = clientConnection.EndSend(ar, out errorCode);

                if (errorCode == SocketError.Success)
                {
                    if (bytesSent > 0)
                    {
                        try
                        {
                            clientConnection.BeginReceive(state.ClientBuffer, 0, Socks5State.RECV_BUFFER_SZ, 0, OnClientRecv, state);
                            remoteConnection.BeginReceive(state.RemoteBuffer, 0, Socks5State.RECV_BUFFER_SZ, 0, OnRemoteRecv, state);
                            return;
                        }
                        catch { }
                    }
                }

                state.Dispose();
            }

#if DEBUG
            catch (System.Exception ex)
            {
                if (ex is ObjectDisposedException)
                {
                    return;
                }

                Console.WriteLine(ex.ToString());
            }
#else
            catch { }
#endif
        }

        /// <summary>
        /// public void OnClientRecv(IAsyncResult ar)
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        public void OnClientRecv(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                Socks5State state = (Socks5State)ar.AsyncState;
                Socket clientConnection = state.ClientSocket;
                Socket remoteConnection = state.RemoteSocket;

                SocketError errorCode;

                // Read data from the client socket.
                int bytesRead = clientConnection.EndReceive(ar, out errorCode);

                if (errorCode == SocketError.Success)
                {
                    if (bytesRead > 0)
                    {
                        try
                        {
                            state.NumberOfBytesToSend = bytesRead;
                            remoteConnection.BeginSend(state.ClientBuffer, 0, state.SendLength, 0, OnRemoteSend, state);
                            return;
                        }
                        catch { }
                    }
                }

                state.Dispose();
            }
#if DEBUG
            catch (System.Exception ex)
            {
                if (ex is ObjectDisposedException)
                {
                    return;
                }

                Console.WriteLine(ex.ToString());
            }
#else
            catch { }
#endif
        }

        /// <summary>
        /// public void OnRemoteSend(IAsyncResult ar)
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        public void OnRemoteSend(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                Socks5State state = (Socks5State)ar.AsyncState;
                Socket clientConnection = state.ClientSocket;
                Socket remoteConnection = state.RemoteSocket;

                SocketError errorCode;

                // Read data from the client socket.
                int bytesSent = remoteConnection.EndSend(ar, out errorCode);

                if (errorCode == SocketError.Success)
                {
                    if (bytesSent > 0)
                    {
                        state.BytesSent += bytesSent;

                        try
                        {
                            if (state.HasSent())
                            {
                                clientConnection.BeginReceive(state.ClientBuffer, 0, Socks5State.RECV_BUFFER_SZ, 0, OnClientRecv, state);
                            }
                            else
                            {
                                remoteConnection.BeginSend(state.ClientBuffer, state.BytesSent, state.SendLength, 0, OnRemoteSend, state);
                            }

                            return;
                        }
                        catch { }
                    }
                }

                state.Dispose();
            }

#if DEBUG
            catch (System.Exception ex)
            {
                if (ex is ObjectDisposedException)
                {
                    return;
                }

                Console.WriteLine(ex.ToString());
            }
#else
            catch { }
#endif
        }

        /// <summary>
        /// public void OnRemoteRecv(IAsyncResult ar)
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        public void OnRemoteRecv(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                Socks5State state = (Socks5State)ar.AsyncState;
                Socket clientConnection = state.ClientSocket;
                Socket remoteConnection = state.RemoteSocket;

                SocketError errorCode;

                // Read data from the client socket.
                int bytesRead = remoteConnection.EndReceive(ar, out errorCode);

                if (errorCode == SocketError.Success)
                {
                    if (bytesRead > 0)
                    {
                        try
                        {
                            state.NumberOfBytesToSendR = bytesRead;
                            clientConnection.BeginSend(state.RemoteBuffer, 0, state.SendLengthR, 0, OnClientSend, state);
                            return;
                        }
                        catch { }
                    }
                }

                state.Dispose();
            }
#if DEBUG
            catch (System.Exception ex)
            {
                if (ex is ObjectDisposedException)
                {
                    return;
                }

                Console.WriteLine(ex.ToString());
            }
#else
            catch { }
#endif
        }

        /// <summary>
        /// public void OnClientSend(IAsyncResult ar)
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        public void OnClientSend(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                Socks5State state = (Socks5State)ar.AsyncState;
                Socket clientConnection = state.ClientSocket;
                Socket remoteConnection = state.RemoteSocket;

                SocketError errorCode;

                // Read data from the client socket.
                int bytesSent = clientConnection.EndSend(ar, out errorCode);

                if (errorCode == SocketError.Success)
                {
                    if (bytesSent > 0)
                    {
                        state.BytesSentR += bytesSent;

                        try
                        {
                            if (state.HasSentR())
                            {
                                remoteConnection.BeginReceive(state.RemoteBuffer, 0, Socks5State.RECV_BUFFER_SZ, 0, OnRemoteRecv, state);
                            }
                            else
                            {
                                clientConnection.BeginSend(state.RemoteBuffer, state.BytesSentR, state.SendLengthR, 0, OnClientSend, state);
                            }

                            return;
                        }
                        catch { }
                    }
                }

                state.Dispose();
            }
#if DEBUG
            catch (System.Exception ex)
            {
                if (ex is ObjectDisposedException)
                {
                    return;
                }

                Console.WriteLine(ex.ToString());
            }
#else
            catch { }
#endif
        }
    }
}

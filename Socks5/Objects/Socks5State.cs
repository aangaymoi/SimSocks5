using System;
using System.Net.Sockets;
using Helpers;

public class Socks5State : IDisposable
{
    public const int SEND_BUFFER_SZ = 4096;
    public const int RECV_BUFFER_SZ = 8192;

    public Socket ClientSocket = null;
    public byte[] ClientBuffer = new byte[RECV_BUFFER_SZ];

    public Socket RemoteSocket = null;    
    public byte[] RemoteBuffer = new byte[RECV_BUFFER_SZ];

    private int _numberOfBytesToSendR = 0;

    public int NumberOfBytesToSendR
    {
        get { return _numberOfBytesToSendR; }
        set
        {
            _numberOfBytesToSendR = value;
            BytesSentR = 0;
        }
    }

    public int BytesSentR = 0;

    public bool HasSentR()
    {
        return BytesSentR >= NumberOfBytesToSendR;
    }

    public int SendLengthR
    {
        get
        {
            int next = NumberOfBytesToSendR - BytesSentR;

            if (next > SEND_BUFFER_SZ)
            {
                return SEND_BUFFER_SZ;
            }

            return next;
        }
    }

    //
    private int _numberOfBytesToSend = 0;

    public int NumberOfBytesToSend
    {
        get { return _numberOfBytesToSend; }
        set
        {
            _numberOfBytesToSend = value;
            BytesSent = 0;
        }
    }

    public int BytesSent = 0;

    public bool HasSent()
    {
        return BytesSent >= NumberOfBytesToSend;
    }

    public int SendLength
    {
        get
        {
            int next = NumberOfBytesToSend - BytesSent;

            if (next > SEND_BUFFER_SZ)
            {
                return SEND_BUFFER_SZ;
            }

            return next;
        }
    }
    //

    private bool _disposed = false;

    ~Socks5State()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                ClientSocket.ShutdownClose();
                RemoteSocket.ShutdownClose();

                ClientBuffer = null;
                RemoteBuffer = null;

                /*GC.Collect();*/
            }

            _disposed = true;
        }
    }
}

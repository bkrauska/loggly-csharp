﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Loggly.Transports.Syslog
{


    /// <summary>
    /// Exposes the Active propery of UdpClient
    /// </summary>
    public class UdpClientEx : UdpClient
    {
        public UdpClientEx() : base() { }
        public UdpClientEx(IPEndPoint ipe) : base (ipe) { }
        ~UdpClientEx()
        {
            if (this.Active) this.Close();
        }

        public bool IsActive
        {
            get {  return this.Active ; }
        }
    }

    internal class SyslogUdpTransport : SyslogTransportBase
    {
        private IPHostEntry _ipHostInfo;
        private IPAddress _ipAddress;
        private IPEndPoint      _ipLocalEndPoint;
        private UdpClientEx _udpClient;
        public int Port { get; set; }

        public SyslogUdpTransport()
        {
            Port = 514;
            _ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            _ipAddress = _ipHostInfo.AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            _ipLocalEndPoint = new IPEndPoint(_ipAddress, 0);
            _udpClient= new UdpClientEx(_ipLocalEndPoint);
        }

        public bool IsActive
        {
            get {  return _udpClient.IsActive ; }
        }

        public void Close()
        {
            if (_udpClient.IsActive)
            {
                _udpClient.Close();
            }
        }


        protected override void Send(SyslogMessage syslogMessage)
        {
            if (!_udpClient.IsActive)
            {
                var logglyEndpointIp = Dns.GetHostEntry("logs-01.loggly.com").AddressList[0];
                _udpClient.Connect(logglyEndpointIp, Port);
            }

            try
            {
                if (_udpClient.IsActive)
                {
                    var bytes = syslogMessage.GetBytes();
                    _udpClient.Send(bytes, bytes.Length);
                }
                else
                {
                    LogglyException.Throw("Syslog client Socket is not connected.");
                }
            }
            finally
            {
                Close();
            }
        }
    }
}

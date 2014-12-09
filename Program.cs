using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication9
{
    class Pingr
    {

        public static void GetNetworkStats(string host, int pingAmount, int timeout, out int averagePing, out int packetLoss)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions(); // default is: don't fragment and 128 Time-to-Live
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data); 

            var failedPings = 0;
            var latencySum = 0;

            for (int i = 0; i < pingAmount; i++)
            {
                PingReply reply = pingSender.Send(host, timeout, buffer, options);

                if (reply != null)
                {
                    if (reply.Status != IPStatus.Success)
                        failedPings += 1;
                    else
                        latencySum += (int)reply.RoundtripTime;
                }
            }

            averagePing = (latencySum / pingAmount);
            packetLoss = (failedPings / pingAmount) * 100;

        }

    }
    public class TraceLocation
    {

        public int Hop { get; set; }

        public long Time { get; set; }

        public String IpAddress { get; set; }

    }



    public class Trace
    {

        public static List<TraceLocation> Traceroute(string ipAddressOrHostName, int maximumHops)
        {

            if (maximumHops < 1 || maximumHops > 100)
            {

                maximumHops = 30;

            }



            IPAddress ipAddress = Dns.GetHostEntry(ipAddressOrHostName).AddressList[0];



            List<TraceLocation> traceLocations = new List<TraceLocation>();



            using (Ping pingSender = new Ping())
            {

                PingOptions pingOptions = new PingOptions();

                Stopwatch stopWatch = new Stopwatch();

                byte[] bytes = new byte[32];

                pingOptions.DontFragment = true;

                pingOptions.Ttl = 1;



                for (int i = 1; i < maximumHops + 1; i++)
                {

                    TraceLocation traceLocation = new TraceLocation();



                    stopWatch.Reset();

                    stopWatch.Start();

                    PingReply pingReply = pingSender.Send(

                        ipAddress,

                        5000,

                        new byte[32], pingOptions);

                    stopWatch.Stop();



                    traceLocation.Hop = i;

                    traceLocation.Time = stopWatch.ElapsedMilliseconds;

                    if (pingReply.Address != null)
                    {

                        traceLocation.IpAddress = pingReply.Address.ToString();

                    }



                    traceLocations.Add(traceLocation);

                    traceLocation = null;



                    if (pingReply.Status == IPStatus.Success)
                    {

                        break;

                    }

                    pingOptions.Ttl++;

                }

            }

            return traceLocations;

        }



    }

    class Program
    {

        static void Main(string[] args)
        {

            List<string> domainNames = new List<string>();

            Console.WriteLine("Please add domain:");
            string str = Console.ReadLine();
            domainNames.Add(str);

            int val;
            int packetloss;

            foreach (String domainName in domainNames)
            {

                Console.Write("------" + domainName + "-------");


                IPAddress ipaddress = Dns.GetHostEntry(domainName).AddressList[0];
                Console.WriteLine("---- IP: " + ipaddress);

               


                foreach (TraceLocation traceLocation in Trace.Traceroute(domainName, 100))
                {

                    Console.Write(traceLocation.Hop + " ");

                    Console.Write(traceLocation.Time + "ms  ");

                    Console.Write(traceLocation.IpAddress + "   ");

                    
                    Pingr.GetNetworkStats(traceLocation.IpAddress, 5, 5000, out val, out packetloss);
                    int val1 = val;
                    int pl = packetloss;
                    Console.WriteLine(" avrage ping: " + val1 +"ms " +  " packet loss: " + pl+"%");

                    if (!String.IsNullOrWhiteSpace(traceLocation.IpAddress) && !traceLocation.IpAddress.StartsWith("10.") && !traceLocation.IpAddress.StartsWith("192."))
                    {

                        try
                        {
                            
                            Console.WriteLine(Dns.GetHostEntry(traceLocation.IpAddress).HostName.ToString());
                          

                            if (ipaddress.ToString() == traceLocation.IpAddress.ToString())
                            {

                                Console.WriteLine("Traceroute complete!");
                            }
                        }
                        catch (Exception ex)
                        {

                            Console.WriteLine(ex.Message);
                            if (ipaddress.ToString() == traceLocation.IpAddress.ToString())
                            {

                                Console.WriteLine("Traceroute complete!");
                            }
                        }

                    }
                    else
                    {

                        Console.WriteLine();
                    }


                }

                Console.ReadKey();



            }
        }
    }
}      
        
       
    


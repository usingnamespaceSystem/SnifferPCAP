using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using PcapDotNet.Packets.Http;
using System.Threading;

namespace Sniffer
{

    
    public partial class Sniff : Form
    {
        private IList<LivePacketDevice> DeviceList;
        private PacketDevice selectedDevice;
        AbortableBackgroundWorker worker;

        string count = String.Empty, source = String.Empty, destination = String.Empty, s_port = String.Empty, d_port = String.Empty, protocol = String.Empty,
            length = String.Empty, tcpack = String.Empty, tcpsec = String.Empty, tcpnsec = String.Empty, tcpsrc = String.Empty,
            tcpdes = String.Empty, udpscr = String.Empty, udpdes = String.Empty, httpheader = String.Empty, httpbody = String.Empty,
            httpver = String.Empty, httpayload = String.Empty, reqres = String.Empty, time = String.Empty, payload = String.Empty;

        private bool isDragging = false;

        private Point lastCursor;
        private Point lastForm;

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;

            lastCursor = Cursor.Position;
            lastForm = this.Location;
        }

        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                this.Location =
                Point.Add(lastForm, new Size(Point.Subtract(Cursor.Position, new Size(lastCursor))));
            }
        }

        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }


        private void close_app_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public Sniff()
        {
            InitializeComponent();
            _tcp.Checked = true;
            _udp.Checked = true;
            _hip.Checked = true;
            _icmp.Checked = true;
            _igmp.Checked = true;
            _other.Checked = true;

            try
            {
                DeviceList = LivePacketDevice.AllLocalMachine;
            }
            catch
            {
                MessageBox.Show("Запустите программу от имени администратора!");
            }

            if (DeviceList.Count == 0)
            {

                MessageBox.Show("Устройства не найдены");

                return;

            }

            for (int i = 0; i != DeviceList.Count; ++i)
            {
                LivePacketDevice Device = DeviceList[i];

                if (Device.Description != null)

                    device_list.Items.Add(Device.Description);
                else
                    device_list.Items.Add("Неизвестный");
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!count.Equals(""))
            {
                if (_tcp.Checked && protocol == "Tcp")
                {
                    ListViewItem item = new ListViewItem(time);
                    item.SubItems.Add(source + " (" + s_port + ")");
                    item.SubItems.Add(destination + " (" + d_port + ")");
                    item.SubItems.Add(protocol);
                    item.SubItems.Add(length+ payload);
                    listView1.Items.Insert(0, item);
                }
                 if (_udp.Checked && protocol == "Udp")
                {
                    ListViewItem item = new ListViewItem(time);
                    item.SubItems.Add(source + " (" + s_port + ")");
                    item.SubItems.Add(destination + " (" + d_port + ")");
                    item.SubItems.Add(protocol);
                    item.SubItems.Add(length + payload);
                    listView1.Items.Insert(0, item);
                }
                 if (_hip.Checked && protocol == "Hip")
                {
                    ListViewItem item = new ListViewItem(time);
                    item.SubItems.Add(source + " (" + s_port + ")");
                    item.SubItems.Add(destination + " (" + d_port + ")");
                    item.SubItems.Add(protocol);
                    item.SubItems.Add(length + payload);
                    listView1.Items.Insert(0, item);
                }
                if (_icmp.Checked && protocol == "Icmp")
                {
                    ListViewItem item = new ListViewItem(time);
                    item.SubItems.Add(source + " (" + s_port + ")");
                    item.SubItems.Add(destination + " (" + d_port + ")");
                    item.SubItems.Add(protocol);
                    item.SubItems.Add(length + payload);
                    listView1.Items.Insert(0, item);
                
                }
                if (_igmp.Checked && protocol == "Igmp")
                {
                    ListViewItem item = new ListViewItem(time);
                    item.SubItems.Add(source + " (" + s_port + ")");
                    item.SubItems.Add(destination + " (" + d_port + ")");
                    item.SubItems.Add(protocol);
                    item.SubItems.Add(length + payload);
                    listView1.Items.Insert(0, item);
                }
                if (_other.Checked && protocol != "Igmp" && protocol != "Icmp" && protocol != "Hip" && protocol != "Tcp" && protocol != "Udp")
                {
                    ListViewItem item = new ListViewItem(time);
                    item.SubItems.Add(source + " (" + s_port + ")");
                    item.SubItems.Add(destination + " (" + d_port + ")");
                    item.SubItems.Add(protocol);
                    item.SubItems.Add(length + payload);
                    listView1.Items.Insert(0, item);

                }
            }
        }

        private void start_button_Click(object sender, EventArgs e)
        {
            worker = new AbortableBackgroundWorker();
            worker.DoWork += worker_do;

            if (device_list.SelectedIndex >= 0)
            {
                selectedDevice = DeviceList[device_list.SelectedIndex];
                timer1.Start();
                worker.RunWorkerAsync();
                start_button.Enabled = false;
                stop_button.Enabled = true;
                device_list.Enabled = false;
                _tcp.Enabled = false;
                _udp.Enabled = false;
                _hip.Enabled = false;
                _icmp.Enabled = false;
                _igmp.Enabled = false;
                _other.Enabled = false;

            }
            else
            {
                MessageBox.Show("Выберите устройство");
            }
        }

        private void stop_button_Click(object sender, EventArgs e)  
        {
            timer1.Stop();
            start_button.Enabled = true;
            stop_button.Enabled = false;
            device_list.Enabled = true;
            _tcp.Enabled = true;
            _udp.Enabled = true;
            _hip.Enabled = true;
            _icmp.Enabled = true;
            _igmp.Enabled = true;
            _other.Enabled = true;

            if (worker.IsBusy == true)
            {
                worker.Abort();
                worker.Dispose();
                worker.DoWork -= worker_do;

                this.count = String.Empty; this.time = String.Empty; this.source = String.Empty; this.destination = String.Empty; this.s_port = String.Empty; this.d_port = String.Empty;  this.protocol = String.Empty; this.length = String.Empty;
                this.tcpack = String.Empty; this.tcpsec = String.Empty; this.tcpnsec = String.Empty; this.tcpsrc = String.Empty; this.tcpdes = String.Empty; this.udpscr = String.Empty;
                this.udpdes = String.Empty; this.httpheader = String.Empty; this.httpver = String.Empty; this.httpayload = String.Empty; this.reqres = String.Empty; this.httpbody = String.Empty; payload = String.Empty;
            }
        }

        private void worker_do(object sender, DoWorkEventArgs e)
        {
            PacketCommunicator pc = selectedDevice.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1000);
            pc.ReceivePackets(0, PacketHandler);
        }

        private void PacketHandler(Packet packet)
        {
            this.count = String.Empty; this.time = String.Empty; this.source = String.Empty; this.destination = String.Empty; this.s_port = String.Empty; this.d_port = String.Empty; this.protocol = String.Empty; this.length = String.Empty;
            this.tcpack = String.Empty; this.tcpsec = String.Empty; this.tcpnsec = String.Empty; this.tcpsrc = String.Empty; this.tcpdes = String.Empty; this.udpscr = String.Empty;
            this.udpdes = String.Empty; this.httpheader = String.Empty; this.httpver = String.Empty; this.httpayload = String.Empty; this.reqres = String.Empty; this.httpbody = String.Empty; payload = String.Empty;

            IpV4Datagram ip = packet.Ethernet.IpV4;
            TcpDatagram tcp = ip.Tcp;
            UdpDatagram udp = ip.Udp;
            HttpDatagram http = null;

            try
            {
                if (ip.Protocol.ToString().Equals("Tcp"))
                {
                    http = tcp.Http;

                    if (http.Header != null)
                    {
                        protocol = "Http";
                        httpheader = http.Header.ToString();
                        httpver = http.Version.ToString();
                        httpayload = http.Length.ToString();
                        httpbody = http.Body.ToString();

                        if (http.IsRequest)
                        {
                            reqres = "Request";
                        }
                        else
                        {
                            reqres = "Response";
                        }
                    }

                    else
                    {
                        protocol = ip.Protocol.ToString();
                        s_port = tcp.SourcePort.ToString();
                        d_port = tcp.DestinationPort.ToString();
                        tcpack = tcp.AcknowledgmentNumber.ToString();
                        tcpsec = tcp.SequenceNumber.ToString();
                        tcpnsec = tcp.NextSequenceNumber.ToString();
                        payload = ", data:" + tcp.Payload.Length.ToString();
                    }


                }
                else if ((ip.Protocol.ToString().Equals("Udp")))
                {
                    protocol = ip.Protocol.ToString();
                    s_port = udp.SourcePort.ToString();
                    d_port = udp.DestinationPort.ToString();
                    payload = ", data:" + udp.Payload.Length.ToString();
                }
                else
                {
                    protocol = ip.Protocol.ToString();
                }
                count = packet.Count.ToString();
                time = packet.Timestamp.ToString();
                this.source = ip.Source.ToString();
                this.destination = ip.Destination.ToString();
                length = ip.Length.ToString();
                payload = ", data:" + ip.Payload.Length.ToString();
            }
            catch { }
        }
    }

    public class AbortableBackgroundWorker : BackgroundWorker
    {

        private Thread worker_thread;

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            worker_thread = Thread.CurrentThread;
            try
            {
                base.OnDoWork(e);
            }
            catch (ThreadAbortException)
            {
                e.Cancel = true;
                Thread.ResetAbort();
            }
        }


        public void Abort()
        {
            if (worker_thread != null)
            {
                worker_thread.Abort();
                worker_thread = null;
            }
        }
    }

}
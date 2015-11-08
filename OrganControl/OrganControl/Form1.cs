using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace OrganControl
{

    public partial class Form1 : Form
    {

        IPEndPoint remoteEPKeys;
        IPEndPoint remoteEPStops;
        Socket rawSocket;
        SoloStatus soloRank;
        SwellStops swellStops;

        public void simpleSend(byte[] toSend, IPEndPoint remoteEP) {
            byte[] icmpData = new byte[20 + toSend.Length];

            //Header
            byte headerSize = 0x45; //????
            byte headerServices = 8;
            short headerTotalLength = 112;
            short headerID = 1390;
            byte headerFlags = 0;
            byte headerFragmentOffset = 0;
            byte headerTtl = 10;
            byte headerProtocol = 200;
            short headerChecksum = 0;

            byte[] sourceIP = new byte[4]{192,168,0,254};
            byte[] destIP = remoteEP.Address.GetAddressBytes();

            icmpData[0] = headerSize;
            icmpData[1] = headerServices;
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(headerTotalLength)),0, icmpData, 2, 2);
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(headerID)), 0, icmpData, 4, 2);
            icmpData[6] = headerFlags;
            icmpData[7] = headerFragmentOffset;
            icmpData[8] = headerTtl;
            icmpData[9] = headerProtocol;
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(headerChecksum)), 0, icmpData, 10, 2);
            Array.Copy(sourceIP, 0, icmpData, 12, 4);
            Array.Copy(destIP, 0, icmpData, 16, 4);

            Array.Copy(toSend, 0, icmpData, 20, toSend.Length);

            rawSocket.SendTo(icmpData, remoteEP);
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Establish the remote endpoint for the socket
            remoteEPKeys = new IPEndPoint(IPAddress.Parse("192.168.0.3"), 0);
            remoteEPStops = new IPEndPoint(IPAddress.Parse("192.168.0.4"), 0);

            // Create a TCP/IP  socket.
            rawSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            rawSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, 1);
            try
            {
                rawSocket.Bind(new IPEndPoint(IPAddress.Parse("192.168.0.254"), 255));
            }
            catch(Exception e1)
            {
                MessageBox.Show(e1.ToString());
            }

            swellStops = new SwellStops();
            soloRank = new SoloStatus();

            foreach (Control b in this.Controls)
            {
                if(b is Button){
                    b.MouseDown += Note_MouseDown;
                    b.MouseUp += Note_MouseUp;
                }
            }
        }

        private void Note_MouseDown(object sender, MouseEventArgs e)
        {
            byte ranks = 0;
            if (checkBox1.Checked) ranks += (byte)Math.Pow(2, 0);
            if (checkBox2.Checked) ranks += (byte)Math.Pow(2, 0);
            if (checkBox3.Checked) ranks += (byte)Math.Pow(2, 0);
            if (checkBox4.Checked) ranks += (byte)Math.Pow(2, 0);
            if (checkBox5.Checked) ranks += (byte)Math.Pow(2, 0);
            if (checkBox6.Checked) ranks += (byte)Math.Pow(2, 0);
            if (checkBox7.Checked) ranks += (byte)Math.Pow(2, 0);
            if (checkBox8.Checked) ranks += (byte)Math.Pow(2, 0);

            soloRank[(sender as Button).Name] = true;
            soloRank.updateSoloCommand(ranks);
            simpleSend(soloRank.SwellPacket, remoteEPKeys);
        }

        private void Note_MouseUp(object sender, MouseEventArgs e)
        {

            byte ranks = 0;
            if (checkBox1.Checked) ranks += (byte)Math.Pow(2, 0);
            if (checkBox2.Checked) ranks += (byte)Math.Pow(2, 0);
            if (checkBox3.Checked) ranks += (byte)Math.Pow(2, 0);
            if (checkBox4.Checked) ranks += (byte)Math.Pow(2, 0);
            if (checkBox5.Checked) ranks += (byte)Math.Pow(2, 0);
            if (checkBox6.Checked) ranks += (byte)Math.Pow(2, 0);
            if (checkBox7.Checked) ranks += (byte)Math.Pow(2, 0);
            if (checkBox8.Checked) ranks += (byte)Math.Pow(2, 0);

            soloRank[(sender as Button).Name] = false;
            soloRank.updateSoloCommand(ranks);
            simpleSend(soloRank.SwellPacket, remoteEPKeys);
        }

        private void B4_Click(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void PleinJeu_CheckedChanged(object sender, EventArgs e)
        {
            swellStops.PleinJeu = (sender as CheckBox).Checked;
            swellStops.updateStopCommand(0);
            simpleSend(swellStops.stopCommand,remoteEPStops);
        }

        private void Octavin_CheckedChanged(object sender, EventArgs e)
        {
            swellStops.Octavin = (sender as CheckBox).Checked;
            swellStops.updateStopCommand(0);
            simpleSend(swellStops.stopCommand, remoteEPStops);
        }

        private void B2_Click(object sender, EventArgs e)
        {

        }
    }

    public class SwellStops
    {
        public bool PleinJeu { get; set; }
        public bool Octavin { get; set; }

        public object this[string propertyName]
        {
            get { return this.GetType().GetProperty(propertyName).GetValue(this, null); }
            set { this.GetType().GetProperty(propertyName).SetValue(this, value, null); }
        }

        public byte[] stopCommand;

        public void updateStopCommand(byte ranks)
        {
            ulong firstHalf = 0;

            if (PleinJeu) firstHalf += (ulong)Math.Pow(2, 24);
            if (Octavin) firstHalf += (ulong)Math.Pow(2, 22);

            byte[] firstHalfBytes = BitConverter.GetBytes(firstHalf);

            stopCommand = new byte[92] {0x0f, 0xe0, 0x0f, 0xe0, 0x00, 0x5c, 0x00, 0x01, 0x00,
                0x04, 0x00, 0x50, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x01, 0x00, 0x10, 0x00, 0x01, 0xf0, 0x0f, firstHalfBytes[5], firstHalfBytes[4], firstHalfBytes[3], firstHalfBytes[2], firstHalfBytes[1], firstHalfBytes[0], 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x03, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00,
                0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        }

        public SwellStops()
        {
            PleinJeu = false;
            Octavin = false;
        }

    }

    public class SoloStatus
    {
        public bool C0 {get; set;}
        public bool Cs0 {get; set;}
        public bool D0 {get; set;}
        public bool Ds0 {get; set;}
        public bool E0 {get; set;}
        public bool F0 {get; set;}
        public bool Fs0 {get; set;}
        public bool G0 {get; set;}
        public bool Gs0 {get; set;}
        public bool A0 {get; set;}
        public bool As0 {get; set;}
        public bool B0 {get; set;}

        public bool C1 {get; set;}
        public bool Cs1 {get; set;}
        public bool D1 {get; set;}
        public bool Ds1 {get; set;}
        public bool E1 {get; set;}
        public bool F1 {get; set;}
        public bool Fs1 {get; set;}
        public bool G1 {get; set;}
        public bool Gs1 {get; set;}
        public bool A1 {get; set;}
        public bool As1 {get; set;}
        public bool B1 {get; set;}

        public bool C2 { get; set; }
        public bool Cs2 { get; set; }
        public bool D2 { get; set; }
        public bool Ds2 { get; set; }
        public bool E2 { get; set; }
        public bool F2 { get; set; }
        public bool Fs2 { get; set; }
        public bool G2 { get; set; }
        public bool Gs2 { get; set; }
        public bool A2 { get; set; }
        public bool As2 { get; set; }
        public bool B2 { get; set; }

        public bool C3 { get; set; }
        public bool Cs3 { get; set; }
        public bool D3 { get; set; }
        public bool Ds3 { get; set; }
        public bool E3 { get; set; }
        public bool F3 { get; set; }
        public bool Fs3 { get; set; }
        public bool G3 { get; set; }
        public bool Gs3 { get; set; }
        public bool A3 { get; set; }
        public bool As3 { get; set; }
        public bool B3 { get; set; }

        public bool C4 { get; set; }
        public bool Cs4 { get; set; }
        public bool D4 { get; set; }
        public bool Ds4 { get; set; }
        public bool E4 { get; set; }
        public bool F4 { get; set; }
        public bool Fs4 { get; set; }
        public bool G4 { get; set; }
        public bool Gs4 { get; set; }
        public bool A4 { get; set; }
        public bool As4 { get; set; }
        public bool B4 { get; set; }

        public byte[] SoloCommand;
        public byte[] SoloPacket;
        public byte[] SwellPacket;
        public byte[] ChoirPacket;
        public byte[] GreatPacket;

        public object this[string propertyName]
        {
            get { return this.GetType().GetProperty(propertyName).GetValue(this, null); }
            set { this.GetType().GetProperty(propertyName).SetValue(this, value, null); }
        }


        public void updateSoloCommand(byte ranks)
        {
            ulong firstHalf = 0;

            if (C0) firstHalf += (ulong)Math.Pow(2,4);
            if (Cs0) firstHalf += (ulong)Math.Pow(2,5);
            if (D0) firstHalf += (ulong)Math.Pow(2,6);
            if (Ds0) firstHalf += (ulong)Math.Pow(2,7);
            if (E0) firstHalf += (ulong)Math.Pow(2,8);
            if (F0) firstHalf += (ulong)Math.Pow(2,9);
            if (Fs0) firstHalf += (ulong)Math.Pow(2,10);
            if (G0) firstHalf += (ulong)Math.Pow(2,11);
            if (Gs0) firstHalf += (ulong)Math.Pow(2,12);
            if (A0) firstHalf += (ulong)Math.Pow(2,13);
            if (As0) firstHalf += (ulong)Math.Pow(2,14);
            if (B0) firstHalf += (ulong)Math.Pow(2,15);

            if (C1) firstHalf += (ulong)Math.Pow(2,16);
            if (Cs1) firstHalf += (ulong)Math.Pow(2,17);
            if (D1) firstHalf += (ulong)Math.Pow(2,18);
            if (Ds1) firstHalf += (ulong)Math.Pow(2,19);
            if (E1) firstHalf += (ulong)Math.Pow(2,20);
            if (F1) firstHalf += (ulong)Math.Pow(2,21);
            if (Fs1) firstHalf += (ulong)Math.Pow(2,22);
            if (G1) firstHalf += (ulong)Math.Pow(2,23);
            if (Gs1) firstHalf += (ulong)Math.Pow(2,24);
            if (A1) firstHalf += (ulong)Math.Pow(2,25);
            if (As1) firstHalf += (ulong)Math.Pow(2,26);
            if (B1) firstHalf += (ulong)Math.Pow(2,27);

            if (C2) firstHalf += (ulong)Math.Pow(2, 28);
            if (Cs2) firstHalf += (ulong)Math.Pow(2, 29);
            if (D2) firstHalf += (ulong)Math.Pow(2, 30);
            if (Ds2) firstHalf += (ulong)Math.Pow(2, 31);
            if (E2) firstHalf += (ulong)Math.Pow(2, 32);
            if (F2) firstHalf += (ulong)Math.Pow(2, 33);
            if (Fs2) firstHalf += (ulong)Math.Pow(2, 34);
            if (G2) firstHalf += (ulong)Math.Pow(2, 35);
            if (Gs2) firstHalf += (ulong)Math.Pow(2, 36);
            if (A2) firstHalf += (ulong)Math.Pow(2, 37);
            if (As2) firstHalf += (ulong)Math.Pow(2, 38);
            if (B2) firstHalf += (ulong)Math.Pow(2, 39);

            if (C3) firstHalf += (ulong)Math.Pow(2, 40);
            if (Cs3) firstHalf += (ulong)Math.Pow(2, 41);
            if (D3) firstHalf += (ulong)Math.Pow(2, 42);
            if (Ds3) firstHalf += (ulong)Math.Pow(2, 43);
            if (E3) firstHalf += (ulong)Math.Pow(2, 44);
            if (F3) firstHalf += (ulong)Math.Pow(2, 45);
            if (Fs3) firstHalf += (ulong)Math.Pow(2, 46);
            if (G3) firstHalf += (ulong)Math.Pow(2, 47);
            if (Gs3) firstHalf += (ulong)Math.Pow(2, 48);
            if (A3) firstHalf += (ulong)Math.Pow(2, 49);
            if (As3) firstHalf += (ulong)Math.Pow(2, 50);
            if (B3) firstHalf += (ulong)Math.Pow(2, 51);

            if (C4) firstHalf += (ulong)Math.Pow(2, 52);
            if (Cs4) firstHalf += (ulong)Math.Pow(2, 53);
            if (D4) firstHalf += (ulong)Math.Pow(2, 54);
            if (Ds4) firstHalf += (ulong)Math.Pow(2, 55);
            if (E4) firstHalf += (ulong)Math.Pow(2, 56);
            if (F4) firstHalf += (ulong)Math.Pow(2, 57);
            if (Fs4) firstHalf += (ulong)Math.Pow(2, 58);
            if (G4) firstHalf += (ulong)Math.Pow(2, 59);
            if (Gs4) firstHalf += (ulong)Math.Pow(2, 60);
            if (A4) firstHalf += (ulong)Math.Pow(2, 61);
            if (As4) firstHalf += (ulong)Math.Pow(2, 62);
            if (B4) firstHalf += (ulong)Math.Pow(2, 63);

            byte[] firstHalfBytes = BitConverter.GetBytes(firstHalf);
            SoloPacket = new byte[92] {0x0f, 0xe0, 0x0f, 0xe0, 0x00, 0x5c, 0x00, 0x01, 0x00,
                0x03, 0x00, 0x50, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, ranks, 0x00, 0x10, 0x00, firstHalfBytes[7], firstHalfBytes[6], firstHalfBytes[5], firstHalfBytes[4], firstHalfBytes[3], firstHalfBytes[2], firstHalfBytes[1], firstHalfBytes[0], 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x03, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00,
                0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            SwellPacket = new byte[92] {0x0f, 0xe0, 0x0f, 0xe0, 0x00, 0x5c, 0x00, 0x01, 0x00,
                0x03, 0x00, 0x50, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x01, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                ranks, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x00,
                0x10, 0x00, firstHalfBytes[7], firstHalfBytes[6], firstHalfBytes[5], firstHalfBytes[4], firstHalfBytes[3], firstHalfBytes[2], firstHalfBytes[1], firstHalfBytes[0], 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            GreatPacket = new byte[92] {0x0f, 0xe0, 0x0f, 0xe0, 0x00, 0x5c, 0x00, 0x01, 0x00,
                0x03, 0x00, 0x50, 0x00, 0x00, 0x00, 0x10, 0x00,  firstHalfBytes[7], firstHalfBytes[6], firstHalfBytes[5], firstHalfBytes[4], firstHalfBytes[3], firstHalfBytes[2], firstHalfBytes[1], firstHalfBytes[0], 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x01, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                ranks, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00,
                0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            ChoirPacket = new byte[92] {0x0f, 0xe0, 0x0f, 0xe0, 0x00, 0x5c, 0x00, 0x01, 0x00,
                0x03, 0x00, 0x50, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x01, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                ranks, 0x00, 0x10, 0x00, firstHalfBytes[7], firstHalfBytes[6], firstHalfBytes[5], firstHalfBytes[4], firstHalfBytes[3], firstHalfBytes[2], firstHalfBytes[1], firstHalfBytes[0], 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00,
                0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        }

        public SoloStatus()
        {
            C0 = false;
            Cs0 = false;
            D0 = false;
            Ds0 = false;
            E0 = false;
            F0 = false;
            Fs0 = false;
            G0 = false;
            Gs0 = false;
            A0 = false;
            As0 = false;
            B0 = false;

            C1 = false;
            Cs1 = false;
            D1 = false;
            Ds1 = false;
            E1 = false;
            F1 = false;
            Fs1 = false;
            G1 = false;
            Gs1 = false;
            A1 = false;
            As1 = false;
            B1 = false;
        }
    }
}

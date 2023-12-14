using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Touchless.Vision.Camera;

// UDP y Multicast.
using System.Net.Sockets;
using System.Net;
using System.IO;

// IMAGE
using System.Drawing.Imaging;
using System.Threading;
using System.Security.Cryptography.X509Certificates;

namespace WebcamUDPMulticast
{
    public partial class Form1 : Form
    {
        private IPAddress multicastaddress = IPAddress.Parse("224.0.0.3");
        private CameraFrameSource _frameSource;
        private static Bitmap _latestFrame;
        private int numeroImagen = 0;
        private int longitudPayload = 1400;


        // Chat UDP Multicast.
        private UdpClient udpServerChat;
        private IPEndPoint remoteChat;


        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            // Añadimos las cámaras disponibles al combobox.
            foreach(Camera cam in CameraService.AvailableCameras)
            {
                comboBoxCameras.Items.Add(cam);                
            }
            
        }

        private void setFrameSource(CameraFrameSource cameraFrameSource)
        {
            if (_frameSource == cameraFrameSource)
                return;

            _frameSource = cameraFrameSource;
        }

        public void OnImageCaptured(Touchless.Vision.Contracts.IFrameSource frameSource, Touchless.Vision.Contracts.Frame frame, double fps)
        {
            _latestFrame = frame.Image;
            pictureBox2.Invalidate();

        }

        private void drawLatestImage(object sender, PaintEventArgs e)
        {
            if (_latestFrame != null)
            {
                _latestFrame = new Bitmap(_latestFrame, new Size(320, 240));
                e.Graphics.DrawImage(_latestFrame, 0, 0, _latestFrame.Width, _latestFrame.Height);
                UdpClient udpServer = new UdpClient();
                udpServer.JoinMulticastGroup(multicastaddress);
                
                IPEndPoint remote = new IPEndPoint(multicastaddress, 8081);

                //Dividimos el payload en partes de 1400 bytes.
                short longitudImagen = (short) (ImageToByteArray(_latestFrame).Length);
                int numeroPaquetes = longitudImagen / longitudPayload;
                int resto = longitudImagen % longitudPayload;
                short numeroPaquetesEnLaImagen = (short)(numeroPaquetes + 1);


                for (short i = 0; i < numeroPaquetes; i++)
                {
                    byte[] payload = new byte[1400];
                    Array.Copy(ImageToByteArray(_latestFrame), i * 1400, payload, 0, 1400);
                    int timeStamp = DateTime.Now.Millisecond;
                    short numeroSecuencia = i;
                    PaqueteRTP paquete = new PaqueteRTP(payload, numeroSecuencia, timeStamp, numeroImagen, longitudImagen, numeroPaquetesEnLaImagen);
                    Byte[] buffer = paquete.Paquete;
                    udpServer.Send(buffer, buffer.Length, remote);
                }

                if (resto > 0)
                {
                    byte[] payload = new byte[resto];
                    Array.Copy(ImageToByteArray(_latestFrame), numeroPaquetes * 1400, payload, 0, resto);
                    int timeStamp = DateTime.Now.Millisecond;
                    short numeroSecuencia = (short)(numeroPaquetes);
                    PaqueteRTP paquete = new PaqueteRTP(payload, numeroSecuencia, timeStamp, numeroImagen, longitudImagen, numeroPaquetesEnLaImagen);
                    Byte[] buffer = paquete.Paquete;
                    udpServer.Send(buffer, buffer.Length, remote);
                }

                numeroImagen++;
            }
        }

        public byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            // Convertimos la imagen a un array de bytes.
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

            try
            {
                // Iniciamos la captura de la cámara seleccionada.
                Camera c = (Camera)comboBoxCameras.SelectedItem;
                setFrameSource(new CameraFrameSource(c));
                _frameSource.Camera.CaptureWidth = 320;
                _frameSource.Camera.CaptureHeight = 240;
                _frameSource.Camera.Fps = 20;
                _frameSource.NewFrame += OnImageCaptured;
                pictureBox2.Paint += new PaintEventHandler(drawLatestImage);
                _frameSource.StartFrameCapture();

                // Iniciamos el chat.
                iniciarChat();
            }
            catch (Exception x)
            {
                Console.WriteLine(x.ToString());
            }
        }

        private void iniciarChat()
        {
            // Iniciamos el chat.
            udpServerChat = new UdpClient(8082);
            udpServerChat.JoinMulticastGroup(multicastaddress);
            remoteChat = null;
            
            Thread t = new Thread(new ThreadStart(recibirChat));
            t.Start();
            
        }

        private void recibirChat()
        {
            while (true)
            {
                Byte[] buffer = udpServerChat.Receive(ref remoteChat);
                string mensaje = Encoding.Unicode.GetString(buffer);

                this.Invoke((MethodInvoker)delegate
                {
                    textBox1.Text = mensaje;
                    listBox1.Items.Add(mensaje);
                });
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}

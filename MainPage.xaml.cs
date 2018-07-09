using System;
using System.IO;
using Windows.UI.Xaml.Controls;
using Windows.Networking.Sockets;
using System.Diagnostics;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Collections.Generic;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Get_Images
{
    public sealed partial class MainPage : Page
    {
        static string ServerPortNumber = "5000";

        public MainPage()
        {
            this.InitializeComponent();
            this.StartServer();
        }

        private async void StartServer()
        {
            Debug.WriteLine("starting");

            // Inbound and outbound DatagramSocket
            DatagramSocket udpListener = new DatagramSocket();

            try
            {
                // Bind listener
                udpListener.MessageReceived += ServerDatagramSocket_MessageReceived;
                await udpListener.BindServiceNameAsync(ServerPortNumber);             
            }
            catch (Exception ex)
            {
                // Some exception handling
            }
        
        }

        private void ServerDatagramSocket_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender, Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
        {
            Debug.WriteLine("received packet");
            Stream streamIn = args.GetDataStream().AsStreamForRead();
            MemoryStream image = ToMemoryStream(streamIn);
            Save_Image(image);
        }

        private async void Save_Image(MemoryStream image)
        {
            // Launch file picker
            FileSavePicker picker = new FileSavePicker();
            picker.FileTypeChoices.Add("JPeg", new List<string>() { ".jpg", ".jpeg" });
            StorageFile file = await picker.PickSaveFileAsync();

            if (file == null)
                return;
            using (Stream x = await file.OpenStreamForWriteAsync())
            {
                x.Seek(0, SeekOrigin.Begin);
                image.WriteTo(x);
            }

        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

        static MemoryStream ToMemoryStream(Stream input)
        {
            try
            {                                         // Read and write in
                byte[] block = new byte[0x1000];       // blocks of 4K.
                MemoryStream ms = new MemoryStream();
                while (true)
                {
                    int bytesRead = input.Read(block, 0, block.Length);
                    if (bytesRead == 0) return ms;
                    ms.Write(block, 0, bytesRead);
                }
            }
            finally { }
        }
    }
}


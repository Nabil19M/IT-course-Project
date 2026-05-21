using System.IO.Compression;
using System.Net;
using System.Net.Sockets;

class Server
{
    static void Main()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 5000);
        listener.Start();
        Console.WriteLine("Server started on port 5000...");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected!");

            Thread t = new Thread(() => HandleClient(client));
            t.Start();
        }
    }

    static void HandleClient(TcpClient client)
    {
        using (NetworkStream stream = client.GetStream())
        {

            byte[] sizeBuffer = new byte[4];
            stream.Read(sizeBuffer, 0, 4);
            int fileSize = BitConverter.ToInt32(sizeBuffer, 0);
            Console.WriteLine($"Receiving file ({fileSize} bytes)");

            byte[] fileBytes = new byte[fileSize];
            int totalRead = 0;
            while (totalRead < fileSize)
            {
                int read = stream.Read(fileBytes, totalRead, fileSize - totalRead);
                if (read == 0) break;
                totalRead += read;
            }
            Console.WriteLine("File received. Compressing...");

            byte[] compressed;
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream gz = new GZipStream(ms, CompressionMode.Compress))
                {
                    gz.Write(fileBytes, 0, fileBytes.Length);
                }
                compressed = ms.ToArray();
            }
            Console.WriteLine($"Compressed: {fileSize} -> {compressed.Length} bytes");
            byte[] compSizeBytes = BitConverter.GetBytes(compressed.Length);
            stream.Write(compSizeBytes, 0, 4);
            stream.Write(compressed, 0, compressed.Length);
            Console.WriteLine("Compressed file sent!\n");
        }
        client.Close();
    }
}

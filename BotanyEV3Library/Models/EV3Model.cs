using System.Net;
using System.Net.Sockets;
using System.Threading;

using Lego.Ev3.Core;
using Lego.Ev3.Desktop;

namespace BotanyEV3Library.Models
{
    class EV3Model
    {
        const byte INIT_CMD = 97; // a
        const byte TAKE_PICT_CMD = 116; // t
        const byte PIC_DONE_CMD = 100; // d

        Brick brick;

        UdpClient udpSocket;
        Thread cameraThread;

        // Red = 5
        // Blue = 2
        // Green = 3
        readonly int[] colorArray = { 5, 2 };

        int resetColor = 3;

        int colorIndex;
        int colorCounter;
        int iterations;

        bool cameraSystemReady;
        bool resetting;

        public EV3Model()
        {
            IPEndPoint ipEndpoint = new IPEndPoint(IPAddress.Any, 10000);
            udpSocket = new UdpClient(ipEndpoint);
            cameraSystemReady = false;
            resetting = false;

            brick = new Brick(new UsbCommunication());
            brick.BrickChanged += OnBrickChanged;
            iterations = 6;
        }

        public async void Initialize()
        {
            //cameraProcess = Process.Start("BotanyCameraController");
            await brick.ConnectAsync();
            await brick.DirectCommand.TurnMotorAtSpeedAsync(OutputPort.A, 0);
            await brick.DirectCommand.TurnMotorAtSpeedAsync(OutputPort.B, 0);
            brick.Ports[InputPort.One].SetMode(ColorMode.Color);

            cameraThread = new Thread(CameraManagerThread);
            cameraThread.Start();
        }

        public void Execute()
        {
            if (!cameraSystemReady)
                return;

            colorIndex = 0;
            colorCounter = 0;

            brick.DirectCommand.TurnMotorAtSpeedAsync(OutputPort.A, 10);
            brick.DirectCommand.TurnMotorAtSpeedAsync(OutputPort.A, -10);
        }

        public void Stop()
        {
            brick.DirectCommand.StopMotorAsync(OutputPort.A, true);
            if (cameraThread != null)
                cameraThread.Abort();
            cameraSystemReady = false;
            //cameraProcess.Kill();
        }

        public void Reset()
        {

        }

        public void CameraManagerThread()
        {
            byte[] data;
            IPEndPoint senderEndpoint = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                data = udpSocket.Receive(ref senderEndpoint);
                if (data.Length < 1)
                    continue;

                if (data[0] == INIT_CMD)
                    break;
            }

            cameraSystemReady = true;

            while (true)
            {
                while (true)
                {
                    data = udpSocket.Receive(ref senderEndpoint);
                    if (data.Length < 1)
                        continue;

                    if (data[0] == PIC_DONE_CMD)
                        break;
                }

                ++colorCounter;

                if (colorCounter == iterations)
                {
                    resetting = true;
                    brick.DirectCommand.TurnMotorAtSpeedAsync(OutputPort.A, -10);
                    brick.DirectCommand.TurnMotorAtSpeedAsync(OutputPort.B, 10);
                    return;
                }

                brick.DirectCommand.TurnMotorAtSpeedAsync(OutputPort.A, 10);
                brick.DirectCommand.TurnMotorAtSpeedAsync(OutputPort.B, -10);
            }
        }

        async void OnBrickChanged(object sender, BrickChangedEventArgs e)
        {
            byte[] messagePacket = new byte[1];

            if (resetting)
            {
                if (e.Ports[InputPort.One].SIValue != resetColor)
                    return;

                await brick.DirectCommand.TurnMotorAtSpeedAsync(OutputPort.A, 0);
                resetting = false;
            }

            if (e.Ports[InputPort.One].SIValue != colorArray[colorIndex])
                return;

            await brick.DirectCommand.TurnMotorAtSpeedAsync(OutputPort.A, 0);
            colorIndex = colorCounter % 2;

            messagePacket[0] = 116; // t
            IPEndPoint sendEndpoint = new IPEndPoint(IPAddress.Loopback, 10001);

            udpSocket.Send(messagePacket, messagePacket.Length, sendEndpoint);
        }
    }
}

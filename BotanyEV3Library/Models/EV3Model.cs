using System.Net;
using System.Net.Sockets;
using System.Threading;

using Lego.Ev3.Core;
using Lego.Ev3.Desktop;

namespace BotanyEV3Library.Models
{
    public class EV3Model
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

        int motorSpeed;

        bool cameraSystemReady;
        bool resetting;
        bool testRun;
        bool initialized;

        static EV3Model ev3ModelInstance = null;

        public static EV3Model GetInstance()
        {
            return ev3ModelInstance;
        }

        public EV3Model()
        {
            IPEndPoint ipEndpoint = new IPEndPoint(IPAddress.Any, 10000);
            udpSocket = new UdpClient(ipEndpoint);
            cameraSystemReady = false;
            resetting = false;

            brick = new Brick(new UsbCommunication());
            brick.BrickChanged += OnBrickChanged;
            iterations = 1;
            testRun = false;
            initialized = false;
            motorSpeed = 10;
            ev3ModelInstance = this;
        }

        public async void Initialize()
        {
            //cameraProcess = Process.Start("BotanyCameraController");
            if (initialized)
                return;
            await brick.ConnectAsync();
            StopMotor();
            brick.Ports[InputPort.One].SetMode(ColorMode.Color);

            initialized = true;

            if (testRun)
                return;
            cameraThread = new Thread(CameraManagerThread);
            cameraThread.Start();
        }

        public void Execute()
        {
            if (!initialized)
                return;

            if (!testRun && !cameraSystemReady)
                return;

            colorIndex = 0;
            colorCounter = 0;

            ForwardMotor();
        }

        public void Stop()
        {
            if (!initialized)
                return;
            StopMotor();
        }

        public async void Reset()
        {
            if (!initialized)
                return;
            resetting = true;
            await brick.DirectCommand.TurnMotorAtSpeedAsync(OutputPort.A, -motorSpeed);
            await brick.DirectCommand.TurnMotorAtSpeedAsync(OutputPort.B, motorSpeed);
            colorCounter = 0;
        }

        public void Shutdown()
        {
            if (!initialized)
                return;

            StopMotor();
            brick.Disconnect();
            if (cameraThread != null)
                cameraThread.Abort();
            cameraSystemReady = false;
            //cameraProcess.Kill();
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
                    Reset();
                    return;
                }

                ForwardMotor();
            }
        }

        void OnBrickChanged(object sender, BrickChangedEventArgs e)
        {
            byte[] messagePacket = new byte[1];

            if (resetting)
            {
                if (e.Ports[InputPort.One].SIValue != resetColor)
                    return;

                StopMotor();
                resetting = false;
            }

            if (e.Ports[InputPort.One].SIValue != colorArray[colorIndex])
                return;

            StopMotor();
            colorIndex = colorCounter % 2;

            if(testRun)
            {
                Thread.Sleep(2000);
                ++colorCounter;
                if (colorCounter == iterations)
                {
                    Reset();
                    return;
                }
                ForwardMotor();
                return;
            }
            messagePacket[0] = 116; // t
            IPEndPoint sendEndpoint = new IPEndPoint(IPAddress.Loopback, 10001);

            udpSocket.Send(messagePacket, messagePacket.Length, sendEndpoint);
        }

        public int MotorSpeed
        {
            get { return motorSpeed; }
            set {
                if (value > 50) return;
                motorSpeed = value;
            }
        }

        public int Iterations
        {
            get { return iterations; }
            set { iterations = value; }
        }

        public bool TestRun
        {
            get { return testRun; }
            set { testRun = value; }
        }

        private async void ForwardMotor()
        {
            await brick.DirectCommand.TurnMotorAtSpeedAsync(OutputPort.A, motorSpeed);
            await brick.DirectCommand.TurnMotorAtSpeedAsync(OutputPort.B, -motorSpeed);
        }

        private async void StopMotor()
        {
            await brick.DirectCommand.TurnMotorAtSpeedAsync(OutputPort.A, 0);
            await brick.DirectCommand.TurnMotorAtSpeedAsync(OutputPort.B, 0);
        }
    }
}

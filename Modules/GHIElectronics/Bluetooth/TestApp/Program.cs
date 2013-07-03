using System.Threading;
using Microsoft.SPOT;

using Gadgeteer.Modules.GHIElectronics;

namespace TestApp
{
    public partial class Program
    {
        public static Bluetooth.Client client;

        void ProgramStarted()
        {
            bluetooth.Reset();

            client = bluetooth.ClientMode;
            //client.EnterPairingMode();

            // set up bluetooth module connection parameters
            bluetooth.SetDeviceName("lulzlookpics");    // change this to whatever name you want
            bluetooth.SetPinCode("1234");           //likewise, set whatever PIN you want.

            // need a handler for state changes and data recieved.
            bluetooth.BluetoothStateChanged += new Bluetooth.BluetoothStateChangedHandler(bluetooth_BluetoothStateChanged);
            bluetooth.DataReceived += new Bluetooth.DataReceivedHandler(bluetooth_DataReceived);

            button.ButtonPressed += new Button.ButtonEventHandler(button_ButtonPressed);
            
            // put the device in pairing mode as part of a normal execution
            client.EnterPairingMode();
            
            Debug.Print("Program Started");
        }

        void button_ButtonPressed(Button sender, Button.ButtonState state)
        {
            // check to see that the bluetooth module is connected, and if so send the joystick pressed notification
            if (bluetooth.IsConnected)
            {
                client.Send("You pressed the button\r\n");
            }
            // otherwise, just go into first-time pairing mode.
            else
            {
                client.EnterPairingMode();
            }
        }

        private void bluetooth_DataReceived(Bluetooth sender, string data)
        {
            // For sample purposes, we'll just debug print what we get
            Debug.Print("Recieved: " + data);
        }

        void bluetooth_BluetoothStateChanged(Bluetooth sender, Bluetooth.BluetoothState btState)
        {
            // here the bluetooth module's state has changed. First, just debug print the value so we know what is happening
            Debug.Print("New state:" + btState.ToString());

            // If the state is now "connected", we can do stuff over the link.
            if (btState == Bluetooth.BluetoothState.Connected)
            {
                Debug.Print("Connected");
                Thread.Sleep(900);      // do this to wait for BT module to connect; 900 may be too long, but a quick trial and error seemed to show this was ok.
                // if we don't have this pause, then the BT module will take the data we send it and loop it back as input.
                client.Send("Connected to Fez\r\n");
            }
            // if the state is now "disconnected", you might need to stop other processes but for this example we'll just confirm that in the debug output window
            if (btState == Bluetooth.BluetoothState.Disconnected)
            {
                Debug.Print("Disconnected");
            }
        }

    }
}

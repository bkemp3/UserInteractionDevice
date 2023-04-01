using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.UI;

public class BleTest : MonoBehaviour
{
    // Change this to match your device.
    string targetDeviceName = "ESP32-RUST";
    // string targetDeviceName = "ESP32";
    // string targetDeviceName = "Long name works now";
    string serviceUuid = "{4fafc201-1fb5-459e-8fcc-c5c9c331914b}";
    string[] characteristicUuids = {
         "{beb5483e-36e1-4688-b7f5-ea07361b26a8}"      // CUUID 1
        //  "{617c753e-5199-11eb-ae93-0242ac130002}"       // CUUID 2
    };
    string characteristicUuid = "beb5483e-36e1-4688-b7f5-ea07361b26a8";

    BLE ble;
    BLE.BLEScan scan;
    bool isScanning = false, isConnected = false;
    string deviceId = null, toSend = "";
    IDictionary<string, string> discoveredDevices = new Dictionary<string, string>();
    int devicesCount = 0;
    int i = 0;

    // BLE Threads 
    Thread scanningThread, connectionThread, readingThread, writingThread;

    // GUI elements
    public Text TextDiscoveredDevices, TextIsScanning, TextTargetDeviceConnection, TextTargetDeviceData;
    // TODO: change back to pressablebutton (HL2)
    public Button ButtonEstablishConnection, ButtonStartScan;
    string remoteAngle, lastRemoteAngle;
    // byte remoteAngle, lastRemoteAngle;
    float qw, qx, qy, qz;
    string[] q = null;
    public GameObject body;

    // Start is called before the first frame update
    void Start()
    {
        ble = new BLE();
        ButtonEstablishConnection.enabled = false;
        TextTargetDeviceConnection.text = targetDeviceName + " not found.";
        readingThread = new Thread(ReadBleData);

        // Initialize GameObjects that need to be searched
        body = GameObject.FindWithTag("body");
        // writingThread = new Thread(WriteBLEData);
        // writingThread = new ParameterizedThreadStart(WriteBLEData);
        // writingThread = new Thread (() => WriteBLEData(toSend));
        // writingThread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (isScanning)
        {
            if (ButtonStartScan.enabled)
                ButtonStartScan.enabled = false;

            if (discoveredDevices.Count > devicesCount)
            {
                UpdateGuiText("scan");
                devicesCount = discoveredDevices.Count;
            }
        }
        else
        {
            /* Restart scan in same play session not supported yet.
            if (!ButtonStartScan.enabled)
                ButtonStartScan.enabled = true;
            */
            if (TextIsScanning.text != "Not scanning.")
            {
                TextIsScanning.color = Color.white;
                TextIsScanning.text = "Not scanning.";
            }
        }

        // The target device was found.
        if (deviceId != null && deviceId != "-1")
        {
            // Target device is connected and GUI knows.
            if (ble.isConnected && isConnected)
            {
                UpdateGuiText("writeData");
                UpdateGuiText("readData");
            }
            // Target device is connected, but GUI hasn't updated yet.
            else if (ble.isConnected && !isConnected)
            {
                UpdateGuiText("connected");
                isConnected = true;
                Debug.Log("UpdateGUIConnect");
                // Device was found, but not connected yet. 
            }
            else if (!ButtonEstablishConnection.enabled && !isConnected)
            {
                ButtonEstablishConnection.enabled = true;
                TextTargetDeviceConnection.text = "Found target device:\n" + targetDeviceName;
            }
        }
        if (q != null)
        {
            rotation();
        }
    }

    private void OnDestroy()
    {
        CleanUp();
    }

    private void OnApplicationQuit()
    {
        CleanUp();
    }

    // Prevent threading issues and free BLE stack.
    // Can cause Unity to freeze and lead
    // to errors when omitted.
    private void CleanUp()
    {
        // try
        // {
        scan.Cancel();
        ble.Close();
        if (scanningThread.IsAlive)
        {
            Debug.Log("Scanning Thread is Alive");
            scanningThread.Abort();
        }
        if (connectionThread.IsAlive)
        {
            Debug.Log("Connection Thread is Alive");
            connectionThread.Abort();
        }
        if (readingThread.IsAlive)
        {
            Debug.Log("Reading Thread is Alive");
            readingThread.Abort();
        }
        // if (writingThread.IsAlive){
        // Debug.Log("Writing Thread is Alive");
        // writingThread.Abort();}
        // Environment.Exit(Environment.ExitCode);

        // } catch(NullReferenceException e)
        // {
        // Debug.Log("Thread or object never initialized.\n" + e);
        // }        
    }

    public void StartScanHandler()
    {
        Debug.Log("Start Scanning");
        devicesCount = 0;
        isScanning = true;
        discoveredDevices.Clear();
        scanningThread = new Thread(ScanBleDevices);
        scanningThread.Start();
        TextIsScanning.color = new Color(244, 180, 26);
        TextIsScanning.text = "Scanning...";
        TextDiscoveredDevices.text = "";
    }

    public void ResetHandler()
    {
        TextTargetDeviceData.text = "";
        TextTargetDeviceConnection.text = targetDeviceName + " not found.";
        // Reset previous discovered devices
        discoveredDevices.Clear();
        TextDiscoveredDevices.text = "No devices.";
        deviceId = null;
        CleanUp();
    }

    private void WriteBLEData(string toSend)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(toSend);
        bool res = BLE.WritePackage(deviceId, serviceUuid, characteristicUuid, data);
        Debug.Log(String.Format("Sent {0}", toSend));
        if (res)
        {
            Debug.Log(String.Format("Sent {0}", toSend));
        }
        Thread.Sleep(100);
    }

    private void ReadBleData(object obj)
    {
        byte[] packageReceived = BLE.ReadPackage();
        // Convert little Endian.
        // In this example we're interested about an angle
        // value on the first field of our package.
        remoteAngle = System.Text.Encoding.ASCII.GetString(packageReceived).TrimEnd('\0');
        Debug.Log("Angle: " + remoteAngle);
        // Debug.Log(i);
        // i++;
        q = remoteAngle.Split(",");
        Thread.Sleep(25);
    }


    // NOTE: Uncomment the following function if you want to read byte arrays.
    // private void ReadBleData(object obj)
    // {
    //     byte[] packageReceived = BLE.ReadBytes();
    //     // Convert little Endian.
    //     // In this example we're interested about an angle
    //     // value on the first field of our package.
    //     remoteAngle = packageReceived[0];
    //     Debug.Log("Angle: " + remoteAngle);
    //     //Thread.Sleep(100);
    // }

    void UpdateGuiText(string action)
    {
        switch (action)
        {
            case "scan":
                TextDiscoveredDevices.text = "";
                foreach (KeyValuePair<string, string> entry in discoveredDevices)
                {
                    TextDiscoveredDevices.text += "DeviceID: " + entry.Key + "\nDeviceName: " + entry.Value + "\n\n";
                    Debug.Log("Added device: " + entry.Key);
                }
                break;
            case "connected":
                ButtonEstablishConnection.enabled = false;
                TextTargetDeviceConnection.text = "Connected to target device:\n" + targetDeviceName;
                break;
            case "readData":
                if (!readingThread.IsAlive)
                {
                    // InvokeRepeating("readingThread", 0.1f, 0.001f);

                    readingThread = new Thread(ReadBleData);
                    readingThread.Start();
                }
                if (remoteAngle != lastRemoteAngle)
                {
                    TextTargetDeviceData.text = "Remote angle: " + remoteAngle;
                    lastRemoteAngle = remoteAngle;
                }
                break;
                // case "writeData":
                // // Writing is on the main thread for now
                // // TODO: Figure out a way to make threading work when the function has arguments
                //     // WriteBLEData("toSend");
                //     if (!writingThread.IsAlive)
                //     {
                //         Debug.Log("Starting writing thread");
                //         writingThread = new Thread(()=>WriteBLEData(toSend));
                //         writingThread.Start();
                //     }
                //     break;
        }
    }

    void ScanBleDevices()
    {
        scan = BLE.ScanDevices();
        Debug.Log("BLE.ScanDevices() started.");
        scan.Found = (_deviceId, deviceName) =>
        {
            Debug.Log("found device with name: " + deviceName);
            discoveredDevices.TryAdd(_deviceId, deviceName);

            if (deviceId == null && deviceName == targetDeviceName)
                deviceId = _deviceId;
        };

        scan.Finished = () =>
        {
            isScanning = false;
            Debug.Log("scan finished");
            if (deviceId == null)
                deviceId = "-1";
        };
        while (deviceId == null)
            Thread.Sleep(500);
        scan.Cancel();
        // scanningThread = null;
        isScanning = false;

        if (deviceId == "-1")
        {
            Debug.Log("no device found!");
            return;
        }
    }

    // Start establish BLE connection with
    // target device in dedicated thread.
    public void StartConHandler()
    {
        connectionThread = new Thread(ConnectBleDevice);
        connectionThread.Start();
    }

    void ConnectBleDevice()
    {
        Debug.Log("Connecting to ESP32");
        if (deviceId != null)
        {
            try
            {
                ble.Connect(deviceId,
                serviceUuid,
                characteristicUuids);
            }
            catch (Exception e)
            {
                Debug.Log("Could not establish connection to device with ID " + deviceId + "\n" + e);
            }
        }
        if (ble.isConnected)
            Debug.Log("Connected to: " + targetDeviceName);
    }

    ulong ConvertLittleEndian(byte[] array)
    {
        int pos = 0;
        ulong result = 0;
        foreach (byte by in array)
        {
            result |= ((ulong)by) << pos;
            pos += 8;
        }
        return result;
    }
    void rotation()
    {
        if (q.Length < 4)
        {
            return;
        }
        try
        {
            qx = float.Parse(q[0]);
            qy = float.Parse(q[1]);
            qz = float.Parse(q[2]);
            qw = float.Parse(q[3]);
        }
        catch
        {
            Debug.LogError("Cannot Parse IMU data");
            return;
        }
        if (qx == float.NaN || qy == float.NaN || qz == float.NaN || qw == float.NaN)
        {
            Debug.LogError("IMU data is nan");
            return;
        }
        // Unity accepts x,y,z,w
        Quaternion rot = new Quaternion(qx, qy, qz, qw);
        // Quaternion spin=Quaternion.Euler(-y, x,-z);
        Quaternion spin1 = Quaternion.Euler(new Vector3(-90, 0, 0));
        Quaternion spin2 = Quaternion.Euler(new Vector3(0, 0, -90));
        Quaternion spin3 = Quaternion.Euler(new Vector3(180, 0, 0));
        //body.transform.position;
        body.transform.rotation = spin1 * spin2 * spin3 * rot;
    }
}


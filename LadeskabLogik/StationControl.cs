﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using LadeskabLogik;

namespace Ladeskab
{
    public class StationControl
    {
        // Enum med tilstande ("states") svarende til tilstandsdiagrammet for klassen
        private enum LadeskabState
        {
            Available,
            Locked,
            DoorOpen
        };

        // Her mangler flere member variable
        private LadeskabState _state;
        private static IUsbCharger _charger = new UsbChargerSimulator();
        private int _oldId;
        public bool CurrentDoorStatus { get; set; }
        public bool CurrentRfidSensedStatus { get; set; }

        //private string logFile = "logfile.txt"; // Navnet på systemets log-fil

        private static IConsoleWriter _consoleWriter = new ConsoleWriter();
        private static ILog _logFile;

        private IDoor _door;
        static IDisplay _display = new Display(_consoleWriter);
        private IChargeControl _chargeControl = new ChargeControl(_charger, _display);
        public StationControl(IDoor doorStatus, IRfidReader rfidStatus, ILog logFile)
        {
            doorStatus.DoorChangedEvents += HandleDoorStatusChangedEvent;//attacher 
            _door = doorStatus;
            rfidStatus.RfidSensedEvents += HandleRfidStatusChangedEvent;//attacher
            _logFile = logFile;
        }

        private void HandleRfidStatusChangedEvent(object sender, RfidSensedEventArgs e)
        {
            CurrentRfidSensedStatus = e.RfidSensed;
            RfidDetected(10);//Evt. ændre ID senere

        }

        private void HandleDoorStatusChangedEvent(object sender, DoorChangedEventArgs e)
        {
            CurrentDoorStatus = e.DoorStatus;
            if (CurrentDoorStatus == false && _state != LadeskabState.Locked)//lukket dør
            {
                _display.writeDisplay("Indlæs RFID");
                _state = LadeskabState.Available;
            }

            if (CurrentDoorStatus == true && _state != LadeskabState.Locked)//åbnet dør
            {
                _display.writeDisplay("Tilslut telefon");
                _state = LadeskabState.DoorOpen;
            }
            
        }

        // Eksempel på event handler for eventet "RFID Detected" fra tilstandsdiagrammet for klassen
        private void RfidDetected(int id)
        {
            switch (_state)
            {
                case LadeskabState.Available:
                    // Check for ladeforbindelse
                    _chargeControl.IsConnected();//returnere en boolean 
                    if (_charger.Connected)
                    {
                        _door.LockDoor("Døren er lukket");
                        _charger.StartCharge();
                        _oldId = id;
                        _logFile.LogLadeskabAvailable(id);
                        Console.WriteLine("Skabet er låst og din telefon lades. Brug dit RFID tag til at låse op.");
                        _state = LadeskabState.Locked;
                    }
                    else
                    {
                        Console.WriteLine("Din telefon er ikke ordentlig tilsluttet. Prøv igen.");
                    }

                    break;

                case LadeskabState.DoorOpen:
                    // Ignore
                    break;

                case LadeskabState.Locked:
                    // Check for correct ID
                    if (id == _oldId)
                    {
                        _charger.StopCharge();
                        _door.UnlockDoor("Døren er åbnet");
                        _logFile.LogLadeskabLocked(id);
                        Console.WriteLine("Tag din telefon ud af skabet og luk døren");
                        _state = LadeskabState.Available;
                    }
                    else
                    {
                        Console.WriteLine("Forkert RFID tag");
                    }

                    break;
            }
        }

    }
}

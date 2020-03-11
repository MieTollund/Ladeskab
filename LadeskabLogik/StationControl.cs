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
        private IUsbCharger _charger;
        private int _oldId;
        public bool CurrentDoorStatus { get; set; }
        public bool CurrentRfidSensedStatus { get; set; }

        private string logFile = "logfile.txt"; // Navnet på systemets log-fil

        public StationControl(IDoor doorStatus, IRfidReader rfidStatus)
        {
            doorStatus.DoorChangedEvents += HandleDoorStatusChangedEvent;//attacher 
            rfidStatus.RfidSensedEvents += HandleRfidStatusChangedEvent;//attacher
        }

        private void HandleRfidStatusChangedEvent(object sender, RfidSensedEventArgs e)
        {
            CurrentRfidSensedStatus = e.RfidSensed;
        }

        private void HandleDoorStatusChangedEvent(object sender, DoorChangedEventArgs e)
        {
            CurrentDoorStatus = e.DoorStatus;
            if (CurrentDoorStatus == false)
            {
                Console.WriteLine("Indlæs RFID");
            }

            if (CurrentDoorStatus == true)
            {
                Console.WriteLine("Tilslut telefon");
            }
            
        }

        // Eksempel på event handler for eventet "RFID Detected" fra tilstandsdiagrammet for klassen
        private void RfidDetected(int id)
        {
            switch (_state)
            {
                case LadeskabState.Available:
                    // Check for ladeforbindelse
                    if (_charger.Connected)
                    {
                        //_door.LockDoor();
                        _charger.StartCharge();
                        _oldId = id;
                        using (var writer = File.AppendText(logFile))
                        {
                            writer.WriteLine(DateTime.Now + ": Skab låst med RFID: {0}", id);
                        }

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
                        //_door.UnlockDoor();
                        using (var writer = File.AppendText(logFile))
                        {
                            writer.WriteLine(DateTime.Now + ": Skab låst op med RFID: {0}", id);
                        }

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

        // Her mangler de andre trigger handlere
    }
}

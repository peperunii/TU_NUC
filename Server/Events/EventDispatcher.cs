using System;

namespace Server.Events
{
    public class EventDispatcher
    {
        // Delegates
        public delegate void AdminStartRecordingEventHandler(object sender, EventArgs e);
        public delegate void AdminStartCalibrationEventHandler(object sender, EventArgs e);
        public delegate void AdminStartDiscoveryEventHandler(object sender, EventArgs e);

        // Events
        public event AdminStartRecordingEventHandler AdminStartRecording;
        public event AdminStartCalibrationEventHandler AdminStartCalibration;
        public event AdminStartDiscoveryEventHandler AdminStartDiscovery;

        public virtual void OnAdminStartRecording()
        {
            AdminStartRecording?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnAdminStartCalibration()
        {
            AdminStartCalibration?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnAdminStartDiscovery()
        {
            AdminStartDiscovery?.Invoke(this, EventArgs.Empty);
        }
    }
}

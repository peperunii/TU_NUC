namespace Network.Messages
{
    public enum MessageType : ushort
    {
        Discovery = 0,    //
        DiscoveryResponse,//

        Info,             //
        KeepAlive,        //
        TimeSyncRequest,  //
        TimeInfo,         //

        RestartClientApp,  //
        RestartClientDevice,//
        RestartServerApp,   //
        RestartServerDevice,//
        ShutdownDevice,

        CameraStart,
        CameraStop,
        CameraStatus,
        CameraStatusRequest,

        ConnectedClients,    //
        GetConnectedClients,  //

        ReloadConfiguration, //
        GetConfigurationPerClient, //
        ShowConfigurationPerClient, //
        StoreConfigurationPerClient,

        Calibration,
        CalibrationRequest, //
        
        ColorFrame,         //
        DepthFrame,
        IRFrame,
        Skeleton,

        ColorFrameRequest, //
        DepthFrameRequest, //
        IRFrameRequest,  //
        SkeletonRequest  //
    }
}

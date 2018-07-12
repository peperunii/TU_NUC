namespace Client.Cameras
{
    public abstract class Camera
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public abstract object GetData(CameraDataType type);
    }
}

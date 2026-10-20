namespace VMixClone.Models
{
    public enum InputType
    {
        Video,
        Camera,
        DesktopCapture,
        Image,
        AudioFile,
        AudioDevice,
        Title,
        Colour,
        ColorBars,
        Clock,
        WebBrowser,
        VirtualSet,
        Playlist,
        PhotoSlideshow
    }

    public enum TransitionEffect
    {
        Cut,
        Fade,
        Zoom,
        Wipe,
        Slide,
        Fly,
        Merge,
        VerticalWipe,
        CubeZoom,
        BarnDoor
    }

    public enum InputCategory
    {
        All,
        Red,
        Green,
        Orange,
        Purple,
        Aqua,
        Blue
    }
}

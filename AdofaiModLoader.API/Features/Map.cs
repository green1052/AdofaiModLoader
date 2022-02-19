namespace AdofaiModLoader.API.Features;

public static class Map
{
    public static float Pitch
    {
        get => scrConductor.instance.song.pitch;
        set => scrConductor.instance.song.pitch = value;
    }
}
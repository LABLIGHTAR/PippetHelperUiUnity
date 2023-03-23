public class Tool
{
    public string name;
    public int numChannels;
    public string orientation;
    public float volume;

    public Tool(string name, int numChannels, string orientation, float volume)
    {
        this.name = name;
        this.numChannels = numChannels;
        this.orientation = orientation;
        this.volume = volume;
    }

    public void SetVolume(float value)
    {
        if (volume != value)
        {
            volume = value;
        }
    }
}

namespace MidiMapper
{
    public interface INoteMap
    {
        bool TryMap(byte source, out byte target);
    }
}
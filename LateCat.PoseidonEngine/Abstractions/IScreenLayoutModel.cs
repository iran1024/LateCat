namespace LateCat.PoseidonEngine.Abstractions
{
    public interface IScreenLayoutModel
    {
        string PropertyPath { get; set; }
        IMonitor Monitor { get; set; }
        string Title { get; set; }
    }
}
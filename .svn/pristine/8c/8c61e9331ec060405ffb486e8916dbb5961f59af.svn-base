using LateCat.PoseidonEngine.Core;
using System.Drawing;

namespace LateCat.PoseidonEngine.Abstractions
{
    public interface ITaskbarOperator
    {
        bool IsRunning { get; }

        string CheckIncompatiblePrograms();

        Color GetAverageColor(string filePath);

        void SetAccentColor(Color color);

        void Start(TaskbarTheme theme);

        void Stop();
    }
}

using System;
using System.Threading.Tasks;

namespace UserProfileImageUpdater
{
    public interface IUpdaterTask
    {
        DateTime LastChecked { get; set; }
        TimeSpan CheckInterval { get; set; }

        Task Check();
        Task<bool> Update();
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UserProfileImageUpdater
{
    public class UserProfileImageUpdater
    {
        public DateTime StartTime { get; set; }
        private static List<IUpdaterTask> Tasks = new List<IUpdaterTask>();
        public UserProfileImageUpdater()
        {
            Tasks.Add(new ProfileImageUpdater(TimeSpan.FromSeconds(Properties.Settings.Default.ProfileImageInterval), Properties.Settings.Default.ImageProfilePath));
        }

        public async Task CheckSources()
        {
            foreach(var task in Tasks)
            {
                await task.Check();
            }
        }
    }
}

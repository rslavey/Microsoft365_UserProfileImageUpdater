using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace UserProfileImageUpdater
{
    public partial class UserProfileImageUpdaterService : ServiceBase
    {
        public async Task StartUpdater()
        {
            var updater = new UserProfileImageUpdater
            {
                StartTime = DateTime.Now
            };
            updater.CheckSources();
        }

        public UserProfileImageUpdaterService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Logger.LogMessage("Service Starting", EventLogEntryType.Information, Logger.ServiceEventID.StatusMessage);
            StartUpdater();
        }

        protected override void OnStop()
        {
        }
    }
}

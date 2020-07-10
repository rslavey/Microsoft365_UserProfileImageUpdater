using System.ComponentModel;
using System.Diagnostics;

namespace UserProfileImageUpdater
{
    class Logger
    {
        public enum ServiceEventID
        {
            EncryptionError = 55001,
            ExchangeOnlineConnectionError = 55002,
            ExchangeOnlineUpdateError = 55003,
            StatusMessage = 55004,
            ProfileImageUploaderFailure = 55005,
            LocalFileSystemError = 55006,
            SharePointOnlineError = 5507
        }

        internal static void LogMessage(string message, EventLogEntryType level, ServiceEventID eventId)
        {
            //Event Log max message size
            message = message.Length > 32765 ? message.Substring(0, 32765) : message;

            var eventLog = new EventLog
            {
                Source = Properties.Settings.Default.EventLogSource,
                Log = Properties.Settings.Default.EventLogName
            };

            ((ISupportInitialize)(eventLog)).BeginInit();

            if (!EventLog.SourceExists(eventLog.Source))
            {
                EventLog.CreateEventSource(eventLog.Source, eventLog.Log);
            }

            ((ISupportInitialize)(eventLog)).EndInit();

            var systemLoggingLevel = Properties.Settings.Default.LoggingLevel;

            if (systemLoggingLevel == "Verbose" ||
                (systemLoggingLevel == "Warning" && (int)level <= 2) ||
                (systemLoggingLevel == "Error" && (int)level <= 1))
            {
                eventLog.WriteEntry(message: $"{message}", type: level, eventID: (int)eventId);
            }
        }
    }
}

using System;

namespace CloudCoinCore
{
    public class ProgressChangedEventArgs : EventArgs
    {
        public double MinorProgress;
        public double MajorProgress;
        public String MinorProgressMessage;
        public String MajorProgressMessage;
        public ProgressChangedEventArgs()
        {

        }
    }

    public class ProgressEventArgs : EventArgs
    {
        public string Status { get; private set; }
        public int percentage { get; private set; }
        public ProgressEventArgs(string status, int percentage = 0)
        {
            Status = status;
            this.percentage = percentage;
        }


    }

    public enum DepositStage { Echo, Detect, None };

    public class ProgressReport
    {
        public DepositStage Stage;
        //current progress
        public double CurrentProgressAmount { get; set; }
        //total progress
        public int TotalProgressAmount { get; set; }
        //some message to pass to the UI of current progress
        public string CurrentProgressMessage { get; set; }
    }

}

using System;

namespace CloudCoin
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
}

namespace Muffin.Common.Util
{
    public class OneTimeWriteString
    {
        private object _lock = new object();
        private bool _canWrite = true;
        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                lock (_lock)
                {
                    if (_canWrite)
                    {
                        _canWrite = false;
                        _value = value;
                    }
                }
            }
        }

        public static implicit operator string(OneTimeWriteString a)
        {
            return a.Value;
        }

        public override string ToString()
        {
            return this;
        }
    }
}

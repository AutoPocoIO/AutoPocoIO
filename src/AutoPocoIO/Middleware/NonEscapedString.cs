namespace AutoPocoIO.Middleware
{
    internal class NonEscapedString
    {
        private readonly string _value;
        public NonEscapedString(string value)
        {
            _value = value;
        }
        public override string ToString()
        {
            return _value;
        }
    }
}

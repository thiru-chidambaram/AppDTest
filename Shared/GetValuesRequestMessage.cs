using System;

namespace Shared
{
    public class GetValuesRequestMessage
    {
        public int Index { get; set; }
    }

    public class GetValuesResponseMessage
    {
        public string Message { get; set; }
    }
}

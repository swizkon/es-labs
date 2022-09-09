using System;

namespace ES.Labs.Domain
{
    public static class EventStoreConfiguration
    {
        public static string StreamName => "transactions";
        public static string DeviceStreamName => "device-mainroom";
    }
}

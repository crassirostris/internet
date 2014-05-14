namespace DnsCache.Dns
{
    internal enum DnsMessageResponseCode
    {
        NoError,
        FormatError,
        ServerFailure,
        NameError,
        NotImplemented,
        Refused,
    }
}
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command;

[Serializable]
[ExcludeFromCodeCoverage]
public sealed class CommandDispatcherException : Exception
{
    public CommandDispatcherException()
    {
    }

    public CommandDispatcherException(string message)
        : base(message)
    {
    }

    public CommandDispatcherException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

#if NET8_0_OR_GREATER
    [Obsolete(DiagnosticId = "SYSLIB0051")] // add this attribute to the serialization ctor
#endif
    private CommandDispatcherException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

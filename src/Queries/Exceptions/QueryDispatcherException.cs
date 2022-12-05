﻿using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.Exceptions
{
    [Serializable]
    public sealed class QueryDispatcherException : Exception
    {
        public QueryDispatcherException()
        {
        }

        public QueryDispatcherException(string message)
            : base(message)
        {
        }

        public QueryDispatcherException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        private QueryDispatcherException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
   
}

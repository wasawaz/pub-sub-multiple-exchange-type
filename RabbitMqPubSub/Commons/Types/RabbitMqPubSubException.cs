using System;

namespace RabbitMqPubSub.Commons.Types
{
    public class RabbitMqPubSubException : Exception
    {
        public string Code { get; }

        public RabbitMqPubSubException()
        {
        }

        public RabbitMqPubSubException(string code)
        {
            Code = code;
        }

        public RabbitMqPubSubException(string message, params object[] args) 
            : this(string.Empty, message, args)
        {
        }

        public RabbitMqPubSubException(string code, string message, params object[] args) 
            : this(null, code, message, args)
        {
        }

        public RabbitMqPubSubException(Exception innerException, string message, params object[] args)
            : this(innerException, string.Empty, message, args)
        {
        }

        public RabbitMqPubSubException(Exception innerException, string code, string message, params object[] args)
            : base(string.Format(message, args), innerException)
        {
            Code = code;
        }        
    }
}
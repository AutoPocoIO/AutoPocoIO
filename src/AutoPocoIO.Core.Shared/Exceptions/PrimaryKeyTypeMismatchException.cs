using AutoPocoIO.DynamicSchema.Models;
using System;

namespace AutoPocoIO.Exceptions
{
    public class PrimaryKeyTypeMismatchException : BaseCaughtException
    {
        private readonly string _message;

        public PrimaryKeyTypeMismatchException(PrimaryKeyInformation keyInfo, Type typeFound)
        {
            Check.NotNull(keyInfo, nameof(keyInfo));
            Check.NotNull(typeFound, nameof(typeFound));

            _message = $"Primary key for entity {keyInfo.Name} is expecting {keyInfo.Type.Name} but {typeFound.Name} was provided.";
        }

        public PrimaryKeyTypeMismatchException(PrimaryKeyInformation keyInfo, string valueFound)
        {
            Check.NotNull(keyInfo, nameof(keyInfo));
            Check.NotNull(valueFound, nameof(valueFound));

            _message = $@"Primary key for entity {keyInfo.Name} is expecting {keyInfo.Type.Name} but the string ""{valueFound}"" cannot be converted.";
        }

        public override string Message => _message;
    }
}

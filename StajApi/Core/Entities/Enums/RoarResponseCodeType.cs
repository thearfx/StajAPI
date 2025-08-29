// StajApi/Entities/Enums/RoarResponseCodeType.cs
using System.Runtime.Serialization;

namespace StajApi.Entities.Enums
{
    public enum RoarResponseCodeType
    {
        [EnumMember]

        Success = 200,

        [EnumMember]

        Warning = 400,

        [EnumMember]

        Error = 402,

             ValidationError = 4,
       
        Info = 3,          // <-- Bu satırı ekleyin!
      
    }
}
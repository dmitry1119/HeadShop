namespace RH.HeadShop.IO
{
    /// <summary> Enum, for log message type </summary>
    public enum EchoMessageType
    {
        Information,
        Warning,
        Error
    }

    /// <summary> Extention for enumeration. By enum element return title </summary>
    public static class EchoMessageTypeEx
    {
        /// <summary> Get title for enum </summary>
        /// <param name="messageType">enum element</param>
        /// <returns>Title of enum element</returns>
        public static string GetTitle(this EchoMessageType messageType)
        {
            switch (messageType)
            {
                case EchoMessageType.Information:
                    return "Information";
                case EchoMessageType.Warning:
                    return "Warning";
                case EchoMessageType.Error:
                    return "Error";
                default:
                    return string.Empty;
            }
        }
    }
}

namespace repository.doraemon.Entities
{
    public enum ImageRecognitionStatus
    {
        /// <summary>
        /// User prompt for recognition
        /// </summary>
        Prompt = 0,

        /// <summary>
        /// Processing recognition
        /// </summary>
        Processing = 1,

        /// <summary>
        /// Succeeded
        /// </summary>
        Succeeded = 2,

        /// <summary>
        /// Failed
        /// </summary>
        Failed = 3
    }
}

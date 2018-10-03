namespace RazorLight
{
    public class RazorLightProjectIsNotSpecified : RazorLightException
    {
        public RazorLightProjectIsNotSpecified(string message =
            "There is no project specified to use with builder. You have to specify it before build") : base(message)
        {
        }
    }
}
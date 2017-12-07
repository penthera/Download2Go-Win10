using Penthera.VirtuosoClient.Public;

[assembly: ImplementationProvider("Penthera.VirtuosoClient.Client.Implementations.MimeChecker.Implementation")]
namespace Penthera.VirtuosoClient.Client.Implementations
{
    public class MimeChecker
    {
        public static IMimeTypeValidation Implementation()
        {
            return new MimeTypeValidation();
        }
    }

    class MimeTypeValidation : IMimeTypeValidation
    {
        
        public MimeTypeValidationHandler MimeTypeValidationHandler => ValidateMimeType;

        public string Description()
        {
            return "Implementation for validating mime types of downloaded assets";
        }

        public IMimeTypeValidation Implementation()
        {
            return this;
        }

        public string Name()
        {
            return "MimeTypeValidation";
        }

        private bool ValidateMimeType(IMimeTypeValidationArgs args)
        {
            if (args.MimeType.StartsWith("text"))
            {
                return args.EAssetType == EAssetType.Segment && args.ESegmentType == ESegmentType.OTHER;
            }
            return true;
        }
    }
}

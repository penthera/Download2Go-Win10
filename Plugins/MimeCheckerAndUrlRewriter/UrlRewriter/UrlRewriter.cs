using Penthera.VirtuosoClient.Public;
using System;
[assembly: ImplementationProvider("Penthera.VirtuosoClient.Client.Implementations.UrlRewriter.Implementation")]
namespace Penthera.VirtuosoClient.Client.Implementations
{
    public class UrlRewriter
    {
        public static IUrlRewriter Implementation()
        {
            return new Rewriter();
        }

       // Inner class implementing the Rewriter interface

        class Rewriter : IUrlRewriter
        {
            public UrlRewriteHandler UrlRewriteHandler => UrlRewriter;

            public string Description()
            {
                return "Rewrites the Uri to add signature or other params if needed";
            }

            public string Name()
            {
                return "UrlRewriter";
            }

            IUrlRewriter IImplementation<IUrlRewriter>.Implementation()
            {
                return this;
            }

            private Uri UrlRewriter(IUriRewriteArgs args)
            {
                //modify the Uri if needed and return the new value.
                // default implementation just returns the same Uri with no mods.
                return args.Url;
            }
        }
    }
}

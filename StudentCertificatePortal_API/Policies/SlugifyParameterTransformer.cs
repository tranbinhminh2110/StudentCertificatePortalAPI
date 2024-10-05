using System.Text.RegularExpressions;

namespace StudentCertificatePortal_API.Policies
{
    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        public string? TransformOutbound(object? value)
        {
            // Slugify value
            return value == null ? null : Regex.Replace(value.ToString()!, "([a-z])([A-Z])", "$1-$2").ToLower();
        }
    }
}

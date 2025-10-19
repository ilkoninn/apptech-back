using System.Text.RegularExpressions;

namespace AppTech.Business.Helpers
{
    public static class SlugCreator
    {
        public static string GenerateSlug(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var slug = input.ToLower();

            slug = Regex.Replace(slug, @"[^a-z0-9]+", "-");

            slug = slug.Trim('-');

            return slug;
        }
    }
}

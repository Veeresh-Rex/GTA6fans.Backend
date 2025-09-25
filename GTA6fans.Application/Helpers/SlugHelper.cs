namespace GTA6fans.Application.Helpers;

public static class SlugHelper
{
    public static string GenerateSlug(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return string.Empty;

        var slug = title.ToLowerInvariant();

        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");

        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");

        slug = slug.Trim('-');

        return slug;
    }
}

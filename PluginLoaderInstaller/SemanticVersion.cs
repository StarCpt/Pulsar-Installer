using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace PulsarInstaller;

public struct SemanticVersion(int major, int minor, int patch)
{
    public int Major = major, Minor = minor, Patch = patch;

    public static bool operator >(SemanticVersion a, SemanticVersion b)
    {
        if (a.Major > b.Major) return true;
        if (a.Minor > b.Minor) return true;
        if (a.Patch > b.Patch) return true;
        return false;
    }

    public static bool operator <(SemanticVersion a, SemanticVersion b)
    {
        if (a.Major < b.Major) return true;
        if (a.Minor < b.Minor) return true;
        if (a.Patch < b.Patch) return true;
        return false;
    }

    public static bool operator >=(SemanticVersion a, SemanticVersion b) => a > b || a == b;
    public static bool operator <=(SemanticVersion a, SemanticVersion b) => a < b || a == b;

    public static bool operator ==(SemanticVersion a, SemanticVersion b)
    {
        return a.Major == b.Major && a.Minor == b.Minor && a.Patch == b.Patch;
    }

    public static bool operator !=(SemanticVersion a, SemanticVersion b)
    {
        return !(a == b);
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is SemanticVersion other && this == other;
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(Major, Minor, Patch);
    }

    public static SemanticVersion Parse(string str)
    {
        str = Regex.Replace(str, "[A-Za-z]", ""); // remove any alphabet characters like "v" from "v1.2.3"
        string[] versionStrArr = str.Split('.');
        int majorVersion = versionStrArr.Length >= 1 && int.TryParse(versionStrArr[0], out int major) ? major : 0;
        int minorVersion = versionStrArr.Length >= 2 && int.TryParse(versionStrArr[1], out int minor) ? minor : 0;
        int patchVersion = versionStrArr.Length >= 3 && int.TryParse(versionStrArr[2], out int patch) ? patch : 0;
        return new SemanticVersion(majorVersion, minorVersion, patchVersion);
    }

    public override readonly string ToString() => $"{Major}.{Minor}.{Patch}";
}


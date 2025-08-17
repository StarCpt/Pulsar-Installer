using System.Diagnostics.CodeAnalysis;

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
}


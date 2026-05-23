namespace PRLab.Utilities;

public static class CoreUtilities
{
    private static Clock _clock;

    private static GuidGenerator? _guidGenerator;
    
    public static Clock Clock => _clock = _clock ?? new Clock();
    
    public static GuidGenerator GuidGenerator => _guidGenerator = _guidGenerator ?? new GuidGenerator();
    
    public static Guid GetOrGenerateGuid(Guid? guid)
    {
        return guid ?? GuidGenerator.New();
    }

    public static string GetSystemName() => "System";
}
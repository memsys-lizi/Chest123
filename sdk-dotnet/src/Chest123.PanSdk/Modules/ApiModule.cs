using Chest123.PanSdk.Internal;

namespace Chest123.PanSdk.Modules;

public abstract class ApiModule
{
    protected Pan123HttpClient Http { get; }

    protected ApiModule(Pan123HttpClient http)
    {
        Http = http;
    }
}


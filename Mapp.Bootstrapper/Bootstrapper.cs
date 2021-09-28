using System;

namespace Shmap.Bootstrapper
{
    public class Bootstrapper
    {
        // TODO configure container for the whole application
        // having this class separated into a standalone assembly allows to avoid dependencies in UI layer
        // jinak by UI WPF layer, tam kde startuje aplikace by musel vedet o vsech assemblies ktere se musi navazat do IoC containeru
        // coz je pak takova GOD-assembly
        // misto toho pouze Bootstrap assembly bude o tom vedet
    }
}

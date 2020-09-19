# AddUp.AnyLog

A dependency-free code-only logging abstraction.

## About

We currently make heavy use of [**Common.Logging**](https://github.com/net-commons/common-logging) in our internal shared libraries. Howvever, this poses several problems. I won't bother explaining why I made this lib as others have done before and will simply quote [**LibLog**](https://github.com/damianh/LibLog/wiki)'s author:

    LibLog is designed specifically and primarily for library developers that want to provide logging capabilities.

    This has been typically achieved in one of three ways:

    Take a hard dependency on a logging framework. Thus forcing all consumers of your library to also take a dependency on your choice of logging framework. This causes issues when they want to use a different framework or are using a different version of the same one.
    Use Common.Logging. Providing an abstraction over all the other frameworks has merit but it will add a reference to your consumers' projects. It's still a versionable dependency and can be prone to versioning clashes with other libraries. Common.Logging's ILog contains ~65 members whereas LibLog's ILog has just single method making it easier to implement custom loggers.
    Define your own Logging abstraction. A common approach used by many libraries that don't want to do 1 or 2. LibLog is designed to help those that would otherwise go down this path.

### Then, why not use LibLog?

[LibLog](https://github.com/damianh/LibLog) concept is very similar to this lib's. However:

* **LibLog** is not mainained anymore, The author's [rationale](https://github.com/damianh/LibLog/issues/270) is that from now on, library authors will probably use logging abstractions provided by Microsoft. Well, I'm not so sure about that happening soon, and in the meantime I need a solution.
* When I started implementing **AnyLog** I wasn't aware of the existence of **LibLog** and I did not feel like throwing my code away and take over **LibLog** maintenance. However it's stuffed with great ideas and I will happily cherry pick any stuff I find interesting in it and adapt it to **AnyLog** (starting with the Nuget packaging).
* **LibLog** supports several logging framework (and advanced features such as semantic logging or nested contexts), but not **Common.Logging**. For our internal use, supporting **Common.Logging** is mandatory (because we'll need time to completely get rid of it). And because our current logging framework of choice is [**NLog**](https://github.com/NLog/NLog) we also do support it.
* I already mentioned we needed to support **Common.Logging**, therefore I also modeled the logging API after **Common.Logging**'s. It is probably not the best one, but it eases our migration path.

Maybe some/all of the great features in **LibLog** will make it into this library. But only time will say. And it is definitely not a requirement to be API-compatible with **LibLog**.

## History

### [Version 0.1.0](WIP)

* WIP

## Credits

* [LibLog](https://github.com/damianh/LibLog) is the main source of inspiration of this library. A great thanks to the author for having cleared the path I'm now taking.

## License

This work is provided under the terms of the [MIT License](LICENSE).

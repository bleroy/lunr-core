# lunr-core
A port of [lunr.js](https://lunrjs.com/guides/getting_started.html) to .NET Core.
Lunr is a bit like Solr, but much smaller and not as bright.

![.NET Core](https://github.com/bleroy/lunr-core/workflows/.NET%20Core/badge.svg)

## TODO / up for grabs

* Multilingual support (lunr has optional support that remains to be ported)
* Documentation (adapted from [lunr docs](https://lunrjs.com/guides/getting_started.html))

## Example

A very simple search index can be created using the following:

```csharp
var index = await Index.Build(async builder =>
{
    builder
        .AddField("title")
        .AddField("body");

    await builder.Add(new Document
    {
        { "title", "Twelfth-Night" },
        { "body", "If music be the food of love, play on: Give me excess of it…" },
        { "author", "William Shakespeare" },
        { "id", "1" },
    });
});
```

Then searching is as simple as:

```csharp
await foreach (Result result in index.Search("love"))
{
    // do something with that result
}
```

This returns a list of matching documents with a [score](https://lunrjs.com/guides/searching.html#scoring) of how closely they match, the search query as well as any associated metadata about the match:

```csharp
new List<Result>
{
    new Result(
        documentReference: "1",
        score: 0.3535533905932737,
        matchData: new MatchData(
            term: "love",
            field: "body"
        )
    )
}
```

<!--[API documentation](https://lunrjs.com/docs/index.html) is available, as well as a [full working example](https://olivernn.github.io/moonwalkers/).-->

## Description

Lunr-core is a small, full-text search library for use in small applications.
It indexes documents and provides a simple search interface for retrieving documents that best match text queries.
It is 100% compatible with [lunr.js](https://lunrjs.com/guides/getting_started.html), meaning that an index file prepared on the server with lunr-core can be used on the client using lunr.js.

## Why

Lunr-core is suitable for small applications that require a simple search engine but without the overhead of a full-scale search engine such as Lucene.
Its compatibility with lunr.js also opens up some interesting client-side search scenarios.

<!--
## Installation

Simply include the lunr-core package in your application.
Lunr-core supports all .NET Standard 2.0 platforms, including .NET Core and .NET Framework 4.6.
-->

## Features

* Soon: Full text search support for 14 languages
* Boost terms at query time or boost entire documents at index time
* Scope searches to specific fields
* Fuzzy term matching with wildcards or edit distance
* No runtime dependencies beyond SDK, BCL AsyncInterfaces and System.Text.Json

<!--
## Contributing

See the [`CONTRIBUTING.md` file](CONTRIBUTING.md).
-->

## Credits

* Original code by [Oliver Nightingale](https://github.com/olivernn) and contributors, ported to .NET Core by [Bertrand Le Roy](https://github.com/bleroy).
* Icon adapted from https://commons.wikimedia.org/wiki/File:Internal_Structure_of_the_Moon.JPG by Iqbal Mahmud under Creative Commons Attribution Share Alike 4.0 International.
* Perf tests use a [word list by Sindre Sorhus](https://github.com/sindresorhus/word-list).

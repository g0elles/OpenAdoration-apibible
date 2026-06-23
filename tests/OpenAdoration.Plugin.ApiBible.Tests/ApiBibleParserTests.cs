using OpenAdoration.Plugin.ApiBible;
using Xunit;

namespace OpenAdoration.Plugin.ApiBible.Tests;

public class ApiBibleParserTests
{
    [Fact]
    public void ParseVersions_maps_id_name_abbreviation_language()
    {
        const string json = """
        {"data":[{"id":"de4e-02","name":"King James Version","abbreviation":"engKJV",
                  "language":{"id":"eng","name":"English"}}]}
        """;

        var versions = ApiBibleParser.ParseVersions(json);

        var v = Assert.Single(versions);
        Assert.Equal("de4e-02", v.Id);
        Assert.Equal("King James Version", v.Name);
        Assert.Equal("engKJV", v.Abbreviation);
        Assert.Equal("English", v.Language);
    }

    [Fact]
    public void ParseChapterVerses_groups_text_by_verseId_in_order()
    {
        // content-type=json: each text node carries attrs.verseId; verse 1's text is split across
        // two nodes to prove concatenation; book name comes from the caller (the DB join key).
        const string json = """
        {"data":{"id":"GEN.1","content":[
          {"type":"tag","name":"para","attrs":{"style":"p"},"items":[
            {"type":"tag","name":"verse","attrs":{"number":"1","sid":"GEN 1:1"},"items":[]},
            {"type":"text","text":"In the beginning God created ","attrs":{"verseId":"GEN.1.1"}},
            {"type":"text","text":"the heavens and the earth.","attrs":{"verseId":"GEN.1.1"}},
            {"type":"tag","name":"verse","attrs":{"number":"2","sid":"GEN 1:2"},"items":[]},
            {"type":"text","text":"Now the earth was formless and empty.","attrs":{"verseId":"GEN.1.2"}}
          ]}
        ]},"meta":{"fumsToken":"abc123"}}
        """;

        var verses = ApiBibleParser.ParseChapterVerses(json, "Genesis");

        Assert.Equal(2, verses.Count);
        Assert.Equal("Genesis", verses[0].Book);
        Assert.Equal(1, verses[0].Chapter);
        Assert.Equal(1, verses[0].Verse);
        Assert.Equal("In the beginning God created the heavens and the earth.", verses[0].Text);
        Assert.Equal(2, verses[1].Verse);
        Assert.Equal("Now the earth was formless and empty.", verses[1].Text);
    }

    [Fact]
    public void ParseChapterVerses_returns_empty_when_no_content()
    {
        Assert.Empty(ApiBibleParser.ParseChapterVerses("""{"data":{"id":"GEN.1"}}""", "Genesis"));
    }
}

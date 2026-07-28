using System.Text.Json.Nodes;
using Logistics.Infrastructure.AI.Models;
using Logistics.Infrastructure.AI.Services;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Services;

/// <summary>
/// Tool_use ids must round-trip exactly - the provider rejects any replayed tool_result whose id
/// no longer matches.
/// </summary>
public class CopilotTranscriptCodecTests
{
    [Fact]
    public void EncodeDecode_RoundTripsInterleavedBlocksWithToolUseIds()
    {
        var blocks = new List<LlmContentBlock>
        {
            new LlmTextBlock("Let me look that up."),
            new LlmToolUseBlock("toolu_abc123", "get_load", JsonNode.Parse("""{"load_id":"x"}""")),
            new LlmTextBlock("Found it.")
        };

        var decoded = CopilotTranscriptCodec.Decode(CopilotTranscriptCodec.Encode(blocks));

        Assert.Collection(decoded,
            b => Assert.Equal("Let me look that up.", Assert.IsType<LlmTextBlock>(b).Text),
            b =>
            {
                var toolUse = Assert.IsType<LlmToolUseBlock>(b);
                Assert.Equal("toolu_abc123", toolUse.Id);
                Assert.Equal("get_load", toolUse.Name);
                Assert.Equal("x", toolUse.Input!["load_id"]!.GetValue<string>());
            },
            b => Assert.Equal("Found it.", Assert.IsType<LlmTextBlock>(b).Text));
    }

    [Fact]
    public void EncodeDecode_RoundTripsToolResults()
    {
        var blocks = new List<LlmContentBlock>
        {
            new LlmToolResultBlock("toolu_abc123", """{"loads":[],"count":0}""")
        };

        var decoded = CopilotTranscriptCodec.Decode(CopilotTranscriptCodec.Encode(blocks));

        var result = Assert.IsType<LlmToolResultBlock>(Assert.Single(decoded));
        Assert.Equal("toolu_abc123", result.ToolUseId);
        Assert.Equal("""{"loads":[],"count":0}""", result.Content);
    }

    [Fact]
    public void Decode_UnknownBlockTypesAndMalformedJson_AreSkippedNotThrown()
    {
        Assert.Empty(CopilotTranscriptCodec.Decode("not json at all"));
        Assert.Empty(CopilotTranscriptCodec.Decode("""{"type":"text"}"""));

        var withUnknown = """[{"type":"thinking","text":"..."},{"type":"text","text":"kept"}]""";
        var block = Assert.Single(CopilotTranscriptCodec.Decode(withUnknown));
        Assert.Equal("kept", Assert.IsType<LlmTextBlock>(block).Text);
    }

    [Fact]
    public void Encode_SkipsImageAndDocumentBlocks()
    {
        var blocks = new List<LlmContentBlock>
        {
            new LlmTextBlock("hello"),
            new LlmImageBlock("image/png", "base64data")
        };

        var decoded = CopilotTranscriptCodec.Decode(CopilotTranscriptCodec.Encode(blocks));

        Assert.Single(decoded);
        Assert.IsType<LlmTextBlock>(decoded[0]);
    }
}

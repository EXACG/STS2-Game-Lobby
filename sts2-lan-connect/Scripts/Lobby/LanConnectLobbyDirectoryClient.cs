using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GD = Godot.GD;

namespace Sts2LanConnect.Scripts;

internal static class LanConnectLobbyDirectoryClient
{
    private const string DirectoryServersUrl = "https://sts.exacg.cc/servers/";

    public static async Task<IReadOnlyList<LobbyDirectoryServerEntry>> GetServersAsync(CancellationToken cancellationToken = default)
    {
        using HttpClient client = new()
        {
            Timeout = TimeSpan.FromSeconds(10d)
        };

        GD.Print($"sts2_lan_connect directory api: GET {DirectoryServersUrl}");
        using HttpResponseMessage response = await client.GetAsync(DirectoryServersUrl, cancellationToken);
        string text = await response.Content.ReadAsStringAsync(cancellationToken);
        GD.Print($"sts2_lan_connect directory api: GET {DirectoryServersUrl} -> {(int)response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            throw new LobbyServiceException("中心服务器列表请求失败。", "directory_http_error", (int)response.StatusCode);
        }

        LobbyDirectoryServerListResponse? parsed = JsonSerializer.Deserialize<LobbyDirectoryServerListResponse>(text, LanConnectJson.Options);
        if (parsed == null)
        {
            throw new LobbyServiceException("中心服务器返回了无效数据。", "directory_invalid_response", (int)response.StatusCode);
        }

        return parsed.Servers ?? new List<LobbyDirectoryServerEntry>();
    }
}

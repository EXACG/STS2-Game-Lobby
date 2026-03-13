# STS2 Game Lobby Releases

这个仓库用于分发《Slay the Spire 2》联机大厅相关发布产物。

当前发布内容：

- `sts2_lan_connect/`
  - 客户端 MOD 发布目录
  - 包含 macOS 双击入口 `install-sts2-lan-connect-macos.command`
  - 包含 Windows 双击入口 `install-sts2-lan-connect-windows.bat`
  - 包含默认大厅绑定 `lobby-defaults.json`
  - 包含大厅与续局联机使用说明
- `sts2_lan_connect-release.zip`
  - 客户端压缩发布包
- `联机大厅mod.zip`
  - 客户端压缩发布包的中文别名
- `sts2_lobby_service/`
  - Linux 服务端发布目录
  - 包含一键部署脚本和服务端源码
- `sts2_lobby_service.zip`
  - 服务端压缩发布包

当前客户端特性：

- 游戏内大厅支持关键词搜索、分页、房间状态标签和服务健康图标
- 多人续局存档会和大厅房间绑定，房主重新进入续局时自动重新发布
- 房间显示真实游戏版本、真实 MOD 版本、`relay` 状态和是否已开局
- 加入失败会细分为版本不一致、MOD 不一致、房间已开局、房间已满等原因
- macOS 安装 / 卸载脚本会自动刷新 `SlayTheSpire2.app` 的签名，避免 bundle 被修改后无法启动
- 当前公开发布包使用的大厅地址、兼容档位和连接策略，以 `sts2_lan_connect/lobby-defaults.json` 为准

使用说明：

- 客户端说明见 [sts2_lan_connect/README.md](./sts2_lan_connect/README.md)
- 客户端玩家手册见 [sts2_lan_connect/STS2_LAN_CONNECT_USER_GUIDE_ZH.md](./sts2_lan_connect/STS2_LAN_CONNECT_USER_GUIDE_ZH.md)
- 服务端说明见 [sts2_lobby_service/README.md](./sts2_lobby_service/README.md)

公网部署提醒：

- 大厅 API 需要放行 `8787/TCP`
- relay fallback 需要额外放行 `39000-39063/UDP`
- 如果客户端启用了 `Clash`、`Surge`、系统全局代理或 `TUN`，请让大厅服务器 IP 走 `DIRECT`

# STS2 LAN Connect 安装说明

这是 `STS2 LAN Connect` 的客户端发布包。

## 当前版本说明

- 当前客户端版本：`0.1.2`
- 大厅支持房间搜索、分页、状态标签和服务健康图标
- 房间列表支持分页、鼠标滚轮、移动端按住滑动和加宽滚动条的多层兜底交互
- 房间会显示真实游戏版本、真实 MOD 版本、`relay` 状态和是否已开局
- 加入失败会细分为版本不一致、MOD 不一致、缺少哪些 MOD、房间已开局、房间已满等原因
- 多人续局存档会自动和大厅房间绑定，房主重新进入续局时会自动重新发布
- 安装包内的默认大厅地址、兼容档位和连接策略以 `lobby-defaults.json` 为准
- 当前公开包默认指向腾讯云大厅 `118.25.157.52:8787`，并固定使用 `test_relaxed + relay-first`
- 当前公开包和腾讯云公开服默认不做游戏版本字符串与 MOD 版本字符串的严格校验
- `mod_manifest.json` 是当前发布包内的 MOD 版本单一真源
- 本地 `config.json` 现在写到用户数据目录，不再回写到游戏 app 包内
- Windows / Steam 下不需要额外加 `--force-steam=off`；当前发布版会在多人续局 `载入` / `放弃` 时做内部兼容保护
- 当前发布包固定包含 Windows 双击入口 `install-sts2-lan-connect-windows.bat`

## 安装前

- 先关闭《Slay the Spire 2》
- 保证所有联机玩家使用同一版 MOD
- 如果发布包里已经包含 `lobby-defaults.json`，普通玩家不需要手动填写大厅地址
- 如果你正在使用 `Clash`、`Surge`、系统全局代理或 `TUN`，请让大厅服务器 IP 走 `DIRECT`
- 当前腾讯云大厅 / relay 的直连目标是 `118.25.157.52`

## 一键安装 / 卸载

macOS：

- 双击 `install-sts2-lan-connect-macos.command`
- 如果已安装 MOD，则自动卸载
- 如果未安装 MOD，则自动安装
- 安装 / 卸载后会自动刷新 `SlayTheSpire2.app` 的 macOS 签名，避免手动拷文件后游戏无法启动

Windows：

- 双击 `install-sts2-lan-connect-windows.bat`
- 如果已安装 MOD，则自动卸载
- 如果未安装 MOD，则自动安装

## 命令行强制安装

macOS：

```bash
./install-sts2-lan-connect-macos.sh --install --package-dir .
```

Windows：

```powershell
powershell -ExecutionPolicy Bypass -File .\install-sts2-lan-connect-windows.ps1 -Action Install -PackageDir .
```

## 命令行强制卸载

macOS：

```bash
./install-sts2-lan-connect-macos.sh --uninstall --package-dir .
```

Windows：

```powershell
powershell -ExecutionPolicy Bypass -File .\install-sts2-lan-connect-windows.ps1 -Action Uninstall -PackageDir .
```

## 切换行为

- 未安装时：
  - 复制 `sts2_lan_connect.dll`
  - 复制 `sts2_lan_connect.pck`
  - 复制 `mod_manifest.json`
  - 如果存在 `lobby-defaults.json`，一并复制到游戏的 `mods/sts2_lan_connect/`
  - 自动刷新 `SlayTheSpire2.app` 的签名，保证安装后仍可启动
  - 执行一次从 vanilla 到 modded 的单向存档同步
- 已安装时：
  - 删除游戏的 `mods/sts2_lan_connect/`
  - 自动刷新 `SlayTheSpire2.app` 的签名，保证卸载后仍可启动

如果你只想安装 MOD、不做存档同步，请改用命令行：

macOS：

```bash
./install-sts2-lan-connect-macos.sh --install --package-dir . --no-save-sync
```

Windows：

```powershell
powershell -ExecutionPolicy Bypass -File .\install-sts2-lan-connect-windows.ps1 -Action Install -PackageDir . -NoSaveSync
```

## 大厅与续局使用要点

- 房间列表支持关键词搜索、分页和状态标签。单击会选中房间，双击会直接加入。
- 房间列表支持鼠标滚轮滚动、移动端按住滑动滚动，右侧滚动条会保留为保底入口。
- 进入多人续局存档后，房主对应的房间会自动重新出现在大厅里。
- 队友加入续局房间时，如果有多个空闲角色槽位，需要先选择要接管的角色。
- 加入时间较长时，界面会弹出加载中的进度提示。
- 大厅会显示真实版本号、`relay` 状态和右上角服务健康图标。
- 当前公开包默认使用 `relay-first`，会先尝试腾讯云 relay，再回退到公网 / 局域网候选地址。
- 如果大厅能正常刷新、但加入总是卡到超时，优先检查代理/TUN 是否接管了大厅服务器的 UDP 流量。
- 如果你在 Windows / Steam 下从多人菜单继续载入旧多人续局，不需要额外启动参数；优先确认所有玩家都升级到当前发布包。
- 如果提示 `MOD 不一致`，当前版本会优先直接告诉你“你缺少哪些 MOD”和“房主缺少哪些 MOD”。

# STS2 游戏大厅部署指南

这份文档对应当前公开仓库里的两部分：

- 服务端：`lobby-service/`
- 客户端：`sts2-lan-connect/`

说明：

- 官方公共服务器母面板为私有服务，不再包含在公开仓库中
- 公开仓库里的子服务仍然可以接入官方母面板
- 当前客户端默认大厅固定为阿里云：`http://47.111.146.69:8787`

目标结果：

1. 在 Linux 机器上部署并启动 `lobby-service`
2. 让这台子服务在需要时自动向官方母面板申请公开展示
3. 生成带默认大厅绑定的客户端发布包
4. 在公开仓库中同步源码和发布产物

## 一、服务端部署

### 方式 A：直接从仓库部署

```bash
sudo ./scripts/install-lobby-service-linux.sh --install-dir /opt/sts2-lobby
```

脚本会自动：

- 复制源码到 `/opt/sts2-lobby/lobby-service`
- 首次安装生成 `.env`
- 执行 `npm ci`
- 执行 `npm run build`
- 生成启动脚本
- 在 root + systemd 环境下自动安装并启动 `sts2-lobby.service`

默认需要放行：

- `8787/TCP`
- `39000-39149/UDP`

部署完成后检查：

```bash
curl http://127.0.0.1:8787/health
curl http://127.0.0.1:8787/probe
```

### 方式 B：先打包再发到服务器

```bash
./scripts/package-lobby-service.sh
```

产物：

- `lobby-service/release/sts2_lobby_service/`
- `lobby-service/release/sts2_lobby_service.zip`

上传并解压后，在服务器执行：

```bash
sudo ./install-lobby-service-linux.sh --install-dir /opt/sts2-lobby
```

### 方式 C：先清理旧版本再重装

```bash
sudo systemctl stop sts2-lobby || true
sudo rm -rf /opt/sts2-lobby/lobby-service /opt/sts2-lobby/start-lobby-service.sh
sudo find /opt/sts2-lobby -maxdepth 1 -type f \( -name 'sts2_lobby_service*.zip' -o -name '*.tgz' \) -delete
sudo ./install-lobby-service-linux.sh --install-dir /opt/sts2-lobby
```

## 二、官方公开列表说明

官方公共服务器母面板不在公开仓库里，但公开仓库内的子服务默认已经准备好接入它。

当前默认配置：

- 官方母面板：`http://47.111.146.69:18787`
- 官方默认大厅：`http://47.111.146.69:8787`

如果你使用仓库默认配置或安装脚本默认配置：

- `SERVER_REGISTRY_BASE_URL` 会默认写成 `http://47.111.146.69:18787`
- 当你在 `/server-admin` 里打开“公开列表申请”后，子服务会自动：
  - 创建申请
  - claim 审核结果
  - 按固定周期发送心跳

也就是说，第 1 个问题的结论是：现在已经会自动发送申请，不需要额外改逻辑。

如果你不想接入官方公开列表，可以清空：

```text
SERVER_REGISTRY_BASE_URL=
```

或者直接不要打开 `/server-admin` 里的“公开列表申请”。

### 线上故障记录

这次线上迁移时，实际遇到过一个和 Docker 网络模型相关的问题：

- 当 `lobby-service` 在小规格阿里云 ECS 上通过 Docker bridge 发布大段 UDP relay 端口时
- 可能同时出现 `8787` 空响应、`18787` 超时
- 更严重时，`22` 端口只剩 TCP 连接，但 SSH banner 不返回

最终结论：

- 问题不在子服务的业务逻辑
- 问题点在“Docker bridge + 大段 UDP 端口映射”这层
- 官方线上已经改为让 `lobby-service` 走宿主机网络，避免再触发这一类故障

如果你将来在自己的私有环境中也部署“子服务 + 私有母面板”的同机栈，建议优先避开这类大段 UDP bridge 发布方式。

## 三、客户端打包

### 1. 使用仓库默认大厅

当前仓库内的 [`lobby-defaults.json`](../sts2-lan-connect/lobby-defaults.json) 已经固定指向：

- `baseUrl`: `http://47.111.146.69:8787`
- `registryBaseUrl`: `http://47.111.146.69:18787`
- `wsUrl`: `ws://47.111.146.69:8787/control`

所以如果你不额外设置环境变量，打出来的客户端默认大厅就是阿里云这台。

### 2. 生成客户端包

```bash
./scripts/package-sts2-lan-connect.sh
```

产物：

- `sts2-lan-connect/release/sts2_lan_connect/`
- `sts2-lan-connect/release/sts2_lan_connect-release.zip`

### 3. 如需临时覆盖默认大厅

```bash
export STS2_LOBBY_DEFAULT_BASE_URL="http://<your-host-or-domain>:8787"
export STS2_LOBBY_DEFAULT_WS_URL="ws://<your-host-or-domain>:8787/control"
export STS2_LOBBY_DEFAULT_REGISTRY_BASE_URL="http://<your-registry-host-or-domain>:18787"

./scripts/package-sts2-lan-connect.sh
```

如果不显式设置 `STS2_LOBBY_DEFAULT_WS_URL`，打包脚本会根据 `STS2_LOBBY_DEFAULT_BASE_URL` 自动推导。

## 四、客户端安装 / 卸载

### macOS 玩家

- 双击 `install-sts2-lan-connect-macos.command`
- 或命令行执行：

```bash
./install-sts2-lan-connect-macos.sh --install --package-dir .
```

### Windows 玩家

- 双击 `install-sts2-lan-connect-windows.bat`
- 或 PowerShell 执行：

```powershell
powershell -ExecutionPolicy Bypass -File .\install-sts2-lan-connect-windows.ps1 -Action Install -PackageDir .
```

## 五、公开仓库同步

如果你本地已经 clone 了公开仓库 `STS-Game-Lobby`：

```bash
./scripts/sync-release-repo.sh --repo-dir ~/Desktop/STS-Game-Lobby
```

同步结果：

- 源码目录会同步到公开仓库根目录
- 发布产物会集中同步到公开仓库 `releases/`
- 私有母面板源码、脚本和 release 产物不会再同步到公开仓库

## 六、游戏内验证

建议至少验证：

1. 大厅刷新是否正常
2. 顶部公告轮播是否正常
3. 搜索、分页、筛选是否正常
4. 建房和加入是否正常
5. `复制本地调试报告` 是否可用
6. 子服 `/server-admin` 是否可登录并维护大厅公告
7. 如果打开了“公开列表申请”，检查同步状态是否进入 `pending_review`、`approved` 或 `heartbeat_ok`

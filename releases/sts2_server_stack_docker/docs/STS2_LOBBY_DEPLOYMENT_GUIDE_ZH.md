# STS2 游戏大厅双端部署指南

这份文档对应当前仓库的两端：

- 服务端：`lobby-service/`
- 母面板：`server-registry/`
- 客户端：`sts2-lan-connect/`

目标结果：

1. 在 Linux 机器上部署并启动 `lobby-service`
2. 在 Linux 机器上部署并启动 `server-registry`
3. 生成带默认大厅绑定与中心服务器地址的客户端发布包
4. 房主和玩家通过一键安装 / 卸载脚本完成客户端管理
5. 在公开仓库中同步源码和发布产物

当前推荐部署路线已经调整为：

1. 优先使用 Docker Compose 部署 `lobby-service + server-registry + PostgreSQL`
2. `install-*.sh` 的 systemd 安装脚本保留为兼容旧环境的兜底方案

如果你现在是在维护公网机器，建议先看：

- [Docker 部署与运维指南](./STS2_SERVER_DOCKER_OPERATION_GUIDE_ZH.md)

当前推荐的固定部署路径：

- `lobby-service`：`/opt/sts2-lobby`
- `server-registry`：`/opt/sts2-server-registry`

不再建议继续沿用 `/home/admin/...` 下的临时上传目录、解压目录和历史补丁目录；完成迁移后应一并清理，避免后续重复从旧路径重启或覆盖。

本次版本新增：

- 子服 `/server-admin` 面板中的大厅公告配置
- 客户端大厅顶部公告轮播与整页暗金 UI 改版

因此部署完成后，建议额外验证：

- `GET /announcements` 是否能返回启用中的公告
- `/server-admin` 登录后是否能保存大厅公告
- 客户端进入大厅后是否能看到新公告栏与新版布局

## 一、服务端部署

### 推荐方式：Docker 双服务栈

打包：

```bash
./scripts/package-server-stack-docker.sh
```

安装：

```bash
sudo ./scripts/install-server-stack-docker-linux.sh --install-dir /opt/sts2-server-stack-docker
```

容器部署默认会一起拉起：

- `lobby-service`
- `server-registry`
- `postgres`

当前公共 Docker 栈里，`lobby-service` 默认使用宿主机网络，而不是 Docker bridge 端口发布模式。

这是基于真实线上故障做出的调整：在阿里云小规格 ECS 上，如果 `lobby-service` 用 bridge 发布整段 relay UDP 端口，可能同时导致 `8787` 空响应、`18787` 超时，以及 SSH 只剩 TCP 连接不回 banner。

默认需要放行：

- `8787/TCP`
- `18787/TCP`
- `39000-39149/UDP`

部署完成后检查：

```bash
curl http://127.0.0.1:8787/health
curl http://127.0.0.1:18787/health
```

如果当前线上还是旧版 systemd，再参考本文后面的兼容迁移方式。

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

适合线上服务器从旧路径、旧解压包或多份残留目录迁移到统一目录时使用：

```bash
sudo systemctl stop sts2-lobby || true
sudo rm -rf /opt/sts2-lobby/lobby-service /opt/sts2-lobby/start-lobby-service.sh
sudo find /opt/sts2-lobby -maxdepth 1 -type f \( -name 'sts2_lobby_service*.zip' -o -name '*.tgz' \) -delete
sudo ./install-lobby-service-linux.sh --install-dir /opt/sts2-lobby
```

如果你之前还在 `/home/admin/...` 或其他历史目录里保留了解压版本，确认新服务启动正常后再清理旧目录。

## 二、母面板部署

如果已经采用上一节的 Docker 双服务栈，这一节可以跳过。

### 方式 A：直接从仓库部署

```bash
sudo ./scripts/install-server-registry-linux.sh --install-dir /opt/sts2-server-registry
```

脚本会自动：

- 复制源码到 `/opt/sts2-server-registry/server-registry`
- 首次安装生成 `.env`
- 执行 `npm ci`
- 执行 `npm run build`
- 生成启动脚本
- 在 root + systemd 环境下自动安装并启动 `sts2-server-registry.service`

部署完成后检查：

```bash
curl http://127.0.0.1:18787/health
```

### 方式 B：先打包再发到服务器

```bash
./scripts/package-server-registry.sh
```

产物：

- `server-registry/release/sts2_server_registry/`
- `server-registry/release/sts2_server_registry.zip`

上传并解压后，在服务器执行：

```bash
sudo ./install-server-registry-linux.sh --install-dir /opt/sts2-server-registry
```

## 三、客户端打包

### 1. 生成带默认大厅绑定的客户端包

```bash
export STS2_LOBBY_DEFAULT_BASE_URL="http://<your-host-or-domain>:8787"
export STS2_LOBBY_DEFAULT_WS_URL="ws://<your-host-or-domain>:8787/control"
export STS2_LOBBY_DEFAULT_REGISTRY_BASE_URL="http://<your-registry-host-or-domain>:18787"

./scripts/package-sts2-lan-connect.sh
```

如果不显式设置 `STS2_LOBBY_DEFAULT_WS_URL`，打包脚本会根据 `STS2_LOBBY_DEFAULT_BASE_URL` 自动推导。

产物：

- `sts2-lan-connect/release/sts2_lan_connect/`
- `sts2-lan-connect/release/sts2_lan_connect-release.zip`

### 2. 只刷新发布目录，不重新编译

```bash
./scripts/package-sts2-lan-connect.sh --skip-build
```

适用于只改了文档、安装脚本或发布目录内容的场景。

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
- 旧的根目录 release-only 布局会被清理掉

同步完成后，在公开仓库里执行常规的：

```bash
git add -A
git commit -m "Open source STS2 LAN Connect 0.2.1"
git push
```

## 六、游戏内验证

建议至少验证：

1. 大厅刷新是否正常
2. 顶部公告轮播是否正常，桌面端点状页码和左到右进度条是否正常，空公告时是否显示默认提示
3. 搜索、分页、筛选是否正常
4. 建房和加入是否正常
5. `复制本地调试报告` 是否可用
6. 子服 `/server-admin` 是否可登录并维护大厅公告

如果当前是 Docker 部署，还建议额外验证：

7. `docker compose ps` 中 3 个容器都是 `healthy` / `running`
8. `./scripts/maintain-server-stack-docker.sh --install-dir /opt/sts2-server-stack-docker logs lobby-service --tail 200` 能看到正常启动日志

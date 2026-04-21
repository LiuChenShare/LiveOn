# v1.0.1 背包管理增强 — 测试报告

**测试日期**：2026-04-13
**测试类型**：代码审查（Code Review / 静态测试）
**测试范围**：需求设计文档第 5 节 11 条验收标准 + 技术方案全部实现

---

## 1. 需求覆盖检查

逐条对照需求设计文档第 5 节验收标准：

| # | 验收标准 | 覆盖状态 | 说明 |
|---|---------|---------|------|
| 1 | 种子类物品（有 `ToEntityCode`）在背包中显示「使用」按钮，其他物品不显示 | **通过** | `renderItems` 第 233 行 `if (item.canPlant)` 控制按钮显示；后端 `GetItemSummary` 第 416 行 `CanPlant = !string.IsNullOrEmpty(first.ToEntityCode)` |
| 2 | 点击「使用」后弹出模态框，展示所有空闲地块 | **通过** | `useItem()` 调用 `GET /Game/GetFreeBlocks`，渲染 `free-block-grid` 网格 |
| 3 | 无空闲地块时显示"没有空闲的地块了"提示 | **通过** | `useItem()` 第 252-255 行，blocks 为空时设置提示文本并弹出模态框 |
| 4 | 选择空闲地块后种植成功，背包中种子数量减 1，区块显示种植结果 | **通过** | `plantOnBlock()` 调用 `POST /Game/ExecuteScript`，成功后刷新 blocks/items/logs |
| 5 | 所有物品在背包中显示「丢弃」按钮 | **通过** | `renderItems` 第 236 行，每个物品行都渲染「丢弃」按钮 |
| 6 | 丢弃弹窗支持输入数量，默认值为 1，最大值为当前持有数量 | **通过** | `openDiscardDialog()` 设置 `val(1)` 和 `attr('max', holdCount)` |
| 7 | 丢弃数量校验：不能超过持有量，不能为 0 或负数 | **通过** | 前端 `btnConfirmDiscard` 事件中校验 `count <= 0` 和 `count > maxCount`；后端 `DiscardItem()` 校验 `count <= 0` 和 `count > available` |
| 8 | 丢弃后物品从背包消失（标记 IsDeleted），API 返回的物品列表中不再包含 | **通过** | `DiscardItem()` 调用 `item.Deleted()` 设置 `IsDeleted = true`；`GetItemSummary()` 的 `Where(x => !x.IsDeleted)` 排除已删除项 |
| 9 | 背包列表按种子优先、材料次之、其他最后排序，同类型数量降序 | **通过** | `GetItemSummary()` 第 420-421 行 `OrderBy` 种子=0、材料=1、其他=9，`ThenByDescending(x => x.Count)` |
| 10 | 已删除的物品不影响排序和统计 | **通过** | `GetItemSummary()` 第 405 行 `.Where(x => !x.IsDeleted)` 在分组前过滤 |
| 11 | 所有操作反馈使用 Toast 提示，不使用 alert/confirm | **通过** | 丢弃成功/失败、种植成功/失败均使用 `showToast()`；模态框使用 Bootstrap Modal |

---

## 2. 逻辑检查

### 2.1 RemoveItems bug 修复

**文件**：`LiveOn/Game/MainGame.cs` 第 235 行

**修复状态**：**已修复**

原 bug 为 `if (items.Count == item.Value) return false;`（逻辑反转），现已改为 `if (items.Count != item.Value) return false;`。当找到的数量不等于期望数量时返回失败，逻辑正确。

### 2.2 GetItemSummary 排序逻辑

**文件**：`LiveOn/Game/MainGame.cs` 第 402-423 行

**检查结果**：**通过**

- 按编码分组（`GroupBy(x => x.Code)`），同编码物品类型必然相同，`First()` 取值安全
- 排序：Seed=0 > Material=1 > Equipment=2 > Consumable=3 > Other=9，逻辑正确
- 同类型按数量降序 `ThenByDescending(x => x.Count)`
- 已删除物品在 `Where` 阶段过滤

### 2.3 DiscardItem 参数校验和边界处理

**文件**：`LiveOn/Game/MainGame.cs` 第 250-279 行 + `GameController.cs` 第 122-130 行

**检查结果**：**通过（含 P2 建议）**

- Controller 层校验 `string.IsNullOrEmpty(code) \|\| count <= 0`，返回"参数无效"
- Service 层同样校验，形成双层防御
- 查询持有数量，`available == 0` 和 `count > available` 分别处理
- 按创建时间升序取最旧的物品标记删除，符合 FIFO 原则
- `SaveGame(force: true)` 立即持久化

**注意**：前端输入框 `type="number"` 浏览器会阻止输入非数字，但 `min` 和 `max` 属性不阻止表单提交，前端 JS 校验做了兜底，安全。

### 2.4 LoadGame 中 ItemType 恢复逻辑

**文件**：`LiveOn/Game/MainGame.cs` 第 680 行

**检查结果**：**通过**

```csharp
item.ItemType = (Items.ItemType)(dbItem.ItemType >= 0 && dbItem.ItemType <= 9 ? dbItem.ItemType : 9);
```

- 区间校验 `>= 0 && <= 9` 防止非法值
- 默认回退到 `9`（Other），与数据库 DEFAULT 9 一致
- 仅加载 `!dbItem.IsDeleted` 的物品，符合预期

### 2.5 数据库迁移脚本

**文件**：`LiveOn/Game/DB/SQLiteDBScript.cs` 第 62-64 行

**检查结果**：**通过**

```csharp
{ 2, @"ALTER TABLE Item ADD COLUMN ItemType integer DEFAULT 9;"}
```

- SQLite 的 `ALTER TABLE ADD COLUMN` 对已有行自动填充 DEFAULT 9
- 旧数据归为 Other 类型，不影响现有功能
- 列名 `ItemType` 与 DbItem 属性一致
- 版本号 2 递增，不与版本 1 冲突

---

## 3. 数据流一致性

### 3.1 后端 VO 字段 vs 前端 renderItems 使用

| 后端 ItemSummaryVO 字段 | JSON 序列化键 | 前端使用位置 | 一致性 |
|---|---|---|---|
| `Code` | `code` | `item.code` (onclick) | **通过** |
| `Name` | `name` | `item.name` (显示、onclick) | **通过** |
| `Count` | `count` | `item.count` (显示、max属性) | **通过** |
| `ItemType` | `itemType` | `item.itemType` (类型标签) | **通过** |
| `CanPlant` | `canPlant` | `item.canPlant` (使用按钮条件) | **通过** |
| `ToEntityCode` | `toEntityCode` | `item.toEntityCode` (onclick传参) | **通过** |

> 注：C# 属性名 `ItemType` 默认序列化为 `"itemType"`（ASP.NET Core JSON 默认 camelCase），前端使用 `item.itemType` 匹配。

### 3.2 API 路径和参数 vs 前端 AJAX 调用

| 操作 | API 路径 | 前端调用 | 一致性 |
|---|---|---|---|
| 获取物品列表 | `GET /Game/GetItems` | `$.getJSON('/Game/GetItems', ...)` | **通过** |
| 丢弃物品 | `POST /Game/DiscardItem` 参数: code, count | `$.post('/Game/DiscardItem', { code: code, count: count })` | **通过** |
| 获取空闲区块 | `GET /Game/GetFreeBlocks` | `$.getJSON('/Game/GetFreeBlocks', ...)` | **通过** |
| 种植（执行脚本） | `POST /Game/ExecuteScript` 参数: blockId, interactionId | `$.post('/Game/ExecuteScript', { blockId: blockId, interactionId: interactionId })` | **通过** |

### 3.3 模态框 HTML ID vs JS 引用

| 模态框 | HTML ID | JS 引用 | 一致性 |
|---|---|---|---|
| 丢弃确认 | `#discardModal` | `new bootstrap.Modal('#discardModal')` / `getInstance('#discardModal')` | **通过** |
| 种植选择 | `#plantBlockModal` | `new bootstrap.Modal('#plantBlockModal')` / `getInstance('#plantBlockModal')` | **通过** |
| 丢弃标题 | `#discardModalTitle` | `$('#discardModalTitle').text(...)` | **通过** |
| 丢弃消息 | `#discardModalMsg` | `$('#discardModalMsg').text(...)` | **通过** |
| 丢弃数量输入 | `#discardCountInput` | `$('#discardCountInput').val()` / `.attr('max')` | **通过** |
| 丢弃持有信息 | `#discardHoldInfo` | `$('#discardHoldInfo').text(...)` | **通过** |
| 确认丢弃按钮 | `#btnConfirmDiscard` | `$('#btnConfirmDiscard').on('click', ...)` / `.data('code')` | **通过** |
| 种植标题 | `#plantModalTitle` | `$('#plantModalTitle').text(...)` | **通过** |
| 种植内容 | `#plantModalBody` | `$('#plantModalBody').empty()` / `.html(...)` | **通过** |

---

## 4. 边界情况

### 4.1 丢弃数量为 0 或负数

- **前端**：`count <= 0` 时 `showToast('丢弃数量必须大于 0', 'error')` 并 return，不会发送请求
- **后端 Controller**：`count <= 0` 返回 `{ success: false, message: "参数无效" }`
- **后端 Service**：`count <= 0` 返回 `(false, "参数无效")`
- **结论**：**通过**，三层防御完整

### 4.2 丢弃数量超过持有量

- **前端**：`count > maxCount`（即 `count > holdCount`）时显示 Toast
- **后端 Service**：`count > available` 返回 `(false, "丢弃数量超过持有量（当前持有 N 个）")`
- **结论**：**通过**

### 4.3 没有空闲区块时点击使用

- **前端**：`blocks.length === 0` 时显示"没有空闲的地块了"提示，仍弹出模态框（符合需求——有提示但无操作按钮）
- **结论**：**通过**

### 4.4 背包为空时的渲染

- **前端**：`renderItems` 第 205-208 行，`items.length === 0` 时显示"背包空空如也"，`itemCount` 设为 "0 件"
- **结论**：**通过**

### 4.5 物品名称包含特殊字符（XSS）

- **渲染物品名称**：第 227 行使用 `escapeLogHtml(item.name)` 转义 HTML，**安全**
- **onclick 中的物品名称**：第 234、236 行 `escapeLogHtml(item.name)` 用于 onclick 字符串拼接。但此处存在风险——`escapeLogHtml` 只转义 HTML 实体（`<`, `>`, `&`, `"`, `'`），**不会转义反引号和反斜杠**。如果物品名包含单引号 `'`，`escapeLogHtml` 会将其转为 `&#39;`，在 HTML 属性上下文中是安全的。**通过**
- **showToast 中的消息**：第 437 行 `showToast` 直接将 `message` 拼接为 HTML 内容 `$('<div class="game-toast ' + type + '">' + message + '</div>')`。当 `message` 来自后端返回的 `res.message`（如"已丢弃 2 个 树枝"），如果物品名称包含 HTML 标签，将导致 **XSS 注入**。

**详细分析**：`showToast` 使用的 jQuery `$()` 创建元素时，如果 message 包含 `<script>` 标签，jQuery 1.x/2.x 会执行脚本。虽然当前 ASP.NET Core 的 `Json()` 序列化默认不会在字符串值中注入脚本（因为物品名称来自数据库硬编码模板），但这是一个潜在的安全隐患。

---

## 5. 代码规范

### 5.1 中文 XML 注释

| 文件 | 检查项 | 结果 |
|---|---|---|
| `ItemType.cs` | 枚举和值均有 `///` 注释和 `[Description]` | **通过** |
| `Item.cs` | 新增属性 `ItemType` 有注释；`Create`/`Init` 方法有注释 | **通过** |
| `DbModels.cs` | `DbItem.ItemType` 有注释 | **通过** |
| `VOModels.cs` | 三个新增字段均有注释 | **通过** |
| `MainGame.cs` | `DiscardItem`/`GetFreeBlocks` 有注释；`GetItemSummary` 注释已更新 | **通过** |
| `GameController.cs` | `DiscardItem`/`GetFreeBlocks` 端点有注释 | **通过** |
| `site.js` | 新增函数均有 `/** */` 注释 | **通过** |
| `site.css` | 各样式区块有注释分隔 | **通过** |

### 5.2 CSS 变量使用

- 新增样式全部使用 `:root` 中定义的 CSS 变量（`--bg-tertiary`、`--accent-green`、`--accent-red`、`--accent-amber`、`--border-color`、`--text-primary`、`--text-secondary`、`--text-dim`、`--block-idle`、`--block-hover`），无硬编码颜色值（`#fff` 除外，用于白色文字）
- **结论**：**通过**

### 5.3 escapeLogHtml 使用情况

| 场景 | 是否使用转义 | 状态 |
|---|---|---|
| 物品名称显示（`renderItems` 第 227 行） | 使用 `escapeLogHtml` | **通过** |
| 物品名称在 onclick 中（第 234、236 行） | 使用 `escapeLogHtml` | **通过** |
| 区块卡片上的实体名称（`renderBlocks` 第 183 行） | **未使用**，直接 `b.entityName` | **P2 问题** |
| 区块详情中的实体名称/描述（第 324-325 行） | **未使用** | **P2 问题** |
| 日志内容（第 417 行） | 使用 `escapeLogHtml` | **通过** |
| Toast 消息（第 437 行） | **未使用** | **P2 问题** |
| 丢弃模态框消息（第 295 行） | **未使用**，但 name 来自已转义的 onclick 参数 | **安全**（间接受保护） |

---

## 6. 缺陷清单

| # | 严重程度 | 文件 | 问题描述 | 修复建议 |
|---|---------|------|---------|---------|
| 1 | **P2** | `wwwroot/js/site.js` 第 437 行 | `showToast` 函数直接将 `message` 参数作为 HTML 拼接（`$('<div>...' + message + '</div>')`），未做 HTML 转义。当后端返回的消息中包含用户可控内容（如物品名含 `<script>` 标签）时存在 XSS 风险。 | 在 `showToast` 内部对 `message` 调用 `escapeLogHtml()` 进行转义后再拼接。或在调用处转义。 |
| 2 | **P2** | `wwwroot/js/site.js` 第 183 行 | `renderBlocks` 中 `b.entityName` 直接拼入 HTML，未调用 `escapeLogHtml()`。虽然当前实体名称来自硬编码模板，但如果未来支持自定义名称将存在 XSS 风险。 | 对 `b.entityName` 调用 `escapeLogHtml()` 转义。 |
| 3 | **P2** | `wwwroot/js/site.js` 第 324-325 行 | `openBlockDetail` 中实体名称 `e.name` 和描述 `e.description` 直接拼入 HTML，未调用 `escapeLogHtml()`。 | 对 `e.name` 和 `e.description` 调用 `escapeLogHtml()` 转义。 |
| 4 | **P3** | `LiveOn/Game/MainGame.cs` 第 420 行 | `GetItemSummary` 排序使用字符串硬编码优先级映射（`x.ItemType == "Seed" ? 0 : ...`），与 `ItemType` 枚举值重复定义。如果未来新增类型或调整枚举值，需要同步修改两处。 | 可考虑直接用 `Enum.Parse<ItemType>(x.ItemType)` 转回枚举再比较，或抽取为辅助方法。当前方案可接受，仅作为优化建议。 |
| 5 | **P3** | `wwwroot/js/site.js` 第 234 行 | `renderItems` 中 onclick 属性使用字符串拼接构建（`onclick="useItem('" + ... + "')"`），而非事件委托。如果物品编码或实体编码中包含特殊字符（虽然当前均为数字字符串），可能导致 JS 语法错误。 | 建议改用事件委托（如 `data-*` 属性 + 事件代理），避免在 HTML 属性中拼接 JS 代码。 |

---

## 7. 测试结论

### 结论：**通过**

本次 v1.0.1 背包管理增强功能的需求覆盖完整，所有 11 条验收标准均已通过代码验证。核心逻辑正确，数据流前后端一致，API 路径匹配，模态框 ID 引用正确。

### P0/P1 缺陷

无。未发现阻断性（P0）或严重功能异常（P1）缺陷。

### P2 缺陷（建议在后续版本修复）

1. `showToast` 函数的 XSS 风险（缺陷 #1）—— 建议优先修复，因为 Toast 消息中可包含物品名称等动态内容
2. `renderBlocks` 和 `openBlockDetail` 中实体名称/描述未转义（缺陷 #2、#3）—— 当前数据源为硬编码模板，风险较低，但应提前防御

### P3 建议（可延后处理）

1. 排序逻辑中枚举优先级的重复定义（缺陷 #4）
2. onclick 字符串拼接改为事件委托（缺陷 #5）

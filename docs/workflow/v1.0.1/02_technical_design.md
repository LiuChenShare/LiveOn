# v1.0.1 背包管理增强 — 技术方案

## 1. 涉及文件清单

| 文件路径 | 操作类型 | 改动说明 |
|----------|---------|---------|
| `LiveOn/Game/Items/ItemType.cs` | **新建** | 新增 ItemType 枚举，定义 Seed/Material/Equipment/Consumable/Other 五种物品类型 |
| `LiveOn/Game/Items/Item.cs` | **修改** | 新增 `ItemType` 属性，`Create()` / `Init()` 方法从模板复制 ItemType |
| `LiveOn/Core/VariableUtility.cs` | **修改** | 物品模板字典 ItemModel 中为每种物品配置 ItemType |
| `LiveOn/Game/DB/DBModels/DbModels.cs` | **修改** | DbItem 新增 `ItemType` 字段（int 类型） |
| `LiveOn/Game/DTO/VOModels.cs` | **修改** | ItemSummaryVO 新增 `ItemType`、`CanPlant`、`ToEntityCode` 字段 |
| `LiveOn/Game/DB/SQLiteDBScript.cs` | **修改** | 新增版本 2 迁移脚本：ALTER TABLE Item ADD COLUMN ItemType |
| `LiveOn/Game/DB/DBResponse.cs` | **修改** | SaveItems 的 INSERT/UPDATE SQL 增加 ItemType 列；LoadGame 恢复 ItemType |
| `LiveOn/Game/MainGame.cs` | **修改** | 修复 RemoveItems bug；GetItemSummary 增加字段和排序；LoadGame 恢复 ItemType；新增 DiscardItem 方法；新增 GetFreeBlocks 方法 |
| `LiveOn/Controllers/GameController.cs` | **修改** | 新增 DiscardItem / GetFreeBlocks 端点；修改 GetItems 返回增加新字段 |
| `LiveOn/wwwroot/game.html` | **修改** | 新增丢弃确认模态框 (#discardModal) 和种植区块选择模态框 (#plantBlockModal) |
| `LiveOn/wwwroot/js/site.js` | **修改** | 修改 renderItems 增加类型标签和按钮；新增 useItem / plantOnBlock / openDiscardDialog 函数及事件绑定 |
| `LiveOn/wwwroot/css/site.css` | **修改** | 新增物品类型标签、操作按钮、空闲区块网格等样式 |

## 2. 数据模型变更

### 2.1 新增 ItemType 枚举

新建文件 `LiveOn/Game/Items/ItemType.cs`：

```csharp
using System.ComponentModel;

namespace LiveOn.Game.Items
{
    /// <summary>
    /// 物品类型枚举，决定物品在背包中的排序优先级和 UI 展示
    /// </summary>
    public enum ItemType
    {
        /// <summary>种子类 — 有 ToEntityCode，可种植，排序优先级最高</summary>
        [Description("种子")]
        Seed = 0,

        /// <summary>材料类 — 基础资源材料</summary>
        [Description("材料")]
        Material = 1,

        /// <summary>装备类 — 可穿戴的装备</summary>
        [Description("装备")]
        Equipment = 2,

        /// <summary>消耗品类 — 一次性使用的道具</summary>
        [Description("消耗品")]
        Consumable = 3,

        /// <summary>其他类 — 默认类型，排序优先级最低</summary>
        [Description("其他")]
        Other = 9
    }
}
```

**枚举值选择说明**：枚举值直接映射排序优先级。Seed=0 优先级最高，Other=9 最低。中间留出间隔（2、3）便于后续插入新类型。Equipment 和 Consumable 虽然当前无对应物品，但预留以支持后续版本扩展。

### 2.2 Item 模型变更

文件 `LiveOn/Game/Items/Item.cs`：

新增属性：
```csharp
/// <summary>
/// 物品类型，影响背包排序和 UI 展示
/// </summary>
public ItemType ItemType { get; set; } = ItemType.Other;
```

修改 `Create()` 方法，从模板复制 ItemType：
```csharp
public static Item Create(string code)
{
    var template = Core.VariableUtility.ItemModel.GetValueOrDefault(code);
    if (template == null) return null;

    return new Item
    {
        Code = template.Code,
        Name = template.Name,
        ToEntityCode = template.ToEntityCode,
        ItemType = template.ItemType,  // 新增：从模板复制类型
        Id = Guid.NewGuid().ToString(),
        CreateTime = MainGame.Instance.GameDate
    };
}
```

修改 `Init()` 方法，增加 ItemType 复制：
```csharp
public bool Init(string code)
{
    var item = Create(code);
    if (item == null) return false;

    Name = item.Name;
    Code = item.Code;
    ToEntityCode = item.ToEntityCode;
    ItemType = item.ItemType;  // 新增
    Id = item.Id;
    CreateTime = item.CreateTime;
    return true;
}
```

### 2.3 DbItem 模型变更

文件 `LiveOn/Game/DB/DBModels/DbModels.cs`，DbItem 类新增字段：

```csharp
/// <summary>物品类型（0=Seed, 1=Material, 2=Equipment, 3=Consumable, 9=Other）</summary>
public int ItemType { get; set; } = 9;
```

> 注意：数据库存储为 int，应用层通过 `(ItemType)dbItem.ItemType` 转换。

### 2.4 ItemSummaryVO 变更

文件 `LiveOn/Game/DTO/VOModels.cs`，ItemSummaryVO 新增字段：

```csharp
/// <summary>物品类型（字符串，如 "Seed"、"Material"）</summary>
public string ItemType { get; set; }

/// <summary>是否可种植（种子类物品为 true）</summary>
public bool CanPlant { get; set; }

/// <summary>种植后生成的实体编码（仅种子类有值）</summary>
public string ToEntityCode { get; set; }
```

### 2.5 数据库迁移

文件 `LiveOn/Game/DB/SQLiteDBScript.cs`，新增版本 2 脚本：

```csharp
{ 2, @"
    ALTER TABLE Item ADD COLUMN ItemType integer DEFAULT 9;
"},
```

**迁移机制**：应用启动时 `DBUpdateHelper.DBUpdate()` 检查 DBVersion 表，若当前版本 < 2 则执行此脚本。SQLite 的 ALTER TABLE ADD COLUMN 对已有行自动填充 DEFAULT 9，旧数据的物品类型均归为 Other。

## 3. API 接口设计

### 3.1 新增 POST /Game/DiscardItem

**用途**：丢弃指定数量的物品。

**请求参数**：
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| code | string | 是 | 物品编码 |
| count | int | 是 | 丢弃数量 |

**返回格式**：
```json
{ "success": true, "message": "已丢弃 2 个 树枝" }
{ "success": false, "message": "丢弃数量超过持有量" }
```

**后端逻辑**（GameController）：
1. 参数校验：code 非空、count > 0
2. 调用 `MainGame.Instance.DiscardItem(code, count)`
3. 成功后调用 `SaveGame(force: true)` 立即持久化
4. 返回 `{ success, message }`

### 3.2 新增 GET /Game/GetFreeBlocks

**用途**：获取所有空闲区块列表（无实体且未被占用的地块）。

**请求参数**：无

**返回格式**：复用 BlockOverviewVO 数组，仅返回空闲区块：
```json
[
    { "id": 5, "stata": 0, "hasEntity": false, "entityName": null, "entityType": null, "properties": {} },
    { "id": 6, "stata": 0, "hasEntity": false, "entityName": null, "entityType": null, "properties": {} }
]
```

**后端逻辑**：
1. 从 `Grain.Instance.Blocks` 过滤出 `Entity == null` 的区块
2. 映射为 BlockOverviewVO 列表返回

### 3.3 修改 GET /Game/GetItems

**变更内容**：返回的 ItemSummaryVO 增加三个字段。

**返回示例**：
```json
[
    { "code": "3", "name": "杂树种子", "count": 3, "itemType": "Seed", "canPlant": true, "toEntityCode": "1" },
    { "code": "1", "name": "树枝", "count": 5, "itemType": "Material", "canPlant": false, "toEntityCode": null },
    { "code": "2", "name": "木材", "count": 3, "itemType": "Material", "canPlant": false, "toEntityCode": null }
]
```

**变更说明**：后端 `GetItemSummary()` 方法增加排序逻辑（按 ItemType 升序、同类型按数量降序），并在 VO 中填充 ItemType / CanPlant / ToEntityCode。

### 3.4 已有接口复用

**种植操作**：复用 `POST /Game/ExecuteScript`，参数为：
- `blockId`：目标区块 ID
- `interactionId`：格式为 `plant:{ToEntityCode}`（如 `plant:1`）

此接口已在 `MainGame.ExecuteBlockScript` 中实现种植逻辑（包含种子消耗），无需修改。

## 4. 后端实现细节

### 4.1 MainGame 修改

文件 `LiveOn/Game/MainGame.cs`。

#### 4.1.1 修复 RemoveItems bug（第 235 行）

**当前代码**（第 235 行）：
```csharp
if (items.Count == item.Value)
    return false;
```

**问题**：`==` 应为 `!=`。当找到的数量不等于期望数量时才应返回失败，当前逻辑恰好相反。

**修复**：
```csharp
if (items.Count != item.Value)
    return false;
```

#### 4.1.2 修改 GetItemSummary（第 346-352 行）

**当前代码**：
```csharp
public List<ItemSummaryVO> GetItemSummary()
{
    return Grain.Instance.Items.Where(x => !x.IsDeleted)
        .GroupBy(x => new { x.Code, x.Name })
        .Select(g => new ItemSummaryVO { Code = g.Key.Code, Name = g.Key.Name, Count = g.Count() })
        .ToList();
}
```

**修改为**：
```csharp
/// <summary>
/// 获取物品汇总信息，按编码分组统计数量，按类型和数量排序
/// </summary>
public List<ItemSummaryVO> GetItemSummary()
{
    return Grain.Instance.Items
        .Where(x => !x.IsDeleted)
        .GroupBy(x => x.Code)
        .Select(g =>
        {
            var first = g.First();
            return new ItemSummaryVO
            {
                Code = first.Code,
                Name = first.Name,
                Count = g.Count(),
                ItemType = first.ItemType.ToString(),
                CanPlant = !string.IsNullOrEmpty(first.ToEntityCode),
                ToEntityCode = first.ToEntityCode
            };
        })
        .OrderBy(x => x.ItemType == "Seed" ? 0 : x.ItemType == "Material" ? 1 : x.ItemType == "Equipment" ? 2 : x.ItemType == "Consumable" ? 3 : 9)
        .ThenByDescending(x => x.Count)
        .ToList();
}
```

> 排序使用字符串硬编码优先级值而非直接比较枚举值，避免前端传入 itemType 字符串与 int 比较的问题。按编码分组（而非编码+名称），取 First() 的 ItemType，同编码物品类型一定相同。

#### 4.1.3 修改 LoadGame（第 597-611 行）

在物品加载循环中增加 ItemType 恢复：

```csharp
foreach (var dbItem in dbItems)
{
    if (!dbItem.IsDeleted)
    {
        var item = new Item();
        item.Id = dbItem.Id;
        item.Name = dbItem.Name;
        item.Code = dbItem.Code;
        item.CreateTime = dbItem.CreateTime;
        item.ToEntityCode = dbItem.ToEntityCode;
        item.ItemType = (Items.ItemType)(dbItem.ItemType >= 0 && dbItem.ItemType <= 9 ? dbItem.ItemType : 9); // 新增
        grain.Items.Add(item);
    }
}
```

#### 4.1.4 新增 DiscardItem 方法

```csharp
/// <summary>
/// 丢弃指定编码的物品
/// </summary>
/// <param name="code">物品编码</param>
/// <param name="count">丢弃数量</param>
/// <returns>是否成功及提示消息</returns>
public (bool success, string message) DiscardItem(string code, int count)
{
    if (string.IsNullOrEmpty(code) || count <= 0)
        return (false, "参数无效");

    // 查询当前持有数量
    var available = Grain.Instance.Items.Count(x => x.Code == code && !x.IsDeleted);
    if (available == 0)
        return (false, "背包中没有该物品");
    if (count > available)
        return (false, $"丢弃数量超过持有量（当前持有 {available} 个）");

    // 按创建时间升序取最旧的 count 个标记删除
    var toRemove = Grain.Instance.Items
        .Where(x => x.Code == code && !x.IsDeleted)
        .OrderBy(x => x.CreateTime)
        .Take(count)
        .ToList();

    foreach (var item in toRemove)
        item.Deleted();

    SaveGame(force: true);

    // 获取物品名称
    var template = Core.VariableUtility.ItemModel.GetValueOrDefault(code);
    var name = template?.Name ?? code;
    AddLog("丢弃", $"已丢弃 {count} 个 {name}");
    return (true, $"已丢弃 {count} 个 {name}");
}
```

#### 4.1.5 新增 GetFreeBlocks 方法

```csharp
/// <summary>
/// 获取所有空闲区块（无实体）
/// </summary>
/// <returns>空闲区块概览列表</returns>
public List<BlockOverviewVO> GetFreeBlocks()
{
    return Grain.Instance.Blocks
        .Where(b => b.Entity == null)
        .Select(b => new BlockOverviewVO
        {
            Id = b.Id,
            Stata = b.Stata,
            HasEntity = false,
            EntityName = null,
            EntityType = null
        })
        .ToList();
}
```

### 4.2 VariableUtility.ItemModel 修改

文件 `LiveOn/Core/VariableUtility.cs`，为每种物品模板配置 ItemType：

```csharp
public static Dictionary<string, Item> ItemModel = new Dictionary<string, Item>
{
    { "1", new Item() { Code = "1", Name = "树枝", ItemType = ItemType.Material } },
    { "2", new Item() { Code = "2", Name = "木材", ItemType = ItemType.Material } },
    { "3", new Item() { Code = "3", Name = "杂树种子", ToEntityCode = "1", ItemType = ItemType.Seed } }
};
```

需要在文件顶部增加 `using LiveOn.Game.Items;`（当前未引用）。

### 4.3 DBResponse.SaveItems 修改

文件 `LiveOn/Game/DB/DBResponse.cs`，SaveItems 方法（第 221-260 行）。

INSERT SQL 增加 ItemType 列：
```csharp
conn.Execute(@"INSERT INTO Item(Id, Name, Code, CreateTime, ToEntityCode, IsDeleted, ItemType)
              VALUES (@Id, @Name, @Code, @CreateTime, @ToEntityCode, @IsDeleted, @ItemType);",
    new
    {
        Id = item.Id, Name = item.Name, Code = item.Code,
        CreateTime = item.CreateTime, ToEntityCode = item.ToEntityCode,
        IsDeleted = item.IsDeleted ? 1 : 0,
        ItemType = (int)item.ItemType  // 新增
    });
```

UPDATE SQL 增加 ItemType 列：
```csharp
conn.Execute(@"UPDATE Item SET Name=@Name, Code=@Code, CreateTime=@CreateTime,
              ToEntityCode=@ToEntityCode, IsDeleted=@IsDeleted, ItemType=@ItemType WHERE Id=@Id;",
    new
    {
        Id = item.Id, Name = item.Name, Code = item.Code,
        CreateTime = item.CreateTime, ToEntityCode = item.ToEntityCode,
        IsDeleted = item.IsDeleted ? 1 : 0,
        ItemType = (int)item.ItemType  // 新增
    });
```

### 4.4 GameController 修改

文件 `LiveOn/Controllers/GameController.cs`。

#### 4.4.1 修改 GetItems

无需修改 Controller 层代码，因为 `GetItemSummary()` 返回的 VO 已包含新字段，`Json()` 序列化会自动输出。

#### 4.4.2 新增 DiscardItem 端点

```csharp
/// <summary>
/// 丢弃物品
/// </summary>
[HttpPost]
public IActionResult DiscardItem(string code, int count)
{
    if (string.IsNullOrEmpty(code) || count <= 0)
        return Json(new { success = false, message = "参数无效" });

    var (success, message) = MainGame.Instance.DiscardItem(code, count);
    return Json(new { success, message });
}
```

#### 4.4.3 新增 GetFreeBlocks 端点

```csharp
/// <summary>
/// 获取空闲区块列表（用于种植选择）
/// </summary>
[HttpGet]
public IActionResult GetFreeBlocks()
{
    return Json(MainGame.Instance.GetFreeBlocks());
}
```

## 5. 前端实现细节

### 5.1 game.html 新增模态框

在 `<!-- 区块详情模态框 -->` 之后新增两个模态框：

#### 5.1.1 丢弃确认模态框 (#discardModal)

```html
<!-- 丢弃确认模态框 -->
<div class="modal fade" id="discardModal" tabindex="-1">
    <div class="modal-dialog modal-dialog-centered modal-sm">
        <div class="modal-content game-modal">
            <div class="modal-header">
                <h5 class="modal-title" id="discardModalTitle">丢弃物品</h5>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <p id="discardModalMsg" style="font-size:0.9rem;"></p>
                <div class="mb-2">
                    <label class="form-label text-muted" style="font-size:0.8rem;">丢弃数量</label>
                    <input type="number" class="form-control form-control-sm" id="discardCountInput"
                           min="1" value="1" style="background-color:var(--bg-tertiary);border-color:var(--border-color);color:var(--text-primary);" />
                    <div class="text-muted" style="font-size:0.75rem;margin-top:4px;" id="discardHoldInfo"></div>
                </div>
            </div>
            <div class="modal-footer" style="border-top-color:var(--border-color);">
                <button type="button" class="btn btn-sm" data-bs-dismiss="modal"
                        style="background-color:var(--bg-tertiary);color:var(--text-secondary);border:1px solid var(--border-color);">取消</button>
                <button type="button" class="btn btn-sm" id="btnConfirmDiscard"
                        style="background-color:var(--accent-red);color:#fff;border:none;">确认丢弃</button>
            </div>
        </div>
    </div>
</div>
```

#### 5.1.2 种植区块选择模态框 (#plantBlockModal)

```html
<!-- 种植区块选择模态框 -->
<div class="modal fade" id="plantBlockModal" tabindex="-1">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content game-modal">
            <div class="modal-header">
                <h5 class="modal-title" id="plantModalTitle">种植</h5>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body" id="plantModalBody">
                <!-- 动态渲染空闲区块网格或提示 -->
            </div>
        </div>
    </div>
</div>
```

### 5.2 site.js 函数变更

文件 `LiveOn/wwwroot/js/site.js`。

#### 5.2.1 修改 renderItems 函数（第 171-192 行）

替换现有 `renderItems` 函数：

```javascript
/** 渲染背包物品列表，显示类型标签、名称、数量和操作按钮 */
function renderItems(items) {
    var $list = $('#itemList');
    $list.empty();

    if (!items || items.length === 0) {
        $list.html('<div class="text-center text-muted py-3" style="font-size:0.85rem;">背包空空如也</div>');
        $('#itemCount').text('0 件');
        return;
    }

    var totalCount = 0;
    for (var i = 0; i < items.length; i++) {
        var item = items[i];
        totalCount += item.count;

        // 类型标签样式映射
        var typeLabel = item.itemType || 'Other';
        var typeClass = '';
        if (typeLabel === 'Seed') typeClass = 'item-type-badge seed';
        else if (typeLabel === 'Material') typeClass = 'item-type-badge material';
        else typeClass = 'item-type-badge other';

        var html = '<div class="item-row">';
        // 左侧：类型标签 + 名称
        html += '<div class="item-info">';
        html += '<span class="' + typeClass + '">' + typeLabel + '</span>';
        html += '<span class="item-name">' + item.name + '</span>';
        html += '</div>';
        // 中间：数量
        html += '<span class="item-count">x' + item.count + '</span>';
        // 右侧：操作按钮
        html += '<div class="item-actions">';
        if (item.canPlant) {
            html += '<button class="item-btn item-btn-use" onclick="useItem(\'' + item.code + '\',\'' + item.name + '\',\'' + item.toEntityCode + '\')">使用</button>';
        }
        html += '<button class="item-btn item-btn-discard" onclick="openDiscardDialog(\'' + item.code + '\',\'' + item.name + '\',' + item.count + ')">丢弃</button>';
        html += '</div>';
        html += '</div>';
        $list.append(html);
    }
    $('#itemCount').text(totalCount + ' 件');
}
```

#### 5.2.2 新增 useItem 函数

```javascript
/** 点击「使用」按钮：获取空闲区块，打开种植选择面板 */
function useItem(code, name, toEntityCode) {
    $('#plantModalTitle').text('种植 ' + name);

    $.getJSON('/Game/GetFreeBlocks', function (blocks) {
        var $body = $('#plantModalBody');
        $body.empty();

        if (!blocks || blocks.length === 0) {
            $body.html('<div class="text-center py-3" style="color:var(--text-secondary);">没有空闲的地块了</div>');
            new bootstrap.Modal('#plantBlockModal').show();
            return;
        }

        var html = '<div class="free-block-grid">';
        for (var i = 0; i < blocks.length; i++) {
            var b = blocks[i];
            html += '<div class="free-block-card" onclick="plantOnBlock(' + b.id + ',\'' + toEntityCode + '\',\'' + name + '\')">';
            html += '<div class="free-block-id">地块 #' + b.id + '</div>';
            html += '<div class="free-block-icon">-</div>';
            html += '<div class="free-block-label">空地</div>';
            html += '</div>';
        }
        html += '</div>';
        $body.html(html);
        new bootstrap.Modal('#plantBlockModal').show();
    });
}
```

#### 5.2.3 新增 plantOnBlock 函数

```javascript
/** 选择空闲区块后执行种植 */
function plantOnBlock(blockId, toEntityCode, name) {
    var interactionId = 'plant:' + toEntityCode;
    $.post('/Game/ExecuteScript', { blockId: blockId, interactionId: interactionId }, function (res) {
        if (res.success) {
            showToast(res.message, 'success');
        } else {
            showToast(res.message, 'error');
        }
        // 关闭种植模态框
        var modal = bootstrap.Modal.getInstance('#plantBlockModal');
        if (modal) modal.hide();
        // 刷新数据
        refreshBlocks();
        refreshItems();
        refreshLogs();
    });
}
```

#### 5.2.4 新增 openDiscardDialog 函数

```javascript
/** 打开丢弃确认模态框 */
function openDiscardDialog(code, name, holdCount) {
    $('#discardModalTitle').text('丢弃物品');
    $('#discardModalMsg').text('确定要丢弃「' + name + '」吗？');
    var $input = $('#discardCountInput');
    $input.val(1).attr('max', holdCount);
    $('#discardHoldInfo').text('当前持有 ' + holdCount + ' 个');
    // 存储丢弃参数到 DOM
    $('#btnConfirmDiscard').data('code', code).data('name', name);
    new bootstrap.Modal('#discardModal').show();
}
```

#### 5.2.5 事件绑定

在 `$(document).ready` 中新增：

```javascript
// 丢弃确认按钮
$('#btnConfirmDiscard').on('click', function () {
    var code = $(this).data('code');
    var name = $(this).data('name');
    var count = parseInt($('#discardCountInput').val()) || 0;
    var maxCount = parseInt($('#discardCountInput').attr('max')) || 1;

    // 前端校验
    if (count <= 0) {
        showToast('丢弃数量必须大于 0', 'error');
        return;
    }
    if (count > maxCount) {
        showToast('丢弃数量不能超过持有量', 'error');
        return;
    }

    $.post('/Game/DiscardItem', { code: code, count: count }, function (res) {
        if (res.success) {
            showToast(res.message, 'success');
            var modal = bootstrap.Modal.getInstance('#discardModal');
            if (modal) modal.hide();
            refreshItems();
            refreshLogs();
        } else {
            showToast(res.message, 'error');
        }
    });
});
```

### 5.3 site.css 新增样式

文件 `LiveOn/wwwroot/css/site.css`，在 `/* ===== 物品列表 ===== */` 区块内追加：

```css
/* ===== 物品行增强布局 ===== */
.item-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0.4rem 0.6rem;
    background-color: var(--bg-tertiary);
    border-radius: 4px;
    font-size: 0.85rem;
    gap: 0.5rem;
}

.item-info {
    display: flex;
    align-items: center;
    gap: 0.4rem;
    flex: 1;
    min-width: 0;
}

/* ===== 物品类型标签 ===== */
.item-type-badge {
    display: inline-block;
    padding: 0.1rem 0.4rem;
    border-radius: 3px;
    font-size: 0.65rem;
    font-weight: 600;
    flex-shrink: 0;
    line-height: 1.4;
}

.item-type-badge.seed {
    background-color: rgba(95, 163, 74, 0.2);
    color: var(--accent-green);
    border: 1px solid rgba(95, 163, 74, 0.4);
}

.item-type-badge.material {
    background-color: rgba(196, 154, 60, 0.15);
    color: var(--accent-amber);
    border: 1px solid rgba(196, 154, 60, 0.3);
}

.item-type-badge.other {
    background-color: rgba(138, 154, 122, 0.15);
    color: var(--text-secondary);
    border: 1px solid rgba(138, 154, 122, 0.3);
}

/* ===== 物品操作按钮 ===== */
.item-actions {
    display: flex;
    gap: 4px;
    flex-shrink: 0;
}

.item-btn {
    padding: 0.15rem 0.45rem;
    border-radius: 3px;
    font-size: 0.7rem;
    cursor: pointer;
    border: 1px solid var(--border-color);
    background-color: transparent;
    transition: all 0.15s;
    line-height: 1.4;
}

.item-btn-use {
    color: var(--accent-green);
    border-color: rgba(95, 163, 74, 0.4);
}

.item-btn-use:hover {
    background-color: rgba(95, 163, 74, 0.2);
    border-color: var(--accent-green);
}

.item-btn-discard {
    color: var(--accent-red);
    border-color: rgba(139, 58, 58, 0.4);
}

.item-btn-discard:hover {
    background-color: rgba(139, 58, 58, 0.2);
    border-color: var(--accent-red);
}

/* ===== 空闲区块网格（种植选择面板） ===== */
.free-block-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
    gap: 8px;
}

.free-block-card {
    background-color: var(--block-idle);
    border: 1px solid var(--border-color);
    border-radius: 6px;
    padding: 0.5rem;
    cursor: pointer;
    text-align: center;
    transition: all 0.15s;
}

.free-block-card:hover {
    background-color: var(--block-hover);
    border-color: var(--accent-green);
    transform: translateY(-1px);
}

.free-block-id {
    font-size: 0.65rem;
    color: var(--text-dim);
}

.free-block-icon {
    font-size: 1.2rem;
    color: var(--text-dim);
    padding: 0.3rem 0;
}

.free-block-label {
    font-size: 0.75rem;
    color: var(--text-secondary);
}
```

## 6. 实施步骤

按依赖关系从底层到上层排序：

### 步骤 1：数据层 — 新建 ItemType 枚举
- 新建 `LiveOn/Game/Items/ItemType.cs`
- 无外部依赖，是最底层的类型定义

### 步骤 2：数据层 — 数据库迁移
- 修改 `LiveOn/Game/DB/SQLiteDBScript.cs`，新增版本 2 脚本（ALTER TABLE Item ADD COLUMN ItemType）
- 修改 `LiveOn/Game/DB/DBModels/DbModels.cs`，DbItem 增加 ItemType 字段
- 修改 `LiveOn/Game/DB/DBResponse.cs`，SaveItems 的 INSERT/UPDATE SQL 增加 ItemType 列

### 步骤 3：业务层 — Item 模型与模板
- 修改 `LiveOn/Game/Items/Item.cs`，新增 ItemType 属性，修改 Create/Init 方法
- 修改 `LiveOn/Core/VariableUtility.cs`，ItemModel 中配置 ItemType，添加 using

### 步骤 4：业务层 — MainGame
- 修改 `LiveOn/Game/MainGame.cs`：
  - 修复 RemoveItems 第 235 行 bug（`==` 改 `!=`）
  - 修改 GetItemSummary 增加字段和排序
  - 修改 LoadGame 恢复 ItemType
  - 新增 DiscardItem 方法
  - 新增 GetFreeBlocks 方法

### 步骤 5：DTO 层
- 修改 `LiveOn/Game/DTO/VOModels.cs`，ItemSummaryVO 增加 ItemType、CanPlant、ToEntityCode

### 步骤 6：API 层 — Controller
- 修改 `LiveOn/Controllers/GameController.cs`，新增 DiscardItem / GetFreeBlocks 端点

### 步骤 7：前端 — 样式
- 修改 `LiveOn/wwwroot/css/site.css`，新增物品增强布局、类型标签、操作按钮、空闲区块网格样式

### 步骤 8：前端 — 页面结构
- 修改 `LiveOn/wwwroot/game.html`，新增丢弃确认模态框和种植区块选择模态框

### 步骤 9：前端 — 交互逻辑
- 修改 `LiveOn/wwwroot/js/site.js`：
  - 修改 renderItems 增加类型标签和操作按钮
  - 新增 useItem / plantOnBlock / openDiscardDialog 函数
  - 新增 #btnConfirmDiscard 事件绑定

### 步骤 10：验证测试
- 启动应用，确认数据库迁移执行成功（DBVersion 表版本 = 2）
- 验证旧存档加载后物品 ItemType 恢复为 Other
- 测试背包排序：种子 > 材料 > 其他，同类型数量降序
- 测试种植流程：点击使用 → 弹出空闲地块 → 选择 → 种植成功
- 测试丢弃流程：点击丢弃 → 输入数量 → 确认 → 物品减少
- 测试边界情况：无空闲地块提示、丢弃数量超限、丢弃数量为 0

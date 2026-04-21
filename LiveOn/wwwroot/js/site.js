// ===== 游戏面板交互 =====
var gameState = {
    status: 'Init',
    pollTimer: null,
    blocksTimer: null,
    lastBlocksJson: ''
};

// 全局 AJAX 401 处理 — 跳转登录页
$(document).ajaxError(function (e, xhr) {
    if (xhr.status === 401) {
        window.location.href = '/login.html';
    }
});

$(document).ready(function () {
    // 初始化游戏（加载存档）
    $.get('/Game/InitGame', function () {
        refreshGameState();
        refreshBlocks();
        refreshItems();
        refreshLogs();
        startPolling();
    });

    $('#btnStartGame').on('click', startGame);
    $('#btnPause').on('click', pauseGame);
    $('#btnResume').on('click', resumeGame);
    $('#btnClearLog').on('click', function () { $('#logContainer').empty(); });

    // 背包类型筛选按钮（多选，「全部」点击后仅选中它）
    $('#itemFilterBar').on('click', '.filter-btn', function () {
        var filter = $(this).data('filter');
        if (filter === 'All') {
            $('#itemFilterBar .filter-btn').removeClass('active');
            $(this).addClass('active');
        } else {
            $('#itemFilterBar .filter-btn[data-filter="All"]').removeClass('active');
            $(this).toggleClass('active');
        }
        renderItems();
    });

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
});

// ===== 轮询 =====
function startPolling() {
    stopPolling();
    // 游戏状态：1秒轮询（游戏日期每秒变）
    gameState.pollTimer = setInterval(refreshGameState, 1000);
    // 区块：60秒轮询（实体状态变化很慢）
    gameState.blocksTimer = setInterval(refreshBlocks, 60000);
}

function stopPolling() {
    if (gameState.pollTimer) { clearInterval(gameState.pollTimer); gameState.pollTimer = null; }
    if (gameState.blocksTimer) { clearInterval(gameState.blocksTimer); gameState.blocksTimer = null; }
}

// ===== 游戏状态 =====
function refreshGameState() {
    $.getJSON('/Game/GetGameState', function (data) {
        gameState.status = data.gameState;
        updateStatusBar(data);
        // 暂停时停止轮询
        if (data.gameState === 'Paused') stopPolling();
    });
}

/** 更新状态栏显示（游戏日期、状态徽标、按钮显隐） */
function updateStatusBar(data) {
    $('#gameDate').text(data.gameDate);
    var $badge = $('#gameStateBadge');

    switch (data.gameState) {
        case 'InGame':
            $badge.text('游戏中').removeClass('paused init');
            $('#btnStartGame').addClass('d-none');
            $('#btnPause').removeClass('d-none');
            $('#btnResume').addClass('d-none');
            break;
        case 'Paused':
            $badge.text('已暂停').addClass('paused').removeClass('init');
            $('#btnStartGame').addClass('d-none');
            $('#btnPause').addClass('d-none');
            $('#btnResume').removeClass('d-none');
            break;
        default:
            $badge.text('未开始').addClass('init').removeClass('paused');
            $('#btnStartGame').removeClass('d-none');
            $('#btnPause').addClass('d-none');
            $('#btnResume').addClass('d-none');
            break;
    }
}

// ===== 游戏控制 =====
function startGame() {
    $.post('/Game/StartGame', function () {
        refreshGameState();
        refreshBlocks();
        refreshLogs();
    });
}

function pauseGame() {
    $.post('/Game/PauseGame', function () {
        refreshGameState();
        refreshLogs();
    });
}

function resumeGame() {
    $.post('/Game/ProceedGame', function () {
        startPolling();
        refreshGameState();
        refreshBlocks();
        refreshLogs();
    });
}

// ===== 区块 =====
function refreshBlocks() {
    $.getJSON('/Game/GetBlocks', function (data) {
        var json = JSON.stringify(data);
        if (json === gameState.lastBlocksJson) return; // 数据没变，跳过 DOM 重建
        gameState.lastBlocksJson = json;
        renderBlocks(data);
    });
}

/** 渲染区块网格，根据实体类型显示不同图标和信息 */
function renderBlocks(blocks) {
    var $grid = $('#blockGrid');
    $grid.empty();

    for (var i = 0; i < blocks.length; i++) {
        var b = blocks[i];
        var iconClass = 'empty';
        var iconText = '-';
        var nameText = '空地';
        var infoText = '';
        var cardClass = 'block-card';

        if (b.hasEntity) {
            cardClass += ' has-entity';
            if (b.entityType === 'TreeEntity') {
                iconClass = 'tree';
                iconText = '🌳';
                nameText = b.entityName || '树木';
                infoText = '高度: ' + (b.properties && b.properties.tree_high ? b.properties.tree_high : 0).toFixed(1) + 'm';
            } else if (b.entityType === 'SeedEntity') {
                iconClass = 'seed';
                iconText = '🌱';
                nameText = b.entityName || '种子';
                infoText = '生长中...';
            } else {
                iconClass = 'entity';
                iconText = '❓';
                nameText = b.entityName || '未知';
            }
        }

        var html = '<div class="' + cardClass + '" data-block-id="' + b.id + '" onclick="openBlockDetail(' + b.id + ')">';
        html += '<div class="block-card-id">地块 #' + b.id + '</div>';
        html += '<div class="block-card-icon ' + iconClass + '">' + iconText + '</div>';
        html += '<div class="block-card-name">' + nameText + '</div>';
        if (infoText) {
            html += '<div class="block-card-info">' + infoText + '</div>';
        }
        html += '</div>';
        $grid.append(html);
    }
    $('#blockCount').text(blocks.length + ' 块');
}

// ===== 物品 =====
/** 缓存后端返回的全部物品数据 */
var cachedItems = [];

/** 物品类型：英文→中文映射 */
var itemTypeMap = { Seed: '种子', Material: '材料', Equipment: '装备', Consumable: '消耗品', Other: '其他' };

/** 当前选中的筛选类型集合，空数组表示全选 */

function refreshItems() {
    $.getJSON('/Game/GetItems', function (data) {
        cachedItems = data || [];
        renderItems();
    });
}

/** 渲染背包物品列表，根据选中筛选类型从缓存中过滤并渲染 */
function renderItems() {
    var $list = $('#itemList');
    $list.empty();

    if (cachedItems.length === 0) {
        $list.html('<div class="text-center text-muted py-3" style="font-size:0.85rem;">背包空空如也</div>');
        $('#itemCount').text('0 件');
        return;
    }

    // 多选筛选：「全部」按钮或全不选均视为全选
    var activeTypes = [];
    var hasAll = false;
    $('#itemFilterBar .filter-btn.active').each(function () {
        var f = $(this).data('filter');
        if (f === 'All') hasAll = true;
        else activeTypes.push(f);
    });
    var items = cachedItems;
    if (!hasAll && activeTypes.length > 0) {
        items = cachedItems.filter(function (item) { return activeTypes.indexOf(item.itemType) !== -1; });
    }

    if (items.length === 0) {
        $list.html('<div class="text-center text-muted py-3" style="font-size:0.85rem;">没有该类型物品</div>');
        $('#itemCount').text('0 件');
        return;
    }

    var totalCount = 0;
    for (var i = 0; i < items.length; i++) {
        var item = items[i];
        totalCount += item.count;

        var typeLabel = item.itemType || 'Other';
        var typeClass = 'item-type-badge ' + typeLabel.toLowerCase();
        var displayName = itemTypeMap[typeLabel] || typeLabel;

        var html = '<div class="item-row">';
        html += '<div class="item-info">';
        html += '<span class="' + typeClass + '">' + displayName + '</span>';
        html += '<span class="item-name">' + escapeLogHtml(item.name) + '</span>';
        html += '</div>';
        html += '<span class="item-count">x' + item.count + '</span>';
        html += '<div class="item-actions">';
        if (item.canPlant) {
            html += '<button class="item-btn item-btn-use" onclick="useItem(\'' + item.code + '\',\'' + escapeLogHtml(item.name) + '\',\'' + item.toEntityCode + '\')">使用</button>';
        }
        html += '<button class="item-btn item-btn-discard" onclick="openDiscardDialog(\'' + item.code + '\',\'' + escapeLogHtml(item.name) + '\',' + item.count + ')">丢弃</button>';
        html += '</div>';
        html += '</div>';
        $list.append(html);
    }
    $('#itemCount').text(totalCount + ' 件');
}

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
            html += '<div class="free-block-card" onclick="plantOnBlock(' + b.id + ',\'' + toEntityCode + '\',\'' + escapeLogHtml(name) + '\')">';
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

// ===== 区块详情模态框 =====
function openBlockDetail(blockId) {
    $.getJSON('/Game/GetBlockDetail', { blockId: blockId }, function (res) {
        if (!res.success) {
            showToast(res.message, 'error');
            return;
        }

        var data = res.data;
        $('#blockModalTitle').text('地块 #' + data.id);

        var html = '';

        if (data.entity) {
            var e = data.entity;
            var typeIcon = e.type === 'TreeEntity' ? '🌳' : e.type === 'SeedEntity' ? '🌱' : '?';

            html += '<div class="block-detail-entity">';
            html += '<div class="entity-icon">' + typeIcon + '</div>';
            html += '<div class="entity-name">' + e.name + '</div>';
            html += '<div class="entity-desc">' + e.description + '</div>';
            html += '</div>';

            html += '<div class="block-detail-stats">';
            html += '<div class="stat-item"><span class="stat-label">类型 </span><span class="stat-value">' + e.type + '</span></div>';
            html += '<div class="stat-item"><span class="stat-label">存活时间 </span><span class="stat-value">' + e.lifeTime + '</span></div>';

            // 动态渲染子类特有属性
            if (e.properties) {
                var propLabels = { tree_high: '树高', growth_rate: '生长速率', growth_time: '成长时间', to_code: '目标编码' };
                for (var key in e.properties) {
                    var label = propLabels[key] || key;
                    var val = e.properties[key];
                    if (key === 'tree_high' || key === 'growth_rate') val = parseFloat(val).toFixed(2);
                    if (key === 'growth_time') val = val + ' 分钟';
                    html += '<div class="stat-item"><span class="stat-label">' + label + ' </span><span class="stat-value">' + val + '</span></div>';
                }
            }

            html += '<div class="stat-item"><span class="stat-label">阶段 </span><span class="stat-value">' + e.stage + '</span></div>';
            html += '</div>';
        } else {
            html += '<div class="text-center py-3" style="color:var(--text-secondary);">这块地是空的，可以种植</div>';
        }

        if (data.scripts && data.scripts.length > 0) {
            html += '<div class="script-actions">';
            for (var i = 0; i < data.scripts.length; i++) {
                var s = data.scripts[i];
                // 有子选项（如种植）渲染为下拉列表
                if (s.items && s.items.length > 0) {
                    html += '<div class="script-dropdown">';
                    html += '<div class="script-dropdown-toggle">' + s.name + ' <span class="dropdown-arrow">▾</span></div>';
                    html += '<div class="script-dropdown-menu">';
                    for (var j = 0; j < s.items.length; j++) {
                        var item = s.items[j];
                        html += '<button class="script-dropdown-item" onclick="executeScript(' + data.id + ',\'' + item.scriptCode + '\')">';
                        html += '<span class="dropdown-item-name">' + item.name + '</span>';
                        html += '<span class="dropdown-item-desc">' + item.description + '</span>';
                        html += '</button>';
                    }
                    html += '</div></div>';
                } else {
                    html += '<button class="script-btn" onclick="executeScript(' + data.id + ',\'' + s.scriptCode + '\')">';
                    html += '<div class="script-name">' + s.name + '</div>';
                    html += '<div class="script-desc">' + s.description + '</div>';
                    html += '</button>';
                }
            }
            html += '</div>';
        }

        $('#blockModalBody').html(html);
        // 绑定下拉菜单展开/收起
        $('#blockModalBody').find('.script-dropdown-toggle').on('click', function () {
            $(this).parent().toggleClass('open');
        });
        // 点击选项后关闭菜单
        $('#blockModalBody').find('.script-dropdown-item').on('click', function () {
            $(this).closest('.script-dropdown').removeClass('open');
        });
        new bootstrap.Modal('#blockModal').show();
    });
}

// ===== 执行操作 =====
function executeScript(blockId, interactionId) {
    $.post('/Game/ExecuteScript', { blockId: blockId, interactionId: interactionId }, function (res) {
        if (res.success) {
            showToast(res.message, 'success');
        } else {
            showToast(res.message, 'error');
        }
        refreshBlocks();
        refreshItems();
        refreshLogs();

        var modal = bootstrap.Modal.getInstance('#blockModal');
        if (modal) modal.hide();
    });
}

// ===== 日志 =====
function refreshLogs() {
    $.getJSON('/Game/GetLogs', { count: 20 }, function (data) {
        var $container = $('#logContainer');
        $container.empty();
        if (!data || data.length === 0) return;
        for (var i = 0; i < data.length; i++) {
            var l = data[i];
            var msgClass = l.type === 1 ? 'success' : l.type === 2 ? 'error' : l.type === 3 ? 'warning' : '';
            var html = '<div class="log-entry">';
            html += '<span class="log-time">[' + l.createTime + ']</span>';
            html += '<span class="log-msg ' + msgClass + '">[' + l.source + '] ' + escapeLogHtml(l.content) + '</span>';
            html += '</div>';
            $container.append(html);
        }
    });
}

/** 转义日志文本中的 HTML 特殊字符，防止 XSS */
function escapeLogHtml(str) {
    var div = document.createElement('div');
    div.appendChild(document.createTextNode(str));
    return div.innerHTML;
}

/** 数字补零，将个位数前补 '0' */
function padZero(n) { return n < 10 ? '0' + n : '' + n; }

// ===== Toast =====
function showToast(message, type) {
    type = type || 'success';
    var $toast = $('<div class="game-toast ' + type + '">' + message + '</div>');
    $('body').append($toast);
    setTimeout(function () { $toast.addClass('show'); }, 10);
    setTimeout(function () {
        $toast.removeClass('show');
        setTimeout(function () { $toast.remove(); }, 300);
    }, 2500);
}

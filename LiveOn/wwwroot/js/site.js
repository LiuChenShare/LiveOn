// ===== 游戏面板交互 =====
var gameState = {
    status: 'Init',
    pollTimer: null
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
});

// ===== 轮询 =====
function startPolling() {
    if (gameState.pollTimer) clearInterval(gameState.pollTimer);
    gameState.pollTimer = setInterval(function () {
        refreshGameState();
        refreshBlocks();
        refreshItems();
    }, 1000);
}

// ===== 游戏状态 =====
function refreshGameState() {
    $.getJSON('/Game/GetGameState', function (data) {
        gameState.status = data.gameState;
        updateStatusBar(data);
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
        refreshGameState();
        refreshLogs();
    });
}

// ===== 区块 =====
function refreshBlocks() {
    $.getJSON('/Game/GetBlocks', function (data) {
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
function refreshItems() {
    $.getJSON('/Game/GetItems', function (data) {
        renderItems(data);
    });
}

/** 渲染背包物品列表，显示物品名称和数量 */
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
        var html = '<div class="item-row">';
        html += '<span class="item-name">' + item.name + '</span>';
        html += '<span class="item-count">x' + item.count + '</span>';
        html += '</div>';
        $list.append(html);
    }
    $('#itemCount').text(totalCount + ' 件');
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
                html += '<button class="script-btn" onclick="executeScript(' + data.id + ',\'' + s.scriptCode + '\')">';
                html += '<div class="script-name">' + s.name + '</div>';
                html += '<div class="script-desc">' + s.description + '</div>';
                html += '</button>';
            }
            html += '</div>';
        }

        $('#blockModalBody').html(html);
        new bootstrap.Modal('#blockModal').show();
    });
}

// ===== 执行操作 =====
function executeScript(blockId, scriptCode) {
    $.post('/Game/ExecuteScript', { blockId: blockId, scriptCode: scriptCode }, function (res) {
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

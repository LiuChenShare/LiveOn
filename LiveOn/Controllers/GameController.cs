using LiveOn.Core;
using LiveOn.Game;
using Microsoft.AspNetCore.Mvc;

namespace LiveOn.Controllers
{
    public class GameController : Controller
    {
        /// <summary>
        /// 初始化游戏（前端加载时调用）
        /// </summary>
        [HttpGet]
        public IActionResult InitGame()
        {
            MainGame.Instance.InitGame();
            return Json(new { success = true });
        }

        /// <summary>
        /// 获取游戏状态
        /// </summary>
        [HttpGet]
        public IActionResult GetGameState()
        {
            return Json(MainGame.Instance.GetGameStateSummary());
        }

        /// <summary>
        /// 获取区块列表概览
        /// </summary>
        [HttpGet]
        public IActionResult GetBlocks()
        {
            return Json(MainGame.Instance.GetBlocksOverview());
        }

        /// <summary>
        /// 获取区块详情
        /// </summary>
        [HttpGet]
        public IActionResult GetBlockDetail(int blockId)
        {
            var detail = MainGame.Instance.GetBlockDetail(blockId);
            if (detail == null) return Json(new { success = false, message = "区块不存在" });
            return Json(new { success = true, data = detail });
        }

        /// <summary>
        /// 获取物品列表
        /// </summary>
        [HttpGet]
        public IActionResult GetItems()
        {
            return Json(MainGame.Instance.GetItemSummary());
        }

        /// <summary>
        /// 执行区块操作
        /// </summary>
        [HttpPost]
        public IActionResult ExecuteScript(int blockId, int scriptCode)
        {
            var (success, message) = MainGame.Instance.ExecuteBlockScript(blockId, scriptCode);
            return Json(new { success, message });
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        [HttpPost]
        public IActionResult PauseGame()
        {
            MainGame.Instance.PauseGame();
            return Json(new { success = true });
        }

        /// <summary>
        /// 继续游戏
        /// </summary>
        [HttpPost]
        public IActionResult ProceedGame()
        {
            MainGame.Instance.ProceedGame();
            return Json(new { success = true });
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        [HttpPost]
        public IActionResult StartGame()
        {
            MainGame.Instance.GameStart();
            return Json(new { success = true });
        }

        /// <summary>
        /// 获取最新日志（首页用）
        /// </summary>
        [HttpGet]
        public IActionResult GetLogs(int count = 20)
        {
            return Json(MainGame.Instance.GetLogs(count));
        }

        /// <summary>
        /// 获取分页日志（日志页面用）
        /// </summary>
        [HttpGet]
        public IActionResult GetLogsPaged(int page = 1, int pageSize = 50)
        {
            var (logs, totalCount, totalPages) = MainGame.Instance.GetLogsPaged(page, pageSize);
            return Json(new { logs, totalCount, totalPages, page });
        }
    }
}

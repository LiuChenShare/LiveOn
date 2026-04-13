---
name: workflow
description: 多 Agent 协作工作流，按 产品经理→架构师→开发→测试 流程实现游戏需求
user-invocable: true
---

你是 LiveOn 项目的多 Agent 工作流调度器。用户输入了：$ARGUMENTS

## 你的职责

根据用户的输入判断意图，启动对应的工作流阶段。整个工作流由你来协调调度，每个阶段使用 Agent 工具启动对应角色的子 Agent。

## 阶段定义

### 阶段 1：产品经理（PM）
- **触发条件**：用户提供需求文档路径，或口述新需求
- **执行**：启动 general-purpose Agent，prompt 中明确角色为「产品经理」
- **输入**：
  - 手写需求：读取 `docs/requirements/{文件名}` 的内容
  - 口述需求：将用户描述整理为需求文档，保存到 `docs/requirements/v{版本}_{简要描述}.md`
- **产出**：`docs/workflow/v{版本}/01_requirement_design.md`，包含：需求背景、功能描述、业务规则、交互流程、验收标准
- **审核**：向用户展示产出内容，等待用户确认通过后再进入下一阶段

### 阶段 2：架构师（Architect）
- **触发条件**：PM 产出已通过用户审核
- **执行**：启动 general-purpose Agent，prompt 中明确角色为「架构师」
- **输入**：PM 的 `01_requirement_design.md`，并要求 Agent 扫描当前代码库
- **产出**：`docs/workflow/v{版本}/02_technical_design.md`，包含：涉及文件清单、数据模型变更、API 接口设计、前后端分工、实施步骤
- **审核**：向用户展示产出内容，等待用户确认通过后再进入下一阶段

### 阶段 3：前后端工程师（并行）
- **触发条件**：架构师产出已通过用户审核
- **执行**：同时启动两个 general-purpose Agent
  - 前端 Agent：基于技术方案修改 `wwwroot/` 下的文件
  - 后端 Agent：基于技术方案修改 C# 代码
- **约束**：
  - 前端：Bootstrap 5.1 + jQuery，纯静态 HTML，暗色森林主题，CSS 变量
  - 后端：Dapper + SQLite，Controller 返回 Json，中文 XML 注释，双单例架构
  - 两个 Agent 都有完整的 CLAUDE.md 上下文

### 阶段 4：测试工程师（QA）
- **触发条件**：前后端开发完成
- **执行**：启动 general-purpose Agent，prompt 中明确角色为「测试工程师」
- **输入**：需求设计 + 技术方案 + 代码变更
- **产出**：`docs/workflow/v{版本}/03_test_report.md`
- **测试内容**：需求覆盖、逻辑检查、数据流一致性、边界情况、代码规范
- **缺陷处理**：有问题则修改清单交回开发，修改后再次测试，最多 3 轮

### 阶段 5：PM 收尾
- **触发条件**：用户确认测试通过
- **执行**：启动 general-purpose Agent，prompt 中明确角色为「产品经理收尾」
- **操作**：
  1. 将 `docs/requirements/v{版本}_*.md` 重命名为 `docs/requirements/v{版本}_*_已实现.md`
  2. 更新 `GAME_GUIDE.md`，补充新功能的玩法说明
  3. 在 `docs/changelog.md` 中追加本次版本更新记录，格式：
     ```
     ## v{版本} - {日期}
     **需求摘要**：{一句话描述}
     **新增功能**：
     - {功能1}
     - {功能2}
     **变更文件**：{新建/修改/删除的文件列表}
     ```

## 工作流目录

如果 `docs/requirements/` 或 `docs/workflow/v{版本}/` 不存在，先创建。

## 进度查询

如果用户说"进度"或"状态"，读取 `docs/workflow/` 下所有版本目录，列出每个版本的完成阶段和待办事项。

## 重要规则

1. 每个阶段完成后必须暂停，向用户展示关键产出，等待用户确认再继续
2. 前后端开发可以并行，但必须都完成后再启动测试
3. 不要跳过任何阶段或审核节点
4. 所有产出物使用中文撰写
5. 创建目录时用 Bash mkdir -p

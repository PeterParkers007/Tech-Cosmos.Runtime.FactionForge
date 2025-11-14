# FactionForge - 可视化阵营关系系统

[![Unity Version](https://img.shields.io/badge/Unity-2021.3%2B-blue.svg)](https://unity3d.com)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

**让游戏阵营管理变得简单直观的专业解决方案** - 提供完整的可视化编辑器和运行时API，支持动态阵营关系和智能AI集成

## ✨ 核心特性

### 🎯 可视化编辑
- **关系矩阵编辑器** - 专业的N×N关系矩阵，类似Excel表格操作
- **智能阵营选择** - 下拉菜单选择已有阵营，避免拼写错误
- **实时颜色编码** - 敌对(红)、友好(绿)、同盟(青)、中立(灰)
- **一键快速操作** - 点击关系单元格直接修改，无需切换界面

### 🔄 动态管理
- **运行时API** - 完整的查询和设置接口
- **动态阵营变更** - 支持游戏内实时改变阵营关系
- **自动数据同步** - 阵营增删时自动更新所有关联关系

### 🛠️ 开发者友好
- **即插即用** - 添加组件即可使用，零配置入门
- **完整文档** - 详细的API参考和使用示例
- **高性能** - 基于字典的快速关系查询

## 🚀 快速开始

### 安装

1. 通过Unity Package Manager安装：
```
https://github.com/PeterParkers007/Tech-Cosmos.Runtime.FactionForge.git
```

2. 或下载后放入项目的 `Packages` 目录

### 5分钟上手

1. **创建阵营管理器**
```csharp
// 方法1：菜单栏创建
// GameObject → Tools → FactionForge → 创建阵营管理器

// 方法2：代码创建
var manager = new GameObject("FactionManager").AddComponent<FactionManager>();
```

2. **添加阵营和关系**
- 在FactionManager Inspector中点击"添加新阵营"
- 输入阵营名称：`人类`、`兽人`、`精灵`
- 设置关系：人类↔兽人(敌对)、人类↔精灵(同盟)

3. **为角色添加阵营**
```csharp
// 为角色添加FactionMember组件
var factionMember = character.AddComponent<FactionMember>();
factionMember.SetFaction("人类");
```

4. **在游戏逻辑中使用**
```csharp
// 查询关系
var relationship = FactionManager.Instance.GetRelationship("人类", "兽人");

if (relationship == FactionRelationship.Hostile)
{
    // 触发战斗逻辑
    StartCombat();
}
```

## 📖 核心功能详解

### 阵营关系可视化窗口

打开方式：
- **菜单栏**: `Tools → FactionForge → 阵营关系可视化` 
- **快捷键**: `Alt + F`

功能特色：
- 🎨 **矩阵视图** - 专业的关系矩阵，一目了然
- ⚡ **实时交互** - 点击任意单元格快速修改关系
- 🔄 **自动刷新** - 每秒自动更新数据变化
- 📊 **统计面板** - 各类关系数量统计

### FactionMember组件智能编辑器

为角色添加`FactionMember`组件后，Inspector中会显示：

- **下拉选择器** - 从已有阵营中选择，避免拼写错误
- **关系预览** - 实时显示与其他所有阵营的关系状态
- **快速测试** - 一键输出所有关系到Console
- **颜色编码** - 直观的关系状态视觉反馈

## 🎮 集成示例

### 与AI系统集成
```csharp
public class AICharacter : MonoBehaviour
{
    private FactionMember factionMember;
    
    private void EvaluateTarget(Character target)
    {
        var targetFaction = target.GetComponent<FactionMember>();
        if (targetFaction == null) return;
        
        var relationship = factionMember.GetRelationshipWith(targetFaction);
        
        switch (relationship)
        {
            case FactionRelationship.Hostile:
                // 敌对目标 - 攻击
                AttackTarget(target);
                break;
            case FactionRelationship.Friendly:
                // 友好目标 - 治疗/辅助
                SupportAlly(target);
                break;
            case FactionRelationship.Allied:
                // 同盟目标 - 保护
                DefendAlly(target);
                break;
        }
    }
}
```

### 动态关系变化
```csharp
public class QuestSystem : MonoBehaviour
{
    public void OnQuestCompleted(string playerFaction, string targetFaction, bool improvedRelations)
    {
        var newRelationship = improvedRelations ? 
            FactionRelationship.Friendly : FactionRelationship.Hostile;
            
        FactionManager.Instance.SetRelationship(playerFaction, targetFaction, newRelationship);
        
        Debug.Log($"{playerFaction} 与 {targetFaction} 的关系变为 {newRelationship}");
    }
}
```

### 与存档系统集成
```csharp
[System.Serializable]
public class FactionSaveData
{
    public List<Faction> factions;
}

public class SaveSystem : MonoBehaviour
{
    public FactionSaveData SaveFactionData()
    {
        return new FactionSaveData
        {
            factions = FactionManager.Instance.Factions
        };
    }
    
    public void LoadFactionData(FactionSaveData data)
    {
        FactionManager.Instance.LoadFactions(data.factions);
    }
}
```

## 📚 API参考

### FactionManager
```csharp
// 单例访问
FactionManager.Instance

// 核心API
FactionRelationship GetRelationship(string factionA, string factionB)
void SetRelationship(string factionA, string factionB, FactionRelationship relationship)
void AddFaction(string factionName)
void RemoveFaction(string factionName)
List<Faction> Factions { get; }
```

### FactionMember
```csharp
// 组件属性
string FactionName { get; }
void SetFaction(string newFaction)

// 关系查询
FactionRelationship GetRelationshipWith(FactionMember other)
FactionRelationship GetRelationshipWith(string otherFaction)
bool IsHostileTo(FactionMember other)
bool IsFriendlyTo(FactionMember other)
bool IsAlliedTo(FactionMember other)
```

### FactionRelationship枚举
```csharp
public enum FactionRelationship
{
    Hostile,    // 敌对 - 红色
    Neutral,    // 中立 - 灰色
    Friendly,   // 友好 - 绿色  
    Allied      // 同盟 - 青色
}
```

## 🔧 高级用法

### 自定义关系类型
```csharp
// 扩展枚举支持新关系
public enum FactionRelationship
{
    Hostile,
    Neutral,
    Friendly,
    Allied,
    Rival,      // 竞争关系
    Subordinate // 从属关系
}
// 编辑器会自动适应新枚举值
```

### 复杂关系网络
```csharp
// 实现三方制衡
FactionManager.Instance.SetRelationship("人类", "精灵", FactionRelationship.Allied);
FactionManager.Instance.SetRelationship("精灵", "兽人", FactionRelationship.Hostile); 
FactionManager.Instance.SetRelationship("兽人", "人类", FactionRelationship.Neutral);
```

## 🗂️ 项目结构

```
FactionForge/
├── Runtime/
│   ├── FactionManager.cs          # 核心管理器
│   ├── FactionMember.cs           # 角色阵营组件
│   ├── Faction.cs                 # 阵营数据类
│   ├── FactionRelationship.cs     # 关系枚举
│   └── SerializableDictionary.cs  # 序列化字典
└── Editor/
    ├── FactionSystemEditor.cs     # Manager编辑器
    ├── FactionMemberEditor.cs     # Member智能编辑器
    └── FactionRelationshipWindow.cs # 关系可视化窗口

```

## 🤝 支持与贡献

### 贡献指南
1. Fork 本项目
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

---

**FactionForge** - 专业级的阵营关系管理解决方案，让复杂的游戏逻辑变得简单可控！ 🎮

> 从独立开发者到3A团队，都能找到适合的使用方式
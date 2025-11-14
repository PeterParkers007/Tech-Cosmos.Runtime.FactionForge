# FactionForge - 可视化阵营关系系统

[![Unity Version](https://img.shields.io/badge/Unity-2021.3%2B-blue.svg)](https://unity3d.com)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Documentation](https://img.shields.io/badge/Documentation-Online-brightgreen.svg)](https://techcosmos.github.io/FactionForge)

一个强大的Unity编辑器扩展，提供可视化的阵营关系配置工具，让游戏中的阵营系统管理变得简单直观。

## ✨ 特性

- 🎯 **可视化配置** - 直观的阵营关系矩阵编辑器
- 🔄 **动态管理** - 运行时动态添加/移除阵营
- 🎮 **即插即用** - 简单的API，快速集成到现有项目
- 📊 **数据驱动** - 基于配置的阵营关系管理
- 🛡️ **类型安全** - 完整的枚举支持和编译时检查

## 🚀 快速开始

### 安装

1. 通过Unity Package Manager安装：
```
https://github.com/TechCosmos/FactionForge.git
```

2. 或下载后放入项目的 `Packages` 目录

### 基础使用

1. **创建阵营管理器**
```csharp
// 在场景中创建空物体并添加FactionManager组件
// 或通过代码创建：
var factionManager = new GameObject("FactionManager").AddComponent<FactionManager>();
```

2. **配置阵营关系**
- 在Inspector中点击"添加新阵营"
- 输入阵营名称（如"人类"、"兽人"、"精灵"）
- 当有≥2个阵营时，自动显示关系配置矩阵
- 设置各阵营间的关系：友好、中立、敌对、同盟

3. **在代码中使用**
```csharp
// 查询两个阵营的关系
var relationship = FactionManager.Instance.GetRelationship("人类", "兽人");

switch (relationship)
{
    case FactionRelationship.Friendly:
        // 友好逻辑
        break;
    case FactionRelationship.Hostile:
        // 敌对逻辑 - 触发战斗
        break;
    case FactionRelationship.Allied:
        // 同盟逻辑 - 共享资源
        break;
    default:
        // 中立逻辑
        break;
}
```

## 📖 API 参考

### 核心类

#### FactionManager
阵营系统的核心管理器，使用单例模式提供全局访问。

**主要方法：**
```csharp
// 获取两个阵营的关系
public FactionRelationship GetRelationship(string factionA, string factionB)

// 设置两个阵营的关系
public void SetRelationship(string factionA, string factionB, FactionRelationship relationship)

// 动态添加阵营（运行时）
public void AddFaction(string factionName)

// 动态移除阵营（运行时）  
public void RemoveFaction(string factionName)
```

#### FactionRelationship 枚举
```csharp
public enum FactionRelationship
{
    Friendly,  // 友好
    Neutral,   // 中立
    Hostile,   // 敌对
    Allied     // 同盟
}
```

### 高级用法

#### 与AI系统集成
```csharp
public class AICharacter : MonoBehaviour
{
    private void EvaluateThreat(Character target)
    {
        var relationship = FactionManager.Instance.GetRelationship(
            gameObject.name, target.name);
            
        if (relationship == FactionRelationship.Hostile)
        {
            // 敌对目标，采取攻击行为
            AttackTarget(target);
        }
        else if (relationship == FactionRelationship.Allied)
        {
            // 同盟目标，提供支援
            SupportAlly(target);
        }
    }
}
```

#### 动态关系修改
```csharp
// 游戏事件触发关系变化
public void OnQuestCompleted(string factionA, string factionB)
{
    // 完成任务改善两个阵营的关系
    FactionManager.Instance.SetRelationship(
        factionA, factionB, FactionRelationship.Friendly);
        
    Debug.Log($"{factionA} 和 {factionB} 的关系改善了！");
}
```

## 🎨 编辑器功能

### 阵营列表管理
- ✅ 动态添加/删除阵营
- ✅ 阵营名称验证
- ✅ 实时关系数量显示

### 关系矩阵视图
- ✅ 自动检测阵营数量（≥2时显示）
- ✅ 直观的关系选择下拉菜单
- ✅ 实时数据同步

### 可视化反馈
- ✅ 友好的空状态提示
- ✅ 操作确认和撤销支持
- ✅ 数据持久化

## 🔧 扩展开发

### 自定义关系类型
```csharp
// 1. 扩展关系枚举
public enum FactionRelationship
{
    Friendly,
    Neutral, 
    Hostile,
    Allied,
    // 添加自定义关系
    Rival,      // 竞争
    Subordinate // 从属
}

// 2. 编辑器会自动适应新的枚举值
```

### 集成到现有系统
```csharp
// 与存档系统集成
public class SaveSystem
{
    public void SaveFactionData()
    {
        var factionData = new FactionData
        {
            factions = FactionManager.Instance.Factions
        };
        SaveToFile(factionData);
    }
    
    public void LoadFactionData(FactionData data)
    {
        FactionManager.Instance.LoadFactions(data.factions);
    }
}
```

## 📁 项目结构

```
FactionForge/
├── Runtime/
│   ├── FactionManager.cs          # 核心管理器
│   ├── Faction.cs                 # 阵营数据类
│   ├── FactionRelationship.cs     # 关系枚举
│   └── SerializableDictionary.cs  # 序列化字典
└── Editor/
    └── FactionSystemEditor.cs     # 编辑器扩展
```

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

1. Fork 本项目
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情.

---

**FactionForge** - 让游戏阵营管理变得简单！ 🎮
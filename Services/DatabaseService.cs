using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaNovel.Data;
using AvaloniaNovel.Models;
using Microsoft.EntityFrameworkCore;

namespace AvaloniaNovel.Services;

public class DatabaseService : IDatabaseService
{
    private readonly ICoverImageService _coverImageService;

    public DatabaseService(ICoverImageService coverImageService)
    {
        _coverImageService = coverImageService;
    }

    public async Task<List<Novel>> GetAllNovelsAsync()
    {
        using var db = new NovelDbContext();
        return await db.Novels
            .Include(n => n.Chapters)
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync();
    }

    public async Task<Novel?> GetNovelByIdAsync(int id)
    {
        using var db = new NovelDbContext();
        return await db.Novels
            .Include(n => n.Chapters.OrderBy(c => c.Order))
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<Novel> CreateNovelAsync(string title, string genre, string worldSetting, string? coverImageSourcePath = null)
    {
        using var db = new NovelDbContext();
        var coverImagePath = string.IsNullOrWhiteSpace(coverImageSourcePath)
            ? string.Empty
            : await _coverImageService.SaveCoverImageAsync(coverImageSourcePath);

        var novel = new Novel
        {
            Title = title,
            Genre = genre,
            WorldSetting = worldSetting,
            CoverImagePath = coverImagePath,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        db.Novels.Add(novel);
        await db.SaveChangesAsync();
        return novel;
    }

    public async Task DeleteNovelAsync(int id)
    {
        using var db = new NovelDbContext();
        var novel = await db.Novels.FindAsync(id);
        if (novel != null)
        {
            _coverImageService.DeleteCoverImage(novel.CoverImagePath);
            db.Novels.Remove(novel);
            await db.SaveChangesAsync();
        }
    }

    public async Task<Chapter> AddChapterAsync(Chapter chapter)
    {
        using var db = new NovelDbContext();
        db.Chapters.Add(chapter);
        await db.SaveChangesAsync();
        return chapter;
    }

    public async Task UpdateChapterAsync(Chapter chapter)
    {
        using var db = new NovelDbContext();
        var existing = await db.Chapters.FindAsync(chapter.Id);
        if (existing != null)
        {
            existing.Title = chapter.Title;
            existing.Summary = chapter.Summary;
            existing.Content = chapter.Content;
            existing.Status = chapter.Status;
            existing.Order = chapter.Order;
            await db.SaveChangesAsync();
        }
    }

    public async Task<Chapter?> GetChapterByIdAsync(int id)
    {
        using var db = new NovelDbContext();
        return await db.Chapters.FindAsync(id);
    }

    public async Task<AppSettings?> GetAppSettingsAsync()
    {
        using var db = new NovelDbContext();
        var settings = await db.AppSettings.FirstOrDefaultAsync();
        if (settings != null)
        {
            settings.DeepSeekApiKey = KeyEncryption.Unprotect(settings.DeepSeekApiKey);
        }
        return settings;
    }

    public async Task SaveAppSettingsAsync(string apiKey)
    {
        using var db = new NovelDbContext();
        var settings = await db.AppSettings.FirstOrDefaultAsync();
        var encryptedKey = KeyEncryption.Protect(apiKey);
        if (settings == null)
        {
            settings = new AppSettings { DeepSeekApiKey = encryptedKey };
            db.AppSettings.Add(settings);
        }
        else
        {
            settings.DeepSeekApiKey = encryptedKey;
        }
        await db.SaveChangesAsync();
    }

    public async Task UpdateNovelTimestampAsync(int novelId)
    {
        using var db = new NovelDbContext();
        var novel = await db.Novels.FindAsync(novelId);
        if (novel != null)
        {
            novel.UpdatedAt = DateTime.Now;
            await db.SaveChangesAsync();
        }
    }

    // ── PromptTemplate CRUD ─────────────────────────────────────────────

    public async Task<List<PromptTemplate>> GetAllPromptTemplatesAsync()
    {
        using var db = new NovelDbContext();
        return await db.PromptTemplates
            .OrderBy(t => t.Type)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<List<PromptTemplate>> GetPromptTemplatesByTypeAsync(PromptTemplateType type)
    {
        using var db = new NovelDbContext();
        return await db.PromptTemplates
            .Where(t => t.Type == type)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<PromptTemplate?> GetPromptTemplateByIdAsync(int id)
    {
        using var db = new NovelDbContext();
        return await db.PromptTemplates.FindAsync(id);
    }

    /// <summary>获取指定类型的默认模板（第一个 IsBuiltIn 的模板）</summary>
    public async Task<PromptTemplate?> GetDefaultPromptTemplateAsync(PromptTemplateType type)
    {
        using var db = new NovelDbContext();
        return await db.PromptTemplates
            .FirstOrDefaultAsync(t => t.Type == type && t.IsBuiltIn);
    }

    public async Task<PromptTemplate> CreatePromptTemplateAsync(PromptTemplate template)
    {
        using var db = new NovelDbContext();
        template.CreatedAt = DateTime.Now;
        template.UpdatedAt = DateTime.Now;
        db.PromptTemplates.Add(template);
        await db.SaveChangesAsync();
        return template;
    }

    public async Task UpdatePromptTemplateAsync(PromptTemplate template)
    {
        using var db = new NovelDbContext();
        var existing = await db.PromptTemplates.FindAsync(template.Id);
        if (existing != null)
        {
            existing.Name = template.Name;
            existing.Type = template.Type;
            existing.Content = template.Content;
            existing.UpdatedAt = DateTime.Now;
            await db.SaveChangesAsync();
        }
    }

    public async Task DeletePromptTemplateAsync(int id)
    {
        using var db = new NovelDbContext();
        var template = await db.PromptTemplates.FindAsync(id);
        if (template != null && !template.IsBuiltIn)
        {
            db.PromptTemplates.Remove(template);
            await db.SaveChangesAsync();
        }
    }

    /// <summary>确保内置默认模板存在，支持增量插入新模板</summary>
    public async Task EnsureDefaultTemplatesAsync()
    {
        using var db = new NovelDbContext();
        var existingNames = (await db.PromptTemplates
            .Where(t => t.IsBuiltIn)
            .Select(t => t.Name)
            .ToListAsync())
            .ToHashSet();

        var now = DateTime.Now;
        var defaults = GetBuiltInTemplates(now);
        var newTemplates = defaults.Where(d => !existingNames.Contains(d.Name)).ToList();

        if (newTemplates.Count == 0)
            return;

        db.PromptTemplates.AddRange(newTemplates);
        await db.SaveChangesAsync();
    }

    // ── ChapterVersion 版本管理 ──────────────────────────────────────────

    // 获取章节所有版本（按时间倒序）
    public async Task<List<ChapterVersion>> GetVersionsAsync(int chapterId)
    {
        await using var db = new NovelDbContext();
        return await db.ChapterVersions
            .Where(v => v.ChapterId == chapterId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }

    // 保存新版本
    public async Task<ChapterVersion> AddVersionAsync(ChapterVersion version)
    {
        await using var db = new NovelDbContext();
        version.CreatedAt = DateTime.UtcNow;
        db.ChapterVersions.Add(version);
        await db.SaveChangesAsync();
        return version;
    }

    // 获取章节最新版本
    public async Task<ChapterVersion?> GetLatestVersionAsync(int chapterId)
    {
        await using var db = new NovelDbContext();
        return await db.ChapterVersions
            .Where(v => v.ChapterId == chapterId)
            .OrderByDescending(v => v.CreatedAt)
            .FirstOrDefaultAsync();
    }

    // 清理旧自动保存版本（每个章节保留最近 20 个 auto-save）
    public async Task DeleteOldVersionsAsync(int chapterId)
    {
        await using var db = new NovelDbContext();
        var oldVersions = await db.ChapterVersions
            .Where(v => v.ChapterId == chapterId && v.Trigger == "auto-save")
            .OrderByDescending(v => v.CreatedAt)
            .Skip(20)
            .ToListAsync();

        if (oldVersions.Count > 0)
        {
            db.ChapterVersions.RemoveRange(oldVersions);
            await db.SaveChangesAsync();
        }
    }

    public static List<PromptTemplate> GetBuiltInTemplates(DateTime now) => new()
    {
        // ── 系统人设模板 ─────────────────────────────────────────────────
        new()
        {
            Name = "通用网文作家",
            Type = PromptTemplateType.System,
            IsBuiltIn = true,
            Content = @"你是一个经验丰富的网络小说作家，精通各种网文套路和风格。
你有10年以上的网文创作经验，写过都市、玄幻、悬疑、科幻等多种类型的小说。
你的写作风格：
- 情节紧凑，不拖沓
- 人物刻画鲜明，对话自然
- 善于设置悬念和爽点
- 章节结尾留有钩子，吸引读者继续阅读",
            CreatedAt = now, UpdatedAt = now
        },
        new()
        {
            Name = "玄幻爽文风格",
            Type = PromptTemplateType.System,
            IsBuiltIn = true,
            Content = @"你是一个玄幻爽文作家，深谙读者爽点，写过《斗破苍穹》《完美世界》式的爆款。
你的写作风格：
- 节奏快，升级打脸不拖沓
- 金手指设定合理但强大，有清晰的成长体系
- 爽点密集，每章至少一个小高潮
- 对话简练有力，主角话少但狠
- 善于用配角衬托主角的强大，打脸要痛快
- 章节结尾必须有钩子，吊足胃口
- 战斗描写气势磅礴，招式名字霸气",
            CreatedAt = now, UpdatedAt = now
        },
        new()
        {
            Name = "仙侠修真风格",
            Type = PromptTemplateType.System,
            IsBuiltIn = true,
            Content = @"你是一个仙侠修真小说作家，深受《凡人修仙传》《遮天》影响。
你的写作风格：
- 修仙体系严谨，境界划分清晰合理
- 淡化等级碾压，注重智谋与机缘
- 场景描写空灵缥缈，有仙气
- 对话古风韵味，但不晦涩
- 主角性格沉稳内敛，不张扬但有底线
- 善于写机缘造化和因果循环
- 章节结尾留有悬念或感悟",
            CreatedAt = now, UpdatedAt = now
        },
        new()
        {
            Name = "都市言情风格",
            Type = PromptTemplateType.System,
            IsBuiltIn = true,
            Content = @"你是一个都市言情小说作家，擅长写甜宠、虐恋和破镜重圆。
你的写作风格：
- 对话甜而不腻，有化学反应
- 感情线推进细腻，不突兀
- 善于写暧昧期的拉扯感
- 配角生动有趣，不抢主角风头
- 日常互动自然有爱，不刻意撒糖
- 误会不强行制造，冲突有逻辑
- 适当加入职场/商战元素增加厚度
- 每章结尾让人想磕下一章",
            CreatedAt = now, UpdatedAt = now
        },
        new()
        {
            Name = "悬疑推理风格",
            Type = PromptTemplateType.System,
            IsBuiltIn = true,
            Content = @"你是一个擅长悬疑推理小说的作家，深受东野圭吾和紫金陈的影响。
你的写作风格：
- 善于铺设伏笔，草蛇灰线
- 人物心理描写细腻入微
- 逻辑严密，推理过程丝丝入扣
- 每章结尾留有悬念，让读者欲罢不能
- 善用反转，但反转必须合乎逻辑
- 线索埋设均匀，不突兀也不太明显
- 氛围营造到位，紧张感层层递进",
            CreatedAt = now, UpdatedAt = now
        },
        new()
        {
            Name = "科幻末世风格",
            Type = PromptTemplateType.System,
            IsBuiltIn = true,
            Content = @"你是一个科幻末世小说作家，深受《三体》《全球高武》影响。
你的写作风格：
- 世界观设定严谨自洽，科技树合理
- 末世氛围营造到位，压迫感十足
- 生存智慧描写真实，不靠运气靠能力
- 人性在极端环境下的挣扎刻画深刻
- 主角冷静理性，决策有逻辑
- 战斗场面紧张刺激，节奏把控精准
- 伏笔长远，世界观逐步展开
- 章节结尾悬念感强",
            CreatedAt = now, UpdatedAt = now
        },
        new()
        {
            Name = "历史穿越风格",
            Type = PromptTemplateType.System,
            IsBuiltIn = true,
            Content = @"你是一个历史穿越小说作家，擅长写种田、争霸和权谋。
你的写作风格：
- 历史知识扎实，但不过度考据影响节奏
- 穿越者的现代思维与古代规则的碰撞写得有趣
- 权谋博弈逻辑严密，不是简单的开挂碾压
- 人物对话符合古风但不过于文绉绉
- 善于写从微末崛起的成就感
- 军事、经济、政治描写有深度
- 节奏张弛有度，不急于称王称霸
- 配角智商在线，不是无脑工具人",
            CreatedAt = now, UpdatedAt = now
        },
        new()
        {
            Name = "无限流风格",
            Type = PromptTemplateType.System,
            IsBuiltIn = true,
            Content = @"你是一个无限流小说作家，深受《无限恐怖》《轮回乐园》影响。
你的写作风格：
- 副本设计精巧，规则清晰有趣
- 主角冷静果断，善于分析规则找破局点
- 团队互动真实，不是无脑跟随
- 悬念把控精准，反转合理
- 生死关头描写紧张刺激
- 通关逻辑严密，不用主角光环糊弄
- 副本之间有伏笔串联
- 每章结尾都有新的悬念或发现",
            CreatedAt = now, UpdatedAt = now
        },

        // ── 大纲生成模板 ─────────────────────────────────────────────────
        new()
        {
            Name = "通用大纲生成",
            Type = PromptTemplateType.Outline,
            IsBuiltIn = true,
            Content = @"## 任务
根据以下设定，生成网络小说大纲。

## 输入
- 题材：{{genre}}
- 世界观：{{worldSetting}}

## 要求
1. 生成 10-15 章的章节列表
2. 每章需要有 50-100 字的简要描述
3. 确保整体故事有起承转合，高潮迭起
4. 章节标题要吸引人，有网感

## 输出格式
JSON 格式，字段：title（章节标题），summary（章节概要）",
            CreatedAt = now, UpdatedAt = now
        },
        new()
        {
            Name = "爽文节奏大纲",
            Type = PromptTemplateType.Outline,
            IsBuiltIn = true,
            Content = @"## 任务
根据以下设定，生成爽文节奏的大纲，注重节奏感和爽点分布。

## 输入
- 题材：{{genre}}
- 世界观：{{worldSetting}}

## 要求
1. 生成 15-20 章的章节列表
2. 前3章必须快速建立主角人设和核心矛盾
3. 每隔2-3章设置一个小爽点（打脸、升级、获得等）
4. 每5章左右一个大高潮或大反转
5. 章节概要需标注该章的爽点类型（如：[升级][打脸][获宝]）
6. 每章 50-80 字简要描述

## 输出格式
JSON 格式，字段：title（章节标题），summary（章节概要，含爽点标注）",
            CreatedAt = now, UpdatedAt = now
        },
        new()
        {
            Name = "悬疑大纲生成",
            Type = PromptTemplateType.Outline,
            IsBuiltIn = true,
            Content = @"## 任务
根据以下设定，生成悬疑推理小说的大纲，注重伏笔和悬念设计。

## 输入
- 题材：{{genre}}
- 世界观：{{worldSetting}}

## 要求
1. 生成 12-18 章的章节列表
2. 第1章引入核心谜团，制造悬念
3. 每2-3章揭示一条线索，同时引出新的疑问
4. 中段必须有一次重大反转
5. 章节概要需标注该章埋设的伏笔和揭示的线索
6. 所有伏笔必须在后续章节有呼应
7. 结局揭示真相时，读者回看能发现前文伏笔

## 输出格式
JSON 格式，字段：title（章节标题），summary（章节概要，含伏笔/线索标注）",
            CreatedAt = now, UpdatedAt = now
        },
        new()
        {
            Name = "言情大纲生成",
            Type = PromptTemplateType.Outline,
            IsBuiltIn = true,
            Content = @"## 任务
根据以下设定，生成言情小说的大纲，注重感情线推进和情感节奏。

## 输入
- 题材：{{genre}}
- 世界观：{{worldSetting}}

## 要求
1. 生成 12-16 章的章节列表
2. 前3章完成主角相遇和第一印象建立
3. 每章有至少一个有化学反应的互动场景
4. 感情推进节奏：初识→暧昧→试探→确认→考验→坚定
5. 中段设置一次合理的误会或外部考验
6. 章节概要需标注该章的感情推进阶段
7. 配角互动也要有趣，不抢戏但有记忆点

## 输出格式
JSON 格式，字段：title（章节标题），summary（章节概要，含感情阶段标注）",
            CreatedAt = now, UpdatedAt = now
        },

        // ── 章节写作模板 ─────────────────────────────────────────────────
        new()
        {
            Name = "通用章节写作",
            Type = PromptTemplateType.Chapter,
            IsBuiltIn = true,
            Content = @"## 任务
根据以下大纲，写出章节正文。

## 输入
- 章节标题：{{chapterTitle}}
- 章节概要：{{chapterSummary}}
- 前文剧情：{{previousSummary}}
- 题材：{{genre}}
- 世界观：{{worldSetting}}

## 要求
1. 字数：2000-5000 字
2. 情节紧凑，避免水文
3. 适当加入对话和心理描写
4. 章节结尾留下悬念，吸引读者
5. 注意起承转合，情节要完整

## 输出
直接输出章节正文，不需要额外格式。",
            CreatedAt = now, UpdatedAt = now
        },
        new()
        {
            Name = "爽文章节写作",
            Type = PromptTemplateType.Chapter,
            IsBuiltIn = true,
            Content = @"## 任务
根据以下大纲，写出爽文风格的章节正文。

## 输入
- 章节标题：{{chapterTitle}}
- 章节概要：{{chapterSummary}}
- 前文剧情：{{previousSummary}}
- 题材：{{genre}}
- 世界观：{{worldSetting}}

## 要求
1. 字数：2500-5000 字
2. 节奏要快，不拖沓，直奔主题
3. 对话简练有力，主角话少但句句到位
4. 打脸/升级场景要写得痛快，让读者有代入感
5. 战斗描写节奏快，招式名字霸气，画面感强
6. 配角反应要衬托主角，但不刻意贬低
7. 章节结尾必须有钩子：新敌人出现、新机缘暗示、重大消息
8. 避免大段心理描写，用行动和对话推动情节

## 输出
直接输出章节正文，不需要额外格式。",
            CreatedAt = now, UpdatedAt = now
        },
        new()
        {
            Name = "悬疑章节写作",
            Type = PromptTemplateType.Chapter,
            IsBuiltIn = true,
            Content = @"## 任务
根据以下大纲，写出悬疑推理风格的章节正文。

## 输入
- 章节标题：{{chapterTitle}}
- 章节概要：{{chapterSummary}}
- 前文剧情：{{previousSummary}}
- 题材：{{genre}}
- 世界观：{{worldSetting}}

## 要求
1. 字数：2000-4500 字
2. 氛围营造到位，适当使用环境描写烘托紧张感
3. 线索埋设自然，不刻意也不太隐晦
4. 角色心理描写细腻，尤其是焦虑和怀疑的情绪
5. 对话要有信息量，每句话都可能藏有线索
6. 章节结尾必须留下新的疑问或发现
7. 伏笔与前后文呼应，不自相矛盾
8. 避免上帝视角，保持悬念的神秘感

## 输出
直接输出章节正文，不需要额外格式。",
            CreatedAt = now, UpdatedAt = now
        },
        new()
        {
            Name = "言情章节写作",
            Type = PromptTemplateType.Chapter,
            IsBuiltIn = true,
            Content = @"## 任务
根据以下大纲，写出言情风格的章节正文。

## 输入
- 章节标题：{{chapterTitle}}
- 章节概要：{{chapterSummary}}
- 前文剧情：{{previousSummary}}
- 题材：{{genre}}
- 世界观：{{worldSetting}}

## 要求
1. 字数：2000-4500 字
2. 互动场景有化学反应，甜而不腻
3. 对话自然有趣，有来有往，不是独角戏
4. 暧昧拉扯感要写到位，眼神、小动作、心跳都是好素材
5. 内心独白要真实，不是矫情的自我感动
6. 日常细节有温度，用小场景传递感情
7. 误会和冲突有逻辑，不强行制造
8. 章节结尾让人想磕下一章

## 输出
直接输出章节正文，不需要额外格式。",
            CreatedAt = now, UpdatedAt = now
        }
    };
}

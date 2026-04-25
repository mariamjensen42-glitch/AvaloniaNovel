using System;

namespace AvaloniaNovel.Models;

/// <summary>
/// 提示词模板：用户可自定义系统人设、大纲生成、章节写作的 Prompt。
/// 支持占位符变量：{{genre}}、{{worldSetting}}、{{chapterTitle}}、{{chapterSummary}}、{{previousSummary}}
/// </summary>
public class PromptTemplate
{
    public int Id { get; set; }

    /// <summary>模板名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>模板类型</summary>
    public PromptTemplateType Type { get; set; }

    /// <summary>模板内容，支持 Mustache 风格占位符</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>是否为内置默认模板（不可删除）</summary>
    public bool IsBuiltIn { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }
}

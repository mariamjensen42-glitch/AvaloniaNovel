namespace AvaloniaNovel.Models;

/// <summary>
/// 提示词模板类型：系统人设、大纲生成、章节写作
/// </summary>
public enum PromptTemplateType
{
    System = 0,       // 系统人设（SystemPrompt）
    Outline = 1,      // 大纲生成
    Chapter = 2       // 章节写作
}

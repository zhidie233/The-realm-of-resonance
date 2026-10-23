using UnityEngine;

/// 在 Inspector 中将 List/Array 显示为可排序列表
public class ReorderableAttribute : PropertyAttribute
{
    public string ElementHeader { get; protected set; }
    public bool HeaderZeroIndex { get; protected set; }
    public bool ElementSingleLine { get; protected set; }

    /// 在 Inspector 中将 List/Array 显示为可排序列表
    public ReorderableAttribute()
    {
        ElementHeader = string.Empty;
        HeaderZeroIndex = false;
        ElementSingleLine = false;
    }

    /// 在 Inspector 中将 List/Array 显示为可排序列表
    /// <param name="headerString">自定义 Inspector 中的元素名称</param>
    /// <param name="isZeroIndex">为 false 时，元素编号从 1 开始</param>
    /// <param name="isSingleLine">尝试将数组元素排成一行</param>
    public ReorderableAttribute(string headerString = "", bool isZeroIndex = true, bool isSingleLine = false)
    {
        ElementHeader = headerString;
        HeaderZeroIndex = isZeroIndex;
        ElementSingleLine = isSingleLine;
    }
}

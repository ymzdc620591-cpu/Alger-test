using System.Collections.Generic;

namespace Starter.Core
{
    // 支持按索引访问和按引用移除的栈，供 UIManager 使用
    public class ExtStack<T>
    {
        readonly List<T> _list = new();

        public int Count => _list.Count;

        public void Push(T item) => _list.Add(item);

        // 弹出栈顶
        public T Pop()
        {
            if (_list.Count == 0) return default;
            var item = _list[^1];
            _list.RemoveAt(_list.Count - 1);
            return item;
        }

        // 移除栈内指定元素（任意位置）
        public void Pop(T item) => _list.Remove(item);

        // 查看栈顶（不弹出）
        public T Peek() => _list.Count > 0 ? _list[^1] : default;

        // 按深度查看（0 = 栈顶，1 = 次顶，依此类推）
        public T CheckByIndex(int index) => _list[_list.Count - 1 - index];

        public bool Contains(T item) => _list.Contains(item);
    }
}

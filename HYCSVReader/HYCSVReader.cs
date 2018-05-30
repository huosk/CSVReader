using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Frame.Data
{
    /// <summary>
    /// 数据表
    /// </summary>
    public class CSVTable
    {
        private List<CSVTitle> _titles;
        private List<CSVRow> _rows;

        public CSVTable()
        {
            _titles = new List<CSVTitle>();
            _rows = new List<CSVRow>();
        }

        internal void AddRow(CSVRow row)
        {
            _rows.Add(row);
        }

        internal void AddTitle(CSVTitle title)
        {
            _titles.Add(title);
        }

        /// <summary>
        /// 设置列数据类型
        /// </summary>
        /// <param name="titleName"></param>
        /// <param name="type"></param>
        public void SetColumnType(string titleName, DataType type)
        {
            CSVTitle title = FindTitle(titleName);
            if (title == null)
                return;

            title.dataType = type;
        }

        /// <summary>
        /// 获取列数据类型
        /// </summary>
        /// <param name="titleName"></param>
        /// <returns></returns>
        public DataType GetColumnType(string titleName)
        {
            CSVTitle title = FindTitle(titleName);
            if (title == null)
                throw (new Exception(string.Format("Can not find column with title:{0}", titleName)));

            return title.dataType;
        }

        /// <summary>
        /// 获取行数
        /// </summary>
        /// <returns></returns>
        public int GetRowCount()
        {
            return _rows.Count;
        }

        /// <summary>
        /// 获取列数
        /// </summary>
        /// <returns></returns>
        public int GetColunmCount()
        {
            return _titles.Count;
        }

        /// <summary>
        /// 根据标题名称，获取标题列索引
        /// </summary>
        /// <param name="titleName"></param>
        /// <returns></returns>
        private int FindTitleIndex(string titleName)
        {
            for (int i = 0; i < _titles.Count; i++)
            {
                if (_titles[i] == null)
                    continue;

                if (_titles[i].IsMatchTitle(titleName))
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// 查找指定名称列标题
        /// </summary>
        /// <param name="titleName"></param>
        /// <returns></returns>
        public CSVTitle FindTitle(string titleName)
        {
            int titleIndex = FindTitleIndex(titleName);
            if (titleIndex < 0)
                return null;

            return FindTitle(titleIndex);
        }

        /// <summary>
        /// 查找指定列索引的标题
        /// </summary>
        /// <param name="titleIndex"></param>
        /// <returns></returns>
        public CSVTitle FindTitle(int titleIndex)
        {
            if (titleIndex < 0 || titleIndex >= _titles.Count)
                return null;

            return _titles[titleIndex];
        }

        /// <summary>
        /// 查找指定名称的列标题，并返回列索引
        /// </summary>
        /// <param name="titleName"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public int FindTitle(string titleName, out CSVTitle title)
        {
            for (int i = 0; i < _titles.Count; i++)
            {
                if (_titles[i] == null)
                    continue;

                if (_titles[i].IsMatchTitle(titleName))
                {
                    title = _titles[i];
                    return i;
                }
            }

            title = null;
            return -1;
        }


        /// <summary>
        /// 获取指定行号的数据行
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        public CSVRow FindRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _rows.Count)
                return null;
            return _rows[rowIndex];
        }

        /// <summary>
        /// 查找所有满足条件的数据行
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public List<CSVRow> FindRows(Predicate<CSVRow> predicate)
        {
            return _rows.FindAll(predicate);
        }

        /// <summary>
        /// 查找第一个满足条件的数据行
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public CSVRow FindFirstRow(Predicate<CSVRow> predicate)
        {
            return _rows.Find(predicate);
        }

        /// <summary>
        /// 查找最后一个满足条件的数据行
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public CSVRow FindLastRow(Predicate<CSVRow> predicate)
        {
            return _rows.FindLast(predicate);
        }

        /// <summary>
        /// 根据指定主键，查找满足条件的数据行
        /// </summary>
        /// <param name="keyTitle"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public List<CSVRow> FindRowsByPK(string keyTitle, Predicate<CSVEntry> match)
        {
            return FindRows((CSVRow row) =>
            {
                var keyEntry = row.GetEntry(keyTitle);
                return keyEntry != null && match(keyEntry);
            });
        }
    }

    /// <summary>
    /// 数据行
    /// </summary>
    public class CSVRow
    {
        //行元素
        private List<CSVEntry> _rowElements;

        //所在表格
        private CSVTable _container;

        public CSVRow() : this(null, null) { }
        public CSVRow(CSVTable table, IEnumerable<CSVEntry> entries)
        {
            _rowElements = new List<CSVEntry>(entries);
            _container = table;
        }

        /// <summary>
        /// 获取指定索引的单元
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public CSVEntry GetEntry(int index)
        {
            if (index < 0 || index >= _rowElements.Count)
                return null;

            return _rowElements[index];
        }

        /// <summary>
        /// 获取单元个数
        /// </summary>
        /// <returns></returns>
        public int GetElementCount()
        {
            return _rowElements.Count;
        }

        /// <summary>
        /// 获取指定标题的单元
        /// </summary>
        /// <param name="titleName"></param>
        /// <returns></returns>
        public CSVEntry GetEntry(string titleName)
        {
            if (string.IsNullOrEmpty(titleName))
                return null;

            CSVTitle title_ = null;
            int titleIndex_ = _container.FindTitle(titleName, out title_);
            if (title_ == null)
                return null;

            return GetEntry(titleIndex_);
        }
    }

    /// <summary>
    /// 数据单元
    /// </summary>
    public class CSVEntry
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct NumberHolder
        {
            [FieldOffset(0)]
            public int intValue;

            [FieldOffset(0)]
            public float floatValue;

            [FieldOffset(0)]
            public bool boolValue;
        }

        //单元文本信息
        private string _content;

        //行号
        private int _rowIndex;

        //列号
        private int _columnIndex;

        //所在表格
        private CSVTable _container;

        //缓存数字类型值
        private NumberHolder? _numberHolder;

        //缓存标题
        private CSVTitle _title;

        public string content { get { return _content; } }
        public int rowIndex { get { return _rowIndex; } }
        public int columnIndex { get { return _columnIndex; } }
        public CSVTable container { get { return _container; } }

        public CSVEntry(string content, int rowIndex, int colIndex, CSVTable table)
        {
            _content = content;
            _rowIndex = rowIndex;
            _columnIndex = colIndex;
            _container = table;
        }

        private CSVTitle FindTitleCache()
        {
            if (_title == null || _title._columnIndex != _columnIndex)
                _title = _container.FindTitle(_columnIndex);

            return _title;
        }

        private void CheckAndCache()
        {
            var title_ = FindTitleCache();
            switch (title_.dataType)
            {
                case DataType.Integer:
                    _numberHolder = new NumberHolder()
                    {
                        intValue = Convert.ToInt32(content)
                    };
                    break;
                case DataType.Float:
                    _numberHolder = new NumberHolder()
                    {
                        floatValue = Convert.ToSingle(content)
                    };
                    break;
                case DataType.Bool:
                    _numberHolder = new NumberHolder()
                    {
                        boolValue = Convert.ToBoolean(content)
                    };
                    break;
                default:
                    break;
            }
        }

        public static explicit operator int (CSVEntry entry)
        {
            entry.CheckAndCache();
            if (entry._title.dataType != DataType.Integer &&
                entry._title.dataType != DataType.Unknown)
                throw (new InvalidCastException(string.Format("Can not cast entry to {0}", DataType.Integer.ToString())));

            return entry._numberHolder.Value.intValue;
        }

        public static explicit operator bool (CSVEntry entry)
        {
            entry.CheckAndCache();
            if (entry._title.dataType != DataType.Bool &&
                entry._title.dataType != DataType.Unknown)
                throw (new InvalidCastException(string.Format("Can not cast entry to {0}", DataType.Bool.ToString())));

            return entry._numberHolder.Value.boolValue;
        }

        public static explicit operator string (CSVEntry entry)
        {
            return entry.content;
        }

        public static explicit operator float (CSVEntry entry)
        {
            entry.CheckAndCache();
            if (entry._title.dataType != DataType.Float &&
                entry._title.dataType != DataType.Unknown)
                throw (new InvalidCastException(string.Format("Can not cast entry to {0}", DataType.Float.ToString())));

            return entry._numberHolder.Value.floatValue;
        }
    }

    /// <summary>
    /// 数据头信息,它包括了对应列数据的信息，包括标题名称、数据类型等信息
    /// </summary>
    public class CSVTitle : CSVEntry
    {
        //数据类型
        private DataType _dataType;

        //列标题
        private string _title;

        /// <summary>
        /// 列标题名称
        /// </summary>
        public string title
        {
            get
            {
                return _title;
            }
        }

        /// <summary>
        /// 列数据类型
        /// </summary>
        public DataType dataType
        {
            get
            {
                return _dataType;
            }
            set
            {
                _dataType = value;
            }
        }

        public CSVTitle(string content, int rowIndex, int colIndex, CSVTable table, DataType type)
            : base(content, rowIndex, colIndex, table)
        {
            _dataType = type;
            _title = GetTitle();
        }

        protected virtual string GetTitle()
        {
            return content;
        }

        public virtual bool IsMatchTitle(string titleName)
        {
            return title.Equals(titleName, StringComparison.InvariantCulture);
        }
    }

    public enum DataType
    {
        /// <summary>
        /// 未指定
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 文本
        /// </summary>
        Text,

        /// <summary>
        /// 整形数据
        /// </summary>
        Integer,

        /// <summary>
        /// 浮点型
        /// </summary>
        Float,

        /// <summary>
        /// 布尔型
        /// </summary>
        Bool,

        /// <summary>
        /// 数据类型，形式：item1 | item2 | item3
        /// </summary>
        Array,

        /// <summary>
        /// 字典类型，key:value | key:value | key:value
        /// </summary>
        Dictionary,
    }

    public static class DataTypeName
    {
        const string INTGER = "int";
        const string FLOAT = "float";
        const string BOOL = "bool";
        const string TEXT = "text";
        const string ARRAY = "array";
        const string DICTIONARY = "map";

        public static DataType ConvertToDateType(string typeInf)
        {
            if (typeInf.StartsWith(INTGER, StringComparison.InvariantCultureIgnoreCase))
                return DataType.Integer;
            else if (typeInf.StartsWith(FLOAT, StringComparison.InvariantCultureIgnoreCase))
                return DataType.Float;
            else if (typeInf.StartsWith(BOOL, StringComparison.InvariantCultureIgnoreCase))
                return DataType.Bool;
            else if (typeInf.StartsWith(TEXT, StringComparison.InvariantCultureIgnoreCase))
                return DataType.Text;
            else if (typeInf.StartsWith(ARRAY, StringComparison.InvariantCultureIgnoreCase))
                return DataType.Array;
            else if (typeInf.StartsWith(DICTIONARY, StringComparison.InvariantCultureIgnoreCase))
                return DataType.Dictionary;
            else
                return DataType.Unknown;
        }
    }

    /// <summary>
    /// CSV解析接口
    /// </summary>
    public interface ICSVResolver
    {
        void Resolve(string content);
        CSVTable GetTable();
    }

    /// <summary>
    /// 简单csv文件解析器
    /// csv格式：第一行为表头信息，从第二行开始为数据
    /// </summary>
    public class SimpleResolver : ICSVResolver
    {
        protected CSVTable _table;

        public const char COMMA = ',';

        /// <summary>
        /// 数据行起始索引值
        /// </summary>
        protected int _dataBodyBeginIndex = 1;

        public virtual void Resolve(string content)
        {
            _table = new CSVTable();

            using (StringReader sr = new StringReader(content))
            {
                //解析标题行
                ResolveTitle(sr);

                //数据行
                ResolveData(sr);
            }
        }

        /// <summary>
        /// 解析表头信息
        /// </summary>
        /// <param name="sr"></param>
        protected virtual void ResolveTitle(StringReader sr)
        {
            string titleLine_ = sr.ReadLine();
            string[] titles = titleLine_.Split(COMMA);
            for (int i = 0; i < titles.Length; i++)
            {
                CSVTitle title = new CSVTitle(titles[i], 0, i, _table, DataType.Unknown);
                _table.AddTitle(title);
            }
        }

        /// <summary>
        /// 解析表数据体
        /// </summary>
        /// <param name="sr"></param>
        protected virtual void ResolveData(StringReader sr)
        {
            string dataLine_ = sr.ReadLine();
            int rowIndex = _dataBodyBeginIndex;
            while (dataLine_ != null)
            {
                List<CSVEntry> dataEntries = new List<CSVEntry>();
                string[] dataCells = dataLine_.Split(COMMA);
                for (int i = 0; i < dataCells.Length; i++)
                {
                    dataEntries.Add(new CSVEntry(dataCells[i], rowIndex, i, _table));
                }

                _table.AddRow(new CSVRow(_table, dataEntries));

                rowIndex++;
                dataLine_ = sr.ReadLine();
            }
        }

        public CSVTable GetTable()
        {
            return _table;
        }
    }

    /// <summary>
    /// 解析带数据类型的csv解析器
    /// csv格式：第一行为表头信息，第二行为列数据类型
    /// </summary>
    public class ResolverWithType : SimpleResolver
    {
        public ResolverWithType()
        {
            //设置数据起始索引为2
            _dataBodyBeginIndex = 2;
        }

        protected override void ResolveTitle(StringReader sr)
        {
            base.ResolveTitle(sr);

            string typeLine = sr.ReadLine();
            string[] typeCells = typeLine.Split(COMMA);
            for (int i = 0; i < typeCells.Length; i++)
            {
                var title = _table.FindTitle(i);
                title.dataType = DataTypeName.ConvertToDateType(typeCells[i].Trim());
            }
        }
    }

    public class CSVReader
    {

        ICSVResolver _csvResolver;

        public CSVReader() : this(null) { }
        public CSVReader(ICSVResolver resolver)
        {
            if (resolver == null)
                _csvResolver = new SimpleResolver();
            else
                _csvResolver = resolver;
        }

        public CSVTable LoadTable(string content)
        {
            if (string.IsNullOrEmpty(content))
                return null;

            _csvResolver.Resolve(content);

            return _csvResolver.GetTable();
        }
    }
}

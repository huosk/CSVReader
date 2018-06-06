# CSVReader
简单CSV文件解析库

## 使用示例

* 加载CSV文件

```csharp
string content = File.ReadAllText("test.csv");
CSVReader reader = new CSVReader();
CSVTable table = reader.LoadTable(content);
```

* 加载带数据类型的CSV文件

csv文件格式如下，第二行是对应列的数据类型
```
id,name,level,gender,model,goden,attack
int,text,int,int,text,int,float
1,张三1,101,1,avatar01,2000,247.54
```

加载代码：
```csharp
string content = File.ReadAllText("testtype.csv");
CSVReader reader = new CSVReader(new ResolverWithType());
CSVTable table = reader.LoadTable(content);
```

* 查找数据行

```csharp
CSVTable table = null; //替换成加载数据表

//找到第一个id为10的数据行
var firstRow = table.FindFirstRow((row)=>{
	var entry = row.GetEntry("id");
	return entry.content == "10";
});

//找到所有gender为 0 的数据行
var rows = table.FindRows((row)=>{
	var entry = row.GetEntry("gender");
	return entry.content =="0";
})

//根据整数范围查找

//设置列数据类型，如果是加载带数据类型的csv文件，则不需要显示设置
table.SetColumnType("level", DataType.Integer);
table.SetColumnType("name", DataType.Text);
table.SetColumnType("attack", DataType.Float);

var rowsInRange = table.FindRows((row)=>{
	var levelEntry = row.GetEntry("level");
	var attackEntry = row.GetEntry("attack");
	return (int)levelEntry > 110 && (flaot)attackEntry >= 249; 
});
```


* 定义解析器

实现 ICSVResolver 

```csharp

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
```

继承 SimpleResolver 基类

```csharp

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

```

* 使用自定义解析器

```csharp

//创建CSVReader时，传入解析器实例
CSVReader reader =  new CSVReader(new ResolverWithType());
```
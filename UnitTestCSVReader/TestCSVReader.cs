using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Frame.Data;

namespace UnitTestCSVReader
{
    [TestClass]
    public class TestCSVReader
    {
        [TestMethod]
        public void TestLoadNull()
        {
            CSVReader reader = new CSVReader();
            CSVTable table = reader.LoadTable(null);

            Assert.AreEqual(null, table);
        }

        [TestMethod]
        public void TestlegalContent()
        {
            string content = @"id,name,level
0,
1,ll,22";

            var reader = new CSVReader();
            var table = reader.LoadTable(content);

            Assert.AreNotEqual(null, table);

            Assert.AreEqual(2, table.GetRowCount());

            var firstRow = table.FindRow(0);

            Assert.AreNotEqual(null, firstRow);

            Assert.AreEqual(2, firstRow.GetElementCount());
            Assert.AreNotEqual(null, firstRow.GetEntry(0));
            Assert.AreNotEqual(null, firstRow.GetEntry(1));

            Assert.AreEqual("0", firstRow.GetEntry(0).content);
            Assert.AreEqual(string.Empty, firstRow.GetEntry(1).content);
        }

        [TestMethod]
        public void TestLoadCSV()
        {
            CSVTable table = LoadTable();

            //table不为空
            Assert.AreNotEqual(null, table);

            //行数30
            Assert.AreEqual(30, table.GetRowCount());
        }

        private CSVTable LoadTable()
        {
            string content = File.ReadAllText("test.csv", System.Text.Encoding.UTF8);
            CSVReader reader = new CSVReader();
            CSVTable table = reader.LoadTable(content);

            return table;
        }

        private CSVTable LoadTypedTable()
        {
            string content = File.ReadAllText("testtype.csv", System.Text.Encoding.UTF8);
            CSVReader reader = new CSVReader(new ResolverWithType());
            CSVTable table = reader.LoadTable(content);

            return table;
        }

        [TestMethod]
        public void TestFindFirstRow()
        {
            CSVTable table = LoadTable();

            var firstRow = table.FindFirstRow((row) =>
             {
                 var entry = row.GetEntry("id");
                 return entry.content == "10";
             });

            Assert.AreNotEqual(null, firstRow);

            Assert.AreEqual("张三10", firstRow.GetEntry("name").content);
            Assert.AreEqual("110", firstRow.GetEntry("level").content);
            Assert.AreEqual("0", firstRow.GetEntry("gender").content);
            Assert.AreEqual("avatar10", firstRow.GetEntry("model").content);
            Assert.AreEqual("2000", firstRow.GetEntry("goden").content);
            Assert.AreEqual("248.44", firstRow.GetEntry("attack").content);
        }

        [TestMethod]
        public void TestFindRows()
        {
            CSVTable table = LoadTable();
            var rows = table.FindRows((row) =>
            {
                var entry = row.GetEntry("gender");
                return entry.content == "0";
            });

            Assert.AreEqual(7, rows.Count);
        }

        [TestMethod]
        public void TestFindLastRow()
        {
            CSVTable table = LoadTable();

            var lastRow = table.FindLastRow((row) =>
            {
                var entry = row.GetEntry("gender");
                return entry.content == "0";
            });

            Assert.AreNotEqual(null, lastRow);

            Assert.AreEqual("24", lastRow.GetEntry("id").content);
            Assert.AreEqual("张三24", lastRow.GetEntry("name").content);
            Assert.AreEqual("124", lastRow.GetEntry("level").content);
            Assert.AreEqual("0", lastRow.GetEntry("gender").content);
            Assert.AreEqual("avatar24", lastRow.GetEntry("model").content);
            Assert.AreEqual("2000", lastRow.GetEntry("goden").content);
            Assert.AreEqual("249.84", lastRow.GetEntry("attack").content);
        }

        [TestMethod]
        public void TestTypeConvert()
        {
            CSVTable table = LoadTypedTable();

            Assert.AreEqual(DataType.Integer, table.GetColumnType("level"));
            Assert.AreEqual(DataType.Text, table.GetColumnType("name"));
            Assert.AreEqual(DataType.Float, table.GetColumnType("attack"));

            var rows = table.FindRows((row) =>
            {
                var levelEntry = row.GetEntry("level");
                var attackEntry = row.GetEntry("attack");

                return (int)levelEntry > 110 && (float)attackEntry >= 249.44f;
            });

            Assert.AreEqual(11, rows.Count);
        }
    }
}

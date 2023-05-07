﻿// See https://aka.ms/new-console-template for more information
using System.Data;
using System.Diagnostics;
using Spectre.Console;
using SQLiteLib;
using SQLiteLib.Table.Interfaces;
using DataColumn = SQLiteLib.Table.Impl.DataColumn;
using DataColumnCollection = SQLiteLib.Table.Impl.DataColumnCollection;
using DataRowCollection = SQLiteLib.Table.Impl.DataRowCollection;
using DataTable = SQLiteLib.Table.Impl.DataTable;
using Rule = Spectre.Console.Rule;

namespace SqliteLib.Tests;

public static class Program
{
    private static string DBPath = "";
    private static IDataTable table;
    private static IDataTable rightTable;
    private static IDataTable tmpTable;
    private static IDataColumnCollection columns;
    private static IDataColumnCollection tmpColumns;
    private static int Max1 = 100;
    private static int Max2 = 1000;

    [STAThread]
    public static async Task Main(string[] args)
    {
        GlobalService.Registers();

        var tester = new ParaTableTest();
        var stop = Stopwatch.StartNew();
        var stype = new Style(foreground: Color.Orange1);
        AnsiConsole.Write(new FigletText("Sqlite Lib Test").Centered().Color(Color.Red));
        var paracount = AnsiConsole.Ask<int>("input parameter count:");
        var rowcount = AnsiConsole.Ask<int>("input row count:");
        var chunkcount = AnsiConsole.Ask<int>("input chunk count:");

        tester.RowCount = rowcount;
        tester.ParaCount = paracount;
        tester.ChunkCount = chunkcount;

        AnsiConsole.Write(new Rule($"[White]Create Table WAT_PARA_LONG [/]").Centered());
        var mainTable = await tester.CrateParaLongTableAsync("WAT_PARA_LONG");
        AnsiConsole.Write(new Rule().Centered());
        AnsiConsole.Write(new Rule($"[White]Write Data WAT_PARA_LONG[/]").Centered());
        await tester.WriteParaLongTableAsync(mainTable);
        AnsiConsole.Write(new Rule().Centered());

        stop.Stop();
        AnsiConsole.Write(new Rule($"[White]Sqlite Lib Test Times {stop.Elapsed.TotalSeconds} s[/]").Centered());
        AnsiConsole.Ask<string>("input any key exit.");
    }


    public static async Task MainForSmallTable(string[] args)
    {
        GlobalService.Registers();

        var tester = new ParaTableTest();
        var stop = Stopwatch.StartNew();
        var stype = new Style(foreground: Color.Orange1);
        AnsiConsole.Write(new FigletText("Sqlite Lib Test").Centered().Color(Color.Red));

        AnsiConsole.Write(new Rule($"[White]Create Table WAT_PARA_MAIN [/]").Centered());
        var mainTable = await tester.CreateParaMainTableTestAsync("WAT_PARA_MAIN");
        AnsiConsole.Write(new Rule().Centered());
        AnsiConsole.Write(new Rule($"[White]Write Data WAT_PARA_MAIN[/]").Centered());
        await tester.WriteParaMainTableAsync(mainTable);
        AnsiConsole.Write(new Rule().Centered());

        for (int i = 0; i < 1; i++)
        {
            AnsiConsole.Write(new Rule($"[White]Create Table WAT_PARA_{i} [/]").Centered());
            var table = await tester.CreateParaDetailTableTestAsync($"WAT_PARA_{i}");
            AnsiConsole.Write(new Rule().Centered());
            AnsiConsole.Write(new Rule($"[White]Write Data WAT_PARA_{i}[/]").Centered());
            await tester.WriteParaDetailAsync(table);
            AnsiConsole.Write(new Rule().Centered());
        }

        AnsiConsole.Write(new Rule($"[White]Write Data WAT_PARA_VIEW[/]").Centered());
        await tester.CreateParaViewAsync(mainTable, " WAT_PARA_VIEW");
        AnsiConsole.Write(new Rule().Centered());

        stop.Stop();
        AnsiConsole.Write(new Rule($"[White]Sqlite Lib Test Times {stop.Elapsed.TotalSeconds} s[/]").Centered());
    }

    public static async Task Maisn(string[] args)
    {
        GlobalService.Registers();

        var rows = new DataRowCollection { Table = table };
        DBPath = Path.Combine(AppContext.BaseDirectory, "Data", "Sqlite.db");
        DataTable.DBPath = DBPath;
        var stype = new Style(foreground: Color.Orange1);
        AnsiConsole.Write(new FigletText("Sqlite Lib Test").Centered().Color(Color.Red));
        AnsiConsole.Write(new Rule("[White]Create Table[/]").Centered());
        await CreateTableTest();
        AnsiConsole.Write(new Rule().Centered());
        AnsiConsole.Write(new Rule("[White]Create Data[/]").Centered());
        await WriteTest();
        AnsiConsole.Write(new Rule().Centered());
        AnsiConsole.Write(new Rule("[White]Add Columns[/]").Centered());
        await AddColumnsTest();
        AnsiConsole.Write(new Rule().Centered());
        AnsiConsole.Write(new Rule("[White]Update Data[/]").Centered());
        await UpdateTest();
        AnsiConsole.Write(new Rule().Centered());
        AnsiConsole.Write(new Rule("[White]Merge Rows[/]").Centered());
        await MergeRowsTest();
        AnsiConsole.Write(new Rule().Centered());
        AnsiConsole.Write(new Rule("[White]Merge Table[/]").Centered());
        await MergeTableTest();
        AnsiConsole.Write(new Rule().Centered());
        AnsiConsole.Write(new Rule("[White]Merge Columns[/]").Centered());
        await MergeColumnsTest();
        AnsiConsole.Write(new Rule().Centered());
        AnsiConsole.Write(new Rule("[White]Query Data[/]").Centered());
        await QueryAsync();
        AnsiConsole.Write(new Rule().Centered());
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        AnsiConsole.Markup($"[red]{e.ExceptionObject}[/]"); // [World]
    }

    public static async Task CreateTableForParameterTest()
    {
        //columns = GlobalService.GetService<IDataColumnCollection>();
        columns = new DataColumnCollection();
        columns.Add(new DataColumn() { Name = "RowIndex", Field = "RowIndex", IsPK = true, IsAutoincrement = true, ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.Int32 });
        columns.Add(new DataColumn() { Name = "RowKey", Field = "RowKey", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.String });
        columns.Add(new DataColumn() { Name = "WaferId", Field = "WaferId", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.String });
        columns.Add(new DataColumn() { Name = "DieX", Field = "DieX", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.Int32 });
        columns.Add(new DataColumn() { Name = "DieY", Field = "DieY", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.Int32 });
        columns.Add(new DataColumn() { Name = "OrigX", Field = "OrigX", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.Int32 });
        columns.Add(new DataColumn() { Name = "OrigY", Field = "OrigY", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.Int32 });
        columns.Add(new DataColumn() { Name = "Product", Field = "Product", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.String });

        table = await DataTable.CreateTableAsync("DefectInfo", "DefectInfo", columns);
    }

    public static async Task CreateTableTest()
    {
        //columns = GlobalService.GetService<IDataColumnCollection>();
        columns = new DataColumnCollection();
        columns.Add(new DataColumn() { Name = "RowIndex", Field = "RowIndex", IsPK = true, IsAutoincrement = true, ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.Int32 });
        columns.Add(new DataColumn() { Name = "RowKey", Field = "RowKey", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.String });
        columns.Add(new DataColumn() { Name = "WaferId", Field = "WaferId", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.String });
        columns.Add(new DataColumn() { Name = "DieX", Field = "DieX", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.Int32 });
        columns.Add(new DataColumn() { Name = "DieY", Field = "DieY", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.Int32 });
        columns.Add(new DataColumn() { Name = "OrigX", Field = "OrigX", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.Int32 });
        columns.Add(new DataColumn() { Name = "OrigY", Field = "OrigY", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.Int32 });
        columns.Add(new DataColumn() { Name = "Product", Field = "Product", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.String });

        table = await DataTable.CreateTableAsync("DefectInfo", "DefectInfo", columns);
    }

    public static async Task WriteTest()
    {
        AnsiConsole.MarkupLine("[Green] Start processing data...[/]");

        for (int i = 0; i < Max1; i++)
        {
            for (int j = 0; j < Max2; j++)
            {
                var row = table.NewRow();
                row["RowIndex"] = table.Rows.Count;
                row["RowKey"] = Guid.NewGuid().ToString();
                row["Product"] = $"Hi3680_{i}";
                row["WaferId"] = $"SDS_{i}_{j}";
                row["DieX"] = i;
                row["DieY"] = j;
                row["OrigX"] = i;
                row["OrigY"] = j;
                table.Rows.Add(row);
            }
        }

        AnsiConsole.MarkupLine($"[Green] Processing data end. count {table.RowCount} [/]");
        AnsiConsole.MarkupLine($"[Green] Start write table to sqlite.... [/]");
        var st = Stopwatch.StartNew();
        await table.WriteAsync();
        AnsiConsole.MarkupLine($"[Green] Write table to sqlite end, times {st.Elapsed.TotalSeconds} s, count {table.RowCount}.... [/]");
    }

    public static async Task AddColumnsTest()
    {
        AnsiConsole.MarkupLine("[Green] Start add datatable columns...[/]");
        tmpColumns = new DataColumnCollection() { Table = table };
        tmpColumns.Add(new DataColumn() { Name = "Aera", Field = "Aera", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.String });
        tmpColumns.Add(new DataColumn() { Name = "ClassNumber", Field = "ClassNumber", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.Int32 });

        await table.AddColumnsAsync(new UpdateSetting { NewColumns = tmpColumns, Table = table.SqliteTable });
        AnsiConsole.MarkupLine("[Green] Add datatable columns end...[/]");
    }

    public static async Task UpdateTest()
    {
        AnsiConsole.MarkupLine("[Green] Start processing data...[/]");

        var rows = new DataRowCollection() { Table = table };
        var pkColumns = new DataColumnCollection { Table = table };
        pkColumns.AddRange(table.Columns.Where(m => m.IsPK).ToList());

        for (int i = 0; i < Max1; i++)
        {
            for (int j = 0; j < Max2; j++)
            {
                var row = table.NewRow();
                row["RowIndex"] = rows.Count;
                row["Aera"] = $"Aera_{i}";
                row["ClassNumber"] = (i + 3) * (j + 4);
                rows.Add(row);
            }
        }

        AnsiConsole.MarkupLine($"[Green] Processing data end. count {table.RowCount} [/]");
        AnsiConsole.MarkupLine($"[Green] Start update table to sqlite.... [/]");
        var st = Stopwatch.StartNew();
        await table.UpdateAsync(new UpdateSetting { Table = table.SqliteTable, UpdateColumns = tmpColumns, Rows = rows, PrimaryColumns = pkColumns });
        AnsiConsole.MarkupLine($"[Green] Update table to sqlite end, times {st.Elapsed.TotalSeconds} s, count {table.RowCount}.... [/]");
    }

    public static async Task MergeRowsTest()
    {
        AnsiConsole.MarkupLine("[Green] Start processing data...[/]");

        var rows = new DataRowCollection { Table = table };
        var rowIndex = table.RowCount;
        var tmColumns = GlobalService.GetService<IDataColumnCollection>(); //new DataColumnCollection(table);
        //tmColumns.Table = table;
        tmColumns.Add(new DataColumn() { Name = "ReclassNumber", Field = "ReclassNumber", ColumnIndex = table.ColumnCount, VisbleIndex = table.ColumnCount, TypeCode = TypeCode.Int32 });

        table.Columns.AddRange(tmColumns);

        for (int i = 0; i < Max1; i++)
        {
            for (int j = 0; j < Max2; j++)
            {
                var row = table.NewRow();
                row["RowIndex"] = rowIndex++;
                row["RowKey"] = Guid.NewGuid().ToString();
                row["Product"] = $"Hi3680_{i}";
                row["WaferId"] = $"SDS_{i}_{j}";
                row["DieX"] = i;
                row["DieY"] = j;
                row["OrigX"] = i;
                row["OrigY"] = j;
                row["Aera"] = $"Aera_{i}";
                row["ClassNumber"] = (i + 3) * (j + 4);
                row["ReclassNumber"] = (i + 5) * (j + 7);
                rows.Add(row);
            }
        }

        AnsiConsole.MarkupLine($"[Green] Processing data end. count {table.RowCount} [/]");
        AnsiConsole.MarkupLine($"[Green] Start merge table to sqlite.... [/]");
        var st = Stopwatch.StartNew();
        await table.MergeRowsAsync(new UpdateSetting { Table = table.SqliteTable, Rows = rows, NewColumns = tmColumns });
        AnsiConsole.MarkupLine($"[Green] Merge table to sqlite end, times {st.Elapsed.TotalSeconds} s, count {table.RowCount}.... [/]");
    }

    public static async Task MergeTableTest()
    {
        AnsiConsole.MarkupLine("[Green] Start create right table...[/]");

        var rightColumns = table.Columns.Copy();

        var tmColumns = GlobalService.GetService<IDataColumnCollection>();
        tmColumns.Table = table;
        tmColumns.Add(new DataColumn() { Name = "FinalBinNumber", Field = "FinalBinNumber", ColumnIndex = table.ColumnCount, VisbleIndex = table.ColumnCount, TypeCode = TypeCode.Int32 });

        rightColumns.AddRange(tmColumns);

        rightTable = await DataTable.CreateTableAsync("RightDefectInfo", "RightDefectInfo", rightColumns);

        AnsiConsole.MarkupLine("[Green] Create right table end...[/]");
        AnsiConsole.MarkupLine("[Green] Start processing data...[/]");

        for (int i = 0; i < Max1; i++)
        {
            for (int j = 0; j < Max2; j++)
            {
                var row = rightTable.NewRow();
                row["RowIndex"] = rightTable.RowCount;
                row["RowKey"] = Guid.NewGuid().ToString();
                row["Product"] = $"Hi3680_{i}";
                row["WaferId"] = $"SDS_{i}_{j}";
                row["DieX"] = i;
                row["DieY"] = j;
                row["OrigX"] = i;
                row["OrigY"] = j;
                row["Aera"] = $"Aera_{i}";
                row["ClassNumber"] = (i + 3) * (j + 4);
                row["ReclassNumber"] = (i + 5) * (j + 7);
                row["FinalBinNumber"] = (i + 2) * (j + 3);
                rightTable.Rows.Add(row);
            }
        }

        AnsiConsole.MarkupLine($"[Green] Processing data end. count {table.RowCount} [/]");
        AnsiConsole.MarkupLine($"[Green] Start write right table to sqlite.... [/]");
        var st = Stopwatch.StartNew();
        await rightTable.WriteAsync();
        st.Stop();
        AnsiConsole.MarkupLine($"[Green] Write right table to sqlite end, times {st.Elapsed.TotalSeconds} s, count {table.RowCount}.... [/]");
        AnsiConsole.MarkupLine($"[Green] Start merge table to sqlite.... [/]");
        var mathColumns = new List<(IDataColumn Left, IDataColumn Right)>();

        for (int i = 0; i < table.Columns.Count; i++)
        {
            mathColumns.Add((table.Columns[i], rightTable.Columns[i]));
        }

        var mergeSetting = new MergeSetting() { LeftColumns = table.Columns, RightColumns = rightTable.Columns, LeftTable = table.SqliteTable, RightTable = rightTable.SqliteTable, NewColumns = tmColumns, MacthCloumns = mathColumns };
        st.Restart();
        await rightTable.MergeRowsAsync(mergeSetting);
        AnsiConsole.MarkupLine($"[Green] Merge table to sqlite end, times {st.Elapsed.TotalSeconds} s, count {table.RowCount}.... [/]");
    }

    public static async Task MergeColumnsTest()
    {
        var columnIndex = 0;
        AnsiConsole.MarkupLine("[Green] Start create right table...[/]");
        var rightColumns = new DataColumnCollection();
        rightColumns.Add(new DataColumn() { Name = "RowIndex", Field = "RowIndex", IsPK = true, IsAutoincrement = true, ColumnIndex = columnIndex++, VisbleIndex = columnIndex, TypeCode = TypeCode.Int32 });

        var tmColumns = GlobalService.GetService<IDataColumnCollection>();
        tmColumns.Table = table;
        tmColumns.Add(new DataColumn() { Name = "FinalNumber", Field = "FinalNumber", ColumnIndex = columnIndex++, VisbleIndex = columnIndex, TypeCode = TypeCode.Int32 });
        tmColumns.Add(new DataColumn() { Name = "ReFinalNumber", Field = "ReFinalNumber", ColumnIndex = columnIndex++, VisbleIndex = columnIndex, TypeCode = TypeCode.Int32 });

        rightColumns.AddRange(tmColumns);

        tmpTable = await DataTable.CreateTableAsync("RightDefectInfo2", "RightDefectInfo2", rightColumns);

        AnsiConsole.MarkupLine("[Green] Create right table end...[/]");
        AnsiConsole.MarkupLine("[Green] Start processing data...[/]");

        for (int i = 0; i < Max1; i++)
        {
            for (int j = 0; j < Max2; j++)
            {
                var row = tmpTable.NewRow();
                row["RowIndex"] = tmpTable.RowCount;
                row["FinalNumber"] = (i + 5) * (j + 7);
                row["ReFinalNumber"] = (i + 2) * (j + 3);
                tmpTable.Rows.Add(row);
            }
        }

        AnsiConsole.MarkupLine($"[Green] Processing data end. count {tmpTable.RowCount} [/]");
        AnsiConsole.MarkupLine($"[Green] Start write right table to sqlite.... [/]");
        var st = Stopwatch.StartNew();
        await tmpTable.WriteAsync();
        st.Stop();
        AnsiConsole.MarkupLine($"[Green] Write right table to sqlite end, times {st.Elapsed.TotalSeconds} s, count {tmpTable.RowCount}.... [/]");
        AnsiConsole.MarkupLine($"[Green] Start merge columns to sqlite.... [/]");
        var mathColumns = new List<(IDataColumn Left, IDataColumn Right)>();
        mathColumns.Add((table.Columns["RowIndex"], rightTable.Columns["RowIndex"]));

        var mergeSetting = new MergeSetting() { LeftColumns = table.Columns, RightColumns = tmpTable.Columns, LeftTable = table.SqliteTable, RightTable = tmpTable.SqliteTable, NewColumns = tmColumns, MacthCloumns = mathColumns };
        st.Restart();
        await rightTable.MergeColumnsAsync(mergeSetting);
        AnsiConsole.MarkupLine($"[Green] Merge columns to sqlite end, times {st.Elapsed.TotalSeconds} s, count {table.RowCount}.... [/]");
    }

    public static async Task QueryAsync()
    {
        var st = Stopwatch.StartNew();
        var setting = new QuerySetting()
        {
            Table = table,
            Columns = table.Columns,
        };

        setting.Parameters = new List<Condition>();
        setting.Parameters.Add(new Condition { DataColumn = table.Columns["Aera"], Binary = LogicMode.OR, Logic = LogicMode.IN, Value = new List<int> { 45, 35, 25, 15, 55, 65, 66, 58, 69, 10, 22, 12, 62, 71 } });
        setting.Parameters.Add(new Condition { DataColumn = table.Columns["WaferId"], Binary = LogicMode.OR, Logic = LogicMode.Equal, Value = "SDS_6_9" });
        setting.Parameters.Add(new Condition { DataColumn = table.Columns["RowIndex"], Binary = LogicMode.OR, Logic = LogicMode.LessThan, Value = 100 });

        var rows = await table.QueryAsync(setting);

        var ansiTable = new Table().Centered();
        AnsiConsole.Live(ansiTable)
            .Start(ctx =>
            {
                table.Columns.ForEach(col => ansiTable.AddColumns(col.Field));
                ctx.Refresh();

                rows.ForEach(row =>
                {
                    var values = row.Values.Select(m => $"{m}").ToArray();
                    ansiTable.AddRow(values);
                    ctx.Refresh();
                });
            });

        st.Stop();
        AnsiConsole.MarkupLine($"[Green] Merge columns to sqlite end, times {st.Elapsed.TotalSeconds} s, count {rows.Count}.... [/]");
    }
}
﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using DataLib;
using DataLib.Table;
using DataLib.Table.Impl;
using DataLib.Table.Interfaces;
using HDF5.NET;
using HDF5CSharp;
using Hdf5Lib.Table.Impl;
using Spectre.Console;

namespace HDF5.ConsoleTest;

internal class Hdf5Test
{
    public Hdf5Test()
    {
        Action<ContainerBuilder> build = builder => 
        {
            builder.RegisterType<DBContext>().Keyed<IDBContext>(TableMode.HDF5);
        };


        GlobalService.Register += build;
        GlobalService.Registers();
    }

    public string DBPath { get; set; }

    public int ParaCount { get; set; } = 10;

    public int RowCount { get; set; } = 10;

    public async Task<IDataTable> CreateDataTableAsync(string tableName)
    {
        var start = 10;
        this.DBPath = string.IsNullOrWhiteSpace(DBPath) ? Path.Combine(@"C:\Users\jiede\Documents\HDF5", $"{DateTime.Now:yyyyMMddHH}.H5") : DBPath;
        if (File.Exists(this.DBPath))
            File.Delete(this.DBPath);

        var st = Stopwatch.StartNew();
        var columns = new DataColumnCollection();
        columns.Add(new DataColumn() { Name = "RowKey", Field = "RowKey", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.Int32 });
        columns.Add(new DataColumn() { Name = "WaferId", Field = "WaferId", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.String });
        columns.Add(new DataColumn() { Name = "DieX", Field = "DieX", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.Int32 });
        columns.Add(new DataColumn() { Name = "DieY", Field = "DieY", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.Int32 });
        columns.Add(new DataColumn() { Name = "OrigX", Field = "OrigX", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.Int32 });
        columns.Add(new DataColumn() { Name = "OrigY", Field = "OrigY", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.Int32 });
        columns.Add(new DataColumn() { Name = "Product", Field = "Product", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.String });

        for (int i = start; i < start + ParaCount; i++)
        {
            columns.Add(new DataColumn() { Name = $"Para_{i}", Field = $"Para_{i}", ColumnIndex = columns.Count, VisbleIndex = columns.Count, TypeCode = TypeCode.Double });
        }

        var table = await DataTable.CreateTableAsync(tableName, tableName, columns, this.DBPath);


        st.Stop();
        return table;
    }

    public async Task WriteDataTableAsync(IDataTable table)
    {
        var stop = Stopwatch.StartNew();
        var waferId = 1;
        AnsiConsole.Write(new Rule("[Green] Start processing data...[/]").Centered());
        var paraColumns = table.Columns.Where(m => m.Field.StartsWith($"Para_")).OrderBy(m => m.ColumnIndex).ToList();

        for (int i = 0; i < RowCount; i++)
        {
            var row = table.NewRow();
            row["RowKey"] = i;
            row["Product"] = $"Hi3680_{i}";
            row["WaferId"] = $"SDS_{waferId++}";
            row["DieX"] = i;
            row["DieY"] = i + 1;
            row["OrigX"] = i;
            row["OrigY"] = i + 1;

            foreach (var column in paraColumns)
                row[column] = 3.1415926789 * (i + 5);

            table.Rows.Add(row);

            if (waferId > 50)
                waferId = 1;
        }

        stop.Stop();
        AnsiConsole.Write(new Rule($"[Green] Processing data end.column count {table.ColumnCount},row count {table.RowCount}, times {stop.Elapsed.TotalSeconds} s [/]").Centered());

        stop.Restart();
        AnsiConsole.Write(new Rule($"[Green] Start write table to hdf5.... [/]").Centered());
        await table.InsertAsync(table.Rows);
        stop.Stop();
        AnsiConsole.Write(new Rule($"[Green] Write table to hdf5 end, times {stop.Elapsed.TotalSeconds} s, count {table.RowCount}... [/]").Centered());
    }

    public async Task QueryDataAsync(IDataTable table)
    {
        var stop = Stopwatch.StartNew();
        AnsiConsole.Write(new Rule("[Green] Start processing Hdf5QueryFilter ...[/]").Centered());
        this.DBPath = string.IsNullOrWhiteSpace(DBPath) ? Path.Combine(@"C:\Users\jiede\Documents\HDF5", $"{DateTime.Now:yyyyMMddHH}.H5") : DBPath;
        var paraColumns = table.Columns.Where(m => m.Field.StartsWith($"Para_")).OrderBy(m => m.ColumnIndex).ToList();
        var querySetting = new QuerySetting { Table = table };
        querySetting.Columns.Add(table.Columns["WaferId"]);
        querySetting.Columns.Add(table.Columns["DieX"]);
        querySetting.Columns.Add(table.Columns["DieY"]);

        for (int i = 0; i < 10; i++)
            querySetting.Columns.Add(paraColumns[i]);

        var setting = new Hdf5QueryFilter
        {
            Logic = LogicMode.IN,
            DataColumn = table.Columns["WaferId"],
            Value = new string[] { "SDS_12", "SDS_15", "SDS_19", "SDS_21", "SDS_18", "SDS_23" },
            Binary = LogicMode.AND
        };

        querySetting.Parameters.Add(setting);

        var settingDieX = new Hdf5QueryFilter
        {
            Logic = LogicMode.Between,
            DataColumn = table.Columns["DieX"],
            Value = new int[] { 100, 200 },
            Binary = LogicMode.AND
        };

        querySetting.Parameters.Add(settingDieX);

        stop.Stop();
        AnsiConsole.Write(new Rule($"[Green] Processing Hdf5QueryFilter end. times {stop.Elapsed.TotalSeconds} s [/]").Centered());
        stop.Restart();
        var datatable = await table.QueryAsync(querySetting);
        stop.Stop();
        AnsiConsole.Write(new Rule($"[Green] query data end. count {datatable.RowCount}, times {stop.Elapsed.TotalSeconds} s [/]").Centered());

        // Create a table
        var actable = new Table();

        // Add some columns
        actable.AddColumn(new TableColumn("RowIndex").Centered());
        datatable.Columns.ForEach(col => actable.AddColumn(new TableColumn(col.Field).Centered()));

        // Add some rows 
        datatable.Rows.ForEach(row =>
        {
            var values = new string[row.Values.Length + 1];
            values[0] = $"{row.RowIndex}";
            for (int i = 0; i < row.Values.Length; i++)
                values[i + 1] = $"{row.Values.GetValue(i)}";

            actable.AddRow(values);
        });

        // Render the table to the console
        AnsiConsole.Write(actable);
    }

    public async Task MergeColumnsTest()
    {
        var setting = new MergeSetting();
        var table = new DataTable { Name = "HDF5_TABLE_TEST", DBFile = Path.Combine(@"C:\Users\jiede\Documents\HDF5", "2023012816.H5"), OriginalTable = "HDF5_TABLE_TEST" };
        var col1 = new DataColumn { Name = "Para_10", ColumnIndex = 1, Field = "Para_10", TypeCode = TypeCode.Double };
        var col2 = new DataColumn { Name = "Para_1", ColumnIndex = 1, Field = "Para_1", TypeCode = TypeCode.Double };
        var col0 = new DataColumn { Name = "RowKey", ColumnIndex = 1, Field = "RowKey", TypeCode = TypeCode.Int32 };
        setting.MacthCloumns.Add((col0, col0));
        setting.LeftColumns.Add(col0);
        setting.LeftColumns.Add(col2);
        setting.RightColumns.Add(col0);
        setting.RightColumns.Add(col1);
        setting.NewColumns.Add(col1);
        setting.TableId = Path.Combine(@"C:\Users\jiede\Documents\HDF5", "2023012816.H5");
        setting.RightTableId = Path.Combine(@"C:\Users\jiede\Documents\HDF5", "2023012817.H5");
        setting.TableName = "HDF5_TABLE_TEST";
        setting.RightTableName = "HDF5_TABLE_TEST";
        setting.Join = DataLib.JoinMode.INNER_JOIN;
        await table.MergeColumnsAsync(setting);

        table.Columns.AddRange(setting.LeftColumns);
        table.Columns.AddRange(setting.RightColumns);
        var newtable = await table.QueryAsync(new QuerySetting { Table = table, Columns = table.Columns });
        // Create a table
        var actable = new Table();

        // Add some columns
        actable.AddColumn(new TableColumn("RowIndex").Centered());
        newtable.Columns.ForEach(col => actable.AddColumn(new TableColumn(col.Field).Centered()));

        // Add some rows 
        newtable.Rows.ForEach(row =>
        {
            var values = new string[row.Values.Length + 1];
            values[0] = $"{row.RowIndex}";
            for (int i = 0; i < row.Values.Length; i++)
                values[i + 1] = $"{row.Values.GetValue(i)}";

            actable.AddRow(values);
        });

        // Render the table to the console
        AnsiConsole.Write(actable);
    }

    public async Task MergeRowsTest()
    {
        var setting = new MergeSetting();
        var table = new DataTable { Name = "HDF5_TABLE_TEST", DBFile = Path.Combine(AppContext.BaseDirectory, "2023012816.H5"), OriginalTable = "HDF5_TABLE_TEST" };
        var index = 0;
        var colA = new DataColumn { Name = "RowKey", ColumnIndex = 0, Field = "RowKey", TypeCode = TypeCode.Int32, IsAutoincrement = true };
        var colB = new DataColumn { Name = "WaferId", ColumnIndex = 1, Field = "WaferId", TypeCode = TypeCode.String };
        var col1 = new DataColumn { Name = "Para_0", ColumnIndex = 2, Field = "Para_0", TypeCode = TypeCode.Double };
        var col2 = new DataColumn { Name = "Para_1", ColumnIndex = 3, Field = "Para_1", TypeCode = TypeCode.Double };
        var col3 = new DataColumn { Name = "Para_2", ColumnIndex = 4, Field = "Para_2", TypeCode = TypeCode.Double };
        var col4 = new DataColumn { Name = "Para_3", ColumnIndex = 5, Field = "Para_3", TypeCode = TypeCode.Double };
        var col5 = new DataColumn { Name = "Para_4", ColumnIndex = 6, Field = "Para_4", TypeCode = TypeCode.Double };
        var col6 = new DataColumn { Name = "Para_5", ColumnIndex = 7, Field = "Para_5", TypeCode = TypeCode.Double };
        var col7 = new DataColumn { Name = "Para_6", ColumnIndex = 8, Field = "Para_6", TypeCode = TypeCode.Double };
        var col8 = new DataColumn { Name = "Para_7", ColumnIndex = 9, Field = "Para_7", TypeCode = TypeCode.Double };
        var col9 = new DataColumn { Name = "Para_8", ColumnIndex = 10, Field = "Para_8", TypeCode = TypeCode.Double };
        var col10 = new DataColumn { Name = "Para_9", ColumnIndex = 11, Field = "Para_9", TypeCode = TypeCode.Double };
        var col11 = new DataColumn { Name = "Para_10", ColumnIndex = 2, Field = "Para_10", TypeCode = TypeCode.Double };
        var col12 = new DataColumn { Name = "Para_11", ColumnIndex = 3, Field = "Para_11", TypeCode = TypeCode.Double };
        var col13 = new DataColumn { Name = "Para_12", ColumnIndex = 4, Field = "Para_12", TypeCode = TypeCode.Double };
        var col14 = new DataColumn { Name = "Para_13", ColumnIndex = 5, Field = "Para_13", TypeCode = TypeCode.Double };
        var col15 = new DataColumn { Name = "Para_14", ColumnIndex = 6, Field = "Para_14", TypeCode = TypeCode.Double };
        var col16 = new DataColumn { Name = "Para_15", ColumnIndex = 7, Field = "Para_15", TypeCode = TypeCode.Double };
        var col17 = new DataColumn { Name = "Para_16", ColumnIndex = 8, Field = "Para_16", TypeCode = TypeCode.Double };
        var col18 = new DataColumn { Name = "Para_17", ColumnIndex = 9, Field = "Para_17", TypeCode = TypeCode.Double };
        var col19 = new DataColumn { Name = "Para_18", ColumnIndex = 10, Field = "Para_18", TypeCode = TypeCode.Double };
        var col20 = new DataColumn { Name = "Para_19", ColumnIndex = 11, Field = "Para_19", TypeCode = TypeCode.Double };


        setting.MacthCloumns.Add((colA, colA));
        setting.LeftColumns.AddRange(new DataColumn[] { colA, colB, col1, col2, col3, col4, col5, col6, col7, col8, col9, col10 });
        setting.RightColumns.AddRange(new DataColumn[] { colA, colB, col11, col12, col13, col14, col15, col16, col17, col18, col19, col20 });
        setting.NewColumns.AddRange(new DataColumn[] { col11, col12, col13, col14, col15, col16, col17, col18, col19, col20 });
        setting.TableId = Path.Combine(AppContext.BaseDirectory, "2023012816.H5");
        setting.RightTableId = Path.Combine(AppContext.BaseDirectory, "2023012817.H5");
        setting.TableName = "HDF5_TABLE_TEST";
        setting.RightTableName = "HDF5_TABLE_TEST";
        setting.Join = DataLib.JoinMode.INNER_JOIN;
        await table.MergeRowsAsync(setting);

        table.Columns.AddRange(setting.LeftColumns);
        table.Columns.AddRange(setting.RightColumns);
        var newtable = await table.QueryAsync(new QuerySetting { Table = table, Columns = table.Columns });
        // Create a table
        var actable = new Table();

        // Add some columns
        actable.AddColumn(new TableColumn("RowIndex").Centered());
        newtable.Columns.ForEach(col => actable.AddColumn(new TableColumn(col.Field).Centered()));

        // Add some rows 
        newtable.Rows.ForEach(row =>
        {
            var values = new string[row.Values.Length + 1];
            values[0] = $"{row.RowIndex}";
            for (int i = 0; i < row.Values.Length; i++)
                values[i + 1] = $"{row.Values.GetValue(i)}";

            actable.AddRow(values);
        });

        // Render the table to the console
        AnsiConsole.Write(actable);
    }
}

﻿using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ALE.ETLBox.DataFlow
{
    /// <summary>
    /// Reads data from a csv source. While reading the data from the file, data is also asnychronously posted into the targets.
    /// Data is read a as string from the source and dynamically converted into the corresponding data format.
    /// </summary>
    /// <example>
    /// <code>
    /// CsvSource&lt;CSVData&gt; source = new CsvSource&lt;CSVData&gt;("Demo.csv");
    /// source.Configuration.Delimiter = ";";
    /// </code>
    /// </example>
    public class CsvSource<TOutput> : DataFlowSource<TOutput>, ITask, IDataFlowSource<TOutput>
    {
        /* ITask Interface */
        public override string TaskName => $"Read Csv data from file {FileName ?? ""}";

        /* Public properties */
        public Configuration Configuration { get; set; }
        public int SkipRows { get; set; } = 0;
        public string FileName { get; set; }
        public string[] FieldHeaders { get; private set; }
        public bool IsHeaderRead => FieldHeaders != null;

        /* Private stuff */
        CsvReader CsvReader { get; set; }
        StreamReader StreamReader { get; set; }
        TypeInfo TypeInfo { get; set; }
        public CsvSource()
        {
            Configuration = new Configuration(CultureInfo.InvariantCulture);
            TypeInfo = new TypeInfo(typeof(TOutput));
        }

        public CsvSource(string fileName) : this()
        {
            FileName = fileName;
        }

        public override void Execute()
        {
            NLogStart();
            Open();
            try
            {
                ReadAll();
                Buffer.Complete();
            }
            finally
            {
                Close();
            }
            NLogFinish();
        }

        private void Open()
        {
            StreamReader = new StreamReader(FileName, Configuration.Encoding ?? Encoding.UTF8, true);
            SkipFirstRows();
            CsvReader = new CsvReader(StreamReader, Configuration);
        }

        private void SkipFirstRows()
        {
            for (int i = 0; i < SkipRows; i++)
                StreamReader.ReadLine();
        }


        private void ReadAll()
        {
            if (Configuration.HasHeaderRecord == true)
            {
                CsvReader.Read();
                CsvReader.ReadHeader();
                FieldHeaders = CsvReader.Context.HeaderRecord;
            }
            while (CsvReader.Read())
            {
                ReadLineAndSendIntoBuffer();
                LogProgress();
            }
        }

        private void ReadLineAndSendIntoBuffer()
        {
            try
            {
                if (TypeInfo.IsArray)
                {
                    string[] line = CsvReader.Context.Record;
                    Buffer.SendAsync((TOutput)(object)line).Wait();
                }
                else if (TypeInfo.IsDynamic)
                {
                    TOutput bufferObject = CsvReader.GetRecord<dynamic>();
                    Buffer.SendAsync(bufferObject).Wait();
                }
                else
                {
                    TOutput bufferObject = CsvReader.GetRecord<TOutput>();
                    Buffer.SendAsync(bufferObject).Wait();
                }
            }
            catch (Exception e)
            {
                if (!ErrorHandler.HasErrorBuffer) throw e;
                if (e is CsvHelperException csvex)
                    ErrorHandler.Send(e,
                        $"Row: {csvex.ReadingContext?.Row} -- StartPos: {csvex.ReadingContext?.RawRecordStartPosition} -- RawRecord: {csvex.ReadingContext?.RawRecord ?? string.Empty}");
                else
                    ErrorHandler.Send(e, "N/A");
            }
        }

        private void Close()
        {
            CsvReader?.Dispose();
            StreamReader?.Dispose();
        }
    }

    /// <summary>
    /// Reads data from a csv source. While reading the data from the file, data is also asnychronously posted into the targets.
    /// CsvSource as a nongeneric type uses dynamic object as output. If you need typed output, use
    /// the CsvSource&lt;TOutput&gt; object instead.
    /// </summary>
    /// <see cref="CsvSource{TOutput}"/>
    /// <example>
    /// <code>
    /// CsvSource source = new CsvSource("demodata.csv");
    /// source.LinkTo(dest); //Link to transformation or destination
    /// source.Execute(); //Start the dataflow
    /// </code>
    /// </example>
    public class CsvSource : CsvSource<ExpandoObject>
    {
        public CsvSource() : base() { }
        public CsvSource(string fileName) : base(fileName) { }
    }
}

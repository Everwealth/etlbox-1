﻿using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;

namespace ALE.ETLBox.DataFlow
{
    /// <summary>
    /// A destination in memory - it will store all you data in a list. Inserts are done in batches.
    /// </summary>
    /// <see cref="MemoryDestination"/>
    /// <typeparam name="TInput">Type of data input.</typeparam>
    public class MemoryDestination<TInput> : DataFlowBatchDestination<TInput>, ITask, IDataFlowDestination<TInput>
    {
        /* ITask Interface */
        public override string TaskName => $"Write data into memory";

        internal const int DEFAULT_BATCH_SIZE = 1000;
        public BlockingCollection<TInput> Data { get; set; }

        public MemoryDestination()
        {
            BatchSize = DEFAULT_BATCH_SIZE;
        }

        protected override void InitObjects(int batchSize)
        {
            base.InitObjects(batchSize);
        }

        protected override void WriteBatch(ref TInput[] data)
        {
            if (Data == null) InitMemoryCollection();
            base.WriteBatch(ref data);
            foreach (TInput record in data) {
                if (record == null) continue;
                Data.Add(record);
            }
            LogProgressBatch(data.Length);
        }

        private void InitMemoryCollection()
        {
            Data = new BlockingCollection<TInput>();
            this.CloseStreamsAction = CloseStream;
        }

        private void CloseStream()
        {
            Data?.CompleteAdding();
        }
    }

    /// <summary>
    /// A destination in memory - it will store all you data in a list. Inserts are done in batches.
    /// The MemoryDestination uses a dynamic object as input type. If you need other data types, use the generic CsvDestination instead.
    /// </summary>
    /// <see cref="MemoryDestination{TInput}"/>
    public class MemoryDestination : MemoryDestination<ExpandoObject>
    {
        public MemoryDestination() : base() { }
    }
}

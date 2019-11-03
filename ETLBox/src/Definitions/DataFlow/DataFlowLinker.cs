﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;
using CF = ALE.ETLBox.ControlFlow;

namespace ALE.ETLBox.DataFlow
{
    public class DataFlowLinker<TOutput>
    {
        public ISourceBlock<TOutput> SourceBlock { get; set; }
        public bool DisableLogging { get; set; }
        public NLog.Logger NLogger { get; set; } = CF.ControlFlow.GetLogger();
        public ITask CallingTask { get; set; }

        public DataFlowLinker(ITask callingTask, ISourceBlock<TOutput> sourceBlock, bool disableLogging)
        {
            this.CallingTask = callingTask;
            this.SourceBlock = sourceBlock;
            this.DisableLogging = disableLogging;
        }

        public IDataFlowLinkSource<TOutput> LinkTo(IDataFlowLinkTarget<TOutput> target)
        {
            SourceBlock.LinkTo(target.TargetBlock, new DataflowLinkOptions() { PropagateCompletion = true });
            if (!DisableLogging)
                NLogger.Debug(CallingTask.TaskName + " was linked to Target!", CallingTask.TaskType, "LOG", CallingTask.TaskHash, ControlFlow.ControlFlow.STAGE, ControlFlow.ControlFlow.CurrentLoadProcess?.LoadProcessKey);
            return target as IDataFlowLinkSource<TOutput>;
        }

        public IDataFlowLinkSource<TOutput> LinkTo(IDataFlowLinkTarget<TOutput> target, Predicate<TOutput> predicate)
        {
            SourceBlock.LinkTo(target.TargetBlock, new DataflowLinkOptions() { PropagateCompletion = true }, predicate);
            if (!DisableLogging)
                NLogger.Debug(CallingTask.TaskName + " was linked to Target!", CallingTask.TaskType, "LOG", CallingTask.TaskHash, ControlFlow.ControlFlow.STAGE, ControlFlow.ControlFlow.CurrentLoadProcess?.LoadProcessKey);
            return target as IDataFlowLinkSource<TOutput>;
        }

    }
}
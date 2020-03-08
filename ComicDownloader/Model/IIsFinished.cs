using System;

namespace ComicDownloader.Model
{
    interface IIsFinished
    {
        bool IsFinished { get; }
        event EventHandler Finished;
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[ExecuteInEditMode]
public class ThreadedDataRequester : MonoBehaviour
{
    readonly Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();

    static ThreadedDataRequester instance;


    public void Awake()
    {
        instance = FindObjectOfType<ThreadedDataRequester>();
    }

    public void Update()
    {
        if (dataQueue.Count > 0)
        {
            for (int i = 0; i < dataQueue.Count; i++)
            {
                ThreadInfo threadInfo = dataQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }


    public static void RequestData(Func<object> generateData, Action<object> callback)
    {
        void threadStart()
        {
            instance.DataThread(generateData, callback);
        }

        new Thread(threadStart).Start();
    }

    void DataThread(Func<object> generateData, Action<object> callback)
    {
        object data = generateData();

        lock (dataQueue)
        {
            dataQueue.Enqueue(new ThreadInfo(callback, data));
        }
    }


    struct ThreadInfo
    {
        public readonly Action<object> callback;
        public readonly object parameter;

        public ThreadInfo(Action<object> callback, object parameter)
        {
            this.callback  = callback;
            this.parameter = parameter;
        }
    }
}

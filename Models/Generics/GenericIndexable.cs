using System;
using System.Collections.Generic;

namespace elasticsearch.Models.Generics
{
    public class GenericIndexable<T>
    {
        public T Data { get; set; }
        public List<T> DataList { get; set; }
        public Guid GuidIndex { get; set; }
        public DateTime TimeStamp { get; set; }

        public GenericIndexable(T data)
        {
            Data = data;
            TimeStamp = DateTime.Now;
        }

        public GenericIndexable(List<T> dataList)
        {
            DataList = dataList;
            TimeStamp = DateTime.Now;
        }

        private void GetData(List<T> dataList)
        {
            foreach (var item in dataList)
            {
                Data = item;
            }
        }
    }
}
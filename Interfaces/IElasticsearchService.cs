using System;
using System.Threading.Tasks;
using elasticsearch.Models.Generics;

namespace elasticsearch.Interfaces;

public interface IElasticsearchService
{
    Task IndexRequestResponse(GenericRequestResponse requestResponse, string indexName);

    Task IndexDataToElastic<T>(GenericIndexable<T> indexable, Guid guid) where T : class;

    Task IndexDataWithLogExceptionToElastic<T>(GenericLog<T> indexable, Guid guid, Exception exception = null)
        where T : class;
}
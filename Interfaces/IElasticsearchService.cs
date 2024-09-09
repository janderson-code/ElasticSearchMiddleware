using br.com.sharklab.elasticsearch.Models.Generics;
using System;
using System.Threading.Tasks;

public interface IElasticsearchService
{
    Task IndexRequestResponse(GenericRequestResponse requestResponse, string indexName);

    Task IndexDataToElastic<T>(GenericIndexable<T> indexable, Guid guid) where T : class;

    Task IndexDataWithLogExceptionToElastic<T>(GenericLog<T> indexable, Guid guid, Exception exception = null)
        where T : class;
}
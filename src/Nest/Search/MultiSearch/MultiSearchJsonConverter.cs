﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Elasticsearch.Net.Serialization;
using Nest.Resolvers;
using Elasticsearch.Net;

namespace Nest
{
	internal class MultiSearchJsonConverter : JsonConverter
	{
		private class MultiHitTuple
		{
			public JToken Hit { get; set; }
			public KeyValuePair<string, ISearchRequest> Descriptor { get; set; }
		}

		private readonly IMultiSearchRequest _request;

		private static MethodInfo MakeDelegateMethodInfo = typeof(MultiSearchJsonConverter).GetMethod("CreateMultiHit", BindingFlags.Static | BindingFlags.NonPublic);
		private readonly IConnectionSettingsValues _settings;

		public MultiSearchJsonConverter()
		{
			
		}

		public MultiSearchJsonConverter(IConnectionSettingsValues settings, IMultiSearchRequest request)
		{
			this._settings = settings;
			_request = request;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var request = value as IMultiSearchRequest;
			if (request == null) return;
			var contract = serializer.ContractResolver as SettingsContractResolver;
			var elasticsearchSerializer = contract?.ConnectionSettings.Serializer;
			if (elasticsearchSerializer == null) return;

			foreach (var operation in request.Operations.Values)
			{
				var index = (request.Index != null && !request.Index.Equals(operation.Index))
					? operation.Index
					: null;

				var type = (request.Type != null && !request.Type.Equals(operation.Type))
					? operation.Type
					: null;

				var searchType = operation.RequestParameters.GetQueryStringValue<SearchType>("search_type").GetStringValue();
				if (searchType == "query_then_fetch")
					searchType = null;

				var header = new
				{
					index = index,
					type = type,
					search_type = searchType,
					preference = operation.Preference,
					routing = operation.Routing,
					ignore_unavailable = operation.IgnoreUnavalable
				};

				var headerBytes = elasticsearchSerializer.SerializeToBytes(header, SerializationFormatting.None);
				writer.WriteRaw(headerBytes.Utf8String() + "\n");
				var bodyBytes = elasticsearchSerializer.SerializeToBytes(operation, SerializationFormatting.None);
				writer.WriteRaw(bodyBytes.Utf8String() + "\n");
			}
		}
		
		private static void CreateMultiHit<T>(
			MultiHitTuple tuple, 
			JsonSerializer serializer, 
			IDictionary<string, object> collection, 
			IConnectionSettingsValues settings
		)
			where T : class
		{
			var hit = new SearchResponse<T>();
			var reader = tuple.Hit.CreateReader();
			serializer.Populate(reader, hit);

			var errorProperty = tuple.Hit.Children<JProperty>().FirstOrDefault(c=>c.Name == "error");
			if (errorProperty != null)
			{
				//hit.IsValid = false;
				//TODO es 1.0 will return statuscode pass that into exception
			}

			collection.Add(tuple.Descriptor.Key, hit);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (this._settings == null)
			{
				var realConverter = (
					(serializer.ContractResolver as SettingsContractResolver)
					?.PiggyBackState?.ActualJsonConverter as MultiSearchJsonConverter
				);
				if (realConverter == null)
					throw new DslException("could not find a stateful multi search converter");

				var mr = realConverter.ReadJson(reader, objectType, existingValue, serializer) as MultiSearchResponse;
				return mr;
			}


			var response = new MultiSearchResponse();
			var jsonObject = JObject.Load(reader);

			var docsJarray = (JArray)jsonObject["responses"];
			if (docsJarray == null)
				return response;
			var multiSearchDescriptor = this._request;
			if (this._request == null)
				return multiSearchDescriptor;

			var withMeta = docsJarray.Zip(this._request.Operations, (doc, desc) => new MultiHitTuple { Hit = doc, Descriptor = desc });
			var originalResolver = serializer.ContractResolver;
			foreach (var m in withMeta)
			{
				var descriptor = m.Descriptor.Value;
				var concreteTypeSelector = descriptor.TypeSelector;
				var baseType = m.Descriptor.Value.ClrType ?? typeof(object);
				
				var generic = MakeDelegateMethodInfo.MakeGenericMethod(baseType);

				if (concreteTypeSelector != null)
				{
					var elasticSerializer = new NestSerializer(this._settings);
					var state = typeof(ConcreteTypeConverter<>).CreateGenericInstance(baseType, concreteTypeSelector) as JsonConverter;
					if (state != null)
					{
						var settings = elasticSerializer.CreateSettings(SerializationFormatting.None, piggyBackJsonConverter: state);

						var jsonSerializer = new JsonSerializer()
						{
							NullValueHandling = settings.NullValueHandling,
							DefaultValueHandling = settings.DefaultValueHandling,
							ContractResolver = settings.ContractResolver,
						};
						foreach (var converter in settings.Converters.EmptyIfNull())
							jsonSerializer.Converters.Add(converter);
						generic.Invoke(null, new object[] { m, jsonSerializer, response._Responses, this._settings });
						continue;
					}
				}
				generic.Invoke(null, new object[] { m, serializer, response._Responses, this._settings });
			}
			
			return response;
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(MultiSearchResponse);
		}
	}
}

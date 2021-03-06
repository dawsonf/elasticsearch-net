﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Nest
{
	[MapsApi("cluster.put_settings.json")]
	public partial interface IClusterPutSettingsRequest
	{
		[DataMember(Name ="persistent")]
		IDictionary<string, object> Persistent { get; set; }

		[DataMember(Name ="transient")]
		IDictionary<string, object> Transient { get; set; }
	}

	public partial class ClusterPutSettingsRequest
	{
		public IDictionary<string, object> Persistent { get; set; }

		public IDictionary<string, object> Transient { get; set; }
	}

	public partial class ClusterPutSettingsDescriptor
	{
		IDictionary<string, object> IClusterPutSettingsRequest.Persistent { get; set; }

		IDictionary<string, object> IClusterPutSettingsRequest.Transient { get; set; }

		public ClusterPutSettingsDescriptor Persistent(Func<FluentDictionary<string, object>, FluentDictionary<string, object>> selector) =>
			Assign(selector, (a, v) => a.Persistent = v?.Invoke(new FluentDictionary<string, object>()));

		public ClusterPutSettingsDescriptor Transient(Func<FluentDictionary<string, object>, FluentDictionary<string, object>> selector) =>
			Assign(selector, (a, v) => a.Transient = v?.Invoke(new FluentDictionary<string, object>()));
	}
}
